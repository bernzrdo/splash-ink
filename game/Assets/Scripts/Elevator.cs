using UnityEngine;

public class Elevator : MonoBehaviour {

    public Transform door1;
    public Transform door2;
    public AudioSource sfxSource;
    public AudioClip dingSound;
    public AudioClip openSound;
    public AudioClip closeSound;
    public bool open = false;
    public Multiplayer multiplayer;
    public bool inIt = false;

    bool lastState = false;

    void Update(){
        Vector3 newPos;

        newPos = door1.localPosition;
        newPos.x = Mathf.Lerp(newPos.x, open ? 1.45f : 0.5025f, 2 * Time.deltaTime);
        door1.localPosition = newPos;

        newPos = door2.localPosition;
        newPos.x = Mathf.Lerp(newPos.x, open ? -1.45f : -0.5025f, 2 * Time.deltaTime);
        door2.localPosition = newPos;

        if(open != lastState) {
            sfxSource.volume = (float)(PlayerPrefs.HasKey("sfx") ? PlayerPrefs.GetInt("sfx") : 100) / 100;
            sfxSource.PlayOneShot(dingSound);
            sfxSource.PlayOneShot(open ? openSound : closeSound);
            lastState = open;
        }
    }

    private void OnTriggerEnter(Collider other){
        multiplayer.Send("join");
        inIt = true;
    }

    private void OnTriggerExit(Collider other){
        if (multiplayer.IsOnline()) multiplayer.Send("leave");
        else open = false;
        inIt = false;
    }

}