using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CSG {
	public class UnityCSGOutput : ICSGOutput {
		int limit;

		public UnityCSGOutput(int limit) {
			this.limit = limit;
		}

		/// <summary>
		/// For collecting mesh data. We assume (for now) that each distinct key will produce a single mesh.
		/// Later, we will need to deal with spatial division of meshes - in which case the values used in this dictionary 
		/// may be of a different type. Or maybe all I need to do is have a separate general method for dividing meshes.
		/// 
		/// Actually a better approach may be to do spatial division first and generate meshes with different Bounds
		/// Hopefully then the meshes would not need further processing, although there may be some artefacts in the case
		/// of complex objects around mesh boundaries.
		/// 
		/// We may want to have different vertex structure depending on type of provenance.
		/// 
		/// I don't really want the type of the key to be string - what I might do is change the implementation of Csg 
		/// so that rather than string Texture, it has some other 'object properties' object attached. I could just use
		/// string to look up such a thing in a Dictionary - which is what I may do first... but not much point in doing
		/// that when we could just have a meaningful object.
		/// 
		/// </summary>
		IDictionary<string, BasicMeshData> meshData = new Dictionary<string, BasicMeshData>();

		public void Add(Poly p) {
			if (p.Count < 3) {
				//ignore degenerate polys
				p.ConditionalReleaseToPool();
				return;
			}

			string text = p.Csg == null ? "nocsg" : p.Csg.Texture ?? "notexture";
			if (!meshData.ContainsKey(text))
				meshData.Add(text, new BasicMeshData(limit));
			meshData[text].Add(p);
		}

		public void AddLoop(CSGPrim prim, Vector3[] polyp) {
			Poly poly = Poly.Make();
			foreach (Vector3 v in polyp)
				poly.Add(v);
			poly.AddTo(prim, this);
		}

		public int Count { 
			get {
				int n = 0;
				foreach (BasicMeshData d in meshData.Values)
					n += d.Count;
				return n;
			}
		}

		// expect to refactor return type
		public IDictionary<string, Mesh> Meshes { 
			get { 
				var newDict = new Dictionary<string, Mesh>();
				foreach (var k in meshData.Keys)
					newDict[k] = meshData[k].GetMesh();
				return newDict;
			}
		}

		// generate meshes with usual limit
		public static IDictionary<string, BasicMeshData> MeshesFromCsg(CSGNode csg, Bounds bounds) {
			return MeshesFromCsg(csg, bounds, 64000);
		}

		// generate meshes with specified limit
		public static IDictionary<string, BasicMeshData> MeshesFromCsg(CSGNode csg, Bounds bounds, int limit) {
			UnityCSGOutput output = new UnityCSGOutput(limit);
			csg.CSGToCSGOutput(bounds, output); 
			return output.meshData; // .Meshes;
		}
	}

	/// <summary>
	/// Mesh data. We collect into ILists since we don't know how much data to expect in advance.
	/// Equivalent to XList in original.
	/// </summary>
	public class BasicMeshData {
		int limit;

		public BasicMeshData(int limit) {
			this.limit = limit;
		}

		public BasicMeshData() {
			this.limit = 64000;
		}

		public static int Sides = 3;
		// 0 neither, 1 frone, 2 back, 3 both
		public static bool CheckWind = false;
		public static int WindShow = 3;
		public static int WrongWind = 0;
		public static int RightWind = 0;
		public static int VeryWrongWind = 0;

		//I could use IList<VertStruct> maybe instead
		private IList<Vector3> verts = new List<Vector3>();
		private IList<Vector3> normals = new List<Vector3>();
		private IList<Vector2> uvs = new List<Vector2>();
		private IList<Color> colors = new List<Color>();

		public IList<Mesh> extraMeshes = new List<Mesh>();
		/// <summary>
		/// Vertex index data.
		/// </summary>
		private IList<int> triangles = new List<int>();
		public int fullsize = 0;

		public IList<Mesh> GetAllMeshes() {
			extraMeshes.Add(GetMesh());
			return extraMeshes;
		}

		public Mesh GetMesh() {
			Mesh mesh = new Mesh();
			mesh.vertices = verts.ToArray<Vector3>(); //nb: ToArray method is in Linq namespace
			mesh.uv = uvs.ToArray<Vector2>();
			mesh.colors = colors.ToArray<Color>();
			mesh.triangles = triangles.ToArray<int>();
			//mesh.RecalculateNormals();
			mesh.normals = normals.ToArray<Vector3>();
			//if (fullsize > 64000) 
			//	GUIBits.Log("mesh truncated from " + fullsize + " to " + mesh.vertices.Count());
			return mesh;
		}

		public BigMesh GetBigMesh() {
			BigMesh mesh = new BigMesh();
			mesh.vertices = verts.ToArray<Vector3>(); //nb: ToArray method is in Linq namespace
			mesh.uv = uvs.ToArray<Vector2>();
			mesh.colors = colors.ToArray<Color>();
			mesh.triangles = triangles.ToArray<int>();
			//mesh.RecalculateNormals();
			mesh.normals = normals.ToArray<Vector3>();
			//if (fullsize > 64000) 
			//	GUIBits.Log("mesh truncated from " + fullsize + " to " + mesh.vertices.Count());
			return mesh;
		}


		public void Add(Poly poly) {
			int n = poly.Count;
			fullsize += n;
			if (poly.Count < 3) {
				poly.ConditionalReleaseToPool();
				return;
			}  // ignore degenerate polys
			int startIndex = verts.Count;
			if (startIndex > limit) {
				extraMeshes.Add(GetMesh());
				verts = new List<Vector3>();
				normals = new List<Vector3>();
				uvs = new List<Vector2>();
				colors = new List<Color>();
				triangles = new List<int>();
				startIndex = 0;
			}
			for (int i = 0; i < n; i++) {
				Vector3 p = poly[i].point;
				//note, this mechanism doesn't allow for re-using vertices.
				//if we collect the data in Dictionary we could get around that...
				//would need to be using entire vert struct as key, not just point.
				verts.Add(p);
				Vector2 uv = poly.Csg.TextureCoordinate(p);
				uvs.Add(uv);
				colors.Add(Color.white);
				normals.Add(poly.Csg.Normal(p));
			}
			//simple fan: pretty sure poly is always convex.
			int w0 = 99;
			for (int i = 1; i < n - 1; i++) {
				int w = 0;  // 1 for wrong
				if (CheckWind) {  // note, CheckWrap costing about 10% on Minst
					float test = Vector3.Dot(
						             normals[startIndex], // .Last(),
						             Vector3.Cross(verts[startIndex] - verts[startIndex + i], verts[startIndex] - verts[startIndex + i + 1])
					             );
					if (test < 0) {
						WrongWind++;
						w = 1;
					} else {
						RightWind++;
					}
					if (i == 1) {
						w0 = w;
					} else if (w != w0) {
						VeryWrongWind++;
					}

				}
				if (((1 << w) & WindShow) != 0) {  // debug wrong wind, WindShow: 1 right, 2 wrong, 3 (default) both
					//option for changing winding order.
					// the w allows for corrected winding
					if ((Sides & 1) != 0) {
						triangles.Add(startIndex);
						triangles.Add(startIndex + i + w);
						triangles.Add(startIndex + i + 1 - w);
					}

					if ((Sides & 2) != 0) {
						triangles.Add(startIndex);
						triangles.Add(startIndex + i + 1 - w);
						triangles.Add(startIndex + i + w);
			
					}
				}
			}

		}

		public void SwitchWinding() {
			if (triangles.Count % 3 != 0)
				GUIBits.Log("triangles.Count not multiple of 3 in BasicMeshData.");// throw new 
			var newTriangles = new List<int>();
			for (int i = 0; i < triangles.Count; i += 3) {
				newTriangles[i] = triangles[i];
				newTriangles[i + 1] = triangles[i + 2];
				newTriangles[i + 2] = triangles[i + 1];
			}
			triangles = newTriangles;
		}

		public int Count { get { return triangles.Count / 3; } }

		public static void ToGame(GameObject gameObject, IDictionary<string, BasicMeshData> meshes, string basename) {
			int nverts = 0, nfullverts = 0;
			foreach (var kvp in meshes) {
				BasicMeshData bmd = kvp.Value; // meshes[k];
				nfullverts += bmd.fullsize;
				string name = basename + "_" + kvp.Key;
				GameObject child = new GameObject(name);
				child.transform.parent = gameObject.transform;
				child.transform.position.Set(0, 0, 0);
				child.transform.Rotate(new Vector3(0, 0, 0));
				child.transform.position = (gameObject.transform.position);
				child.transform.rotation = (gameObject.transform.rotation);


				// prepare one material for all children
				int cnum = 7;
				System.Int32.TryParse(kvp.Key.Split('_')[0], out cnum);
				cnum = cnum % 10;
				Color col = Color.white; // CSGXX.colors[cnum];
				Material mat;
				if (cnum == 999) {
					GameObject pdb = GameObject.Find("ProtoMaterial");
					MeshRenderer pdbr = pdb.GetComponent<MeshRenderer>();
					mat = pdbr.material;
				} else {
					Shader shader = Shader.Find("Custom/Color");	// Set standard shader
					mat = new Material(shader);
				}
				mat.SetColor("_Color", col);

				kvp.Value.ToGame(child, name, mat, ref nverts);

				//child.AddComponent<MeshRenderer> ().material = mat;
				
			}
			//GUIBits.Log ("total verts=" + nverts);
		}

		void ToGame(GameObject parent, string basename, Material mat, ref int nverts) {
			int nextId = 0;
			foreach (var mesh in GetAllMeshes ()) {
				ToGame(parent, mesh, basename + "_" + nextId++, mat, ref nverts);
			}
		}

		public static void ToGame(GameObject parent, Mesh mesh, string name, Material mat, ref int nverts) {
			try {
				GameObject cchild = new GameObject(name);
				cchild.transform.parent = parent.transform;
				cchild.transform.position.Set(0, 0, 0);
				cchild.transform.Rotate(new Vector3(0, 0, 0));
				cchild.transform.position = (parent.transform.position);
				cchild.transform.rotation = (parent.transform.rotation);
				
				cchild.AddComponent<MeshRenderer>().material = mat;
				
				//Mesh mesh = bmd.GetMesh();
				//http://answers.unity3d.com/questions/380284/attaching-an-model-via-script.html
				//child.AddComponent<MeshFilter> ().mesh = mesh;
				//foreach (Mesh m2 in bmd.extraMeshes)  // not sure it works like this ???
				cchild.AddComponent<MeshFilter>().mesh = mesh;
				//GUIBits.Log (kvp.Key + ": mesh vertices=" + mesh.vertexCount);
				nverts += mesh.vertexCount;
			} catch (System.Exception e) {
				GUIBits.Log(e);
			}
		}
		// ToGame for Mesh


	}
	// BasicMeshData

	//public class CSUnityException :

	public static class MathsExtensions {
		public static float Determinant(this Matrix4x4 m) {
			//borrowed from java vecmath Matrix4f
			float f = m.m00
			          * (m.m11 * m.m22 * m.m33 + m.m12 * m.m23
			          * m.m31 + m.m13 * m.m21 * m.m32
			          - (m.m13 * m.m22 * m.m31)
			          - (m.m11 * m.m23 * m.m32) - (m.m12 * m.m21 * m.m33));
			
			f -= m.m01
			* (m.m10 * m.m22 * m.m33 + m.m12 * m.m23
			* m.m30 + m.m13 * m.m20 * m.m32
			- (m.m13 * m.m22 * m.m30)
			- (m.m10 * m.m23 * m.m32) - (m.m12 * m.m20 * m.m33));
			
			f += m.m02
			* (m.m10 * m.m21 * m.m33 + m.m11 * m.m23
			* m.m30 + m.m13 * m.m20 * m.m31
			- (m.m13 * m.m21 * m.m30)
			- (m.m10 * m.m23 * m.m31) - (m.m11 * m.m20 * m.m33));
			
			f -= m.m03
			* (m.m10 * m.m21 * m.m32 + m.m11 * m.m22
			* m.m30 + m.m12 * m.m20 * m.m31
			- (m.m12 * m.m21 * m.m30)
			- (m.m10 * m.m22 * m.m31) - (m.m11 * m.m20 * m.m32));
			
			return f;
		}
	}

	// like a simplified mesh but without limits
	public class BigMesh {
		public Vector3[] vertices;
		public Vector3[] normals;
		public Vector2[] uv;
		public Color[] colors;
		public int[] triangles;

		public BigMesh() {
		}

		public BigMesh(Mesh mesh) { 
			vertices = mesh.vertices;
			normals = mesh.normals;
			uv = mesh.uv;
			colors = mesh.colors;
			triangles = mesh.triangles;
		}

		public Mesh ToMesh() {
			if (vertices.Length > 64000)
				throw new System.Exception("BigMesh too big to convert");
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.normals = normals;
			mesh.uv = uv;
			mesh.colors = colors;
			mesh.triangles = triangles;
			return mesh;
		}

		/** make a mesh with reversed triangles and normals */
		public Mesh ToMeshBack() {
			if (vertices.Length > 64000)
				throw new System.Exception("BigMesh too big to convert");
			Mesh mesh = new Mesh();
			mesh.vertices = vertices;
			mesh.uv = uv;
			mesh.colors = colors;

			// coding question, for some reason not naming the intermediates
			// meant that the assignments to them didn't work
			Vector3[] nnormals = new Vector3[normals.Length];
			for (int i = 0; i < normals.Length; i++)
				nnormals[i] = -normals[i];
			mesh.normals = nnormals;

			int[] ntri = new int[triangles.Length];
			for (int t = 0; t < triangles.Length; t += 3) {
				ntri[t] = triangles[t];
				ntri[t + 1] = triangles[t + 2];
				ntri[t + 2] = triangles[t + 1];
			}
			mesh.triangles = ntri;
			return mesh;
		}

		/** remp the mesh commoning up the vertices 
		 * This is not always valid as it dopes not take account of normals, etc,
		 * but is valid for a 'homogeneous' object such as a metaballs.
		 */
		public BigMesh RemapMesh() {
			int n = vertices.Length;
			bool hascolor = colors != null;
			// remap the vertices, safe for this though not for general CSG ------------
			// (could precompute)
			Dictionary<Vector3, int> newiforv = new Dictionary<Vector3, int>();
			int[] newifori = new int[n];  
			int nnn = 0;
			for (int i = 0; i < n; i++) {
				if (newiforv.ContainsKey(vertices[i])) {
					newifori[i] = newiforv[vertices[i]];
				} else {
					newiforv[vertices[i]] = nnn;
					newifori[i] = nnn;
					nnn++;
					// could verify normals etc equal here, we are assuming this
				}
			}
			GUIBits.Log("vertices=" + n + ", unique vertices=" + nnn);
			
			// now pack down the structures -----------
			Vector3[] n1vertices = new Vector3[nnn];
			Vector3[] n1normals = new Vector3[nnn];
			Vector2[] n1uv = new Vector2[nnn];
			Color[] n1colors = (hascolor) ? new Color[nnn] : null;
			for (int oi = 0; oi < n; oi++) {  //
				int ni = newifori[oi]; 
				n1vertices[ni] = vertices[oi];
				n1normals[ni] = normals[oi];
				n1uv[ni] = uv[oi];
				if (hascolor)
					n1colors[ni] = colors[oi];
			}
			int[] n1triangles = new int[triangles.Length];  // could do in place if precomputed just once
			for (int t = 0; t < triangles.Length; t++)
				n1triangles[t] = newifori[triangles[t]];
			
			
			// experiment, won't usually apply
			if (!hascolor) {
				//int l = CSGXX.colors.Length;
				n1colors = new Color[nnn];
				for (int i = 0; i < nnn; i++) {
					n1colors[i] = Color.white; // CSGXX.colors[i % l];
				}			
			}
			
			BigMesh nmesh = new BigMesh();
			nmesh.vertices = n1vertices;
			nmesh.normals = n1normals;
			nmesh.uv = n1uv;
			nmesh.colors = n1colors;
			nmesh.triangles = n1triangles;
			
			if (!hascolor) {
				n1colors = new Color[nnn];
				
			}
			return nmesh;
			
		}

	}

	// temporary class for bridge to other code
	static class CSGXX {
		public static Color[] colors = new [] {
			Color.grey,
			Color.blue,
			Color.green,
			Color.cyan,
			Color.red,
			Color.magenta,
			Color.yellow,
			Color.white
		};
	}
}

