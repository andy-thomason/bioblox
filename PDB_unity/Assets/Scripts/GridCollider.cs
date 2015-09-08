using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using AssemblyCSharp;

// This class generates a list of collisions given two molecules with a BVH sphere tree
public class GridCollider
{
	// basic paramaters of the search.
    PDB_molecule mol0;
    Transform t0;
    PDB_molecule mol1;
    Transform t1;
    public int work_done = 0;


	// allow us to inflate the search to return more pairs.
	float radiusInflate = 0.0f;
    //static System.IO.StreamWriter debug = new System.IO.StreamWriter(@"D:\BioBlox\GridCollider.txt");

	// Result record. i0 and i1 index the atom_* arrays.
	public struct Result {
        public int i0;
        public int i1;
        public Result(int i0, int i1) { this.i0 = i0; this.i1 = i1; }
    };

	// List of results returned by the collider.
	public List<Result> results = new List<Result>();

	// Constructor: generate a list of collision pairs.
	public GridCollider(PDB_molecule mol0, Transform t0, PDB_molecule mol1, Transform t1, float inflation)
	{
		this.mol0 = mol0;
		this.t0 = t0;
		this.mol1 = mol1;
		this.t1 = t1;
		results = new List<Result>();
		radiusInflate = inflation;
		//collide_recursive(0, 0);

		Vector3[] ac0 = mol0.atom_centres;
		int len0 = mol0.atom_centres.Length;

		const float grid_spacing = 1.0f;
		const float rgs = 1.0f / grid_spacing;
		
		// Create a 3D array for each molecule
		Vector3 min = ac0[0];
		Vector3 max = min;
		for (int j = 0; j != len0; ++j)
		{
			//Vector3 r = new Vector3(atom_radii[j], atom_radii[j], atom_radii[j]) + 1.0f;
			min = Vector3.Min(min, ac0[j]);
			max = Vector3.Max(max, ac0[j]);
		}
		
		int x0 = Mathf.FloorToInt (min.x * rgs);
		int y0 = Mathf.FloorToInt (min.y * rgs);
		int z0 = Mathf.FloorToInt (min.z * rgs);
		int x1 = Mathf.FloorToInt (max.x * rgs);
		int y1 = Mathf.FloorToInt (max.y * rgs);
		int z1 = Mathf.FloorToInt (max.z * rgs);
		
		int xdim = x1-x0+1, ydim = y1-y0+1, zdim = z1-z0+1;

		int[] cells = new int[xdim * ydim * zdim + 1];
		for (int i = 0; i != len0; ++i) {
			int x = Mathf.FloorToInt (ac0[i].x * rgs) - x0;
			int y = Mathf.FloorToInt (ac0[i].y * rgs) - y0;
			int z = Mathf.FloorToInt (ac0[i].z * rgs) - z0;
			int idx = (z * ydim + y) * xdim + x;
			cells[idx]++;
		}

		int tot = 0;
		for (int i = 0; i != xdim * ydim * zdim; ++i) {
			tot += cells[i];
			cells[i] = tot;
		}
		cells[xdim * ydim * zdim] = tot;

		int[] atom_ids = new int[len0];
		for (int i = 0; i != len0; ++i) {
			int x = Mathf.FloorToInt (ac0[i].x * rgs) - x0;
			int y = Mathf.FloorToInt (ac0[i].y * rgs) - y0;
			int z = Mathf.FloorToInt (ac0[i].z * rgs) - z0;
			int idx = (z * ydim + y) * xdim + x;
			atom_ids[i] = --cells[idx];
		}

		float rtot = 4.0f;
		int len1 = mol1.atom_centres.Length;
		Vector3[] ac1 = mol1.atom_centres;
		for (int i = 0; i != len1; ++i) {
			Vector3 pos = t1.TransformPoint(ac1[i]);
			pos = t0.InverseTransformPoint(pos);
			int xmin = Mathf.FloorToInt ((pos.x - rtot) * rgs) - x0;
			int ymin = Mathf.FloorToInt ((pos.y - rtot) * rgs) - y0;
			int zmin = Mathf.FloorToInt ((pos.z - rtot) * rgs) - z0;
			int xmax = Mathf.FloorToInt ((pos.x + rtot) * rgs) - x0;
			int ymax = Mathf.FloorToInt ((pos.y + rtot) * rgs) - y0;
			int zmax = Mathf.FloorToInt ((pos.z + rtot) * rgs) - z0;
			xmin = Mathf.Max(0, xmin);
			ymin = Mathf.Max(0, ymin);
			zmin = Mathf.Max(0, zmin);
			xmax = Mathf.Min(xdim-1, xmin);
			ymax = Mathf.Min(ydim-1, ymin);
			zmax = Mathf.Min(zdim-1, zmin);
			for (int z = zmin; z <= zmax; ++z) {
				for (int y = ymin; y <= ymax; ++y) {
					int idx = (z * ydim + y) * xdim;
					int b = cells[idx + xmin], e = cells[idx+xmax+1];
					for (int j = b; j != e; ++j) {
						results.Add(new Result(cells[j], i));
					}
				}
			}
		}
	}
};
