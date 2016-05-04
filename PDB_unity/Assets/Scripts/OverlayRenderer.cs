using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

// overlay of atoms for AA selection and 
public class OverlayRenderer : MonoBehaviour {
	public struct Icon {
		public Vector3 centre;
		public float r;
		public Vector2 uv0;
		public Vector2 uv1;
        public Color32 colour;

		public Icon(Vector3 centre, float r, Vector2 uv0, Vector2 uv1, Color32 colour) {
            this.centre = centre;
            this.r = r;
			this.uv0 = uv0;
			this.uv1 = uv1;
            this.colour = colour;
		}
	}

	List<Icon> icons = new List<Icon>();

	public Camera lookat_camera;

	// Use this for initialization
	void Start () {
		MeshFilter mf = GetComponent<MeshFilter> ();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;
		mesh.MarkDynamic ();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        mr.material.renderQueue = 4000;
    }

    // Update is called once per frame
    void Update () {
        BioBlox bb = GameObject.FindObjectsOfType<BioBlox>()[0];
        if (bb)
        {
            clear();
            for (int i = 0; i != bb.molecules.Length; ++i)
            {
                GameObject obj = bb.molecules[i];
                PDB_mesh msh = obj.GetComponent<PDB_mesh>();
                PDB_molecule mol = msh.mol;
                Transform t = msh.transform;
                if (msh == null) return;
                if (mol == null) return;

                BitArray sel = new BitArray(mol.names.Length);
                foreach (int a in msh.selected_atoms) sel.Set(a, true);

                float c10 = Mathf.Cos(Time.time * 10.0f) * 0.5f + 0.5f;
                float c20 = Mathf.Cos(Time.time * 20.0f) * 0.5f + 0.5f;
                Color32 selected = new Color32(255, 255, (byte)(255.0f*c10), 255);
                Color32 touching = new Color32(128, (byte)(128 - 128.0f * c10), 128, 255);
                Color32 bad = new Color32((byte)(255.0f * c20), 0, 0, 255);

                for (int j = 0; j != mol.names.Length; ++j)
                {
                    bool is_selected = j < sel.Length && sel[j];
                    bool is_touching = bb.atoms_touching != null && bb.atoms_touching[i][j];
                    bool is_bad = bb.atoms_bad != null && bb.atoms_bad[i][j];
                    if (is_selected || is_touching || is_bad)
                    {
                        int name = mol.names[j];
                        int atom = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
                        int uvy = 3 - atom / 4;
                        int uvx = atom & 3;
                        add_Icon(
                            new Icon(
                                t.TransformPoint(mol.atom_centres[j]),
                                mol.atom_radii[j], new Vector2(uvx * 0.25f, (uvy + 1) * 0.25f), new Vector2((uvx + 1) * 0.25f, uvy * 0.25f),
                                is_bad ? bad : is_touching ? touching : selected
                            )
                        );
                    }
                }
            }
        }

        if (lookat_camera == null) {
			Debug.Log("warning: lookat_camera missing");
			return;
		}

		Vector3[] vertices = new Vector3[icons.Count * 4];
		Vector2[] uvs = new Vector2[icons.Count * 4];
        Color32[] colours = new Color32[icons.Count * 4];
        int[] indices = new int[icons.Count * 6];

		Vector3 camera_pos = lookat_camera.transform.position;
        Vector3 up = Vector3.up; // lookat_camera.transform.up;
        Vector3 right = Vector3.right; // lookat_camera.transform.right;
        float plane_distance = lookat_camera.nearClipPlane + 1.0f;
        for (int i = 0; i != icons.Count; ++i) {
            Icon icon = icons[i];
            Vector3 centre = lookat_camera.transform.InverseTransformPoint(icon.centre);
            float scale = plane_distance / centre.z;
            centre.x = centre.x * scale;
            centre.y = centre.y * scale;
            centre.z = plane_distance;
            float r = icon.r * scale;
            vertices[i*4+0] = centre + (  up - right) * r;
			vertices[i*4+1] = centre + (- up - right) * r;
            vertices[i*4+2] = centre + (- up + right) * r;
            vertices[i*4+3] = centre + (  up + right) * r;
            uvs[i*4+0] = new Vector2(icon.uv0.x, icon.uv0.y);
			uvs[i*4+1] = new Vector2(icon.uv0.x, icon.uv1.y);
			uvs[i*4+2] = new Vector2(icon.uv1.x, icon.uv1.y);
			uvs[i*4+3] = new Vector2(icon.uv1.x, icon.uv0.y);
            colours[i * 4 + 0] = icon.colour;
            colours[i * 4 + 1] = icon.colour;
            colours[i * 4 + 2] = icon.colour;
            colours[i * 4 + 3] = icon.colour;
            // 0 3
            // 1 2
            indices[i*6+0] = i*4+0;
			indices[i*6+1] = i*4+2;
			indices[i*6+2] = i*4+1;
			indices[i*6+3] = i*4+0;
			indices[i*6+4] = i*4+3;
			indices[i*6+5] = i*4+2;
		}

		MeshFilter mf = GetComponent<MeshFilter> ();
		Mesh mesh = mf.mesh;
		mesh.Clear ();
		mesh.vertices = vertices;
        mesh.colors32 = colours;
		mesh.uv = uvs;
		mesh.triangles = indices;
		mf.mesh = mesh;
	}

	public Icon add_Icon(Icon Icon) {
		icons.Add (Icon);
		return Icon;
	}

	public void clear() {
		icons.Clear ();
	}

	public void delete_Icon(Icon Icon) {
		icons.Remove (Icon);
	}
}
