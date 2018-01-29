using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopControl : MonoBehaviour, IPointerEnterHandler {

	public Scrollbar horizontal;
	ScrollRect scrollRect;
	Image lifesButton;
	Image coinsButton;
    Text lifesText;
    Text coinsText;
	public Text textLifes;
	public Text textCoins;
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
		//TODO Eliminar esta línea, solo sirve para pruebas
		PlayerPrefs.SetInt ("Coins", 100);
		scrollRect.content.GetComponent<RectTransform> ().sizeDelta = new Vector2 (Screen.width * 2, scrollRect.content.GetComponent<RectTransform> ().rect.height);
		horizontal.value = 0.0001f;
		scrollRect.verticalScrollbar.value = 1;
		
	}

    void FindObjects() {
		lifesButton = GameObject.Find("TabVidas").GetComponent<Image>();
		coinsButton = GameObject.Find("TabMonedas").GetComponent<Image>();
        lifesText = GameObject.Find("TextoVidas").GetComponent<Text>();
        coinsText = GameObject.Find("TextoMonedas").GetComponent<Text>();
        scrollRect = this.GetComponent<ScrollRect>();
//        GameObject.Find("ObjetoSingleton").GetComponent<AdManager>().RequestRewardVideo();
    }

	public void OnPointerEnter(PointerEventData eventData){
		isInsideContent = true;
	}

	// Update is called once per frame
	void Update () {

		foreach (Touch touch in Input.touches) { //For every touch in the Input.touches - array...

			switch (touch.phase) {
			case TouchPhase.Began: //The finger first touched the screen --> It could be(come) a swipe
				touchInicio = touch.position;  //Position where the touch started
				valueScroll = horizontal.value;
				mustLerpLeft = false;
				mustLerpRight = false;
				enoughSpeed = false;
				checkSpeed = false;
				if(horizontal.value < 0.001f || horizontal.value > 0.999f)
					directionChecked = false;
				break;
			case TouchPhase.Ended:
				checkSpeed = true;
				break;
			}

			if (isInsideContent) {
				if (Mathf.Abs((touch.position - touchInicio).magnitude) > 10 && !directionChecked){
					if (Mathf.Abs(touch.position.x - touchInicio.x) > Mathf.Abs(touch.position.y - touchInicio.y)) {
						xMovement = true;
						scrollRect.vertical = false;
						scrollRect.horizontal = true;
					} else {
						xMovement = false;
						scrollRect.horizontal = false;
						scrollRect.vertical = true;
					}
					directionChecked = true;
				}
			}
			if (xMovement) {
				if (checkSpeed) {
					if (touch.deltaPosition.magnitude / touch.deltaTime > 50) {
						enoughSpeed = true;
						if (touch.position.x - touchInicio.x < 0 && valueScroll < 0.5f) {//Si el movimiento se hace hacia la derecha y el scroll está en la pantalla de vidas entonces el swipe es correcto
							mustLerpLeft = false;
							mustLerpRight = true;
						} else if (touch.position.x - touchInicio.x > 0 && valueScroll >= 0.5f){ //Si el movimiento se hace hacia la izquierda y el scroll está en la pantalla de monedas entonces el swipe es correcto
							mustLerpRight = false;
							mustLerpLeft = true;
						}
					}
					checkSpeed = false;
				}
			}
		}
		//TODO Comprobar el punto de inicio y el punto de final. Si el cambio en x es mayor que el cambio en y, entonces han hecho swipe
		//Si el cambio en y es mayor que en x, entonces han hecho scroll
		if (isInsideContent) {
			if (Input.touchCount > 0 && Input.GetTouch (0).phase == TouchPhase.Ended) {
				isInsideContent = false;
				if (isMoving && !enoughSpeed) {
					if (xPosition <= 0.5f) {
						mustLerpRight = false;
						mustLerpLeft = true;
					} else {
						mustLerpLeft = false;
						mustLerpRight = true;
					}
					isMoving = false;
				}
			}
		}
		if (mustLerpLeft) {
			if (horizontal.value > 0.001) {
				horizontal.value -= 4 * Time.deltaTime;
			}
			else {
				horizontal.value = 0;
				mustLerpLeft = false;
				lifesButton.color = Constantes.shopTabSelected;
                lifesText.color = Constantes.shopTabSelectedText;
				coinsButton.color = Constantes.shopTabNotSelected;
                coinsText.color = Constantes.shopTabNotSelectedText;
				enoughSpeed = false;
			}
		}
		if (mustLerpRight) {
			if (horizontal.value < 0.99) {
				horizontal.value += 4 * Time.deltaTime;
			}
			else {
				horizontal.value = 1;
				mustLerpRight = false;
                lifesButton.color = Constantes.shopTabNotSelected;
                coinsButton.color = Constantes.shopTabSelected;
				lifesText.color = Constantes.shopTabNotSelectedText;
				coinsText.color = Constantes.shopTabSelectedText;
				enoughSpeed = false;
			}
		}
	}

	public void MoveScroll(Vector2 vector){
		isMoving = true;
		xPosition = vector.x;
	}

	public void BuyCoins(int monedas){
		int totalCoins = PlayerPrefs.GetInt ("Coins") + monedas;
		PlayerPrefs.SetInt ("Coins", PlayerPrefs.GetInt ("Coins") + monedas);
		textCoins.text = totalCoins + "";
	}

	public void BuyLifes(int vidas){
		int totalLifes = PlayerPrefs.GetInt ("Lifes") + vidas;
		PlayerPrefs.SetInt ("Lifes", PlayerPrefs.GetInt ("Lifes") + vidas);
		textLifes.text = totalLifes + "";
	}

	public void ActivateLifesPanel(){
		mustLerpLeft = true;
	}

	public void ActivateCoinsPanel(){
		mustLerpRight = true;
	}

	public void WatchVideoLifes(){
		GameObject.Find("ObjetoSingleton").GetComponent<AdManager>().ShowRewardVideoLifes();
	}

	public void WatchVideoCoins(){
		GameObject.Find("ObjetoSingleton").GetComponent<AdManager>().ShowRewardVideoCoins();
	}
	
	public void BackToMainMenu() {
        AutoFade.LoadLevel("EscenaInicio", 0.2f, 0.2f, Color.white);
    }
}
