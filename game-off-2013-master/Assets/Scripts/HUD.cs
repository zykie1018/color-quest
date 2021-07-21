using UnityEngine;
using System.Collections;

public class HUD : MonoBehaviour
{
	public Player player;
	// End game Text
	public GUIText finalDistanceLabelText;
	public GUIText finalDistanceLabelTextShadow;
	public GUIText finalDistanceText;
	public GUIText finalDistanceTextShadow;
	public GUIText crystalsCollectedLabelText;
	public GUIText crystalsCollectedLabelTextShadow;
	public GUIText crystalsCollectedText;
	public GUIText crystalsCollectedTextShadow;
	public GUIText newHighScoreText;
	public GUIText newHighScoreTextShadow;
	// In Game GUI
	public GameObject redMeter;
	public GameObject greenMeter;
	public GameObject blueMeter;
	public Light lighting;
	public GUIText inGameCrystalsCountText;
	public GUIText inGameCrystalsCountTextShadow;
	public GUIText inGameCrystalsLabel;
	public GUIText inGameCrystalsLabelShadow;
	public GUIText inGameDistanceText;
	public GUIText inGameDistanceTextShadow;
	public GUIText inGameDistanceLabel;
	public GUIText inGameDistanceLabelShadow;
	public GUIText bestDistanceText;
	public GUIText bestDistanceTextShadow;
	public GUIText bestDistanceLabel;
	public GUIText bestDistanceLabelShadow;
	public GUIText storeCrystalsLabel;
	public GUIText storeCrystalsLabelShadow;
	public GUIText debugText;
	public GUIText pressSpaceText;
	public GUIText pressSpaceTextShadow;
	
	public Texture leftArrowTexture;
	public Texture rightArrowTexture;
	
	public GUIStyle areaStyle;
	public GUIStyle redButtonStyle;
	public GUIStyle blueButtonStyle;
	public GUIStyle greenButtonStyle;
	public GUIStyle greyButtonStyle;
	
	const float AREA_WIDTH = 400.0f;
	const float AREA_HEIGHT = 350.0f;
	
	const float LIGHTING_DEFAULT = 0.5f;
	const float LIGHTING_DARK = 0.2f;
	
	Treadmill treadmill;
	
	void Awake ()
	{
		treadmill = GameObject.Find (ObjectNames.TREADMILL).GetComponent<Treadmill> ();
	}
	
	void Update ()
	{
	}
	
	void OnGUI ()
	{
		if (GameManager.Instance.IsOnMenu ()) {
			lighting.intensity = LIGHTING_DEFAULT;
			DisplayMainMenu ();
		} else if (GameManager.Instance.IsGameOver ()) {
			lighting.intensity = LIGHTING_DARK;
			DisplayDeadMenu ();
		} else if (GameManager.Instance.IsShopping ()) {
			SetItemTexts ();
			lighting.intensity = LIGHTING_DEFAULT;
			DisplayStoreMenu ();
		} else {
			lighting.intensity = LIGHTING_DEFAULT;
			DisplayInGameText ();
		}
	}
	
	/*
	 * Display the distance and any other in game text we need to show the player.
	 */
	void DisplayInGameText ()
	{
		EnableInGameGUI (true);
		DisplayNewHighScore (false);

		// Display the tutorial text
		if (!treadmill.IsShowingTutorial ()) {
			SetShadowGUIText (string.Empty, pressSpaceText, pressSpaceTextShadow);
			pressSpaceText.enabled = false;
			pressSpaceTextShadow.enabled = false;
		} else {
			SetShadowGUIText ("Press [Space] to Start", pressSpaceText, pressSpaceTextShadow);
			pressSpaceText.enabled = true;
			pressSpaceTextShadow.enabled = true;
		}
		EnableGameOverGUI (false);
		string bestDistance =  Mathf.RoundToInt (GameManager.Instance.highestScore).ToString ();
		SetShadowGUIText (bestDistance, bestDistanceText, bestDistanceTextShadow);
		string currentDistance = Mathf.RoundToInt (treadmill.distanceTraveled).ToString ();
		SetShadowGUIText (currentDistance, inGameDistanceText, inGameDistanceTextShadow);
		SetShadowGUIText (GameManager.Instance.numPickupsThisRound.ToString (),
			inGameCrystalsCountText, inGameCrystalsCountTextShadow);
		if (GameManager.Instance.DEBUG_MODE) {
			debugText.enabled = true;
			debugText.text = string.Format ("Passed Pigments: {0}\nHealth: {1}\nWildcards: {2}\nDifficulty: {3}",
				GameManager.Instance.numPickupsPassed, player.curHealth, player.WildcardCount,
				GameManager.Instance.difficulty);
		}
	}
	
