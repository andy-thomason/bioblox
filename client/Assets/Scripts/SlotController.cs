using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SlotController : MonoBehaviour, IPointerClickHandler
{

    public int slot_id;
    public string ie_score = "0";
    public string ljp_score = "0";
    public string atoms_score = "0";
    public string total_score = "0";
    bool is_empty = true;

    public void SetValues(string[] values)
    {
        total_score = values[0];
        atoms_score = values[1];
        ljp_score = values[2];
        ie_score = values[3];

        //change buttom color
        if (total_score != "0" && atoms_score != "0")
        {
            GetComponent<Image>().color = new Color(101, 172, 218);
            is_empty = false;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        if (!is_empty)
        {
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().alpha = 1;
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().interactable = true;
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().blocksRaycasts = true;
            transform.parent.transform.parent.transform.GetChild(0).transform.GetChild(1).GetComponent<Text>().text = total_score;
            transform.parent.transform.parent.transform.GetChild(0).transform.GetChild(2).GetComponent<Text>().text = atoms_score;
            transform.parent.transform.parent.transform.GetChild(0).transform.GetChild(3).GetComponent<Text>().text = ljp_score;
            transform.parent.transform.parent.transform.GetChild(0).transform.GetChild(4).GetComponent<Text>().text = ie_score;
        }
        else
        {
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().alpha = 0;
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().interactable = false;
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().blocksRaycasts = false;
        }
        transform.parent.transform.parent.GetChild(4).GetComponent<LevelSelectionButton>().slot_number = slot_id;
    }
}
