using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControladorjuegoRelax : ControladorMaestro
{

	Dictionary<string, Transform> dropZoneDic = new Dictionary<string, Transform>();
	public AudioClip[] clipsFX = new AudioClip[4]; //Audios para los efectos.
	List<Transform> chipList = new List<Transform>(); //Lista de fichas.
	GameObject powerupFirstChip; //Objeto de powerup 
	ControladorEstados controladorEstado;
	GameObject chipsSet; //Objeto que tiene como childs las fichas colocadas.
	GameObject chipZone; //Zona donde se coloca las fichas.
	GameObject initialDropZone; //Zona de drop inicial
	DropHere dropHereTableroJuego;
	//Textos
	Text correctText;
	Text coinsText;
	Text nextQuestionPriceText;
	Text setFirstPriceText;
	Text deleteChipPriceText;
	public Text currentWord;
	public Text questionText;
	public Image questionImage; //Imagen de pregunta
	TextAsset txt; //Objeto para leer txt
	TextAsset aux; //Objeto para leer txt
	int playerCoins; //Monedas actuales del jugador.
	public string userWord;
	string winningWord;
	List<question_answer> list_question_answer = new List<question_answer>(); //Lista de estructura para saber preguntas y respuestas del juego (texto)
	List<image_answer> list_image_answer = new List<image_answer>(); //Lista de estructura para saber preguntas y respuestas del juego (imagenes)
	List<string> listAuxWords = new List<string>();
	List<string> pairChips = new List<string>();
	int correctAnswers;
	bool startedGame = false; //Booleano que servira para manejar el contador de tiempo.
	bool startTimerCorrectAnswer = false;
	int oldCorrectAnswers;
	//Precios iniciales para los powerups.
	int newQuestionPrice = 4;
	int firstChipPrice = 2;
	int deleteChipPrice = 4;
	int tenAnswersBlock = 0; //Bloque de juego (de 10 en 10).
	float correctAnswerCounter = 0; //Tiempo que contamos para dejar las fichas colocadas en el tablero.
	Transform firstCorrectChip; //Guardamos el objeto de la primera ficha correcta.
	List<Transform> listNoValidChips = new List<Transform>(); //Lista de objetos (fichas) no validas.
	int counterDeleteChips = 0; //Contador que servirá para saber cuantas fichas se han eliminado.
	bool isFirtsChipSet = false; //Hay una primera ficha colocada en el tablero para saber si debemos mostrar las ayudas.
	bool powerupFirstChipSet = false; //Este bool determina si se ha utilizado el pwp de colocar primera ficha.
	float timeWithoutSet = 0; //Tiempo que contara si el usuario no ha puesto ficha
	static float maxTimeWithoutSet = 8; //Maximo de tiempo para que el jugador pueda poner una ficha
										//Eventos y delegados.
	public delegate void delegateHelp(); //Declaramos un delegado
	public static event delegateHelp OnTimeHelp; //Declaramos el evento
	public static event delegateHelp OnHideTimeHelp;
	public static event delegateHelp OnMiniDropMoves;
	public enum Clips { CorrectAnswer, Clean, Bomb, NextQuestion } //ENUMERADOS DE POWERUPS PARA LOS SONIDOS.

	//ESTRUCTURAS
	public struct question_answer
	{
		public string question;
		public string answer;
	}
	public struct image_answer
	{
		public Sprite image;
		public string answer;
	}


    override public void MoreTime(){
        
    }

    override public void ResumeGame(int hola){
        
    }

	void Start()
	{
		if (Constantes.tipoDePregunta == (int)Constantes.TipoDePregunta.CompletaLaFrase)
		{
			txt = (TextAsset)Resources.Load("termina_la_frase", typeof(TextAsset));
			oldCorrectAnswers = PlayerPrefs.GetInt(Constantes.recordCompletaLaFrase);
		}
		else if (Constantes.tipoDePregunta == (int)Constantes.TipoDePregunta.Jeroglifico)
		{
			txt = (TextAsset)Resources.Load("jeroglificos", typeof(TextAsset));
			oldCorrectAnswers = PlayerPrefs.GetInt(Constantes.recordJeroglifico);
		}
		else
		{
			txt = (TextAsset)Resources.Load("que_nos_une", typeof(TextAsset));
			oldCorrectAnswers = PlayerPrefs.GetInt(Constantes.recordQueNosUne);
		}

		FindObjects();
		SearchMiniDrops();
		aux = (TextAsset)Resources.Load("jeroglificos", typeof(TextAsset));
		ReadTxts();
		readAuxTxts();
		SetPrices();
		correctText.text = "" + correctAnswers;
		ShowCoins();
	}

	void FindObjects()
	{
		powerupFirstChip = GameObject.Find("PonerPrimera");
		chipsSet = GameObject.Find("FichasColocadas");
		print(chipsSet);
		chipZone = GameObject.Find("ZonaFichas");
		controladorEstado = GameObject.Find("ControladorEstados").GetComponent<ControladorEstados>();
		initialDropZone = GameObject.Find("DropZone");
		dropHereTableroJuego = GameObject.Find("TableroJuego").GetComponent<DropHere>();
		correctText = GameObject.Find("textoAciertos").GetComponent<Text>();
		coinsText = GameObject.Find("TextoMonedas").GetComponent<Text>();
		nextQuestionPriceText = GameObject.Find("PrecioPasarPregunta").GetComponent<Text>();
		setFirstPriceText = GameObject.Find("PrecioPonerPrimera").GetComponent<Text>();
		deleteChipPriceText = GameObject.Find("PrecioEliminarFicha").GetComponent<Text>();
	}

	override public string DeleteSpecialChars(string imageName)
	{
		string specialChars = "áàäéèëíìïóòöúùüñÁÀÄÉÈËÍÌÏÓÒÖÚÙÜÑçÇ";
		string charsASCII = "aaaeeeiiiooouuunAAAEEEIIIOOOUUUNcC";
		for (int i = 0; i < specialChars.Length; i++)
		{
			imageName = imageName.Replace(specialChars[i], charsASCII[i]);
		}
		return imageName;
	}

	void SetPrices()
	{
		nextQuestionPriceText.text = "" + newQuestionPrice;
		setFirstPriceText.text = "" + firstChipPrice;
		deleteChipPriceText.text = "" + deleteChipPrice;
	}

	override public void GameStarts() {
		if (Constantes.tipoDePregunta == (int)Constantes.TipoDePregunta.CompletaLaFrase)
		{
			oldCorrectAnswers = PlayerPrefs.GetInt(Constantes.recordCompletaLaFrase);
		}
		else if (Constantes.tipoDePregunta == (int)Constantes.TipoDePregunta.Jeroglifico)
		{
			oldCorrectAnswers = PlayerPrefs.GetInt(Constantes.recordJeroglifico);
		}
		else
		{
			oldCorrectAnswers = PlayerPrefs.GetInt(Constantes.recordQueNosUne);
		}
		startedGame = true;
		PrepareQuestion(); //Preparamos preguntas.
		GenerateCorrectChips(); //Generamos los pares de letra correctos. 
		GenerateIncorrectChips(); //Generamos los pares de letras aleatorios e incorrectos para confundir al jugador.
		WritePairsIntoChips();
	}

	void SearchMiniDrops() {
        foreach (Transform child in chipsSet.transform){
			if (child.tag == "MiniDrop")
				dropZoneDic.Add(child.name, child);
		}
	}

	void ReadTxts() {
		string fullTxt = txt.text;
		switch (Constantes.tipoDePregunta) {
			case (int)Constantes.TipoDePregunta.CompletaLaFrase:
				questionText.gameObject.SetActive(true);
				questionImage.gameObject.SetActive(false);
				ReadCompleteLines(fullTxt);
				break;
			case (int)Constantes.TipoDePregunta.Jeroglifico:
				questionText.gameObject.SetActive(false);
				questionImage.gameObject.SetActive(true);
				ReadImages(fullTxt);
				break;
			case (int)Constantes.TipoDePregunta.QueNosUne:
				questionText.gameObject.SetActive(false);
				questionImage.gameObject.SetActive(true);
				ReadImages(fullTxt);
				break;
			default:
				break;
		}
	}

	void ReadCompleteLines(string fullTxt) {
		string[] txtLines = fullTxt.Split('\n');
		for (int i = 0; i < txtLines.Length; i += 2) {
			question_answer p_r = new question_answer();
			p_r.answer = txtLines[i].Trim();
			p_r.question = txtLines[i + 1].Trim();
			list_question_answer.Add(p_r);
		}
	}

	void ReadImages(string fullTxt) {
		string[] txtLines = fullTxt.Split('\n');
		for (int i = 0; i < txtLines.Length; i++)
		{
			image_answer i_r = new image_answer();
			i_r.answer = txtLines[i].Trim();
			i_r.image = Resources.Load<Sprite>(DeleteSpecialChars(txtLines[i].Trim()));
			list_image_answer.Add(i_r);
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

	override public void ReadjustBoard(int myPosition, string orientationName)
	{
		if (orientationName.IndexOf("Horizontal") != -1)
		{ //La ficha recibida es horizontal
			int counter = 0;
			foreach (Transform recolocationChip in chipsSet.transform)
			{

				if (counter >= myPosition)
				{
					//Comprobamos que ficha estamos moviendo. En caso de ser horizontal...
					if (recolocationChip.transform.name.IndexOf("Horizontal") != -1)
					{
						recolocationChip.transform.localPosition = recolocationChip.transform.localPosition + new Vector3(-recolocationChip.GetComponent<RectTransform>().rect.width, recolocationChip.GetComponent<RectTransform>().rect.height);
					}
					else
					{
						recolocationChip.transform.localPosition = recolocationChip.transform.localPosition + new Vector3(-recolocationChip.GetComponent<RectTransform>().rect.height, recolocationChip.GetComponent<RectTransform>().rect.width);
					}
				}
				counter++;
			}
		}
		else
		{
			int counter = 0;
			foreach (Transform recolocationChip in chipsSet.transform)
			{
				if (counter >= myPosition)
				{
					//Comprobamos que ficha estamos moviendo. En caso de ser horizontal...
					if (recolocationChip.transform.name.IndexOf("Horizontal") != -1)
					{
						recolocationChip.transform.localPosition = recolocationChip.transform.localPosition + new Vector3(-recolocationChip.GetComponent<RectTransform>().rect.height, recolocationChip.GetComponent<RectTransform>().rect.width);
					}
					else
					{
						recolocationChip.transform.localPosition = recolocationChip.transform.localPosition + new Vector3(-recolocationChip.GetComponent<RectTransform>().rect.width, recolocationChip.GetComponent<RectTransform>().rect.height);
					}
				}
				counter++;
			}
		}
	}

	void WritePairsIntoChips()
	{
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
				//Recorremos la palabra en pares de letras
				for (int i = 0; i < auxWinningWord.Length; i += 2)
				{
					//Si la ficha tiene las mismas letras que la palabra ganadora, guardamos esa ficha
					if (auxWinningWord.Substring(i, 2) == pairOfChars)
					{
						listAuxCorrectChips.Add(child);
						auxWinningWord.Remove(i, 2);
					}
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
				if (child == listAuxCorrectChips[loopCounter])
					correctChipFound = true;
				loopCounter++;
			}
			if (!correctChipFound)
				listNoValidChips.Add(child);
		}
	}

	void PrepareQuestion()
	{
		if (Constantes.tipoDePregunta == (int)Constantes.TipoDePregunta.CompletaLaFrase) //Preparamos preguntas del nivel si es un texto
		{
			int randomNumberQuestion = Random.Range(0, list_question_answer.Count);
			winningWord = list_question_answer[randomNumberQuestion].answer;
			questionText.text = list_question_answer[randomNumberQuestion].question;
		}
		else //Preparamos preguntas del nivel si es una imagen
		{
			int randomNumberImage = Random.Range(0, list_image_answer.Count);
			winningWord = list_image_answer[randomNumberImage].answer;
			questionImage.sprite = list_image_answer[randomNumberImage].image;
		}
	}

	void GenerateIncorrectChips()
	{

		while (pairChips.Count < 7) //Va a generar fichas random solo cuando haya espacio en las 7
		{
			string selectedWord = listAuxWords[Random.Range(0, listAuxWords.Count)]; //Cogemos una palabra al azar de las posibles.
			if (selectedWord.Length % 2 != 0)
				selectedWord = selectedWord + " ";
			int numberRandomChar = Random.Range(0, (selectedWord.Length / 2)) * 2;
			string random_pair = selectedWord.Substring(numberRandomChar, 2);
			pairChips.Add(random_pair);
		}
	}

	void GenerateCorrectChips()
	{
		pairChips.Clear();
		int counter = 0;
		string duoChars = "";

		if (winningWord.Length % 2 != 0) //Comprobamos si una palabra es impar o no.
			winningWord = winningWord + " "; //Le añadimos un espacio, así tiene un caracter más.

		do
		{
			duoChars = winningWord.Substring(counter, 2);
			counter += 2;
			pairChips.Add(duoChars);
		} while (!duoChars.Equals("") && counter < winningWord.Length);
	}


	override public void CheckDragDrop(Transform lastChipSet, Vector3 recolocationPositionValue)
	{

		if (timeWithoutSet > 7.9)
		{ //Si el jugador coloca, ocultamos la ayuda.
			OnHideTimeHelp();
		}
		isFirtsChipSet = true;
		timeWithoutSet = 0;
		userWord = "";
		chipList.Clear();
		int countChip = 0;
		bool firstChip = false;

		foreach (Transform child in chipsSet.transform)
		{ //Comprueba si la palabra formada por las fichas es la ganadora.
			countChip++;
			if (child.name == "DropZone" && child.gameObject.activeInHierarchy) //Es la primera ficha que se coloca
			{
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

		CheckWordOnBoard(lastChipSet, recolocationPositionValue, firstChip);
	}


	void CheckWordOnBoard(Transform lastChipSet, Vector3 recolocationPositionValue, bool firstChip)
	{
        currentWord.text = userWord;
		if (userWord.ToUpper() == winningWord.ToUpper()) //Es palabra ganadora.
			WinningWordFunction(lastChipSet, recolocationPositionValue);

		else //No es la palabra ganadora, sigue poniendo fichas. Generamos nuevos drops.
		{
			if (lastChipSet.name.IndexOf("Horizontal") != -1) //Si encuentra la cadena "Horizontal" (valor distinto de -1)
			{
				if (firstChip)
					GenerateHorizontalDrops(lastChipSet, dropZoneDic["Derecha"], dropZoneDic["Abajo"], dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
				else //Si no es primera ficha.....
				{
					chipsSet.transform.localPosition += recolocationPositionValue;
					if (lastChipSet.GetSiblingIndex() == 0) //Si la última ficha puesta está en la primera posición entonces hay que  cambiar la posición de las dropZone "Arriba" e "Izquierda"
						MoveUpperDropzonesWithHorizontalChip(lastChipSet, dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);

					else //En caso contrario se moverán las dropZone inferiores
						MoveLowerDropzonesWithHorizontalChip(lastChipSet, dropZoneDic["Abajo"], dropZoneDic["Derecha"]);
				}
			}
			else
			{
				if (firstChip)
					GenerateVerticalDrops(lastChipSet, dropZoneDic["Derecha"], dropZoneDic["Abajo"], dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
				else
				{
					chipsSet.transform.localPosition += recolocationPositionValue;

					if (lastChipSet.GetSiblingIndex() == 0) //Si la última ficha puesta está en la primera posición entonces hay que  cambiar la posición de las dropZone "Arriba" e "Izquierda"
						MoveUpperDropzonesWithVerticalChip(lastChipSet, dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
					else //En caso contrario se moverán las dropZone inferiores
						MoveLowerDropzonesWithVerticalChip(lastChipSet, dropZoneDic["Abajo"], dropZoneDic["Derecha"]);
				}
			}

			OnMiniDropMoves(); //Evento donde deben calcular la normal de nuevo.
		}
	}

	void WinningWordFunction(Transform lastChipSet, Vector3 recolocationPositionValue)
	{
		PlaySounds((int)Clips.CorrectAnswer); //Contador para mantener las palabras por 'x' segundos en el tablero.
		currentWord.text = "";
		correctAnswers++;
		correctText.text = "" + correctAnswers;
		correctText.GetComponent<Animator>().SetTrigger("jump");
		startTimerCorrectAnswer = true;
		if (lastChipSet.name.IndexOf("Horizontal") != -1) //Si encuentra la cadena "Horizontal" (valor distinto de -1)
			chipsSet.transform.localPosition += recolocationPositionValue;
		else
			chipsSet.transform.localPosition += recolocationPositionValue;
	}

	override public void SkipQuestion() {
		if (PlayerPrefs.GetInt("Coins") < newQuestionPrice){
			//NoCoins
		} else {
			PlaySounds((int)Clips.NextQuestion);
			PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - newQuestionPrice);
			newQuestionPrice = (newQuestionPrice * 3) / 2;
			nextQuestionPriceText.text = "" + newQuestionPrice;
			ShowCoins();
			NewQuestion();
		}
	}

	override public void PowerUpSetFirstChip() {
		if (!powerupFirstChipSet) //Si ya ha utilizado el power up no se le permite volver a lanzarlo (no tendría sentido)
			if (PlayerPrefs.GetInt("Coins") >= firstChipPrice) {
				CleanAllBoard();
				firstCorrectChip.GetComponent<Image>().color = Color.green;
				firstCorrectChip.GetComponent<DragFicha>().parentToReturnTo = chipsSet.transform;
				firstCorrectChip.GetComponent<DragFicha>().ColocarFicha(initialDropZone.transform.GetComponent<RectTransform>());
				setFirstPriceText.text = "" + firstChipPrice;
				powerupFirstChipSet = true;
				powerupFirstChip.SetActive(false);
				PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - firstChipPrice);
				firstChipPrice = firstChipPrice * 3 / 2;
				coinsText.text = "" + PlayerPrefs.GetInt("Coins");
			}
	}

	override public void PowerUpDeleteChip()
	{
		if (counterDeleteChips < listNoValidChips.Count) {
			if (PlayerPrefs.GetInt("Coins") >= deleteChipPrice) {
				PlaySounds((int)Clips.Bomb);
				CleanAllBoard();
				listNoValidChips[counterDeleteChips].gameObject.SetActive(false);
				counterDeleteChips++;
				PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - deleteChipPrice);
				deleteChipPrice = deleteChipPrice * 3 / 2;
				deleteChipPriceText.text = "" + deleteChipPrice;
				coinsText.text = "" + PlayerPrefs.GetInt("Coins");
			}
		}
	}

	override public void NewQuestion() {
		counterDeleteChips = 0;
		powerupFirstChip.SetActive(true);
		powerupFirstChipSet = false;
		firstCorrectChip.GetComponent<Image>().color = new Color32(238, 231, 231, 255);
		RecoverDeletedChips();
		CleanAllBoard();
		PrepareQuestion();
		GenerateCorrectChips(); //Generamos los pares de letra correctos. 
		GenerateIncorrectChips(); //Generamos los pares de letras aleatorios e incorrectos para confundir al jugador.
		WritePairsIntoChips(); //Colocamos el array con las letras en las fichas del juego
	}

	void RecoverDeletedChips() {
		foreach (Transform chips in listNoValidChips) {
			chips.gameObject.SetActive(true);
		}
		listNoValidChips.Clear();
	}

	override public void CleanBoardButton()
	{
		if (chipsSet.transform.childCount > 6)
		{
			PlaySounds((int)Clips.Clean);
		}
		else if (chipsSet.transform.childCount > 5 && !powerupFirstChipSet)
		{
			PlaySounds((int)Clips.Clean);
		}
		CleanAllBoard();
	}

	override public void CleanAllBoard()
	{
		dropHereTableroJuego.enabled = false;
		isFirtsChipSet = false;
		currentWord.text = "";
		timeWithoutSet = 0;
		List<DragFicha> recolocationChips = new List<DragFicha>();
		foreach (Transform child in chipsSet.transform)
		{
			DragFicha componente = child.GetComponent<DragFicha>();
			if (child.tag == "MiniDrop")
				child.gameObject.SetActive(false);
			if (componente != null)
				recolocationChips.Add(componente);
		}

		foreach (DragFicha chip in recolocationChips)
		{
			chip.enabled = true;
			chip.volverAMiPosicionInicial();
		}

		//Recuperar zona de drop grande y recolocar en el centro.
		initialDropZone.SetActive(true);
		chipsSet.transform.localPosition = Vector3.zero;
		if (powerupFirstChipSet)
		{
			firstCorrectChip.GetComponent<DragFicha>().deboSonar = false;
			firstCorrectChip.GetComponent<DragFicha>().parentToReturnTo = chipsSet.transform;
			firstCorrectChip.GetComponent<DragFicha>().ColocarFicha(initialDropZone.transform.GetComponent<RectTransform>());
		}
	}

	void GenerateHorizontalDrops(Transform chip, Transform rightDropChip, Transform downDropChip, Transform upDropChip, Transform leftDropChip)
	{
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

	void GenerateVerticalDrops(Transform chip, Transform rightDropChip, Transform downDropChip, Transform upDropChip, Transform leftDropChip)
	{
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

	void MoveUpperDropzonesWithHorizontalChip(Transform lastChipSet, Transform upDropChip, Transform leftDropChip)
	{
		Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
		//Arriba
		upDropChip.localPosition = new Vector3(lastChipSet.localPosition.x - (rectLastChip.width / 4), lastChipSet.localPosition.y + (rectLastChip.height));
		//Izquierda
		leftDropChip.localPosition = new Vector3(lastChipSet.localPosition.x - ((rectLastChip.width * 3) / 4), lastChipSet.localPosition.y);
	}

	void MoveLowerDropzonesWithHorizontalChip(Transform lastChipSet, Transform downDropChip, Transform rightDropChip)
	{
		Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
		//Arriba
		downDropChip.localPosition = new Vector3(lastChipSet.localPosition.x + (rectLastChip.width / 4), lastChipSet.localPosition.y - (rectLastChip.height));
		//Izquierda
		rightDropChip.localPosition = new Vector3(lastChipSet.localPosition.x + ((rectLastChip.width * 3) / 4), lastChipSet.localPosition.y);
	}

	void MoveUpperDropzonesWithVerticalChip(Transform lastChipSet, Transform upDropChip, Transform leftDropChip)
	{
		Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
		//Arriba
		upDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x, lastChipSet.localPosition.y + ((rectLastChip.height * 3) / 4));
		//Izquierda
		leftDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x - rectLastChip.width, lastChipSet.localPosition.y + (rectLastChip.height / 4));
	}

	void MoveLowerDropzonesWithVerticalChip(Transform lastChipSet, Transform downDropChip, Transform rightDropChip)
	{
		Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
		//Derecha
		rightDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x + rectLastChip.width, lastChipSet.localPosition.y - (rectLastChip.height / 4));
		//Abajo 
		downDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x, lastChipSet.localPosition.y - ((rectLastChip.height * 3) / 4));
	}

	void Update()
	{
		if (startedGame)
		{
			if (isFirtsChipSet) {
				if (timeWithoutSet > maxTimeWithoutSet)
					OnTimeHelp();
				else
					timeWithoutSet += 1 * Time.deltaTime;
			}
		}
		if (startTimerCorrectAnswer)
		{
			if (correctAnswerCounter > 0.75f)
			{
				correctAnswerCounter = 0;
				startTimerCorrectAnswer = false;
				NewQuestion();
			}
			else
				correctAnswerCounter += 1 * Time.deltaTime;
		}
	}

	override public void ShowCoins()
	{
		coinsText.text = "" + PlayerPrefs.GetInt("Coins");
	}

	void GameIsOver()
	{
		switch (Constantes.tipoDePregunta)
		{
			case (int)Constantes.TipoDePregunta.CompletaLaFrase:
				if (oldCorrectAnswers < correctAnswers)
					PlayerPrefs.SetInt(Constantes.recordCompletaLaFrase, correctAnswers);
				break;
			case (int)Constantes.TipoDePregunta.Jeroglifico:
				if (oldCorrectAnswers < correctAnswers)
					PlayerPrefs.SetInt(Constantes.recordJeroglifico, correctAnswers);
				break;
			case (int)Constantes.TipoDePregunta.QueNosUne:
				if (oldCorrectAnswers < correctAnswers)
					PlayerPrefs.SetInt(Constantes.recordQueNosUne, correctAnswers);
				break;
		}
		controladorEstado.TerminarJugar(oldCorrectAnswers, correctAnswers);
		startedGame = false;
	}

	override public void ResetGame()
	{
		correctAnswers = 0;
		correctText.text = "" + correctAnswers;
		questionText.text = "";
		NewQuestion();
		//Resetear el precio de los power-ups
		newQuestionPrice = 4;
		firstChipPrice = 2;
		deleteChipPrice = 4;
		SetPrices();
	}

	override public void StartAnimation()
	{
		questionImage.GetComponent<Animator>().SetTrigger("Agrandar");
	}

	void PlaySounds(int clip)
	{
		this.transform.GetComponent<AudioSource>().clip = clipsFX[clip];
		this.transform.GetComponent<AudioSource>().Play();
	}
}