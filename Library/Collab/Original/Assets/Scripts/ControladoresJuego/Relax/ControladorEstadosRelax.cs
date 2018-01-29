using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class ControladorEstadosRelax : MonoBehaviour
{
	public Toggle toggleFX;
	public Toggle toggleMusic;
	public AudioMixer musicMixer;
    GameObject hiddenScreen;
	GameObject[] states;
    ControladorjuegoRelax controladorJuego;
    public enum StatesEnum {Initial, Pause};

	void Start() {
        SearchObjects();
		BuscarEstados();
        AdjustMusic();
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

	void AdjustMusic()
	{
		if (PlayerPrefs.GetInt("FX") == 0){
			toggleFX.isOn = false;
			musicMixer.SetFloat("FXMusica", -80);
		}else{
			toggleFX.isOn = true;
			musicMixer.SetFloat("FXMusica", -1);
		}
	
        if (PlayerPrefs.GetInt("MUSIC") == 0){
			toggleMusic.isOn = false;
			musicMixer.SetFloat("FondoMusica", -80);
		}else{
			toggleMusic.isOn = true;
			musicMixer.SetFloat("FondoMusica", -5);
		}
	}

	public void ValueChangedFX()
	{
		if (toggleFX.isOn)
		{
			PlayerPrefs.SetInt("FX", 1);
			musicMixer.SetFloat("FXMusica", -1);
		}
		else
		{
			PlayerPrefs.SetInt("FX", 0);
			musicMixer.SetFloat("FXMusica", -80);
		}
	}

	public void ValueChangedMusic()
	{
		if (toggleMusic.isOn)
		{
			PlayerPrefs.SetInt("MUSIC", 1);
			musicMixer.SetFloat("FondoMusica", -5);
		}
		else
		{
			PlayerPrefs.SetInt("MUSIC", 0);
			musicMixer.SetFloat("FondoMusica", -80);
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
