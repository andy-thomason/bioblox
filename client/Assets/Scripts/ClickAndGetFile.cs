using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;

public class ClickAndGetFile : MonoBehaviour
{
    string file_pdb;

    public void LoadFile()
    {
        // NOTE: gameObject.name MUST BE UNIQUE!!!!
        GetFile.GetFileFromUserAsync(gameObject.name, "ReceivePDB");
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

        WWW w = new WWW("http://bioblox3d.org/test/upload_file.php", form);

        yield return w;

        Debug.Log(w.error);
    }
}