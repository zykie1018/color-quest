using UnityEngine;
using System.Collections;
using System;

public class PigmentBody : MonoBehaviour
{
	ColorWheel currentColor;
	public Material[] bodyMaterials = new Material[3];
	public Material[] redBodyMaterials = new Material[3];
	public Material[] greenBodyMaterials = new Material[3];
	public Material[] blueBodyMaterials = new Material[3];
	public Material[] bwBodyMaterials = new Material[3];
	public GameObject[] limbs = new GameObject[Enum.GetNames (typeof(Limb)).Length];
	public GameObject[] fxLimbs = new GameObject[Enum.GetNames (typeof(Limb)).Length];
	public GameObject[] fxLimbPrefabs = new GameObject[Enum.GetNames (typeof(Limb)).Length];
	public GameObject[] crystalPrefabs = new GameObject[3];
	
	enum Limb//
	{
		Body,
		ArmL,
		ArmR,
		LegL,
		LegR
	}

	public void SetColor (ColorWheel color, bool colorFX)
	{
		GameObject bodyToColor = colorFX ? fxLimbs [(int)Limb.Body] : limbs [(int)Limb.Body];
		
		// Get materials to color limbs with
		Material armLegMat = ColorManager.Instance.red;
		//Material bodyMat = ColorManager.Instance.red;
		if (color == ColorWheel.red) {
			armLegMat = ColorManager.Instance.red;
			bodyMaterials = redBodyMaterials;
		} else if (color == ColorWheel.green) {
			armLegMat = ColorManager.Instance.green;
			bodyMaterials = greenBodyMaterials;
		} else if (color == ColorWheel.blue) {
			armLegMat = ColorManager.Instance.blue;
			bodyMaterials = blueBodyMaterials;
		} else if (color == ColorWheel.neutral) {
			armLegMat = ColorManager.Instance.neutral;
			bodyMaterials = bwBodyMaterials;
		}
		
		ColorLimbs (armLegMat, colorFX);
		bodyToColor.renderer.materials = bodyMaterials;
		currentColor = color;
	}
	
	public void SetColor (ColorWheel color)
	{
		SetColor (color, false);
	}
	
	void ColorLimbs (Material mat, bool colorFX)
	{
		GameObject[] limbsToColor = colorFX ? fxLimbs : limbs;
		foreach (GameObject limb in limbsToColor) {
			limb.renderer.material = mat;
		}
	}
	
	/*
	 * Replaces the character with ragdoll pieces and gives them impulse to match
	 * the character's velocity.
	 */
	public void ReplaceWithRagdoll ()
	{
		// Spawn ragdoll limbs with force
		float blockImpediment = 0.25f;
		Vector3 force = GameManager.Instance.player.perceivedVelocity * blockImpediment;
		for (int i = 0; i < Enum.GetNames(typeof(Limb)).Length; i++) {
			fxLimbs [i] = ReplaceLimb (limbs [i], fxLimbPrefabs [i], force);
		}
		
		// Color the limbs to match the body
		SetColor (currentColor, true);
		
		// Disable the body from rendering
		transform.gameObject.SetActive (false);
		
		// Spawn the crystals as rigid bodies.
		foreach(GameObject crystalPrefab in crystalPrefabs)
		{
			GameObject crystalRigidBody = (GameObject)Instantiate (crystalPrefab, transform.position,
				transform.rotation);
			crystalRigidBody.rigidbody.AddForce (force);
			// Attach to any treadmill section so they will get cleaned up on retry.
			crystalRigidBody.transform.parent = GameManager.Instance.treadmill.GetLastSectionInPlay ().transform;
		}
	}
	
	/*
	 * Revive the character from ragdoll by lerping all the limbs back to the body.
	 */
	public void StartReviving ()
	{
		foreach (GameObject limb in fxLimbs) {
			limb.GetComponent<FX_PigmentLimb> ().SetLerping (true);
		}
	}
	
	/*
	 * Called when a limb is done lerping back to its original game object.
	 */
	public void OnLimbDoneLerping ()
	{
		if (AreAllLimbsDoneLerping ()) {
			FinishRevive ();
		}
	}
	
	/*
	 * Returns true when all limbs are done lerping
	 */
	bool AreAllLimbsDoneLerping ()
	{
		foreach (GameObject limb in fxLimbs) {
			if (limb.GetComponent<FX_PigmentLimb> ().IsLerping) {
				return false;
			}
		}
		return true;
	}
	
	/*
	 * Performs all actions that must be done when the revive is finished.
	 */
	void FinishRevive ()
	{
		GameManager.Instance.player.OnBodyRevived ();
	}
	
	/*
	 * Destroys all the ragdolled limbs and restores the original.
	 */
	public void RestoreBody ()
	{	
		foreach (GameObject limb in fxLimbs) {
			Destroy (limb);
		}
		
		transform.gameObject.SetActive (true);
	}
	
	/*
	 * Replaces the specified GameObject with the specified Prefab and applies the specified force.
	 */
	GameObject ReplaceLimb (GameObject limb, GameObject limbFX, Vector3 force)
	{
		GameObject fx = (GameObject)Instantiate (limbFX, limb.transform.position,
			limb.transform.rotation);
		fx.rigidbody.AddForce (force, ForceMode.Impulse);
		fx.GetComponent<FX_PigmentLimb> ().SetOriginalLimb (limb, transform.gameObject);
		return fx;
	}
	
	/*
	 * Return the body ragdoll game object
	 */
	public GameObject GetRagdollBody ()
	{
		return (GameObject) fxLimbs[(int)Limb.Body];
	}
}
