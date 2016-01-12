
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

//using Microsoft.xna.Framework;
using UnityEngine;
using CSG;

namespace CSGNonPlane {
    using Matrix = Matrix4x4; //pjt Unity;

    /// <summary>
    /// CSG extended primitive, not a plane,
    /// and computed using Dist between primitive and centre of vol (eg assume vol as too big sphere)
    /// subclasses must define Dist, the rest have default implementations
    /// subclasses may override the others for efficiency
    /// </summary>
    public abstract class CSGXPrim : CSGRPrim {

        //protected bool neg = false;

        protected CSGXPrim() { nodes = 1; }

        // for faceted versions
        public static float CylNum = -1;

        public override abstract float Dist(float x, float y, float z);


        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            nUnodes += 9999;  // force these to lowest level
            float dist = Dist(vol.x, vol.y, vol.z);
            if (dist * dist < vol.rsq) return this;
            if (dist > 0) return S.NONE;
            return S.ALL;
        }


        /// <summary>
        /// Accumulate polygons for this primitive associated with volume vol.
        /// Return 1; there must have been just 1 primitive involved in this Draw.
        /// </summary>
        /// <param name="xl"></param>
        /// <param name="vol"></param>
        /// <returns></returns>
        public override int Draw(ICSGOutput xl, Volume vol) {
            Poly poly = PolyForVol(vol); //pjt Unity debugging - getting null here, maybe wrong?
            if (poly != null) {
                poly.AddTo(this, xl);
                if (poly.extraPolys != null)
                    foreach (Poly xp in poly.extraPolys) xp.AddTo(this, xl);
            }
            return 1;
        }


        //@@ public override ulong TT() {
        //    Debug.Assert(S.Simpguid == lastguid + 1);
        //    return ttt;
        //}

        protected Volume ValVol = null;  // volume for which p0Val etc have been computed
        protected Vector3 mid;
        protected float p0Val, pXVal, pYVal, pZVal;  // distance values at four corners

        /// <summary>
        /// compute vals at the corners, and cache
        /// </summary>
        /// <param name="vol"></param>
        protected void ComputeVals(Volume vol) {
            if (vol == ValVol) return;
            mid = new Vector3(vol.x1, vol.y1, vol.z1);
            p0Val = Dist(mid.x, mid.y, mid.z);
            pXVal = Dist(vol.x2, mid.y, mid.z);
            pYVal = Dist(mid.x, vol.y2, mid.z);
            pZVal = Dist(mid.x, mid.y, vol.z2);
            ValVol = vol;
        }

        /// <summary>
        /// Compute a normal to this primitive approprate to this volume
        /// </summary>
        /// <param name="vol"></param>
        /// <returns></returns>
        public virtual Vector3 NormalForVol(Volume vol) {
            ComputeVals(vol);
            Vector3 v = new Vector3(pXVal - p0Val, pYVal - p0Val, pZVal - p0Val);
            if (v.sqrMagnitude == 0) { //pjt Unity. XNA method was LengthSquared()
                v.x = 9999;
                return v;
            }
            return v.Normal();
        }

        /// <summary>
        /// Compute a normal to this primitve appropriate to this point
        /// </summary>
        /// <param name="vol"></param>
        /// <returns></returns>
        public override Vector3 Normal(Vector3 pos) {
            // return Vector3.UnitX; // todo: temp for time testing
            // if (!(((CSGFSOLID)this).fnode is MSPHERE)) { }
            float d = 0.0001f;
            Vector3 n;
            do {
                Volume vol = new Volume(pos.x, pos.x + d, pos.y, pos.y + d, pos.z, pos.z + d);
                n = NormalForVol(vol);
                d *= 10;
                if (d > 2) return Vector3.zero; // .UnitX;
            } while (n.x > 1);
            return n;
        }

        /// <summary>
        /// Compute a point on the primitive surface appropraite to this volume
        /// </summary>
        /// <param name="vol"></param>
        /// <returns></returns>
        public virtual Vector3 PointForVol(Volume vol) {
            Vector3 norm = NormalForVol(vol);
            Vector3 p2 = mid + norm * (vol.x2 - mid.x);
            float val2 = Dist(p2);
            Vector3 v0 = (mid * val2 - p2 * p0Val) * (1 / (val2 - p0Val));
            return v0;
        }

