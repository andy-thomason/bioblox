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

    bool switch_material = true;

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

        //camera zoom
        if (Input.GetAxis("Mouse ScrollWheel") > 0) // back
        {
            if (MainCameraComponent.fieldOfView > 1)
                MainCameraComponent.fieldOfView -= 2f;
            //MainCamera.transform.LookAt(MainCameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, MainCameraComponent.nearClipPlane)), Vector3.up);

        }
        else if(Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            if (MainCameraComponent.fieldOfView < 33)
                MainCameraComponent.fieldOfView += 2f;
            //MainCamera.transform.LookAt(MainCameraComponent.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, MainCameraComponent.nearClipPlane)), Vector3.up);
        }

        //if (Input.GetMouseButtonDown(1))
        //{
        //    dragOrigin = Input.mousePosition;
        //}

        //if (Input.GetMouseButton(1))
        //{ 

        //    Vector3 pos = MainCameraComponent.ScreenToViewportPoint(Input.mousePosition - dragOrigin);

        //    if (MainCamera.transform.position.y > -25)
        //    {
        //        move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0);
        //    }
        //    else
        //    {
        //        if (pos.y < 0)
        //            move = new Vector3(pos.x * dragSpeed, 0, 0);
        //        else
        //            move = new Vector3(pos.x * dragSpeed, pos.y * dragSpeed, 0);
        //    }


        //    MainCamera.transform.Translate(move,Space.Self);
        //}

        if (first_person && Input.GetMouseButtonDown(1))
        {
            //SHOOTING THE ARPON
            Vector3 position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 40.0f);
            position = GameObject.FindGameObjectWithTag("FirstPerson").GetComponent<Camera>().ScreenToWorldPoint(position);
            GameObject ArponObject = GameObject.FindGameObjectWithTag("arpon");
            GameObject arpon_reference = Instantiate(ArponObject, new Vector3(GameObject.FindGameObjectWithTag("FirstPerson").transform.position.x, GameObject.FindGameObjectWithTag("FirstPerson").transform.position.y - 5, GameObject.FindGameObjectWithTag("FirstPerson").transform.position.z), Quaternion.identity) as GameObject;
            arpon_reference.GetComponent<ArponController>().enabled = true;
            //GameObject arpon_reference = Instantiate(prefab, transform.position, Quaternion.identity) as GameObject;
            arpon_reference.transform.LookAt(position);
            Debug.Log(position);
            arpon_reference.GetComponent<Rigidbody>().AddForce(arpon_reference.transform.forward * 700);
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

	/*void ChangeSliderViewToFunction()
	{
		foreach (Transform AminoButton in SliderProtein1.transform)
		{
			AminoButton.GetComponent<Image>().color = AminoButton.GetComponent<AminoButtonController>().FunctionColor;
		}
		foreach (Transform AminoButton in SliderProtein2.transform)
		{
			AminoButton.GetComponent<Image>().color = AminoButton.GetComponent<AminoButtonController>().FunctionColor;
		}
		FunctionInfoObject.SetActive (true);
		//Change the coor of the buttons connections to function
		ChangeAminoLinkPanelButtonColorToFunction ();
        //default states of the filter bar
        FunctionInfoPanelOpenImage.SetActive(false);
        FunctionInfoPanelCloseImage.SetActive(true);
        FunctionInfoPanelStatus = true;
    }

	void ChangeSliderViewToNormal()
	{
		foreach (Transform AminoButton in SliderProtein1.transform)
		{
			AminoButton.GetComponent<Image>().color = AminoButton.GetComponent<AminoButtonController>().NormalColor;
		}
		foreach (Transform AminoButton in SliderProtein2.transform)
		{
			AminoButton.GetComponent<Image>().color = AminoButton.GetComponent<AminoButtonController>().NormalColor;
		}
		FunctionInfoObject.SetActive (false);
        //set all the buttons ON when return to normal view
        //protein 1
        ToggleAllButtonsSlider(aminoSliderController.A1Hydro, true);
        ToggleAllButtonsSlider(aminoSliderController.A1Negative, true);
        ToggleAllButtonsSlider(aminoSliderController.A1Polar, true);
        ToggleAllButtonsSlider(aminoSliderController.A1Positive, true);
        ToggleAllButtonsSlider(aminoSliderController.A1Other, true);
        //protein 2
        ToggleAllButtonsSlider(aminoSliderController.A2Hydro, true);
        ToggleAllButtonsSlider(aminoSliderController.A2Polar, true);
        ToggleAllButtonsSlider(aminoSliderController.A2Positive, true);
        ToggleAllButtonsSlider(aminoSliderController.A2Negative, true);
        ToggleAllButtonsSlider(aminoSliderController.A2Other, true);
		//set all the togles ON
		for (int i = 0; i< ToggleButtonFunctionsView.Length; i++)
		{
			ToggleButtonFunctionsView[i].isOn = true;
		}
		//set the color of the buttons of the connections tonormal
		ChangeAminoLinkPanelButtonColorToNormal ();
			 
	}*/

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

    public void AutoFilterAminoSlider()
    {
        //set all the buttons ON when return to normal view
        //protein 1
        ToggleAllButtonsSlider(aminoSliderController.A1Hydro, auto_filter);
        ToggleAllButtonsSlider(aminoSliderController.A1Negative, auto_filter);
        ToggleAllButtonsSlider(aminoSliderController.A1Polar, auto_filter);
        ToggleAllButtonsSlider(aminoSliderController.A1Positive, auto_filter);
        ToggleAllButtonsSlider(aminoSliderController.A1Other, auto_filter);
        //protein 2
        ToggleAllButtonsSlider(aminoSliderController.A2Hydro, auto_filter);
        ToggleAllButtonsSlider(aminoSliderController.A2Polar, auto_filter);
        ToggleAllButtonsSlider(aminoSliderController.A2Positive, auto_filter);
        ToggleAllButtonsSlider(aminoSliderController.A2Negative, auto_filter);
        ToggleAllButtonsSlider(aminoSliderController.A2Other, auto_filter);

        //set all the togles ON
        for (int i = 0; i < ToggleButtonFunctionsView.Length; i++)
        {
            ToggleButtonFunctionsView[i].isOn = auto_filter;
        }

        auto_filter = !auto_filter;
    }

    public void ToggleAminoAcidsToggles(bool status)
    {
        //set all the buttons ON when return to normal view
        //protein 1
        ToggleAllButtonsSlider(aminoSliderController.A1Hydro, status);
        ToggleAllButtonsSlider(aminoSliderController.A1Negative, status);
        ToggleAllButtonsSlider(aminoSliderController.A1Polar, status);
        ToggleAllButtonsSlider(aminoSliderController.A1Positive, status);
        ToggleAllButtonsSlider(aminoSliderController.A1Other, status);
        //protein 2
        ToggleAllButtonsSlider(aminoSliderController.A2Hydro, status);
        ToggleAllButtonsSlider(aminoSliderController.A2Polar, status);
        ToggleAllButtonsSlider(aminoSliderController.A2Positive, status);
        ToggleAllButtonsSlider(aminoSliderController.A2Negative, status);
        ToggleAllButtonsSlider(aminoSliderController.A2Other, status);

        //set all the togles ON
        for (int i = 0; i < ToggleButtonFunctionsView.Length; i++)
        {
            ToggleButtonFunctionsView[i].isOn = status;
        }
    }

    //bool amino_status;

    public void TogglePositiveA1()
	{
        if (auto_filter)
        {
            ToggleAminoAcidsToggles(false);
            //amino_status = !ToggleButtonFunctionsView[0].isOn;
            //ToggleButtonFunctionsView[0].isOn = amino_status;
            foreach (GameObject AminoButton in aminoSliderController.A1Positive)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[0].isOn);
            }
            foreach (GameObject AminoButton in aminoSliderController.A2Negative)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[0].isOn);
            }
            ToggleButtonFunctionsView[0].isOn = ToggleButtonFunctionsView[6].isOn = true;
        }
        else
        {
            foreach (GameObject AminoButton in aminoSliderController.A1Positive)
            {
                AminoButton.SetActive(ToggleButtonFunctionsView[0].isOn);
            }

        }
        //set id_s
        aminoSliderController.UpdateButtonTempIDA1();
	}

	public void ToggleNegativeA1()
	{
        if (auto_filter)
        {
            ToggleAminoAcidsToggles(false);
            //amino_status = !ToggleButtonFunctionsView[0].isOn;
            //ToggleButtonFunctionsView[0].isOn = amino_status;
            foreach (GameObject AminoButton in aminoSliderController.A1Negative)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[1].isOn);
            }
            foreach (GameObject AminoButton in aminoSliderController.A2Positive)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[1].isOn);
            }
            ToggleButtonFunctionsView[1].isOn = ToggleButtonFunctionsView[5].isOn = true;
        }
        else {
            foreach (GameObject AminoButton in aminoSliderController.A1Negative)
            {
                AminoButton.SetActive(ToggleButtonFunctionsView[1].isOn);
            }
        }
        //set id_s
        aminoSliderController.UpdateButtonTempIDA1();
    }

	public void TogglePolarA1()
	{
        if (auto_filter)
        {
            ToggleAminoAcidsToggles(false);
            //amino_status = !ToggleButtonFunctionsView[0].isOn;
            //ToggleButtonFunctionsView[0].isOn = amino_status;
            foreach (GameObject AminoButton in aminoSliderController.A1Polar)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[3].isOn);
            }
            foreach (GameObject AminoButton in aminoSliderController.A2Polar)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[3].isOn);
            }
            ToggleButtonFunctionsView[3].isOn = ToggleButtonFunctionsView[8].isOn = true;
        }
        else {
            foreach (GameObject AminoButton in aminoSliderController.A1Polar)
            {
                AminoButton.SetActive(ToggleButtonFunctionsView[3].isOn);
            }
        }
        //set id_s
        aminoSliderController.UpdateButtonTempIDA1();
    }

	public void ToggleOtherA1()
	{
        if (auto_filter)
        {
            ToggleAminoAcidsToggles(false);
            //amino_status = !ToggleButtonFunctionsView[0].isOn;
            //ToggleButtonFunctionsView[0].isOn = amino_status;
            foreach (GameObject AminoButton in aminoSliderController.A1Other)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[4].isOn);
            }
            foreach (GameObject AminoButton in aminoSliderController.A2Other)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[4].isOn);
            }
            ToggleButtonFunctionsView[4].isOn = ToggleButtonFunctionsView[9].isOn = true;
        }
        else {
            foreach (GameObject AminoButton in aminoSliderController.A1Other)
            {
                AminoButton.SetActive(ToggleButtonFunctionsView[4].isOn);
            }
        }
        //set id_s
        aminoSliderController.UpdateButtonTempIDA1();
    }

	public void ToggleHydroA1()
	{
        if (auto_filter)
        {
            ToggleAminoAcidsToggles(false);
            //amino_status = !ToggleButtonFunctionsView[0].isOn;
            //ToggleButtonFunctionsView[0].isOn = amino_status;
            foreach (GameObject AminoButton in aminoSliderController.A1Hydro)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[2].isOn);
            }
            foreach (GameObject AminoButton in aminoSliderController.A2Hydro)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[2].isOn);
            }
            ToggleButtonFunctionsView[2].isOn = ToggleButtonFunctionsView[7].isOn = true;
        }
        else {
            foreach (GameObject AminoButton in aminoSliderController.A1Hydro)
            {
                AminoButton.SetActive(ToggleButtonFunctionsView[2].isOn);
            }
        }
        //set id_s
        aminoSliderController.UpdateButtonTempIDA1();
    }

	//AMINO ACIDS 2

	public void TogglePositiveA2()
	{
        if (auto_filter)
        {
            ToggleAminoAcidsToggles(false);
            //amino_status = !ToggleButtonFunctionsView[0].isOn;
            //ToggleButtonFunctionsView[0].isOn = amino_status;
            foreach (GameObject AminoButton in aminoSliderController.A2Positive)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[5].isOn);
            }
            foreach (GameObject AminoButton in aminoSliderController.A1Negative)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[5].isOn);
            }
            ToggleButtonFunctionsView[1].isOn = ToggleButtonFunctionsView[5].isOn = true;
        }
        else {
            foreach (GameObject AminoButton in aminoSliderController.A2Positive)
            {
                AminoButton.SetActive(ToggleButtonFunctionsView[5].isOn);
            }
        }
        //set id_s
        aminoSliderController.UpdateButtonTempIDA2();
    }
	
	public void ToggleNegativeA2()
	{
        if (auto_filter)
        {
            ToggleAminoAcidsToggles(false);
            //amino_status = !ToggleButtonFunctionsView[0].isOn;
            //ToggleButtonFunctionsView[0].isOn = amino_status;
            foreach (GameObject AminoButton in aminoSliderController.A1Positive)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[6].isOn);
            }
            foreach (GameObject AminoButton in aminoSliderController.A2Negative)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[6].isOn);
            }
            ToggleButtonFunctionsView[0].isOn = ToggleButtonFunctionsView[6].isOn = true;

        }
        else {
            foreach (GameObject AminoButton in aminoSliderController.A2Negative)
            {
                AminoButton.SetActive(ToggleButtonFunctionsView[6].isOn);
            }
        }
        //set id_s
        aminoSliderController.UpdateButtonTempIDA2();
    }
	
	public void TogglePolarA2()
	{
        if (auto_filter)
        {
            ToggleAminoAcidsToggles(false);
            //amino_status = !ToggleButtonFunctionsView[0].isOn;
            //ToggleButtonFunctionsView[0].isOn = amino_status;
            foreach (GameObject AminoButton in aminoSliderController.A1Polar)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[8].isOn);
            }
            foreach (GameObject AminoButton in aminoSliderController.A2Polar)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[8].isOn);
            }
            ToggleButtonFunctionsView[3].isOn = ToggleButtonFunctionsView[8].isOn = true;
        }
        else {
            foreach (GameObject AminoButton in aminoSliderController.A2Polar)
            {
                AminoButton.SetActive(ToggleButtonFunctionsView[8].isOn);
            }
        }
        //set id_s
        aminoSliderController.UpdateButtonTempIDA2();
    }
	
	public void ToggleOtherA2()
	{
        if (auto_filter)
        {
            ToggleAminoAcidsToggles(false);
            //amino_status = !ToggleButtonFunctionsView[0].isOn;
            //ToggleButtonFunctionsView[0].isOn = amino_status;
            foreach (GameObject AminoButton in aminoSliderController.A2Other)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[9].isOn);
            }
            foreach (GameObject AminoButton in aminoSliderController.A1Other)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[9].isOn);
            }
            ToggleButtonFunctionsView[4].isOn = ToggleButtonFunctionsView[9].isOn = true;
        }
        else {
            foreach (GameObject AminoButton in aminoSliderController.A2Other)
            {
                AminoButton.SetActive(ToggleButtonFunctionsView[9].isOn);
            }
        }
        //set id_s
        aminoSliderController.UpdateButtonTempIDA2();
    }
	
	public void ToggleHydroA2()
	{
        if (auto_filter)
        {
            ToggleAminoAcidsToggles(false);
            //amino_status = !ToggleButtonFunctionsView[0].isOn;
            //ToggleButtonFunctionsView[0].isOn = amino_status;
            foreach (GameObject AminoButton in aminoSliderController.A2Hydro)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[7].isOn);
            }
            foreach (GameObject AminoButton in aminoSliderController.A1Hydro)
            {
                AminoButton.SetActive(!ToggleButtonFunctionsView[7].isOn);
            }
            ToggleButtonFunctionsView[2].isOn = ToggleButtonFunctionsView[7].isOn = true;
        }
        else {
            foreach (GameObject AminoButton in aminoSliderController.A2Hydro)
            {
                AminoButton.SetActive(ToggleButtonFunctionsView[7].isOn);
            }
        }
        //set id_s
        aminoSliderController.UpdateButtonTempIDA2();
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

    public CanvasGroup AminoAcids;
    public CanvasGroup ConnectionButton;
    public CanvasGroup AminoLinks;
    public Toggle Tutorial;
    public Slider CutAway;
    public Toggle ToggleBeacon;

    public void FirstPersonToggle()
    {
        CutAway.value = -30;
        isOverUI = false;
        MainCamera.GetComponent<Camera>().fieldOfView = 33;
        Tutorial.isOn = false;
        first_person = !first_person;
		//FreeCameraToggle.interactable = !first_person;
        ExplorerButton.interactable = !first_person;

        MainCamera.GetComponent<Animator>().SetBool("Start", first_person);
        if (first_person)
		{
			cctv_overlay.sprite = cctv_start;
			MainCamera.transform.position = new Vector3 (0, 0, -150);
			MainCamera.transform.rotation = Quaternion.identity;
		}

        //hide elements
        AminoAcids.alpha = !first_person ? 1 : 0;
        ConnectionButton.alpha = !first_person ? 1 : 0;
        AminoLinks.alpha = !first_person ? 1 : 0;

        //if(ToggleFreeCameraStatus)
        //ToggleFreeCamera();
    }

    public Material space_skybox;
    public Material normal_skybox;
    public GameObject floor;

    public void StartExplore()
    {

        //RenderSettings.skybox = space_skybox;
        //floor.SetActive(false);
        isOverUI = false;
        MainCamera.GetComponent<Camera>().fieldOfView = 33;
        CutAway.value = -30;
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

    bool toggle_score = true;
    public CanvasGroup score_panel_alpha;
    public CanvasGroup close_score_panel;
    public CanvasGroup open_score_panel;

    public void ToggleScore()
    {
        score_panel_alpha.alpha = toggle_score ? 0 : 1;
        close_score_panel.alpha = toggle_score ? 0 : 1;
        open_score_panel.alpha = toggle_score ? 1 : 0;
        toggle_score = !toggle_score;
    }

    public Text level_description;
    public Text level_title;
    public Image image_description;
    public GameObject button_play;

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
                foreach (Transform molecule_renderer in BioBloxReference.molecules[0].transform.GetChild(0).transform)
                {
                    molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", CutAway.value);
                }
            break;
            case 1:
                foreach (Transform molecule_renderer in BioBloxReference.molecules[0].transform.GetChild(0).transform)
                {
                    molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", 30);
                }
                break;
        }
    }

    public void ChangeRenderProtein2()
    {
        switch (DropDownP2.value)
        {
            case 0:
                foreach (Transform molecule_renderer in BioBloxReference.molecules[1].transform.GetChild(0).transform)
                {
                    molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", CutAway.value);
                }
                break;
            case 1:
                foreach (Transform molecule_renderer in BioBloxReference.molecules[1].transform.GetChild(0).transform)
                {
                    molecule_renderer.GetComponent<Renderer>().material.SetFloat("_Distance", 30);
                }
                break;
        }
    }

    public void CutAwayTool()
    {
        //Change the material to cutaway material
        //if (!BioBloxReference.molecules[0].transform.GetChild(0).transform.GetChild(0).GetComponent<Renderer>().material.HasProperty("_Distance"))
        //{
        //    foreach (Transform molecule_renderer in BioBloxReference.molecules[0].transform.GetChild(0).transform)
        //    {
        //        molecule_renderer.GetComponent<Renderer>().material = default_material;
        //    }
        //    foreach (Transform molecule_renderer in BioBloxReference.molecules[1].transform.GetChild(0).transform)
        //    {
        //        molecule_renderer.GetComponent<Renderer>().material = default_material;
        //    }
        //}

        //Move the cutaway 
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


    //TEMP
    public void TransparentRender()
    {
        if (switch_material)
        {
            for (int i = 0; i < BioBloxReference.molecules.Length; i++)
            {
                foreach (Transform molecule_renderer in BioBloxReference.molecules[i].transform.GetChild(0).transform)
                {
                    molecule_renderer.GetComponent<Renderer>().material = transparent_material;
                }
            }
            switch_material = !switch_material;
        }
        else
        {
            for (int i = 0; i < BioBloxReference.molecules.Length; i++)
            {
                foreach (Transform molecule_renderer in BioBloxReference.molecules[i].transform.GetChild(0).transform)
                {
                    molecule_renderer.GetComponent<Renderer>().material = default_material;
                }
            }
            switch_material = !switch_material;

        }
    }
    //TEMP

}
