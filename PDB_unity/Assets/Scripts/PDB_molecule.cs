using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using AssemblyCSharp;

/// Class containing the description of a single molecule.
public class PDB_molecule
{
	// the molecule data itself.
    public Vector3[] atom_centres;
    public float[] atom_radii;
	public Color[] atom_colours;

	public Tuple<int,int>[] pairedLabels=new Tuple<int, int>[0];
    public int[] names;
    public int[] residues;
    public int[] N_atoms;
    public Vector3 pos;
	public List<string> aminoAcidsNames;
	public List<int[]> aminoAcidsAtomIds;
	public Mesh[] mesh;

	// bounding volume heirachy to accelerate collisions
    public Vector3[] bvh_centres;
    public float[] bvh_radii;
    public int[] bvh_terminals;


	public PDB_molecule.Label[] labels = new PDB_molecule.Label[0];
	public Tuple<int,int>[] spring_pairs = new Tuple<int,int>[0];
	public int[] serial_to_atom;

    //const float c = 1.618033988749895f;
    const float e = 0.52573111f;
    const float d = 0.8506508f;
    // icosphere (http://en.wikipedia.org/wiki/Icosahedron#Cartesian_coordinates)
    readonly float[] vproto = {-e,d,0,e,d,0,-e,-d,0,e,-d,0,0,-e,d,0,e,d,0,-e,-d,0,e,-d,d,0,-e,d,0,e,-d,0,-e,-d,0,e};
	readonly int[] iproto = {0,11,5,0,5,1,0,1,7,0,7,10,0,10,11,1,5,9,5,11,4,11,10,2,10,7,6,7,1,8,3,9,4,3,4,2,3,2,6,3,6,8,3,8,9,4,9,5,2,4,11,6,2,10,8,6,7,9,8,1};
    Vector3 [] vsphere;
    int [] isphere;

	Vector3[] aoccRays = {
		new Vector3 (0, 1, 0),
		new Vector3 (0, -1, 0),
		new Vector3 (1, 0, 0),
		new Vector3 (-1, 0, 0),
		new Vector3 (0, 0, 1),
		new Vector3 (0, 0, -1),
    };

	public enum Mode { Ball, Ribbon, Metasphere };
	public static Mode mode = Mode.Metasphere;
    
    public string name;


	public struct Label
	{
		public Label(int id){
			uniqueLabelID = id;
			atomIds = new List<int>();
		}
		public int uniqueLabelID;
		public List<int> atomIds;
	}
    
    //static System.IO.StreamWriter debug = new System.IO.StreamWriter(@"C:\tmp\PDB_molecule.csv");
    
    Vector3 get_v(int i) { return new Vector3(vproto[i*3+0], vproto[i*3+1], vproto[i*3+2]); }


	List<int> GetAminoIndexes(string aminoName)
	{
		List<int> returnList = new List<int> ();
		for(int i = 0; i < aminoAcidsNames.Count; ++i)
		{
			if(aminoName == aminoAcidsNames[i])
			{
				returnList.Add(i);
			}
		}
		return returnList;
	}

