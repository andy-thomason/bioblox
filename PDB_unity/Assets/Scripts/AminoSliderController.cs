using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using AssemblyCSharp;

public class AminoSliderController : MonoBehaviour {

	//static public Color32[] AminoButtonsColors = new Color32[22];

	static private Dictionary<string, Color32> AminoColor = new Dictionary<string, Color32> {
		{" ALA",  new Color32(255, 255, 255, 255)}, {" ARG",  new Color32(253, 125, 125, 255)}, {" ASN",  new Color32(248, 221, 173, 255)}, {" ASP",  new Color32(253, 198, 0, 255)}, {" CYS",  new Color32(220, 234, 153, 255)}, {" GLU",  new Color32(216, 244, 88, 255)}, {" GLY",  new Color32(159, 233, 148, 255)},
		{" HIS",  new Color32(53, 218, 22, 255)}, {" ILE",  new Color32(140, 221, 194, 255)}, {" LEU",  new Color32(44, 219, 212, 255)}, {" LYS",  new Color32(44, 161, 219, 255)}, {" MET",  new Color32(0, 255, 160, 255)}, {" PHE",  new Color32(161, 164, 223, 255)},
		{" PRO",  new Color32(194, 161, 223, 255)}, {" SER",  new Color32(223, 161, 223, 255)}, {" THR",  new Color32(223, 161, 221, 255)}, {" GLN",  new Color32(242, 105, 164, 255)}, {" TRP",  new Color32(176, 159, 167, 255)}, {" TYR",  new Color32(234, 162, 169, 255)}, {" VAL",  new Color32(249, 101, 116, 255)},
	};

	/*static private Dictionary<string, Color32> AminoColor = new Dictionary<string, Color32> {
		{" ALA",  AminoButtonsColors[0]}, {" ARG",  AminoButtonsColors[1]}, {" ASN",  AminoButtonsColors[2]}, {" ASP",  AminoButtonsColors[3]}, {" CYS",  AminoButtonsColors[4]}, {" GLU",  AminoButtonsColors[5]}, {" GLY",  AminoButtonsColors[6]},
		{" HIS",  AminoButtonsColors[7]}, {" ILE",  AminoButtonsColors[8]}, {" LEU",  AminoButtonsColors[9]}, {" LYS",  AminoButtonsColors[10]}, {" MET",  AminoButtonsColors[11]}, {" PHE",  AminoButtonsColors[12]},
		{" PRO",  AminoButtonsColors[13]}, {" SER",  AminoButtonsColors[14]}, {" THR",  AminoButtonsColors[15]}, {" GLN", AminoButtonsColors[16]}, {" TRP",  AminoButtonsColors[17]}, {" TYR",  AminoButtonsColors[18]}, {" VAL",  AminoButtonsColors[19]},
	};*/

	public GameObject AminoButton;
	GameObject AminoButtonReference;
	public GameObject SliderMol1;
	public GameObject SliderMol2;
	
	List<string> aminoAcidsNames1;
	List<string> aminoAcidsNames2;

	//pair of buttons
	GameObject ButtonPickedA1;
	GameObject ButtonPickedA2;
	//panel for the links of amino
	public GameObject AminoLinkPanel;
	public GameObject AminoLinkPanelParent;
	public RectTransform AminoLinkBackground;
	float[] button_displace = {18.5f,-7f};
	BioBlox BioBloxReference;
	public List<AtomConnection> connections = new List<AtomConnection> ();
	public GameObject AddConnection;
	//slider buttons
	bool ButtonA1LDown, ButtonA1RDown, ButtonA2LDown, ButtonA2RDown = false;	
	public Scrollbar ScrollbarAmino1;
	public Scrollbar ScrollbarAmino2;
	List<Button> A1Buttons = new List<Button> ();
	List<Button> A2Buttons = new List<Button> ();
	//current button
	public int CurrentButtonA1, CurrentButtonA2 = 0;
	void Update()
	{

		if (ButtonA1LDown)
		{
			if(CurrentButtonA1>0)
			{
				A1Buttons[CurrentButtonA1].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
				CurrentButtonA1 --;
				A1Buttons[CurrentButtonA1].transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
				ScrollbarAmino1.value = (float)CurrentButtonA1 / ((float)SliderMol1.transform.childCount-1);
				A1Buttons[CurrentButtonA1].GetComponent<AminoButtonController>().HighLight();
			}
		}
		
		if (ButtonA1RDown)
		{
			if(CurrentButtonA1<SliderMol1.transform.childCount-1)
			{
				A1Buttons[CurrentButtonA1].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
				CurrentButtonA1 ++;
				A1Buttons[CurrentButtonA1].transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
				ScrollbarAmino1.value = (float)CurrentButtonA1 / ((float)SliderMol1.transform.childCount-1);
				A1Buttons[CurrentButtonA1].GetComponent<AminoButtonController>().HighLight();
			}
		}
		
		if (ButtonA2LDown)
		{			
			if(CurrentButtonA2>0)
			{
				A2Buttons[CurrentButtonA2].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
				CurrentButtonA2 --;
				A2Buttons[CurrentButtonA2].transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
				ScrollbarAmino2.value = (float)CurrentButtonA2 / ((float)SliderMol2.transform.childCount-1);
				A2Buttons[CurrentButtonA2].GetComponent<AminoButtonController>().HighLight();
			}
		}
		
		if (ButtonA2RDown)
		{
			if(CurrentButtonA2<SliderMol2.transform.childCount-1)
			{
				A2Buttons[CurrentButtonA2].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
				CurrentButtonA2 ++;
				A2Buttons[CurrentButtonA2].transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
				ScrollbarAmino2.value = (float)CurrentButtonA2 / ((float)SliderMol2.transform.childCount-1);
				A2Buttons[CurrentButtonA2].GetComponent<AminoButtonController>().HighLight();
			}
		}
	}

