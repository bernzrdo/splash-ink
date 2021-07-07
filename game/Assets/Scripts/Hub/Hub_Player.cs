using UnityEngine;
using UnityEngine.UI;

public class Hub_Player : MonoBehaviour {

    public Camera cameraComponent;
    public AudioSource elevatorMusic;
    public AudioSource hubMusic;
    public Text hint;
    public Multiplayer multiplayer;
    public Hub_Settings settings;
    public Pause pause;

    void Start() {
        if (!PlayerPrefs.HasKey("music"))  PlayerPrefs.SetInt("music", 1);
        if (PlayerPrefs.GetInt("music") == 0) {
            hubMusic.pitch = 0;
            elevatorMusic.Pause();
        }
    }

    void Update() {
        RaycastHit hit;
        bool clicked = Input.GetButtonDown("Fire1") && !pause.paused;
        if (Physics.Raycast(cameraComponent.transform.position, cameraComponent.transform.TransformDirection(Vector3.forward), out hit, 5.0f)) {
            if (hit.collider.name == "Computer") {
                hint.text = "Settings";
                if (clicked) settings.Open();

            } else hint.text = "";
        } else hint.text = "";
    }

}