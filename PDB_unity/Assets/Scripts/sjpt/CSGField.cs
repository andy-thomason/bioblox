
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using CSG;
using CSGNonPlane;


///
// This file contains classes for implementing field based CSG objects
// including simple metaballs.
// Note: for updated metaballs see the Java code.

namespace CSGFIELD {
    using System.Runtime.CompilerServices;
    using Matrix = Matrix4x4;

    /// <summary>
    /// field based node
    /// </summary>
    public abstract class CSGFNODE {
        public abstract Interval ifield(Volume vol);  // not currently used

        public abstract float field(Vector3 point);
        // public abstract void dd(Vector3 p, out float dxx, out float dxy, out float dxz, out float dyy, out float dyz, out float dzz);


        public abstract CSGFNODE simplify(Volume vol);
    }

    public class CSGFBIG : CSGFNODE {
        public static CSGFBIG std = new CSGFBIG();

        public override Interval ifield(Volume vol) {
            return Interval.MAX;
        }

        public override float field(Vector3 point) {
            return Single.MaxValue;
        }

        //public override void dd(Vector3 p, out float dxx, out float dxy, out float dxz, out float dyy, out float dyz, out float dzz) {
        //    dxx = dxy = dxz = dyy = dyz = dzz = 0;
        //}

        public override CSGFNODE simplify(Volume vol) {
            return this;
        }
    }

    public class CSGFZERO : CSGFNODE {
        public static CSGFZERO std = new CSGFZERO();

        public override Interval ifield(Volume vol) {
            return Interval.ZERO;
        }

        public override float field(Vector3 point) {
            return Single.MaxValue;
        }
        //public override void dd(Vector3 p, out float dxx, out float dxy, out float dxz, out float dyy, out float dyz, out float dzz) {
        //    dxx = dxy = dxz = dyy = dyz = dzz = 0;
        //}


        public override CSGFNODE simplify(Volume vol) {
            return this;
        }
    }

    public abstract class CSGFOP2 : CSGFNODE {
        public override abstract Interval ifield(Volume vol);

        public override abstract float field(Vector3 point);

        public override abstract CSGFNODE simplify(Volume vol);

        protected CSGFNODE l, r;

        public CSGFOP2(CSGFNODE ll, CSGFNODE rr) {
            l = ll;
            r = rr;
        }
    }

    
    public class CSGFPLUS : CSGFOP2 {
        public CSGFPLUS(CSGFNODE ll, CSGFNODE rr)
            : base(ll, rr) {
        }

        public override Interval ifield(Volume vol) {
            return l.ifield(vol) + r.ifield(vol);
        }

        public override float field(Vector3 point) {
            return l.field(point) + r.field(point);
        }

        /***
        public void dd(Vector3 p, out float dxx, out float dxy, out float dxz, out float dyy, out float dyz, out float dzz) {
            float ldxx, ldxy, ldxz, ldyy, ldyz, ldzz;
            float rdxx, rdxy, rdxz, rdyy, rdyz, rdzz;
            l.dd(p, out ldxx, out ldxy, out ldxz, out ldyy, out ldyz, out ldzz);
            r.dd(p, out rdxx, out rdxy, out rdxz, out rdyy, out rdyz, out rdzz);
            dxx = ldxx + rdxx;
            dxy = ldxy + rdxy;
            dxz = ldxz + rdxz;
            dyy = ldyy + rdyy;
            dyz = ldyz + rdyz;
            dzz = ldzz + rdzz;
        }
        ***/


        public override CSGFNODE simplify(Volume vol) {
            CSGFNODE nl = l.simplify(vol);
            if (nl is CSGFBIG)
                return nl;
            CSGFNODE nr = r.simplify(vol);
            if (nr is CSGFBIG)
                return nr;
            if (nl is CSGFZERO)
                return nr;
            if (nr is CSGFZERO)
                return nl;
            
            return new CSGFPLUS(nl, nr);
        }
    }

    /// <summary>
    /// metasphere class
    /// </summary>
    public class MSPHERE : CSGFNODE {
        public float cx, cy, cz, r, ri;  // centre, radius and radius inverse
        public Color color = Color.white;
        public Vector3 c {  get { return new Vector3(cx, cy, cz); } }

        public MSPHERE(float px, float py, float pz, float pr, float radInfluence) {
            cx = px;
            cy = py;
            cz = pz;
            r = pr;
            ri = 1 / pr;
            UpdateRadInfluence(radInfluence);
        }

