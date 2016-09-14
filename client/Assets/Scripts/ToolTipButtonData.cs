using UnityEngine;
using System.Collections;

public class ToolTipButtonData : MonoBehaviour {

    public string ButtonToolTipText;
    public enum TOOLTIP_DIRECTION { RIGHT = 0, LEFT, UP, DOWN };
    public TOOLTIP_DIRECTION Tooltip_Offset;

}
