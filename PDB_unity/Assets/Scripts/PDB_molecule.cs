using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class PDB_molecule
{
    public Vector3[] atom_centres;
    public float[] atom_radii;
    public int[] names;
    public int[] residues;
    public int[] N_atoms;
    public Vector3 pos;
    public Mesh mesh;
    public Vector3[] bvh_centres;
    public float[] bvh_radii;
    public int[] bvh_terminals;

    //const float c = 1.618033988749895f;
    const float e = 0.52573111f;
    const float d = 0.8506508f;
    // icosphere (http://en.wikipedia.org/wiki/Icosahedron#Cartesian_coordinates)
    float[] vproto = {-e,d,0,e,d,0,-e,-d,0,e,-d,0,0,-e,d,0,e,d,0,-e,-d,0,e,-d,d,0,-e,d,0,e,-d,0,-e,-d,0,e};
    int[] iproto = {0,11,5,0,5,1,0,1,7,0,7,10,0,10,11,1,5,9,5,11,4,11,10,2,10,7,6,7,1,8,3,9,4,3,4,2,3,2,6,3,6,8,3,8,9,4,9,5,2,4,11,6,2,10,8,6,7,9,8,1};
    Vector3 [] vsphere;
    int [] isphere;

    public enum Mode { Ball, Ribbon };
    public static Mode mode = Mode.Ball;

    public string name;

    //static System.IO.StreamWriter debug = new System.IO.StreamWriter(@"D:\BioBlox\PDB_molecule.csv");

    Vector3 get_v(int i) { return new Vector3(vproto[i*3+0], vproto[i*3+1], vproto[i*3+2]); }

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
        int idx = num_verts;
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

    void build_ball_mesh() {
        //debug.WriteLine("building mesh");
        mesh = new Mesh();
        mesh.name = "ball view";
        int num_atoms = atom_centres.Length;
        int vlen = vsphere.Length;
        int ilen = isphere.Length;
        Vector3[] vertices = new Vector3[vlen*num_atoms];
        Vector3[] normals = new Vector3[vlen*num_atoms];
        Vector2[] uvs = new Vector2[vlen*num_atoms];
        int[] indices = new int[ilen*num_atoms];
        int v = 0;
        int idx = 0;
        for (int j = 0; j != num_atoms; ++j) {
            Vector3 pos = atom_centres[j];
            //if (j < 10) debug.WriteLine(pos);
            float r = atom_radii[j];
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
        if (mode == Mode.Ball) {
            build_ball_mesh();
        } else if (mode == Mode.Ribbon) {
            build_ribbon_mesh();
        }
    }

    class BvhCollider
    {
        PDB_molecule mol0;
        Transform t0;
        PDB_molecule mol1;
        Transform t1;
        int work_done = 0;

        //static System.IO.StreamWriter debug = new System.IO.StreamWriter(@"D:\BioBlox\BvhCollider.txt");

        public struct Result {
            public int i0;
            public int i1;
            public Result(int i0, int i1) { this.i0 = i0; this.i1 = i1; }
        };

        public List<Result> results;

        public void collide_recursive(int bvh0, int bvh1)
        {
            if (work_done++ == 100000) return;
            //if (bvh0 >= mol0.bvh_terminals.Length) debug.WriteLine("BOOM!");
            //if (bvh1 >= mol1.bvh_terminals.Length) debug.WriteLine("BOOM!");
            int bt0 = mol0.bvh_terminals[bvh0];
            int bt1 = mol1.bvh_terminals[bvh1];

            //debug.WriteLine("[" + bvh0 + ", " + bvh1 + "] / [" + bt0 + ", " + bt1 + "]");
            //debug.Flush();
            Vector3 c0 = t0.TransformPoint(mol0.bvh_centres[bvh0]);
            Vector3 c1 = t1.TransformPoint(mol1.bvh_centres[bvh1]);
            float r0 = mol0.bvh_radii[bvh0];
            float r1 = mol1.bvh_radii[bvh1];


            /*GameObject lhs = GameObject.Find("lhs" + bvh0);
            if (lhs) {
                lhs.transform.localPosition = c0;
                lhs.transform.localScale = new Vector3(r0*2, r0*2, r0*2);
            }

            GameObject rhs = GameObject.Find("rhs" + bvh1);
            if (rhs) {
                rhs.transform.localPosition = c1;
                rhs.transform.localScale = new Vector3(r1*2, r1*2, r1*2);
            }*/

            //debug.WriteLine("[" + bvh0 + ", " + bvh1 + "] c0=" + c0 + " c1=" + c1 + " d0=" + (c0 - c1).sqrMagnitude + " d1=" + (r0 + r1)*(r0 + r1));
            //debug.Flush();
            if ((c0 - c1).sqrMagnitude < (r0 + r1)*(r0 + r1) && r0 != 0 && r1 != 0)
            {
                if (bt0 == -1) {
                    if (bt1 == -1) {
                        collide_recursive(bvh0*2+1, bvh1*2+1);
                        collide_recursive(bvh0*2+1, bvh1*2+2);
                        collide_recursive(bvh0*2+2, bvh1*2+1);
                        collide_recursive(bvh0*2+2, bvh1*2+2);
                    }
                    else
                    {
                        collide_recursive(bvh0*2+1, bvh1);
                        collide_recursive(bvh0*2+2, bvh1);
                    }
                }
                else
                {
                    if (bt1 == -1) {
                        collide_recursive(bvh0, bvh1*2+1);
                        collide_recursive(bvh0, bvh1*2+2);
                    }
                    else
                    {
                        results.Add(new Result(bt0, bt1));
                    }
                }
            }
        }

        public BvhCollider(PDB_molecule mol0, Transform t0, PDB_molecule mol1, Transform t1)
        {
            //debug.WriteLine("colliding " + mol0.name + "/" + mol0.bvh_centres.Length + " with " + mol1.name + "/" + mol1.bvh_centres.Length);
            this.mol0 = mol0;
            this.t0 = t0;
            this.mol1 = mol1;
            this.t1 = t1;
            results = new List<Result>();
            collide_recursive(0, 0);

            //Debug.Log("hits=" + results.Count + " work=" + work_done + "/" + ((mol0.atom_centres.Length + 1) * mol1.atom_centres.Length / 2));
        }
    };

	class BvhRayCollider
	{
		PDB_molecule mol;
		Transform t;
		Ray ray;
		int work_done=0;

		public struct Result
		{
			public int index;
			public Result(int argIndex){this.index=argIndex;}
		}

		public List<Result> results;

		public void collide_recursive(int bvh)
		{
			if (work_done++ > 100000) {
				return;
			}
			int bt = mol.bvh_terminals[bvh];

			Vector3 c = t.TransformPoint(mol.bvh_centres[bvh]);
			float r = mol.bvh_radii[bvh];

			Vector3 q = c - ray.origin;
			float f = Vector3.Dot (q, ray.direction);

			float d = 0;

			if (f < 0) {
				d = q.sqrMagnitude;
			} else {
				d=(c-(ray.origin+(ray.direction*f))).sqrMagnitude;
			}

			if (d<r*r)
			{
				if (bt == -1) {
						collide_recursive(bvh*2+1);
						collide_recursive(bvh*2+2);
				}
				else
				{
					Debug.Log("Hit!");
					results.Add(new Result(bt));
				}
			}
		}
		
		public BvhRayCollider(PDB_molecule mol,Transform t, Ray ray)
		{
			this.mol=mol;
			this.t=t;
			this.ray=ray;
			results =new List<Result>();
			collide_recursive(0);
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
			BvhRayCollider.Result r= b.results[i];
			Vector3 c= t.TransformPoint(mol.atom_centres[r.index]);
			float dist=(ray.origin-c).sqrMagnitude;
			if(closestDistance>dist)
			{
				closestDistance=dist;
				closestIndex=r.index;
			}
		}
		return closestIndex;
	}
	
	
	static public void collide(
		GameObject obj0, PDB_molecule mol0, Transform t0,
		GameObject obj1, PDB_molecule mol1, Transform t1
		)
	{
		BvhCollider b = new BvhCollider(mol0, t0, mol1, t1);
        Rigidbody r0 = obj0.GetComponent<Rigidbody>();
        Rigidbody r1 = obj1.GetComponent<Rigidbody>();
        foreach (BvhCollider.Result r in b.results) {
            Vector3 c0 = t0.TransformPoint(mol0.atom_centres[r.i0]);
            Vector3 c1 = t1.TransformPoint(mol1.atom_centres[r.i1]);
            float min_d = mol0.atom_radii[r.i0] + mol1.atom_radii[r.i1];
            float distance = (c1 - c0).magnitude;
            //Debug.Log("distance=" + distance
            if (distance < min_d) {
                Vector3 normal = (c0 - c1).normalized * (min_d - distance);
				r0.velocity=new Vector3(0,0,0);
                //r1.AddForceAtPosition(normal, c1);
            }
        }
    }
};

