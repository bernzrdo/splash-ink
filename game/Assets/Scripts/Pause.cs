using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Pause : MonoBehaviour {

    public Button exitButton;
    public Player_Controller player;
    public Multiplayer multiplayer;
    public Hub_Settings settings;

    public bool paused = false;

    void Update(){
        #if UNITY_EDITOR
            if (Input.GetKeyDown(KeyCode.P)) {
        #else
            if (Input.GetKeyDown(KeyCode.Escape)) {
        #endif
                if (settings.gameObject.activeSelf) return;
                paused = !paused;

                if (paused) {
                    player.UnlockCursor();
                    exitButton.interactable = multiplayer.inGame;
                    foreach (Transform child in transform) child.gameObject.SetActive(true);
                } else Resume();
            }
    }

    public void Resume() {
        paused = false;
        foreach (Transform child in transform) child.gameObject.SetActive(false);
        player.LockCursor();
    }

    public void Settings() {
        Resume();
        settings.Open();
    }

    public void Exit() {
        if (!multiplayer.inGame) return;
        Resume();
        multiplayer.transition.Close();
        player.canControl = false;
        StartCoroutine(ExitDelay());
    }

    IEnumerator ExitDelay() {
        yield return new WaitForSeconds(1.5f);
        multiplayer.ExitGame();
    }

    public void Quit() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

}