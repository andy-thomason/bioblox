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

	void Start()
	{

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


}
