using UnityEngine;
using System.Collections;

public class FX_PigmentLimb : MonoBehaviour
{
	
	GameObject sourceBody;
	GameObject originalLimb;

	public bool IsLerping { get; private set; }
	
	void Update ()
	{
		if (IsLerping) {
			UpdateLerpToOriginalLimb ();
		}
	}
	
	void UpdateLerpToOriginalLimb ()
	{
		float maxDistanceStep = 0.25f;
		float maxRotationDelta = 5.0f;
		transform.position = Vector3.MoveTowards (transform.position, originalLimb.transform.position, maxDistanceStep);
		transform.rotation = Quaternion.RotateTowards (transform.rotation, originalLimb.transform.rotation, maxRotationDelta);
		
		// When limb gets close enough to its original limb, stop lerping.
		float lerpCompleteDistanceSquared = 0.1f;
		if (Vector3.SqrMagnitude (originalLimb.transform.position - transform.position) <= lerpCompleteDistanceSquared) {
			EndLerp ();
		}
	}
	
	void EndLerp ()
	{
		IsLerping = false;
		transform.position = originalLimb.transform.position;
		transform.rotation = originalLimb.transform.rotation;
		
		sourceBody.GetComponent<PigmentBody> ().OnLimbDoneLerping();
		
	}
	
	public void SetLerping (bool lerp)
	{
		IsLerping = lerp;
		rigidbody.isKinematic = lerp;
		collider.enabled = !lerp;
	}
	
	public void SetOriginalLimb (GameObject limb, GameObject body)
	{
		originalLimb = limb;
		sourceBody = body;
	}
}
