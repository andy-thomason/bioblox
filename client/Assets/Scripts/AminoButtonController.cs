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

    //infi poanels
    public GameObject AminoInfoPanel_simple;
    public GameObject AminoInfoPanel_multi;
    public GameObject AminoInfoPanel_element;

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
        transform.GetChild(2).GetComponent<Image>().color = Color.black;
        transform.GetChild(3).GetComponent<Text>().color = Color.black;
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
        aminoSli.DeleteCurrentAminoInfoPanel();

        //exist helpo dock - multi panel
        if(dock_amino_id.Count != 0)
        {
            //no help, just info of the button
            GameObject AminoInfoPanel_temp = Instantiate(AminoInfoPanel_multi);
            //asign the current for reference
            aminoSli.AminoInfoPanelCurrentOpen = AminoInfoPanel_temp;
            AminoInfoPanel_temp.transform.SetParent(ui.MainCanvas.transform, false);
            
            AminoInfoPanel_temp.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = aminoSli.AminoFullNames[name_amino];
            AminoInfoPanel_temp.transform.GetChild(0).transform.GetChild(1).GetComponent<Text>().text = name_amino + " " + tag_amino;
            AminoInfoPanel_temp.transform.GetChild(0).transform.GetChild(2).GetComponent<Image>().color = aminoSli.buttonStructure.NormalColor[name_amino];

            for (int i = 0; i < dock_amino_id.Count; i++)
            {
                Debug.Log("AminoID: " + dock_amino_id[i] + " AtomID: " + dock_atom_id[i] + " Amino name: " + dock_amino_name_tag_atom[i]);
                //no help, just info of the button
                GameObject AminoInfoPanel_element_temp = Instantiate(AminoInfoPanel_element);
                AminoInfoPanel_element_temp.transform.SetParent(AminoInfoPanel_temp.transform.GetChild(0).transform.GetChild(3).transform.GetChild(0).transform, false);

                AminoInfoPanel_element_temp.transform.GetChild(0).GetComponent<Text>().text = dock_amino_name_tag_atom[i];
            }
            
            AminoInfoPanel_temp.transform.position = Input.mousePosition;
        }
        else
        {
            //no help, just info of the button
            GameObject AminoInfoPanel_temp = Instantiate(AminoInfoPanel_simple);
            //asign the current for reference
            aminoSli.AminoInfoPanelCurrentOpen = AminoInfoPanel_temp;
            AminoInfoPanel_temp.transform.SetParent(ui.MainCanvas.transform, false);

            AminoInfoPanel_temp.transform.GetChild(0).transform.GetChild(0).GetComponent<Text>().text = aminoSli.AminoFullNames[name_amino];
            AminoInfoPanel_temp.transform.GetChild(0).transform.GetChild(1).GetComponent<Text>().text = name_amino +" "+ tag_amino;
            AminoInfoPanel_temp.transform.GetChild(0).transform.GetChild(2).GetComponent<Image>().color = aminoSli.buttonStructure.NormalColor[name_amino];

            AminoInfoPanel_temp.transform.position = Input.mousePosition;
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
            aminoSli.current_atom_child_id0 = aminoSli.current_atom_child_id1 = 0;

            if (protein_id == 0)
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
            
            //get the values
            PDB_molecule P_mol = bb.molecules_PDB_mesh[protein_id].mol;
            int[] A_atoms = P_mol.aminoAcidsAtomIds[AminoButtonID];

            //go throuygh atoms and enable
            for (int i = 0; i < A_atoms.Length; i++)
            {
                temp_panel.transform.GetChild(0).transform.GetChild(i).GetComponentInChildren<Text>().text = P_mol.atomNames[A_atoms[i]];
                temp_panel.transform.GetChild(0).transform.GetChild(i).GetComponent<CanvasGroup>().alpha = 1;
                temp_panel.transform.GetChild(0).transform.GetChild(i).GetComponent<CanvasGroup>().interactable = true;
                temp_panel.transform.GetChild(0).transform.GetChild(i).GetComponent<AtomOnAminoController>().protein_id = protein_id;
                temp_panel.transform.GetChild(0).transform.GetChild(i).GetComponent<AtomOnAminoController>().atom_id = A_atoms[i];
                temp_panel.transform.GetChild(0).transform.GetChild(i).GetComponent<AtomOnAminoController>().amino_child_index = AminoButtonID;
            }
            temp_panel.transform.GetChild(0).transform.GetChild(0).GetComponent<Image>().color = FindObjectOfType<UIController>().GridToggleColor_pressed;

        }
        else
        {
            //transform.GetChild(2).GetComponent<Image>().sprite = OpenAtomPanel;
            //Destroy(transform.parent.GetChild(transform.GetSiblingIndex() + 1).gameObject);

            if (protein_id == 0)
            {
                ui.EraseAminoButton_Atom_reference_0();
                ui.AminoButton_Atom_reference_0 = null;
                ui.AminoButton_reference_0 = null;
            }
            else
            {
                ui.EraseAminoButton_Atom_reference_1();
                ui.AminoButton_Atom_reference_1 = null;
                ui.AminoButton_reference_1 = null;
            }
        }

        is_AminoButton_Atom_open = !is_AminoButton_Atom_open;
        HighLightOnClick();
    }

    //public void HighlightCurrentAtom(int current_child_index)
    //{
    //    if(current_atom_child_id != -1)
    //    {
    //        transform.parent.GetChild(current_atom_child_id).GetComponent<Image>().color = ui.GridToggleColor_normal;
    //        transform.parent.GetChild(current_child_index).GetComponent<Image>().color = ui.normal_button_color;
    //        current_atom_child_id = current_child_index;
    //    }
    //}
}
