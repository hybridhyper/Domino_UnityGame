using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Purchasing;

public class Purchaser : MonoBehaviour, IStoreListener {
    
	static IStoreController m_StoreController;	//The Unity purchasing system
	static IExtensionProvider m_StoreExtensionProvider; //The store-specific Purchasing subsystems

	// Product identifiers for all products capable of being purchased:
	// "convenience" general identifiers for use with Purchasing, and their store-specific identifier
	// counterparts for use with and outside of Unity Purchasing. Define store-specific indetifiers
	// also on each platform's publisher dashboard (iTunes Connect, Google Play Developer Console, etc.)

	// General product indetifiers for the consumable, non-consumable, and subscription products.
	// Use these handles in the code to reference wich product to purchase. Also use these values
	// when defining the Product Identifiers on the store. Except, for the illustration purposes, the 
	// kProductIDSubscription - it has custom Apple and Google identifiers. We declare their store-
	// specific mapping to Unity Purchasing's AddProduct, below.

	//Estos son los identificadores de cada producto
	public static string k50Coins = "50coins";
	public static string k100Coins = "100coins";
	public static string k500Coins = "500coins";
	public static string k1500Coins = "1500coins";
	public static string k30000Coins = "30000coins";
	public static string k50Lifes = "50lifes";
	public static string k100Lifes = "100lifes";
	public static string k500Lifes = "500lifes";
	public static string k1500Lifes = "1500lifes";
	public static string k30000Lifes = "30000lifes";

	void Start () {
		// If we haven't set up the Unity Purchasing reference
		if (m_StoreController == null) {
			// Begin to configure our connection to Purchasing
			InitializePurchasing();
		}
	}

	public void InitializePurchasing(){
		// If we have already connected to Purchasing ...
		if (IsInitialized ()) {
			// ... we are done here.
			return;
		}

		// Create a builder, first passing in a suite of Unity provided stores.
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

		// Add a product to sell / respore by way of its identifier, associating the general identifier
		// with its store-specific identifiers.
		builder.AddProduct(k50Coins, ProductType.Consumable);
		builder.AddProduct(k100Coins, ProductType.Consumable);
		builder.AddProduct(k500Coins, ProductType.Consumable);
		builder.AddProduct(k1500Coins, ProductType.Consumable);
		builder.AddProduct(k30000Coins, ProductType.Consumable);
		builder.AddProduct(k50Lifes, ProductType.Consumable);
		builder.AddProduct(k100Lifes, ProductType.Consumable);
		builder.AddProduct(k500Lifes, ProductType.Consumable);
		builder.AddProduct(k1500Lifes, ProductType.Consumable);
		builder.AddProduct(k30000Lifes, ProductType.Consumable);


		// Kick off the remainder of the set-up with an asynchrounous call, passing the configuration
		// and this class' instance. Expect a response either in OnInitialized or OnInitializedFailed.
		UnityPurchasing.Initialize(this, builder);
	}

	bool IsInitialized(){
		// Only say we are initialized if both the Purchasing references are set.
		return m_StoreController != null && m_StoreExtensionProvider != null;
	}

	public void Buy50Coins(){
		BuyProductID(k50Coins);
	}

	public void Buy100Coins(){
		BuyProductID(k100Coins);
	}

	public void Buy500Coins(){
		BuyProductID(k500Coins);
	}

	public void Buy1500Coins(){
		BuyProductID(k1500Coins);
	}

	public void Buy30000Coins(){
		BuyProductID(k30000Coins);
	}

	void BuyProductID(string productId){
		// If Purchasing has been initialized ...
		if (IsInitialized ()) {
			// ... look up the Product reference with the general product identifier and the Purchasing
			// system's products collection.
			Product product = m_StoreController.products.WithID (productId);

			// If the look up found a product for this edvice's store and that product is ready to be sold.
			if (product != null && product.availableToPurchase) {
				Debug.Log (string.Format ("Purchasing product asynchronously: '{0}'", product.definition.id));
				// ... buy the product. Expect a response either through ProcessPurchase or OnPruchaseFailed
				// asynchronously.
				m_StoreController.InitiatePurchase (product);
			}
			//Otherwise
			else {
				// ... report the product lol-up failure situation
				Debug.Log ("BuyProductID: FAIL. Not purchasing product, either isnot found or is not available for purchase");
			}
		}
		// Otherwise ...
		else {
			// ... report the fact Purchasing has not succeded initializing yet, consider waiting longer or
			// retying initialization.
			Debug.Log("BuyProductID FAIL. Not initialized.");
		}
	}


	// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google.
	// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
	public void RestorePurchases(){
		// If Purchasing has not yet been set up ...
		if (!IsInitialized ()) {
			// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
			Debug.Log("RestorePurchases FAIL. Not initialized.");
			return;
		}

		// If we are running on an Apple device ...
		if (Application.platform == RuntimePlatform.IPhonePlayer ||
		    Application.platform == RuntimePlatform.OSXPlayer) {
			// ... begin restoring purchases
			Debug.Log ("ResorePurchases started ...");

			// Fetch the Apple store-specific subsystem.
			var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions> ();
			// Begin the asynchronous process of restoring purchases. Expect a confirmation repsonse in
			// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
			apple.RestoreTransactions ((result) => {
				// The first phase of restoration. If no more responses are received on ProcessPurchase then
				// no purchases are available to be restored.
				Debug.Log ("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
			});
		}
		// Otherwise ...
		else {
			// We are not running on an Apple device. No work is necessary to restore purchases.
			Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
		}
	}

	//
	// --- IStoreListener
	//

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions){
		// Purchasing has succeeded initializing. Collect our Purchasing references.
		Debug.Log("OnInitialized: PASS");

		// Overall Purchasing system, configured with products for this application.
		m_StoreController = controller;
		// Store specific subsystems, for accessing device-specific store features.
		m_StoreExtensionProvider = extensions;
	}

	public void OnInitializeFailed(InitializationFailureReason error){
		// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
		Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
	}

	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args){
		// A consumable has been purchased by this user.
		//Comprobamos qué consumible ha comprado y se lo añadimos a su cantidad actual
		if (string.Equals (args.purchasedProduct.definition.id, k50Coins, System.StringComparison.Ordinal)) {
			PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 50);
		}
		else if (string.Equals (args.purchasedProduct.definition.id, k100Coins, System.StringComparison.Ordinal)) {
			PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 100);
		}
		else if (string.Equals (args.purchasedProduct.definition.id, k500Coins, System.StringComparison.Ordinal)) {
			PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 500);
		}
		else if (string.Equals (args.purchasedProduct.definition.id, k1500Coins, System.StringComparison.Ordinal)) {
			PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 1500);
		}
		else if (string.Equals (args.purchasedProduct.definition.id, k30000Coins, System.StringComparison.Ordinal)) {
			PlayerPrefs.SetInt("Coins", PlayerPrefs.GetInt("Coins") + 30000);
		}
		else if (string.Equals (args.purchasedProduct.definition.id, k50Lifes, System.StringComparison.Ordinal)) {
			PlayerPrefs.SetInt("Lifes", PlayerPrefs.GetInt("Lifes") + 50);
		}
		else if (string.Equals (args.purchasedProduct.definition.id, k100Lifes, System.StringComparison.Ordinal)) {
			PlayerPrefs.SetInt("Lifes", PlayerPrefs.GetInt("Lifes") + 100);
		}
		else if (string.Equals (args.purchasedProduct.definition.id, k500Lifes, System.StringComparison.Ordinal)) {
			PlayerPrefs.SetInt("Lifes", PlayerPrefs.GetInt("Lifes") + 500);
		}
		else if (string.Equals (args.purchasedProduct.definition.id, k1500Lifes, System.StringComparison.Ordinal)) {
			PlayerPrefs.SetInt("Lifes", PlayerPrefs.GetInt("Lifes") + 1500);
		}
		else if (string.Equals (args.purchasedProduct.definition.id, k30000Lifes, System.StringComparison.Ordinal)) {
			PlayerPrefs.SetInt("Lifes", PlayerPrefs.GetInt("Lifes") + 30000);
		}

		print ("AHORA TENGO: " + PlayerPrefs.GetInt ("Coins"));
		// Return a flag inidcating wether this product has completely been received, or if the application needs
		// to be reminded of this purchase at next app launch. Use PurchaseProcessingResult.Pending when still
		// saving purchased products to the cloud, and when that save is delayed.
		return PurchaseProcessingResult.Complete;
	}

	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason){
		// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing
		// this reason with the user to guide their troubleshooting actions.
		Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
	}
}
