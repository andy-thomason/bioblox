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
        Shader shader;

        GameObject goMolA, goMolB;
        GameObject[] goMol;
        MeshFilter[] mfMol;
        MeshRenderer[] mrMol;
        protected PDB_molecule molA, molB;
        protected Vector3 filterpointA = new Vector3(float.NaN, float.NaN, float.NaN);
        protected Vector3 filterpointB = new Vector3(float.NaN, float.NaN, float.NaN);


        public GUIInsideSurface() {
            PDB_parser.automesh = false;
            molB = PDB_parser.get_molecule("pdb2ptcWithTags.2");  // read the (cached) molecule data
            molA = PDB_parser.get_molecule("pdb2ptcWithTags.1");  // read the (cached) molecule data
            CSGControl.QuickUnion = false;
            BasicMeshData.defaultShaderName = "Custom/Color";
        }


        // main function that will be called for display when something interesting has happened
        public override bool Show(string ptoshow) {
            if (base.Show(ptoshow)) return true;

            CSGFMETA.computeCurvature = computeCurvature;
            if (CSGControl.MinLev < 5)
                CSGControl.MinLev = 7;
            CSGControl.MaxLev = CSGControl.MinLev;


            text = "";
            toshow = ptoshow;
            if (testop("mwin")) {
                setmwin();
                return true;
            }
            if (testop("onewin")) {
                setonewin();
                return true;
            }
            if (testop("4lights")) {
                setLights();
                return true;
            }

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
                bool useb = ptoshow == "pdb prepB";
                PDB_molecule mol;

                mol = (useb) ? molB : molA;

                // prepare and populate the metaball object
                csg = meta(mol, radInfluence);
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
                    prepRadinf = radInfluence;

                    // precompute the compacted version (common vertices)
                    tsavemesh = tsavemesh.RemapMesh();
                    tsavemesh = tsavemesh.removeSeparated();
                    Log("mesh external verts={0}, triangles={1}", tsavemesh.vertices.Length, tsavemesh.triangles.Length);

                    // Render outside and inside separately with different colours
                    // It would be more sensible to have a two-sided shader,
                    // but I haven't managed to make a sensible one yet. .... (Stephen 13 Dec 2015)
                    // and the extra cost doesn't seem to significant.
                    // Sensible double-sided shader impeded by Unity bug submitted 8 Jan 2016
                    //    (Case 760127) Incorrect shader generation: normal not passed into surf(), black results
                    if (!useb) {
                        BasicMeshData.ToGame(goMol[Mols.molA], tsavemesh.ToMeshes(), mrMol[Mols.molA].material);
                        BasicMeshData.ToGame(goMol[Mols.molAback], tsavemesh.ToMeshesBack(), mrMol[Mols.molAback].material);
                        savemeshA = tsavemesh;
                    } else {
                        BasicMeshData.ToGame(goMol[Mols.molB], tsavemesh.ToMeshes(), mrMol[Mols.molB].material);
                        BasicMeshData.ToGame(goMol[Mols.molBback], tsavemesh.ToMeshesBack(), mrMol[Mols.molBback].material);
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

            if (testop("intersect")) {
                Log("positions for intersect: A {0} B {1}", molA.pos, molB.pos);
                csg = meta(molA, radInfluence).Colour(7) 
                    * spheres(molB, 2.5f).At(molB.pos - molA.pos).Noshow().Colour(1);
            }

            if (testop("spheres")) {
                csg = spheres(molA);
            }


            if (opdone) showCSGParallel(bounds);
            return opdone;

        }  // Show()

        /// <summary>
        /// prepare metaball for molecule
        /// </summary>
        /// <param name="mol"></param>
        /// <param name="radInfluence"></param>
        /// <returns></returns>
        CSGFMETA meta(PDB_molecule mol, float radInfluence, float radMult = 1) {
            // prepare and populate the metaball object
            CSGFMETA csgm = new CSGFMETA();
            Vector3[] v = mol.atom_centres;
            float[] r = mol.atom_radii;
            for (int i = 0; i < v.Length; i++) {
                csgm.Add(new MSPHERE(v[i], r[i] * radMult, radInfluence));
            }
            return csgm;
        }

        CSGNode spheres(PDB_molecule mol, float radMult = 1) {
            CSGNode csgm = S.NONE;
            Vector3[] v = mol.atom_centres;
            float[] r = mol.atom_radii;
            for (int i = 0; i < v.Length; i++) {
                csgm += new Sphere(v[i], r[i] * radMult);
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
            BasicMeshData.ToGame(goMol[Mols.molAfilt], BigMesh.ToMeshes(), mrMol[Mols.molA].material);
            BasicMeshData.ToGame(goMol[Mols.molAfiltback], BigMesh.ToMeshesBack(), mrMol[Mols.molAback].material);
        }

        void filterB() {
            if (molB == null || savemeshB == null || prepRadinf != radInfluence)
                Show("pdb prepB");

            BigMesh BigMesh = savemeshB.Filter(filterpointB);

            // double-sided filtered surface (again, silly way to do double-sided)
            BasicMeshData.ToGame(goMol[Mols.molBfilt], BigMesh.ToMeshes(), mrMol[Mols.molB].material);
            BasicMeshData.ToGame(goMol[Mols.molBfiltback], BigMesh.ToMeshesBack(), mrMol[Mols.molBback].material);

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

            if (MSlider("MaxNeighbourDist", ref BigMesh.MaxNeighbourDistance, 0, 50)) 
                filter();
            if (MSlider("MustIncludeDistance", ref BigMesh.MustIncludeDistance, 0, 250))
                filter();
            if (MSlider("Detail Level", ref CSGControl.MinLev, 5, 8)) { } //  Show("pdb prep");
            if (MSlider("Curv Map range", ref CurveMapRange, -1, 1))  
                CurveMap();

            if (curcamnum >= 0) {
                for (int i = 0; i < N / 2; i++)
                    if (Mcheck(Mols.name[2 * i], ref shown[curcamnum, i]))
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