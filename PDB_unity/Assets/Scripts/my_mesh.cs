using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;

public class my_mesh : MonoBehaviour
{

    [StructLayout(LayoutKind.Explicit)]
    struct byte_array
    {
        [FieldOffset(0)]
        public byte byte0;

        [FieldOffset(1)]
        public byte byte1;

        [FieldOffset(2)]
        public byte byte2;

        [FieldOffset(3)]
        public byte byte3;

        [FieldOffset(0)]
        public float f;

        [FieldOffset(0)]
        public int i;
    };

    public Material mat;

    byte_array ba = new byte_array();

    private int get_int(byte[] b, int offset)
    {
        ba.byte0 = b[offset];
        ba.byte1 = b[offset + 1];
        ba.byte2 = b[offset + 2];
        ba.byte3 = b[offset + 3];
        return ba.i;
    }

    private float get_float(byte[] b, int offset)
    {
        ba.byte0 = b[offset];
        ba.byte1 = b[offset + 1];
        ba.byte2 = b[offset + 2];
        ba.byte3 = b[offset + 3];
        return ba.f;
    }

    string url;

    public void init(string name)
    {
        url = "http://158.223.59.221:8080/mesh/"+name+".0.50.bin";
        StartCoroutine(GenerateMeshCoru());
    }

    IEnumerator GenerateMeshCoru()
    {
        WWW www = new WWW(url);
        yield return www;

        byte[] b = www.bytes;

        Debug.Log("got " + b.Length + " bytes");

        if (b.Length < 8) yield break;

        int indices_size = get_int(b, 0);
        int vertices_size = get_int(b, 4);

        int[] indices = new int[indices_size];
        int addr = 8;

        for (int i = 0; i != indices_size; ++i)
        {
            indices[i] = get_int(b, addr);
            addr += 4;
        }

        Vector3[] vertices = new Vector3[vertices_size];
        for (int i = 0; i != vertices_size; ++i)
        {
            float x = get_float(b, addr);
            float y = get_float(b, addr + 4);
            float z = get_float(b, addr + 8);
            vertices[i] = new Vector3(x, y, z);
            addr += 12;
        }

        /*Debug.Log("addr=" + addr + " size=" + b.Length);

        for (int i = 0; i != 3; ++i)
        {
          Debug.Log(String.Format("{0}", vertices[indices[i]]));
        }*/

        int[] vertex_map = new int[vertices_size];
        int num_vertices = 0;
        int first_index = 0;
        for (int i = 0; i + 2 < indices_size; i += 3)
        {
            int i0 = indices[i];
            int i1 = indices[i + 1];
            int i2 = indices[i + 2];
            if (vertex_map[i0] == 0) vertex_map[i0] = ++num_vertices;
            if (vertex_map[i1] == 0) vertex_map[i1] = ++num_vertices;
            if (vertex_map[i2] == 0) vertex_map[i2] = ++num_vertices;

            if (num_vertices >= 65000 - 3 || i + 5 >= indices_size)
            {
                Vector3[] sub_vertices = new Vector3[num_vertices];
                int[] sub_indices = new int[i - first_index];
                for (int j = first_index; j != i; ++j)
                {
                    int old_idx = indices[j];
                    int new_idx = vertex_map[old_idx] - 1;
                    sub_indices[j - first_index] = new_idx;
                    sub_vertices[new_idx] = vertices[old_idx];
                }

                GameObject go = new GameObject();
                go.transform.SetParent(transform, false);
                go.name = "mesh";

                MeshFilter mf = go.AddComponent<MeshFilter>();
                MeshRenderer mr = go.AddComponent<MeshRenderer>();
                mr.material = mat;

                Mesh mesh = new Mesh();
                mesh.vertices = sub_vertices;
                mesh.triangles = sub_indices;
                mesh.RecalculateNormals();
                mf.mesh = mesh;

                num_vertices = 0;
                first_index = i + 3;
                vertex_map = new int[vertices_size];
            }
        }
    }
}
