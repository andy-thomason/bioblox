using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using AssemblyCSharp;

public class AminoSliderController : MonoBehaviour {

	public GameObject AminoButton;
	public GameObject AminoButtonReference;

    // todo: put these in an array	
	public GameObject[] SliderMol = new GameObject[2];

    // todo: put these in an array	
	public List<string> aminoAcidsNames1;
	public List<string> aminoAcidsNames2;
	public List<string> aminoAcidsTags1;
	public List<string> aminoAcidsTags2;

	// Currently selected buttons.
    // todo: put these in an array	
	public GameObject ButtonPickedA1;
	public GameObject ButtonPickedA2;

	// Amino acid links panel.
	public GameObject AminoLinkPanel;
	public GameObject AminoLinkPanelParent;
	public RectTransform AminoLinkBackground;
	float[] button_displace = {22.7f,-3.7f};
	BioBlox BioBloxReference;
	public List<AtomConnection> connections = new List<AtomConnection> ();
	public GameObject AddConnection;

	//slider buttons
    // todo: put these in an array	
	bool ButtonA1LDown, ButtonA1RDown, ButtonA2LDown, ButtonA2RDown = false;	
    // todo: put these in an array	
	public Scrollbar ScrollbarAmino1;
	public Scrollbar ScrollbarAmino2;
    // todo: put these in an array	
	public List<Button> A1Buttons = new List<Button> ();
	public List<Button> A2Buttons = new List<Button> ();

	//current button
	public int CurrentButtonA1, CurrentButtonA2, LastButtonA1, LastButtonA2 = 0;

	//text to link when there is the free camera
	public GameObject AddConnectionText;
	public Animator ConnectionExistMessage;

	//linked
	public GameObject LinkedGameObject;
	GameObject LinkedGameObjectReference;
	ButtonStructure buttonStructure;
    UIController uIController;

    //local score
    List<Vector2> score_aminolinks_holder_max = new List<Vector2>();
    List<Vector2> score_aminolinks_holder_elec = new List<Vector2>();
    List<Vector2> score_aminolinks_holder_len = new List<Vector2>();

    //max scores
    float score_total_max = 0;
    float score_len_max = 0;
    float score_elec_max = 0;

    //load score dropwdown
    public Dropdown DropDownLoadScore;

    //messages animator
    public Animator LPJMessage;
    public Animator ElecMessage;
    public Animator MaxMessage;

    //messages animator
    public Animator LPJLoadMessage;
    public Animator ElecLoadMessage;
    public Animator MaxLoadMessage;

    //proteins rotation
    Vector3 max_protein0;
    Vector3 max_protein1;
    Vector3 len_protein0;
    Vector3 len_protein1;
    Vector3 elec_protein0;
    Vector3 elec_protein1;

    Text[] ButtonText;

    public GameObject AminoHolderConnection;

	//dictionary for the function types of aunoacids
	//protein 1
	public List<GameObject> A1Positive = new List<GameObject>();
	public List<GameObject> A1Negative = new List<GameObject>();
	public List<GameObject> A1Hydro = new List<GameObject>();
	public List<GameObject> A1Polar = new List<GameObject>();
	public List<GameObject> A1Other = new List<GameObject>();
	//protein 2
	public List<GameObject> A2Positive = new List<GameObject>();
	public List<GameObject> A2Negative = new List<GameObject>();
	public List<GameObject> A2Hydro = new List<GameObject>();
	public List<GameObject> A2Polar = new List<GameObject>();
	public List<GameObject> A2Other = new List<GameObject>();
	public Dictionary<string, string> FunctionTypes = new Dictionary<string, string>{
		{"ALA",  "Hydro"}, {"ARG",  "Positive"}, {"ASN",  "Polar"}, {"ASP",  "Negative"}, {"CYS",  "Other"}, {"GLU",  "Negative"},
		{"GLY",  "Other"}, {"HIS",  "Positive"}, {"ILE",  "Hydro"}, {"LEU",  "Hydro"}, {"LYS",  "Positive"}, {"MET",  "Hydro"},
		{"PHE",  "Hydro"},	{"PRO",  "Other"}, {"SER",  "Polar"}, {"THR",  "Polar"}, {"GLN", "Polar"}, {"TRP",  "Hydro"},
		{"TYR",  "Hydro"}, {"VAL",  "Hydro"},
	};

	void Awake()
	{
		buttonStructure = FindObjectOfType<ButtonStructure> ();
        uIController = FindObjectOfType<UIController>();
    }

    int number_childs_A1 = 0;
    int number_childs_A2 = 0;

