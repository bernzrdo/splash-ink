using UnityEngine;

public class Transition : MonoBehaviour {

    RectTransform rectTransform;
    float targetX;

    void Start() {
        rectTransform = GetComponent<RectTransform>();
        Open();
    }

    void Update(){
        float x = Mathf.Lerp(rectTransform.localPosition.x, targetX, 3 * Time.deltaTime);
        rectTransform.localPosition = new Vector3(x, 0, 0);
    }

    void Prepare() {
        float w = Screen.width * 2;
        float h = Screen.height * 2;
        float diameter = Mathf.Sqrt((w * w) + (h * h));
        rectTransform.sizeDelta = new Vector2(diameter, diameter);
    }

    public void Open() {
        Prepare();
        rectTransform.localPosition = Vector3.zero;
        targetX = -Screen.width * 3;
    }

    public void Close() {
        Prepare();
        rectTransform.localPosition = new Vector3(Screen.width * 3, 0, 0);
        targetX = 0;
    }

}