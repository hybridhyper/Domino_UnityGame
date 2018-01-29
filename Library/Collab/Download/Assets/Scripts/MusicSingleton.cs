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
