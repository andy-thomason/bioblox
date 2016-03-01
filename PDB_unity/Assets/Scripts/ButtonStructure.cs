using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonStructure : MonoBehaviour {

  public Color32 ALAColor = new Color32(255, 255, 255, 255), ARGColor = new Color32(253, 125, 125, 255), ASNColor = new Color32(248, 221, 173, 255),
  ASPColor = new Color32(253, 198, 0, 255), CYSColor = new Color32(220, 234, 153, 255), GLUColor = new Color32(216, 244, 88, 255),
  GLYColor = new Color32(159, 233, 148, 255), HISColor = new Color32(53, 218, 22, 255), ILEColor = new Color32(140, 221, 194, 255),
  LEUColor = new Color32(44, 219, 212, 255), LYSColor = new Color32(44, 161, 219, 255), METColor = new Color32(0, 255, 160, 255),
  PHEColor = new Color32(161, 164, 223, 255), PROColor = new Color32(194, 161, 223, 255), SERColor = new Color32(223, 161, 223, 255),
  THRColor = new Color32(223, 161, 221, 255), GLNColor = new Color32(242, 105, 164, 255), TRPColor = new Color32(176, 159, 167, 255),
  TYRColor = new Color32(234, 162, 169, 255), VALColor = new Color32(249, 101, 116, 255);

  public Color32 NEGATIVEColor = new Color32(255,  30,  30, 255);
  public Color32 POSITIVEColor = new Color32( 90,  90, 255, 255);
  public Color32 HYDROColor =    new Color32( 90,  90,  90, 255);
  public Color32 POLARColor =    new Color32( 90, 255, 255, 255);
  public Color32 SPECIALColor =  new Color32(192, 192, 192, 255);
  public Color32 SULPHURColor =  new Color32(255, 255,   0, 255);

  public Dictionary<string, Color32> NormalColor;
	public Dictionary<string, Color32> FunctionColor;
  public Dictionary<string, int> FunctionType;

	// Use this for initialization
	void Awake () {

		NormalColor = new Dictionary<string, Color32>{
      {" ALA",  HYDROColor}, {" ARG",  POSITIVEColor}, {" ASN",  POLARColor}, {" ASP",  NEGATIVEColor}, {" CYS",  SPECIALColor}, {" GLU",  NEGATIVEColor},
      {" GLY",  SPECIALColor}, {" HIS",  POSITIVEColor}, {" ILE",  HYDROColor}, {" LEU",  HYDROColor}, {" LYS",  POSITIVEColor}, {" MET",  HYDROColor},
      {" PHE",  HYDROColor},  {" PRO",  SPECIALColor}, {" SER",  POLARColor}, {" THR",  POLARColor}, {" GLN", POLARColor}, {" TRP",  HYDROColor},
      {" TYR",  HYDROColor}, {" VAL",  HYDROColor},
      /*{" ALA",  ALAColor}, {" ARG",  ARGColor}, {" ASN",  ASNColor}, {" ASP",  ASPColor}, {" CYS",  CYSColor}, {" GLU", GLUColor},
			{" GLY",  GLYColor},{" HIS",  HISColor}, {" ILE",  ILEColor}, {" LEU",  LEUColor}, {" LYS",  LYSColor}, {" MET",  METColor},
			{" PHE",  PHEColor},{" PRO",  PROColor}, {" SER",  SERColor}, {" THR",  THRColor}, {" GLN", GLNColor}, {" TRP",  TRPColor},
			{" TYR",  TYRColor}, {" VAL",  VALColor},*/
		};

		FunctionColor = new Dictionary<string, Color32>{
			{" ALA",  HYDROColor}, {" ARG",  POSITIVEColor}, {" ASN",  POLARColor}, {" ASP",  NEGATIVEColor}, {" CYS",  SPECIALColor}, {" GLU",  NEGATIVEColor},
			{" GLY",  SPECIALColor}, {" HIS",  POSITIVEColor}, {" ILE",  HYDROColor}, {" LEU",  HYDROColor}, {" LYS",  POSITIVEColor}, {" MET",  HYDROColor},
			{" PHE",  HYDROColor},	{" PRO",  SPECIALColor}, {" SER",  POLARColor}, {" THR",  POLARColor}, {" GLN", POLARColor}, {" TRP",  HYDROColor},
			{" TYR",  HYDROColor}, {" VAL",  HYDROColor},
		};
        //hydro - 0 / posi - 1 / polar - 2 / nega - 3 / special - 4
        FunctionType = new Dictionary<string, int>{
            {" ALA",  0}, {" ARG",  1}, {" ASN",  2}, {" ASP",  3}, {" CYS",  4}, {" GLU",  3},
            {" GLY",  4}, {" HIS",  1}, {" ILE",  0}, {" LEU",  0}, {" LYS",  1}, {" MET",  0},
            {" PHE",  0},  {" PRO",  4}, {" SER",  2}, {" THR",  2}, {" GLN", 2}, {" TRP",  0},
            {" TYR",  0}, {" VAL",  0},
        };
    }
}
