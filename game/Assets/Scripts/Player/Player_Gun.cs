using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Player_Gun : MonoBehaviour {

    public GameObject gun;
    public Rigidbody bullet;
    public AudioClip[] shootSounds;
    public AudioClip[] conveySounds;
    public AudioClip noAmmoSound;
    public AudioClip reloadSound;
    public AudioClip reloadedSound;
    public float shootMaxAmmo = 30;
    public float conveyMaxAmmo = 100;
    public float shootSpeed = 10;
    public float conveySpeed = 1;
    public Text hint;
    public Text ammoText;
    public Image radialProgress;
    public new Camera camera;
    public CharacterController characterController;
    public Multiplayer multiplayer;

    public bool canShoot = true;
    AudioSource audioSource;
    bool shootMode = true;
    float conveyNext = 0;
    float conveyPeriod = .1f;

    float shootAmmo;
    float conveyAmmo;
    bool reloading;

    void Start() {

        audioSource = GetComponent<AudioSource>();

        shootAmmo = shootMaxAmmo;
        conveyAmmo = conveyMaxAmmo;

    }

    void Update(){
        if (reloading) {
            if (shootMode) shootAmmo = Mathf.Lerp(shootAmmo, shootMaxAmmo, 2 * Time.deltaTime);
            else conveyAmmo = Mathf.Lerp(conveyAmmo, conveyMaxAmmo, Time.deltaTime);
        }

        if (Cursor.lockState == CursorLockMode.None || !gun.activeSelf) return;

        hint.text = shootMode ? "Shoot Mode" : "Convey Mode";

        ammoText.text = Mathf.Floor(shootMode ? shootAmmo : conveyAmmo).ToString();
        radialProgress.fillAmount = shootMode ? (shootAmmo / shootMaxAmmo) : (conveyAmmo / conveyMaxAmmo);

        if (!canShoot) return;

        if (Input.GetKeyDown(KeyCode.R) && (shootMode ? (shootAmmo < shootMaxAmmo) : (conveyAmmo < conveyMaxAmmo))) StartCoroutine(Reload());

        if (Input.GetMouseButtonDown(1)) shootMode = !shootMode;

        if (Input.GetMouseButtonDown(0)) {
            if (shootMode) SpawnBullet(shootSpeed);
            else conveyNext = Time.time;
        }

        if (!shootMode && Input.GetMouseButton(0) && conveyNext <= Time.time) {
            SpawnBullet(conveySpeed);
            conveyNext += conveyPeriod;
        }
    }

    void SpawnBullet(float speed) {
        audioSource.volume = (float)(PlayerPrefs.HasKey("sfx") ? PlayerPrefs.GetInt("sfx") : 100) / 100;
        if (shootMode) {
            if (shootAmmo == 0) {
                audioSource.PlayOneShot(noAmmoSound);
                return;
            }
            shootAmmo--;
            audioSource.PlayOneShot(shootSounds[Random.Range(0, shootSounds.Length)]);
        } else {
            if (conveyAmmo == 0) {
                audioSource.PlayOneShot(noAmmoSound);
                return;
            }
            conveyAmmo--;
            audioSource.PlayOneShot(conveySounds[Random.Range(0, conveySounds.Length)]);
        }

        Vector3 pos = camera.transform.position + (camera.transform.forward * 1.2f + camera.transform.right * .7f + camera.transform.up * -.3f);

        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        Vector3 target;
        if (Physics.Raycast(ray, out hit)) target = hit.point;
        else target = ray.GetPoint(1000);
        Vector3 vel = (target - pos).normalized * speed;

        Rigidbody bulletClone = Instantiate(bullet, pos, Quaternion.identity);
        bulletClone.GetComponent<Bullet>().self = true;
        Physics.IgnoreCollision(bulletClone.GetComponent<Collider>(), characterController);
        bulletClone.velocity = vel;
        multiplayer.Send("b " + (bullet.name == "Bullet Pink" ? "p" : "b") + " " + multiplayer.Vector3ToString(pos) + " " + multiplayer.Vector3ToString(vel) + " " + (shootMode ? "s" : "c"));

    }

    public void ResetGun() {
        canShoot = true;
        reloading = false;
        conveyAmmo = conveyMaxAmmo;
        conveyNext = Time.time;
        shootAmmo = shootMaxAmmo;
    }

    IEnumerator Reload() {
        reloading = true;
        canShoot = false;

        audioSource.volume = (float)(PlayerPrefs.HasKey("sfx") ? PlayerPrefs.GetInt("sfx") : 100) / 100;
        audioSource.PlayOneShot(reloadSound);

        yield return new WaitForSeconds(shootMode ? .5f : 4);

        ResetGun();

        audioSource.volume = (float)(PlayerPrefs.HasKey("sfx") ? PlayerPrefs.GetInt("sfx") : 100) / 100;
        audioSource.PlayOneShot(reloadedSound);

        reloading = false;
    }

}