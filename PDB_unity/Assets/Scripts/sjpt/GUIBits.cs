// various helper GUI bits to fit Stephen's experiments
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using CSG;
using CSGNonPlane;
using CSGFIELD;
using Random = UnityEngine.Random;

namespace CSG {  // [ExecuteInEditMode]
    public abstract class GUIBits : MonoBehaviour {
        Material material;
        GameObject child;

        [Range(0, 4)] public int NBOX = 2;  // number of boxes to draw behind text to get enough opacity
        [Range(0, 5)]
        public float radInfluence = 2.5f;  // controls the blobiness of the metaballs,
                                           // a sphere of radius r has influence up to distance r*radInfluence


        public bool keepUpright = false;
        public bool parallel = true;
        public bool CSGStats = false;

        [Range(1, 12)]
        public int MinLev = 1;

        //[Range(1, 12)]
        //public int MaxLev = 10;

        [Range(-1, 20)]
        public float CylNum = 6;

        [Range(10, 2000)]
        public int GiveUpNodes = 50;


        [Range(0, 3)]
        public int CullSides = 1;

        [Range(0, 3)]
        public int WindShow = 3;
        public bool CheckWind = true;
        public bool windDebug = true;

        protected BigMesh savemeshA, savemeshB;
        // saved for processing
        //protected Mesh filtermesh;
        // saved molecule
        protected float prepRadinf;
        public Vector3 hitpoint = new Vector3(float.NaN, float.NaN, float.NaN);
        // for refreshing filter
        protected Vector3 hitpointA = new Vector3(float.NaN, float.NaN, float.NaN);
        protected Vector3 hitpointB = new Vector3(float.NaN, float.NaN, float.NaN);

        public GameObject goTest, goFiltered;
        public TransformData savedTransform;
        public Vector3 lookat = Vector3.zero;
        protected CSGNode csg;
        protected Camera[] Cameras;

        protected string toshow;
        static int nexty = 0, yinc = 1;

        Vector3 lastmouse = new Vector3(0, 0, 0);

        class ButtonPrepare {
            public int y;
            public string name;

            public ButtonPrepare(string name, int y) {
                this.name = name;
                this.y = y;
            }

        }

        public static string text = "";
        public static Dictionary<string, string> ktexts = new Dictionary<string, string>();

        Dictionary<string, ButtonPrepare> ops = new Dictionary<string, ButtonPrepare>();
        protected Bounds bounds;

        protected bool opdone = false;
        protected bool testop(string s) {
            if (!ops.ContainsKey(s)) {
                nexty += yinc;
                ops.Add(s, new ButtonPrepare(s, nexty));
            }

            bool yes = (s == toshow);
            opdone |= yes;
            return yes;
        }

        /// <summary>
        /// find and perform operation, return true if operation complete
        /// </summary>
        /// <param name="ptoshow"></param>
        /// <returns></returns>
        public virtual bool Show(string ptoshow) {
            opdone = false;
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
            gameObject.transform.position = new Vector3(0, 0, 0);
            gameObject.transform.rotation = new Quaternion(0, 0, 0, 1);


            if (testop("clear")) {  // clear button
                DeleteChildren(goTest);
                DeleteChildren(goFiltered);
                csg = S.NONE;
                return true;
            }
            if (testop("progress")) {
                showProgress();
                return true;
            }
            if (testop("interrupt")) {
                interrupt();
            }

            bounds = new Bounds();
            t0 = Time.realtimeSinceStartup;

            return false;
        }  // Show()

        protected float t0;
        private float t1 = 0;

        private IDictionary<string, BasicMeshData> parallelMeshes;
        private string parallelPending = null;
        private Thread parallelThread = null;
        private float progressInterval;
        private float lastProgressTime = 0;

        private void interrupt() {
            if (parallelThread != null) {
                CSGControl.Interrupt = true;
                Log("Interrupt requested by user");
                float t2 = Time.realtimeSinceStartup;
                parallelThread.Abort();
                Log("INTERRUPTED: {0} interrupted at done={1}% time={2} est total time={3}", parallelPending, Subdivide.done * 100, (t2 - t1), (t2 - t1) / Subdivide.done);
                parallelPending = null;
                parallelThread = null;
                if (CSGNode.csgmode != CSGMode.collecting) {
                    Log("csgmode forced after interrupt from " + CSGNode.csgmode);
                    CSGNode.csgmode = CSGMode.collecting;
                }
            } else {
                // Log("interrupt: no parallel thread to kill");
            }
        } // interrupt

