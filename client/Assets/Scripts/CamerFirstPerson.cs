using UnityEngine;
using System.Collections;

public class CamerFirstPerson : MonoBehaviour {

	public GameObject CameraFrame;
    public GameObject MainCanvas;
    public GameObject FirstPersonText;
    UIController uiController;

    void Start()
    {
        uiController = FindObjectOfType<UIController>();
    }

	public void DeleteCamera()
    {
        //only 1 active
        GameObject check_new = GameObject.FindGameObjectWithTag("FirstPerson");
        if (check_new) Destroy(check_new);
        //ship
        GameObject temp = GameObject.FindGameObjectWithTag("space_ship");
        if (temp) Destroy(temp);

        if (!uiController.explore_view)
        {
            MainCanvas.SetActive(true);
            uiController.ExplorerBackToDefault();
        }

    }
	
	public void CameraFrameOff()
	{
		CameraFrame.SetActive(false);
        //FirstPersonText.SetActive(false);
    }
	
	public void CameraFrameOn()
	{
		CameraFrame.SetActive(true);
        //FirstPersonText.SetActive(true);
    }
}
