using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class RedBlueTools : EditorWindow
{
	[MenuItem("RedBlue Tools/RedBlue Tools")]
	static void Init ()
	{
		RedBlueTools window = (RedBlueTools)EditorWindow.CreateInstance (typeof(RedBlueTools));
		window.Show ();
	}
	
	void OnGUI ()
	{
		if (GUILayout.Button ("Refresh Colors")) {
			RefreshColors ();
		}
		if (GUILayout.Button ("Calculate Section Stats")) {
			CalculateSectionBitmapsAndPickups ();
		}
		if (GUILayout.Button ("Set Node Position Array")) {
			if (EditorApplication.currentScene == "Assets/_Scenes/LevelDesigns.unity") {
				SetNodePositions ();
			} else {
				Debug.LogWarning ("This tool can only be used on the LevelDesigns scene.");
			}
		}
		if (GUILayout.Button ("Finalize Sections")) {
			if (EditorApplication.currentScene == "Assets/_Scenes/LevelDesigns.unity") {
				CalculateSectionBitmapsAndPickups ();
				SetNodePositions ();
				CopySectionsToPrefabs ();
			} else {
				Debug.LogWarning ("This tool can only be used on the LevelDesigns scene.");
			}
		}
	}
	
	/*
	 * Read in a CSV Attack file generated from our Google Spreadsheet of attacks. Each
	 * line will be an Attack, stored as a comma delimited value for each field in our Attack class.
	 * Once complete, write the name of each attack in a lookup file of strings.
	 */
	void RefreshColors ()
	{
		RGB[] allBlocks = (RGB[])GameObject.FindObjectsOfType (typeof(RGB));
		foreach (RGB colorLogic in allBlocks) {
			colorLogic.Refresh ();
		}
	}
	
	/*
	 * Iterate through all our sections and set the bitmap value and pickup count.
	 * This is meant to help out whoever is building sections in the editor add
	 * meaningful stats on each section without manual calculation.
	 */
	void CalculateSectionBitmapsAndPickups ()
	{
		Section[] allSections = (Section[])GameObject.FindObjectsOfType (typeof(Section));
		foreach (Section section in allSections) {
			section.SetEntranceAndExitBitmaps ();
			//Debug.Log (string.Format ("Set {0} entrance to {1} and exit to {2}.", section.name, 
			//	section.entranceBitmap, section.exitBitmap));
			section.SetPickupCount ();
			//Debug.Log (string.Format ("Set {0} pickup count to {1}", section.name, 
			//	section.numberOfPickups));
		}
	}
	
	/*
	 * Iterate through all our sections and set the position array. This Must
	 * be done when changing or creating new sections.
	 */
	void SetNodePositions ()
	{
		Section[] allSections = (Section[])GameObject.FindObjectsOfType (typeof(Section));
		foreach (Section section in allSections) {
			section.SetNodePositions ();
		}
	}
	
	/*
	 * Iterate over all the sections in the scene, clean them up, and then make a copy
	 * into the finalized sections folder. Create the folder if needed.
	 */
	void CopySectionsToPrefabs ()
	{
		// Create containing folder if it doesn't exist
		string folderName = "FinalizedSections";
		string parentFolder = "Assets/Prefabs";
		string fullPath = parentFolder + "/" + folderName;
		if (!System.IO.Directory.Exists (fullPath)) {
			AssetDatabase.CreateFolder (parentFolder, folderName);
		}
		
		// Copy our sections from the scene into the containing folder, removing nodes as we go
		string[] allTags = {Tags.BLOCK, Tags.PICKUP_GROUP_A, Tags.PICKUP_GROUP_B, Tags.PICKUP_GROUP_C,
							Tags.RED_PICKUP, Tags.GREEN_PICKUP, Tags.BLUE_PICKUP, Tags.WILDCARD};
		GameObject[] allSections = GameObject.FindGameObjectsWithTag (Tags.SECTION);
		List<GameObject> trash = new List<GameObject> ();
		foreach (GameObject sectionObject in allSections) {
			GameObject editedSection = (GameObject) GameObject.Instantiate (sectionObject);
			// Check all children for tags for objects that can be cleaned
			// This assumes you've already set the node arrays for these objects
			foreach (Transform child in editedSection.transform) {
				foreach (string tag in allTags) {
					if (child.CompareTag (tag)) {
						trash.Add (child.gameObject);
					}
				}
			}
			foreach (GameObject child in trash) {
				DestroyImmediate (child);
			}
			PrefabUtility.CreatePrefab (fullPath + "/" + sectionObject.name + ".prefab", editedSection,
				ReplacePrefabOptions.ReplaceNameBased);
			DestroyImmediate (editedSection);
		}
	}
}
