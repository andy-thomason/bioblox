using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
public class ConnectionManager : MonoBehaviour {

	int numChainClicks = 0;

	int contractionKVal;

	public float dampingFactor = 1.0f;
	public float force = 10.0f;
	public float minDistance = 100.0f;

	public bool shouldContract = false;

	List<AtomConnection> connections = new List<AtomConnection> ();

	public void Reset()
	{
		connections.Clear ();

	}

	public void CreateLinks(PDB_mesh mol1, int[] mol1AtomIndicies,
	                   PDB_mesh mol2, int[] mol2AtomIndicies)
	{
		connections.Clear ();
		for(int i=0;i<mol1AtomIndicies.Length; ++i)
		{
			AtomConnection con = new Grappel();

			con.molecules[0] = mol1;
			con.molecules[1] = mol2;

			con.atomIds[0]=mol1AtomIndicies[i];
			con.atomIds[1]=mol2AtomIndicies[i];

			con.isActive=true;

			connections.Add(con);
		}
	}

	public bool RegisterClick (PDB_mesh mol, int atomIndex)
	{
		if (atomIndex != -1) {
			if (numChainClicks == 0) {
				connections.Add (new Grappel ());
				connections [connections.Count - 1].molecules [0] = mol;
				connections [connections.Count - 1].atomIds [0] = atomIndex;
				numChainClicks++;
				return true;
			} else if (numChainClicks > 0) {
				AtomConnection grap = (Grappel)connections [connections.Count - 1];

				if(grap.molecules[0]==mol)
				{
					connections.Remove(grap);
					numChainClicks=0;
					return false;
				}
				else if(atomIndex!=-1)
				{
					numChainClicks = 0;
					grap.molecules [1] = mol;
					grap.atomIds [1] = atomIndex;
					grap.isActive = true;
					return true;
				}
			}
		}
		return false;
	}

	public void Contract()
	{
		shouldContract = !shouldContract;
	}

	// Use this for initialization
	void Start () {
	}


	void FixedUpdate()
	{
		if (shouldContract) {
			for(int i=0; i < connections.Count; ++i)
			{
				connections[i].Update(dampingFactor, force, minDistance);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		shouldContract = true;
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

		if (Input.GetMouseButtonDown (0) &&
		    !Input.GetKey(KeyCode.LeftShift)) {
			Camera c = GameObject.Find("Main Camera").GetComponent<Camera>();
			Ray cursorRay = c.ScreenPointToRay(Input.mousePosition);
			for(int i = 0; i < connections.Count; ++i)
			{
				AtomConnection con=connections[i];
				if(con.isActive)
				{
					Vector3 aPos = con.molecules[0].GetAtomWorldPositon(con.atomIds[0]);
					Vector3 bPos = con.molecules[1].GetAtomWorldPositon(con.atomIds[1]);

					Vector3 aToB = bPos - aPos;;
					Plane p = new Plane(aPos,bPos,aPos+ new Vector3(0,1,0));
					float distOut;
					p.Raycast(cursorRay,out distOut);
					Vector3 cursorWorldPos = cursorRay.GetPoint(distOut);

					Vector3 aToCursor = cursorWorldPos - aPos;

					float f = Vector3.Dot(aToCursor,aToB.normalized);

					if(f < aToB.magnitude && f > 0)
					{
						Vector3 closestPoint = aPos + (aToB.normalized * f);
						float dist = (cursorWorldPos - closestPoint).sqrMagnitude;

						if(dist <1.0f)
						{
							connections.RemoveAt(i);
						}
					}
				}
			}
		}

	}
}
