/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/

using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;

public class AdManager : MonoBehaviour
{
	public static AdManager instance { get; set; } //Instancia del AdManager
    //Anuncios que vamos a motrar
	InterstitialAd interstitial; //Objeto del anuncio intersticial
	BannerView bannerView; //Objeto del banner
	RewardBasedVideoAd rewardVideo; //Objeto del video de recompensa
	string bannerID; //ID del banner
	string interstitialID; //ID del intersticial
	string rewardID; //ID del video de recompensa
    enum Rewards { Coins, Lifes };
    int reward; //Valor que utilizamos para diferenciar si el jugador ha visto un video recompensado de Monedas o Vidas.
    bool appStarted = false; //Bool que indica si la aplicación se ha iniciado
    bool backToMenu = false; //Bool que indica si se vuelve al menú tras el intersticial o no
	//BANNER PRUEBA: ca-app-pub-3940256099942544/6300978111
	//INTERTICIAL PRUEBA: ca-app-pub-3940256099942544/1033173712
	//BANNER IOS: ca-app-pub-1913552533139737/6284321189
	//INTERSTICIAL IOS: ca-app-pub-1913552533139737/2657380445
	//BANNER ANDROID: ca-app-pub-1913552533139737/9129142221
	//INTERSTICIAL ANDROID: ca-app-pub-1913552533139737/4806753839

	void Start () {
		//Seleccionamos el ID de cada tipo de anuncio en función de la plataforma que se esté utilizando
		if (Application.platform == RuntimePlatform.Android) {
			bannerID = "ca-app-pub-3940256099942544/6300978111";
			interstitialID = "ca-app-pub-3940256099942544/1033173712";
			rewardID = "ca-app-pub-3940256099942544/5224354917";
		} else if (Application.platform == RuntimePlatform.IPhonePlayer) {
			bannerID = "ca-app-pub-3940256099942544/6300978111";
			interstitialID = "ca-app-pub-3940256099942544/1033173712";
			rewardID = "ca-app-pub-3940256099942544/1712485313";
		} else {
			print ("No play ads on editor");
		}

        //Cargamos los anuncios al principio del juego: es preferible ya que nos aseguramos que estén listos para cuando juegan. 
        RequestBanner();
        RequestInterstitial();
		RequestRewardVideo ();

        appStarted = true;
	}



	public void RequestBanner (){ //Se hace la petición del banner con su ID correspondiente
        bannerView = new BannerView (bannerID, AdSize.SmartBanner, AdPosition.Bottom); // Create a 320x50 banner at the bottom of the screen.										  
		AdRequest request = new AdRequest.Builder ().Build (); // Create an empty ad request
		//Se carga el anuncio
		bannerView.LoadAd (request); // Load the banner with the request.
		bannerView.OnAdLoaded += HandleOnAdLoaded; //Nos suscribimos al evento de Banner ha cargado. 
	}

	public InterstitialAd GetInterstitial (){
		return interstitial;
	}

	public void RequestInterstitial () { //Se hace la petición del intersticial
        interstitial = new InterstitialAd (interstitialID); //Initialize an InterstitialAd								 
		AdRequest request = new AdRequest.Builder ().Build ();  // Create an empty ad request												
		interstitial.LoadAd (request); // Load the interstitial  with the request
		interstitial.OnAdClosed += HandleInterstitialClosed;
	}

	public void RequestRewardVideo (){ //Se hace la petición del vídeo de recompensa
		rewardVideo = RewardBasedVideoAd.Instance; 
		AdRequest request = new AdRequest.Builder ().Build ();
        rewardVideo.LoadAd (request, rewardID); //Cargamos el rewarded video
		rewardVideo.OnAdRewarded += HandleReward; //Nos suscribimos al evento de recompensa tras ser recompensados.
		rewardVideo.OnAdClosed += DestroyRewardVideo; //Nos suscribimos al evento de destruir ad tras cerrar el ad.
	}

	//Función del evento OnAdLoaded del Banner
	public void HandleOnAdLoaded (object sender, EventArgs args){
		HideBanner ();
	}

	//Función que muestra el banner
	public void ShowBanner (){
		bannerView.Show ();
	}

