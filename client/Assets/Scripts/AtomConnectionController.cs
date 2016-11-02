using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class AtomConnectionController : MonoBehaviour, IPointerClickHandler {

    AminoSliderController asc;
    SFX sfx;
    public int atom_id;
    public int protein_id;
    public int atom_child_index;
    public int amino_acid_index;
    public int element_type;
    public string atom_name;

	// Use this for initialization
	void Start ()
    {
        asc = FindObjectOfType<AminoSliderController>();
        sfx = FindObjectOfType<SFX>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        sfx.PlayTrack(SFX.sound_index.button_click);
        if (protein_id == 0)
            asc.atom_selected_p1 = atom_child_index;
        else
            asc.atom_selected_p2 = atom_child_index;
    }


}
