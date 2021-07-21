using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Section : MonoBehaviour
{
	public int numberOfPickups;
	
	public bool[] entranceOpenings;
	public bool[] exitOpenings;

	public byte entranceBitmap;
	public byte exitBitmap;
	
	public GameObject redCrystalPrefab;
	public GameObject greenCrystalPrefab;
	public GameObject blueCrystalPrefab;
	GameObject blockPrefab;
	GameObject wildcardPrefab;

	GameObject tempPrefabHolder;
	
	// Placeholder arrays for instantiating in-game prefabs
	public Vector3[] blockPlaceholders;
	public Vector3[] pickupAPlaceholders;
	public Vector3[] pickupBPlaceholders;
	public Vector3[] pickupCPlaceholders;
	public Vector3[] redPickupPlaceholders;
	public Vector3[] greenPickupPlaceholders;
	public Vector3[] bluePickupPlaceholders;
	public Vector3[] wildcardPlaceholders;
	
	Treadmill treadmill;
	
	void Awake ()
	{
		// Start by moving the new Section onto the Treadmill (as a child of the object)
		treadmill = GameManager.Instance.treadmill;
		transform.parent = treadmill.transform;
		ReferencePrefabs ();
	}
	
		
	/*
	 * Reference all the prefabs that GameManager stores which sections can spawn.
	 */
	void ReferencePrefabs ()
	{
		redCrystalPrefab = GameManager.Instance.redCrystalPrefab;
		greenCrystalPrefab = GameManager.Instance.greenCrystalPrefab;
		blueCrystalPrefab = GameManager.Instance.blueCrystalPrefab;
		blockPrefab = GameManager.Instance.blockPrefab;
		wildcardPrefab = GameManager.Instance.wildcardPrefab;
	}
	
	void Start ()
	{
		// Create an empty object to parent prefabs to (for some reason, the children prefabs can't
		// be attached to this section itself.
		tempPrefabHolder = new GameObject("Prefabs");
		// Move our prefabs now that they've been created
		tempPrefabHolder.transform.parent = transform;
		
		// Shuffle the three crystal prefabs
		List<GameObject> threeCrystals = new List<GameObject> {redCrystalPrefab, greenCrystalPrefab, blueCrystalPrefab};
		RBRandom.Shuffle<GameObject> (threeCrystals);
		GameObject randomCrystalForPickupA = threeCrystals[0];
		GameObject randomCrystalForPickupB = threeCrystals[1];
		GameObject randomCrystalForPickupC = threeCrystals[2];
		
		// Place all our prefabs on the section
		InstantiatePrefabsFromArray (blockPrefab, blockPlaceholders, tempPrefabHolder.transform);
		InstantiatePrefabsFromArray (randomCrystalForPickupA, pickupAPlaceholders, tempPrefabHolder.transform);
		InstantiatePrefabsFromArray (randomCrystalForPickupB, pickupBPlaceholders, tempPrefabHolder.transform);
		InstantiatePrefabsFromArray (randomCrystalForPickupC, pickupCPlaceholders, tempPrefabHolder.transform);
		InstantiatePrefabsFromArray (redCrystalPrefab, redPickupPlaceholders, tempPrefabHolder.transform);
		InstantiatePrefabsFromArray (greenCrystalPrefab, greenPickupPlaceholders, tempPrefabHolder.transform);
		InstantiatePrefabsFromArray (blueCrystalPrefab, bluePickupPlaceholders, tempPrefabHolder.transform);
		if (treadmill.NeedsWildcard ()) {
			foreach (Vector3 wildcardPostion in wildcardPlaceholders) {
				GameObject clonedPrefab = (GameObject)Instantiate(wildcardPrefab, transform.position + wildcardPostion,
					Quaternion.identity);
				clonedPrefab.transform.parent = tempPrefabHolder.transform;
				treadmill.OnWildcardSpawn ();
			}
		}
	}
	
	/*
	 * Iterate over an array of positions and create an instance of a prefab in resources at the same
	 * location as each placeholder. Also, parent the prefab to any specified Transform.
	 */
	void InstantiatePrefabsFromArray (GameObject prefab, Vector3[] positionArray, Transform prefabParent)
	{
		foreach (Vector3 prefabPosition in positionArray) {
			GameObject clonedPrefab = (GameObject)Instantiate(prefab, transform.position + prefabPosition, Quaternion.identity);
			clonedPrefab.transform.parent = prefabParent;
		}
	}

	/*
	 * Take an array of booleans and calculate what they are as a bitmap if transposed
	 * to the lowest bits. For example the values TFTT would be 0000 1011 as a byte.
	 * The values TFFFT would be 0001 0001. Then with that bitmap, return the decimal
	 * that it equates to.
	 * 
	 * Examples
	 * TTTTT = 11111 = 31 (all columns are open)
	 * TFTFF = 10100 = 20
	 * FFFTT = 00011 = 3
	 */
	byte CalculateDecimalValue (bool[] openings)
	{
		if (openings == null || openings.Length == 0) {
			Debug.LogWarning (string.Format("Cannot CalculateDecimalValue for {0} until openings are set.", 
				gameObject.name));
			return 0;
		}
		byte highestBitVal = (byte) (Mathf.Pow (2, openings.Length-1));
		byte curBitVal = 0;
		for (int i = 0; i < openings.Length; i++) {
			if (openings[i]) {
				curBitVal += (byte) (highestBitVal / (byte) Mathf.Pow (2, i));
			}
		}
		return curBitVal;
	}
	
	/*
	 * Test whether the Section can be followed by a provided Section. This compares
	 * this.Section's exit with the passed in Section's entrance. If there are any
	 * openings in line, it will return true.
	 */
	public bool CanBeFollowedBy (Section nextSection)
	{
		return (exitBitmap & nextSection.entranceBitmap) > 0;
	}
	
	#region Editor Tools
	
	/*
	 * Ensure our pickup count is set correctly. This should be called in the
	 * Editor so that we can make calculations against prefabs before instantiating them.
	 */
	public void SetPickupCount ()
	{
		numberOfPickups = 0;
		foreach (Transform child in transform) {
			if (child.CompareTag (Tags.PICKUP_GROUP_A) ||
				child.CompareTag (Tags.PICKUP_GROUP_B) ||
				child.CompareTag (Tags.PICKUP_GROUP_C)) {
				numberOfPickups++;
			}
		}
	}
	
	/*
	 * Take our boolean blockage values for entrance and exit and set the
	 * bitmaps that will be used to match up sequences.
	 */
	public void SetEntranceAndExitBitmaps ()
	{
		entranceBitmap = CalculateDecimalValue (entranceOpenings);
		exitBitmap = CalculateDecimalValue (exitOpenings);
	}
	
	/*
	 * Set the vector array that stores positions of all nodes.
	 */
	public void SetNodePositions ()
	{
		List<Vector3> blocks = new List<Vector3> ();
		List<Vector3> pickupsA = new List<Vector3> ();
		List<Vector3> pickupsB = new List<Vector3> ();
		List<Vector3> pickupsC = new List<Vector3> ();
		List<Vector3> redPickups = new List<Vector3> ();
		List<Vector3> greenPickups = new List<Vector3> ();
		List<Vector3> bluePickups = new List<Vector3> ();
		List<Vector3> wildcardPickups = new List<Vector3> ();
		
		foreach (Transform child in transform) {
			if (child.CompareTag (Tags.BLOCK)) {
				AddPositionToList (child, ref blocks);
			} else if (child.CompareTag (Tags.PICKUP_GROUP_A)) {
				AddPositionToList (child, ref pickupsA);
			} else if (child.CompareTag (Tags.PICKUP_GROUP_B)) {
				AddPositionToList (child, ref pickupsB);
			} else if (child.CompareTag (Tags.PICKUP_GROUP_C)) {
				AddPositionToList (child, ref pickupsC);
			} else if (child.CompareTag (Tags.RED_PICKUP)) {
				AddPositionToList (child, ref redPickups);
			} else if (child.CompareTag (Tags.GREEN_PICKUP)) {
				AddPositionToList (child, ref greenPickups);
			} else if (child.CompareTag (Tags.BLUE_PICKUP)) {
				AddPositionToList (child, ref bluePickups);
			} else if (child.CompareTag (Tags.WILDCARD)) {
				AddPositionToList (child, ref wildcardPickups);
			}
		}
		blockPlaceholders = blocks.ToArray ();
		pickupAPlaceholders = pickupsA.ToArray ();
		pickupBPlaceholders = pickupsB.ToArray ();
		pickupCPlaceholders = pickupsC.ToArray ();
		redPickupPlaceholders = redPickups.ToArray ();
		greenPickupPlaceholders = greenPickups.ToArray ();
		bluePickupPlaceholders = bluePickups.ToArray ();
		wildcardPlaceholders = wildcardPickups.ToArray ();
	}
	
	/*
	 * Helper method that adds the location of a provided Transform to the provided
	 * list of positions. List is passed by reference.
	 */
	void AddPositionToList (Transform transformToPlace, ref List<Vector3> listToAddTo)
	{
		listToAddTo.Add (transformToPlace.localPosition);
	}
	
	#endregion
}