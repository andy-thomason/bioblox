using UnityEngine;
using System.Collections;

public class FadePanel : MonoBehaviour {

    bool FadeStatus = false;

	public void FadeCycle()
    {
        gameObject.GetComponent<Animator>().SetBool("Fade", FadeStatus);
        FadeStatus = !FadeStatus;
    }
}
