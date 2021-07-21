using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour
{
	bool collecting;
	GameObject collector;
	float originalDistance;
	float distanceScaleup = 0.1f;
	bool applicationIsQuitting;
	public float size;

	protected virtual void Start ()
	{
		// Put the pickups into a list of pickups. This is so that we can do distance checks instead of collisions.
		GameManager.Instance.player.RememberPickup (gameObject);
		InitializeSize ();
	}
	
	/*
	 * Cache the size of this pickup, used to detect collisions.
	 */
	protected virtual void InitializeSize ()
	{
		if (collider != null) {
			size = collider.bounds.extents.x;
		}
	}
	
	void LateUpdate ()
	{
		if (collecting) {
			AnimateCollect ();
		} else {
			AnimateIdle ();
		}
	}
	
	void OnTriggerEnter (Collider other)
	{
		if (!other.CompareTag (Tags.PLAYER)) {
			Debug.LogWarning ("Had a block collision with something that's not the player. Exiting method.");
			return;
		}
		
		StartCollecting (other.gameObject);
	}
	
	
	/*
	 * Verify we award the pickup when destroyed, if we had a valid sucker.
	 */
	void OnDestroy ()
	{
		// Don't need to collect the pickup for the player when destroyed through application quitting.
		if (applicationIsQuitting) {
			return;
		}
		Player player = GameManager.Instance.player;
		if (player != null) {
			player.ForgetPickup (gameObject);
			if (collecting && collector != null) {
				player.CollectPickup (gameObject);
			}
		}
	}
	
	/*
	 * Called when the game is exited
	 */
	public void OnApplicationQuit ()
	{
		applicationIsQuitting = true;
	}
	
	protected virtual void AnimateIdle ()
	{
		transform.Rotate (new Vector3 (-30, -60, -45) * Time.deltaTime);
	}
	/*
	 * Play a transition to scale to nothing and then destroy the pickup.
	 */
	protected virtual void AnimateCollect ()
	{
		distanceScaleup *= 1.22f;
		float maxDistance = distanceScaleup * originalDistance;
		transform.position = Vector3.MoveTowards (transform.position, collector.transform.position, maxDistance);
		
		transform.localScale = transform.localScale * 0.90f;
		if (Vector3.SqrMagnitude (transform.position - collector.transform.position) <= 0.25f) {
			Destroy (gameObject);
		}
	}

	/*
	 * Begin the collection animation and disable the collider
	 */
	public virtual void StartCollecting (GameObject pickingUpGameObject)
	{
		collector = pickingUpGameObject;
		collecting = true;
		originalDistance = Vector3.Distance (collector.transform.position, transform.position);
		
		// Turn off the collider if we have one
		if (collider != null) {
			collider.enabled = false;
		}
	}
}
