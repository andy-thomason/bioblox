using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

// Bioblox
// Removed all GUI stuff
// Removed all code based on Mathnet and left only code based on alglib
// Fnat is calculated in a naive way, checking all pairwise distances
// Fnat also changed because of changes in PDbInterface
// TODO: BVH should be used for efficient Fnat calculation
// TODO: check how pos is being used compared to old gc
public class ScoreHandler : MonoBehaviour {

	public GameObject receptorGO;
	public GameObject ligandGO;
	// How often to call calculation/update
	public float timeStep = 1 / 15.0f;

	private PDB_molecule ligand;
	private PDB_molecule receptor;
	private Transform ligandT;
	private Transform receptorT;
	private PdbInterface iPdb;

	private static int numLAtoms = 0;
	private static int numRAtoms = 0;

	private static float ligandRmsd = 0f;
	private static float interfaceRmsd = 0f;
	private static float fnat = 0f;
	private static String capri = "EPIC-FAIL";
	private static float capriPerc = 0f;
	
	private static float[] runningTime = { 0f, 0f, 0f, 0f};

	private static float interface1 = 5.0f * 5.0f;
	private static float interface2 = 10.0f * 10.0f;

	void Awake () {

		// ATTENTION!!!!! PDBs must be parsed already!
		// Check script execution order

		ligand = ligandGO.GetComponent<PDB_mesh> ().Mol;
		receptor = ligandGO.GetComponent<PDB_mesh> ().Mol;
		iPdb = new PdbInterface (receptor, ligand);

		numLAtoms = ligand.atom_centres.Length;
		numRAtoms = receptor.atom_centres.Length;

		ligandT = ligandGO.transform;
		receptorT = receptorGO.transform;

		InvokeRepeating("Score", 0.01f, timeStep);
	}

	void Score() {
    
        if(ligand == null || receptor == null)
            return;
        
		float startTime = Time.realtimeSinceStartup;
		ligandRmsd = LigandRmsd ();
		runningTime[0] = Time.realtimeSinceStartup - startTime;
		interfaceRmsd = InterfaceRmsdAlglib ();		
		runningTime[1] = Time.realtimeSinceStartup - startTime;
		fnat = FNat();
		runningTime[2] = Time.realtimeSinceStartup - startTime;
		capri = CapriRank ();
		capriPerc = CapriPerc ();
		runningTime[3] = Time.realtimeSinceStartup - startTime;
		for(int i = 0; i < 2; i++) {
			for(int j = i+1; j < 3; j++) {
				runningTime[j] -= runningTime[i];
			}
		}
	}

	private float FNat() {
		int all = iPdb.Contacts.Count;
		int native = 0;
		Matrix4x4 transfMat = receptorT.worldToLocalMatrix * ligandT.localToWorldMatrix;
		HashSet<Pair<int, int>> contacts = new HashSet<Pair<int, int>> ();
		foreach (int i in iPdb.LResidues) {
			int fromLAtom = ligand.residues[2*i + 1];
			int toLAtom = (2*i+3 >= ligand.residues.Length)? ligand.names.Length : ligand.residues[2*i + 3];
			for (int ii = fromLAtom; ii < toLAtom; ii++) {
				Vector3 lCoords = transfMat.MultiplyPoint3x4(ligand.atom_centres[ii]);
				foreach (int j in iPdb.RResidues) {
					Pair<int, int> p = new Pair<int, int>(i, j);
					if (iPdb.Contacts.Contains(p)) {
						int fromRAtom = receptor.residues[2*i + 1];
						int toRAtom = (2*i+3 >= receptor.residues.Length)? receptor.names.Length : receptor.residues[2*i + 3];
						for (int jj = fromRAtom; jj < toRAtom; jj++) {
							float distance = (lCoords - receptor.atom_centres[jj]).sqrMagnitude;
							if (distance <= interface1) {
								contacts.Add (p);
								native++;
								continue;
							}						
						}
					}
				}
			}
		}
		return 1.0f * native / all;
	}