        public MSPHERE(float px, float py, float pz, float pr) : this(px, py, pz, pr, CSGControl.radInfluence) { }
        public MSPHERE(Vector3 c, float rr, float radInfluence) : this(c.x, c.y, c.z, rr, radInfluence) { }
        public MSPHERE(Vector3 c, float rr) : this(c, rr, CSGControl.radInfluence) { }

        public float strength = 1;
        public float sqrt2 = (float)Math.Sqrt(2);

        public float radInfluence;// = CSGControl.radInfluence;          // rad influence
        public float radInfluence2;// = radInfluence * radInfluence;      // sqr of rad influence
        public float radInfluenceNorm2;// = 1 / ((1 - radInfluence2) * (1 - radInfluence2));    // compensate factor
        public float radInfluenceNorm3;  	                        // compensate factor
        public bool cubic = true;

        public MSPHERE BCopy(Bakery bk, float sc) {
            // Vector3 nc = bk.invM.MultiplyPoint3x4(c);
            Vector4 c = new Vector4(cx, cy, cz, 1);
            Vector4 tc = bk.m.transpose * c;

            MSPHERE nsp = new MSPHERE(tc.x, tc.y, tc.z, r*sc, radInfluence);
            return nsp;
        }

        public void UpdateRadInfluence(float radInfluence) {
            this.radInfluence = radInfluence;  // rad influence
            radInfluence2 = radInfluence * radInfluence;  // sqr of rad influence
            float ddd = 1 / (radInfluence2 - 1);
            radInfluenceNorm2 = ddd * ddd;  // compensate factor
            radInfluenceNorm3 = ddd * ddd * ddd;  // compensate factor

        }

        public override Interval ifield(Volume vol) {
            //throw new Exception("not used");
            Interval dx = (new Interval(vol.x1, vol.x2) - cx) * ri;
            Interval dy = (new Interval(vol.y1, vol.y2) - cy) * ri;
            Interval dz = (new Interval(vol.z1, vol.z2) - cz) * ri;
            Interval r2 = dx.sq() + dy.sq() + dz.sq();
            if (r2.hi < 1)
                return Interval.MAX;
            if (r2.lo > radInfluence2)
                return Interval.ZERO;
            if (r2.lo < 0)
                r2.lo = 0;  // todo, ?? not needed
            if (r2.hi > 2)
                r2.hi = radInfluence2;
            Interval ret = (r2 - radInfluence2).sq() * radInfluenceNorm2; //<< TODO update to cubic if we use Interval
            return ret;
        }

        public override float field(Vector3 p) {
            float dx = (p.x - cx);
            float dy = (p.y - cy);
            float dz = (p.z - cz);
            float d2 = dx * dx + dy * dy + dz * dz;
            float dri2 = d2 * ri * ri;  // scale r2 to operate as if radius is 1
            if (dri2 > radInfluence2) return 0;
            float ddd = radInfluence2 - dri2;
            if (cubic) {
                return ddd * ddd * ddd * radInfluenceNorm3 * strength;
            } else {
                return ddd * ddd * radInfluenceNorm2 * strength;
            }
        }

        public void dd(Vector3 p, out float dxx, out float dxy, out float dxz, out float dyy, out float dyz, out float dzz, out Vector3 grad) {

            float dx1 = (p.x - cx);
            float dy1 = (p.y - cy);
            float dz1 = (p.z - cz);
            float d2 = dx1 * dx1 + dy1 * dy1 + dz1 * dz1;
            float dri2 = d2 * ri * ri;  // scale r2 to operate as if radius is 1
            float ridiff = radInfluence2 - dri2;
            if (ridiff < 0) {
                dxx = dxy = dxz = dyy = dyz = dzz = 0;
                grad = new Vector3();
                return;
            }


            float x = p.x, y = p.y, z = p.z;
            float ri2 = ri * ri;
            float ri4 = ri2 * ri2;

            float dx, dy, dz;
            dxx = (-6 * ((-4 * ridiff * (ri4) * ((x - cx).sq()) * radInfluenceNorm3 * strength) + (((ridiff * ri).sq()) * radInfluenceNorm3 * strength)));
            dxy = (24 * ridiff * (ri4) * (y - cy) * (x - cx) * radInfluenceNorm3 * strength);
            dxz = (24 * ridiff * (ri4) * (z - cz) * (x - cx) * radInfluenceNorm3 * strength);
            dyy = (-6 * ((-4 * ridiff * (ri4) * ((y - cy).sq()) * radInfluenceNorm3 * strength) + (((ridiff * ri).sq()) * radInfluenceNorm3 * strength)));
            dyz = (24 * ridiff * (ri4) * (z - cz) * (y - cy) * radInfluenceNorm3 * strength);
            dzz = (-6 * ((-4 * ridiff * (ri4) * ((z - cz).sq()) * radInfluenceNorm3 * strength) + (((ridiff * ri).sq()) * radInfluenceNorm3 * strength)));
            dx = (-6 * ((ridiff * ri).sq()) * (x - cx) * radInfluenceNorm3 * strength);
            dy = (-6 * ((ridiff * ri).sq()) * (y - cy) * radInfluenceNorm3 * strength);
            dz = (-6 * ((ridiff * ri).sq()) * (z - cz) * radInfluenceNorm3 * strength);

            grad = new Vector3(dx, dy, dz);
        }

