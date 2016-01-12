using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MatrixLibrary;

using UnityEngine;
using CSGFIELD;


namespace CSG {
    class Curvature {

        static int kgnegs = 0;
        /// <summary>
        /// compute a colour from curvature using input dxx etc partial derivatives,
        /// </summary>
        /// <param name="dxx"></param>
        /// <param name="dxy"></param>
        /// <param name="dxz"></param>
        /// <param name="dyy"></param>
        /// <param name="dyz"></param>
        /// <param name="dzz"></param>
        /// <returns></returns>
        public static Color ctest(float dxx, float dxy, float dxz, float dyy, float dyz, float dzz, Vector3 grad) {
            // from http://www.cgeo.ulg.ac.be/CAO/Goldman_Curvature_formulas_implicit.pdf
            // page 642 (4.1, 4.2)
            float dyx = dxy, dzx = dxz, dzy = dyz;
            double[,] H = new double[3, 3];              // H(F)
            H[0, 0] = dxx;
            H[0, 1] = H[1, 0] = dxy;
            H[0, 2] = H[2, 0] = dxz;
            H[1, 1] = dyy;
            H[1, 2] = H[2, 1] = dyz;
            H[2, 2] = dzz;

            double[,] HX = new double[,] {              // H*(F)
                { dyy * dzz - dyz * dzy, dyz * dzx - dyx* dzz, dyx*dzy - dyy* dzx },
                { dxz * dzy - dxy * dzz, dxx * dzz - dxz* dzx, dxy*dzx - dxx* dzy },
                { dxy * dyz - dxz * dyy, dyx * dxz - dxx* dyz, dxx*dyy - dxy* dyx }
            };

            double[,] G = new double[,] { { grad.x, grad.y, grad.z } };   // delta F
            double[,] GT = MatrixX.Transpose(G);
            double[,] KGA = MatrixX.Multiply(G, MatrixX.Multiply(HX, GT));
            double KGX = KGA[0, 0];

            double gradl = grad.magnitude;                  // | delta F |
            double KG = KGX / (gradl.sq().sq());            // (4.1)
            double KM = (MatrixX.Multiply(G, MatrixX.Multiply(H, GT))[0,0] - gradl.sq() * (dxx + dyy + dzz)) / (2 * gradl.sq() * gradl);      // (4.2)
            double ss = KM * KM - KG;
            if (ss < -0.001) GUIBits.Log("odd ss in curvature {0}", ss);
            if (ss < 0) ss = 0;
            double sss = Math.Sqrt(ss);
            double K1 = KM + sss;       // (4.3)
            double K2 = KM - sss;
            if (KG < 0)
                kgnegs++; //  GUIBits.LogK("kgnegs", kgnegs++ + "");

            //curv range min RGBA(-3.402, -10.734, -10.353, -4.928) max RGBA(2.687, 0.481, 18.382, 0.527)
            // 0.4 typical for purish sphere, 0 for flat
            return new Color((float) K1, (float)K2, (float)KG, (float)KM);

            /** /
                        double[,] om1, om2;
                        Matrix.Eigen(H, out om1, out om2);
                        Vector3 nx1 = new Vector3((float)om2[2, 0], (float)om2[2, 1], (float)om2[2, 2]);
                        Vector3 nx2 = new Vector3((float)om2[0, 2], (float)om2[1,2], (float)om2[2, 2]);
                        double[,] ommm = Matrix.Multiply(om2, Matrix.Transpose(om2));
                        Vector3 normal = grad.Normal();
                        float dd1 = nx1.Normal().dot(normal);  // should be close to 1/-1 if theory correct
                        float dd2 = nx2.Normal().dot(normal);  // should be close to 1/-1 if theory correct
                        //return new Color((float)om1[0, 0], (float)om1[1, 0], (float)om2[2, 0]);
                        return new Color((float)om1[0, 0], (float)om1[1, 0], dd1, dd2);
            /**/
        }


    }
}
