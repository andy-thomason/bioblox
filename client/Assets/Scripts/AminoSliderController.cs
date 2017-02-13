using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using AssemblyCSharp;

public class AminoSliderController : MonoBehaviour
{

    public GameObject AminoButton;
    public GameObject AminoButtonReference;

    // todo: put these in an array	
    public GameObject[] SliderMol = new GameObject[2];

    // todo: put these in an array	
    public List<string>[] aminoAcidsNames = new List<string>[2];
    public List<string>[] aminoAcidsTags = new List<string>[2];

    // Currently selected buttons.
    // todo: put these in an array	
    public GameObject ButtonPickedA1;
    public GameObject ButtonPickedA2;

    // Amino acid links panel.
    public GameObject AminoLinkPanel;
    public GameObject AminoLinkPanelParent;
    public RectTransform AminoLinkBackground;
    float[] button_displace = { 22.7f, -3.7f };
    BioBlox BioBloxReference;
    public List<AtomConnection> connections = new List<AtomConnection>();
    public CanvasGroup AddConnection;

    //slider buttons
    // todo: put these in an array	
    bool ButtonA1LDown, ButtonA1RDown, ButtonA2LDown, ButtonA2RDown = false;
    // todo: put these in an array	
    public Scrollbar ScrollbarAmino1;
    public Scrollbar ScrollbarAmino2;
    // todo: put these in an array	
    public List<AminoButtonController> A1Buttons;
    public List<AminoButtonController> A2Buttons;

    //current button
    public int CurrentButtonA1, CurrentButtonA2, LastButtonA1, LastButtonA2 = 0;

    //text to link when there is the free camera
    public Animator ConnectionExistMessage;

    //linked
    public GameObject LinkedGameObject;
    GameObject LinkedGameObjectReference;
    public GameObject LinkedAtom;
    ButtonStructure buttonStructure;
    UIController uIController;

    //local score
    List<Vector2> score_aminolinks_holder_max = new List<Vector2>();
    List<Vector2> score_aminolinks_holder_elec = new List<Vector2>();
    List<Vector2> score_aminolinks_holder_len = new List<Vector2>();

    //gamestate
    public CanvasGroup GameState;

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
    Vector3 normal_scale = new Vector3(1, 1, 1);
    Vector3 selected_scale = new Vector3(1.2f, 1.2f, 1.2f);

    Text ButtonText;

