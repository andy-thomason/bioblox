using UnityEngine;
using System.Collections;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System;

public class XML : MonoBehaviour {

    TextAsset xml_file;
    public XmlDocument database_xml;
    string xml_path;

    // Use this for initialization
    void Start ()
    {
        xml_path = Application.dataPath + "/Resources/database.xml";
        xml_file = Resources.Load("database") as TextAsset;
        database_xml = new XmlDocument();
        database_xml.LoadXml(xml_file.text);
    }

    public int GetInstance()
    {
        //get the patient element
        XmlElement instance_element = (XmlElement)database_xml.SelectSingleNode("/database");
        return int.Parse(instance_element.GetAttribute("current_instance")) + 1;
    }

    public void SaveXML(int level, string atoms, float time)
    {
        //get the root element
        XmlElement root_tag = (XmlElement)database_xml.SelectSingleNode("/database");

        //create the query
        XmlElement instance_node = database_xml.CreateElement("instance");
        instance_node.SetAttribute("number", ""+ GetInstance());
        instance_node.SetAttribute("level", ""+ level);
        instance_node.SetAttribute("atoms", atoms);
        instance_node.SetAttribute("time", ""+ time);
        //update the current instance
        root_tag.SetAttribute("current_instance", "" + GetInstance());

        //append to root node
        root_tag.AppendChild(instance_node);
        //save doc 
        database_xml.Save(xml_path);
    }
}
