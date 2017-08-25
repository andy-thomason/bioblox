using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;
using System.Runtime.InteropServices;

public class PDBCustom : MonoBehaviour {

    [DllImport("__Internal")]
    private static extern void download_file(string str);

    public InputField pdb_id_input;
    public InputField pdb_id_input_chain_0;
    public InputField pdb_id_input_chain_1;
    public string pdb_1;
    public string pdb_2;
    public string pdb_complex;
    public string pdb_file;
    public GameObject pdb_error;
    Stream stream;
    GameManager gm;
    BioBlox bb;

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
    public string pdb_custom_complex_name;

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
        Debug.Log("dataUrl: " + dataUrl);
        Debug.Log("string.IsNullOrEmpty(dataUrl): " + string.IsNullOrEmpty(dataUrl));
        if (pdb_id == 0)
        {
            if (dataUrl != "0" || !string.IsNullOrEmpty(dataUrl))
            {
                pdb_id_1.color = Color.white;
                pdb_1 = dataUrl;
                Debug.Log(pdb_1);
                is_pdb_1_loaded = true;
                pdb_custom_1_name = pdb_1.Substring(62, 4);
                pdb_id_1.text = pdb_custom_1_name;
                gm.pdb_custom_1_name = pdb_custom_1_name;
                StartCoroutine(upload_file());
            }
            else
            {
                pdb_id_1.color = Color.red;
                pdb_id_1.text = "INVALID FILE";
                is_pdb_1_loaded = false;
            }
        }
        else if(pdb_id == 1)
        {
            Debug.Log(string.IsNullOrEmpty(dataUrl));
            Debug.Log(dataUrl);
            if (dataUrl != "0" || !string.IsNullOrEmpty(dataUrl))
            {
                pdb_id_2.color = Color.white;
                pdb_2 = dataUrl;
                Debug.Log(pdb_2);
                is_pdb_2_loaded = true;
                pdb_custom_2_name = pdb_2.Substring(62, 4);
                pdb_id_2.text = pdb_custom_2_name;
                gm.pdb_custom_2_name = pdb_custom_2_name;
                StartCoroutine(upload_file());
            }
            else
            {
                pdb_id_2.color = Color.red;
                pdb_id_2.text = "INVALID FILE";
                is_pdb_2_loaded = false;
            }
        }
        else if (pdb_id == 2)
        {
            if (dataUrl == "0" || string.IsNullOrEmpty(dataUrl)) //invalid
            {
                if (dataUrl.Substring(7, 7) != "BIOBLOX")
                {
                pdb_id_complex.color = Color.red;
                pdb_id_complex.text = "INVALID FILE";
                load_complex.interactable = false;
                }
            }
            else
            {
                Debug.Log("ENTROOOOOOOOOOOOOOOOOOOOOOOOOOOOO");
                pdb_id_complex.color = Color.white;
                pdb_complex = pdb_file = dataUrl;
                Debug.Log(pdb_complex);
                pdb_custom_complex_name = pdb_complex.Substring(15, 9);
                pdb_id_complex.text = pdb_custom_complex_name;
                gm.pdb_custom_complex_name = pdb_custom_complex_name;
                StartCoroutine(upload_file());
                load_complex.interactable = true;
            }
        }

