using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AminoButtonController : MonoBehaviour, IPointerClickHandler {

    public int protein_id;
    public bool is_disabled;
	public int AminoButtonID;
    public int[] amino_id;
	public bool Linked = false;
    public string name_amino;
    public string tag_amino;
    public Color32 NormalColor;
	public Color32 FunctionColor;
    public int temp_AminoButtonID;
    BioBlox bb;
    SFX sfx;
    AminoSliderController aminoSli;
    UIController ui;

    void Start()
    {
        bb = FindObjectOfType<BioBlox>();
        sfx = FindObjectOfType<SFX>();
        aminoSli = FindObjectOfType<AminoSliderController>();
        ui = FindObjectOfType<UIController>();
        is_disabled = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
            DisableAmino();

        sfx.PlayTrack(SFX.sound_index.amino_click);
    }


    public void HighLight()
    {
        if (transform.parent.name == "ContentPanelA2")
		{
            bb.molecules_PDB_mesh[1].SelectAminoAcid(AminoButtonID);
            //ui.p2_atom_status = UIController.p_atom_status_enum.find_atoms.GetHashCode();
        }
		else
		{
            bb.molecules_PDB_mesh[0].SelectAminoAcid(AminoButtonID);
            //ui.p1_atom_status = UIController.p_atom_status_enum.find_atoms.GetHashCode();
        }
        aminoSli.ChangeAminoAcidSelection (gameObject);
	}

	public void HighLightOnClick()
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        if (transform.parent.name == "ContentPanelA2")
		{
			bb.molecules_PDB_mesh[1].SelectAminoAcid(AminoButtonID);
            aminoSli.UpdateCurrentButtonA2(AminoButtonID);
            //ui.p2_atom_status = UIController.p_atom_status_enum.find_atoms.GetHashCode();
		}
		else
		{
            bb.molecules_PDB_mesh[0].SelectAminoAcid(AminoButtonID);
            aminoSli.UpdateCurrentButtonA1(AminoButtonID);
            //ui.p1_atom_status = UIController.p_atom_status_enum.find_atoms.GetHashCode();
        }
        aminoSli.ChangeAminoAcidSelection (gameObject);
    }

    //was calling twice the aminoacidssleection function
    public void SetGameObject()
    {
        aminoSli.ChangeAminoAcidSelection(gameObject);
    }

    public void DisableAmino()
    {
        is_disabled = !is_disabled;
        transform.GetChild(3).gameObject.SetActive(is_disabled);

        for (int i = 0; i < amino_id.Length; i++)
            bb.atoms_disabled[protein_id][amino_id[i]] = is_disabled;

        HighLightOnClick();
    }

    public void DisableAminoReset()
    {
        is_disabled = !is_disabled;
        transform.GetChild(3).gameObject.SetActive(is_disabled);

        for (int i = 0; i < amino_id.Length; i++)
            bb.atoms_disabled[protein_id][amino_id[i]] = is_disabled;
    }
}
