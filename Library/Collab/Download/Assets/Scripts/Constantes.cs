/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using UnityEngine;

public class Constantes {

	public static int TYPE_OF_QUESTION = 1;
    public static Color COLOR_GREEN = new Color32(131, 201, 146, 255);
    public static Color COLOR_RED =  new Color32(201, 131, 131, 255);
    public static Color COLOR_WHITE_CHIP = new Color32(238, 231, 231, 255);
    public static Color SHOP_TAB_SELECTED = new Color32(94, 94, 94, 255);
    public static Color SHOP_TAB_NOTSELECTED = new Color32(155, 155, 155, 255);
    public static Color SHOP_TAB_SELECTED_TEXT = new Color32(235, 235, 235, 255);
	public static Color SHOP_TAB_NOT_SELECTED_TEXT = new Color32(50, 50, 50, 255);
    public static Color ON_POINTER_COLOR = new Color32(212, 229, 182, 128);
    //public static Color COLOR_TIMETRIAL_POWERUPS_ACTIVE = new Color(121, 31, 41, 255);
    //public static Color COLOR_TIMETRIAL_POWERUPS_INACTIVE = new Color(121, 31, 41, 126);
    //Precios Iniciales
    public static int SKIPQUESTION_PRICE = 3;
    public static int FIRSTCHIP_PRICE = 2;
    public static int DELETECHIP_PRICE = 2;
    public static float MAX_TIME_WITHOUTSET = 2; //Maximo de tiempo para que el jugador pueda poner una ficha

	public enum ENUM_TYPEOFQUESTION{
        QueNosUne,
        Jeroglifico,
        CompletaLaFrase
    }

	public const string RECORD_COMPLETA_FRASE = "RecordCompletarLaFrase";
	public const string RECORD_QUENOSUNE = "RecordQueNosUne";
	public const string RECORD_JEROGLIFICO = "RecordJeroglifico";
}

/*
 * Esta clase la estamos haciendo para poder guardar tanto ultima ficha como su minidrop correspondiente, así la podemos volver a su posición
 * y recolocar el tablero. Para recolocar el tablero necesitamos saber que minidrop se ha utilizado, es por esto que estamos guardando el minidrop
 * y la ficha colocada
 */
public class LastChipObject {
	public Transform lastChipSet { get; set; }
	public Transform miniDropSelected { get; set; }
	public Transform miniDropPartner { get; set; }
	public Vector3 miniDropSelectedPos { get; set; }
	public Vector3 miniDropPartnerPos { get; set; }

    ControladorMaestro gameController; 


	public LastChipObject(Transform _lastChipSet, Transform _miniDropSelected) {
		lastChipSet = _lastChipSet;
		miniDropSelected = _miniDropSelected;
		miniDropSelectedPos = _miniDropSelected.localPosition;
        int gameControllerNumber = GameObject.Find("ObjetoSingleton").GetComponent<PreguntasSingleton>().getControllerGameNumber();
			switch (gameControllerNumber) {
			case 0:
				break;
			case 1:
				gameController = GameObject.Find("ControladorJuego").GetComponent<ControladorJuego>();
				break;
			case 2:
				gameController = GameObject.Find("ControladorJuego").GetComponent<ControladorjuegoRelax>();
				break;
			case 3:
				gameController = GameObject.Find("ControladorJuego").GetComponent<ControladorTutorial>();
				break;
			default: break;
        }

		if (miniDropSelected.name.IndexOf("Arriba") != -1) {
			//Se le asigna la izquierda. Hay que tener cuidado ya que pueden ser 3 tipos de controladores:
			//tutorial, relax o controlador.... 
			miniDropPartner = gameController.GetMiniDrop("Izquierda");
		}
		else if (miniDropSelected.name.IndexOf("Izquierda") != -1) {
			miniDropPartner = gameController.GetMiniDrop("Arriba");
		}
		else if (miniDropSelected.name.IndexOf("Derecha") != -1) {
			miniDropPartner = gameController.GetMiniDrop("Abajo");
		}
		else if (miniDropSelected.name.IndexOf("Abajo") != -1) {
			miniDropPartner = gameController.GetMiniDrop("Derecha");
		}
			
		if (miniDropPartner != null)
			miniDropPartnerPos = miniDropPartner.localPosition;


	}
}