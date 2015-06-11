using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using AssemblyCSharp;
// ATOM AND CONECT LINE FORMATS
//COLUMNS        DATA TYPE       CONTENTS                            
//--------------------------------------------------------------------------------
//1 -  6         Record name     "ATOM  "                                            
//7 - 11         Integer         Atom serial number.                   
//13 - 16        Atom            Atom name.                            
//17             Character       Alternate location indicator.         
//18 - 20        Residue name    Residue name.                         
//22             Character       Chain identifier.                     
//23 - 26        Integer         Residue sequence number.              
//27             AChar           Code for insertion of residues.       
//31 - 38        Real(8.3)       Orthogonal coordinates for X in Angstroms.                       
//39 - 46        Real(8.3)       Orthogonal coordinates for Y in Angstroms.                            
//47 - 54        Real(8.3)       Orthogonal coordinates for Z in Angstroms.                            
//55 - 60        Real(6.2)       Occupancy.                            
//61 - 66        Real(6.2)       Temperature factor (Default = 0.0).                   
//73 - 76        LString(4)      Segment identifier, left-justified.   
//77 - 78        LString(2)      Element symbol, right-justified.      
//79 - 80        LString(2)      Charge on the atom. 
//COLUMNS       DATA TYPE       FIELD         DEFINITION
//-------------------------------------------------------
//1 -  6        Record name     "CONECT"
//7 - 11        Integer         serial        Atom serial number
//12 - 16       Integer         serial        Serial number of bonded atom
//17 - 21       Integer         serial        Serial number of bonded atom
//22 - 26       Integer         serial        Serial number of bonded atom
//27 - 31       Integer         serial        Serial number of bonded atom
//------------------------------------------------------
//1 -6          BioBlox name    "BBPAIR"
//7 -11			Integer			index		Index of atom in first molecule to pair
//13-16			Integer			index		Index of atom in second molecule to pair
//------------------------------------------------------
//1 -6			BioBlox name	"BBSPNG"
//7 -11			Integer			index		Index of first atom in spring
//13-16			Integer			index		Index of second atom in spring
//------------------------------------------------------
//1 -6			BioBlox name    "BIOB"
//7-11			Integer			index		Index of labeled atom
//12-16 		Integer			index		Molecule index
//17-21			LString(8)      string		Tag for the atom



public class PDB_parser {
    static private Dictionary<string, int> colour = new Dictionary<string, int> {
        {" H", 0xcccccc},{" C", 0xaaaaaa},{" O", 0xcc0000},{" N", 0x0000cc},{" S", 0xcccc00},
        {" P", 0x6622cc},{" F", 0x00cc00},{"CL", 0x00cc00},{"BR", 0x882200},{" I", 0x6600aa},
        {"FE", 0xcc6600},{"CA", 0x8888aa},
    };

    // Reference: glMol / A. Bondi, J. Phys. Chem., 1964, 68, 441.
    static private Dictionary<string, float> radii = new Dictionary<string, float> {
       {" H",  1.2f}, {"LI",  1.82f}, {"NA",  2.27f}, {" K",  2.75f}, {" C",  1.7f}, {" N",  1.55f}, {" O",  1.52f},
       {" F",  1.47f}, {" P",  1.80f}, {" S",  1.80f}, {"CL",  1.75f}, {"BR",  1.85f}, {"SE",  1.90f},
       {"ZN",  1.39f}, {"CU",  1.4f}, {"NI",  1.63f},
    };

