using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class LabelScript : MonoBehaviour{

	// the 0th element of the atomId's list is assumed to be our location atom
	public List<int> atomIds;
	public BioBlox owner;
	public int labelID;
	public int moleculeNumber;

	public GameObject cloudPrefab;
	public int numClouds;

	public List<Sprite> sprites;
	public List<string> spriteNames;

	Sprite clicked;
	Sprite nonClicked;
//	private int linkIndex=-1;

	public bool is3D = false;
	public bool isInteractable = true;
	public bool shouldGlow = false;
	public bool useMiniLabel = true;

	List<MiniLabel> miniLabels = new List<MiniLabel> ();

	public bool cloudIs3D;
	
	List<GameObject> clouds = new List<GameObject> ();
	GameObject cloudStorer;
	
	// Use this for initialization


	public void Start () {

	}

	public void Init(PDB_molecule mol)
	{
		if (atomIds.Count == 0) {
			Debug.LogError("Init called with no atomIDs");
		}
		gameObject.GetComponent<Light> ().enabled = false;
		for (int i = 0; i < atomIds.Count; ++i) {
			
			atomIds[i] = mol.serial_to_atom[atomIds[i]];
		}

		if (!useMiniLabel) {
			GameObject cloudSorter = new GameObject ();
			cloudSorter.name = "Clouds" + this.name;
			cloudSorter.transform.SetParent (GameObject.Find ("Clouds").transform);
			cloudSorter.transform.localScale = new Vector3 (1, 1, 1);
			cloudStorer = cloudSorter;
			for (int j = 0; j < atomIds.Count; ++j) {
				for (int i=0; i<numClouds; ++i) {
					MakeCloud ();
				}
			}
		} else {
			GameObject prefab = GameObject.Instantiate(this.gameObject);
			for(int i = 0; i < atomIds.Count; ++ i)
			{
				GameObject miniLab = GameObject.Instantiate(prefab);
				GameObject.Destroy(miniLab.GetComponent<LabelScript>());
				miniLab.AddComponent<MiniLabel>().owner = this;
				miniLabels.Add(miniLab.GetComponent<MiniLabel>());
				miniLab.name = "MiniLabel" + i;
				miniLab.transform.SetParent(this.transform,false);
				miniLab.transform.localScale = new Vector3(1,1,1);
			}
			GameObject.Destroy(prefab);
			GameObject.Destroy(gameObject.GetComponent<Image>());
			GameObject.Destroy(gameObject.GetComponent<Light>());
		}

		/*
		for (int i=0; i<sprites.Count; ++i) {
			if(spriteNames[i] == this.name)
			{
				clicked = nonClicked = sprites[i];
			}
		}*/
	}

	void MakeCloud()
	{
		GameObject c = GameObject.Instantiate (cloudPrefab);
		if (!cloudIs3D) {
			c.transform.SetParent (cloudStorer.transform);
		} 
			clouds.Add (c);
	}

	public void OnDisable()
	{
		if (cloudStorer && !useMiniLabel) {
			cloudStorer.SetActive (false);
		}
		if (useMiniLabel) {
			for(int i = 0; i < miniLabels.Count; ++i)
			{
				miniLabels[i].enabled = false;
			}
		}
	}
	public void OnEnable()
	{
		if (cloudStorer && !useMiniLabel) {
			cloudStorer.SetActive (true);
		}
		if (useMiniLabel) {
			for(int i = 0; i < miniLabels.Count; ++i)
			{
				miniLabels[i].enabled = true;
			}
		}
	}

	public void OnDestroy()
	{
		if (cloudStorer) {
			GameObject.Destroy (cloudStorer);
		}
		if (useMiniLabel) {
			for(int i = 0; i < miniLabels.Count; ++i)
			{
				GameObject.Destroy(miniLabels[i]);
			}
		}
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


	void GenerateTail()
	{
		for (int j = 0; j < atomIds.Count; ++j) {
			Vector3 atomPos = owner.GetAtomWorldPos (atomIds [j], moleculeNumber);
			Vector3 toAtom = atomPos - this.transform.position;
			float tIncrement = 1.0f / numClouds;
			//Sprite s = this.GetComponent<Image> ().sprite;
			Vector3 scale = new Vector3 (1.0f, 1.0f, 1.0f);
			if (cloudIs3D) {
				scale = new Vector3 (6.0f, 6.0f, 6.0f);
			}
			Vector3 targetScale = new Vector3 (0.1f, 0.1f, 0.1f);
			Vector3 scaleDiff = targetScale - scale;
			for (int i = 0; i < numClouds; ++i) {
				int offset = numClouds * j; 
				clouds [i + offset].transform.position = this.transform.position +
					toAtom * tIncrement * (i + 1);
				clouds [i + offset].transform.localScale = scale +
					scaleDiff * tIncrement * (i + 1);

				if (!cloudIs3D) {
					Vector3 back = new Vector3 (0, 0, 1);
					Vector3 up = Vector3.Cross (toAtom, back);
					Vector3 down = Vector3.Cross (up, toAtom);
					clouds [i + offset].transform.rotation = Quaternion.LookRotation (
				down, up);

					//clouds[i].GetComponent<Image>().sprite=s;
				} else {
					clouds [i + offset].transform.rotation = Quaternion.LookRotation (
					toAtom, new Vector3 (0, 1, 0));
				}
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (is3D) {
			owner.GetLabelPos(atomIds,moleculeNumber,this.transform);

			if(useMiniLabel)
			{
				for(int i = 0; i < miniLabels.Count; ++i)
				{
					owner.GetMiniLabelPos(atomIds[i],moleculeNumber,miniLabels[i].transform);
				}
			}

			GameObject cam = GameObject.Find("Main Camera");
			GameObject mol = owner.molecules[moleculeNumber];

			if(!useMiniLabel && mol)
			{
				Vector3 toMol = mol.transform.position - this.transform.position;
				this.transform.LookAt(cam.transform,-toMol);
			}
			else if(useMiniLabel)
			{
				for(int i = 0; i < miniLabels.Count; ++i)
				{
					Vector3 toAtom = owner.GetAtomWorldPos(atomIds[i],moleculeNumber) - miniLabels[i].transform.position;
					Vector3 toCamera = cam.transform.position - miniLabels[i].transform.position;
					miniLabels[i].transform.LookAt(cam.transform,-toAtom);
					miniLabels[i].transform.position +=toCamera.normalized*5;
				}
			}
		}
		if (shouldGlow) {
			if(!useMiniLabel)
			{
				gameObject.GetComponent<Light> ().enabled = true;
			}
			else
			{
				for(int i = 0; i < miniLabels.Count; ++i)
				{
					miniLabels[i].GetComponent<Light>().enabled = true;
				}
			}
		} else 
		{
			if(!useMiniLabel)
			{
				gameObject.GetComponent<Light> ().enabled = false;
			}
			else{
				for(int i = 0; i < miniLabels.Count; ++i)
				{
					miniLabels[i].GetComponent<Light>().enabled = false;
				}
			}
		}
		if (!useMiniLabel) {
			GenerateTail ();
		}
	}
}