    void build_sphere() {
        int num_tris = iproto.Length / 3;
        int num_verts = vproto.Length / 3;
        vsphere = new Vector3[num_verts + num_tris];
        isphere = new int[num_tris * 9];
        for (int i = 0; i != num_verts; ++i) {
            vsphere[i].x = vproto[i*3+0];
            vsphere[i].y = vproto[i*3+1];
            vsphere[i].z = vproto[i*3+2];
			vsphere[i] = vsphere[i].normalized;
        }
        for (int i = 0; i != num_tris; ++i) {
            int i0 = iproto[i*3+0];
            int i1 = iproto[i*3+1];
            int i2 = iproto[i*3+2];
            Vector3 p0 = get_v(i0);
            Vector3 p1 = get_v(i1);
            Vector3 p2 = get_v(i2);

            /*for (int ty = 0; ty <= 3; ++ty)
            {
                Vector3 q0 = Vector3.Lerp(p0, p1, ty / 3.0f);
                Vector3 q1 = Vector3.Lerp(p0, p2, ty / 3.0f);
                for (int tx = 0; tx <= ty; ++tx)
                {
                    vsphere[idx++] = Vector3.Lerp(q0, q1, tx == 0 ? 0.0f : tx / (float)ty);
                }
            }*/
            int i3 = num_verts + i;
            vsphere[i3] = (p0 + p1 + p2).normalized;
            //    i0
            //    i3
            // i2    i1
            isphere[i*9+0] = i0;
            isphere[i*9+1] = i1;
            isphere[i*9+2] = i3;
            isphere[i*9+3] = i1;
            isphere[i*9+4] = i2;
            isphere[i*9+5] = i3;
            isphere[i*9+6] = i2;
            isphere[i*9+7] = i0;
            isphere[i*9+8] = i3;
        }
        //debug.WriteLine(isphere.Length);
    }

	float CalcAmbientOcclusion(Vector3 pos, Vector3 norm, float radius)
	{
		float occlusion = 0;
		float maxDist = 7;
		for(int i=0;i<aoccRays.Length;++i)
		{
			Vector3 posOffset = pos + aoccRays[i] * radius;
			Ray r = new Ray (posOffset, aoccRays[i]);
			BvhRayCollider b = new BvhRayCollider (this, r);
			float minDist = maxDist;
			for (int j=0; j< b.results.Count; ++j) {
				float dist=(pos-atom_centres[b.results[j].index]).sqrMagnitude;
				if(dist<maxDist&&dist!=0)
				{
					minDist=Math.Min(minDist,dist);
				}
			}
		occlusion += (1 - (minDist / maxDist));
		}
		return (1 - ((occlusion) / aoccRays.Length));
	}

