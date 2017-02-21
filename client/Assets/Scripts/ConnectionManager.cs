using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using AssemblyCSharp;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class ConnectionManager : MonoBehaviour
{

	int numChainClicks = 0;

    //int contractionKVal;
    BioBlox bb;
    LineRenderer line_renderer;
    AminoSliderController asc;

    public float dampingFactor = 1.0f;
	//public float force = 10.0f;
	public float minDistance = 0.0f;
	public float maxDistance = 60.0f;
	public float spring_constant = 10000.0f;
	public Slider SliderStrings;
    SFX sfx;

    public float[] connectionMinDistances;

	public bool shouldContract = false;
	

	public float slideBackSpeed = 2.0f;

	public List<AtomConnection> connections = new List<AtomConnection> ();
	public int nc;
    public bool scrolling_up = false;
    public bool scrolling_down = false;
    float current_slider_value = 1;

	public void StringManager()
	{
        //delay the submit button for 5 secs everytime the slider is moved
        bb.is_score_valid = false;

        for (int i = 0; i<connectionMinDistances.Length; i++)
		{
			connectionMinDistances[i] = 60 * SliderStrings.value;
        }

        if (SliderStrings.value < current_slider_value && !scrolling_up)
        {
            sfx.ReelSound(SFX.sound_index.string_reel_in);
            scrolling_up = true;
            scrolling_down = false;
        }
        else if(SliderStrings.value > current_slider_value && !scrolling_down)
        {
            sfx.ReelSound(SFX.sound_index.string_reel_out);
            scrolling_up = false;
            scrolling_down = true;
        }

        if (SliderStrings.value == SliderStrings.maxValue || SliderStrings.value == SliderStrings.minValue || SliderStrings.value == current_slider_value)
            sfx.StopReel();

        //Debug.Log(" current_slider_value: " + current_slider_value + " = SliderStrings.value:" + SliderStrings.value);
        current_slider_value = SliderStrings.value;
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
			connectionMinDistances[i] = maxDistance * SliderStrings.value;
		}
		
		shouldContract = true;

		return con;
	}

	public AtomConnection CreateAminoAcidLink(PDB_mesh mol1, int amino_acid_index1, PDB_mesh mol2, int amino_acid_index2)
    {
        int[] atoms1 = mol1.mol.aminoAcidsAtomIds [amino_acid_index1];
        //picking the last atom of the chain of the amino acid 1
        //if no atom clicked - normal
        int index1 = asc.atom_selected_p1 == -1 ? atoms1 [atoms1.Length - 1] : atoms1[asc.atom_selected_p1];

        int[] atoms2 = mol2.mol.aminoAcidsAtomIds [amino_acid_index2];
        //picking the last atom of the chain of the amino acid 2
        //if no atom clicked - normal
        int index2 = asc.atom_selected_p2 == -1 ? atoms2 [atoms2.Length - 1] : atoms2[asc.atom_selected_p2];
		return CreateLink (mol1, index1, mol2, index2);
	}

    public AtomConnection CreateAminoAcidLink_atom_modification(PDB_mesh mol1, int amino_acid_index1, int atom_index1, PDB_mesh mol2, int amino_acid_index2, int atom_index2)
    {
        int[] atoms1 = mol1.mol.aminoAcidsAtomIds[amino_acid_index1];
        //picking the last atom of the chain of the amino acid 1
        //if no atom clicked - normal
        int index1 = atoms1[atom_index1];

        int[] atoms2 = mol2.mol.aminoAcidsAtomIds[amino_acid_index2];
        //picking the last atom of the chain of the amino acid 2
        //if no atom clicked - normal
        int index2 = atoms2[atom_index2];
        return CreateLink(mol1, index1, mol2, index2);
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
        bb = FindObjectOfType<BioBlox>();
        line_renderer = GameObject.FindObjectOfType<LineRenderer>() as LineRenderer;
        sfx = FindObjectOfType<SFX>();
        asc = FindObjectOfType<AminoSliderController>();
    }


	void FixedUpdate()
	{
		if (shouldContract) {
			for(int i=0; i < connections.Count; ++i)
			{
				connections[i].Update(spring_constant, dampingFactor, bb.stringForce, connectionMinDistances[i]);
			}
		}
	}

	// Update is called once per frame
	void Update () 
    {
		for (int i = 0; i < connections.Count; ++i)
        {
			connections[i].Draw(line_renderer);
		}
	}
}