    static public List<PDB_molecule> parse(string asset_name) {
        List<Vector3> atom_centres = new List<Vector3>();
        List<float> atom_radii = new List<float>();
        List<int> names = new List<int>();
        List<int> residues = new List<int>();
		List<Tuple<int,int>> pairs = new List<Tuple<int,int>> ();
		List<Tuple<int,int>> springPairs = new List<Tuple<int,int>> ();
		List<List<PDB_molecule.Label>> labels = new List<List<PDB_molecule.Label>> ();

        TextAsset pdbTA = (TextAsset)Resources.Load(asset_name, typeof(TextAsset));
        PDB_molecule cur = new PDB_molecule();
        List<PDB_molecule> result = new List<PDB_molecule>();
        float minx = 1e37f, miny = 1e37f, minz = 1e37f;
        float maxx = -1e37f, maxy = -1e37f, maxz = -1e37f;
        Vector3 cofg = new Vector3();
		List<int> serial_to_atom = new List<int>();
        using (StringReader reader = new StringReader(pdbTA.text)) {
            string line;
            while ((line = reader.ReadLine()) != null) {
                string kind = line.Substring(0, 6);
                if (kind == "ATOM  ") // && line.Substring(13 - 1, 4) == " N  ")
                {
                    int serial = int.Parse(line.Substring(7 - 1, 5));
                    float x = -float.Parse(line.Substring(31 - 1, 8));
                    float y = float.Parse(line.Substring(39 - 1, 8));
                    float z = float.Parse(line.Substring(47 - 1, 8));
                    float r = radii[line.Substring(77 - 1, 2)];
                    int name = PDB_molecule.encode(line[12], line[13], line[14], line[15]);
                    if (name == PDB_molecule.atom_N) {
                        residues.Add(PDB_molecule.encode(line[17], line[18], line[19], ' '));
                        residues.Add(names.Count);
                    }
                    names.Add(name);
					if (serial >= 0) {
						while (serial >= serial_to_atom.Count) serial_to_atom.Add (-1);
						serial_to_atom[serial] = atom_centres.Count;
					}
                    atom_centres.Add(new Vector3(x, y, z));
                    atom_radii.Add(r);
                    minx = Mathf.Min(minx, x); miny = Mathf.Min(miny, y); minz = Mathf.Min(minz, z);
                    maxx = Mathf.Max(maxx, x); maxy = Mathf.Max(maxy, y); maxz = Mathf.Max(maxz, z);
                } else if (kind == "CONECT") {
                    /*int len = line.Length;
                    int idx = int.Parse(line.Substring(7 - 1, 5));
                    int a = int.Parse(line.Substring(12 - 1, 5));
                    pairs.Add((idx << 16) | a);
                    string sb = line.Substring(17 - 1, 5);
                    string sc = line.Substring(22 - 1, 5);
                    if (sb != "     ") {
                        int b = int.Parse(sb);
                        pairs.Add((idx << 16) | b);
                    }
                    if (sc != "     ") {
                        int c = int.Parse(sc);
                        pairs.Add((idx << 16) | c);
                    }*/
				} else if(kind == "BBSPNG"){
					int firstMeshAtom = int.Parse(line.Substring(7, 4));
					int secondMeshAtom = int.Parse(line.Substring(13, 4));
					springPairs.Add(new Tuple<int, int>(firstMeshAtom, secondMeshAtom));
				} else if (kind == "BBPAIR") {
					int firstMeshAtom = int.Parse(line.Substring(7, 4));
					int secondMeshAtom = int.Parse(line.Substring(13, 4));
					pairs.Add(new Tuple<int, int>(firstMeshAtom, secondMeshAtom));
				} else if (kind == "BIOB  ") {
					int atomIndex = int.Parse(line.Substring(7, 4));
					int molIndex = int.Parse(line.Substring(12, 4));
					string tag = line.Substring(17, 4);
					while(labels.Count<=molIndex)
					{
						labels.Add(new List<PDB_molecule.Label>());
					}
					Debug.Log(tag+" attached to " + atomIndex + " on molecule " + molIndex);
					labels[molIndex-1].Add(new PDB_molecule.Label(atomIndex, tag));
				} else if (kind == "TER   ") {
                    cur = new PDB_molecule();
                    cur.residues = residues.ToArray();
                    cur.names = names.ToArray();
                    cur.pos.x = (maxx + minx) * 0.5f;
                    cur.pos.y = (maxy + miny) * 0.5f;
                    cur.pos.z = (maxz + minz) * 0.5f;
                    cofg += cur.pos;
                    cur.atom_centres = new Vector3[atom_centres.Count];
                    cur.atom_radii = atom_radii.ToArray(); //new float[atom_radii.Count];
					cur.serial_to_atom = serial_to_atom.ToArray();
                    for (int j = 0; j != names.Count; ++j) {
                        cur.atom_centres[j] = atom_centres[j] - cur.pos;
                        //cur.atom_radii[j] = atom_radii[j];
                    }
                    minx = 1e37f; miny = 1e37f; minz = 1e37f;
                    maxx = -1e37f; maxy = -1e37f; maxz = -1e37f;
                    result.Add(cur);
                    names.Clear();
                    atom_centres.Clear();
                    atom_radii.Clear();
                    residues.Clear();
					serial_to_atom.Clear();
                } 
            }
        }

        cofg = cofg * (1.0f / result.Count);
        Debug.Log(cofg);

        for (int i = 0; i != result.Count; ++i) {
            PDB_molecule m = result[i];
			m.pairedAtoms=pairs.ToArray();
			m.spring_pairs=springPairs.ToArray();
			if(labels.Count>i)
			{
				m.labels=labels[i].ToArray();
			}
            m.name = asset_name + "." + (i+1);
            m.build_mesh();
            m.pos -= cofg;
            //Debug.Log(m.pos);
        }
        return result;
    }

    static public Dictionary<string, PDB_molecule> mols = new Dictionary<string, PDB_molecule>();

    public static PDB_molecule get_molecule(string name) {
        if (!mols.ContainsKey(name)) {
            string[] parts = name.Split('.');
            List<PDB_molecule> list = parse(parts[0]);
            for (int i = 0; i != list.Count; ++i) {
                mols[parts[0]+"."+(i+1)] = list[i];
            }
        }
        return mols[name];
    }
}

