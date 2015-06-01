using UnityEngine;
using System.Collections;

public class LabelScript : MonoBehaviour {


	public PDB_molecule.Label label;
	public PDB_mesh ownerMesh;
	// Use this for initialization
	void Start () {
	
	}

	void AlignToCamera()
	{
		GameObject c = GameObject.FindGameObjectWithTag ("MainCamera");
		this.transform.rotation = c.transform.rotation;
	}
	
	// Update is called once per frame
	void Update () {
		AlignToCamera ();

		if (Input.GetMouseButtonDown (0)) {
			Camera c=GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
			Ray r = c.ScreenPointToRay (
				Input.mousePosition);

			RaycastHit hitInfo=new RaycastHit();

			Physics.Raycast(r,out hitInfo);
			if(hitInfo.transform==this.transform)
			{
				Debug.Log ("GetClicked");
			}
		}


	}
}
