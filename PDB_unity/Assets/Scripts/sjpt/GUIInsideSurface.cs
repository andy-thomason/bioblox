using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using CSG;
using CSGNonPlane;
using CSGFIELD;
using Random = UnityEngine.Random;
using Filter;

namespace CSG {
    //[ExecuteInEditMode]
    public class GUIInsideSurface : GUIBits {

        //[Range(1, 8)]
        //public int MaxLev = 10;


        public float outsideDist = 20;  // distance to jump beyond surface when using ';' or '/' to go inside object 
        public float insideDist = 20;  // distance to jump beyond surface when using ';' or '/' to go outside object 
        public float smooth = 0.6f;
        public float metallic = 0.8f;
        public float CurveMapRange = 0.4f;  // range of curvature to map to colours
        public bool computeCurvature = CSGFMETA.computeCurvature;

        // following are class so they can get reused between frames
        CSGFMETA csgm;
        Shader shader;

        GameObject goMolA, goMolB;
        GameObject[] goMol;
        MeshFilter[] mfMol;
        MeshRenderer[] mrMol;
        protected PDB_molecule molA, molB;
        protected Vector3 filterpointA = new Vector3(float.NaN, float.NaN, float.NaN);
        protected Vector3 filterpointB = new Vector3(float.NaN, float.NaN, float.NaN);


        public GUIInsideSurface() {
            molB = PDB_parser.get_molecule("pdb2ptcWithTags.2");  // read the (cached) molecule data
            molA = PDB_parser.get_molecule("pdb2ptcWithTags.1");  // read the (cached) molecule data
        }


