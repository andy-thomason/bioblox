using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CSG;

namespace CSGFIELD {
	/// <summary>
	/// Interval class implements interval arithmetic.
	/// This is used as an implemention helper for CSG fields.
	/// </summary>
	public class Interval {
		public static Interval MAX = new Interval(Single.MaxValue, Single.MaxValue);
		public static Interval MIN = new Interval(Single.MinValue, Single.MinValue);
		public static Interval ALL = new Interval(Single.MinValue, Single.MaxValue);
		public static Interval ZERO = new Interval(0, 0);
		public float lo;
		public float hi;

		public Interval(float plo, float phi) {
			lo = plo;
			hi = phi;
		}

		public Interval(float p) {
			lo = p;
			hi = p;
		}

		public static Interval operator +(Interval a, Interval b) {
			return new Interval(a.lo + b.lo, a.hi + b.hi);
		}

		public static Interval operator -(Interval a, Interval b) {
			return new Interval(a.lo - b.hi, a.hi - b.lo);
		}

		public static Interval operator -(Interval a, float b) {
			return new Interval(a.lo - b, a.hi - b);
		}

		public static Interval operator *(Interval a, Interval b) {
			float x = a.lo * b.lo;
			float y = a.lo * b.hi;
			float z = a.hi * b.lo;
			float w = a.hi * b.hi;
			return new Interval(x.Min(y, z, w), x.Max(y, z, w));
		}

		public static Interval operator *(Interval a, float b) {
			float x = a.lo * b;
			float y = a.hi * b;
			return new Interval(x.Min(y), x.Max(y));
		}

		public Interval sq() {
			Interval rr;
			if (lo < 0) {
				if (hi > 0)
					rr = new Interval(0, -lo > hi ? lo * lo : hi * hi);  // cross 0
				else
					rr = new Interval(hi * hi, lo * lo);
			} else {
				rr = new Interval(lo * lo, hi * hi);
			}
			return rr;
		}

		public override string ToString() {
			return "[" + lo + ".." + hi + "]";
		}
	}

	public static class floatX {
		public static float Max(params float[] a) {
			return a.Max();
		}

		public static float Min(params float[] a) {
			return a.Min();
		}

		public static float Max(this float a, params float[] b) {
			return Math.Max(a, b.Max());
		}

		public static float Min(this float a, params float[] b) {
			return Math.Min(a, b.Min());
		}
		
	}
}

