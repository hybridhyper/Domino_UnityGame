/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using System.Collections.Generic;
using UnityEngine;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using UnityEngine.SceneManagement;

public class PreguntasSingleton : MonoBehaviour {

    public static PreguntasSingleton instance = null; //Instancia del objeto.
    public static ItemDatabase itemDB; //ItemDB será el objeto donde guardemos las listas. 
    TextAsset txt; //Txt con las preguntas de la aplicación. A partir de este se crea el XML
    public bool loadSceneAdMob = false;
	int controllerGameNumber; //Integer que indica el tipo de controlador que se está usando (contrarreloj, relax o tutorial)

    //Get / Set para obtener el controlador del juego que estamos utilizando ahora mismo (0 Clasico, 1 Contrarreloj, 2 Relax, 3 Tutorial).
    public int getControllerGameNumber() {
        return controllerGameNumber;
    }

    public void setControllerGameNumber(int number) {
        this.controllerGameNumber = number;
    }

    void Start () {
        Screen.fullScreen = false; //Necesario para que la barra de acción inferior en algunos dispositivos android no se ocult
        //Patron Singleton
		if (instance == null) {
			DontDestroyOnLoad(this);
            instance = this;
		} else {
			Destroy(this.gameObject);
		}

        itemDB = new ItemDatabase(); //Objeto que contiene las preguntas del juego

        //Si el archivo existe, lee los datos del XML. Si no existe, lee txts y crea un nuevo archivo. 
		if (File.Exists(Application.persistentDataPath + "/item_data.xml")){
			LoadDataFromXML();
		}else{
			ReadTxTs();
		}
	}

	public string DeleteSpecialChars(string imageName) //Se sustituyen los caracteres especiales (tildes, ñ, ...)
	{
		string specialChars = "áàäéèëíìïóòöúùüñÁÀÄÉÈËÍÌÏÓÒÖÚÙÜÑçÇ";
		string charsASCII = "aaaeeeiiiooouuunAAAEEEIIIOOOUUUNcC";
		for (int i = 0; i < specialChars.Length; i++)
		{
			imageName = imageName.Replace(specialChars[i], charsASCII[i]);
		}
		return imageName;
	}
	
