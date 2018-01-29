using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RecoverFicha: MonoBehaviour {

    Vector3 posicionInicial;
    GameObject zonaFichas;
    ControladorJuego controladorEscena;
    public bool volviendoAPosicion;


	void Start () {
        posicionInicial = this.transform.position;
        zonaFichas = GameObject.Find("ZonaFichas");
        controladorEscena = GameObject.Find("ControladorJuego").GetComponent<ControladorJuego>();
	}

    private void Update()
    {
        if(volviendoAPosicion){

            this.transform.position = Vector3.Lerp(this.transform.position, posicionInicial, Time.deltaTime * 5);

            if(Mathf.Approximately(this.transform.position.x, posicionInicial.x) && Mathf.Approximately(this.transform.position.y, posicionInicial.y)){
				volviendoAPosicion = false;
			}
        }
    }

    public  void recolocarFicha(){
        if(this.transform.parent.gameObject != zonaFichas){
            
            volviendoAPosicion = true;
			int miPosition = this.transform.GetSiblingIndex();
			this.transform.SetParent(zonaFichas.transform); //Asignamos a su padre									//El usuario toca una ficha del tablero.. por tanto, tendremos que reajustar el tablero.
			controladorEscena.ReadjustBoard(miPosition, this.transform.name);//Obtenemos la posicion de la jerarquia para saber como recolocar el tablero.
            controladorEscena.currentWord.text = controladorEscena.userWord;
            //controladorEscena.comprobarNumeroDeFichasEnTablero();
			this.GetComponent<DragFicha>().enabled = true;
        } 
	}
}
