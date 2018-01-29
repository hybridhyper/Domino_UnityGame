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

//Esta clase solo se utiliza en la plataforma Android y controla el uso del botón de Back
public class AndroidSingleton : MonoBehaviour {

	private static AndroidSingleton instance = null;
	ControladorModoJuego controller; //Controlador de la escena de selección de modo de juego
	Stack stackOfScenes; //Pila de escenas. Esta pila guarda las escenas que se deberán ir cargando según se pulsa back en el dispositivo.

	public static AndroidSingleton Instance
	{
		get { return instance; }
	}

	private void OnEnable()
	{
		if (instance != null && instance != this) {
			Destroy(this.gameObject);
			return;
		}
		instance = this;
	}

	public int PruebaShowStackContent(){
		return stackOfScenes.Count;
	}

	public void SetController(){
		controller = GameObject.Find ("ControladorEscena").GetComponent<ControladorModoJuego> ();
	}

	void Start(){
		stackOfScenes = new Stack ();
	}

	void Update(){
		if (Input.GetKeyDown(KeyCode.Escape)) { //Al pulsar el botón de Back
			if (stackOfScenes.Count == 0) { //Si el jugador pulsa Back en EscenaInicio, la aplicación se cierra
				if (Application.platform == RuntimePlatform.Android) {
					AndroidJavaObject activity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject> ("currentActivity");
					activity.Call<bool> ("moveTaskToBack", true);
				}
			} else { //Si el jugador pulsa Back en cualquier otra escena
				if (SceneManager.GetActiveScene ().name == "ModosJuego") { //Si la escena es Modos de juego y hay algún panel abierto, entonces se cierra el panel
					if (controller.panelIsOpen) {
						controller.ClosePanelRelax ();
						controller.CloseTimeTrialPanel ();
					} else { //Si no hay paneles abiertos funciona de forma normal, volviendo a la escena anterior
						AutoFade.LoadLevel ((int)stackOfScenes.Pop (), 0.2f, 0.2f, Color.black);
					}
				} else {
					AutoFade.LoadLevel((int)stackOfScenes.Pop(), 0.2f, 0.2f, Color.black);
				}
			}
		}
	}

	public void SaveScene(){ //Se salva la escena actual en la pila.
		stackOfScenes.Push (SceneManager.GetActiveScene().buildIndex);
	}

	public void StackBackToMainMenu(){ //Se limpia la pila
		stackOfScenes.Clear ();
	}
}