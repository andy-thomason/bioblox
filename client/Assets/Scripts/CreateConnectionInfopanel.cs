using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CreateConnectionInfopanel : MonoBehaviour, IPointerClickHandler
{

    public int amino_id_0;
    public int amino_id_1;
    public int atom_id;
    public int protein_id;
    public GameObject A1Button;
    public GameObject A2Button;

    public void OnPointerClick(PointerEventData eventData)
    {
        AminoSliderController asc = FindObjectOfType<AminoSliderController>();
        if (protein_id == 0)
            asc.atom_selected_p2 = atom_id;
        else
            asc.atom_selected_p1 = atom_id;

        asc.AddConnectionButton_from_amino_info_panel(A1Button, A2Button, amino_id_0, amino_id_1);
    }

    public void SetValues(int amino1, int amino2, int atom, int protein, GameObject a1, GameObject a2)
    {
        amino_id_0 = amino1;
        amino_id_1 = amino2;
        atom_id = atom;
        protein_id = protein;
        A1Button = a1;
        A2Button = a2;
    }
}
