/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using UnityEngine;

public class MusicSingleton : MonoBehaviour {
    //MusicSingleton 2 de noviembre
    public AudioClip[] music; //Array de los clips de'música de fondo
    public enum enumAudios { //Enumerador de los distintos clips de música de fondo
        MenuMusic,
        TimeTrialMusic,
        RelaxMusic,
        ClassicMusic
    }



	public void PlayMenu() { //Se reproduce la música del menú (Si no se está reproduciendo ya)
        if(this.transform.GetComponent<AudioSource>().clip != music[(int)enumAudios.MenuMusic]){
			this.transform.GetComponent<AudioSource>().clip = music[(int)enumAudios.MenuMusic];
			this.transform.GetComponent<AudioSource>().Play();
        }
    }

    public void PlayRelax(){ //Se reproduce la música de relax
        this.transform.GetComponent<AudioSource>().clip = music[(int)enumAudios.RelaxMusic];
		this.transform.GetComponent<AudioSource>().Play();
    }

    public void PlayTimeTrial(){ //Se reproduce la música de contrarreloj
        this.transform.GetComponent<AudioSource>().clip = music[(int)enumAudios.TimeTrialMusic];
		this.transform.GetComponent<AudioSource>().Play();
    }

    public void PlayClassic(){
        //TODO: Poner musica para el modo clasico. 
    }
}
