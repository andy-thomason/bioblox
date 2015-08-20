using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using AssemblyCSharp;

/// Class containing the description of a single molecule.
public class PDB_molecule
{
	// the molecule data itself.
    public Vector3[] atom_centres;
    public float[] atom_radii;
	public Color[] atom_colours;

	public Tuple<int,int>[] pairedLabels=new Tuple<int, int>[0];
    public int[] names;
    public int[] residues;
    public int[] N_atoms;
    public Vector3 pos;

	// bounding volume heirachy to accelerate collisions
	public Mesh[] mesh;

	public Vector3[] bvh_centres;
    public float[] bvh_radii;
    public int[] bvh_terminals;


	public PDB_molecule.Label[] labels = new PDB_molecule.Label[0];
	public Tuple<int,int>[] spring_pairs = new Tuple<int,int>[0];
	public int[] serial_to_atom;

    //const float c = 1.618033988749895f;
    const float e = 0.52573111f;
    const float d = 0.8506508f;
    // icosphere (http://en.wikipedia.org/wiki/Icosahedron#Cartesian_coordinates)
    float[] vproto = {-e,d,0,e,d,0,-e,-d,0,e,-d,0,0,-e,d,0,e,d,0,-e,-d,0,e,-d,d,0,-e,d,0,e,-d,0,-e,-d,0,e};
    int[] iproto = {0,11,5,0,5,1,0,1,7,0,7,10,0,10,11,1,5,9,5,11,4,11,10,2,10,7,6,7,1,8,3,9,4,3,4,2,3,2,6,3,6,8,3,8,9,4,9,5,2,4,11,6,2,10,8,6,7,9,8,1};
    Vector3 [] vsphere;
    int [] isphere;

	Vector3[] aoccRays = {
		new Vector3 (0, 1, 0),
		new Vector3 (0, -1, 0),
		new Vector3 (1, 0, 0),
		new Vector3 (-1, 0, 0),
		new Vector3 (0, 0, 1),
		new Vector3 (0, 0, -1),
    };

