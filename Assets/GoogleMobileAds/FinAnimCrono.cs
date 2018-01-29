using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FinAnimCrono : MonoBehaviour {

    public void AnimCronoEnded() {
        GameObject.Find("ControladorEstados").GetComponent<ControladorEstados>().ShowInitial();
    }
	
}