	private float LigandRmsd() {
		float rmsd = 0f;
		int count = 0;
		Vector3 offset = ligand.pos - receptor.pos;
		Matrix4x4 transfMat = ligandT.worldToLocalMatrix * receptorT.localToWorldMatrix;
		for (int i = 0; i < numLAtoms; i++) {
			if (ligand.names[i] == PDB_molecule.atom_CA) {
				rmsd += (transfMat.MultiplyPoint3x4(ligand.atom_centres[i]+offset) - ligand.atom_centres[i]).sqrMagnitude;
				count++;
			}
		}
		return Mathf.Sqrt (rmsd / count);
	}

	private float InterfaceRmsdAlglib() {
		Vector3 offset = ligand.pos - receptor.pos;
		Matrix4x4 transfMat = receptorT.worldToLocalMatrix * ligandT.localToWorldMatrix;
		double[,] reference = new double[iPdb.NumAtomsExt, 3];
		double[,] target = new double[iPdb.NumAtomsExt, 3];
		Vector3[] lCoordsRef = new Vector3[iPdb.LAtomsExt.Count];
		Vector3[] lCoordsTrg = new Vector3[iPdb.LAtomsExt.Count];
		Vector3 gcRef = Vector3.zero;
		Vector3 gcTrg = Vector3.zero;

		foreach (int i in iPdb.RAtomsExt) {
			gcRef += receptor.atom_centres[i];
		}	
		gcTrg = gcRef;
		int j = 0;
		foreach (int i in iPdb.LAtomsExt) {
			lCoordsRef[j] = ligand.atom_centres[i] + offset;
			gcRef += lCoordsRef[j];
			lCoordsTrg[j] = transfMat.MultiplyPoint3x4(ligand.atom_centres[i]);
			gcTrg += lCoordsTrg[j];
			j++;
		}
		gcRef /= iPdb.NumAtomsExt;
		gcTrg /= iPdb.NumAtomsExt;		

		j = 0;
		foreach (int i in iPdb.RAtomsExt) {
			Vector3 coords = receptor.atom_centres[i] - gcRef;
			reference[j, 0] = coords.x;
			reference[j, 1] = coords.y;
			reference[j, 2] = coords.z;
			coords = receptor.atom_centres[i] - gcTrg;
			target[j, 0] = coords.x;
			target[j, 1] = coords.y;
			target[j, 2] = coords.z;
			j++;
		}
		for(int i = 0; i < lCoordsRef.Length; i++) {
			Vector3 coords = lCoordsRef[i] - gcRef;
			reference[j, 0] = coords.x;
			reference[j, 1] = coords.y;
			reference[j, 2] = coords.z;
			coords = lCoordsTrg[i] - gcTrg;
			target[j, 0] = coords.x;
			target[j, 1] = coords.y;
			target[j, 2] = coords.z;
			j++;
		}
		lCoordsRef = null;
		lCoordsTrg = null;

		// Initial residual
		double eInit = 0;
		for(int i = 0; i < reference.GetLength(0); i++) {
			for(j = 0; j < reference.GetLength(1); j++) {
				eInit += (reference[i, j] * reference[i, j]) + (target[i,j] * target[i,j]);
			}
		}

		double[,] res = new double[3, 3];
		alglib.rmatrixgemm(3, 3, iPdb.NumAtomsExt, 1.0, reference, 0, 0, 1, target, 0, 0, 0, 1.0, ref res, 0, 0);
		double[] w;
		double[,] u;
		double[,] vt;
		alglib.rmatrixsvd (res, 3, 3, 2, 2, 2, out w, out u, out vt);
		double reflect = alglib.rmatrixdet(u) * alglib.rmatrixdet(vt);
		if (reflect < 0) {
			w[w.Length-1] -= w[w.Length-1];
			for(j = 0; j < vt.GetLength(1); j++) {
				vt[vt.GetLength(0)-1, j] -= vt[vt.GetLength(0)-1, j];
			}
		}

		double rmsd = 0;
		foreach(double k in w) {
			rmsd += k;
		}
		rmsd = eInit - (2.0f * rmsd);
		rmsd = Math.Sqrt(Math.Abs(rmsd/iPdb.NumAtomsExt));
		return (float)rmsd;
	}

