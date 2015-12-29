using UnityEngine;
using System.Collections;

public class CamScript : MonoBehaviour {
    public static bool useWireframe = false;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    // Attach this script to a camera, this will make it render in wireframe
    void OnPreRender() {
        GL.wireframe = useWireframe;
    }
    void OnPostRender() {
        GL.wireframe = false;
    }
}
