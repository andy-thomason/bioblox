using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using CSG;
using CSGNonPlane;
using CSGFIELD;
using Random = UnityEngine.Random;

namespace CSG {
    //[ExecuteInEditMode]
    public class GUIInsideSurface : GUIBits {

        //[Range(1, 8)]
        //public int MaxLev = 10;

        [Range(0.5f, 2)]  public float radMult = 1f;  // controls the radius multiplier of the spheres/metaballs,
        [Range(0.5f, 2)]  public float radAdd = 0f;  // controls the radius multiplier of the spheres/metaballs,

        [Range(-1, 1)]
        public float radShrink = 0f;  // amounmt to shrink towards sphere centre


        public float outsideDist = 20;  // distance to jump beyond surface when using ';' or '/' to go inside object 
        public float insideDist = 20;  // distance to jump beyond surface when using ';' or '/' to go outside object 
        public int detailLevel = 7;
        public float smooth = 0.6f;
        public float metallic = 0.8f;
        public float CurveMapRange = 0.4f;  // range of curvature to map to colours
        public bool computeCurvature = CSGFMETA.computeCurvature;

        // following are class so they can get reused between frames
        Shader shader;

        GameObject goMolA, goMolB;
        GameObject[] goMol;
        MeshFilter[] mfMol;
        // MeshRenderer[] mrMol;
        protected PDB_molecule molA, molB;
        protected Vector3 filterpointA = new Vector3(float.NaN, float.NaN, float.NaN);
        protected Vector3 filterpointB = new Vector3(float.NaN, float.NaN, float.NaN);

        protected CSGFMETA csgfmetaA, csgfmetaB;
        protected Vector3[] vertAfilt, vertBfilt;


        public GUIInsideSurface() { init(); }
        private void init() { 
            PDB_parser.automesh = false;
            molB = PDB_parser.get_molecule("pdb2ptcWithTags.2");  // read the (cached) molecule data
            molA = PDB_parser.get_molecule("pdb2ptcWithTags.1");  // read the (cached) molecule data
            if (molA == null || molB == null) LogK("mol load", "molecule data missing");
            CSGControl.QuickUnion = false;
            BasicMeshData.defaultShaderName = "Custom/Color";
        }


        // main function that will be called for display when something interesting has happened
        public override bool Show(string ptoshow) {
            if (base.Show(ptoshow)) return true;
            CSGFMETA.computeCurvature = computeCurvature;
            CSGFMETA.radShrink = radShrink;
            int smin = CSGControl.MinLev, smax = CSGControl.MaxLev;
            CSGControl.MaxLev = CSGControl.MinLev = detailLevel;
            bool r = true;
            try {
                r = IShow(ptoshow);
            } finally {
                CSGControl.MinLev = smin; CSGControl.MaxLev = smax;
            }
            return r;
        }

