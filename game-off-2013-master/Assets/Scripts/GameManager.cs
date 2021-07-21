using UnityEngine;
using System.Collections;

/*
 * This singleton class will keep references to global objects
 * like the player and his/her statistics.
 */
public class GameManager : Singleton<GameManager>
{
	public Player player;
	public Difficulty difficulty = Difficulty.Easy;
	GameState gameState = GameState.Running;
	public int numPickupsPassed;
	public int numPickupsThisRound;
	Transform playerSpawn;
	public Treadmill treadmill;
	GameObject wildcardRevealerPrefab;
	GUI_WildcardReveal wildcardRevealer;
	public GameObject WildcardRevealGO;
	const int MEDIUM_THRESHOLD = 300; // Number of pickups passed
	const int HARD_THRESHOLD = 2000;
	float timeDeathDelayStarted;
	float deathDelayTime = 1.0f;
	public bool didPlayerReviveThisRun {get; private set;}
	
	public float highestScore {get; private set;}
	HUD hud;
	
	// Prefabs used for generating new sections
	public GameObject redCrystalPrefab;
	public GameObject greenCrystalPrefab;
	public GameObject blueCrystalPrefab;
	public GameObject blockPrefab;
	public GameObject wildcardPrefab;
	
	public bool SAVE_TUTORIAL_COMPLETE { get; private set; }
	public bool SAVE_LASER_LESSON_COMPLETE { get; private set; }
	public bool SAVE_SHIELDS_LESSON_COMPLETE { get; private set; }
	public bool SAVE_SLOW_LESSON_COMPLETE { get; private set; }
	
	public bool DEBUG_MODE = false;
	public bool DEBUG_SKIP_TUTORIAL = false;

	public enum Difficulty
	{
		Easy,
		Hard
	}
	
	enum GameState
	{
		Menu,
		Tutorial,
		Running,
		DeathDelay,
		WildcardReveal,
		Reviving,
		GameOver,
		Store
	}
	
	void Awake ()
	{
		LoadReferencedObjects ();
		LinkSceneObjects ();
		
		// Pretend tutorial is scene if Debug skip is flagged.
		if(DEBUG_SKIP_TUTORIAL) {
			SAVE_TUTORIAL_COMPLETE = true;
		}
		
		GoToMainMenu ();
	}
	
	/*
	 * Loads any prefabs that need to be referenced by the GameManager.
	 */
	void LoadReferencedObjects ()
	{
		wildcardRevealerPrefab = (GameObject)Resources.Load (ObjectNames.GUI_WILDCARD_REVEAL, typeof(GameObject));
	}

	
	/* Search for and assign references to scene objects the GameManager needs to know about.
	 */
	void LinkSceneObjects ()
	{
		player = GameObject.FindGameObjectWithTag (Tags.PLAYER).GetComponent<Player> ();
		hud = GameObject.Find (ObjectNames.HUD).GetComponent<HUD> ();
		// Link Player Spawn
		GameObject foundObject = GameObject.Find (ObjectNames.PLAYER_SPAWN);
		if (foundObject != null) {
			playerSpawn = (Transform)foundObject.transform;
			// Move spawnpoint to the player's starting point.
			playerSpawn.transform.position = player.transform.position;
		}
		
		// Link Treadmill
		foundObject = GameObject.Find (ObjectNames.TREADMILL);
		if (foundObject == null) {
			Debug.LogError ("Cannot find treadmill. This map will not work without a Treadmill object.");
			return;
		}
		treadmill = (Treadmill)foundObject.GetComponent<Treadmill> ();
	}
	
	void Update ()
	{
		if (gameState == GameState.Tutorial) {
			if (Input.GetKeyDown (KeyCode.Space)) {
				GoToRunning (false);
			}
		} else if (gameState == GameState.Running && !IsHard ()) {
			UpdateDifficulty ();
		} else if (gameState == GameState.DeathDelay) {
			if (Time.time > timeDeathDelayStarted + deathDelayTime) {
				if (player.WildcardCount > 0) {
					GoToWildCardState ();
				} else {
					GoToGameOver ();
				}
			}
		}
			
	}
	
	/*
	 * Return true if game is in Menu state.
	 */
	public bool IsOnMenu ()
	{
		return gameState == GameState.Menu;
	}
	
	/*
	 * Return true if the game and all subsequent states are over and we are ready to retry to go to main.
	 */
	public bool IsGameOver ()
	{
		return gameState == GameState.GameOver;
	}
	
	/*
	 * Return true if the game is running.
	 */
	public bool IsPlaying ()
	{
		return gameState == GameState.Running;
	}
	
	/*
	 * Return true if the game should be on the Store screen.
	 */
	public bool IsShopping ()
	{
		return gameState == GameState.Store;
	}
	
		
	/*
	 * Check how far we've traveled. Once we've passed the threshold for hard,
	 * update the difficulty respectively.
	 */
	void UpdateDifficulty ()
	{
		if (treadmill.IsPastHardThreshold ()) {
			difficulty = Difficulty.Hard;
		}
	}
	
	/*
	 * Return true if difficulty is set to easy.
	 */
	public bool IsEasy ()
	{
		return difficulty == Difficulty.Easy;
	}

	/*
	 * Return true if difficulty is set to hard.
	 */
	public bool IsHard ()
	{
		return difficulty == Difficulty.Hard;
	}
	
	/*
	 * Add a point to our score, to be displayed at the end of each round.
	 */
	public void AddPickupPoints (int numPickups)
	{
		if (SAVE_TUTORIAL_COMPLETE) {
			numPickupsThisRound += numPickups;
		}
	}
	
