using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AminoButtonController : MonoBehaviour {

	static private Dictionary<string, Color32> AminoColor = new Dictionary<string, Color32> {
		{" ALA",  new Color32(255, 255, 255, 255)}, {" ARG",  new Color32(253, 125, 125, 255)}, {" ASN",  new Color32(248, 221, 173, 255)}, {" ASP",  new Color32(253, 198, 0, 255)}, {" CYS",  new Color32(220, 234, 153, 255)}, {" GLU",  new Color32(216, 244, 88, 255)}, {" GLY",  new Color32(159, 233, 148, 255)},
		{" HIS",  new Color32(53, 218, 22, 255)}, {" ILE",  new Color32(140, 221, 194, 255)}, {" LEU",  new Color32(44, 219, 212, 255)}, {" LYS",  new Color32(44, 161, 219, 255)}, {" MET",  new Color32(0, 255, 160, 255)}, {" PHE",  new Color32(161, 164, 223, 255)},
		{" PRO",  new Color32(194, 161, 223, 255)}, {" SER",  new Color32(223, 161, 223, 255)}, {" THR",  new Color32(223, 161, 221, 255)}, {" GLN",  new Color32(242, 105, 164, 255)}, {" TRP",  new Color32(176, 159, 167, 255)}, {" TYR",  new Color32(234, 162, 169, 255)}, {" VAL",  new Color32(249, 101, 116, 255)},
	};

	public GameObject AminoButton;
	GameObject AminoButtonReference;
	public GameObject SliderMol1;
	public GameObject SliderMol2;
	
	List<string> aminoAcidsNames1;
	List<string> aminoAcidsNames2;


	public void init () {

		EmptyAminoSliders ();
		BioBlox BioBloxReference = GameObject.Find ("BioBlox").GetComponent<BioBlox> ();
		aminoAcidsNames1 = BioBloxReference.molecules [0].GetComponent<PDB_mesh> ().mol.aminoAcidsNames;
		aminoAcidsNames2 = BioBloxReference.molecules [1].GetComponent<PDB_mesh> ().mol.aminoAcidsNames;

		for(int i = 0; i < aminoAcidsNames1.Count; ++i)
		{
			if(aminoAcidsNames1[i] != null)
			{
				GeneratesAminoButtons1(aminoAcidsNames1[i]);
			}
		}

		for(int i = 0; i < aminoAcidsNames2.Count; ++i)
		{
			if(aminoAcidsNames2[i] != null)
			{
				GeneratesAminoButtons2(aminoAcidsNames2[i]);
			}
		}
	}
	
	public void GeneratesAminoButtons1(string currentAmino)
	{		
		Debug.Log (currentAmino);
		AminoButtonReference = Instantiate<GameObject>(AminoButton);
		AminoButtonReference.transform.SetParent (SliderMol1.transform,false);
		AminoButtonReference.GetComponent<Image>().color = AminoColor [currentAmino];
		Debug.Log (AminoColor [currentAmino]);
		AminoButtonReference.GetComponentInChildren<Text>().text = currentAmino.Replace(" ","");
	}

	public void GeneratesAminoButtons2(string currentAmino)
	{		
		AminoButtonReference = Instantiate<GameObject>(AminoButton);
		AminoButtonReference.transform.SetParent (SliderMol2.transform,false);
		AminoButtonReference.GetComponent<Image>().color = AminoColor [currentAmino];
		AminoButtonReference.GetComponentInChildren<Text>().text = currentAmino.Replace(" ","");;
	}

	public void EmptyAminoSliders()
	{
		foreach (Transform childTransform in SliderMol1.transform) Destroy(childTransform.gameObject);
		foreach (Transform childTransform in SliderMol2.transform) Destroy(childTransform.gameObject);
	}
}