        protected void showProgress() {
            if (parallelThread == null) return;
            try {
                GameObject test1 = CSGControl.UseBreak ? goFiltered : goTest;
                BasicMeshData.showProgress(test1);
            } catch (Exception e) {
                Log("Error showing progress:" + e);
            }
            lastProgressTime = Time.realtimeSinceStartup;
        }  // showProgress()

        protected bool showCSGParallel(Bounds bounds) {
            if (toshow == "")
                return false;

            t1 = Time.realtimeSinceStartup;
            Log("csg stats" + csg.Stats() + ", csg generation time=" + (t1 - t0));

            if (parallelThread != null) interrupt(); // even if not parallel now, parallel may just have changed
            if (parallel) {
                parallelPending = toshow;
                Log("running in parallel:  " + parallelPending);
                parallelThread = new Thread(() => {
                    try {
                        parallelMeshes = UnityCSGOutput.MeshesFromCsg(csg, bounds);  // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    } catch (Exception e) {
                        Log("parallel thread failed: " + e);
                        Log(e.StackTrace);
}
                });
                parallelThread.Name = "CSGWorker";
                parallelThread.Start();
            } else {
                parallelMeshes = UnityCSGOutput.MeshesFromCsg(csg, bounds);  // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
            }
            return true;

        }  // showCSGparallel


        private void finishParallel() {
            if (parallelThread != null) {
                float t2 = Time.realtimeSinceStartup;
                LogK("done", "{0,3:0.0}%, time={1,5:0.00}, est remaining time={2,5:0.00}, splits={3}", Subdivide.done * 100, (t2 - t1), (t2 - t1) * (1 - Subdivide.done) / Subdivide.done, Poly.splits);


                if (!parallelThread.IsAlive) {
                    //showProgress();  // ???? <<< killer ???
                    if (parallelMeshes == null) {
                        Log("parallel thread finished but no meshes created, time=" + (t2 - t1));
                        parallelPending = null;
                        parallelThread = null;
                        return;
                    }
                    LogK("done", "parallel complete: " + parallelPending + " time=" + (t2 - t1));
                    parallelPending = null;
                    parallelThread = null;

                    var meshes = parallelMeshes;
                    parallelMeshes = null;
                    GameObject test1 = CSGControl.UseBreak ? goFiltered : goTest;
                    DeleteChildren(test1);
                    CSGStats stats = BasicMeshData.ToGame(test1, meshes, "CSGStephen");

                    Log2("mesh count {0}, lev {1}..{2} time={3} givup#={4} stats={5}", meshes.Count, CSGControl.MinLev, CSGControl.MaxLev, (t2 - t1),
                        CSGNode.GiveUpCount, stats);
                    if (BasicMeshData.CheckWind)
                        Log("wrong winding=" + BasicMeshData.WrongWind + " very wrong winding="
                        + BasicMeshData.VeryWrongWind + " right winding=" + BasicMeshData.RightWind
                        + " model=" + toshow);
                    if (CSGStats)
                        Log2("verttypes {0:N0} {1:N0} {2:N0}", Poly.verttype[0], Poly.verttype[1], Poly.verttype[2]);
                }

                // nb, minster model from OpenSCAD 1/2 minster vertices=30,656, triangles=61,891
            }
        }  // finishParallel

        public static void Log(string s, params object[] xparms) {
            string ss = String.Format(s, xparms);
            if (text.Length < 5000) text += ss + "\n";
        }

        public static void LogK(string k, string s, params object[] xparms) {
            string ss = String.Format(s, xparms);
            lock(ktexts) ktexts[k] = ss;
        }

        public static void Log2(string s, params object[] xparms) {
            string ss = String.Format(s, xparms);
            Debug.Log(ss);
            Log(ss);
        }

        public static void Log(Exception e) {
            Debug.Log(e);
            Log("!!!!!!!!!!!!!!!!!!" + e.ToString());
        }

        protected void Logcsg() {
            string sss = csg.ToStringF("\n");
            Log(sss);
            CSGNode cc = csg.Bake();
            sss = cc.ToStringF("\n");
            Log2("baked=" + sss);
        }

