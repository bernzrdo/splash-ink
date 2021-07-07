using UnityEngine;

public class Scores : MonoBehaviour {

    public RectTransform bar;
    public RectTransform crown;

    float targetWidth = 375;
    int winningSide = 0;

    void Update(){
        bar.sizeDelta = new Vector2(
            Mathf.Lerp(bar.sizeDelta.x, targetWidth, 2 * Time.deltaTime),
            bar.sizeDelta.y
        );

        crown.anchoredPosition = new Vector2(
            Mathf.Lerp(crown.anchoredPosition.x, 395 * winningSide, 2 * Time.deltaTime),
            crown.anchoredPosition.y
        );
    }

    public void UpdateScores(float cow, float pug) {

        if (pug > cow) winningSide = -1;
        else if (pug < cow) winningSide = 1;
        else winningSide = 0;

        float total = cow + pug;
        if (total == 0) {
            total = 2;
            pug = 1;
        }

        targetWidth = pug * 750 / total;
    }

}