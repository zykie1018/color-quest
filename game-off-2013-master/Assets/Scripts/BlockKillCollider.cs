using UnityEngine;
using System.Collections;

public class BlockKillCollider : MonoBehaviour
{
	/*
	 * When a player enters the trigger for a block, it means a collision has
	 * occurred with a black block. Play the animation and perform player collision
	 * logic.
	 */
	void OnTriggerEnter (Collider other)
	{
		if (!other.CompareTag (Tags.PLAYER)) {
			Debug.LogWarning ("Block collided with non-player object. Fix this.");
			return;
		}
		Player player = other.GetComponent<Player> ();
		transform.parent.GetComponent<BlockLogic> ().BlowUp (other.transform.position);
		player.CollideWithBlock ();
	}
}
