using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.Rendering;

public class UIController : MonoBehaviour {

	public GameObject FreeCameraKeysFreeze;
	public GameObject FreeCameraKeysUnfreeze;
	public Toggle FreeCameraToggle;
    public Button ExplorerButton;
	public GameObject MainCamera;
	public Animator ToolPanel;
	public GameObject OpenToolImage;
	public GameObject CloseToolImage;
	public GameObject AddConnectionText;
	bool CameraFreeze = false;
	public bool ToggleFreeCameraStatus = true;
	//tool panel togle protein view
	public GameObject Protein1;
	public GameObject Protein2;
	//change the visualization od the slider
	public GameObject SliderProtein1;
	public GameObject SliderProtein2;
	//slider info toggles
	public GameObject FunctionInfoProtein1;
	public GameObject FunctionInfoProtein2;
	public Animator FunctionInfoPanel;
	public GameObject FunctionInfoPanelOpenImage;
	public GameObject FunctionInfoPanelCloseImage;
	bool FunctionInfoPanelStatus = true;
	public GameObject FunctionInfoObject;
	//father of the gameobjects of the connections
	public GameObject AminoLinkPanel;
    //dropdowns
    public Dropdown DropDownProtein1;
    public Dropdown DropDownProtein2;
    public Dropdown DropDownSlider;

    public GameObject CameraSlider;
    //autofilter
    public bool auto_filter = false;
    public Toggle AutoFilterToggle;
    //first person
    public bool first_person = false;
    public Toggle first_person_toggle;
    public GameObject score_panel;
    public Image cctv_overlay;
    public Sprite cctv_start;
    public Sprite cctv_loading;
    public Sprite cctv_ship;
    //explore
    public bool explore_view = false;
    public GameObject MainCanvas;

    AminoSliderController aminoSliderController;
    BioBlox BioBloxReference;
    ExploreController explorerController;

	public Toggle[] ToggleButtonFunctionsView;
    Camera MainCameraComponent;
    
    public bool isOverUI = false;

    public Material transparent_material;
    public Material default_material;
    public Material cutaway_material;
    public Dropdown DropDownP1;
    public Dropdown DropDownP2;

    public Texture2D cursor_move;

    public int first_person_protein = -1;

    //camera first person
    public GameObject FirstPersonCameraReference;
    public Camera FirstPersonCameraReferenceCamera;

    public GameObject Arpon;
    public CanvasGroup ScrollbarLinks;


    public CanvasGroup AminoAcids;
    public CanvasGroup ConnectionButton;
    public CanvasGroup AminoLinks;
    public Toggle Tutorial;
    public Slider CutAway;
    public Toggle ToggleBeacon;

    public Material space_skybox;
    public Material normal_skybox;
    public GameObject floor;
    bool toggle_score = false;
    public CanvasGroup score_panel_alpha;
    public CanvasGroup close_score_panel;
    public CanvasGroup open_score_panel;

    public Text level_description;
    public Text level_title;
    public Image image_description;
    public GameObject button_play;

    public Transform AddConnectionButton;
    public Text atom_name_firstperson;
   

    bool switch_material = true;

    public bool is_moving_camera_first_person = false;
    public bool is_hovering = false;

    public bool is_both_selected = false;

    //cutaway wall
    public Transform cutaway_wall;
    float camera_distance = 200.0f;
    
    public Text number_atoms_end_level;
    public Text time_end_level;

    //save button
    public GameObject SaveButton;
    public Animator HintPanel;
    public GameObject HintPanelOpen;
    public GameObject HintPanelClose;
    public Image HintImage;
    SFX sfx;
    public Toggle FixProtein1Toggle;
    public Toggle FixProtein2Toggle;

    public enum protein_render { normal, transparent, bs, carbon};

    public GameObject LevelClickled;
    public GameObject A1R;
    public GameObject A1L;
    public GameObject A2R;
    public GameObject A2L;

    public bool tool_panel_status = false;
    public bool hint_panel_status = false;

    int number_of_meshes;

    public int p1_atom_status;
    public int p2_atom_status;
    public enum p_atom_status_enum { find_atoms, done };
    public GameObject[] atom_buttons = new GameObject[4];
    public Transform p1_atom_holder;
    public Transform p2_atom_holder;
    OverlayRenderer or;
    #region EXPLORER VIEW
    //reference for ship/explore view to know which one is being scanned
    public int protein_scanned;
    public Text atom_name;
    GameObject space_ship;
    public Animator FadeCanvasToExplore;
    #endregion
    public GameObject AtomHolder;
    public Transform P1AtomInfo;
    public Transform P2AtomInfo;
    LineRenderer lr;
    DataManager dm;
    ConnectionManager cm;
    GameManager gm;

    int transparent_render;

    public int amino_id = 0;
    public int amino_clicked = -1;

    public Text[] slot_score;

    public GameObject AminoButton_Atom_reference_0;
    public GameObject AminoButton_reference_0;
    public GameObject AminoButton_Atom_reference_1;
    public GameObject AminoButton_reference_1;
    public Sprite CloseAtomPanel;
    
    bool is_grid_active = true;
    public Image GridToggleButton;
    public Color GridToggleColor_normal;
    public Color GridToggleColor_pressed;
    public Color normal_button_color;

    public Image[] protein0_render_buttons;
    public Image[] protein1_render_buttons;

    public bool is_over_amino_info_panel = false;

