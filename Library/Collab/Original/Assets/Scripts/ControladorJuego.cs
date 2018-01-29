using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class ControladorJuego : MonoBehaviour {

    public AudioClip[] clipsFX = new AudioClip[4];

    bool primeraFichaColocada = false;
    bool powerUpPrimeraFichaColocada = false;
    ControladorEstados controladorEstado;
	GameObject fichasColocadas;
    GameObject fichaDrop;
    GameObject zonaFichas;
    GameObject zonaDropInicial;
    DropHere dropHereTableroJuego;
    public Text textoAciertos;
    public Text palabraFormada;
    public Text textoPregunta;
    public Image imagenPregunta;
    public Text textoMonedas;
    public Text textoTiempo;
    public Text textPrecioPasarPregunta;
    public Text textPrecioPonerPrimera;
    public Text textPrecioEliminarFicha;
    TextAsset txt;
	TextAsset aux;
    string winningWord;
    string currentQuestion;
    int monedasJugador; //Monedas actuales del jugador.
    float tiempoInicial = 30; //Tiempo que tendrá el jugador al iniciar la partida.
    float tiempoActual; //Tiempo del jugador.
    public string userWord;
    List<pregunta_respuesta> lista_preguntas_respuestas = new List<pregunta_respuesta>();
    List<imagen_respuesta> lista_imagenes_respuestas = new List<imagen_respuesta>();
    List<string> lista_palabrasAuxiliares = new List<string>();
    List<string> fichasPairs = new List<string>();
    int aciertos;
    bool juegoEmpezado = false; //Booleano que servira para manejar el contador de tiempo.
    int aciertosAntiguo;
    int precioPasarPregunta = 4;
    int precioPonerPrimera = 2;
    int precioEliminarFicha = 4;

    Transform primeraFichaCorrecta;
    List<Transform> listaFichasNoValidas = new List<Transform>();
    int contadorUsosEliminarFichas = 0;

    float tiempoSinColocar = 0;
    static float maximoTiempoSinColocar = 8;
	public delegate void delegateHelp(); //Declaramos un delegado
    public static event delegateHelp OnTimeHelp; //Declaramos el evento
    public static event delegateHelp OnHideTimeHelp;
    public static event delegateHelp OnMiniDropMoves;


	public struct pregunta_respuesta
	{
        public string pregunta;
		public string respuesta;
	}

    public struct imagen_respuesta
    {
        public Sprite imagen;
        public string respuesta;
    }

    public enum Clips
    {
        Aciertos,
        Limpiar,
        Bomba,
        PasarPregunta
    }

	void Start () {
        if(Constantes.tipoDePregunta == (int)Constantes.TipoDePregunta.CompletaLaFrase)
        {
            txt = (TextAsset)Resources.Load("termina_la_frase", typeof(TextAsset));
        }
        else if (Constantes.tipoDePregunta == (int)Constantes.TipoDePregunta.Jeroglifico)
        {
            txt = (TextAsset)Resources.Load("jeroglificos", typeof(TextAsset));
        }
        else
        {
            txt = (TextAsset)Resources.Load("que_nos_une", typeof(TextAsset));
        }
		aux = (TextAsset)Resources.Load("jeroglificos", typeof(TextAsset));
        leerTxts();
        leerTxtAuxiliar();
        ponerPrecios();
        fichasColocadas = GameObject.Find("FichasColocadas");
        fichaDrop = (GameObject)Resources.Load("FichaDrop", typeof(GameObject));
        zonaFichas = GameObject.Find("ZonaFichas");
        controladorEstado = GameObject.Find("ControladorEstados").GetComponent<ControladorEstados>();
        zonaDropInicial = GameObject.Find("DropZone");
        dropHereTableroJuego = GameObject.Find("TableroJuego").GetComponent<DropHere>();
        tiempoActual = tiempoInicial;
        textoTiempo.text = "" + tiempoActual;
        textoAciertos.text = "" + aciertos;
        PlayerPrefs.SetInt("Coins", 2000);
        MostrarMonedas();
        //InstanciarZonaDrops(); //Instanciamos zona de drops.
    }

    public void ponerPrecios(){
        textPrecioPasarPregunta.text = "" + precioPasarPregunta;
        textPrecioPonerPrimera.text = "" + precioPonerPrimera;
        textPrecioEliminarFicha.text = "" + precioEliminarFicha;
    }

    public void JuegoEmpieza(){
        juegoEmpezado = true;
		prepareQuestion(); //Preparamos preguntas.
		generateInitialCorrectFichas(); //Generamos los pares de letra correctos. 
		generateRandomInitialFichas(); //Generamos los pares de letras aleatorios e incorrectos para confundir al jugador.
		verFichasGeneradas();
    }

	private void leerTxts()
    {
        string fullTxt = txt.text;
        switch (Constantes.tipoDePregunta)
        {
            case (int)Constantes.TipoDePregunta.CompletaLaFrase:
                textoPregunta.gameObject.SetActive(true);
                imagenPregunta.gameObject.SetActive(false);
                leerCompletarFrases(fullTxt);
                break;
            case (int)Constantes.TipoDePregunta.Jeroglifico:
                textoPregunta.gameObject.SetActive(false);
                imagenPregunta.gameObject.SetActive(true);
                leerImagenes(fullTxt);
                break;
            case (int)Constantes.TipoDePregunta.QueNosUne:
                textoPregunta.gameObject.SetActive(false);
                imagenPregunta.gameObject.SetActive(true);
                leerImagenes(fullTxt);
                break;
            default:
                break;
        }
        
        //file.Close();
     }

    private void leerCompletarFrases(string fullTxt)
    {
        string[] lineasTxt = fullTxt.Split('\n');
        for(int i = 0; i < lineasTxt.Length; i += 2)
        {
            pregunta_respuesta p_r = new pregunta_respuesta();
            p_r.respuesta = lineasTxt[i].Trim();
            p_r.pregunta = lineasTxt[i + 1].Trim();
            lista_preguntas_respuestas.Add(p_r);
        }

        /*int contadorArray_preguntas_respuestas = 0;
        string line;
        while ((line = file.ReadLine()) != null)
        { //while text exists.. repeat
            pregunta_respuesta p_r = new pregunta_respuesta();
            p_r.respuesta = line;
            p_r.pregunta = file.ReadLine();
            contadorArray_preguntas_respuestas++;
            lista_preguntas_respuestas.Add(p_r);
        }
        */
    }

    private void leerImagenes(string fullTxt)
    {
        string[] lineasTxt = fullTxt.Split('\n');
        for (int i = 0; i < lineasTxt.Length; i++)
        {
            imagen_respuesta i_r = new imagen_respuesta();
            i_r.respuesta = lineasTxt[i].Trim();
            i_r.imagen = Resources.Load<Sprite>(lineasTxt[i].Trim());
            lista_imagenes_respuestas.Add(i_r);
        }
        /*
        int contadorArray_preguntas_respuestas = 0;
        string line;
        while ((line = file.ReadLine()) != null)
        { //while text exists.. repeat
            imagen_respuesta i_r = new imagen_respuesta();
            i_r.respuesta = line;
            print("EL TEXTO LEIDO ES: " + line);
            i_r.imagen = Resources.Load<Sprite>(line);
            contadorArray_preguntas_respuestas++;
            lista_imagenes_respuestas.Add(i_r);
        }
        */
    }

    private void leerTxtAuxiliar(){
        string fullTxt = aux.text;
        string[] lineAuxiliar = fullTxt.Split('\n');
        for(int i = 0; i < lineAuxiliar.Length; i++)
        {
            string p_r = lineAuxiliar[i].Trim();
            lista_palabrasAuxiliares.Add(p_r);
        }
		print("Imprimimos el array de palabras auxiliares");
		for (int i = 0; i < lista_palabrasAuxiliares.Count; i++)
		{
			print("LA PALABRA AUXILIAR ES.. " + lista_palabrasAuxiliares[i]);
		}
    }

	public void reajustarTablero(int miPosition, string nombreOrientacion){
        if(nombreOrientacion.IndexOf("Horizontal") != -1){ //La ficha recibida es horizontal

            int contador = 0;
            foreach (Transform fichaARecolocar in fichasColocadas.transform){
                
                if(contador >= miPosition){
                    //Comprobamos que ficha estamos moviendo. En caso de ser horizontal...
                    if (fichaARecolocar.transform.name.IndexOf("Horizontal") != -1){
                        fichaARecolocar.transform.localPosition = fichaARecolocar.transform.localPosition + new Vector3(-fichaARecolocar.GetComponent<RectTransform>().rect.width, fichaARecolocar.GetComponent<RectTransform>().rect.height);
                    } else {
                        fichaARecolocar.transform.localPosition = fichaARecolocar.transform.localPosition + new Vector3(-fichaARecolocar.GetComponent<RectTransform>().rect.height, fichaARecolocar.GetComponent<RectTransform>().rect.width);
                    }
                }

                contador++;
            }

        } else {
            int contador = 0;
			foreach (Transform fichaARecolocar in fichasColocadas.transform)
			{
				if (contador >= miPosition)
				{
					//Comprobamos que ficha estamos moviendo. En caso de ser horizontal...
					if (fichaARecolocar.transform.name.IndexOf("Horizontal") != -1)
					{
                        fichaARecolocar.transform.localPosition = fichaARecolocar.transform.localPosition + new Vector3(-fichaARecolocar.GetComponent<RectTransform>().rect.height, fichaARecolocar.GetComponent<RectTransform>().rect.width);
					}
					else
					{
                        fichaARecolocar.transform.localPosition = fichaARecolocar.transform.localPosition + new Vector3(-fichaARecolocar.GetComponent<RectTransform>().rect.width, fichaARecolocar.GetComponent<RectTransform>().rect.height);
					}
				}

                contador++;
			}
        }
    }


    void verFichasGeneradas(){
        //Primero que nada, hacemos un shuffle de la lista.
        for (int i = 1; i < fichasPairs.Count; i++){
            int numeroRandom = Random.Range(0, i);
            string pairAuxiliar = fichasPairs[i];
            fichasPairs[i] = fichasPairs[numeroRandom];
            fichasPairs[numeroRandom] = pairAuxiliar;
        }

        List<Transform> listaFichasCorrectasAux = new List<Transform>();
        int contador = 0;
        bool primeraFichaEncontrada = false;
        string auxWinningWord = winningWord;
        foreach (Transform child in zonaFichas.transform)
        {
            int contadorLetras = 0;
            string parDeLetras = "";
            foreach (Transform text in child.transform)
            {
                parDeLetras += fichasPairs[contador].Substring(contadorLetras, 1);
                text.GetComponent<Text>().text = fichasPairs[contador].Substring(contadorLetras,1);
                contadorLetras++;
            }
            if (parDeLetras == winningWord.Substring(0, 2) && !primeraFichaEncontrada)
            {
                primeraFichaCorrecta = child;
                primeraFichaEncontrada = true;
            }
            //Si aún quedan pares de letra por encontrar entre las fichas entramos
            if (auxWinningWord.Length != 0)
            {
                //Recorremos la palabra en pares de letras
                for (int i = 0; i < auxWinningWord.Length; i += 2)
                {
                    //Si la ficha tiene las mismas letras que la palabra ganadora, guardamos esa ficha
                    if (auxWinningWord.Substring(i, 2) == parDeLetras)
                    {
                        listaFichasCorrectasAux.Add(child);
                        auxWinningWord.Remove(i, 2);
                    }
                }
            }
            print("TAMAÑO DE LA LISTA DE FICHAS CORRECTAS = " + listaFichasCorrectasAux.Count);
            parDeLetras = "";
			contador++;
        }
        int contadorPrint = 0;
        foreach(Transform child in zonaFichas.transform)
        {
            bool encontrado = false;
            int contadorVueltas = 0;
            while (!encontrado && contadorVueltas < listaFichasCorrectasAux.Count)
            {
                if(child == listaFichasCorrectasAux[contadorVueltas])
                {
                    encontrado = true;
                }
                contadorVueltas++;
            }
            if (!encontrado)
            {
                listaFichasNoValidas.Add(child);
                contadorPrint++;
            }
        }
    }

    void prepareQuestion(){
        if(Constantes.tipoDePregunta == (int)Constantes.TipoDePregunta.CompletaLaFrase)
        {
            //Preparamos preguntas del nivel
            int numeroRandomPregunta = Random.Range(0, lista_preguntas_respuestas.Count);
            winningWord = lista_preguntas_respuestas[numeroRandomPregunta].respuesta;
            textoPregunta.text = lista_preguntas_respuestas[numeroRandomPregunta].pregunta;
        }
        else
        {
            //Preparamos preguntas del nivel
            int numeroRandomPregunta = Random.Range(0, lista_imagenes_respuestas.Count);
            winningWord = lista_imagenes_respuestas[numeroRandomPregunta].respuesta;
            imagenPregunta.sprite = lista_imagenes_respuestas[numeroRandomPregunta].imagen;
        }
	}



    void generateRandomInitialFichas(){

        while (fichasPairs.Count < 7) //Va a generar fichas random solo cuando haya espacio en las 7
        {
            string palabraEscogida = lista_palabrasAuxiliares[Random.Range(0, lista_palabrasAuxiliares.Count)]; //Cogemos una palabra al azar de las posibles.

			if (palabraEscogida.Length % 2 != 0)
				palabraEscogida = palabraEscogida + " ";
            
			int numeroRandomLetra = Random.Range(0, (palabraEscogida.Length / 2)) * 2;
            string random_pair = palabraEscogida.Substring(numeroRandomLetra, 2);
            fichasPairs.Add(random_pair);
		}
    }

    void generateInitialCorrectFichas(){
        fichasPairs.Clear();
        int contador = 0;
        string duoLetras = "";

        if(winningWord.Length % 2 != 0) //Comprobamos si una palabra es impar o no.
            winningWord = winningWord + " "; //Le añadimos un espacio, así tiene un caracter más.
        
        do
        {
            duoLetras = winningWord.Substring(contador, 2);
            contador += 2;
            fichasPairs.Add(duoLetras);
            print("DUOLETRAS     " + duoLetras);
        } while (!duoLetras.Equals("") && contador < winningWord.Length);
    }


    public void CheckDragDrop(Transform lastFichaSet, Vector3 recolocationPositionValue){
        primeraFichaColocada = true;
        if(tiempoSinColocar > 7.9){
            OnHideTimeHelp();
        }
        tiempoSinColocar = 0;
        userWord = "";
        List<Transform> fichasList = new List<Transform>();
        Dictionary<string, Transform> dropZoneDic = new Dictionary<string, Transform>();

        int fichaCount = 0;
        bool firstFicha = false;
        //Comprueba si la palabra formada por las fichas es la ganadora.
        foreach (Transform child in fichasColocadas.transform){
            fichaCount++;
            foreach (Transform text in child.transform) {
                string letra = text.GetComponent<Text>().text;
                userWord = userWord + letra;
            }
            if(child.name == "DropZone" && child.gameObject.activeInHierarchy)
            {
                //Es la primera ficha que se coloca
                dropHereTableroJuego.enabled = true;                
                child.gameObject.SetActive(false);
                firstFicha = true;
            }
            if (child.tag == "Ficha")
            {
                fichasList.Add(child);
                if (firstFicha)
                {
                    fichasList[0].localPosition = new Vector3(0, 0, 0);
                }
            }else if(child.tag == "MiniDrop")
            {
                dropZoneDic.Add(child.name, child);
            }
            
        }

        if (isWinningWord(userWord))
        {
            //Contador para mantener las palabras por 'x' segundos en el tablero.
				PlaySounds((int)Clips.Aciertos);
				palabraFormada.text = "";
				tiempoActual = Mathf.Clamp(tiempoActual + 10 - (aciertos / 4), 3, 200);
				aciertos++;
				textoAciertos.text = "" + aciertos;
            textoAciertos.GetComponent<Animator>().SetTrigger("jump");
				newQuestion(fichasList);
        }
        else //No es la palabra ganadora, sigue poniendo fichas.
        {
            //Si encuentra la cadena "Horizontal" (valor distinto de -1)
            if (lastFichaSet.name.IndexOf("Horizontal") != -1)
            {
                if(firstFicha)
                    generateHorizontalDrops(lastFichaSet, dropZoneDic["Derecha"], dropZoneDic["Abajo"], dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
                else
                {
                    fichasColocadas.transform.localPosition += recolocationPositionValue;
                    //Si la última ficha puesta está en la primera posición entonces hay que 
                    //cambiar la posición de las dropZone "Arriba" e "Izquierda"
                    if (lastFichaSet.GetSiblingIndex() == 0)
                    {
                        moveUpperDropzonesWithHorizontalFicha(lastFichaSet, dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
                    }
                    //En caso contrario se moverán las dropZone inferiores
                    else
                    {
                        moveLowerDropzonesWithHorizontalFicha(lastFichaSet, dropZoneDic["Abajo"], dropZoneDic["Derecha"]);
                    }
                }
            }
            else
            {
                if (firstFicha)
                    generateVerticalDrops(lastFichaSet, dropZoneDic["Derecha"], dropZoneDic["Abajo"], dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
                else
                {
                    fichasColocadas.transform.localPosition += recolocationPositionValue;
                    //Si la última ficha puesta está en la primera posición entonces hay que 
                    //cambiar la posición de las dropZone "Arriba" e "Izquierda"
                    if (lastFichaSet.GetSiblingIndex() == 0)
                    {
                        moveUpperDropzonesWithVerticalFicha(lastFichaSet, dropZoneDic["Arriba"], dropZoneDic["Izquierda"]);
                    }
                    //En caso contrario se moverán las dropZone inferiores
                    else
                    {
                        moveLowerDropzonesWithVerticalFicha(lastFichaSet, dropZoneDic["Abajo"], dropZoneDic["Derecha"]);
                    }
                }
            }
            OnMiniDropMoves();
        }
    }

    public void pasarPregunta(bool powerup){
        contadorUsosEliminarFichas = 0;
        powerUpPrimeraFichaColocada = false;
        primeraFichaCorrecta.GetComponent<Image>().color = new Color32(238, 231, 231, 255);
        if (powerup){
            if(PlayerPrefs.GetInt("Coins") < precioPasarPregunta)
            {
                print("NO TE QUEDAN MONEDAS");
            }
            else
            {
				PlaySounds ((int)Clips.PasarPregunta);
                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - precioPasarPregunta);
                precioPasarPregunta = (precioPasarPregunta * 3) / 2;
                textPrecioPasarPregunta.text = "" + precioPasarPregunta;
                MostrarMonedas();
                recuperarFichasEliminadas();
                cleanAllBoard();
                prepareQuestion();
                generateInitialCorrectFichas(); //Generamos los pares de letra correctos. 
                generateRandomInitialFichas(); //Generamos los pares de letras aleatorios e incorrectos para confundir al jugador.
                verFichasGeneradas(); //Colocamos el array con las letras en las fichas del juego
            }
        }
        else
        {
            recuperarFichasEliminadas();
            cleanAllBoard();
            prepareQuestion();
            generateInitialCorrectFichas(); //Generamos los pares de letra correctos. 
            generateRandomInitialFichas(); //Generamos los pares de letras aleatorios e incorrectos para confundir al jugador.
            verFichasGeneradas(); //Colocamos el array con las letras en las fichas del juego
        }
	}

    public void powerUpColocarFicha()
    {
        //Necesitaremos guardar las fichas que forman la palabra en un array ordenado.
        //Una vez tengamos las fichas tan solo será necesario llamar a la función ColocarFicha del script de la ficha correspondiente.
        //Si ya ha utilizado el power up no se le permite volver a lanzarlo (no tendría sentido)
        if (!powerUpPrimeraFichaColocada)
        {
            if (PlayerPrefs.GetInt("Coins") >= precioPonerPrimera)
            {
                cleanAllBoard();
                primeraFichaCorrecta.GetComponent<Image>().color = Color.green;
                primeraFichaCorrecta.GetComponent<DragFicha>().parentToReturnTo = fichasColocadas.transform;
                primeraFichaCorrecta.GetComponent<DragFicha>().ColocarFicha(zonaDropInicial.transform.GetComponent<RectTransform>());
                powerUpPrimeraFichaColocada = true;

                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - precioPonerPrimera);
                precioPonerPrimera = precioPonerPrimera * 3 / 2;
                textPrecioPonerPrimera.text = "" + precioPonerPrimera;
                textoMonedas.text = "" + PlayerPrefs.GetInt("Coins");
            }
        }
    }

    public void powerUpEliminarFicha()
    {
        if(contadorUsosEliminarFichas < listaFichasNoValidas.Count)
        {
            if (PlayerPrefs.GetInt("Coins") >= precioEliminarFicha)
            {
                //TODO Asegurarse de que limpiar la mesa no suene si tiene que sonar la bomba
                PlaySounds((int)Clips.Bomba);
                cleanAllBoard();
                listaFichasNoValidas[contadorUsosEliminarFichas].gameObject.SetActive(false);
                contadorUsosEliminarFichas++;

                PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") - precioEliminarFicha);
                precioEliminarFicha = precioEliminarFicha * 3 / 2;
                textPrecioEliminarFicha.text = "" + precioEliminarFicha;
                textoMonedas.text = "" + PlayerPrefs.GetInt("Coins");
            }
        }
        else
        {
            print("YA NO QUEDAN FICHAS QUE QUITAR");
        }
    }

    public void newQuestion(List<Transform> fichasList)
    {
        contadorUsosEliminarFichas = 0;
        powerUpPrimeraFichaColocada = false;
        primeraFichaCorrecta.GetComponent<Image>().color = new Color32(238, 231, 231, 255);
        recuperarFichasEliminadas();
        cleanAllBoard();
        prepareQuestion();
		generateInitialCorrectFichas(); //Generamos los pares de letra correctos. 
		generateRandomInitialFichas(); //Generamos los pares de letras aleatorios e incorrectos para confundir al jugador.
        verFichasGeneradas(); //Colocamos el array con las letras en las fichas del juego
	}

    void recuperarFichasEliminadas()
    {
        foreach(Transform fichas in listaFichasNoValidas)
        {
            fichas.gameObject.SetActive(true);
        }
        listaFichasNoValidas.Clear();
    }

    public void cleanBoardButon(){
        if (fichasColocadas.transform.childCount > 6){
            PlaySounds((int)Clips.Limpiar);
        } else if (fichasColocadas.transform.childCount > 5 && !powerUpPrimeraFichaColocada){
            PlaySounds((int)Clips.Limpiar);
        }

        cleanAllBoard();
    }

	public void cleanAllBoard(){
        dropHereTableroJuego.enabled = false;
		primeraFichaColocada = false;
        palabraFormada.text = "";
        tiempoSinColocar = 0;
        List<DragFicha> fichasAColocar = new List<DragFicha>();

        foreach (Transform child in fichasColocadas.transform)
        {
			DragFicha componente = child.GetComponent<DragFicha>();

            if (child.tag == "MiniDrop")
			{
                child.gameObject.SetActive(false);
			}

			if (componente != null)
			{
                fichasAColocar.Add(componente);
			}
        }

        foreach (DragFicha ficha in fichasAColocar){
			ficha.enabled = true;
			ficha.volverAMiPosicionInicial();
        }
		//3. Recuperar zona de drop grande y recolocar en el centro.
		zonaDropInicial.SetActive(true);
		fichasColocadas.transform.localPosition = Vector3.zero;
        if (powerUpPrimeraFichaColocada)
        {
            primeraFichaCorrecta.GetComponent<DragFicha>().deboSonar = false;
            primeraFichaCorrecta.GetComponent<DragFicha>().parentToReturnTo = fichasColocadas.transform;
            primeraFichaCorrecta.GetComponent<DragFicha>().ColocarFicha(zonaDropInicial.transform.GetComponent<RectTransform>());
        }
    }

    /*void cleanBoard(List<Transform> fichasList)
    {
        dropHereTableroJuego.enabled = false;
		primeraFichaColocada = false;
        int contador = 0;
        tiempoSinColocar = 0;

        //1. Retiramos las fichas del tablero.
        foreach (Transform ficha in fichasList)
        {
            DragFicha componente = ficha.GetComponent<DragFicha>();

            if (componente != null)
            {
                componente.enabled = true;
                componente.volverAMiPosicionInicial();
            }

             
        }
        //2. Destruir fichas drop_zones
        foreach (Transform zonaDrop in fichasColocadas.transform)
        {
            if (zonaDrop.tag == "MiniDrop")
            {
                zonaDrop.gameObject.SetActive(false);
            }
        }
        //3. Recuperar zona de drop grande y recolocar en el centro.
        zonaDropInicial.SetActive(true);
        fichasColocadas.transform.localPosition = Vector3.zero;
    }*/


    private bool isWinningWord(string userWord){
        palabraFormada.text = userWord.ToUpper();
        //Leer las fichas que contiene fichas colocadas.
        return userWord.ToUpper() == winningWord.ToUpper();
    }

    private void InstanciarZonaDrops(){
		//Derecha
		GameObject fichaDropDerecha = Instantiate(fichaDrop, fichasColocadas.transform);
		fichaDropDerecha.transform.name = "Derecha";
        fichaDropDerecha.SetActive(false);
		//Abajo:
		GameObject fichaDropAbajo = Instantiate(fichaDrop, fichasColocadas.transform);
		fichaDropAbajo.transform.name = "Abajo";
        fichaDropAbajo.SetActive(false);
		//Arriba
		GameObject fichaDropArriba = Instantiate(fichaDrop, fichasColocadas.transform);
		fichaDropArriba.transform.name = "Arriba";
        fichaDropArriba.SetActive(false);
		//Izquierda
		GameObject fichaDropIzquierda = Instantiate(fichaDrop, fichasColocadas.transform);
		fichaDropIzquierda.transform.name = "Izquierda";
        fichaDropIzquierda.SetActive(false);
    }

    private void generateHorizontalDrops(Transform ficha, Transform fichaDropDerecha, Transform fichaDropAbajo, Transform fichaDropArriba, Transform fichaDropIzquierda)
    {
        float sizeFichaRenderX = ficha.GetComponent<RectTransform>().rect.width;
        float sizeFichaRenderY = ficha.GetComponent<RectTransform>().rect.height;
        float positionFichaX = ficha.localPosition.x;
        float positionFichaY = ficha.localPosition.y;
        //Horizontales
        //Derecha
        fichaDropDerecha.gameObject.SetActive(true);
        fichaDropDerecha.transform.localPosition = new Vector3(positionFichaX + ((sizeFichaRenderX * 3) / 4), positionFichaY);
        //Abajo:
        fichaDropAbajo.gameObject.SetActive(true);
        fichaDropAbajo.transform.localPosition = new Vector3(positionFichaX + (sizeFichaRenderX / 4), positionFichaY - (sizeFichaRenderY));
        //Arriba
        fichaDropArriba.gameObject.SetActive(true);
        fichaDropArriba.transform.localPosition = new Vector3(positionFichaX - (sizeFichaRenderX / 4), positionFichaY + (sizeFichaRenderY));
        //Izquierda
        fichaDropIzquierda.gameObject.SetActive(true);
        fichaDropIzquierda.transform.localPosition = new Vector3(positionFichaX - ((sizeFichaRenderX * 3) / 4), positionFichaY);
    }

    private void generateVerticalDrops(Transform ficha, Transform fichaDropDerecha, Transform fichaDropAbajo, Transform fichaDropArriba, Transform fichaDropIzquierda)
    {
        float sizeFichaRenderX = ficha.GetComponent<RectTransform>().rect.width;
        float sizeFichaRenderY = ficha.GetComponent<RectTransform>().rect.height;
        float positionFichaX = ficha.GetComponent<Transform>().localPosition.x;
        float positionFichaY = ficha.GetComponent<Transform>().localPosition.y;

		//VERTICALES
		//Derecha
		fichaDropDerecha.gameObject.SetActive(true);
        fichaDropDerecha.transform.localPosition = new Vector3(positionFichaX + sizeFichaRenderX , positionFichaY - (sizeFichaRenderY/4));
        //Abajo 
        fichaDropAbajo.gameObject.SetActive(true);
        fichaDropAbajo.transform.localPosition = new Vector3(positionFichaX, positionFichaY - ((sizeFichaRenderY * 3) / 4));
        //Arriba
        fichaDropArriba.gameObject.SetActive(true);
		fichaDropArriba.transform.localPosition = new Vector3(positionFichaX, positionFichaY + ((sizeFichaRenderY * 3) / 4));
        //Izquierda
        fichaDropIzquierda.gameObject.SetActive(true);
		fichaDropIzquierda.transform.localPosition = new Vector3(positionFichaX - sizeFichaRenderX, positionFichaY + (sizeFichaRenderY / 4));

    }

    private void moveUpperDropzonesWithHorizontalFicha(Transform lastFichaSet, Transform fichaDropArriba, Transform fichaDropIzquierda)
    {
        Rect rectLastFicha = lastFichaSet.GetComponent<RectTransform>().rect;
        //Arriba
        fichaDropArriba.localPosition = new Vector3(lastFichaSet.localPosition.x - (rectLastFicha.width / 4), lastFichaSet.localPosition.y + (rectLastFicha.height));
        //Izquierda
        fichaDropIzquierda.localPosition = new Vector3(lastFichaSet.localPosition.x - ((rectLastFicha.width * 3) / 4), lastFichaSet.localPosition.y);
    }

    private void moveLowerDropzonesWithHorizontalFicha(Transform lastFichaSet, Transform fichaDropAbajo, Transform fichaDropDerecha)
    {
        Rect rectLastFicha = lastFichaSet.GetComponent<RectTransform>().rect;
        //Arriba
        fichaDropAbajo.localPosition = new Vector3(lastFichaSet.localPosition.x + (rectLastFicha.width / 4), lastFichaSet.localPosition.y - (rectLastFicha.height));
        //Izquierda
        fichaDropDerecha.localPosition = new Vector3(lastFichaSet.localPosition.x + ((rectLastFicha.width * 3) / 4), lastFichaSet.localPosition.y);
    }

    private void moveUpperDropzonesWithVerticalFicha(Transform lastFichaSet, Transform fichaDropArriba, Transform fichaDropIzquierda)
    {
        Rect rectLastFicha = lastFichaSet.GetComponent<RectTransform>().rect;
        //Arriba
        fichaDropArriba.transform.localPosition = new Vector3(lastFichaSet.localPosition.x, lastFichaSet.localPosition.y + ((rectLastFicha.height * 3) / 4));
        //Izquierda
        fichaDropIzquierda.transform.localPosition = new Vector3(lastFichaSet.localPosition.x - rectLastFicha.width, lastFichaSet.localPosition.y + (rectLastFicha.height / 4));
    }
    private void moveLowerDropzonesWithVerticalFicha(Transform lastFichaSet, Transform fichaDropAbajo, Transform fichaDropDerecha)
    {
        Rect rectLastFicha = lastFichaSet.GetComponent<RectTransform>().rect;
        //Derecha
        fichaDropDerecha.transform.localPosition = new Vector3(lastFichaSet.localPosition.x + rectLastFicha.width, lastFichaSet.localPosition.y - (rectLastFicha.height / 4));
        //Abajo 
        fichaDropAbajo.transform.localPosition = new Vector3(lastFichaSet.localPosition.x, lastFichaSet.localPosition.y - ((rectLastFicha.height * 3) / 4));
    }


    // Update is called once per frame
    void Update () {
        if (juegoEmpezado)
        {
            if (tiempoActual <= 0) //Jugador se queda sin tiempo
            {
                JuegoAcaba();
            }
            else
            { //Jugador todavia tiene tiempo.
                if (tiempoActual <= 5)
                {
                    textoTiempo.color = Color.red;
                }
                else
                {
                    textoTiempo.color = Color.black;
                }
                tiempoActual -= 1 * Time.deltaTime;
                textoTiempo.text = "" + System.Math.Ceiling(tiempoActual);
            }

            if (primeraFichaColocada)
            {
                if (tiempoSinColocar > maximoTiempoSinColocar) {
                    OnTimeHelp();
                } else {
                    tiempoSinColocar += 1 * Time.deltaTime;
                }
            }
        }
	}

    public void MostrarMonedas(){
        textoMonedas.text = "" + PlayerPrefs.GetInt("Coins");
    }

    void JuegoAcaba(){
        controladorEstado.TerminarJugar(aciertosAntiguo, aciertos);
        juegoEmpezado = false;
        //TODO: Sonidos?
    }

    public void ResetGame()
    {
        textoTiempo.color = Color.black;
        tiempoActual = tiempoInicial;
        aciertos = 0;
        textoPregunta.text = "";
        pasarPregunta(false);
        //TODO: Resetear el precio de los power-ups
    }

    public void ResumeGame(int segundosExtra)
    {
        textoTiempo.color = Color.black;
        tiempoActual = segundosExtra;
        juegoEmpezado = true;
    }

    public void StartAnimation()
    {
        imagenPregunta.GetComponent<Animator>().SetTrigger("Agrandar");
    }

    private void PlaySounds(int clip)
    {
        this.transform.GetComponent<AudioSource>().clip = clipsFX[clip];
        this.transform.GetComponent<AudioSource>().Play();
    }
}