    int selected_index = 0;

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
        {"PHE",  "Hydro"},  {"PRO",  "Other"}, {"SER",  "Polar"}, {"THR",  "Polar"}, {"GLN", "Polar"}, {"TRP",  "Hydro"},
        {"TYR",  "Hydro"}, {"VAL",  "Hydro"},
    };

    SFX sfx;
    OverlayRenderer or;
    int number_childs_A1 = 0;
    int number_childs_A2 = 0;

    #region ATOM TO ATOM CONNECTION
    public int atom_selected_p1 = -1;
    public int atom_selected_p2 = -1;
    public GameObject[] atom_conn;
    PDB_molecule P1_mol;
    PDB_molecule P2_mol;
    PDB_mesh P1_mesh;
    PDB_mesh P2_mesh;
    public Transform P1AtomsHolder;
    public Transform P2AtomsHolder;
    #endregion

    float step_for_A1;
    float step_for_A2;

    void Awake()
    {
        buttonStructure = FindObjectOfType<ButtonStructure>();
        uIController = FindObjectOfType<UIController>();
        sfx = FindObjectOfType<SFX>();
        or = FindObjectOfType<OverlayRenderer>();
    }

    public void init()
    {
        EmptyAminoSliders();
        BioBloxReference = GameObject.Find("BioBlox").GetComponent<BioBlox>();
        P1_mesh = BioBloxReference.molecules_PDB_mesh[0];
        P2_mesh = BioBloxReference.molecules_PDB_mesh[1];
        P1_mol = P1_mesh.mol;
        P2_mol = P2_mesh.mol;
        aminoAcidsNames[0] = P1_mol.aminoAcidsNames;
        aminoAcidsNames[1] = P2_mol.aminoAcidsNames;
        aminoAcidsTags[0] = P1_mol.aminoAcidsTags;
        aminoAcidsTags[1] = P2_mol.aminoAcidsTags;

        for (int i = 0; i < aminoAcidsNames[0].Count; ++i)
        {
            if (aminoAcidsNames[0][i] != null)
            {
                GenerateAminoButtons1(aminoAcidsNames[0][i], aminoAcidsTags[0][i], i, P1_mol.aminoAcidsAtomIds[i]);
            }
        }

        for (int i = 0; i < aminoAcidsNames[1].Count; ++i)
        {
            if (aminoAcidsNames[1][i] != null)
            {
                GenerateAminoButtons2(aminoAcidsNames[1][i], aminoAcidsTags[1][i], i, P2_mol.aminoAcidsAtomIds[i]);
            }
        }
        GetNumberOfAmino_0();
        GetNumberOfAmino_1();
        //create a custom state
        GameObject.FindGameObjectWithTag("data_1").GetComponent<ButtonGameState>().SaveCustom();

        step_for_A1 = 1.0f / number_childs_A1;
        step_for_A2 = 1.0f / number_childs_A2;
    }

    public void Reset()
    {
        //reset buttons for amino acids
        CurrentButtonA1 = CurrentButtonA2 = LastButtonA1 = LastButtonA2 = 0;
        EmptyAminoSliders();
    }


    void Update()
    {
        if (BioBloxReference && BioBloxReference.game_status == BioBlox.GameStatus.GameScreen)
        {

            if (ButtonA1LDown)
            {
                LastButtonA1 = CurrentButtonA1;

                CurrentButtonA1++;

                //CHECK IF EXIST
                if (A1Buttons.Count > CurrentButtonA1) 
                {
                    sfx.PlayTrack(SFX.sound_index.amino_click);
                    //A1Buttons[LastButtonA1].transform.localScale = normal_scale;
                    A1Buttons[LastButtonA1].transform.GetChild(selected_index).gameObject.SetActive(false);
                    //A1Buttons[CurrentButtonA1].transform.localScale = selected_scale;
                    A1Buttons[CurrentButtonA1].transform.GetChild(selected_index).gameObject.SetActive(true);
                    ScrollbarAmino1.value = ScrollbarAmino1.value - step_for_A1;
                    A1Buttons[CurrentButtonA1].HighLight();
                }
                else
                {
                    CurrentButtonA1--;
                    ScrollbarAmino1.value = 0;
                }
            }

            if (ButtonA1RDown)
            {
                //save the last active one
                LastButtonA1 = CurrentButtonA1;

                CurrentButtonA1--;
                //CHECK IF EXIST
                if (CurrentButtonA1 >= 0)
                {
                    sfx.PlayTrack(SFX.sound_index.amino_click);
                    //A1Buttons[LastButtonA1].transform.localScale = normal_scale;
                    A1Buttons[LastButtonA1].transform.GetChild(selected_index).gameObject.SetActive(false);
                    //A1Buttons[CurrentButtonA1].transform.localScale = selected_scale;
                    A1Buttons[CurrentButtonA1].transform.GetChild(selected_index).gameObject.SetActive(true);
                    ScrollbarAmino1.value = ScrollbarAmino1.value + step_for_A1;
                    A1Buttons[CurrentButtonA1].HighLight();
                }
                else
                {
                    CurrentButtonA1++;
                    ScrollbarAmino1.value = 1;
                }
            }

            if (ButtonA2LDown)
            {
                //save the last active one
                LastButtonA2 = CurrentButtonA2;
                CurrentButtonA2++;
                //CHECK IF EXIST
                if (A2Buttons.Count > CurrentButtonA2)
                {
                    sfx.PlayTrack(SFX.sound_index.amino_click);
                    //A2Buttons[LastButtonA2].transform.localScale = normal_scale;
                    A2Buttons[LastButtonA2].transform.GetChild(selected_index).gameObject.SetActive(false);
                    //A2Buttons[CurrentButtonA2].transform.localScale = selected_scale;
                    A2Buttons[CurrentButtonA2].transform.GetChild(selected_index).gameObject.SetActive(true);
                    ScrollbarAmino2.value = ScrollbarAmino2.value - step_for_A2;
                    A2Buttons[CurrentButtonA2].HighLight();
                }
                else
                {
                    CurrentButtonA2--;
                    ScrollbarAmino2.value = 0;
                }
            }

            if (ButtonA2RDown)
            {
                //save the last active one
                LastButtonA2 = CurrentButtonA2;
                CurrentButtonA2--;
                //CHECK IF EXIST
                if (CurrentButtonA2 >= 0)
                {
                    sfx.PlayTrack(SFX.sound_index.amino_click);
                    //A2Buttons[LastButtonA2].transform.localScale = normal_scale;
                    A2Buttons[LastButtonA2].transform.GetChild(selected_index).gameObject.SetActive(false);
                    //A2Buttons[CurrentButtonA2].transform.localScale = selected_scale;
                    A2Buttons[CurrentButtonA2].transform.GetChild(selected_index).gameObject.SetActive(true);
                    ScrollbarAmino2.value = ScrollbarAmino2.value + step_for_A2;
                    A2Buttons[CurrentButtonA2].HighLight();
                }
                else
                {
                    CurrentButtonA2++;
                    ScrollbarAmino2.value = 1;
                }
            }
        }
    }

    public void UpdateCurrentButtonA1(int index)
    {
        //A1Buttons[CurrentButtonA1].transform.localScale = normal_scale;
        A1Buttons[CurrentButtonA1].transform.GetChild(selected_index).gameObject.SetActive(false);
        CurrentButtonA1 = index;
        //A1Buttons[CurrentButtonA1].transform.localScale = selected_scale;

    }

    public void UpdateCurrentButtonA2(int index)
    {
        //A2Buttons[CurrentButtonA2].transform.localScale = normal_scale;
        A2Buttons[CurrentButtonA2].transform.GetChild(selected_index).gameObject.SetActive(false);
        CurrentButtonA2 = index;
        //A2Buttons[CurrentButtonA2].transform.localScale = selected_scale;
    }

    public void DeselectAmino()
    {
        //amino buttons
        //A1Buttons[CurrentButtonA1].transform.localScale = normal_scale;
        //A2Buttons[CurrentButtonA2].transform.localScale = normal_scale;
        ButtonPickedA1 = null;
        ButtonPickedA2 = null;
        AddConnection.interactable = false;
        A2Buttons[CurrentButtonA2].transform.GetChild(selected_index).gameObject.SetActive(false);
        A1Buttons[CurrentButtonA1].transform.GetChild(selected_index).gameObject.SetActive(false);
    }

    public void HighLight3DMesh(int index, int molecule)
    {
        if (molecule == 0)
        {
            //A1Buttons[CurrentButtonA1].transform.localScale = normal_scale;
            A1Buttons[CurrentButtonA1].transform.GetChild(selected_index).gameObject.SetActive(false);
            CurrentButtonA1 = index;
            ChangeAminoAcidSelection(A1Buttons[CurrentButtonA1].gameObject);
            //A1Buttons[CurrentButtonA1].transform.localScale = selected_scale;
            A1Buttons[CurrentButtonA1].transform.GetChild(selected_index).gameObject.SetActive(true);
            ScrollbarAmino1.value = 1 - ((float)CurrentButtonA1 / ((float)SliderMol[0].transform.childCount - 1));
            //A1Buttons [CurrentButtonA1].GetComponent<AminoButtonController> ().SetGameObject();
        }
        else
        {
            //A2Buttons[CurrentButtonA2].transform.localScale = normal_scale;
            A2Buttons[CurrentButtonA2].transform.GetChild(selected_index).gameObject.SetActive(false);
            CurrentButtonA2 = index;
            ChangeAminoAcidSelection(A2Buttons[CurrentButtonA2].gameObject);
            //A2Buttons[CurrentButtonA2].transform.localScale = selected_scale;
            A2Buttons[CurrentButtonA2].transform.GetChild(selected_index).gameObject.SetActive(true);
            ScrollbarAmino2.value = 1 - ((float)CurrentButtonA2 / ((float)SliderMol[1].transform.childCount - 1));
            //A2Buttons [CurrentButtonA2].GetComponent<AminoButtonController> ().SetGameObject();
        }
    }

    public void HighLight3DMeshAll(int index_protein1, int index_protein2)
    {
        //A1Buttons[CurrentButtonA1].transform.localScale = normal_scale;
        A1Buttons[CurrentButtonA1].transform.GetChild(selected_index).gameObject.SetActive(false);
        //CurrentButtonA1 = index_protein1;
        //A1Buttons[CurrentButtonA1].transform.localScale = selected_scale;
        A1Buttons[CurrentButtonA1].transform.GetChild(selected_index).gameObject.SetActive(true);
        ScrollbarAmino1.value = (float)CurrentButtonA1 / ((float)SliderMol[0].transform.childCount - 1);
        //A1Buttons[CurrentButtonA1].GetComponent<AminoButtonController>().HighLight();

        //A2Buttons[CurrentButtonA2].transform.localScale = normal_scale;
        A2Buttons[CurrentButtonA2].transform.GetChild(selected_index).gameObject.SetActive(false);
        //CurrentButtonA2 = index_protein2;
        //A2Buttons[CurrentButtonA2].transform.localScale = selected_scale;
        A2Buttons[CurrentButtonA2].transform.GetChild(selected_index).gameObject.SetActive(true);
        ScrollbarAmino2.value = (float)CurrentButtonA2 / ((float)SliderMol[1].transform.childCount - 1);
        //A2Buttons[CurrentButtonA2].GetComponent<AminoButtonController>().HighLight();   
    }

    public void GenerateAminoButtons1(string currentAmino, string tag, int index, int[] atom_id)
    {
        //ButtonText.Initialize ();
        //Debug.Log (currentAmino);
        AminoButtonReference = Instantiate(AminoButton);
        //update the gameobejct name to grab later
        AminoButtonReference.name = index.ToString();
        A1Buttons.Add(AminoButtonReference.GetComponent<AminoButtonController>());
        AminoButtonReference.transform.SetParent(SliderMol[0].transform, false);
        AminoButtonReference.transform.GetChild(1).GetComponent<Image>().color = buttonStructure.NormalColor[currentAmino];
        //store the color in the button
        //AminoButtonReference.GetComponent<AminoButtonController>().NormalColor = buttonStructure.NormalColor [currentAmino];		
        //AminoButtonReference.GetComponent<AminoButtonController>().FunctionColor = buttonStructure.FunctionColor [currentAmino];

        //InsertButtonToListOfAminoAcidsFuntionA1 (AminoButtonReference, currentAmino);

        //Debug.Log (AminoColor [currentAmino]);
        ButtonText = AminoButtonReference.GetComponentInChildren<Text>();
        ButtonText.text = currentAmino + " " + tag; // currentAmino.Replace (" ", "");
        //ButtonText[1].text = tag;
        AminoButtonReference.GetComponent<AminoButtonController>().name_amino = currentAmino;
        AminoButtonReference.GetComponent<AminoButtonController>().tag_amino = tag;
        AminoButtonReference.GetComponent<AminoButtonController>().amino_id = atom_id;
        AminoButtonReference.GetComponent<AminoButtonController>().protein_id = 0;

        //AminoButtonReference.GetComponentsInChildrenGetComponentInChildren<Text>().text = currentAmino.Replace(" ","")+System.Environment.NewLine+tag;
        //set the button id
        AminoButtonReference.GetComponent<AminoButtonController>().AminoButtonID = index;
    }

    public void GenerateAminoButtons2(string currentAmino, string tag, int index, int[] atom_id)
    {
        //ButtonText.Initialize ();
        AminoButtonReference = Instantiate(AminoButton);
        //update the gameobejct name to grab later
        AminoButtonReference.name = index.ToString();
        A2Buttons.Add(AminoButtonReference.GetComponent<AminoButtonController>());
        AminoButtonReference.transform.SetParent(SliderMol[1].transform, false);
        AminoButtonReference.transform.GetChild(1).GetComponent<Image>().color = buttonStructure.NormalColor[currentAmino];
        //store the color in the button
        //AminoButtonReference.GetComponent<AminoButtonController>().NormalColor = buttonStructure.NormalColor [currentAmino];		
        //AminoButtonReference.GetComponent<AminoButtonController>().FunctionColor = buttonStructure.FunctionColor [currentAmino];

        //InsertButtonToListOfAminoAcidsFuntionA2 (AminoButtonReference, currentAmino);

        ButtonText = AminoButtonReference.GetComponentInChildren<Text>();
        ButtonText.text = currentAmino +" "+ tag; // currentAmino.Replace (" ", "");
        AminoButtonReference.GetComponent<AminoButtonController>().name_amino = currentAmino;
        AminoButtonReference.GetComponent<AminoButtonController>().tag_amino = tag;
        AminoButtonReference.GetComponent<AminoButtonController>().amino_id = atom_id;
        AminoButtonReference.GetComponent<AminoButtonController>().protein_id = 1;

        //AminoButtonReference.GetComponentInChildren<Text>().text = currentAmino.Replace(" ","")+System.Environment.NewLine+tag;
        //set the button id
        AminoButtonReference.GetComponent<AminoButtonController>().AminoButtonID = index;
    }

    void InsertButtonToListOfAminoAcidsFuntionA1(GameObject CurrentAmino, string NameAmino)
    {
        //insert the button to the correct list
        if (FunctionTypes[NameAmino] == "Hydro")
            A1Hydro.Add(CurrentAmino);
        else if (FunctionTypes[NameAmino] == "Positive")
            A1Positive.Add(CurrentAmino);
        else if (FunctionTypes[NameAmino] == "Negative")
            A1Negative.Add(CurrentAmino);
        else if (FunctionTypes[NameAmino] == "Polar")
            A1Polar.Add(CurrentAmino);
        else if (FunctionTypes[NameAmino] == "Other")
            A1Other.Add(CurrentAmino);
    }

    void InsertButtonToListOfAminoAcidsFuntionA2(GameObject CurrentAmino, string NameAmino)
    {
        //insert the button to the correct list
        if (FunctionTypes[NameAmino] == "Hydro")
            A2Hydro.Add(CurrentAmino);
        else if (FunctionTypes[NameAmino] == "Positive")
            A2Positive.Add(CurrentAmino);
        else if (FunctionTypes[NameAmino] == "Negative")
            A2Negative.Add(CurrentAmino);
        else if (FunctionTypes[NameAmino] == "Polar")
            A2Polar.Add(CurrentAmino);
        else if (FunctionTypes[NameAmino] == "Other")
            A2Other.Add(CurrentAmino);
    }

    public void EmptyAminoSliders()
    {
        foreach (Transform childTransform in SliderMol[0].transform) Destroy(childTransform.gameObject);
        foreach (Transform childTransform in SliderMol[1].transform) Destroy(childTransform.gameObject);
        //delete connetions		
        foreach (Transform childTransform in AminoLinkPanelParent.transform) Destroy(childTransform.gameObject);
        A1Buttons.Clear();
        A2Buttons.Clear();
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

        if (ButtonPickedA1 != null && ButtonPickedA2 != null)
        {
            //AddConnection.GetComponent<Animator> ().enabled = true;
            AddConnection.interactable = true;
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
        if (!CheckIfConnectionExist(ButtonPickedA1, ButtonPickedA2))
        {
            sfx.PlayTrack(SFX.sound_index.connection_click);
            //ButtonPickedA1.GetComponent<Animator>().SetBool("High", false);
            //ButtonPickedA2.GetComponent<Animator>().SetBool("High", false);
            //normal size for link manger
            ButtonPickedA1.transform.localScale = new Vector3(1, 1, 1);
            ButtonPickedA2.transform.localScale = new Vector3(1, 1, 1);
            AminoAcidsLinkPanel(BioBloxReference.GetComponent<ConnectionManager>().CreateAminoAcidLink(BioBloxReference.molecules[0].GetComponent<PDB_mesh>(), ButtonPickedA1.GetComponent<AminoButtonController>().AminoButtonID, BioBloxReference.molecules[1].GetComponent<PDB_mesh>(), ButtonPickedA2.GetComponent<AminoButtonController>().AminoButtonID), ButtonPickedA1, ButtonPickedA2);
            //ButtonPickedA1 = ButtonPickedA2 = null;
            FindObjectOfType<ConnectionManager>().SliderStrings.interactable = true;
            //DeactivateAddConnectionButton ();
        }
        else
        {
            ConnectionExistMessage.SetBool("Play", true);
            if (!sfx.isPlaying(SFX.sound_index.connection_exist))
                sfx.PlayTrack(SFX.sound_index.connection_exist);
        }
    }

    //void DeactivateAddConnectionButton()
    //{
    //	AddConnection.GetComponent<Animator>().enabled = false;
    //	AddConnection.GetComponent<Button> ().enabled = false;
    //	Color c = AddConnection.GetComponent<Image>().color;
    //	c.a = 0.3f;
    //	AddConnection.GetComponent<Image> ().color = c;
    //}

    public void AminoAcidsLinkPanel(AtomConnection connection, GameObject ButtonPickedA1, GameObject ButtonPickedA2)
    {
        GameObject AminoLinkPanelReference;
        //activate the linked image
        LinkedGameObjectReference = Instantiate(LinkedGameObject);
        LinkedGameObjectReference.transform.SetParent(ButtonPickedA1.transform.GetChild(2).transform, false);
        LinkedGameObjectReference = Instantiate(LinkedGameObject);
        LinkedGameObjectReference.transform.SetParent(ButtonPickedA2.transform.GetChild(2).transform, false);

        AminoLinkPanelReference = Instantiate(AminoLinkPanel);
        AminoLinkPanelReference.transform.SetParent(AminoLinkPanelParent.transform, false);
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().connection = connection;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button1 = ButtonPickedA1.GetComponent<AminoButtonController>().AminoButtonID;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button2 = ButtonPickedA2.GetComponent<AminoButtonController>().AminoButtonID;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A1_name = ButtonPickedA1.GetComponent<AminoButtonController>().name_amino + " " + ButtonPickedA1.GetComponent<AminoButtonController>().tag_amino;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A2_name = ButtonPickedA2.GetComponent<AminoButtonController>().name_amino + " " + ButtonPickedA2.GetComponent<AminoButtonController>().tag_amino;

        //UpdateBackgroundSize (AminoLinkPanelParent.transform.childCount);

        FixButton(ButtonPickedA1, AminoLinkPanelReference, 0, AminoLinkPanelReference.transform.GetChild(0).transform, 0);
        FixButton(ButtonPickedA2, AminoLinkPanelReference, 1, AminoLinkPanelReference.transform.GetChild(0).transform, 1);


        //get the atom connected
        int[] A_atoms = P1_mol.aminoAcidsAtomIds[ButtonPickedA1.GetComponent<AminoButtonController>().AminoButtonID];
        //set the number of childs
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A1_number_of_atoms = A_atoms.Length;
        //is the last one by default or the user selected an atom???
        int A_atom_index = atom_selected_p1 == -1 ? A_atoms.Length - 1 : atom_selected_p1;
        //SET THE ATOM ID
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().AT1_index = A_atom_index;
        //picking the last atom of the chain of the amino acid 1 or the selected one
        string A_atom = P1_mol.atomNames[A_atoms[A_atom_index]];
        //check element A1
        int name = P1_mol.names[A_atoms[A_atom_index]];
        int A_atom_element = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
        //seting the atom index
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A1_atom_index = A_atoms[A_atom_index];
        //seting atom name
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().AT1_name = A_atom;

        //INSTIATE THE ELEMENT
        GameObject temp_reference = Instantiate(atom_conn[A_atom_element]);
        temp_reference.GetComponentInChildren<Text>().text = A_atom;
        temp_reference.transform.SetParent(AminoLinkPanelReference.transform.GetChild(0).transform.GetChild(2).transform, false);
        //mark the atom	
        LinkedGameObjectReference = Instantiate(LinkedAtom);
        if (P1AtomsHolder.childCount > A_atom_index)
            LinkedGameObjectReference.transform.SetParent(P1AtomsHolder.GetChild(A_atom_index).transform, false);
        //animation
        // P1AtomsHolder.GetChild(A_atom_index).GetComponent<Animator>().SetBool("High", true);

        //get the atom connected
        A_atoms = P2_mol.aminoAcidsAtomIds[ButtonPickedA2.GetComponent<AminoButtonController>().AminoButtonID];
        //set the number of childs
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A2_number_of_atoms = A_atoms.Length;
        //is the last one by default or the user selected an atom???
        A_atom_index = atom_selected_p2 == -1 ? A_atoms.Length - 1 : atom_selected_p2;
        //SET THE ATOM ID
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().AT2_index = A_atom_index;
        //picking the last atom of the chain of the amino acid 1
        A_atom = P2_mol.atomNames[A_atoms[A_atom_index]];
        //check element A1
        name = P2_mol.names[A_atoms[A_atom_index]];
        A_atom_element = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
        //seting the atom index
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A2_atom_index = A_atoms[A_atom_index];
        //seting atom name
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().AT2_name = A_atom;

        //INSTIATE THE ELEMENT
        temp_reference = Instantiate(atom_conn[A_atom_element]);
        temp_reference.GetComponentInChildren<Text>().text = A_atom;
        temp_reference.transform.SetParent(AminoLinkPanelReference.transform.GetChild(0).transform.GetChild(3).transform, false);
        //mark the atom	
        LinkedGameObjectReference = Instantiate(LinkedAtom);
        if (P2AtomsHolder.childCount > A_atom_index)
            LinkedGameObjectReference.transform.SetParent(P2AtomsHolder.GetChild(A_atom_index).transform, false);
        //animation
        // P2AtomsHolder.GetChild(A_atom_index).GetComponent<Animator>().SetBool("High", true);
    }

    bool CheckIfConnectionExist(GameObject A1, GameObject A2)
    {
        int A1ID = A1.GetComponent<AminoButtonController>().AminoButtonID;
        int A2ID = A2.GetComponent<AminoButtonController>().AminoButtonID;
        bool existe = false;

        foreach (Transform childLinks in AminoLinkPanelParent.transform)
        {
            if (childLinks.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button1 == A1ID && childLinks.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button2 == A2ID)
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
        if (hijos == 0)
        {
            AminoLinkBackground.offsetMax = new Vector2(-244.0f, temp.y);
        }
    }

    void FixButton(GameObject AminoButton, GameObject AminoLinkPanelReference, int i, Transform PanelParent, int child)
    {
        GameObject AminoButtonReference = Instantiate(AminoButton);
        AminoButtonReference.SetActive(true);
        AminoButtonReference.transform.position = Vector3.zero;
        AminoButtonReference.GetComponent<LayoutElement>().enabled = false;
        AminoButtonReference.GetComponent<Button>().interactable = true;
        AminoButtonReference.GetComponent<Button>().enabled = false;
        AminoButtonReference.GetComponent<Image>().raycastTarget = false;
        //deactivate the linked image
        Destroy(AminoButtonReference.transform.GetChild(2).gameObject);

        AminoButtonReference.transform.SetParent(PanelParent.GetChild(child), false);
        RectTransform AminoButtonRect = AminoButtonReference.GetComponent<RectTransform>();
        AminoButtonRect.anchorMin = new Vector2(0.5f, 0.5f);
        AminoButtonRect.anchorMax = new Vector2(0.5f, 0.5f);
        AminoButtonRect.pivot = new Vector2(0.5f, 0.5f);
        AminoButtonRect.sizeDelta = new Vector2(35, 35);
        //AminoButtonRect.localPosition += new Vector3(0,button_displace[i],0);
    }

    public void RestoreDeletedAminoButtons(int B1, int B2)
    {
        //UpdateBackgroundSize (AminoLinkPanelParent.transform.childCount-1);
        SliderMol[0].transform.GetChild(B1).GetComponent<AminoButtonController>().Linked = false;
        SliderMol[1].transform.GetChild(B2).GetComponent<AminoButtonController>().Linked = false;
        Destroy(SliderMol[0].transform.GetChild(B1).transform.GetChild(2).transform.GetChild(0).gameObject);
        Destroy(SliderMol[1].transform.GetChild(B2).transform.GetChild(2).transform.GetChild(0).gameObject);
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

    //public void RegisterScore()
    //{

    //    //new higuest lennars score
    //    if (BioBloxReference.lennard_score > score_len_max)
    //    {
    //        //clear the list
    //        score_aminolinks_holder_len.Clear();
    //        //show message high score
    //        LPJMessage.SetBool("Play", true);
    //        foreach (Transform childLinks in AminoLinkPanelParent.transform)
    //        {
    //            score_aminolinks_holder_len.Add(new Vector2(childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button1, childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button2));
    //        }
    //        //set the new high score to compare
    //        score_len_max = BioBloxReference.lennard_score;
    //        //store the rotation of the molecules
    //        len_protein0 = BioBloxReference.molecules[0].transform.localEulerAngles;
    //        len_protein1 = BioBloxReference.molecules[1].transform.localEulerAngles;
    //    }//new higuest electrotatic score
    //    if (BioBloxReference.electric_score > score_elec_max)
    //    {
    //        //clear the list
    //        score_aminolinks_holder_elec.Clear();
    //        //show message high score
    //        ElecMessage.SetBool("Play", true);
    //        foreach (Transform childLinks in AminoLinkPanelParent.transform)
    //        {
    //            score_aminolinks_holder_elec.Add(new Vector2(childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button1, childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button2));
    //        }
    //        //set the new high score to compare
    //        score_elec_max = BioBloxReference.electric_score;
    //        //store the rotation of the molecules
    //        elec_protein0 = BioBloxReference.molecules[0].transform.localEulerAngles;
    //        elec_protein1 = BioBloxReference.molecules[1].transform.localEulerAngles;
    //    }//new higuest max score
    //    if (BioBloxReference.electric_score + BioBloxReference.lennard_score > score_total_max)
    //    {
    //        //clear the list
    //        score_aminolinks_holder_max.Clear();
    //        //show message high score
    //        MaxMessage.SetBool("Play", true);
    //        foreach (Transform childLinks in AminoLinkPanelParent.transform)
    //        {
    //            score_aminolinks_holder_max.Add(new Vector2(childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button1, childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button2));
    //        }
    //        //set the new high score to compare
    //        score_total_max = BioBloxReference.electric_score + BioBloxReference.lennard_score;
    //        //store the rotation of the molecules
    //        max_protein0 = BioBloxReference.molecules[0].transform.localEulerAngles;
    //        max_protein1 = BioBloxReference.molecules[1].transform.localEulerAngles;
    //    }

    //}

    public void LoadScore()
    {
        DeleteAllAminoConnections();
        switch (DropDownLoadScore.value)
        {
            case 0:
                if (score_aminolinks_holder_max.Count != 0)
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

    public void DeleteAllAminoConnections()
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

    public void ModifyConnectionHolder(AtomConnection connection_temp, GameObject ButtonPickedA1_temp, GameObject ButtonPickedA2_temp, Transform HolderPanelFather_temp)
    {

        P2_mesh.SelectAminoAcid(ButtonPickedA2_temp.GetComponent<AminoButtonController>().AminoButtonID);
        P1_mesh.SelectAminoAcid(ButtonPickedA1_temp.GetComponent<AminoButtonController>().AminoButtonID);

        ButtonPickedA1_temp.transform.localScale = new Vector3(1, 1, 1);
        ButtonPickedA2_temp.transform.localScale = new Vector3(1, 1, 1);
        //activate the linked image
        LinkedGameObjectReference = Instantiate(LinkedGameObject);
        LinkedGameObjectReference.transform.SetParent(ButtonPickedA1_temp.transform.GetChild(2).transform, false);
        LinkedGameObjectReference = Instantiate(LinkedGameObject);
        LinkedGameObjectReference.transform.SetParent(ButtonPickedA2_temp.transform.GetChild(2).transform, false);
        //connection mpdifi
        GameObject AminoHolderReference = Instantiate(AminoHolderConnection);
        AminoHolderReference.transform.SetParent(HolderPanelFather_temp, false);
        AminoHolderReference.GetComponent<AminoConnectionHolder>().connection = connection_temp;
        AminoHolderReference.GetComponent<AminoConnectionHolder>().ID_button1 = ButtonPickedA1_temp.GetComponent<AminoButtonController>().AminoButtonID;
        AminoHolderReference.GetComponent<AminoConnectionHolder>().ID_button2 = ButtonPickedA2_temp.GetComponent<AminoButtonController>().AminoButtonID;
        AminoHolderReference.GetComponent<AminoConnectionHolder>().A1_name = ButtonPickedA1_temp.GetComponent<AminoButtonController>().name_amino + " " + ButtonPickedA1_temp.GetComponent<AminoButtonController>().tag_amino;
        AminoHolderReference.GetComponent<AminoConnectionHolder>().A2_name = ButtonPickedA2_temp.GetComponent<AminoButtonController>().name_amino + " " + ButtonPickedA2_temp.GetComponent<AminoButtonController>().tag_amino;
        

        //UpdateBackgroundSize (AminoLinkPanelParent.transform.childCount);

        FixButton(ButtonPickedA1_temp, AminoHolderReference, 0, AminoHolderReference.transform, 0);
        FixButton(ButtonPickedA2_temp, AminoHolderReference, 1, AminoHolderReference.transform, 1);

        //get the atom connected
        int[] A_atoms = P1_mol.aminoAcidsAtomIds[ButtonPickedA1_temp.GetComponent<AminoButtonController>().AminoButtonID];
        //set the number of childs
        AminoHolderReference.GetComponent<AminoConnectionHolder>().A1_number_of_atoms = A_atoms.Length;
        AminoHolderReference.GetComponent<AminoConnectionHolder>().AT1_index = A_atoms.Length - 1;

        //picking the last atom of the chain of the amino acid 1
        string A_atom = P1_mol.atomNames[A_atoms[A_atoms.Length - 1]];
        //check element A1
        int name = P1_mol.names[A_atoms[A_atoms.Length - 1]];
        int A_atom_element = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
        //seting atom name
        AminoHolderReference.GetComponent<AminoConnectionHolder>().AT1_name = A_atom;
        //seting the atom index
        AminoHolderReference.GetComponent<AminoConnectionHolder>().A1_atom_index = A_atoms[A_atoms.Length - 1];

        //INSTIATE THE ELEMENT
        GameObject temp_reference = Instantiate(atom_conn[A_atom_element]);
        temp_reference.GetComponentInChildren<Text>().text = A_atom;
        temp_reference.transform.SetParent(AminoHolderReference.transform.GetChild(2).transform, false);
        //mark the atom	
        LinkedGameObjectReference = Instantiate(LinkedAtom);
        LinkedGameObjectReference.transform.SetParent(P1AtomsHolder.GetChild(A_atoms.Length - 1).transform, false);
        //animation
        //P1AtomsHolder.GetChild(0).GetComponent<Animator>().SetBool("High", true);

        //get the atom connected
        A_atoms = P2_mol.aminoAcidsAtomIds[ButtonPickedA2_temp.GetComponent<AminoButtonController>().AminoButtonID];
        //set the number of childs
        AminoHolderReference.GetComponent<AminoConnectionHolder>().A2_number_of_atoms = A_atoms.Length;
        AminoHolderReference.GetComponent<AminoConnectionHolder>().AT2_index = A_atoms.Length - 1;
        //picking the last atom of the chain of the amino acid 1
        A_atom = P2_mol.atomNames[A_atoms[A_atoms.Length - 1]];
        //check element A1
        name = P2_mol.names[A_atoms[A_atoms.Length - 1]];
        A_atom_element = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
        //seting atom name
        AminoHolderReference.GetComponent<AminoConnectionHolder>().AT2_name = A_atom;
        //seting the atom index
        AminoHolderReference.GetComponent<AminoConnectionHolder>().A2_atom_index = A_atoms[A_atoms.Length - 1];

        //INSTIATE THE ELEMENT
        temp_reference = Instantiate(atom_conn[A_atom_element]);
        temp_reference.GetComponentInChildren<Text>().text = A_atom;
        temp_reference.transform.SetParent(AminoHolderReference.transform.GetChild(3).transform, false);
        //mark the atom	
        LinkedGameObjectReference = Instantiate(LinkedAtom);
        LinkedGameObjectReference.transform.SetParent(P2AtomsHolder.GetChild(A_atoms.Length - 1).transform, false);
        //animation
        // P2AtomsHolder.GetChild(0).GetComponent<Animator>().SetBool("High", true);
    }

    public void ModifyConnectionHolder_atomic(AtomConnection connection_temp, int A1_atom_index, int A1_atom_index_previous, int A2_atom_index, int A2_atom_index_previous, int protein_id_temp, GameObject current_panel, bool atom_exist)
    {
        GameObject temp_reference;
        if (protein_id_temp == 0)
        {
            if (atom_exist)
            {
                //UNmark the atoM
                //P1AtomsHolder.GetChild(A1_atom_index_previous).GetComponent<Animator>().SetBool("High", false);
                Destroy(P1AtomsHolder.GetChild(A1_atom_index_previous).transform.GetChild(2).gameObject);
            }

            //P1_mesh.SelectAminoAcid(ButtonPickedA1_temp.GetComponent<AminoButtonController>().AminoButtonID);
            current_panel.GetComponent<AminoConnectionHolder>().AT1_index = A1_atom_index;
            current_panel.GetComponent<AminoConnectionHolder>().AT1_name = P1AtomsHolder.GetChild(A1_atom_index).GetComponent<AtomConnectionController>().atom_name;
            temp_reference = Instantiate(atom_conn[P1AtomsHolder.GetChild(A1_atom_index).GetComponent<AtomConnectionController>().element_type]);
            temp_reference.GetComponentInChildren<Text>().text = P1AtomsHolder.GetChild(A1_atom_index).GetComponent<AtomConnectionController>().atom_name;
            temp_reference.transform.SetParent(current_panel.transform.GetChild(2).transform, false);
            //mark the atom	
            HighlightAtomWhenConnectionClicked(A1_atom_index, 0);
            //HighlightAtomWhenConnectionClicked(A2_atom_index, 1);
            //LinkedGameObjectReference = Instantiate(LinkedAtom);
            //LinkedGameObjectReference.transform.SetParent(P1AtomsHolder.GetChild(A1_atom_index).transform, false);
            ////animation
            //P1AtomsHolder.GetChild(A1_atom_index).GetComponent<Animator>().SetBool("High", true);
        }
        else
        {
            if (atom_exist)
            {
                //UNmark the atoM
                // P2AtomsHolder.GetChild(A2_atom_index_previous).GetComponent<Animator>().SetBool("High", false);
                Destroy(P2AtomsHolder.GetChild(A2_atom_index_previous).transform.GetChild(2).gameObject);
            }

            //P2_mesh.SelectAminoAcid(ButtonPickedA2_temp.GetComponent<AminoButtonController>().AminoButtonID);
            current_panel.GetComponent<AminoConnectionHolder>().AT2_index = A2_atom_index;
            current_panel.GetComponent<AminoConnectionHolder>().AT2_name = P2AtomsHolder.GetChild(A2_atom_index).GetComponent<AtomConnectionController>().atom_name;
            temp_reference = Instantiate(atom_conn[P2AtomsHolder.GetChild(A2_atom_index).GetComponent<AtomConnectionController>().element_type]);
            temp_reference.GetComponentInChildren<Text>().text = P2AtomsHolder.GetChild(A2_atom_index).GetComponent<AtomConnectionController>().atom_name;
            temp_reference.transform.SetParent(current_panel.transform.GetChild(3).transform, false);
            //mark the atom	
            HighlightAtomWhenConnectionClicked(A2_atom_index, 1);
            //HighlightAtomWhenConnectionClicked(A1_atom_index, 0);
            //LinkedGameObjectReference = Instantiate(LinkedAtom);
            //LinkedGameObjectReference.transform.SetParent(P2AtomsHolder.GetChild(A2_atom_index).transform, false);
            ////animation
            //P2AtomsHolder.GetChild(A2_atom_index).GetComponent<Animator>().SetBool("High", true);

        }
        current_panel.GetComponent<AminoConnectionHolder>().connection = connection_temp;
    }

    Transform atomholder_temp;
    public void HighlightAtomWhenConnectionClicked(int atom_id, int protein_id)
    {
        atomholder_temp = protein_id == 0 ? P1AtomsHolder : P2AtomsHolder;
        //mark the atom	
        LinkedGameObjectReference = Instantiate(LinkedAtom);
        LinkedGameObjectReference.transform.SetParent(atomholder_temp.GetChild(atom_id).transform, false);
        //animation
        atomholder_temp.GetChild(atom_id).transform.localScale = selected_scale;
        //if (protein_id == 0)
        //    or.P1_selected_atom_id = atom_index;
        //else
        //    or.P2_selected_atom_id = atom_index;
    }


    public void GetNumberOfAmino_0()
    {
        number_childs_A1 = 0;
        foreach (Transform child in SliderMol[0].transform)
        {
            if (number_childs_A1 == 0)
            {
                CurrentButtonA1 = child.GetComponent<AminoButtonController>().AminoButtonID;
            }
            child.GetComponent<AminoButtonController>().temp_AminoButtonID = number_childs_A1;
            number_childs_A1++;
        }
        ScrollbarAmino1.value = 0;
    }

    public void GetNumberOfAmino_1()
    {
        number_childs_A2 = 0;
        foreach (Transform child in SliderMol[1].transform)
        {
            if (number_childs_A2 == 0)
            {
                CurrentButtonA2 = child.GetComponent<AminoButtonController>().AminoButtonID;
            }
            child.GetComponent<AminoButtonController>().temp_AminoButtonID = number_childs_A2;
            number_childs_A2++;
        }
        ScrollbarAmino2.value = 0;
    }

    public bool IsSelected(int slider, string id)
    {
        GameObject button = slider == 1 ? ButtonPickedA2 : ButtonPickedA1;
        List<string> names = slider == 1 ? aminoAcidsNames[1] : aminoAcidsNames[0];
        List<string> tags = slider == 1 ? aminoAcidsTags[1] : aminoAcidsTags[0];
        if (button == null) return false;
        int bid = button.GetComponent<AminoButtonController>().AminoButtonID;
        string val = names[bid] + " " + tags[bid];
        //Debug.Log("val=[" + val + "] id=[" + id + "]");
        return val == id;
    }

    public bool IsConnectionMade(string id1, string id2)
    {
        //Debug.Log("icm " + id1 + " " + id2);
        foreach (Transform childLinks in AminoLinkPanelParent.transform)
        {
            int b1 = childLinks.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button1;
            int b2 = childLinks.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button2;
            string val1 = aminoAcidsNames[0][b1] + " " + aminoAcidsTags[0][b1];
            string val2 = aminoAcidsNames[1][b2] + " " + aminoAcidsTags[1][b2];
            //Debug.Log("val1=[" + val1 + "] val2=[" + val2 + "]" + (val1 == id1 && val2 == id2));
            if (val1 == id1 && val2 == id2)
            {
                return true;
            }
        }
        return false;
    }

    public void FilterButtons(string[] aas)
    {
        foreach (AminoButtonController button in A1Buttons)
        {
            int bid = button.AminoButtonID;
            string tag = aminoAcidsNames[0][bid] + " " + aminoAcidsTags[0][bid];
            bool active = false;
            foreach (string aa in aas)
            {
                if (tag == aa)
                {
                    active = true;
                    break;
                }
            }
            button.gameObject.SetActive(active);
        }
        foreach (AminoButtonController button in A2Buttons)
        {
            int bid = button.AminoButtonID;
            string tag = aminoAcidsNames[1][bid] + " " + aminoAcidsTags[1][bid];
            bool active = false;
            foreach (string aa in aas)
            {
                if (tag == aa)
                {
                    active = true;
                    break;
                }
            }
            button.gameObject.SetActive(active);
        }
    }

    public void LoadSaveButtonScore(bool status)
    {
        GameState.alpha = status ? 1 : 0;
        GameState.interactable = status;
        GameState.blocksRaycasts = status;
    }

    public int ReturnNumberOfConnection()
    {
        return AminoLinkPanelParent.transform.childCount;
    }

    public void AminoAcidsLinkPanel_load_score(AtomConnection connection, GameObject ButtonPickedA1, GameObject ButtonPickedA2)
    {
        GameObject AminoLinkPanelReference;
        //activate the linked image
        LinkedGameObjectReference = Instantiate(LinkedGameObject);
        LinkedGameObjectReference.transform.SetParent(ButtonPickedA1.transform.GetChild(2).transform, false);
        LinkedGameObjectReference = Instantiate(LinkedGameObject);
        LinkedGameObjectReference.transform.SetParent(ButtonPickedA2.transform.GetChild(2).transform, false);

        AminoLinkPanelReference = Instantiate(AminoLinkPanel);
        AminoLinkPanelReference.transform.SetParent(AminoLinkPanelParent.transform, false);
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().connection = connection;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button1 = ButtonPickedA1.GetComponent<AminoButtonController>().AminoButtonID;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().ID_button2 = ButtonPickedA2.GetComponent<AminoButtonController>().AminoButtonID;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A1_name = ButtonPickedA1.GetComponent<AminoButtonController>().name_amino + " " + ButtonPickedA1.GetComponent<AminoButtonController>().tag_amino;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A2_name = ButtonPickedA2.GetComponent<AminoButtonController>().name_amino + " " + ButtonPickedA2.GetComponent<AminoButtonController>().tag_amino;


        //SET THE ATOMS AS THE FIRST ONE
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().AT1_index = 0;
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().AT2_index = 0;

        //UpdateBackgroundSize (AminoLinkPanelParent.transform.childCount);

        FixButton(ButtonPickedA1, AminoLinkPanelReference, 0, AminoLinkPanelReference.transform.GetChild(0).transform, 0);
        FixButton(ButtonPickedA2, AminoLinkPanelReference, 1, AminoLinkPanelReference.transform.GetChild(0).transform, 1);

        //get the atom connected
        int[] A_atoms = P1_mol.aminoAcidsAtomIds[ButtonPickedA1.GetComponent<AminoButtonController>().AminoButtonID];
        //set the number of childs
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A1_number_of_atoms = A_atoms.Length;
        //picking the last atom of the chain of the amino acid 1
        string A_atom = P1_mol.atomNames[A_atoms[A_atoms.Length - 1]];
        //check element A1
        int name = P1_mol.names[A_atoms[A_atoms.Length - 1]];
        int A_atom_element = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
        //seting atom name
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().AT1_name = A_atom;
        //seting the atom index
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A1_atom_index = A_atoms[A_atoms.Length - 1];

        //INSTIATE THE ELEMENT
        GameObject temp_reference = Instantiate(atom_conn[A_atom_element]);
        temp_reference.GetComponentInChildren<Text>().text = A_atom;
        temp_reference.transform.SetParent(AminoLinkPanelReference.transform.GetChild(0).transform.GetChild(2).transform, false);

        //get the atom connected
        A_atoms = P2_mol.aminoAcidsAtomIds[ButtonPickedA2.GetComponent<AminoButtonController>().AminoButtonID];
        //set the number of childs
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A2_number_of_atoms = A_atoms.Length;
        //picking the last atom of the chain of the amino acid 1
        A_atom = P2_mol.atomNames[A_atoms[A_atoms.Length - 1]];
        //check element A1
        name = P2_mol.names[A_atoms[A_atoms.Length - 1]];
        A_atom_element = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
        //seting atom name
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().AT2_name = A_atom;
        //seting the atom index
        AminoLinkPanelReference.transform.GetChild(0).GetComponent<AminoConnectionHolder>().A2_atom_index = A_atoms[A_atoms.Length - 1];

        //INSTIATE THE ELEMENT
        temp_reference = Instantiate(atom_conn[A_atom_element]);
        temp_reference.GetComponentInChildren<Text>().text = A_atom;
        temp_reference.transform.SetParent(AminoLinkPanelReference.transform.GetChild(0).transform.GetChild(3).transform, false);

    }

    #region ATOM SELECTION
    public void HighLightWhenAtomClicked(int protein_id, int AminoButtonID_temp)
    {
        if (protein_id == 0)
        {
            P1_mesh.SelectAminoAcid(AminoButtonID_temp);
            UpdateCurrentButtonA1(AminoButtonID_temp);
        }
        else
        {
            P2_mesh.SelectAminoAcid(AminoButtonID_temp);
            UpdateCurrentButtonA2(AminoButtonID_temp);
        }
    }

    public void AtomSelected(int protein_id, int atom_child_index, int atom_index)
    {
        if (protein_id == 0)
        {
            if (atom_selected_p1 != -1)
                P1AtomsHolder.GetChild(atom_selected_p1).transform.localScale = normal_scale;
            else
                P1AtomsHolder.GetChild(0).transform.localScale = normal_scale;
            atom_selected_p1 = atom_child_index;
            P1AtomsHolder.GetChild(atom_selected_p1).transform.localScale = selected_scale;
            or.P1_selected_atom_id = atom_index;
        }
        else
        {
            if (atom_selected_p2 != -1)
                P2AtomsHolder.GetChild(atom_selected_p2).transform.localScale = normal_scale;
            else
                P2AtomsHolder.GetChild(0).transform.localScale = normal_scale;
            atom_selected_p2 = atom_child_index;
            P2AtomsHolder.GetChild(atom_selected_p2).transform.localScale = selected_scale;
            or.P2_selected_atom_id = atom_index;
        }
    }

    public void ScaleAtomAtGenerationP1(int atom_index)
    {
        P1AtomsHolder.GetChild(atom_index).transform.localScale = selected_scale;
    }

    public void ScaleAtomAtGenerationP2(int atom_index)
    {
        P2AtomsHolder.GetChild(atom_index).transform.localScale = selected_scale;
    }
    #endregion

}
