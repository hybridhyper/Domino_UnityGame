/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using UnityEngine;
using UnityEngine.UI;

public class DropZoneVectors : MonoBehaviour {

	Color initialColor;
    int gameControllerNumber = -1;

	void Start(){
		initialColor = this.transform.GetComponent<Image>().color;
    }

    void OnEnable() {
        if(gameControllerNumber == -1)
            gameControllerNumber = GameObject.Find("ObjetoSingleton").GetComponent<PreguntasSingleton>().getControllerGameNumber();
		if (this.transform.tag == "MiniDrop") {
			if (gameControllerNumber == 1) {
				ControladorJuego.OnTimeHelp += ShowHelp; //Me suscribo al evento.
				ControladorJuego.OnHideTimeHelp += HideHelp;
			}
			else if (gameControllerNumber == 2) {
				ControladorjuegoRelax.OnTimeHelp += ShowHelp; //Me suscribo al evento.
				ControladorjuegoRelax.OnHideTimeHelp += HideHelp;
			}
			else if (gameControllerNumber == 3) {
				ControladorTutorial.OnTimeHelp += ShowHelp; //Me suscribo al evento.
				ControladorTutorial.OnHideTimeHelp += HideHelp;
			}
		}
	}

	private void OnDisable() {
		this.transform.GetComponent<Image>().color = initialColor;
		if (gameControllerNumber == 1) {
			ControladorJuego.OnTimeHelp -= ShowHelp; //Me suscribo al evento.
			ControladorJuego.OnHideTimeHelp -= HideHelp;
		} else if (gameControllerNumber == 2){
			ControladorjuegoRelax.OnTimeHelp -= ShowHelp; //Me suscribo al evento.
			ControladorjuegoRelax.OnHideTimeHelp -= HideHelp;
		} else if(gameControllerNumber == 3){
			ControladorTutorial.OnTimeHelp -= ShowHelp; //Me suscribo al evento.
			ControladorTutorial.OnHideTimeHelp -= HideHelp;
		}
    }

	public void ShowHelp() {
        this.transform.GetComponent<Image>().color = Constantes.ON_POINTER_COLOR;
	}
	public void HideHelp() {
		this.transform.GetComponent<Image>().color = new Color32(199, 255, 94, 0);
	}

	public void RecoverInitialColor(){
		this.transform.GetComponent<Image>().color = initialColor;
	}
}
