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

	void Awake()
	{
		distancia = GetComponentInChildren<Text> ();
	}

	void Update()
	{
		distancia.text = (connection.distance).ToString ("F1");
	}
}
