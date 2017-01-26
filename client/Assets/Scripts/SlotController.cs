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

    public void SetValues(string[] values)
    {
        total_score = values[0];
        atoms_score = values[1];
        ljp_score = values[2];
        ie_score = values[3];
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        transform.parent.transform.parent.GetChild(1).GetComponent<Text>().text = total_score;
        transform.parent.transform.parent.GetChild(2).GetComponent<Text>().text = atoms_score;
        transform.parent.transform.parent.GetChild(3).GetComponent<Text>().text = ljp_score;
        transform.parent.transform.parent.GetChild(4).GetComponent<Text>().text = ie_score;
        transform.parent.transform.parent.GetChild(13).GetComponent<LevelSelectionButton>().slot_number = slot_id;
    }
}