    void Awake()
	{
		aminoSliderController = FindObjectOfType<AminoSliderController> ();
        BioBloxReference = FindObjectOfType<BioBlox>();
        explorerController = FindObjectOfType<ExploreController>();
        MainCameraComponent = MainCamera.transform.GetChild(0).GetComponent<Camera>();
        lr = FindObjectOfType<LineRenderer>();
        //button_erase_connections_1p_button = button_erase_connections_1p.GetComponent<Button>();
        sfx = FindObjectOfType<SFX>();
        or = FindObjectOfType<OverlayRenderer>();
        dm = FindObjectOfType<DataManager>();
        cm = FindObjectOfType<ConnectionManager>();
        gm = FindObjectOfType<GameManager>();
    }

    public void init()
    {
        number_of_meshes = BioBloxReference.molecules[0].transform.GetChild(0).transform.childCount;
        //DONT FIND ATOMS
        p1_atom_status = 0;
        p2_atom_status = 0;
        transparent_render = BioBloxReference.molecules[0].transform.GetChild(0).transform.childCount - 1;
        DropDownP1.value = 0;
        DropDownP2.value = 0;
        //SET VALUES TO SAVE BUTTONS
        Transform level_holder = GameObject.FindGameObjectWithTag("level_holder").gameObject.transform;//slots holder
        int slot_holder_index = level_holder.GetChild(0).transform.childCount - 1;
        slot_score[0].text = "score: " + level_holder.GetChild(gm.current_level).transform.GetChild(slot_holder_index).transform.GetChild(0).GetComponent<SlotController>().total_score;
        slot_score[1].text = "score: " + level_holder.GetChild(gm.current_level).transform.GetChild(slot_holder_index).transform.GetChild(1).GetComponent<SlotController>().total_score;
        slot_score[2].text = "score: " + level_holder.GetChild(gm.current_level).transform.GetChild(slot_holder_index).transform.GetChild(2).GetComponent<SlotController>().total_score;
    }


    Vector3 dragOrigin;
    float dragSpeed = 20.0f;
    Vector3 move;

    // Update is called once per frame
    void Update ()
    {
        if (BioBloxReference && BioBloxReference.game_status == BioBlox.GameStatus.GameScreen)
        {
            //deselection aminoacids
            if (Input.GetMouseButtonDown(0) && !first_person && BioBloxReference.molecules.Length != 0)
            {
                Ray ray = MainCameraComponent.ScreenPointToRay(Input.mousePosition);
                int atomID_molecule_temp_0 = PDB_molecule.collide_ray(FirstPersonCameraReference, BioBloxReference.molecules_PDB_mesh[0].mol, BioBloxReference.molecules[0].transform, ray);
                int atomID_molecule_temp_1 = PDB_molecule.collide_ray(FirstPersonCameraReference, BioBloxReference.molecules_PDB_mesh[1].mol, BioBloxReference.molecules[1].transform, ray);
                if (!isOverUI && atomID_molecule_temp_0 == -1 && atomID_molecule_temp_1 == -1)
                {
                    BioBloxReference.molecules_PDB_mesh[0].DeselectAminoAcid();
                    BioBloxReference.molecules_PDB_mesh[1].DeselectAminoAcid();
                    aminoSliderController.DeselectAmino();
                    //DeselectAtoms();
                }
            }

            //esc to spawn the menu
            if (Input.GetKey(KeyCode.Escape))
                OpenLevelSelectionPanel();
        }

        if (!is_over_amino_info_panel && Input.GetMouseButton(0))
            aminoSliderController.DeleteCurrentAminoInfoPanel();
    }

    public void DeselectOnClick()
    {
        if (!isOverUI && (BioBloxReference.molecules_PDB_mesh[0].atom != -1 || BioBloxReference.molecules_PDB_mesh[1].atom != -1))
        {
            BioBloxReference.molecules_PDB_mesh[0].DeselectAminoAcid();
            BioBloxReference.molecules_PDB_mesh[1].DeselectAminoAcid();
        }
    }

	////tool panel
	//public void ToogleToolMenu()
	//{
 //       tool_panel_status = !tool_panel_status;
 //       sfx.PlayTrack(SFX.sound_index.button_click);
	//	ToolPanel.SetBool ("Open", tool_panel_status);
 //       CloseToolImage.SetActive(tool_panel_status);
 //       OpenToolImage.SetActive(!tool_panel_status);
 //   }

 //   public void ToogleToolMenuStart()
 //   {
 //       tool_panel_status = !tool_panel_status;
 //       ToolPanel.SetBool("Open", tool_panel_status);
 //       CloseToolImage.SetActive(tool_panel_status);
 //       OpenToolImage.SetActive(!tool_panel_status);
 //   }