	// marching cubes edge lists
	readonly sbyte[] mc_triangles = new sbyte[]{
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 1, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 8, 3, 9, 8, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 8, 3, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		9, 2, 10, 0, 2, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		2, 8, 3, 2, 10, 8, 10, 9, 8, -1, -1, -1, -1, -1, -1, -1,
		3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 11, 2, 8, 11, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 9, 0, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 11, 2, 1, 9, 11, 9, 8, 11, -1, -1, -1, -1, -1, -1, -1,
		3, 10, 1, 11, 10, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 10, 1, 0, 8, 10, 8, 11, 10, -1, -1, -1, -1, -1, -1, -1,
		3, 9, 0, 3, 11, 9, 11, 10, 9, -1, -1, -1, -1, -1, -1, -1,
		9, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		4, 3, 0, 7, 3, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 1, 9, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		4, 1, 9, 4, 7, 1, 7, 3, 1, -1, -1, -1, -1, -1, -1, -1,
		1, 2, 10, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		3, 4, 7, 3, 0, 4, 1, 2, 10, -1, -1, -1, -1, -1, -1, -1,
		9, 2, 10, 9, 0, 2, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1,
		2, 10, 9, 2, 9, 7, 2, 7, 3, 7, 9, 4, -1, -1, -1, -1,
		8, 4, 7, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		11, 4, 7, 11, 2, 4, 2, 0, 4, -1, -1, -1, -1, -1, -1, -1,
		9, 0, 1, 8, 4, 7, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1,
		4, 7, 11, 9, 4, 11, 9, 11, 2, 9, 2, 1, -1, -1, -1, -1,
		3, 10, 1, 3, 11, 10, 7, 8, 4, -1, -1, -1, -1, -1, -1, -1,
		1, 11, 10, 1, 4, 11, 1, 0, 4, 7, 11, 4, -1, -1, -1, -1,
		4, 7, 8, 9, 0, 11, 9, 11, 10, 11, 0, 3, -1, -1, -1, -1,
		4, 7, 11, 4, 11, 9, 9, 11, 10, -1, -1, -1, -1, -1, -1, -1,
		9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		9, 5, 4, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 5, 4, 1, 5, 0, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		8, 5, 4, 8, 3, 5, 3, 1, 5, -1, -1, -1, -1, -1, -1, -1,
		1, 2, 10, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		3, 0, 8, 1, 2, 10, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1,
		5, 2, 10, 5, 4, 2, 4, 0, 2, -1, -1, -1, -1, -1, -1, -1,
		2, 10, 5, 3, 2, 5, 3, 5, 4, 3, 4, 8, -1, -1, -1, -1,
		9, 5, 4, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 11, 2, 0, 8, 11, 4, 9, 5, -1, -1, -1, -1, -1, -1, -1,
		0, 5, 4, 0, 1, 5, 2, 3, 11, -1, -1, -1, -1, -1, -1, -1,
		2, 1, 5, 2, 5, 8, 2, 8, 11, 4, 8, 5, -1, -1, -1, -1,
		10, 3, 11, 10, 1, 3, 9, 5, 4, -1, -1, -1, -1, -1, -1, -1,
		4, 9, 5, 0, 8, 1, 8, 10, 1, 8, 11, 10, -1, -1, -1, -1,
		5, 4, 0, 5, 0, 11, 5, 11, 10, 11, 0, 3, -1, -1, -1, -1,
		5, 4, 8, 5, 8, 10, 10, 8, 11, -1, -1, -1, -1, -1, -1, -1,
		9, 7, 8, 5, 7, 9, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		9, 3, 0, 9, 5, 3, 5, 7, 3, -1, -1, -1, -1, -1, -1, -1,
		0, 7, 8, 0, 1, 7, 1, 5, 7, -1, -1, -1, -1, -1, -1, -1,
		1, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		9, 7, 8, 9, 5, 7, 10, 1, 2, -1, -1, -1, -1, -1, -1, -1,
		10, 1, 2, 9, 5, 0, 5, 3, 0, 5, 7, 3, -1, -1, -1, -1,
		8, 0, 2, 8, 2, 5, 8, 5, 7, 10, 5, 2, -1, -1, -1, -1,
		2, 10, 5, 2, 5, 3, 3, 5, 7, -1, -1, -1, -1, -1, -1, -1,
		7, 9, 5, 7, 8, 9, 3, 11, 2, -1, -1, -1, -1, -1, -1, -1,
		9, 5, 7, 9, 7, 2, 9, 2, 0, 2, 7, 11, -1, -1, -1, -1,
		2, 3, 11, 0, 1, 8, 1, 7, 8, 1, 5, 7, -1, -1, -1, -1,
		11, 2, 1, 11, 1, 7, 7, 1, 5, -1, -1, -1, -1, -1, -1, -1,
		9, 5, 8, 8, 5, 7, 10, 1, 3, 10, 3, 11, -1, -1, -1, -1,
		5, 7, 0, 5, 0, 9, 7, 11, 0, 1, 0, 10, 11, 10, 0, -1,
		11, 10, 0, 11, 0, 3, 10, 5, 0, 8, 0, 7, 5, 7, 0, -1,
		11, 10, 5, 7, 11, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 8, 3, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		9, 0, 1, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 8, 3, 1, 9, 8, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1,
		1, 6, 5, 2, 6, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 6, 5, 1, 2, 6, 3, 0, 8, -1, -1, -1, -1, -1, -1, -1,
		9, 6, 5, 9, 0, 6, 0, 2, 6, -1, -1, -1, -1, -1, -1, -1,
		5, 9, 8, 5, 8, 2, 5, 2, 6, 3, 2, 8, -1, -1, -1, -1,
		2, 3, 11, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		11, 0, 8, 11, 2, 0, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1,
		0, 1, 9, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1, -1, -1, -1,
		5, 10, 6, 1, 9, 2, 9, 11, 2, 9, 8, 11, -1, -1, -1, -1,
		6, 3, 11, 6, 5, 3, 5, 1, 3, -1, -1, -1, -1, -1, -1, -1,
		0, 8, 11, 0, 11, 5, 0, 5, 1, 5, 11, 6, -1, -1, -1, -1,
		3, 11, 6, 0, 3, 6, 0, 6, 5, 0, 5, 9, -1, -1, -1, -1,
		6, 5, 9, 6, 9, 11, 11, 9, 8, -1, -1, -1, -1, -1, -1, -1,
		5, 10, 6, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		4, 3, 0, 4, 7, 3, 6, 5, 10, -1, -1, -1, -1, -1, -1, -1,
		1, 9, 0, 5, 10, 6, 8, 4, 7, -1, -1, -1, -1, -1, -1, -1,
		10, 6, 5, 1, 9, 7, 1, 7, 3, 7, 9, 4, -1, -1, -1, -1,
		6, 1, 2, 6, 5, 1, 4, 7, 8, -1, -1, -1, -1, -1, -1, -1,
		1, 2, 5, 5, 2, 6, 3, 0, 4, 3, 4, 7, -1, -1, -1, -1,
		8, 4, 7, 9, 0, 5, 0, 6, 5, 0, 2, 6, -1, -1, -1, -1,
		7, 3, 9, 7, 9, 4, 3, 2, 9, 5, 9, 6, 2, 6, 9, -1,
		3, 11, 2, 7, 8, 4, 10, 6, 5, -1, -1, -1, -1, -1, -1, -1,
		5, 10, 6, 4, 7, 2, 4, 2, 0, 2, 7, 11, -1, -1, -1, -1,
		0, 1, 9, 4, 7, 8, 2, 3, 11, 5, 10, 6, -1, -1, -1, -1,
		9, 2, 1, 9, 11, 2, 9, 4, 11, 7, 11, 4, 5, 10, 6, -1,
		8, 4, 7, 3, 11, 5, 3, 5, 1, 5, 11, 6, -1, -1, -1, -1,
		5, 1, 11, 5, 11, 6, 1, 0, 11, 7, 11, 4, 0, 4, 11, -1,
		0, 5, 9, 0, 6, 5, 0, 3, 6, 11, 6, 3, 8, 4, 7, -1,
		6, 5, 9, 6, 9, 11, 4, 7, 9, 7, 11, 9, -1, -1, -1, -1,
		10, 4, 9, 6, 4, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		4, 10, 6, 4, 9, 10, 0, 8, 3, -1, -1, -1, -1, -1, -1, -1,
		10, 0, 1, 10, 6, 0, 6, 4, 0, -1, -1, -1, -1, -1, -1, -1,
		8, 3, 1, 8, 1, 6, 8, 6, 4, 6, 1, 10, -1, -1, -1, -1,
		1, 4, 9, 1, 2, 4, 2, 6, 4, -1, -1, -1, -1, -1, -1, -1,
		3, 0, 8, 1, 2, 9, 2, 4, 9, 2, 6, 4, -1, -1, -1, -1,
		0, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		8, 3, 2, 8, 2, 4, 4, 2, 6, -1, -1, -1, -1, -1, -1, -1,
		10, 4, 9, 10, 6, 4, 11, 2, 3, -1, -1, -1, -1, -1, -1, -1,
		0, 8, 2, 2, 8, 11, 4, 9, 10, 4, 10, 6, -1, -1, -1, -1,
		3, 11, 2, 0, 1, 6, 0, 6, 4, 6, 1, 10, -1, -1, -1, -1,
		6, 4, 1, 6, 1, 10, 4, 8, 1, 2, 1, 11, 8, 11, 1, -1,
		9, 6, 4, 9, 3, 6, 9, 1, 3, 11, 6, 3, -1, -1, -1, -1,
		8, 11, 1, 8, 1, 0, 11, 6, 1, 9, 1, 4, 6, 4, 1, -1,
		3, 11, 6, 3, 6, 0, 0, 6, 4, -1, -1, -1, -1, -1, -1, -1,
		6, 4, 8, 11, 6, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		7, 10, 6, 7, 8, 10, 8, 9, 10, -1, -1, -1, -1, -1, -1, -1,
		0, 7, 3, 0, 10, 7, 0, 9, 10, 6, 7, 10, -1, -1, -1, -1,
		10, 6, 7, 1, 10, 7, 1, 7, 8, 1, 8, 0, -1, -1, -1, -1,
		10, 6, 7, 10, 7, 1, 1, 7, 3, -1, -1, -1, -1, -1, -1, -1,
		1, 2, 6, 1, 6, 8, 1, 8, 9, 8, 6, 7, -1, -1, -1, -1,
		2, 6, 9, 2, 9, 1, 6, 7, 9, 0, 9, 3, 7, 3, 9, -1,
		7, 8, 0, 7, 0, 6, 6, 0, 2, -1, -1, -1, -1, -1, -1, -1,
		7, 3, 2, 6, 7, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		2, 3, 11, 10, 6, 8, 10, 8, 9, 8, 6, 7, -1, -1, -1, -1,
		2, 0, 7, 2, 7, 11, 0, 9, 7, 6, 7, 10, 9, 10, 7, -1,
		1, 8, 0, 1, 7, 8, 1, 10, 7, 6, 7, 10, 2, 3, 11, -1,
		11, 2, 1, 11, 1, 7, 10, 6, 1, 6, 7, 1, -1, -1, -1, -1,
		8, 9, 6, 8, 6, 7, 9, 1, 6, 11, 6, 3, 1, 3, 6, -1,
		0, 9, 1, 11, 6, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		7, 8, 0, 7, 0, 6, 3, 11, 0, 11, 6, 0, -1, -1, -1, -1,
		7, 11, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		3, 0, 8, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 1, 9, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		8, 1, 9, 8, 3, 1, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1,
		10, 1, 2, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 2, 10, 3, 0, 8, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1,
		2, 9, 0, 2, 10, 9, 6, 11, 7, -1, -1, -1, -1, -1, -1, -1,
		6, 11, 7, 2, 10, 3, 10, 8, 3, 10, 9, 8, -1, -1, -1, -1,
		7, 2, 3, 6, 2, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		7, 0, 8, 7, 6, 0, 6, 2, 0, -1, -1, -1, -1, -1, -1, -1,
		2, 7, 6, 2, 3, 7, 0, 1, 9, -1, -1, -1, -1, -1, -1, -1,
		1, 6, 2, 1, 8, 6, 1, 9, 8, 8, 7, 6, -1, -1, -1, -1,
		10, 7, 6, 10, 1, 7, 1, 3, 7, -1, -1, -1, -1, -1, -1, -1,
		10, 7, 6, 1, 7, 10, 1, 8, 7, 1, 0, 8, -1, -1, -1, -1,
		0, 3, 7, 0, 7, 10, 0, 10, 9, 6, 10, 7, -1, -1, -1, -1,
		7, 6, 10, 7, 10, 8, 8, 10, 9, -1, -1, -1, -1, -1, -1, -1,
		6, 8, 4, 11, 8, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		3, 6, 11, 3, 0, 6, 0, 4, 6, -1, -1, -1, -1, -1, -1, -1,
		8, 6, 11, 8, 4, 6, 9, 0, 1, -1, -1, -1, -1, -1, -1, -1,
		9, 4, 6, 9, 6, 3, 9, 3, 1, 11, 3, 6, -1, -1, -1, -1,
		6, 8, 4, 6, 11, 8, 2, 10, 1, -1, -1, -1, -1, -1, -1, -1,
		1, 2, 10, 3, 0, 11, 0, 6, 11, 0, 4, 6, -1, -1, -1, -1,
		4, 11, 8, 4, 6, 11, 0, 2, 9, 2, 10, 9, -1, -1, -1, -1,
		10, 9, 3, 10, 3, 2, 9, 4, 3, 11, 3, 6, 4, 6, 3, -1,
		8, 2, 3, 8, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1,
		0, 4, 2, 4, 6, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 9, 0, 2, 3, 4, 2, 4, 6, 4, 3, 8, -1, -1, -1, -1,
		1, 9, 4, 1, 4, 2, 2, 4, 6, -1, -1, -1, -1, -1, -1, -1,
		8, 1, 3, 8, 6, 1, 8, 4, 6, 6, 10, 1, -1, -1, -1, -1,
		10, 1, 0, 10, 0, 6, 6, 0, 4, -1, -1, -1, -1, -1, -1, -1,
		4, 6, 3, 4, 3, 8, 6, 10, 3, 0, 3, 9, 10, 9, 3, -1,
		10, 9, 4, 6, 10, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		4, 9, 5, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 8, 3, 4, 9, 5, 11, 7, 6, -1, -1, -1, -1, -1, -1, -1,
		5, 0, 1, 5, 4, 0, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1,
		11, 7, 6, 8, 3, 4, 3, 5, 4, 3, 1, 5, -1, -1, -1, -1,
		9, 5, 4, 10, 1, 2, 7, 6, 11, -1, -1, -1, -1, -1, -1, -1,
		6, 11, 7, 1, 2, 10, 0, 8, 3, 4, 9, 5, -1, -1, -1, -1,
		7, 6, 11, 5, 4, 10, 4, 2, 10, 4, 0, 2, -1, -1, -1, -1,
		3, 4, 8, 3, 5, 4, 3, 2, 5, 10, 5, 2, 11, 7, 6, -1,
		7, 2, 3, 7, 6, 2, 5, 4, 9, -1, -1, -1, -1, -1, -1, -1,
		9, 5, 4, 0, 8, 6, 0, 6, 2, 6, 8, 7, -1, -1, -1, -1,
		3, 6, 2, 3, 7, 6, 1, 5, 0, 5, 4, 0, -1, -1, -1, -1,
		6, 2, 8, 6, 8, 7, 2, 1, 8, 4, 8, 5, 1, 5, 8, -1,
		9, 5, 4, 10, 1, 6, 1, 7, 6, 1, 3, 7, -1, -1, -1, -1,
		1, 6, 10, 1, 7, 6, 1, 0, 7, 8, 7, 0, 9, 5, 4, -1,
		4, 0, 10, 4, 10, 5, 0, 3, 10, 6, 10, 7, 3, 7, 10, -1,
		7, 6, 10, 7, 10, 8, 5, 4, 10, 4, 8, 10, -1, -1, -1, -1,
		6, 9, 5, 6, 11, 9, 11, 8, 9, -1, -1, -1, -1, -1, -1, -1,
		3, 6, 11, 0, 6, 3, 0, 5, 6, 0, 9, 5, -1, -1, -1, -1,
		0, 11, 8, 0, 5, 11, 0, 1, 5, 5, 6, 11, -1, -1, -1, -1,
		6, 11, 3, 6, 3, 5, 5, 3, 1, -1, -1, -1, -1, -1, -1, -1,
		1, 2, 10, 9, 5, 11, 9, 11, 8, 11, 5, 6, -1, -1, -1, -1,
		0, 11, 3, 0, 6, 11, 0, 9, 6, 5, 6, 9, 1, 2, 10, -1,
		11, 8, 5, 11, 5, 6, 8, 0, 5, 10, 5, 2, 0, 2, 5, -1,
		6, 11, 3, 6, 3, 5, 2, 10, 3, 10, 5, 3, -1, -1, -1, -1,
		5, 8, 9, 5, 2, 8, 5, 6, 2, 3, 8, 2, -1, -1, -1, -1,
		9, 5, 6, 9, 6, 0, 0, 6, 2, -1, -1, -1, -1, -1, -1, -1,
		1, 5, 8, 1, 8, 0, 5, 6, 8, 3, 8, 2, 6, 2, 8, -1,
		1, 5, 6, 2, 1, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 3, 6, 1, 6, 10, 3, 8, 6, 5, 6, 9, 8, 9, 6, -1,
		10, 1, 0, 10, 0, 6, 9, 5, 0, 5, 6, 0, -1, -1, -1, -1,
		0, 3, 8, 5, 6, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		10, 5, 6, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		11, 5, 10, 7, 5, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		11, 5, 10, 11, 7, 5, 8, 3, 0, -1, -1, -1, -1, -1, -1, -1,
		5, 11, 7, 5, 10, 11, 1, 9, 0, -1, -1, -1, -1, -1, -1, -1,
		10, 7, 5, 10, 11, 7, 9, 8, 1, 8, 3, 1, -1, -1, -1, -1,
		11, 1, 2, 11, 7, 1, 7, 5, 1, -1, -1, -1, -1, -1, -1, -1,
		0, 8, 3, 1, 2, 7, 1, 7, 5, 7, 2, 11, -1, -1, -1, -1,
		9, 7, 5, 9, 2, 7, 9, 0, 2, 2, 11, 7, -1, -1, -1, -1,
		7, 5, 2, 7, 2, 11, 5, 9, 2, 3, 2, 8, 9, 8, 2, -1,
		2, 5, 10, 2, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1,
		8, 2, 0, 8, 5, 2, 8, 7, 5, 10, 2, 5, -1, -1, -1, -1,
		9, 0, 1, 5, 10, 3, 5, 3, 7, 3, 10, 2, -1, -1, -1, -1,
		9, 8, 2, 9, 2, 1, 8, 7, 2, 10, 2, 5, 7, 5, 2, -1,
		1, 3, 5, 3, 7, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 8, 7, 0, 7, 1, 1, 7, 5, -1, -1, -1, -1, -1, -1, -1,
		9, 0, 3, 9, 3, 5, 5, 3, 7, -1, -1, -1, -1, -1, -1, -1,
		9, 8, 7, 5, 9, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		5, 8, 4, 5, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1,
		5, 0, 4, 5, 11, 0, 5, 10, 11, 11, 3, 0, -1, -1, -1, -1,
		0, 1, 9, 8, 4, 10, 8, 10, 11, 10, 4, 5, -1, -1, -1, -1,
		10, 11, 4, 10, 4, 5, 11, 3, 4, 9, 4, 1, 3, 1, 4, -1,
		2, 5, 1, 2, 8, 5, 2, 11, 8, 4, 5, 8, -1, -1, -1, -1,
		0, 4, 11, 0, 11, 3, 4, 5, 11, 2, 11, 1, 5, 1, 11, -1,
		0, 2, 5, 0, 5, 9, 2, 11, 5, 4, 5, 8, 11, 8, 5, -1,
		9, 4, 5, 2, 11, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		2, 5, 10, 3, 5, 2, 3, 4, 5, 3, 8, 4, -1, -1, -1, -1,
		5, 10, 2, 5, 2, 4, 4, 2, 0, -1, -1, -1, -1, -1, -1, -1,
		3, 10, 2, 3, 5, 10, 3, 8, 5, 4, 5, 8, 0, 1, 9, -1,
		5, 10, 2, 5, 2, 4, 1, 9, 2, 9, 4, 2, -1, -1, -1, -1,
		8, 4, 5, 8, 5, 3, 3, 5, 1, -1, -1, -1, -1, -1, -1, -1,
		0, 4, 5, 1, 0, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		8, 4, 5, 8, 5, 3, 9, 0, 5, 0, 3, 5, -1, -1, -1, -1,
		9, 4, 5, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		4, 11, 7, 4, 9, 11, 9, 10, 11, -1, -1, -1, -1, -1, -1, -1,
		0, 8, 3, 4, 9, 7, 9, 11, 7, 9, 10, 11, -1, -1, -1, -1,
		1, 10, 11, 1, 11, 4, 1, 4, 0, 7, 4, 11, -1, -1, -1, -1,
		3, 1, 4, 3, 4, 8, 1, 10, 4, 7, 4, 11, 10, 11, 4, -1,
		4, 11, 7, 9, 11, 4, 9, 2, 11, 9, 1, 2, -1, -1, -1, -1,
		9, 7, 4, 9, 11, 7, 9, 1, 11, 2, 11, 1, 0, 8, 3, -1,
		11, 7, 4, 11, 4, 2, 2, 4, 0, -1, -1, -1, -1, -1, -1, -1,
		11, 7, 4, 11, 4, 2, 8, 3, 4, 3, 2, 4, -1, -1, -1, -1,
		2, 9, 10, 2, 7, 9, 2, 3, 7, 7, 4, 9, -1, -1, -1, -1,
		9, 10, 7, 9, 7, 4, 10, 2, 7, 8, 7, 0, 2, 0, 7, -1,
		3, 7, 10, 3, 10, 2, 7, 4, 10, 1, 10, 0, 4, 0, 10, -1,
		1, 10, 2, 8, 7, 4, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		4, 9, 1, 4, 1, 7, 7, 1, 3, -1, -1, -1, -1, -1, -1, -1,
		4, 9, 1, 4, 1, 7, 0, 8, 1, 8, 7, 1, -1, -1, -1, -1,
		4, 0, 3, 7, 4, 3, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		4, 8, 7, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		9, 10, 8, 10, 11, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		3, 0, 9, 3, 9, 11, 11, 9, 10, -1, -1, -1, -1, -1, -1, -1,
		0, 1, 10, 0, 10, 8, 8, 10, 11, -1, -1, -1, -1, -1, -1, -1,
		3, 1, 10, 11, 3, 10, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 2, 11, 1, 11, 9, 9, 11, 8, -1, -1, -1, -1, -1, -1, -1,
		3, 0, 9, 3, 9, 11, 1, 2, 9, 2, 11, 9, -1, -1, -1, -1,
		0, 2, 11, 8, 0, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		3, 2, 11, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		2, 3, 8, 2, 8, 10, 10, 8, 9, -1, -1, -1, -1, -1, -1, -1,
		9, 10, 2, 0, 9, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		2, 3, 8, 2, 8, 10, 0, 1, 8, 1, 10, 8, -1, -1, -1, -1,
		1, 10, 2, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		1, 3, 8, 9, 1, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 9, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		0, 3, 8, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1,
		-1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1
	};
    
