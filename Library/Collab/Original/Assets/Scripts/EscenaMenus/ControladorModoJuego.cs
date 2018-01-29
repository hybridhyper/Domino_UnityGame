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

	Transform darkenTimeTrial;
	Transform darkenRelax;

	void Start(){
		GetRecords();
        FindObjects();
        darkenRelax.gameObject.SetActive(false);  //Ocultamos los paneles de pop up de modos de juego.
		darkenTimeTrial.gameObject.SetActive(false);
	}

    void FindObjects(){
		GameObject.Find("TextoVidas").GetComponent<Text>().text = "" + PlayerPrefs.GetInt("Lifes"); ;
		GameObject.Find("TextoMonedas").GetComponent<Text>().text = "" + PlayerPrefs.GetInt("Coins");
		darkenTimeTrial = GameObject.Find("OscurecerContrarreloj").transform;
		darkenRelax = GameObject.Find("OscurecerRelax").transform;
    }

    void GetRecords(){
		GameObject.Find("RecordQueNosUne").GetComponent<Text>().text = "Record: " + PlayerPrefs.GetInt(Constantes.RECORD_QUENOSUNE, 0);
		GameObject.Find("RecordJeroglifico").GetComponent<Text>().text = "Record: " + PlayerPrefs.GetInt(Constantes.RECORD_JEROGLIFICO, 0);
		GameObject.Find("RecordCompleta").GetComponent<Text>().text = "Record: " + PlayerPrefs.GetInt(Constantes.RECORD_COMPLETA_FRASE, 0);
    }

    //CONTRARRELOJ
	public void OpenTimeTrialPanel(){
        darkenTimeTrial.gameObject.SetActive(true);
        this.darkenTimeTrial.GetChild(0).GetComponent<Animator>().SetTrigger("Agrandar"); ;

	}

	public void CloseTimeTrialPanel(){
        darkenTimeTrial.gameObject.SetActive(false);
	}

    public void LoadCompletaLaFraseTimeTrial(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
        Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase;
        AutoFade.LoadLevel("JuegoContrarreloj", 0.2f, 0.2f, Color.black);
    }

    public void LoadJeroglificosTimeTrial(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
        Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico;
		AutoFade.LoadLevel("JuegoContrarreloj", 0.2f, 0.2f, Color.black);
    }

    public void LoadQueNosUneTimeTrial(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
        Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne;
		AutoFade.LoadLevel("JuegoContrarreloj", 0.2f, 0.2f, Color.black);
    }

    public void LoadTutorial(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		AutoFade.LoadLevel("Tutorial", 0.2f, 0.2f, Color.black);
    }

    //RELAX
	public void OpenPanelRelax(){
		darkenRelax.gameObject.SetActive(true);
        this.darkenRelax.GetChild(0).GetComponent<Animator>().SetTrigger("Agrandar"); ;
	}

	public void ClosePanelRelax(){
		darkenRelax.gameObject.SetActive(false);
	}

	public void LoadCompletarFrasesRelax(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase;
		AutoFade.LoadLevel("JuegoRelax", 0.2f, 0.2f, Color.black);
	}

	public void LoadJeroglificosRelax(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico;
		AutoFade.LoadLevel("JuegoRelax", 0.2f, 0.2f, Color.black);
	}

	public void LoadQueNosUneRelax(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		Constantes.TYPE_OF_QUESTION = (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne;
		AutoFade.LoadLevel("JuegoRelax", 0.2f, 0.2f, Color.black);
	}

    public void BackToMainMenu(){
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.SaveScene();
		AutoFade.LoadLevel("EscenaInicio", 0.2f, 0.2f, Color.white);
    }
}
