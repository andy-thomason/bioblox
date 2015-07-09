using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
public class ConnectionManager : MonoBehaviour {

	int numChainClicks = 0;

	int contractionKVal;

	bool shouldContract = false;

	List<AtomConnection> connections = new List<AtomConnection> ();

	public void RegisterClick (PDB_mesh mol, int atomIndex)
	{
		if (atomIndex != -1) {
			if (numChainClicks == 0) {
				connections.Add (new Grappel ());
				connections [connections.Count - 1].molecules [0] = mol;
				connections [connections.Count - 1].atomIds [0] = atomIndex;
				numChainClicks++;
				Debug.Log ("StartConnection: " + atomIndex);
			} else if (numChainClicks > 0) {
				AtomConnection grap = (Grappel)connections [connections.Count - 1];

				if(grap.molecules[0]==mol)
				{
					connections.Remove(grap);
					numChainClicks=0;
				}
				else if(atomIndex!=-1)
				{
					numChainClicks = 0;
					grap.molecules [1] = mol;
					grap.atomIds [1] = atomIndex;
					grap.isActive = true;
					Debug.Log ("EndConnection: " + atomIndex);
				}
			}
		}
	}

	public void Contract()
	{
		shouldContract = true;

	}

	// Use this for initialization
	void Start () {
	}

	void FixedUpdate()
	{
		if (shouldContract) {
			for(int i=0; i < connections.Count; ++i)
			{
				connections[i].Update();
			}
		}
	}

	// Update is called once per frame
	void Update () {
		if (numChainClicks > 0) {
			Camera c = GameObject.Find("Main Camera").GetComponent<Camera>();
			AtomConnection con = connections[connections.Count-1];
			Vector3 atomCenter = con.molecules[0].mol.atom_centres[con.atomIds[0]];
			
			Vector3 to = new Vector3(Input.mousePosition.x,
			                         Input.mousePosition.y,
			                        -c.transform.position.z);
			to=c.ScreenToWorldPoint(to);
			Debug.DrawLine(con.molecules[0].transform.TransformPoint(atomCenter),
			               to);
		}
		for (int i = 0; i < connections.Count; ++i) {
			connections[i].Draw();
		}
	}
}
