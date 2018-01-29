/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControladorJuego : ControladorMaestro {
    //UI
    Image imageFilter; //Filtro verde de la respuesta correcta
    //TEXTOS
    Text correctText; //Texto de aciertos
    Text coinsText; //Texto de monedas
    Text timeText; //Texto de tiempo restante
    Text nextQuestionPriceText; //Texto de Precio de powerup Pasar Pregunta
    Text setFirstPriceText; //Texto de Precio de powerup Poner Primera
	Text deleteChipPriceText; //Texto de Precio de powerup Borrar Ficha
	public Text currentWord; //Texto de palabra formada.
    public Text moreTimeText; //Texto de animación para Más tiempo.
    public Text questionText; //Texto de la pregunta
    public Image questionImage; //Imagen de la pregunta
    TextAsset aux; //Objeto para leer txt
    public AudioClip[] clipsFX = new AudioClip[4]; //Audios para los efectos.
    //VARIABLES Y GAME OBJECTS.
    Dictionary<string, Transform> dropZoneDic = new Dictionary<string, Transform>(); //Diccionario de MiniDrops
    GameObject powerupFirstChip; //Objeto de powerup Poner Primera
    GameObject powerupDeleteChip; //Objeto de powerup Bomba
    GameObject powerupSkipQuestion; //Objeto de powerup pasar pregunta
    ControladorEstados controladorEstado; //Script del controlador estado. 
    GameObject chipsSet; //Objeto que tiene como childs las fichas colocadas.
    GameObject chipZone; //Zona donde se coloca las fichas.
    GameObject initialDropZone; //Zona de drop inicial
    GameObject boardGame; //Tablero de juego donde se colocan fichas y tiene el UNDO
    GameObject cleanBoardButton; //Boton de limpiar tablero
    PreguntasSingleton singletonQuestions; //Script para las preguntas del singleton. 
    DropHere dropHereTableroJuego; //Script de DropHere que tiene el tablero de juego para poder colocar fichas.
    int playerCoins; //Monedas actuales del jugador.
    float initialTime = Constantes.TIMETRIAL_TIME; //Tiempo que tendrá el jugador al iniciar la partida.
    float currentTime; //Tiempo del jugador.
    public string userWord { get; set; } //Palabra que se ha formado por las fichas del jugador. 
    string winningWord; //Palabra que es la ganadora
    List<string> listAuxWords = new List<string>(); //Lista en la que se va a guardar las palabras erroneas para coger de ahi los pares de letras
    List<string> pairChips = new List<string>(); //IMPORTANTE: lista donde se guardan los pares de letras.
    List<int> listAuxRandomNumbers = new List<int>(); //Guardamos aqui aquellos "ids" que de las preguntas que no han sido utilizadas
    int selectedQuestionPosition; //Es la posicion de la pregunta elegida de la lista de preguntas de arriba
	Stack<LastChipObject> stackChipSet = new Stack<LastChipObject>(); //Stack de fichas que han sido puestas (esto servira para quitarlas)
    int correctAnswers; //Numero de preguntas acertadas
    bool startedGame = false; //Booleano que servira para manejar el contador de tiempo.
    bool startTimerCorrectAnswer = false; //Booleano que sirve para animar el filtro verde cuando se acierta
	bool under5seconds = false;
	bool upper5seconds = true;
    //Precios iniciales para los powerups.
    int skipQuestionPrice = Constantes.SKIPQUESTION_PRICE;
    int firstChipPrice = Constantes.FIRSTCHIP_PRICE;
    int deleteChipPrice = Constantes.DELETECHIP_PRICE;
    int tenAnswersBlock = 0; //Bloque de juego (de 10 en 10).
    float correctAnswerCounter = 0; //Tiempo que contamos para dejar las fichas colocadas en el tablero.
    Transform firstCorrectChip; //Guardamos el objeto de la primera ficha correcta.
    List<Transform> listNoValidChips = new List<Transform>(); //Lista de objetos (fichas) no validas.
    int counterDeleteChips = 0; //Contador que servirá para saber cuantas fichas se han eliminado.
    bool isFirstChipSet = false; //Hay una primera ficha colocada en el tablero para saber si debemos mostrar las ayudas.
    bool powerupFirstChipSet = false; //Este bool determina si se ha utilizado el pwp de colocar primera ficha.
    float timeWithoutSet = 0; //Tiempo que contara si el usuario no ha puesto ficha
    //Eventos y delegados.
    public delegate void delegateHelp(); //Declaramos un delegado
    public static event delegateHelp OnTimeHelp; //Declaramos el evento
    public static event delegateHelp OnHideTimeHelp;
    public enum Clips { CorrectAnswer, Clean, Bomb, NextQuestion } //ENUMERADOS DE POWERUPS PARA LOS SONIDOS.
    int oldCorrectAnswers;

    void Start() {
        FindObjects(); //Buscamos los objetos que vamos a utilizar.
        SearchMiniDrops(); //Buscamos los minidrops ya que serán puntos de referencia para ver donde se coloca la ficha.
        aux = (TextAsset)Resources.Load("jeroglificos", typeof(TextAsset)); //TXT auxiliar.
        SelectTypeOfQuestion(); //Elige el tipo de pregunta y rellena un array auxiliar con aquellas posiciones no utilizadas.
        readAuxTxts(); //Lee un txt con palabras para rellenar las fichas.
        SetPrices(); //Pone precios a los powerups;
        currentTime = initialTime; //Ajusta el tiempo actual al que nos dan por defecto.
        timeText.text = "" + currentTime; //Actualizamos el texto.
        correctText.text = "" + correctAnswers; //Actualizamos pregunta actual. 
        ShowCoins();
        CheckPowerUpPrices();
        SearchRecords();
    }


    void FindObjects() { //Metodo que busca los objetos.
        singletonQuestions = GameObject.Find("ObjetoSingleton").GetComponent<PreguntasSingleton>();
        singletonQuestions.setControllerGameNumber(1);
        powerupFirstChip = GameObject.Find("PanelPrimera");
        powerupDeleteChip = GameObject.Find("PanelEliminar");
        powerupSkipQuestion = GameObject.Find("PanelPasar");
        chipsSet = GameObject.Find("FichasColocadas");
        chipZone = GameObject.Find("ZonaFichas");
        controladorEstado = GameObject.Find("ControladorEstados").GetComponent<ControladorEstados>();
        initialDropZone = GameObject.Find("DropZone");
        boardGame = GameObject.Find("TableroJuego");
        dropHereTableroJuego = boardGame.GetComponent<DropHere>();
        correctText = GameObject.Find("TextoAciertos").GetComponent<Text>();
        coinsText = GameObject.Find("TextoMonedas").GetComponent<Text>();
        timeText = GameObject.Find("TextoTiempo").GetComponent<Text>();
        nextQuestionPriceText = GameObject.Find("PrecioPasarPregunta").GetComponent<Text>();
        setFirstPriceText = GameObject.Find("PrecioPonerPrimera").GetComponent<Text>();
        deleteChipPriceText = GameObject.Find("PrecioEliminarFicha").GetComponent<Text>();
        imageFilter = GameObject.Find("Filtro").GetComponent<Image>();
        cleanBoardButton = GameObject.Find("LimpiarTableroButton");
    }

    override public Transform GetMiniDrop(string _miniDropName) { 
        //Este get se hace para obtener el transform de un MiniDrop (se ejecuta cuando hacemos el Undo
        return dropZoneDic[_miniDropName];
    }

    void SearchMiniDrops() {
        foreach (Transform child in chipsSet.transform) {
            if (child.tag == "MiniDrop")
                dropZoneDic.Add(child.name, child);
        }
    }

    override public string DeleteSpecialChars(string imageName) {
        string specialChars = "áàäéèëíìïóòöúùüñÁÀÄÉÈËÍÌÏÓÒÖÚÙÜÑçÇ";
        string charsASCII = "aaaeeeiiiooouuunAAAEEEIIIOOOUUUNcC";
        for (int i = 0; i < specialChars.Length; i++) {
            imageName = imageName.Replace(specialChars[i], charsASCII[i]);
        }
        return imageName;
    }

    void SetPrices() {
        nextQuestionPriceText.text = "" + skipQuestionPrice;
        setFirstPriceText.text = "" + firstChipPrice;
        deleteChipPriceText.text = "" + deleteChipPrice;
    }

    override public bool GetStartedGame() {
        return startedGame;
    }

    public void SetStartedGame(bool _startedGame) {
        startedGame = _startedGame;
    }

    override public void GameStarts() {
        startedGame = true;
        PrepareQuestion(); //Preparamos preguntas, generamos correctas, incorrectas y las ponemos dentro de las fichas. 
    }

    void SelectTypeOfQuestion() {
        switch (Constantes.TYPE_OF_QUESTION) {
            case (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase:
                questionText.gameObject.SetActive(true);
                questionImage.gameObject.SetActive(false);
                FindFirstQuestionNotUsed((int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase); //Buscamos la posicion de la primera pregunta sin contestar.
                break;
            case (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico:
                questionText.gameObject.SetActive(false);
                questionImage.gameObject.SetActive(true);
                FindFirstQuestionNotUsed((int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico); //Buscamos la posicion de la primera pregunta sin contestar.
                break;
            case (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne:
                questionText.gameObject.SetActive(false);
                questionImage.gameObject.SetActive(true);
                FindFirstQuestionNotUsed((int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne); //Buscamos la posicion de la primera pregunta sin contestar.
                break;
            default:
                break;
        }
    }


    void FindFirstQuestionNotUsed(int tipoPregunta) { //Recorremos la lista statica que tenemos en el singleton donde hemos bajado las preguntas
        switch (tipoPregunta) {
            case (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase:
                for (int i = 0; i < PreguntasSingleton.itemDB.listCompletaFrase.Count; i++) {
                    //Buscamos la posición de la primera pregunta que aún no se haya contestado
                    if (!PreguntasSingleton.itemDB.listCompletaFrase[i].used) {
                        for (int j = i; j < PreguntasSingleton.itemDB.listCompletaFrase.Count; j++) {
                            listAuxRandomNumbers.Add(j); //Una vez recorremos la lista entera, vamos metiendo en este array aquellas que no hayan sido contestadas.
                        }
                        return; //En cuanto encontramos la posición, salimos de la función
                    }
                }
                break;
            case (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico:
                for (int i = 0; i < PreguntasSingleton.itemDB.listJeroglificos.Count; i++) {
                    //Buscamos la posición de la primera pregunta que aún no se haya contestado
                    if (!PreguntasSingleton.itemDB.listJeroglificos[i].used) {
                        for (int j = i; j < PreguntasSingleton.itemDB.listJeroglificos.Count; j++) {
                            listAuxRandomNumbers.Add(j); //Una vez recorremos la lista entera, vamos metiendo en este array aquellas que no hayan sido contestadas.
                        }
                        return; //En cuanto encontramos la posición, salimos de la función
                    }
                }
                break;
            case (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne:
                for (int i = 0; i < PreguntasSingleton.itemDB.listQueNosUne.Count; i++) {
                    //Buscamos la posición de la primera pregunta que aún no se haya contestado
                    if (!PreguntasSingleton.itemDB.listQueNosUne[i].used) {
                        for (int j = i; j < PreguntasSingleton.itemDB.listQueNosUne.Count; j++) {
                            listAuxRandomNumbers.Add(j); //Una vez recorremos la lista entera, vamos metiendo en este array aquellas que no hayan sido contestadas.
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

    void PrepareQuestion() {
        if (Constantes.TYPE_OF_QUESTION == (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase) { //Preparamos preguntas del nivel si es un texto
            if (listAuxRandomNumbers.Count == 0) { //En caso de que todas las preguntas hayan sido contestadas:
                singletonQuestions.ResetFalseQuestions(); //llamamos a la funcion del singleton que se reinicie la lista 
                FindFirstQuestionNotUsed(Constantes.TYPE_OF_QUESTION); //Y buscamos aquella pregunta que no ha sido usada.
            }

            int randomNumberQuestion = Random.Range(0, listAuxRandomNumbers.Count - 1); //Busca un random de aquellos numeros que no han sido usados
            selectedQuestionPosition = listAuxRandomNumbers[randomNumberQuestion]; //Se guarda su "id" (posicion en la lista)
            //Pone la respuesta y la pregunta
			winningWord = PreguntasSingleton.itemDB.listCompletaFrase[selectedQuestionPosition].answer; 
            questionText.text = PreguntasSingleton.itemDB.listCompletaFrase[selectedQuestionPosition].question;
            //(Para los demas es lo mismo)
        } else if (Constantes.TYPE_OF_QUESTION == (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne) {
            if (listAuxRandomNumbers.Count == 0) {
                singletonQuestions.ResetFalseQuestions(); 
                FindFirstQuestionNotUsed(Constantes.TYPE_OF_QUESTION);
            }
            int randomNumberImage = Random.Range(0, listAuxRandomNumbers.Count - 1);
            selectedQuestionPosition = listAuxRandomNumbers[randomNumberImage];
            winningWord = PreguntasSingleton.itemDB.listQueNosUne[selectedQuestionPosition].answer;
            questionImage.sprite = Resources.Load<Sprite>(PreguntasSingleton.itemDB.listQueNosUne[selectedQuestionPosition].question);
        } else {
            if (listAuxRandomNumbers.Count == 0) {
                singletonQuestions.ResetFalseQuestions(); 
                FindFirstQuestionNotUsed(Constantes.TYPE_OF_QUESTION);
            }
            int randomNumberImage = Random.Range(0, listAuxRandomNumbers.Count - 1);
            selectedQuestionPosition = listAuxRandomNumbers[randomNumberImage];
            winningWord = PreguntasSingleton.itemDB.listJeroglificos[selectedQuestionPosition].answer;
            questionImage.sprite = Resources.Load<Sprite>(PreguntasSingleton.itemDB.listJeroglificos[selectedQuestionPosition].question);
        }

        GenerateCorrectChips(); //Generamos una lista llena de pares correctos.
        GenerateIncorrectChips(); //Generamos una lista llena de pares incorrectos.
        WritePairsIntoChips(); //Y ponemos en las fichas esos pares de letras generados.
    }

    void GenerateCorrectChips() { //Funcion que genera las fichas correctas (se ejecuta antes que la generación de incorrectas)
		pairChips.Clear(); //limpia el pairChips
		int counter = 0; //Asigna un contador a 0
		string duoChars = ""; //Y declara una variable local que sea vacía (aquí se pondrán los 2 chars.

		if (winningWord.Length % 2 != 0) //Comprobamos si una palabra es impar o no.
			winningWord = winningWord + " "; //Le añadimos un espacio, así tiene un caracter más.

		do { //Bucle que se ejecuta siempre y cuando duochars no esté vacío y se haya recorrido toda la palabra.
			duoChars = winningWord.Substring(counter, 2); //Asignamos desde el contador, el numero de chars que queremos.
			counter += 2; //Sumamos de 2 en 2
			pairChips.Add(duoChars); //Y añadimos a la lista de pairChips el string 'xx'
		} while (!duoChars.Equals("") && counter < winningWord.Length);
	}

    void GenerateIncorrectChips() {
        while (pairChips.Count < 7) { //Va a generar fichas random solo cuando haya espacio en las 7
            string selectedWord = listAuxWords[Random.Range(0, listAuxWords.Count)]; //Cogemos una palabra al azar de las posibles.
            if (selectedWord.Length % 2 != 0) //Vemos si es par o impar (si es impar añadimos un espacio"
                selectedWord = selectedWord + " ";
            int numberRandomChar = Random.Range(0, (selectedWord.Length / 2)) * 2; //Cogemos un numero al azar de 0 al tamaño de la palabra.
            string random_pair = selectedWord.Substring(numberRandomChar, 2); //Cogemos un par de letras al azar de la palabra elegida.
            pairChips.Add(random_pair); //Añadimos a pairChips aquellas erroneas.
        }
    }

	void WritePairsIntoChips() {
        //TODO: Explicar a Sebas este shuffle.
		for (int i = 1; i < pairChips.Count; i++) { //Desordenamos la lista para no tener erroneas y correctas juntas
			int randomNumber = Random.Range(0, i); 
			string pairAuxiliar = pairChips[i];
			pairChips[i] = pairChips[randomNumber];
			pairChips[randomNumber] = pairAuxiliar;
		}

        List<Transform> listAuxCorrectChips = new List<Transform>(); //Lista de fichas correctas: usamos esta lista auxiliar para poder usar el powerup de bomba
		int counter = 0;
		bool firstChipFound = false;
		string auxWinningWord = winningWord;
		foreach (Transform child in chipZone.transform) { //Recorre la zona de fichas.
			int charCounter = 0; //Contador de chars
			string pairOfChars = "";
            //Una vez ya estamos en la ficha, nos metemos dentro de sus 2 hijos (2 textos)
			foreach (Transform text in child.transform) {//En cada vuelta...
                pairOfChars += pairChips[counter].Substring(charCounter, 1); //Cogemos la letra en el char counter (0,1)
				text.GetComponent<Text>().text = pairChips[counter].Substring(charCounter, 1); //Y se la asignamos al texto
				charCounter++;
			}

			if (pairOfChars == winningWord.Substring(0, 2) && !firstChipFound) { //Si se encuentra aquella primera ficha correcta...
				firstCorrectChip = child;
				firstChipFound = true;
			}

			//Si aún quedan pares de letra por encontrar entre las fichas entramos
			if (auxWinningWord.Length != 0) {
				//Recorremos la palabra en pares de letras
				for (int i = 0; i < auxWinningWord.Length; i += 2) {
					//Si la ficha tiene las mismas letras que la palabra ganadora, guardamos esa ficha
					if (auxWinningWord.Substring(i, 2) == pairOfChars) {
						listAuxCorrectChips.Add(child);
						auxWinningWord.Remove(i, 2);
					}
				}
			}

			pairOfChars = "";
			counter++;
		}
            
		foreach (Transform child in chipZone.transform) { //Bucle que añade a listNoValidChips aquellas fichas que no sean validas
			bool correctChipFound = false;
			int loopCounter = 0;
			while (!correctChipFound && loopCounter < listAuxCorrectChips.Count) {
				if (child == listAuxCorrectChips[loopCounter])
					correctChipFound = true;
				loopCounter++;
			}
			if (!correctChipFound)
				listNoValidChips.Add(child);
		}
	}

    //CheckDragDrop es llamada desde Drag Ficha
    override public void CheckDragDrop(Vector3 recolocationPositionValue, LastChipObject lastChipSetObject) {
        if (timeWithoutSet > 7.9) { //Si el jugador coloca, ocultamos la ayuda.
            OnHideTimeHelp();
        }

        isFirstChipSet = true; //primera ficha está colocada
        timeWithoutSet = 0; //quitamos el tiempo de ayuda
        userWord = ""; //limpiamos palabra y volvemos a formarla.
        bool firstChip = false;

        //Este bucle se ejecuta siempre que se coloca una ficha.. en caso de que el hijo sea DropZone y este activo lo desactivamos.
        foreach (Transform child in chipsSet.transform) { //Recorre las fichas colocadas
            if (child.name == "DropZone" && child.gameObject.activeInHierarchy) { //Es la primera ficha que se coloca
                dropHereTableroJuego.enabled = true;
                child.gameObject.SetActive(false);
                firstChip = true;
            }

            if (child.tag == "Ficha") { //Si se coloca una ficha....
                if (firstChip) //Comprobamos que sea la primera...
                    child.localPosition = new Vector3(0, 0);
                
                foreach (Transform text in child.transform) { //Y recorremos todas aquellas fichas que estén en el tablero para formar la palabra.
                    string charWord = text.GetComponent<Text>().text;
                    userWord = userWord + charWord;
                }
            }
        }

        if(!firstChip) //Si es primera ficha no la añadimos al stack. 
			stackChipSet.Push(lastChipSetObject); 

		CheckWordOnBoard(lastChipSetObject.lastChipSet ,recolocationPositionValue, firstChip);
    }

    //Esta funcion se encarga de comprobar si es la palabra ganadora, si no lo es.... sigue generando Dropzones.
    void CheckWordOnBoard(Transform lastChipSet, Vector3 recolocationPositionValue, bool firstChip) {
		/* Recibe 3 parametros:
         * lastChipSet (que es un transform de la posicion de la ficha)
         * RecolocationPositionValue (que es calculado en DragFicha) y será lo que deba moverse el tablero
         * Y si es primera ficha, así podemos saber que minidrops deben ser creados
         */

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
                        MoveUpperDropzonesWithHorizontalChip(lastChipSet, dropZoneDic["Arriba"], dropZoneDic["Izquierda"], 1);
                    else //En caso contrario se moverán las dropZone inferiores
                        MoveLowerDropzonesWithHorizontalChip(lastChipSet, dropZoneDic["Abajo"], dropZoneDic["Derecha"], 1);
                }
            } else {
                if (firstChip)
                    GenerateVerticalDrops(lastChipSet, dropZoneDic["Derecha"], dropZoneDic["Abajo"], dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
                else {
                    chipsSet.transform.localPosition += recolocationPositionValue;

                    if (lastChipSet.GetSiblingIndex() == 0) //Si la última ficha puesta está en la primera posición entonces hay que  cambiar la posición de las dropZone "Arriba" e "Izquierda"
                        MoveUpperDropzonesWithVerticalChip(lastChipSet, dropZoneDic["Arriba"], dropZoneDic["Izquierda"], 1);
                    else //En caso contrario se moverán las dropZone inferiores
                        MoveLowerDropzonesWithVerticalChip(lastChipSet, dropZoneDic["Abajo"], dropZoneDic["Derecha"], 1);
                }
            }
        }
    }

	void WinningWordFunction(Transform lastChipSet, Vector3 recolocationPositionValue) {
        boardGame.GetComponent<Button>().enabled = false; //Quitamos el "Undo" del botón
        cleanBoardButton.GetComponent<Button>().enabled = false; //No puede limpiar mientras gana
		SetQuestionToUsed(); //Actualizamos la pregunta que acabamos de usar y la ponemos en false.
		PlaySounds((int)Clips.CorrectAnswer); //Contador para mantener las palabras por 'x' segundos en el tablero.
        moreTimeText.text = "" + Mathf.Clamp(Constantes.TIME_REWARD - (correctAnswers / 4), 3, 200) + "s"; //Animacion del texto subiendo al tiempo.
        moreTimeText.gameObject.SetActive(true); //Activamos el objeto (que por defecto activa la animacion) del tiempo subiendo
		correctText.text = "" + ++correctAnswers;
		correctText.GetComponent<Animator>().SetTrigger("jump");
		startTimerCorrectAnswer = true; //pequeño tiempo que dejamos entre pregunta y pregunt
        //Recoloca el tablero.										
		if (lastChipSet.name.IndexOf("Horizontal", System.StringComparison.Ordinal) != -1) //Si encuentra la cadena "Horizontal" (valor distinto de -1)
			chipsSet.transform.localPosition += recolocationPositionValue;
		else
			chipsSet.transform.localPosition += recolocationPositionValue;
	}


    void SetQuestionToUsed() { //Esta funcion se ejecuta tras utilizar una palabra
        //SelectedPosition es el identificador de la pregunta -> Vamos a la lista de Singleton y editamos el valor que tengamos que editar.
        if (Constantes.TYPE_OF_QUESTION == (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase)  //Actualizamos la lista del singleton en el modo de juego
            PreguntasSingleton.itemDB.listCompletaFrase[selectedQuestionPosition].used = true;
        else if (Constantes.TYPE_OF_QUESTION == (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne) //Actualizamos la lista del singleton en el modo de juego
            PreguntasSingleton.itemDB.listQueNosUne[selectedQuestionPosition].used = true;
        else
            PreguntasSingleton.itemDB.listJeroglificos[selectedQuestionPosition].used = true; //Actualizamos la lista del singleton en el modo de juego

        listAuxRandomNumbers.Remove(selectedQuestionPosition); //Quitamos la posicion de la lista para que no vuelva a cogerla.
    }

    override public void MoreTime() { //Añadimos más tiempo al jugador.
        currentTime = Mathf.Clamp(currentTime + Constantes.TIME_REWARD - (correctAnswers / 4), 3, 200);
        if (tenAnswersBlock > 0)
            if (correctAnswers / 10 == tenAnswersBlock) {
                currentTime = Mathf.Clamp(currentTime + 20 - (correctAnswers / 4), 3, 200);
                tenAnswersBlock++;
            }
    }

    //Powerup de Pasar Pregunta
    override public void SkipQuestion() {
        if (PlayerPrefs.GetInt("Coins") > skipQuestionPrice) {
            PlaySounds((int)Clips.NextQuestion); //Reproducir sonido de pasar pregunta 
            listAuxRandomNumbers.Remove(selectedQuestionPosition); //Quitamos la pregunta de las posibles, asi no vuelve a aparecer
            PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - skipQuestionPrice); //Restamos las monedas
            skipQuestionPrice = (skipQuestionPrice * 3) / 2; //Aumentamos el precio de pasar pregunta
            nextQuestionPriceText.text = "" + skipQuestionPrice;
            ShowCoins();
            NewQuestion();
            CheckPowerUpPrices();
        }
    }

    override public void PowerUpSetFirstChip() {
        if (!powerupFirstChipSet) //Si ya ha utilizado el power up no se le permite volver a lanzarlo (no tendría sentido)
            if (PlayerPrefs.GetInt("Coins") > firstChipPrice) {
                CleanAllBoard();
                firstCorrectChip.GetComponent<Image>().color = Constantes.COLOR_GREEN; //Cambiamos el color de la ficha.
                firstCorrectChip.GetComponent<DragFicha>().parentToReturnTo = chipsSet.transform; //La primera ficha correcta se queda en chip set asi no vuelve a zona fichas.
                firstCorrectChip.GetComponent<DragFicha>().SetChip(initialDropZone.transform.GetComponent<RectTransform>());
                firstChipPrice = firstChipPrice * 3 / 2;
                setFirstPriceText.text = "" + firstChipPrice;
                powerupFirstChipSet = true;
                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - firstChipPrice);
                ShowCoins();
                CheckPowerUpPrices();
            }
    }

    override public void PowerUpDeleteChip() {
        if (counterDeleteChips < listNoValidChips.Count) { //Si ya se ha eliminado todas las fichas.
            if (PlayerPrefs.GetInt("Coins") > deleteChipPrice) {
                PlaySounds((int)Clips.Bomb); //Reproduce el sonido de este powerup
                CleanAllBoard(); //Limpia el tablero
                listNoValidChips[counterDeleteChips].gameObject.SetActive(false); //Borramos una ficha en la posicion X
                counterDeleteChips++; //Sumamos al contador que se encarga de eliminar fichas
                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - deleteChipPrice);
                deleteChipPrice = deleteChipPrice * 3 / 2;
                deleteChipPriceText.text = "" + deleteChipPrice;
                ShowCoins();
                CheckPowerUpPrices();
            }
        }
    }

    void CheckPowerUpPrices() { //Este metodo se encarga de comprobar los precios de cada powerup
        if (firstChipPrice > PlayerPrefs.GetInt("Coins") || powerupFirstChipSet) {
            powerupFirstChip.GetComponent<Button>().interactable = false;
        }else {
            powerupFirstChip.GetComponent<Button>().interactable = true;
        }
        if (skipQuestionPrice > PlayerPrefs.GetInt("Coins")) {
            powerupSkipQuestion.GetComponent<Button>().interactable = false;
        }
        else {
            powerupSkipQuestion.GetComponent<Button>().interactable = true;
        }
        if (deleteChipPrice > PlayerPrefs.GetInt("Coins")) {
            powerupDeleteChip.GetComponent<Button>().interactable = false;
        }
        else {
            powerupDeleteChip.GetComponent<Button>().interactable = true;
        }
    }

    override public void NewQuestion() {
        counterDeleteChips = 0; //El contador de fichas borradas del powerup bomba
        powerupFirstChipSet = false; //No hay primera ficha colocada.
        firstCorrectChip.GetComponent<Image>().color = Constantes.COLOR_WHITE_CHIP;
        RecoverDeletedChips(); //Recuperamos aquellas fichas que estaban en false.
        CleanAllBoard(); //Limpiamos tablero
        PrepareQuestion(); //Preparamos preguntas.
    }

    void NewQuestionAfterRestart() //Hemos copiado la funcion de arriba porque tiene que hacer lo mismo (una vez vuelve a jugar) menos preparar la pregunta, que ya lo hace en GameStarts
    {
		counterDeleteChips = 0; //El contador de fichas borradas del powerup bomba
		powerupFirstChipSet = false; //No hay primera ficha colocada.
		firstCorrectChip.GetComponent<Image>().color = Constantes.COLOR_WHITE_CHIP;
		RecoverDeletedChips(); //Recuperamos aquellas fichas que estaban en false.
		CleanAllBoard(); //Limpiamos tablero
	}

    void RecoverDeletedChips() { //Recorremos la lista de aquellas preguntas borradas y las activamos de nuevo.
        foreach (Transform chips in listNoValidChips) {
            chips.gameObject.SetActive(true);
        }
        listNoValidChips.Clear();
    }

    override public void CleanBoardButton() { //Borramos el tablero
        if (chipsSet.transform.childCount > 6) {  
            PlaySounds((int)Clips.Clean);
        }
        else if (chipsSet.transform.childCount > 5 && !powerupFirstChipSet) {
            PlaySounds((int)Clips.Clean);
        }

        CleanAllBoard();
    }

    override public void CleanAllBoard() {
        stackChipSet.Clear(); //Limpiamos la pila 
        dropHereTableroJuego.enabled = false; //Mientras limpia no puede poner una ficha
        isFirstChipSet = false; //Ya no está la primera ficha puesta
        currentWord.text = ""; //Limpiamos el cuadro de texto
        userWord = ""; //Limpiamos la palabra
        timeWithoutSet = 0;
        List<DragFicha> cleanListChips = new List<DragFicha>(); //Usamos esta lista para no quitar hijos mientras los busca. Primero buscamos y luego quitamos.
        foreach (Transform child in chipsSet.transform) { //Recorremos todos los objetos dentro de fichasColocadas
            if (child.tag == "MiniDrop") { //Si encontramos los minidrops, lo desactivamos
                child.gameObject.SetActive(false);
                continue;
            }
            
            DragFicha componente = child.GetComponent<DragFicha>(); //Buscamos el componente de Drag Ficha
            if (componente != null) {
                cleanListChips.Add(componente);
            }
        }

        foreach (DragFicha chip in cleanListChips) { //Una vez los tenemos, los activamos
            chip.enabled = true;
            chip.BackToInitialPosition();
        }

        //Recuperar zona de drop grande y recolocar en el centro.
        initialDropZone.SetActive(true); //Recuperamos la zona inicial del drop
        chipsSet.transform.localPosition = Vector3.zero; //Y recolocamos el tablero
        if (powerupFirstChipSet) { //Y si hay una ficha colocada....
            firstCorrectChip.GetComponent<DragFicha>().shouldSound = false; //Que no suene al volver a decirle que se vuelva
            firstCorrectChip.GetComponent<DragFicha>().parentToReturnTo = chipsSet.transform; //Que se quede en el padre en el que esta (chipSet
            firstCorrectChip.GetComponent<DragFicha>().SetChip(initialDropZone.transform.GetComponent<RectTransform>());
        }
    }

    //Funcion de generacion de dropzpnes para fichas horizontales. (Solo se ejecuta en caso de que sea la primera ficha)
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
	//Funcion de generacion de dropzpnes para fichas verticales. (Solo se ejecuta en caso de que sea la primera ficha)
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

    //Ejecutamos estas funciones para cuando se coloca una ficha y movemos los minidrops (minidrops de arriba con ficha horizontal)
    void MoveUpperDropzonesWithHorizontalChip(Transform lastChipSet, Transform upDropChip, Transform leftDropChip, int multiplier) {
        Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
        //Arriba
        upDropChip.localPosition = new Vector3(lastChipSet.localPosition.x - (rectLastChip.width / 4), lastChipSet.localPosition.y + (rectLastChip.height)) * multiplier;
        //Izquierda
        leftDropChip.localPosition = new Vector3(lastChipSet.localPosition.x - ((rectLastChip.width * 3) / 4), lastChipSet.localPosition.y) * multiplier;
    }

	//Ejecutamos estas funciones para cuando se coloca una ficha y movemos los minidrops (minidrops de abajo con ficha horizontal)
	void MoveLowerDropzonesWithHorizontalChip(Transform lastChipSet, Transform downDropChip, Transform rightDropChip, int multiplier) {
        Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
        //Arriba
        downDropChip.localPosition = new Vector3(lastChipSet.localPosition.x + (rectLastChip.width / 4), lastChipSet.localPosition.y - (rectLastChip.height)) * multiplier;
        //Izquierda
        rightDropChip.localPosition = new Vector3(lastChipSet.localPosition.x + ((rectLastChip.width * 3) / 4), lastChipSet.localPosition.y) * multiplier;
    }

	//Ejecutamos estas funciones para cuando se coloca una ficha y movemos los minidrops (minidrops de arriba con ficha vertical)
	void MoveUpperDropzonesWithVerticalChip(Transform lastChipSet, Transform upDropChip, Transform leftDropChip, int multiplier) {
        Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
        //Arriba
        upDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x, lastChipSet.localPosition.y + ((rectLastChip.height * 3) / 4)) * multiplier;
        //Izquierda
        leftDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x - rectLastChip.width, lastChipSet.localPosition.y + (rectLastChip.height / 4)) * multiplier;
    }

	//Ejecutamos estas funciones para cuando se coloca una ficha y movemos los minidrops (minidrops de abajo con ficha vertical)
	void MoveLowerDropzonesWithVerticalChip(Transform lastChipSet, Transform downDropChip, Transform rightDropChip, int multiplier) {
        Rect rectLastChip = lastChipSet.GetComponent<RectTransform>().rect;
        //Derecha
        rightDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x + rectLastChip.width, lastChipSet.localPosition.y - (rectLastChip.height / 4)) * multiplier;
        //Abajo 
        downDropChip.transform.localPosition = new Vector3(lastChipSet.localPosition.x, lastChipSet.localPosition.y - ((rectLastChip.height * 3) / 4)) * multiplier;
    }

    void Update() {
        if (startedGame) {
            if (currentTime <= 0) //Jugador se queda sin tiempo
                GameIsOver();
            else { //Jugador todavia tiene tiempo.
                if (currentTime <= 15) { //Si pasa de 15 segundos...
                    GameObject.Find("TimeSound").GetComponent<AudioSource>().volume = GameObject.Find("TimeSound").GetComponent<AudioSource>().volume + (Time.deltaTime / (currentTime + 1));
                    if (under5seconds) {
                        PlayTimeOut(true); //Se ejecuta el sonido de reloj
                        timeText.color = Color.red; //El texto cambia
                        under5seconds = false; //Está bajo los 5 segundos por tanto no puede volver a entrar aqui
                        upper5seconds = true;  //Pero puede volver a entrar en upper5seconds
                    }
                } else {
                    if (upper5seconds) {
                        GameObject.Find("TimeSound").GetComponent<AudioSource>().volume = 0.1f;
                        timeText.color = Color.black; //Cambiamos el color del reloj
                        PlayTimeOut(false); //Quitamos el sonido de reloj
                        under5seconds = true; //Esta por encima de los 5 segundos, por tanto puede volver a entrar en 5 por debajo
                        upper5seconds = false; //Esta por encima de los 5 segundos, por tanto no puede volver a estar en por arriba
                    }
                }


                currentTime -= 1 * Time.deltaTime; //Resta de tiempo
                timeText.text = "" + System.Math.Ceiling(currentTime); //Set text y redondeo hacia arriba
            }

            if (isFirstChipSet) { //Si esta la ficha colocada, empezamos a contar para mostrar las ayudas
                if (timeWithoutSet > Constantes.MAX_TIME_WITHOUTSET) //Si pasan x segundos, ejecutamos la ayuyda.
                    OnTimeHelp();
                else
                    timeWithoutSet += 1 * Time.deltaTime; //Si no, sigue sumando tiempo que lleva sin poner,
            }
        }
        if (startTimerCorrectAnswer) { //Si acierta, este booleano se activa
            if (correctAnswerCounter > 0.75f) { //Si el contador es mayor que 0'75 se reinician todos los valores
                correctAnswerCounter = 0; //El contador a 0
                startTimerCorrectAnswer = false; //Ya no hay timer para respuesta correcta
                imageFilter.fillAmount = 0; //Reiniciamos el filtro
                boardGame.GetComponent<Button>().enabled = true;
                cleanBoardButton.GetComponent<Button>().enabled = true;
                NewQuestion(); //Nos traemos una nueva pregunta
            }
            else {
                correctAnswerCounter += 1 * Time.deltaTime; //Sumamos al contador
                imageFilter.fillAmount = correctAnswerCounter / 0.75f; //Ponemos el filtro
            }
        }
    }

    override public void ShowCoins() {
        coinsText.text = "" + PlayerPrefs.GetInt("Coins");
    }

    void GameIsOver() {
        PlayTimeOut(false);
        controladorEstado.StopPlaying(oldCorrectAnswers, correctAnswers);
        startedGame = false;
    }

    public void SetRecords() {

		switch (Constantes.TYPE_OF_QUESTION)
		{
			case (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase:
					PlayerPrefs.SetInt(Constantes.RECORD_COMPLETA_FRASE, correctAnswers);
				break;
			case (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico:
					PlayerPrefs.SetInt(Constantes.RECORD_JEROGLIFICO, correctAnswers);
				break;
			case (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne:
					PlayerPrefs.SetInt(Constantes.RECORD_QUENOSUNE, correctAnswers);
				break;
		}
	}


    //Funcion llamada desde el controlador de estados
    override public void ResetGame() {
        timeText.color = Color.black; //Color en negro
        currentTime = initialTime; //Asignamos de nuevo el tiempo
        correctAnswers = 0; //Respeustas en 0
        correctText.text = "" + correctAnswers; 
        questionText.text = ""; //Quitamos el texto
		questionImage.sprite = null; //Quitamos la pregunta
        //Resetear el precio de los power-ups
        skipQuestionPrice = Constantes.SKIPQUESTION_PRICE;
        firstChipPrice = Constantes.FIRSTCHIP_PRICE;
        deleteChipPrice = Constantes.DELETECHIP_PRICE;
        if (questionImage.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Agrandar")) //Si el jugador ha ampliado la imagen, la devolvemos a su sitio.
            StartAnimation();
        SearchRecords();
        NewQuestionAfterRestart();
        CheckPowerUpPrices();
        SetPrices();
    }

    int SearchRecords() {
        if (Constantes.TYPE_OF_QUESTION == (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase)
            oldCorrectAnswers = PlayerPrefs.GetInt(Constantes.RECORD_COMPLETA_FRASE);
        else if (Constantes.TYPE_OF_QUESTION == (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico)
            oldCorrectAnswers = PlayerPrefs.GetInt(Constantes.RECORD_JEROGLIFICO);
        else
            oldCorrectAnswers = PlayerPrefs.GetInt(Constantes.RECORD_QUENOSUNE);

        return oldCorrectAnswers;
    }

    override public void ResumeGame() {
        startedGame = true;
        CheckPowerUpPrices();
    }

    //Funcion que sera llamada desde ControladorEstados para actualizar el tiempo una vez haya pagado por 20 segundos más.
    public void RefreshTime(int segundosExtra) {
        timeText.color = Color.black;
        timeText.text = "" + segundosExtra;
        currentTime = segundosExtra;
		if (questionImage.GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("Agrandar")) //Si el jugador ha ampliado la imagen, la devolvemos a su sitio.
			StartAnimation();
    }

    override public void StartAnimation() {
        questionImage.GetComponent<Animator>().SetTrigger("Agrandar");
    }

    void PlaySounds(int clip) {
        this.transform.GetComponent<AudioSource>().clip = clipsFX[clip];
        this.transform.GetComponent<AudioSource>().Play();
    }

    void PlayTimeOut(bool play) {
        if (play)
            GameObject.Find("TimeSound").GetComponent<AudioSource>().Play();
        else
            GameObject.Find("TimeSound").GetComponent<AudioSource>().Stop();
    }

    public void Undo() { //Funcion que sirve para deshacer un movimiento
        if (stackChipSet.Count == 0) {
            CleanAllBoard();
            return;
        }
        Vector3 recolocationPositionValue; //Este vector 3 sera el nuevo valor de fichas colocadas así podra reajustarse cuando se quite una ficha.
        //Necesitamos saber cual es la ultima ficha colocada.
        Transform lastChipSet = stackChipSet.Peek().lastChipSet; //Cogemos de la pila el transform de la ficha
        Transform lastMiniDropUsed = stackChipSet.Peek().miniDropSelected; //Cogemos de la pila el minidrop que ha elegido
        Vector3 lastMiniDropUsedPos = stackChipSet.Peek().miniDropSelectedPos; //Cogemos de la pila la posicion del minidrop elegido
        Transform lastMiniDropPartner = stackChipSet.Peek().miniDropPartner; //Cogemos de la pila el transform del minidrop partner
        Vector3 lastMiniDropPartnerPos = stackChipSet.Pop().miniDropPartnerPos; //Cogemos de la pila la posicion del minidrop partner
        int positionOnBoard = lastChipSet.GetSiblingIndex(); //O es 0 o es la ultima posicion de los hijos de FichasColocadas.
        //Cogemos ambos minidrops ya que necesitamos recolocarlos en donde estaban. 
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

