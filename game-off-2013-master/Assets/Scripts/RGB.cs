using UnityEngine;
using System.Collections;

/*
 * Attach this script to objects that the player will interact with
 * and to the player.
 */
public class RGB : MonoBehaviour
{
	public ColorWheel color;
	public bool autoSetMaterial = true;
	
	void Awake () {
		if(autoSetMaterial){
			SetMaterialToCurrentColor ();
		}
	}
	
	/*
	 * Check whether the color of the object this script is attached to is
	 * compatible with the passed in color. Compatible colors are those that
	 * either match or match one of the component colors in the color wheel.
	 */
	public bool isCompatible (RGB theirRGB) {
		if (theirRGB.color == color) {
			return true;
		}
		return false;
	}
	
	/*
	 * Set the renderer's color to whatever color this script is set to. This will
	 * keep our display color in sync with our logic. We can update these colors
	 * to custom colors later.
	 */
	void SetMaterialToCurrentColor ()
	{
		renderer.material = GetMaterialForRGB(this);
	}
	
	/*
	 * Gets the material that is associated with the specified RGB's color
	 */
	static public Material GetMaterialForRGB ( RGB rgb)
	{
		Material returnMaterial = null;
		switch (rgb.color) {
		case ColorWheel.red:
			returnMaterial = ColorManager.Instance.red;
			break;
		case ColorWheel.green:
			returnMaterial = ColorManager.Instance.green;
			break;
		case ColorWheel.blue:
			returnMaterial = ColorManager.Instance.blue;
			break;
		case ColorWheel.black:
			returnMaterial = ColorManager.Instance.black;
			break;
		}
		return returnMaterial;
	}
	
	public void Refresh()
	{
		SetMaterialToCurrentColor();
	}
}