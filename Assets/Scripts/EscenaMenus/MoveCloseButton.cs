/**
 * 
 * Created by DUWAFARM on 27/10/17.
 * Copyright © 2017. All rights reserved.
 * 
 **/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCloseButton : MonoBehaviour {

	void Start () {
		print (Application.persistentDataPath);
		//La posición del botón que cierra el panel se mueve arriba y a la derecha para colocarse en la esquina.
		this.transform.localPosition = new Vector3 (this.transform.localPosition.x + this.transform.GetComponent<RectTransform> ().rect.width / 3, this.transform.localPosition.y + this.transform.GetComponent<RectTransform> ().rect.height / 3);
	}

}
