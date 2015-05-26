using UnityEngine;
using System.Collections;

namespace prashant
{
	public class MedialSurfaceNew : MonoBehaviour {
		//Public variables
		public float waitTime;	//Time to generate next Height-map
		public float medialPrecision;
		public float medialPad;
		public GameObject surface;
		public Texture texture;

		//Private variables
		private float lastElapsedTime;	//Last elapsed time
		private GameObject [] molecule;	//Array of molecules
		private Vector3 medialSurfacePos;	//Position of MedialSurface
		private Vector3 [][] vertices;		//Vertices in the gameobjects
		private Vector3 minSize;
		private Vector3 maxSize;

		private Vector3 [] medialSurfaceVertices;
		private Vector3 [][] surfaceValues;
		private bool [][] isVertex;
		private ArrayList [] facingVertices;


		void createMedialSurface()
		{
			int num=0;
			for(int i=0; i<vertices.Length; i++)
			{
				Vector3 center = molecule[i].transform.position;	//Center of the molecule
				facingVertices[i] = new ArrayList();
				
				for(int j=0; j<vertices[i].Length; j++)
				{
					Vector3 a = vertices[i][j] - center;
					Vector3 b = medialSurfacePos - center;
					if(Vector3.Dot(a,b)>0)
					{
						Vector3 vtx = vertices[i][j];
						facingVertices[i].Add(vtx);
						if(num==0)
						{
							maxSize = minSize = vtx;
						}
						else
						{
							if(vtx.x<minSize.x)
								minSize.x = vtx.x;
							if(vtx.y<minSize.y)
								minSize.y = vtx.y;
							if(vtx.z<minSize.z)
								minSize.z = vtx.z;
							
							if(vtx.x>maxSize.x)
								maxSize.x = vtx.x;
							if(vtx.y>maxSize.y)
								maxSize.y = vtx.y;
							if(vtx.z>maxSize.z)
								maxSize.z = vtx.z;
						}
						num++;
					}
				}
			}

			findFacingVerticesProjection(Mathf.Pow(10,medialPrecision));
		}

		void createMesh(int y_dim, int z_dim)
		{
			Vector3 [] newVertices = new Vector3[y_dim*z_dim];
			Vector2 [] newUV = new Vector2[(y_dim)*(z_dim)];
			int[] newTriangles = new int[(y_dim-1)*(z_dim-1)*6];
			
			
			for(int i=0; i<y_dim; i++)
			{
				for(int j=0; j<z_dim; j++)
				{
					newVertices[i*z_dim+j] = medialSurfaceVertices[i*z_dim+j];
					newUV[i*z_dim+j] = new Vector2(i*z_dim+j,i*z_dim+j);
				}
			}
			
			int num = 0;
			for(int i=0; i<y_dim-1; i++)
			{
				for(int j=0; j<z_dim-1; j++)
				{
					newTriangles[num++] = i*z_dim+j;
					newTriangles[num++] = i*z_dim+j+1;
					newTriangles[num++] = (i+1)*z_dim+j+1;
					
					newTriangles[num++] = (i+1)*z_dim+j+1;
					newTriangles[num++] = (i+1)*z_dim+j;
					newTriangles[num++] = i*z_dim+j;
				}
			}
			Mesh mesh = new Mesh();
			mesh.vertices = newVertices;
			mesh.uv = newUV;
			mesh.triangles = newTriangles;
			mesh.RecalculateNormals();
			
			surface.GetComponent<MeshFilter>().mesh = mesh;
		}

		void fillSurfaceVertices(int z_dim, float multiplier)
		{
			for(int i=0; i<facingVertices.Length; i++)
			{
				for(int j=0; j<facingVertices[i].Count; j++)
				{
					Vector3 currV = (Vector3)facingVertices[i][j];

					int yPos = (int)(((medialPad/2)+currV.y-minSize.y)*multiplier);
					int zPos = (int)(((medialPad/2)+currV.z-minSize.z)*multiplier);

					if(molecule[i].transform.position.x>medialSurfacePos.x)
					{
						if(surfaceValues[i][yPos*z_dim+zPos].x<currV.x)
						{
							surfaceValues[i][yPos*z_dim+zPos].x = currV.x;
						}
					}
					else
					{
						if(surfaceValues[i][yPos*z_dim+zPos].x>currV.x)
						{
							surfaceValues[i][yPos*z_dim+zPos].x = currV.x;
						}
					}

					isVertex[i][yPos*z_dim+zPos] = true;
				}
			}
		}

