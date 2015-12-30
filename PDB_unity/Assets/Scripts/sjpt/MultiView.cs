using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;


namespace CSG {
    /// <summary>
    /// class to allow setup of multiple viewports/cameras
    /// </summary>
    class MultiView : GUIInsideSurface {

        Camera cam1, cam2;
        GameObject goCam2;

        // main function that will be called for display when something interesting has happened
        public override void Show(string ptoshow) {
            text = "";
            toshow = ptoshow;
            if (testop("mwin")) {
                setmwin();
                return;
            }
            if (testop("onewin")) {
                setonewin();
                return;
            }
            base.Show(ptoshow);
        }

        protected override void UpdateI() {
            base.UpdateI();

            if (Input.GetKey("q")) {

                Vector3 h = Hitpoint();
                if (!float.IsNaN(h.x)) {
                    cam2.transform.position = h + Camera.main.transform.forward * insideDist;
                    cam2.transform.rotation = cam1.transform.rotation;
                    cam2.transform.Rotate(new Vector3(0, 180, 0));
                }
            }

        }


        void setmwin() {
            if (cam1 == null) cam1 = Camera.main;
            if (goCam2 == null) {
                goCam2 = new GameObject("cam2");
                cam2 = goCam2.AddComponent<Camera>();
            }
            cam1.rect = new Rect(0, 0, 0.5f, 0.5f);
            cam2.rect = new Rect(0, 0.5f, 0.5f, 0.5f);
            cam2.clearFlags = CameraClearFlags.Color;
            cam2.backgroundColor = Color.green;
            cam2.transform.position = cam1.transform.position;
            cam2.transform.rotation = cam1.transform.rotation;
            cam2.transform.localScale = cam1.transform.localScale;
            cam2.enabled = true;
            cam2.cullingMask = (1 << (Mols.molAfilt + 8)) | (1 << (Mols.molAfiltback + 8));
        }

        void setonewin() {
            if (cam1 == null) cam1 = Camera.main;
            if (goCam2 == null) {
                goCam2 = new GameObject("cam2");
                cam2 = goCam2.AddComponent<Camera>();
            }
            cam1.rect = new Rect(0, 0, 1,1);
            cam2.enabled = false;
        }

    }

}
