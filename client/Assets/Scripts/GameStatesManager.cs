using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GameStatesManager : MonoBehaviour {

    public GameObject StateSelected;
    public Text n_atoms_display;
    public Text ele_score_display;
    public Text lp_score_display;
    public Text state_display;

    public void Load()
    {
        if(StateSelected)
            StateSelected.GetComponent<ButtonGameState>().Load();
    }

    public void Save()
    {
        if (StateSelected)
            StateSelected.GetComponent<ButtonGameState>().Save();
    }

}