        /** for http://www.tusanga.com/calc,
            help at http://www.tusanga.com/help/examples.html#idp221376

            d2 := (x-cx)^2 + (y-cy)^2 + (z-cz)^2;
            dri2 = d2 * ri * ri;
            ddd := radInfluence2 - dri2; 
            f := ddd*ddd*ddd * radInfluenceNorm3 * strength; 
            dxx := diff(diff(f,x),x);
            dxy := diff(diff(f,x),y);
            dxz := diff(diff(f,x),z);
            dyy := diff(diff(f,y),y);
            dyz := diff(diff(f,y),z);
            dzz := diff(diff(f,z),z);

            dx = diff(f,x);
            dy = diff(f,y);
            dz = diff(f,z);


            dyx := diff(diff(f,y),x);
            check := factor(dxy - dyx);
            fdxx := factor(dxx);
            fdxy := factor(dxy);
            edxy := expand(dxy);
>>
dxx := (-6*((-4*(radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*(ri^4)*((x-cx)^2)*radInfluenceNorm3*strength)+((((radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*ri)^2)*radInfluenceNorm3*strength)))
dxy := (24*(radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*(ri^4)*(y-cy)*(x-cx)*radInfluenceNorm3*strength)
dxz := (24*(radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*(ri^4)*(z-cz)*(x-cx)*radInfluenceNorm3*strength)
dyy := (-6*((-4*(radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*(ri^4)*((y-cy)^2)*radInfluenceNorm3*strength)+((((radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*ri)^2)*radInfluenceNorm3*strength)))
dyz := (24*(radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*(ri^4)*(z-cz)*(y-cy)*radInfluenceNorm3*strength)
dzz := (-6*((-4*(radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*(ri^4)*((z-cz)^2)*radInfluenceNorm3*strength)+((((radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*ri)^2)*radInfluenceNorm3*strength)))
dx := (-6*(((radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*ri)^2)*(x-cx)*radInfluenceNorm3*strength)
dy := (-6*(((radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*ri)^2)*(y-cy)*radInfluenceNorm3*strength)
dz := (-6*(((radInfluence2-((((x-cx)^2)+((y-cy)^2)+((z-cz)^2))*(ri^2)))*ri)^2)*(z-cz)*radInfluenceNorm3*strength)

grads = ddd * ddd * 6 * radInfluenceNorm3 * strength * ri * ri;

                        **/

        public override CSGFNODE simplify(Volume vol) {
            // new code using vol as sphere and simple test, much faster
            //bool unew = true;
            //if (unew) {
            float dx = (vol.x - cx);
            float dy = (vol.y - cy);
            float dz = (vol.z - cz);
            float r2 = dx * dx + dy * dy + dz * dz;
            float vr = vol.Radius(); // todo: check R correct
            if (r2 > (vr + radInfluence * r) * (vr + radInfluence * r))
                return CSGFZERO.std;  // vol outside sphere influence
            if (vr < r && r2 < (r - vr) * (r - vr))
                return CSGFBIG.std; // vol inside sphere
            //} else {
            //    // old code using interval field 
            //    if (field(vol).hi == 0) return CSGFZERO.std;
            //    if (field(vol).lo >= 1) return CSGFBIG.std;
            //}
            return this;
        }
        
