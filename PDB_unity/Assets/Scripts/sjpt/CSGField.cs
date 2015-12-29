
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
    using Matrix = Matrix4x4;

    /// <summary>
    /// field based node
    /// </summary>
    public abstract class CSGFNODE {
        public abstract Interval field(Volume vol);

        public abstract float field(Vector3 point);

        public abstract CSGFNODE simplify(Volume vol);
    }

    public class CSGFBIG : CSGFNODE {
        public static CSGFBIG std = new CSGFBIG();

        public override Interval field(Volume vol) {
            return Interval.MAX;
        }

        public override float field(Vector3 point) {
            return Single.MaxValue;
        }

        public override CSGFNODE simplify(Volume vol) {
            return this;
        }
    }

    public class CSGFZERO : CSGFNODE {
        public static CSGFZERO std = new CSGFZERO();

        public override Interval field(Volume vol) {
            return Interval.ZERO;
        }

        public override float field(Vector3 point) {
            return Single.MaxValue;
        }

        public override CSGFNODE simplify(Volume vol) {
            return this;
        }
    }

    public abstract class CSGFOP2 : CSGFNODE {
        public override abstract Interval field(Volume vol);

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

        public override Interval field(Volume vol) {
            return l.field(vol) + r.field(vol);
        }

        public override float field(Vector3 point) {
            return l.field(point) + r.field(point);
        }

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
        public float x, y, z, r, ri;
        // centre, radius and radius inverse
        public MSPHERE(float cx, float cy, float cz, float rr) {
            x = cx;
            y = cy;
            z = cz;
            r = rr;
            ri = 1 / rr;
        }

        public MSPHERE(Vector3 c, float rr)
            : this(c.x, c.y, c.z, rr) {
        }

        public float sqrt2 = (float)Math.Sqrt(2);
        
        public static float radInfluence = CSGControl.radInfluence;
        // rad influence
        public static float radInfluence2 = radInfluence * radInfluence;
        // sqr of rad influence
        public static float radInfluenceNorm = 1 / ((1 - radInfluence2) * (1 - radInfluence2));
        // compensate factor
        static MSPHERE() {
            UpdateRadInfluence();
        }

        public static void UpdateRadInfluence() {
            radInfluence = CSGControl.radInfluence;  // rad influence
            radInfluence2 = radInfluence * radInfluence;  // sqr of rad influence
            radInfluenceNorm = 1 / ((1 - radInfluence2) * (1 - radInfluence2));  // compensate factor
        }

        public override Interval field(Volume vol) {
            //throw new Exception("not used");
            Interval dx = (new Interval(vol.x1, vol.x2) - x) * ri;
            Interval dy = (new Interval(vol.y1, vol.y2) - y) * ri;
            Interval dz = (new Interval(vol.z1, vol.z2) - z) * ri;
            Interval r2 = dx.sq() + dy.sq() + dz.sq();
            if (r2.hi < 1)
                return Interval.MAX;
            if (r2.lo > radInfluence2)
                return Interval.ZERO;
            if (r2.lo < 0)
                r2.lo = 0;  // todo, ?? not needed
            if (r2.hi > 2)
                r2.hi = radInfluence2;
            Interval ret = (r2 - radInfluence2).sq() * radInfluenceNorm;
            return ret;
        }

        public override float field(Vector3 point) {
            float dx = (point.x - x) * ri;
            float dy = (point.y - y) * ri;
            float dz = (point.z - z) * ri;
            float r2 = dx * dx + dy * dy + dz * dz;
            if (r2 > radInfluence2)
                return 0;
            return ((r2 - radInfluence2) * (r2 - radInfluence2)) * radInfluenceNorm;
        }

        public override CSGFNODE simplify(Volume vol) {
            // new code using vol as sphere and simple test, much faster
            //bool unew = true;
            //if (unew) {
            float dx = (vol.x - x);
            float dy = (vol.y - y);
            float dz = (vol.z - z);
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
            Vector3 n = new Vector3(p.x - x, p.y - y, p.z - z);
            return n.Normal();
        }
    }

    /// <summary>
    /// tuned class to turn metaballs into a solid
    /// </summary>
    public class CSGFMETA : CSGXPrim {
        int MAXD = 15;
        // max depth, just for allocation
        int MAXN = 2500;
        // max num spheres, just for allocation
        MSPHERE[][] spheres;
        // list of spheres for each recursion level
        int[] levspheres;
        //number of used spheres at each level
        public CSGFMETA() {
            spheres = new MSPHERE[MAXD][];
            levspheres = new int[MAXD];
            for (int d = 0; d < MAXD; d++) {
                spheres[d] = new MSPHERE[MAXN];
                levspheres[d] = 0;
            }
        }

        public override CSGNode BCopy(Bakery bk) {
            this.bk = bk;
            return this;  // todo not complete
        }


        /// <summary>
        /// add an msphere
        /// </summary>
        /// <param name="ms"></param>
        public void Add(MSPHERE ms) {
            spheres[0][levspheres[0]] = ms;
            levspheres[0]++;
        }

        public override float Dist(float x, float y, float z) {
            int inlev = CSGControl.MinLev + 1; // todo mlevspheres[vol.lev];
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

        public override CSGNode Expand(float e) {
            throw new NotImplementedException();
        }


        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            int inlev = vol.lev;
            int outlev = inlev + 1;
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
                v = v.union(new Volume(s.x - rr, s.x + rr, s.y - rr, s.y + rr, s.z - rr, s.z + rr));
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

    }
    // end CSGFSOLID
}


