using UnityEngine;
using System.Collections.Generic;

public class GameStates {

    public string n_atoms;
    public string ele_score;
    public string ljp_score;
    public Vector3 protein1_position;
    public Vector3 protein2_position;
    public List<Vector2> amino_acids = new List<Vector2>();


    public GameStates(string t_n_atoms, string t_ele_score, string t_ljp_score, Vector3 t_protein1_position, Vector3 t_protein2_position, List<Vector2> t_amino_acids)
    {
        n_atoms = t_n_atoms;
        ele_score = t_ele_score;
        ljp_score = t_ljp_score;
        protein1_position = t_protein1_position;
        protein2_position = t_protein2_position;
        amino_acids = t_amino_acids;
    }
}
