using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour {

    public Brush brush;
    public string team;
    public double amp;
    public AudioClip[] soundEffects;
    public AudioClip[] shootSounds;
    public AudioClip[] conveySounds;
    public bool self = false;

    Multiplayer multiplayer;
    AudioSource audioSource;

    void Start() {
        multiplayer = FindObjectOfType<Multiplayer>();
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(SelfDestruct());
    }

    void OnCollisionEnter(Collision collision) {
        if (self && collision.collider.tag == "Player") {
            float damage = Mathf.Round((float)(GetComponent<Rigidbody>().velocity.magnitude * amp * 100)) / 100;
            multiplayer.Send("d " + collision.collider.name + " " + damage.ToString());
        } else {
            foreach (ContactPoint contact in collision.contacts) {
                PaintTarget paintTarget = contact.otherCollider.GetComponent<PaintTarget>();
                if (paintTarget == null) continue;
                if (self) multiplayer.Send("s");
                PaintTarget.PaintObject(paintTarget, contact.point, contact.normal, brush);
            }
        }
        AudioClip audio = soundEffects[Random.Range(0, soundEffects.Length)];
        float volume = (float)(PlayerPrefs.HasKey("sfx") ? PlayerPrefs.GetInt("sfx") : 100) / 100;
        AudioSource.PlayClipAtPoint(audio, transform.position, volume);
        Destroy(gameObject);
    }

    IEnumerator SelfDestruct() {
        yield return new WaitForSeconds(5);
        Destroy(gameObject);
    }

    public void SFX(bool shootMode) {
        AudioSource audioSource = GetComponent<AudioSource>();
        audioSource.volume = (float)(PlayerPrefs.HasKey("sfx") ? PlayerPrefs.GetInt("sfx") : 100) / 100;
        if(shootMode) audioSource.PlayOneShot(shootSounds[Random.Range(0, shootSounds.Length)]);
        else audioSource.PlayOneShot(conveySounds[Random.Range(0, conveySounds.Length)]);
    }

}