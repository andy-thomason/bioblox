using UnityEngine;
using System.Collections;

public class FirstPersonToggle : MonoBehaviour
{
    //first view
    public CanvasGroup CanvasUI;
    public GameObject SubmarineView;
    public GameObject Bubbles;
    public Light MainLight;

    public void ToggleViewOn()
    {
        CanvasUI.alpha = 0;
        MainLight.intensity = 0.1f;
        SubmarineView.SetActive(true);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void StartBubbles()
    {
        Bubbles.SetActive(true);
    }

    public void ToggleViewOff()
    {
        CanvasUI.alpha = 1;
        MainLight.intensity = 1.0f;
        SubmarineView.SetActive(false);
        Bubbles.SetActive(false);
        Cursor.lockState = CursorLockMode.None;
    }
}
