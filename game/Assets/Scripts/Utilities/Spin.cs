using UnityEngine;

public class Spin : MonoBehaviour {

    public Vector3 increment;
    public bool local;

    void Update(){
        if (local) transform.localEulerAngles += increment * Time.deltaTime;
        else transform.eulerAngles += increment * Time.deltaTime;
    }

}