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
        [Range(1.1f, 5)]         public float radInfluence = 2.5f;  // controls the blobiness of the metaballs,
                                                                    // a sphere of radius r has influence up to distance r*radInfluence

        public static GUIBits maingui;
        public bool keepUpright = false;
        public bool parallel = true;
        public bool showCSGStats = false;

        [Range(0, 12)]
        public int MinLev = 1;

        [Range(0, 12)]
        public int MaxLev = 10;

        [Range(-1, 20)]
        public float CylNum = 6;

        [Range(10, 2000)]
        public int GiveUpNodes = 50;


        [Range(0, 3)]
        public int CullSides = 1;

        public bool CheckWind = true;
        [Range(0, 3)]
        public int WindShow = 3;
        public bool windDebug = true;

        protected BigMesh savemeshA, savemeshB;
        // saved for processing
        //protected Mesh filtermesh;
        // saved molecule
        protected float prepRadinf;
        public Vector3 hitpoint = new Vector3(float.NaN, float.NaN, float.NaN);
        public Vector3 lookat = Vector3.zero;
        // for refreshing filter
        protected Vector3 hitpointA = new Vector3(float.NaN, float.NaN, float.NaN);
        protected Vector3 hitpointB = new Vector3(float.NaN, float.NaN, float.NaN);

        public GameObject goTest, goFiltered;
        public TransformData savedTransform;
        protected CSGNode csg;
        protected Camera[] Cameras;

        protected string toshow;
        static int nexty = 0, yinc = 1;

        Vector3 lastmouse = new Vector3(0, 0, 0);

        public GUIBits() {
            maingui = this;
            //home(Camera.main.transform);
        }

        class ButtonPrepare {
            public string name, tooltip;

            public ButtonPrepare(string name, string tooltip) {
                this.name = name;
                this.tooltip = tooltip;
            }

        }

        public static string text = "";
        public static Dictionary<string, string> ktexts = new Dictionary<string, string>();

        Dictionary<string, ButtonPrepare> ops = new Dictionary<string, ButtonPrepare>();

        protected bool opdone = false;
        protected bool testop(string s, string tip) {
            if (!ops.ContainsKey(s)) {
                ops.Add(s, new ButtonPrepare(s, tip));
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
            BasicMeshData.Sides = 1;                // just show outside surface on main
            BasicMeshData.CheckWind = CheckWind;    // check and correct winding
            BasicMeshData.WindShow = WindShow;             //show all windings, correct = 1 + wrong = 2
            Poly.windDebug = windDebug;                 // do not do further winding debug
            BasicMeshData.RightWind = BasicMeshData.WrongWind = BasicMeshData.VeryWrongWind = 0;

            CSGControl.MinLev = MinLev;
            CSGControl.MaxLev = MaxLev;
            BasicMeshData.Sides = CullSides;
            CSGXPrim.CylNum = CylNum;
            CSGControl.GiveUpNodes = GiveUpNodes;
            // CSGControl.radInfluence = radInfluence; MSPHERE.UpdateRadInfluence();
            CSGControl.stats = showCSGStats;


            // update local control variables (so they show in Unity editor) 
            // to be reflected in the 'real' places
            gameObject.transform.position = new Vector3(0, 0, 0);
            gameObject.transform.rotation = new Quaternion(0, 0, 0, 1);


            if (testop("clear", "Clear out the objects associated with Test and Filtered, but not molecule objects.")) {  // clear button
                clear();
                return true;
            }
            if (testop("progress", "Show progress so far for parallel csg->mesh")) {
                showProgress();
                return true;
            }
            if (testop("interrupt", "Interrupt any outstanding csg->mesh")) {
                interrupt();
            }

            t0 = Time.realtimeSinceStartup;

            return false;
        }  // Show()

        protected float t0;
        private float t1 = 0;

        private List<ParallelCSG> parallels = new List<ParallelCSG>();
        void showProgress() { foreach (var p in parallels) p.showProgress(); }
        void interrupt() { foreach (var p in parallels) p.interrupt(); }

        void finishParallel() { 
            List<ParallelCSG> remove = new List<ParallelCSG>();
            foreach (var p in parallels) {
                if (p.testFinishParallel())
                    remove.Add(p);
            }
            foreach (var p in remove)
                parallels.Remove(p);
            GUIBits.LogK("||", "running in parallel:  n={0}", parallels.Count);
        }

         internal static float progressInterval;



        protected bool showCSGParallel(CSGNode csg, Bounds bounds, GameObject front, GameObject back = null, voidvoid after = null, bool clear = true) {
            Poly.verttype[0] = Poly.verttype[1] = Poly.verttype[2] = 0;
            if (toshow == "")
                return false;

            t1 = Time.realtimeSinceStartup;
            Log("csg stats" + csg.Stats() + ", csg generation time=" + (t1 - t0));

            // if (parallelThread != null) interrupt(); // even if not parallel now, parallel may just have changed
            ParallelCSG pcsg = new ParallelCSG(csg, bounds, front, back, toshow, parallel, CSGControl.MinLev, CSGControl.MaxLev, after, clear);
            if (parallel) {
                parallels.Add(pcsg);
                GUIBits.LogK("||", "running in parallel:  n={0}, {1}", parallels.Count, toshow);
            }
            return true;

        }  // showCSGparallel

        protected void clear() {
            DeleteChildren(goTest);
            DeleteChildren(goFiltered);
            csg = S.NONE;
            Poly.ClearPool();
        }


        public static void Log(string s, params object[] xparms) {
            string ss = String.Format(s, xparms);
            if (text.Length < 5000) text += "\n" + ss;
        }
        public static void LogC(string s, params object[] xparms) {
            string ss = String.Format(s, xparms);
            if (text.Length < 5000) text += " " + ss;
        }

        public static void LogK(string k, string s, params object[] xparms) {
            string ss = String.Format(s, xparms);
            lock (ktexts) ktexts[k] = ss;
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

        protected Camera curcam { get { if (Cameras == null) Cameras = Camera.allCameras; return curcamnum < 0 ? null : Cameras[curcamnum]; } }
        protected int curcamnum = 0;
        protected Camera findcam() {
            if (Cameras == null) Cameras = Camera.allCameras;

            curcamnum = -1;
            for (int cam = 0; cam < Cameras.Length; cam++) {
                if (Cameras[cam].pixelRect.Contains(Input.mousePosition)) {
                    curcamnum = cam;
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

            // keys that can operate regardless of curcam
            if (Input.GetKeyDown("p")) showProgress();
            if (Input.GetKeyDown("escape")) {
                lock (ktexts) ktexts.Clear();
                showing = false;
            }
            showProgress();


            int mousebuts = (Input.GetMouseButton(0) ? 1 : 0) + (Input.GetMouseButton(1) ? 2 : 0) + (Input.GetMouseButton(2) ? 4 : 0);
            // mouse may go down in odd position on new click
            if ((Input.mousePosition.x > currentMenuWidth) && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))) {
                mousebuts = 0;
                findcam();
            }
            if (curcamnum < 0) return;

            var t = curcam.transform;
            var p = t.position;
            var a = Input.GetKey("left ctrl") ? 0.1f : Input.GetKey("left shift") ? 10 : 1;
            // LogK("control", GUI.GetNameOfFocusedControl());  // not helpful for sliders

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


            if (Input.GetKey("left alt")) {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                    t.RotateAround(Vector3.zero, t.up, 90);
                if (Input.GetKeyDown(KeyCode.RightArrow))
                    t.RotateAround(Vector3.zero, t.up, -90);
                if (Input.GetKeyDown(KeyCode.UpArrow))
                    t.RotateAround(Vector3.zero, t.right, -90);
                if (Input.GetKeyDown(KeyCode.DownArrow))
                    t.RotateAround(Vector3.zero, t.right, 90);
            } else {
                if (Input.GetKey(KeyCode.LeftArrow))
                    p -= t.right * mrate;
                if (Input.GetKey(KeyCode.RightArrow))
                    p += t.right * mrate;
                if (Input.GetKey(KeyCode.UpArrow))
                    p += t.up * mrate;
                if (Input.GetKey(KeyCode.DownArrow))
                    p -= t.up * mrate;
            }

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

            if (Input.GetMouseButtonDown(1)) {
                if (Input.mousePosition.x < currentMenuWidth) {
                    boxopacity++;  // right click in menu to darken opacity
                }
            }

            if (Input.GetMouseButtonDown(0)) {
                if (Input.mousePosition.x < currentMenuWidth) {
                    // boxopacity++;  // for some reason this caused us to need two clicks to operate sliders
                } else {
                    Vector3 hit = Hitpoint();
                    if (!float.IsNaN(hit.x))
                        lookat = hit;
                    else
                        lookat = Vector3.zero;
                }
            }

            //lookat = Vector3.zero; // t.position + t.forward * 20;
            if (!showing) {     // only use mouse drag if ! showing
                if (!Input.GetKey("left alt")) {// standard mouse drag
                    switch (mousebuts) {
                        case 0:
                            break;
                        case 1:
                            mousedelta *= mouseRotRate;
                            t.RotateAround(lookat, -mousedelta.y * t.right + mousedelta.x * t.up, mousedelta.magnitude);
                            break;
                        case 2:
                            mousedelta *= mousePanRate;
                            t.position -= (mousedelta.x * t.right + mousedelta.y * t.up);
                            break;
                        case 3:
                            mousedelta *= mouseRotRate;
                            t.RotateAround(lookat, t.forward, mousedelta.x);
                            break;
                    }
                } else {  // end standard mouse drag, start alt mouse drag
                    GameObject so = getSubObject();
                    if (so != null)
                        switch (mousebuts) {
                            case 0:
                                break;
                            case 1:
                                break;
                            case 2:
                                mousedelta *= mousePanRate;
                                so.transform.localPosition += (mousedelta.x * t.right + mousedelta.y * t.up);
                                break;
                            case 3:
                                break;

                        }
                }  // end alt mouse drag
            }     // only use mouse drag if ! showing


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

            if (Input.GetKey("home")) home(t);


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

        protected virtual GameObject getSubObject() { return null; }

        private bool showing = true;

        float boxopacity = 0.5f;
        int currentMenuWidth = 0;
        Dictionary<string, Vector2> tippos;

        // standard Unity OnGUI function, protecting from exceptions
        void OnGUI() {
            int ww = currentMenuWidth = sliderWidth + 2 * slidermargin;
            if (Input.mousePosition.x < 20) showing = true;
            if (Input.mousePosition.x > ww) { showing = false; boxopacity = 0.5f; }

            // main gui, which will draw all the buttons etc within the box
            try {
                Rect rect = new Rect(0, 0, showing ? ww : 0, Screen.height);
                GUI.BeginGroup(rect);

                // draw a background
                GUI.color = new Color(1.0f, 1.0f, 1.0f, boxopacity); //0.5 is half opacity 
                for (int i = 0; i < boxopacity; i++) GUI.Box(rect, "box");

                OnGUII();

                GUI.EndGroup();


                // now, a bit of a faffle, show any tooltip in the correct place
                // keep to left to allow full width, tooltip is also limited to current parent region
                // ?? better to display toolip at higher level after 
                if (GUI.tooltip != "") {
                    Vector2 pos = tippos[GUI.tooltip];
                    GUI.skin.label.wordWrap = true;
                    // sadly the line below just causes problems so we can't get sensible height
                    // Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(GUI.tooltip), GUI.skin.label, GUILayout.MaxWidth(currentMenuWidth));
                    Rect labelRect = new Rect((int)pos.x + 20, (int)pos.y + 20, currentMenuWidth, 80);
                    for (int i = 0; i < NBOX; i++)  // get right opacity, seems silly but ...
                        GUI.Box(labelRect, "");
                    GUI.Label(labelRect, GUI.tooltip);
                    GUI.skin.button.wordWrap = false;
                }

            } catch (System.Exception e) {
                Log(e.ToString() + e.StackTrace);
            }

            showlog();
        }

        int slidery = 0;
        protected int butheight = 18, butwidth = 80;

        protected virtual void OnGUII() {
            GUI.contentColor = Color.white;
            GUI.color = Color.white;
            GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1f);


            if (ops.Count == 0)
                Show("");  // get things initialized

            // display buttons for all the options set up by Show()
            int y = 0, yh = butheight, x = 2;
            int opsc = ops.Count;
            GUI.skin.button.wordWrap = false;
            tippos = new Dictionary<string, Vector2>();
            foreach (var kvp in ops) {    // and show buttons for tests
                string tooltip = kvp.Key + ": " + kvp.Value.tooltip;
                tippos[tooltip] = new Vector2(x, y);
                if (GUI.Button(new Rect(x, y, butwidth, yh), new GUIContent(kvp.Key, tooltip)))
                    Show(kvp.Key);
                if (ops.Count != opsc) break; // happens when items dynamically added
                //y = kvp.Value.y;
                y += yh;
                if (y > Screen.height/2) { x += butwidth;  y = 0; }
            }

            y += 2;
            slidery = Screen.height / 2 + 40;

            MSlider("progress rate", ref progressInterval, 0.0f, 10.0f, 0);

        }     // OnGUII

        void showlog() {
            // int w = Screen.width / 2;  // width of text
            string xtext = "";
            lock (ktexts) foreach (var kvp in ktexts) xtext += kvp.Key + ": " + kvp.Value + "\n";
            xtext += text.Length == 0 ? "" : text.Substring(1);

            try {
                Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(xtext), GUI.skin.label);
                labelRect.x = Screen.width - labelRect.width;
                for (int i = 0; i < NBOX; i++)  // get right opacity, seems silly but ...
                    GUI.Box(labelRect, "");
                GUI.contentColor = Color.white;
                if (GUI.Button(labelRect, xtext, "label"))
                    text = "";
            } catch (Exception e) {
                Log2("exception preparing labelRect {0}\n{1}", e, e.StackTrace);
            }
        }


        int sliderWidth = 160, sliderHeight = 25, slidermargin = 20;
        /** make an int slider, y position w.i.p, return true if value changed */
        protected bool MSlider(string sliderName, ref int v, int low, int high, int def) {
            int o = v;
            float vv = v;
            bool changed = MSlider(sliderName, ref vv, low - 0.45f, high + 0.45f, def);
            v = (int)(vv + 0.5);

            changed = o != v;
            if (changed) lastmouse = Input.mousePosition;
            return changed;
        }

        /** make an int slider, y position w.i.p, return true if value changed */
        protected bool MSlider(string sliderName, ref float v, float low, float high, float def) {
            GUI.contentColor = Color.white;
            float o = v;
            GUI.SetNextControlName(sliderName);
            v = GUI.HorizontalSlider(new Rect(slidermargin, slidery, sliderWidth, sliderHeight / 2), v, low, high);
            if (GUI.Button(new Rect(slidermargin / 4, slidery - 15, sliderWidth*2, 20), String.Format("{0} = {1:0.00}", sliderName, v), "Label"))
            v = def;
            slidery += sliderHeight;
            bool changed = o != v;
            if (changed) lastmouse = Input.mousePosition;
            return changed;
        }

        /** return IF CHANGED */
        protected bool Mcheck(string checkName, ref bool v) {
            GUI.contentColor = Color.white;
//            GUI.color = Color.white;
            bool o = v;
            v = GUI.Toggle(new Rect(20, slidery, sliderWidth, 20), v, checkName);
            slidery += sliderHeight;
            bool changed = o != v;
            if (changed) lastmouse = Input.mousePosition;
            return changed;
        }

        /** return IF CHANGED */
        protected bool Mcheck(string checkName, ref bool v, Rect rect) {
            GUI.contentColor = Color.white;
//            GUI.color = Color.white;
            bool o = v;
            v = GUI.Toggle(rect, v, checkName);
            bool changed = o != v;
            if (changed) lastmouse = Input.mousePosition;
            return changed;
        }

        // Update is called once per frame: call UpdateI with exception protection
        void Update() {
            try {
                UpdateI();
            } catch (System.Exception e) {
                Log("Exception in update: " + e + "\n" + e.Source);
            }
        }

        private static int simpguid = -1;  // use so we can call Simplify with unique guid

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
                    Matrix4x4 tm = mf.transform.worldToLocalMatrix;
                    Ray lray = new Ray(tm.MultiplyPoint3x4(ray.origin), tm.MultiplyVector(ray.direction));
                    Vector3 hp = XRay.intersect(lray, mf.mesh, out t, out r);
                    if (r < bestr) {
                        bestr = r;
                        bestpoint = hp;
                        bestmf = mf;
                        triangleNumber = t;
                    }
                }
            }
            if (bestmf != null) {
                LogK("lasthit", "p={0} mf={1} t#={2}", bestpoint, bestmf.name, triangleNumber);
                Mesh mesh = bestmf.mesh;
                Vector3[] vv = new Vector3[3];
                for (int i=0; i<3; i++) {
                    int v = mesh.triangles[triangleNumber + i];
                    vv[i] = mesh.vertices[v];
                    LogK("v" + i, "v#={3} vert = {0}, norm = {1}, col = {2}", mesh.vertices[v], mesh.normals[v], mesh.colors[v], v);
                }
                LogK("cross", "x={0}", (vv[1] - vv[0]).cross(vv[2] - vv[0]).Normal());
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

        public virtual void home(Transform t) {
            t.rotation = new Quaternion(0, 0, 0, 1);
            t.position = new Vector3(0, 0, -30);
        }


        #endregion


    }     // class GUIBits

    public delegate void voidvoid();

    class ParallelCSG {
        private UnityCSGOutput parallelOutput;
        
        private Thread parallelThread = null;
        private float lastProgressTime = 0;
        private float t1 = Time.realtimeSinceStartup;
        private readonly GameObject goFront, goBack;
        private string toshow;
        private int minLev, maxLev;
        private voidvoid after;
        private bool clear;

        internal ParallelCSG(CSGNode csg, Bounds bounds, GameObject front, GameObject back, string toshow, bool parallel, int minLev, int maxLev, voidvoid after = null, bool clear = true) {
            goFront = front;
            goBack = back;
            this.toshow = toshow;
            this.minLev = minLev;
            this.maxLev = maxLev;
            this.after = after;
            this.clear = clear;

            if (parallel) {
                // GUIBits.Log("running in parallel:  " + toshow);
                parallelThread = new Thread(() => {
                    try {
                        Poly.ClearPool();
                        S.Simpguid = 1001;
                        parallelOutput = UnityCSGOutput.MakeCSGOutput(csg, bounds, this.minLev, this.maxLev);  // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                    } catch (Exception e) {
                        GUIBits.Log2("parallel thread failed: " + e);
                        GUIBits.Log2(e.StackTrace);
                    }
                });
                parallelThread.Name = "CSGWorker_" + toshow;
                parallelThread.Start();
            } else {
                parallelOutput = UnityCSGOutput.MakeCSGOutput(csg, bounds, minLev, maxLev);  // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<
                finish();
            }
        }

        internal void interrupt() {
            if (parallelThread != null) {
                CSGControl.Interrupt = true;
                GUIBits.Log("Interrupt requested by user");
                float t2 = Time.realtimeSinceStartup;
                parallelThread.Abort();
                GUIBits.Log("INTERRUPTED: {0} interrupted at done={1}% time={2} est total time={3}", toshow, Subdivide.done * 100, (t2 - t1), (t2 - t1) / Subdivide.done);
                toshow = null;
                parallelThread = null;
                if (CSGNode.csgmode != CSGMode.collecting) {
                    GUIBits.Log("csgmode forced after interrupt from " + CSGNode.csgmode);
                    CSGNode.csgmode = CSGMode.collecting;
                }
            } else {
                // GUIBits.Log("interrupt: no parallel thread to kill");
            }
        } // interrupt

        internal void showProgress() {

            if (parallelThread == null) return;
            if (!(GUIBits.progressInterval != 0 && Time.realtimeSinceStartup > lastProgressTime + GUIBits.progressInterval)) return;

            try {
                GameObject test1 = goFront;
                parallelOutput.showProgress(test1, toshow);
            } catch (Exception e) {
                GUIBits.Log("Error showing progress:" + e);
            }
            lastProgressTime = Time.realtimeSinceStartup;
        }  // showProgress()

        internal bool testFinishParallel() {
            if (parallelThread == null) {
                GUIBits.Log("Incorrect parallelThread == null");
                return true;
            }
            float t2 = Time.realtimeSinceStartup;
            GUIBits.LogK("done", "{0,3:0.0}%, time={1,5:0.00}, est remaining time={2,5:0.00}, splits={3}", Subdivide.done * 100, (t2 - t1), (t2 - t1) * (1 - Subdivide.done) / Subdivide.done, Poly.splits);


            if (!parallelThread.IsAlive) {
                finish();
                return true;
            }
            // nb, minster model from OpenSCAD 1/2 minster vertices=30,656, triangles=61,891
            return false;
        }  // testFinishParallel

        protected void finish() {
            //showProgress();  // ???? <<< killer ???
            parallelThread = null;
            float t2 = Time.realtimeSinceStartup;
            if (parallelOutput == null) {
                GUIBits.Log("parallel thread finished but no meshes created, time=" + (t2 - t1));
                toshow = null;
                return;
            }
            GUIBits.LogK("done", "parallel complete: {0}: time={1}  {2,3:0.0}% ", toshow, t2 - t1, Subdivide.done * 100);

            if (clear)
                GUIBits.DeleteChildren(goFront);
            var meshes = parallelOutput.MeshesSoFar();
            CSGStats stats = BasicMeshData.ToGame(goFront, meshes, toshow);
            if (goBack != null) {
                GUIBits.DeleteChildren(goBack);
                BasicMeshData.ToGame(goBack, meshes, toshow, back: true);
            }

            GUIBits.Log2("mesh count {0}, lev {1}..{2} time={3} givup#={4} stats={5}", parallelOutput.Count, CSGControl.MinLev, CSGControl.MaxLev, (t2 - t1),
                CSGNode.GiveUpCount, stats);
            if (BasicMeshData.CheckWind)
                GUIBits.Log("wrong winding=" + BasicMeshData.WrongWind + " very wrong winding="
                + BasicMeshData.VeryWrongWind + " right winding=" + BasicMeshData.RightWind
                + " model=" + toshow);
            if (GUIBits.maingui.showCSGStats)
                GUIBits.Log2("verttypes {0:N0} {1:N0} {2:N0}", Poly.verttype[0], Poly.verttype[1], Poly.verttype[2]);
            if (Poly.polymixup != 0)
                GUIBits.Log2("poly mix up {0:N0}", Poly.polymixup);
            Poly.polymixup = 0;
            parallelOutput = null;
            toshow = null;

            if (after != null) after();
        }
    } // class ParallelCSG

}   // namespace CSG