        // normal at a point
        public Vector3 Normal(Vector3 p) {
            Vector3 n = new Vector3(p.x - cx, p.y - cy, p.z - cz);
            return n.Normal();
        }

        public Vector3 grad(Vector3 pos) {
            float dx = (pos.x - cx);
            float dy = (pos.y - cy);
            float dz = (pos.z - cz);
            float d2 = dx * dx + dy * dy + dz * dz;
            float dri2 = d2 * ri * ri;  // scale r2 to operate as if radius is 1
            float grads;    // grad strength / dist (dist will come back in with size of dd)
            if (dri2 > radInfluence2)
                return new Vector3(0, 0, 0);
            else {
                float ddd = radInfluence2 - dri2;
                if (cubic) {
                    grads = ddd * ddd * 6 * radInfluenceNorm3 * strength * ri * ri;
                } else {
                    grads = ddd * 4 * radInfluenceNorm2 * strength * ri * ri;
                }
            }

            return new Vector3(dx * grads, dy * grads, dz * grads);
        }

    }

    /// <summary>
    /// tuned class to turn metaballs into a solid
    /// </summary>
    public class CSGFMETA : CSGXPrim {
        public bool grayscale = false;
        public static bool computeCurvature = false;

        int MAXD = 15;          // max depth, just for allocation
        int MAXN = 2500;        // max num spheres, just for allocation
        MSPHERE[][] spheres;    // list of spheres for each recursion level
        int[] levspheres;       //number of used spheres at each level

        public CSGFMETA() {
            spheres = new MSPHERE[MAXD][];
            levspheres = new int[MAXD];
            for (int d = 0; d < MAXD; d++) {
                spheres[d] = new MSPHERE[MAXN];
                levspheres[d] = 0;
            }
        }

        public void UpdateRadInfluence(float radInfluence) {
            baked = null;
            for (int s = 0; s < levspheres[0]; s++)
                spheres[0][s].UpdateRadInfluence(radInfluence);
        }

        public override CSGNode BCopy(Bakery bk) {
            CSGFMETA n = new CSGFMETA();
            n.bk = bk;
            for (int s = 0; s < levspheres[0]; s++) {
                n.Add(spheres[0][s].BCopy(bk, bk.scale()));
            }
            return n;  // todo not complete
        }


        /// <summary>
        /// add an msphere
        /// </summary>
        /// <param name="ms"></param>
        public void Add(MSPHERE ms) {
            baked = null;
            spheres[0][levspheres[0]] = ms;
            levspheres[0]++;
        }

        public override float Dist(float x, float y, float z) {
            int inlev = simplev; // todo mlevspheres[vol.lev];
            MSPHERE[] inspheres = spheres[inlev];
            int inn = levspheres[inlev];
            float field = 0;
            Vector3 p = new Vector3(x, y, z);
            for (int ini = 0; ini < inn; ini++) {  // iterate the input spheres
                field += inspheres[ini].field(p);
            }
            // the fields are +ve inside region of influence, but the dist is measured as +ve outside
            return CSGControl.fieldThresh - field;
        }


        /**** generic CSGXPrim Normal is only slightly more expensive, and looks better * /
        public override Vector3 Normal(Vector3 p) {
            int inlev = CSGControl.MinLev + 1; // todo mlevspheres[vol.lev];
            MSPHERE[] inspheres = spheres[inlev];
            int inn = levspheres[inlev];
            //float field = 0;
            Vector3 n = new Vector3();
            //Vector3 p = new Vector3(x, y, z);
            for (int ini = 0; ini < inn; ini++) {  // iterate the input spheres
                float myfield = inspheres[ini].field(p) + 0.0001f;  // delta in case radInf very close to 1
                //field += myfield;
                n += inspheres[ini].Normal(p) * myfield;
            }
            return n.Normal();
        }
        /***********/
/* temporary tilll we do colour properly */
        public override Vector3 Normal(Vector3 p) {
            Vector3 normal;
            Color color;
            normalColor(p, out normal, out color);                   
            return normal;
        }
        /**/

