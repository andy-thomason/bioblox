using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonGameState : MonoBehaviour, IPointerClickHandler {

    GameStates GameState;
    GameStatesManager gameStateManager;
    AminoSliderController aminoSliderController;
    BioBlox bb;

    string n_atoms;
    string ele_score;
    string ljp_score;
    Vector3 protein1_position;
    Vector3 protein2_position;
    List<Vector2> amino_acids = new List<Vector2>();

    void Awake()
    {
        gameStateManager = FindObjectOfType<GameStatesManager>();
        aminoSliderController = FindObjectOfType<AminoSliderController>();
        bb = FindObjectOfType<BioBlox>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        //Debug.Log(ljp_score + " - " + n_atoms + " - " + ele_score);
        //Debug.Log(transform.name);
        //show info and set as selected one
        gameStateManager.StateSelected = gameObject;
        UpdateInfoPanel();
    }

    void UpdateInfoPanel()
    {
        //show info
        gameStateManager.state_display.text = name;
        gameStateManager.lp_score_display.text = ljp_score != null ? ljp_score : "no record";
        gameStateManager.n_atoms_display.text = n_atoms != null ? n_atoms : "no record";
        gameStateManager.ele_score_display.text = ele_score != null ? ele_score : "no record";
    }

	public void Save()
    {
        n_atoms =  GameObject.Find("NumAtoms").GetComponent<Text>().text;
        ele_score = bb.ElectricScore.text;
        ljp_score = bb.LennardScore.text;
        amino_acids = new List<Vector2>();
        //get the amino
        foreach (Transform childLinks in aminoSliderController.AminoLinkPanelParent.transform)
        {
            amino_acids.Add(new Vector2(childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button1, childLinks.GetComponentInChildren<AminoConnectionHolder>().ID_button2));
        }

        protein1_position = bb.molecules[0].transform.localEulerAngles;
        protein2_position = bb.molecules[1].transform.localEulerAngles;

        GameState = new GameStates(n_atoms, ele_score, ljp_score, protein1_position, protein2_position, amino_acids);
        UpdateInfoPanel();
    }

    public void Load()
    {
        aminoSliderController.DeleteAllAminoConnections();
        //Debug.Log(GameState);
        if (GameState != null)
        {
            for (int i = 0; i < amino_acids.Count; i++)
            {
                //turn to normal
                aminoSliderController.SliderMol[0].transform.Find(((int)amino_acids[i].x).ToString()).gameObject.transform.localScale = new Vector3(1, 1, 1);
                aminoSliderController.SliderMol[1].transform.Find(((int)amino_acids[i].y).ToString()).gameObject.transform.localScale = new Vector3(1, 1, 1);
                aminoSliderController.AminoAcidsLinkPanel(bb.GetComponent<ConnectionManager>().CreateAminoAcidLink(bb.molecules[0].GetComponent<PDB_mesh>(), (int)amino_acids[i].x, bb.molecules[1].GetComponent<PDB_mesh>(), (int)amino_acids[i].y), aminoSliderController.SliderMol[0].transform.Find(((int)amino_acids[i].x).ToString()).gameObject, aminoSliderController.SliderMol[1].transform.Find(((int)amino_acids[i].y).ToString()).gameObject);
            }
            FindObjectOfType<ConnectionManager>().SliderStrings.interactable = true;
            //set the rotation of the molecuels
            bb.molecules[0].transform.localEulerAngles = protein1_position;
            bb.molecules[1].transform.localEulerAngles = protein2_position;
        }
    }

    public void SaveCustom()
    {
        n_atoms = "Good dock";
        ele_score = "Good dock";
        ljp_score = "Good dock";
        amino_acids = new List<Vector2>();
        amino_acids.Add(new Vector2(168, 14));
        amino_acids.Add(new Vector2(54, 16));
        amino_acids.Add(new Vector2(154, 38));

        protein1_position = bb.molecules[0].transform.localEulerAngles;
        protein2_position = bb.molecules[1].transform.localEulerAngles;

        GameState = new GameStates(n_atoms, ele_score, ljp_score, protein1_position, protein2_position, amino_acids);
    }
}
