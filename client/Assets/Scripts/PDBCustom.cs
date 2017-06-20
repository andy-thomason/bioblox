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

    public string pdb_1_temp;
    public string pdb_2_temp;

    // Use this for initialization
    void Start ()
    {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    //FIRST PDB
    public void load_pdb_id_1()
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        // NOTE: gameObject.name MUST BE UNIQUE!!!!
        GetFile.GetFileFromUserAsync(gameObject.name, "ReceivePDB_one");
        //StartCoroutine(check_pdb_id_IE());
    }

    //static string s_dataUrlPrefix = "data:image/png;base64,";
    public void ReceivePDB_one(string dataUrl)
    {
        Debug.Log("111");
        pdb_1 = dataUrl;
        //file_output.text = dataUrl;
        Debug.Log(pdb_1);
        StartCoroutine(upload_file_1());
    }

    IEnumerator upload_file_1()
    {
        byte[] file_pdb_bytes = System.Text.Encoding.UTF8.GetBytes(pdb_1);
        WWWForm form = new WWWForm();
        form.AddField("file", "file");
        form.AddBinaryData("file", file_pdb_bytes, "file_1.pdb");

        WWW w = new WWW("http://www.atomicincrement.com/bioblox/pro/upload_file.php", form);

        yield return w;

        Debug.Log(w.error);

       // GetComponent<GameManager>().Custom_ChangeLevel();
    }

    //SECOND PDB
    public void load_pdb_id_2()
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        // NOTE: gameObject.name MUST BE UNIQUE!!!!
        GetFile.GetFileFromUserAsync(gameObject.name, "ReceivePDB_two");
        //StartCoroutine(check_pdb_id_IE());
    }

    //static string s_dataUrlPrefix = "data:image/png;base64,";
    public void ReceivePDB_two(string dataUrl)
    {
        Debug.Log("222");
        pdb_2 = dataUrl;
        //file_output.text = dataUrl;
        Debug.Log(pdb_2);
        StartCoroutine(upload_file_2());
    }

    IEnumerator upload_file_2()
    {
        byte[] file_pdb_bytes = System.Text.Encoding.UTF8.GetBytes(pdb_2);
        WWWForm form = new WWWForm();
        form.AddField("file", "file");
        form.AddBinaryData("file", file_pdb_bytes, "file_2.pdb");

        WWW w = new WWW("http://www.atomicincrement.com/bioblox/pro/upload_file.php", form);

        yield return w;

        Debug.Log(w.error);
        
        //GetComponent<GameManager>().Custom_ChangeLevel();
    }

    public void load_custom_pdbs()
    {
        pdb_1_temp = string.Empty;
        using (StringReader reader = new StringReader(pdb_1))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string kind = line.Substring(0, Mathf.Min(6, line.Length));

                if (kind == "ATOM  ")
                {
                    pdb_1_temp = string.Concat(pdb_1_temp, Environment.NewLine, line.Substring(0, Mathf.Max(6, line.Length)));
                }
            }
        }

        pdb_2_temp = string.Empty;
        using (StringReader reader = new StringReader(pdb_2))
        {
            string line;

            while ((line = reader.ReadLine()) != null)
            {
                string kind = line.Substring(0, Mathf.Max(6, line.Length));

                if (kind == "ATOM  ")
                {
                    pdb_2_temp = string.Concat(pdb_2_temp, Environment.NewLine, line.Substring(0, Mathf.Min(6, line.Length)));
                }
            }
        }

        pdb_file = string.Concat(pdb_1_temp, pdb_2_temp);
        Debug.Log(pdb_file);
        StartCoroutine(upload_file_pdb());

        //GetComponent<GameManager>().Custom_ChangeLevel();
    }

    IEnumerator upload_file_pdb()
    {
        byte[] file_pdb_bytes = System.Text.Encoding.UTF8.GetBytes(pdb_file);
        WWWForm form = new WWWForm();
        form.AddField("file", "file");
        form.AddBinaryData("file", file_pdb_bytes, "file_merged.pdb");

        WWW w = new WWW("http://www.atomicincrement.com/bioblox/pro/upload_file.php", form);

        yield return w;

        Debug.Log(w.error);

        //GetComponent<GameManager>().Custom_ChangeLevel();
    }

}
