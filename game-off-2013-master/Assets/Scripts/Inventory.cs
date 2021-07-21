using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
	List<string> inventory = new List<string> ();
	
	/*
	 * Return whether a given item is in the inventory.
	 */
	public bool HasItem (string itemName)
	{
		return inventory.Contains (itemName);
	}
	
	/*
	 * Add an item to the inventory. Log a warning if the item
	 * already exists.
	 */
	public void AddItem (string itemName)
	{
		if (inventory.Contains (itemName)) {
			Debug.LogWarning ("Tried to add an item already in inventory [" + itemName + "].");
		} else {
			inventory.Add (itemName);
		}
	}
	
	/*
	 * Remove an item from the inventory. Log a warning if item
	 * doesn't exist.
	 */
	public void RemoveItem (string itemName)
	{
		if (inventory.Contains (itemName)) {
			inventory.Remove (itemName);
		} else {
			Debug.LogWarning (string.Format ("Attempted to remove an item ({0}) " +
				"that did not exist in inventory.", itemName));
		}
	}
	
	/*
	 * This method returns the contents of the collection as a JSON string.
	 * It will most likely be used for debugging and may be handy when saving player
	 * data.
	 */
	public string GetContentsAsJSON ()
	{
		string str = "{[";
		foreach (string itemName in inventory) {
			str += string.Format("\"{0}\", ", itemName);
		}
		// Clean up trailing comma and space
		str = str.TrimEnd (',', ' ');
		str += "]}";
		return str;
	}
}