    public enum Mode { Ball, Ribbon, Metasphere };
	public static Mode mode = Mode.Metasphere;
    
    public string name;


	public struct Label
	{
		public Label(int id){
			uniqueLabelID = id;
			atomIds = new List<int>();
		}
		public int uniqueLabelID;
		public List<int> atomIds;
	}
    
    //static System.IO.StreamWriter debug = new System.IO.StreamWriter(@"C:\tmp\PDB_molecule.csv");
    
    Vector3 get_v(int i) { return new Vector3(vproto[i*3+0], vproto[i*3+1], vproto[i*3+2]); }

    void build_sphere() {
        int num_tris = iproto.Length / 3;
        int num_verts = vproto.Length / 3;
        vsphere = new Vector3[num_verts + num_tris];
        isphere = new int[num_tris * 9];
        for (int i = 0; i != num_verts; ++i) {
            vsphere[i].x = vproto[i*3+0];
            vsphere[i].y = vproto[i*3+1];
            vsphere[i].z = vproto[i*3+2];
			vsphere[i] = vsphere[i].normalized;
        }
        for (int i = 0; i != num_tris; ++i) {
            int i0 = iproto[i*3+0];
            int i1 = iproto[i*3+1];
            int i2 = iproto[i*3+2];
            Vector3 p0 = get_v(i0);
            Vector3 p1 = get_v(i1);
            Vector3 p2 = get_v(i2);

            /*for (int ty = 0; ty <= 3; ++ty)
            {
                Vector3 q0 = Vector3.Lerp(p0, p1, ty / 3.0f);
                Vector3 q1 = Vector3.Lerp(p0, p2, ty / 3.0f);
                for (int tx = 0; tx <= ty; ++tx)
                {
                    vsphere[idx++] = Vector3.Lerp(q0, q1, tx == 0 ? 0.0f : tx / (float)ty);
                }
            }*/
            int i3 = num_verts + i;
            vsphere[i3] = (p0 + p1 + p2).normalized;
            //    i0
            //    i3
            // i2    i1
            isphere[i*9+0] = i0;
            isphere[i*9+1] = i1;
            isphere[i*9+2] = i3;
            isphere[i*9+3] = i1;
            isphere[i*9+4] = i2;
            isphere[i*9+5] = i3;
            isphere[i*9+6] = i2;
            isphere[i*9+7] = i0;
            isphere[i*9+8] = i3;
        }
        //debug.WriteLine(isphere.Length);
    }

