/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ControladorModoJuego : MonoBehaviour {

    GameObject coinsForTutorial; //Mensaje que indica si el tutorial recompensa o no con monedas. La primera vez se regalan 100 monedas por completarlo
	Transform darkenTimeTrial; //Panel de Contrarreloj
	Transform darkenRelax; //Panel de Relax
	public bool panelIsOpen = false; //Bool que indica si hay algún panel abierto.

	void Start(){
		GetRecords(); //Obtenemos los records del modo contrarreloj
        FindObjects();
        darkenRelax.gameObject.SetActive(false);  //Ocultamos los paneles de pop up de modos de juego.
		darkenTimeTrial.gameObject.SetActive(false); //Quitamos la pantalla oscurecida
        CheckTutorialDone(); //Se comprueba si el tutorial se ha realizado ya o no.
	}

    void FindObjects(){
		UpdatePlayerCurrency ();
		darkenTimeTrial = GameObject.Find("OscurecerContrarreloj").transform;
		darkenRelax = GameObject.Find("OscurecerRelax").transform;
        GameObject.Find("ObjetoSingleton").GetComponent<AndroidSingleton>().SetController();
        coinsForTutorial = GameObject.Find("Bocadillo");
    }

	void UpdatePlayerCurrency(){
		GameObject.Find ("TextoMonedas").GetComponent<Text> ().text = PlayerPrefs.GetInt ("Coins") + "";
		GameObject.Find ("TextoVidas").GetComponent<Text> ().text = PlayerPrefs.GetInt ("Lifes") + "";
	}

    void CheckTutorialDone() {
		//Si el jugador ya ha completado el tutorial se quita el mensaje de las monedas de recompensa.
        if (PlayerPrefs.GetInt("TutorialDone") == 1) {
            coinsForTutorial.SetActive(false);
        } else {
            coinsForTutorial.SetActive(true);
        }
    }

    void GetRecords(){ //Se obtienen los record de PlayerPrefs y se actualizan los textos que los muestran
		GameObject.Find("RecordQueNosUne").GetComponent<Text>().text = "Record: " + PlayerPrefs.GetInt(Constantes.RECORD_QUENOSUNE, 0);
		GameObject.Find("RecordJeroglifico").GetComponent<Text>().text = "Record: " + PlayerPrefs.GetInt(Constantes.RECORD_JEROGLIFICO, 0);
		GameObject.Find("RecordCompleta").GetComponent<Text>().text = "Record: " + PlayerPrefs.GetInt(Constantes.RECORD_COMPLETA_FRASE, 0);
    }

    //CONTRARRELOJ
	public void OpenTimeTrialPanel(){
		//Se activa el panel de contrarreloj
        darkenTimeTrial.gameObject.SetActive(true);
		panelIsOpen = true;
		//Se activa la animación del menú.
        this.darkenTimeTrial.GetChild(0).GetComponent<Animator>().SetTrigger("Agrandar"); ;

	}

	public void CloseTimeTrialPanel(){
		//Se cierra el panel de contrarreloj
        darkenTimeTrial.gameObject.SetActive(false);
		panelIsOpen = false;
	}

    public void LoadCompletaLaFraseTimeTrial(){ //Se inicia el juego de contrarreloj
		//Si la plataforma es Android, se guarda la escena en la pila
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		//Se guarda en constantes el tipo de pregunta que se ha elegido (en este caso Completa la frase)
        Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase;
		//Se carga el juego de contrarreloj
        AutoFade.LoadLevel("JuegoContrarreloj", 0.2f, 0.2f, Color.black);
    }

	public void LoadJeroglificosTimeTrial(){ //Se inicia el juego de contrarreloj
		//Si la plataforma es Android, se guarda la escena en la pila
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		//Se guarda en constantes el tipo de pregunta que se ha elegido (en este caso Jeroglífico)
        Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico;
		//Se carga el juego de contrarreloj
		AutoFade.LoadLevel("JuegoContrarreloj", 0.2f, 0.2f, Color.black);
    }

	public void LoadQueNosUneTimeTrial(){ //Se inicia el juego de contrarreloj
		//Si la plataforma es Android, se guarda la escena en la pila
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		//Se guarda en constantes el tipo de pregunta que se ha elegido (en este caso Jeroglífico)
        Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne;
		//Se carga el juego de contrarreloj
		AutoFade.LoadLevel("JuegoContrarreloj", 0.2f, 0.2f, Color.black);
    }

    public void LoadTutorial(){ //Se carga el tutorial
		//Si la plataforma es Android, se guarda la escena en la pila
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		//Se carga el tutorial
		AutoFade.LoadLevel("Tutorial", 0.2f, 0.2f, Color.black);
    }

    //RELAX
	public void OpenPanelRelax(){ //Se abre el panel de relax
		darkenRelax.gameObject.SetActive(true);
		panelIsOpen = true;
		//Se inicia la animación de apertura del panel
        this.darkenRelax.GetChild(0).GetComponent<Animator>().SetTrigger("Agrandar"); ;
	}

	public void ClosePanelRelax(){ //Se cierra el panel de relax
		panelIsOpen = false;
		darkenRelax.gameObject.SetActive(false);
	}

	public void LoadCompletarFrasesRelax(){ //Se carga el juego de relax
		//Si la plataforma es Android se guarda la escena en la pila
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		//Se guarda en constantes el tipo de pregunta que se ha elegido (en este caso Completa la frase)
		Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase;
		//Se carga el juego de relax
		AutoFade.LoadLevel("JuegoRelax", 0.2f, 0.2f, Color.black);
	}

	public void LoadJeroglificosRelax(){ //Se carga el juego de relax
		//Si la plataforma es Android se guarda la escena en la pila
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		//Se guarda en constantes el tipo de pregunta que se ha elegido (en este caso Jeroglífico)
		Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico;
		//Se carga el juego de relax
		AutoFade.LoadLevel("JuegoRelax", 0.2f, 0.2f, Color.black);
	}

	public void LoadQueNosUneRelax(){ //Se carga el juego de relax
		//Si la plataforma es Android se guarda la escena en la pila
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		//Se guarda en constantes el tipo de pregunta que se ha elegido (en este caso Que nos une)
		Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne;
		//Se carga el juego de relax
		AutoFade.LoadLevel("JuegoRelax", 0.2f, 0.2f, Color.black);
	}

    public void BackToMainMenu(){ //Se regresa al menú principal
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.StackBackToMainMenu ();
		AutoFade.LoadLevel("EscenaInicio", 0.2f, 0.2f, Color.white);
    }
}
