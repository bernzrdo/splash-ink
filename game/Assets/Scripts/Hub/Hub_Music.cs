using UnityEngine;

public class Hub_Music : MonoBehaviour {

	public AudioClip[] playlist;
	public AudioSource elevator;
	public AudioSource hum;
	public bool fadeOutElevator = false;

	AudioSource source;
	int index = -1;

	void Start() {

		source = GetComponent<AudioSource>();

		for (int i = playlist.Length - 1; i > 0; i--) {
			int r = Random.Range(0, i);
			AudioClip t = playlist[i];
			playlist[i] = playlist[r];
			playlist[r] = t;
		}
	}

    void Update() {
		source.volume = (float)(PlayerPrefs.HasKey("music") ? PlayerPrefs.GetInt("music") : 50) / 100;
		hum.volume = (float)(PlayerPrefs.HasKey("sfx") ? PlayerPrefs.GetInt("sfx") : 100) / 100;

		if(fadeOutElevator) elevator.volume = Mathf.Lerp(elevator.volume, 0, 3 * Time.deltaTime);
		else elevator.volume = source.volume;

		if (source.isPlaying) return;
		if (++index > 3) index = 0;
		source.clip = playlist[index];
		source.Play();
	}

}
