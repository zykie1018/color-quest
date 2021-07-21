using UnityEngine;
using System.Collections;

public class Item : MonoBehaviour
{
	public Type type;
	public string itemName;
	public string displayName;
	public int cost;
	public Material wildcardMaterial;
	
	public enum Type {
		consumable,
		upgrade
	}
	
	/* 
	 * Returns true if the two items are the same kind
	 */
	public bool CompareItem ( Item comparisonItem )
	{
		return itemName == comparisonItem.itemName;
	}
}
