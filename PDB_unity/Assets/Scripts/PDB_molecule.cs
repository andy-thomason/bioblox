using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;

public class PDB_molecule
{
    public Vector4[] atoms;
    public int[] names;
    public int[] residues;
    public int[] N_atoms;
    public Vector3 pos;
    public Mesh mesh;
    public Vector4[] bvh_tree;
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
        Debug.Log(isphere.Length);
    }

    void build_ball_mesh() {
        Debug.Log("building mesh");
        mesh = new Mesh();
        mesh.name = "ball view";
        int num_atoms = atoms.Length;
        int vlen = vsphere.Length;
        int ilen = isphere.Length;
        Vector3[] vertices = new Vector3[vlen*num_atoms];
        Vector3[] normals = new Vector3[vlen*num_atoms];
        Vector2[] uvs = new Vector2[vlen*num_atoms];
        int[] indices = new int[ilen*num_atoms];
        int v = 0;
        int idx = 0;
        for (int j = 0; j != num_atoms; ++j) {
            Vector3 pos = new Vector3(atoms[j].x, atoms[j].y, atoms[j].z);
            //if (j < 10) Debug.Log(pos);
            float r = atoms[j].w;
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
        Debug.Log("building mesh");
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
        Vector4 axis;
        Vector4[] atoms_;

        public Sorter(Vector3 dim, Vector4[] atoms)
        {
            atoms_ = atoms;
            if (dim.x >= dim.y && dim.x >= dim.z)
            {
                axis = new Vector4(1, 0, 0, 0);
            } else if (dim.y >= dim.x && dim.y >= dim.z)
            {
                axis = new Vector4(0, 1, 0, 0);
            } else
            {
                axis = new Vector4(0, 0, 1, 0);
            }
        }

        public int Compare(object x, object y) {
            float a = Vector4.Dot(atoms_[(int)x], axis);
            float b = Vector4.Dot(atoms_[(int)y], axis);
            return a < b ? -1 : a > b ? 1 : 0;
        }
    }

    private void partition(List<Vector4> tree, List<int> terminals, int[] index, int b, int e) {
        //Debug.Log("partition " + b + ".." + e);
        if (e-b <= 2) {
            if (b == e) return;
            terminals.Add(index[b]);
            terminals.Add(e-b == 1 ? -1 : index[b+1]);
            //tree.Add(atoms[b]);
            //tree.Add(e-b == 1 ? new Vector4(0, 0, 0, 0) : atoms[b+1]);
            return;
        }

        Vector3 min = new Vector3(atoms[b].x, atoms[b].y, atoms[b].z);
        Vector3 max = min;
        for (int j = b; j != e; ++j)
        {
            float r = atoms[j].w;
            min = Vector3.Min(min, new Vector3(atoms[j].x-r, atoms[j].y-r, atoms[j].z-r));
            max = Vector3.Max(max, new Vector3(atoms[j].x+r, atoms[j].y+r, atoms[j].z+r));
        }

        Vector3 dim = max - min;
        Vector3 centre = (max + min) * 0.5f;

        Array.Sort(index, b, e-b, new Sorter(dim, atoms));

        int mid = b + (e-b)/2;

        float radius = 0;
        for (int j = b; j != e; ++j)
        {
            Vector3 pos = new Vector3(atoms[j].x, atoms[j].y, atoms[j].z) - centre;
            float r = atoms[j].w + pos.magnitude;
            radius = Mathf.Max(radius, r);
        }

        //if (tree.Count < 30) Debug.Log("" + (e - b) + " r=" + radius);

        tree.Add(new Vector4(centre.x, centre.y, centre.z, radius));
        partition(tree, terminals, index, b, mid);
        partition(tree, terminals, index, mid, e);
    }

    public void build_bvh() {
        int num_atoms = atoms.Length;
        int[] index = new int[num_atoms];
        for (int j = 0; j != num_atoms; ++j) {
            index[j] = j;
        }
        List<Vector4> tree = new List<Vector4>();
        List<int> terminals = new List<int>();
        partition(tree, terminals, index, 0, num_atoms);

        Debug.Log("atoms=" + num_atoms);
        Debug.Log("tree=" + tree.Count);
        Debug.Log("terminals=" + terminals.Count);

        bvh_tree = tree.ToArray();
        bvh_terminals = terminals.ToArray();
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

    static public void collide(PDB_molecule mol0, Transform t0, PDB_molecule mol1, Transform t1)
    {
        //Debug.Log("t0=" +t0);
        //Debug.Log("t1=" +t1);
        t0.TransformPoint();
    }
};

