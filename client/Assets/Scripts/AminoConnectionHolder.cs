using UnityEngine;
using AssemblyCSharp;
using UnityEngine.UI;

public class AminoConnectionHolder : MonoBehaviour {

	public AtomConnection connection;
    //AMINO ACID 1 ID
	public int ID_button1;
    //AMINO ACID 2 ID
    public int ID_button2;
	Text distancia;
    GameObject AminoPanel1;
    GameObject AminoPanel2;
    SFX sfx;
    AminoSliderController asc;
    UIController ui;
    BioBlox bb;
    ConnectionManager cn;
    OverlayRenderer or;
    public int AT1_index;
    public int A1_number_of_atoms;
    public int A1_atom_index;
    public int AT2_index;
    public int A2_number_of_atoms;
    public int A2_atom_index;
    public string A1_name;
    public string A2_name;
    public string AT1_name;
    public string AT2_name;
    Vector3 normal_scale = new Vector3(1, 1, 1);
    Vector3 selected_scale = new Vector3(1.2f, 1.2f, 1.2f);

    public void UpdateLink()
	{		
		FindObjectOfType<ConnectionManager> ().DeleteAminoAcidLink (connection);
		FindObjectOfType<AminoSliderController> ().RestoreDeletedAminoButtons (ID_button1, ID_button2);
		FindObjectOfType<ConnectionManager> ().DisableSlider ();
        //ui.p1_atom_status = UIController.p_atom_status_enum.find_atoms.GetHashCode();
        //ui.p2_atom_status = UIController.p_atom_status_enum.find_atoms.GetHashCode();
        Destroy (gameObject);
	}

