/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class DragFicha : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler {
    public Transform parentToReturnTo = null; //Transform del objeto al que tiene que volver.
    public AudioClip[] clipsChips; //Array de clips.
    AudioSource audioChip; //AudioSource de cada ficha.
	Vector3 initialPosition; //Posicion inicial en la zona de fichas 
	ControladorMaestro gameController; //Controlador maestro el cual será sustituido por controladores especificos del juego.
    GameObject chipZone; //Zona de fichas.
    bool chipSet = false; 
    public bool backToPosition;
    public bool shouldSound = true;
    int gameControllerNumber;

	enum ClipsChips{
		SetChip,
		GrabChip,
        WrongSetChip
	}

    private void Start() {
        SearchObjects();
        initialPosition = transform.position;
    }

    void SearchObjects(){
        gameControllerNumber = GameObject.Find("ObjetoSingleton").GetComponent<PreguntasSingleton>().getControllerGameNumber();
        audioChip = this.transform.GetComponent<AudioSource>();
        chipZone = GameObject.Find("ZonaFichas");
        switch (gameControllerNumber){
			case 0:
				break;
			case 1:
				gameController = GameObject.Find("ControladorJuego").GetComponent<ControladorJuego>();
				break;
			case 2:
				gameController = GameObject.Find("ControladorJuego").GetComponent<ControladorjuegoRelax>();
				break;
			case 3:
				gameController = GameObject.Find ("ControladorJuego").GetComponent<ControladorTutorial> ();
				break;
			default: break;
		}
    }

    private void Update() {
        if(backToPosition){
            transform.position = Vector3.Lerp(transform.position, initialPosition, Time.deltaTime * 5);
			if (Mathf.Abs(this.transform.position.y - initialPosition.y) < 0.1f){
				backToPosition = false;
                if(gameControllerNumber == 3 && chipSet)
                    gameController.CheckEnabledChip(this.transform);
			}
        }
    }

    public void OnBeginDrag(PointerEventData eventData){
        if(gameControllerNumber == 3)
            gameController.StopAnimationHand();
        
		audioChip.clip = clipsChips [(int)ClipsChips.GrabChip];
		audioChip.Play ();
        chipSet = false;
        backToPosition = false; //Si cogemos la ficha, le quitamos el lerp. 
        parentToReturnTo = transform.parent;
        transform.SetParent(FindInParents<Canvas>(gameObject).transform); //La ponemos al nivel de Canvas, así tiene prioridad y no se pone por debajo de nada.
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData){
        if (gameController.GetStartedGame()) {
            transform.position = eventData.position;
        } else {
            BackToInitialPosition();
        }

    }

    public void OnEndDrag(PointerEventData eventData){ //Primero se produce el Drop Here y luego el EndDrag.
        //OnEndDrag no se ejecuta en las zonas de drop porque es desactivado una vez una ficha es colocada. 
        //En caso contrario....
		transform.SetParent(parentToReturnTo); //Le devolvemos a zona fichas
		backToPosition = true; //Debe devolver
		GetComponent<CanvasGroup>().blocksRaycasts = true;
		audioChip.clip = clipsChips[(int)ClipsChips.WrongSetChip];
		audioChip.Play();
        if(gameControllerNumber == 3)
            gameController.StartAnimationHand();
    }

    public void SetChip(RectTransform miniDrop){ //Recibe un minidrop, el cual es el minidrop mas cercano
        transform.SetParent(parentToReturnTo);
        if (parentToReturnTo.name != "FichasColocadas")
            backToPosition = true;
        else {
            if (shouldSound){   
                audioChip.clip = clipsChips[(int)ClipsChips.SetChip];
                audioChip.Play();
            } else 
                shouldSound = false;
            
            chipSet = true;
            Vector3 recolocationPositionValue = new Vector3();
            switch (miniDrop.transform.name) {
                case "Arriba":
                    transform.SetAsFirstSibling();
                    SetUpperPartChip(miniDrop);
                    //Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
                    if (this.transform.name.IndexOf("Horizontal") != -1)
                        recolocationPositionValue = new Vector3(transform.GetComponent<RectTransform>().rect.width / 4, -transform.GetComponent<RectTransform>().rect.height / 2);
                    else
                        recolocationPositionValue = new Vector3(0, -transform.GetComponent<RectTransform>().rect.height / 2);
                    break;
                case "Izquierda":
                    transform.SetAsFirstSibling();
                    SetUpperPartChip(miniDrop);
					//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
					if (this.transform.name.IndexOf("Horizontal") != -1)
                        recolocationPositionValue = new Vector3(transform.GetComponent<RectTransform>().rect.width / 2, 0);
                    else
                        recolocationPositionValue = new Vector3(transform.GetComponent<RectTransform>().rect.width / 2, -transform.GetComponent<RectTransform>().rect.height / 4);
                    break;
                case "Abajo":
                    SetLowerPartChip(miniDrop);
					//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
					if (this.transform.name.IndexOf("Horizontal") != -1)
                        recolocationPositionValue = new Vector3(-transform.GetComponent<RectTransform>().rect.width / 4, transform.GetComponent<RectTransform>().rect.height / 2);
                    else
                        recolocationPositionValue = new Vector3(0, transform.GetComponent<RectTransform>().rect.height / 2);
                    break;
                case "Derecha":
                    SetLowerPartChip(miniDrop);
					//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
					if (this.transform.name.IndexOf("Horizontal") != -1)
                        recolocationPositionValue = new Vector3(-transform.GetComponent<RectTransform>().rect.width / 2, 0);
                    else
                        recolocationPositionValue = new Vector3(-transform.GetComponent<RectTransform>().rect.width / 2, +transform.GetComponent<RectTransform>().rect.height / 4);
                    break;
                default:
                    recolocationPositionValue = Vector3.zero;
                    break;
            }

            this.enabled = false;

			//Crear objeto de FichaColocada y su Minidrop para poder quitarlo en caso de que el usuario quiera hacerlo.
            gameController.CheckDragDrop(recolocationPositionValue, new LastChipObject(this.transform, miniDrop.transform));
            //Cuando el usuario suelta una ficha en fichas colocadas.... debo desactivar este codigo y activar el de recoverficha.
        }
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

    void SetLowerPartChip(RectTransform miniDrop){
        //Si encuentra la cadena "Horizontal" (valor distinto de -1)
        if (this.transform.name.IndexOf("Horizontal") != -1)
            this.transform.localPosition = new Vector3(miniDrop.transform.localPosition.x + (this.GetComponent<RectTransform>().rect.width / 4), miniDrop.transform.localPosition.y);
        else
            this.transform.localPosition = new Vector3(miniDrop.transform.localPosition.x, miniDrop.transform.localPosition.y - (this.GetComponent<RectTransform>().rect.height / 4));
    }

    void SetUpperPartChip(RectTransform miniDrop){
        //Si encuentra la cadena "Horizontal" (valor distinto de -1)
        if (this.transform.name.IndexOf("Horizontal") != -1)
            this.transform.localPosition = new Vector3(miniDrop.transform.localPosition.x - (this.GetComponent<RectTransform>().rect.width / 4), miniDrop.transform.localPosition.y);
        else
            this.transform.localPosition = new Vector3(miniDrop.transform.localPosition.x, miniDrop.transform.localPosition.y + (this.GetComponent<RectTransform>().rect.height / 4));
    }

    public void BackToInitialPosition(){
        this.enabled = true;
        this.transform.SetParent(chipZone.transform); //Y le ponemos su padre nuevo, la zona de fichas.
        backToPosition = true;
    }

    static public T FindInParents<T>(GameObject go) where T : Component{
        if (go == null) return null;
        var comp = go.GetComponent<T>();
        if (comp != null)
            return comp;
        var t = go.transform.parent;
        while (t != null && comp == null)
        {
            comp = t.gameObject.GetComponent<T>();
            t = t.parent;
        }
        return comp;
    }
}
