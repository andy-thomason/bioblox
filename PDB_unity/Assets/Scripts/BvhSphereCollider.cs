using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using AssemblyCSharp;

/// class to allow accelerated collisions of a sphere with the molecule
public class BvhSphereCollider
{
	PDB_molecule mol;
	Transform t;
	Vector3 center;
	float radius;
	int work_done=0;

	public struct Result
	{
		public int index;
		public Result(int argIndex){this.index=argIndex;}
	}
	public List<Result> results;

	public void collide_recursive(int bvh)
	{
		if (work_done++ >= 100000) {
			return;
		}
		int bt = mol.bvh_terminals[bvh];
		
		Vector3 c = t.TransformPoint(mol.bvh_centres[bvh]);
		float r = mol.bvh_radii[bvh];

		Vector3 between = c - center;
		float dist = between.sqrMagnitude;

		if (dist<(r+radius)*(r*radius))
		{
			if (bt == -1) {
				collide_recursive(bvh*2+1);
				collide_recursive(bvh*2+2);
			}
			else
			{
				results.Add(new Result(bt));
			}
		}
	}

	public void collide_recursiveNT(int bvh)
	{
		if (work_done++ >= 100000) {
			return;
		}
		int bt = mol.bvh_terminals[bvh];
		
		Vector3 c =mol.bvh_centres[bvh];
		float r = mol.bvh_radii[bvh];
		
		Vector3 between = c - center;
		float dist = between.sqrMagnitude;
		
		if (dist<(r+radius)*(r*radius))
		{
			if (bt == -1) {
				collide_recursiveNT(bvh*2+1);
				collide_recursiveNT(bvh*2+2);
			}
			else
			{
				results.Add(new Result(bt));
			}
		}
	}
	public BvhSphereCollider(PDB_molecule amol, Transform at, Vector3 spherePos, float sphereRad)
	{
		mol=amol;
		t=at;
		center=spherePos;
		radius=sphereRad;
		results=new List<Result>();
		collide_recursive(0);
	}
	public BvhSphereCollider(PDB_molecule amol, Vector3 spherePos, float sphereRad)
	{
		mol=amol;
		center=spherePos;
		radius=sphereRad;
		results=new List<Result>();
		collide_recursiveNT(0);
	}
}
