using UnityEngine;
using System.Collections;

public class BlockLogic : MonoBehaviour
{
	public GameObject destroyFX;
	
	/*
	 * Blow up the block with default explosion parameters
	 */
	public void BlowUp(Vector3 position)
	{
		float defaultExplosionForce = 40.0f;
		float defaultRadius = 8.0f;
		BlowUp (position, defaultExplosionForce, defaultRadius);
	}
	
	/*
	 * Blow up the block with special force and explosion radius
	 */
	public void BlowUp(Vector3 position, float forceMagnitude, float explosionRadius)
	{
		GameObject fx = (GameObject)Instantiate (destroyFX, transform.position,
				Quaternion.LookRotation (Vector3.forward, Vector3.up));
		FX_BlockBreak fxScript = (FX_BlockBreak)fx.GetComponent<FX_BlockBreak>();
		if(fxScript != null) {
			fxScript.Explode(position, forceMagnitude, explosionRadius);
			Destroy (fx, 3.0f);
			// Parent the explosion to the treadmill
			fx.transform.parent = transform.parent;
		}
		else {
			Destroy (fx, 0.8f);
		}
		Destroy (gameObject);
	}
}