        private bool IShow(string ptoshow) {
            if (molA == null) init();  // for some reason, does not get called from GUIInsideSurface() in build version



            text = "";
            toshow = ptoshow;
            if (testop("mwin", "Set up multiple windows.")) { setmwin(); return true; }
            if (testop("onewin", "Set up single window.")) { setonewin(); return true; }
            // if (testop("4lights", "Set up standard 4 lights")) { setLights();  return true; }

            if (testop("clearMol", "Clear the molecular associated game objects (MolA etc")) {
                foreach (var mf in mfMol)
                    mf.mesh = null;

                foreach (var go in goMol)
                    DeleteChildren(go);

                meshverts.Clear();
                return true;
            }

            /** for debugging two faced shaders **/
            if (testop("flipfrontback", "Flip visibility of front and back,  for debugging two faced shaders")) {
                bool a = goMol[Mols.molA].activeSelf;
                goMol[Mols.molA].SetActive(!a);
                goMol[Mols.molAback].SetActive(a);
                return true;
            }

            //  if (testop("FindStacks")) {
            //      Dictionary<int, string[]> p = Probe.Probe.FindStacks();
            //      Log("Findtacks #' {0}", p.Count);
            //  }

            if (testop("BioBloxMesh", "Run the BioBlox mesh generation for comparison")) {
                useBioBloxMesh(molA);
                return true;
            }

            // note, clear because running any test with outstanding graphics considerably slows down CSG preparation thread
            if (testop("pdb TEST4", "run 4 parallel tests")) { clear(); for (int i = 0; i < 4; i++) IShow("pdb TEST"); return true; }
            if (testop("pdb TEST10", "run 10 parallel tests")) { clear();  for (int i = 0; i < 10; i++) IShow("pdb TEST"); return true; }
            /**/
            Bounds bounds = new Bounds(Vector3.zero, new Vector3(64, 64, 64));  // same bounds for both molecules
            if (testop("pdb TEST", "generate sample pdb" ) || testop("pdb prep", "prepare mesh for molecule A") || testop("pdb prepB", "prepare mesh for molecule B")) {
                bool useb = ptoshow == "pdb prepB";
                PDB_molecule mol;

                mol = (useb) ? molB : molA;

                // prepare and populate the metaball object
                csg = meta(mol, radInfluence, radMult, radAdd);
                Volume molvol = csg.Volume();       // find bounding volume   
                // bounds = molvol.BoundsExpand(1.1f); // and get a little leway
                // bounds = new Bounds(Vector3.zero, new Vector3(70,70,70));  // same bounds for both molecules


                // prepare the mesh in a way I can raycast it and filter it
                // common up the common vertices, and if easily possible display
                if (ptoshow.StartsWith("pdb prep")) {
                    //Probe.Probe.StartProbe("MeshesFromCsg");  // freezes in both edit game mode and built run mode
                    var savemeshx = UnityCSGOutput.MeshesFromCsg(csg, bounds, 999999, detailLevel, detailLevel);
                    //Probe.Probe.StopProbe();

                    BigMesh tsavemesh = null;
                    foreach (var s in savemeshx) tsavemesh = s.Value.GetBigMesh();  // should only be one

                    float tf1 = Time.realtimeSinceStartup;
                    Log("pdb prep time={0}, vol used {1}", (tf1 - t0), molvol);


                    Log("basic mesh saved, triangles=" + tsavemesh.triangles.Length);
                    if (BasicMeshData.CheckWind) {
                        Log("wrong winding=" + BasicMeshData.WrongWind + " very wrong winding="
                        + BasicMeshData.VeryWrongWind + " right winding=" + BasicMeshData.RightWind
                        + " model=" + toshow);
                        //CSGPrim.showbm();
                    }

                    prepRadinf = radInfluence;

                    // precompute the compacted version (common vertices)
                    tsavemesh = tsavemesh.RemapMesh();
                    //tsavemesh.unmatched();  // debug
                  //  if (radShrink != 0)
                  //      shrink(tsavemesh.vertices, tsavemesh.uv, (CSGFMETA)csg);
                    tsavemesh = tsavemesh.removeSeparated();
                    Log("mesh external verts={0}, triangles={1}", tsavemesh.vertices.Length, tsavemesh.triangles.Length);

                    // Render outside and inside separately with different colours
                    // It would be more sensible to have a two-sided shader,
                    // but I haven't managed to make a sensible one yet. .... (Stephen 13 Dec 2015)
                    // and the extra cost doesn't seem to significant.
                    // Sensible double-sided shader impeded by Unity bug submitted 8 Jan 2016
                    //    (Case 760127) Incorrect shader generation: normal not passed into surf(), black results
                    if (!useb) {
                        BasicMeshData.ToGame(goMol[Mols.molA], tsavemesh.ToMeshes());
                        BasicMeshData.ToGame(goMol[Mols.molAback], tsavemesh.ToMeshesBack());
                        csgfmetaA = (CSGFMETA)csg;
                        shrinkA();
                        savemeshA = tsavemesh;
                    } else {
                        BasicMeshData.ToGame(goMol[Mols.molB], tsavemesh.ToMeshes());
                        BasicMeshData.ToGame(goMol[Mols.molBback], tsavemesh.ToMeshesBack());
                        csgfmetaB = (CSGFMETA)csg;
                        shrinkB();
                        savemeshB = tsavemesh;
                    }
                    Color cmax = tsavemesh.colors.Aggregate(new Color(-999,-999,-999,-999), (Color c1, Color c2) => c1.Max(c2));
                    Color cmin = tsavemesh.colors.Aggregate(new Color(999, 999, 999, 999), (Color c1, Color c2) => c1.Min(c2));
                    // LogK("curv range", "min {0} max {1}", cmin, cmax);
                    Log2("curv range min {0} max {1}", cmin, cmax);
                    //curv range min RGBA(-3.402, -10.734, -10.353, -4.928) max RGBA(2.687, 0.481, 18.382, 0.527)

                    return true; // 
                }
            }

            if  (testop("dock", "set molB in docking position to MolA")) { dock(); return true; }

            if (testop("roiA", "Prepare mesh for docking area of A")) { roiA(bounds); return true; }
            //if (testop("shrinkA")) { shrinkA(); return true; }
            if (testop("roiB", "Prepare mesh for docking area of B")) { roiB(bounds); return true; }
            //if (testop("shrinkB")) { shrinkB(); return true; }
            if (testop("overlap", "Show the int4ersection between spheres for A and B")) { intersect(bounds); return true; }
            if (testop("holes", "Show the holes between A and B in the docking area")) { holes(bounds); return true; }
            if (testop("copyProps", "Copy the properties from material for MolAH into other molecule materials")) { copyProps(); return true; }

            if (testop("spheres", "Show sphere represnetation for A")) {
                BasicMeshData.defaultShaderName = "Standard";
                csg = spheres(molA, radMult, radAdd);
            }

            if (testop("triangles", "Show all triangles (internal and external) for 'close' sphere triples (SLOW, DEAD? EXPERIMENT)")) {
                //BasicMeshData.defaultShaderName = "Standard";
                BigMesh bm = triangles(molA, radInfluence / 4, radMult, radAdd);
                BasicMeshData.ToGame(goTest, bm.ToMeshes());
                return true;
            }

            if (testop("unmatched", "show the 'holes' inside mol A usually discarded")) { savemeshA.unmatched(); return true; }
            // if (testop("resetPolyEdges")) { CSGPrim.resetPolyEdges(); return true; }

            if (opdone) showCSGParallel(csg, bounds, goTest);
            return opdone;

        }  // Show()

