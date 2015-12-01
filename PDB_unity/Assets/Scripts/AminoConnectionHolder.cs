using UnityEngine;
using System.Collections;
using AssemblyCSharp;

public class AminoConnectionHolder : MonoBehaviour {

	public AtomConnection connection;
	public int ID_button1;
	public int ID_button2;

	
	public void DeleteAminoLink()
	{		
		FindObjectOfType<ConnectionManager> ().DeleteAminoAcidLink (connection);
		FindObjectOfType<AminoSliderController> ().RestoreDeletedAminoButtons (ID_button1, ID_button2);
		Destroy (gameObject);
	}
}