    public void DeleteLink()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        cn.DeleteAminoAcidLink(connection);
        asc.RestoreDeletedAminoButtons(ID_button1, ID_button2);
        cn.DisableSlider();
        ui.P1CleanAtomButtons();
        ui.P2CleanAtomButtons();
        Destroy(transform.parent.gameObject);
    }

    public void HighlightClick()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        bb.molecules_PDB_mesh[1].SelectAminoAcid_when_connection_clicked(ID_button2,A2_atom_index);
        asc.UpdateCurrentButtonA2(ID_button2);
        bb.molecules_PDB_mesh[0].SelectAminoAcid_when_connection_clicked(ID_button1,A1_atom_index);
        asc.UpdateCurrentButtonA1(ID_button1);
        asc.HighlightAtomWhenConnectionClicked(AT1_index, 0);
        asc.HighlightAtomWhenConnectionClicked(AT2_index, 1);
        asc.HighLight3DMeshAll(ID_button1, ID_button2);
        //ui.p1_atom_status = UIController.p_atom_status_enum.find_atoms.GetHashCode();
        //ui.p2_atom_status = UIController.p_atom_status_enum.find_atoms.GetHashCode();
    }

    void HightlighClickForNextAtom()
    {
        bb.molecules_PDB_mesh[1].SelectAminoAcid(ID_button2);
        asc.UpdateCurrentButtonA2(ID_button2);
        bb.molecules_PDB_mesh[0].SelectAminoAcid(ID_button1);
        asc.UpdateCurrentButtonA1(ID_button1);
        asc.HighLight3DMeshAll(ID_button1, ID_button2);
    }

    public void HighlightWhenMovingThroughAtomsA1()
    {
        bb.molecules_PDB_mesh[0].SelectAminoAcid(ID_button1);
        asc.UpdateCurrentButtonA1(ID_button1);
    }

    public void HighlightWhenMovingThroughAtomsA2()
    {
        bb.molecules_PDB_mesh[1].SelectAminoAcid(ID_button2);
        asc.UpdateCurrentButtonA2(ID_button2);
    }

    void Awake()
	{
		distancia = GetComponentInChildren<Text> ();
        sfx = FindObjectOfType<SFX>();
        ui = FindObjectOfType<UIController>();
        AminoPanel1 = GameObject.Find("ContentPanelA1").gameObject;
        AminoPanel2 = GameObject.Find("ContentPanelA2").gameObject;
        asc = FindObjectOfType<AminoSliderController>();
        bb = FindObjectOfType<BioBlox>();
        cn = FindObjectOfType<ConnectionManager>();
        or = FindObjectOfType<OverlayRenderer>();

        if (ui.expert_mode)
        {
            transform.GetChild(2).gameObject.SetActive(true);
            transform.GetChild(3).gameObject.SetActive(true);
            transform.GetChild(4).gameObject.SetActive(true);
        }
    }

	void Update()
	{
        if (distancia == null) return;
		distancia.text = (connection.distance).ToString ("F1");
	}

    //SLIDER amino BUTTONS

    public void A1L()
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        ModifyConnectionMINUS(-1, 0);
    }
    public void A1R()
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        ModifyConnectionPLUS(1, 0);
    }
    public void A2L()
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        ModifyConnectionMINUS(0, -1);
    }
    public void A2R()
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        ModifyConnectionPLUS(0, 1);
    }

    public void ModifyConnectionMINUS(int A1, int A2)
    {
        //A1
        if (A1 == -1)
        {
            if ((ID_button1 - 1) >= 0)
            {
                asc.ModifyConnectionHolder(cn.CreateAminoAcidLink(bb.molecules_PDB_mesh[0], ID_button1 + A1, bb.molecules_PDB_mesh[1], ID_button2 + A2), AminoPanel1.transform.GetChild(ID_button1 + A1).gameObject, AminoPanel2.transform.GetChild(ID_button2 + A2).gameObject, transform.parent);
                cn.SliderStrings.interactable = true;
                UpdateLink();
            }
        }
        else
        {
            if ((ID_button2 - 1) >= 0)
            {
                asc.ModifyConnectionHolder(cn.CreateAminoAcidLink(bb.molecules_PDB_mesh[0], ID_button1 + A1, bb.molecules_PDB_mesh[1], ID_button2 + A2), AminoPanel1.transform.GetChild(ID_button1 + A1).gameObject, AminoPanel2.transform.GetChild(ID_button2 + A2).gameObject, transform.parent);
                cn.SliderStrings.interactable = true;
                UpdateLink();
            }
        }
       
    }

    public void ModifyConnectionPLUS(int A1, int A2)
    {
        //A1
        if(A1 == 1)
        {
            if (AminoPanel1.transform.childCount > (ID_button1 + 1))
            {
                asc.ModifyConnectionHolder(cn.CreateAminoAcidLink(bb.molecules_PDB_mesh[0], ID_button1 + A1, bb.molecules_PDB_mesh[1], ID_button2 + A2), AminoPanel1.transform.GetChild(ID_button1 + A1).gameObject, AminoPanel2.transform.GetChild(ID_button2 + A2).gameObject, transform.parent);
                cn.SliderStrings.interactable = true;
                UpdateLink();
            }
        }
        else
        {
            if (AminoPanel2.transform.childCount > (ID_button2 + 1))
            {
                asc.ModifyConnectionHolder(cn.CreateAminoAcidLink(bb.molecules_PDB_mesh[0], ID_button1 + A1, bb.molecules_PDB_mesh[1], ID_button2 + A2), AminoPanel1.transform.GetChild(ID_button1 + A1).gameObject, AminoPanel2.transform.GetChild(ID_button2 + A2).gameObject, transform.parent);
                cn.SliderStrings.interactable = true;
                UpdateLink();
            }
        }
    }

    //SLIDER atom BUTTONS
    bool atom_exist;
    public void AT1L()
    {
        if(AT1_index > 0)
        {
            sfx.PlayTrack(SFX.sound_index.amino_click);
            //if false, no atoms are displayed
            atom_exist = ui.p1_atom_holder.childCount != 0 ? true : false;

            if (ui.p1_atom_holder.childCount != 0)
            {
                ui.p1_atom_holder.GetChild(AT1_index).localScale = normal_scale;
                ui.p1_atom_holder.GetChild(AT1_index - 1).localScale = selected_scale;
            }
            
            AT1_index--;

            //update the eatom highligght
            A1_atom_index--;
            or.P1_selected_atom_id = A1_atom_index;

            if (!atom_exist)
            {
                HightlighClickForNextAtom();
                asc.HighlightAtomWhenConnectionClicked(AT2_index, 1);
            }
            else
            {
                if (asc.CurrentButtonA1 != ID_button1 || asc.CurrentButtonA2 != ID_button2)
                {
                    HighlightWhenMovingThroughAtomsA1();
                    HighlightWhenMovingThroughAtomsA2();
                    atom_exist = false;
                    asc.HighlightAtomWhenConnectionClicked(AT2_index, 1);
                }
            }

            cn.DeleteAminoAcidLink(connection);
            Destroy(transform.GetChild(2).transform.GetChild(0).gameObject);

            

            //if (!atom_exist)
            //    asc.ModifyConnectionHolder_atomic_when_no_selection(cn.CreateAminoAcidLink_atom_modification(bb.molecules_PDB_mesh[0], ID_button1, AT1_index, bb.molecules_PDB_mesh[1], ID_button2, AT2_index), AT1_index, AT1_index + 1, AT2_index, AT2_index, 0,gameObject);
            //else
                asc.ModifyConnectionHolder_atomic(cn.CreateAminoAcidLink_atom_modification(bb.molecules_PDB_mesh[0], ID_button1, AT1_index, bb.molecules_PDB_mesh[1], ID_button2, AT2_index), AT1_index, AT1_index + 1, AT2_index, AT2_index, 0, gameObject, atom_exist);
        }
    }
    public void AT1R()
    {
        if (AT1_index < A1_number_of_atoms-1)
        {
            sfx.PlayTrack(SFX.sound_index.amino_click);
            //if false, no atoms are displayed
            atom_exist = ui.p1_atom_holder.childCount != 0 ? true : false;

            if(ui.p1_atom_holder.childCount != 0)
            {
                ui.p1_atom_holder.GetChild(AT1_index).localScale = normal_scale;
                ui.p1_atom_holder.GetChild(AT1_index + 1).localScale = selected_scale;
            }

            AT1_index++;

            //update the eatom highligght
            A1_atom_index++;
            or.P1_selected_atom_id = A1_atom_index;

            if (!atom_exist)
            {
                HightlighClickForNextAtom();
                asc.HighlightAtomWhenConnectionClicked(AT2_index, 1);
            }
            else
            {
                if (asc.CurrentButtonA1 != ID_button1 || asc.CurrentButtonA2 != ID_button2)
                {
                    HighlightWhenMovingThroughAtomsA1();
                    HighlightWhenMovingThroughAtomsA2();
                    atom_exist = false;
                    asc.HighlightAtomWhenConnectionClicked(AT2_index, 1);
                }
            }

            cn.DeleteAminoAcidLink(connection);
            Destroy(transform.GetChild(2).transform.GetChild(0).gameObject);

           
            //if (!atom_exist)
            //    asc.ModifyConnectionHolder_atomic_when_no_selection(cn.CreateAminoAcidLink_atom_modification(bb.molecules_PDB_mesh[0], ID_button1, AT1_index, bb.molecules_PDB_mesh[1], ID_button2, AT2_index), AT1_index, AT1_index - 1, AT2_index, AT2_index, 0, gameObject);
            //else
                asc.ModifyConnectionHolder_atomic(cn.CreateAminoAcidLink_atom_modification(bb.molecules_PDB_mesh[0], ID_button1, AT1_index, bb.molecules_PDB_mesh[1], ID_button2, AT2_index), AT1_index, AT1_index - 1, AT2_index, AT2_index, 0, gameObject, atom_exist);
        }
    }
    public void AT2L()
    {
        if (AT2_index > 0)
        {

            sfx.PlayTrack(SFX.sound_index.amino_click);
            //if false, no atoms are displayed
            atom_exist = ui.p2_atom_holder.childCount != 0 ? true : false;

            if (ui.p2_atom_holder.childCount != 0)
            {
                ui.p2_atom_holder.GetChild(AT2_index).localScale = normal_scale;
                ui.p2_atom_holder.GetChild(AT2_index - 1).localScale = selected_scale;
            }

            AT2_index--;

            //update the eatom highligght
            A2_atom_index--;
            or.P2_selected_atom_id = A2_atom_index;

            if (!atom_exist)
            {
                HightlighClickForNextAtom();
                asc.HighlightAtomWhenConnectionClicked(AT1_index, 0);
            }
            else
            {
                if(asc.CurrentButtonA1 != ID_button1 || asc.CurrentButtonA2 != ID_button2)
                {
                    HighlightWhenMovingThroughAtomsA1();
                    HighlightWhenMovingThroughAtomsA2();
                    atom_exist = false;
                    asc.HighlightAtomWhenConnectionClicked(AT1_index, 0);
                }
                
            }

            cn.DeleteAminoAcidLink(connection);
            Destroy(transform.GetChild(3).transform.GetChild(0).gameObject);

            //if (!atom_exist)
            //    asc.ModifyConnectionHolder_atomic_when_no_selection(cn.CreateAminoAcidLink_atom_modification(bb.molecules_PDB_mesh[0], ID_button1, AT1_index, bb.molecules_PDB_mesh[1], ID_button2, AT2_index), AT1_index, AT1_index, AT2_index, AT2_index + 1, 1, gameObject);
            //else
                asc.ModifyConnectionHolder_atomic(cn.CreateAminoAcidLink_atom_modification(bb.molecules_PDB_mesh[0], ID_button1, AT1_index, bb.molecules_PDB_mesh[1], ID_button2, AT2_index), AT1_index, AT1_index, AT2_index, AT2_index + 1, 1, gameObject, atom_exist);
        }
    }
    public void AT2R()
    {
        if (AT2_index < A2_number_of_atoms-1)
        {

            sfx.PlayTrack(SFX.sound_index.amino_click);
            //if false, no atoms are displayed
            atom_exist = ui.p2_atom_holder.childCount != 0 ? true : false;

            if (ui.p2_atom_holder.childCount != 0)
            {
                ui.p2_atom_holder.GetChild(AT2_index).localScale = normal_scale;
                ui.p2_atom_holder.GetChild(AT2_index + 1).localScale = selected_scale;
            }
            
            AT2_index++;

            //update the eatom highligght
            A2_atom_index++;
            or.P2_selected_atom_id = A2_atom_index;

            if (!atom_exist)
            {
                HightlighClickForNextAtom();
                asc.HighlightAtomWhenConnectionClicked(AT1_index, 0);
            }
            else
            {
                if (asc.CurrentButtonA1 != ID_button1 || asc.CurrentButtonA2 != ID_button2)
                {
                    HighlightWhenMovingThroughAtomsA1();
                    HighlightWhenMovingThroughAtomsA2();
                    atom_exist = false;
                    asc.HighlightAtomWhenConnectionClicked(AT1_index, 0);
                }
            }

            cn.DeleteAminoAcidLink(connection);
            Destroy(transform.GetChild(3).transform.GetChild(0).gameObject);

            //if (!atom_exist)
            //    asc.ModifyConnectionHolder_atomic_when_no_selection(cn.CreateAminoAcidLink_atom_modification(bb.molecules_PDB_mesh[0], ID_button1, AT1_index, bb.molecules_PDB_mesh[1], ID_button2, AT2_index), AT1_index, AT1_index, AT2_index, AT2_index - 1, 1, gameObject);
            //else
                asc.ModifyConnectionHolder_atomic(cn.CreateAminoAcidLink_atom_modification(bb.molecules_PDB_mesh[0], ID_button1, AT1_index, bb.molecules_PDB_mesh[1], ID_button2, AT2_index), AT1_index, AT1_index, AT2_index, AT2_index - 1, 1, gameObject, atom_exist);
        }
    }

}