        void dock() {
            Log("positions: A {0} B {1}", molA.pos, molB.pos);
            goMolB.transform.position = molB.pos - molA.pos;

            int ba = -999, bb = -999; float bd = 999999; float d;
            for (int a = 0; a < molA.atom_centres.Length; a++) {
                for (int b = 0; b < molB.atom_centres.Length; b++) {
                    if ((d = molA.atom_centres[a].Distance(molA.atom_centres[b])) < bd) {
                        ba = a; bb = b;
                        bd = d;
                    }
                }
            }
            //filterpointA = (molA.atom_centres[ba] + molB.atom_centres[bb]) / 2;
            //filterpointB = filterpointA + molA.pos - molB.pos;
            //filter();
        }

        void roiA(Bounds bounds) {
            Log("positions for intersect: A {0} B {1}", molA.pos, molB.pos);
            csgfmetaA = meta(molA, radInfluence, radMult, radAdd);
            csg = csgfmetaA
                * spheres(molB, 2.5f, 0).At(molB.pos - molA.pos).Noshow().Colour(1);
            showCSGParallel(csg, bounds, goMol[Mols.molAfilt], goMol[Mols.molAfiltback], after: shrinkAPrep);

            //remap(goMol[Mols.molAfilt].transform);
            //remap(goMol[Mols.molAfiltback].transform);
            //savevert(goMol[Mols.molAfilt].transform);
            //savevert(goMol[Mols.molAfiltback].transform);
        }

        void shrinkAPrep() {
            remap(goMolA.transform);
            shrinkA();
        }

        void shrinkA() {
            shrink(goMolA.transform, csgfmetaA);
        }

        void shrinkBPrep() {
            remap(goMolB.transform);
            shrinkB();
        }

        void shrinkB() {
            shrink(goMolB.transform, csgfmetaB);
        }


        void roiB(Bounds bounds) {
            Log("positions for intersect: B {0} B {1}", molA.pos, molB.pos);
            csgfmetaB = meta(molB, radInfluence, radMult, radAdd);

            csg = csgfmetaB
                * spheres(molA, 2.5f, 0).At(molA.pos - molB.pos).Noshow().Colour(1);
            showCSGParallel(csg, bounds, goMol[Mols.molBfilt], goMol[Mols.molBfiltback], after: shrinkBPrep);
        }

        void intersect(Bounds bounds) {
            csg = spheres(molA, radMult, radAdd)
                * spheres(molB, radMult, radAdd).At(molB.pos - molA.pos).Colour(1);
            showCSGParallel(csg, bounds, goMol[Mols.molAfilt], goTest);
        }

        void holes(Bounds bounds) {
            float rroi = 2;
            CSGNode roi = spheres(molA, rroi, 0)
                * spheres(molB, rroi, 0).At(molB.pos - molA.pos);
            csg = roi - spheres(molA, radMult, radAdd) - spheres(molB, radMult, radAdd).At(molB.pos - molA.pos);

            showCSGParallel(csg, bounds, goMol[Mols.molAfilt], goTest);
        }


        /// <summary>
        /// prepare metaball for molecule
        /// </summary>
        /// <param name="mol"></param>
        /// <param name="radInfluence"></param>
        /// <returns></returns>
        CSGFMETA meta(PDB_molecule mol, float radInfluence, float pradMult, float pradAdd) {
            if (mol == null) { LogK("mol load", "molecule data missing"); return null; }
            // prepare and populate the metaball object
            Log("in meta, mol {0}", mol);
            CSGFMETA csgm = new CSGFMETA();
            Log("in meta, csgm {0}", csgm);
            Vector3[] v = mol.atom_centres;
            float[] r = mol.atom_radii;
            for (int i = 0; i < v.Length; i++) {
                csgm.Add(new MSPHERE(v[i], r[i] * pradMult + pradAdd, radInfluence));
            }
            return csgm;
        }