        public float keyPanRate = 0.05f;
        public float keyRotRate = 0.5f;
        public float wheelScrollRate = 0.3f;

        public float mousePanRate = 0.05f;
        public float mouseRotRate = 0.1f;

        protected Camera curcam;
        protected Camera findcam() {
            if (Cameras == null) Cameras = Camera.allCameras;

            curcam = null;
            foreach (Camera cam in Cameras) {
                if (cam.pixelRect.Contains(Input.mousePosition)) {
                    curcam = cam;
                    break;
                }
            }
            return curcam;
        }

        protected virtual void UpdateI() {

            // save these early, maybe more efficient, and difficult to find again after hiding otherwise
            if (!goTest) goTest = GameObject.Find("Test");
            if (!goFiltered) goFiltered = GameObject.Find("Filtered");

            finishParallel();
            if (Input.GetKeyDown("p")) showProgress();
            if (progressInterval != 0 && Time.realtimeSinceStartup > lastProgressTime + progressInterval)
                showProgress();

            if (!findcam()) return;


            var t = curcam.transform;
            var p = t.position;
            var a = Input.GetKey("left ctrl") ? 0.1f : Input.GetKey("left shift") ? 10 : 1;

            float mrate = a * keyPanRate;

            if (Input.GetKey("w"))
                p += t.forward * mrate;
            if (Input.GetKey("s"))
                p -= t.forward * mrate;
            if (Input.GetKey("a"))
                p -= t.right * mrate;
            if (Input.GetKey("d"))
                p += t.right * mrate;

            if (Input.GetKey("r"))
                p = p * (1 - mrate) + lookat * mrate;
            if (Input.GetKey("f"))
                p = p * (1 + mrate) - lookat * mrate;


            if (Input.GetKey(KeyCode.LeftArrow))
                p -= t.right * mrate;
            if (Input.GetKey(KeyCode.RightArrow))
                p += t.right * mrate;
            if (Input.GetKey(KeyCode.UpArrow))
                p += t.up * mrate;
            if (Input.GetKey(KeyCode.DownArrow))
                p -= t.up * mrate;

            if (Input.inputString.Contains("!"))
                p = (p + lookat) / 2;

            float srate = a * wheelScrollRate;
            p += t.forward * Input.mouseScrollDelta.x * srate;
            p += t.forward * Input.mouseScrollDelta.y * srate;

            t.position = p;

            float rrate = a * keyRotRate;
            if (Input.GetKey("i"))
                t.Rotate(-rrate, 0, 0);
            if (Input.GetKey("k"))
                t.Rotate(rrate, 0, 0);
            if (Input.GetKey("j"))
                t.Rotate(0, -rrate, 0);
            if (Input.GetKey("l"))
                t.Rotate(0, rrate, 0);

            Vector3 mousedelta = Input.mousePosition - lastmouse;
            lastmouse = Input.mousePosition;
            mousedelta *= a;
            // int triangleNumber = -1;

            if (Input.GetMouseButtonDown(0)) {
                Vector3 hit = Hitpoint();
                if (!float.IsNaN(hit.x))
                    lookat = hit;
            }

            int k = (Input.GetMouseButton(0) ? 1 : 0) + (Input.GetMouseButton(1) ? 2 : 0) + (Input.GetMouseButton(2) ? 4 : 0);
            // mouse may go down in odd position on new click
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
                k = 0;
            //lookat = Vector3.zero; // t.position + t.forward * 20;
            switch (k) {
                case 0:
                    break;
                case 1:
                    mousedelta *= mouseRotRate;
                    t.RotateAround(lookat, -mousedelta.y * t.right + mousedelta.x * t.up, mousedelta.magnitude);
                    break;
                case 2:
                    mousedelta *= mousePanRate;
                    t.position -= 0.3f * (mousedelta.x * t.right + mousedelta.y * t.up);
                    break;
                case 3:
                    mousedelta *= mouseRotRate;
                    t.RotateAround(lookat, t.forward, mousedelta.x);
                    break;

            }

            if (Input.GetKeyDown("u") && (Input.GetKey("left shift") || Input.GetKey("right shift")))
                keepUpright = !keepUpright;

            if (Input.GetKey("u") || keepUpright) {
                // seems a little convoluted but gets desired effect
                Vector3 f = t.forward;
                Vector3 r = t.right;
                r.y = 0;
                t.up = -r.cross(t.forward);
                t.forward = f;
            }

            if (Input.GetKey("home")) {
                t.rotation = new Quaternion(0, 0, 0, 1);
                t.position = new Vector3(0, 0, -30);
            }


            if (Input.GetKeyDown("m") || Input.GetKey("n")) {  // use hitpoint for CSG feedback or simple feedback
                Vector3 hit = Hitpoint();
                if (float.IsNaN(hit.x)) {
                    //Log ("no hit");
                    text = ("no hit");
                    CSGControl.BreakOff();
                } else {
                    text = "hit at " + hit;
                    lookat = hit;
                    if (Input.GetKeyDown("m")) {
                        CSGControl.Break(hit);
                        Show(toshow);
                        CSGControl.BreakOff();
                    } else {  // "n"
                        float d = 0.0001f;
                        Volume v = new Volume(lookat.x - d, lookat.x + d, lookat.y - d, lookat.y + d, lookat.z - d, lookat.z + d);
                        int unodes = 0;
                        CSGNode csgs = csg.Bake().Simplify(v, simpguid--, ref unodes);
                        Log(csgs.ToStringF("\n")); 
                        if (csgs is CSGPrim && ((CSGPrim)csgs).realCSG != null)
                            Log("from " + ((CSGPrim)csgs).realCSG.ToStringF("\n"));
                    }
                }
            }

            if (Input.GetKeyDown("z")) {
                curcam.transform.RestoreData(savedTransform);
            }

            if (Input.GetKeyDown("x")) {
                if (goTest != null)
                    goTest.SetActive(!goTest.activeSelf);
            }

            if (Input.GetKeyDown("o")) {
                CamScript.useWireframe = !CamScript.useWireframe;
            }

            goFiltered.transform.position = -curcam.transform.forward * 0.1f;

            //RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            //RenderSettings.ambientLight = Color.white;
            //RenderSettings.ambientIntensity = 0;

        }

