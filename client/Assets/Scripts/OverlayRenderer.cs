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

    //public struct Atom
    //{
    //    public int atom_material;
    //    public int atom_id;
    //    public int protein_id;

    //    public Atom(int atom_material, int atom_id, int protein_id)
    //    {
    //        this.atom_material = atom_material;
    //        this.atom_id = atom_id;
    //        this.protein_id = protein_id;
    //    }
    //};

    List<Icon> icons = new List<Icon>();
    List<GameObject> icons_spheres = new List<GameObject>();
    List<GameObject> icons_spheres_store = new List<GameObject>();

    public Material Atom_1;
    public Material Atom_2;
    public Material Atom_selected;
    public Material Atom_selected_fp;
    public Material disabled_material;

    //Material material_to_use;
    UIController ui;

    public Camera lookat_camera;
    public GameObject Sphere_atom;
    //GameObject Sphere_atom_reference;
    public GameObject Sphere_atom_holder;
    BioBlox bb;
    SFX sfx;
    AminoSliderController asc;
    public Material[] P1atom_material;
    public Material[] P2atom_material;
    public Material[] P1atom_material_o;
    public Material[] P2atom_material_o;
    public Material overlaping;
    //List<Atom> p1_atomos = new List<Atom>();
    //List<Atom> p2_atomos = new List<Atom>();
    public bool atom_2d_overlay = false;
    public bool atom_3d_overlay = false;

    public int P1_selected_atom_id = -1;
    public int P2_selected_atom_id = -1;

    // Use this for initialization
    void Start ()
    {
        bb = GameObject.FindObjectsOfType<BioBlox>()[0];
        MeshFilter mf = GetComponent<MeshFilter> ();
        ui = FindObjectOfType<UIController>();
        sfx = FindObjectOfType<SFX>();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;
		mesh.MarkDynamic ();
        MeshRenderer mr = GetComponent<MeshRenderer>();
        asc = FindObjectOfType<AminoSliderController>();
        mr.material.renderQueue = 4000;
        get_spheres();
    }

    // Update is called once per frame
    void Update () {
        if (bb && bb.game_status == BioBlox.GameStatus.GameScreen && !ui.cutawayON)
        {
            clear_spheres();
            clear();

            int sphere_index = 0;
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

                float c10 = Mathf.Cos(Time.time * 5.0f) * 0.5f + 0.5f;
                float c20 = Mathf.Cos(Time.time * 20.0f) * 0.5f + 0.5f;
                Color32 selected = new Color32(128, 128, 0, 240);
                Color32 touching = new Color32(128, 128, 128, (byte)(63.0f*c10+192));
                Color32 bad = new Color32(255, 0, 0, (byte)(255.0f*c20));
                Color32 atom_color = new Color32(0, 0, 0, 255);
                for (int j = 0; j != mol.names.Length; ++j)
                {
                    bool is_selected = j < sel.Length && sel[j];
                    bool is_touching = bb.atoms_touching != null && bb.atoms_touching[i][j];
                    bool is_disabled = bb.atoms_disabled != null && bb.atoms_disabled[i][j];
                    bool is_bad = bb.atoms_bad != null && bb.atoms_bad[i][j];
                    bool is_atom_selected = false;
                    //is_bad = msh.selected_atoms[j] == asc.atom_selected_p1;
                    //Debug.Log("msh.selected_atoms[j]: " + msh.selected_atoms[j] + " == " + asc.atom_selected_p1 + " :asc.atom_selected_p1");
                    int name = mol.names[j];
                    int atom = name == PDB_molecule.atom_C ? 0 : name == PDB_molecule.atom_N ? 1 : name == PDB_molecule.atom_O ? 2 : name == PDB_molecule.atom_S ? 3 : 4;
                    int uvy = 3 - atom / 4;
                    int uvx = atom & 3;
                    //higjlight the atom seleted in expoert mode
                    if(ui.expert_mode && is_selected)
                    {
                        if (j == P1_selected_atom_id || j == P2_selected_atom_id)
                            is_atom_selected = true;
                    }

                    //2D OVERLAY
                    if ((is_touching || is_bad) && atom_2d_overlay)
                    {
                        add_Icon(
                            new Icon(
                                t.TransformPoint(mol.atom_centres[j]),
                                mol.atom_radii[j], new Vector2(uvx * 0.25f, (uvy + 1) * 0.25f), new Vector2((uvx + 1) * 0.25f, uvy * 0.25f),
                                is_bad ? bad : is_touching ? touching : selected
                            )
                        );
                    }
                    //show selected always and only in the protein which the camera is when first eprson
                    //HERE TO SYNC THE PDB WITH THE MESH - is_selected && 
                    if (is_selected)
                    {
                        if (!(ui.first_person && ui.first_person_protein != i))
                        {
                            if (sphere_index < icons_spheres_store.Count)
                            {
                                //spheres on transparent render protein 0
                                if (ui.DropDownP1.value != UIController.protein_render.normal.GetHashCode() && i == 0)
                                {
                                    //icons_spheres_store[sphere_index].GetComponent<Renderer>().material = Atom_selected;
                                    icons_spheres_store[sphere_index].transform.position = t.TransformPoint(mol.atom_centres[j]);
                                    add_Icon_sphere(icons_spheres_store[sphere_index]);
                                }
                                //spheres on transparent render protein 1
                                if (ui.DropDownP2.value != UIController.protein_render.normal.GetHashCode() && i == 1)
                                {
                                    //icons_spheres_store[sphere_index].GetComponent<Renderer>().material = Atom_selected;
                                    icons_spheres_store[sphere_index].transform.position = t.TransformPoint(mol.atom_centres[j]);
                                    add_Icon_sphere(icons_spheres_store[sphere_index]);
                                }
                                icons_spheres_store[sphere_index].GetComponent<Renderer>().material = P2atom_material[atom];
                                sphere_index++;
                            }
                            add_Icon(
                                new Icon(
                                    t.TransformPoint(mol.atom_centres[j]),
                                    mol.atom_radii[j], new Vector2(uvx * 0.25f, (6 + 1) * 0.25f), new Vector2((uvx + 1) * 0.25f, 6 * 0.25f), is_atom_selected ? atom_color : selected
                                )
                            );
                        }
                    }
                    else if (is_selected && ui.is_hovering)
                    {
                        if (sphere_index < icons_spheres_store.Count)
                        {
                            if (ui.DropDownP1.value != UIController.protein_render.normal.GetHashCode() || ui.DropDownP2.value != UIController.protein_render.normal.GetHashCode())
                            {
                                icons_spheres_store[sphere_index].GetComponent<Renderer>().material = P2atom_material[atom];
                                icons_spheres_store[sphere_index].transform.position = t.TransformPoint(mol.atom_centres[j]);
                                add_Icon_sphere(icons_spheres_store[sphere_index]);
                                sphere_index++;
                            }
                        }
                    }
                    else if (is_disabled)
                    {
                        if (sphere_index < icons_spheres_store.Count)
                        {
                            icons_spheres_store[sphere_index].GetComponent<Renderer>().material = disabled_material;
                            icons_spheres_store[sphere_index].transform.position = t.TransformPoint(mol.atom_centres[j]);
                            add_Icon_sphere(icons_spheres_store[sphere_index]);
                            sphere_index++;
                        }
                    }
                    //3D OVERLAY
                    if (is_touching && atom_3d_overlay)
                    {
                        if (sphere_index < icons_spheres_store.Count)
                        {
                            if (ui.DropDownP1.value != UIController.protein_render.normal.GetHashCode() && i == 0)
                            {
                                //material_to_use = i == 1 ? Atom_1 : Atom_2;
                                //icons_spheres_store[sphere_index].GetComponent<Renderer>().material = Atom_1;
                                icons_spheres_store[sphere_index].transform.position = t.TransformPoint(mol.atom_centres[j]);
                                add_Icon_sphere(icons_spheres_store[sphere_index]);
                            }
                            if (ui.DropDownP2.value != UIController.protein_render.normal.GetHashCode() && i == 1)
                            {
                                //material_to_use = i == 1 ? Atom_1 : Atom_2;
                                //icons_spheres_store[sphere_index].GetComponent<Renderer>().material = Atom_2;
                                icons_spheres_store[sphere_index].transform.position = t.TransformPoint(mol.atom_centres[j]);
                                add_Icon_sphere(icons_spheres_store[sphere_index]);
                            }

                            icons_spheres_store[sphere_index].GetComponent<Renderer>().material = i == 0 ? P1atom_material[atom] : P2atom_material[atom];

                            if (is_bad)
                                icons_spheres_store[sphere_index].GetComponent<Renderer>().material = i == 0 ? P1atom_material_o[atom] : P2atom_material_o[atom];
                            else
                                sphere_index++;
                        }
                    }
                }
            }

            //if (ui.p1_atom_status == UIController.p_atom_status_enum.find_atoms.GetHashCode())
            //{
            //    ui.P1CreateAtomButtons(p1_atomos);
            //    ui.p1_atom_status = UIController.p_atom_status_enum.done.GetHashCode();
            //}

            //if (ui.p2_atom_status == UIController.p_atom_status_enum.find_atoms.GetHashCode())
            //{
            //    ui.P2CreateAtomButtons(p2_atomos);
            //    ui.p2_atom_status = UIController.p_atom_status_enum.done.GetHashCode();
            //}

            //p1_atomos.Clear();
            //p2_atomos.Clear();
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

    //shperes

    public GameObject add_Icon_sphere(GameObject Icon)
    {
        icons_spheres.Add(Icon);
        return Icon;
    }

    public void clear_spheres()
    {
        foreach (GameObject sphere in icons_spheres) sphere.transform.position = new Vector3(1000.0f,-500.0f,0);
        icons_spheres.Clear();
    }

    public void delete_Icon_sphere(GameObject Icon)
    {
        icons_spheres.Remove(Icon);
    }

    void get_spheres()
    {
        foreach(Transform sphere_son in Sphere_atom_holder.transform)
        {
            icons_spheres_store.Add(sphere_son.gameObject);
        }
    }
}
