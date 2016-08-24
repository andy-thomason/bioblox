using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

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

    void Awake()
	{
		aminoSliderController = FindObjectOfType<AminoSliderController> ();
        BioBloxReference = FindObjectOfType<BioBlox>();
        explorerController = FindObjectOfType<ExploreController>();
        MainCameraComponent = MainCamera.GetComponent<Camera>();
    }

    Vector3 dragOrigin;
    float dragSpeed = 20.0f;
    Vector3 move;

    // Update is called once per frame
    void Update () {

        //if(Input.GetKeyDown (KeyCode.F) && !ToggleFreeCameraStatus) {
        //	MainCamera.GetComponent<CameraMovement> ().enabled = CameraFreeze;			
        //	FreeCameraKeysFreeze.SetActive (CameraFreeze); 
        //	FreeCameraKeysUnfreeze.SetActive (!CameraFreeze);
        //	CameraFreeze = !CameraFreeze;
        //}
        //if first person of exploration not zoom nor movement
        if (!first_person && !explore_view)
        {
            //camera zoom
            if (Input.GetAxis("Mouse ScrollWheel") > 0) // back
            {
                if (MainCameraComponent.fieldOfView > 1)
                    MainCameraComponent.fieldOfView -= 2f;
                //MainCamera.transform.LookAt(MainCameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, MainCameraComponent.nearClipPlane)), Vector3.up);

            }
            else if (Input.GetAxis("Mouse ScrollWheel") < 0)
            {
                if (MainCameraComponent.fieldOfView < 33)
                    MainCameraComponent.fieldOfView += 2f;
                //MainCamera.transform.LookAt(MainCameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, MainCameraComponent.nearClipPlane)), Vector3.up);
            }

            if (Input.GetMouseButtonDown(1))
            {
                dragOrigin = Input.mousePosition;
            }

            if (Input.GetMouseButton(1))
            {

                Cursor.SetCursor(cursor_move, Vector2.zero, CursorMode.Auto);

                Vector3 pos = MainCameraComponent.ScreenToViewportPoint(Input.mousePosition - dragOrigin);

                if (MainCamera.transform.position.y > -25)
                {
                    move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0);
                }
                else
                {
                    if (pos.y < 0)
                        move = new Vector3(pos.x * dragSpeed, 0, 0);
                    else
                        move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0);
                }


                MainCamera.transform.Translate(move, Space.Self);
            }
            else
            {
                Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            }
        }

        //if the user is over the small camera in the first person, the harpoon will not be shot
        if (first_person)
        {
            if (first_person_protein != -1)
            {
                if (MainCameraComponent.pixelRect.Contains(Input.mousePosition))
                    is_moving_camera_first_person = true;
                else
                    is_moving_camera_first_person = false;
            }

            atom_name_firstperson.text = "";
            if (FirstPersonCameraReferenceCamera && !is_moving_camera_first_person && !isOverUI)
            {
                //scanning
                Ray ray = FirstPersonCameraReferenceCamera.ScreenPointToRay(Input.mousePosition);
                int protein_raycast = first_person_protein != 0 ? 0 : 1;

                //MOLECULE 1 ATOM ID UI
                int atomID_molecule = PDB_molecule.collide_ray(FirstPersonCameraReference, BioBloxReference.molecules[protein_raycast].GetComponent<PDB_mesh>().mol, BioBloxReference.molecules[protein_raycast].transform, ray);
                int atom_id_molecule = BioBloxReference.molecules[protein_raycast].GetComponent<PDB_mesh>().return_atom_id(atomID_molecule);
                //change the text in the UI depending which atom is being raycasted
                if (atom_id_molecule >= 0)
                    atom_name_firstperson.text = BioBloxReference.molecules[protein_raycast].GetComponent<PDB_mesh>().mol.aminoAcidsNames[BioBloxReference.molecules[protein_raycast].GetComponent<PDB_mesh>().return_atom_id(atom_id_molecule)] + " - " + BioBloxReference.molecules[protein_raycast].GetComponent<PDB_mesh>().mol.aminoAcidsTags[BioBloxReference.molecules[protein_raycast].GetComponent<PDB_mesh>().return_atom_id(atom_id_molecule)];
            }

        }

        if (first_person && !is_moving_camera_first_person && !isOverUI)
        {
            if (Input.GetMouseButtonDown(0) && FirstPersonCameraReference != null)
            {
                //SHOOTING THE ARPON
                //where to shoot
                Vector3 position;
                //scanning
                Ray ray = FirstPersonCameraReferenceCamera.ScreenPointToRay(Input.mousePosition);
                int protein_raycast = first_person_protein != 0 ? 0 : 1;

                //MOLECULE 1 ATOM ID UI
                int atomID_molecule = PDB_molecule.collide_ray(FirstPersonCameraReference, BioBloxReference.molecules[protein_raycast].GetComponent<PDB_mesh>().mol, BioBloxReference.molecules[protein_raycast].transform, ray);
                int atom_id_molecule = BioBloxReference.molecules[protein_raycast].GetComponent<PDB_mesh>().return_atom_id(atomID_molecule);
                //change the text in the UI depending which atom is being raycasted
                if (atom_id_molecule >= 0)
                {
                    position = BioBloxReference.molecules[protein_raycast].transform.TransformPoint(BioBloxReference.molecules[protein_raycast].GetComponent<PDB_mesh>().mol.atom_centres[atomID_molecule]);
                }
                else
                {
                    position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 200.0f);
                    position = FirstPersonCameraReference.GetComponent<Camera>().ScreenToWorldPoint(position);
                }

                //GameObject ArponObject = GameObject.FindGameObjectWithTag("arpon");
                GameObject arpon_reference = Instantiate(Arpon, BioBloxReference.molecules[first_person_protein].transform.TransformPoint(BioBloxReference.molecules[first_person_protein].GetComponent<PDB_mesh>().mol.atom_centres[BioBloxReference.molecules[first_person_protein].GetComponent<PDB_mesh>().atom]), Quaternion.identity) as GameObject;
                arpon_reference.GetComponent<ArponController>().enabled = true;
                //GameObject arpon_reference = Instantiate(prefab, transform.position, Quaternion.identity) as GameObject;
                arpon_reference.transform.LookAt(position);
                Debug.Log(position);
                arpon_reference.GetComponent<Rigidbody>().AddForce(arpon_reference.transform.forward * 1000);
            }
        }
        


    }

	public void ToggleFreeCamera()
	{
		MainCamera.GetComponent<CameraMovement> ().enabled = ToggleFreeCameraStatus;
		FreeCameraKeysFreeze.SetActive (ToggleFreeCameraStatus);
        CameraSlider.SetActive(!ToggleFreeCameraStatus);

        FreeCameraKeysUnfreeze.SetActive (false);
		ToogleToolMenu (false);
		AddConnectionText.SetActive (false);
		ToggleFreeCameraStatus = !ToggleFreeCameraStatus;
	}

	//tool panel
	public void ToogleToolMenu(bool Status)
	{
		ToolPanel.SetBool ("Open", Status);
		OpenToolImage.SetActive (!Status);
		CloseToolImage.SetActive (Status);
	}

	public void RestartCamera()
	{
		MainCamera.transform.position = new Vector3 (0, 0, -150);
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

   public void DropDownProteinView(int molecule)
    {
        if(molecule == 0)
        {
            switch(DropDownProtein1.value)
            {
                case 0:
                    BioBloxReference.SolidClicked(0);
                    break;
                case 1:
                    BioBloxReference.PointClicked(0);
                    break;
                case 2:
                    BioBloxReference.WireClicked(0);
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (DropDownProtein2.value)
            {
                case 0:
                    BioBloxReference.SolidClicked(1);
                    break;
                case 1:
                    BioBloxReference.PointClicked(1);
                    break;
                case 2:
                    BioBloxReference.WireClicked(1);
                    break;
                default:
                    break;
            }
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
        CutAway.value = -60;
        isOverUI = false;
        SetCameraDefaultPosition();
        Tutorial.isOn = false;
        Tutorial.enabled = first_person;
        //show the slider of more than 3 conenction
        ScrollbarLinks.alpha = !first_person ? 0 : 1;
        first_person = !first_person;
		//FreeCameraToggle.interactable = !first_person;
        ExplorerButton.interactable = !first_person;

        MainCamera.GetComponent<Animator>().SetBool("Start", first_person);
        if (first_person)
		{
			cctv_overlay.sprite = cctv_start;
			MainCamera.transform.position = new Vector3 (0, 0, -150);
			MainCamera.transform.rotation = Quaternion.identity;
            first_person_protein = -1;
        }

        //hide elements
        AminoAcids.alpha = !first_person ? 1 : 0;
        ConnectionButton.alpha = !first_person ? 1 : 0;
        AminoLinks.alpha = !first_person ? 1 : 0;

        if(!first_person)
        {
            DropDownP1.value = 0;
            DropDownP2.value = 0;
        }

        //if(ToggleFreeCameraStatus)
        //ToggleFreeCamera();
    }

    public void StartExplore()
    {

        //RenderSettings.skybox = space_skybox;
        //floor.SetActive(false);
        isOverUI = false;
        SetCameraDefaultPosition();
        CutAway.value = -60;
        Tutorial.isOn = false;
        explore_view = true;
        MainCamera.GetComponent<Animator>().SetBool("Start", true);
        MainCanvas.SetActive(false);
        ChangeCCTVShip();
        MainCamera.transform.position = new Vector3(0, 0, -150);
        MainCamera.transform.rotation = Quaternion.identity;
    }

    public void EndExplore()
    {
        //RenderSettings.skybox = normal_skybox;
       // floor.SetActive(true);
        explore_view = false;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        MainCamera.GetComponent<Animator>().SetBool("Start", false);
        if (explorerController.beacon_holder.Count > 0)
            ToggleBeacon.isOn = true;
    }

    public void ExplorerBackToDefault()
    {
        ToolPanel.SetBool("Open", true);
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
        score_panel_alpha.alpha = toggle_score ? 0 : 1;
        close_score_panel.alpha = toggle_score ? 0 : 1;
        open_score_panel.alpha = toggle_score ? 1 : 0;
        toggle_score = !toggle_score;
    }

    public void LoadLevelDescription(string temp_title, string temp_description, Sprite temp_image)
    {
        level_description.text = temp_description;
        level_title.text = temp_title;
        image_description.sprite = temp_image;
        button_play.SetActive(true);
    }

    bool isProtein1Fixed = false;
    bool isProtein2Fixed = false;

    public void FixProtein1()
    {
        BioBloxReference.molecules[0].GetComponent<Rigidbody>().isKinematic = !isProtein1Fixed;
        isProtein1Fixed = !isProtein1Fixed;
    }

    public void FixProtein2()
    {
        BioBloxReference.molecules[1].GetComponent<Rigidbody>().isKinematic = !isProtein2Fixed;
        isProtein2Fixed = !isProtein2Fixed;
    }

    public void ChangeRenderProtein1()
    {
        switch (DropDownP1.value)
        {
            case 0:
                DefaultProtein(0);
            break;
            case 1:
                TransparencyProtein(0);
                break;
        }
    }

    public void ChangeRenderProtein2()
    {
        switch (DropDownP2.value)
        {
            case 0:
                DefaultProtein(1);
                break;
            case 1:
                TransparencyProtein(1);
                break;
        }
    }

    public void CutAwayTool()
    {
        if(DropDownP1.value != 1)
        {
            foreach (Transform molecule_renderer in BioBloxReference.molecules[0].transform.GetChild(0).transform)
            {
                molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", CutAway.value);
            }
        }
        if (DropDownP2.value != 1)
        {
            foreach (Transform molecule_renderer in BioBloxReference.molecules[1].transform.GetChild(0).transform)
            {
                molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", CutAway.value);
            }
        }

    }
   
    public void SetCameraDefaultPosition()
    {
        MainCamera.GetComponent<Camera>().fieldOfView = 33;
        CameraSlider.GetComponent<Slider>().value = -90;
    }

    public void TransparencyProtein(int protein_index)
    {

        foreach (Transform molecule_renderer in BioBloxReference.molecules[protein_index].transform.GetChild(0).transform)
        {
            molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", 30);
        }
    }

    public void DefaultProtein(int protein_index)
    {
        foreach (Transform molecule_renderer in BioBloxReference.molecules[protein_index].transform.GetChild(0).transform)
        {
            molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", CutAway.value);
        }
    }

}
