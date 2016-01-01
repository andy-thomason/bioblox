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


        public float outsideDist = 20;  // distance to jump beyond surface when using ';' or '/' to go inside object 
        public float insideDist = 20;  // distance to jump beyond surface when using ';' or '/' to go outside object 
        public float smooth = 0.66f;        

        // following are class so they can get reused between frames
        CSGFMETA csgm;
        Shader shader;

        GameObject[] goMol;
        MeshFilter[] mfMol;
        MeshRenderer[] mrMol;
        protected PDB_molecule molA, molB;


        // main function that will be called for display when something interesting has happened
        public override bool Show(string ptoshow) {
            if (base.Show(ptoshow)) return true;


            if (testop("clearMol")) {
                foreach (var mf in mfMol) mf.mesh = null;
                return true;
            }

            if (testop("pdb") || testop("pdb prep") || testop("pdb prepB")) {
                if (CSGControl.MinLev < 5) 
                    CSGControl.MinLev = CSGControl.MinLev = 7;
                CSGControl.MaxLev = CSGControl.MinLev;
                bool useb = ptoshow == "pdb prepB";
                PDB_molecule mol;

                if (useb)
                    mol = molB = PDB_parser.get_molecule("pdb2ptcWithTags.2");  // read the (cached) molecule data
                else
                    mol = molA = PDB_parser.get_molecule("pdb2ptcWithTags.1");  // read the (cached) molecule data

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
                if (ptoshow.StartsWith("pdb prep")) {
                    var savemeshx = UnityCSGOutput.MeshesFromCsg(csg, bounds, 999999);
                    BigMesh tsavemesh = null;
                    foreach (var s in savemeshx) tsavemesh = s.Value.GetBigMesh();  // should only be one

                    float tf1 = Time.realtimeSinceStartup;
                    Log("pdb prep time=" + (tf1 - t0));


                    Log("basic mesh saved, triangles=" + tsavemesh.triangles.Length);
                    prepRadinf = CSGControl.radInfluence;

                    // precompute the compacted version (common vertices)
                    tsavemesh = tsavemesh.RemapMesh();

                    // Render outside and inside separately with different colours
                    // It would be more sensible to have a two-sided shader,
                    // but I haven't managed to make a sensible one yet. .... (Stephen 13 Dec 2015)
                    // and the extra cost doesn't seem to significant.
                    if (!useb) {
                        mfMol[Mols.molA].mesh = tsavemesh.ToMesh();
                        mfMol[Mols.molAback].mesh = tsavemesh.ToMeshBack();
                        savemesh = tsavemesh;
                    } else {
                        mfMol[Mols.molB].mesh = tsavemesh.ToMesh();
                        mfMol[Mols.molBback].mesh = tsavemesh.ToMeshBack();
                    }

                    return true; // 
                }
            }

            if (testop("filter")) {
                if (molA == null || savemesh == null || prepRadinf != CSGControl.radInfluence)
                    Show("pdb prep");

                filtermesh = Filter.FilterMesh.Filter(savemesh, hitpoint);
                float tf1 = Time.realtimeSinceStartup;

                Log("mesh filter time=" + (tf1 - t0));
                DeleteChildren(goFiltered);

                // double-sided filtered surface (again, silly way to do double-sided)
                mfMol[Mols.molAfilt].mesh = filtermesh.ToMesh();
                mfMol[Mols.molAfiltback].mesh = filtermesh.ToMeshBack();

                return true;  // don't go through the standard csg display part appropriate to full display
            }

            if (opdone) showCSGParallel(bounds);
            return opdone;

        }  // Show()

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

            if (Input.GetKeyDown("x")) {
                bool act = ! isShown(curcam, Mols.molA);
                show(curcam, Mols.molA, act);
                show(curcam, Mols.molAback, act);
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