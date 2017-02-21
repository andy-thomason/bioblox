using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfoPanelController : MonoBehaviour {

    public GameObject AminoObject;
    
    public void DisableAminoToggleMulti()
    {
        AminoObject.GetComponent<AminoButtonController>().DisableAmino();
    }

    public void DisableAminoToggleSingle()
    {
        AminoObject.GetComponent<AminoButtonController>().DisableAmino();
    }
}
