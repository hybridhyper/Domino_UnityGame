using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopControlRelax : MonoBehaviour {

	Text textCoins;
	AdManager adManager;

	void Start(){
		textCoins = GameObject.Find("TextoMonedas").GetComponent<Text>();
		adManager = GameObject.Find ("ObjetoSingleton").GetComponent<AdManager> ();
	}

	public void BuyCoins(int monedas){
		int totalCoins = PlayerPrefs.GetInt ("Coins") + monedas;
		PlayerPrefs.SetInt ("Coins", PlayerPrefs.GetInt ("Coins") + monedas);
		textCoins.text = totalCoins + "";
	}

	public void WatchRewardedVideo(){
		adManager.ShowRewardVideoCoins ();
	}
}
