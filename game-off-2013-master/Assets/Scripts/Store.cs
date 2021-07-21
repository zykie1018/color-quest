using UnityEngine;
using System.Collections;

public class Store : MonoBehaviour
{	
	//public ItemCollection inventory;
	public GameObject[] allItems;
	public int selectedItem = 0;
	Inventory playerInventory;
	
	public AudioClip purchaseSound;
	Transform scroller;
	
	void Start ()
	{
		scroller = (Transform) GameObject.Find (ObjectNames.STORE_SCROLLER).transform;
		playerInventory = GameManager.Instance.player.GetComponent<Inventory> ();
	}
	
	void Update () 
	{
		if (GameManager.Instance.IsShopping ()) {
			if (Input.GetKeyDown ("a")) {
				ScrollToPrevious ();
			} else if (Input.GetKeyDown ("d")) {
				ScrollToNext ();
			}
			ScrollToItem (selectedItem);
		}
	}
	
	/*
	 * Adjust our selected item index to the previous one in the array.
	 * Prevent going out of bounds.
	 */
	public void ScrollToPrevious ()
	{
		selectedItem--;
		if (selectedItem < 0) {
			selectedItem = 0;
		}
	}
	
	/*
	 * Adjust our selected item index to the next one in the array. Cap
	 * it out at the end of the array.
	 */
	public void ScrollToNext ()
	{
		selectedItem++;
		if (selectedItem >= allItems.Length) {
			selectedItem = allItems.Length -1;
		}
	}
	
	/*
	 * (Slowly) Scroll to a provided index in the item list.
	 */
	void ScrollToItem (int index)
	{
		int adjustment = index * 8;
		float speed = 8.0f;
		// New position relative to shop
		Vector3 newLocation = transform.position - new Vector3(adjustment, 0, 0);
		scroller.position = Vector3.Lerp (scroller.position, newLocation, speed * Time.deltaTime);
	}
	
	/*
	 * Check if the player has already purcahsed the item. If so, return true.
	 */
	public bool IsAlreadyPurchased ()
	{
		Item itemToBuy = allItems[selectedItem].GetComponent<Item> ();
		return playerInventory.HasItem (itemToBuy.itemName);
	}
	
	/*
	 * Check if the player has enough money
	 */
	public bool HasEnoughMoney ()
	{
		Item itemToBuy = allItems[selectedItem].GetComponent<Item> ();
		return GameManager.Instance.player.money >= itemToBuy.cost;
	}
	
	/*
	 * Add the currently selected item to the player's inventory.
	 */
	public void BuyItem ()
	{
		Item itemToBuy = allItems[selectedItem].GetComponent<Item> ();
		playerInventory.AddItem (itemToBuy.itemName);
		GameManager.Instance.player.RemoveMoney (itemToBuy.cost);
		audio.PlayOneShot (purchaseSound);
		Debug.Log (playerInventory.GetContentsAsJSON ());
	}
	
	/*
	 * Get the item object that is currently selected in the shop.
	 */
	public Item GetSelectedItem ()
	{
		return allItems[selectedItem].GetComponent<Item> ();
	}
}
