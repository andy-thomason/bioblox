using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using AssemblyCSharp;

public class AminoSliderController : MonoBehaviour {

	public GameObject AminoButton;
	GameObject AminoButtonReference;
	public GameObject SliderMol1;
	public GameObject SliderMol2;
	
	List<string> aminoAcidsNames1;
	List<string> aminoAcidsNames2;
	List<string> aminoAcidsTags1;
	List<string> aminoAcidsTags2;

	//pair of buttons
	GameObject ButtonPickedA1;
	GameObject ButtonPickedA2;
	//panel for the links of amino
	public GameObject AminoLinkPanel;
	public GameObject AminoLinkPanelParent;
	public RectTransform AminoLinkBackground;
	float[] button_displace = {22.7f,-3.7f};
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
	//text to link when there is the free camera
	public GameObject AddConnectionText;
	public Animator ConnectionExistMessage;
	//linked
	public GameObject LinkedGameObject;
	GameObject LinkedGameObjectReference;
	ButtonStructure buttonStructure;

	void Awake()
	{
		buttonStructure = FindObjectOfType<ButtonStructure> ();
	}

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

		if(Input.GetKeyDown (KeyCode.L) && AddConnectionText.activeSelf) {
			AddConnectionButton();
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
		aminoAcidsTags1 = BioBloxReference.molecules [0].GetComponent<PDB_mesh> ().mol.aminoAcidsTags;
		aminoAcidsTags2 = BioBloxReference.molecules [1].GetComponent<PDB_mesh> ().mol.aminoAcidsTags;

		for(int i = 0; i < aminoAcidsNames1.Count; ++i)
		{
			if(aminoAcidsNames1[i] != null)
			{
				GeneratesAminoButtons1(aminoAcidsNames1[i],aminoAcidsTags1[i], i);
			}
		}

		for(int i = 0; i < aminoAcidsNames2.Count; ++i)
		{
			if(aminoAcidsNames2[i] != null)
			{
				GeneratesAminoButtons2(aminoAcidsNames2[i],aminoAcidsTags2[i], i);
			}
		}
	}

	Text[] ButtonText;
	
	public void GeneratesAminoButtons1(string currentAmino, string tag, int index)
	{		
		//ButtonText.Initialize ();
		//Debug.Log (currentAmino);
		AminoButtonReference = Instantiate<GameObject>(AminoButton);
		A1Buttons.Add (AminoButtonReference.GetComponent<Button>());
		AminoButtonReference.transform.SetParent (SliderMol1.transform,false);
		AminoButtonReference.GetComponent<Image>().color = buttonStructure.NormalColor [currentAmino];
		//store the color in the button
		AminoButtonReference.GetComponent<AminoButtonController>().NormalColor = buttonStructure.NormalColor [currentAmino];		
		AminoButtonReference.GetComponent<AminoButtonController>().ChargedColor = buttonStructure.ChargedColor [currentAmino];
		//Debug.Log (AminoColor [currentAmino]);
		ButtonText = AminoButtonReference.GetComponentsInChildren<Text> ();
		ButtonText [0].text = currentAmino.Replace (" ", "");
		ButtonText [1].text = tag;

		//AminoButtonReference.GetComponentsInChildrenGetComponentInChildren<Text>().text = currentAmino.Replace(" ","")+System.Environment.NewLine+tag;
		//set the button id
		AminoButtonReference.GetComponent<AminoButtonController> ().AminoButtonID = index;
	}

	public void GeneratesAminoButtons2(string currentAmino, string tag, int index)
	{		
		//ButtonText.Initialize ();
		AminoButtonReference = Instantiate<GameObject>(AminoButton);
		A2Buttons.Add (AminoButtonReference.GetComponent<Button>());
		AminoButtonReference.transform.SetParent (SliderMol2.transform,false);
		AminoButtonReference.GetComponent<Image>().color = buttonStructure.NormalColor [currentAmino];
		//store the color in the button
		AminoButtonReference.GetComponent<AminoButtonController>().NormalColor = buttonStructure.NormalColor [currentAmino];		
		AminoButtonReference.GetComponent<AminoButtonController>().ChargedColor = buttonStructure.ChargedColor [currentAmino];

		ButtonText = AminoButtonReference.GetComponentsInChildren<Text> ();
		ButtonText [0].text = currentAmino.Replace (" ", "");
		ButtonText [1].text = tag;

		//AminoButtonReference.GetComponentInChildren<Text>().text = currentAmino.Replace(" ","")+System.Environment.NewLine+tag;
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

		if (ButtonPickedA1 != null && ButtonPickedA2 != null && ButtonPickedA1.GetComponent<Button> ().interactable == true && ButtonPickedA2.GetComponent<Button> ().interactable == true) {
			AddConnection.GetComponent<Animator> ().enabled = true;
			AddConnectionText.SetActive(!BioBloxReference.GetComponent<UIController>().ToggleFreeCameraStatus);
		} else {
			AddConnectionText.SetActive(false);
			DeactivateAddConnectionButton ();
		}
	}

