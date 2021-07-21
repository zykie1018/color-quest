using UnityEngine;
using System.Collections;

public class FX_BoostLine : MonoBehaviour
{
	int vertexCount = 0;
	int maxVerts = 50;
	LineRenderer lineRenderer;
	Vector3[] vertexSet;
	bool isEmitting;
	
	void Awake ()
	{
		isEmitting = true;
		vertexSet = new Vector3[maxVerts];
		LinkLineRenderers ();
	}
	
	void Update ()
	{
		// When we stop emitting, reduce verts from the end until we get to 0.
		if(!isEmitting) {
			ReduceMaxVerts ();
		}
		ShiftVerteces ();
		SpoofMoving ();
	}
	
	/* Removes a vert from the end of the line
	 */
	void ReduceMaxVerts ()
	{
		maxVerts = Mathf.Max (maxVerts -3, 0);
		// Notify parent we are done emitting. This requires this component comes from an FX_Boost.
		if(maxVerts == 0) {
			transform.parent.gameObject.GetComponent<FX_Boost> ().OnStoppedEmitting ();
		}
	}
	
	/*
	 * Cache references to the lineRenderer componenet
	 */
	void LinkLineRenderers ()
	{
		lineRenderer = GetComponent<LineRenderer> ();
	}
	
	/*
	 * Shifts the verteces down to make room for a new position at the front
	 * of the line.
	 */
	void ShiftVerteces ()
	{
		// Add an extra vertex if not maxed
		if (vertexCount < maxVerts) {
			lineRenderer.SetVertexCount (++vertexCount);
		} else {
			lineRenderer.SetVertexCount (maxVerts);
			vertexCount = maxVerts;
		}
				
		
		// Shift verteces down
		int i = vertexCount - 1;
		while (i > 0) {
			lineRenderer.SetPosition (i, vertexSet [i - 1]);
			vertexSet [i] = vertexSet [i - 1];
			i--;
		}
		if(vertexCount > 0) {
			lineRenderer.SetPosition (0, transform.position);
			vertexSet [0] = transform.position;
		}
	}
	
	/*
	 * Moves all the segments in the line down (except the first) as if the player moved.
	 */
	void SpoofMoving ()
	{
		int j = 1;
		while (j < vertexCount) {
			vertexSet [j] = vertexSet [j] + new Vector3 (0.0f, 0.0f, 
				-GameManager.Instance.treadmill.scrollspeed * Time.deltaTime);
			j++;
		}
		int i = 1;
		while (i < vertexCount) {
			lineRenderer.SetPosition (i, vertexSet [i]);
			i++;
		}
	}
	
	public void StopEmitting () {
		isEmitting = false;
	}
}
