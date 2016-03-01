using UnityEngine;
using AssemblyCSharp;
using UnityEngine.UI;

public class AminoConnectionHolder : MonoBehaviour {

	public AtomConnection connection;
	public int ID_button1;
	public int ID_button2;
	Text distancia;

	
	public void DeleteAminoLink()
	{		
		FindObjectOfType<ConnectionManager> ().DeleteAminoAcidLink (connection);
		FindObjectOfType<AminoSliderController> ().RestoreDeletedAminoButtons (ID_button1, ID_button2);
		FindObjectOfType<ConnectionManager> ().DisableSlider ();
		Destroy (gameObject);
	}

    public void HighlightClick()
    {
        FindObjectOfType<BioBlox>().molecules[1].GetComponent<PDB_mesh>().SelectAminoAcid(ID_button2);
        FindObjectOfType<AminoSliderController>().UpdateCurrentButtonA2(ID_button2);			
		FindObjectOfType<BioBlox>().molecules[0].GetComponent<PDB_mesh>().SelectAminoAcid(ID_button1);
        FindObjectOfType<AminoSliderController>().UpdateCurrentButtonA1(ID_button1);
        FindObjectOfType<AminoSliderController>().HighLight3DMeshAll(ID_button1, ID_button2);
    }

    void Awake()
	{
		distancia = GetComponentInChildren<Text> ();
	}

	void Update()
	{
		distancia.text = (connection.distance).ToString ("F1");
	}
}
