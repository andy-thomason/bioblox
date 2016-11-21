using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using AssemblyCSharp;

/// class to allow accelerated ray atom colisions
public class BvhRayCollider
{
	PDB_molecule mol;
	Transform t;
	Ray ray;
	int work_done=0;

	public struct Result
	{
		public int index;
		public Result(int argIndex){this.index=argIndex;}
	}

	public List<Result> results;

	public void collide_recursive(int bvh)
	{
    if (mol == null || bvh >= mol.bvh_centres.Length) {
      return;
    }

		if (work_done++ > 100000) {
			return;
		}

		Vector3 bvh_world_pos = t.TransformPoint(mol.bvh_centres[bvh]);
		float bvh_radius = mol.bvh_radii[bvh];

		Vector3 bvh_ray_pos = bvh_world_pos - ray.origin;
		float projection = Vector3.Dot (bvh_ray_pos, ray.direction);

		Vector3 point_on_ray = ray.origin + (ray.direction * projection);

		float d = 0;
		if (projection < 0) {
			d = bvh_ray_pos.sqrMagnitude;
		} else {
			d = ( bvh_world_pos - point_on_ray ).sqrMagnitude;
		}

		if (d < bvh_radius * bvh_radius)
		{
			int terminal = mol.bvh_terminals[bvh];
			if (terminal == -1) {
					collide_recursive(bvh*2+1);
					collide_recursive(bvh*2+2);
			}
			else
			{
				results.Add(new Result(terminal));
			}
		}
	}

	//no transform
	public void collide_recursiveNT(int bvh)
	{
		if (work_done++ > 100000) {
			return;
		}
		int bt = mol.bvh_terminals[bvh];
		
		Vector3 c = mol.bvh_centres[bvh];
		float r = mol.bvh_radii[bvh];
		
		Vector3 q = c - ray.origin;
		float f = Vector3.Dot (q, ray.direction);
		
		float d = 0;
		
		if (f < 0) {
			d = q.sqrMagnitude;
		} else {
			d=(c-(ray.origin+(ray.direction*f))).sqrMagnitude;
		}
		
		if (d<r*r)
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
	
	public BvhRayCollider(PDB_molecule mol,Transform t, Ray ray)
	{
		this.mol=mol;
		this.t=t;
		this.ray=ray;
		results =new List<Result>();
		collide_recursive(0);
	}

	public BvhRayCollider(PDB_molecule mol, Ray ray)
	{
		this.mol=mol;
		this.ray=ray;
		results=new List<Result>();
		collide_recursiveNT(0);
	}
}