    void Update()
	{

		if (ButtonA1LDown)
		{
            if (ScrollbarAmino1.value > 0.0f)
            {
                A1Buttons[CurrentButtonA1].GetComponent<Animator>().SetBool("High", false);
                //save the last active one
                LastButtonA1 = CurrentButtonA1;
                //pass over non-active
                do
                {
                    if (CurrentButtonA1 == 0)
                    {
                        CurrentButtonA1 = LastButtonA1;
                        break;
                    }
                    else
                    {
                        CurrentButtonA1--;
                    }
                } while (!A1Buttons[CurrentButtonA1].IsActive());

                A1Buttons[CurrentButtonA1].GetComponent<Animator>().SetBool("High", true);
                ScrollbarAmino1.value = (float)A1Buttons[CurrentButtonA1].GetComponent<AminoButtonController>().temp_AminoButtonID / ((float)number_childs_A1 - 1);
				A1Buttons[CurrentButtonA1].GetComponent<AminoButtonController>().HighLight();
                Debug.Log("CurrentButtonA1: " + CurrentButtonA1 + " - A1Buttons.count: " + A1Buttons.Count + "- number_childs_A1: "+number_childs_A1);

            }
		}
		
		if (ButtonA1RDown)
		{
			if(ScrollbarAmino1.value < 1.0f)
			{
                A1Buttons[CurrentButtonA1].GetComponent<Animator>().SetBool("High", false);
                //save the last active one
                LastButtonA1 = CurrentButtonA1;
                //pass over non-active
                do
                {
                    if (CurrentButtonA1 == A1Buttons.Count - 1)
                    {
                        CurrentButtonA1= LastButtonA1;
                        break;
                    }
                    else
                    {
                        CurrentButtonA1++;
                    }
                } while (!A1Buttons[CurrentButtonA1].IsActive());
                

                A1Buttons[CurrentButtonA1].GetComponent<Animator>().SetBool("High", true);
                ScrollbarAmino1.value = (float)A1Buttons[CurrentButtonA1].GetComponent<AminoButtonController>().temp_AminoButtonID / ((float)number_childs_A1 - 1);
				A1Buttons[CurrentButtonA1].GetComponent<AminoButtonController>().HighLight();
               // Debug.Log("CurrentButtonA1: " + CurrentButtonA1 + " - A1Buttons.count: " + A1Buttons.Count + "- number_childs_A1: "+number_childs_A1);
            }
		}
        
        if (ButtonA2LDown)
		{			
			if(ScrollbarAmino2.value > 0.0f)
			{
                A2Buttons[CurrentButtonA2].GetComponent<Animator>().SetBool("High", false);
                //save the last active one
                LastButtonA2 = CurrentButtonA2;
                //pass over non-active
                do
                {
                    if (CurrentButtonA2 == 0)
                    {
                        CurrentButtonA2 = LastButtonA2;
                        break;
                    }
                    else
                    {
                        CurrentButtonA2--;
                    }

                } while (!A2Buttons[CurrentButtonA2].IsActive());

                A2Buttons[CurrentButtonA2].GetComponent<Animator>().SetBool("High", true);
                ScrollbarAmino2.value = (float)A2Buttons[CurrentButtonA2].GetComponent<AminoButtonController>().temp_AminoButtonID / ((float)number_childs_A2 - 1);
				A2Buttons[CurrentButtonA2].GetComponent<AminoButtonController>().HighLight();
			}
		}
		
		if (ButtonA2RDown)
		{
			if(ScrollbarAmino2.value < 1.0f)
			{

                A2Buttons[CurrentButtonA2].GetComponent<Animator>().SetBool("High", false);
                //save the last active one
                LastButtonA2 = CurrentButtonA2;
                //pass over non-active
                do
                {
                    if (CurrentButtonA2 == A2Buttons.Count - 1)
                    {
                        CurrentButtonA2 = LastButtonA2;
                        break;
                    }
                    else
                    {
                        CurrentButtonA2++;
                    }

                } while (!A2Buttons[CurrentButtonA2].IsActive());

                A2Buttons[CurrentButtonA2].GetComponent<Animator>().SetBool("High", true);
                ScrollbarAmino2.value = (float)A2Buttons[CurrentButtonA2].GetComponent<AminoButtonController>().temp_AminoButtonID / ((float)number_childs_A2 - 1);
                A2Buttons[CurrentButtonA2].GetComponent<AminoButtonController>().HighLight();
			}
		}

		if(Input.GetKeyDown (KeyCode.L) && AddConnectionText.activeSelf) {
            AddConnectionButton();
		}

	}