	float CalcAmbientOcclusion(Vector3 pos, Vector3 norm, float radius)
	{
		float occlusion = 0;
		float maxDist = 7;
		for(int i=0;i<aoccRays.Length;++i)
		{
			Vector3 posOffset = pos + aoccRays[i] * radius;
			Ray r = new Ray (posOffset, aoccRays[i]);
			BvhRayCollider b = new BvhRayCollider (this, r);
			float minDist = maxDist;
			for (int j=0; j< b.results.Count; ++j) {
				float dist=(pos-atom_centres[b.results[j].index]).sqrMagnitude;
				if(dist<maxDist&&dist!=0)
				{
					minDist=Math.Min(minDist,dist);
				}
			}
		occlusion += (1 - (minDist / maxDist));
		}
		return (1 - ((occlusion) / aoccRays.Length));
	}

    // https://en.wikipedia.org/wiki/Marching_cubes
    void build_metasphere_mesh(out Vector3[] vertices, out Vector3[] normals, out Vector2[] uvs, out Color[] colours, out int[] indices) {
		const float grid_spacing = 0.5f;
		const float rgs = 1.0f / grid_spacing;

		// Create a 3D array for each molecule
		Vector3 min = atom_centres[0];
		Vector3 max = min;
		float max_r = 0.0f;
		for (int j = 0; j != atom_centres.Length; ++j)
		{
			//Vector3 r = new Vector3(atom_radii[j], atom_radii[j], atom_radii[j]) + 1.0f;
			float r = atom_radii[j];
			max_r = Mathf.Max(max_r, r);
			min = Vector3.Min(min, atom_centres[j]);
			max = Vector3.Max(max, atom_centres[j]);
		}

		int irgs = Mathf.CeilToInt (max_r * rgs) + 1;
		int x0 = Mathf.FloorToInt (min.x * rgs) - irgs;
		int y0 = Mathf.FloorToInt (min.y * rgs) - irgs;
		int z0 = Mathf.FloorToInt (min.z * rgs) - irgs;
		int x1 = Mathf.CeilToInt (max.x * rgs) + irgs;
		int y1 = Mathf.CeilToInt (max.y * rgs) + irgs;
		int z1 = Mathf.CeilToInt (max.z * rgs) + irgs;

		int xdim = x1-x0+1, ydim = y1-y0+1, zdim = z1-z0+1;

		// Array contains values (-0.5..>1) positive means inside molecule.
		// Normals are drived from values.
		// Colours shaded by ambient occlusion (TODO). 
		float[] mc_values = new float[xdim * ydim * zdim];
		Vector3[] mc_normals = new Vector3[xdim * ydim * zdim];
		Color[] mc_colours = new Color[xdim * ydim * zdim];
		float diff = 0.125f, rec = 2.0f / diff;
		for (int i = 0; i != xdim * ydim * zdim; ++i) {
			mc_values[i] = -0.5f;
			mc_colours[i] = new Color(1, 1, 1, 1);
		}

		// For each atom add in the values and normals surrounding
		// the centre up to a reasonable radius.
		int acmax = atom_centres.Length;
		DateTime start_time = DateTime.Now;
		for (int ac = 0; ac != acmax; ++ac) {
			Vector3 c = atom_centres[ac];
			float r = atom_radii[ac] * 0.8f * rgs;
			Color colour = atom_colours[ac];
			float cx = c.x * rgs;
			float cy = c.y * rgs;
			float cz = c.z * rgs;

			// define a box around the atom.
			int cix = Mathf.FloorToInt(c.x * rgs);
			int ciy = Mathf.FloorToInt(c.y * rgs);
			int ciz = Mathf.FloorToInt(c.z * rgs);
			int xmin = Mathf.Max(x0, cix-irgs);
			int ymin = Mathf.Max(y0, ciy-irgs);
			int zmin = Mathf.Max(z0, ciz-irgs);
			int xmax = Mathf.Max(x1, cix+irgs);
			int ymax = Mathf.Max(y1, ciy+irgs);
			int zmax = Mathf.Max(z1, ciz+irgs);
			float fk = Mathf.Log(0.5f) / (r * r);
			float fkk = -fk * 0.5f; // 0.5 is a magic number!
			bool wcol = colour != Color.white;

			for (int z = zmin; z != zmax; ++z) {
				float fdz = z - cz;
				for (int y = ymin; y != ymax; ++y) {
					float fdy = y - cy;
					int idx = ((z-z0) * ydim + (y-y0)) * xdim + (xmin-x0);
					float d2_base = fdy*fdy + fdz*fdz;
					for (int x = xmin; x != xmax; ++x) {
						float fdx = x - cx;
						float d2 = fdx*fdx + d2_base;
						float v = fkk * d2;
						if (v < 1) {
							// a little like exp(-v)
							float val = (2 * v - 3) * v * v + 1;
							mc_values[idx] += val;
							float rcp = val / Mathf.Sqrt(d2);
							mc_normals[idx].x += fdx * rcp;
							mc_normals[idx].y += fdy * rcp;
							mc_normals[idx].z += fdz * rcp;
							if (wcol) mc_colours[idx] = colour;
						}
						idx++;
					}
				}
			}
		}
		Debug.Log (DateTime.Now - start_time + "s to make values");
		start_time = DateTime.Now;

		// This reproduced the vertex order of Paul Bourke's (borrowed) table.
		// The indices in edge_indices have the following offsets.
		//
		//     7 6   y   z
		// 3 2 4 5   | /
		// 0 1       0 - x
		//

		// Now build the marching cubes triangles.
		// Each cube owns three edges 0->1 0->3 0->4
		int[] edge_indices = new int[xdim*ydim*zdim*3];
		byte[] masks = new byte[xdim*ydim*zdim];


		// We store three indices per cube for the edges closest to vertex 0
		// All other indices can be derived from adjacent cubes.
		// This gives a single index offset value for each edge.
		// There are twelve edges here because we consider adjacent cubes also.
		int dx = 3;
        int dy = xdim * 3;
		int dz = xdim * ydim * 3;
		int[] edge_offsets = new int[]{
			0*dx+0*dy+0*dz+0,  // 0,1,
			1*dx+0*dy+0*dz+1,  // 1,2,
			0*dx+1*dy+0*dz+0,  // 2,3,
			0*dx+0*dy+0*dz+1,  // 3,0,
			0*dx+0*dy+1*dz+0,  // 4,5,
			1*dx+0*dy+1*dz+1,  // 5,6,
			0*dx+1*dy+1*dz+0,  // 6,7,
			0*dx+0*dy+1*dz+1,  // 7,4,
			0*dx+0*dy+0*dz+2,  // 0,4,
			1*dx+0*dy+0*dz+2,  // 1,5,
			1*dx+1*dy+0*dz+2,  // 2,6,
			0*dx+1*dy+0*dz+2,  // 3,7
        };
        
		// two passes for vertices (pass = 0 sizes the array)
		vertices = null;
		normals = null;
		colours = null;
		uvs = null;
		for (int i = 0; i != edge_indices.Length; ++i) {
			edge_indices [i] = -1;
		}

		// Build the vertices first in two passes.
		for (int pass = 0; pass != 2; ++pass) {
			int num_edges = 0;

			for (int k = 0; k != zdim; ++k) {
				for (int j = 0; j != ydim; ++j) {
					for (int i = 0; i != xdim; ++i) {
						int idx = (k * ydim + j) * xdim + i;
						float v0 = mc_values [idx];
						// x edges
						if (i != xdim-1) {
							float v1 = mc_values [idx + 1];
							if (v0 * v1 < 0) {
								if (pass == 1) {
									float lambda = v0 / (v0 - v1);
									edge_indices[idx*3+0] = num_edges;
									vertices[num_edges] = new Vector3(x0 + i + lambda, y0 + j, z0 + k) * grid_spacing;
									normals[num_edges] = Vector3.Lerp (mc_normals[idx], mc_normals[idx+1], lambda).normalized;
									colours[num_edges] = Color.Lerp (mc_colours[idx], mc_colours[idx+1], lambda);
									//debug.WriteLine("x " + normals[num_edges]);
								}
								num_edges++;
							}
						}
						// y edges
						if (j != ydim-1) {
							float v1 = mc_values [idx + xdim];
							if (v0 * v1 < 0) {
								if (pass == 1) {
									float lambda = v0 / (v0 - v1);
									edge_indices[idx*3+1] = num_edges;
									vertices[num_edges] = new Vector3(x0 + i, y0 + j + lambda, z0 + k) * grid_spacing;
									normals[num_edges] = Vector3.Lerp (mc_normals[idx], mc_normals[idx+xdim], lambda).normalized;
									colours[num_edges] = Color.Lerp (mc_colours[idx], mc_colours[idx+xdim], lambda);
									//debug.WriteLine("y " + normals[num_edges]);
								}
                                num_edges++;
                            }
                        }
						// z edges
						if (k != zdim-1) {
                            float v1 = mc_values [idx + xdim*ydim];
							if (v0 * v1 < 0) {
								if (pass == 1) {
									float lambda = v0 / (v0 - v1);
									edge_indices[idx*3+2] = num_edges;
									vertices[num_edges] = new Vector3(x0 + i, y0 + j, z0 + k + lambda) * grid_spacing;
									normals[num_edges] = Vector3.Lerp (mc_normals[idx], mc_normals[idx+xdim*ydim], lambda).normalized;
									colours[num_edges] = Color.Lerp (mc_colours[idx], mc_colours[idx+xdim*ydim], lambda);
									//debug.WriteLine("z " + normals[num_edges]);
								}
                                num_edges++;
                            }
                        }
						if (pass == 1) {
							//debug.WriteLine(i + ", " + j + ", " + k + ", " + idx + ", " + mc_values[idx] + ", " + edge_indices[idx*3+0] + ", " + edge_indices[idx*3+1] + ", " + edge_indices[idx*3+2]);
						}
					}
					//debug.WriteLine("");
                }
				//debug.WriteLine("");
            }
            if (pass == 0) {
				vertices = new Vector3[num_edges];
				normals = new Vector3[num_edges];
				colours = new Color[num_edges];
				uvs = new Vector2[num_edges];
			}
        }
        
        // Two passes for indices (pass = 0 sizes the array)
		indices = null;
        for (int pass = 0; pass != 2; ++pass) {
			int num_idx = 0;
			// loop over all cubes
			for (int k = 0; k != zdim-1; ++k) {
				for (int j = 0; j != ydim-1; ++j) {
					for (int i = 0; i != xdim-1; ++i) {
						int idx = (k * ydim + j) * xdim + i;
						Vector3 pos0 = new Vector3 (x0 + i, y0 + j, z0 + k);

						// Mask of vertices outside the isosurface (values are negative)
						// Example:
						//   00000001 means only vertex 0 is outside the surface.
						//   10000000 means only vertex 7 is outside the surface.
						//   11111111 all vertices are outside the surface.
						int mask =
							(mc_values [idx] < 0 ? 1 << 0 : 0) |
							(mc_values [idx + 1] < 0 ? 1 << 1 : 0) |
							(mc_values [idx + 1 + xdim] < 0 ? 1 << 2 : 0) |
							(mc_values [idx + xdim] < 0 ? 1 << 3 : 0) |
						
							(mc_values [idx + xdim * ydim] < 0 ? 1 << 4 : 0) |
							(mc_values [idx + 1 + xdim * ydim] < 0 ? 1 << 5 : 0) |
							(mc_values [idx + 1 + xdim + xdim * ydim] < 0 ? 1 << 6 : 0) |
							(mc_values [idx + xdim + xdim * ydim] < 0 ? 1 << 7 : 0
						);
						//if (pass == 1) debug.WriteLine("mask={0:X2}", mask);

						int off = mask * 16;
						while (mc_triangles[off] != -1) {
							if (pass == 1) {
								indices [num_idx + 0] = edge_indices [idx*3 + edge_offsets [mc_triangles [off + 0]]];
								indices [num_idx + 1] = edge_indices [idx*3 + edge_offsets [mc_triangles [off + 1]]];
								indices [num_idx + 2] = edge_indices [idx*3 + edge_offsets [mc_triangles [off + 2]]];
								//debug.WriteLine("tri, " + i + ", " + j + ", " + k + ", " + idx + ", " + indices [num_idx + 0] + ", " + indices [num_idx + 1] + ", " + indices [num_idx + 2]);
                            }
							num_idx += 3;
							off += 3;
						}
					}
				}
			}
			if (pass == 0) {
				indices = new int[num_idx];
            }
		}
		//debug.Flush();
		Debug.Log (DateTime.Now - start_time + "s to make mesh");
	}
	