        /// <summary>
        /// compute triangles for the molecule based on ALL triples close enough to be possible 'surface triples' for SAS
        /// temporary inefficient algorithm, finds all inside triples as well
        /// </summary>
        /// <param name="mol"></param>
        /// <param name="probeRad"></param>
        /// <param name="pradMult"></param>
        /// <returns></returns>
        BigMesh triangles(PDB_molecule mol, float probeRad, float pradMult, float pradAdd) {
            float probeDi = 2 * probeRad;
            Vector3[] v = mol.atom_centres;
            float[] r = mol.atom_radii;

            List<Vector3> vertices = new List<Vector3>();
            List<Vector3> normals = new List<Vector3>();
            List<Color> colours = new List<Color>();
            List<Vector2> uvs = new List<Vector2>();
            List<int> indices = new List<int>();
            int V = 0;  // # vertices
            Color W = Color.white;
            for (int i = 0; i < v.Length; i++) {
                float ri = r[i] * pradMult + pradAdd;
                for (int j = 0; j < v.Length; j++) {
                    float rj = r[j] * pradMult + pradAdd;
                    if (v[i].Distance(v[j]) > ri + rj + probeDi) continue;
                    for (int k = 0; k < v.Length; k++) {
                        float rk = r[k] * pradMult + pradAdd;
                        if (v[i].Distance(v[k]) > ri + rk + probeDi) continue;
                        if (v[j].Distance(v[k]) > rj + rk + probeDi) continue;
                        Vector3 n = (v[i] - v[j]).cross(v[i] - v[k]).Normal();

                        // insert two triangles
                        vertices.Add(v[i]); colours.Add(W); normals.Add(n); uvs.Add(new Vector2(j, k)); indices.Add(V++);
                        vertices.Add(v[j]); colours.Add(W); normals.Add(n); uvs.Add(new Vector2(k, i)); indices.Add(V++);
                        vertices.Add(v[k]); colours.Add(W); normals.Add(n); uvs.Add(new Vector2(i, j)); indices.Add(V++);

                        vertices.Add(v[i]); colours.Add(W); normals.Add(-n); uvs.Add(new Vector2(k, j)); indices.Add(V++);
                        vertices.Add(v[k]); colours.Add(W); normals.Add(-n); uvs.Add(new Vector2(j, i)); indices.Add(V++);
                        vertices.Add(v[j]); colours.Add(W); normals.Add(-n); uvs.Add(new Vector2(i, k)); indices.Add(V++);

                    }
                }
            }
            LogK("triangles", "{0} t={1}", V / 3, Time.realtimeSinceStartup - t0);
            return new BigMesh(vertices.ToArray(), normals.ToArray(), uvs.ToArray(), colours.ToArray(), indices.ToArray());
        }

        /// <summary>
        ///  shrink mesh in place
        /// </summary>
        /// <param name="mesh"></param>
        /// <param name="meta"></param>
        void shrink(Vector3[] vertices, Vector2[] uv, CSGFMETA meta) {
            if (meta == null || radShrink == 0) return;
            for (int i=0; i < vertices.Length; i++) {
                vertices[i] -= meta.sphereAt((int)uv[i].x).Normal(vertices[i]) * radShrink;
            }
            // p -= near.Normal(p) * radShrink; // defer till all combined
        }

        Dictionary<string, Vector3[]> meshverts = new Dictionary<string, Vector3[]>();

        void bshrink(Mesh mesh, CSGFMETA meta) {
            if (meta == null || radShrink == 0) return;
            Vector3[] vertices = mesh.vertices;
            if (vertices.Length == 0) return;
            Vector2[] uv = mesh.uv;
            if (!meshverts.ContainsKey(mesh.name) || meshverts[mesh.name].Length != mesh.vertices.Length)
                meshverts[mesh.name] = (Vector3[])mesh.vertices.Clone();
            Vector3[] invertices = meshverts[mesh.name];

            for (int i = 0; i < vertices.Length; i++) {
                Vector3 in3 = invertices[i];
                vertices[i] = in3 - meta.sphereAt((int)uv[i].x).Normal(in3) * radShrink;
            }
            mesh.vertices = vertices;
            // p -= near.Normal(p) * radShrink; // defer till all combined
        }


