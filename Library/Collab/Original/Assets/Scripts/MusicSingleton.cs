using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicSingleton : MonoBehaviour {

    private static MusicSingleton instance = null;
    public AudioClip[] music;
    public enum enumAudios
    {
        MenuMusic,
        TimeTrialMusic,
        RelaxMusic,
        ClassicMusic
    }

	Stack stackOfScenes;


    public static MusicSingleton Instance
	{
		get { return instance; }
	}

    private void OnEnable()
    {
		if (instance != null && instance != this)
		{
			Destroy(this.gameObject);
			return;
		}
		else
		{
			instance = this;
		}

		DontDestroyOnLoad(this.gameObject);
    }

	void Start(){
		stackOfScenes = new Stack ();
	}

	void Update(){
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (stackOfScenes.Count == 0) {
				if (Application.platform == RuntimePlatform.Android) {
					AndroidJavaObject activity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject> ("currentActivity");
					activity.Call<bool> ("moveTaskToBack", true);
				}
			} else {
				print ((int)stackOfScenes.Peek ());
				SceneManager.LoadScene ((int)stackOfScenes.Pop());
			}
		}
	}

	public void SaveScene(){
		stackOfScenes.Push (SceneManager.GetActiveScene().buildIndex);
	}

	public void StackBackToMainMenu(){
		stackOfScenes.Clear ();
	}

    public void PlayMenu()
    {
        if(this.transform.GetComponent<AudioSource>().clip != music[(int)enumAudios.MenuMusic]){
			this.transform.GetComponent<AudioSource>().clip = music[(int)enumAudios.MenuMusic];
			this.transform.GetComponent<AudioSource>().Play();
        }
    }

    public void PlayRelax(){
        this.transform.GetComponent<AudioSource>().clip = music[(int)enumAudios.RelaxMusic];
		this.transform.GetComponent<AudioSource>().Play();
    }

    public void PlayTimeTrial(){
        this.transform.GetComponent<AudioSource>().clip = music[(int)enumAudios.TimeTrialMusic];
		this.transform.GetComponent<AudioSource>().Play();
    }

    public void PlayClassic(){
        
    }
}
