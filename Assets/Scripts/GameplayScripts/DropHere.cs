/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropHere : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {
    Color initialColor;
	TableroCalculos boardCalculations;

    void Start(){
        initialColor = this.transform.GetComponent<Image>().color;
        boardCalculations = GameObject.Find("TableroJuego").GetComponent<TableroCalculos>();
    }

    public void OnDrop(PointerEventData eventData) {
        DragFicha d = eventData.pointerDrag.GetComponent<DragFicha>();
        if (d != null){
            if(this.transform.name == "ZonaFichas"){
                d.parentToReturnTo = this.transform;
            } else if(this.transform.name == "TableroJuego"){
                Transform miniDrop = boardCalculations.MeasureDistance(d.transform);
				if (miniDrop != null) {
					d.parentToReturnTo = this.transform.GetChild (0).transform;
					d.SetChip (miniDrop.GetComponent<RectTransform> ());
				} else {
					//Si se suelta en el tablero de juego demasiado lejos ed cualquier miniDrop, se llama a ColocarFicha
					//y se utiliza como parentToReturn ZonAFichas (que se le asigna en OnBeginDrag)
					d.SetChip (null);
				}
            }else { //SI ES ZONA DE DROP INICIAL
                d.parentToReturnTo = this.transform.parent;
                d.SetChip(this.transform.GetComponent<RectTransform>());
                RecoverInitialColor();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData){
        if (this.transform.name == "DropZone")
        {
            this.transform.GetComponent<Image>().color = Constantes.ON_POINTER_COLOR;
            print("entra aqui");
        }
    }

    public void OnPointerExit(PointerEventData eventData){
        if (this.transform.name == "DropZone")
            RecoverInitialColor();   
    }

    public void RecoverInitialColor(){
        this.transform.GetComponent<Image>().color = initialColor;
    }
}