        void shrink(Transform tr, CSGFMETA meta) {
            if (meta == null || radShrink == 0) return;
            MeshFilter mf = tr.GetComponent<MeshFilter>();
            if (mf != null) {
                //Vector3[] vertices = mf.mesh.vertices;
                mf.mesh.name = "mesh.." + mf.name;
                bshrink(mf.mesh, meta);
                //mf.mesh.vertices = vertices;
            }
            for (int i = 0; i < tr.childCount; i++)
                shrink(tr.GetChild(i), meta);
        }

        void remap(Transform tr) {
            MeshFilter mf = tr.GetComponent<MeshFilter>();
            if (mf != null) {
                BigMesh bm = new BigMesh(mf.mesh);
                bm = bm.RemapMesh();
                mf.mesh = bm.ToMesh();
            }
            for (int i = 0; i < tr.childCount; i++)
                remap(tr.GetChild(i));
        }

        CSGNode spheres(PDB_molecule mol, float radMult, float radAdd) {
            CSGNode csgm = S.NONE;
            Vector3[] v = mol.atom_centres;
            float[] r = mol.atom_radii;
            for (int i = 0; i < v.Length; i++) {
                csgm += new Sphere(v[i], r[i] * radMult + radAdd).Colour(i);
            }
            csgm = ((Union)csgm).balance();
            return csgm;
        }

        void filter() {
            filterA();
            filterB();
        }

        void filterA() {
            if (molA == null || savemeshA == null || prepRadinf != radInfluence)
                Show("pdb prep");

            //float tf0 = Time.realtimeSinceStartup;
            BigMesh BigMesh = savemeshA.Filter(filterpointA);
            //float tf1 = Time.realtimeSinceStartup;
            //Log("mesh filter time=" + (tf1 - tf0));

            // double-sided filtered surface (again, silly way to do double-sided)
            BasicMeshData.ToGame(goMol[Mols.molAfilt], BigMesh.ToMeshes()); 
            BasicMeshData.ToGame(goMol[Mols.molAfiltback], BigMesh.ToMeshesBack());
        }

        void filterB() {
            if (molB == null || savemeshB == null || prepRadinf != radInfluence)
                Show("pdb prepB");

            BigMesh BigMesh = savemeshB.Filter(filterpointB);

            // double-sided filtered surface (again, silly way to do double-sided)
            BasicMeshData.ToGame(goMol[Mols.molBfilt], BigMesh.ToMeshes());
            BasicMeshData.ToGame(goMol[Mols.molBfiltback], BigMesh.ToMeshesBack());

        }


        /// <summary>
        /// check if the given molecule part is shown in given camera view
        /// </summary>
        /// <param name="c"></param>
        /// <param name="molid"></param>
        /// <returns></returns>
        protected bool isShown(Camera c, int molid) {
            return ( c.cullingMask & (1 << (molid + 8)) ) != 0;
        }

        /// <summary>
        /// set the visibility of the given molecule part for the given camera view
        /// </summary>
        /// <param name="c"></param>
        /// <param name="molid"></param>
        /// <param name="s"></param>
        protected void show(Camera c, int molid, bool s) {
            if (s)
                c.cullingMask |= (1 << (molid + 8));
            else
                c.cullingMask &= ~ (1 << (molid + 8));
        }

        /// <summary>
        /// show just the given modelule parts in the given camera view
        /// </summary>
        /// <param name="c"></param>
        /// <param name="molid"></param>
        protected void show(Camera c, params int[] molids) {
            c.cullingMask = 0;
            foreach (var molid in molids)
                show(c, molid, true);
        }

