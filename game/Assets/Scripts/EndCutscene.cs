using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.SceneManagement;

public class EndCutscene : MonoBehaviour {

    public Multiplayer multiplayer;
    public AudioSource sfxSource;
    public AudioClip whistle;
    public AudioClip drumroll;
    public AudioClip crowdCheer;
    public GameObject hud;
    public GameObject crosshair;
    public GameObject hint;
    public Object ui;
    public Object cow;
    public Object pug;
    public Player_Health health;

    [Header("Towerrific")]
    public Vector3 towerrificUIPos;
    public Vector3 towerrificUIRot;
    public Vector3 towerrificCowPos;
    public Vector3 towerrificCowRot;
    public Vector3 towerrificPugPos;
    public Vector3 towerrificPugRot;

    [Header("Farmilicious")]
    public Vector3 farmiliciousUIPos;
    public Vector3 farmiliciousUIRot;
    public Vector3 farmiliciousCowPos;
    public Vector3 farmiliciousCowRot;
    public Vector3 farmiliciousPugPos;
    public Vector3 farmiliciousPugRot;

    Animator animator;
    AudioSource audioSource;
    Player_Controller player;
    Player_Gun gun;
    Transition transition;
    bool fadeOutMusic = false;
    ColorGrading colorGrading;

    void Start() {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = FindObjectOfType<Player_Controller>();
        gun = FindObjectOfType<Player_Gun>();
        transition = FindObjectOfType<Transition>();
        gameObject.SetActive(false);
    }

    void Update() {
        if (!fadeOutMusic) return;
        audioSource.volume = Mathf.Lerp(audioSource.volume, 0, 3 * Time.deltaTime);
    }

    public void StartCutscene(float c, float p) {
        StartCoroutine(Cutscene(c, p));
    }

    IEnumerator Cutscene(float cowScore, float pugScore) {

        multiplayer.StopMusic();
        player.canControl = gun.canShoot = false;
        sfxSource.PlayOneShot(whistle);

        yield return new WaitForSeconds(2);

        transition.Close();
        yield return new WaitForSeconds(1.5f);

        health.ResetHealth();
        health.deathScreen.SetActive(false);
        GameObject.Find("Post Processing").GetComponent<PostProcessVolume>().profile.TryGetSettings(out colorGrading);
        colorGrading.saturation.value = 0;
        colorGrading.contrast.value = 0;

        foreach (GameObject player in GameObject.FindGameObjectsWithTag("Player")) Destroy(player);

        Vector3 uiPos = Vector3.zero;
        Vector3 uiRot = Vector3.zero;
        Vector3 cowPos = Vector3.zero;
        Vector3 cowRot = Vector3.zero;
        Vector3 pugPos = Vector3.zero;
        Vector3 pugRot = Vector3.zero;

        string scene = SceneManager.GetActiveScene().name;
        int sceneAnim = Animator.StringToHash(scene);

        switch (scene) {
            case "Towerrific":
                uiPos = towerrificUIPos;
                uiRot = towerrificUIRot;
                cowPos = towerrificCowPos;
                cowRot = towerrificCowRot;
                pugPos = towerrificPugPos;
                pugRot = towerrificPugRot;
                break;
            case "Farmilicious":
                uiPos = farmiliciousUIPos;
                uiRot = farmiliciousUIRot;
                cowPos = farmiliciousCowPos;
                cowRot = farmiliciousCowRot;
                pugPos = farmiliciousPugPos;
                pugRot = farmiliciousPugRot;
                break;
        }

        EndUI endUI = ((GameObject)Instantiate(ui, uiPos, Quaternion.Euler(uiRot))).GetComponent<EndUI>();
        Animator cowAnim = ((GameObject)Instantiate(cow, cowPos, Quaternion.Euler(cowRot))).GetComponent<Animator>();
        Animator pugAnim = ((GameObject)Instantiate(pug, pugPos, Quaternion.Euler(pugRot))).GetComponent<Animator>();

        player.gameObject.SetActive(false);
        hud.SetActive(false);
        crosshair.SetActive(false);
        hint.SetActive(false);
        GetComponent<Camera>().enabled = GetComponent<AudioListener>().enabled = true;

        yield return new WaitForSeconds(1);

        animator.SetBool(sceneAnim, true);

        yield return new WaitForSeconds(.5f);
        transition.Open();

        audioSource.volume = (float)(PlayerPrefs.HasKey("music") ? PlayerPrefs.GetInt("music") : 50) / 100;
        audioSource.Play();

        yield return new WaitForSeconds(10);

        sfxSource.PlayOneShot(drumroll);
        endUI.StartCounting(cowScore, pugScore);

        yield return new WaitForSeconds(3.6f);

        string result = endUI.StopCounting();
        cowAnim.SetBool(result == "cow" || result == "draw" ? "victory" : "defeat", true);
        pugAnim.SetBool(result == "pug" || result == "draw" ? "victory" : "defeat", true);
        sfxSource.PlayOneShot(crowdCheer);

        yield return new WaitForSeconds(5);

        fadeOutMusic = true;
        transition.Close();
        yield return new WaitForSeconds(1.5f);

        multiplayer.ExitGame();
    }

}