    // https://en.wikipedia.org/wiki/Marching_cubes
    void build_metasphere_mesh(out Vector3[] vertices, out Vector3[] normals, out Vector2[] uvs, out Color[] colours, out int[] indices) {
		const float grid_spacing = 0.5f;
		const float rgs = 1.0f / grid_spacing;

		// Create a 3D array for each molecule
		Vector3 min = atom_centres[0];
		Vector3 max = min;
		float max_r = 0.0f;
		for (int j = 0; j != atom_centres.Length; ++j)
		{
			//Vector3 r = new Vector3(atom_radii[j], atom_radii[j], atom_radii[j]) + 1.0f;
			float r = atom_radii[j];
			max_r = Mathf.Max(max_r, r);
			min = Vector3.Min(min, atom_centres[j]);
			max = Vector3.Max(max, atom_centres[j]);
		}

		int irgs = Mathf.CeilToInt (max_r * rgs) + 1;
		int x0 = Mathf.FloorToInt (min.x * rgs) - irgs;
		int y0 = Mathf.FloorToInt (min.y * rgs) - irgs;
		int z0 = Mathf.FloorToInt (min.z * rgs) - irgs;
		int x1 = Mathf.CeilToInt (max.x * rgs) + irgs;
		int y1 = Mathf.CeilToInt (max.y * rgs) + irgs;
		int z1 = Mathf.CeilToInt (max.z * rgs) + irgs;

		int xdim = x1-x0+1, ydim = y1-y0+1, zdim = z1-z0+1;

		// Array contains values (-0.5..>1) positive means inside molecule.
		// Normals are drived from values.
		// Colours shaded by ambient occlusion (TODO). 
		float[] mc_values = new float[xdim * ydim * zdim];
		Vector3[] mc_normals = new Vector3[xdim * ydim * zdim];
		Color[] mc_colours = new Color[xdim * ydim * zdim];
		float diff = 0.125f, rec = 2.0f / diff;
		for (int i = 0; i != xdim * ydim * zdim; ++i) {
			mc_values[i] = -0.5f;
			mc_colours[i] = new Color(1, 1, 1, 1);
		}

		// For each atom add in the values and normals surrounding
		// the centre up to a reasonable radius.
		int acmax = atom_centres.Length;
		DateTime start_time = DateTime.Now;
		for (int ac = 0; ac != acmax; ++ac) {
			Vector3 c = atom_centres[ac];
			float r = atom_radii[ac] * 0.8f * rgs;
			Color colour = atom_colours[ac];
			float cx = c.x * rgs;
			float cy = c.y * rgs;
			float cz = c.z * rgs;

			// define a box around the atom.
			int cix = Mathf.FloorToInt(c.x * rgs);
			int ciy = Mathf.FloorToInt(c.y * rgs);
			int ciz = Mathf.FloorToInt(c.z * rgs);
			int xmin = Mathf.Max(x0, cix-irgs);
			int ymin = Mathf.Max(y0, ciy-irgs);
			int zmin = Mathf.Max(z0, ciz-irgs);
			int xmax = Mathf.Max(x1, cix+irgs);
			int ymax = Mathf.Max(y1, ciy+irgs);
			int zmax = Mathf.Max(z1, ciz+irgs);
			float fk = Mathf.Log(0.5f) / (r * r);
			float fkk = -fk * 0.5f; // 0.5 is a magic number!
			bool wcol = colour != Color.white;

			for (int z = zmin; z != zmax; ++z) {
				float fdz = z - cz;
				for (int y = ymin; y != ymax; ++y) {
					float fdy = y - cy;
					int idx = ((z-z0) * ydim + (y-y0)) * xdim + (xmin-x0);
					float d2_base = fdy*fdy + fdz*fdz;
					for (int x = xmin; x != xmax; ++x) {
						float fdx = x - cx;
						float d2 = fdx*fdx + d2_base;
						float v = fkk * d2;
						if (v < 1) {
							// a little like exp(-v)
							float val = (2 * v - 3) * v * v + 1;
							mc_values[idx] += val;
							float rcp = val / Mathf.Sqrt(d2);
							mc_normals[idx].x += fdx * rcp;
							mc_normals[idx].y += fdy * rcp;
							mc_normals[idx].z += fdz * rcp;
							if (wcol) mc_colours[idx] = colour;
						}
						idx++;
					}
				}
			}
		}
		Debug.Log (DateTime.Now - start_time + "s to make values");

		//MarchingCubes(int x0, int y0, int z0, int xdim, int ydim, int zdim, float grid_spacing, float[] mc_values, Vector3[] mc_normals, Color[] mc_colours) {
		MarchingCubes mc = new MarchingCubes(x0, y0, z0, xdim, ydim, zdim, grid_spacing, mc_values, mc_normals, mc_colours);
		start_time = DateTime.Now;

		vertices = mc.vertices;
		normals = mc.normals;
		uvs = mc.uvs;
		colours = mc.colours;
		indices = mc.indices;
	}
	
	void build_ball_mesh(out Vector3[] vertices,out Vector3[] normals,out Vector2[] uvs,out Color[] colors,out int[] indices) {
        //debug.WriteLine("building mesh");
 
        int num_atoms = atom_centres.Length;
        int vlen = vsphere.Length;
        int ilen = isphere.Length;
        vertices = new Vector3[vlen*num_atoms];
       	normals = new Vector3[vlen*num_atoms];
        uvs = new Vector2[vlen*num_atoms];
		colors = new Color[vlen * num_atoms];
        indices = new int[ilen*num_atoms];
        int v = 0;
        int idx = 0;
        for (int j = 0; j != num_atoms; ++j) {
            Vector3 pos = atom_centres[j];
			/*Color col=new 
				Color(0.1f,0.1f,0.1f,
			    CalcAmbientOcclusion(pos,pos.normalized,atom_radii[j]));
			*/
			Color col = atom_colours[j];
            //if (j < 10) debug.WriteLine(pos);
            float r = atom_radii[j];
            for (int i = 0; i != vlen; ++i) {
                vertices[v] = vsphere[i]*r + pos;
                normals[v] = vsphere[i];
                uvs[v].x = normals[v].x;
                uvs[v].y = normals[v].y;
				colors[v] = col;
                ++v;
            }
            for (int i = 0; i != ilen; ++i) {
                indices[idx++] = isphere[i] + vlen*j;
            }
        }


        //mesh.RecalculateNormals();
    }

