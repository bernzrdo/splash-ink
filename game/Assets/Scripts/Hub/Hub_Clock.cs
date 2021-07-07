using System;
using UnityEngine;

public class Hub_Clock : MonoBehaviour {

    public Transform hours;
    public Transform minutes;
    public Transform seconds;

    void Update(){
        long now = DateTimeOffset.Now.ToUnixTimeSeconds();

        hours.localEulerAngles = new Vector3((now % 86400) * 360 / 86400, 0, 0);
        minutes.localEulerAngles = new Vector3((now % 3600) * 360 / 3600, 0, 0);
        seconds.localEulerAngles = new Vector3((now % 60) * 360 / 60, 0, 0);
    }

}