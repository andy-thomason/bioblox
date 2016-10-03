using UnityEngine;
using AssemblyCSharp;
using UnityEngine.UI;

public class AminoConnectionHolder : MonoBehaviour {

	public AtomConnection connection;
	public int ID_button1;
	public int ID_button2;
	Text distancia;
    GameObject AminoPanel1;
    GameObject AminoPanel2;
    SFX sfx;
	
	public void UpdateLink()
	{		
		FindObjectOfType<ConnectionManager> ().DeleteAminoAcidLink (connection);
		FindObjectOfType<AminoSliderController> ().RestoreDeletedAminoButtons (ID_button1, ID_button2);
		FindObjectOfType<ConnectionManager> ().DisableSlider ();
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
    }

    void Awake()
	{
		distancia = GetComponentInChildren<Text> ();
        sfx = FindObjectOfType<SFX>();
        AminoPanel1 = GameObject.Find("ContentPanelA1").gameObject;
        AminoPanel2 = GameObject.Find("ContentPanelA2").gameObject;
    }

	void Update()
	{
        if (distancia == null) return;
		distancia.text = (connection.distance).ToString ("F1");
	}

    //SLIDER BUTTONS

    public void A1L()
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        ModifyConnection(-1, 0);
    }
    public void A1R()
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        ModifyConnection(1, 0);
    }
    public void A2L()
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        ModifyConnection(0, -1);
    }
    public void A2R()
    {
        sfx.PlayTrack(SFX.sound_index.amino_click);
        ModifyConnection(0, 1);
    }

    public void ModifyConnection(int A1, int A2)
    {
        AminoPanel2.transform.GetChild(ID_button2).GetComponent<Animator>().SetBool("High", false);
        AminoPanel1.transform.GetChild(ID_button1).GetComponent<Animator>().SetBool("High", false);
        FindObjectOfType<AminoSliderController>().ModifyConnectionHolder(FindObjectOfType<ConnectionManager>().CreateAminoAcidLink(FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>(), ID_button1+A1, FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>(), ID_button2+A2), AminoPanel1.transform.GetChild(ID_button1 + A1).gameObject, AminoPanel2.transform.GetChild(ID_button2 + A2).gameObject,transform.parent);
        FindObjectOfType<ConnectionManager>().SliderStrings.interactable = true;
        AminoPanel2.transform.GetChild(ID_button2 + A2).GetComponent<Animator>().SetBool("High", true);
        AminoPanel1.transform.GetChild(ID_button1 + A1).GetComponent<Animator>().SetBool("High", true);
        UpdateLink();
    }

}
