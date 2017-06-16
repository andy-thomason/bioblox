using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class PDBCustom : MonoBehaviour {

    public InputField pdb_id_input;
    public InputField pdb_id_input_chain_0;
    public InputField pdb_id_input_chain_1;
    public string pdb_url;
    public GameObject pdb_error;
    Stream stream;
    string file_pdb;

    // Use this for initialization
    void Start ()
    {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void check_pdb_id()
    {
        FindObjectOfType<SFX>().PlayTrack(SFX.sound_index.button_click);
        // NOTE: gameObject.name MUST BE UNIQUE!!!!
        GetFile.GetFileFromUserAsync(gameObject.name, "ReceivePDB");
        //StartCoroutine(check_pdb_id_IE());
    }

    //static string s_dataUrlPrefix = "data:image/png;base64,";
    public void ReceivePDB(string dataUrl)
    {
        file_pdb = dataUrl;
        //file_output.text = dataUrl;
        Debug.Log(file_pdb);
        StartCoroutine(upload_file());
    }

    IEnumerator upload_file()
    {
        byte[] file_pdb_bytes = System.Text.Encoding.UTF8.GetBytes(file_pdb);
        WWWForm form = new WWWForm();
        form.AddField("file", "file");
        form.AddBinaryData("file", file_pdb_bytes, "test.pdb");

        WWW w = new WWW("http://quiley.com/BB/upload_file.php", form);

        yield return w;

        Debug.Log(w.error);

        pdb_url = file_pdb;

        GetComponent<GameManager>().Custom_ChangeLevel();
    }


    //IEnumerator check_pdb_id_IE()
    //{
    //    using (WWW www = new WWW("https://files.rcsb.org/view/" + pdb_id_input.text + ".pdb"))
    //    {
    //        yield return www;

    //        if (www.error != null)
    //            pdb_error.SetActive(true);
    //        else
    //        {
    //            pdb_error.SetActive(false);
    //            pdb_url = "https://files.rcsb.org/view/" + pdb_id_input.text + ".pdb";

    //            GetComponent<GameManager>().Custom_ChangeLevel();

    //            //WWWForm www_form = new WWWForm();
    //            //www_form.AddField("url", pdb_url);
    //            //www_form.AddField("chain", pdb_id_input_chain_0.text);

    //            //WWW custom_www = new WWW(server_url, www_form);
    //            //yield return custom_www;

    //            //stream = new MemoryStream(www.bytes);
    //            //bb.PLYDecoder(stream, parent_molecule_reference.transform, 0, protein_view.normal);
    //        }
    //    }
    //}

//    IEnumerator Custom_DownloadMolecules()
//    {

//#if UNITY_WEBGL
//        string BundleURL = "https://bioblox3d.org/wp-content/themes/write/game_data/Asset/AssetBundlesWebGL/" + level.pdbFile.ToLower();
//#endif

//#if UNITY_STANDALONE
//        string BundleURL = "https://bioblox3d.org/wp-content/themes/write/game_data/Asset/AssetBundlesWindows/" + level.pdbFile.ToLower();
//#endif

//        // These filenames refer to the fbx in the asset bundle in the server
//        mol1_se_filename = level.pdbFile + "_" + level.chainsA + "_se_" + level.lod + ".bytes";
//        mol2_se_filename = level.pdbFile + "_" + level.chainsB + "_se_" + level.lod + ".bytes";
//        mol1_bs_filename = level.pdbFile + "_" + level.chainsA + "_bs_" + level.lod_bs + ".bytes";
//        mol2_bs_filename = level.pdbFile + "_" + level.chainsB + "_bs_" + level.lod_bs + ".bytes";
//        mol1_ca_filename = level.pdbFile + "_" + level.chainsA + "_ca_" + level.lod_bs + ".bytes";
//        mol2_ca_filename = level.pdbFile + "_" + level.chainsB + "_ca_" + level.lod_bs + ".bytes";
//        mol1_vr_filename = level.pdbFile + "_" + level.chainsA + "_vr_" + level.lod_vr + ".bytes";
//        mol2_vr_filename = level.pdbFile + "_" + level.chainsB + "_vr_" + level.lod_vr + ".bytes";

//        using (WWW www = new WWW(BundleURL))
//        {
//            yield return www;

//            if (www.error != null)
//                throw new System.Exception("WWW download had an error:" + www.error);

//            AssetBundle bundle = www.assetBundle;

//            mol1_se_filename_txt = bundle.LoadAsset(mol1_se_filename, typeof(TextAsset)) as TextAsset;
//            mol2_se_filename_txt = bundle.LoadAsset(mol2_se_filename, typeof(TextAsset)) as TextAsset;
//            mol1_bs_filename_txt = bundle.LoadAsset(mol1_bs_filename, typeof(TextAsset)) as TextAsset;
//            mol2_bs_filename_txt = bundle.LoadAsset(mol2_bs_filename, typeof(TextAsset)) as TextAsset;
//            mol1_ca_filename_txt = bundle.LoadAsset(mol1_ca_filename, typeof(TextAsset)) as TextAsset;
//            mol2_ca_filename_txt = bundle.LoadAsset(mol2_ca_filename, typeof(TextAsset)) as TextAsset;
//            mol1_vr_filename_txt = bundle.LoadAsset(mol1_vr_filename, typeof(TextAsset)) as TextAsset;
//            mol2_vr_filename_txt = bundle.LoadAsset(mol2_vr_filename, typeof(TextAsset)) as TextAsset;

//            bundle.Unload(false);
//        }

//        using (WWW www = new WWW("https://bioblox3d.org/wp-content/themes/write/game_data/pdb/" + level.pdbFile + ".txt"))
//        {
//            yield return www;

//            if (www.error != null)
//                throw new System.Exception("WWW download had an error:" + www.error);

//            pdb_file = www.text;
//        }


//        // Make two PDB_mesh instances from the PDB file and a chain selection.
//        mol1 = make_molecule(level.pdbFile + "." + level.chainsA, "Proto1", 7, MeshTopology.Triangles, 0);
//        mol1.transform.SetParent(Molecules);
//        mol2 = make_molecule(level.pdbFile + "." + level.chainsB, "Proto2", 7, MeshTopology.Triangles, 1);
//        mol2.transform.SetParent(Molecules);

//        //create holder of amino views
//        GameObject molecule_0_views = new GameObject();
//        molecule_0_views.name = "molecule_0";
//        molecule_0_views.transform.SetParent(mol1.transform);

//        //create holder of amino views
//        GameObject molecule_1_views = new GameObject();
//        molecule_1_views.name = "molecule_1";
//        molecule_1_views.transform.SetParent(mol2.transform);

//        //disabled atoms holder 1
//        GameObject temp_atom_disable_holder = new GameObject();
//        temp_atom_disable_holder.name = "holder_1";
//        temp_atom_disable_holder.transform.SetParent(mol1.transform);

//        //disabled atoms holder 2
//        temp_atom_disable_holder = new GameObject();
//        temp_atom_disable_holder.name = "holder_2";
//        temp_atom_disable_holder.transform.SetParent(mol2.transform);

//        molecules[0] = mol1.gameObject;
//        molecules[1] = mol2.gameObject;
//        molecules_PDB_mesh[0] = mol1.gameObject.GetComponent<PDB_mesh>();
//        molecules_PDB_mesh[1] = mol2.gameObject.GetComponent<PDB_mesh>();

//        // Set a bit in this bit array to disable an atom from collision
//        BitArray bad0 = new BitArray(molecules_PDB_mesh[0].mol.atom_centres.Length);
//        BitArray bad1 = new BitArray(molecules_PDB_mesh[1].mol.atom_centres.Length);
//        atoms_disabled = new BitArray[] { bad0, bad1 };

//        // Ioannis scoring
//        scoring = new PDB_score(molecules_PDB_mesh[0].mol, mol1.gameObject.transform, molecules_PDB_mesh[1].mol, mol2.gameObject.transform);

//        offset_position_0 = new Vector3(-molecules_PDB_mesh[0].mol.pos.x, -molecules_PDB_mesh[0].mol.pos.y, -molecules_PDB_mesh[0].mol.pos.z);
//        offset_position_1 = new Vector3(-molecules_PDB_mesh[1].mol.pos.x, -molecules_PDB_mesh[1].mol.pos.y, -molecules_PDB_mesh[1].mol.pos.z);

//        System.GC.Collect();

//        parent_molecule_reference = Instantiate(parent_molecule);
//        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsA + "_se_" + level.lod;
//        txt_bytes = mol1_se_filename_txt.bytes;
//        stream = new MemoryStream(txt_bytes);
//        PLYDecoder(stream, parent_molecule_reference.transform, 0, protein_view.normal);
//        transparency_0 = Instantiate(parent_molecule_reference);
//        parent_molecule_reference.transform.SetParent(molecule_0_views.transform);
//        parent_molecule_reference.SetActive(true);
//        //save the docking position/rotation of the protein 0
//        //docking_position_0 = parent_molecule_reference.transform.localPosition;
//        docking_rotation_0 = parent_molecule_reference.transform.localRotation;
//        parent_molecule_reference.transform.Translate(offset_position_0);

//        position_molecule_0 = parent_molecule_reference.transform.localPosition;

//        parent_molecule_reference.transform.localPosition = Vector3.zero;

//        //DEFAULT
//        parent_molecule_reference = Instantiate(parent_molecule);
//        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsB + "_se_" + level.lod;
//        txt_bytes = mol2_se_filename_txt.bytes;
//        stream = new MemoryStream(txt_bytes);
//        PLYDecoder(stream, parent_molecule_reference.transform, 1, protein_view.normal);
//        transparency_1 = Instantiate(parent_molecule_reference);
//        parent_molecule_reference.transform.SetParent(molecule_1_views.transform);
//        parent_molecule_reference.SetActive(true);
//        //save the docking position/rotation of the protein 0
//        //docking_position_1 = parent_molecule_reference.transform.localPosition;
//        docking_rotation_1 = parent_molecule_reference.transform.localRotation;
//        parent_molecule_reference.transform.Translate(offset_position_1);

//        position_molecule_1 = parent_molecule_reference.transform.localPosition;

//        parent_molecule_reference.transform.localPosition = Vector3.zero;

//        //BALLS AND STICK 1
//        parent_molecule_reference = Instantiate(parent_molecule);
//        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsA + "_bs_" + level.lod_bs;
//        txt_bytes = mol1_bs_filename_txt.bytes;
//        stream = new MemoryStream(txt_bytes);
//        PLYDecoder(stream, parent_molecule_reference.transform, 0, protein_view.bs);
//        parent_molecule_reference.transform.SetParent(molecule_0_views.transform);
//        parent_molecule_reference.SetActive(false);
//        //parent_molecule_reference.transform.Translate(offset_position_0);
//        parent_molecule_reference.transform.localPosition = Vector3.zero;

//        //BALLS AND STICK 2
//        parent_molecule_reference = Instantiate(parent_molecule);
//        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsB + "_bs_" + level.lod_bs;
//        txt_bytes = mol2_bs_filename_txt.bytes;
//        stream = new MemoryStream(txt_bytes);
//        PLYDecoder(stream, parent_molecule_reference.transform, 1, protein_view.bs);
//        parent_molecule_reference.transform.SetParent(molecule_1_views.transform);
//        parent_molecule_reference.SetActive(false);
//        //parent_molecule_reference.transform.Translate(offset_position_1);
//        parent_molecule_reference.transform.localPosition = Vector3.zero;

//        //C&A 1
//        parent_molecule_reference = Instantiate(parent_molecule);
//        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsA + "_ca_" + level.lod_bs;
//        txt_bytes = mol1_ca_filename_txt.bytes;
//        stream = new MemoryStream(txt_bytes);
//        PLYDecoder(stream, parent_molecule_reference.transform, 0, protein_view.normal);
//        parent_molecule_reference.transform.SetParent(molecule_0_views.transform);
//        parent_molecule_reference.SetActive(false);
//        //parent_molecule_reference.transform.Translate(offset_position_0);
//        parent_molecule_reference.transform.localPosition = Vector3.zero;

//        //C&A 2
//        parent_molecule_reference = Instantiate(parent_molecule);
//        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsB + "_ca_" + level.lod_bs;
//        txt_bytes = mol2_ca_filename_txt.bytes;
//        stream = new MemoryStream(txt_bytes);
//        PLYDecoder(stream, parent_molecule_reference.transform, 1, protein_view.normal);
//        parent_molecule_reference.transform.SetParent(molecule_1_views.transform);
//        parent_molecule_reference.SetActive(false);
//        //parent_molecule_reference.transform.Translate(offset_position_1);
//        parent_molecule_reference.transform.localPosition = Vector3.zero;

//        //HELADO 1
//        parent_molecule_reference = Instantiate(parent_molecule);
//        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsA + "_vr_" + level.lod_vr;
//        txt_bytes = mol1_vr_filename_txt.bytes;
//        stream = new MemoryStream(txt_bytes);
//        PLYDecoder(stream, parent_molecule_reference.transform, 0, protein_view.normal);
//        parent_molecule_reference.transform.SetParent(molecule_0_views.transform);
//        parent_molecule_reference.SetActive(false);
//        //parent_molecule_reference.transform.Translate(offset_position_0);
//        parent_molecule_reference.transform.localPosition = Vector3.zero;

//        //HELADO 2
//        parent_molecule_reference = Instantiate(parent_molecule);
//        parent_molecule_reference.name = level.pdbFile + "_" + level.chainsB + "_vr_" + level.lod_vr;
//        txt_bytes = mol2_vr_filename_txt.bytes;
//        stream = new MemoryStream(txt_bytes);
//        PLYDecoder(stream, parent_molecule_reference.transform, 1, protein_view.normal);
//        parent_molecule_reference.transform.SetParent(molecule_1_views.transform);
//        parent_molecule_reference.SetActive(false);
//        //parent_molecule_reference.transform.Translate(offset_position_1);
//        parent_molecule_reference.transform.localPosition = Vector3.zero;

//        //TRANSPARENT 1
//        transparency_0.transform.SetParent(molecule_0_views.transform);
//        // mol1_mesh.name = "transparent_p1";
//        FixTransparentMolecule(transparency_0, 0);
//        transparency_0.SetActive(false);
//        //transparency_0.transform.Translate(offset_position_0);
//        transparency_0.transform.localPosition = Vector3.zero;

//        //TRANSPARENT 2
//        transparency_1.transform.SetParent(molecule_1_views.transform);
//        // mol1_mesh.name = "transparent_p1";
//        FixTransparentMolecule(transparency_1, 1);
//        transparency_1.SetActive(false);
//        //transparency_1.transform.Translate(offset_position_1);
//        transparency_1.transform.localPosition = Vector3.zero;

//        Vector3 xoff = new Vector3(level.separation, 0, 0);

//        reset_molecule(molecules[0], 0, level.offset - xoff);
//        reset_molecule(molecules[1], 1, level.offset + xoff);

//        //setting the position of each molecule
//        molecule_0_views.transform.localPosition = position_molecule_0;
//        molecule_1_views.transform.localPosition = position_molecule_1;

//        /*foreach (Vector3 c in molecules_PDB_mesh[0].mol.atom_centres) {
//          GameObject go = new GameObject();
//          go.transform.SetParent(mol1.transform);
//          MeshFilter mf = go.AddComponent<MeshFilter>();
//          //mf.mesh = 
//          MeshRenderer mr = go.AddComponent<MeshRenderer>();
//          go.transform.position = c;
//        }*/

//        //uiController.SetHintImage(level.pdbFile); //HINT

//        StartGame();

//        //create_mesh_1();
//        //create_mesh_11();
//        //create_mesh2();
//        //StartGame();
//    }
}