    public void RestartCamera()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        //Debug.Log("camera_distance: "+ camera_distance);
        MainCamera.GetComponent<MouseOrbitImproved_main>().enabled = false;
        MainCamera.GetComponent<MouseOrbitImproved_main>().distance = camera_distance;
        MainCamera.transform.rotation = Quaternion.identity;
        MainCamera.GetComponent<MouseOrbitImproved_main>().enabled = true;
        MainCamera.GetComponent<MouseOrbitImproved_main>().Init();
        MainCamera.transform.GetChild(0).localPosition = Vector3.zero;
    }

    public void RestartCameraFromIntro()
    {
        MainCamera.GetComponent<MouseOrbitImproved_main>().enabled = false;
        MainCamera.GetComponent<MouseOrbitImproved_main>().distance = camera_distance;
        MainCamera.transform.rotation = Quaternion.identity;
        MainCamera.GetComponent<MouseOrbitImproved_main>().enabled = true;
        MainCamera.GetComponent<MouseOrbitImproved_main>().Init();
    }

    public void RepositionCameraWOMovement()
    {
        MainCamera.GetComponent<MouseOrbitImproved_main>().enabled = false;
        MainCamera.GetComponent<MouseOrbitImproved_main>().distance = camera_distance;
        MainCamera.transform.rotation = Quaternion.identity;
    }

    public void ToggleProteinView1()
	{
		Protein1.SetActive (true);		
		Protein2.SetActive (false);
	}

	public void ToggleProteinView2()
	{
		Protein1.SetActive (false);		
		Protein2.SetActive (true);
	}

	void ChangeAminoLinkPanelButtonColorToNormal()
	{
		foreach(Transform AminoLinkChild in AminoLinkPanel.transform)
		{
			Debug.Log (AminoLinkChild.name);
			Debug.Log (AminoLinkChild.GetChild(3).name);
			AminoLinkChild.GetChild(3).GetComponent<Image>().color = AminoLinkChild.GetChild(3).GetComponent<AminoButtonController>().NormalColor;			
			AminoLinkChild.GetChild(4).GetComponent<Image>().color = AminoLinkChild.GetChild(4).GetComponent<AminoButtonController>().NormalColor;
		}
	}

	void ChangeAminoLinkPanelButtonColorToFunction()
	{
		foreach(Transform AminoLinkChild in AminoLinkPanel.transform)
		{
			AminoLinkChild.GetChild(3).GetComponent<Image>().color = AminoLinkChild.GetChild(3).GetComponent<AminoButtonController>().FunctionColor;
			AminoLinkChild.GetChild(4).GetComponent<Image>().color = AminoLinkChild.GetChild(4).GetComponent<AminoButtonController>().FunctionColor;
		}
	}

	public void SwitchFunctionInfoProtein2()
	{
		FunctionInfoProtein1.SetActive (false);
		FunctionInfoProtein2.SetActive (true);
	}

	public void SwitchFunctionInfoProtein1()
	{		
		FunctionInfoProtein2.SetActive (false);
		FunctionInfoProtein1.SetActive (true);
	}

	public void SwitchFunctionInfoPanel()
	{
		FunctionInfoPanelOpenImage.SetActive (FunctionInfoPanelStatus);
		FunctionInfoPanelCloseImage.SetActive (!FunctionInfoPanelStatus);
		FunctionInfoPanel.SetBool ("Status", FunctionInfoPanelStatus);
		FunctionInfoPanelStatus = !FunctionInfoPanelStatus;
	}

	void ToggleAllButtonsSlider(List<GameObject> CurrentList, bool status)
	{
		foreach (GameObject AminoButton in CurrentList)
		{
			AminoButton.SetActive(status);
		}
	}
    

    /*public void DropDownSliderView()
    {
        switch (DropDownSlider.value)
        {
            case 0:
                ChangeSliderViewToNormal();
                AutoFilterToggle.interactable = false;
                break;
            case 1:
                ChangeSliderViewToFunction();
                AutoFilterToggle.interactable = true;
                break;
            default:
                break;
        }
    }*/

    public void FirstPersonToggle()
    {
        first_person = !first_person;
        CutAway.value = CutAway.minValue;
        isOverUI = false;
        RepositionCameraWOMovement();
        Tutorial.isOn = false;
        Tutorial.enabled = !first_person;
        //show the slider of more than 3 conenction
        ScrollbarLinks.alpha = !first_person ? 1 : 0;
        //erase connections
        ExplorerButton.interactable = !first_person;
        SaveButton.SetActive(!first_person);
        MainCamera.GetComponent<MouseOrbitImproved_main>().enabled = !first_person ? true : false;

        MainCamera.GetComponent<Animator>().SetBool("Start", first_person);

        if (first_person)
		{
			cctv_overlay.sprite = cctv_start;
			MainCamera.transform.position = new Vector3 (0, 0, camera_distance);
			MainCamera.transform.rotation = Quaternion.identity;
            first_person_protein = -1;
            //change the overlay renderer camera to default one
            //MainCamera.transform.GetComponentInChildren<OverlayRenderer>().lookat_camera = MainCameraComponent;
            //check conenctions
            sfx.PlayTrack(SFX.sound_index.camera_shrink);
        }
        else
        {
            sfx.PlayTrack(SFX.sound_index.camera_expand);
        }

        //hide elements
        //AminoAcids.alpha = !first_person ? 1 : 0;
        //ConnectionButton.alpha = !first_person ? 1 : 0;
        AminoLinks.alpha = !first_person ? 1 : 0;

        //if(ToggleFreeCameraStatus)
        //ToggleFreeCamera();
    }
    

    //public void EndExplore()
    //{
    //    //RenderSettings.skybox = normal_skybox;
    //    // floor.SetActive(true);
    //    sfx.PlayTrack(SFX.sound_index.button_click);
    //    sfx.PlayTrack(SFX.sound_index.camera_shrink);
    //    sfx.StopTrack(SFX.sound_index.ship);
    //    MainCamera.GetComponent<Camera>().enabled = true;
    //    explore_view = false;
    //    Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    //    MainCamera.GetComponent<Animator>().SetBool("Start", false);
    //    if (explorerController.beacon_holder.Count > 0)
    //        ToggleBeacon.isOn = true;
    //    MainCamera.GetComponent<MouseOrbitImproved_main>().enabled = true;
    //    SaveButton.SetActive(true);
    //    Destroy(temp_ship);
    //}

    public void ExplorerBackToDefault()
    {
        ToolPanel.SetBool("Open", true);
        isOverUI = false;
    }

    public void ChangeCCTVLoading()
    {
        cctv_overlay.sprite = cctv_loading;
    }

    public void ChangeCCTVShip()
    {
        cctv_overlay.sprite = cctv_ship;
    }


    public void ToggleScore()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        score_panel_alpha.alpha = toggle_score ? 0 : 1;
        close_score_panel.alpha = toggle_score ? 0 : 1;
        open_score_panel.alpha = toggle_score ? 1 : 0;
        toggle_score = !toggle_score;
    }

    public int previous_level = -1;
    int mesh_offset_temp_1;
    int mesh_offset_temp_2;

    public void LoadLevelDescription(int level_temp, int mesh_temp1, int mesh_temp2, int camera_zoom)
    {
        BioBloxReference.current_level = level_temp;
        previous_level = BioBloxReference.current_level;
       
        MainCamera.GetComponent<MouseOrbitImproved_main>().distance = camera_zoom;
        camera_distance = -camera_zoom;
        RestartCameraFromIntro();
    }

    public void LoadLevelDescriptionIntro(string temp_title, string temp_description, Sprite temp_image, int level_temp)
    {
        level_description.text = temp_description;
        level_title.text = temp_title;
        image_description.sprite = temp_image;
        button_play.SetActive(true);
        button_play.GetComponent<Button>().interactable = true;
        if (level_temp == 6)
            button_play.GetComponent<Button>().interactable = false;
    }


    bool isProtein1Fixed = false;
    bool isProtein2Fixed = false;

    public void FixProtein1()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        BioBloxReference.molecules[0].GetComponent<Rigidbody>().isKinematic = !isProtein1Fixed;
        isProtein1Fixed = !isProtein1Fixed;
    }

    public void FixProtein2()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        BioBloxReference.molecules[1].GetComponent<Rigidbody>().isKinematic = !isProtein2Fixed;
        isProtein2Fixed = !isProtein2Fixed;
    }

    public void FixProtein1_save()
    {
        BioBloxReference.molecules[0].GetComponent<Rigidbody>().isKinematic = !isProtein1Fixed;
        isProtein1Fixed = !isProtein1Fixed;
    }

    public void FixProtein2_save()
    {
        BioBloxReference.molecules[1].GetComponent<Rigidbody>().isKinematic = !isProtein2Fixed;
        isProtein2Fixed = !isProtein2Fixed;
    }



    public void ChangeRenderProtein1()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        switch (DropDownP1.value)
        {
            case 0:
                ToggleNormalMesh(0);
            break;
            case 1:
                ToggleTransparentMesh(0);
                break;
            case 2:
                ToggleBSMesh(0);
                break;
            case 3:
                ToggleCMesh(0);
                break;
        }
    }

    public void ChangeRenderProtein2()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        switch (DropDownP2.value)
        {
            case 0:
                ToggleNormalMesh(1);
                break;
            case 1:
                ToggleTransparentMesh(1);
                break;
            case 2:
                ToggleBSMesh(1);
                break;
            case 3:
                ToggleCMesh(1);
                break;
        }
    }

    void UpdateMeshCutaway(int id_protein, int id_child_mesh)
    {
        foreach (Transform molecule_renderer in BioBloxReference.molecules[id_protein].transform.GetChild(0).transform.GetChild(id_child_mesh).transform)
        {
            molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", CutAway.value);
        }
    }

    public bool cutawayON = false;

    public void CutAwayTool()
    {
        sfx.PlayTrack(SFX.sound_index.cutaway_protein);

        for(int j = 0; j < 2; j++)
        {
            for (int i = 0; i < number_of_meshes - 1; i++)
            {
                foreach (Transform molecule_renderer in BioBloxReference.molecules[j].transform.GetChild(0).transform.GetChild(i).transform)
                {
                    molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", CutAway.value);
                }
            }
        }

        cutaway_wall.localPosition = new Vector3(0, 0, CutAway.value);

        if (CutAway.value == CutAway.minValue)
        {
            cutaway_wall.gameObject.SetActive(false);
            cutawayON = false;
            sfx.StopTrack(SFX.sound_index.cutaway_cutting);
            sfx.StopTrack(SFX.sound_index.cutaway_protein);
            //shadow on
            if (DropDownP1.value == protein_render.normal.GetHashCode())
                Shadows(true, 0);
            if (DropDownP2.value == protein_render.normal.GetHashCode())
                Shadows(true, 1);
        }
        else if(!cutawayON)
        {
            sfx.PlayTrack(SFX.sound_index.cutaway_cutting);
            sfx.PlayTrack(SFX.sound_index.cutaway_start);
            cutaway_wall.gameObject.SetActive(true);
            cutawayON = true;
            //shadow off
            Shadows(false, 0);
            Shadows(false, 1);
            //delete overlays - when cutaway active
            or.clear_spheres();
            or.clear();
        }

    }

    //public void TransparencyProtein(int protein_index)
    //{

    //    foreach (Transform molecule_renderer in BioBloxReference.molecules[protein_index].transform.GetChild(0).transform)
    //    {
    //        molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", 100);
    //    }
    //}

    //public void DefaultProtein(int protein_index)
    //{
    //    foreach (Transform molecule_renderer in BioBloxReference.molecules[protein_index].transform.GetChild(0).transform)
    //    {
    //        molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", CutAway.value);
    //    }
    //}

    public void ToggleNormalMesh(int protein_index)
    {
        sfx.PlayTrack(SFX.sound_index.button_click);

        if(protein_index == 0)
        {
            protein0_render_buttons[0].color = GridToggleColor_pressed;
            protein0_render_buttons[1].color = GridToggleColor_normal;
            protein0_render_buttons[2].color = GridToggleColor_normal;
            protein0_render_buttons[3].color = GridToggleColor_normal;
        }
        else
        {
            protein1_render_buttons[0].color = GridToggleColor_pressed;
            protein1_render_buttons[1].color = GridToggleColor_normal;
            protein1_render_buttons[2].color = GridToggleColor_normal;
            protein1_render_buttons[3].color = GridToggleColor_normal;
        }

        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(true);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(transparent_render).gameObject.SetActive(false);
        BioBloxReference.molecules_PDB_mesh[protein_index].protein_render = PDB_mesh.protein_render_options.normal.GetHashCode();
        UpdateMeshCutaway(protein_index, 0);
        //CheckDefaultMesh(protein_index);
    }

    public void ToggleTransparentMesh(int protein_index)
    {
        sfx.PlayTrack(SFX.sound_index.button_click);

        if (protein_index == 0)
        {
            protein0_render_buttons[0].color = GridToggleColor_normal;
            protein0_render_buttons[1].color = GridToggleColor_pressed;
            protein0_render_buttons[2].color = GridToggleColor_normal;
            protein0_render_buttons[3].color = GridToggleColor_normal;
        }
        else
        {
            protein1_render_buttons[0].color = GridToggleColor_normal;
            protein1_render_buttons[1].color = GridToggleColor_pressed;
            protein1_render_buttons[2].color = GridToggleColor_normal;
            protein1_render_buttons[3].color = GridToggleColor_normal;
        }

        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(transparent_render).gameObject.SetActive(true);
        BioBloxReference.molecules_PDB_mesh[protein_index].protein_render = PDB_mesh.protein_render_options.transparent.GetHashCode();
        //CheckDefaultMesh(protein_index);
    }

    public void ToggleBSMesh(int protein_index)
    {
        sfx.PlayTrack(SFX.sound_index.button_click);

        if (protein_index == 0)
        {
            protein0_render_buttons[0].color = GridToggleColor_normal;
            protein0_render_buttons[1].color = GridToggleColor_normal;
            protein0_render_buttons[2].color = GridToggleColor_pressed;
            protein0_render_buttons[3].color = GridToggleColor_normal;
        }
        else
        {
            protein1_render_buttons[0].color = GridToggleColor_normal;
            protein1_render_buttons[1].color = GridToggleColor_normal;
            protein1_render_buttons[2].color = GridToggleColor_pressed;
            protein1_render_buttons[3].color = GridToggleColor_normal;
        }

        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(true);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(transparent_render).gameObject.SetActive(false);
        BioBloxReference.molecules_PDB_mesh[protein_index].protein_render = PDB_mesh.protein_render_options.bs.GetHashCode();
        UpdateMeshCutaway(protein_index, 2);
        //CheckDefaultMesh(protein_index);
    }

    public void ToggleCMesh(int protein_index)
    {
        sfx.PlayTrack(SFX.sound_index.button_click);

        if (protein_index == 0)
        {
            protein0_render_buttons[0].color = GridToggleColor_normal;
            protein0_render_buttons[1].color = GridToggleColor_normal;
            protein0_render_buttons[2].color = GridToggleColor_normal;
            protein0_render_buttons[3].color = GridToggleColor_pressed;
        }
        else
        {
            protein1_render_buttons[0].color = GridToggleColor_normal;
            protein1_render_buttons[1].color = GridToggleColor_normal;
            protein1_render_buttons[2].color = GridToggleColor_normal;
            protein1_render_buttons[3].color = GridToggleColor_pressed;
        }

        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(0).gameObject.SetActive(false);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(1).gameObject.SetActive(false);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(true);
        BioBloxReference.molecules[protein_index].transform.GetChild(0).transform.GetChild(transparent_render).gameObject.SetActive(false);
        BioBloxReference.molecules_PDB_mesh[protein_index].protein_render = PDB_mesh.protein_render_options.carbon.GetHashCode();
        UpdateMeshCutaway(protein_index, 3);
        //CheckDefaultMesh(protein_index);
    }

    public float time_to_save;

    public void OpenLevelSelectionPanel()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        gm.selection_panel.alpha = 1;
        gm.selection_panel.blocksRaycasts = true;
        gm.MenuButtons.SetActive(true);
        //number_atoms_end_level.text = BioBloxReference.NumberOfAtoms.text;

        ////format
        //int minutes = Mathf.FloorToInt(BioBloxReference.game_time / 60F);
        //int seconds = Mathf.FloorToInt(BioBloxReference.game_time - minutes * 60);
        //string time_game;
        ////string niceTime = string.Format("{0:0}:{1:00}", minutes, seconds);
        //if (minutes == 0)
        //    time_game = seconds + " seconds";
        //else
        //    time_game = minutes + " minutes " + seconds + " seconds";


        //time_end_level.text = time_game;

        ////isnert to xml
        //if (number_atoms_end_level.text != "0" && BioBloxReference.is_score_valid)
        //{
        //    //play audio
        //    sfx.PlayTrack(SFX.sound_index.end_level);
        //    time_to_save = BioBloxReference.game_time;
        //}

    }

    public void BackButtonEndPanel()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        isOverUI = false;
    }
    //for the button in the end level panel
    public void RestartLevel()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        RestartCamera();
        BioBloxReference.ResetDisabledAminoAcids();
        aminoSliderController.DeleteAllAminoConnections();
        //reset position
        BioBloxReference.RestartProteinPositions();
        BioBloxReference.game_time = 0;
        Reset_UI();
        CutAway.value = CutAway.minValue;
        if (BioBloxReference.current_level == 0)
            BioBloxReference.ToggleMode.isOn = true;

        sfx.StopTrack(SFX.sound_index.warning);
        isOverUI = false;
    }

    public void Reset_UI()
    {
        tool_panel_status = true;
        //ToogleToolMenuStart();
        FixProtein1Toggle.isOn = false;
        FixProtein2Toggle.isOn = false;
        CutAway.value = CutAway.minValue;
        //ExpertModeOffStart();
        RestartCameraFromIntro();
        DropDownP1.value = 0;
        DropDownP2.value = 0;
        BioBloxReference.molecules_PDB_mesh[0].DeselectAminoAcid();
        BioBloxReference.molecules_PDB_mesh[1].DeselectAminoAcid();
        aminoSliderController.DeselectAmino();
        //DeselectAtoms();

        isOverUI = false;
    }

    //tool panel
    public void ToggleHint()
    {
        hint_panel_status = !hint_panel_status;
        sfx.PlayTrack(SFX.sound_index.button_click);
        HintPanel.SetBool("Start", hint_panel_status);
        HintPanelOpen.SetActive(!hint_panel_status);
        HintPanelClose.SetActive(hint_panel_status);
    }

    //tool panel
    public void ToggleHintFromIntro()
    {
        hint_panel_status = true;
        HintPanel.SetBool("Start", hint_panel_status);
        HintPanelOpen.SetActive(!hint_panel_status);
        HintPanelClose.SetActive(hint_panel_status);
    }

    public void Shadows(bool status, int protein)
    {
        foreach (Transform molecule_renderer in BioBloxReference.molecules[protein].transform.GetChild(0).transform.GetChild(0).transform)
        {
            molecule_renderer.GetComponent<MeshRenderer>().shadowCastingMode = status ? ShadowCastingMode.On : ShadowCastingMode.Off;
        }
    }

    //to restart when the same level is selcted
    public void SaveLastLevelPlayed(GameObject LevelButtonClickled)
    {
        LevelClickled = LevelButtonClickled;
    }

    public void ToggleAminoButtonRL(bool status)
    {
        A1L.SetActive(status);
        A2L.SetActive(status);
        A1R.SetActive(status);
        A2R.SetActive(status);
    }

    GameObject atom_button_temp;

    public void P1CreateAtomButtons(int atom_id_temp, int protein_id_temp, string atom_name_temp, int element_type, int amino_acid_index, int atom_index)
    {
        atom_button_temp = Instantiate(atom_buttons[element_type]);
        atom_button_temp.transform.SetParent(p1_atom_holder, false);
        atom_button_temp.GetComponentInChildren<Text>().text = atom_name_temp;
        atom_button_temp.GetComponent<AtomConnectionController>().atom_id = atom_id_temp;
        atom_button_temp.GetComponent<AtomConnectionController>().protein_id = protein_id_temp;
        atom_button_temp.GetComponent<AtomConnectionController>().amino_acid_index = amino_acid_index;
        atom_button_temp.GetComponent<AtomConnectionController>().atom_child_index = atom_index;
        atom_button_temp.GetComponent<AtomConnectionController>().element_type = element_type;
        atom_button_temp.GetComponent<AtomConnectionController>().atom_name = atom_name_temp;
    }

    GameObject temp_reference;

    public void P1CleanAtomButtons()
    {
        //DESTROY TAKES A FRAME, CREATE A NEW REFEFERENCE AND DUMP THE OTHER
        temp_reference = Instantiate(AtomHolder);
        temp_reference.transform.SetParent(P1AtomInfo, false);
        Destroy(p1_atom_holder.gameObject);
        p1_atom_holder = temp_reference.transform;
        aminoSliderController.P1AtomsHolder = temp_reference.transform;
    }

    public void P2CreateAtomButtons(int atom_id_temp, int protein_id_temp, string atom_name_temp, int element_type, int amino_acid_index, int atom_index)
    {
        atom_button_temp = Instantiate(atom_buttons[element_type]);
        atom_button_temp.transform.SetParent(p2_atom_holder, false);
        atom_button_temp.GetComponentInChildren<Text>().text = atom_name_temp;
        atom_button_temp.GetComponent<AtomConnectionController>().atom_id = atom_id_temp;
        atom_button_temp.GetComponent<AtomConnectionController>().protein_id = protein_id_temp;
        atom_button_temp.GetComponent<AtomConnectionController>().amino_acid_index = amino_acid_index;
        atom_button_temp.GetComponent<AtomConnectionController>().atom_child_index = atom_index;
        atom_button_temp.GetComponent<AtomConnectionController>().element_type = element_type;
        atom_button_temp.GetComponent<AtomConnectionController>().atom_name = atom_name_temp;
    }

    public void P2CleanAtomButtons()
    {
        //while (p2_atom_holder.childCount != 0)
        //    DestroyImmediate(p2_atom_holder.transform.GetChild(0).gameObject);

        temp_reference = Instantiate(AtomHolder);
        temp_reference.transform.SetParent(P2AtomInfo, false);
        Destroy(p2_atom_holder.gameObject);
        p2_atom_holder = temp_reference.transform;
        aminoSliderController.P2AtomsHolder = temp_reference.transform;
    }

    //void DeselectAtoms()
    //{

    //    foreach (Transform child in p2_atom_holder) Destroy(child.gameObject);
    //    foreach (Transform child in p1_atom_holder) Destroy(child.gameObject);
    //}

   // #region TOGGLE GAME MODE
    
    public bool expert_mode = true;

    //public void GameModeClick()
    //{
    //    if(slider_game_mode.value == previous_slider_value)
    //        slider_game_mode.value = slider_game_mode.value == 0 ? slider_game_mode.maxValue : slider_game_mode.minValue;

    //    previous_slider_value = slider_game_mode.value;
    //}

    //public void ExpertModeOn()
    //{
    //    if(!expert_mode)
    //    {
    //        sfx.PlayTrack(SFX.sound_index.button_click);
    //        P1AtomInfo.gameObject.SetActive(!expert_mode);
    //        P2AtomInfo.gameObject.SetActive(!expert_mode);
    //        score_panel_alpha.alpha = expert_mode ? 0 : 1;
    //        expert_mode = !expert_mode;
    //        #region set the color OF THE BUTTON ON/OFF
    //        //set the color OF THE BUTTON ON/OFF
    //        button_color = OnExpert.colors;
    //        button_color.normalColor = Color.white;
    //        button_color.highlightedColor = Color.white;
    //        button_color.pressedColor = Color.white;
    //        OnExpert.colors = button_color;
    //        //off
    //        button_color = OffExpert.colors;
    //        button_color.normalColor = new Color(0.78F, 0.78F, 0.78F, 0.5F);
    //        button_color.highlightedColor = new Color(0.78F, 0.78F, 0.78F, 0.5F);
    //        button_color.pressedColor = new Color(0.78F, 0.78F, 0.78F, 0.5F);
    //        OffExpert.colors = button_color;
    //        #endregion
    //        foreach(Transform child in AminoLinkPanel.transform)
    //        {
    //            child.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(true);
    //            child.transform.GetChild(0).transform.GetChild(3).gameObject.SetActive(true);
    //            child.transform.GetChild(0).transform.GetChild(4).gameObject.SetActive(true);
    //        }
    //    }
    //}

    //public void ExpertModeOff()
    //{
    //    if (expert_mode)
    //    {
    //        sfx.PlayTrack(SFX.sound_index.button_click);
    //        P1AtomInfo.gameObject.SetActive(!expert_mode);
    //        P2AtomInfo.gameObject.SetActive(!expert_mode);
    //        score_panel_alpha.alpha = expert_mode ? 0 : 1;
    //        expert_mode = !expert_mode;
    //        #region set the color OF THE BUTTON ON/OFF
    //        //set the color
    //        button_color = OnExpert.colors;
    //        button_color.normalColor = new Color(0.78F, 0.78F, 0.78F, 0.5F);
    //        button_color.highlightedColor = new Color(0.78F, 0.78F, 0.78F, 0.5F);
    //        button_color.pressedColor = new Color(0.78F, 0.78F, 0.78F, 0.5F);
    //        OnExpert.colors = button_color;
    //        //off
    //        button_color = OffExpert.colors;
    //        button_color.normalColor = Color.white;
    //        button_color.highlightedColor = Color.white;
    //        button_color.pressedColor = Color.white;
    //        OffExpert.colors = button_color;
    //        #endregion
    //        foreach (Transform child in AminoLinkPanel.transform)
    //        {
    //            child.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
    //            child.transform.GetChild(0).transform.GetChild(3).gameObject.SetActive(false);
    //            child.transform.GetChild(0).transform.GetChild(4).gameObject.SetActive(false);
    //        }
    //    }
    //}

    //public void ExpertModeOffStart()
    //{
    //    if (expert_mode)
    //    {
    //        p1_atom_holder.parent.gameObject.SetActive(!expert_mode);
    //        p2_atom_holder.parent.gameObject.SetActive(!expert_mode);
    //        score_panel_alpha.alpha = expert_mode ? 0 : 1;
    //        expert_mode = !expert_mode;
    //        #region set the color OF THE BUTTON ON/OFF
    //        //set the color
    //        button_color = OnExpert.colors;
    //        button_color.normalColor = new Color(0.78F, 0.78F, 0.78F, 0.5F);
    //        button_color.highlightedColor = new Color(0.78F, 0.78F, 0.78F, 0.5F);
    //        button_color.pressedColor = new Color(0.78F, 0.78F, 0.78F, 0.5F);
    //        OnExpert.colors = button_color;
    //        //off
    //        button_color = OffExpert.colors;
    //        button_color.normalColor = Color.white;
    //        button_color.highlightedColor = Color.white;
    //        button_color.pressedColor = Color.white;
    //        OffExpert.colors = button_color;
    //        #endregion
    //        foreach (Transform child in AminoLinkPanel.transform)
    //        {
    //            child.transform.GetChild(0).transform.GetChild(2).gameObject.SetActive(false);
    //            child.transform.GetChild(0).transform.GetChild(3).gameObject.SetActive(false);
    //            child.transform.GetChild(0).transform.GetChild(4).gameObject.SetActive(false);
    //        }
    //    }
    //}
    //#endregion
  

    #region ATOM OVERLAY
    public void Atom2DDisplay()
    {
        or.atom_2d_overlay = !or.atom_2d_overlay;
    }

    public void Atom3DDisplay()
    {
        or.atom_3d_overlay = !or.atom_3d_overlay;
    }
    #endregion

    public void GoToWebsite()
    {
        Application.OpenURL("http://bioblox.org/");
    }

    #region
    public void StopReel()
    {
        sfx.StopReel();
    }

    public void SliderMouseIn()
    {
        sfx.SliderMouseIn();
    }
    #endregion

    #region SAVE PANEL

    public Text n_atoms;
    public Text lpj;
    public Text ei;
    public Text game_score;
    public GameObject SavePanel;
    public GameObject Tick;
    string P1_connections;
    string P2_connections;
    string connections;
    AminoConnectionHolder ach;

    public void OpenSavePanel()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        FixProtein1_save();
        FixProtein2_save();
        SavePanel.SetActive(true);
    }

    public void CloseSavePanel()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        SavePanel.SetActive(false);
        isOverUI = false;
        FixProtein1_save();
        FixProtein2_save();
    }

    public void SubmitSaveToServer(int slot)
    {
        BioBloxReference.is_validating = false;
        sfx.PlayTrack(SFX.sound_index.end_level);
        BioBloxReference.SlotButtons.alpha = 0.0f;
        BioBloxReference.SlotButtons.blocksRaycasts = false;

        P1_connections = "";
        P2_connections = "";
        connections = "";

        foreach (Transform AminoLinkChild in AminoLinkPanel.transform)
        {
            ach = AminoLinkChild.GetComponent<AminoConnectionHolder>();
            P1_connections += ach.A1_name + "-" + ach.AT1_name + " / ";
            P2_connections += ach.A2_name + "-" + ach.AT2_name + " / ";
            connections += ach.ID_button1 + "-" + ach.ID_button2 + "/";
        }
        
        dm.SendSaveData(slot, n_atoms.text, lpj.text, ei.text, game_score.text, P1_connections, P2_connections, cm.SliderStrings.value, connections);
        UpdateLocalScore(slot, n_atoms.text, lpj.text, ei.text, game_score.text);
        //update button save
        slot_score[slot].text = "score: " + BioBloxReference.game_score.text;

        Tick.SetActive(true);
        StartCoroutine(WaitForSec());
    }

    IEnumerator WaitForSec()
    {
        yield return new WaitForSeconds(2);
        SavePanel.SetActive(false);
        Tick.SetActive(false);
        BioBloxReference.validating_holder.SetActive(true);
        isOverUI = false;
        BioBloxReference.is_validating = true;
        FixProtein1_save();
        FixProtein2_save();
    }

    void UpdateLocalScore(int slot, string n_atoms, string lpj, string ei, string game_score)
    {
        string[] new_scores = { game_score, n_atoms, lpj, ei};

        Transform level_holder = GameObject.FindGameObjectWithTag("level_holder").gameObject.transform;

        level_holder.GetChild(BioBloxReference.current_level).transform.GetChild(level_holder.GetChild(0).childCount - 1).transform.GetChild(slot).GetComponent<SlotController>().SetValues(new_scores);

        //if (int.Parse(level_holder.GetChild(BioBloxReference.current_level).transform.GetChild(level_holder.GetChild(0).childCount - 1).transform.GetChild(slot).GetComponent<Text>().text) < int.Parse(game_score))
        //{
        //    level_holder.GetChild(BioBloxReference.current_level).transform.GetChild(1).GetComponent<Text>().text = game_score;
        //    level_holder.GetChild(BioBloxReference.current_level).transform.GetChild(2).GetComponent<Text>().text = n_atoms;
        //    level_holder.GetChild(BioBloxReference.current_level).transform.GetChild(3).GetComponent<Text>().text = lpj;
        //    level_holder.GetChild(BioBloxReference.current_level).transform.GetChild(4).GetComponent<Text>().text = ei;
        //}
    }

    //void GetAminoAtoms()
    //{
    //    P1_connections = "";
    //    P2_connections = "";

    //    foreach (Transform AminoLinkChild in AminoLinkPanel.transform)
    //    {
    //        ach = AminoLinkChild.GetChild(0).GetComponent<AminoConnectionHolder>();
    //        P1_connections += ach.A1_name + "-" + ach.AT1_name + " / ";
    //        P2_connections += ach.A2_name + "-" + ach.AT2_name + " / ";
    //    }

    //}

    #endregion

    public void OpenTutorialInitialPanel()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        gm.tutorial_panel.SetActive(true);
    }

    public void EraseAminoButton_Atom_reference_0()
    {
        if (AminoButton_Atom_reference_0 != null)
        {
            Destroy(AminoButton_Atom_reference_0);
            AminoButton_reference_0.transform.GetChild(2).GetComponent<Image>().sprite = CloseAtomPanel;
        }
    }

    public void EraseAminoButton_Atom_reference_1()
    {
        if (AminoButton_Atom_reference_1 != null)
        {
            Destroy(AminoButton_Atom_reference_1);
            AminoButton_reference_1.transform.GetChild(2).GetComponent<Image>().sprite = CloseAtomPanel;
        }
    }

    public void ToggleGrid()
    {
        floor.SetActive(!is_grid_active);
        GridToggleButton.color = is_grid_active ? GridToggleColor_normal : GridToggleColor_pressed;
        is_grid_active = !is_grid_active;
    }
}