	public Mesh build_section_mesh(int[] atom_indicies)
	{
		Mesh meshy = new Mesh ();
		meshy.Clear ();

		int vlen = vsphere.Length;
		int ilen = isphere.Length;

		Vector3[] verts = new Vector3[atom_indicies.Length * vlen];
		Vector3[] normals = new Vector3[atom_indicies.Length * vlen];
		Vector2[] uvs = new Vector2[atom_indicies.Length * vlen];
		Color[] colors = new Color[atom_indicies.Length * vlen];
		int[] indices = new int[atom_indicies.Length * ilen];

		Vector3 offset = new Vector3 ();
		for(int i=0;i<atom_indicies.Length;++i)
		{
			offset+=atom_centres[atom_indicies[i]];
		}
		offset /= atom_indicies.Length;

		int idx = 0;
		for (int i=0; i<atom_indicies.Length; ++i) {
			int index = atom_indicies[i];
			Vector3 pos = atom_centres[index];
			Color col= Color.green;
			float rad = atom_radii[index];

			for(int j=0; j!= vlen; ++j)
			{
				int v=j+i*vlen;
				verts[v] = vsphere[j]*rad + pos - offset;
				normals[v] = vsphere[j];
				uvs[v].x = normals[v].x;
				uvs[v].y = normals[v].y;
				colors[v] = col;
			}
			for (int j = 0; j != ilen; ++j) {
				indices[idx++] = isphere[j]+ i*vlen;
			}
		}

		meshy.vertices = verts;
		meshy.normals = normals;
		meshy.uv = uvs;
		meshy.colors = colors;
		meshy.triangles = indices;

		return meshy;
	}
	
	void construct_unity_meshes (Vector3[] vertices, Vector3[] normals, Vector2[] uvs, Color[] colors, int[] indices)
	{
		int numMesh = 1;

		while ((atom_centres.Length*vsphere.Length / numMesh) > 65000) {
			numMesh += 1;
		}

		List<Mesh> meshes = new List<Mesh> ();

		int indexOffset = 0;
		while(indexOffset < indices.Length) {
			int[] vertexCounter = new int[vertices.Length];
			List<Vector3> vtx = new List<Vector3>();
			List<Vector3> nrm = new List<Vector3>();
			List<Vector2> uv = new List<Vector2>();
			List<Color> col = new List<Color>();
			List<int> idx = new List<int>();

			for(int i = 0; i < vertexCounter.Length; ++i)
			{
				vertexCounter[i] = -1;
			}

			int numVertices = 0;
			for (int i = indexOffset; i < indices.Length; indexOffset=(i+=3)) {

				int index1 = indices [i];
				int index2 = indices [i + 1];
				int index3 = indices [i + 2];

				if (vertexCounter [index1]==-1) {
					vtx.Add(vertices[index1]);
					nrm.Add(normals[index1]);
					uv.Add(uvs[index1]);
					col.Add(colors[index1]);

					vertexCounter[index1] = vtx.Count-1;
					index1 = vertexCounter[index1];
					numVertices += 1;
				}
				else
				{
					index1 = vertexCounter[index1];
				}

				if (vertexCounter [index2]==-1) {
					vtx.Add(vertices[index2]);
					nrm.Add(normals[index2]);
					uv.Add(uvs[index2]);
					col.Add(colors[index2]);
					
					vertexCounter[index2] = vtx.Count-1;
					index2 = vertexCounter[index2];
					numVertices += 1;
				}
				else
				{
					index2 = vertexCounter[index2];
				}

				if (vertexCounter [index3]==-1) {
					vtx.Add(vertices[index3]);
					nrm.Add(normals[index3]);
					uv.Add(uvs[index3]);
					col.Add(colors[index3]);
					
					vertexCounter[index3] = vtx.Count-1;
					index3 = vertexCounter[index3];
					numVertices += 1;
				}
				else
				{
					index3 = vertexCounter[index3];
				}
				idx.Add(index1);
				idx.Add(index2);
				idx.Add(index3);

				if (numVertices >= 64996) {
					break;
				}
			}
			Mesh m = new Mesh();
			m.Clear();
			m.name = "Mesh" + meshes.Count;
			m.vertices = vtx.ToArray();
			m.normals = nrm.ToArray();
			m.colors = col.ToArray();
			m.uv = uv.ToArray();
			m.triangles = idx.ToArray();
			meshes.Add(m);
		}
		mesh = meshes.ToArray ();
	}