	public void UpdateCurrentButtonA1(int index)
	{
		A1Buttons[CurrentButtonA1].GetComponent<Animator>().SetBool("High", false);
        CurrentButtonA1 = index;
		A1Buttons[CurrentButtonA1].GetComponent<Animator>().SetBool("High", true);

    }

	public void UpdateCurrentButtonA2(int index)
	{
		A2Buttons[CurrentButtonA2].GetComponent<Animator>().SetBool("High", false);
        CurrentButtonA2 = index;
		A2Buttons[CurrentButtonA2].GetComponent<Animator>().SetBool("High", true);
    }

	public void HighLight3DMesh(int index, int molecule)
	{
		if (molecule == 0) {			
			A1Buttons [CurrentButtonA1].GetComponent<Animator>().SetBool("High", false);
            CurrentButtonA1 = index;
			A1Buttons [CurrentButtonA1].GetComponent<Animator>().SetBool("High", true);
            ScrollbarAmino1.value = (float)CurrentButtonA1 / ((float)SliderMol[0].transform.childCount - 1);
			A1Buttons [CurrentButtonA1].GetComponent<AminoButtonController> ().HighLight ();
		} else
		{
			A2Buttons [CurrentButtonA2].GetComponent<Animator>().SetBool("High", false);
            CurrentButtonA2 = index;
			A2Buttons [CurrentButtonA2].GetComponent<Animator>().SetBool("High", true);
            ScrollbarAmino2.value = (float)CurrentButtonA2 / ((float)SliderMol[1].transform.childCount - 1);
			A2Buttons [CurrentButtonA2].GetComponent<AminoButtonController> ().HighLight ();
		}
	}