        int frameNum = 0;
        protected override void UpdateI() {
            frameNum++;
            // perform standard update options
            base.UpdateI();

            if (curcamnum < 0) return;

            if (Input.GetKey("q") && curcam == Cameras[0]) {
                Vector3 h = Hitpoint();
                if (!float.IsNaN(h.x) && Cameras.Length > 1) {
                    Camera cam = Cameras[1];
                    cam.transform.position = h + Camera.main.transform.forward * insideDist;
                    cam.transform.rotation = Camera.main.transform.rotation;
                    cam.transform.Rotate(new Vector3(0, 180, 0));
                }
            }


            // custom update actions that apply only to this application
            if (Input.GetKeyDown(KeyCode.Semicolon)) {  // go to other side of surface and keep looking same way
                Vector3 h = Hitpoint();
                if (!float.IsNaN(h.x))
                    lookat = Camera.main.transform.position = h + Camera.main.transform.forward * outsideDist;
            }

            if (Input.GetKeyDown("/")) {   // go to other side of surface and look back
                Vector3 h = Hitpoint();
                if (!float.IsNaN(h.x)) {
                    lookat = Camera.main.transform.position = h + Camera.main.transform.forward * insideDist;
                    Camera.main.transform.Rotate(new Vector3(0, 180, 0));
                }
            }

            if (Input.GetKeyDown("x")) {
                if (isShown(curcam, Mols.molAfilt)) {  // viewports showing A will always show the filtered, so use that as A/B test
                    bool act = !isShown(curcam, Mols.molA);   //  switch visibility of non-filtered A (front and back)
                    show(curcam, Mols.molA, act);
                    show(curcam, Mols.molAback, act);
                }
                if (isShown(curcam, Mols.molBfilt)) {  // viewports showing A will always show the filtered, so use that as A/B test
                    bool act = !isShown(curcam, Mols.molB);   //  switch visibility of non-filtered B (front and back)
                    show(curcam, Mols.molB, act);
                    show(curcam, Mols.molBback, act);
                }
            }

            if (Input.GetKey("q")) {
                if (savemeshA == null)
                    Show("pdb prep");
                int t; MeshFilter mf;
                Hitpoint(out t, out mf);  // sets hitpoint as side effect
                //Log ("hitp" + hitpoint);
                if (!float.IsNaN(hitpoint.x)) {
                    lookat = hitpoint;
                    if (mf.name.StartsWith("molA")) {
                        filterpointA = hitpoint;
                        filterA();
                    } else if (mf.name.StartsWith("molB")) {
                        filterpointB = hitpoint;
                        filterB();
                    }  else
                        Log("unexpected object hit for key 'q' {0}", mf.name);

                } else {
                    mfMol[Mols.molAfilt].mesh = null;
                    mfMol[Mols.molAfiltback].mesh = null;
                }

                savedTransform = Camera.main.transform.SaveData();
            }

            if (Input.GetKey("c")) {
                int triangleNumber;
                MeshFilter mf;
                hitpoint = Hitpoint(out triangleNumber, out mf);  //   // sets hitpoint as side effect
                if (triangleNumber == -1) { LogK("curvhit", "miss"); return; }
                Mesh mesh = mf.mesh;
                int vertid = mesh.triangles[triangleNumber];  // take one vertex for now
                LogK("curvhit", mesh.colors[vertid] + "");

            }

            if (Input.GetKeyDown("0")) { shownother[curcamnum] = !shownother[curcamnum]; setshow(); }
            if (Input.GetKeyDown("1")) { shown[curcamnum, 0] = !shown[curcamnum, 0]; setshow(); }
            if (Input.GetKeyDown("2")) { shown[curcamnum, 1] = !shown[curcamnum, 1]; setshow(); }
            if (Input.GetKeyDown("3")) { shown[curcamnum, 2] = !shown[curcamnum, 2]; setshow(); }
            if (Input.GetKeyDown("4")) { shown[curcamnum, 3] = !shown[curcamnum, 3]; setshow(); }

            if (Input.GetKeyDown("-")) {
                shown[curcamnum, 0] = !shown[curcamnum, 0];
                shownother[curcamnum] = !shown[curcamnum, 0];
                setshow();
            } // swap 

            autoCopyProps();

        }

        public bool[,] shown  = new bool[,] {   // whether shown, for each camera and each mol type
                    { true, true, true, true},
                    { false, false, true, false},
                    { false, true, false, true},
                    { false, false, false, true}
                };
        public bool[] shownother = new bool[] { true, false, false, false };  // whether shown, for each camera and other

        // application specific parts of GUI code
        protected override void OnGUII() {
            base.OnGUII();

            setobjs();
            //GUIBits.text = "";

            MSlider("RadInfluence", ref radInfluence, 1.1f, 5, 2.5f);
            MSlider("RadMult", ref radMult, 0.25f, 3, 1);
            MSlider("RadAdd", ref radAdd, -2, 2, 0);
            if (MSlider("RadShrink", ref radShrink, -2, 2, 0)) {
                shrinkA();
                shrinkB();
            };
            if (MSlider("MaxNeighbourDist", ref BigMesh.MaxNeighbourDistance, 0, 50, 20)) 
                filter();
            if (MSlider("MustIncludeDistance", ref BigMesh.MustIncludeDistance, 0, 250, 20))
                filter();
            if (MSlider("Detail Level", ref detailLevel, 0, 10, 7)) { } //  Show("pdb prep");
            if (MSlider("Curv Map range", ref CurveMapRange, -1, 1, 0.4f))  
                CurveMap();
            ////MSlider("gradCompPow", ref CSGPrim.gradCompPow, -10, 10);

            if (curcamnum >= 0) {
                for (int i = 0; i < N / 2; i++)
                    if (Mcheck(Mols.name[2 * i] + (i == 0 ? "_" + curcamnum: ""), ref shown[curcamnum, i]))
                        setshow();
                // for some reason, shownother was getting set to an array of length 0, not sure how ... ???
                if (shownother == null || shownother.Length != 4)
                    shownother = new bool[] { true, false, false, false };
                if (Mcheck("other", ref shownother[curcamnum])) setshow();
            }

        }

