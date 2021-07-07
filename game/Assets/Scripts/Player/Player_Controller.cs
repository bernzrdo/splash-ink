using UnityEngine;
using UnityEngine.UI;

public class Player_Controller : MonoBehaviour {

    public new Camera camera;
    public bool canControl = true;

    [Header("Moving")]
    public float speed = .1f;
    public float additionalSpeed = .1f;

    [Header("Jumping")]
    public float jump = .18f;
    public float gravity = .01f;

    [Header("Bobbing")]
    public float bobbingSpeed = .3f;

    CharacterController characterController;
    Vector3 move = Vector3.zero;
    float xRot = 0.0f;
    float timer = 0.0f;

    void Start(){

        LockCursor();

        characterController = GetComponent<CharacterController>();

    }

    void Update() {
        Move();
        if (!canControl) return;
        Rotate();
        Bobbing();
    }

    void Move() {

        // Get direction inputs
        float hAxis = canControl ? Input.GetAxis("Horizontal") : 0;
        float vAxis = canControl ? Input.GetAxis("Vertical") : 0;

        // Check sprint
        bool sprinting = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftControl)) && canControl;

        // Set speed
        float currentSpeed = speed;
        if (sprinting) currentSpeed += additionalSpeed;

        // Set movement
        if (characterController.isGrounded) {
            move = new Vector3(hAxis, 0, vAxis);
            move = transform.TransformDirection(move);
            move *= currentSpeed;
            if (Input.GetButton("Jump") && canControl) move.y = jump;
        } else {
            move = new Vector3(hAxis, move.y, vAxis);
            move = transform.TransformDirection(move);
            move.x *= currentSpeed;
            move.z *= currentSpeed;
        }

        // Set FOV
        float dynamicFov = PlayerPrefs.HasKey("dynamicFov") ? PlayerPrefs.GetInt("dynamicFov") : 20;
        float currentFov = PlayerPrefs.HasKey("fov") ? PlayerPrefs.GetInt("fov") : 70;
        if (move != Vector3.zero && Input.GetAxisRaw("Horizontal") + Input.GetAxisRaw("Vertical") != 0 && canControl && sprinting) currentFov += dynamicFov;
        camera.fieldOfView = Mathf.Lerp(camera.fieldOfView, currentFov, 5 * Time.deltaTime);

        // Move
        move.y -= gravity * Time.deltaTime;
        characterController.Move(move * Time.deltaTime);

    }

    void Rotate() {

        float mouseSensitivity = PlayerPrefs.HasKey("mouseSensitivity") ? PlayerPrefs.GetInt("mouseSensitivity") : 50;

        // Horizontal rotation
        transform.Rotate(0, Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime, 0);

        // Vertical rotation
        xRot += -Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;
        xRot = Mathf.Clamp(xRot, -90f, 90f);
        camera.transform.localRotation = Quaternion.Euler(xRot, 0, 0);

    }

    void Bobbing() {

        float waveslice = 0.0f;
        float horizontal = Mathf.Abs(Input.GetAxis("Horizontal"));
        float vertical = Mathf.Abs(Input.GetAxis("Vertical"));

        if (horizontal == 0 && vertical == 0) timer = 0.0f;
        else {
            waveslice = Mathf.Sin(timer);
            timer += bobbingSpeed * Time.deltaTime;
            if (timer > Mathf.PI * 2) timer -= Mathf.PI * 2;
        }

        Vector3 v3T = camera.transform.localPosition;
        if (waveslice != 0) {
            float bobbingAmount = PlayerPrefs.HasKey("bobbingAmount") ? PlayerPrefs.GetInt("bobbingAmount") : 10;
            float translateChange = waveslice * (bobbingAmount  / 100);
            float totalAxes = Mathf.Clamp(horizontal + vertical, 0.0f, 1.0f);
            translateChange *= totalAxes;
            v3T.y = 0.6f + translateChange;
        } else v3T.y = 0.6f;
        camera.transform.localPosition = v3T;

    }

    public void LockCursor() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        canControl = true;
    }

    public void UnlockCursor() {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        canControl = false;
    }

}
