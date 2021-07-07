using UnityEngine;

public class DiscordName : MonoBehaviour {

    Transform player;

    void Start() => player = GameObject.Find("Camera").transform;

    void Update() => transform.rotation = Quaternion.LookRotation(transform.position - player.position);

}