    static public int encode(char a, char b, char c, char d) {
        return a*0x1000000+b*0x10000+c*0x100+d;
    }

    public static int atom_N = encode(' ', 'N', ' ', ' ');
    public static int atom_O = encode(' ', 'O', ' ', ' ');
    public static int atom_C = encode(' ', 'C', ' ', ' ');

    void build_ribbon_mesh() {
		/*
        debug.WriteLine("building mesh");
        mesh = new Mesh();
        mesh.name = "ribbon view";

        int num_residues = residues.Length;
        int segments = 6;

        Vector3[] vertices = new Vector3[(num_residues+1)*segments];
        Vector3[] normals = new Vector3[(num_residues+1)*segments];
        Vector2[] uvs = new Vector2[(num_residues+1)*segments];
        int[] indices = new int[num_residues*segments*2];
        int v = 0;
        int idx = 0;
        Vector3 a = new Vector3();
        Vector3 b = new Vector3();
        for (int j = 0; j != num_residues; ++j) {
            int r0 = residues[j+0];
            int r1 = residues[j+1];
            a.Set(atoms[r0*4+0], atoms[r0*4+1], atoms[r0*4+2]);
            b.Set(atoms[r1*4+0], atoms[r1*4+1], atoms[r1*4+2]);
            float r = atoms[j*4+3];
            for (int i = 0; i != vlen; ++i) {
                vertices[v] = vsphere[i]*r + pos;
                normals[v] = vsphere[i];
                uvs[v].x = normals[v].x;
                uvs[v].y = normals[v].y;
                ++v;
            }
            for (int i = 0; i != ilen; ++i) {
                indices[idx++] = isphere[i] + vlen*j;
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = indices;
        //mesh.RecalculateNormals();
        */
    }

    public class Sorter : IComparer  {
        Vector3 axis;
        Vector3[] atom_centres;

        public Sorter(Vector3 dim, Vector3[] atoms)
        {
            atom_centres = atoms;
            if (dim.x >= dim.y && dim.x >= dim.z)
            {
                axis = new Vector4(1, 0, 0);
            } else if (dim.y >= dim.x && dim.y >= dim.z)
            {
                axis = new Vector4(0, 1, 0);
            } else
            {
                axis = new Vector4(0, 0, 1);
            }
        }

        public int Compare(object x, object y) {
            float a = Vector3.Dot(atom_centres[(int)x], axis);
            float b = Vector3.Dot(atom_centres[(int)y], axis);
            return a < b ? -1 : a > b ? 1 : 0;
        }
    }

