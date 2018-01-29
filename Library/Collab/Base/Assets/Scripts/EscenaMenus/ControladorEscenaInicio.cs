/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using UnityEngine;
using UnityEngine.UI;  
using UnityEngine.Audio;
using UnityEngine.iOS;


public class ControladorEscenaInicio : MonoBehaviour {

    public Toggle toggleFX;
    public Toggle toggleMusic;
	public AudioMixer musicMixer;
    GameObject PanelTutorial;
	bool checkGUI = false;
    Text textoDebug;
    bool tokenSent = false;

	void Start () {
        SearchGameObjects();
        AdjustMusic();
        FirstTimePlaying();
        //TODO: Controlar cuantas monedas / vidas le vamos a dar al jugador.
		PlayerPrefs.SetInt("Lifes", 5); 
		PlayerPrefs.SetInt("Coins", 10);
        //UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound, true);  
	}

    /*private void Update()
    {
		if (!tokenSent) {
			byte[] token = UnityEngine.iOS.NotificationServices.deviceToken;
            if (token != null) {
                Text respuesta = GameObject.Find("Titulo").GetComponent<Text>();
                respuesta.text = "found";
                tokenSent = true;
            }
            else {
				Text respuesta = GameObject.Find("Titulo").GetComponent<Text>();
				respuesta.text = "not found";
            }
		}
    }*/

    void FirstTimePlaying(){
        if (PlayerPrefs.GetInt("FirstTime") == 0){
            GameObject.Find("OscurecerTutorial").SetActive(true);
        } else {
            GameObject.Find("OscurecerTutorial").SetActive(false);
        }
    }


    void AdjustMusic(){
		if (PlayerPrefs.GetInt("FX") == 0){
			toggleFX.isOn = false;
		}else{
			toggleFX.isOn = true;
			musicMixer.SetFloat("FXMusica", -1);
		}

		if (PlayerPrefs.GetInt("MUSIC") == 0){
			toggleMusic.isOn = false;
		}else{
			toggleMusic.isOn = true;
			musicMixer.SetFloat("FondoMusica", -5);
		}

        GameObject.Find("ObjetoSingleton").GetComponent<MusicSingleton>().PlayMenu();
    }
		
    void SearchGameObjects(){
        PanelTutorial = GameObject.Find("OscurecerTutorial");
		GameObject.Find("TextoVidas").GetComponent<Text>().text = "" + PlayerPrefs.GetInt("Lifes");
		GameObject.Find("TextoMonedas").GetComponent<Text>().text = "" + PlayerPrefs.GetInt("Coins");
    }

    public void LoadGameMode(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene ();
		AutoFade.LoadLevel("ModosJuego", 0.2f, 0.2f, Color.white);
    }

    public void openInfo(){
		Application.OpenURL("https://twitter.com/duwafarm");

	}

	public void LoadShop(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		AutoFade.LoadLevel("Tienda", 0.2f, 0.2f, Color.white);
	}

    public void ValueChangedFX(){
        if(toggleFX.isOn){
            PlayerPrefs.SetInt("FX", 1);
			musicMixer.SetFloat ("FXMusica", -1);
        } else {
            PlayerPrefs.SetInt("FX", 0);
			musicMixer.SetFloat ("FXMusica", -80);
        }
    }

    public void ValueChangedMusic(){
		if (toggleMusic.isOn) {
			PlayerPrefs.SetInt("MUSIC", 1);
			musicMixer.SetFloat ("FondoMusica", -5);
		}
		else {
			PlayerPrefs.SetInt("MUSIC", 0);
			musicMixer.SetFloat ("FondoMusica", -80);
		}
    }

    public void CloseTutorialPanel(){
        PanelTutorial.SetActive(false);
		PlayerPrefs.SetInt ("FirstTime", 1);
    }

    public void GoToTutorial(){
		AutoFade.LoadLevel("Tutorial", 0.2f, 0.2f, Color.black);
		PlayerPrefs.SetInt("FirstTime", 1);
    }

   /* public void NotificateUser() {
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
			UnityEngine.iOS.LocalNotification notif = new UnityEngine.iOS.LocalNotification();
            textoDebug.text = "NOTIF CREADO";
            notif.fireDate = System.DateTime.Now.AddSeconds(15);
            textoDebug.text = "tiempo añadido";
			notif.alertAction = "Domino";
			notif.alertBody = "Prueba";
            notif.soundName = UnityEngine.iOS.LocalNotification.defaultSoundName;
            textoDebug.text = "ALERT ACTION CREADO";
            UnityEngine.iOS.NotificationServices.PresentLocalNotificationNow(notif);
			//UnityEngine.iOS.NotificationServices.ScheduleLocalNotification(notif);
            textoDebug.text = "muestra notification";
            textoDebug.text = "" + UnityEngine.iOS.NotificationServices.deviceToken;
			//textoDebug.text = UnityEngine.iOS.NotificationServices.localNotifications[0].alertBody;
        }
    }*/

}
