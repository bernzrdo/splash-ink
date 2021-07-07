using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Hub_Settings : MonoBehaviour {

    public Player_Controller player;
    public Multiplayer multiplayer;
    public AudioSource sfxSource;

    [Header("Discord")]
    public GameObject discordLoading;
    public GameObject discordJoin;
    public GameObject discordCode;
    public Text discordCodeDisplay;
    public GameObject discordError;
    public Text discordErrorDescription;
    public GameObject discordUser;
    public Text discordUsername;
    public Image discordProfilePic;

    [Header("Controls")]
    public InputField mouseSensitivityInput;
    public Slider mouseSensitivitySlider;
    public InputField bobbingAmountInput;
    public Slider bobbingAmountSlider;

    [Header("FOV")]
    public InputField fovInput;
    public Slider fovSlider;
    public InputField dynamicFovInput;
    public Slider dynamicFovSlider;

    [Header("Volume")]
    public InputField sfxInput;
    public Slider sfxSlider;
    public InputField musicInput;
    public Slider musicSlider;

    string profilePicToLoad;

    void Start(){
        RenderSettings();
    }

    void Update() {
        if (gameObject.activeSelf && Input.GetKeyDown(KeyCode.Escape)) Close();
    }

    // --- UTILITIES ---

    void DiscordShow(GameObject panel) {
        discordLoading.SetActive(false);
        discordJoin.SetActive(false);
        discordCode.SetActive(false);
        discordError.SetActive(false);
        discordUser.SetActive(false);
        panel.SetActive(true);
    }

    void RenderSettings() {

        // -- CONTROLS --

        // Mouse Sensitivity
        if (!PlayerPrefs.HasKey("mouseSensitivity")) PlayerPrefs.SetInt("mouseSensitivity", 50);
        mouseSensitivityInput.text = PlayerPrefs.GetInt("mouseSensitivity").ToString();
        mouseSensitivitySlider.value = PlayerPrefs.GetInt("mouseSensitivity");

        // Bobbing Amount
        if (!PlayerPrefs.HasKey("bobbingAmount")) PlayerPrefs.SetInt("bobbingAmount", 10);
        bobbingAmountInput.text = PlayerPrefs.GetInt("bobbingAmount").ToString();
        bobbingAmountSlider.value = PlayerPrefs.GetInt("bobbingAmount");

        // -- FIELD OF VIEW --

        // Field of View
        if (!PlayerPrefs.HasKey("fov")) PlayerPrefs.SetInt("fov", 70);
        fovInput.text = PlayerPrefs.GetInt("fov").ToString();
        fovSlider.value = PlayerPrefs.GetInt("fov");

        // Dynamic FOV
        if (!PlayerPrefs.HasKey("dynamicFov")) PlayerPrefs.SetInt("dynamicFov", 20);
        dynamicFovInput.text = PlayerPrefs.GetInt("dynamicFov").ToString();
        dynamicFovSlider.value = PlayerPrefs.GetInt("dynamicFov");

        // -- VOLUME --

        // Sound Effects
        if (!PlayerPrefs.HasKey("sfx")) PlayerPrefs.SetInt("sfx", 100);
        sfxInput.text = PlayerPrefs.GetInt("sfx").ToString();
        sfxSlider.value = PlayerPrefs.GetInt("sfx");

        // Music
        if (!PlayerPrefs.HasKey("music")) PlayerPrefs.SetInt("music", 50);
        musicInput.text = PlayerPrefs.GetInt("music").ToString();
        musicSlider.value = PlayerPrefs.GetInt("music");

    }

    // --- OPEN & CLOSE ---

    public void Open() {
        gameObject.SetActive(true);
        if (profilePicToLoad != null) StartCoroutine(DiscordLoadProfilePic(profilePicToLoad));
        player.UnlockCursor();
    }

    public void Close() {
        gameObject.SetActive(false);
        player.LockCursor();
    }

    // --- DISCORD ---

    public void DiscordReady() {
        if (PlayerPrefs.HasKey("discord")) multiplayer.Send("discord i-am '"+PlayerPrefs.GetString("discord")+"'");
        else DiscordShow(discordJoin);
    }

    public void DiscordInfo(string username, string profilePicURL) {
        discordUsername.text = username;
        if (gameObject.activeSelf) StartCoroutine(DiscordLoadProfilePic(profilePicURL));
        else profilePicToLoad = profilePicURL;
        DiscordShow(discordUser);
    }

    public void DiscordNotFound() {
        PlayerPrefs.DeleteKey("discord");
        DiscordShow(discordJoin);
    }

    IEnumerator DiscordLoadProfilePic(string url) {
        profilePicToLoad = null;
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(url);
        yield return www.SendWebRequest();
        Texture img = DownloadHandlerTexture.GetContent(www);
        discordProfilePic.sprite = Sprite.Create((Texture2D)img, new Rect(0, 0, img.width, img.height), Vector2.zero);
    }

    public void DiscordJoin() {
        Application.OpenURL("https://discord.gg/dK5PqYDMFW");
    }

    public void DiscordGenerateCode() {
        multiplayer.Send("discord code");
        DiscordShow(discordLoading);
    }

    public void DiscordCode(string code) {
        discordCodeDisplay.text = ".link " + code;
        DiscordShow(discordCode);
    }

    public void DiscordCopyCode() {
        GUIUtility.systemCopyBuffer = discordCodeDisplay.text;
    }

    public void DiscordCloseCode() {
        DiscordShow(discordJoin);
    }

    public void DiscordLink(string discord) {
        PlayerPrefs.SetString("discord", discord);
        DiscordReady();
    }

    public void DiscordForget() {
        PlayerPrefs.DeleteKey("discord");
        multiplayer.Send("discord forget");
        DiscordShow(discordJoin);
    }

    public void Error(string error) {
        DiscordShow(discordError);
        if (discordErrorDescription == null) return;
        discordErrorDescription.text = error;
    }

    public void Reconnect() {
        DiscordShow(discordLoading);
        multiplayer.Reconnect();
    }

    // --- CONTROLS ---

    public void MouseSensitivityInput() {
        PlayerPrefs.SetInt("mouseSensitivity", Mathf.Clamp(int.Parse(mouseSensitivityInput.text), (int)mouseSensitivitySlider.minValue, (int)mouseSensitivitySlider.maxValue));
        RenderSettings();
    }

    public void MouseSensitivitySlider() {
        PlayerPrefs.SetInt("mouseSensitivity", (int)mouseSensitivitySlider.value);
        RenderSettings();
    }

    public void BobbingAmountInput() {
        PlayerPrefs.SetInt("bobbingAmount", Mathf.Clamp(int.Parse(bobbingAmountInput.text), (int)bobbingAmountSlider.minValue, (int)bobbingAmountSlider.maxValue));
        RenderSettings();
    }

    public void BobbingAmountSlider() {
        PlayerPrefs.SetInt("bobbingAmount", (int)bobbingAmountSlider.value);
        RenderSettings();
    }

    // --- FIELD OF VIEW ---

    public void FovInput() {
        PlayerPrefs.SetInt("fov", Mathf.Clamp(int.Parse(fovInput.text), (int)fovSlider.minValue, (int)fovSlider.maxValue));
        RenderSettings();
    }

    public void FovSlider() {
        PlayerPrefs.SetInt("fov", (int)fovSlider.value);
        RenderSettings();
    }

    public void DynamicFovInput() {
        PlayerPrefs.SetInt("dynamicFov", Mathf.Clamp(int.Parse(dynamicFovInput.text), (int)dynamicFovSlider.minValue, (int)dynamicFovSlider.maxValue));
        RenderSettings();
    }

    public void DynamicFovSlider() {
        PlayerPrefs.SetInt("dynamicFov", (int)dynamicFovSlider.value);
        RenderSettings();
    }

    // --- VOLUME ---

    public void SfxInput() {
        PlayerPrefs.SetInt("sfx", Mathf.Clamp(int.Parse(sfxInput.text), (int)sfxSlider.minValue, (int)sfxSlider.maxValue));
        sfxSource.volume = ((float)PlayerPrefs.GetInt("sfx")) / 100;
        RenderSettings();
    }

    public void SfxSlider() {
        PlayerPrefs.SetInt("sfx", (int)sfxSlider.value);
        sfxSource.volume = ((float)PlayerPrefs.GetInt("sfx")) / 100;
        RenderSettings();
    }

    public void MusicInput() {
        PlayerPrefs.SetInt("music", Mathf.Clamp(int.Parse(musicInput.text), (int)musicSlider.minValue, (int)musicSlider.maxValue));
        RenderSettings();
    }

    public void MusicSlider() {
        PlayerPrefs.SetInt("music", (int)musicSlider.value);
        RenderSettings();
    }

}