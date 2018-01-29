using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using System;
using UnityEngine.UI;

public class AdManager : MonoBehaviour {
    public static AdManager instance { get; set; }
	InterstitialAd interstitial;
	BannerView bannerView;
	RewardBasedVideoAd rewardVideoLifes;
	RewardBasedVideoAd rewardVideoCoins;
	string bannerID;
	string interstitialID;
	string rewardID;
	//BANNER PRUEBA: ca-app-pub-3940256099942544/6300978111
	//INTERTICIAL PRUEBA: ca-app-pub-3940256099942544/1033173712
	//BANNER IOS: ca-app-pub-1913552533139737/6284321189
	//INTERSTICIAL IOS: ca-app-pub-1913552533139737/2657380445
	//BANNER ANDROID: ca-app-pub-1913552533139737/9129142221
	//INTERSTICIAL ANDROID: ca-app-pub-1913552533139737/4806753839

	void Start()
    {
        instance = this;
        DontDestroyOnLoad(this.gameObject);

        if (Application.platform == RuntimePlatform.Android){
			bannerID = "ca-app-pub-3940256099942544/6300978111";
			interstitialID = "ca-app-pub-3940256099942544/1033173712";
			rewardID = "ca-app-pub-3940256099942544/5224354917";
        }else if (Application.platform == RuntimePlatform.IPhonePlayer){
			bannerID = "ca-app-pub-3940256099942544/6300978111";
			interstitialID = "ca-app-pub-3940256099942544/1033173712";
			rewardID = "ca-app-pub-3940256099942544/1712485313";
        }else{
            print("No play ads on editor");
        }
    }

	public void RequestBanner()
	{
		#if UNITY_EDITOR
		string adUnitId = "unused";
		#elif UNITY_ANDROID
		string adUnitId = bannerID;
		#elif UNITY_IPHONE
		string adUnitId = bannerID;
		#else
		string adUnitId = "unexpected_platform";
		#endif

		// Create a 320x50 banner at the top of the screen.
		bannerView = new BannerView(adUnitId, AdSize.Banner, AdPosition.Bottom);
		// Create an empty ad request.
		AdRequest request = new AdRequest.Builder().Build();
		// Load the banner with the request.
		bannerView.LoadAd(request);
		bannerView.OnAdLoaded += HandleOnAdLoaded;
	}

    public InterstitialAd GetInterstitial() {
        return interstitial;
    }

	public void RequestInterstitial(){
		#if UNITY_EDITOR
		string adUnitId = "unused";
		#elif UNITY_ANDROID
		string adUnitId = interstitialID;
		#elif UNITY_IPHONE
		string adUnitId = interstitialID;
		#else
		string adUnitId = "unexpected_platform";
		#endif

		//Initialize an InterstitialAd
		interstitial = new InterstitialAd (adUnitId);
		// Create an empty ad request
		AdRequest request = new AdRequest.Builder ().Build ();
		// Load the interstitial  with the request
		interstitial.LoadAd (request);
		interstitial.OnAdClosed += DestroyAds;
	}

	public void RequestRewardVideoLifes(){
		#if UNITY_EDITOR
		string adUnitId = "unused";
		#elif UNITY_ANDROID
		string adUnitId = rewardID;
		#elif UNITY_IPHONE
		string adUnitId = rewardID;
		#else
		string adUnitId = "unexpected_platform";
		#endif

		rewardVideoLifes = RewardBasedVideoAd.Instance;

		AdRequest request = new AdRequest.Builder().Build ();
		rewardVideoLifes.LoadAd (request, adUnitId);
		rewardVideoLifes.OnAdRewarded += HandleRewardLifes;
		rewardVideoLifes.OnAdClosed += DestroyRewardVideoLifes;
		GameObject.Find ("TituloTienda").GetComponent<Text> ().text = "Request";
	}

	public void RequestRewardVideoCoins(){
		#if UNITY_EDITOR
		string adUnitId = "unused";
		#elif UNITY_ANDROID
		string adUnitId = rewardID;
		#elif UNITY_IPHONE
		string adUnitId = rewardID;
		#else
		string adUnitId = "unexpected_platform";
		#endif

		rewardVideoCoins = RewardBasedVideoAd.Instance;

		AdRequest request = new AdRequest.Builder().Build ();
		rewardVideoCoins.LoadAd (request, adUnitId);
		rewardVideoCoins.OnAdRewarded += HandleRewardCoins;
		rewardVideoCoins.OnAdClosed += DestroyRewardVideoCoins;
	}

	public void HandleOnAdLoaded(object sender, EventArgs args){
		HideBanner ();
	}

	public void ShowBanner(){
		bannerView.Show ();
	}

	public void ShowInterstitial(){
		if (interstitial.IsLoaded()) {
			interstitial.Show ();
		}
	}

	public void ShowRewardVideoLifes(){
		GameObject.Find ("TituloTienda").GetComponent<Text> ().text = "Show";
		if (rewardVideoLifes.IsLoaded()) {
			GameObject.Find ("TituloTienda").GetComponent<Text> ().text = "Loaded";
			rewardVideoLifes.Show ();
		}
	}

	void HandleRewardLifes(object sender, Reward reward){
		GameObject.Find ("TituloTienda").GetComponent<Text> ().text = "Rewarded";
		print ("User rewarded with 10 lifes");
	}

	public void ShowRewardVideoCoins(){
        if (rewardVideoCoins.IsLoaded()) {
            rewardVideoCoins.Show();
        } else {
            AutoFade.LoadLevel("EscenaInicio", 0.2f, 0.2f, Color.white);
    }
	}

	 void HandleRewardCoins(object sender, Reward reward){
		print ("User rewarded with 10 coins");
	}

	public void HideBanner(){
		bannerView.Hide ();
	}

    void DestroyAds(object sender, EventArgs args) {
        interstitial.Destroy();
        bannerView.Destroy();
        AutoFade.LoadLevel("EscenaInicio", 0.2f, 0.2f, Color.white);
        interstitial.OnAdClosed -= DestroyAds;
        bannerView.OnAdLoaded -= HandleOnAdLoaded;
    }

    void DestroyRewardVideoLifes(object sender, EventArgs args) {
        rewardVideoLifes.OnAdRewarded -= HandleRewardLifes;
        rewardVideoLifes.OnAdClosed -= DestroyRewardVideoLifes;
    }

    void DestroyRewardVideoCoins(object sender, EventArgs args) {
        rewardVideoCoins.OnAdRewarded -= HandleRewardCoins;
        rewardVideoCoins.OnAdClosed -= DestroyRewardVideoCoins;
    }

    void OnApplicationPause(bool pause) {
        if (bannerView != null)
            bannerView.Destroy();
    }
}