	public void ShowInterstitial (bool goBack){ //Función que muestra el intersticial. Recibe un bool que indica si tras el intersticial se debe volver al menú principal o no.
        backToMenu = goBack;
		if (interstitial.IsLoaded ()) {
			interstitial.Show ();
        } else if (goBack){ //Si el intersticial no ha podido cargarse a tiempo se pasa directamente al menú principal
			AutoFade.LoadLevel ("EscenaInicio", 0.2f, 0.2f, Color.white);
		}
	}

	public void ShowRewardVideoLifes () { //Se muestra el video de recompensa para las vidas
        reward = (int)Rewards.Lifes;
		if (rewardVideo.IsLoaded ()) {
			rewardVideo.Show ();
		}
	}

	public void ShowRewardVideoCoins (){ //Se muestra el vídeo de recompensa para las monedas
        reward = (int)Rewards.Coins;
		if (rewardVideo.IsLoaded ()) {
            rewardVideo.Show ();
		}
	}

    //En este metodo recompensamos al jugador tras haber visto el video.    
	void HandleReward (object sender, Reward reward_event){
        if (reward == (int)Rewards.Coins) { //reward_event contene información del propio admob como la cantidad a entregar
            PlayerPrefs.SetInt("Coins", (int)reward_event.Amount + PlayerPrefs.GetInt("Coins"));
			GameObject.Find ("TextoMonedas").GetComponent<Text> ().text = PlayerPrefs.GetInt ("Coins") + "";
		} else { 
            PlayerPrefs.SetInt("Lifes", (int)reward_event.Amount + PlayerPrefs.GetInt("Lifes"));
			GameObject.Find ("TextoVidas").GetComponent<Text> ().text = PlayerPrefs.GetInt ("Lifes") + "";
		}
	}

	public void HideBanner (){ //Se oculta el banner (el banner no se destruye, solo se oculta)
		bannerView.Hide ();
	}

	//Método que controla el cierre del anuncio intersticial
    void HandleInterstitialClosed (object sender, EventArgs args){
		interstitial.OnAdClosed -= HandleInterstitialClosed; //Se elimina la suscripción al evento
		interstitial.Destroy(); //Se destruye el intersticial
        RequestInterstitial(); //Se pide un nuevo anuncio intersticial
		if(backToMenu) //Si hay que volver al menú principal se carga la escena
		    AutoFade.LoadLevel ("EscenaInicio", 0.2f, 0.2f, Color.white);
	}

	//Método que se ejecuta al cerrar el vídeo de recompensa
	void DestroyRewardVideo (object sender, EventArgs args){
		rewardVideo.OnAdRewarded -= HandleReward; //Se eliminan las suscripciones a los eventos
		rewardVideo.OnAdClosed -= DestroyRewardVideo;
		RequestRewardVideo (); //Se pide el siguiente video
	}

    //Si la aplicacion se pone en suspendida o en background, eliminamos los banners. 
	void OnApplicationPause (bool pause){ 
        if (Application.platform == RuntimePlatform.Android) {
            OutOfApp(pause);
        }
        else {
            //Si es Iphone Do Nothing
        }
	}

    void OnApplicationQuit(){
        OutOfApp(true);
    }


    void OutOfApp(bool pause) { //Se llama en OnApplicationPause
		if (pause) {
			if (bannerView != null) { //Si el banner está cargado, se elimina
				bannerView.OnAdLoaded -= HandleOnAdLoaded;
				bannerView.Destroy();
			}
		}
		else {
			//Al regresar a la aplicación se vuelve a pedir el banner
			if (appStarted)
				RequestBanner();
		}
    }

    void SetLastTimePlayed() {
        PlayerPrefs.SetInt("LastOpened_Year", System.DateTime.Now.Year);
        PlayerPrefs.SetInt("LastOpened_Month", System.DateTime.Now.Month);
        PlayerPrefs.SetInt("LastOpened_Day", System.DateTime.Now.Day);
        PlayerPrefs.SetInt("LastOpened_Hours", System.DateTime.Now.Hour);
        PlayerPrefs.SetInt("LastOpened_Minutes", System.DateTime.Now.Minute);
        PlayerPrefs.SetInt("LastOpened_Seconds", System.DateTime.Now.Second);
    }

}
