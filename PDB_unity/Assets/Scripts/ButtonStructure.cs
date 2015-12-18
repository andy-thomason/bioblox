using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonStructure : MonoBehaviour {

	public Color32 ALAColor = new Color32 (255, 255, 255, 255), ARGColor = new Color32 (253, 125, 125, 255),ASNColor = new Color32(248, 221, 173, 255),
	ASPColor = new Color32(253, 198, 0, 255), CYSColor = new Color32(220, 234, 153, 255), GLUColor = new Color32(216, 244, 88, 255),
	GLYColor = new Color32(159, 233, 148, 255), HISColor = new Color32(53, 218, 22, 255), ILEColor = new Color32(140, 221, 194, 255),
	LEUColor = new Color32(44, 219, 212, 255), LYSColor = new Color32(44, 161, 219, 255), METColor = new Color32(0, 255, 160, 255),
	PHEColor = new Color32(161, 164, 223, 255), PROColor = new Color32(194, 161, 223, 255), SERColor = new Color32(223, 161, 223, 255),
	THRColor = new Color32(223, 161, 221, 255), GLNColor = new Color32(242, 105, 164, 255), TRPColor = new Color32(176, 159, 167, 255),
	TYRColor = new Color32(234, 162, 169, 255), VALColor = new Color32(249, 101, 116, 255),
	NEGATIVEColor = new Color32(234, 162, 169, 255), POSITIVEColor = new Color32(234, 162, 169, 255), HYDROColor = new Color32(234, 162, 169, 255),
	POLARColor = new Color32(234, 162, 169, 255), SPECIALColor = new Color32(234, 162, 169, 255);

	public Dictionary<string, Color32> NormalColor;
	public Dictionary<string, Color32> ChargedColor;

	// Use this for initialization
	void Awake () {

		NormalColor = new Dictionary<string, Color32>{
			{" ALA",  ALAColor}, {" ARG",  ARGColor}, {" ASN",  ASNColor}, {" ASP",  ASPColor}, {" CYS",  CYSColor}, {" GLU", GLUColor},
			{" GLY",  GLYColor},{" HIS",  HISColor}, {" ILE",  ILEColor}, {" LEU",  LEUColor}, {" LYS",  LYSColor}, {" MET",  METColor},
			{" PHE",  PHEColor},{" PRO",  PROColor}, {" SER",  SERColor}, {" THR",  THRColor}, {" GLN", GLNColor}, {" TRP",  TRPColor},
			{" TYR",  TYRColor}, {" VAL",  VALColor},
		};

		ChargedColor = new Dictionary<string, Color32>{
			{" ALA",  HYDROColor}, {" ARG",  POSITIVEColor}, {" ASN",  POLARColor}, {" ASP",  NEGATIVEColor}, {" CYS",  SPECIALColor}, {" GLU",  NEGATIVEColor},
			{" GLY",  SPECIALColor}, {" HIS",  POSITIVEColor}, {" ILE",  HYDROColor}, {" LEU",  HYDROColor}, {" LYS",  POSITIVEColor}, {" MET",  HYDROColor},
			{" PHE",  HYDROColor},	{" PRO",  SPECIALColor}, {" SER",  POLARColor}, {" THR",  POLARColor}, {" GLN", POLARColor}, {" TRP",  HYDROColor},
			{" TYR",  HYDROColor}, {" VAL",  HYDROColor},
		};
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
