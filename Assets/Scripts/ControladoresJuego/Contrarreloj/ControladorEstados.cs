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
	Text textNewScore;
	Text textNewTitle;
    int counterLastChance = 0;
	int initialCostExtraSeconds = 20;
	int extraSeconds = 20;
    bool isCountdown = false;
    bool timeAnimationDone = false; 
    float countdownTime = 3;
    public Text countdownTimeText;
    public AudioClip[] soundClips = new AudioClip[2];
    //Enumeraciones para los sonidos, para los estados de compra y para los estados del juego.
    public enum EnumSounds { Loses, Wins, MoreTime };
    public enum EnumPurchase {PurchaseList };
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
		//objetoSingleton.GetComponent<AdManager>().ShowBanner();
        darkenScreen.SetActive(true);
        states[(int)EnumStates.Paused].SetActive(true);
        states[(int)EnumStates.Paused].GetComponent<Animator>().SetTrigger("Agrandar");
    }

    public void Resume(){
        darkenScreen.SetActive(false);
        states[(int)EnumStates.Paused].SetActive(false);
        //objetoSingleton.GetComponent<AdManager>().HideBanner();
    }

    public void ShowInitial() {
        states[(int)EnumStates.Initial].SetActive(true);
    }
   
    public void StartPlaying(){  //ESTADO INICIAL
		states[(int)EnumStates.Initial].SetActive(false);
        if (!timeAnimationDone) {
            GameObject.Find("ContenedorTiempo").transform.SetSiblingIndex(GameObject.Find("ContenedorTiempo").transform.GetSiblingIndex() - 1); //Le subimos en la jerarquia 
            timeAnimationDone = true;
        }

        darkenScreen.SetActive(false);
        gameController.GameStarts();   
    }


    //ESTADO ACABADO
    public void StopPlaying(int scoreAntiguo, int scoreNuevo){
        print(scoreNuevo + "NUEVO SCORE");
        print(scoreAntiguo + "SCORE ANTIGUP");
        //objetoSingleton.GetComponent<AdManager>().ShowBanner();
        states[(int)EnumStates.Paused].SetActive(false);
        counterLastChance++;
        darkenScreen.SetActive(true);
        states[(int)EnumStates.Over].SetActive(true);
		Text textOldScore = GameObject.Find("NumeroAntiguo").GetComponent<Text>();
		textNewScore = GameObject.Find("NumeroNuevo").GetComponent<Text>();
		textNewTitle = GameObject.Find("NuevoRecord").GetComponent<Text>();
        price20sText = GameObject.Find("Precio20sMas").GetComponent<Text>();
        price20sText.text = "" + initialCostExtraSeconds * counterLastChance;
		StartCoroutine (CountTo (scoreNuevo, 1, scoreAntiguo));
		textOldScore.text = "" + scoreAntiguo;

		winningTitle.text = "Comprobando...";

        if(scoreNuevo > scoreAntiguo){ //si lo ha superado.
            PlaySounds((int)EnumSounds.Wins);
            gameController.SetRecords();
        } else { //no ha superado el record
            PlaySounds((int)EnumSounds.Loses);
        }
    }

	IEnumerator CountTo(int target, int duration, int oldScore){
		int start = 0;
		int incrementScore = 0;
		bool scoreExceeded = false;
		for (float timer = 0; timer < duration; timer += Time.deltaTime) {
			float progress = timer / duration;
			incrementScore = (int)Mathf.Lerp (start, target, progress);
			textNewScore.text = incrementScore + "";
			if (incrementScore >= oldScore && !scoreExceeded) {
				winningTitle.text = "Has superado tu antigua puntuación!";
                textNewScore.color = Constantes.COLOR_GREEN;
                textNewTitle.color = Constantes.COLOR_GREEN;
				scoreExceeded = true;
			}
			yield return null;
		}
		incrementScore = target;
		textNewScore.text = incrementScore + "";
		if (!scoreExceeded) {
			winningTitle.text = "No has superado tu anterior puntuación.";
            textNewScore.color = Constantes.COLOR_RED;
            textNewTitle.color = Constantes.COLOR_RED;
		}
	}

	void PlaySounds(int sound){
        this.transform.GetComponent<AudioSource>().clip = soundClips[sound];
        this.transform.GetComponent<AudioSource>().Play();
    }

    public void ExitGame() {
        gameController.SetStartedGame(false);
        if (Application.platform == RuntimePlatform.Android)
            AndroidSingleton.Instance.StackBackToMainMenu();
        //objetoSingleton.GetComponent<AdManager>().HideBanner();
        objetoSingleton.GetComponent<PreguntasSingleton>().ReorderListOfQuestions();
        objetoSingleton.GetComponent<PreguntasSingleton>().loadSceneAdMob = true;
        //objetoSingleton.GetComponent<AdManager>().ShowInterstitial(true);
        //#if UNITY_EDITOR
        AutoFade.LoadLevel("EscenaInicio", 0.2f, 0.2f, Color.white);
        //#endif
    }

    public void LastChance(){
        int playerCoins = PlayerPrefs.GetInt("Coins"); //Cogemos las monedas que tiene 
        if(playerCoins >= (initialCostExtraSeconds * counterLastChance)){ //Comprobamos que puede gastar el dinero 
            PlayerPrefs.SetInt("Coins", playerCoins - (initialCostExtraSeconds * counterLastChance));
            PlaySounds((int)EnumSounds.MoreTime);
            isCountdown = true;
            //objetoSingleton.GetComponent<AdManager>().HideBanner();
            states[(int)EnumStates.Over].SetActive(false);
            states[(int)EnumStates.PreGame].SetActive(true);
            gameController.ShowCoins();
            gameController.RefreshTime(extraSeconds);
            textNewScore.color = Color.white;
            textNewTitle.color = Color.white;

        }else{ //No tiene monedas, por tanto, mostramos el panel de que no tiene monedas.
            states[(int)EnumStates.Over].SetActive(false);
            purchaseStates[(int)EnumPurchase.PurchaseList].SetActive(true);
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
        purchaseStates[(int)EnumPurchase.PurchaseList].SetActive(false);
        states[(int)EnumStates.Over].SetActive(true);
    }

    //TODO: Implementar sistema de compras.

    public void Restart(){
        counterLastChance = 0;
        states[(int)EnumStates.Over].SetActive(false);
        states[(int)EnumStates.Initial].SetActive(true);
        //objetoSingleton.GetComponent<AdManager>().ShowInterstitial(false);
        objetoSingleton.GetComponent<PreguntasSingleton>().loadSceneAdMob = true;
        //objetoSingleton.GetComponent<AdManager>().HideBanner();
		textNewScore.color = Color.white;
		textNewTitle.color = Color.white;
        gameController.ResetGame();
    }

	public void BuyCoins(int monedas){
		PlayerPrefs.SetInt ("Coins", PlayerPrefs.GetInt ("Coins") + monedas);
		gameController.ShowCoins ();
	}
}