        ///// <summary>
        ///// Bigpoly returns a polygon (assumed planar) that is appropriate to the given vol
        ///// but may be very much larger
        ///// Now obsolete as we are using more efficient splitting methods: this supported the old ones
        ///// </summary>
        ///// <param name="vol"></param>
        ///// <returns></returns>
        //public override Poly Bigpoly(Volume vol) {
        //    Vector3 p = PointForVol(vol);
        //    Vector3 norm = NormalForVol(vol);
        //    Vector3 up = (Math.Abs(norm.y) < 0.9) ? new Vector3(0, 1, 0) : new Vector3(1, 0, 0);
        //    Vector3 UVec3 = Vector3.Cross(up, norm);
        //    Vector3 VVec3 = Vector3.Cross(norm, UVec3);

        //    Poly poly = new Poly(this);
        //    poly.Add(p + UVec3 * 999);
        //    poly.Add(p + VVec3 * 999);
        //    poly.Add(p - (UVec3 + VVec3) * 999);
        //    return poly;
        //}


        // todo:
        public override Vector2 TextureCoordinate(Vector3 pos) {
            return new Vector2(pos.x * 1.5f, pos.z * 1.5f);
        }

        public override abstract CSGNode Expand(float e);


    }

    public class Sphere : CSGXPrim {

        private float x, y, z, r;
        public Sphere(float cx, float cy, float cz, float cr, Bakery bk) {
            x = cx; y = cy; z = cz; r = cr; // neg = cneg;
            nodes = 50;  // temp to force subdivision
            this.bk = bk;
        }
        public Sphere(float cx, float cy, float cz, float cr) : this(cx, cy, cz, cr, S.DefaultBakery) {}

        public Sphere(Vector3 p, float r) : this(p.x, p.y, p.z, r) {}
        public Sphere(float r) : this(0, 0, 0, r) { }

        public override string ToString() { return "Sphere(" + x + "," + y + "," + z + " r=" + r + ")"; }

        public override CSGNode BCopy(Bakery bk) { 
            Vector4 ax = new Vector4(x, y, z, 1);
            Vector4 axx = bk.m.transpose * ax;
            return new Sphere(axx.x, axx.y, axx.z, r * bk.scale(), bk);
        } 

        public override float Dist(float px, float py, float pz) {
            float dd = (x - px) * (x - px) + (y - py) * (y - py) + (z - pz) * (z - pz);
            float d = (float)Math.Sqrt(dd);
            float fd = bk.neg ? r - d : d - r;
            return fd;
        }


        public override CSGNode Expand(float e) {
            return MyAttrs(new Sphere(x, y, z, r + e));
        }

        public override Volume Volume() { return new Volume (x-r, x+r, y-r, y+r, z-r, z+r); } 

    }  // end Sphere

    public class Cylinder : CSGXPrim {

        //private float ax, ay, az, bx, by, bz, r;
        protected Vector3 a, b, dir;      // two ends and direction
        protected float r, len;           // radius and length
        protected bool infinite = false;  // rounded ends if false,

        /// <summary>
        /// clone a Cylinder
        /// </summary>
        /// <param name="c"></param>
        public Cylinder(Cylinder c) :
        this(c.a, c.b, c.r, c.infinite, c.bk) { }

        /// <summary>
        /// Create a cylinder between two end points
        /// </summary>
        /// <param name="pa">End 1</param>
        /// <param name="pb">End 2</param>
        /// <param name="pr">Radius</param>
        /// <param name="inf">If inf is true, this will be an infinite cylinder.  Otherwise it will have rounded ends.</param>
        public Cylinder(Vector3 pa, Vector3 pb, float pr, bool inf) {
            a = pa; b = pb; r = pr;
            dir = b - a;
            len = dir.magnitude; //pjt Unity :: XNA uses Length
            dir /= len;
            nodes = 50;  // temp to force subdivision
            infinite = inf;
        }
        public Cylinder(Vector3 pa, Vector3 pb, float pr, bool inf, Bakery bk) : this(pa,pb, pr, inf) {
            this.bk = bk;
        }

        public override CSGNode BCopy(Bakery bk) { 
            Vector4 ax = new Vector4(a.x, a.y, a.z, 1);
            Vector4 bx = new Vector4(b.x, b.y, b.z, 1);
            Vector4 axx = bk.m.transpose * ax;
            Vector4 bxx = bk.m.transpose * bx;
            float rr = r * bk.scale();

            Cylinder realcyl = new Cylinder(axx.x, axx.y, axx.z, bxx.x, bxx.y, bxx.z, rr, infinite, bk);
            if (CylNum > 0) {
                Vector4 d = bxx - axx;
                CSGNode fc = FCylinder(rr, d.x, d.y, d.z) .At(axx.x, axx.y, axx.z);
                Bakery nbk = bk;
                nbk.m = nbk.invM = Matrix.identity;
                nbk.provstring = "cy#" + realcyl.Id + "/" + nbk.provstring;
                CSGNode bfc = fc.Bake(nbk);
                int nc = bfc.Nodes().Count();
                int i = 0;
                foreach (var v in bfc.Nodes()) {
                    v.realCSG = realcyl;
                    v.realCSGid = i++;
                    v.realCSGnum = nc;
                }
                return bfc;
            }
            return realcyl;
        } // todo allow for neg


        /// <summary>
        /// Create a cylinder between two end points.
        /// </summary>
        /// <param name="pax">End 1</param>
        /// <param name="pay">End 1</param>
        /// <param name="paz">End 1</param>
        /// <param name="pbx">End 2</param>
        /// <param name="pby">End 2</param>
        /// <param name="pbz">End 2</param>
        /// <param name="pr">Radius</param>
        /// <param name="inf">If inf is true, this will be an infinite cylinder.  Otherwise it will have rounded ends.</param>
        public Cylinder(double pax, double pay, double paz,
                        double pbx, double pby,double pbz, double pr, bool inf, Bakery bk) : this(
            new Vector3((float)pax,(float)pay,(float)paz),
            new Vector3((float)pbx,(float)pby,(float)pbz),
            (float) pr, inf, bk) {
        }

        /// <summary>
        /// Create a cylinder between two end points.
        /// </summary>
        /// <param name="pax">End 1</param>
        /// <param name="pay">End 1</param>
        /// <param name="paz">End 1</param>
        /// <param name="pbx">End 2</param>
        /// <param name="pby">End 2</param>
        /// <param name="pbz">End 2</param>
        /// <param name="pr">Radius</param>
        /// <param name="inf">If inf is true, this will be an infinite cylinder.  Otherwise it will have rounded ends.</param>
        public Cylinder(double pax, double pay, double paz,
                        double pbx, double pby,double pbz, double pr, bool inf = false) : this(
            new Vector3((float)pax,(float)pay,(float)paz),
            new Vector3((float)pbx,(float)pby,(float)pbz),
            (float) pr, inf) {
        }

        /// <summary>
        /// Return a CSG derived from this cylinder with the ends flattened by planes.
        /// </summary>
        /// <returns></returns>
        public CSGNode Flat() {
            CSGNode pa = CSGPlane.Make(-dir, a, bk);
            CSGNode pb = CSGPlane.Make(dir, b, bk);
            return S.Intersect(this.Infinite(), pa, pb);
        }

        /// <summary>
        /// return an infinite version of this cylinder
        /// </summary>
        /// <returns></returns>
        public Cylinder Infinite() {
            Cylinder c = (Cylinder) MyAttrs(new Cylinder(this));
            c.infinite = true;
            return c;
        }

        public override Vector3 Normal(Vector3 p) {
            Vector3 d = p - a;                          // vector start -> point
            float dot = d.dot(dir);                        // distance of 'drop' along centreline
            if (!infinite) {
                if (dot < 0) return (bk.neg ? -d : d).Normal();         // rounded end  << TODO NEG
                if (dot > len) return (bk.neg ? b - p : p - b).Normal();   // rounded end  << TODO NEG
            }

            Vector3 v = d - dot * dir;                    // 'drop' vector
            return (bk.neg ? -v : v).Normal();
        }


        /// <summary>
        /// Implement the Dist function required for a CSGXPrim
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="pz"></param>
        /// <returns></returns>
        public override float Dist(float px, float py, float pz) { 
            Vector3 p = new Vector3(px, py, pz);        // point
            Vector3 d = p - a;                          // vector start -> point
            float dot = d.dot(dir);                        // distance of 'drop' along centreline
            if (!infinite) {
                if (dot < 0) return d.magnitude - r;         // rounded end  << TODO NEG
                if (dot > len) return (p - b).magnitude - r;   // rounded end  << TODO NEG
            }

            Vector3 v = d - dot*dir;                    // 'drop' vector
            float di = v.magnitude - r;                      // distance
            return bk.neg ? -di : di;
        }

        public override CSGNode Expand(float e) {
            return MyAttrs(new Cylinder(a, b, r + e, infinite));
        }

        public override Volume Volume() { 
            return new Volume (Mathf.Min (a.x, b.x)-r, Mathf.Max (a.x, b.x)+r, 
                               Mathf.Min (a.y, b.y)-r, Mathf.Max (a.y, b.y)+r, 
                               Mathf.Min (a.z, b.z)-r, Mathf.Max (a.z, b.z)+r);
        }

        public override string ToString() {
            return string.Format("Cylinder#{1}({0})", r, IdX); 
        }

        static CSGNode basecyl;
        //private static int lastcnum = -999;

        /** Make a Faceted Cylinder.
        * If CylNum > 0 use 4*CylNum facets
        * If CylNum == 0 ignore
        * If CylNum < 0 return 'real' cylinder
        * */
        private static CSGNode FCylinder(double r, double x, double y, double z) {

            if (CylNum > 0) {
                int cnum = (int)Math.Ceiling(Math.Sqrt(r) * CylNum);
                Bakery prov = S.DefaultBakery;  // set as Cylinder for sensible vertex normals
                                                // do not use prov for now as it does not get processed correctly by rotate, etc, etc
                                                //prov = new CSGNonPlane.Cylinder(0,0,0, (float)x, (float)y, (float)z, (float)r, true, false);
                                                //prov = null;
                CSGNode c;
                //if (basecyl == null || cnum != lastcnum) {  // caching cylinders does make a huge difference
                    //lastcnum = cnum;
                    CSGNode p1 = S.Plane(1, 0, 0, -1, prov);
                    c = S.ALL;
                    int n = 4 * cnum;
                    for (int i = 0; i < 360; i += 360 / n) {
                        c = c * p1.RotateY(i);
                        //GUIBits.Log ("cyl plane " + i);
                    }
                    //GUIBits.Log ("cyl nodes " + c.Nodes ().Count);
                    basecyl = c;
                //}

                Vector3 a = new Vector3((float)x, (float)y, (float)z);
                c = basecyl.Scale((float)r);
                // no need for cutting planes, they will have been done at a higher level if wanted
                // c = (S.XPlane(0, 1, 0, -a.magnitude) * S.XPlane(0, -1, 0, 0)) * c;

                return c.RotateFromTo(Vector3.up, a);
            } else if (CylNum < 0) {
                return new CSGNonPlane.Cylinder(0, 0, 0, (float)x, (float)y, (float)z, (float)r, false).Flat();
            } else {
                return S.NONE;
            }
        }



    }  // end Cylinder

    public class Cone : CSGXPrim {

        protected Vector3 a, b, dir, apex;      // two ends, direction and apex
        protected float ra, rb, len, asin, angle;           // radius and length, and sin of angle and angle
        protected bool infinite = false;  // rounded ends if false,

        /// <summary>
        /// clone a Cylinder
        /// </summary>
        /// <param name="c"></param>
        public Cone(Cone c) :
        this(c.a, c.b, c.ra, c.rb, c.infinite, c.bk) { }

        /// <summary>
        /// Create a cone between two end points
        /// </summary>
        /// <param name="pa">End 1</param>
        /// <param name="pb">End 2</param>
        /// <param name="pra">Radius</param>
        /// <param name="prb">Radius</param>
        /// <param name="inf">If inf is true, this will be an infinite cylinder.  Otherwise it will have rounded ends.</param>
        public Cone(Vector3 pa, Vector3 pb, float pra, float prb, bool inf) {
            if (pra > prb) {
                a = pa; b = pb; ra = pra; rb = prb;
            } else {
                a = pb; b = pa; ra = prb; rb = pra;
            }
            dir = b - a;
            len = dir.magnitude; //pjt Unity :: XNA uses Length
            dir /= len;
            nodes = 50;  // temp to force subdivision
            apex = (ra * b - rb * a) / (ra - rb);
            angle = Mathf.Atan2( ra - rb, len);
            //float dangle = Mathf.Rad2Deg * angle;
            infinite = inf;
        }

        public Cone(Vector3 pa, Vector3 pb, float pra, float prb, bool inf, Bakery bk) : this(pa,pb, pra, prb, inf) {
            this.bk = bk;
        }

        private static CSGNode FCone(double r1, double r2, double x, double y, double z) {

            if (CylNum > 0) {
                int cnum = (int)Math.Ceiling(Math.Sqrt(Math.Max(r1, r2)) * CylNum);
                // make a real one to get consistent values for angle, apex etc
                float l = (float) Math.Sqrt(x * x + y * y + z * z);
                Cone cone = new Cone(0, 0, 0, 0, l, 0, (float)r1, (float)r2);

                Bakery prov = S.DefaultBakery;  // set as Cylinder for sensible vertex normals
                                                // do not use prov for now as it does not get processed correctly by rotate, etc, etc
                                                //prov = new CSGNonPlane.Cylinder(0,0,0, (float)x, (float)y, (float)z, (float)r, true, false);
                                                //prov = null;
                CSGNode c;
                // caching cone more difficult because of varying slope, don't try for now
                // if (basecyl == null || cnum != lastcnum) {  // caching cylinders does make a significant difference
                //    lastcnum = cnum;
                    CSGNode p1 = S.Plane(1, 0, 0, 0, prov);
                    p1 = p1.RotateZ(Mathf.Rad2Deg * cone.angle * (r1> r2 ? -1 : 1 ) ).At(cone.apex);
                    c = S.ALL;
                    int n = 4 * cnum;
                    // CSGNode p4 = p1 * p1.RotateY(90) * p1.RotateY(180) * p1.RotateY(270);
                    for (int i = 0; i < n; i++) {
                        c = c * p1.RotateY(i * 360f / n);
                        //GUIBits.Log ("cyl plane " + (i * 360f / n));
                    }
                //    //GUIBits.Log ("cyl nodes " + c.Nodes ().Count);
                //    basecyl = c;
                //}

                //c = c.Scale((float)r);
                //c = c * S.XPlane(0, -1, 0, 0);
                Vector3 a = new Vector3((float)x, (float)y, (float)z);
                //c = c * S.XPlane(0, 1, 0, -a.magnitude);

                return c.RotateFromTo(Vector3.up, a);
            } else if (CylNum < 0) {
                return new Cone(0, 0, 0, (float)x, (float)y, (float)z, (float)r1, (float)r2).Flat();
            } else {
                return S.NONE;
            }
        }

        public override CSGNode BCopy(Bakery bk) { 
            Vector4 ax = new Vector4(a.x, a.y, a.z, 1);
            Vector4 bx = new Vector4(b.x, b.y, b.z, 1);
            Vector4 axx = bk.m.transpose * ax;
            Vector4 bxx = bk.m.transpose * bx;

            float sc = bk.scale();
            Cone realcone = new Cone(axx.x, axx.y, axx.z, bxx.x, bxx.y, bxx.z, ra * sc, rb * sc, infinite, bk);
            if (CylNum > 0) {
                Vector4 d = bxx - axx;
                CSGNode wc = FCone(ra*sc, rb*sc, d.x, d.y, d.z).At(axx.x, axx.y, axx.z);
                Bakery nbk = bk;
                nbk.m = nbk.invM = Matrix.identity;
                nbk.provstring = "cone#" + realcone.Id + "/" + nbk.provstring;
                CSGNode bwc = wc.Bake(nbk);
                int nc = bwc.Nodes().Count();
                int i = 0;
                foreach (var v in bwc.Nodes()) {
                    v.realCSG = realcone;
                    v.realCSGid = i++;
                    v.realCSGnum = nc;
                }
                return bwc;
            }
            return realcone;
        } // todo allow for neg


        /// <summary>
        /// Create a cylinder between two end points.
        /// </summary>
        /// <param name="pax">End 1</param>
        /// <param name="pay">End 1</param>
        /// <param name="paz">End 1</param>
        /// <param name="pbx">End 2</param>
        /// <param name="pby">End 2</param>
        /// <param name="pbz">End 2</param>
        /// <param name="pr">Radius</param>
        /// <param name="inf">If inf is true, this will be an infinite cylinder.  Otherwise it will have rounded ends.</param>
        public Cone(double pax, double pay, double paz,
                        double pbx, double pby,double pbz, double pra, double prb, bool inf, Bakery bk) : this(
            new Vector3((float)pax,(float)pay,(float)paz),
            new Vector3((float)pbx,(float)pby,(float)pbz),
                (float) pra, (float) prb, inf, bk) {
        }


    /// <summary>
        /// Create a cone between two end points with two given radii.
        /// </summary>
        /// <param name="pax">End 1</param>
        /// <param name="pay">End 1</param>
        /// <param name="paz">End 1</param>
        /// <param name="pbx">End 2</param>
        /// <param name="pby">End 2</param>
        /// <param name="pbz">End 2</param>
        /// <param name="pra">Radius</param>
        /// <param name="prb">Radius</param>
        /// <param name="inf">If inf is true, this will be an infinite cylinder.  Otherwise it will have rounded ends.</param>
        public Cone(double pax, double pay, double paz,
                        double pbx, double pby,double pbz, double pra, double prb, bool inf = false) : this(
            new Vector3((float)pax,(float)pay,(float)paz),
            new Vector3((float)pbx,(float)pby,(float)pbz),
                (float) pra, (float) prb, inf) {
        }

        /// <summary>
        /// Return a CSG derived from this cylinder with the ends flattened by planes.
        /// </summary>
        /// <returns></returns>
        public CSGNode Flat() {
            CSGNode pa = CSGPlane.Make(-dir, a, bk);
            CSGNode pb = CSGPlane.Make(dir, b, bk);
            return S.Intersect(this.Infinite(), pa, pb);
        }

        /// <summary>
        /// return an infinite version of this cylinder
        /// </summary>
        /// <returns></returns>
        public Cone Infinite() {
            Cone c = (Cone) MyAttrs(new Cone(this));
            c.infinite = true;
            return c;
        }

        public override Vector3 Normal(Vector3 p) {
            Vector3 d = p - apex;                       // vector apex -> point
            Vector3 x1 = d.cross(dir).cross(d).Normal();
            return bk.neg ? -x1 : x1;
        }

        /// <summary>
        /// Implement the Dist function required for a CSGXPrim
        /// </summary>
        /// <param name="px"></param>
        /// <param name="py"></param>
        /// <param name="pz"></param>
        /// <returns></returns>
        public override float Dist(float px, float py, float pz) {
            Vector3 p = new Vector3(px, py, pz);        // point
            Vector3 d = p - apex;                       // vector apex -> point
            float axd = Mathf.Abs(d.dot(dir));            // distance apex to droppoint on centreline
            float gd = Mathf.Sqrt(d.dot(d));            // distance apex to point
            float pointangle = Mathf.Acos(axd / gd);    // angle axis to point vector
            float dangle = pointangle - angle;
            float dist = gd * Mathf.Sin(dangle);
            return bk.neg ? -dist : dist;


        /** old winsom code
        v is current point
        n1 and n2 are 
        xr:=vx-n1.vx; yr:=vy-n1.vy; zr:=vz-n1.vz; 
        l  := n2.vx*xr + n2.vy*yr + n2.vz*zr;          
        v0 := (xr*xr + yr*yr + zr*zr - l*l)*ncos*ncos;  
        v1 := l*nsin;                                   
        vp := sqr(v1+hsize)-v0;                         
        if  v1> hsize                                   
          then vn:=sqr(v1-hsize)-v0                     
          else vn :=-infinity;                          
        expneeded := true;                              
        ***/

        /*** cyl code /float r = (r1+r2)/2;  // very temp
            Vector3 p = new Vector3(px, py, pz);        // point
            Vector3 d = p - a;                          // vector start -> point
            float dot = Vector3.Dot(d, dir);            // distance of 'drop' along centreline
            if (!infinite) {
                if (dot < 0) return d.magnitude - r;         // rounded end  << TODO NEG
                if (dot > len) return (p - b).magnitude - r;   // rounded end  << TODO NEG
            }

            Vector3 v = d - dot*dir;                    // 'drop' vector
            float di = v.magnitude - r;                      // distance
            return bk.neg ? -di : di;
        ***/
        }

        public override CSGNode Expand(float e) {
            return MyAttrs(new Cone(a, b, ra + e, rb + e, infinite));
        }

        public override Volume Volume() {
            float r = Mathf.Max(ra,rb); 
            return new Volume (Mathf.Min (a.x, b.x)-r, Mathf.Max (a.x, b.x)+r, 
                               Mathf.Min (a.y, b.y)-r, Mathf.Max (a.y, b.y)+r, 
                               Mathf.Min (a.z, b.z)-r, Mathf.Max (a.z, b.z)+r);
        }

        public override string ToString() {
            return string.Format("Cone#{1} {2}({0})", ra, rb, IdX); 
        }

    }  // end Cone




}