        if (is_pdb_1_loaded && is_pdb_2_loaded)
            load_1_2.interactable = true;
    }

    IEnumerator upload_file()
    {
        //GET THE ID OF EACH FILE
        string file_name = pdb_id == 0 ? string.Concat(pdb_custom_1_name, ".pdb") : pdb_id == 1 ? string.Concat(pdb_custom_2_name, ".pdb") : string.Concat(pdb_custom_complex_name, "_complex.pdb");
        byte[] file_pdb_bytes = System.Text.Encoding.UTF8.GetBytes(pdb_id == 0 ? pdb_1 : pdb_id == 1 ? pdb_2: pdb_complex);
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
        string pdb_temp = string.Concat("HEADER BIOBLOX ", pdb_custom_1_name, "-", pdb_custom_2_name);
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

    public void load_custom_complex()
    {
        GetComponent<GameManager>().Custom_complex_ChangeLevel();
    }

    public void test_write_custom_pdb()
    {
        StartCoroutine(save_user_file());
    }

    //public IEnumerator save_user_file()
    //{
    //    //using (WWW www = new WWW("http://13.58.210.151/bb_data/2P2P-TCTC.pdb"))
    //    //{
    //    //    yield return www;

    //    //    if (www.error != null)
    //    //        throw new System.Exception("WWW download had an error:" + www.error);

    //    //    pdb_file = www.text;
    //    //}
    //    //Debug.Log(pdb_file);

    //    bb = FindObjectOfType<BioBlox>();
    //    //create position/rotation
    //    string p1_position = bb.molecules[0].transform.localPosition.x.ToString("F3") + "," + bb.molecules[0].transform.localPosition.y.ToString("F3") + "," + bb.molecules[0].transform.localPosition.z.ToString("F3");
    //    string p2_position = bb.molecules[1].transform.localPosition.x.ToString("F3") + "," + bb.molecules[1].transform.localPosition.y.ToString("F3") + "," + bb.molecules[1].transform.localPosition.z.ToString("F3");
    //    string p1_rotation = bb.molecules[0].transform.eulerAngles.x.ToString("F3") + "," + bb.molecules[0].transform.eulerAngles.y.ToString("F3") + "," + bb.molecules[0].transform.eulerAngles.z.ToString("F3");
    //    string p2_rotation = bb.molecules[1].transform.eulerAngles.x.ToString("F3") + "," + bb.molecules[1].transform.eulerAngles.y.ToString("F3") + "," + bb.molecules[1].transform.eulerAngles.z.ToString("F3");

    //    string pdb_temp;
    //    using (StringReader reader = new StringReader(pdb_file))
    //    {
    //        string line;
    //        pdb_temp = pdb_file.Substring(0, 24);
    //        pdb_temp = string.Concat(pdb_temp, Environment.NewLine, "BIOBLOX POSITION ", p1_position, "/", p2_position);
    //        pdb_temp = string.Concat(pdb_temp, Environment.NewLine, "BIOBLOX ROTATION ", p1_rotation, "/", p2_rotation);
    //        while ((line = reader.ReadLine()) != null)
    //        {
    //            string kind = line.Substring(0, Mathf.Min(6, line.Length));
    //            if (kind == "ATOM  ")
    //            {
    //                pdb_temp = string.Concat(pdb_temp, Environment.NewLine, line);
    //            }
    //        }
    //    }
    //    yield return new WaitForEndOfFrame();

    //    //GET THE ID OF EACH FILE
    //    string file_name = string.Concat(gm.pdb_custom_1_name, "-", gm.pdb_custom_2_name, ".", DateTime.Now.Day, ".", DateTime.Now.Month, ".", DateTime.Now.Year, ".", DateTime.Now.Hour, ".", DateTime.Now.Minute, ".", DateTime.Now.Second, ".", DateTime.Now.Millisecond, ".pdb");
    //    byte[] file_pdb_bytes = System.Text.Encoding.UTF8.GetBytes(pdb_temp);
    //    WWWForm form = new WWWForm();
    //    form.AddField("file", "file");
    //    form.AddBinaryData("file", file_pdb_bytes, file_name);
    //    Debug.Log(file_name);
    //    WWW w = new WWW("http://13.58.210.151/upload_file.php", form);

    //    yield return w;

    //    Debug.Log(w.error);
    //    download_file(file_name);
    //}

    public void tests()
    {
        Vector3 caca = new Vector3(-9.391f,62.562f,28.773f);
        float tt = -5.2f;
        float tt2 = -005.2f;
        Debug.Log(tt2);
        Debug.Log(tt.ToString("000.000"));
        Debug.Log(tt2.ToString("000.000"));
        Debug.Log(((int)tt).ToString("D3F3"));

        Debug.Log(string.Concat(
            string.Concat((caca.x < 0 ? "-" : " "), Math.Abs(caca.x).ToString("000.000")),
            caca.y.ToString("000.000"),
            caca.z.ToString("000.000")
            )
        );
    }


    //FUCTNION TO LOAD ALL THE COORDINATE OF ATOMS
    public IEnumerator save_user_file()
    {
        //using (WWW www = new WWW("http://13.58.210.151/bb_data/2P2P-TCTC.pdb"))
        //{
        //    yield return www;

        //    if (www.error != null)
        //        throw new System.Exception("WWW download had an error:" + www.error);

        //    pdb_file = www.text;
        //}
        //Debug.Log(pdb_file);

        bb = FindObjectOfType<BioBlox>();
        int number_of_atoms_p0 = bb.molecules_PDB_mesh[0].return_number_of_atoms();
        int number_of_atoms_p1 = bb.molecules_PDB_mesh[1].return_number_of_atoms();

        int current_protein = 0;
        int atom_index = 0;
        string pdb_temp;
        using (StringReader reader = new StringReader(pdb_file))
        {
            string line;
            pdb_temp = pdb_file.Substring(0, 24);
            while ((line = reader.ReadLine()) != null)
            {
                string kind = line.Substring(0, Mathf.Min(6, line.Length));

                if (kind == "ATOM  ")
                {
                    pdb_temp = string.Concat(pdb_temp,
                        Environment.NewLine,
                        string.Concat(line.Substring(0, 30),
                        string.Concat(
                            string.Concat((bb.molecules_PDB_mesh[current_protein].GetAtomWorldPositon(atom_index).x < 0 ? " " : "-"), Math.Abs(bb.molecules_PDB_mesh[current_protein].GetAtomWorldPositon(atom_index).x).ToString("000.000")),
                            string.Concat((bb.molecules_PDB_mesh[current_protein].GetAtomWorldPositon(atom_index).y < 0 ? " " : "-"), Math.Abs(bb.molecules_PDB_mesh[current_protein].GetAtomWorldPositon(atom_index).y).ToString("000.000")),
                            string.Concat((bb.molecules_PDB_mesh[current_protein].GetAtomWorldPositon(atom_index).z < 0 ? " " : "-"), Math.Abs(bb.molecules_PDB_mesh[current_protein].GetAtomWorldPositon(atom_index).z).ToString("000.000"))
                            ),
                        line.Substring(54, line.Length - 54)));

                    atom_index++;
                    if (atom_index == number_of_atoms_p0)
                    {
                        atom_index = 0;
                        current_protein = 1;
                    }
                }
            }
        }
        yield return new WaitForEndOfFrame();

        //Debug.Log("procesando " + atom_index);
        //Debug.Log("cacacaaca");

        //GET THE ID OF EACH FILE
        string file_name = string.Concat(gm.pdb_custom_1_name, "-", gm.pdb_custom_2_name, ".", DateTime.Now.Day, ".", DateTime.Now.Month, ".", DateTime.Now.Year, ".", DateTime.Now.Hour, ".", DateTime.Now.Minute, ".", DateTime.Now.Second, ".", DateTime.Now.Millisecond, ".pdb");
        byte[] file_pdb_bytes = System.Text.Encoding.UTF8.GetBytes(pdb_temp);
        WWWForm form = new WWWForm();
        form.AddField("file", "file");
        form.AddBinaryData("file", file_pdb_bytes, file_name);
        Debug.Log(file_name);
        WWW w = new WWW("http://13.58.210.151/upload_file.php", form);

        yield return w;

        Debug.Log(w.error);
        download_file(file_name);
    }

}
