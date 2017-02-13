using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    int selected_index = 0;
    public GameObject AminoButton_Atom;
    public Sprite OpenAtomPanel;
    public Sprite CloseAtomPanel;
    bool is_AminoButton_Atom_open = false;

    public List<int> dock_amino_id;
    public List<int> dock_atom_id;
    public List<string> dock_amino_name_tag_atom;

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
            DisplayAminoInfo();
            //DisableAmino();

        sfx.PlayTrack(SFX.sound_index.amino_click);
    }


    public void HighLight()
    {
        if (protein_id == 1)
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
        if (protein_id == 1)
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
        //highlight color
        transform.GetChild(selected_index).gameObject.SetActive(true);
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
        transform.GetChild(4).gameObject.SetActive(is_disabled);

        for (int i = 0; i < amino_id.Length; i++)
            bb.atoms_disabled[protein_id][amino_id[i]] = is_disabled;

        HighLightOnClick();
    }

    public void DisableAminoReset()
    {
        is_disabled = !is_disabled;
        transform.GetChild(4).gameObject.SetActive(is_disabled);

        for (int i = 0; i < amino_id.Length; i++)
            bb.atoms_disabled[protein_id][amino_id[i]] = is_disabled;
    }

    public void DisplayAminoInfo()
    {
        if(dock_amino_id.Count != 0)
        {
            for(int i = 0; i < dock_amino_id.Count; i++)
            {
                Debug.Log("AminoID: " + dock_amino_id[i] + " AtomID: " + dock_atom_id[i] + " Amino name: " + dock_amino_name_tag_atom[i]);
            }
        }
    }

    public void SpawnAminoButton_Atom()
    {
        if(!is_AminoButton_Atom_open)
        {
            transform.GetChild(2).GetComponent<Image>().sprite = CloseAtomPanel;
            GameObject temp_panel = Instantiate(AminoButton_Atom);
            temp_panel.transform.SetParent(transform.parent, false);
            temp_panel.transform.SetSiblingIndex(transform.GetSiblingIndex() + 1);
            if(protein_id == 0)
            {
                ui.EraseAminoButton_Atom_reference_0();
                ui.AminoButton_Atom_reference_0 = temp_panel;
                ui.AminoButton_reference_0 = gameObject;
            }
            else
            {
                ui.EraseAminoButton_Atom_reference_1();
                ui.AminoButton_Atom_reference_1 = temp_panel;
                ui.AminoButton_reference_1 = gameObject;
            }
        }
        else
        {
            transform.GetChild(2).GetComponent<Image>().sprite = OpenAtomPanel;
            Destroy(transform.parent.GetChild(transform.GetSiblingIndex() + 1).gameObject);
        }

        is_AminoButton_Atom_open = !is_AminoButton_Atom_open;
        HighLightOnClick();
    }
}