    private void partition(int[] index, int b, int e, int addr) {
        if (e == b + 1) {
			int bi = index[b];
			Vector3 centre = atom_centres[bi];
			float radius = atom_radii[bi];
            //debug.WriteLine("[" + addr + "] [T] centre=" + centre + " r=" + radius);
            bvh_terminals[addr] = bi;
            bvh_centres[addr] = centre;
            bvh_radii[addr] = radius;
        }
        else
        {
            Vector3 min = atom_centres[b];
            Vector3 max = min;
            for (int j = b; j != e; ++j)
            {
				int ji = index[j];
                Vector3 r = new Vector3(atom_radii[ji], atom_radii[ji], atom_radii[ji]);
                min = Vector3.Min(min, atom_centres[ji] - r);
                max = Vector3.Max(max, atom_centres[ji] + r);
            }

            Vector3 dim = max - min;
            Vector3 centre = (max + min) * 0.5f;

            Array.Sort(index, b, e-b, new Sorter(dim, atom_centres));

            int mid = b + (e-b)/2;

            float radius = 0;
            for (int j = b; j != e; ++j)
            {
				int ji = index[j];
                Vector3 pos = atom_centres[ji] - centre;
                float r = atom_radii[ji] + pos.magnitude;
                radius = Mathf.Max(radius, r);
            }

            //if (tree.Count < 30) debug.WriteLine("" + (e - b) + " r=" + radius);

            //debug.WriteLine("[" + addr + "] [N] centre=" + centre + " r=" + radius);
            bvh_centres[addr] = centre;
            bvh_radii[addr] = radius;
            bvh_terminals[addr] = -1;
            partition(index, b, mid, addr * 2 + 1);
            partition(index, mid, e, addr * 2 + 2);
        }
    }

    public void build_bvh() {
        int num_atoms = atom_centres.Length;
        int[] index = new int[num_atoms];
        for (int j = 0; j != num_atoms; ++j) {
            index[j] = j;
        }

        int num_bvh = 1;
        while (num_bvh < num_atoms*2) num_bvh *= 2;

        //debug.WriteLine("building bvh for " + name + "with " + num_atoms + " atoms and " + num_bvh + " bvhs");



        bvh_centres = new Vector3[num_bvh];
        bvh_radii = new float[num_bvh];
        bvh_terminals = new int[num_bvh];

        partition(index, 0, num_atoms, 0);
		/*
		for (int i=0; i<num_bvh; ++i) {
			debug.WriteLine("B,"+i+",\""+ bvh_centres[i]+"\","+bvh_terminals[i]+","+bvh_radii[i]);
		}
		for(int i=0; i<num_atoms; ++i)
		{
			debug.WriteLine("A,"+i+",\""+ atom_centres[i]+"\",,"+atom_radii[i]);
		}
        debug.Flush();*/
    }

    public void build_mesh() {
        if (vsphere == null) {
            build_sphere();
        }
        build_bvh();

		Vector3[] verts = new Vector3[0];
		Vector3[] normals = new Vector3[0];
		Vector2[] uvs = new Vector2[0];
		Color[] color = new Color[0];
		int[] index = new int[0];
		if (mode == Mode.Ball) {
			build_ball_mesh (out verts, out normals, out uvs, out color, out index);
			construct_unity_meshes (verts, normals, uvs, color, index);
		} else if (mode == Mode.Metasphere) {
			build_metasphere_mesh(out verts,out normals,out uvs,out color,out index);
			construct_unity_meshes (verts, normals, uvs, color, index);
			/*
			mesh = new Mesh[1];
			mesh [0]= new Mesh();
			mesh [0].Clear ();
			mesh [0].name = "metasphere view" + 0;
			mesh [0].vertices = verts;
			mesh [0].normals = normals;
			mesh [0].uv = uvs;
            mesh [0].colors = color;
			mesh [0].triangles = index;
			*/
		} else if (mode == Mode.Ribbon) {
            build_ribbon_mesh();
        }

    }

