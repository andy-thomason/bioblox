using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LabelScript : MonoBehaviour, IPointerClickHandler, IBeginDragHandler,
IDragHandler,IEndDragHandler{


	public PDB_molecule.Label label;
	public BioBlox owner;
	public int labelID;

//	private int linkIndex=-1;

	public bool is3D =false;
	public bool isInteractable=true;
	public bool shouldGlow=false;
	
	// Use this for initialization
	void Start () {
		gameObject.GetComponent<Light> ().enabled = false;
	}

//	public void BreakLink()
//	{
//		linkIndex = -1;
//	}
//
//	public void MakeLink(int index)
//	{
//		this.linkIndex = index;
//
//	}

	public void OnPointerClick (PointerEventData eventData)
	{
		owner.LabelClicked (this.gameObject);
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
//		if (isInteractable) {
//			if (linkIndex != -1) {
//				owner.BreakLink (linkIndex);
//			}
//		}
	}

	public void OnDrag (PointerEventData eventData)
	{

	}

	public void OnEndDrag (PointerEventData eventData)
	{
//		if (isInteractable) {
//			GameObject first = eventData.pointerEnter;
//			if (first) {
//				LabelScript lab = first.GetComponent<LabelScript> ();
//				if (lab) {
//					if (lab.linkIndex != -1) {
//						owner.BreakLink (lab.linkIndex);
//					}
//					owner.LinkPair (this, lab);
//				}
//			}
//		}
	}
	
	// Update is called once per frame
	void Update () {
		if (is3D) {
			//Vector3 toCamera =c.transform.position-atomPos;
			//toCamera=toCamera.normalized*3;
			owner.GetLabelPos(label.atomIndex,this.transform);
		}
		if (shouldGlow) {
			gameObject.GetComponent<Light> ().enabled = true;
		} else {
			gameObject.GetComponent<Light> ().enabled = false;
		}
	}
}
