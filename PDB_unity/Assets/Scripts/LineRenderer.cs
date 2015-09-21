using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System;

public class LineRenderer : MonoBehaviour {
	public struct Line {
		public Vector3 start;
		public Vector3 end;
		public float width;
		public Vector2 uv0;
		public Vector2 uv1;

		public Line(Vector3 start, Vector3 end) {
			this.start = start;
			this.end = end;
			this.width = 0.1f;
			Vector2 uv0=new Vector2(0,0);
			Vector2 uv1=new Vector2(0,0);
			this.uv0 = uv0;
			this.uv1 = uv1;
		}
	}

	List<Line> lines = new List<Line>();

	public Camera lookat_camera;

	// Use this for initialization
	void Start () {
		MeshFilter mf = GetComponent<MeshFilter> ();
		Mesh mesh = new Mesh();
		mf.mesh = mesh;
		mesh.MarkDynamic ();
	}

	// Update is called once per frame
	void Update () {
		if (lookat_camera == null) {
			Debug.Log("warning: lookat_camera missing");
			return;
		}

		Vector3[] vertices = new Vector3[lines.Count * 4];
		Vector2[] uvs = new Vector2[lines.Count * 4];
		int[] indices = new int[lines.Count * 6];

		Vector3 camera_pos = lookat_camera.transform.position;
		for (int i = 0; i != lines.Count; ++i) {
			Line line = lines[i];
			Vector3 to_end = line.end - line.start;
			Vector3 to_cam = camera_pos - line.start;
			Vector3 up = Vector3.Cross(to_end, to_cam).normalized * (line.width * 0.5f);
			vertices[i*4+0] = line.start + up;
			vertices[i*4+1] = line.start - up;
			vertices[i*4+2] = line.end - up;
			vertices[i*4+3] = line.end + up;
			uvs[i*4+0] = new Vector2(line.uv0.x, line.uv0.y);
			uvs[i*4+1] = new Vector2(line.uv0.x, line.uv1.y);
			uvs[i*4+2] = new Vector2(line.uv1.x, line.uv0.y);
			uvs[i*4+3] = new Vector2(line.uv1.x, line.uv1.y);
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
		mesh.uv = uvs;
		mesh.triangles = indices;
		mf.mesh = mesh;
	}

	public Line add_line(Line line) {
		lines.Add (line);
		return line;
	}

	public void clear() {
		lines.Clear ();
	}

	public void delete_line(Line line) {
		lines.Remove (line);
	}
}
