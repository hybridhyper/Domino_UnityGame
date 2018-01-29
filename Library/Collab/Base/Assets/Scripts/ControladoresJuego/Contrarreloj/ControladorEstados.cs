/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ControladorEstados : MonoBehaviour {

    public Text winningTitle; //Titulo Jugador Supera Record / No supera record
    //Toggles para musica y FX
    public Toggle toggleFX;
	public Toggle toggleMusic;
	public AudioMixer musicMixer; //AudioMixer 
	GameObject darkenScreen; //OscurecerPantalla
	GameObject[] states; //Estados del juego
	GameObject[] purchaseStates; //Estados del juego para la compra.
    GameObject objetoSingleton; 
	ControladorJuego gameController;  //Controlador del juego contrarreloj
	Text price20sText; 
    int counterLastChance = 0;
	int initialCostExtraSeconds = 20;
	int extraSeconds = 20;
    bool isCountdown = false;
    float countdownTime = 3;
    public Text countdownTimeText;
    public AudioClip[] soundClips = new AudioClip[2];
    //Enumeraciones para los sonidos, para los estados de compra y para los estados del juego.
    public enum EnumSounds { Loses, Wins, MoreTime };
    public enum EnumPurchase { NoCoins, PurchaseList };
    public enum EnumStates { Initial, PreGame, Paused, Over };

	void Start () {
        FindObjects(); //Buscamos los objetos.
        SearchStates(); //Buscamos los estados del juego.
        AdjustMusic();
	}

    void FindObjects(){
        objetoSingleton = GameObject.Find("ObjetoSingleton");
        objetoSingleton.GetComponent<MusicSingleton>().PlayTimeTrial();
		gameController = GameObject.Find("ControladorJuego").GetComponent<ControladorJuego>();
		darkenScreen = GameObject.Find("OscurecerPantalla");
    }

    void SearchStates(){
        //Buscamos los 3 estados del juego y los metemos en states. Buscamos los 2 estados de compra y los metemos en purchaseStates.
        states = new GameObject[4];
        purchaseStates = new GameObject[2];
		int counter = 0;
		int purchaseCounter = 0;
		foreach(Transform objStates in darkenScreen.transform){
            if(counter < 4){
				states[counter] = objStates.gameObject;
                counter++;
            } else {
                purchaseStates[purchaseCounter] = objStates.gameObject;
                purchaseCounter++;
            }
        }
    }

	void AdjustMusic(){
        //Toggle para FX
		if (PlayerPrefs.GetInt("FX") == 0)
			toggleFX.isOn = false;
		else
			toggleFX.isOn = true;
        //Toggle para musica
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


    public void ShowPause(){ //ESTADO DE PAUSA
		objetoSingleton.GetComponent<AdManager>().ShowBanner();
        darkenScreen.SetActive(true);
        states[(int)EnumStates.Paused].SetActive(true);
        states[(int)EnumStates.Paused].GetComponent<Animator>().SetTrigger("Agrandar");
    }

    public void Resume(){
        darkenScreen.SetActive(false);
        states[(int)EnumStates.Paused].SetActive(false);
        objetoSingleton.GetComponent<AdManager>().HideBanner();
    }

   
    public void StartPlaying(){  //ESTADO INICIAL
		states[(int)EnumStates.Initial].SetActive(false);
        darkenScreen.SetActive(false);
        gameController.GameStarts();   
    }


    //ESTADO ACABADO
    public void StopPlaying(int scoreAntiguo, int scoreNuevo){
        objetoSingleton.GetComponent<AdManager>().ShowBanner();
        states[(int)EnumStates.Paused].SetActive(false);
        counterLastChance++;
        darkenScreen.SetActive(true);
        states[(int)EnumStates.Over].SetActive(true);
		Text textOldScore = GameObject.Find("NumeroAntiguo").GetComponent<Text>();
		Text textNewScore = GameObject.Find("NumeroNuevo").GetComponent<Text>();
		Text textNewTitle = GameObject.Find("NuevoRecord").GetComponent<Text>();
        price20sText = GameObject.Find("Precio20sMas").GetComponent<Text>();
        price20sText.text = "" + initialCostExtraSeconds * counterLastChance;
		textNewScore.text = "" + scoreNuevo;
		textOldScore.text = "" + scoreAntiguo;

        if(scoreNuevo > scoreAntiguo){ //si lo ha superado.
            winningTitle.text = "¡Has superado tu antigua puntuación!";
            PlaySounds((int)EnumSounds.Wins);
            textNewScore.color = Constantes.colorGreen;
            textNewTitle.color = Constantes.colorGreen;
        } else { //no ha superado el record
            winningTitle.text = "No has superado tu record.";
            PlaySounds((int)EnumSounds.Loses);
            textNewScore.color = Constantes.colorRed;
            textNewTitle.color = Constantes.colorRed;
        }
    }

	void PlaySounds(int sound){
        this.transform.GetComponent<AudioSource>().clip = soundClips[sound];
        this.transform.GetComponent<AudioSource>().Play();
    }

    public void ExitGame() {
        AndroidSingleton.Instance.StackBackToMainMenu();
        objetoSingleton.GetComponent<AdManager>().HideBanner();
        objetoSingleton.GetComponent<PreguntasSingleton>().ReorderListOfQuestions();
        objetoSingleton.GetComponent<PreguntasSingleton>().loadSceneAdMob = true;
        objetoSingleton.GetComponent<AdManager>().ShowInterstitial(true);
        #if UNITY_EDITOR
        AutoFade.LoadLevel("EscenaInicio", 0.2f, 0.2f, Color.white);
        #endif
    }

    public void LastChance(){
        int playerCoins = PlayerPrefs.GetInt("Coins"); //Cogemos las monedas que tiene 
        if(playerCoins >= (initialCostExtraSeconds * counterLastChance)){ //Comprobamos que puede gastar el dinero 
            PlayerPrefs.SetInt("Coins", playerCoins - (initialCostExtraSeconds * counterLastChance));
            gameController.ShowCoins();
            PlaySounds((int)EnumSounds.MoreTime);
            isCountdown = true;
            objetoSingleton.GetComponent<AdManager>().HideBanner();
            states[(int)EnumStates.Over].SetActive(false);
            states[(int)EnumStates.PreGame].SetActive(true);
            gameController.RefreshTime(extraSeconds);

        }else{ //No tiene monedas, por tanto, mostramos el panel de que no tiene monedas.
            states[(int)EnumStates.Over].SetActive(false);
            purchaseStates[(int)EnumPurchase.NoCoins].SetActive(true);
            //TODO: Animacion entre pantallas de puntuacion y ¡no tienes monedas!
        }
    }

    void Update() {
        if (isCountdown) {
            countdownTime = countdownTime - 1 * Time.deltaTime;
            countdownTimeText.text = "" +  Mathf.CeilToInt(countdownTime);
            if (countdownTime <= 0) {
                states[(int)EnumStates.PreGame].SetActive(false);
				darkenScreen.SetActive(false);
                gameController.ResumeGame();
                isCountdown = false;
                countdownTime = 3;
            }
                
        }
    }

    public void BackToOver(){
        purchaseStates[(int)EnumPurchase.NoCoins].SetActive(false);
        states[(int)EnumStates.Over].SetActive(true);
    }

    public void GoToShop(){
        purchaseStates[(int)EnumPurchase.NoCoins].SetActive(false);
        purchaseStates[(int)EnumPurchase.PurchaseList].SetActive(true);
    }

    public void BackToNoCoins(){
		purchaseStates[(int)EnumPurchase.PurchaseList].SetActive(false);
        purchaseStates[(int)EnumPurchase.NoCoins].SetActive(true);
    }

    //TODO: Implementar sistema de compras.

    public void Restart(){
        states[(int)EnumStates.Over].SetActive(false);
        states[(int)EnumStates.Initial].SetActive(true);
        objetoSingleton.GetComponent<AdManager>().ShowInterstitial(false);
        objetoSingleton.GetComponent<AdManager>().HideBanner();
        gameController.ResetGame();
    }

	public void BuyCoins(int monedas){
		PlayerPrefs.SetInt ("Coins", PlayerPrefs.GetInt ("Coins") + monedas);
		gameController.ShowCoins ();
	}
}
