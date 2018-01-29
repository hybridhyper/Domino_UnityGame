using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopControl : MonoBehaviour, IPointerEnterHandler {

	ScrollRect scrollRect; //Componente que une los Scrollbar con el contenido que debe hacer scroll
	Image lifesButton; //Fondo de la pestaña de vidas
	Image coinsButton; //Fondo de la pestaña de monedas
	Text lifesButtonText; //Texto de la pestaña de vidas
	Text coinsButtonText; //Texto de la pestaña de monedas
	public Text textPlayerLifes; //Indicador de vidas del jugador
	public Text textPlayerCoins; //Indicador de monedas del jugador
	bool isMoving = false;
	bool directionChecked = false;
	bool isInsideContent = false;
	bool checkSpeed = false;
	bool enoughSpeed = false;
	bool xMovement = false;
	float xPosition;
	float valueScroll;
	bool mustLerpRight = false;
	bool mustLerpLeft = false;
    Vector2 touchInicio;

	//TODO Persistent data https://unity3d.com/es/learn/tutorials/topics/scripting/persistence-saving-and-loading.data

	void Start () {
        FindObjects();
		UpdatePlayerCurrency();
		scrollRect.content.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 2, scrollRect.content.GetComponent<RectTransform> ().rect.height); //Establecemos el tamaño del contenido a 2 veces la anchura de la pantalla.
		scrollRect.horizontalScrollbar.value = 0.0001f; //El valor 0 de problemas, por eso usamos una aproximación
		scrollRect.verticalScrollbar.value = 1;
	}

	void UpdatePlayerCurrency(){
		textPlayerCoins.text = PlayerPrefs.GetInt ("Coins") + "";
		textPlayerLifes.text = PlayerPrefs.GetInt ("Lifes") + "";
	}

    void FindObjects() {
		lifesButton = GameObject.Find("TabVidas").transform.GetChild(0).GetComponent<Image>();
		coinsButton = GameObject.Find("TabMonedas").transform.GetChild(0).GetComponent<Image>();
        lifesButtonText = GameObject.Find("TextoTabVidas").GetComponent<Text>();
        coinsButtonText = GameObject.Find("TextoTabMonedas").GetComponent<Text>();
        scrollRect = this.GetComponent<ScrollRect>();
    }

	public void OnPointerEnter(PointerEventData eventData){
		isInsideContent = true;
	}

	// Update is called once per frame
	void Update () {
		lifesButton.fillAmount = 1 - scrollRect.horizontalScrollbar.value;
		coinsButton.fillAmount = scrollRect.horizontalScrollbar.value;
		foreach (Touch touch in Input.touches) { //Por cada touch comprobamos

			switch (touch.phase) {
			case TouchPhase.Began: //Se acaba de iniciar el touch
				touchInicio = touch.position;  //Guardamos la posición donde se inició el touch
				valueScroll = scrollRect.horizontalScrollbar.value;  //Guardamos el valor del scroll horizontal
				mustLerpLeft = false; //Reiniciamos todos los booleanos
				mustLerpRight = false;
				enoughSpeed = false;
				checkSpeed = false;
				if(scrollRect.horizontalScrollbar.value < 0.001f || scrollRect.horizontalScrollbar.value > 0.999f) //Si el touch se ha producido estando el scroll horizontal en uno de los dos extremos
					directionChecked = false; //Reiniciamos este booleano para volver a comprobar si el jugador quiere moverse en x o en y (scroll o swipe)
				break;
			case TouchPhase.Ended: //Se ha levantado el dedo de la pantalla
				checkSpeed = true; //Activamos el booleano para comprobar la velocidad del swipe realizado.
				break;
			}

			if (isInsideContent) { //Si se ha tocado en la zona de scroll
				if (Mathf.Abs((touch.position - touchInicio).magnitude) > 10 && !directionChecked){ //Si la dirección del movimiento no se ha calculado aún y ha habido suficiente movimiento para poder calcularla
					if (Mathf.Abs(touch.position.x - touchInicio.x) > Mathf.Abs(touch.position.y - touchInicio.y)) { //Si el movimiento ha sido mayor en x que en y
						xMovement = true; //Indicamos que es un movimiento en el eje x
						scrollRect.vertical = false; //Bloqueamos el scroll vertical
						scrollRect.horizontal = true; //Liberamos el scroll horizontal
					} else {
						xMovement = false; //Indicamos que es un movimiento en el eje y
						scrollRect.vertical = true; //Liberamos el scroll vertical
						scrollRect.horizontal = false; //Bloqueamos el scroll horizontal
					}
					directionChecked = true; //La dirección del movimiento se ha comprobado
				}
			}
			if (xMovement) { //Si el movimiento es en el eje x
				if (checkSpeed) { //Si hay que comprobar la velocidad del swipe (se ha quitado el dedo de la pantalla y se ha finalizado el swipe)
					if (touch.deltaPosition.magnitude / touch.deltaTime > 50) { //Si la velocidad del swipe ha sido suficientemente alta
						enoughSpeed = true;
						if (touch.position.x - touchInicio.x < 0 && valueScroll < 0.5f) {//Si el movimiento se hace hacia la derecha y el scroll está en la pantalla de vidas entonces el swipe es correcto
							mustLerpLeft = false;
							mustLerpRight = true; //Iniciamos el Lerp hacia la derecha
						} else if (touch.position.x - touchInicio.x > 0 && valueScroll >= 0.5f){ //Si el movimiento se hace hacia la izquierda y el scroll está en la pantalla de monedas entonces el swipe es correcto
							mustLerpRight = false;
							mustLerpLeft = true; //Iniciamos el Lerp hacia la izquierda
						}
					}
					checkSpeed = false; //Ya se ha comprobado la velocidad del swipe
				}
			}
		}
		if (isInsideContent) { //Si está dentro de la zona de scroll
			if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Ended) { //Se quita el eddo de la pantalla
				isInsideContent = false;
				if (isMoving && !enoughSpeed) { //Si la velocidad del swipe no ha sido suficiente comprobamos el valor del scroll
					if (xPosition <= 0.5f) { //Si el contenido está más hacia la izquierda hace Lerp a la izquierda
						mustLerpRight = false;
						mustLerpLeft = true;
					} else { //En caso contrario a la derecha
						mustLerpLeft = false;
						mustLerpRight = true;
					}
					isMoving = false;
				}
			}
		}
		if (mustLerpLeft) { //Se inicia el Lerp a la izquierda
			if (scrollRect.horizontalScrollbar.value > 0.001) { //La pantalla se mueve hacia la izquierda hasta llegar al final
				scrollRect.horizontalScrollbar.value -= 4 * Time.deltaTime;
			}
			else { //Al acabar realizamos los cambios correspondientes
				scrollRect.horizontalScrollbar.value = 0;
				mustLerpLeft = false; //Lerp terminado
                lifesButtonText.color = Constantes.SHOP_TAB_SELECTED_TEXT; //Cambiamos el color del botón que activa la pestaña de vidas
				coinsButtonText.color = Constantes.SHOP_TAB_NOT_SELECTED_TEXT; //Cambiamos el color del botón que activa la pestaña de monedas
				enoughSpeed = false;
			}
		}
		if (mustLerpRight) { //Igual que antes pero hacia la derecha
			if (scrollRect.horizontalScrollbar.value < 0.99) { //La pantalla se mueve hacia la derecha hasta llegar al final
				scrollRect.horizontalScrollbar.value += 4 * Time.deltaTime;
			}
			else { //Al acabar realizamos los cambios correspondientes
				scrollRect.horizontalScrollbar.value = 1;
				mustLerpRight = false; //Lerp terminado
				lifesButtonText.color = Constantes.SHOP_TAB_NOT_SELECTED_TEXT; //Cambiamos el color del botón que activa la pestaña de vidas
				coinsButtonText.color = Constantes.SHOP_TAB_SELECTED_TEXT; //Cambiamos el color del botón que activa la pestaña de monedas
				enoughSpeed = false;
			}
		}
	}

	public void MoveScroll(Vector2 vector){ //Función que se ejecuta cada vez que cambia el valor del scroll desde la interfaz
		isMoving = true;
		xPosition = vector.x;
	}

	public void BuyCoins(int monedas){ //Función que añade cierta cantidad de monedas al jugador
		int totalCoins = PlayerPrefs.GetInt ("Coins") + monedas;
		PlayerPrefs.SetInt ("Coins", PlayerPrefs.GetInt ("Coins") + monedas);
		textPlayerCoins.text = totalCoins + "";
	}

	public void BuyLifes(int vidas){ //Función que añade cierta cantidad de vidas al jugador
		int totalLifes = PlayerPrefs.GetInt ("Lifes") + vidas;
		PlayerPrefs.SetInt ("Lifes", PlayerPrefs.GetInt ("Lifes") + vidas);
		textPlayerLifes.text = totalLifes + "";
	}

	public void ActivateLifesPanel(){ //Función que se ejecuta al pulsar la pestaña de vidas
		mustLerpLeft = true;
	}

	public void ActivateCoinsPanel(){ //Función que se ejecuta al pulsar la pestaña de monedas
		mustLerpRight = true;
	}

	public void WatchVideoLifes(){ //Llamada al AdManager para mostrar el video de recompensa de vidas
		GameObject.Find("ObjetoSingleton").GetComponent<AdManager>().ShowRewardVideoLifes();
	}

	public void WatchVideoCoins(){ //Llamada al AdManager para mostrar elvideo de recompensa de vidas
		GameObject.Find("ObjetoSingleton").GetComponent<AdManager>().ShowRewardVideoCoins();
	}
	
	public void BackToMainMenu() { //Se regresa al menú principal
		if (Application.platform == RuntimePlatform.Android)
			AndroidSingleton.Instance.StackBackToMainMenu ();
        AutoFade.LoadLevel("EscenaInicio", 0.2f, 0.2f, Color.white);
    }
}