	void build_ball_mesh(out Vector3[] vertices,out Vector3[] normals,out Vector2[] uvs,out Color[] colors,out int[] indices) {
        //debug.WriteLine("building mesh");
 
        int num_atoms = atom_centres.Length;
        int vlen = vsphere.Length;
        int ilen = isphere.Length;
        vertices = new Vector3[vlen*num_atoms];
       	normals = new Vector3[vlen*num_atoms];
        uvs = new Vector2[vlen*num_atoms];
		colors = new Color[vlen * num_atoms];
        indices = new int[ilen*num_atoms];
        int v = 0;
        int idx = 0;
        for (int j = 0; j != num_atoms; ++j) {
            Vector3 pos = atom_centres[j];
			/*Color col=new 
				Color(0.1f,0.1f,0.1f,
			    CalcAmbientOcclusion(pos,pos.normalized,atom_radii[j]));
			*/
			Color col = atom_colours[j];
            //if (j < 10) debug.WriteLine(pos);
            float r = atom_radii[j];
            for (int i = 0; i != vlen; ++i) {
                vertices[v] = vsphere[i]*r + pos;
                normals[v] = vsphere[i];
                uvs[v].x = normals[v].x;
                uvs[v].y = normals[v].y;
				colors[v] = col;
                ++v;
            }
            for (int i = 0; i != ilen; ++i) {
                indices[idx++] = isphere[i] + vlen*j;
            }
        }


        //mesh.RecalculateNormals();
    }

