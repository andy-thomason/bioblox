using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AminoButtonController : MonoBehaviour {

	public int AminoButtonID;

	public void HighLight()
	{
		if (transform.parent.name == "ContentPanelA2")
		{
			FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>().SelectAminoAcid(AminoButtonID);
		}
		else
		{			
			FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>().SelectAminoAcid(AminoButtonID);
		}
		FindObjectOfType<AminoSliderController> ().AminoAcidsSelection (gameObject);
	}

	public void HighLightOnClick()
	{
		if (transform.parent.name == "ContentPanelA2")
		{
			FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>().SelectAminoAcid(AminoButtonID);
			FindObjectOfType<AminoSliderController>().UpdateCurrentButtonA2(AminoButtonID);
		}
		else
		{			
			FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>().SelectAminoAcid(AminoButtonID);
			FindObjectOfType<AminoSliderController>().UpdateCurrentButtonA1(AminoButtonID);
		}
		FindObjectOfType<AminoSliderController> ().AminoAcidsSelection (gameObject);
	}
}
