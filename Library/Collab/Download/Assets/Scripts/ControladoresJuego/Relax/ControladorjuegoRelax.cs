/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControladorjuegoRelax : ControladorMaestro {
    //UI
    Image imageFilter; //Filtro de la imagen correcta
	Text correctText; //Texto de preguntas acertadas
	Text coinsText; //Texto de preguntas
    Text skipQuestionPriceText; // Texto de precio de Pasar pregunta
	Text setFirstPriceText; // Texto de precio de Poner Primera
	Text deleteChipPriceText; // Texto de Borrar Ficha
	public Text currentWord; //Texto de palabra formada
	public Text questionText; //Texto de la pregunta
	public Image questionImage; //Imagen de pregunta
	TextAsset aux; //Objeto para leer txt
	public AudioClip[] clipsFX = new AudioClip[4]; //Audios para los efectos.
	//Variables y objetos del juego
	ControladorEstadosRelax statesController; //Controlador del estado
	PreguntasSingleton singletonQuestions; //Objeto para controlar las preguntas
    DropHere dropHereTableroJuego; //Script que maneja el drop del tablero
    Dictionary<string, Transform> dropZoneDic = new Dictionary<string, Transform>(); //Diccionario de minidrops 
	GameObject powerupFirstChip; //Objeto de powerup Poner primera
    GameObject powerupDeleteChip;
	GameObject chipsSet; //Objeto de las fichas colocadas.
	GameObject chipZone; //Zona donde estan colocadas al principio las fichas
	GameObject initialDropZone; //Zona de drop inicial
    GameObject boardGame; //Tablero de juego que tiene el drop here y el boton de Undo
    GameObject cleanBoardButton; //Boton que limpia el tablero
	string userWord; //Palabra formada por el usuario
	string winningWord; //Palabra ganadora
	List<string> listAuxWords = new List<string>(); //Lista en la que se va a guardar las palabras erroneas para coger de ahi los pares de letras
    List<string> pairChips = new List<string>(); //Lista donde guardamos los pares de chars (strings) 
    List<int> listAuxRandomNumbers = new List<int>(); //Lista auxiliar de integers donde guardaremos la posicion de aquellas preguntas sin usar en la lista de preguntas de PreguntasSingleton
    List<Transform> listNoValidChips = new List<Transform>(); //Lista de objetos (fichas) no validas.
    Stack<LastChipObject> stackChipSet = new Stack<LastChipObject>(); //Pila de objetos 
    bool isFirtsChipSet = false; //Hay una primera ficha colocada en el tablero para saber si debemos mostrar las ayudas.
	bool powerupFirstChipSet = false; //Este bool determina si se ha utilizado el pwp de colocar primera ficha.
	bool startedGame = false; //Booleano que servira para manejar el contador de tiempo.
	bool startTimerCorrectAnswer = false;
    int correctAnswers;
    int skipQuestionPrice = Constantes.SKIPQUESTION_PRICE;
    int firstChipPrice = Constantes.FIRSTCHIP_PRICE;
    int deleteChipPrice = Constantes.DELETECHIP_PRICE;
    int selectedQuestionPosition = 0; //Posicion elegida en el array de preguntas.
	int counterDeleteChips = 0; //Contador que servirá para saber cuantas fichas se han eliminado.
	float correctAnswerCounter = 0; //Tiempo que contamos para dejar las fichas colocadas en el tablero.
	float timeWithoutSet = 0; //Tiempo que contara si el usuario no ha puesto ficha
	Transform firstCorrectChip; //Guardamos el objeto de la primera ficha correcta.
    //Eventos y delegados.
	public delegate void delegateHelp(); //Declaramos un delegado
	public static event delegateHelp OnTimeHelp; //Declaramos el evento
	public static event delegateHelp OnHideTimeHelp;
	public enum Clips { CorrectAnswer, Clean, Bomb, NextQuestion } //ENUMERADOS DE POWERUPS PARA LOS SONIDOS.

    void Start(){
		FindObjects();
		SearchMiniDrops();
		aux = (TextAsset)Resources.Load("jeroglificos", typeof(TextAsset));
		SelectTypeOfQuestion();
		readAuxTxts();
		SetPrices();
		correctText.text = "" + correctAnswers;
		ShowCoins();
	}

	void FindObjects(){ //Busqueda de objetos
		statesController = GameObject.Find ("ControladorEstados").GetComponent<ControladorEstadosRelax> ();
        singletonQuestions = GameObject.Find("ObjetoSingleton").GetComponent<PreguntasSingleton>();
		singletonQuestions.setControllerGameNumber(2);
		powerupFirstChip = GameObject.Find("PanelPrimera");
        powerupDeleteChip = GameObject.Find("PanelEliminar");
		chipsSet = GameObject.Find("FichasColocadas");
		chipZone = GameObject.Find("ZonaFichas");
		initialDropZone = GameObject.Find("DropZone");
        boardGame = GameObject.Find("TableroJuego");
        dropHereTableroJuego = boardGame.GetComponent<DropHere>();
		correctText = GameObject.Find("textoAciertos").GetComponent<Text>();
		coinsText = GameObject.Find("TextoMonedas").GetComponent<Text>();
		skipQuestionPriceText = GameObject.Find("PrecioPasarPregunta").GetComponent<Text>();
		setFirstPriceText = GameObject.Find("PrecioPonerPrimera").GetComponent<Text>();
		deleteChipPriceText = GameObject.Find("PrecioEliminarFicha").GetComponent<Text>();
        imageFilter = GameObject.Find("Filtro").GetComponent<Image>();
        cleanBoardButton = GameObject.Find("LimpiarTableroButton");
	}

    public override bool GetStartedGame() {
        return startedGame;
    }

	override public Transform GetMiniDrop(string _miniDropName) {
		return dropZoneDic[_miniDropName];
	}

	void SetPrices(){
		skipQuestionPriceText.text = "" + skipQuestionPrice;
		setFirstPriceText.text = "" + firstChipPrice;
		deleteChipPriceText.text = "" + deleteChipPrice;
	}

	override public void GameStarts() {
		startedGame = true;
		PrepareQuestion(); //Preparamos preguntas.
	}

	void SearchMiniDrops() {
        foreach (Transform child in chipsSet.transform){
			if (child.tag == "MiniDrop")
				dropZoneDic.Add(child.name, child);
		}
	}

    void SelectTypeOfQuestion() {
		switch (Constantes.TYPE_OF_QUESTION) {
			case (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase:
				questionText.gameObject.SetActive(true);
				questionImage.gameObject.SetActive(false);
                FindFirstQuestionNotUsed((int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase);
				break;
			case (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico:
				questionText.gameObject.SetActive(false);
				questionImage.gameObject.SetActive(true);
                FindFirstQuestionNotUsed((int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico);
				break;
			case (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne:
				questionText.gameObject.SetActive(false);
				questionImage.gameObject.SetActive(true);
                FindFirstQuestionNotUsed((int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne);
				break;
			default:
				break;
		}
	}

    void FindFirstQuestionNotUsed(int tipoPregunta){ //Recorremos la lista statica que tenemos en el singleton donde hemos bajado las preguntas
        switch (tipoPregunta){
            case (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase:
                for (int i = 0; i < PreguntasSingleton.itemDB.listCompletaFrase.Count; i++){
                    //Buscamos la posición de la primera pregunta que aún no se haya contestado
                    if(!PreguntasSingleton.itemDB.listCompletaFrase[i].used){
                        for (int j = i; j < PreguntasSingleton.itemDB.listCompletaFrase.Count; j++){
                            listAuxRandomNumbers.Add(j);
                        }
                        return; //En cuanto encontramos la posición, salimos de la función
					}
                }
                break;
            case (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico:
                for (int i = 0; i < PreguntasSingleton.itemDB.listJeroglificos.Count; i++){
					//Buscamos la posición de la primera pregunta que aún no se haya contestado
					if (!PreguntasSingleton.itemDB.listJeroglificos[i].used){
						for (int j = i; j < PreguntasSingleton.itemDB.listJeroglificos.Count; j++){
							listAuxRandomNumbers.Add(j);
						}	
						return; //En cuanto encontramos la posición, salimos de la función
					}
				}
				break;
            case (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne:
                for (int i = 0; i < PreguntasSingleton.itemDB.listQueNosUne.Count; i++){
					//Buscamos la posición de la primera pregunta que aún no se haya contestado
					if (!PreguntasSingleton.itemDB.listQueNosUne[i].used){
						for (int j = i; j < PreguntasSingleton.itemDB.listQueNosUne.Count; j++){
							listAuxRandomNumbers.Add(j);
						}
						return; //En cuanto encontramos la posición, salimos de la función
					}
				}
			break;
        }
    }

	void readAuxTxts() {
		string fullTxt = aux.text;
		string[] lineAuxiliar = fullTxt.Split('\n');
		for (int i = 0; i < lineAuxiliar.Length; i++) {
			string p_r = lineAuxiliar[i].Trim();
			listAuxWords.Add(p_r);
		}
	}

	void WritePairsIntoChips() {
		for (int i = 1; i < pairChips.Count; i++)
		{  //Primero que nada, hacemos un shuffle del array de pairs.
			int randomNumber = Random.Range(0, i);
			string pairAuxiliar = pairChips[i];
			pairChips[i] = pairChips[randomNumber];
			pairChips[randomNumber] = pairAuxiliar;
		}

		List<Transform> listAuxCorrectChips = new List<Transform>(); //Lista de fichas correctas.
		int counter = 0;
		bool firstChipFound = false;
		string auxWinningWord = winningWord;
		foreach (Transform child in chipZone.transform)
		{
			int charCounter = 0;
			string pairOfChars = "";
			foreach (Transform text in child.transform)
			{
				pairOfChars += pairChips[counter].Substring(charCounter, 1);
				text.GetComponent<Text>().text = pairChips[counter].Substring(charCounter, 1);
				charCounter++;
			}
			if (pairOfChars == winningWord.Substring(0, 2) && !firstChipFound)
			{
				firstCorrectChip = child;
				firstChipFound = true;
			}
			//Si aún quedan pares de letra por encontrar entre las fichas entramos
			if (auxWinningWord.Length != 0)
			{
				int loopCounter = 0;
				bool chipFound = false;
				while (loopCounter < auxWinningWord.Length && !chipFound) { //Recorremos la palabra en pares de letras
					//Si la ficha tiene las mismas letras que la palabra ganadora, guardamos esa ficha
					if (auxWinningWord.Substring(loopCounter, 2) == pairOfChars)
					{
						chipFound = true;
						listAuxCorrectChips.Add(child);
						auxWinningWord = auxWinningWord.Remove(loopCounter, 2);
					}
					loopCounter+=2;
				}
			}
			pairOfChars = "";
			counter++;
		}

		foreach (Transform child in chipZone.transform)
		{
			bool correctChipFound = false;
			int loopCounter = 0;
			while (!correctChipFound && loopCounter < listAuxCorrectChips.Count)
			{
				if (child == listAuxCorrectChips [loopCounter]) {
					correctChipFound = true;
				}
				loopCounter++;
			}
			if (!correctChipFound) {
				listNoValidChips.Add (child);
			}
		}
	}

	void PrepareQuestion(){
        if (Constantes.TYPE_OF_QUESTION == (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase) { //Preparamos preguntas del nivel si es un texto{
            if (listAuxRandomNumbers.Count == 0) {
                singletonQuestions.ResetFalseQuestions(); //Si no hay mas numeros random a los que acudir.... llamamos a la funcion del singleton que se reinicie la lista y volvemos a llamar a Find First
                FindFirstQuestionNotUsed(Constantes.TYPE_OF_QUESTION);
            }
            int randomNumberQuestion = Random.Range(0, listAuxRandomNumbers.Count - 1);
            selectedQuestionPosition = listAuxRandomNumbers[randomNumberQuestion];
            winningWord = PreguntasSingleton.itemDB.listCompletaFrase[selectedQuestionPosition].answer;
            questionText.text = PreguntasSingleton.itemDB.listCompletaFrase[selectedQuestionPosition].question;

		}else if(Constantes.TYPE_OF_QUESTION == (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne) { //Preparamos preguntas del nivel si es una imagen
			if (listAuxRandomNumbers.Count == 0) {
				singletonQuestions.ResetFalseQuestions(); //Si no hay mas numeros random a los que acudir.... llamamos a la funcion del singleton que se reinicie la lista y volvemos a llamar a Find First
				FindFirstQuestionNotUsed(Constantes.TYPE_OF_QUESTION);
			}
			int randomNumberImage = Random.Range(0, listAuxRandomNumbers.Count - 1);
            selectedQuestionPosition = listAuxRandomNumbers[randomNumberImage];
			winningWord = PreguntasSingleton.itemDB.listQueNosUne[selectedQuestionPosition].answer;
			questionImage.sprite = Resources.Load<Sprite>(PreguntasSingleton.itemDB.listQueNosUne[selectedQuestionPosition].question);
		}else{
			if (listAuxRandomNumbers.Count == 0) {
				singletonQuestions.ResetFalseQuestions(); //Si no hay mas numeros random a los que acudir.... llamamos a la funcion del singleton que se reinicie la lista y volvemos a llamar a Find First
				FindFirstQuestionNotUsed(Constantes.TYPE_OF_QUESTION);
			}
			int randomNumberImage = Random.Range(0, listAuxRandomNumbers.Count - 1);
			selectedQuestionPosition = listAuxRandomNumbers[randomNumberImage];
			winningWord = PreguntasSingleton.itemDB.listJeroglificos[selectedQuestionPosition].answer;
			questionImage.sprite = Resources.Load<Sprite>(PreguntasSingleton.itemDB.listJeroglificos[selectedQuestionPosition].question);
		}

		GenerateCorrectChips(); //Generamos los pares de letra correctos. 
		GenerateIncorrectChips(); //Generamos los pares de letras aleatorios e incorrectos para confundir al jugador.
		WritePairsIntoChips();
	}

	void GenerateIncorrectChips() {

        while (pairChips.Count < 7){ //Va a generar fichas random solo cuando haya espacio en las 7

            string selectedWord = listAuxWords[Random.Range(0, listAuxWords.Count)]; //Cogemos una palabra al azar de las posibles.
            if (selectedWord.Length % 2 != 0)
                selectedWord = selectedWord + " ";
            int numberRandomChar = Random.Range(0, (selectedWord.Length / 2)) * 2;
            string random_pair = selectedWord.Substring(numberRandomChar, 2);
            pairChips.Add(random_pair);
        }
    }

	void GenerateCorrectChips(){
		pairChips.Clear();
		int counter = 0;
		string duoChars = "";

		if (winningWord.Length % 2 != 0) //Comprobamos si una palabra es impar o no.
			winningWord = winningWord + " "; //Le añadimos un espacio, así tiene un caracter más.

		do{
			duoChars = winningWord.Substring(counter, 2);
			counter += 2;
			pairChips.Add(duoChars);
		} while (!duoChars.Equals("") && counter < winningWord.Length);
	}


	override public void CheckDragDrop(Vector3 recolocationPositionValue, LastChipObject lastChipSetObject){
        if (timeWithoutSet > Constantes.MAX_TIME_WITHOUTSET)
            OnHideTimeHelp();
		
		isFirtsChipSet = true;
		timeWithoutSet = 0;
		userWord = "";
		int countChip = 0;
		bool firstChip = false;

        foreach (Transform child in chipsSet.transform){  //Comprueba si la palabra formada por las fichas es la ganadora
			countChip++;
			if (child.name == "DropZone" && child.gameObject.activeInHierarchy) { //Es la primera ficha que se coloca
				dropHereTableroJuego.enabled = true;
				child.gameObject.SetActive(false);
				firstChip = true;
			}
			if (child.tag == "Ficha"){
                if (firstChip)
                    child.localPosition = new Vector3(0, 0);
				foreach (Transform text in child.transform){
					string charWord = text.GetComponent<Text>().text;
					userWord = userWord + charWord;
				}
			}
		}

		if (!firstChip)
			stackChipSet.Push(lastChipSetObject); //Si es primera ficha no la añadimos al stack. 

		CheckWordOnBoard(lastChipSetObject.lastChipSet, recolocationPositionValue, firstChip);
	}


	void CheckWordOnBoard(Transform lastChipSet, Vector3 recolocationPositionValue, bool firstChip){
        currentWord.text = userWord;
		if (userWord.ToUpper() == winningWord.ToUpper()) //Es palabra ganadora.
			WinningWordFunction(lastChipSet, recolocationPositionValue);

        else { //No es la palabra ganadora, sigue poniendo fichas. Generamos nuevos drops.
            if (lastChipSet.name.IndexOf("Horizontal", System.StringComparison.Ordinal) != -1) { //Si encuentra la cadena "Horizontal" (valor distinto de -1)
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
				else{
					chipsSet.transform.localPosition += recolocationPositionValue;

					if (lastChipSet.GetSiblingIndex() == 0) //Si la última ficha puesta está en la primera posición entonces hay que  cambiar la posición de las dropZone "Arriba" e "Izquierda"
						MoveUpperDropzonesWithVerticalChip(lastChipSet, dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
					else //En caso contrario se moverán las dropZone inferiores
						MoveLowerDropzonesWithVerticalChip(lastChipSet, dropZoneDic["Abajo"], dropZoneDic["Derecha"]);
				}
			}
		}
	}

    void SetQuestionToUsed(){
		if (Constantes.TYPE_OF_QUESTION == (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase) //Preparamos preguntas del nivel 
			PreguntasSingleton.itemDB.listCompletaFrase[selectedQuestionPosition].used = true;
        else if (Constantes.TYPE_OF_QUESTION == (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne) //Preparamos preguntas del nivel 
            PreguntasSingleton.itemDB.listQueNosUne[selectedQuestionPosition].used = true;
		else //Preparamos preguntas del nivel 
            PreguntasSingleton.itemDB.listJeroglificos[selectedQuestionPosition].used = true;
		
        listAuxRandomNumbers.Remove(selectedQuestionPosition);
    }

	void WinningWordFunction(Transform lastChipSet, Vector3 recolocationPositionValue) {
		boardGame.GetComponent<Button>().enabled = false; //Quitamos el "Undo" del botón
		cleanBoardButton.GetComponent<Button>().enabled = false; //No puede limpiar mientras gana
		SetQuestionToUsed();
		PlaySounds((int)Clips.CorrectAnswer); //Contador para mantener las palabras por 'x' segundos en el tablero.
		correctAnswers++;
		correctText.text = "" + correctAnswers;
		correctText.GetComponent<Animator>().SetTrigger("jump");
		startTimerCorrectAnswer = true;
		if (lastChipSet.name.IndexOf("Horizontal", System.StringComparison.Ordinal) != -1) //Si encuentra la cadena "Horizontal" (valor distinto de -1)
			chipsSet.transform.localPosition += recolocationPositionValue;
		else
			chipsSet.transform.localPosition += recolocationPositionValue;
	}

	override public void SkipQuestion() {
		if (PlayerPrefs.GetInt("Coins") < skipQuestionPrice){
			statesController.ShowShop ();
		} else {
			PlaySounds((int)Clips.NextQuestion);
			PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - skipQuestionPrice);
			skipQuestionPrice = (skipQuestionPrice * 3) / 2;
			skipQuestionPriceText.text = "" + skipQuestionPrice;
			ShowCoins();
            listAuxRandomNumbers.Remove(selectedQuestionPosition);
			if (powerupFirstChipSet) {
				powerupFirstChip.GetComponent<Button>().interactable = true;
			}
			NewQuestion();
		}
	}

	override public void PowerUpSetFirstChip() {
        if (!powerupFirstChipSet) //Si ya ha utilizado el power up no se le permite volver a lanzarlo (no tendría sentido)
            if (PlayerPrefs.GetInt("Coins") < firstChipPrice) {
				statesController.ShowShop ();
			} else {
                CleanAllBoard();
                firstCorrectChip.GetComponent<Image>().color = Constantes.COLOR_GREEN;
                firstCorrectChip.GetComponent<DragFicha>().parentToReturnTo = chipsSet.transform;
                firstCorrectChip.GetComponent<DragFicha>().SetChip(initialDropZone.transform.GetComponent<RectTransform>());
                setFirstPriceText.text = "" + firstChipPrice;
                powerupFirstChipSet = true;
                powerupFirstChip.GetComponent<Button>().interactable = false;
                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - firstChipPrice);
                firstChipPrice = firstChipPrice * 3 / 2;
                ShowCoins();
            }

	}

	override public void PowerUpDeleteChip() {
        if (counterDeleteChips < listNoValidChips.Count) {
            if (PlayerPrefs.GetInt("Coins") < deleteChipPrice) {
                statesController.ShowShop();
            } else {
                PlaySounds((int)Clips.Bomb);
                CleanAllBoard();
                listNoValidChips[counterDeleteChips].gameObject.SetActive(false);
                counterDeleteChips++;
                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - deleteChipPrice);
                deleteChipPrice = deleteChipPrice * 3 / 2;
                deleteChipPriceText.text = "" + deleteChipPrice;
                ShowCoins();
                if (counterDeleteChips >= listNoValidChips.Count)
                    powerupDeleteChip.GetComponent<Button>().interactable = false;
            }
        } 
    }

	override public void NewQuestion() {
		counterDeleteChips = 0;
		powerupFirstChipSet = false;
        powerupDeleteChip.GetComponent<Button>().interactable = true;
        powerupFirstChip.GetComponent<Button>().interactable = true;
        firstCorrectChip.GetComponent<Image>().color = Constantes.COLOR_WHITE_CHIP;
		RecoverDeletedChips();
		CleanAllBoard();
		PrepareQuestion();
	}

	void RecoverDeletedChips() {
		foreach (Transform chips in listNoValidChips) {
			chips.gameObject.SetActive(true);
		}
		listNoValidChips.Clear();
	}

	override public void CleanBoardButton(){
		if (chipsSet.transform.childCount > 6){
			PlaySounds((int)Clips.Clean);
		}else if (chipsSet.transform.childCount > 5 && !powerupFirstChipSet){
			PlaySounds((int)Clips.Clean);
		}
		CleanAllBoard();
	}

	override public void CleanAllBoard() {
        stackChipSet.Clear();
		dropHereTableroJuego.enabled = false;
		isFirtsChipSet = false;
		currentWord.text = "";
		timeWithoutSet = 0;
        List<DragFicha> cleanListChips = new List<DragFicha>(); //Usamos esta lista para no quitar hijos mientras los busca. Primero buscamos y luego quitamos.
		foreach (Transform child in chipsSet.transform) {
			DragFicha componente = child.GetComponent<DragFicha>();
			if (child.tag == "MiniDrop")
				child.gameObject.SetActive(false);
            if (componente != null) {
                cleanListChips.Add(componente);
            }
		}

        foreach (DragFicha chip in cleanListChips) {
            chip.enabled = true;
            chip.BackToInitialPosition();
        }

		//Recuperar zona de drop grande y recolocar en el centro.
		initialDropZone.SetActive(true);
		chipsSet.transform.localPosition = Vector3.zero;
		if (powerupFirstChipSet){
			firstCorrectChip.GetComponent<DragFicha>().shouldSound = false;
			firstCorrectChip.GetComponent<DragFicha>().parentToReturnTo = chipsSet.transform;
			firstCorrectChip.GetComponent<DragFicha>().SetChip(initialDropZone.transform.GetComponent<RectTransform>());
		}
	}

	void GenerateHorizontalDrops(Transform chip, Transform rightDropChip, Transform downDropChip, Transform upDropChip, Transform leftDropChip){
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

	void GenerateVerticalDrops(Transform chip, Transform rightDropChip, Transform downDropChip, Transform upDropChip, Transform leftDropChip){
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

	void MoveUpperDropzonesWithHorizontalChip(Transform lastChipSet, Transform upDropChip, Transform leftDropChip){
		Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
		//Arriba
		upDropChip.localPosition = new Vector3(lastChipSet.localPosition.x - (rectLastChip.width / 4), lastChipSet.localPosition.y + (rectLastChip.height));
		//Izquierda
		leftDropChip.localPosition = new Vector3(lastChipSet.localPosition.x - ((rectLastChip.width * 3) / 4), lastChipSet.localPosition.y);
	}

	void MoveLowerDropzonesWithHorizontalChip(Transform lastChipSet, Transform downDropChip, Transform rightDropChip){
		Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
		//Arriba
		downDropChip.localPosition = new Vector3(lastChipSet.localPosition.x + (rectLastChip.width / 4), lastChipSet.localPosition.y - (rectLastChip.height));
		//Izquierda
		rightDropChip.localPosition = new Vector3(lastChipSet.localPosition.x + ((rectLastChip.width * 3) / 4), lastChipSet.localPosition.y);
	}

	void MoveUpperDropzonesWithVerticalChip(Transform lastChipSet, Transform upDropChip, Transform leftDropChip){
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
		if (startedGame){
			if (isFirtsChipSet) {
                if (timeWithoutSet > Constantes.MAX_TIME_WITHOUTSET)
					OnTimeHelp();
				else
					timeWithoutSet += 1 * Time.deltaTime;
			}
		}
		if (startTimerCorrectAnswer){
            if (correctAnswerCounter > 0.75f){
                correctAnswerCounter = 0;
                startTimerCorrectAnswer = false;
                currentWord.text = "";
                imageFilter.fillAmount = 0;
				boardGame.GetComponent<Button>().enabled = true;
				cleanBoardButton.GetComponent<Button>().enabled = true;
                NewQuestion();
            }else{
                correctAnswerCounter += 1 * Time.deltaTime;
                imageFilter.fillAmount = correctAnswerCounter / 0.75f;
            }
		}
	}

	override public void ShowCoins(){
		coinsText.text = "" + PlayerPrefs.GetInt("Coins");
	}

	override public void ResetGame(){
		correctAnswers = 0;
		correctText.text = "" + correctAnswers;
		questionText.text = "";
		NewQuestion();
        //Resetear el precio de los power-ups
        skipQuestionPrice = Constantes.SKIPQUESTION_PRICE;
        firstChipPrice = Constantes.FIRSTCHIP_PRICE;
        deleteChipPrice = Constantes.DELETECHIP_PRICE;
		SetPrices();
	}

	override public void StartAnimation(){
		questionImage.GetComponent<Animator>().SetTrigger("Agrandar");
	}

	void PlaySounds(int clip){
		this.transform.GetComponent<AudioSource>().clip = clipsFX[clip];
		this.transform.GetComponent<AudioSource>().Play();
	}

	public void Undo() {
		if (stackChipSet.Count == 0) {
			CleanAllBoard();
			return;
		}
		Vector3 recolocationPositionValue; //Este vector 3 sera el nuevo valor de fichas colocadas así podra reajustarse cuando se quite una ficha.
										   //Necesitamos saber cual es la ultima ficha colocada.
		Transform lastChipSet = stackChipSet.Peek().lastChipSet;
		Transform lastMiniDropUsed = stackChipSet.Peek().miniDropSelected;
		Vector3 lastMiniDropUsedPos = stackChipSet.Peek().miniDropSelectedPos;
		Transform lastMiniDropPartner = stackChipSet.Peek().miniDropPartner;
		Vector3 lastMiniDropPartnerPos = stackChipSet.Pop().miniDropPartnerPos;
		int positionOnBoard = lastChipSet.GetSiblingIndex(); //O es 0 o es la ultima posicion de los hijos de FichasColocadas.
        //RECOLOCAMOS LOS MINIDROPS.
		lastMiniDropUsed.localPosition = lastMiniDropUsedPos;
		lastMiniDropPartner.localPosition = lastMiniDropPartnerPos;
		//Actualizamos el text field
		if (positionOnBoard == 0) {
			userWord = userWord.Substring(2, userWord.Length - 2);
			currentWord.text = userWord;
		}
		else {
			userWord = userWord.Substring(0, userWord.Length - 2);
			currentWord.text = userWord;
		}
		//REAJUSTAMOS EL TABLERO
		switch (lastMiniDropUsed.name) {
			case "Arriba":
				//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
				if (lastChipSet.name.IndexOf("Horizontal", System.StringComparison.Ordinal) != -1)
					recolocationPositionValue = new Vector3(lastChipSet.GetComponent<RectTransform>().rect.width / 4, -lastChipSet.GetComponent<RectTransform>().rect.height / 2);
				else
					recolocationPositionValue = new Vector3(0, -lastChipSet.GetComponent<RectTransform>().rect.height / 2);
				break;
			case "Izquierda":
				//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
				if (lastChipSet.name.IndexOf("Horizontal", System.StringComparison.Ordinal) != -1)
					recolocationPositionValue = new Vector3(lastChipSet.GetComponent<RectTransform>().rect.width / 2, 0);
				else
					recolocationPositionValue = new Vector3(lastChipSet.GetComponent<RectTransform>().rect.width / 2, -lastChipSet.GetComponent<RectTransform>().rect.height / 4);
				break;
			case "Abajo":
				//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
				if (lastChipSet.name.IndexOf("Horizontal", System.StringComparison.Ordinal) != -1)
					recolocationPositionValue = new Vector3(-lastChipSet.GetComponent<RectTransform>().rect.width / 4, lastChipSet.GetComponent<RectTransform>().rect.height / 2);
				else
					recolocationPositionValue = new Vector3(0, lastChipSet.GetComponent<RectTransform>().rect.height / 2);
				break;
			case "Derecha":
				//Comprobamos en qué drop se ha dejado y la orientación de la ficha para poder colocar el tablero centrado
				if (lastChipSet.name.IndexOf("Horizontal", System.StringComparison.Ordinal) != -1)
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