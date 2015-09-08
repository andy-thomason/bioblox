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
	public int work_done;

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
		results = new List<Result>();
		//radiusInflate = inflation;
		//collide_recursive(0, 0);

		Vector3[] ac0 = mol0.atom_centres;
		Vector3[] ac1 = mol1.atom_centres;
		int len0 = mol0.atom_centres.Length;
		int len1 = mol1.atom_centres.Length;

		const float grid_spacing = 2.0f;
		const float rgs = 1.0f / grid_spacing;
		
		// Create a 3D array for one molecule
		Vector3 min0 = t0.TransformPoint(ac0[0]);
		Vector3 max0 = min0;
		for (int j = 0; j != len0; ++j)
		{
			Vector3 pos = t0.TransformPoint(ac0[j]);
			min0 = Vector3.Min(min0, pos);
			max0 = Vector3.Max(max0, pos);
		}
		
		Vector3 min1 = t1.TransformPoint(ac1[0]);
		Vector3 max1 = min0;
		for (int j = 0; j != len1; ++j)
		{
			Vector3 pos = t1.TransformPoint(ac1[j]);
			min1 = Vector3.Min(min1, pos);
			max1 = Vector3.Max(max1, pos);
		}

		Vector3 min = Vector3.Max (min0, min1);
		Vector3 max = Vector3.Min (max0, max1);

		min -= new Vector3 (4.0f, 4.0f, 4.0f);
		max += new Vector3 (4.0f, 4.0f, 4.0f);

		if (min.x > max.x || min.y > max.y || min.z > max.z) {
			return;
		}

		int x0 = Mathf.FloorToInt (min.x * rgs);
		int y0 = Mathf.FloorToInt (min.y * rgs);
		int z0 = Mathf.FloorToInt (min.z * rgs);
		int x1 = Mathf.CeilToInt (max.x * rgs);
		int y1 = Mathf.CeilToInt (max.y * rgs);
		int z1 = Mathf.CeilToInt (max.z * rgs);

		//Debug.DrawLine (min, max);

		/*for (int z = z0; z <= z1; z += z1-z0) {
			for (int x = x0; x <= x1; ++x) {
				Debug.DrawLine (
					grid_spacing * new Vector3 (x, y0, z),
					grid_spacing * new Vector3 (x, y1, z)
				);
			}
			for (int y = y0; y <= y1; ++y) {
				Debug.DrawLine (
					grid_spacing * new Vector3 (x0, y, z),
					grid_spacing * new Vector3 (x1, y, z)
				);
			}
		}*/

		int xdim = x1-x0+1, ydim = y1-y0+1, zdim = z1-z0+1;

		int[] cells = new int[xdim * ydim * zdim + 1];
		for (int i = 0; i != len0; ++i) {
			Vector3 pos = t0.TransformPoint(ac0[i]);
			int x = Mathf.FloorToInt (pos.x * rgs) - x0;
			int y = Mathf.FloorToInt (pos.y * rgs) - y0;
			int z = Mathf.FloorToInt (pos.z * rgs) - z0;
			if ((uint)x < xdim && (uint)y < ydim && (uint)z < zdim) {
				int idx = (z * ydim + y) * xdim + x;
				cells[idx]++;
				/*Debug.DrawLine (
					grid_spacing * new Vector3 (x+x0, y+y0, z+z0),
					grid_spacing * new Vector3 (x+x0+1, y+y0+1, z+z0+1)
				);*/
			}
		}

		int tot = 0;
		for (int i = 0; i != xdim * ydim * zdim; ++i) {
			tot += cells[i];
			cells[i] = tot;
		}
		cells[xdim * ydim * zdim] = tot;

		int[] atom_ids = new int[tot];
		for (int i = 0; i != len0; ++i) {
			Vector3 pos = t0.TransformPoint(ac0[i]);
			int x = Mathf.FloorToInt (pos.x * rgs) - x0;
			int y = Mathf.FloorToInt (pos.y * rgs) - y0;
			int z = Mathf.FloorToInt (pos.z * rgs) - z0;
			if ((uint)x < xdim && (uint)y < ydim && (uint)z < zdim) {
				int idx = (z * ydim + y) * xdim + x;
				atom_ids[--cells[idx]] = i;
			}
		}

		//System.IO.StreamWriter debug = new System.IO.StreamWriter(@"\tmp\1.txt");
		/*
		for (int z = 0; z < zdim; ++z) {
			for (int y = 0; y < ydim; ++y) {
				debug.Write("y=" + y + " z=" + z);
				for (int x = 0; x < xdim; ++x) {
					int idx = (z * ydim + y) * xdim + x;
					debug.Write("[");
					for (int j = cells[idx]; j != cells[idx+1]; ++j) {
						debug.Write(" " + atom_ids[j]);
					}
					debug.Write("] ");
				}
				debug.WriteLine("");
			}
		}
		*/

		float rtot = 4.0f;
		for (int j = 0; j != len1; ++j) {
			Vector3 pos1 = t1.TransformPoint(ac1[j]);
			float r1 = mol1.atom_radii[j];
			int xmin = Mathf.FloorToInt ((pos1.x - rtot) * rgs) - x0;
			int ymin = Mathf.FloorToInt ((pos1.y - rtot) * rgs) - y0;
			int zmin = Mathf.FloorToInt ((pos1.z - rtot) * rgs) - z0;
			int xmax = Mathf.FloorToInt ((pos1.x + rtot) * rgs) - x0;
			int ymax = Mathf.FloorToInt ((pos1.y + rtot) * rgs) - y0;
			int zmax = Mathf.FloorToInt ((pos1.z + rtot) * rgs) - z0;
			xmin = Mathf.Max(0, xmin);
			ymin = Mathf.Max(0, ymin);
			zmin = Mathf.Max(0, zmin);
			xmax = Mathf.Min(xdim-1, xmax);
			ymax = Mathf.Min(ydim-1, ymax);
			zmax = Mathf.Min(zdim-1, zmax);
			if (xmin <= xmax && ymin <= ymax && zmin <= zmax) {
				for (int z = zmin; z <= zmax; ++z) {
					for (int y = ymin; y <= ymax; ++y) {
						int idx = (z * ydim + y) * xdim;
						int b = cells[idx + xmin], e = cells[idx + xmax + 1];
						//debug.Write("i=" + i + " x=" + xmin + ".." + xmax + " y=" + y + " z=" + z + " b=" + b + " e=" + e);
						for (int k = b; k != e; ++k) {
							int i = atom_ids[k];
							float r0 = mol0.atom_radii[i];
							Vector3 pos0 = t0.TransformPoint(ac0[i]);
							if ((pos0 - pos1).magnitude < r0 + (r1 + inflation)) {
								//debug.Write("pos0=" + pos0 + " pos1=" + pos1);
								Debug.DrawLine (pos0, pos1);
								results.Add(new Result(i, j));
							}
						}
						//debug.WriteLine("");
					}
				}
			}
		}
		//debug.Close ();
	}
};
