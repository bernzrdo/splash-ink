using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

public class Player_Health : MonoBehaviour {

    public Text text;
    public RectTransform bar;
    public Object pinkBullet;
    public Object beigeBullet;
    public Multiplayer multiplayer;
    public GameObject deathScreen;
    public GameObject hud;
    public GameObject hint;
    public AudioSource sfxSource;
    public AudioClip deathSound;
    public Text respawnHint;
    public float bwSpeed;
    public Vector3 spawnPos;
    public Vector3 spawnRot;

    float hp = 100;
    public string team;
    Player_Controller controller;
    Player_Gun gun;
    ColorGrading colorGrading;
    bool canTakeDamage = true;
    bool canRespawn = false;

    void Start() {
        controller = GetComponent<Player_Controller>();
        gun = GetComponent<Player_Gun>();
    }

    void Update(){
        if (hp > 0) {
            try {
                colorGrading.saturation.value = Mathf.Lerp(colorGrading.saturation.value, 0, bwSpeed);
                colorGrading.contrast.value = Mathf.Lerp(colorGrading.saturation.value, 0, bwSpeed);
            } catch (System.NullReferenceException) {
                GameObject.Find("Post Processing").GetComponent<PostProcessVolume>().profile.TryGetSettings(out colorGrading);
            }
        } else {
            try {
                colorGrading.saturation.value = Mathf.Lerp(colorGrading.saturation.value, -100, bwSpeed);
                colorGrading.contrast.value = Mathf.Lerp(colorGrading.saturation.value, 100, bwSpeed);
            } catch (System.NullReferenceException) {
                GameObject.Find("Post Processing").GetComponent<PostProcessVolume>().profile.TryGetSettings(out colorGrading);
            }

            if (Input.anyKeyDown && canRespawn) {
                ResetHealth();
                StartCoroutine(Invulnerable());

                multiplayer.Teleport(spawnPos, spawnRot);

                controller.canControl = true;
                gun.ResetGun();
                deathScreen.SetActive(false);
                respawnHint.color = new Color(255, 255, 255, 25);
                hud.SetActive(true);
                hint.SetActive(true);
            }
        }
    }

    void RenderHP() {
        text.text = Mathf.Ceil(hp).ToString();
        bar.sizeDelta = new Vector2(hp * 3.5f, bar.sizeDelta.y);
    }

    public void ResetHealth() {
        hp = 100;
        RenderHP();
    }

    public void Damage(float damage) {
        if (hp == 0 || !canTakeDamage) return;

        if (damage > 20) damage = 20;
        
        hp -= damage;
        if (hp < 0) hp = 0;

        multiplayer.Send("h");

        RenderHP();

        if(hp == 0) {
            multiplayer.Send("died");
            controller.canControl = false;
            gun.canShoot = false;
            StartCoroutine(ScheduleRespawn());
            deathScreen.SetActive(true);
            sfxSource.volume = (float)(PlayerPrefs.HasKey("sfx") ? PlayerPrefs.GetInt("sfx") : 100) / 100;
            sfxSource.PlayOneShot(deathSound);
            hud.SetActive(false);
            hint.SetActive(false);
        }
    }

    IEnumerator Invulnerable() {
        canTakeDamage = false;
        yield return new WaitForSeconds(3);
        canTakeDamage = true;
    }

    IEnumerator ScheduleRespawn() {
        yield return new WaitForSeconds(2);
        canRespawn = true;
        respawnHint.color = new Color(255, 255, 255, 255);
    }

}