	public Mesh build_section_mesh(int[] atom_indicies)
	{
		Mesh meshy = new Mesh ();
		meshy.Clear ();

		int vlen = vsphere.Length;
		int ilen = isphere.Length;

		Vector3[] verts = new Vector3[atom_indicies.Length * vlen];
		Vector3[] normals = new Vector3[atom_indicies.Length * vlen];
		Vector2[] uvs = new Vector2[atom_indicies.Length * vlen];
		Color[] colors = new Color[atom_indicies.Length * vlen];
		int[] indices = new int[atom_indicies.Length * ilen];

		Vector3 offset = new Vector3 ();
		for(int i=0;i<atom_indicies.Length;++i)
		{
			offset+=atom_centres[atom_indicies[i]];
		}
		offset /= atom_indicies.Length;

		int idx = 0;
		for (int i=0; i<atom_indicies.Length; ++i) {
			int index = atom_indicies[i];
			Vector3 pos = atom_centres[index];
			Color col= Color.green;
			float rad = atom_radii[index];

			for(int j=0; j!= vlen; ++j)
			{
				int v=j+i*vlen;
				verts[v] = vsphere[j]*rad + pos - offset;
				normals[v] = vsphere[j];
				uvs[v].x = normals[v].x;
				uvs[v].y = normals[v].y;
				colors[v] = col;
			}
			for (int j = 0; j != ilen; ++j) {
				indices[idx++] = isphere[j]+ i*vlen;
			}
		}

		meshy.vertices = verts;
		meshy.normals = normals;
		meshy.uv = uvs;
		meshy.colors = colors;
		meshy.triangles = indices;

		return meshy;
	}
	
