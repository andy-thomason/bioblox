using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class UIController : MonoBehaviour {

	public GameObject FreeCameraKeysFreeze;
	public GameObject FreeCameraKeysUnfreeze;
	public Toggle FreeCameraToggle;
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

    AminoSliderController aminoSliderController;
    BioBlox BioBloxReference;

	public Toggle[] ToggleButtonFunctionsView;

	void Awake()
	{
		aminoSliderController = FindObjectOfType<AminoSliderController> ();
        BioBloxReference = FindObjectOfType<BioBlox>();
    }

	// Update is called once per frame
	void Update () {

		if(Input.GetKeyDown (KeyCode.F) && !ToggleFreeCameraStatus) {
			MainCamera.GetComponent<CameraMovement> ().enabled = CameraFreeze;			
			FreeCameraKeysFreeze.SetActive (CameraFreeze); 
			FreeCameraKeysUnfreeze.SetActive (!CameraFreeze);
			CameraFreeze = !CameraFreeze;
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
		MainCamera.transform.position = new Vector3 (0, 0, -75);
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

	void ChangeSliderViewToFunction()
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
		ToggleAllButtonsSliderOn (aminoSliderController.A1Hydro);
		ToggleAllButtonsSliderOn (aminoSliderController.A1Negative);
		ToggleAllButtonsSliderOn (aminoSliderController.A1Polar);
		ToggleAllButtonsSliderOn (aminoSliderController.A1Positive);
		ToggleAllButtonsSliderOn (aminoSliderController.A1Other);
		//protein 2
		ToggleAllButtonsSliderOn (aminoSliderController.A2Hydro);
		ToggleAllButtonsSliderOn (aminoSliderController.A2Polar);
		ToggleAllButtonsSliderOn (aminoSliderController.A2Positive);
		ToggleAllButtonsSliderOn (aminoSliderController.A2Negative);
		ToggleAllButtonsSliderOn (aminoSliderController.A2Other);
		//set all the togles ON
		for (int i = 0; i<10; i++)
		{
			ToggleButtonFunctionsView[i].isOn = true;
		}
		//set the color of the buttons of the connections tonormal
		ChangeAminoLinkPanelButtonColorToNormal ();
			 
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

	public void TogglePositiveA1()
	{
		foreach (GameObject AminoButton in aminoSliderController.A1Positive)
		{
			AminoButton.SetActive(ToggleButtonFunctionsView[0].isOn);
		}
	}

	public void ToggleNegativeA1()
	{
		foreach (GameObject AminoButton in aminoSliderController.A1Negative)
		{
			AminoButton.SetActive(ToggleButtonFunctionsView[1].isOn);
		}
	}

	public void TogglePolarA1()
	{
		foreach (GameObject AminoButton in aminoSliderController.A1Polar)
		{
			AminoButton.SetActive(ToggleButtonFunctionsView[3].isOn);
		}
	}

	public void ToggleOtherA1()
	{
		foreach (GameObject AminoButton in aminoSliderController.A1Other)
		{
			AminoButton.SetActive(ToggleButtonFunctionsView[4].isOn);
		}
	}

	public void ToggleHydroA1()
	{
		foreach (GameObject AminoButton in aminoSliderController.A1Hydro)
		{
			AminoButton.SetActive(ToggleButtonFunctionsView[2].isOn);
		}
	}

	//AMINO ACIDS 2

	public void TogglePositiveA2()
	{
		foreach (GameObject AminoButton in aminoSliderController.A2Positive)
		{
			AminoButton.SetActive(ToggleButtonFunctionsView[5].isOn);
		}
	}
	
	public void ToggleNegativeA2()
	{
		foreach (GameObject AminoButton in aminoSliderController.A2Negative)
		{
			AminoButton.SetActive(ToggleButtonFunctionsView[6].isOn);
		}
	}
	
	public void TogglePolarA2()
	{
		foreach (GameObject AminoButton in aminoSliderController.A2Polar)
		{
			AminoButton.SetActive(ToggleButtonFunctionsView[8].isOn);
		}
	}
	
	public void ToggleOtherA2()
	{
		foreach (GameObject AminoButton in aminoSliderController.A2Other)
		{
			AminoButton.SetActive(ToggleButtonFunctionsView[9].isOn);
		}
	}
	
	public void ToggleHydroA2()
	{
		foreach (GameObject AminoButton in aminoSliderController.A2Hydro)
		{
			AminoButton.SetActive(ToggleButtonFunctionsView[7].isOn);
		}
	}

	void ToggleAllButtonsSliderOn(List<GameObject> CurrentList)
	{
		foreach (GameObject AminoButton in CurrentList)
		{
			AminoButton.SetActive(true);
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

    public void DropDownSliderView()
    {
        switch (DropDownSlider.value)
        {
            case 0:
                ChangeSliderViewToNormal();
                break;
            case 1:
                ChangeSliderViewToFunction();
                break;
            default:
                break;
        }
    }
}