        protected void setshow() {
            for (int cam=0; cam < Cameras.Length; cam++) {
                int c = 0;
                for (int i = 0; i < N/2; i++) {
                    if (shown[cam, i]) {
                        c |= 3 << (i * 2 + 8);
                    }
                }
                if (shownother[cam]) c |= 255;  // << show the first 8 'standard' filters
                Cameras[cam].cullingMask = c;
            }
        }

        private void CurveMap() {
            for (int i = 0; i < N; i++) {
                goMol[i].material().SetFloat("_Range", CurveMapRange);
            }
        }

        readonly string[] floatProps = {
            "_Glossiness", "_Brightness", "_Metallic", "_VertexColors",
            "_Range", "_LowR", "_HighR", "_LowG", "_HighG", "_LowB", "_HighB",
            "_P1Step", "_P1Width", "_P2Step", "_P2Width","_PGStep", "_PGWidth","_PMStep", "_PMWidth"
             };
        void copyProps(Material to, Material from) {
            foreach (string s in floatProps)
                to.SetFloat(s, from.GetFloat(s));
        }

        void copyProps(Transform tr, Material from) {
            MeshRenderer mr = tr.GetComponent<MeshRenderer>();
            if (mr != null)
                copyProps(mr.material, from);
    
            for (int i = 0; i < tr.childCount; i++)
                copyProps(tr.GetChild(i), from);
        }

        void autoCopyProps() {
            Material from = goMolA.GetComponent<MeshRenderer>().material;
            if (from.GetFloat("_COPY") != 0)
                copyProps();

        }
        void copyProps() {
            Material from = goMolA.GetComponent<MeshRenderer>().material;
            copyProps(goMolA.transform, from);
            copyProps(goMolB.transform, from);
        }

        int N = 8;
        private void setobjs() { 
            if (goMol != null) return;
            if (!shader) shader = Shader.Find("Custom/Color");    // Set shader

            Log2("gomol changed {0}", Time.realtimeSinceStartup + "");
            goMol = new GameObject[N];
            mfMol = new MeshFilter[N];
            // mrMol = new MeshRenderer[N];

            goMolA = GameObject.Find("MolAH");
            if (goMolA == null) goMolA = new GameObject("MolAH");
            goMolB = GameObject.Find("MolBH");
            if (goMolB == null) goMolB = new GameObject("MolBH");

            // representative top level material, used for copying but not directly
            MeshRenderer mrMolA = goMolA.AddComponent<MeshRenderer>();
            Material matref = new Material(shader);
            matref.SetFloat("_Glossiness", smooth);
            matref.SetFloat("_Metallic", metallic);
            matref.SetFloat("_LowR", -1);
            matref.SetFloat("_HighR", 1);
            matref.SetFloat("_LowG", -1);
            matref.SetFloat("_HighG", 1);
            matref.SetFloat("_LowB", -1000);
            matref.SetFloat("_HighB", 1000);
            matref.SetFloat("_Brightness", 2.5f);
            mrMolA.material = matref;

            for (int i = 0; i < N; i++) {
                goMol[i] = GameObject.Find(Mols.name[i]);
                if (goMol[i] == null) {
                    goMol[i] = new GameObject(Mols.name[i]);
                    goMol[i].transform.parent = Mols.name[i].Contains("molA") ? goMolA.transform : goMolB.transform;
                    goMol[i].layer = i + 8;
                    mfMol[i] = goMol[i].AddComponent<MeshFilter>();
                    MeshRenderer mrMol = goMol[i].AddComponent<MeshRenderer>();
                    Material mat = new Material(shader);
                    mat.color = Mols.colors[i];
                    mat.SetColor("_Albedo", Mols.colors[i]);
                    copyProps(mat, matref);
                    mrMol.material = mat;
                } else {
                    mfMol[i] = goMol[i].GetComponent<MeshFilter>();
                    // mrMol[i] = goMol[i].GetComponent<MeshRenderer>();
                }
            }

            dock();
            setLights();

        }


        static float ddd = 1000;
        static Vector3[] lpos = new Vector3[] { new Vector3(ddd, 0, 0),
            new Vector3(-ddd, 0, 0),
            new Vector3(0, ddd, 0),
            new Vector3(0, -ddd, 0) };

        void setLights() {
            foreach (Light go in UnityEngine.Object.FindObjectsOfType(typeof(Light)))
                Destroy(go.transform.gameObject);

            for (int i = 0; i < 4; i++) {
                GameObject go = new GameObject("light" + i);
                Light l = go.AddComponent<Light>();
                l.type = LightType.Point;
                l.range = 1e20F;
                l.color = Color.white;
                l.intensity = 0.7f;
                l.transform.position = lpos[i];
            }

        }

