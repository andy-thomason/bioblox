using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class PdbInterface {

	// The CA atoms of receptor residues that have at least one atom within 10 Å of an atom on the other molecule
	private HashSet<int> rAtomsExt;
	// The CA atoms of ligand residues that have at least one atom within 10 Å of an atom on the other molecule
	private HashSet<int> lAtomsExt;
	// Pairs of residues across molecules that have any of their atoms within 5 Å
	private HashSet<Pair<int, int>> contacts;
	// All receptor residues that have at least one atom within 5 Å of an atom on the other molecule
	private HashSet<int> rResidues;
	// All ligand residues that have at least one atom within 5 Å of an atom on the other molecule
	private HashSet<int> lResidues;
	// All atoms of receptor residues that are not in the interface
	private HashSet<int> rAtomsNot;
	// All atoms of ligand residues that are not in the interface
	private HashSet<int> lAtomsNot;
	private static float interface1 = 5.0f * 5.0f;
	private static float interface2 = 10.0f * 10.0f;

	// Bioblox
	// Removed filename and changed PDB to PDB_molecule
	public PdbInterface(PDB_molecule receptor, PDB_molecule ligand) {
		rAtomsExt = new HashSet<int>();
		lAtomsExt = new HashSet<int> ();
		contacts = new HashSet<Pair<int, int>> ();
		rAtomsNot = new HashSet<int>();
		lAtomsNot = new HashSet<int>();
		rResidues = new HashSet<int> ();
		lResidues = new HashSet<int> ();
		load (receptor, ligand);
	}

	// Bioblox
	// Changed PDB to PDB_molecule
	// This must be run before pos changes. It is used to retrieve original PDB coordinates
	// All string residue ids have been changed to int
	// I changed the way atoms are iterated so that their residue name/id is known
	// without changing anything in PDB_molecule
	// I also changed the properties of this class as with PDB_molecule you cannot find
	// the residue name of an atom but you can get all atoms of a residue easily
	// TODO: check how pos is being used compared to old gc
	private void load(PDB_molecule receptor, PDB_molecule ligand) {
		HashSet<int> rResiduesExt = new HashSet<int> ();
		HashSet<int> lResiduesExt = new HashSet<int> ();
		HashSet<int> lAtoms = new HashSet<int> ();
		HashSet<int> rAtoms = new HashSet<int> ();

		// Append twice the number of atoms at the end of residues arrays
		int[] lResIds = new int[ligand.residues.Length + 2];
		ligand.residues.CopyTo (lResIds, 0);
		lResIds [ligand.residues.Length] = ligand.names.Length;
		lResIds [ligand.residues.Length + 1] = ligand.names.Length;
		int[] rResIds = new int[receptor.residues.Length + 2];
		receptor.residues.CopyTo (rResIds, 0);
		rResIds [receptor.residues.Length] = receptor.names.Length;
		rResIds [receptor.residues.Length + 1] = receptor.names.Length;

		Vector3 offset = ligand.pos - receptor.pos;
		for (int i = 1; i < ligand.residues.Length; i+=2) {
			int resi = lResIds [i - 1];
			for (int ii = lResIds[i]; ii < lResIds[i+2]; ii++) {
				Vector3 lCoords = ligand.atom_centres [ii] + offset;
				for (int j = 1; j < receptor.residues.Length; j+=2) {
					int resj = rResIds [j - 1];
					for (int jj = rResIds[j]; jj < rResIds[j+2]; jj++) {
						float distance = (lCoords - receptor.atom_centres [jj]).sqrMagnitude;
						if (distance <= interface2) {
							lResiduesExt.Add (resi);
							rResiduesExt.Add (resj);
							if (distance <= interface1) {
								contacts.Add (new Pair<int, int> (resi, resj));
								lResidues.Add (resi);
								rResidues.Add (resj);
								if (!lAtoms.Contains (ii)) {
									lAtoms.Add (ii);
								}
								if (!rAtoms.Contains(jj)) {
									rAtoms.Add (jj);
								}
							}
						}
					}
				}
			}
		}

		for (int i = 1; i < ligand.residues.Length; i+=2) {
			int res = lResIds [i - 1];
			for (int ii = lResIds[i]; ii < lResIds[i+2]; ii++) {
				if (lResiduesExt.Contains(res) && ligand.names[ii] == PDB_molecule.atom_CA) {
					lAtomsExt.Add (ii);
				}
				if (lResidues.Contains (res) && !lAtoms.Contains(ii)) {
					lAtomsNot.Add (ii);
				}
			}
		}

		for (int i = 1; i < receptor.residues.Length; i+=2) {
			int res = rResIds [i - 1];
			for (int ii = rResIds[i]; ii < rResIds[i+2]; ii++) {
				if (rResiduesExt.Contains(res) && receptor.names[ii] == PDB_molecule.atom_CA) {
					rAtomsExt.Add (ii);
				}
				if (rResidues.Contains (res) && !rAtoms.Contains(ii)) {
					rAtomsNot.Add (ii);
				}
			}
		}
	}
	
	public int NumAtomsExt {
		get { return (rAtomsExt.Count+lAtomsExt.Count); }
	}

	public HashSet<int> RAtomsExt {
		get { return rAtomsExt; }
	}
			
	public HashSet<int> LAtomsExt {
		get { return lAtomsExt; }
	}
	
	public HashSet<Pair<int, int>> Contacts {
		get { return contacts; }
	}

	public HashSet<int> RAtomsNot {
		get { return rAtomsNot; }
	}
	
	public HashSet<int> LAtomsNot {
		get { return lAtomsNot; }
	}
	
	public HashSet<int> RResidues {
		get { return rResidues; }
	}
	
	public HashSet<int> LResidues {
		get { return lAtomsExt; }
	}

	public void Export(string filename) {
		string name = Application.persistentDataPath + "/" + filename;
		Debug.Log ("Exporting to: "+name);
		StreamWriter fileWriter = File.CreateText(name);
		foreach (Pair<int, int> c in contacts) {
			fileWriter.WriteLine (c.First+"\t"+c.Second);
		}
		fileWriter.Close ();
	}

}