	public void AddConnectionButton()
	{		
		//ButtonPickedA1.GetComponent<Button>().interactable = false;
		//ButtonPickedA2.GetComponent<Button>().interactable = false;
		if (!CheckIfConnectionExist (ButtonPickedA1, ButtonPickedA2)) {
			ButtonPickedA1.transform.localScale = new Vector3 (1, 1, 1);
			ButtonPickedA2.transform.localScale = new Vector3 (1, 1, 1);
			AminoAcidsLinkPanel (BioBloxReference.GetComponent<ConnectionManager> ().CreateAminoAcidLink (BioBloxReference.molecules [0].GetComponent<PDB_mesh> (), ButtonPickedA1.GetComponent<AminoButtonController> ().AminoButtonID, BioBloxReference.molecules [1].GetComponent<PDB_mesh> (), ButtonPickedA2.GetComponent<AminoButtonController> ().AminoButtonID), ButtonPickedA1, ButtonPickedA2);
			ButtonPickedA1 = ButtonPickedA2 = null;
			FindObjectOfType<ConnectionManager> ().SliderStrings.interactable = true;
			AddConnectionText.SetActive (false);
			DeactivateAddConnectionButton ();
		}
		else
		{
			ConnectionExistMessage.SetBool("Play",true);
		}
	}

	void DeactivateAddConnectionButton()
	{
		AddConnection.GetComponent<Animator>().enabled = false;
		AddConnection.GetComponent<Button> ().enabled = false;
		Color c = AddConnection.GetComponent<Image>().color;
		c.a = 0.3f;
		AddConnection.GetComponent<Image> ().color = c;
	}

	public void AminoAcidsLinkPanel(AtomConnection connection, GameObject ButtonPickedA1, GameObject ButtonPickedA2)
	{
		GameObject AminoLinkPanelReference;
		//activate the linked image
		LinkedGameObjectReference = Instantiate<GameObject>(LinkedGameObject);		
		LinkedGameObjectReference.transform.SetParent(ButtonPickedA1.transform.GetChild(2).transform,false);		
		LinkedGameObjectReference = Instantiate<GameObject>(LinkedGameObject);		
		LinkedGameObjectReference.transform.SetParent(ButtonPickedA2.transform.GetChild(2).transform,false);

		//ButtonPickedA1.transform.GetChild (2).gameObject.SetActive (true);
		//ButtonPickedA2.transform.GetChild (2).gameObject.SetActive (true);
		//ButtonPickedA2.GetComponent<AminoButtonController> ().activateLinkedImage ();
		AminoLinkPanelReference = Instantiate<GameObject>(AminoLinkPanel);
		AminoLinkPanelReference.transform.SetParent(AminoLinkPanelParent.transform,false);
		AminoLinkPanelReference.GetComponent<AminoConnectionHolder> ().connection = connection;
		AminoLinkPanelReference.GetComponent<AminoConnectionHolder> ().ID_button1 = ButtonPickedA1.GetComponent<AminoButtonController> ().AminoButtonID;
		AminoLinkPanelReference.GetComponent<AminoConnectionHolder> ().ID_button2 = ButtonPickedA2.GetComponent<AminoButtonController> ().AminoButtonID;

		UpdateBackGroundSize (AminoLinkPanelParent.transform.childCount);

		FixButton (ButtonPickedA1,AminoLinkPanelReference, 0);
		FixButton (ButtonPickedA2,AminoLinkPanelReference, 1);
	}

	bool CheckIfConnectionExist(GameObject A1, GameObject A2)
	{
		int A1ID = A1.GetComponent<AminoButtonController> ().AminoButtonID;
		int A2ID = A2.GetComponent<AminoButtonController> ().AminoButtonID;
		bool existe = false;

		foreach (Transform childLinks in AminoLinkPanelParent.transform)
		{
			if(childLinks.GetComponent<AminoConnectionHolder>().ID_button1 == A1ID && childLinks.GetComponent<AminoConnectionHolder>().ID_button2 == A2ID)
			{
				existe = true;
			}
		}
		return existe;
	}

	void UpdateBackGroundSize(int hijos)
	{
		Vector2 temp = AminoLinkBackground.offsetMax;
		temp.x = -231.0f + (47.0f * hijos);
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
		//deactivate the linked image
		Destroy (AminoButtonReference.transform.GetChild (2).gameObject);

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
		SliderMol1.transform.GetChild (B1).GetComponent<AminoButtonController> ().Linked = false;
		SliderMol2.transform.GetChild (B2).GetComponent<AminoButtonController> ().Linked = false;
		Destroy(SliderMol1.transform.GetChild (B1).transform.GetChild (2).transform.GetChild (0).gameObject);
		Destroy(SliderMol2.transform.GetChild (B2).transform.GetChild (2).transform.GetChild (0).gameObject);
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
