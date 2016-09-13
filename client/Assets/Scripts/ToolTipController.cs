using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ToolTipController : MonoBehaviour {

    public GameObject ToolTip;
    private GameObject ToolTipReference;
    private ToolTipButtonData toolTipButtonData;
	
    void Awake()
    {
        toolTipButtonData = FindObjectOfType<ToolTipButtonData>();
    }

    public void SpawnToolTip(GameObject CurrentButtom)
    {
        if (ToolTipReference == null)
        {
            ToolTipReference = Instantiate(ToolTip);
            ToolTipReference.transform.SetParent(transform, true);
            //ToolTipReference.transform.Translate(Input.mousePosition + CurrentButtom.GetComponent<ToolTipButtonData>().ToolTipOffset);
            ToolTipReference.GetComponentInChildren<Text>().text = CurrentButtom.GetComponent<ToolTipButtonData>().ButtonToolTipText; 
            //we have to wait to get the value of the flexible text (I dont know why)
            StartCoroutine(ShowToolTip(CurrentButtom));
        }
    }

    public void DestroyToolTip()
    {
        if (ToolTipReference != null)
            Destroy(ToolTipReference);
    }


    IEnumerator ShowToolTip(GameObject CurrentButtom)
    {
        yield return new WaitForSeconds(0.1f);
        //assign the flexible size of the text to the image
        if (ToolTipReference != null)
        {
            ToolTipReference.transform.FindChild("ToolTipBackground").GetComponent<RectTransform>().sizeDelta = new Vector2(ToolTipReference.transform.FindChild("ToolTipText").GetComponent<RectTransform>().rect.width - 45, 12);
            //offset
            switch (CurrentButtom.GetComponent<ToolTipButtonData>().Tooltip_Offset)
            {
                case ToolTipButtonData.TOOLTIP_DIRECTION.RIGHT:
                    ToolTipReference.transform.Translate(Input.mousePosition + new Vector3((ToolTipReference.transform.FindChild("ToolTipText").GetComponent<RectTransform>().rect.width ), 0, 0));
                    break;
                case ToolTipButtonData.TOOLTIP_DIRECTION.LEFT:
                    ToolTipReference.transform.Translate(Input.mousePosition - new Vector3((ToolTipReference.transform.FindChild("ToolTipText").GetComponent<RectTransform>().rect.width), 0, 0));
                    break;
                case ToolTipButtonData.TOOLTIP_DIRECTION.UP:
                    ToolTipReference.transform.Translate(Input.mousePosition + new Vector3(0, 30.0f, 0));
                    break;
                case ToolTipButtonData.TOOLTIP_DIRECTION.DOWN:
                    ToolTipReference.transform.Translate(Input.mousePosition - new Vector3(0, 30.0f, 0));
                    break;
                default:
                    break;
            }

            ToolTipReference.GetComponent<CanvasGroup>().alpha = 1.0f;
        }
    }
    
}
