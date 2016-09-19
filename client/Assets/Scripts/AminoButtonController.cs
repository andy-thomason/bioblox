using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AminoButtonController : MonoBehaviour {

	public int AminoButtonID;
	public bool Linked = false;
    public string name_amino;
	public Color32 NormalColor;
	public Color32 FunctionColor;
    public int temp_AminoButtonID;
    BioBlox bb;
    SFX sfx;
    AminoSliderController aminoSli;

    void Start()
    {
        bb = FindObjectOfType<BioBlox>();
        sfx = FindObjectOfType<SFX>();
        aminoSli = FindObjectOfType<AminoSliderController>();
    }


	public void HighLight()
	{
        sfx.PlayTrack(SFX.sound_index.amino_click);

        if (transform.parent.name == "ContentPanelA2")
		{
            bb.molecules[1].GetComponent<PDB_mesh>().SelectAminoAcid(AminoButtonID);
		}
		else
		{
            bb.molecules[0].GetComponent<PDB_mesh>().SelectAminoAcid(AminoButtonID);
		}
        aminoSli.ChangeAminoAcidSelection (gameObject);
	}

	public void HighLightOnClick()
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        if (transform.parent.name == "ContentPanelA2")
		{
			bb.molecules[1].GetComponent<PDB_mesh>().SelectAminoAcid(AminoButtonID);
            aminoSli.UpdateCurrentButtonA2(AminoButtonID);
		}
		else
		{			
			bb.molecules[0].GetComponent<PDB_mesh>().SelectAminoAcid(AminoButtonID);
            aminoSli.UpdateCurrentButtonA1(AminoButtonID);
		}
        aminoSli.ChangeAminoAcidSelection (gameObject);
    }
}