	// NOTE: ALL slightly different to each other!!!!!!!
	//http://onlinelibrary.wiley.com/doi/10.1002/prot.21804/full 2007
	//http://onlinelibrary.wiley.com/enhanced/doi/10.1002/prot.22818 2009
	//http://onlinelibrary.wiley.com/doi/10.1002/prot.24428/full 2013
	private String CapriRank() {
		//incorrect
		if (fnat < 0.1 || (ligandRmsd > 10.0 && interfaceRmsd > 4.0)) {
			return "EPIC-FAIL";
		}
		//acceptable
		if (((fnat >= 0.1 && fnat < 0.3) && (ligandRmsd <= 10.0 || interfaceRmsd <= 4.0)) ||
		    (fnat >= 0.3 && ligandRmsd >  5.0 && interfaceRmsd > 2.0)) {
			return "*";
		}
		//medium
		if (((fnat >= 0.3 && fnat < 0.5) && (ligandRmsd <= 5.0 || interfaceRmsd <= 2.0)) ||
		    (fnat >= 0.5 && ligandRmsd > 1.0 && interfaceRmsd > 1.0)) {
			return "**";
		}
		//high
		if (fnat >= 0.5 && (ligandRmsd <= 1.0 || interfaceRmsd <= 1.0)) {
			return "***";
		}
		return "EPIC-FAIL";
	}

	private float CapriPerc() {		
		float myFnat = fnat;
		float myLigandRmsd = ligandRmsd;
		float myInterfaceRmsd = interfaceRmsd;

		// restrict values within regions
		if (((fnat >= 0.1 && fnat < 0.3) && (ligandRmsd <= 10.0 || interfaceRmsd <= 4.0)) ||
		    (fnat >= 0.3 && ligandRmsd >  5.0 && interfaceRmsd > 2.0)) {
			myFnat = Mathf.Min (myFnat, 0.3f);
			myLigandRmsd = Mathf.Min (10.0f, Mathf.Max (myLigandRmsd, 5.0f));
			myInterfaceRmsd = Mathf.Min (4.0f, Mathf.Max (myInterfaceRmsd, 2.0f));
		} else if (((fnat >= 0.3 && fnat < 0.5) && (ligandRmsd <= 5.0 || interfaceRmsd <= 2.0)) ||
		    (fnat >= 0.5 && ligandRmsd > 1.0 && interfaceRmsd > 1.0)) {
			myFnat = Mathf.Min (myFnat, 0.5f);
			myLigandRmsd = Mathf.Min (5.0f, Mathf.Max (myLigandRmsd, 1.0f));
			myInterfaceRmsd = Mathf.Min (2.0f, Mathf.Max (myInterfaceRmsd, 1.0f));
		} else if (fnat >= 0.5 && (ligandRmsd <= 1.0 || interfaceRmsd <= 1.0)) {
			myLigandRmsd = Mathf.Min (1.0f, myLigandRmsd);
			myInterfaceRmsd = Mathf.Min (1.0f, myInterfaceRmsd);
		}

		myLigandRmsd = Mathf.Max (scale (myLigandRmsd, 10.0f, 0.0f, 0.1f, 1.0f), 0f);
		myInterfaceRmsd = Mathf.Max (scale (myInterfaceRmsd, 4.0f, 0.0f, 0.1f, 1.0f), 0f);
		return (myFnat + myLigandRmsd + myInterfaceRmsd)/3.0f;
	}

	static float scale(float x, float min, float max, float a, float b) {
		return (a + ((b - a) * (x - min) / (max - min)));
	}

}