        // standard Unity OnGUI function, protecting from exceptions
        void OnGUI() {
            try {
                OnGUII();
            } catch (System.Exception e) {
                Log(e.ToString() + e.StackTrace);
            }
        }
            
        int slidery = 0;
        protected virtual void OnGUII() {
            GUI.contentColor = Color.white;
            GUI.color = Color.white;
            GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1f);


            if (ops.Count == 0)
                Show("");  // get things initialized

            // display buttons for all the options set up by Show()
            int y = 0, yh = 18;
            foreach (var kvp in ops) {    // and show buttons for tests
                if (GUI.Button(new Rect(20, kvp.Value.y * yh, 80, yh), kvp.Key))
                    Show(kvp.Key);
                y = kvp.Value.y;
            }
            y += 2;
            slidery = yh*y;

            MSlider("progress rate", ref progressInterval, 0.0f, 10.0f);

            // int w = Screen.width / 2;  // width of text
            string xtext = text;
            lock(ktexts) foreach (var kvp in ktexts) xtext += "\n" + kvp.Key + ": " + kvp.Value;

            try {
                Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(xtext), "label");
                labelRect.x = Screen.width - labelRect.width;
                for (int i = 0; i < NBOX; i++)  // get right opacity, seems silly but ...
                    GUI.Box(labelRect, ""); 
                GUI.contentColor = Color.white;
                if (GUI.Button(labelRect, xtext, "label"))
                    text = "";
            } catch (Exception e) {
                Log2("exception preparing labelRect {0}\n{1}", e, e.StackTrace);
            }
        }     // OnGUII

        int sliderWidth = 160, sliderHeight = 25;
        /** make an int slider, y position w.i.p, return true if value changed */
        protected bool MSlider(string sliderName, ref int v, int low, int high) {
            int o = v;
            v = (int)GUI.HorizontalSlider(new Rect(20, slidery, sliderWidth, 20), v, low, high);
            //if (o != v) Log (name + v);
            GUI.contentColor = Color.black;
            GUI.Label(new Rect(10, slidery - 15, sliderWidth, 20), sliderName + " = " + v);
            slidery += sliderHeight;
            return o != v;
        }

        /** make an int slider, y position w.i.p, return true if value changed */
        protected bool MSlider(string sliderName, ref float v, float low, float high) {
            float o = v;
            v = GUI.HorizontalSlider(new Rect(20, slidery, sliderWidth, 20), v, low, high);
            //if (o != v) Log (name + v);
            GUI.contentColor = Color.black;
            GUI.Label(new Rect(10, slidery - 15, sliderWidth*2, 20), String.Format("{0} = {1:0.00}", sliderName, v));
            slidery += sliderHeight;
            return o != v;
        }

        // Update is called once per frame: call UpdateI with exception protection
        void Update() {
            try {
                UpdateI();
            } catch (System.Exception e) {
                Log("Exception in update: " + e + "\n" + e.Source);
            }
        }

        private static int simpguid = -1;