	public void UpdateCurrentButtonA1(int index)
	{
		A1Buttons[CurrentButtonA1].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
		CurrentButtonA1 = index;
		A1Buttons[CurrentButtonA1].transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);

	}

	public void UpdateCurrentButtonA2(int index)
	{
		A2Buttons[CurrentButtonA2].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
		CurrentButtonA2 = index;
		A2Buttons[CurrentButtonA2].transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
	}

	public void HighLight3DMesh(int index, int molecule)
	{
		if (molecule == 0) {			
			A1Buttons [CurrentButtonA1].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			CurrentButtonA1 = index;
			A1Buttons [CurrentButtonA1].transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
			ScrollbarAmino1.value = (float)CurrentButtonA1 / ((float)SliderMol1.transform.childCount - 1);
			A1Buttons [CurrentButtonA1].GetComponent<AminoButtonController> ().HighLight ();
		} else
		{
			A2Buttons [CurrentButtonA2].transform.localScale = new Vector3 (1.0f, 1.0f, 1.0f);
			CurrentButtonA2 = index;
			A2Buttons [CurrentButtonA2].transform.localScale = new Vector3 (1.3f, 1.3f, 1.3f);
			ScrollbarAmino2.value = (float)CurrentButtonA2 / ((float)SliderMol2.transform.childCount - 1);
			A2Buttons [CurrentButtonA2].GetComponent<AminoButtonController> ().HighLight ();
		}
	}


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
		A1Buttons.Add (AminoButtonReference.GetComponent<Button>());
		AminoButtonReference.transform.SetParent (SliderMol1.transform,false);
		AminoButtonReference.GetComponent<Image>().color = AminoColor [currentAmino];
		//Debug.Log (AminoColor [currentAmino]);
		AminoButtonReference.GetComponentInChildren<Text>().text = currentAmino.Replace(" ","")+System.Environment.NewLine+index;
		//set the button id
		AminoButtonReference.GetComponent<AminoButtonController> ().AminoButtonID = index;
	}

	public void GeneratesAminoButtons2(string currentAmino, int index)
	{		
		AminoButtonReference = Instantiate<GameObject>(AminoButton);
		A2Buttons.Add (AminoButtonReference.GetComponent<Button>());
		AminoButtonReference.transform.SetParent (SliderMol2.transform,false);
		AminoButtonReference.GetComponent<Image>().color = AminoColor [currentAmino];
		AminoButtonReference.GetComponentInChildren<Text>().text = currentAmino.Replace(" ","")+System.Environment.NewLine+index;
		//set the button id
		AminoButtonReference.GetComponent<AminoButtonController> ().AminoButtonID = index;
	}

	public void EmptyAminoSliders()
	{
		foreach (Transform childTransform in SliderMol1.transform) Destroy(childTransform.gameObject);
		foreach (Transform childTransform in SliderMol2.transform) Destroy(childTransform.gameObject);
		//delete connetions		
		foreach (Transform childTransform in AminoLinkPanelParent.transform) Destroy(childTransform.gameObject);
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
			AddConnection.GetComponent<Animator>().enabled = true;
			BioBloxReference.EnableSlider();
		}
	}

	public void AddConnectionButton()
	{		
		ButtonPickedA1.GetComponent<Button>().interactable = false;
		ButtonPickedA2.GetComponent<Button>().interactable = false;
		ButtonPickedA1.transform.localScale = new Vector3 (1, 1, 1);
		ButtonPickedA2.transform.localScale = new Vector3 (1, 1, 1);
		AminoAcidsLinkPanel(BioBloxReference.GetComponent<ConnectionManager>().CreateAminoAcidLink(BioBloxReference.molecules[0].GetComponent<PDB_mesh>(),ButtonPickedA1.GetComponent<AminoButtonController>().AminoButtonID,BioBloxReference.molecules[1].GetComponent<PDB_mesh>(),ButtonPickedA2.GetComponent<AminoButtonController>().AminoButtonID),ButtonPickedA1,ButtonPickedA2);
		
		ButtonPickedA1 = ButtonPickedA2 = null;
		AddConnection.GetComponent<Animator>().enabled = false;
		AddConnection.GetComponent<Button> ().enabled = false;
		Color c = AddConnection.GetComponent<Image>().color;
		c.a = 0.3f;
		AddConnection.GetComponent<Image> ().color = c;
	}

	public void AminoAcidsLinkPanel(AtomConnection connection, GameObject ButtonPickedA1, GameObject ButtonPickedA2)
	{
		GameObject AminoLinkPanelReference;
		AminoLinkPanelReference = Instantiate<GameObject>(AminoLinkPanel);
		AminoLinkPanelReference.transform.SetParent(AminoLinkPanelParent.transform,false);
		AminoLinkPanelReference.GetComponent<AminoConnectionHolder> ().connection = connection;
		AminoLinkPanelReference.GetComponent<AminoConnectionHolder> ().ID_button1 = ButtonPickedA1.GetComponent<AminoButtonController> ().AminoButtonID;
		AminoLinkPanelReference.GetComponent<AminoConnectionHolder> ().ID_button2 = ButtonPickedA2.GetComponent<AminoButtonController> ().AminoButtonID;

		UpdateBackGroundSize (AminoLinkPanelParent.transform.childCount);

		FixButton (ButtonPickedA1,AminoLinkPanelReference, 0);
		FixButton (ButtonPickedA2,AminoLinkPanelReference, 1);
	}

	void UpdateBackGroundSize(int hijos)
	{
		Vector2 temp = AminoLinkBackground.offsetMax;
		temp.x = -235.0f + (35.0f * hijos);
		AminoLinkBackground.offsetMax = temp;
		if (hijos == 0) {
			AminoLinkBackground.offsetMax = new Vector2 (-244.0f, temp.y);
		}
	}

	void FixButton(GameObject AminoButton, GameObject AminoLinkPanelReference, int i)
	{
		GameObject AminoButtonReference = Instantiate<GameObject>(AminoButton);
		AminoButtonReference.GetComponent<LayoutElement>().enabled = false;
		AminoButtonReference.GetComponent<Button> ().interactable = true;
		AminoButtonReference.GetComponent<Button> ().enabled = false;

		AminoButtonReference.transform.SetParent(AminoLinkPanelReference.transform,false);
		RectTransform AminoButtonRect = AminoButtonReference.GetComponent<RectTransform>();
		AminoButtonRect.anchorMin = new Vector2(0.5f,0.5f);
		AminoButtonRect.anchorMax = new Vector2(0.5f,0.5f);
		AminoButtonRect.pivot = new Vector2(0.5f,0.5f);
		AminoButtonRect.sizeDelta = new Vector2(25, 25);
		AminoButtonRect.localPosition += new Vector3(0,button_displace[i],0);
	}

	public void RestoreDeletedAminoButtons(int B1, int B2)
	{
		UpdateBackGroundSize (AminoLinkPanelParent.transform.childCount-1);
		SliderMol1.transform.GetChild (B1).GetComponent<Button> ().interactable = true;
		SliderMol2.transform.GetChild (B2).GetComponent<Button> ().interactable = true;
	}

	//SLIDER BUTTONS
	
	public void A1LDown()
	{
		ButtonA1LDown = true;
	}

	public void A1LUp()
	{
		ButtonA1LDown = false;
	}

	public void A1RDown()
	{
		ButtonA1RDown = true;
	}
	
	public void A1RUp()
	{
		ButtonA1RDown = false;
	}

	public void A2LDown()
	{
		ButtonA2LDown = true;
	}
	
	public void A2LUp()
	{
		ButtonA2LDown = false;
	}

	public void A2RDown()
	{
		ButtonA2RDown = true;
	}
	
	public void A2RUp()
	{
		ButtonA2RDown = false;
	}
}