	static public int collide_ray(
		GameObject obj, PDB_molecule mol, Transform t,
		Ray ray)
	{
		BvhRayCollider b = new BvhRayCollider (mol, t, ray);
		int closestIndex = -1;
		float closestDistance = float.MaxValue;
		for (int i=0; i<b.results.Count; i++) {
			BvhRayCollider.Result result = b.results[i];
			Vector3 c = t.TransformPoint(mol.atom_centres[result.index]);
			//float dist = (ray.origin-c).sqrMagnitude;
			float dist = Vector3.Dot(c, ray.direction);
			if (closestDistance > dist)
			{
				closestDistance = dist;
				closestIndex = result.index;
			}
		}
		return closestIndex;
	}

	static public bool collide_ray_quick(
		GameObject obj, PDB_molecule mol, Transform t,
		Ray ray)
	{
		
		Vector3 c = mol.bvh_centres[0];
		float r = mol.bvh_radii[0];

		c = t.TransformPoint (c);
		
		Vector3 q = c - ray.origin;
		float f = Vector3.Dot (q, ray.direction);
		
		float d = 0;
		
		if (f < 0) {
			d = q.sqrMagnitude;
		} else {
			d=(c-(ray.origin+(ray.direction*f))).sqrMagnitude;
		}
		
		if (d < r * r) {
			return true;
		}
		return false;
	}

	static public bool collide(
		GameObject obj0, PDB_molecule mol0, Transform t0,
		GameObject obj1, PDB_molecule mol1, Transform t1
		)
	{
		BvhCollider b = new BvhCollider(mol0, t0, mol1, t1, 0);
		foreach (BvhCollider.Result r in b.results) {
			Vector3 c0 = t0.TransformPoint(mol0.atom_centres[r.i0]);
			Vector3 c1 = t1.TransformPoint(mol1.atom_centres[r.i1]);
			float min_d = mol0.atom_radii[r.i0] + mol1.atom_radii[r.i1];
			float distance = (c1 - c0).magnitude;
			//Debug.Log("distance=" + distance
			if (distance < min_d) {
				return true;
			}
		}
		return false;
	}
	
	static public bool pysics_collide(
		GameObject obj0, PDB_molecule mol0, Transform t0,
		GameObject obj1, PDB_molecule mol1, Transform t1,
		float seperationForce,
		float water_dia,
		out int num_touching_0,
		out int num_touching_1
		)
    {
		BvhCollider b = new BvhCollider(mol0, t0, mol1, t1, water_dia);
        Rigidbody r0 = obj0.GetComponent<Rigidbody>();
        Rigidbody r1 = obj1.GetComponent<Rigidbody>();
		num_touching_0 = num_touching_1 = 0;
		if (!r0 || !r1) {
			return false;
		}

		BitArray ba0 = new BitArray (mol0.atom_centres.Length);
		BitArray ba1 = new BitArray (mol1.atom_centres.Length);
		num_touching_0 = 0;
		num_touching_1 = 0;

        foreach (BvhCollider.Result r in b.results) {
            Vector3 c0 = t0.TransformPoint(mol0.atom_centres[r.i0]);
            Vector3 c1 = t1.TransformPoint(mol1.atom_centres[r.i1]);
            float min_d = mol0.atom_radii[r.i0] + mol1.atom_radii[r.i1];
            float distance = (c1 - c0).magnitude;

			if (distance < min_d) {
                Vector3 normal = (c0 - c1).normalized * (min_d - distance);
				normal *= seperationForce * Time.fixedDeltaTime;
				r0.AddForceAtPosition(normal,c0);
                r1.AddForceAtPosition(-normal, c1);
            }

			if (distance < min_d + water_dia) {
				//Debug.Log(r.i0 + ", " + r.i1);
				if (!ba0[r.i0]) { num_touching_0++; ba0.Set(r.i0, true); }
				if (!ba1[r.i1]) { num_touching_1++; ba1.Set(r.i1, true); }
			}
        }

        if (b.results.Count > 0) {
			return true;
		}
		return false;
    }
};

