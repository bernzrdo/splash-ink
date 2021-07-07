using UnityEngine;

public class UniversalObject : MonoBehaviour {

    void Start(){
        DontDestroyOnLoad(this.gameObject);
    }

}