using UnityEngine;
using System.Collections;

public class RedPower : Power
{
	// Ability behavior
	RBTimer abilityCooldownTimer = new RBTimer ();
	float abilityCooldown = 0.5f;
	float abilityUsesPerMeter = 3;
	const float UPGRADED_USES_PER_METER = 10;
	
	void Awake ()
	{
		color = ColorWheel.red;
	}
	
	void Update ()
	{
		// Stop our timers when power and abilities are done being used
		if (IsPowerActive ()) {
			curValue = Mathf.Max (curValue - ((maxValue /powerDuration) * Time.deltaTime), 0);
			if (abilityCooldownTimer.IsTimeUp ()) {
				abilityCooldownTimer.StopTimer ();
			}
			if (curValue <= 0) {
				isPowerActive = false;
				abilityCooldownTimer.StopTimer ();
			}
		}
	}
	
	/*
	 * Reset all timers. Set current value to default (0). Useful when starting
	 * a game.
	 */
	new public void ResetPower ()
	{
		isPowerActive = false;
		abilityCooldownTimer.StopTimer ();
		curValue = 0;
	}
	
	/*
	 * Use the ability associated with this power. This sets the cooldown timer
	 * and the ability behavior itself is controlled by the player.
	 */
	public void UseAbility ()
	{
		if (IsPowerActive () && !abilityCooldownTimer.IsRunning ()) {
			abilityCooldownTimer.StartTimer (abilityCooldown);
			RemovePower (maxValue / abilityUsesPerMeter);
		}
	}
	
	/*
	 * Return true if the ability is on cooldown (unusable).
	 */
	public bool AbilityOnCooldown ()
	{
		return abilityCooldownTimer.IsRunning ();
	}
	
	/*
	 * Set our laser to use less power per use.
	 */
	public void UpgradeLaser ()
	{
		abilityUsesPerMeter = UPGRADED_USES_PER_METER;
	}
	
}
