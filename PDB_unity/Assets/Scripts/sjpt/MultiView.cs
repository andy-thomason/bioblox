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

            if (Input.GetKey("q") && curcam == Cameras[0]) {

                Vector3 h = Hitpoint();
                if (!float.IsNaN(h.x)) {
                    Camera cam = Cameras[1];
                    cam.transform.position = h + Camera.main.transform.forward * insideDist;
                    cam.transform.rotation = Camera.main.transform.rotation;
                    cam.transform.Rotate(new Vector3(0, 180, 0));
                }
            }
        }


        void setmwin() {
            Camera cam0 = Camera.main;

            if (Cameras.Length == 1) {
                Cameras = new Camera[4];
                Cameras[0] = cam0;
                for (int i = 0; i < 4; i++) {
                    GameObject goCam = new GameObject("cam" + i);
                    Camera cam = Cameras[i] = goCam.AddComponent<Camera>();
                    cam.transform.position = cam0.transform.position;
                    cam.transform.rotation = cam0.transform.rotation;
                    cam.transform.localScale = cam0.transform.localScale;
                    cam.clearFlags = CameraClearFlags.Color;
                    cam.backgroundColor = Color.green;
                    cam.enabled = true;
                }
                cam0.rect =       new Rect(0,    0,     0.5f, 0.5f);
                Cameras[1].rect = new Rect(0,    0.5f,  0.5f, 0.5f);
                Cameras[2].rect = new Rect(0.5f, 0,     0.5f, 0.5f);
                Cameras[3].rect = new Rect(0.5f, 0.5f,  0.5f, 0.5f);

                Cameras[1].cullingMask = (1 << (Mols.molAfilt + 8)) | (1 << (Mols.molAfiltback + 8));
            }
        }

        void setonewin() {
            Camera.main.rect = new Rect(0, 0, 1,1);
            for(int i = 1; i < Cameras.Length; i++) Cameras[i].enabled = false;
        }

    }

}
