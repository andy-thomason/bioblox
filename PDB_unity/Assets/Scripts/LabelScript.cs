using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LabelScript : MonoBehaviour, IPointerClickHandler, IBeginDragHandler,
IDragHandler,IEndDragHandler{


	public PDB_molecule.Label label;
	public BioBlox owner;
	public int labelID;
	public int moleculeNumber;

	public GameObject cloudPrefab;
	public int numClouds;

	public Sprite clicked;
	public Sprite nonClicked;
//	private int linkIndex=-1;

	public bool is3D =false;
	public bool isInteractable=true;
	public bool shouldGlow=false;

	public bool cloudIs3D;
	
	List<GameObject> clouds = new List<GameObject> ();
	GameObject cloudStorer;
	
	// Use this for initialization
	void Start () {
		gameObject.GetComponent<Light> ().enabled = false;
		GameObject cloudSorter = new GameObject ();
		cloudSorter.name = "Clouds" + this.name;
		cloudSorter.transform.SetParent(GameObject.Find ("Clouds").transform);
		cloudSorter.transform.localScale = new Vector3 (1, 1, 1);
		cloudStorer = cloudSorter;
		for(int i=0;i<numClouds;++i)
		{
			MakeCloud(i);
		}
	}

	void MakeCloud(int i)
	{
		GameObject c = GameObject.Instantiate (cloudPrefab);
		if (!cloudIs3D) {
			c.transform.SetParent (GameObject.Find ("Clouds" + this.name).transform);
		} 
			clouds.Add (c);
	}

	public void OnDisable()
	{
		if (cloudStorer) {
			cloudStorer.SetActive (false);
		}
	}
	public void OnEnable()
	{
		if (cloudStorer) {
			cloudStorer.SetActive (true);
		}
	}

	public void OnDestroy()
	{
		GameObject.Destroy (cloudStorer);

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
		if (eventData.button == PointerEventData.InputButton.Left &&
		    !Input.GetKey(KeyCode.LeftShift)) {
			owner.LabelClicked (this.gameObject);
		}
		if (eventData.button == PointerEventData.InputButton.Left &&
		    Input.GetKey(KeyCode.LeftShift)) {
			owner.SiteClicked (this.gameObject);
		}
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

	void GenerateTail()
	{
		Vector3 atomPos = owner.GetAtomWorldPos (label.atomIndex,moleculeNumber);
		Vector3 toAtom = atomPos - this.transform.position;
		float tIncrement = 1.0f / clouds.Count;
		//Sprite s = this.GetComponent<Image> ().sprite;
		Vector3 scale = new Vector3 (1.0f, 1.0f, 1.0f);
		if(cloudIs3D)
		{
		  scale = new Vector3 (6.0f, 6.0f, 6.0f);
		}
		Vector3 targetScale = new Vector3 (0.1f, 0.1f, 0.1f);
		Vector3 scaleDiff = targetScale - scale;
		for (int i=0; i<clouds.Count; ++i) {
		clouds[i].transform.position=this.transform.position+
				toAtom*tIncrement*(i+1);
			clouds[i].transform.localScale=scale+
				scaleDiff*tIncrement*(i+1);

			if(!cloudIs3D)
			{
			Vector3 back = new Vector3 (0,0,1);
			Vector3 up = Vector3.Cross(toAtom,back);
			Vector3 down = Vector3.Cross(up,toAtom);
			clouds[i].transform.rotation = Quaternion.LookRotation(
				down,up);

			//clouds[i].GetComponent<Image>().sprite=s;
			}
			else{
				clouds[i].transform.rotation=Quaternion.LookRotation(
					toAtom,new Vector3(0,1,0));
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (is3D) {
			//Vector3 toCamera =c.transform.position-atomPos;
			//toCamera=toCamera.normalized*3;
			owner.GetLabelPos(label.atomIndex,moleculeNumber,this.transform);

		}
		if (shouldGlow) {
			this.GetComponent<Image>().sprite=clicked;
			gameObject.GetComponent<Light> ().enabled = true;
		} else {
			this.GetComponent<Image>().sprite=nonClicked;
			gameObject.GetComponent<Light> ().enabled = false;
		}
		GenerateTail();
	}
}