    void ReadTxTs() { //Lee los texts y con el metodo FillLists las rellena.
        itemDB = new ItemDatabase();
		txt = (TextAsset)Resources.Load("que_nos_une", typeof(TextAsset));
		string fullTxt = txt.text;
        FillLists(fullTxt, (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne);
		txt = (TextAsset)Resources.Load("jeroglificos", typeof(TextAsset));
		fullTxt = txt.text;
        FillLists(fullTxt, (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico);
		txt = (TextAsset)Resources.Load("termina_la_frase", typeof(TextAsset));
		fullTxt = txt.text;
        FillLists(fullTxt, (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase);
        //Tras haber leido los txts y rellenado las listas, salvamos estas preguntas en un xml
		SaveDataIntoXML();
	}

    void FillLists(string fullTxt, int list){ //Lee linea a linea los txts lines y lo añade como un objeto de QuestionAndAnswer. 
		string[] txtLines = fullTxt.Split('\n');
		switch (list){
			case (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne: //Que nos une
				for (int i = 0; i < txtLines.Length; i++){
                    itemDB.listQueNosUne.Add(new QuestionAndAnswer(DeleteSpecialChars(txtLines[i].Trim()), txtLines[i].Trim(), false));
				}
				break;
            case (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico: //Jeroglificos
				for (int i = 0; i < txtLines.Length; i++){
                    itemDB.listJeroglificos.Add(new QuestionAndAnswer(DeleteSpecialChars(txtLines[i].Trim()), txtLines[i].Trim(), false));
				}
				break;
            case (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase: //termina la frase
				for (int i = 0; i < txtLines.Length; i += 2){
					itemDB.listCompletaFrase.Add(new QuestionAndAnswer(txtLines[i + 1], txtLines[i].Trim(), false));
				}
				break;
		}
	}

	void SaveDataIntoXML(){ //Salvar los elementos de la lista en un XML
		XmlSerializer serializer = new XmlSerializer(typeof(ItemDatabase)); //Creamos un serializador de tipo ItemDatabase
		FileStream stream = new FileStream(Application.persistentDataPath + "/item_data.xml", FileMode.Create);
        serializer.Serialize(stream, itemDB); //Toma la informacion, la pone en un archivo xmL
		stream.Close();
	}

    void LoadDataFromXML() { //Cargamos los datos del XML y los deserializamos en itemDB
        XmlSerializer serializer = new XmlSerializer(typeof(ItemDatabase)); //Creamos un serializador de tipo ItemDatabase
        FileStream stream = new FileStream(Application.persistentDataPath + "/item_data.xml", FileMode.Open);
        itemDB = serializer.Deserialize(stream) as ItemDatabase;
        stream.Close();
    }

    public void ResetFalseQuestions() { //Si todas las preguntas ya se han contestado se procede a reiniciarlas todas para poder volver a utilizarlas
		switch (Constantes.TYPE_OF_QUESTION) {
			case (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase:
                for (int i = 0; i < itemDB.listCompletaFrase.Count; i++) {
                    itemDB.listCompletaFrase[i].used = false;
                }
				break;
			case (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico:
                for (int i = 0; i < itemDB.listJeroglificos.Count; i++) {
                    itemDB.listJeroglificos[i].used = false;
				}
				break;
			case (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne:
                for (int i = 0; i < itemDB.listQueNosUne.Count; i++) {
                    itemDB.listQueNosUne[i].used = false;
				}
				break;
			default:
				break;
		}
    }


    public void ReorderListOfQuestions(){ //Reordenamos la lista para que las preguntas utilizadas queden las primeras
        List<QuestionAndAnswer> listAux = new List<QuestionAndAnswer>();
		switch (Constantes.TYPE_OF_QUESTION)
		{
			case (int)Constantes.ENUM_TYPEOFQUESTION.CompletaLaFrase:
                for (int i = 0; i < itemDB.listCompletaFrase.Count; i++){
                    if (itemDB.listCompletaFrase[i].used)
                        listAux.Insert(0, itemDB.listCompletaFrase[i]);
                    else
                        listAux.Add(itemDB.listCompletaFrase[i]);
                }
                itemDB.listCompletaFrase.Clear();
                itemDB.listCompletaFrase.AddRange(listAux);
				break;
			case (int)Constantes.ENUM_TYPEOFQUESTION.Jeroglifico:
				for (int i = 0; i < itemDB.listJeroglificos.Count; i++)
				{
					if (itemDB.listJeroglificos[i].used)
						listAux.Insert(0, itemDB.listJeroglificos[i]);
					else
						listAux.Add(itemDB.listJeroglificos[i]);
				}
				itemDB.listJeroglificos.Clear();
				itemDB.listJeroglificos.AddRange(listAux);
				break;
			case (int)Constantes.ENUM_TYPEOFQUESTION.QueNosUne:
				for (int i = 0; i < itemDB.listQueNosUne.Count; i++)
				{
					if (itemDB.listQueNosUne[i].used)
						listAux.Insert(0, itemDB.listQueNosUne[i]);
					else
						listAux.Add(itemDB.listQueNosUne[i]);
				}
				itemDB.listQueNosUne.Clear();
				itemDB.listQueNosUne.AddRange(listAux);
				break;
			default:
				break;
		}

       
    }

    void OnApplicationPause(bool pause){
        if (Application.platform == RuntimePlatform.IPhonePlayer) {
            //Si es Iphone, no hagas nada, ya que puede ser que haya abierto el Control Center o el sistema de Notificaciones.
            PlayerPrefs.SetString("iphone", "pausada");
        }
        else {
            //Si es Android, si deberias hacerlo: ExitGame.
            ExitGame(pause);
        }
    }

    void OnApplicationQuit() {
        ExitGame(true);
    }

    void ExitGame(bool pause) {
        if (pause) {
            ReorderListOfQuestions();
            SaveDataIntoXML();
        }
        else
            if (SceneManager.GetActiveScene().name == "JuegoContrarreloj" && !loadSceneAdMob) { //Depende de en donde está
            SceneManager.LoadScene("EscenaInicio");
        }
        else {
            loadSceneAdMob = false;
        }
    }
}

[System.Serializable]
public class QuestionAndAnswer {
    public string question;
    public string answer;
	[XmlAttribute("usado")]
    public bool used;

	public QuestionAndAnswer()
	{
	}

	public QuestionAndAnswer(string _question, string _answer, bool _used)
	{
		this.question = _question;
		this.used = _used;
		this.answer = _answer;
	}
}

[XmlRoot("PreguntasYRespuestas")]
[System.Serializable]
public class ItemDatabase
{
	[XmlArray("QueNosUne"), XmlArrayItem("Pregunta")]
	public List<QuestionAndAnswer> listQueNosUne = new List<QuestionAndAnswer>();
	[XmlArray("Jeroglificos"), XmlArrayItem("Pregunta")]
	public List<QuestionAndAnswer> listJeroglificos = new List<QuestionAndAnswer>();
	[XmlArray("CompletaLaFrase"), XmlArrayItem("Pregunta")]
	public List<QuestionAndAnswer> listCompletaFrase = new List<QuestionAndAnswer>();
}
