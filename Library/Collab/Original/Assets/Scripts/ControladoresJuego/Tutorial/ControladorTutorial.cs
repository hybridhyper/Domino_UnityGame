/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControladorTutorial : ControladorMaestro {

	//Estados del tutorial 
	public enum TutorialStates{ //Estados del tutorial
		ShowQuestion, //Con una flecha se señala la zona que contendrá la pregunta (ya sea imagen o texto)
		ShowChips, //Con una flecha se señala la zona de fichas, donde se encuentran las fichas que se habrá de utilizar para formar las respuestas a las preguntas
		SetIncorrectChip, //Se permite al usuario colocar una ficha en el tablero (es una ficha concreta, las demás no sepodrán mover)
		ShowUserWord, //Con una flecha se señala el recuadro que contiene la palabra que el usuario va formando con las fichas
		CleanBoard, //Con una flecha se señala el botón de limpiar tablero
		SetCorrectChip1, //Se permite al usuario colocar la primera ficha correcta en el tablero (es una ficha concreta, las demás no sepodrán mover)
		SetCorrectChip2, //Se permite al usuario colocar la segunda ficha correcta en el tablero (es una ficha concreta, las demás no sepodrán mover)
		SetCorrectChip3, //Se permite al usuario colocar la tercera ficha correcta en el tablero (es una ficha concreta, las demás no sepodrán mover)
		PowerUpSkipQuestion, //Con una flecha se señala el botón de pasar pregunta para que el usuario lo pulse
		ShowPowerUpPrice, //Con una flecha se señalan las monedas del jugador para mostrar que los powerups tienen coste
		PowerUpFirstChip, //Con una flecha se señala el botón de colocar primera ficha para que el usuario lo pulse
		PowerUpDeleteChip, //Con una flecha se señala el botón de eliminar ficha para que el usuario lo pulse
		UserKnowHowToPlay, //Mensaje para el usuario, indicando que ya sabe lo que tiene que saber
		LetUserAnswer, //Se permite jugar al usuario con total libertad para que conteste la pregunta activa (aunque no podrá pasar pregunta)
		ExitTutorial //Fin del tutorial, se espera a que el jugador pulse el botón de salir
	}
	//UI 
	Image imageFilter;
	Text coinsText;
	Text nextQuestionPriceText;
	Text setFirstPriceText;
	Text deleteChipPriceText;
	public Text currentWord;
	public Image questionImage; //Imagen de pregunta
	//Gameobjects y variables del juego
	DropHere dropHereTableroJuego;
	ControladorEstadoTutorial stateController;
	public AudioClip[] clipsFX = new AudioClip[4]; //Audios para los efectos.
	GameObject setChipHere; //Texto de la Zona de drop
	GameObject powerupFirstChip; //Objeto de powerup 
	GameObject powerupSkipQuestion;
	GameObject powerupBomb;
	GameObject chipsSet; //Objeto que tiene como childs las fichas colocadas.
	GameObject chipZone; //Zona donde se coloca las fichas.
	GameObject initialDropZone; //Zona de drop inicial
	GameObject hideChipsPanel; //Panel transparente que oculta las fichas para impedir que puedan usarse en determinados estados
	GameObject changeStateButton; //Botón del tamaño de la pantalla que activa el cambio de estado
	GameObject[] organizedArrayChips = new GameObject[7]; //Array que contiene las fichas ordenadas por su posición en el juego
	GameObject fingerImage; //Objeto con la imagen de una mano que se usa para señalar las fichas que se han de mover con una animación
	string userWord;
	string winningWord;
	List<Transform> chipList = new List<Transform>(); //Lista de fichas.
	Dictionary<string, Transform> dropZoneDic = new Dictionary<string, Transform>();
	List<image_answer> list_image_answer = new List<image_answer>(); //Lista de estructura para saber preguntas y respuestas del juego (imagenes)
	List<string> pairChips = new List<string>();
	Stack<LastChipObject> stackChipSet = new Stack<LastChipObject>();
	Transform firstCorrectChip; //Guardamos el objeto de la primera ficha correcta.
	public List<Transform> listNoValidChips = new List<Transform>(); //Lista de objetos (fichas) no validas.													 //Precios iniciales para los powerups.
	int skipQuestionPrice = Constantes.SKIPQUESTION_PRICE;
	int firstChipPrice = Constantes.FIRSTCHIP_PRICE;
	int deleteChipPrice = Constantes.DELETECHIP_PRICE;
	int playerCoins; //Monedas actuales del jugador.
	int tutorialState = 0; //Estado del tutorial en donde se encuentra el jugador.
	int questionCounter = 0; //Indicador del número de la pregunta utilizada
	int playerCoinsTutorial = 100; //Monedas que tendrá el jugador mientras juega al tutorial
	bool startTimerCorrectAnswer = false;
	bool tutorialIsStarted = false; //Bool que indica que el tutorial ha empezado (al darle al play)
	bool lastStateHelp = false; //Bool usado para activar la ayuda de los minidrops en el último estado
	float correctAnswerCounter = 0; //Tiempo que contamos para dejar las fichas colocadas en el tablero.
	int counterDeleteChips = 0; //Contador que servirá para saber cuantas fichas se han eliminado.
	bool isFirstChipSet = false; //Hay una primera ficha colocada en el tablero para saber si debemos mostrar las ayudas.
	bool powerupFirstChipSet = false; //Este bool determina si se ha utilizado el pwp de colocar primera ficha.
	float timeWithoutSet = 0; //Tiempo que contara si el usuario no ha puesto ficha
	string[] pairsAux = { "EL", "JA", "PE", "FE", "SO", "CA", "CE", "AP", "QU", "FA", "AF", "RT", "SI", "PO", "PA" };
	//Eventos y delegados.
	public delegate void delegateHelp(); //Declaramos un delegado
	public static event delegateHelp OnTimeHelp; //Declaramos el evento de poner ayudas en el tablero 
	public static event delegateHelp OnHideTimeHelp; //Declaramos un evento de esconder ayudas del tablero.
	public enum Clips { CorrectAnswer, Clean, Bomb, NextQuestion } //ENUMERADOS DE POWERUPS PARA LOS SONIDO
	//ESTRUCTURAS
	public struct image_answer {
		public Sprite image;
		public string answer;
	}

    void Start() {
        FindObjects();
        OrderChipsArray();
		SearchMiniDrops();
		SetImageAnswer ();
		SetPrices();
        ShowCoins();
        DisableEverything();
	}

	void FindObjects() {
		GameObject.Find("ObjetoSingleton").GetComponent<PreguntasSingleton>().setControllerGameNumber(3);
		changeStateButton = GameObject.Find("ChangeStateButton");
		changeStateButton.SetActive(false);
		hideChipsPanel = GameObject.Find("HideChipsPanel");
		powerupFirstChip = GameObject.Find("PanelPrimera");
		powerupSkipQuestion = GameObject.Find("PanelPasar");
		powerupBomb = GameObject.Find("PanelEliminar");
		chipsSet = GameObject.Find("FichasColocadas");
		chipZone = GameObject.Find("ZonaFichas");
		stateController = GameObject.Find("ControladorEstados").GetComponent<ControladorEstadoTutorial>();
		initialDropZone = GameObject.Find("DropZone");
		dropHereTableroJuego = GameObject.Find("TableroJuego").GetComponent<DropHere>();
		coinsText = GameObject.Find("TextoMonedas").GetComponent<Text>();
		nextQuestionPriceText = GameObject.Find("PrecioPasarPregunta").GetComponent<Text>();
		setFirstPriceText = GameObject.Find("PrecioPonerPrimera").GetComponent<Text>();
		deleteChipPriceText = GameObject.Find("PrecioEliminarFicha").GetComponent<Text>();
		imageFilter = GameObject.Find("Filtro").GetComponent<Image>();
        setChipHere = GameObject.Find("ArrastraAqui");
        setChipHere.SetActive(false);
        powerupSkipQuestion.GetComponent<Animator>().enabled = false;
        powerupFirstChip.GetComponent<Animator>().enabled = false;
        powerupBomb.GetComponent<Animator>().enabled = false;
	}

    void OrderChipsArray(){ //Mete en el array ordenado de fichas las fichas por su nombre.
        foreach (Transform chips in chipZone.transform){
            organizedArrayChips[int.Parse(chips.name.Substring(chips.name.Length - 1, 1)) - 1] = chips.gameObject;
        }
    }

	void DisableEverything(){
		hideChipsPanel.SetActive(false);
		GameObject.Find("LimpiarTablero").GetComponent<Button>().enabled = false; //Se desactiva el botón de limpiar tablero
		ActivateChips(false); //Se desactivan todas las fichas
	}

	void ActivateChips(bool activate){ //Se activan o desactivan todas las fichas
		for (int i = 0; i < organizedArrayChips.Length; i++)
            organizedArrayChips[i].GetComponent<DragFicha>().enabled = activate;
    }

	void SetImageAnswer(){ //Llenamos el array con las preguntas necesarias para el tutorial
		image_answer i_r = new image_answer();
		i_r.image = Resources.Load<Sprite>("COLLAR"); //Cargamos la imagen
		i_r.answer = "COLLAR"; //Guardamos la respuesta
		list_image_answer.Add(i_r); //Guardamos la pregunta
		i_r = new image_answer();
		i_r.image = Resources.Load<Sprite>("INDICE");
		i_r.answer = "INDICE";
		list_image_answer.Add(i_r);
		i_r = new image_answer();
		i_r.image = Resources.Load<Sprite>("OPERAR");
		i_r.answer = "OPERAR";
		list_image_answer.Add(i_r);
	}

	void SetChipPairs(){ //Llenamos el array con los pares de letras de la pregunta. Las fichas correctas serán la 2, la 4 y la 6
        pairChips.Clear();
		pairChips.Add (pairsAux[Random.Range(0, 14)]);
		pairChips.Add(list_image_answer[questionCounter].answer.Substring(2, 2));
		pairChips.Add (pairsAux[Random.Range(0, 14)]);
		pairChips.Add (list_image_answer[questionCounter].answer.Substring(4, 2));
		pairChips.Add (pairsAux[Random.Range(0, 14)]);
		pairChips.Add (list_image_answer[questionCounter].answer.Substring(0, 2));
		pairChips.Add (pairsAux[Random.Range(0, 14)]);
	}

	public int GetTutorialState(){ //Devuelve el estado del tutorial
        return tutorialState;
    }

	override public string DeleteSpecialChars(string imageName) {
		string specialChars = "áàäéèëíìïóòöúùüñÁÀÄÉÈËÍÌÏÓÒÖÚÙÜÑçÇ";
		string charsASCII = "aaaeeeiiiooouuunAAAEEEIIIOOOUUUNcC";
		for (int i = 0; i < specialChars.Length; i++){
			imageName = imageName.Replace(specialChars[i], charsASCII[i]);
		}
		return imageName;
	}

	void SetPrices() {
		nextQuestionPriceText.text = "" + skipQuestionPrice;
		setFirstPriceText.text = "" + firstChipPrice;
		deleteChipPriceText.text = "" + deleteChipPrice;
	}

	override public void GameStarts() {
		changeStateButton.SetActive (true); //Activamos el botón que permitirá el cambio de estado
		tutorialIsStarted = true; //El tutorial da comienzo
		SetChipPairs(); //Se llenan las fichas con las letras
		PrepareQuestion(); //Preparamos preguntas.
		InactiveLayout(); //Ocultamos los powerups
	}

	void SearchMiniDrops() {
		foreach (Transform child in chipsSet.transform){
			if (child.tag == "MiniDrop")
				dropZoneDic.Add(child.name, child);
		}
	}

	//TODO: Necesitamos acceder a los minidrops de cada controllador.... eso como lo podriamos hacer?
	override public Transform GetMiniDrop(string _miniDropName) {
		return dropZoneDic[_miniDropName];
	}

	void WritePairsIntoChips()
	{
		List<Transform> listAuxCorrectChips = new List<Transform>(); //Lista de fichas correctas.
		int counter = 0;
		bool firstChipFound = false;
		string auxWinningWord = winningWord;
        foreach (GameObject child in organizedArrayChips) {
			int charCounter = 0;
			string pairOfChars = "";
			foreach (Transform text in child.transform) {
				pairOfChars += pairChips[counter].Substring(charCounter, 1);
				text.GetComponent<Text>().text = pairChips[counter].Substring(charCounter, 1);
				charCounter++;
			}

			if (pairOfChars == winningWord.Substring(0, 2) && !firstChipFound){
				firstCorrectChip = child.transform;
				firstChipFound = true;
			}
			//Si aún quedan pares de letra por encontrar entre las fichas entramos
			if (auxWinningWord.Length != 0) {
				//Recorremos la palabra en pares de letras
				for (int i = 0; i < auxWinningWord.Length; i += 2) {
					//Si la ficha tiene las mismas letras que la palabra ganadora, guardamos esa ficha
					if (auxWinningWord.Substring(i, 2) == pairOfChars) {
						listAuxCorrectChips.Add(child.transform);
						auxWinningWord = auxWinningWord.Remove(i, 2);
					}
				}
			}
			pairOfChars = "";
			counter++;
		}

        foreach (GameObject child in organizedArrayChips) {
			bool correctChipFound = false;
			int loopCounter = 0;
            while (!correctChipFound && loopCounter < listAuxCorrectChips.Count) {
                if (child.gameObject == listAuxCorrectChips[loopCounter].gameObject){
                    correctChipFound = true;
                    listAuxCorrectChips.RemoveAt(loopCounter);
                }
				loopCounter++;
			}
            if (!correctChipFound)
                listNoValidChips.Add(child.transform);
		}
	}

	void PrepareQuestion() {
        winningWord = list_image_answer[questionCounter].answer;
        questionImage.sprite = list_image_answer[questionCounter].image;
        questionCounter++;
        WritePairsIntoChips();
    }

    public override bool GetStartedGame() { //Ya que en Tutorial no tenemos "startedGame", esto devuelve true para que pueda funcionar en el drag ficha.
        return true;
    }

	override public void CheckDragDrop(Vector3 recolocationPositionValue, LastChipObject lastChipSetObject){
		if (timeWithoutSet > 7.9) //Si el jugador coloca, ocultamos la ayuda.
            OnHideTimeHelp();
		isFirstChipSet = true;
		timeWithoutSet = 0;
		userWord = "";
		chipList.Clear();
		int countChip = 0;
		bool firstChip = false;
        foreach (Transform child in chipsSet.transform) { //Comprueba si la palabra formada por las fichas es la ganadora.
            countChip++;
            if (child.name == "DropZone" && child.gameObject.activeInHierarchy) { //Es la primera ficha que se coloca
                dropHereTableroJuego.enabled = true;
                child.gameObject.SetActive(false);
                firstChip = true;
            }
            if (child.tag == "Ficha")
            {
                foreach (Transform text in child.transform)
                {
                    string charWord = text.GetComponent<Text>().text;
                    userWord = userWord + charWord;
                }
                chipList.Add(child);
                if (firstChip)
                    chipList[0].localPosition = new Vector3(0, 0, 0);
            }
        }
		if (!firstChip)
			stackChipSet.Push (lastChipSetObject);

       CheckWordOnBoard(lastChipSetObject.lastChipSet, recolocationPositionValue, firstChip);
	}

	void CheckWordOnBoard(Transform lastChipSet, Vector3 recolocationPositionValue, bool firstChip) {
		currentWord.text = userWord;
        if (userWord.ToUpper() == winningWord.ToUpper()) //Es palabra ganadora.
            WinningWordFunction(lastChipSet, recolocationPositionValue);
		else { //No es la palabra ganadora, sigue poniendo fichas. Generamos nuevos drops.
            if (lastChipSet.name.IndexOf("Horizontal", System.StringComparison.Ordinal) != -1){ //Si encuentra la cadena "Horizontal" (valor distinto de -1)
				if (firstChip)
					GenerateHorizontalDrops(lastChipSet, dropZoneDic["Derecha"], dropZoneDic["Abajo"], dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
				else { //Si no es primera ficha.....
					chipsSet.transform.localPosition += recolocationPositionValue;
					if (lastChipSet.GetSiblingIndex() == 0) //Si la última ficha puesta está en la primera posición entonces hay que  cambiar la posición de las dropZone "Arriba" e "Izquierda"
						MoveUpperDropzonesWithHorizontalChip(lastChipSet, dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
					else //En caso contrario se moverán las dropZone inferiores
						MoveLowerDropzonesWithHorizontalChip(lastChipSet, dropZoneDic["Abajo"], dropZoneDic["Derecha"]);
				}
			} else {
				if (firstChip)
					GenerateVerticalDrops(lastChipSet, dropZoneDic["Derecha"], dropZoneDic["Abajo"], dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
				else {
					chipsSet.transform.localPosition += recolocationPositionValue;
					if (lastChipSet.GetSiblingIndex() == 0) //Si la última ficha puesta está en la primera posición entonces hay que  cambiar la posición de las dropZone "Arriba" e "Izquierda"
						MoveUpperDropzonesWithVerticalChip(lastChipSet, dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
					else //En caso contrario se moverán las dropZone inferiores
						MoveLowerDropzonesWithVerticalChip(lastChipSet, dropZoneDic["Abajo"], dropZoneDic["Derecha"]);
				}
			}
				
			if ((int)TutorialStates.LetUserAnswer != tutorialState)
				ChangeOfState(); //El jugador ha colocado bien la ficha.
		}
	}

	void WinningWordFunction(Transform lastChipSet, Vector3 recolocationPositionValue) {
		PlaySounds((int)Clips.CorrectAnswer); //Contador para mantener las palabras por 'x' segundos en el tablero.
		startTimerCorrectAnswer = true;
		if (lastChipSet.name.IndexOf("Horizontal", System.StringComparison.Ordinal) != -1) //Si encuentra la cadena "Horizontal" (valor distinto de -1)
			chipsSet.transform.localPosition += recolocationPositionValue;
		else
			chipsSet.transform.localPosition += recolocationPositionValue;
	}

	override public void SkipQuestion() {
		if (playerCoinsTutorial < skipQuestionPrice){
			//NoCoins
		} else {
			PlaySounds((int)Clips.NextQuestion);
            powerupSkipQuestion.GetComponent<Button>().enabled = false;
			playerCoinsTutorial -= skipQuestionPrice;
			skipQuestionPrice = (skipQuestionPrice * 3) / 2;
			nextQuestionPriceText.text = "" + skipQuestionPrice;
			ShowCoins();
            ChangeOfState();
			NewQuestion();
		}
	}

    override public void PowerUpSetFirstChip() {
        if (!powerupFirstChipSet) //Si ya ha utilizado el power up no se le permite volver a lanzarlo (no tendría sentido)
            if (playerCoinsTutorial >= firstChipPrice) {
                CleanAllBoard();
                firstCorrectChip.GetComponent<Image>().color = Constantes.COLOR_GREEN;
                firstCorrectChip.GetComponent<DragFicha>().parentToReturnTo = chipsSet.transform;
                firstCorrectChip.GetComponent<DragFicha>().SetChip(initialDropZone.transform.GetComponent<RectTransform>());
                setFirstPriceText.text = "" + firstChipPrice;
                powerupFirstChipSet = true;
                powerupFirstChip.SetActive(false);
                playerCoinsTutorial -= firstChipPrice;
                firstChipPrice = firstChipPrice * 3 / 2;
                ShowCoins();
            }
    }

	override public void PowerUpDeleteChip() {
		if (counterDeleteChips < listNoValidChips.Count) {
			if (playerCoinsTutorial >= deleteChipPrice) {
				PlaySounds((int)Clips.Bomb);
				CleanAllBoard();
				listNoValidChips[counterDeleteChips].gameObject.SetActive(false);
				counterDeleteChips++;
				playerCoinsTutorial -= deleteChipPrice;
				deleteChipPrice = deleteChipPrice * 3 / 2;
				deleteChipPriceText.text = "" + deleteChipPrice;
                ShowCoins();
                if (counterDeleteChips >= listNoValidChips.Count)
                    powerupBomb.GetComponent<Button>().interactable = false;
			}
		}
	}

	override public void NewQuestion() {
		counterDeleteChips = 0;
		powerupFirstChipSet = false;
		firstCorrectChip.GetComponent<Image>().color = new Color32(238, 231, 231, 255);
		RecoverDeletedChips();
		CleanAllBoard();
        SetChipPairs();
		PrepareQuestion();
	}

	void RecoverDeletedChips() {
		foreach (Transform chips in listNoValidChips) {
			chips.gameObject.SetActive(true);
		}
		listNoValidChips.Clear();
	}

	override public void CleanBoardButton() {
		if (tutorialState == (int)TutorialStates.CleanBoard)
            ChangeOfState();
		if (chipsSet.transform.childCount > 6)
            PlaySounds((int)Clips.Clean);
		else if (chipsSet.transform.childCount > 5 && !powerupFirstChipSet)
            PlaySounds((int)Clips.Clean);
		CleanAllBoard();
	}

	override public void CleanAllBoard() {
		stackChipSet.Clear ();
		dropHereTableroJuego.enabled = false;
		isFirstChipSet = false;
		currentWord.text = "";
		timeWithoutSet = 0;
        List<DragFicha> cleanListChips = new List<DragFicha>(); //Usamos esta lista para no quitar hijos mientras los busca. Primero buscamos y luego quitamos.
		foreach (Transform child in chipsSet.transform) {
			DragFicha componente = child.GetComponent<DragFicha>();
			if (child.tag == "MiniDrop")
				child.gameObject.SetActive(false);
            if (componente != null) 
                cleanListChips.Add(componente);
            }
		
        foreach (DragFicha chip in cleanListChips) { //Tras haberlo buscado, ya los podemos quitar.
            print(chip.transform.name + "ESTA SIENDO ENABLED");
			chip.enabled = true;
            print(chip.transform.name + " ES " + chip.enabled);
            chip.BackToInitialPosition();
        }

		//Recuperar zona de drop grande y recolocar en el centro.
		initialDropZone.SetActive(true);
		chipsSet.transform.localPosition = Vector3.zero;
		if (powerupFirstChipSet) {
			firstCorrectChip.GetComponent<DragFicha>().shouldSound = false;
			firstCorrectChip.GetComponent<DragFicha>().parentToReturnTo = chipsSet.transform;
			firstCorrectChip.GetComponent<DragFicha>().SetChip(initialDropZone.transform.GetComponent<RectTransform>()); //Al limpiar el tablero vuelve a poner la primera ficha colocada.
		}
	}

	void GenerateHorizontalDrops(Transform chip, Transform rightDropChip, Transform downDropChip, Transform upDropChip, Transform leftDropChip) {
		float sizeChipRenderX = chip.GetComponent<RectTransform>().rect.width;
		float sizeChipRenderY = chip.GetComponent<RectTransform>().rect.height;
		float positionXChip = chip.localPosition.x;
		float positionYChip = chip.localPosition.y;
		//Horizontales
		//Derecha
		rightDropChip.gameObject.SetActive(true);
		rightDropChip.transform.localPosition = new Vector3(positionXChip + ((sizeChipRenderX * 3) / 4), positionYChip);
		//Abajo:
		downDropChip.gameObject.SetActive(true);
		downDropChip.transform.localPosition = new Vector3(positionXChip + (sizeChipRenderX / 4), positionYChip - (sizeChipRenderY));
		//Arriba
		upDropChip.gameObject.SetActive(true);
		upDropChip.transform.localPosition = new Vector3(positionXChip - (sizeChipRenderX / 4), positionYChip + (sizeChipRenderY));
		//Izquierda
		leftDropChip.gameObject.SetActive(true);
		leftDropChip.transform.localPosition = new Vector3(positionXChip - ((sizeChipRenderX * 3) / 4), positionYChip);
	}

	void GenerateVerticalDrops(Transform chip, Transform rightDropChip, Transform downDropChip, Transform upDropChip, Transform leftDropChip) {
    	float sizeChipRenderX = chip.GetComponent<RectTransform>().rect.width;
		float sizeChipRenderY = chip.GetComponent<RectTransform>().rect.height;
		float positionXChip = chip.GetComponent<Transform>().localPosition.x;
		float positionYChip = chip.GetComponent<Transform>().localPosition.y;

		//VERTICALES
		//Derecha
		rightDropChip.gameObject.SetActive(true);
		rightDropChip.transform.localPosition = new Vector3(positionXChip + sizeChipRenderX, positionYChip - (sizeChipRenderY / 4));
		//Abajo 
		downDropChip.gameObject.SetActive(true);
		downDropChip.transform.localPosition = new Vector3(positionXChip, positionYChip - ((sizeChipRenderY * 3) / 4));
		//Arriba
		upDropChip.gameObject.SetActive(true);
		upDropChip.transform.localPosition = new Vector3(positionXChip, positionYChip + ((sizeChipRenderY * 3) / 4));
		//Izquierda
		leftDropChip.gameObject.SetActive(true);
		leftDropChip.transform.localPosition = new Vector3(positionXChip - sizeChipRenderX, positionYChip + (sizeChipRenderY / 4));
	}

	void MoveUpperDropzonesWithHorizontalChip(Transform lastChipSet, Transform upDropChip, Transform leftDropChip) {
		Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
		//Arriba
		upDropChip.localPosition = new Vector3(lastChipSet.localPosition.x - (rectLastChip.width / 4), lastChipSet.localPosition.y + (rectLastChip.height));
		//Izquierda
		leftDropChip.localPosition = new Vector3(lastChipSet.localPosition.x - ((rectLastChip.width * 3) / 4), lastChipSet.localPosition.y);
	}

	void MoveLowerDropzonesWithHorizontalChip(Transform lastChipSet, Transform downDropChip, Transform rightDropChip) {
		Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
		//Arriba
		downDropChip.localPosition = new Vector3(lastChipSet.localPosition.x + (rectLastChip.width / 4), lastChipSet.localPosition.y - (rectLastChip.height));
		//Izquierda
		rightDropChip.localPosition = new Vector3(lastChipSet.localPosition.x + ((rectLastChip.width * 3) / 4), lastChipSet.localPosition.y);
	}

	void MoveUpperDropzonesWithVerticalChip(Transform lastChipSet, Transform upDropChip, Transform leftDropChip) {
		Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
		//Arriba
		upDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x, lastChipSet.localPosition.y + ((rectLastChip.height * 3) / 4));
		//Izquierda
		leftDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x - rectLastChip.width, lastChipSet.localPosition.y + (rectLastChip.height / 4));
	}

	void MoveLowerDropzonesWithVerticalChip(Transform lastChipSet, Transform downDropChip, Transform rightDropChip){
		Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
		//Derecha
		rightDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x + rectLastChip.width, lastChipSet.localPosition.y - (rectLastChip.height / 4));
		//Abajo 
		downDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x, lastChipSet.localPosition.y - ((rectLastChip.height * 3) / 4));
	}

	void Update(){
        if (tutorialIsStarted){
            if (isFirstChipSet) {
                if (lastStateHelp) {
                    if (timeWithoutSet > Constantes.MAX_TIME_WITHOUTSET)
                        OnTimeHelp();
                    else
                        timeWithoutSet += 1 * Time.deltaTime;
                }
            }
            if (startTimerCorrectAnswer) {
                if (correctAnswerCounter > 0.75f) {
                    correctAnswerCounter = 0;
                    startTimerCorrectAnswer = false;
					currentWord.text = "";
                    if ((int)TutorialStates.SetCorrectChip3 == tutorialState)
                        ChangeOfState();
                    if ((int)TutorialStates.LetUserAnswer == tutorialState)
                        ChangeOfState();
                    else
                        NewQuestion();
                } else
                    correctAnswerCounter += 1 * Time.deltaTime;
                    imageFilter.fillAmount = correctAnswerCounter / 0.75f;
            }
        }
	}

	public void InactiveLayout() { //Se ocultan los powerups
        powerupSkipQuestion.transform.parent.GetComponent<HorizontalLayoutGroup>().enabled = false;
		powerupBomb.SetActive(false);
		powerupFirstChip.SetActive(false);
		powerupSkipQuestion.SetActive(false);
	}

	public void ChangeOfState(){
        tutorialState++;
		switch (tutorialState) {
		case (int)TutorialStates.ShowChips:
			stateController.ChangeStateToShowChips (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			break;
		case (int)TutorialStates.SetIncorrectChip:
			setChipHere.SetActive (true); //Activamos el texto de la Zona de drop
			changeStateButton.SetActive (false); //Desactivamos el botón de cambio de estado
			ChangeColorToChip (true, organizedArrayChips [2].transform); //Ponemos la ficha que va a activarse en verde
			AnimateTutorialElement (true, organizedArrayChips [2].transform); //Animamos la ficha activa para hacerla más visible
			AnimateDropZone (true); //Animamos la zona de drop para que destaque más
			stateController.ChangeStateToSetIncorrect (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			organizedArrayChips [2].GetComponent<DragFicha> ().enabled = true; //Habilitar la ficha incorrecta
			break;
		case (int)TutorialStates.ShowUserWord:
			ChangeColorToChip (false, organizedArrayChips [2].transform); //Recuperamos el color blanco de la ficha
			AnimateTutorialElement (false, organizedArrayChips [2].transform); //Paramos la animación de la ficha
			AnimateDropZone (false); //Paramos la animación de la zona de drop
			changeStateButton.SetActive (true); //Volvemos a activar el botón para poder pasar de estado
			stateController.ChangeStateToShowUserWord (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			break;
		case (int)TutorialStates.CleanBoard:
			AnimateTutorialElement (true, GameObject.Find ("LimpiarTablero").transform); //Animamos el botón de limpiar tablero
			changeStateButton.SetActive (false); //Desactivamos el botón de cambio de estado
			GameObject.Find ("LimpiarTablero").GetComponent<Button> ().enabled = true; //Activamos el componente Button del botón Limpiar tablero
			stateController.ChangeStateToCleanBoard (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			//Se oculta el mensaje anterior de UserWord y se muestra un mensaje indicando que hay que borrar el tablero
			//Al pulsar CleanBoard se pasa al siguiente estado directamente
			break;
		case (int)TutorialStates.SetCorrectChip1:
			AnimateTutorialElement (false, GameObject.Find ("LimpiarTablero").transform); //Paramos la animación del botón de limpiar tablero
			GameObject.Find ("LimpiarTablero").GetComponent<Button> ().enabled = false; //Desactivamos el botón de limpiar tablero
			ChangeColorToChip (true, organizedArrayChips [1].transform); //Cambiamos a verde el color de la ficha correspondiente
			AnimateTutorialElement (true, organizedArrayChips [1].transform); //Animamo
			stateController.ChangeStateToSetCorrectChip1 (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			organizedArrayChips [1].GetComponent<DragFicha> ().enabled = true;
			break;
		case (int)TutorialStates.SetCorrectChip2:
			ChangeColorToChip (true, organizedArrayChips [5].transform); //Cambiamos color a la ficha 6
			AnimateTutorialElement (true, organizedArrayChips [5].transform); //Animamos la ficha 6
			ChangeColorToChip (false, organizedArrayChips [1].transform); //Recuperamos el color blanco de la ficha anterior
			AnimateTutorialElement (false, organizedArrayChips [1].transform); //Paramos la animación de la ficha anterior
			stateController.ChangeStateToSetCorrectChip2 (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			organizedArrayChips [5].GetComponent<DragFicha> ().enabled = true; //Activamos el DragFicha de la ficha que se va a colocar para que sepueda mover
			break;

		case (int)TutorialStates.SetCorrectChip3:
			ChangeColorToChip (true, organizedArrayChips [3].transform); //Cambiamos color a la ficha 4
			AnimateTutorialElement (true, organizedArrayChips [3].transform); //Animamos la ficha 4
			ChangeColorToChip (false, organizedArrayChips [5].transform); //Recuperamos el color blanco de la ficha anterior
			AnimateTutorialElement (false, organizedArrayChips [5].transform); //Paramos la animación de la ficha anterior
			stateController.ChangeStateToSetCorrectChip3 (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			organizedArrayChips [3].GetComponent<DragFicha> ().enabled = true; //Activamos el DragFicha de la ficha que se va a colocar para que sepueda mover
			//Cuando el jugador coloca la última ficha correctamente se pasa de pregunta y se activa el siguiente estado.
			break;
        case (int)TutorialStates.PowerUpSkipQuestion:
			setChipHere.SetActive (false); //Desactivamos el texto de la Zona de drop
			ChangeColorToChip (false, organizedArrayChips [3].transform); //Recuperamos el color de la ficha anterior
			AnimateTutorialElement (false, organizedArrayChips [3].transform); //Paramos la animación de la ficha anterior
			hideChipsPanel.SetActive (true); //Activamos el panel transparente de las fichas para que no sepuedan usar
			powerupSkipQuestion.SetActive (true); //Mostramos el powerup de Pasar pregunta
            powerupSkipQuestion.GetComponent<Animator>().enabled = true;
			stateController.ChangeStateToShowPowerupSkipQuestion (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			//Remarcamos que debe pulsar el botón de pasar pregunta y deshabilitamos todos los demás botones.
			//Si le da al botón pasa al siguiente estado.
            break;
		case (int)TutorialStates.ShowPowerUpPrice:
			changeStateButton.SetActive (true); //Activamos el botón grande de cambio de estado
			powerupSkipQuestion.GetComponent<Button> ().enabled = false;
            Destroy(powerupSkipQuestion.GetComponent<Animator>());
            powerupSkipQuestion.transform.localScale = new Vector3(1, 1, 1);
			stateController.ChangeStateToShowPrices (); //Llamada a ControladorEstadosTutorial para cambiar de estado
            break;
		case (int)TutorialStates.PowerUpFirstChip:
			changeStateButton.SetActive (false); //Desactivamos el botón de cambio de estado
			ActivateChips (true); //Activamos todas las fichas (parapoder colocar la primera)
			powerupSkipQuestion.GetComponent<Button> ().enabled = true;
            powerupFirstChip.GetComponent<Animator>().enabled = true;
			powerupFirstChip.SetActive (true); //Activamos el powerup de colocar primera ficha
			powerupSkipQuestion.SetActive (false); //Se desactiva el powerup de pasar pregunta
			stateController.ChangeStateToShowFirstChip (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			//Si le da al botón pasa al siguiente estado.
            break;
        case (int)TutorialStates.PowerUpDeleteChip:
            Destroy(powerupFirstChip.GetComponent<Animator>());
            powerupFirstChip.transform.localScale = new Vector3(1, 1, 1);
			powerupBomb.GetComponent<Animator>().enabled = true;
			powerupBomb.SetActive (true); //Se activa el powerup de eliminar ficha
			stateController.ChangeStateToShowDeleteChip (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			//Si le da al botón pasa al siguiente estado.
            break;
		case (int)TutorialStates.UserKnowHowToPlay:
            Destroy(powerupBomb.GetComponent<Animator>());
            powerupBomb.transform.localScale = new Vector3(1, 1, 1);
			hideChipsPanel.SetActive (false); //Desactivamos el panel que oculta las fichas
			changeStateButton.SetActive (true); //Activamos el botón para cambiar de estado
			powerupBomb.GetComponent<Button> ().enabled = false;
			stateController.ChangeStateToUserKnowHowToPlay (); //Llamada a ControladorEstadosTutorial para cambiar de estado
            break;
		case (int)TutorialStates.LetUserAnswer:
			GameObject.Find ("TableroJuego").GetComponent<Button> ().interactable = true; //Activamos la función Undo del tablero para deshacer movimientos
			lastStateHelp = true;
			changeStateButton.SetActive (false); //Desactivamos el botón de cambio de estado. A partir de aquí ya no es necesario
			hideChipsPanel.SetActive (false); //Desactivamos el panel que oculta las fichas
            powerupBomb.GetComponent<Button> ().enabled = true;
			powerupSkipQuestion.GetComponent<Button> ().interactable = false; //Mostramos el botón de pasar pregunta, pero permanece desactivado
			powerupSkipQuestion.SetActive (true);
			powerupFirstChip.GetComponent<Button> ().interactable = false; //Mostramos el botón de colocar la primera ficha, pero permanece desactivado
			powerupFirstChip.SetActive (true);
            /*organizedArrayChips[2].GetComponent<DragFicha>().enabled = true;
            organizedArrayChips[1].GetComponent<DragFicha>().enabled = true;
            organizedArrayChips[3].GetComponent<DragFicha>().enabled = true;*/
			GameObject.Find ("LimpiarTablero").GetComponent<Button> ().enabled = true; //Se activa el botón de limpiar tablero
			stateController.ChangeStateToLetUserPlay (); //Llamada a ControladorEstadosTutorial para cambiar de estado
			//Al completar la palabra correctamente se pasa al estado de salida
            break;
		case (int)TutorialStates.ExitTutorial:
			lastStateHelp = false; //Se desactiva la ayuda de los minidrops, para que no aparezcan por debajo de este estado
            GameObject.Find("Salir").GetComponent<Button>().interactable = true; 
            GameObject.Find("LimpiarTablero").GetComponent<Button>().enabled = false;
            hideChipsPanel.SetActive(true);
            powerupBomb.GetComponent<Button>().enabled = false;
			stateController.ChangeStateToExitTutorial (); //Llamada a ControladorEstadosTutorial para cambiar de estado
            PlayerPrefs.SetInt("TutorialDone", 1);
			PlayerPrefs.SetInt ("Coins", PlayerPrefs.GetInt ("Coins") + 100);
            break;
        }
    }

	override public void ShowCoins(){ //Actualiza las monedas del jugador dentor del tutorial
		coinsText.text = "" + playerCoinsTutorial;
	}

	override public void StartAnimation(){ //Permite la animación de acercamiento de la imagen en el estado LetUserAnswer
        if (tutorialState == (int)TutorialStates.LetUserAnswer) {
            questionImage.GetComponent<Animator>().SetTrigger("Agrandar");
        }
	}

	void PlaySounds(int clip){
		this.transform.GetComponent<AudioSource>().clip = clipsFX[clip];
		this.transform.GetComponent<AudioSource>().Play();
	}

	override public void StopAnimationHand(){ //Detiene la animación del "dedo" activo en el momento
		if (tutorialState != (int)TutorialStates.LetUserAnswer){
			fingerImage = GameObject.Find("ImagenDedo"); //Guardamos una referencia para poder reiniciarlo posteriormente en caso de no cambiar de estado
			fingerImage.SetActive(false); //Ocultamos el dedo
		}
	}

	override public void StartAnimationHand(){
		if (tutorialState != (int)TutorialStates.LetUserAnswer){
			fingerImage.SetActive(true); //Mostramos el dedo que corresponda
		}
	}

	void ChangeColorToChip(bool colorChange, Transform chipToChange){ //Cambia el color de la ficha indicada a verde o blanco, según corresponda
        if (colorChange)
            chipToChange.GetComponent<Image>().color = Constantes.COLOR_GREEN;
        else
            chipToChange.GetComponent<Image>().color = Constantes.COLOR_WHITE_CHIP;
	}

	void AnimateTutorialElement(bool animate, Transform chipToChange){ //Se anima el elemento correspondiente
		chipToChange.GetComponent<Animator>().SetBool("Anim", animate);
	}

	void AnimateDropZone(bool animate){ //Animación de la zona de Drop
		setChipHere.GetComponent<Animator>().SetBool("Anim", animate);
	}

    //LLamamos a esta función desde el Update de DragFicha
    //Al terminar de moverse la ficha a su posición inicial se comprueba el estado
    //Si no está en el estado LetUserAnswer, entonces se desactiva el DragFicha de la primera ficha colocada, y por tanto, borrada
    /*override public void CheckEnabledChip(Transform chip){
        if (tutorialState != (int)TutorialStates.LetUserAnswer)
            chip.GetComponent<DragFicha>().enabled = false;
    }*/

	public void Undo() {
		if (stackChipSet.Count == 0) {
			CleanAllBoard();
			return;
		}
		Vector3 recolocationPositionValue; //Este vector 3 sera el nuevo valor de fichas colocadas así podra reajustarse cuando se quite una ficha.
		//Necesitamos saber cual es la ultima ficha colocada.
		print(stackChipSet.Peek().miniDropPartner.name + " NUMERO DE FICHAS");
		Transform lastChipSet = stackChipSet.Peek().lastChipSet;
		Transform lastMiniDropUsed = stackChipSet.Peek().miniDropSelected;
		Vector3 lastMiniDropUsedPos = stackChipSet.Peek().miniDropSelectedPos;
		Transform lastMiniDropPartner = stackChipSet.Peek().miniDropPartner;
		Vector3 lastMiniDropPartnerPos = stackChipSet.Pop().miniDropPartnerPos;
		int positionOnBoard = lastChipSet.GetSiblingIndex(); //O es 0 o es la ultima posicion de los hijos de FichasColocadas.
		//RECOLOCAMOS LOS MINIDROPS.
		lastMiniDropUsed.localPosition = lastMiniDropUsedPos;
		lastMiniDropPartner.localPosition = lastMiniDropPartnerPos;

		//ACTUALIZAMOS LA PREGUNTA
		if (positionOnBoard == 0) {
			userWord = userWord.Substring(2, userWord.Length - 2);
			currentWord.text = userWord;
		} else {
			userWord = userWord.Substring(0, userWord.Length - 2);
			currentWord.text = userWord;
		}

		//REAJUSTAMOS EL TABLERO
		switch (lastMiniDropUsed.name) {
		case "Arriba":
			//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
			if (lastChipSet.name.IndexOf("Horizontal") != -1)
				recolocationPositionValue = new Vector3(lastChipSet.GetComponent<RectTransform>().rect.width / 4, -lastChipSet.GetComponent<RectTransform>().rect.height / 2);
			else
				recolocationPositionValue = new Vector3(0, -lastChipSet.GetComponent<RectTransform>().rect.height / 2);
			break;
		case "Izquierda":
			//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
			if (lastChipSet.name.IndexOf("Horizontal") != -1)
				recolocationPositionValue = new Vector3(lastChipSet.GetComponent<RectTransform>().rect.width / 2, 0);
			else
				recolocationPositionValue = new Vector3(lastChipSet.GetComponent<RectTransform>().rect.width / 2, -lastChipSet.GetComponent<RectTransform>().rect.height / 4);
			break;
		case "Abajo":
			//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
			if (lastChipSet.name.IndexOf("Horizontal") != -1)
				recolocationPositionValue = new Vector3(-lastChipSet.GetComponent<RectTransform>().rect.width / 4, lastChipSet.GetComponent<RectTransform>().rect.height / 2);
			else
				recolocationPositionValue = new Vector3(0, lastChipSet.GetComponent<RectTransform>().rect.height / 2);
			break;
		case "Derecha":
			//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
			if (lastChipSet.name.IndexOf("Horizontal") != -1)
				recolocationPositionValue = new Vector3(-lastChipSet.GetComponent<RectTransform>().rect.width / 2, 0);
			else
				recolocationPositionValue = new Vector3(-lastChipSet.GetComponent<RectTransform>().rect.width / 2, lastChipSet.GetComponent<RectTransform>().rect.height / 4);
			break;
		default:
			recolocationPositionValue = Vector3.zero;
			break;
		}

		chipsSet.transform.localPosition -= recolocationPositionValue;
		lastChipSet.SetParent(chipZone.transform); //Le devolvemos al padre: zona fichas.
		lastChipSet.GetComponent<DragFicha>().BackToInitialPosition();
	}
}