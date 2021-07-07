using UnityEngine;
using UnityEngine.UI;

public class EndUI : MonoBehaviour {

    public Multiplayer multiplayer;
    public Text title;
    public RectTransform bar;
    public Text cowPoints;
    public Text pugPoints;
    public float speed = 1;

    bool counting = false;
    float targetWidth;
    float currentCow = 0;
    float targetCow;
    float currentPug = 0;
    float targetPug;

    void Update() {
        if (!counting) return;

        bar.sizeDelta = new Vector2(
            Mathf.Lerp(bar.sizeDelta.x, targetWidth, speed * Time.deltaTime),
            bar.sizeDelta.y
        );

        currentCow = Mathf.Lerp(currentCow, targetCow, speed * Time.deltaTime);
        cowPoints.text = Mathf.Ceil(currentCow) + "%";

        currentPug = Mathf.Lerp(currentPug, targetPug, speed * Time.deltaTime);
        pugPoints.text = Mathf.Ceil(currentPug) + "%";
    }

    public void StartCounting(float cow, float pug) {

        if (float.IsNaN(cow)) cow = 0;
        if (float.IsNaN(pug)) pug = 0;
        if (cow + pug == 0) cow = pug = 1;

        float sum = cow + pug;

        targetWidth = pug * 750 / sum;
        targetPug = cow * 100 / sum;
        targetCow = pug * 100 / sum;

        counting = true;
    }

    public string StopCounting() {
        counting = false;

        bar.sizeDelta = new Vector2(targetWidth,bar.sizeDelta.y);
        cowPoints.text = Mathf.Ceil(targetCow) + "%";
        pugPoints.text = Mathf.Ceil(targetPug) + "%";

        if (targetCow > targetPug) {
            title.text = "Cows Win!";
            return "cow";
        }

        if (targetCow < targetPug) {
            title.text = "Pugs Win!";
            return "pug";
        }
        
        title.text = "Draw!";
        return "draw";
    }

}