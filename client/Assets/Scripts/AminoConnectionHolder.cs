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
    UIController ui;
    public int AT1_index;
    public int A1_number_of_atoms;
    public int AT2_index;
    public int A2_number_of_atoms;

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
        FindObjectOfType<ConnectionManager>().DeleteAminoAcidLink(connection);
        FindObjectOfType<AminoSliderController>().RestoreDeletedAminoButtons(ID_button1, ID_button2);
        FindObjectOfType<ConnectionManager>().DisableSlider();
        Destroy(transform.parent.gameObject);
    }

    public void HighlightClick()
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>().SelectAminoAcid(ID_button2);
        FindObjectOfType<AminoSliderController>().UpdateCurrentButtonA2(ID_button2);			
		FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>().SelectAminoAcid(ID_button1);
        FindObjectOfType<AminoSliderController>().UpdateCurrentButtonA1(ID_button1);
        FindObjectOfType<AminoSliderController>().HighLight3DMeshAll(ID_button1, ID_button2);
        //ui.p1_atom_status = UIController.p_atom_status_enum.find_atoms.GetHashCode();
        //ui.p2_atom_status = UIController.p_atom_status_enum.find_atoms.GetHashCode();
    }

    void Awake()
	{
		distancia = GetComponentInChildren<Text> ();
        sfx = FindObjectOfType<SFX>();
        ui = FindObjectOfType<UIController>();
        AminoPanel1 = GameObject.Find("ContentPanelA1").gameObject;
        AminoPanel2 = GameObject.Find("ContentPanelA2").gameObject;

        if(ui.expert_mode)
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
                AminoPanel2.transform.GetChild(ID_button2).GetComponent<Animator>().SetBool("High", false);
                AminoPanel1.transform.GetChild(ID_button1).GetComponent<Animator>().SetBool("High", false);
                FindObjectOfType<AminoSliderController>().ModifyConnectionHolder(FindObjectOfType<ConnectionManager>().CreateAminoAcidLink(FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>(), ID_button1 + A1, FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>(), ID_button2 + A2), AminoPanel1.transform.GetChild(ID_button1 + A1).gameObject, AminoPanel2.transform.GetChild(ID_button2 + A2).gameObject, transform.parent);
                FindObjectOfType<ConnectionManager>().SliderStrings.interactable = true;
                AminoPanel2.transform.GetChild(ID_button2 + A2).GetComponent<Animator>().SetBool("High", true);
                AminoPanel1.transform.GetChild(ID_button1 + A1).GetComponent<Animator>().SetBool("High", true);
                UpdateLink();
            }
        }
        else
        {
            if ((ID_button2 - 1) >= 0)
            {
                AminoPanel2.transform.GetChild(ID_button2).GetComponent<Animator>().SetBool("High", false);
                AminoPanel1.transform.GetChild(ID_button1).GetComponent<Animator>().SetBool("High", false);
                FindObjectOfType<AminoSliderController>().ModifyConnectionHolder(FindObjectOfType<ConnectionManager>().CreateAminoAcidLink(FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>(), ID_button1 + A1, FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>(), ID_button2 + A2), AminoPanel1.transform.GetChild(ID_button1 + A1).gameObject, AminoPanel2.transform.GetChild(ID_button2 + A2).gameObject, transform.parent);
                FindObjectOfType<ConnectionManager>().SliderStrings.interactable = true;
                AminoPanel2.transform.GetChild(ID_button2 + A2).GetComponent<Animator>().SetBool("High", true);
                AminoPanel1.transform.GetChild(ID_button1 + A1).GetComponent<Animator>().SetBool("High", true);
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
                AminoPanel2.transform.GetChild(ID_button2).GetComponent<Animator>().SetBool("High", false);
                AminoPanel1.transform.GetChild(ID_button1).GetComponent<Animator>().SetBool("High", false);
                FindObjectOfType<AminoSliderController>().ModifyConnectionHolder(FindObjectOfType<ConnectionManager>().CreateAminoAcidLink(FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>(), ID_button1 + A1, FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>(), ID_button2 + A2), AminoPanel1.transform.GetChild(ID_button1 + A1).gameObject, AminoPanel2.transform.GetChild(ID_button2 + A2).gameObject, transform.parent);
                FindObjectOfType<ConnectionManager>().SliderStrings.interactable = true;
                AminoPanel2.transform.GetChild(ID_button2 + A2).GetComponent<Animator>().SetBool("High", true);
                AminoPanel1.transform.GetChild(ID_button1 + A1).GetComponent<Animator>().SetBool("High", true);
                UpdateLink();
            }
        }
        else
        {
            if (AminoPanel2.transform.childCount > (ID_button2 + 1))
            {
                AminoPanel2.transform.GetChild(ID_button2).GetComponent<Animator>().SetBool("High", false);
                AminoPanel1.transform.GetChild(ID_button1).GetComponent<Animator>().SetBool("High", false);
                FindObjectOfType<AminoSliderController>().ModifyConnectionHolder(FindObjectOfType<ConnectionManager>().CreateAminoAcidLink(FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>(), ID_button1 + A1, FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>(), ID_button2 + A2), AminoPanel1.transform.GetChild(ID_button1 + A1).gameObject, AminoPanel2.transform.GetChild(ID_button2 + A2).gameObject, transform.parent);
                FindObjectOfType<ConnectionManager>().SliderStrings.interactable = true;
                AminoPanel2.transform.GetChild(ID_button2 + A2).GetComponent<Animator>().SetBool("High", true);
                AminoPanel1.transform.GetChild(ID_button1 + A1).GetComponent<Animator>().SetBool("High", true);
                UpdateLink();
            }
        }
    }

    //SLIDER atom BUTTONS

    public void AT1L()
    {
        if(AT1_index > 0)
        {
            sfx.PlayTrack(SFX.sound_index.amino_click);
            AT1_index--;
            FindObjectOfType<AminoSliderController>().ModifyConnectionHolder_atomic(FindObjectOfType<ConnectionManager>().CreateAminoAcidLink_atom_modification(FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>(), ID_button1, AT1_index, FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>(), ID_button2, AT2_index), AminoPanel1.transform.GetChild(ID_button1).gameObject, AminoPanel2.transform.GetChild(ID_button2).gameObject, transform.parent, AT1_index, AT2_index,0);
            UpdateLink();
        }
    }
    public void AT1R()
    {
        if (AT1_index < A1_number_of_atoms-1)
        {
            sfx.PlayTrack(SFX.sound_index.amino_click);
            AT1_index++;
            FindObjectOfType<AminoSliderController>().ModifyConnectionHolder_atomic(FindObjectOfType<ConnectionManager>().CreateAminoAcidLink_atom_modification(FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>(), ID_button1, AT1_index, FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>(), ID_button2, AT2_index), AminoPanel1.transform.GetChild(ID_button1).gameObject, AminoPanel2.transform.GetChild(ID_button2).gameObject, transform.parent, AT1_index, AT2_index,0);
            UpdateLink();
        }
    }
    public void AT2L()
    {
        if (AT2_index > 0)
        {
            sfx.PlayTrack(SFX.sound_index.amino_click);
            AT2_index--;
            FindObjectOfType<AminoSliderController>().ModifyConnectionHolder_atomic(FindObjectOfType<ConnectionManager>().CreateAminoAcidLink_atom_modification(FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>(), ID_button1, AT1_index, FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>(), ID_button2, AT2_index), AminoPanel1.transform.GetChild(ID_button1).gameObject, AminoPanel2.transform.GetChild(ID_button2).gameObject, transform.parent, AT1_index, AT2_index,1);
            UpdateLink();
        }
    }
    public void AT2R()
    {
        if (AT2_index < A2_number_of_atoms-1)
        {
            sfx.PlayTrack(SFX.sound_index.amino_click);
            AT2_index++;
            FindObjectOfType<AminoSliderController>().ModifyConnectionHolder_atomic(FindObjectOfType<ConnectionManager>().CreateAminoAcidLink_atom_modification(FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>(), ID_button1, AT1_index, FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>(), ID_button2, AT2_index), AminoPanel1.transform.GetChild(ID_button1).gameObject, AminoPanel2.transform.GetChild(ID_button2).gameObject, transform.parent, AT1_index, AT2_index,1);
            UpdateLink();
        }
    }

}
