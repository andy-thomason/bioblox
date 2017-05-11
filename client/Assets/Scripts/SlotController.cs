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
    WWWForm www_form;

    //public void SetValues(string[] values)
    //{
    //    total_score = values[0];
    //    atoms_score = values[1];
    //    ljp_score = values[2];
    //    ie_score = values[3];

    //    ////change buttom color
    //    //if (total_score != "0" && atoms_score != "0")
    //    //{
    //    //    GetComponent<Image>().color = new Color(101, 172, 218);
    //    //    is_empty = false;
    //    //}
    //}

    public void OnPointerClick(PointerEventData eventData)
    {
        www_form = new WWWForm();
        www_form.AddField("id_user", FindObjectOfType<GameManager>().id_user);
        www_form.AddField("level", transform.parent.transform.parent.GetComponentInChildren<LevelSelectionButton>().level_number);
        www_form.AddField("slot", slot_id);

        StartCoroutine(get_score());
    }

    IEnumerator get_score()
    {

        WWW SQLQuery = new WWW("https://bioblox3d.org/wp-content/themes/write/db/get_score.php", www_form);
        yield return SQLQuery;

        if (SQLQuery.text != "0")
        {
            string[] splitScoresLevel = SQLQuery.text.Split(',');
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().alpha = 1;
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().interactable = true;
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().blocksRaycasts = true;
            transform.parent.transform.parent.transform.GetChild(0).transform.GetChild(1).GetComponent<Text>().text = splitScoresLevel[0];
            transform.parent.transform.parent.transform.GetChild(0).transform.GetChild(2).GetComponent<Text>().text = splitScoresLevel[1];
            transform.parent.transform.parent.transform.GetChild(0).transform.GetChild(3).GetComponent<Text>().text = splitScoresLevel[2];
            transform.parent.transform.parent.transform.GetChild(0).transform.GetChild(4).GetComponent<Text>().text = splitScoresLevel[3];
        }
        else
        {
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().alpha = 0;
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().interactable = false;
            transform.parent.transform.parent.transform.GetChild(0).GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
    }
}
