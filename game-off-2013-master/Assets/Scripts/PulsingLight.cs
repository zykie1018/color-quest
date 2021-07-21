using UnityEngine;
using System.Collections;

public class PulsingLight : MonoBehaviour
{
	float multiplier = 1.1f;

	// Update is called once per frame
	void Update () {
		light.intensity *= multiplier;
		if (light.intensity > 1.0f) {
			multiplier = 0.99f;
		} else if (light.intensity < 0.45f) {
			multiplier = 1.01f;
		}
	}
}