#region Utility functions
        // find the hitpoint on the screen, using savemesh/filtermesh if available, otherwise using meshes in Test game object
        protected Vector3 Hitpoint(out int triangleNumber, out MeshFilter bestmf) {
            triangleNumber = -1;
            float bestr = float.MaxValue;
            Vector3 bestpoint = new Vector3(float.NaN, float.NaN, float.NaN);
            bestmf = null;

            Ray ray = curcam.ScreenPointToRay(Input.mousePosition);
            var mfs = GameObject.FindObjectsOfType<MeshFilter>();
            foreach (MeshFilter mf in mfs) {
                // LogK("mfs" + mf.name, "layer {0} {1} {2}", mf.gameObject.layer, curcam.cullingMask, (curcam.cullingMask & (1 << mf.gameObject.layer)) != 0);
                if ((curcam.cullingMask & (1 << mf.gameObject.layer)) != 0 && ! mf.name.Contains("back")) {
                    float r; int t;
                    Vector3 hp = XRay.intersect(ray, mf.mesh, out t, out r);
                    if (r < bestr) {
                        bestr = r;
                        bestpoint = hp;
                        bestmf = mf;
                        triangleNumber = t;
                    }
                }
            }
            if (bestmf != null) {
                LogK("lasthit", "p={0} mf={1}", bestpoint, bestmf.name);
                if (bestmf.name.StartsWith("molA")) hitpointA = bestpoint;
                else if (bestmf.name.StartsWith("molB")) hitpointB = bestpoint;
            }
            hitpoint = bestpoint;

            return bestpoint;

            //triangleNumber = -1;
            //mesh = savemesh;
            //return (savemesh == null) ? HitpointGO(out triangleNumber, out mesh) : HitpointBM(out triangleNumber);
        }
        protected Vector3 Hitpoint() {
            int triangleNumber; MeshFilter mf;
            return Hitpoint(out triangleNumber, out mf);
        }

        /** / find the hitpoint on the screen, using savemesh (if dispayed), or filtermesh
        Vector3 HitpointBM(out int triangleNumber) {

            Ray ray = curcam.ScreenPointToRay(Input.mousePosition);
            float r;
            return XRay.intersect(ray, goTest.activeSelf ? savemesh : filtermesh, out triangleNumber, out r);
        }
        /**/

        // find the hitpoint on the screen, using meshes in Test game object
        Vector3 HitpointGO(out int triangleNumber, out Mesh mesh) {
            Ray ray = curcam.ScreenPointToRay(Input.mousePosition);
            float r = ray.intersectR(gameObject, out triangleNumber, out mesh);
            if (r == float.MaxValue)
                return new Vector3(float.NaN, float.NaN, float.NaN);
            else
                return ray.origin + r * ray.direction;
        }

        // clear the CSGStephen game objects we have generated
        protected void Clear() {
            var objects = FindObjectsOfType(typeof(GameObject)); 
            foreach (GameObject o in objects) { 
                if (o.name.StartsWith("CSGStephen"))
                    Destroy(o.gameObject); 
            }
            savemeshA = savemeshB = null;
            lock (ktexts) ktexts.Clear();
        }

        // delete all children of some game object
        public static void DeleteChildren(GameObject test) {
            try {
                Transform top = test.transform;
                for (int i = top.childCount - 1; i >= 0; i--)
                    Destroy(top.GetChild(i).gameObject);

                top.DetachChildren();
            } catch (System.Exception e) {
                Log("DeleteChildren failed: " + e);
            }
        }
#endregion


    }     // class GUIBits
}   // namespace CSG
