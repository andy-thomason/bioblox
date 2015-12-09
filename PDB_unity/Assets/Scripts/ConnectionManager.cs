using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine.UI;

public class ConnectionManager : MonoBehaviour {

	int numChainClicks = 0;

	int contractionKVal;

	public float dampingFactor = 1.0f;
	//public float force = 10.0f;
	public float minDistance = 0.0f;
	public float maxDistance = 60.0f;
	public float spring_constant = 10000.0f;
	public Slider SliderStrings;

	public float[] connectionMinDistances;

	public bool shouldContract = false;
	

	public float slideBackSpeed = 2.0f;

	public List<AtomConnection> connections = new List<AtomConnection> ();
	public int nc;

	public void StringManager()
	{
		for (int i = 0; i<connectionMinDistances.Length; i++)
		{
			connectionMinDistances[i] = 60 * SliderStrings.value;
		}
	}

	public void DisableSlider()
	{
		if (connections.Count == 0)
		{
			SliderStrings.value = 1;
			SliderStrings.interactable = false;
		}
	}

	public void Reset()
	{
		connections.Clear ();
		connectionMinDistances = new float[]{};
	}

	public void CreateLinks(PDB_mesh mol1, int[] mol1AtomIndicies,
	                   PDB_mesh mol2, int[] mol2AtomIndicies)
	{
		connections.Clear ();
		for(int i=0;i<mol1AtomIndicies.Length; ++i)
		{
			AtomConnection con = new Rope();

			con.molecules[0] = mol1;
			con.molecules[1] = mol2;

			con.atomIds[0] = mol1AtomIndicies[i];
			con.atomIds[1] = mol2AtomIndicies[i];

			con.isActive=true;

			connections.Add(con);
		}

		connectionMinDistances = new float[mol1AtomIndicies.Length];
		for (int i = 0; i < connectionMinDistances.Length; ++i) {
			connectionMinDistances[i] = maxDistance * SliderStrings.value;
		}

		shouldContract = true;
	}

	public AtomConnection CreateLink(PDB_mesh mol1, int mol1AtomIndex, PDB_mesh mol2, int mol2AtomIndex) {
		AtomConnection con = new Rope();
		
		con.molecules[0] = mol1;
		con.molecules[1] = mol2;
		
		con.atomIds[0] = mol1AtomIndex;
		con.atomIds[1] = mol2AtomIndex;
		
		con.isActive=true;
		
		connections.Add(con);
		nc = connections.Count;

		connectionMinDistances = new float[connections.Count];
		for (int i = 0; i < connectionMinDistances.Length; ++i) {
			connectionMinDistances[i] = maxDistance * SliderStrings.value;;
		}
		
		shouldContract = true;

		return con;
	}

	public AtomConnection CreateAminoAcidLink(PDB_mesh mol1, int amino_acid_index1, PDB_mesh mol2, int amino_acid_index2) {
		int[] atoms1 = mol1.mol.aminoAcidsAtomIds [amino_acid_index1];
		int index1 = atoms1 [atoms1.Length - 1];
		int[] atoms2 = mol2.mol.aminoAcidsAtomIds [amino_acid_index2];
		int index2 = atoms2 [atoms2.Length - 1];
		return CreateLink (mol1, index1, mol2, index2);
	}

	public void DeleteAminoAcidLink(AtomConnection con) {
		connections.Remove (con);
	}

	public bool RegisterClick (PDB_mesh mol, int atomIndex)
	{
		Debug.Log ("clockl");
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
		BioBlox bb = (BioBlox)GameObject.FindObjectOfType (typeof(BioBlox));
		if (shouldContract) {
			for(int i=0; i < connections.Count; ++i)
			{
				connections[i].Update(spring_constant, dampingFactor, bb.stringForce, connectionMinDistances[i]);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		LineRenderer line_renderer = GameObject.FindObjectOfType<LineRenderer> () as LineRenderer;
		//if (line_renderer) line_renderer.clear ();

		if (numChainClicks > 0) {
			Camera c = GameObject.Find("Main Camera").GetComponent<Camera>();
			AtomConnection con = connections[connections.Count-1];
			Vector3 atomCenter = con.molecules[0].mol.atom_centres[con.atomIds[0]];
			
			Vector3 to = new Vector3(Input.mousePosition.x,
			                         Input.mousePosition.y,
			                        -c.transform.position.z);
			to=c.ScreenToWorldPoint(to);
			Vector3 from = con.molecules[0].transform.TransformPoint(atomCenter);
			//Debug.DrawLine(from, to);
		}

		/*if (line_renderer) {
			Debug.Log("zzz");
			Vector2 uv0 = new Vector2(0, 0);
			Vector2 uv1 = new Vector2(1, 1);
			line_renderer.add_line(new LineRenderer.Line(from, to, 0.1f, uv0, uv1));
		}*/

		for (int i = 0; i < connections.Count; ++i) {
			connections[i].Draw(line_renderer);
		}

//		if (Input.GetMouseButtonDown (0) &&
//		    Input.GetKey(KeyCode.LeftShift)) {
//			Camera c = GameObject.Find("Main Camera").GetComponent<Camera>();
//			Ray cursorRay = c.ScreenPointToRay(Input.mousePosition);
//			for(int i = 0; i < connections.Count; ++i)
//			{
//				AtomConnection con=connections[i];
//				if(con.isActive)
//				{
//					Vector3 aPos = con.molecules[0].GetAtomWorldPositon(con.atomIds[0]);
//					Vector3 bPos = con.molecules[1].GetAtomWorldPositon(con.atomIds[1]);
//
//					Vector3 aToB = bPos - aPos;;
//					Plane p = new Plane(aPos,bPos,aPos+ new Vector3(0,1,0));
//					float distOut;
//					p.Raycast(cursorRay,out distOut);
//					Vector3 cursorWorldPos = cursorRay.GetPoint(distOut);
//
//					Vector3 aToCursor = cursorWorldPos - aPos;
//
//					float f = Vector3.Dot(aToCursor,aToB.normalized);
//
//					if(f < aToB.magnitude && f > 0)
//					{
//						Vector3 closestPoint = aPos + (aToB.normalized * f);
//						float dist = (cursorWorldPos - closestPoint).sqrMagnitude;
//
//						if(dist <1.0f)
//						{
//							connections.RemoveAt(i);
//						}
//					}
//				}
//			}
//		}

	}
}
