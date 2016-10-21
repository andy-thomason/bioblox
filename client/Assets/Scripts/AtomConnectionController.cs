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
        {
            if(asc.atom_selected_p1 != -1)
                transform.parent.GetChild(asc.atom_selected_p1).GetComponent<Animator>().SetBool("High", false);
            asc.atom_selected_p1 = atom_child_index;
        }
        else
        {
            if (asc.atom_selected_p2 != -1)
                transform.parent.GetChild(asc.atom_selected_p2).GetComponent<Animator>().SetBool("High", false);
            asc.atom_selected_p2 = atom_child_index;
        }

        GetComponent<Animator>().SetBool("High", true);
    }


}
