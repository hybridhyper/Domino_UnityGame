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

    public Toggle toggleFX; //Toggle que silencia o activa los sonidos FX
    public Toggle toggleMusic; //Toggle que silencia o activa la música de fondo
	public AudioMixer musicMixer; //Music Mixer que contrla el volumen de los distintos sonidos
    GameObject PanelTutorial; //Panel inicial del tutorial
    Text textoDebug;

	void Start () {
        SearchGameObjects();
        FirstTimePlaying();
        AdjustMusic();
		UpdatePlayerCurrency ();
        //UnityEngine.iOS.NotificationServices.RegisterForNotifications(UnityEngine.iOS.NotificationType.Alert | UnityEngine.iOS.NotificationType.Badge | UnityEngine.iOS.NotificationType.Sound, true);  
	}



	void UpdatePlayerCurrency(){
		GameObject.Find ("TextoMonedas").GetComponent<Text> ().text = PlayerPrefs.GetInt ("Coins") + "";
		GameObject.Find ("TextoVidas").GetComponent<Text> ().text = PlayerPrefs.GetInt ("Lifes") + "";
	}


    void FirstTimePlaying(){
        if (PlayerPrefs.GetInt("FirstTime") == 0){ //Si es la primera vez que inicia la aplicación se le muestra el panel del tutorial.
            GameObject.Find("OscurecerTutorial").SetActive(true);
			PlayerPrefs.SetInt ("Coins", 20);
			PlayerPrefs.SetInt ("Lifes", 10);
            PlayerPrefs.SetInt("FX", 1);
            PlayerPrefs.SetInt("MUSIC", 1);
        } else {
            GameObject.Find("OscurecerTutorial").SetActive(false);
        }
    }


    void AdjustMusic(){ //Función que activa los sonidos en función de las preferencias edl usuario
		if (PlayerPrefs.GetInt("FX") == 0){ //Si tenía los sonidos FX activados, se sube el volumen
			toggleFX.isOn = false;
		}else{
			toggleFX.isOn = true;
			musicMixer.SetFloat("FXMusica", -1);
		}

		if (PlayerPrefs.GetInt("MUSIC") == 0){ //Si tenía la música de fondo activada, se sube el volumen
			toggleMusic.isOn = false;
		}else{
			toggleMusic.isOn = true;
			musicMixer.SetFloat("FondoMusica", -5);
		}

        GameObject.Find("ObjetoSingleton").GetComponent<MusicSingleton>().PlayMenu(); //Se activa la música del menú
    }

    void SearchGameObjects(){
        PanelTutorial = GameObject.Find("OscurecerTutorial");
		GameObject.Find("TextoVidas").GetComponent<Text>().text = "" + PlayerPrefs.GetInt("Lifes");
		GameObject.Find("TextoMonedas").GetComponent<Text>().text = "" + PlayerPrefs.GetInt("Coins");
    }

    public void LoadGameMode(){
		if (Application.platform == RuntimePlatform.Android) //Si la plataforma es Android, se guarda la escena en la pila de AndroidSingleton
			AndroidSingleton.Instance.SaveScene ();
        
		AutoFade.LoadLevel("ModosJuego", 0.2f, 0.2f, Color.white);
    }

    public void openInfo(){
		Application.OpenURL("https://twitter.com/duwafarm");

	}

	public void LoadShop(){
		if (Application.platform == RuntimePlatform.Android) //Si la plataforma es Android, se guarda la escena en la pila de AndroidSingleton
			AndroidSingleton.Instance.SaveScene();
		AutoFade.LoadLevel("Tienda", 0.2f, 0.2f, Color.white); //Se carga la tienda con una animación de fade
	}

	//Función que se activa al cambiar el valor del toggleFX
    public void ValueChangedFX(){
        if(toggleFX.isOn){ //Si está activado el toggle se sube el volumen
            PlayerPrefs.SetInt("FX", 1);
			musicMixer.SetFloat ("FXMusica", -1);
        } else { //Si está desactivado, se silencia el sonido FX
            PlayerPrefs.SetInt("FX", 0);
			musicMixer.SetFloat ("FXMusica", -80);
        }
    }

	//Función que se activa al cambiar el valor del toggleMusic
    public void ValueChangedMusic(){
		if (toggleMusic.isOn) { //Si está activado el toggle se sube el volumen
			PlayerPrefs.SetInt("MUSIC", 1);
			musicMixer.SetFloat ("FondoMusica", -5);
		}
		else { //Si está activado el toggle se sube el volumen
			PlayerPrefs.SetInt("MUSIC", 0);
			musicMixer.SetFloat ("FondoMusica", -80);
		}
    }

    public void CloseTutorialPanel(){ //Se cierra el panel del tutorial
        PanelTutorial.SetActive(false);
		PlayerPrefs.SetInt ("FirstTime", 1);
    }

    public void GoToTutorial(){ //Se carga la escena del tutorial
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
