using UnityEngine;
using System.Collections;
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

	AminoSliderController aminoSliderController;

	//togls for the types 1
	public Toggle A1P;
	public Toggle A1N;
	public Toggle A1Po;
	public Toggle A1O;
	public Toggle A1H;
	//togls for the types 2
	public Toggle A2P;
	public Toggle A2N;
	public Toggle A2Po;
	public Toggle A2O;
	public Toggle A2H;

	void Start()
	{
		aminoSliderController = FindObjectOfType<AminoSliderController> ();
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

	public void ChangeSliderViewToCharged()
	{
		foreach (Transform AminoButton in SliderProtein1.transform)
		{
			AminoButton.GetComponent<Image>().color = AminoButton.GetComponent<AminoButtonController>().ChargedColor;
		}
		foreach (Transform AminoButton in SliderProtein2.transform)
		{
			AminoButton.GetComponent<Image>().color = AminoButton.GetComponent<AminoButtonController>().ChargedColor;
		}
		FunctionInfoObject.SetActive (true);
	}

	public void ChangeSliderViewToNormal()
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
			AminoButton.SetActive(A1P.isOn);
		}
	}

	public void ToggleNegativeA1()
	{
		foreach (GameObject AminoButton in aminoSliderController.A1Negative)
		{
			AminoButton.SetActive(A1N.isOn);
		}
	}

	public void TogglePolarA1()
	{
		foreach (GameObject AminoButton in aminoSliderController.A1Polar)
		{
			AminoButton.SetActive(A1Po.isOn);
		}
	}

	public void ToggleOtherA1()
	{
		foreach (GameObject AminoButton in aminoSliderController.A1Other)
		{
			AminoButton.SetActive(A1O.isOn);
		}
	}

	public void ToggleHydroA1()
	{
		foreach (GameObject AminoButton in aminoSliderController.A1Hydro)
		{
			AminoButton.SetActive(A1H.isOn);
		}
	}

	//AMINO ACIDS 2

	public void TogglePositiveA2()
	{
		foreach (GameObject AminoButton in aminoSliderController.A2Positive)
		{
			AminoButton.SetActive(A2P.isOn);
		}
	}
	
	public void ToggleNegativeA2()
	{
		foreach (GameObject AminoButton in aminoSliderController.A2Negative)
		{
			AminoButton.SetActive(A2N.isOn);
		}
	}
	
	public void TogglePolarA2()
	{
		foreach (GameObject AminoButton in aminoSliderController.A2Polar)
		{
			AminoButton.SetActive(A2Po.isOn);
		}
	}
	
	public void ToggleOtherA2()
	{
		foreach (GameObject AminoButton in aminoSliderController.A2Other)
		{
			AminoButton.SetActive(A2O.isOn);
		}
	}
	
	public void ToggleHydroA2()
	{
		foreach (GameObject AminoButton in aminoSliderController.A2Hydro)
		{
			AminoButton.SetActive(A2H.isOn);
		}
	}
}
