using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AtomOnAminoController : MonoBehaviour, IPointerClickHandler
{
    public int protein_id;
    public int atom_id;
    public int amino_child_index;

    public void OnPointerClick(PointerEventData eventData)
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.amino_click);
        FindObjectOfType<AminoSliderController>().UpdateCurrentAtomSelected(protein_id, amino_child_index, transform.GetSiblingIndex());

        //transform.parent.transform.GetChild(transform.GetSiblingIndex() - 1).GetComponent<AminoButtonController>().HighlightCurrentAtom(transform.GetSiblingIndex());
    }
}