        void setmwin() {
            Camera cam0 = Camera.main;

            if (Cameras.Length == 1) {
                Cameras = new Camera[4];
                Cameras[0] = cam0;
                for (int i = 1; i < 4; i++) {
                    GameObject goCam = new GameObject("cam" + i);
                    Camera cam = Cameras[i] = goCam.AddComponent<Camera>();
                    cam.transform.position = cam0.transform.position;
                    cam.transform.rotation = cam0.transform.rotation;
                    cam.transform.localScale = cam0.transform.localScale;
                    cam.clearFlags = CameraClearFlags.Color;
                    cam.backgroundColor = Color.green;
                    cam.enabled = true;
                }
                cam0.rect = new Rect(0, 0, 0.5f, 0.5f);
                Cameras[1].rect = new Rect(0, 0.5f, 0.5f, 0.5f);
                Cameras[2].rect = new Rect(0.5f, 0, 0.5f, 0.5f);
                Cameras[3].rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);

            }
            shown = new bool[,] {
                    { true, false, true, false},
                    { false, false, true, false},
                    { false, true, false, true},
                    { false, false, false, true}
                };
            shownother = new bool[] { true, false, false, false };
            setshow();
            for (int i = 1; i < Cameras.Length; i++) Cameras[i].enabled = true;
            cam0.rect = new Rect(0, 0, 0.5f, 0.5f);

        }

        void setonewin() {
            Camera.main.rect = new Rect(0, 0, 1, 1);
            for (int i = 1; i < Cameras.Length; i++) Cameras[i].enabled = false;
            for (int i = 0; i < 4; i++) shown[0, i] = true;
            curcamnum = 0;
            setshow();
        }

        public override void home(Transform t) {
            t.rotation = new Quaternion(0, 0, 0, 1);
            t.position = new Vector3(0, 11, -70);
        }

        protected override GameObject getSubObject() { return goMolB; }

        BigMesh useBioBloxMesh(PDB_molecule mol) {
#if CSGUNITY
            Log("useBioBloxMesh not supported in CSGUNITY");
            return null;
#else
            int save = CSGControl.MinLev;
            CSGControl.MinLev = -1;
            CSGFMETA csg = meta(mol, radInfluence, radMult, radAdd);
            CSGFMETA.computeCurvature = false;  // no need to save, will be reset from local anyway

            Vector3[] vertices; Vector3[] normals; Vector2[] uvs; Color[] colours; int[] indices;
            float spacing = 64f / (1 << detailLevel);
            mol.build_metasphere_mesh(out vertices, out normals, out uvs, out colours, out indices, spacing, csg);
            BigMesh bm = new BigMesh(vertices, normals, uvs, colours, indices);
            Log("mesh bioblox verts={0}, triangles={1}", bm.vertices.Length, bm.triangles.Length);

            bm = bm.RemapMesh();
            Log("mesh bioblox verts={0}, triangles={1}", bm.vertices.Length, bm.triangles.Length);
            bm.unmatched();
            BasicMeshData.ToGame(goTest, bm.ToMeshes());
            CSGControl.MinLev = save;
            return bm;
#endif
        }
    } // class GUIInsideSurface

    /***
    class ParallelCSGMol : ParallelCSG {
        internal ParallelCSGMol(CSGNode csg, Bounds bounds, GameObject front, GameObject back, string toshow, bool parallel, int minLev, int maxLev) :
            base(csg, bounds, front, back, toshow, parallel, minLev, maxLev) {
         }
        protected override void finish() {

        }
    }
    ***/


    static class Mols {
        public static int molA = 0, molAback = 1, molB = 2, molBback = 3, molAfilt = 4, molAfiltback = 5, molBfilt = 6, molBfiltback = 7;
        public static string[] name = { "molA", "molAback", "molB", "molBback", "molAfilt", "molAfiltback", "molBfilt", "molBfiltback" };
        public static Color[] colors = {
            new Color(0.7f, 0.7f, 0.7f), // A
            new Color(0.5f, 1,    0.5f), // Aback
            new Color(0.5f, 0.5f, 0.5f), // B
            new Color(0.5f, 0.5f, 0.5f), // Bback

            new Color(0.8f, 0.5f, 0.5f), // Afilt
            new Color(0.5f, 0.5f, 0.5f), // Afiltback  <<<
            new Color(0.5f, 0.5f, 0.5f), // Bfilt      <<<
            new Color(0.8f, 0.5f, 0.5f)  // Bfiltback
        };
    }

} // namespace CSG