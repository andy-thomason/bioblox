using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class LabelScript : MonoBehaviour, IPointerClickHandler, IBeginDragHandler,
IDragHandler,IEndDragHandler{


	public PDB_molecule.Label label;
	public BioBlox owner;
	public int labelID;

	private int linkIndex=-1;
	// Use this for initialization
	void Start () {
	}

	public void BreakLink()
	{
		linkIndex = -1;
	}

	public void MakeLink(int index)
	{
		this.linkIndex = index;

	}

	public void OnPointerClick (PointerEventData eventData)
	{
		owner.LabelClicked (this.gameObject);
	}

	public void OnBeginDrag (PointerEventData eventData)
	{
		if (linkIndex != -1) {
			owner.BreakLink(linkIndex);
		}

	}

	public void OnDrag (PointerEventData eventData)
	{

	}

	public void OnEndDrag (PointerEventData eventData)
	{
		GameObject first = eventData.pointerEnter;
		if (first) {
			LabelScript lab=first.GetComponent<LabelScript>();
			if(lab)
			{
				if(lab.linkIndex!=-1)
				{
					owner.BreakLink(lab.linkIndex);
				}
				owner.LinkPair(this,lab);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {

//		if (Input.GetMouseButtonDown (0)) {
//			Camera c=GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
//			Ray r = c.ScreenPointToRay (
//				Input.mousePosition);
//
//			RaycastHit hitInfo=new RaycastHit();
//
//			Physics.Raycast(r,out hitInfo);
//			if(hitInfo.transform==this.transform)
//			{
//				Debug.Log ("GetClicked");
//			}
//		}


	}
}
