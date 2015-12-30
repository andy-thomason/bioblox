using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

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

        [Range(0, 5)]
        [SerializeField] float radInfluence = 2.5f;  // controls the blobiness of the metaballs,
                            // a sphere of radius r has influence up to distance r*radInfluence

        public float outsideDist = 20;  // distance to jump beyond surface when using ';' or '/' to go inside object 
        public float insideDist = 20;  // distance to jump beyond surface when using ';' or '/' to go outside object 
        public float smooth = 0.66f;        

        // following are class so they can get reused between frames
        CSGFMETA csgm;
        Shader shader;

        GameObject[] goMol;
        MeshFilter[] mfMol;
        MeshRenderer[] mrMol;

        // main function that will be called for display when something interesting has happened
        public override void Show(string ptoshow) {
            text = "";
            toshow = ptoshow;

            // make sure some CSG metaball details correct for this experiment
            BasicMeshData.Sides = 1;             // just show outside surface on main
            BasicMeshData.CheckWind = true;        // check and correct winding
            BasicMeshData.WindShow = 3;            //show all windings, correct = 1 + wrong = 2
            Poly.windDebug = false;                // do not do further winding debug
            BasicMeshData.RightWind = BasicMeshData.WrongWind = BasicMeshData.VeryWrongWind = 0;

            // update local control variables (so they show in Unity editor) 
            // to be reflected in the 'real' places
            CSGControl.radInfluence = radInfluence; MSPHERE.UpdateRadInfluence();

            // In order to use sphere etc we must force subdivision to some level
            // or objects are just 'lost'
            float t0 = Time.realtimeSinceStartup;
            
            csg = S.NONE;

            if (testop("clear")) {  // clear button
                DeleteChildren(goTest);
                DeleteChildren(goFiltered);
                return;
            } 
            Bounds bounds = new Bounds();

            if (testop("pdb") || testop("pdb prep")) {
                if (CSGControl.MinLev < 5) 
                    CSGControl.MinLev = CSGControl.MinLev = 7;
                CSGControl.MaxLev = CSGControl.MinLev;

                mol = PDB_parser.get_molecule("pdb2ptcWithTags.1");  // read the (cached) molecule data

                // prepare and populate the metaball object
                csgm = new CSGFMETA();
                Vector3[] v = mol.atom_centres;
                float[] r = mol.atom_radii;
                for (int i = 0; i < v.Length; i++) {
                    csgm.Add(new MSPHERE(v[i], r[i]));
                }
                csg = csgm;                         // and save csg in 'standard' named variable
                Volume molvol = csg.Volume();       // find bounding volume   
                bounds = molvol.BoundsExpand(1.1f); // and get a little leway


                // prepare the mesh in a way I can raycast it and filter it
                // common up the common vertices, and if easily possible display
                if (testop("pdb prep")) {
                    var savemeshx = UnityCSGOutput.MeshesFromCsg(csg, bounds, 999999);
                    savemesh = savemeshx["notexture"].GetBigMesh();

                    float tf1 = Time.realtimeSinceStartup;
                    Log("pdb prep time=" + (tf1 - t0));


                    Log("basic mesh saved, triangles=" + savemesh.triangles.Length);
                    prepRadinf = CSGControl.radInfluence;

                    // precompute the compacted version (common vertices)
                    savemesh = savemesh.RemapMesh();

                    DeleteChildren(goTest);

                    // Render outside and inside separately with different colours
                    // It would be more sensible to have a two-sided shader,
                    // but I haven't managed to make a sensible one yet. .... (Stephen 13 Dec 2015)
                    // and the extra cost doesn't seem to significant.
                    mfMol[Mols.molA].mesh = savemesh.ToMesh();
                    mfMol[Mols.molAback].mesh = savemesh.ToMeshBack();

                    return; // 
                }
            }

            if (testop("filter")) {
                if (mol == null || savemesh == null || prepRadinf != CSGControl.radInfluence)
                    Show("pdb prep");

                filtermesh = Filter.FilterMesh.Filter(savemesh, hitpoint);
                float tf1 = Time.realtimeSinceStartup;

                Log("mesh filter time=" + (tf1 - t0));
                DeleteChildren(goFiltered);

                // double-sided filtered surface (again, silly way to do double-sided)
                mfMol[Mols.molAfilt].mesh = filtermesh.ToMesh();
                mfMol[Mols.molAfiltback].mesh = filtermesh.ToMeshBack();

                return;  // don't go through the standard csg display part appropriate to full display
            }

            if (toshow == "") return;  // toshow = "" used to collect buttons, so nothing to display

            float t1 = Time.realtimeSinceStartup;
            Log("csg stats" + csg.Stats() + ", csg generation time=" + (t1 - t0));

            // This is where the real rendering happens
            var meshes = UnityCSGOutput.MeshesFromCsg(csg, bounds);  // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            float t2 = Time.realtimeSinceStartup;
            Log2("mesh count " + meshes.Count + " maxlev=" + CSGControl.MaxLev + " time=" + (t2 - t1) +
            ", giveUpCount=" + CSGNode.GiveUpCount);
            if (BasicMeshData.CheckWind)
                Log("wrong winding=" + BasicMeshData.WrongWind + " very wrong winding="
                + BasicMeshData.VeryWrongWind + " right winding=" + BasicMeshData.RightWind
                + " model=" + toshow);

            if (toshow != "pdb prep") {  // already done for filter
                GameObject test1 = CSGControl.UseBreak ? goFiltered : goTest;
                DeleteChildren(test1);
                BasicMeshData.ToGame(test1, meshes, "CSGStephen");
            }
        }  // Show()

        protected override void UpdateI() {
            if (!shader) shader = Shader.Find("Standard");    // Set shader

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

            if (Input.GetKey("q")) {
                if (savemesh == null)
                    Show("pdb prep");
                hitpoint = Hitpoint();
                //Log ("hitp" + hitpoint);
                if (!float.IsNaN(hitpoint.x)) {
                    Show("filter");
                    lookat = hitpoint;
                }
                savedTransform = Camera.main.transform.SaveData();
            }

            // perform standard update options
            base.UpdateI();

        }

        // application specific parts of GUI code
        protected override void OnGUII() {
            setobjs();
            //GUIBits.text = "";

            if (MSlider("MaxNeighbourDistance", ref FilterMesh.MaxNeighbourDistance, 0, 50)) 
                Show("filter");
            if (MSlider("MustIncludeDistance", ref FilterMesh.MustIncludeDistance, 0, 50))
                Show("filter");
            
            if (MSlider("Detail Level", ref CSGControl.MinLev, 5, 7))
                Show("pdb prep");
            
            base.OnGUII();
        }

        private void setobjs() {
            if (goMol != null) return;
            int N = 8;
            goMol = new GameObject[N];
            mfMol = new MeshFilter[N];
            mrMol = new MeshRenderer[N];


            for (int i = 0; i < N; i++) {
                goMol[i] = GameObject.Find(Mols.name[i]);
                if (goMol[i] == null) {
                    goMol[i] = new GameObject(Mols.name[i]);
                    goMol[i].layer = i + 8;
                    mfMol[i] = goMol[i].AddComponent<MeshFilter>();
                    mrMol[i] = goMol[i].AddComponent<MeshRenderer>();
                    Material mat = new Material(shader);
                    mat.color = Mols.colors[i];
                    mat.SetFloat("_Glossiness", smooth);
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
            new Color(0.5f, 0.5f, 0.5f), // A
            new Color(0.5f, 1,    0.5f), // Aback
            new Color(0.5f, 0.5f, 0.5f), // B
            new Color(0.5f, 0.5f, 0.5f), // Bback
            new Color(0.8f, 0.5f, 0.5f), // Afilt
            new Color(0.5f, 0.5f, 0.5f), // Afiltback
            new Color(0.5f, 0.5f, 0.5f), // Bfilt
            new Color(0.5f, 0.5f, 0.5f)  // Bfiltback
        };
    }

} // namespace CSG