using UnityEngine;
using System.Collections;

public class GUIMeter : MonoBehaviour
{
	float maxWidth;
	Power power;
	public GameObject fill;
	public AnimationClip glowAnimation;
	Color originalColor;
	
	public PowerComponent powerComponent;
	
	public enum PowerComponent {
		Red,
		Blue,
		Green
	}
	
	void Start ()
	{
		power = GetPowerComponent (powerComponent);
		
		if (power == null) {
			Debug.LogError ("Meter [" + name + "] not tied to a power.");
		}
		
		// Cache the width of the fill meter.
		maxWidth = fill.guiTexture.pixelInset.width;
		originalColor = fill.guiTexture.color;
	}
	
	/*
	 * Gets the player's power associated with this meter
	 */
	Power GetPowerComponent (PowerComponent powerToGet)
	{
		Power returnPower;
		switch (powerToGet ) {
		case PowerComponent.Red :
			returnPower = GameManager.Instance.player.redPower;
			break;
		case PowerComponent.Blue :
			returnPower = GameManager.Instance.player.bluePower;
			break;
		case PowerComponent.Green :
			returnPower = GameManager.Instance.player.greenPower;
			break;
		default :
			returnPower = GameManager.Instance.player.redPower;
			break;
		}
		return returnPower;
	}
	
	void Update ()
	{
		float currentFillPercentage = power.GetFillPercentage ();
		
		// Set scale to the current fill percentage
		Rect newInset = fill.guiTexture.pixelInset;
		newInset.width = currentFillPercentage*maxWidth;
		fill.guiTexture.pixelInset = newInset;
		
		// Glow an active meter
		if (power.IsPowerActive () || power.IsChargedAndReady ()) {
			Animation animation = GetComponent<Animation>();
			animation.Play (glowAnimation.name);
			// Animation is too fast so I am scaling it down.
			animation[glowAnimation.name].speed = 0.5f;
		} else {
			Animation animation = GetComponent<Animation>();
			animation.Stop();
			fill.guiTexture.color = originalColor;
		}
		
	}
}