		void findFacingVerticesProjection(float multiplier)
		{
			int y_dim = (int)((maxSize.y-minSize.y+medialPad)*multiplier);
			int z_dim = (int)((maxSize.z-minSize.z+medialPad)*multiplier);

			medialSurfaceVertices = new Vector3[y_dim*z_dim];
			for(int k=0; k<molecule.Length; k++)
			{
				surfaceValues[k] = new Vector3[y_dim*z_dim];
				isVertex[k] = new bool[y_dim*z_dim];
			}
			
			for(int i=0; i<y_dim; i++)
			{
				for(int j=0; j<z_dim; j++)
				{
					medialSurfaceVertices[i*z_dim+j] = new Vector3(medialSurfacePos.x,minSize.y+(i/multiplier),minSize.z+(j/multiplier));
					for(int k=0; k<molecule.Length; k++)
					{
						surfaceValues[k][i*z_dim+j] = new Vector3(medialSurfacePos.x,minSize.y+(i/multiplier),minSize.z+(j/multiplier));
						isVertex[k][i*z_dim+j] = false;
					}
				}
			}

			fillSurfaceVertices(z_dim,multiplier);
			populateMedialSurface(y_dim,z_dim);
			createMesh(y_dim,z_dim);

			//fillMedialSurfaceVertices(multiplier,z_dim,min);
			//recalculateMedialSurfaceVertices(y_dim,z_dim,min,multiplier);
			//createMesh(y_dim,z_dim);
		}

		Vector3 getMidPoint(GameObject g1, GameObject g2)
		{
			return (g1.transform.position*0.5f+g2.transform.position*0.5f);
		}

		Vector3 [] getVertices(GameObject g, int num)
		{
			return g.GetComponent<MeshFilter>().mesh.vertices;
		}

		void initializeVariables()
		{
			//Public Variables
			waitTime = 0.02f;				//Default value if 0.5 seconds
			medialPrecision = 0f;
			medialPad = 6.0f;

			surface = new GameObject();
			surface.transform.name = "HeightMap";
			surface.AddComponent<MeshFilter>();
			surface.AddComponent<MeshRenderer>();
			surface.GetComponent<Renderer>().material.mainTexture = texture;

			//Private Variables
			lastElapsedTime = Time.realtimeSinceStartup;
			molecule = new GameObject[2];	//For the current version, we are using 2 molecules
			vertices = new Vector3[molecule.Length][];
			minSize = new Vector3(0f,0f,0f);
			maxSize = new Vector3(0f,0f,0f);
			surfaceValues = new Vector3[molecule.Length][];
			isVertex = new bool[molecule.Length][];
			facingVertices	= new ArrayList[molecule.Length];
		}

		bool isTimeElapsed() 
		{
			if(Time.realtimeSinceStartup>=lastElapsedTime+waitTime) 
			{
				lastElapsedTime = Time.realtimeSinceStartup;
				return true;
			}
			else
				return false;
		}

		void populateMedialSurface(int y_dim, int z_dim)
		{
			for(int i=0; i<y_dim; i++)
			{
				for(int j=0; j<z_dim; j++)
				{
					if(isVertex[0][i*z_dim+j] && isVertex[1][i*z_dim+j])
						medialSurfaceVertices[i*z_dim+j].x = (surfaceValues[0][i*z_dim+j].x + surfaceValues[1][i*z_dim+j].x)/2;
				}
			}
		}

		// Use this for initialization
		void Start () 
		{
			initializeVariables();	//Initialize local and/or global variables
		}
		
		// Update is called once per frame
		void Update () 
		{
			if(isTimeElapsed()) //Wait till waitTIme is elapsed to generate next Height-map
			{
				//Finding the two molecules
				molecule[0] = GameObject.Find("R_Pos") as GameObject; // Right Molecule
				molecule[1] = GameObject.Find("L_Pos") as GameObject; // Left Molecule

				medialSurfacePos = getMidPoint(molecule[0],molecule[1]);

				//Getting the vertices from the molecules
				for(int i=0; i<molecule.Length; i++)
				{
					Component [] cmp = molecule[i].GetComponentsInChildren<Component>();
					//Iterating through each component
					for(int j=0; j<cmp.Length; j++)
					{
						string str = cmp[j].GetType().ToString();
						//If the component is a mesh renderer and an outer surface
						if(str.Equals("UnityEngine.MeshRenderer") && cmp[j].name.Contains("surfacese"))
						{
							GameObject g = cmp[j].transform.gameObject;
							vertices[i] = getVertices(g, i);	//Get all the local vertices from this mesh renderer

							//Convert the local verticed to world co-ordinate
							for(int l=0; l<vertices[i].Length; l++)
								vertices[i][l] = g.transform.TransformPoint(vertices[i][l]);
						}
					}
				}

				//Create Height-map through extracted vertices
				createMedialSurface();
			}
		}
	}
}
