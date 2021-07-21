using UnityEngine;
using System.Collections;

public class GUI_Wildcard : MonoBehaviour
{
	public AnimationClip onAnim;
	public AnimationClip revealAnim;
	public AnimationClip hideAnim;
	public Item myItem;
	public GameObject backFace;
	public GameObject text;
	public AudioClip revealSound;
	
	void Awake ()
	{
		gameObject.SetActive (false);
	}
	
	public void Show ()
	{
		gameObject.SetActive (true);
		animation.Play (onAnim.name);
	}
	
	public void Reveal ()
	{
		animation.Play (revealAnim.name);
	}
	
	public void Hide ()
	{
		animation.Play (hideAnim.name);
	}
	
	public void SetItem(Item item)
	{
		myItem = item;
		backFace.renderer.material = item.wildcardMaterial;
		text.GetComponent<TextMesh> ().text = item.itemName;
	}
	
	public void PlaySound ()
	{
		audio.PlayOneShot (revealSound);
	}
}