    public void HighLight3DMeshAll(int index_protein1, int index_protein2)
    {
        A1Buttons[CurrentButtonA1].GetComponent<Animator>().SetBool("High", false);
        CurrentButtonA1 = index_protein1;
        A1Buttons[CurrentButtonA1].GetComponent<Animator>().SetBool("High", true);
        ScrollbarAmino1.value = (float)CurrentButtonA1 / ((float)SliderMol[0].transform.childCount - 1);
        A1Buttons[CurrentButtonA1].GetComponent<AminoButtonController>().HighLight();
        
        A2Buttons[CurrentButtonA2].GetComponent<Animator>().SetBool("High", false);
        CurrentButtonA2 = index_protein2;
        A2Buttons[CurrentButtonA2].GetComponent<Animator>().SetBool("High", true);
        ScrollbarAmino2.value = (float)CurrentButtonA2 / ((float)SliderMol[1].transform.childCount - 1);
        A2Buttons[CurrentButtonA2].GetComponent<AminoButtonController>().HighLight();   
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
				GenerateAminoButtons1(aminoAcidsNames1[i],aminoAcidsTags1[i], i);
			}
		}

		for(int i = 0; i < aminoAcidsNames2.Count; ++i)
		{
			if(aminoAcidsNames2[i] != null)
			{
				GenerateAminoButtons2(aminoAcidsNames2[i],aminoAcidsTags2[i], i);
			}
		}

        UpdateButtonTempIDA1();
        UpdateButtonTempIDA2();
    }
	
	public void GenerateAminoButtons1(string currentAmino, string tag, int index)
	{		
		//ButtonText.Initialize ();
		//Debug.Log (currentAmino);
		AminoButtonReference = Instantiate<GameObject>(AminoButton);
        //update the gameobejct name to grab later
        AminoButtonReference.name = index.ToString();

        A1Buttons.Add (AminoButtonReference.GetComponent<Button>());
		AminoButtonReference.transform.SetParent (SliderMol[0].transform,false);
		AminoButtonReference.GetComponent<Image>().color = buttonStructure.NormalColor [currentAmino];
		//store the color in the button
		AminoButtonReference.GetComponent<AminoButtonController>().NormalColor = buttonStructure.NormalColor [currentAmino];		
		AminoButtonReference.GetComponent<AminoButtonController>().FunctionColor = buttonStructure.FunctionColor [currentAmino];

		InsertButtonToListOfAminoAcidsFuntionA1 (AminoButtonReference, currentAmino);

		//Debug.Log (AminoColor [currentAmino]);
		ButtonText = AminoButtonReference.GetComponentsInChildren<Text> ();
        ButtonText[0].text = currentAmino; // currentAmino.Replace (" ", "");
		ButtonText [1].text = tag;
        AminoButtonReference.GetComponent<AminoButtonController>().name_amino = currentAmino;

        //AminoButtonReference.GetComponentsInChildrenGetComponentInChildren<Text>().text = currentAmino.Replace(" ","")+System.Environment.NewLine+tag;
        //set the button id
        AminoButtonReference.GetComponent<AminoButtonController> ().AminoButtonID = index;
	}

	public void GenerateAminoButtons2(string currentAmino, string tag, int index)
	{		
		//ButtonText.Initialize ();
		AminoButtonReference = Instantiate<GameObject>(AminoButton);
        //update the gameobejct name to grab later
        AminoButtonReference.name = index.ToString();
        A2Buttons.Add (AminoButtonReference.GetComponent<Button>());
		AminoButtonReference.transform.SetParent (SliderMol[1].transform,false);
		AminoButtonReference.GetComponent<Image>().color = buttonStructure.NormalColor [currentAmino];
		//store the color in the button
		AminoButtonReference.GetComponent<AminoButtonController>().NormalColor = buttonStructure.NormalColor [currentAmino];		
		AminoButtonReference.GetComponent<AminoButtonController>().FunctionColor = buttonStructure.FunctionColor [currentAmino];

		InsertButtonToListOfAminoAcidsFuntionA2 (AminoButtonReference, currentAmino);

		ButtonText = AminoButtonReference.GetComponentsInChildren<Text> ();
		ButtonText [0].text = currentAmino.Replace (" ", "");
		ButtonText [1].text = tag;
        AminoButtonReference.GetComponent<AminoButtonController>().name_amino = currentAmino;

        //AminoButtonReference.GetComponentInChildren<Text>().text = currentAmino.Replace(" ","")+System.Environment.NewLine+tag;
        //set the button id
        AminoButtonReference.GetComponent<AminoButtonController> ().AminoButtonID = index;
	}

	void InsertButtonToListOfAminoAcidsFuntionA1(GameObject CurrentAmino, string NameAmino)
	{
		//insert the button to the correct list
		if (FunctionTypes [NameAmino] == "Hydro")
			A1Hydro.Add (CurrentAmino);
		else if (FunctionTypes [NameAmino] == "Positive")
			A1Positive.Add (CurrentAmino);
		else if (FunctionTypes [NameAmino] == "Negative")
			A1Negative.Add (CurrentAmino);
		else if (FunctionTypes [NameAmino] == "Polar")
			A1Polar.Add (CurrentAmino);
		else if (FunctionTypes [NameAmino] == "Other")
			A1Other.Add (CurrentAmino);
	}

	void InsertButtonToListOfAminoAcidsFuntionA2(GameObject CurrentAmino, string NameAmino)
	{
		//insert the button to the correct list
		if (FunctionTypes [NameAmino] == "Hydro")
			A2Hydro.Add (CurrentAmino);
		else if (FunctionTypes [NameAmino] == "Positive")
			A2Positive.Add (CurrentAmino);
		else if (FunctionTypes [NameAmino] == "Negative")
			A2Negative.Add (CurrentAmino);
		else if (FunctionTypes [NameAmino] == "Polar")
			A2Polar.Add (CurrentAmino);
		else if (FunctionTypes [NameAmino] == "Other")
			A2Other.Add (CurrentAmino);
	}

	public void EmptyAminoSliders()
	{
		foreach (Transform childTransform in SliderMol[0].transform) Destroy(childTransform.gameObject);
		foreach (Transform childTransform in SliderMol[1].transform) Destroy(childTransform.gameObject);
		//delete connetions		
		foreach (Transform childTransform in AminoLinkPanelParent.transform) Destroy(childTransform.gameObject);
	}

	public void ChangeAminoAcidSelection(GameObject ButtonSelected)
	{
		if (ButtonSelected.transform.parent.name == "ContentPanelA1")
		{
			ButtonPickedA1 = ButtonSelected;
            //udate the slider value
            //ScrollbarAmino1.value = (float)ButtonSelected.GetComponent<AminoButtonController>().temp_AminoButtonID / ((float)number_childs_A1 - 1);
        }
		else
		{
			ButtonPickedA2 = ButtonSelected;
            //udate the slider value
            //ScrollbarAmino2.value = (float)ButtonSelected.GetComponent<AminoButtonController>().temp_AminoButtonID / ((float)number_childs_A2 - 1);
        }

		if (ButtonPickedA1 != null && ButtonPickedA2 != null && ButtonPickedA1.GetComponent<Button> ().interactable == true && ButtonPickedA2.GetComponent<Button> ().interactable == true) {
            //AddConnection.GetComponent<Animator> ().enabled = true;
            AddConnection.SetActive(true);
            AddConnectionText.SetActive(!BioBloxReference.GetComponent<UIController>().ToggleFreeCameraStatus);
		} else {
			AddConnectionText.SetActive(false);
			//DeactivateAddConnectionButton ();
		}


/*
        //if function type activated
        if (uIController.auto_filter)
        { 
            switch(buttonStructure.FunctionType[ButtonSelected.GetComponent<AminoButtonController>().name_amino])
            {//hydro - 0 / posi - 1 / polar - 2 / nega - 3 / special - 4
                case 0:
                    uIController.ToggleHydroA1();
                    break;
                case 1:
                    uIController.


                default:
                    break;
            }
        }
        Debug.Log(buttonStructure.FunctionType[ButtonSelected.GetComponent<AminoButtonController>().name_amino]);*/
    }

	public void AddConnectionButton()
	{		
		//ButtonPickedA1.GetComponent<Button>().interactable = false;
		//ButtonPickedA2.GetComponent<Button>().interactable = false;
		if (!CheckIfConnectionExist (ButtonPickedA1, ButtonPickedA2))
        {
            //ButtonPickedA1.GetComponent<Animator>().SetBool("High", false);
            //ButtonPickedA2.GetComponent<Animator>().SetBool("High", false);
            //normal size for link manger
            ButtonPickedA1.transform.localScale = new Vector3 (1, 1, 1);
			ButtonPickedA2.transform.localScale = new Vector3 (1, 1, 1);
			AminoAcidsLinkPanel (BioBloxReference.GetComponent<ConnectionManager> ().CreateAminoAcidLink (BioBloxReference.molecules [0].GetComponent<PDB_mesh> (), ButtonPickedA1.GetComponent<AminoButtonController> ().AminoButtonID, BioBloxReference.molecules [1].GetComponent<PDB_mesh> (), ButtonPickedA2.GetComponent<AminoButtonController> ().AminoButtonID), ButtonPickedA1, ButtonPickedA2);
			//ButtonPickedA1 = ButtonPickedA2 = null;
			FindObjectOfType<ConnectionManager> ().SliderStrings.interactable = true;
			AddConnectionText.SetActive (false);
			//DeactivateAddConnectionButton ();
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
		AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder> ().connection = connection;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder> ().ID_button1 = ButtonPickedA1.GetComponent<AminoButtonController> ().AminoButtonID;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder> ().ID_button2 = ButtonPickedA2.GetComponent<AminoButtonController> ().AminoButtonID;

		//UpdateBackgroundSize (AminoLinkPanelParent.transform.childCount);

		FixButton (ButtonPickedA1,AminoLinkPanelReference, 0, AminoLinkPanelReference.transform.GetChild(0).transform);
		FixButton (ButtonPickedA2,AminoLinkPanelReference, 1, AminoLinkPanelReference.transform.GetChild(0).transform);
	}

	bool CheckIfConnectionExist(GameObject A1, GameObject A2)
	{
		int A1ID = A1.GetComponent<AminoButtonController> ().AminoButtonID;
		int A2ID = A2.GetComponent<AminoButtonController> ().AminoButtonID;
		bool existe = false;

		foreach (Transform childLinks in AminoLinkPanelParent.transform)
		{
			if(childLinks.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button1 == A1ID && childLinks.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button2 == A2ID)
			{
				existe = true;
			}
		}
		return existe;
	}

	void UpdateBackgroundSize(int hijos)
	{
		Vector2 temp = AminoLinkBackground.offsetMax;
		temp.x = -231.0f + (47.0f * hijos);
		AminoLinkBackground.offsetMax = temp;
		if (hijos == 0) {
			AminoLinkBackground.offsetMax = new Vector2 (-244.0f, temp.y);
		}
	}

	void FixButton(GameObject AminoButton, GameObject AminoLinkPanelReference, int i, Transform PanelParent)
	{
		GameObject AminoButtonReference = Instantiate<GameObject>(AminoButton);
		AminoButtonReference.SetActive (true);
		AminoButtonReference.transform.position = Vector3.zero;
		AminoButtonReference.GetComponent<LayoutElement>().enabled = false;
		AminoButtonReference.GetComponent<Button> ().interactable = true;
		AminoButtonReference.GetComponent<Button> ().enabled = false;
		//deactivate the linked image
		Destroy (AminoButtonReference.transform.GetChild (2).gameObject);

		AminoButtonReference.transform.SetParent(PanelParent, false);
		RectTransform AminoButtonRect = AminoButtonReference.GetComponent<RectTransform>();
		AminoButtonRect.anchorMin = new Vector2(0.5f,0.5f);
		AminoButtonRect.anchorMax = new Vector2(0.5f,0.5f);
		AminoButtonRect.pivot = new Vector2(0.5f,0.5f);
		AminoButtonRect.sizeDelta = new Vector2(25, 25);
		AminoButtonRect.localPosition += new Vector3(0,button_displace[i],0);
	}

	public void RestoreDeletedAminoButtons(int B1, int B2)
	{
		//UpdateBackgroundSize (AminoLinkPanelParent.transform.childCount-1);
		SliderMol[0].transform.GetChild (B1).GetComponent<AminoButtonController> ().Linked = false;
		SliderMol[1].transform.GetChild (B2).GetComponent<AminoButtonController> ().Linked = false;
		Destroy(SliderMol[0].transform.GetChild (B1).transform.GetChild (2).transform.GetChild (0).gameObject);
		Destroy(SliderMol[1].transform.GetChild (B2).transform.GetChild (2).transform.GetChild (0).gameObject);
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

    public void RegisterScore()
    {
        
        //new higuest lennars score
        if (BioBloxReference.lennard_score > score_len_max)
        {
            //clear the list
            score_aminolinks_holder_len.Clear();
            //show message high score
            LPJMessage.SetBool("Play", true);
            foreach (Transform childLinks in AminoLinkPanelParent.transform)
            {
                score_aminolinks_holder_len.Add(new Vector2(childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button1, childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button2));
            }
            //set the new high score to compare
            score_len_max = BioBloxReference.lennard_score;
            //store the rotation of the molecules
            len_protein0 = BioBloxReference.molecules[0].transform.localEulerAngles;
            len_protein1 = BioBloxReference.molecules[1].transform.localEulerAngles;
        }//new higuest electrotatic score
        if (BioBloxReference.electric_score > score_elec_max)
        {
            //clear the list
            score_aminolinks_holder_elec.Clear();
            //show message high score
            ElecMessage.SetBool("Play", true);
            foreach (Transform childLinks in AminoLinkPanelParent.transform)
            {
                score_aminolinks_holder_elec.Add(new Vector2(childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button1, childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button2));
            }
            //set the new high score to compare
            score_elec_max = BioBloxReference.electric_score;
            //store the rotation of the molecules
            elec_protein0 = BioBloxReference.molecules[0].transform.localEulerAngles;
            elec_protein1 = BioBloxReference.molecules[1].transform.localEulerAngles;
        }//new higuest max score
        if (BioBloxReference.electric_score + BioBloxReference.lennard_score > score_total_max)
        {
            //clear the list
            score_aminolinks_holder_max.Clear();
            //show message high score
            MaxMessage.SetBool("Play", true);
            foreach (Transform childLinks in AminoLinkPanelParent.transform)
            {
                score_aminolinks_holder_max.Add(new Vector2(childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button1, childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button2));
            }
            //set the new high score to compare
            score_total_max = BioBloxReference.electric_score + BioBloxReference.lennard_score;
            //store the rotation of the molecules
            max_protein0 = BioBloxReference.molecules[0].transform.localEulerAngles;
            max_protein1 = BioBloxReference.molecules[1].transform.localEulerAngles;
        }
        
    }

    public void LoadScore()
    {
        DeleteAllAminoConnections();
        switch (DropDownLoadScore.value)
        {
            case 0:
                if(score_aminolinks_holder_max.Count != 0)
                {
                    for (int i = 0; i < score_aminolinks_holder_max.Count; i++)
                    {
                        //turn to normal
                        SliderMol[0].transform.Find(((int)score_aminolinks_holder_elec[i].x).ToString()).gameObject.transform.localScale = new Vector3(1, 1, 1);
                        SliderMol[1].transform.Find(((int)score_aminolinks_holder_elec[i].y).ToString()).gameObject.transform.localScale = new Vector3(1, 1, 1);
                        AminoAcidsLinkPanel(BioBloxReference.GetComponent<ConnectionManager>().CreateAminoAcidLink(BioBloxReference.molecules[0].GetComponent<PDB_mesh>(), (int)score_aminolinks_holder_max[i].x, BioBloxReference.molecules[1].GetComponent<PDB_mesh>(), (int)score_aminolinks_holder_max[i].y), SliderMol[0].transform.Find(((int)score_aminolinks_holder_max[i].x).ToString()).gameObject, SliderMol[1].transform.Find(((int)score_aminolinks_holder_max[i].y).ToString()).gameObject);
                    }
                    FindObjectOfType<ConnectionManager>().SliderStrings.interactable = true;
                    //set the rotation of the molecuels
                    BioBloxReference.molecules[0].transform.localEulerAngles = max_protein0;
                    BioBloxReference.molecules[1].transform.localEulerAngles = max_protein1;
                    //show loadede message
                    MaxLoadMessage.SetBool("Play", true);
                }
                break;
            case 1:
                if (score_aminolinks_holder_elec.Count != 0)
                {
                    for (int i = 0; i < score_aminolinks_holder_elec.Count; i++)
                    {
                        //turn to normal
                        SliderMol[0].transform.Find(((int)score_aminolinks_holder_elec[i].x).ToString()).gameObject.transform.localScale = new Vector3(1, 1, 1);
                        SliderMol[1].transform.Find(((int)score_aminolinks_holder_elec[i].y).ToString()).gameObject.transform.localScale = new Vector3(1, 1, 1);
                        AminoAcidsLinkPanel(BioBloxReference.GetComponent<ConnectionManager>().CreateAminoAcidLink(BioBloxReference.molecules[0].GetComponent<PDB_mesh>(), (int)score_aminolinks_holder_elec[i].x, BioBloxReference.molecules[1].GetComponent<PDB_mesh>(), (int)score_aminolinks_holder_elec[i].y), SliderMol[0].transform.Find(((int)score_aminolinks_holder_elec[i].x).ToString()).gameObject, SliderMol[1].transform.Find(((int)score_aminolinks_holder_elec[i].y).ToString()).gameObject);
                    }
                    FindObjectOfType<ConnectionManager>().SliderStrings.interactable = true;
                    //set the rotation of the molecuels
                    BioBloxReference.molecules[0].transform.localEulerAngles = elec_protein0;
                    BioBloxReference.molecules[1].transform.localEulerAngles = elec_protein0;
                    //show loadede message
                    ElecLoadMessage.SetBool("Play", true);
                }  
                break;
            case 2:
                if (score_aminolinks_holder_len.Count != 0)
                {
                    for (int i = 0; i < score_aminolinks_holder_len.Count; i++)
                    {
                        //turn to normal
                        SliderMol[0].transform.Find(((int)score_aminolinks_holder_elec[i].x).ToString()).gameObject.transform.localScale = new Vector3(1, 1, 1);
                        SliderMol[1].transform.Find(((int)score_aminolinks_holder_elec[i].y).ToString()).gameObject.transform.localScale = new Vector3(1, 1, 1);
                        AminoAcidsLinkPanel(BioBloxReference.GetComponent<ConnectionManager>().CreateAminoAcidLink(BioBloxReference.molecules[0].GetComponent<PDB_mesh>(), (int)score_aminolinks_holder_len[i].x, BioBloxReference.molecules[1].GetComponent<PDB_mesh>(), (int)score_aminolinks_holder_len[i].y), SliderMol[0].transform.Find(((int)score_aminolinks_holder_len[i].x).ToString()).gameObject, SliderMol[1].transform.Find(((int)score_aminolinks_holder_len[i].y).ToString()).gameObject);
                    }
                    FindObjectOfType<ConnectionManager>().SliderStrings.interactable = true;
                    //set the rotation of the molecuels
                    BioBloxReference.molecules[0].transform.localEulerAngles = len_protein0;
                    BioBloxReference.molecules[1].transform.localEulerAngles = len_protein1;
                    //show loadede message
                    LPJLoadMessage.SetBool("Play", true);
                }
                break;
            default:
                break;
        }
        //UpdateBackgroundSize(AminoLinkPanelParent.transform.childCount-1);
    }

    void DeleteAllAminoConnections()
    {
        foreach (Transform childTransform in AminoLinkPanelParent.transform) childTransform.GetComponentInChildren<AminoConnectionHolder>().DeleteLink();
    }

    /*public void DeselectAmino()
    {
        if(ButtonPickedA1 && ButtonPickedA2)
        {
            ButtonPickedA1.GetComponent<Animator>().SetBool("High", false);
            ButtonPickedA2.GetComponent<Animator>().SetBool("High", false);
            //normal size for link manger
            ButtonPickedA1.transform.localScale = new Vector3(1, 1, 1);
            ButtonPickedA2.transform.localScale = new Vector3(1, 1, 1);
            ButtonPickedA1 = ButtonPickedA2 = null;
            FindObjectOfType<ConnectionManager>().SliderStrings.interactable = false;
            //AddConnectionText.SetActive(false);
            DeactivateAddConnectionButton();
        }
    }*/

    public void ModifyConnectionHolder(AtomConnection connection, GameObject ButtonPickedA1, GameObject ButtonPickedA2,Transform HolderPanelFather)
    {
        //ButtonPickedA1.GetComponent<Animator>().SetBool("High", false);
        //ButtonPickedA2.GetComponent<Animator>().SetBool("High", false);
        //normal size for link manger
        ButtonPickedA1.transform.localScale = new Vector3(1, 1, 1);
        ButtonPickedA2.transform.localScale = new Vector3(1, 1, 1);
        //activate the linked image
        LinkedGameObjectReference = Instantiate<GameObject>(LinkedGameObject);
        LinkedGameObjectReference.transform.SetParent(ButtonPickedA1.transform.GetChild(2).transform, false);
        LinkedGameObjectReference = Instantiate<GameObject>(LinkedGameObject);
        LinkedGameObjectReference.transform.SetParent(ButtonPickedA2.transform.GetChild(2).transform, false);
        //connection mpdifi
        GameObject AminoHolderReference = Instantiate<GameObject>(AminoHolderConnection);
        AminoHolderReference.transform.SetParent(HolderPanelFather, false);
        AminoHolderReference.GetComponent<AminoConnectionHolder>().connection = connection;
        AminoHolderReference.GetComponent<AminoConnectionHolder>().ID_button1 = ButtonPickedA1.GetComponent<AminoButtonController>().AminoButtonID;
        AminoHolderReference.GetComponent<AminoConnectionHolder>().ID_button2 = ButtonPickedA2.GetComponent<AminoButtonController>().AminoButtonID;

        //UpdateBackgroundSize (AminoLinkPanelParent.transform.childCount);

        FixButton(ButtonPickedA1, AminoHolderReference, 0, AminoHolderReference.transform);
        FixButton(ButtonPickedA2, AminoHolderReference, 1, AminoHolderReference.transform);

        //hightlight th mesh
        FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>().SelectAminoAcid(ButtonPickedA2.GetComponent<AminoButtonController>().AminoButtonID);
        FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>().SelectAminoAcid(ButtonPickedA1.GetComponent<AminoButtonController>().AminoButtonID);
    }



    public void UpdateButtonTempIDA1()
    {
        number_childs_A1 = 0;
        foreach (Transform child in SliderMol[0].transform)
        {
            if (child.gameObject.activeSelf)
            {
                if (number_childs_A1 == 0) CurrentButtonA1 = child.GetComponent<AminoButtonController>().AminoButtonID;
                child.GetComponent<AminoButtonController>().temp_AminoButtonID = number_childs_A1;
                number_childs_A1++;
            }
        }
        ScrollbarAmino1.value = 0;
        Debug.Log(ScrollbarAmino1.value);
    }

    public void UpdateButtonTempIDA2()
    {
        number_childs_A2 = 0;
        foreach (Transform child in SliderMol[1].transform)
        {
            if (child.gameObject.activeSelf)
            {
                if (number_childs_A2 == 0) CurrentButtonA2 = child.GetComponent<AminoButtonController>().AminoButtonID;
                child.GetComponent<AminoButtonController>().temp_AminoButtonID = number_childs_A2;
                number_childs_A2++;
            }
        }
        ScrollbarAmino2.value = 0;
    }

    public bool IsSelected(int slider, string id) {
        GameObject button = slider == 1 ? ButtonPickedA2 : ButtonPickedA1;
        List<string> names = slider == 1 ? aminoAcidsNames2 : aminoAcidsNames1;
        List<string> tags = slider == 1 ? aminoAcidsTags2 : aminoAcidsTags1;
        if (button == null) return false;
        int bid = button.GetComponent<AminoButtonController> ().AminoButtonID;
        string val = names[bid] + " " + tags[bid];
        //Debug.Log("val=[" + val + "] id=[" + id + "]");
        return val == id;
    }

    public bool IsConnectionMade(string id1, string id2) {
        //Debug.Log("icm " + id1 + " " + id2);
		foreach (Transform childLinks in AminoLinkPanelParent.transform)
		{
            int b1 = childLinks.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button1;
            int b2 = childLinks.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button2;
            string val1 = aminoAcidsNames1[b1] + " " + aminoAcidsTags1[b1];
            string val2 = aminoAcidsNames2[b2] + " " + aminoAcidsTags2[b2];
            //Debug.Log("val1=[" + val1 + "] val2=[" + val2 + "]" + (val1 == id1 && val2 == id2));
			if (val1 == id1 && val2 == id2) {
                return true;
			}
		}
        return false;
    }

}
