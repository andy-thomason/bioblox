
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
		}
		public Sphere(float cx, float cy, float cz, float cr) : this(cx, cy, cz, cr, S.DefaultBakery) {}

		public Sphere(Vector3 p, float r) : this(p.x, p.y, p.z, r) {}
		public override string ToString() { return "Sphere(" + x + "," + y + "," + z + " r=" + r + ")"; }
		public override CSGNode BCopy(Bakery bk) { return MyAttrs(new Sphere (x, y, z, r, bk)); } 

		public override float Dist(float px, float py, float pz) {
			float dd = (x - px) * (x - px) + (y - py) * (y - py) + (z - pz) * (z - pz);
			float d = (float)Math.Sqrt(dd);
			return bk.neg ? r - d : d - r;
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

            return new Cylinder(axx.x, axx.y, axx.z, bxx.x, bxx.y, bxx.z, r * Mathf.Pow(bk.m.Determinant(), (1.0f / 3)), infinite, bk);
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
		                double pbx, double pby,double pbz, double pr, bool inf) : this(
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
			float dot = Vector3.Dot(d, dir);            // distance of 'drop' along centreline
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

	}  // end Cylinder


}
