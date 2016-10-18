using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class AtomConnectionController : MonoBehaviour, IPointerClickHandler {

    public int atom_id;
    public int protein_id;
    AminoSliderController asc;

	// Use this for initialization
	void Start ()
    {
        asc = FindObjectOfType<AminoSliderController>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (protein_id == 0)
            asc.atom_selected_p1 = atom_id;
        else
            asc.atom_selected_p2 = atom_id;
        Debug.Log("si");
    }


}