	/*
	 * Display the menu for when the player is dead. Receive inputs and call
	 * the appropriate GameManager implemented method.
	 */
	void DisplayDeadMenu ()
	{
		EnableInGameGUI (false);
		EnableGameOverGUI (true);

		int finalDistance = Mathf.RoundToInt (treadmill.distanceTraveled);
		SetShadowGUIText (finalDistance + "m", finalDistanceText, finalDistanceTextShadow);
		SetShadowGUIText (GameManager.Instance.numPickupsThisRound.ToString (), crystalsCollectedText,
			crystalsCollectedTextShadow);

		GUILayout.BeginArea (new Rect ((Screen.width - AREA_WIDTH)/2, (Screen.height - AREA_HEIGHT), AREA_WIDTH, AREA_HEIGHT), areaStyle);
		// Push our buttons down to bottom of screen
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();
		// Center our buttons with space on both sides
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		if (GUILayout.Button ("Store", greenButtonStyle)) {
			GameManager.Instance.GoToStore ();
		}
		if (GUILayout.Button ("Retry [Enter]", blueButtonStyle)) {
			//Application.LoadLevel (Application.loadedLevel);
			GameManager.Instance.StartGame (true);
			EnableInGameGUI (true);
		}
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
		GUILayout.EndArea ();
		
		if (Input.GetKeyDown (KeyCode.KeypadEnter) || Input.GetKeyDown (KeyCode.Return)) {
			GameManager.Instance.StartGame (true);
			EnableInGameGUI (true);
		}
	}
	
	/*
	 * Helper method to display money on GUIText object.
	 */
	void PrintMoneyToShopScreen ()
	{
		inGameCrystalsCountText.enabled = true;
		inGameCrystalsCountTextShadow.enabled = true;
		storeCrystalsLabel.enabled = true;
		storeCrystalsLabelShadow.enabled = true;
		SetShadowGUIText (player.money.ToString(), inGameCrystalsCountText, inGameCrystalsCountTextShadow);
	}
	
	/*
	 * Display the menu for when the player is at the store. Receive inputs and call
	 * the appropriate GameManager implemented methods.
	 */
	void DisplayStoreMenu ()
	{
		EnableGameOverGUI (false);
		EnableInGameGUI (false);
		DisplayNewHighScore (false);

		PrintMoneyToShopScreen ();
		Store store = (Store)GameObject.Find (ObjectNames.STORE).GetComponent<Store> ();

		// Add our left and right arrows
		GUILayout.BeginArea (new Rect (0, 0, Screen.width, Screen.height));
		GUILayout.BeginVertical ();
		// Push our arrows to center
		GUILayout.FlexibleSpace ();
		GUILayout.BeginHorizontal ();
		if (store.selectedItem != 0) {
			if (GUILayout.Button (leftArrowTexture, GUIStyle.none)) {
				store.ScrollToPrevious ();
			}
		}
		GUILayout.FlexibleSpace ();
		if (store.selectedItem != store.allItems.Length -1) {
			if (GUILayout.Button (rightArrowTexture, GUIStyle.none)) {
				store.ScrollToNext ();
			}
		}
		GUILayout.EndHorizontal ();
		GUILayout.FlexibleSpace ();
		GUILayout.EndVertical ();
		GUILayout.EndArea ();
		
		GUILayout.BeginArea (new Rect (Screen.width - AREA_WIDTH, (Screen.height - AREA_HEIGHT), AREA_WIDTH, AREA_HEIGHT), areaStyle);
		// Push our buttons to bottom of screen
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();
		// Push our buttons to right of screen.
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		if (store.IsAlreadyPurchased ()) {
			GUILayout.Button ("Already Owned", greyButtonStyle);
		}
		else if (!store.HasEnoughMoney ()) {
			GUILayout.Button ("Not Enough Money", redButtonStyle);
		} else {
			if (GUILayout.Button ("Buy (" + store.GetSelectedItem ().cost + ")", greenButtonStyle)) {
				store.BuyItem ();
			}
		}
		GUILayout.EndHorizontal ();
		// Push more buttons to the right of the screen
		GUILayout.BeginHorizontal ();
		GUILayout.FlexibleSpace ();
		if (GUILayout.Button ("Play", blueButtonStyle)) {
			GameManager.Instance.StartGame (true);
		}
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
		GUILayout.EndArea ();
	}
	