	void construct_unity_meshes (Vector3[] vertices, Vector3[] normals, Vector2[] uvs, Color[] colors, int[] indices)
	{
		int numMesh = 1;

		while ((atom_centres.Length*vsphere.Length / numMesh) > 65000) {
			numMesh += 1;
		}

		int atomsPerMesh = atom_centres.Length / numMesh;

		int vertsPerMesh = atomsPerMesh * vsphere.Length;
		int indiciesPerMesh = atomsPerMesh * isphere.Length;
		   
		mesh = new Mesh[numMesh];
		for (int i = 0; i < mesh.Length; ++i) {
			int vOff = vertsPerMesh*i;
			int iOff = indiciesPerMesh*i;

			int delta = vertices.Length-(vOff +vertsPerMesh);
			if(delta <0)
			{
				Debug.LogError("BAD VERTS PER MESH ASSIGN");
			}


			mesh [i]= new Mesh();
			mesh [i].Clear ();
			mesh [i].name = "ball view" + i;

			Vector3[] v = new Vector3[vertsPerMesh];
			Array.Copy(vertices, vOff, v, 0, vertsPerMesh);
			mesh [i].vertices = v;

			//reusing V as it is the appropriate size
			Array.Copy(normals, vOff, v, 0, vertsPerMesh);
			mesh [i].normals = v;

			Vector2[] u = new Vector2[vertsPerMesh];
			Array.Copy (uvs, vOff, u, 0, vertsPerMesh);
			mesh [i].uv = u;

			Color[] c = new Color[vertsPerMesh];
			Array.Copy(colors, vOff, c, 0, vertsPerMesh);
			mesh [i].colors = c;

			int[] index = new int[indiciesPerMesh];
			Array.Copy(indices, iOff, index, 0, indiciesPerMesh);
			if(i!=0)
			{
				for(int j = 0; j < index.Length; ++j)
				{
					index[j] -= vOff;

				}
			}
			mesh [i].triangles = index;
		}
	}

    static public int encode(char a, char b, char c, char d) {
        return a*0x1000000+b*0x10000+c*0x100+d;
    }

    public static int atom_N = encode(' ', 'N', ' ', ' ');
    public static int atom_O = encode(' ', 'O', ' ', ' ');
    public static int atom_C = encode(' ', 'C', ' ', ' ');

    void build_ribbon_mesh() {
		/*
        debug.WriteLine("building mesh");
        mesh = new Mesh();
        mesh.name = "ribbon view";

        int num_residues = residues.Length;
        int segments = 6;

        Vector3[] vertices = new Vector3[(num_residues+1)*segments];
        Vector3[] normals = new Vector3[(num_residues+1)*segments];
        Vector2[] uvs = new Vector2[(num_residues+1)*segments];
        int[] indices = new int[num_residues*segments*2];
        int v = 0;
        int idx = 0;
        Vector3 a = new Vector3();
        Vector3 b = new Vector3();
        for (int j = 0; j != num_residues; ++j) {
            int r0 = residues[j+0];
            int r1 = residues[j+1];
            a.Set(atoms[r0*4+0], atoms[r0*4+1], atoms[r0*4+2]);
            b.Set(atoms[r1*4+0], atoms[r1*4+1], atoms[r1*4+2]);
            float r = atoms[j*4+3];
            for (int i = 0; i != vlen; ++i) {
                vertices[v] = vsphere[i]*r + pos;
                normals[v] = vsphere[i];
                uvs[v].x = normals[v].x;
                uvs[v].y = normals[v].y;
                ++v;
            }
            for (int i = 0; i != ilen; ++i) {
                indices[idx++] = isphere[i] + vlen*j;
            }
        }
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.uv = uvs;
        mesh.triangles = indices;
        //mesh.RecalculateNormals();
        */
    }

    public class Sorter : IComparer  {
        Vector3 axis;
        Vector3[] atom_centres;

        public Sorter(Vector3 dim, Vector3[] atoms)
        {
            atom_centres = atoms;
            if (dim.x >= dim.y && dim.x >= dim.z)
            {
                axis = new Vector4(1, 0, 0);
            } else if (dim.y >= dim.x && dim.y >= dim.z)
            {
                axis = new Vector4(0, 1, 0);
            } else
            {
                axis = new Vector4(0, 0, 1);
            }
        }

        public int Compare(object x, object y) {
            float a = Vector3.Dot(atom_centres[(int)x], axis);
            float b = Vector3.Dot(atom_centres[(int)y], axis);
            return a < b ? -1 : a > b ? 1 : 0;
        }
    }

    private void partition(int[] index, int b, int e, int addr) {
        if (e == b + 1) {
			int bi = index[b];
			Vector3 centre = atom_centres[bi];
			float radius = atom_radii[bi];
            //debug.WriteLine("[" + addr + "] [T] centre=" + centre + " r=" + radius);
            bvh_terminals[addr] = bi;
            bvh_centres[addr] = centre;
            bvh_radii[addr] = radius;
        }
        else
        {
            Vector3 min = atom_centres[b];
            Vector3 max = min;
            for (int j = b; j != e; ++j)
            {
				int ji = index[j];
                Vector3 r = new Vector3(atom_radii[ji], atom_radii[ji], atom_radii[ji]);
                min = Vector3.Min(min, atom_centres[ji] - r);
                max = Vector3.Max(max, atom_centres[ji] + r);
            }

            Vector3 dim = max - min;
            Vector3 centre = (max + min) * 0.5f;

            Array.Sort(index, b, e-b, new Sorter(dim, atom_centres));

            int mid = b + (e-b)/2;

            float radius = 0;
            for (int j = b; j != e; ++j)
            {
				int ji = index[j];
                Vector3 pos = atom_centres[ji] - centre;
                float r = atom_radii[ji] + pos.magnitude;
                radius = Mathf.Max(radius, r);
            }

            //if (tree.Count < 30) debug.WriteLine("" + (e - b) + " r=" + radius);

            //debug.WriteLine("[" + addr + "] [N] centre=" + centre + " r=" + radius);
            bvh_centres[addr] = centre;
            bvh_radii[addr] = radius;
            bvh_terminals[addr] = -1;
            partition(index, b, mid, addr * 2 + 1);
            partition(index, mid, e, addr * 2 + 2);
        }
    }

    public void build_bvh() {
        int num_atoms = atom_centres.Length;
        int[] index = new int[num_atoms];
        for (int j = 0; j != num_atoms; ++j) {
            index[j] = j;
        }

        int num_bvh = 1;
        while (num_bvh < num_atoms*2) num_bvh *= 2;

        //debug.WriteLine("building bvh for " + name + "with " + num_atoms + " atoms and " + num_bvh + " bvhs");



        bvh_centres = new Vector3[num_bvh];
        bvh_radii = new float[num_bvh];
        bvh_terminals = new int[num_bvh];

        partition(index, 0, num_atoms, 0);
		/*
		for (int i=0; i<num_bvh; ++i) {
			debug.WriteLine("B,"+i+",\""+ bvh_centres[i]+"\","+bvh_terminals[i]+","+bvh_radii[i]);
		}
		for(int i=0; i<num_atoms; ++i)
		{
			debug.WriteLine("A,"+i+",\""+ atom_centres[i]+"\",,"+atom_radii[i]);
		}
        debug.Flush();*/
    }

