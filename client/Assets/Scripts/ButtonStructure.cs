using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonStructure : MonoBehaviour {
    public Color32 NEGATIVEColor = new Color32(255,   0,   0, 255);
    public Color32 POSITIVEColor = new Color32(  0,   0, 255, 255);
    public Color32 HYDROColor =    new Color32(128, 128, 128, 255);
    public Color32 POLARColor =    new Color32(255, 128, 128, 255);
    public Color32 SPECIALColor =  new Color32(192, 192, 192, 255);
    public Color32 SULPHURColor =  new Color32(255, 255, 128, 255);

    public Dictionary<string, Color32> NormalColor;
    public Dictionary<string, Color32> FunctionColor;
    public Dictionary<string, int> FunctionType;

    // Use this for initialization
    void Awake () {

        NormalColor = new Dictionary<string, Color32>{
            {"ALA",  HYDROColor}, {"ARG",  POSITIVEColor}, {"ASN",  POLARColor}, {"ASP",  NEGATIVEColor}, {"CYS",  SPECIALColor}, {"GLU",  NEGATIVEColor},
            {"GLY",  SPECIALColor}, {"HIS",  POSITIVEColor}, {"ILE",  HYDROColor}, {"LEU",  HYDROColor}, {"LYS",  POSITIVEColor}, {"MET",  HYDROColor},
            {"PHE",  HYDROColor},  {"PRO",  SPECIALColor}, {"SER",  POLARColor}, {"THR",  POLARColor}, {"GLN", POLARColor}, {"TRP",  HYDROColor},
            {"TYR",  HYDROColor}, {"VAL",  HYDROColor},
        };

        FunctionColor = new Dictionary<string, Color32>{
            {"ALA",  HYDROColor}, {"ARG",  POSITIVEColor}, {"ASN",  POLARColor}, {"ASP",  NEGATIVEColor}, {"CYS",  SPECIALColor}, {"GLU",  NEGATIVEColor},
            {"GLY",  SPECIALColor}, {"HIS",  POSITIVEColor}, {"ILE",  HYDROColor}, {"LEU",  HYDROColor}, {"LYS",  POSITIVEColor}, {"MET",  HYDROColor},
            {"PHE",  HYDROColor},    {"PRO",  SPECIALColor}, {"SER",  POLARColor}, {"THR",  POLARColor}, {"GLN", POLARColor}, {"TRP",  HYDROColor},
            {"TYR",  HYDROColor}, {"VAL",  HYDROColor},
        };
        //hydro - 0 / posi - 1 / polar - 2 / nega - 3 / special - 4
        FunctionType = new Dictionary<string, int>{
            {"ALA",  0}, {"ARG",  1}, {"ASN",  2}, {"ASP",  3}, {"CYS",  4}, {"GLU",  3},
            {"GLY",  4}, {"HIS",  1}, {"ILE",  0}, {"LEU",  0}, {"LYS",  1}, {"MET",  0},
            {"PHE",  0},  {"PRO",  4}, {"SER",  2}, {"THR",  2}, {"GLN", 2}, {"TRP",  0},
            {"TYR",  0}, {"VAL",  0},
        };
    }
}
