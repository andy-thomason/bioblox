using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

public class PDBCustom : MonoBehaviour {

    public InputField pdb_id_input;
    public InputField pdb_id_input_chain_0;
    public InputField pdb_id_input_chain_1;
    public string pdb_1;
    public string pdb_2;
    public string pdb_file;
    public GameObject pdb_error;
    Stream stream;
    GameManager gm;

    public string pdb_1_temp;
    public string pdb_2_temp;
    public Text pdb_id_1;
    public Text pdb_id_2;
    bool is_pdb_1_loaded = false;
    bool is_pdb_2_loaded = false;
    public Button load_1_2;

    public Text pdb_id_complex;
    bool is_pdb_complex1_loaded = false;
    public Button load_complex;

    public string pdb_custom_1_name;
    public string pdb_custom_2_name;

    int pdb_id;

    // Use this for initialization
    void Start ()
    {
        gm = GetComponent<GameManager>();
    }
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    //FIRST PDB
    public void load_pdb_id(int id_pdb_temp)
    {
        pdb_id = id_pdb_temp;
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        // NOTE: gameObject.name MUST BE UNIQUE!!!!
        GetFile.GetFileFromUserAsync(gameObject.name, "ReceivePDB");
        //StartCoroutine(check_pdb_id_IE());
    }

    //static string s_dataUrlPrefix = "data:image/png;base64,";
    public void ReceivePDB(string dataUrl)
    {
        if(pdb_id == 0)
        {
            pdb_1 = dataUrl;
            Debug.Log(pdb_1);
            is_pdb_1_loaded = true;
            pdb_custom_1_name = pdb_1.Substring(62, 4);
            pdb_id_1.text = pdb_custom_1_name;
            gm.pdb_custom_1_name = pdb_custom_1_name;
        }
        else
        {
            pdb_2 = dataUrl;
            Debug.Log(pdb_2);
            is_pdb_2_loaded = true;
            pdb_custom_2_name = pdb_2.Substring(62, 4);
            pdb_id_2.text = pdb_custom_2_name;
            gm.pdb_custom_2_name = pdb_custom_2_name;
        }

        if (is_pdb_1_loaded && is_pdb_2_loaded)
            load_1_2.interactable = true;

        //file_output.text = dataUrl;
        StartCoroutine(upload_file());
    }

    IEnumerator upload_file()
    {
        //GET THE ID OF EACH FILE
        string file_name = pdb_id == 0 ? string.Concat(pdb_custom_1_name, ".pdb") : string.Concat(pdb_custom_2_name, ".pdb");
        byte[] file_pdb_bytes = System.Text.Encoding.UTF8.GetBytes(pdb_id == 0 ? pdb_1 : pdb_2);
        WWWForm form = new WWWForm();
        form.AddField("file", "file");
        form.AddBinaryData("file", file_pdb_bytes, file_name);

        WWW w = new WWW("http://13.58.210.151/upload_file.php", form);

        yield return w;

        Debug.Log(w.error);

       // GetComponent<GameManager>().Custom_ChangeLevel();
    }

    public void merge_pdb()
    {
        Debug.Log("****************************");
        Debug.Log(pdb_1);
        Debug.Log(pdb_2);
        Debug.Log("****************************");
        
        pdb_file = string.Concat(read_pdb_1(pdb_1), read_pdb_2(pdb_2));
        Debug.Log(pdb_file);
        StartCoroutine(upload_file_pdb());

        //GetComponent<GameManager>().Custom_ChangeLevel();
    }

    string index_amino_to_follow_s;
    int index_amino_to_follow;
    string empty_space = "    ";
    int index_atom = 1;
    string previous_amino;

    public string read_pdb_1(string pdb_file)
    {
        index_amino_to_follow_s = string.Empty;
        string pdb_temp = string.Concat("HEADER    ", pdb_custom_1_name, "-", pdb_custom_2_name);
        using (StringReader reader = new StringReader(pdb_file))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string kind = line.Substring(0, Mathf.Min(6, line.Length));

                if (kind == "ATOM  ")
                {
                    pdb_temp = string.Concat(pdb_temp, Environment.NewLine, string.Concat(line.Substring(0, 21), "A", line.Substring(22, line.Length - 22)));
                    index_amino_to_follow_s = line.Substring(23, 3);
                    index_atom++;
                }
            }
        }
        Debug.Log("****************************");
        Debug.Log(pdb_temp);
        Debug.Log("****************************");
        return pdb_temp;

    }

    public string read_pdb_2(string pdb_file)
    {
        index_amino_to_follow = int.Parse(index_amino_to_follow_s) + 1;
        string pdb_temp = string.Empty;
        using (StringReader reader = new StringReader(pdb_file))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {

                string kind = line.Substring(0, Mathf.Min(6, line.Length));

                if (kind == "ATOM  ")
                {
                    if (index_atom < 1000)
                        empty_space = "    ";
                    else
                        empty_space = "   ";
                    
                    string line_0 = string.Concat("ATOM", empty_space, index_atom.ToString(), line.Substring(11, 10));

                    if(previous_amino != line.Substring(17, 3))
                    {
                        index_amino_to_follow++;
                        previous_amino = line.Substring(17, 3);
                    }

                    if (index_amino_to_follow >= 100)
                        empty_space = " ";
                    else if (index_amino_to_follow <= 99 && index_amino_to_follow >= 10)
                        empty_space = "  ";
                    else
                        empty_space = "   ";

                    string line_1 = string.Concat("B", empty_space, index_amino_to_follow.ToString());

                    string line_pdb = string.Concat(line_0, line_1, line.Substring(26, (line.Length - 26)));
                    pdb_temp = string.Concat(pdb_temp, Environment.NewLine, line_pdb);
                    index_atom++;
                }
            }
        }
        Debug.Log("****************************");
        Debug.Log(pdb_temp);
        Debug.Log("****************************");
        return pdb_temp;

    }

    IEnumerator upload_file_pdb()
    {
        byte[] file_pdb_bytes = System.Text.Encoding.UTF8.GetBytes(pdb_file);
        WWWForm form = new WWWForm();
        form.AddField("file", "file");
        form.AddBinaryData("file", file_pdb_bytes, string.Concat(pdb_custom_1_name, "-", pdb_custom_2_name, ".pdb"));

        WWW w = new WWW("http://13.58.210.151/upload_file.php", form);

        yield return w;

        Debug.Log(w.error);

        GetComponent<GameManager>().Custom_ChangeLevel();
    }

}