        public override void dd(Vector3 p, out float dxx, out float dxy, out float dxz, out float dyy, out float dyz, out float dzz, out Vector3 grad) {
            dxx = dxy = dxz = dyy = dyz = dzz = 0;
            grad = new Vector3();
            int inlev = simplev; // todo mlevspheres[vol.lev];
            MSPHERE[] inspheres = spheres[inlev];
            int inn = levspheres[inlev];
            for (int ini = 0; ini < inn; ini++) {  // iterate the input spheres
                float ldxx, ldxy, ldxz, ldyy, ldyz, ldzz;
                Vector3 lgrad;
                inspheres[ini].dd(p, out ldxx, out ldxy, out ldxz, out ldyy, out ldyz, out ldzz, out lgrad);
                dxx += ldxx;
                dxy += ldxy;
                dxz += ldxz;
                dyy += ldyy;
                dyz += ldyz;
                dzz += ldzz;
                grad += lgrad;
            }
        }

        //        public @GUIVal(description = "colour threshold for special colour (eg docking difference), -999 means off", level= GUIVal.ADVANCED) float colThresh = -999;
        //        public @GUIVal(description = "field threshold, usually 1", level= GUIVal.ADVANCED) float fieldThresh = 1;  // probably fixed, algorithm is incorrect with other values
        public static float colThresh = -999;

		// grad for metaballs without other overheads.  not generally used, but ...
        public Vector3 grad(Vector3 p) {
            int inlev = simplev;
            MSPHERE[] inspheres = spheres[inlev];
            int inn = levspheres[inlev];
            if (inn == 1) return inspheres[0].grad(p);
            Vector3 grad = Vector3.zero;
            for (int ini = 0; ini < inn; ini++) {  // iterate the input spheres
                float myfield = inspheres[ini].field(p);
                grad += inspheres[ini].grad(p);
            }
            return grad;
        }

        public override void normalColor(Vector3 p, out Vector3 normal, out Color col) {
            int inlev = simplev; // todo mlevspheres[vol.lev];
            MSPHERE[] inspheres = spheres[inlev];
            int inn = levspheres[inlev];

            Vector3 grad = new Vector3();
            col = new Color();
            float ftot = 0;
            if (inn == 0) {  // unexpected
                grad = normal = Vector3.forward;
                col.r = 0; col.g = 1; col.b = 1; col.a = 1; 
            } else if (inn == 1 && colThresh == -999) {  // partly for efficiency, partly for tight radInfluence
                grad = inspheres[0].grad(p);
                normal = inspheres[0].Normal(p);  // don't take from grad, grad might be 0 if out of range, but we can still use normal
                col.set(inspheres[0].color);
            } else {
                //normal.set(0, 0, 0);
                //col.set(0, 0, 0, 0);
                float ftotpos = 0; // sum of positive contributions
                for (int ini = 0; ini < inn; ini++) {  // iterate the input spheres
                    float myfield = inspheres[ini].field(p);
                    grad += inspheres[ini].grad(p);

                    col += inspheres[ini].color * myfield;
                    ftot += myfield;
                    if (myfield > 0) ftotpos += ftot;
                }
                if (colThresh != -999) {
                    float x = ftotpos / colThresh;
                    Color cc = inspheres[0].color;
                    if (!grayscale) {
                        col.set(x * cc.r, 0.1f * cc.g, (1 - x) * cc.b, (x > 0.9f ? 1 : x / 0.9f) * cc.a); //pjt TODO: allow other colour palettes
                    } else col.set(x * cc.r, x * cc.g, x * cc.b, (x > 0.3f ? 1 : x / 0.3f) * cc.a);
                    normal = grad.Normal();
                } else if (ftot < 0.001f) {  // can happen with very tight radInfluence
                    ftot = 0;
                    normal = inspheres[0].Normal(p);
                    grad = Vector3.zero;
                    col.set(inspheres[0].color);
                } else {
                    col /= ftot;
                    normal = grad.Normal();
                }
            }


            if (computeCurvature) {
                if (ftot == 0) {
                    col = new Color(0, 0, 0, 0);  // should take it from first sphere ?
                } else {
                    float dxx, dxy, dxz, dyy, dyz, dzz;
                    Vector3 grad2;   // used as cross-check ?? why is it -ve from grad to check ??
                    dd(p, out dxx, out dxy, out dxz, out dyy, out dyz, out dzz, out grad2);

                    Vector3 gradd = grad + grad2;
                    float ddd = gradd.magnitude;
                    if (ddd > 0.001)
                        GUIBits.LogK("graderr", graderr++ + "");
                    col = Curvature.ctest(dxx, dxy, dxz, dyy, dyz, dzz, grad2);
                    if (float.IsNaN(col.grayscale)) {
                        if (grad2.sqrMagnitude == 0) {
                            col = new Color(0, 1000, 0, 1);
                            GUIBits.Log("odd curvature A {0} dxx{1}, dxy{2}, dxz{3}, dyy{4}, dyz{5}, dzz{6}, grad2{7}, ftot{8}", col, dxx, dxy, dxz, dyy, dyz, dzz, grad2, ftot);
                        } else {
                            GUIBits.Log("odd curvature B {0} dxx{1}, dxy{2}, dxz{3}, dyy{4}, dyz{5}, dzz{6}, grad2{7}, ftot{8}", col, dxx, dxy, dxz, dyy, dyz, dzz, grad2, ftot);
                            col = Color.red;
                        }
                    }
                    normal = -grad2.Normal();
                }
            } else {
                col = CSGXX.colors[Math.Min(inn, CSGXX.colors.Length-1)];
            }

            float s = normal.sqrMagnitude;
            if (s < 0.99 || s > 1.01) {
                GUIBits.Log("odd normal {0}  {1}", s, normal);
            }

        }
        static int graderr = 0;


