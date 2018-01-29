/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ControladorEstadosRelax : MonoBehaviour
{
	public Toggle toggleFX;
	public Toggle toggleMusic;
	public AudioMixer musicMixer;
    GameObject hiddenScreen;
	GameObject[] states;
    GameObject singletonObject;
    ControladorjuegoRelax gameController;
    public enum StatesEnum {Initial, Pause, Shop};

	void Start() {
        SearchObjects();
		SearchStates();
        AdjustMusic();
	}

    void SearchObjects(){
        singletonObject = GameObject.Find("ObjetoSingleton");
        singletonObject.GetComponent<MusicSingleton>().PlayRelax();
		gameController = GameObject.Find("ControladorJuego").GetComponent<ControladorjuegoRelax>();
		hiddenScreen = GameObject.Find("OscurecerPantalla");
    }

    void SearchStates() {
		states = new GameObject[4];
        int counter = 0;
		foreach (Transform estadosobj in hiddenScreen.transform) {
			states[counter] = estadosobj.gameObject;
			counter++;
		}
	}

	void AdjustMusic(){
		if (PlayerPrefs.GetInt("FX") == 0)
			toggleFX.isOn = false;
		else
			toggleFX.isOn = true;
        
        if (PlayerPrefs.GetInt("MUSIC") == 0)
            toggleMusic.isOn = false;
		else
			toggleMusic.isOn = true;
		
	}

	public void ValueChangedFX(){
		if (toggleFX.isOn){
			PlayerPrefs.SetInt("FX", 1);
			musicMixer.SetFloat("FXMusica", -1);
		}else{
			PlayerPrefs.SetInt("FX", 0);
			musicMixer.SetFloat("FXMusica", -80);
		}
	}

	public void ValueChangedMusic(){
		if (toggleMusic.isOn){
			PlayerPrefs.SetInt("MUSIC", 1);
			musicMixer.SetFloat("FondoMusica", -5);
		}else{
			PlayerPrefs.SetInt("MUSIC", 0);
			musicMixer.SetFloat("FondoMusica", -80);
		}
	}

	//ESTADO DE PAUSA
    public void ShowPause(){
		hiddenScreen.SetActive(true);
		states[(int)StatesEnum.Pause].SetActive(true);
        states[(int)StatesEnum.Pause].GetComponent<Animator>().SetTrigger("Agrandar");
        //singletonObject.GetComponent<AdManager>().ShowBanner();
	}

    public void QuitPause(){
		hiddenScreen.SetActive(false);
		states[(int)StatesEnum.Pause].SetActive(false);
        //singletonObject.GetComponent<AdManager>().HideBanner();
	}

	//ESTADO INICIAL
    public void StartPlaying(){
		states[(int)StatesEnum.Initial].SetActive(false);
		hiddenScreen.SetActive(false);
        gameController.GameStarts();
	}

	public void ShowShop(){
		hiddenScreen.SetActive (true);
		states [(int)StatesEnum.Shop].SetActive (true);
		states[(int)StatesEnum.Shop].GetComponent<Animator>().SetTrigger("Agrandar");
	}

	public void HideShop(){
		states [(int)StatesEnum.Shop].SetActive (false);
		hiddenScreen.SetActive (false);
	}

    //SALIR DEL JUEGO
    public void OutOfGame(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.StackBackToMainMenu();
        //singletonObject.GetComponent<AdManager>().HideBanner();
        //singletonObject.GetComponent<AdManager>().ShowInterstitial(true);
		singletonObject.GetComponent<PreguntasSingleton>().ReorderListOfQuestions();
        //#if UNITY_EDITOR
        		AutoFade.LoadLevel("EscenaInicio", 0.2f, 0.2f, Color.white);
        //#endif
	}
}
