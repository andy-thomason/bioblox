using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PDBCustom : MonoBehaviour {

    public InputField pdb_id_input;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void check_pdb_id()
    {
        StartCoroutine(check_pdb_id_IE());
    }

    IEnumerator check_pdb_id_IE()
    {
        using (WWW www = new WWW("https://files.rcsb.org/view/" + pdb_id_input.text + ".pdb"))
        {
            yield return www;

            if (www.error != null)
                throw new System.Exception("WWW download had an error:" + www.error);
            else
                Debug.Log("exifste");
        }
    }
}
