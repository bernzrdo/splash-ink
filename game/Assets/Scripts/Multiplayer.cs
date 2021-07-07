using NativeWebSocket;
using System.Collections;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Multiplayer : MonoBehaviour {

    public Hub_Settings settings;
    public Hub_Music hubMusic;
    public TextMeshPro hubText;
    public Elevator elevator;
    public TextMeshPro elevatorScreen;
    public Transition transition;
    public EndCutscene endCutscene;
    public Player_Controller player;
    public Player_Gun gun;
    public Player_Health health;
    public Object cow;
    public Object pug;
    public Rigidbody pinkBullet;
    public Rigidbody beigeBullet;
    public AudioClip towerrificMusic;
    public AudioClip farmiliciousMusic;
    public AudioClip deathSFX;

    [Header("HUD")]
    public GameObject hud;
    public Text timer;
    public GameObject cowIcon;
    public GameObject pugIcon;
    public Image radialProgress;

    WebSocket websocket;
    public bool inGame = false;
    string error;
    CharacterController characterController;
    Vector3 lastPos = Vector3.zero;
    Quaternion lastRot = new Quaternion(0,0,0,0);
    Scores scores;
    AudioSource music;
    bool autoReconnecting = false;
    bool usesPeriod;

    async void Start() {

        float tmp;
        usesPeriod = float.TryParse("1.1", out tmp);

        characterController = player.GetComponent<CharacterController>();
        scores = GetComponent<Scores>();
        music = GetComponent<AudioSource>();

        websocket = new WebSocket("ws://148.71.57.67:4523");

        websocket.OnOpen += () => {
            Debug.Log("Connection open!");
            error = null;
            settings.DiscordReady();

            if(elevator.inIt) Send("join");
        };

        websocket.OnError += e => {
            error = e;
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += e=>{
            Debug.Log("Connection closed!");

            if (inGame) StartCoroutine(ErrorInGame());
            else {
                elevator.open = elevator.inIt;
                error = error ?? "The connection was unexpectedly closed.";
                hubText.text = error;
                elevatorScreen.text = error;
                settings.Error(error);
            }

            if(!autoReconnecting) StartCoroutine(AutoRecconect());

        };

        websocket.OnMessage += b=> {
            string[] message = Encoding.UTF8.GetString(b).Split('\n');

            // Move a template
            if (message[0] == "m" && inGame) {
                // 0       - m
                // 1       - id
                // 2,3,4   - pos
                // 5,6,7,8 - rot
                // 9       - speed
                // 10      - team
                // 11      - discord name

                GameObject player = GameObject.Find(message[1]);
                Vector3 pos = new Vector3(F(message[2]), F(message[3]), F(message[4]));
                Quaternion rot = new Quaternion(F(message[5]), F(message[6]), F(message[7]), F(message[8]));
                if (player == null) {
                    player = (GameObject)Instantiate(message[10] == "c" ? cow : pug, pos, rot);
                    player.name = message[1];
                    player.transform.Find("Discord Name").GetComponent<TMP_Text>().text = message[11];
                } else {
                    player.transform.position = pos;
                    player.transform.rotation = rot;
                }
                player.GetComponent<Animator>().SetFloat("speed", F(message[9]) / 10);
                return;
            }

            // Shoot a bullet
            if(message[0] == "b" && inGame) {
                Vector3 pos = new Vector3(F(message[3]), F(message[4]), F(message[5]));
                Vector3 vel = new Vector3(F(message[6]), F(message[7]), F(message[8]));
                Rigidbody bulletClone = Instantiate(message[2] == "p" ? pinkBullet : beigeBullet, pos, Quaternion.identity);
                bulletClone.velocity = vel;
                bulletClone.GetComponent<Bullet>().SFX(message[9] == "s");
                StartCoroutine(ShootAnimation(message[1]));
                return;
            }

            // Damage
            if(message[0] == "d" && inGame) {
                health.Damage(F(message[1]));
                return;
            }

            // Hit Animation
            if (message[0] == "h" && inGame) {
                StartCoroutine(HitAnimation(message[1]));
                return;
            }

            if(message[0] == "s" && inGame) {
                scores.UpdateScores(F(message[1]), F(message[2]));
                return;
            }

            // Open/Close the elevator
            if (message[0] == "elevator") elevator.open = message[1] == "open";

            // Change the office text
            else if (message[0] == "hubText") hubText.text = message[1];

            // Change the elevator screen
            else if (message[0] == "elevatorScreen") elevatorScreen.text = message[1].Replace("\\n", "\n");

            // Start the game
            else if (message[0] == "start") {
                StartCoroutine(StartGame(
                    message[1],
                    new Vector3(F(message[2]), F(message[3]), F(message[4])),
                    new Vector3(F(message[5]), F(message[6]), F(message[7]))
                ));

            // Know which team whe're playing
            } else if (message[0] == "team") {
                if (message[1] == "p") {
                    health.team = "pug";
                    gun.bullet = beigeBullet;
                    cowIcon.SetActive(false);
                    pugIcon.SetActive(true);
                    radialProgress.color = new Color32(204, 154, 86, 100);
                } else {
                    health.team = "cow";
                    gun.bullet = pinkBullet;
                    pugIcon.SetActive(false);
                    cowIcon.SetActive(true);
                    radialProgress.color = new Color32(204, 45, 55, 100);
                }

            // Remove template from game
            } else if (message[0] == "died" && inGame) {
                GameObject subject = GameObject.Find(message[1]);
                float volume = (float)(PlayerPrefs.HasKey("sfx") ? PlayerPrefs.GetInt("sfx") : 100) / 100;
                AudioSource.PlayClipAtPoint(deathSFX, subject.transform.position, volume);
                Destroy(subject);

            // Remove template from game
            } else if (message[0] == "left" && inGame) {
                Destroy(GameObject.Find(message[1]));

            // Change HUD
            } else if (message[0] == "hud" && inGame) {
                timer.text = message[1];

            // End Game
            } else if (message[0] == "end" && inGame) {
                endCutscene.gameObject.SetActive(true);
                endCutscene.StartCutscene(F(message[1]), F(message[2]));

            // Discord Info
            }else if(message[0] == "discord") {
                if (message[1] == "info") settings.DiscordInfo(message[2], message[3]);
                else if (message[1] == "not-found") settings.DiscordNotFound();
                else if (message[1] == "code") settings.DiscordCode(message[2]);
                else if (message[1] == "link") settings.DiscordLink(message[2]);

            }

        };

        await websocket.Connect();
    }

    void Update() {

        #if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
        #endif

        if (!IsOnline()) return;

        if (inGame) {

            music.volume = (float)(PlayerPrefs.HasKey("music") ? PlayerPrefs.GetInt("music") : 50) / 100;

            if (player.transform.position != lastPos || player.transform.rotation != lastRot) {
                lastPos = player.transform.position;
                lastRot = player.transform.rotation;
                float speed = new Vector3(characterController.velocity.x, 0, characterController.velocity.z).magnitude;
                Send("m " + Vector3ToString(lastPos) + " " + QuaternionToString(lastRot) + " " + speed);
            }

        }
    }

    public async void Send(string msg) {
        if (!IsOnline()) return;
        await websocket.SendText(msg);
    }

    public bool IsOnline() => websocket.State == WebSocketState.Open;

    async void OnApplicationQuit() => await websocket.Close();

    IEnumerator ErrorInGame() {
        transition.Close();
        player.canControl = false;
        yield return new WaitForSeconds(1.5f);

        ExitGame();
    }

    public string Vector3ToString(Vector3 v) => v.x + " " + v.y + " " + v.z;
    public string QuaternionToString(Quaternion q) => q.x + " " + q.y + " " + q.z + " " + q.w;

    IEnumerator StartGame(string scene, Vector3 pos, Vector3 rot) {
        player.canControl = false;
        hubMusic.fadeOutElevator = true;
        transition.Close();
        yield return new WaitForSeconds(1.5f);

        SceneManager.LoadScene(scene);
        inGame = true;

        if (scene == "Towerrific") music.clip = towerrificMusic;
        if (scene == "Farmilicious") music.clip = farmiliciousMusic;
        music.Play();

        health.enabled = gun.enabled = true;

        health.ResetHealth();
        gun.gun.SetActive(true);
        gun.ResetGun();
        hud.SetActive(true);
        player.GetComponent<Hub_Player>().enabled = false;

        health.spawnPos = pos;
        health.spawnRot = rot;
        Teleport(pos, rot);

        yield return new WaitForSeconds(1.5f);
        transition.Open();
        player.canControl = true;
    }

    IEnumerator ShootAnimation(string id) {
        GameObject player = GameObject.Find(id);
        if (player == null) yield break;
        Animator animator = player.GetComponent<Animator>();
        animator.SetBool("shooting", true);
        yield return new WaitForSeconds(.13f);
        animator.SetBool("shooting", false);
    }

    IEnumerator HitAnimation(string id) {
        GameObject player = GameObject.Find(id);
        if (player == null) yield break;
        Animator animator = player.GetComponent<Animator>();
        animator.SetBool("hit", true);
        yield return new WaitForSeconds(.15f);
        animator.SetBool("hit", false);
    }

    public void ExitGame() {
        foreach (UniversalObject obj in FindObjectsOfType<UniversalObject>()) Destroy(obj.gameObject);
        Destroy(player.gameObject);

        SceneManager.LoadScene("Hub");

        websocket.Close();
    }

    public async void Reconnect() => await websocket.Connect();

    IEnumerator AutoRecconect() {
        yield return new WaitForSeconds(5);
        Reconnect();
    }

    public void StopMusic() => music.Stop();

    public void Teleport(Vector3 pos, Vector3 rot) {
        characterController.enabled = false;
        player.transform.position = pos;
        player.transform.eulerAngles = rot;
        characterController.enabled = true;
    }

    float F(string input) {
        if (usesPeriod) return float.Parse(input.Replace(",", "."));
        else return float.Parse(input.Replace(".", ","));
    }
}
