using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class AminoSliderController : MonoBehaviour {

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

	//pair of buttons
	GameObject ButtonPickedA1;
	GameObject ButtonPickedA2;
	//list of all bounds
	List<GameObject> AminoLinks = new List<GameObject>();
	//panel for the links of amino
	public GameObject AminoLinkPanel;
	public GameObject AminoLinkPanelParent;
	float[] button_displace = {16f,-5.3f};
	BioBlox BioBloxReference;


	public void init () {

		EmptyAminoSliders ();
		BioBloxReference = GameObject.Find ("BioBlox").GetComponent<BioBlox> ();
		aminoAcidsNames1 = BioBloxReference.molecules [0].GetComponent<PDB_mesh> ().mol.aminoAcidsNames;
		aminoAcidsNames2 = BioBloxReference.molecules [1].GetComponent<PDB_mesh> ().mol.aminoAcidsNames;

		for(int i = 0; i < aminoAcidsNames1.Count; ++i)
		{
			if(aminoAcidsNames1[i] != null)
			{
				GeneratesAminoButtons1(aminoAcidsNames1[i],i);
			}
		}

		for(int i = 0; i < aminoAcidsNames2.Count; ++i)
		{
			if(aminoAcidsNames2[i] != null)
			{
				GeneratesAminoButtons2(aminoAcidsNames2[i],i);
			}
		}
	}
	
	public void GeneratesAminoButtons1(string currentAmino, int index)
	{		
		//Debug.Log (currentAmino);
		AminoButtonReference = Instantiate<GameObject>(AminoButton);
		AminoButtonReference.transform.SetParent (SliderMol1.transform,false);
		AminoButtonReference.GetComponent<Image>().color = AminoColor [currentAmino];
		//Debug.Log (AminoColor [currentAmino]);
		AminoButtonReference.GetComponentInChildren<Text>().text = currentAmino.Replace(" ","");
		//set the button id
		AminoButtonReference.GetComponent<AminoButtonController> ().AminoButtonID = index;
	}

	public void GeneratesAminoButtons2(string currentAmino, int index)
	{		
		AminoButtonReference = Instantiate<GameObject>(AminoButton);
		AminoButtonReference.transform.SetParent (SliderMol2.transform,false);
		AminoButtonReference.GetComponent<Image>().color = AminoColor [currentAmino];
		AminoButtonReference.GetComponentInChildren<Text>().text = currentAmino.Replace(" ","");;
		//set the button id
		AminoButtonReference.GetComponent<AminoButtonController> ().AminoButtonID = index;
	}

	public void EmptyAminoSliders()
	{
		foreach (Transform childTransform in SliderMol1.transform) Destroy(childTransform.gameObject);
		foreach (Transform childTransform in SliderMol2.transform) Destroy(childTransform.gameObject);
	}

	public void AminoAcidsSelection(GameObject ButtonSelected)
	{
		if (ButtonSelected.transform.parent.name == "ContentPanelA1")
		{
			ButtonPickedA1 = ButtonSelected;
		}
		else
		{
			ButtonPickedA2 = ButtonSelected;
		}

		if (ButtonPickedA1 != null && ButtonPickedA2 != null)
		{
			Debug.Log ("pickeado");
			AminoLinks.Add(ButtonPickedA1);			
			AminoLinks.Add(ButtonPickedA2);
			ButtonPickedA1.GetComponent<Button>().interactable = false;
			ButtonPickedA2.GetComponent<Button>().interactable = false;

			//create the connection
			BioBloxReference.GetComponent<ConnectionManager>().CreateAminoAcidLink(BioBloxReference.molecules[0].GetComponent<PDB_mesh>(),ButtonPickedA1.GetComponent<AminoButtonController>().AminoButtonID,BioBloxReference.molecules[1].GetComponent<PDB_mesh>(),ButtonPickedA2.GetComponent<AminoButtonController>().AminoButtonID);
			
			ButtonPickedA1 = ButtonPickedA2 = null;
			AminoAcidsLinkPanel();
		}
	}

	public void AminoAcidsLinkPanel()
	{
		//Clean
		foreach (Transform childTransform in AminoLinkPanelParent.transform) Destroy(childTransform.gameObject);
		int i = 0;
		int panel_displace = 0;
		GameObject AminoLinkPanelReference;
		AminoLinkPanelReference = AminoLinkPanel;
		foreach(GameObject AminoButton in AminoLinks)
		{
			//insitaite the panel holder 
			if(i % 2 == 0)
			{				
				AminoLinkPanelReference = Instantiate<GameObject>(AminoLinkPanel);
				AminoLinkPanelReference.transform.SetParent(AminoLinkPanelParent.transform,false);
				AminoLinkPanelReference.transform.localPosition += new Vector3((33.0f * panel_displace),0,0);
				panel_displace++;
			}
			AminoButton.GetComponent<LayoutElement>().enabled = false;
			GameObject AminoButtonReference = Instantiate<GameObject>(AminoButton);
			AminoButtonReference.transform.SetParent(AminoLinkPanelReference.transform,false);
			RectTransform AminoButtonRect = AminoButtonReference.GetComponent<RectTransform>();
			AminoButtonRect.anchorMin = new Vector2(0.5f,0.5f);
			AminoButtonRect.anchorMax = new Vector2(0.5f,0.5f);
			AminoButtonRect.pivot = new Vector2(0.5f,0.5f);
			AminoButtonRect.sizeDelta = new Vector2(25, 20);
			AminoButtonRect.localPosition += new Vector3(0,button_displace[i%2],0);
			i++;
		}
	}

	public void DeleteAminoLink()
	{
		//AminoLinks.re
	}
}