	void SetItemTexts ()
	{
		Store store = (Store)GameObject.Find (ObjectNames.STORE).GetComponent<Store> ();
		GameObject[] itemObjs = GameObject.FindGameObjectsWithTag (Tags.ITEM);
		foreach (GameObject obj in itemObjs) {
			Item item = obj.GetComponent<Item> ();
			TextMesh[] itemTexts = obj.GetComponentsInChildren<TextMesh> ();

			string displayText = "{0}\nCost: {1}";
			if (store.IsAlreadyPurchased ()) {
				displayText = "{0}\n(Already Owned)";
			} else if (item.type == Item.Type.consumable) {
				displayText = "{0}\n(Consumable)\nCost: {1}";
			}
			itemTexts[0].text = string.Format (displayText, item.displayName, item.cost);
			itemTexts[1].text = string.Format (displayText, item.displayName, item.cost);
		}
	}
	
	/*
	 * Show the buttons for the main menu and include logic when buttons
	 * are pressed.
	 */
	void DisplayMainMenu ()
	{
		EnableInGameGUI (false);
		EnableGameOverGUI (false);
		DisplayNewHighScore (false);

		GUILayout.BeginArea (new Rect ((Screen.width - AREA_WIDTH)/2, (Screen.height - AREA_HEIGHT), AREA_WIDTH, AREA_HEIGHT), areaStyle);
		// Push buttons to the bottom of the screen
		GUILayout.BeginVertical ();
		GUILayout.FlexibleSpace ();
		GUILayout.BeginHorizontal ();
		// Push buttons to the center of the screen
		GUILayout.FlexibleSpace ();
		if (GUILayout.Button ("Store", greenButtonStyle)) {
			GameManager.Instance.GoToStore ();
		}
		if (GUILayout.Button ("Start Game [ENTER]", blueButtonStyle)) {
			GameManager.Instance.StartGame (true);
		}
		// Push more buttons to the center of the screen
		GUILayout.FlexibleSpace ();
		GUILayout.EndHorizontal ();
		GUILayout.EndVertical ();
		GUILayout.EndArea ();
		
		if (Input.GetKeyDown (KeyCode.KeypadEnter) || Input.GetKeyDown (KeyCode.Return)) {
			GameManager.Instance.StartGame (true);
		}
	}
	
	/*
	 * Helper method to enable or disable in game text.
	 */
	void EnableInGameGUI (bool enable)
	{
		redMeter.SetActive (enable);
		greenMeter.SetActive (enable);
		blueMeter.SetActive (enable);
		pressSpaceText.enabled = enable;
		pressSpaceTextShadow.enabled = enable;
		inGameCrystalsCountText.enabled = enable;
		inGameCrystalsCountTextShadow.enabled = enable;
		inGameDistanceText.enabled = enable;
		inGameDistanceTextShadow.enabled = enable;
		bestDistanceText.enabled = enable;
		bestDistanceTextShadow.enabled = enable;
		inGameCrystalsLabel.enabled = enable;
		inGameCrystalsLabelShadow.enabled = enable;
		inGameDistanceLabel.enabled = enable;
		inGameDistanceLabelShadow.enabled = enable;
		bestDistanceLabel.enabled = enable;
		bestDistanceLabelShadow.enabled = enable;
		// Turn off store label for crystals
		storeCrystalsLabel.enabled = false;
		storeCrystalsLabelShadow.enabled = false;
	}
	
	/*
	 * Helper method to enable or disable game over text elements.
	 */
	void EnableGameOverGUI (bool enable)
	{
		finalDistanceLabelText.enabled = enable;
		finalDistanceLabelTextShadow.enabled = enable;
		finalDistanceText.enabled = enable;
		finalDistanceTextShadow.enabled = enable;
		crystalsCollectedText.enabled = enable;
		crystalsCollectedTextShadow.enabled = enable;
		crystalsCollectedLabelText.enabled = enable;
		crystalsCollectedLabelTextShadow.enabled = enable;
	}
	
	/*
	 * Show/hide the New High Score! label on the final distance screen.
	 */
	public void DisplayNewHighScore (bool display)
	{
		newHighScoreText.enabled = display;
		newHighScoreTextShadow.enabled = display;
	}
	
	/*
	 * Helper method to set provided GUITText and its shadow to a string.
	 */
	void SetShadowGUIText (string newText, GUIText guiText, GUIText guiTextShadow)
	{
		guiText.text = newText;
		guiTextShadow.text = newText;
	}
}
