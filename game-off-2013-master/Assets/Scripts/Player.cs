using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Player : MonoBehaviour
{
	public int money;
	public int curHealth;
	const int BASE_HEALTH = 1;
	public int curShields;

	public bool IsDead { get; private set; }

	bool isWaitingToStart;
	bool isReviving;
	float invulnerabilityTime;
	RBTimer reviveTimeout;
	
	// Abilities
	public BluePower bluePower;
	bool isUsingSlowdown;
	float slowDownStrength = 20f;
	float slowDownMovespeed = DEFAULT_MOVESPEED + 5f;
	const float UPGRADED_MOVESPEED = DEFAULT_MOVESPEED + 10f;
	bool isBoosting;
	bool isBoostBraking;
	RBTimer boostTimer;
	RBTimer boostBrakeTimer;

	public RedPower redPower;
	public GreenPower greenPower;
	bool isShieldUp;
	int shieldStrength = 4;
	const int UPGRADED_SHIELD_STRENGTH = 6;
	public RGB playerRGB;
	public float MAGNET_DIST = 6.0f;
	Inventory inventory;

	public int WildcardCount { get; private set; }
	
	public List<GameObject> pickups;
	const float POWER_UNIT = 1.0f;

	public AudioClip explosionSound;
	public AudioClip blockHitSound;
	public AudioClip wildcardSound;
	public AudioClip pickupSound00;
	public AudioClip pickupSound01;
	public AudioClip deathSound;
	public AudioClip slowDownSound;
	public AudioClip speedUpSound;
	public AudioClip shieldUpSound;
	public AudioClip shieldHitSound;
	public AudioClip shieldWeakSound;
	public AudioClip shieldDownSound;
	public AudioClip laserSound;
	Animation shieldAnimation;
	Treadmill treadmill;
	GameObject playerGeo;
	GameObject nodeLaser;
	GameObject shieldObject;
	GameObject boostFX;
	public GameObject boostFXPrefab;
	public GameObject laserBeamFX;
	public GameObject wildcardFX;
	public GameObject reviveFX;
	public GameObject boostTextFX;
	public GameObject superBoostTextFX;
	public GameObject explosionFX;
	float worldZClamp;
	float worldYClamp;
	float movespeed;
	const float DEFAULT_MOVESPEED = 20f;
	
	public Vector3 perceivedVelocity;
	
	#region #1 Awake and Update
	void Awake ()
	{
		LinkSceneReferences ();
		LinkNodeReferences ();
		LinkComponents ();
		
		// Set our health and powers
		curHealth = BASE_HEALTH;
		movespeed = DEFAULT_MOVESPEED;

		// Remember their initial Y and Z position and keep them there forever.
		worldZClamp = transform.position.z;
		worldYClamp = transform.position.y;
		
		// Render initial color
		RenderCurrentColor ();
		
		// Disable shield FX
		shieldObject.SetActive (false);
	}

	/*
	 * Sets references to our "node" empty game objects which are used for position and rotation values of the player.
	 */
	void LinkNodeReferences ()
	{
		playerGeo = transform.FindChild ("PlayerGeo").gameObject;
		
		nodeLaser = playerGeo.transform.FindChild ("node_laser").gameObject;
		shieldObject = transform.FindChild (ObjectNames.SHIELD).gameObject;
		shieldAnimation = shieldObject.GetComponent<Animation> ();
	}
	
	/*
	 * Sets references to components on this Player game object or one of its children
	 */
	void LinkComponents ()
	{
		bluePower = (BluePower)GetComponent<BluePower> ();
		redPower = (RedPower)GetComponent<RedPower> ();
		greenPower = (GreenPower)GetComponent<GreenPower> ();
		playerRGB = (RGB)playerGeo.GetComponent<RGB> ();
		inventory = (Inventory)GetComponent<Inventory> ();
	}
	
	/*
	 * Finds and sets references to objects in the scene.
	 */
	void LinkSceneReferences ()
	{
		treadmill = GameManager.Instance.treadmill;
	}
	
	void Update ()
	{
		// Waiting to be told by GameManager to start running
		if (isWaitingToStart) {
			return;
		}
		
		// Reduce invulnerability frames until none remain.
		invulnerabilityTime = (int) Mathf.Max (invulnerabilityTime - Time.deltaTime, 0.0f);
		
		bool isAlive = !IsDead;
		if (isAlive) {
			
			CheckBoostingFinish ();
			
			// Update boosting speed
			if (isBoosting) {
				SetRunspeed (15.0f, treadmill.scrollspeed);
			} else {
				// When not boosting, match with treadmill.
				if (isUsingSlowdown) {
					SetRunspeed (slowDownMovespeed, treadmill.scrollspeed);
				} else {
					MatchSpeedToTreadmill ();
				}
			}
	
			TryMove ();
			TryActivateAbilities ();
	
			CheckShieldTimeout ();
			CheckSlowDownTimeout ();
			
			UpdateYZPosition ();
			
			// If no colors are active, go neutral
			if (!bluePower.IsPowerActive () && !redPower.IsPowerActive () && !greenPower.IsPowerActive ()) {
				ChangeColors (ColorWheel.neutral);
			}
			RenderCurrentColor ();
			PullNearbyPickups ();
		} else if (isReviving) {
			// Check if we need to force restore from ragdoll due to timeout
			if (reviveTimeout.IsTimeUp ()) {
				OnBodyRevived ();
			}
		}
	}
	
	/*
	 * Adjusts the Player's Z position to reflect his current state.
	 */
	void UpdateYZPosition () {
		// When boosting, we go up the board a bit.
		if (isBoosting) {
			if (isBoostBraking) {
				LerpZToClamp ();
			} else {
				LerpZToBoostPosition ();
			} 
		} else {
			ClampToWorldYZ (worldYClamp, worldZClamp);
		}
	}
	
	/*
	 * Adjusts the player's movespeed left and right to keep up with the treadmill. This allows us to
	 * make challenges that stay consistently possible for the player as the board speeds up.
	 */
	void MatchSpeedToTreadmill ()
	{
		SetRunspeed (treadmill.scrollspeed, treadmill.scrollspeed);
	}
	
	/*
	 * Sets the character's left and right movespeed as well as forward apparent "movespeed"
	 */
	void SetRunspeed (float speedLeftRight, float speedForward)
	{
		movespeed = speedLeftRight;
		
		// Set animation playback speed on animation to match new movespeed
		float ANIM_NORMAL_RUNSPEED = 30.0f;
		playerGeo.animation ["pigment_run"].speed = speedForward / ANIM_NORMAL_RUNSPEED;
	}
		
	/*
	 * Refresh the current color on the character
	 */
	void RenderCurrentColor ()
	{
		PigmentBody body = (PigmentBody)playerGeo.GetComponent<PigmentBody> ();
		body.SetColor (playerRGB.color);
	}
	
	/*
	 * Sets the character's position to the specified world Y and worldZ, preventing
	 * him from shifting.
	 */
	void ClampToWorldYZ (float worldY, float worldZ)
	{
		transform.position = new Vector3 (transform.position.x, worldY, worldZ);
	}
	
	/*
	 * Lerps the Player into the boost position
	 */
	void LerpZToBoostPosition ()
	{
		transform.position = Vector3.Lerp (transform.position, new Vector3 (transform.position.x, 
			transform.position.y, -10.0f), 3.5f * Time.deltaTime);
	}
	
	/*
	 * Moves the player back from boost position into normal clamp position
	 */
	void LerpZToClamp ()
	{
		transform.position = Vector3.MoveTowards (transform.position, new Vector3 (transform.position.x, 
			transform.position.y, worldZClamp), 0.5f);
	}
	#endregion
	
	#region #2 Input Tries
	/*
	 * Polls input and moves the character accordingly
	 */
	void TryMove ()
	{
		float direction = Input.GetAxis ("Horizontal");
		Move (new Vector3 (direction, 0.0f, 0.0f), movespeed);
		// Translate in place helps hit trigger colliders
		if (direction == 0) {
			transform.Translate (0, 0, 0);
		}
	}
	
	/*
	 * Logic for using abilities. First try to activate the power that
	 * the player has pushed the key for. Then try and use the ability
	 * if it is off of cooldown.
	 */
	void TryActivateAbilities ()
	{
		// Don't let them activate their abilities while boosting
		if (isBoosting) {
			return;
		}
		
		if (Input.GetKeyDown ("j")) {
			if (redPower.IsChargedAndReady ()) {
				SetActivePower (redPower, greenPower, bluePower);
				ChangeColors (ColorWheel.red);
			}
			if (redPower.IsPowerActive () && !redPower.AbilityOnCooldown ()) {
				Laser ();
			}
		} else if (Input.GetKeyDown ("k")) {
			if (greenPower.IsChargedAndReady ()) {
				SetActivePower (greenPower, redPower, bluePower);
				ChangeColors (ColorWheel.green);
				RaiseShield ();
			}
		} else if (Input.GetKeyDown ("l")) {
			if (bluePower.IsChargedAndReady ()) {
				SetActivePower (bluePower, redPower, greenPower);
				SlowDown ();
				ChangeColors (ColorWheel.blue);
			}
		}
	}
	
	/*
	 * Helper method to set the active power and turn off others if in use.
	 */
	void SetActivePower (Power activePower, Power inactivePower1, Power inactivePower2)
	{
		activePower.ActivatePower ();
	}
	
	/*
	 * Helper method to deactivate power only if it's in use.
	 */
	void DeactivatePowerIfActive (Power power)
	{
		if (power.IsPowerActive ()) {
			power.ResetPower ();
		}
	}

	#endregion

	#region #3 Player and Block interaction
	/*
	 * Store a reference to all pickups the player could encounter.
	 */
	public void RememberPickup (GameObject pickup)
	{
		pickups.Add (pickup);
	}
	
	/*
	 * Remove the reference to a pickup in play.
	 */
	public void ForgetPickup (GameObject pickup)
	{
		pickups.Remove (pickup);
	}
	
	/*
	 * Pull in the pickups that are near the player. Adjust the distance if
	 * the player has a magnet.
	 */
	public void PullNearbyPickups ()
	{
		float extraPullPadding = 1.0f;
		float noMagnetDist = transform.collider.bounds.extents.x + extraPullPadding;
		float pullDistance;
		foreach (GameObject pickup in pickups) {
			pullDistance = noMagnetDist;
			Pickup pickupScript = pickup.GetComponent<Pickup> ();
			if (pickup.GetComponent<RGB> () != null) {
				if (HasMagnetForColor (pickup.GetComponent<RGB> ().color)) {
					pullDistance = MAGNET_DIST;
				}
			}
			if (Vector3.SqrMagnitude (pickup.transform.position - transform.position) <= (Mathf.Pow (pullDistance, 2) + 
				pickupScript.size)) {
				pickupScript.StartCollecting (gameObject);
			}
		}
	}
	
	/*
	 * When a pickup is gathered, increment stats for the player and game. Play
	 * the sounds needed and charge the player's meters.
	 */
	public void CollectPickup (GameObject pickup)
	{
		if (pickup.GetComponent<CrystalPickup> () != null) {
			RGB pickupRGB = pickup.GetComponent<RGB> ();
			int randSound = Random.Range (0,2);
			if (randSound == 1) {
				audio.PlayOneShot (pickupSound01);
			} else {
				audio.PlayOneShot (pickupSound00);
			}
			Power powerToCharge = GetPowerForColor (pickupRGB);
			if (powerToCharge.IsChargedAndReady ()) {
				// Logic for spillover goes here
			} else {
				powerToCharge.AddPower (POWER_UNIT);
			}
			GameManager.Instance.AddPickupPoints (1);
			// Add up our money (only if tutorial challenge is over)
			if (GameManager.Instance.SAVE_TUTORIAL_COMPLETE) {
				AddMoney (1);
			}
			
			ForgetPickup (pickup);
		} else if (pickup.CompareTag (Tags.WILDCARD)) {
			AwardWildcard ();
			audio.PlayOneShot (wildcardSound);
		} else {
			Debug.Log ("Player encountered unknown Pickup! Handle this with a new tag on the pickup.");
		}
	}
	
	/*
	 * When a player collides with a black block, take damage.
	 */
	public void CollideWithBlock ()
	{
		// While boosting go straight through blocks like it's no thang.
		if (isBoosting) {
			audio.PlayOneShot(blockHitSound);
			return;
		}
		
		// Don't let the player take double damage just for hitting two blocks at their seam.
		if (invulnerabilityTime > 0) {
			return;
		}
		invulnerabilityTime = 0.3f;
		
		if (greenPower.IsPowerActive ()) {
			TakeShieldHit ();
		} else {
			// Subtract the health
			curHealth = curHealth - 1;
		}
		
		// Handle death
		if (curHealth <= 0) {
			Die ();
		}
	}
	
	/*
	 * Increments the number of wildcards the player has collected.
	 */
	public void AwardWildcard ()
	{
		WildcardCount++;
		GameObject fx = (GameObject)Instantiate (wildcardFX, transform.position + wildcardFX.transform.position,
			wildcardFX.transform.rotation);
		fx.transform.parent = transform;
		Destroy (fx, 1.5f);
	}
	
	/*
	 * Map our player's power bars to the color passed in by returning
	 * the power associated with the provided color.
	 */
	Power GetPowerForColor (RGB rgb)
	{
		ColorWheel color = rgb.color;
		Power returnPower = null;
		switch (color) {
		case ColorWheel.blue:
			returnPower = bluePower;
			break;
		case ColorWheel.red:
			returnPower = redPower;
			break;
		case ColorWheel.green:
			returnPower = greenPower;
			break;
		}
		return returnPower;
	}
	
	/*
	 * Kill the player
	 */
	public void Die ()
	{
		IsDead = true;
		collider.enabled = false;
		
		// Change the background to neutral
		ChangeColors (ColorWheel.neutral);
		redPower.ResetPower ();
		greenPower.ResetPower ();
		bluePower.ResetPower ();
		
		GameManager.Instance.EndRun ();
		audio.PlayOneShot (deathSound);
		PigmentBody body = (PigmentBody)playerGeo.GetComponent<PigmentBody> ();
		body.ReplaceWithRagdoll ();
		
		// pause running animation
		playerGeo.animation["pigment_run"].speed = 0.0f;
		
	}
	
	/*
	 * Spawns the player at the specified position
	 */
	public void Spawn (Vector3 spawnPosition)
	{
		isWaitingToStart = true;
		IsDead = false;
		collider.enabled = true;
		
		PigmentBody body = (PigmentBody)playerGeo.GetComponent<PigmentBody> ();
		body.RestoreBody ();
		body.SetColor(ColorWheel.neutral);
		
		InitializeStatsOnSpawn ();
		
		// Snap to spawn position
		transform.position = spawnPosition;
	}
	
	/*
	 * Revives the player from being ragdoll
	 */
	public void Revive (Vector3 revivePosition)
	{
		isWaitingToStart = true;
		InitializeStatsOnRevive ();
		
		// Restore ragdolled limbs
		PigmentBody body = (PigmentBody)playerGeo.GetComponent<PigmentBody> ();
		body.StartReviving ();
		
		// Snap to revive position
		transform.position = revivePosition;
		
		isReviving = true;
		
		reviveTimeout = new RBTimer ();
		reviveTimeout.StartTimer (2.0f);
		
		// Spawn the Revive FX at the Ragdoll's location.
		GameObject ragdollTorso = body.GetRagdollBody ();
		Vector3 torsoXZ = new Vector3( ragdollTorso.transform.position.x, 0.0f, ragdollTorso.transform.position.z);
		Vector3 fxPosition = torsoXZ + new Vector3(0.0f, reviveFX.transform.position.y, reviveFX.transform.position.z);
		GameObject fx = (GameObject)Instantiate (reviveFX, fxPosition, reviveFX.transform.rotation);
		fx.transform.parent = transform;
		Destroy (fx, 1.5f);
	}
	
	/*
	 * Called when the ragdoll is done bringing the body parts back together.
	 */
	public void OnBodyRevived ()
	{
		reviveTimeout.StopTimer ();
		isReviving = false;
		IsDead = false;
		collider.enabled = true;
		
		PigmentBody body = (PigmentBody)playerGeo.GetComponent<PigmentBody> ();
		body.RestoreBody ();
		
		DoBlockClearExplosion ();
		
		GameManager.Instance.OnReviveDone ();
	}
	
	/*
	 * Explode blocks out of the way so the player doesn't have unfair deaths.
	 */
	public void DoBlockClearExplosion ()
	{
		// Do an explosion to clear blocks out of the way
		float explosionRadius = 25;
		GameObject[] blackblocks = GameObject.FindGameObjectsWithTag (Tags.BLOCK);
		foreach (GameObject block in blackblocks) {
			if (Vector3.SqrMagnitude (block.transform.position - transform.position) <= Mathf.Pow (explosionRadius, 2.0f)) {
				block.GetComponent<BlockLogic> ().BlowUp (transform.position, 200, explosionRadius);
				
			}
		}
		
		audio.PlayOneShot(explosionSound);
		// Spawn the explosion FX.
		GameObject fx = (GameObject)Instantiate (explosionFX, transform.position,
			transform.rotation);
		Destroy (fx, 1.0f);
	}
	
	/*
	 * Tell the Player to start running along the treadmill
	 */
	public void StartRunning ()
	{
		isWaitingToStart = false;
		if (inventory.HasItem (ItemNames.BOOST) || inventory.HasItem(ItemNames.SUPERBOOST)) {
			// Defaults to using the superboost first if they have both.
			StartBoosting (inventory.HasItem(ItemNames.SUPERBOOST));
		}
	}
		
	/*
	 * Add specified amount of money to the player currency.
	 */
	public void AddMoney (int amount)
	{
		money += amount;
	}
	
	/*
	 * Remove specified amount of money from player. Log warning
	 * if not enough money.
	 */
	public void RemoveMoney (int amount)
	{
		if (money - amount < 0) {
			Debug.LogWarning (string.Format ("Tried to remove more " +
				"money ({0}) than player owns ({1}).", amount, money));
		}
		money -= amount;
	}
	
	#endregion
	
	#region #4 Player Powers and Abilities
	
	/*
	 * Remaps color wheel enum to a material in the color manager.
	 * If we have time to come back to this, I'd probably refactor this.
	 */
	void ChangeColors (ColorWheel color)
	{
		switch (color) {
		case ColorWheel.blue:
			Camera.main.backgroundColor = ColorManager.Instance.blue.color;
			break;
		case ColorWheel.red:
			Camera.main.backgroundColor = ColorManager.Instance.red.color;
			break;
		case ColorWheel.green:
			Camera.main.backgroundColor = ColorManager.Instance.green.color;
			break;
		case ColorWheel.neutral:
			Camera.main.backgroundColor = ColorManager.Instance.black.color;
			break;
		}
		playerRGB.color = color;
	}
	
	/*
	 * Move the character in the specified direction with the specfied speed.
	 */
	void Move (Vector3 direction, float speed)
	{
		Vector3 movement = (direction.normalized * speed);

		// Update perceived velocity vector
		perceivedVelocity = movement + new Vector3 (0.0f, 0.0f, (treadmill.scrollspeed));
		
		// Apply movement vector
		movement *= Time.deltaTime;
		CharacterController biped = GetComponent<CharacterController> ();
		biped.Move (movement);
	}

	/*
	 * Cast our laser ability if cooldown is ready.
	 */
	void Laser ()
	{
		redPower.UseAbility ();
		// Spawn the laser FX
		GameObject fx = (GameObject)Instantiate (laserBeamFX, nodeLaser.transform.position,
			nodeLaser.transform.rotation);
		fx.transform.parent = nodeLaser.transform;
		Destroy (fx, 1.0f);
		
		audio.PlayOneShot (laserSound);
		
		const float LASER_HALFWIDTH = 1.5f;
		const float LASER_LENGTH = 60.0f;
		
		GameObject[] allBlocks = GameObject.FindGameObjectsWithTag ("Block");
		foreach (GameObject block in allBlocks) {
			Vector3 directionToBlock = block.transform.position - transform.position;
			float distanceToBlockRelativeToMyForward = Vector3.Project (directionToBlock, transform.forward).magnitude;
			
			// Note this will not work if the blocks are rotated,and it assumes they are square
			float blockWidth = block.collider.bounds.extents.x;
			float blockHeight = blockWidth;
			// Is the block in range of my laser?
			if (distanceToBlockRelativeToMyForward <= LASER_LENGTH + blockHeight) {
				// Compare distance along my X axis
				float distanceToBlockRelativeToMyRight = Vector3.Project (directionToBlock, transform.right).magnitude;
				if (distanceToBlockRelativeToMyRight <= (LASER_HALFWIDTH + blockWidth)) {
					RGB blockRGB = block.GetComponent<RGB> ();
					if (blockRGB.color == ColorWheel.black) {
						Vector3 explosionPosition = transform.position +
							transform.TransformDirection (Vector3.forward * distanceToBlockRelativeToMyForward);
						block.GetComponent<BlockLogic> ().BlowUp (explosionPosition);
					}
				}
			}
		}
	}
	
	/*
	 * Put up shields for the player. Set our shield value, play sound,
	 * and show shield.
	 */
	public void RaiseShield ()
	{
		audio.PlayOneShot (shieldUpSound);
		shieldAnimation.Play (ObjectNames.FX_SHIELD_IDLE);
		shieldObject.SetActive (true);
		isShieldUp = true;
	}
	
	/*
	 * Turn off shields. Should be called when green power times out or player
	 * is out of shield hits.
	 */
	public void LowerShields ()
	{
		audio.PlayOneShot (shieldDownSound);
		shieldObject.SetActive (false);
		shieldAnimation.Stop ();
		greenPower.ResetPower ();
		isShieldUp = false;
	}
	
	/*
	 * Calculate a hit to the shields (as opposed to health). Play the right sound
	 * depending on a shield being hit or going down.
	 */
	public void TakeShieldHit ()
	{
		float powerLoss = greenPower.maxValue / shieldStrength;
		float newShieldPower = Mathf.Max (greenPower.curValue - powerLoss, 0);
		if (newShieldPower > 0) {
			audio.PlayOneShot (shieldHitSound);
			greenPower.RemovePower (powerLoss);
		} else if (newShieldPower == 0) {
			LowerShields ();
		}
		greenPower.curValue = newShieldPower;
	}
	
	/*
	 * If shields are up but green power is all out, turn off shields. Also,
	 * show them as weak if time is almost up.
	 */
	void CheckShieldTimeout ()
	{
		if (greenPower.IsPowerActive ()) {
			if (greenPower.curValue <= (greenPower.maxValue / shieldStrength)) {
				// Play weak animation if within one hit away from breaking
				if (!shieldAnimation.IsPlaying (ObjectNames.FX_SHIELD_WEAK)) {
					shieldAnimation.Play (ObjectNames.FX_SHIELD_WEAK);
					audio.PlayOneShot (shieldWeakSound);
				}
			} else if (shieldAnimation.IsPlaying (ObjectNames.FX_SHIELD_WEAK)) {
				// Play idle animation if player gained enough power to get out of weak
				// threshold
				shieldAnimation.Play (ObjectNames.FX_SHIELD_IDLE);
			}
		} else if (isShieldUp) {
			// Shields timed out, lower them
			LowerShields ();
		}
	}
	
	void CheckSlowDownTimeout ()
	{
		if (isUsingSlowdown && !bluePower.IsPowerActive ()) {
			GameObject.Find (ObjectNames.TREADMILL).GetComponent<Treadmill> ().ResumeFromSlowdown ();
			isUsingSlowdown = false;
			audio.PlayOneShot (speedUpSound);
		}
	}
	
	public void SlowDown ()
	{
		isUsingSlowdown = true;
		audio.PlayOneShot (slowDownSound);
		GameManager.Instance.treadmill.TemporarySlowDown (slowDownStrength);
		bluePower.ActivatePower ();
	}
	
	/*
	 * Begin using the boost consumable
	 */
	void StartBoosting (bool isSuperBoost)
	{
		// Set state variables
		isBoosting = true;
		isBoostBraking = false;
		
		treadmill.StartBoostSpeed ();
		
		// Start the boost timer.
		boostTimer = new RBTimer ();
		float BOOST_TIME = isSuperBoost ? 15.0f : 8.0f;
		boostTimer.StartTimer (BOOST_TIME);
		
		// Remove the inventory item if they had one
		if(isSuperBoost) {
			inventory.RemoveItem (ItemNames.SUPERBOOST);
		} else {
			inventory.RemoveItem (ItemNames.BOOST);
		}
				
		boostFX = (GameObject)Instantiate (boostFXPrefab, transform.position,
			transform.rotation);
		boostFX.transform.parent = transform;
		
		// Show Boost use Text
		GameObject fxPrefab = isSuperBoost ? superBoostTextFX : boostTextFX;
		GameObject fx = (GameObject)Instantiate (fxPrefab, transform.position + fxPrefab.transform.position,
			fxPrefab.transform.rotation);
		fx.transform.parent = transform;
		Destroy (fx, 1.5f);
	}
	/*
	 * Starts restoring back to normal gameplay after a boost
	 */
	void StartBoostBraking ()
	{
		isBoostBraking = true;
		boostFX.GetComponent<FX_Boost> ().StopEmitting ();
		
		float BRAKE_TIME = 0.9f;
		boostBrakeTimer = new RBTimer ();
		boostBrakeTimer.StartTimer (BRAKE_TIME);
		
		treadmill.StopBoostSpeed ();
	}
	
	
	/*
	 * Handles stopping the boost functionality when the time is expired.
	 */
	void CheckBoostingFinish ()
	{
		if (isBoosting) {
			if(boostTimer.IsTimeUp () && !isBoostBraking) {
				StartBoostBraking ();
			} else if (boostTimer.IsTimeUp () && boostBrakeTimer.IsTimeUp ()) {
				StopBoosting ();
			}
		}
	}
	
	
	/*
	 * Stop using the boost consumable
	 */
	void StopBoosting ()
	{
		isBoosting = false;
	}
	
	#endregion
	
	#region #5 Player Upgrades
	
	/*
	 * Call this to make sure new powers or necessary resets occur.
	 */
	public void InitializeStatsOnSpawn ()
	{
		pickups = new List<GameObject> ();
		
		// Give player their upgrades
		// Charges Faster Upgrades
		if (inventory.HasItem (ItemNames.BLUE_FILLS_FASTER)) {
			bluePower.UpgradeMaximumCharge ();
		}
		if (inventory.HasItem (ItemNames.RED_FILLS_FASTER)) {
			redPower.UpgradeMaximumCharge ();
		}
		if (inventory.HasItem (ItemNames.GREEN_FILLS_FASTER)) {
			greenPower.UpgradeMaximumCharge ();
		}

		// Stronger Item Upgrades
		if (inventory.HasItem (ItemNames.RED_MORE_EFFECTIVE)) {
			redPower.UpgradeLaser ();
		}
		if (inventory.HasItem (ItemNames.GREEN_MORE_EFFECTIVE)) {
			shieldStrength = UPGRADED_SHIELD_STRENGTH;
		}
		if (inventory.HasItem (ItemNames.BLUE_MORE_EFFECTIVE)) {
			slowDownMovespeed = UPGRADED_MOVESPEED;
		}

		// Duration Upgrades
		if (inventory.HasItem (ItemNames.RED_LASTS_LONGER)) {
			redPower.UpgradeDuration ();
		}
		if (inventory.HasItem (ItemNames.GREEN_LASTS_LONGER)) {
			greenPower.UpgradeDuration ();
		}
		if (inventory.HasItem (ItemNames.BLUE_LASTS_LONGER)) {
			bluePower.UpgradeDuration ();
		}
		
		// Also reset the same stats that should be reset on revive
		InitializeStatsOnRevive ();
	}
	
	/*
	 * This should be called to reset stats when the player comes back to life or first time spawn.
	 */
	void InitializeStatsOnRevive ()
	{
		redPower.ResetPower ();
		greenPower.ResetPower ();
		greenPower.curValue = 0;
		bluePower.curValue = 0;
		isUsingSlowdown = false;
		isShieldUp = false;
		curHealth = 1;
		
		// Reset the number of wildcards that have been collected.
		WildcardCount = 0;
	}
	
	/*
	 * Return if the player has the magnet powerup for the
	 * provided color.
	 */
	public bool HasMagnetForColor (ColorWheel color)
	{
		switch (color) {
		case ColorWheel.red:
			if (inventory.HasItem (ItemNames.RED_MAGNET)) {
				return true;
			}
			break;
		case ColorWheel.green:
			if (inventory.HasItem (ItemNames.GREEN_MAGNET)) {
				return true;
			}
			break;
		case ColorWheel.blue:
			if (inventory.HasItem (ItemNames.BLUE_MAGNET)) {
				return true;
			}
			break;
		}
		return false;
	}
	#endregion
}