        // main function that will be called for display when something interesting has happened
        public override bool Show(string ptoshow) {
            if (base.Show(ptoshow)) return true;
            CSGFMETA.computeCurvature = computeCurvature;

            if (testop("clearMol")) {
                foreach (var mf in mfMol)
                    mf.mesh = null;
                
                foreach ( var go in goMol)
                    DeleteChildren(go);

                return true;
            }

            /** for debugging two faced shaders **/
            if (testop("flipfrontback")) {
                bool a = goMol[Mols.molA].activeSelf;
                goMol[Mols.molA].SetActive(!a);
                goMol[Mols.molAback].SetActive(a);
                return true;
            }
            /**/
            Bounds bounds = new Bounds(Vector3.zero, new Vector3(70, 70, 70));  // same bounds for both molecules
            if (testop("pdb") || testop("pdb prep") || testop("pdb prepB")) {
                if (CSGControl.MinLev < 5) 
                    CSGControl.MinLev = 7;
                CSGControl.MaxLev = CSGControl.MinLev;
                bool useb = ptoshow == "pdb prepB";
                PDB_molecule mol;

                mol = (useb) ? molB : molA;

                // prepare and populate the metaball object
                csgm = new CSGFMETA();
                Vector3[] v = mol.atom_centres;
                float[] r = mol.atom_radii;
                for (int i = 0; i < v.Length; i++) {
                    csgm.Add(new MSPHERE(v[i], r[i]));
                }
                csg = csgm;                         // and save csg in 'standard' named variable
                Volume molvol = csg.Volume();       // find bounding volume   
                // bounds = molvol.BoundsExpand(1.1f); // and get a little leway
                // bounds = new Bounds(Vector3.zero, new Vector3(70,70,70));  // same bounds for both molecules


                // prepare the mesh in a way I can raycast it and filter it
                // common up the common vertices, and if easily possible display
                if (ptoshow.StartsWith("pdb prep")) {
                    var savemeshx = UnityCSGOutput.MeshesFromCsg(csg, bounds, 999999);
                    BigMesh tsavemesh = null;
                    foreach (var s in savemeshx) tsavemesh = s.Value.GetBigMesh();  // should only be one

                    float tf1 = Time.realtimeSinceStartup;
                    Log("pdb prep time={0}, vol used {1}", (tf1 - t0), molvol);


                    Log("basic mesh saved, triangles=" + tsavemesh.triangles.Length);
                    prepRadinf = CSGControl.radInfluence;

                    // precompute the compacted version (common vertices)
                    tsavemesh = tsavemesh.RemapMesh();

                    // Render outside and inside separately with different colours
                    // It would be more sensible to have a two-sided shader,
                    // but I haven't managed to make a sensible one yet. .... (Stephen 13 Dec 2015)
                    // and the extra cost doesn't seem to significant.
                    if (!useb) {
                        BasicMeshData.ToGame(goMol[Mols.molA], tsavemesh.ToMeshes(), "CSGStephen", mrMol[Mols.molA].material);
                        BasicMeshData.ToGame(goMol[Mols.molAback], tsavemesh.ToMeshesBack(), "CSGStephen", mrMol[Mols.molAback].material);
                        savemeshA = tsavemesh;
                    } else {
                        BasicMeshData.ToGame(goMol[Mols.molB], tsavemesh.ToMeshes(), "CSGStephen", mrMol[Mols.molB].material);
                        BasicMeshData.ToGame(goMol[Mols.molBback], tsavemesh.ToMeshesBack(), "CSGStephen", mrMol[Mols.molBback].material);
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

            if  (testop("dock")) {
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
                filterpointA = (molA.atom_centres[ba] + molB.atom_centres[bb]) / 2;
                filterpointB = filterpointA + molA.pos - molB.pos;
                filter();
                return true;
            }

            if (opdone) showCSGParallel(bounds);
            return opdone;

        }  // Show()

        void filter() {
            filterA();
            filterB();
        }

        void filterA() {
            if (molA == null || savemeshA == null || prepRadinf != CSGControl.radInfluence)
                Show("pdb prep");

            //float tf0 = Time.realtimeSinceStartup;
            BigMesh filtermesh = Filter.FilterMesh.Filter(savemeshA, filterpointA);
            //float tf1 = Time.realtimeSinceStartup;
            //Log("mesh filter time=" + (tf1 - tf0));

            // double-sided filtered surface (again, silly way to do double-sided)
            BasicMeshData.ToGame(goMol[Mols.molAfilt], filtermesh.ToMeshes(), "CSGStephen", mrMol[Mols.molA].material);
            BasicMeshData.ToGame(goMol[Mols.molAfiltback], filtermesh.ToMeshesBack(), "CSGStephen", mrMol[Mols.molAback].material);
        }

        void filterB() {
            if (molB == null || savemeshB == null || prepRadinf != CSGControl.radInfluence)
                Show("pdb prepB");

            BigMesh filtermesh = Filter.FilterMesh.Filter(savemeshB, filterpointB);

            // double-sided filtered surface (again, silly way to do double-sided)
            BasicMeshData.ToGame(goMol[Mols.molBfilt], filtermesh.ToMeshes(), "CSGStephen", mrMol[Mols.molB].material);
            BasicMeshData.ToGame(goMol[Mols.molBfiltback], filtermesh.ToMeshesBack(), "CSGStephen", mrMol[Mols.molBback].material);

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

        protected override void UpdateI() {
            if (!findcam()) return;
            
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
                    if (mf.name.StartsWith("molA"))
                        filterA();
                    else if (mf.name.StartsWith("molB"))
                        filterB();
                    else
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


            // perform standard update options
            base.UpdateI();

        }

        // application specific parts of GUI code
        protected override void OnGUII() {
            setobjs();
            //GUIBits.text = "";

            if (MSlider("MaxNeighbourDistance", ref FilterMesh.MaxNeighbourDistance, 0, 50)) 
                filter();
            if (MSlider("MustIncludeDistance", ref FilterMesh.MustIncludeDistance, 0, 50))
                filter();          
            if (MSlider("Detail Level", ref CSGControl.MinLev, 5, 7))
                Show("pdb prep");
            if (MSlider("Curv Map range", ref CurveMapRange, -1, 1))
                CurveMap();

            base.OnGUII();
        }

        private void CurveMap() {
            for (int i = 0; i < N; i++) {
                mrMol[i].material.SetFloat("_Range", CurveMapRange);
            }
        }

        int N = 8;
        private void setobjs() { 
            if (goMol != null) return;
            if (!shader) shader = Shader.Find("Custom/Color");    // Set shader

            Log2("gomol changed {0}", Time.realtimeSinceStartup + "");
            goMol = new GameObject[N];
            mfMol = new MeshFilter[N];
            mrMol = new MeshRenderer[N];

            goMolA = GameObject.Find("MolAH");
            if (goMolA == null) goMolA = new GameObject("MolAH");
            goMolB = GameObject.Find("MolBH");
            if (goMolB == null) goMolB = new GameObject("MolBH");


            for (int i = 0; i < N; i++) {
                goMol[i] = GameObject.Find(Mols.name[i]);
                if (goMol[i] == null) {
                    goMol[i] = new GameObject(Mols.name[i]);
                    goMol[i].transform.parent = Mols.name[i].Contains("molA") ? goMolA.transform : goMolB.transform;
                    goMol[i].layer = i + 8;
                    mfMol[i] = goMol[i].AddComponent<MeshFilter>();
                    mrMol[i] = goMol[i].AddComponent<MeshRenderer>();
                    Material mat = new Material(shader);
                    mat.color = Mols.colors[i];
                    mat.SetColor("_Albedo", Mols.colors[i]);
                    mat.SetFloat("_Glossiness", smooth);
                    mat.SetFloat("_Metallic", metallic);
                    mat.SetFloat("_LowR", -1);
                    mat.SetFloat("_HighR", 1);
                    mat.SetFloat("_LowG", -1);
                    mat.SetFloat("_HighG", 1);
                    mat.SetFloat("_LowB", -1000);
                    mat.SetFloat("_HighB", 1000);
                    mat.SetFloat("_Brightness", 2.5f);
                    mrMol[i].material = mat;
                } else {
                    mfMol[i] = goMol[i].GetComponent<MeshFilter>();
                    mrMol[i] = goMol[i].GetComponent<MeshRenderer>();
                }
            }
        }
    } // class GUIInsideSurface


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