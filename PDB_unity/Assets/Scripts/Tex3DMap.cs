using UnityEngine;
using System.Collections;

public class Tex3DMap : MonoBehaviour {


	Texture3D tex;
	Color[] arr;

	int dim;


	public void Build(PDB_molecule mol)
	{
		Vector3 max = mol.atom_centres [0];
		Vector3 min = mol.atom_centres [0];
		for (int i=0; i<mol.atom_centres.Length; ++i) {
			Vector3 rad=new Vector3(mol.atom_radii[i],
			                        mol.atom_radii[i],
			                        mol.atom_radii[i]);
			max=Vector3.Max(max,mol.atom_centres[i]+rad);
			min=Vector3.Min(min,mol.atom_centres[i]-rad);
		}
		int xDist = Mathf.FloorToInt (max.x - min.x);
		int yDist = Mathf.FloorToInt (max.y - min.y);
		int zDist = Mathf.FloorToInt (max.z - min.z);


		int yStride = xDist;
		int zStride = xDist*yDist;

		int size = 16;
		while(size<xDist)
		{
			size*=2;
		}
		xDist = size;

		size = 16;
		while(size<yDist)
		{
			size*=2;
		}
		yDist = size;

		size = 16;
		while(size<zDist)
		{
			size*=2;
		}
		zDist = size;


		tex = new Texture3D (xDist, yDist, zDist, TextureFormat.ARGB32, false);
		arr = new Color[xDist * yDist * zDist];
		for(int i=0;i<arr.Length;++i)
		{
			arr[i]=Color.black;
		}

		for(int i=0;i<mol.atom_centres.Length;++i)
		{
			int x_index = Mathf.FloorToInt( mol.atom_centres[i].x - min.x );
			int y_index = Mathf.FloorToInt( mol.atom_centres[i].y - min.y );
			int z_index = Mathf.FloorToInt( mol.atom_centres[i].z - min.z );
			int readRadius=1;
			for(int x=-readRadius;x<readRadius+1;++x)
			{
				if(x+x_index<0||x+x_index>xDist)continue;
				for(int y=-readRadius;y<readRadius+1;++y)
				{
					if(y+y_index<0||y+y_index>yDist)continue;
					for(int z=-readRadius;z<readRadius+1;++z)
					{
						if(z+z_index<0||z+z_index>zDist)continue;
						Vector3 pos=new Vector3(x+x_index+min.x,y+y_index+min.y,z+z_index+min.z);
						float sqrdDist=(mol.atom_centres[i]-pos).sqrMagnitude;
						float gauss= 0.5f-Mathf.Exp(-0.5f*sqrdDist);
						int index =x_index+x+(y_index+y)*yStride + (z_index+z)*zStride;
						arr[index]+=new Color(gauss,gauss,gauss,1);
					}
				}
			}
			//int dexy=x_index+y_index*yStride+z_index*zStride;
		}



		tex.SetPixels (arr);
		tex.Apply ();
		this.gameObject.GetComponent<MeshRenderer> ().material.SetTexture (
			"_AmbientOcclusion", tex);
	}


	// Use this for initialization
	void Start () {

		if (this.GetComponent<PDB_mesh> ()) {
			Build(this.GetComponent<PDB_mesh>().mol);
		}
		/*dim = 64;
		arr = new Color[dim * dim * dim];
		tex = new Texture3D (dim, dim, dim, TextureFormat.ARGB32, false);
		for(int i=0;i<arr.Length;++i)
		{
			if(i>arr.Length/4)
			{
				arr[i]=new Color(0,1,0,1);
			}
			else{
				arr[i]=new Color(0,0,0,1);
			}
		}
		tex.SetPixels (arr);
		tex.Apply ();
		this.gameObject.GetComponent<MeshRenderer> ().material.SetTexture (
			"_AmbientOcclusion", tex);*/
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