        public override CSGNode Expand(float e) {
            throw new NotImplementedException();
        }

        int simplev;  // used to record the 'current' depth for which spheres is valid
        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            int inlev = vol.lev;
            int outlev = simplev = inlev + 1;
            MSPHERE[] inspheres = spheres[inlev];
            MSPHERE[] outspheres = spheres[outlev];
            int inn = levspheres[inlev];
            int outn = 0;
            for (int ini = 0; ini < inn; ini++) {  // iterate the input spheres
                CSGFNODE s = inspheres[ini].simplify(vol);
                if (s == inspheres[ini]) {
                    outspheres[outn++] = (MSPHERE)s;
                } else if (s == CSGFZERO.std) {  // outside, so do not pass on
                } else if (s == CSGFBIG.std) {  // inside
                    return S.ALL;
                } else {
                    throw new Exception("unexpected simplificaiton of MSPHERE");
                }
            }
            if (outn == 0)
                return S.NONE;
            levspheres[outlev] = outn;
            return this;
        }

        public override Volume Volume() {
            Volume v = new CSG.Volume(float.MaxValue, float.MinValue, float.MaxValue, float.MinValue, float.MaxValue, float.MinValue);
            for (int i = 0; i < levspheres[0]; i++) {
                MSPHERE s = spheres[0][i];
                float rr = s.r * radInfluence;
                v = v.union(new Volume(s.cx - rr, s.cx + rr, s.cy - rr, s.cy + rr, s.cz - rr, s.cz + rr));
            }
            return v;
        }

    }
    // end CSGFMETA
    
    /// <summary>
    /// generic class to turn a field into a solid
    /// </summary>
    public class CSGFSOLID : CSGXPrim {

        internal CSGFNODE fnode;
        // mainly internal for debug
        public CSGFSOLID(CSGFNODE node)
            : base() {
            nodes = 1;
            fnode = node;
        }

        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            CSGFNODE s = fnode.simplify(vol);
            if (s is CSGFZERO)
                return S.NONE;
            if (s == fnode)
                return this;
            return new CSGFSOLID(s);
        }


        public override CSGNode BCopy(Bakery bk) {
            this.bk = bk;
            return this;  // todo not complete
        }

        public override float Dist(float x, float y, float z) {
            return fnode.field(new Vector3(x, y, z)) - CSGControl.fieldThresh;  // VERY TODO: also make Vector3 parameter version
            // throw new NotImplementedException();
        }
        
        //public override void Nodes(List<CSGPrim> nodelist) {
        //    nodelist.Add(this);
        //    //throw new NotImplementedException();
        //}
        
        //public override void UNodes(List<CSGPrim> nodelist, ref int num, int simpguid) {
        //    // todo: allow for repeat use of this node, fairly unlikely in the near term ...
        //    nodelist.Add(this);
        //    num++;
        //    // throw new NotImplementedException();
        //}
        
        //public override Poly PolyForVol(Volume vol) {
        //    throw new NotImplementedException();
        //}
        
        //public override Vector2 TextureCoordinate(Vector3 pos) {
        //    throw new NotImplementedException();
        //}
        
        //public override Vector3 Normal(Vector3 pos) {
        //    throw new NotImplementedException();
        //}
        

        public override CSGNode Expand(float e) {
            throw new NotImplementedException();
        }

        public override Volume Volume() {
            return UnknownVolume();
        }

    }     // end CSGFSOLID
}


