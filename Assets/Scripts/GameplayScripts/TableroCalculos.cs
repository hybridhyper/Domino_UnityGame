/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using UnityEngine;
public class TableroCalculos : MonoBehaviour {

    Transform[] arrayMiniDrops = new Transform[4];
    int gameControllerNumber;

	// Use this for initialization
	void Start () {
        gameControllerNumber = GameObject.Find("ObjetoSingleton").GetComponent<PreguntasSingleton>().getControllerGameNumber();
        //Buscamos las 4 instancias de los minidrops para hacer calculos.
		Transform chipsSet = this.transform.GetChild(0).transform;
		int counter = 0;
        foreach (Transform miniDrops in chipsSet){
            if (miniDrops.tag == "MiniDrop") {
                arrayMiniDrops[counter] = miniDrops;
                counter++;
            }
        }
	} 

    public Transform MeasureDistance(Transform chipPosition){
        //Obtenemos el Transform de la última ficha colocada
		int closerDropPosition = 0;
		float closerDropDistance = 999;
        //Comprobamos qué miniDrop está mas cerca del punto en el que e sltó la ficha
        for (int i = 0; i < arrayMiniDrops.Length; i++){
			float currentDropDistance = Vector3.Magnitude(chipPosition.position - arrayMiniDrops[i].position);
			if (currentDropDistance < closerDropDistance) {
				closerDropPosition = i;
				closerDropDistance = currentDropDistance;
			}
        }

        if(gameControllerNumber == 3){ //Si estamos en el tutorial ponemos nuestras reglas (no puede colocarla donde quiera). 
            //Si el estado es SetChip2 entonces solo permitimos la colocación en los minidrops superiores.
            //En caso contrario se usarán los inferiores,
			ControladorTutorial gameController = GameObject.Find("ControladorJuego").GetComponent<ControladorTutorial>();
            int tutorialState = gameController.GetTutorialState();
            if(tutorialState == (int)ControladorTutorial.TutorialStates.SetCorrectChip2){
                if(arrayMiniDrops[closerDropPosition].name == "Abajo" || arrayMiniDrops[closerDropPosition].name == "Derecha"){
                    return null;
                }
            }

            if (tutorialState == (int)ControladorTutorial.TutorialStates.SetCorrectChip3){
				if (arrayMiniDrops[closerDropPosition].name == "Izquierda" || arrayMiniDrops[closerDropPosition].name == "Arriba")
					return null;
			}
        }

        //1.- Comprobamos si la ficha se va a colocar al principio o al final
		string chipSetName;
		DragFicha[] chipListSet = this.transform.GetChild(0).GetComponentsInChildren<DragFicha>();
        if (closerDropPosition < 2) //Si es menor que dos, entonces el miniDrop más ecrcano es Arriba o Izquierda, que son los primeros en la jerarquía
        {
            //2.- Obtener el nombre de la ficha correspondiente que ya está colocada
            chipSetName = chipListSet[0].name;
        }
        else
        {
            //2.- Obtener el nombre de la ficha correspondiente que ya está colocada
            chipSetName = chipListSet[chipListSet.Length - 1].name;
        }
        //3.- Comprobamos las dos condiciones no válidas
        //     - Dos fichas horizontales colocadas una encima de la otra
        if(chipSetName.Substring(0,6) == chipPosition.name.Substring(0,6) && chipSetName.Substring(0,6) == "FichaH" && 
            (arrayMiniDrops[closerDropPosition].name == "Arriba" || arrayMiniDrops[closerDropPosition].name == "Abajo"))
        {
            return null;
        }
        else if (chipSetName.Substring(0, 6) == chipPosition.name.Substring(0, 6) && chipSetName.Substring(0, 6) == "FichaV" &&
            (arrayMiniDrops[closerDropPosition].name == "Izquierda" || arrayMiniDrops[closerDropPosition].name == "Derecha"))
        {
            return null;
        }
			
		float distanceAllowed = this.transform.GetComponent<RectTransform>().position.x / 2;
        if (closerDropDistance < distanceAllowed){
            return arrayMiniDrops[closerDropPosition];
        }
        return null;
    }
}
