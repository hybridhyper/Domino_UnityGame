using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControladorEstadosRelax : MonoBehaviour
{
    GameObject hiddenScreen;
	GameObject[] states;
    ControladorjuegoRelax controladorJuego;
    public enum StatesEnum {Initial, Pause};

	void Start() {
        SearchObjects();
		BuscarEstados();
	}

    void SearchObjects(){
        GameObject.Find("ObjetoSingleton").GetComponent<MusicSingleton>().PlayRelax();
		controladorJuego = GameObject.Find("ControladorJuego").GetComponent<ControladorjuegoRelax>();
		hiddenScreen = GameObject.Find("OscurecerPantalla");
    }

	void BuscarEstados() {
		states = new GameObject[2];
        int counter = 0;
		foreach (Transform estadosobj in hiddenScreen.transform) {
			if (counter < 3) {
				states[counter] = estadosobj.gameObject;
				counter++;
			}
		}
	}

	//ESTADO DE PAUSA
    public void ShowPause()
	{
		hiddenScreen.SetActive(true);
		hiddenScreen.GetComponent<Image>().color = new Color32(0, 0, 0, 160);
		states[(int)StatesEnum.Pause].SetActive(true);
	}

    public void QuitPause()
	{
		hiddenScreen.SetActive(false);
		hiddenScreen.GetComponent<Image>().color = new Color32(0, 0, 0, 190);
		states[(int)StatesEnum.Pause].SetActive(false);
	}

	//ESTADO INICIAL
    public void StartPlaying()
	{
		states[(int)StatesEnum.Initial].SetActive(false);
		hiddenScreen.SetActive(false);
        controladorJuego.GameStarts();
	}

    public void OutOfGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("EscenaInicio");
    }
}
