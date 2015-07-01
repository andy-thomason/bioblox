using UnityEngine;
using System.Collections;

public class RayTraceTest : MonoBehaviour {

	public Vector3 centre = new Vector3 (1, 0, 0);
	public float radius = 5;
	public Texture2D my_tex;
	public Color32[] colors = new Color32[2 * 1];
	public Color32[] colors1 = new Color32[1 * 1];

	// Use this for initialization
	void Start () {
		MeshRenderer mr = this.GetComponent<MeshRenderer> ();
		my_tex = new Texture2D (2, 1, TextureFormat.ARGB32, false, false);
		my_tex.name = "BVH";
		my_tex.wrapMode = TextureWrapMode.Clamp;
		my_tex.filterMode = FilterMode.Point;
		my_tex.SetPixels32(colors, 0);
		my_tex.Apply( false );
		mr.material.SetTexture ("_BVH", my_tex);
	}
	
	// Update is called once per frame
	void Update () {
		MeshRenderer mr = this.GetComponent<MeshRenderer> ();
		int ix = Mathf.FloorToInt ((centre.x + 128) * 256);
		int iy = Mathf.FloorToInt ((centre.y + 128) * 256);
		int iz = Mathf.FloorToInt ((centre.z + 128) * 256);
		int ir = Mathf.FloorToInt ((radius + 128) * 256);

		colors [0].r = (byte)(ix >> 8);
		colors [0].g = (byte)(ix & 0xff);
		colors [0].b = (byte)(iy >> 8);
		colors [0].a = (byte)(iy & 0xff);
		colors [1].r = (byte)(iz >> 8);
		colors [1].g = (byte)(iz & 0xff);
		colors [1].b = (byte)(ir >> 8);
		colors [1].a = (byte)(ir & 0xff);

		mr.material.SetColor ("_CullPos", new Color (centre.x, 0, 0, 0));
		my_tex.SetPixels32(colors, 0);
		my_tex.Apply( false );
	}
}
