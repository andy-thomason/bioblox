// various helper GUI bits to fit Stephen's experiments
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;

using CSG;
using CSGNonPlane;
using CSGFIELD;
using Random = UnityEngine.Random;

namespace CSG {  // [ExecuteInEditMode]
    public abstract class GUIBits : MonoBehaviour {
        Material material;
        GameObject child;

        [Range(0, 4)] public int NBOX = 2;  // number of boxes to draw behind text to get enough opacity

        public bool keepUpright = false;

        //public Material[] materials = new Material[8];

        // for reuse with different influence
        protected BigMesh savemesh;
        // saved for processing
        protected BigMesh filtermesh;
        // saved molecule
        protected float prepRadinf;
        public Vector3 hitpoint = new Vector3(float.NaN, float.NaN, float.NaN);
        protected GameObject goTest, goLight0, goLight1, goFiltered;
        protected TransformData savedTransform;
        public Vector3 lookat = Vector3.zero;
        public Quaternion lightquat0 = new Quaternion(0.3f, 0.3f, 0, 1);
        public Quaternion lightquat1 = new Quaternion(-0.3f, -0.3f, 0, 1);
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

        protected bool testop(string s) {
            if (!ops.ContainsKey(s)) {
                nexty += yinc;
                ops.Add(s, new ButtonPrepare(s, nexty));
            }

            return (s == toshow);
        }

        public abstract void Show(string ptoshow);

        public static void Log(string s, params object[] xparms) {
            string ss = String.Format(s, xparms);
            if (text.Length < 5000) text += ss + "\n";
        }

        public static void LogK(string k, string s, params object[] xparms) {
            string ss = String.Format(s, xparms);
            ktexts[k] = ss;
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

        protected void showcsg() {
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
            if (!goLight0) goLight0 = GameObject.Find("Light0");
            if (!goLight1) goLight1 = GameObject.Find("Light1");
            if (!goFiltered) goFiltered = GameObject.Find("Filtered");

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

            if (Input.GetKeyDown("o")) {
                CamScript.useWireframe = !CamScript.useWireframe;
            }

            goFiltered.transform.position = -curcam.transform.forward * 0.1f;

            // ? no effect RenderSettings.ambientLight = Color.red;
            Light l = (Light)(goLight0.GetComponent(typeof(Light)));
            l.transform.localRotation = lightquat0;
            l = (Light)(goLight1.GetComponent(typeof(Light)));
            l.transform.localRotation = lightquat1;

        }

        // standard Unity OnGUI function, protecting from exceptions
        void OnGUI() {
            try {
                OnGUII();
            } catch (System.Exception e) {
                Log(e.ToString());
            }
        }
            
        int slidery = 0;
        protected virtual void OnGUII() {
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
        
/***
            MSlider("MaxLev", ref CSGControl.MaxLev, 0, 12 );
            MSlider("MinLev", ref CSGControl.MinLev, 0, 12 );
            MSlider("CylNum", ref CSGXprim.CylNum, -1, 20 );
            MSlider("GiveUpNodes", ref CSGControl.GiveUpNodes, 10, 2000 );
            MSlider("Cull Sides", ref BasicMeshData.Sides, 0, 3 );

            MSlider("NumSpheres", ref NumSpheres, 0, 2500 );
            MSlider("radInfluence", ref CSGControl.radInfluence, 0f, 5f );
            MSlider("MaxNeighbourDistance", ref FilterMesh.MaxNeighbourDistance, 0, 50 );
            MSlider("MustIncludeDistance", ref FilterMesh.MustIncludeDistance, 0, 50 );
            MSlider("WindShow", ref BasicMeshData.WindShow, 0, 3 );
***/


            //float xx = GUI.HorizontalSlider(new Rect (20  * 30, 80, 20), lastxx, 0, 10);
            //if (lastxx != xx) Log("xx=" + xx);
            //lastxx = xx;
            int w = Screen.width / 2;  // width of text
            GUI.contentColor = Color.white;
            GUI.color = Color.white;
            GUI.backgroundColor = new Color(1.0f, 1.0f, 1.0f, 1f);
            string xtext = text;
            foreach (var kvp in ktexts) xtext += kvp.Key + ": " + kvp.Value;

            Rect labelRect = GUILayoutUtility.GetRect(new GUIContent(xtext), "label");
            labelRect.x = Screen.width - labelRect.width;
            for (int i = 0; i < NBOX; i++)
                GUI.Box(labelRect, ""); 
            GUI.contentColor = Color.white;
            if (GUI.Button(labelRect, xtext, "label"))
                text = ""; 
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
            GUI.Label(new Rect(10, slidery - 15, sliderWidth, 20), sliderName + " = " + v);
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
        protected Vector3 Hitpoint() {
            return (savemesh == null) ? HitpointGO() : HitpointBM();
        }

        // find the hitpoint on the screen, using savemesh (if dispayed), or filtermesh
        Vector3 HitpointBM() {
            Ray ray = curcam.ScreenPointToRay(Input.mousePosition);
            return XRay.intersect(ray, goTest.activeSelf ? savemesh : filtermesh);
        }

        // find the hitpoint on the screen, using meshes in Test game object
        Vector3 HitpointGO() {
            Ray ray = curcam.ScreenPointToRay(Input.mousePosition);
            float r = ray.intersectR(gameObject);
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
            savemesh = null;
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