    public void build_mesh() {
        if (vsphere == null) {
            build_sphere();
        }
        build_bvh();

		Vector3[] verts = new Vector3[0];
		Vector3[] normals = new Vector3[0];
		Vector2[] uvs = new Vector2[0];
		Color[] color = new Color[0];
		int[] index = new int[0];
		if (mode == Mode.Ball) {
			build_ball_mesh (out verts, out normals, out uvs, out color, out index);
			construct_unity_meshes (verts, normals, uvs, color, index);
		} else if (mode == Mode.Metasphere) {
			build_metasphere_mesh(out verts,out normals,out uvs,out color,out index);
			mesh = new Mesh[1];
			mesh [0]= new Mesh();
			mesh [0].Clear ();
			mesh [0].name = "metasphere view" + 0;
			mesh [0].vertices = verts;
			mesh [0].normals = normals;
			mesh [0].uv = uvs;
            mesh [0].colors = color;
			mesh [0].triangles = index;
		} else if (mode == Mode.Ribbon) {
            build_ribbon_mesh();
        }

    }

#if XXX
    public class BvhCollider
    {
        PDB_molecule mol0;
        Transform t0;
        PDB_molecule mol1;
        Transform t1;
        int work_done = 0;

		float radiusInflate=0.0f;
        //static System.IO.StreamWriter debug = new System.IO.StreamWriter(@"D:\BioBlox\BvhCollider.txt");

        public struct Result {
            public int i0;
            public int i1;
            public Result(int i0, int i1) { this.i0 = i0; this.i1 = i1; }
        };

        public List<Result> results;

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

            /*GameObject lhs = GameObject.Find("lhs" + bvh0);
            if (lhs) {
                lhs.transform.localPosition = c0;
                lhs.transform.localScale = new Vector3(r0*2, r0*2, r0*2);
            }

            GameObject rhs = GameObject.Find("rhs" + bvh1);
            if (rhs) {
                rhs.transform.localPosition = c1;
                rhs.transform.localScale = new Vector3(r1*2, r1*2, r1*2);
            }*/

            //debug.WriteLine("[" + bvh0 + ", " + bvh1 + "] c0=" + c0 + " c1=" + c1 + " d0=" + (c0 - c1).sqrMagnitude + " d1=" + (r0 + r1)*(r0 + r1));
            //debug.Flush();
            if ((c0 - c1).sqrMagnitude < (r0 + r1)*(r0 + r1) && r0 > 0 && r1 > 0)
            {
                if (bt0 == -1) {
                    if (bt1 == -1) {
                        collide_recursive(bvh0*2+1, bvh1*2+1);
                        collide_recursive(bvh0*2+1, bvh1*2+2);
                        collide_recursive(bvh0*2+2, bvh1*2+1);
                        collide_recursive(bvh0*2+2, bvh1*2+2);
                    }
                    else
                    {
                        collide_recursive(bvh0*2+1, bvh1);
                        collide_recursive(bvh0*2+2, bvh1);
                    }
                }
                else
                {
                    if (bt1 == -1) {
                        collide_recursive(bvh0, bvh1*2+1);
                        collide_recursive(bvh0, bvh1*2+2);
                    }
                    else
                    {
                        results.Add(new Result(bt0, bt1));
                    }
                }
            }
        }

        public BvhCollider(PDB_molecule mol0, Transform t0, PDB_molecule mol1, Transform t1)
        {
            //debug.WriteLine("colliding " + mol0.name + "/" + mol0.bvh_centres.Length + " with " + mol1.name + "/" + mol1.bvh_centres.Length);
            this.mol0 = mol0;
            this.t0 = t0;
            this.mol1 = mol1;
            this.t1 = t1;
            results = new List<Result>();
            collide_recursive(0, 0);
            //Debug.Log("hits=" + results.Count + " work=" + work_done + "/" + ((mol0.atom_centres.Length + 1) * mol1.atom_centres.Length / 2));
        }
		public BvhCollider(PDB_molecule mol0, Transform t0, PDB_molecule mol1, Transform t1,float inflation)
		{
			this.mol0 = mol0;
			this.t0 = t0;
			this.mol1 = mol1;
			this.t1 = t1;
			results = new List<Result>();
			radiusInflate=inflation;
			collide_recursive(0, 0);
		}

    };

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
#endif

	static public int collide_ray(
		GameObject obj, PDB_molecule mol, Transform t,
		Ray ray)
	{
		BvhRayCollider b = new BvhRayCollider (mol, t, ray);
		int closestIndex = -1;
		float closestDistance = float.MaxValue;
		for (int i=0; i<b.results.Count; i++) {
			BvhRayCollider.Result result = b.results[i];
			Vector3 c = t.TransformPoint(mol.atom_centres[result.index]);
			//float dist = (ray.origin-c).sqrMagnitude;
			float dist = Vector3.Dot(c, ray.direction);
			if (closestDistance > dist)
			{
				closestDistance = dist;
				closestIndex = result.index;
			}
		}
		return closestIndex;
	}

	static public bool collide_ray_quick(
		GameObject obj, PDB_molecule mol, Transform t,
		Ray ray)
	{
		
		Vector3 c = mol.bvh_centres[0];
		float r = mol.bvh_radii[0];

		c = t.TransformPoint (c);
		
		Vector3 q = c - ray.origin;
		float f = Vector3.Dot (q, ray.direction);
		
		float d = 0;
		
		if (f < 0) {
			d = q.sqrMagnitude;
		} else {
			d=(c-(ray.origin+(ray.direction*f))).sqrMagnitude;
		}
		
		if (d < r * r) {
			return true;
		}
		return false;
	}

	static public bool collide(
		GameObject obj0, PDB_molecule mol0, Transform t0,
		GameObject obj1, PDB_molecule mol1, Transform t1
		)
	{
		BvhCollider b = new BvhCollider(mol0, t0, mol1, t1);
		foreach (BvhCollider.Result r in b.results) {
			Vector3 c0 = t0.TransformPoint(mol0.atom_centres[r.i0]);
			Vector3 c1 = t1.TransformPoint(mol1.atom_centres[r.i1]);
			float min_d = mol0.atom_radii[r.i0] + mol1.atom_radii[r.i1];
			float distance = (c1 - c0).magnitude;
			//Debug.Log("distance=" + distance
			if (distance < min_d) {
				return true;
			}
		}
		return false;
	}
	
	static public bool pysics_collide(
		GameObject obj0, PDB_molecule mol0, Transform t0,
		GameObject obj1, PDB_molecule mol1, Transform t1,
		float seperationForce
		)
	{
		BvhCollider b = new BvhCollider(mol0, t0, mol1, t1);
        Rigidbody r0 = obj0.GetComponent<Rigidbody>();
        Rigidbody r1 = obj1.GetComponent<Rigidbody>();
		if (!r0 || !r1) {
			return false;
		}
        foreach (BvhCollider.Result r in b.results) {
            Vector3 c0 = t0.TransformPoint(mol0.atom_centres[r.i0]);
            Vector3 c1 = t1.TransformPoint(mol1.atom_centres[r.i1]);
            float min_d = mol0.atom_radii[r.i0] + mol1.atom_radii[r.i1];
            float distance = (c1 - c0).magnitude;
            //Debug.Log("distance=" + distance
            if (distance < min_d) {
                Vector3 normal = (c0 - c1).normalized * (min_d - distance);
				normal *= seperationForce;
				r0.AddForceAtPosition(normal,c0);
                r1.AddForceAtPosition(-normal, c1);
            }
        }
		if (b.results.Count > 0) {
			return true;
		}
		return false;
    }
};

