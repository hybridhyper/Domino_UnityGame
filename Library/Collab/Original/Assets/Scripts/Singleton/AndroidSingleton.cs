using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AndroidSingleton : MonoBehaviour {

	private static AndroidSingleton instance = null;
	ControladorModoJuego controller;
	Stack stackOfScenes;


	public static AndroidSingleton Instance
	{
		get { return instance; }
	}

	private void OnEnable()
	{
		if (instance != null && instance != this)
		{
			Destroy(this.gameObject);
			return;
		}
		else
		{
			instance = this;
		}

		DontDestroyOnLoad(this.gameObject);
	}

	public void SetController(){
		controller = GameObject.Find ("ControladorEscena").GetComponent<ControladorModoJuego> ();
	}

	void Start(){
		stackOfScenes = new Stack ();
	}

	void Update(){
		if (Input.GetKeyDown(KeyCode.Escape)) {
			if (stackOfScenes.Count == 0) { //Si el jugador pulsa Back en EscenaInicio, la aplicación se cierra
				mensaje = "Saliendo";
				if (Application.platform == RuntimePlatform.Android) {
					AndroidJavaObject activity = new AndroidJavaClass ("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject> ("currentActivity");
					activity.Call<bool> ("moveTaskToBack", true);
				}
			} else { //Si el jugador pulsa Back en cualquier otra escena, se carga la escena anterior
				mensaje = "COUNT SUPERIOR";
				if (SceneManager.GetActiveScene ().name == "ModosJuego") {
					mensaje = "ModosJuego";
					if (controller.panelIsOpen) {
						controller.ClosePanelRelax ();
						controller.CloseTimeTrialPanel ();
					} else {
						AutoFade.LoadLevel ((int)stackOfScenes.Pop (), 0.2f, 0.2f, Color.black);
					}
				} else {
					mensaje = "FADING";
					AutoFade.LoadLevel((int)stackOfScenes.Pop(), 0.2f, 0.2f, Color.black);
				}
			}
		}
	}

	public void SaveScene(){
		stackOfScenes.Push (SceneManager.GetActiveScene().buildIndex);
	}

	public void StackBackToMainMenu(){
		stackOfScenes.Clear ();
	}
}