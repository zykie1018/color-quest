using UnityEngine;
using System.Collections;

/*
 * "Dumb" class that just holds and manipulates values.
 * Avoid putting logic here as this class is used for all
 * three colors.
 */
public abstract class Power : MonoBehaviour
{
	public AudioClip powerReadySound;
	protected ColorWheel color;
	
	// Power behavior
	public float curValue = 0;
	public float maxValue = 35;
	const float UPGRADED_MAX = 30;
	protected bool isPowerActive;
	protected float powerDuration = 5;
	protected const float UPGRADED_DURATION = 7.5f;
	protected RBTimer powerTimer = new RBTimer ();
	
	/*
	 * Reset all timers. Set current value to default (0). Useful when starting
	 * a game.
	 */
	public void ResetPower ()
	{
		isPowerActive = false;
		curValue = 0;
	}
	
	void Update ()
	{
		// Stop our timers when power and abilities are done being used
		if (IsPowerActive ()) {
			curValue = Mathf.Max (curValue - ((maxValue /powerDuration) * Time.deltaTime), 0);
			if (curValue <= 0) {
				isPowerActive = false;
			}
		}
	}
	
	/*
	 * Get how full the power is (used for GUI display).
	 */
	public float GetFillPercentage ()
	{
		return ((float)curValue / maxValue);
	}
	
	/*
	 * Add a provided amount of power.
	 */
	public void AddPower (float amount)
	{
		curValue = Mathf.Min (curValue + amount, maxValue);
		if (IsChargedAndReady ()) {
			audio.PlayOneShot (powerReadySound);
		}
	}
	
	/*
	 * Remove a provided amount of power.
	 */
	public void RemovePower (float amount)
	{
		curValue = Mathf.Max (curValue - amount, 0);
	}
	
	/*
	 * Fully charge power.
	 */
	public void Charge ()
	{
		curValue = maxValue;
	}
	
	/*
	 * Check if a power is ready to use (off cooldown and enough energy).
	 */
	public bool IsChargedAndReady ()
	{
		return curValue == maxValue && !IsPowerActive ();
	}
	
	/*
	 * Return whether the power is active. Abilities tied to powers shouldn't
	 * be usable unless the power is active.
	 */
	public bool IsPowerActive ()
	{
		return isPowerActive;
	}
	
	/*
	 * Call this to use all energy and activate the power for its
	 * duration.
	 */
	public void ActivatePower ()
	{
		isPowerActive = true;
	}
	
	/*
	 * Set our maximum charge value to the upgraded value.
	 */
	public void UpgradeMaximumCharge ()
	{
		maxValue = UPGRADED_MAX;
	}

	/*
	 * Set the power duration to the upgraded value.
	 */
	public void UpgradeDuration ()
	{
		powerDuration = UPGRADED_DURATION;
	}
}
