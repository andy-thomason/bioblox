using UnityEngine;
using System.Collections;

public class RayTraceTest : MonoBehaviour {

	public Vector3 centre = new Vector3 (1, 0, 0);
	public float radius = 5;
	public Texture2D my_tex;
	public Color32[] colors;

	// Use this for initialization
	void Start () {
		MeshRenderer mr = this.GetComponent<MeshRenderer> ();
	}

	public int num_bvh_db;
	
	// Update is called once per frame
	void Update () {
		MeshRenderer mr = this.GetComponent<MeshRenderer> ();

		GameObject obj1 = GameObject.Find("pdb2ptcWithTags.1");
		if (obj1 == null) return;

		//GameObject obj2 = GameObject.Find("pdb2ptcWithTags.2");
		PDB_mesh mesh1 = obj1.GetComponent<PDB_mesh> ();
		PDB_molecule mol1 = mesh1.mol;
		Transform t1 = obj1.transform;

		Vector3[] bvh_centres1 = mol1.bvh_centres;
		float[] bvh_radii1 = mol1.bvh_radii;
		int num_bvh = bvh_centres1.Length;
		num_bvh_db = num_bvh;
		mr.material.SetFloat ("_BVH_SCALE", 1.0f/num_bvh);

		my_tex = new Texture2D (2, num_bvh, TextureFormat.ARGB32, false, false);
		my_tex.name = "BVH";
		my_tex.wrapMode = TextureWrapMode.Clamp;
		my_tex.filterMode = FilterMode.Point;
		colors = my_tex.GetPixels32 ();
		for (int i = 0; i != num_bvh; ++i) {
			Vector3 centre = t1.TransformPoint(bvh_centres1[i]);
			float radius = bvh_radii1[i];
			int ix = Mathf.FloorToInt ((centre.x + 128) * 256);
			int iy = Mathf.FloorToInt ((centre.y + 128) * 256);
			int iz = Mathf.FloorToInt ((centre.z + 128) * 256);
			int ir = Mathf.FloorToInt ((radius + 128) * 256);
			//if (i == 0) Debug.Log("c="+centre+" r="+radius);
			colors [i*2+0] = new Color32((byte)(ix >> 8), (byte)(ix & 0xff), (byte)(iy >> 8), (byte)(iy & 0xff));
			colors [i*2+1] = new Color32((byte)(iz >> 8), (byte)(iz & 0xff), (byte)(ir >> 8), (byte)(ir & 0xff));
		}

		my_tex.SetPixels32(colors, 0);
		my_tex.Apply( false );
		mr.material.SetTexture ("_BVH", my_tex);
	}
}
