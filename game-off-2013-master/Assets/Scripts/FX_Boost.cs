using UnityEngine;
using System.Collections;

public class FX_Boost : MonoBehaviour
{
	public AudioSource boostSound;
	public FX_BoostLine redLine;
	public FX_BoostLine greenLine;
	public FX_BoostLine blueLine;
	
	void Awake ()
	{
		boostSound = GetComponent<AudioSource> ();
	}
	
	/*
	 * Stop all the BoostLines from emitting.
	 */
	public void StopEmitting ()
	{
		redLine.StopEmitting ();
		greenLine.StopEmitting ();
		blueLine.StopEmitting ();
		boostSound.Stop ();
	}
	
	/*
	 * Called by the boost lines when they are done emitting
	 */
	public void OnStoppedEmitting ()
	{
		GameManager.Instance.player.DoBlockClearExplosion ();
		Destroy (gameObject);
	}
}
