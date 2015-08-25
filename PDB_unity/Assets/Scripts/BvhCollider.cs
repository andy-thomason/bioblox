using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using AssemblyCSharp;

// This class generates a list of collisions given two molecules with a BVH sphere tree
public class BvhCollider
{
	// basic paramaters of the search.
    PDB_molecule mol0;
    Transform t0;
    PDB_molecule mol1;
    Transform t1;
    int work_done = 0;

	// allow us to inflate the search to return more pairs.
	float radiusInflate = 0.0f;
    //static System.IO.StreamWriter debug = new System.IO.StreamWriter(@"D:\BioBlox\BvhCollider.txt");

	// Result record. i0 and i1 index the atom_* arrays.
	public struct Result {
        public int i0;
        public int i1;
        public Result(int i0, int i1) { this.i0 = i0; this.i1 = i1; }
    };

	// List of results returned by the collider.
    public List<Result> results;

	// Collide by subdivision. Reduce the size of the search space by half on each call.
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

		r0 += radiusInflate;
		r1 += radiusInflate;

		// collide two spheres at this level
        if ((c0 - c1).sqrMagnitude < (r0 + r1)*(r0 + r1) && r0 > 0 && r1 > 0)
        {
			// spheres collide.
            if (bt0 == -1) {
                if (bt1 == -1) {
					// neither is a terminal, continue to descend on bvh0 and hbv1
                    collide_recursive(bvh0*2+1, bvh1*2+1);
                    collide_recursive(bvh0*2+1, bvh1*2+2);
                    collide_recursive(bvh0*2+2, bvh1*2+1);
                    collide_recursive(bvh0*2+2, bvh1*2+2);
                }
                else
                {
					// bt1 is a terminal, continue to descend on bvh0.
					collide_recursive(bvh0*2+1, bvh1);
                    collide_recursive(bvh0*2+2, bvh1);
                }
            }
            else
            {
                if (bt1 == -1) {
					// bt0 is a terminal, continue to descend on bvh1.
					collide_recursive(bvh0, bvh1*2+1);
                    collide_recursive(bvh0, bvh1*2+2);
                }
                else
                {
					// both are terminals, collect result.
					results.Add(new Result(bt0, bt1));
                }
            }
        }
    }

	// Constructor: generate a list of collision pairs.
	public BvhCollider(PDB_molecule mol0, Transform t0, PDB_molecule mol1, Transform t1, float inflation)
	{
		this.mol0 = mol0;
		this.t0 = t0;
		this.mol1 = mol1;
		this.t1 = t1;
		results = new List<Result>();
		radiusInflate = inflation;
		collide_recursive(0, 0);
	}
};