	/*
	 * Starts the infinite running game. Begins with a tutorial if requested.
	 */
	public void StartGame (bool showTutorial)
	{
		InitializeGame ();
		if (showTutorial) {
			GoToTutorial ();
		} else {
			GoToRunning (false);
		}
	}
	
	/*
	 * Does all required data reseting to begin a game.
	 */
	private void InitializeGame ()
	{
		Camera menuCamera = GameObject.Find (ObjectNames.MENU_CAMERA).camera;
		Camera gameCamera = GameObject.Find (ObjectNames.GAME_CAMERA).camera;
		Camera storeCamera = GameObject.Find (ObjectNames.STORE_CAMERA).camera;
		menuCamera.enabled = false;
		gameCamera.enabled = true;
		storeCamera.enabled = false;
		
		numPickupsPassed = 0;
		numPickupsThisRound = 0;
		difficulty = Difficulty.Easy;
		didPlayerReviveThisRun = false;
		
		player.Spawn (playerSpawn.position);
		treadmill.InitializeTreadmill ();
	}
	
	/*
	 * Revives the game in the last configuration.
	 */
	public void RevivePlayer ()
	{
		didPlayerReviveThisRun = true;
		player.Revive (playerSpawn.position);
		GoToReviving ();
	}
	/*
	 * Ends the current run for the player.
	 */
	public void EndRun ()
	{
		GoToDeathDelay ();
	}
	
	/*
	 * Puts the game manager into the state where it waits for the player's death animation.
	 */
	void GoToDeathDelay ()
	{
		gameState = GameState.DeathDelay;
		timeDeathDelayStarted = Time.time;
		treadmill.PauseScrolling ();
	}
	
	/*
	 * Puts the game manager in the state where it awaits player's retry or main menu command.
	 */
	public void GoToGameOver ()
	{
		gameState = GameState.GameOver;
		if (CheckForHighScore (treadmill.distanceTraveled)) {
			hud.DisplayNewHighScore (true);
		}
	}
	
	/*
	 * Puts the game manager in the state where it awaits player's retry or main menu command.
	 */
	public void GoToReviving ()
	{
		gameState = GameState.Reviving;
	}
	
	/*
	 * Called by the Player when he's done reviving.
	 */
	public void OnReviveDone ()
	{
		// This can be called on Retry
		if (gameState != GameState.Tutorial) {
			GoToRunning (true);
		}
	}
	
	/*
	 * Puts the game manager in the state where it reveals the wildcards.
	 */
	void GoToWildCardState ()
	{
		gameState = GameState.WildcardReveal;
		
		// Spawn wildcard revealer
		GameObject spawnedObject = (GameObject)Instantiate (wildcardRevealerPrefab, wildcardRevealerPrefab.transform.position,
				Quaternion.LookRotation (Vector3.forward, Vector3.up));
		wildcardRevealer = spawnedObject.GetComponent<GUI_WildcardReveal> ();
		wildcardRevealer.StartShowing (player.WildcardCount);
	}
	
	/*
	 * Go to the store section of our scene.
	 */
	public void GoToStore ()
	{
		gameState = GameState.Store;
		Camera menuCamera = GameObject.Find (ObjectNames.MENU_CAMERA).camera;
		Camera gameCamera = GameObject.Find (ObjectNames.GAME_CAMERA).camera;
		Camera storeCamera = GameObject.Find (ObjectNames.STORE_CAMERA).camera;
		menuCamera.enabled = false;
		gameCamera.enabled = false;
		storeCamera.enabled = true;
	}
	
	/*
	 * Go to the running state.
	 */
	private void GoToRunning (bool isResuming)
	{
		gameState = GameState.Running;
		if (isResuming) {
			treadmill.UnpauseScrolling ();
		} else {
			treadmill.StartScrolling ();
		}
		player.StartRunning ();
	}
	
	/*
	 * Return to the main menu.
	 */
	public void GoToMainMenu ()
	{
		gameState = GameState.Menu;
		Camera menuCamera = GameObject.Find (ObjectNames.MENU_CAMERA).camera;
		Camera gameCamera = GameObject.Find (ObjectNames.GAME_CAMERA).camera;
		Camera storeCamera = GameObject.Find (ObjectNames.STORE_CAMERA).camera;
		menuCamera.enabled = true;
		storeCamera.enabled = false;
		gameCamera.enabled = false;
	}
	
	/*
	 * Return true and set the new high score if the provided distance
	 * exceeds the saved high score.
	 */
	public bool CheckForHighScore (float distance)
	{
		if (distance > highestScore) {
			highestScore = distance;
			return true;
		}
		return false;
	}
	
	/*
	 * Put the game in the tutorial state
	 */
	void GoToTutorial ()
	{
		gameState = GameState.Tutorial;
		treadmill.ShowTutorial ();
	}
	
	/*
	 * Set the preference for tutorial complete to true.
	 */
	public void MarkTutorialComplete ()
	{
		SAVE_TUTORIAL_COMPLETE = true;
	}
	
	/*
	 * Set the Save booleans when a lesson has been finished.
	 */
	public void MarkLessonComplete (TutorialLesson.Lesson lesson)
	{
		switch (lesson) {
		case TutorialLesson.Lesson.Laser:
			SAVE_LASER_LESSON_COMPLETE = true;
			break;
		case TutorialLesson.Lesson.Shields:
			SAVE_SHIELDS_LESSON_COMPLETE = true;
			break;
		case TutorialLesson.Lesson.Slow:
			SAVE_SLOW_LESSON_COMPLETE = true;
			break;
		}
	}
}
