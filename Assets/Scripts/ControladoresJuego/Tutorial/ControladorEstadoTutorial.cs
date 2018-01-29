/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ControladorEstadoTutorial : MonoBehaviour {
	public Toggle toggleFX; //Toggle que controla la activación de los sonidos FX
	public Toggle toggleMusic; //Toggle que controla la activación de la música de fondo
	public AudioMixer musicMixer; //Control del volumen de los sonidos
	GameObject hiddenScreen; //OscurecerPantalla
    GameObject pausePanel; //Panel de pausa
	GameObject[] gameStates; //Array con los estados de juego (inicio del tutorial y pausa)
	GameObject[] tutorialStates; //Array con los estados del tutorial (cada paso del tutorial)
    ControladorTutorial gameController; //ContoladorTutorial
	enum GameStatesEnum {Initial, Pause};

	void Start() {
		SearchObjects();
		SearchStates();
        AdjustMusic();
	}

	void SearchObjects(){
		gameController = GameObject.Find("ControladorJuego").GetComponent<ControladorTutorial>();
		hiddenScreen = GameObject.Find("OscurecerPantalla");
		GameObject tutorialFather = GameObject.Find("EstadosTutorial"); //Objeto que contiene los paneles con los estados
        pausePanel = GameObject.Find("PanelPausa");
		pausePanel.SetActive(false); //Desactivamos el panel de pausa (activo por defecto)
        tutorialStates = new GameObject[(int)(ControladorTutorial.TutorialStates.ExitTutorial) + 1]; //Estados del tutorial. El tamaño del array será el número de estados
		int counter = 0;
        foreach (Transform estadosHijos in tutorialFather.transform){ //Recorremos los hijos de tutorialFather y guardamos dichos estados en el array tutorialStates
            tutorialStates[counter] = estadosHijos.gameObject;
            counter++;
        }
	}

    void SearchStates() { //Búsqueda de los estados de juego
		gameStates = new GameObject[2];
		int counter = 0;
		foreach (Transform estadosobj in hiddenScreen.transform) { //Los estados de juego se obtienen de los hijos de OscurecerPantalla
			if (counter < 3) {
				gameStates[counter] = estadosobj.gameObject;
				counter++;
			}
		}
	}

	void AdjustMusic() { //Función que activa los sonidos en función de las preferencias edl usuario
		if (PlayerPrefs.GetInt ("FX") == 0) //Si tenía los sonidos FX activados, se activa el toggle
			toggleFX.isOn = false;
		else
			toggleFX.isOn = true;

		if (PlayerPrefs.GetInt("MUSIC") == 0) //Si tenía la música de fondo activada, se activa el toggle
			toggleMusic.isOn = false;
		else
			toggleMusic.isOn = true;

	}

	//Función que se activa al cambiar el valor del toggleFX
	public void ValueChangedFX() {
		if (toggleFX.isOn) { //Si está activado el toggle se sube el volumen
			PlayerPrefs.SetInt("FX", 1);
			musicMixer.SetFloat("FXMusica", -1);
		}
		else { //Si está desactivado, se silencia el sonido FX
			PlayerPrefs.SetInt("FX", 0);
			musicMixer.SetFloat("FXMusica", -80);
		}
	}

	//Función que se activa al cambiar el valor del toggleMusic
	public void ValueChangedMusic() {
		if (toggleMusic.isOn) { //Si está activado el toggle se sube el volumen
			PlayerPrefs.SetInt("MUSIC", 1);
			musicMixer.SetFloat("FondoMusica", -5);
		}
		else { //Si está activado el toggle se sube el volumen
			PlayerPrefs.SetInt("MUSIC", 0);
			musicMixer.SetFloat("FondoMusica", -80);
		}
	}

	//ESTADO DE PAUSA
	public void ShowPause(){ //Se llama a este método al pulsar el botón de salir
        if(!((int)ControladorTutorial.TutorialStates.ExitTutorial == gameController.GetTutorialState())){ //Si no estamos en el último estado se activa el panel de pausa
            tutorialStates[gameController.GetTutorialState()].SetActive(false); //Se desactiva el estado actual para dejar paso al panel de pausa
            hiddenScreen.SetActive(true);
            pausePanel.SetActive(true);
            pausePanel.GetComponent<Animator>().SetTrigger("Agrandar"); //El panel de pausa aparece con una animación
        } else { //Si estamos en el último estado se sale directamente al menú principal.
            ExitTutorial();
        }
	}

	public void BackToTutorial(){ //Método que desactiva el panel de pausa y activa el estado que corresponda (el mismo que se quitó al pulsar pausa)
        pausePanel.SetActive(false);
        hiddenScreen.SetActive(false);
        tutorialStates[gameController.GetTutorialState()].SetActive(true);
    }

    public void ExitTutorial(){ //Se carga el menú principal con una animación de fade
		if (Application.platform == RuntimePlatform.Android) {
			AndroidSingleton.Instance.StackBackToMainMenu ();
		}
		AutoFade.LoadLevel ("EscenaInicio", 0.2f, 0.2f, Color.white);
    }

	//ESTADO INICIAL
	//Método que se llama con el botón de play
	public void StartPlaying() {
		gameStates[(int)GameStatesEnum.Initial].SetActive(false); //Se desactiva el estado inicial de juego
		hiddenScreen.SetActive(false); 
        tutorialStates[(int)ControladorTutorial.TutorialStates.ShowQuestion].SetActive(true); //Se activa el primer estado que muestra la pregunta a contestar
		gameController.GameStarts(); //Se inicia el tutorial
	}


    //T U T O R I A L __ E S T A D O S
    public void ChangeStateToShowChips(){ //Se cambia al estado de mostrar fichas
        tutorialStates[(int)ControladorTutorial.TutorialStates.ShowQuestion].SetActive(false);
        tutorialStates[(int)ControladorTutorial.TutorialStates.ShowChips].SetActive(true);
    }

    public void ChangeStateToSetIncorrect(){ //Se cambia al estado a colocar ficha incorrecta
        tutorialStates[(int)ControladorTutorial.TutorialStates.ShowChips].SetActive(false);
        tutorialStates[(int)ControladorTutorial.TutorialStates.SetIncorrectChip].SetActive(true);
    }

    public void ChangeStateToShowUserWord(){ //Se cambia el estado a mostrar palabra formada
		tutorialStates[(int)ControladorTutorial.TutorialStates.SetIncorrectChip].SetActive(false);
		tutorialStates[(int)ControladorTutorial.TutorialStates.ShowUserWord].SetActive(true);
    }

    public void ChangeStateToCleanBoard(){ //Se cambia el estado a limpiar tablero
		tutorialStates[(int)ControladorTutorial.TutorialStates.ShowUserWord].SetActive(false);
		tutorialStates[(int)ControladorTutorial.TutorialStates.CleanBoard].SetActive(true);
    }

    public void ChangeStateToSetCorrectChip1(){ //Se cambia el estado a colocar la primera ficha correcta
		tutorialStates[(int)ControladorTutorial.TutorialStates.CleanBoard].SetActive(false);
		tutorialStates[(int)ControladorTutorial.TutorialStates.SetCorrectChip1].SetActive(true);
    }

	public void ChangeStateToSetCorrectChip2(){ //Se cambia el estado a colocar la segunda ficha correcta
		tutorialStates[(int)ControladorTutorial.TutorialStates.SetCorrectChip1].SetActive(false);
		tutorialStates[(int)ControladorTutorial.TutorialStates.SetCorrectChip2].SetActive(true);
	}

	public void ChangeStateToSetCorrectChip3(){ //Se cambia el estado a colocar la tercera ficha correcta
		tutorialStates[(int)ControladorTutorial.TutorialStates.SetCorrectChip2].SetActive(false);
		tutorialStates[(int)ControladorTutorial.TutorialStates.SetCorrectChip3].SetActive(true);
	}

    public void ChangeStateToShowPowerupSkipQuestion(){ //Se cambia el estado a usar powerup de pasar pregunta
		tutorialStates[(int)ControladorTutorial.TutorialStates.SetCorrectChip3].SetActive(false);
        tutorialStates[(int)ControladorTutorial.TutorialStates.PowerUpSkipQuestion].SetActive(true);
    }

    public void ChangeStateToShowPrices(){ //Se cambia el estado a mostrar las monedas gastadas
        tutorialStates[(int)ControladorTutorial.TutorialStates.PowerUpSkipQuestion].SetActive(false);
        tutorialStates[(int)ControladorTutorial.TutorialStates.ShowPowerUpPrice].SetActive(true);
	}

    public void ChangeStateToShowFirstChip(){ //Se cambia el estado a usar powerup de colocar primera ficha
        tutorialStates[(int)ControladorTutorial.TutorialStates.ShowPowerUpPrice].SetActive(false);
        tutorialStates[(int)ControladorTutorial.TutorialStates.PowerUpFirstChip].SetActive(true);
    }

    public void ChangeStateToShowDeleteChip(){ //Se cambia el estado a usar powerup de eliminar fichas incorrectas
        tutorialStates[(int)ControladorTutorial.TutorialStates.PowerUpFirstChip].SetActive(false);
        tutorialStates[(int)ControladorTutorial.TutorialStates.PowerUpDeleteChip].SetActive(true);
    }

    public void ChangeStateToUserKnowHowToPlay(){ //Se cambia el estado al usuario ya sabe jugar
        tutorialStates[(int)ControladorTutorial.TutorialStates.PowerUpDeleteChip].SetActive(false);
        tutorialStates[(int)ControladorTutorial.TutorialStates.UserKnowHowToPlay].SetActive(true);
	}

    public void ChangeStateToLetUserPlay(){ //Se cambia el estado para que el usuario pueda contestar normalmente
        tutorialStates[(int)ControladorTutorial.TutorialStates.UserKnowHowToPlay].SetActive(false);
        tutorialStates[(int)ControladorTutorial.TutorialStates.LetUserAnswer].SetActive(true);
    }

	public void ChangeStateToExitTutorial(){ //Se cambia el estado al último, para salir del tutorial
		tutorialStates [(int)ControladorTutorial.TutorialStates.LetUserAnswer].SetActive (false);
		tutorialStates [(int)ControladorTutorial.TutorialStates.ExitTutorial].SetActive (true);
	}

}
