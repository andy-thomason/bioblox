#define NOSTATS
/* ==================================================================================
CSG todos:
 * more added Dec 2015:
 * equal real cylinders of different colours render randomly, often missing completely
 *         reason? no canon checking for cylinders
 * * should we create a complete graph including provenance, colour, texture?
 * *        and only unwind it (and find equal/canonical planes etc) just before render
 * *        may be cheaper than lots of copying for each Colour() etc, and easier to make reliable
 * above done, but still a little to fix
 *         xor and 'dynamic' Neg
 * *        repeated values in provstring
 *        do not do unnecessary BCopy (eg for Volume and then for render
 * *     remove 'dead' code
 * * faceted cylinders give wrong (base) colour on body of dee when there is a base (floor height fh != 0)
 * *   fixed by correcting provenance
 *
 * add alternative faceted cylinders (to plug in as replacement at Draw time after subdivision), esp. for very small radii

 * old: 
 * material separation, use of material etc on output
 * polygon split with point on split / zero crossing on split [part done]
 *     not worth while so far
 * truth table during split process for early and more complete elimination of redundant
 *
 * complete generation including colour etc, then common up
 *
 * subdivision cuts at primitives rather than regular
 *     almost certainly not worth it, subdivision is only small part
 *
 * optimize CoreRenderMesh to reduce data movement, eg direct placement into right slot
 *
 * get colour right (overrides in place) ~ consider Prepare state so we can just make a graph at creation stage
 *
 * 13 June 2009 added canon to allow same geometric object with different colours
 *   cleaner to separate a colour object that references a geometric object (many->1)
 *   but the current code may be faster
 * 24 Sept 2009 added control to avoid effective use of canon; wrong results
 *
 * November 2011 extensive changes to do provneg correctly, add parallel tests and truth table for lower split levels
 *
 * //@@ tt node not used (we still have tt used for draw/split, using field ttt
 * done:
 * allow for otherwise common primitives but with different colour
 * Transform() to work forwards so it works 'naturally': just make backwards in Plane
 * defer Poly->triangle till render time
 * hybrid array/list implementation of poly (0.0081 to 0.0068)
 * change internal calculations to float (saves around 10%)
 *     double 0.018, float 0.011, mixed (orginal) 0.012
 * implement Expand operator (for helping with roofs), 15% improvement on BigRoof
 * simplify more where we have combined -ve primitives ??? getting close to truth tables
 *     truth tables also part done, but probably not going to help too much ???
 * polygon not to include start point at end
 * allow for -ve primitives and use them in Poly/Split drawing as well as subdivision
 * allow for -ve primitives (plane and -ve)
 * code done, ongoing experiments with depth optimization (where to give up recursion)
 * n-way intersection to get real lines/polys
 * * * part done graphical interface
 * common up primities
 *  * unique primitives, with unique counting
 * common up operations (eg make union(a,b) twice
 *     why does this not help performance much???? should make awkward areas go away.
 *     does in extreme cases (eg difference(x,x))
 *   implement custom hash for MakeShared binary ops (helps 10-15%)
 * take advantage of (near) equal points when Poly is cut
 *    done: from 5628->4937 triangles, small time saving
 * op to recognise a op a.

================================================================================== */

/* ==========================================================================
 * We have several namespaces to factor the CSG code.
 *  CSG             main namespace for computation on planar objects  CSG.cs, FieldPolyForVol.cs
 *  CSGNonPlanar    extra features for Sphere and Cylinder primitives (requires CSG)
 *  CSGFIELD        extra features for fields, including metaballs (requires CSG, CSGNonPlanar)  CSGField.cs, Interval.cs
 *  CSGExtras       features for providing control GUI and test/experimental/timing features  CSGExtras.cs, CSGUI.cs, BigRoof.cs
 *
 *
 * Class heirachy for the CSG namespace.
 * ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
 *
 * CSGControl                   helper class of static control constants
 *  CSGNode                     basic class for all CSG expression nodes
 *      CSGTrivial              trivial nodes
 *          None                node for empty space
 *          All                 node for solid space
 *      CSGPrim                 node for any primitive
 *          CSGRPrim            intermediate class for factoring (??? may not be needed) sphere, metaballs etc hang from here
 *              CSGPlane        arbitrary CSG plane
 *                  PlaneXP     axially aligned plane (for optimization)
 *                  PlaneXN     axially aligned plane (for optimization)
 *                  PlaneYP     axially aligned plane (for optimization)
 *                  PlaneYN     axially aligned plane (for optimization)
 *                  PlaneZP     axially aligned plane (for optimization)
 *                  PlaneZN     axially aligned plane (for optimization)
 *      CSGOP2                  CSG binary operation
 *          Union               CSG union
 *          Difference          CSG difference
 *          Intersect           CSG intersecion
 *          Xor                 CSG exclusive or
 *
 * Poly                         Polygon, used to assemble CSG output
 * PolyDecPoint                 Point within a polygon
 *
 * Subdivide                    Internal class to control subdivision
 * Volume                       Class to hold an easily subdivided voume
 *
 * CSGTT                        Static class holding truth table constants (may have real truth table nodes later ???)
 * VHelpers                     Class with various vector helper functions
 *
 * S                            Class with various CSG construction helpers.
 *
 * ICSGOutput                   Interface to collect output of CSG subdivision process
 *  NOCSGOut                    Throw away subdivision results (for time testing etc)
 *  XList                       Collect results into polygons ordered by texture
 *
 * IColorable                   Iterface to define an object that may be colored
 * IDist                        Interface to define an object to which we can measure a distance
 * ISelectable                  Interface for an object that can be selected
 * IProvenance                  Interface to an object that can refer back to a provenance object
 * InterruptedException         Exception that may be thrown by CSG interrupt

 *  * PrimVertex

 * * ============================================================================ */
using UnityEngine;
﻿
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Diagnostics;

//using TraceNamespace;

//using Mutator2;  // just for Break
using SystemHelpers;

//using CommandNamespace;


namespace CSG {
    using ICSGPoly = Poly;

    // see comments below under Class Poly
    // aliasing for Unity...
    using BoundingBox = Bounds;
    using Matrix = Matrix4x4;

    /// <summary>
    /// Class S is used to define a few helper functions
    /// </summary>
    public static class S {
        public static readonly All ALL = new All();
        public static readonly None NONE = new None();
        public static Bakery DefaultBakery = new Bakery("default", texweight: 0);

        // debug flag

        static S() {
            ALL.canonneg = NONE;
            NONE.canonneg = ALL;
        }


        private static CSGNode[] Append(this CSGNode[] others, CSGNode extra) {
            Array.Resize(ref others, others.Length + 1);
            others[others.Length - 1] = extra;
            return others;
        }

        /// <summary>
        /// Intersect a (nonempty) array of nodes (todo, worth balancing??)
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static CSGNode Intersect(params CSGNode[] nodes) {
            if (nodes.Length == 0)
                return ALL;
            CSGNode n = nodes[0];
            for (int i = 1; i < nodes.Length; i++)
                n = CSG.Intersect.Make(n, nodes[i]);
            return n;
        }

        public static CSGNode Intersect(this IEnumerable<CSGNode> nodes) {
            return Intersect(nodes.ToArray());
        }

        public static CSGNode Intersect(this CSGNode original, params CSGNode[] others) {
            return Intersect(others.Append(original));
        }

        /// <summary>
        /// Union a (nonempty) array of nodes (todo, worth balancing??)
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        public static CSGNode Union(params CSGNode[] nodes) {
            if (nodes.Length == 0)
                return NONE;
            CSGNode n = nodes[0];
            for (int i = 1; i < nodes.Length; i++)
                n = CSG.Union.Make(n, nodes[i]);
            return n;
        }

        public static CSGNode Union(this IEnumerable<CSGNode> nodes) {
            return Union(nodes.ToArray());
        }

        public static CSGNode Union(this CSGNode original, params CSGNode[] others) {
            return Union(others.Append(original));
        }


        /// <summary>
        /// Make a node for a rectangular box by intersecting six planes
        /// </summary>
        /// <param name="Xlow"></param>
        /// <param name="Ylow"></param>
        /// <param name="Zlow"></param>
        /// <param name="Xhigh"></param>
        /// <param name="Yhigh"></param>
        /// <param name="Zhigh"></param>
        /// <returns></returns>
        public static CSGNode Box(float Xlow, float Ylow, float Zlow, float Xhigh, float Yhigh, float Zhigh) {
            float t;
            if (Xlow > Xhigh) {
                t = Xlow;
                Xlow = Xhigh;
                Xhigh = t;
            }
            if (Ylow > Yhigh) {
                t = Ylow;
                Ylow = Yhigh;
                Yhigh = t;
            }
            if (Zlow > Zhigh) {
                t = Zlow;
                Zlow = Zhigh;
                Zhigh = t;
            }
            return Intersect(
                PlaneXP.Make(Xlow, S.DefaultBakery), PlaneXN.Make(Xhigh, S.DefaultBakery),
                PlaneYP.Make(Ylow, S.DefaultBakery), PlaneYN.Make(Yhigh, S.DefaultBakery),
                PlaneZP.Make(Zlow, S.DefaultBakery), PlaneZN.Make(Zhigh, S.DefaultBakery));
        }

        public static CSGNode Box(Vector3 low, Vector3 high) {
            return Box(low.x, low.y, low.z, high.x, high.y, high.z);
        }

        /// <summary>
        /// make a plane
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static CSGNode Plane(float a, float b, float c, float d, Bakery bk) {
            return CSG.CSGPlane.Make(a, b, c, d, bk);
        }

        /// <summary>
        /// make a plane
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        public static CSGNode XPlane(float a, float b, float c, float d) {
            return Plane(a, b, c, d, S.DefaultBakery);
        }

        public static int Simpguid = 1001;
        // global place to generate a guid

        /// <summary>
        /// Clear the cache on known planes
        /// </summary>
        public static void ClearCache() {
            CSGPlane.ClearCache();
            CSGOP2.ClearCache();
        }


        /// <summary>
        /// Perform CSG->CSGOutput
        /// </summary>
        /// <param name="csg"></param>
        /// <param name="vol"></param>
        /// <returns></returns>
        public static void CSGToCSGOutput(this CSGNode csg, Volume vol, ICSGOutput output) {
            CSGNode.GiveUpCount = 0;
            CSGNode.MaxnodesForDraw = 0;

            Subdivide.SetVals();  // set the level control each cycle, so we can tinker with the levels
            Subdivide sd = new Subdivide(output);

            //ClearCache();
            CSGNode csgnp = csg;
            if (CSGControl.CheatProvenance) {
                csgnp = csg.SingleProvenance(csg);
                //ClearCache();
            }
            Poly.fromnew = 0;
            Poly.frompool = 0;
            CSGNode ncsg = csgnp.Bake();    // this should get a copy in the cache, with appropriate deduplication, and new cache
            sd.SD(vol, ncsg, 0);
        }

        public static void CSGToCSGOutput(this CSGNode csg, BoundingBox bb, ICSGOutput output) {
            csg.CSGToCSGOutput(new Volume(bb), output);
        }

        public static T TProvenance<T>(this T applyTo, object prov) where T : IHasProvenance {
            return (T)(applyTo.SetProvenance(prov));
        }



    }
    // utility class S


    /// <summary>
    /// Volume class is an axially aligned rectangular volume
    /// used mainly in subdivision.
    /// For efficient use, all sides should generally have the same length; but no check is made for this.
    /// </summary>
    public class Volume {

        public float x1, x2, y1, y2, z1, z2, x, y, z, rsq;
        public int lev;

        /// <summary>
        /// Define a new volume
        /// </summary>
        public Volume(int plev, float px1, float px2, float py1, float py2, float pz1, float pz2) {
            x1 = px1;
            y1 = py1;
            z1 = pz1;
            x2 = px2;
            y2 = py2;
            z2 = pz2;
            x = (x1 + x2) / 2;
            y = (y1 + y2) / 2;
            z = (z1 + z2) / 2;
            rsq = (x2 - x) * (x2 - x) + (y2 - y) * (y2 - y) + (z2 - z) * (z2 - z);
            lev = plev;
        }

        /// <summary>
        /// define a central cubic volume
        /// </summary>
        // public Volume(float k) : this(-k, k + 0.013, -k, k + 0.017, -k, k + 0.011) { }
        public Volume(float px1, float px2, float py1, float py2, float pz1, float pz2)
            : this(0, px1, px2, py1, py2, pz1, pz2) {
        }

        public Volume(float k)
            : this(-k, k) {
        }

        public Volume(float k1, float k2)
            : this(k1, k2, k1, k2, k1, k2) {
        }

        public Volume(BoundingBox bounds)
            : this(bounds.min.x, bounds.max.x, bounds.min.y, bounds.max.y, bounds.min.z, bounds.max.z) {
        }

        public Bounds Bounds() {
            return new Bounds(Centre(), Size());
        }

        
        public Bounds BoundsExpand(float sc) {
            float s = sc * MaxSize();
            return new Bounds(Centre(), new Vector3(s, s, s));
        }

        /// <summary>
        /// return one of 8 subdivisions of this volume
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public Volume SV(int i) {
            switch (i) {
            case 0:
                return new Volume(lev + 1, x1, x, y1, y, z1, z);
            case 1:
                return new Volume(lev + 1, x1, x, y1, y, z, z2);
            case 2:
                return new Volume(lev + 1, x1, x, y, y2, z1, z);
            case 3:
                return new Volume(lev + 1, x1, x, y, y2, z, z2);
            case 4:
                return new Volume(lev + 1, x, x2, y1, y, z1, z);
            case 5:
                return new Volume(lev + 1, x, x2, y1, y, z, z2);
            case 6:
                return new Volume(lev + 1, x, x2, y, y2, z1, z);
            case 7:
                return new Volume(lev + 1, x, x2, y, y2, z, z2);
            default:
                throw new NotImplementedException();
            }
        }

        static readonly float sqrt3 = (float)Math.Sqrt(3);

        /// <summary>
        /// return the Radius for this volume
        /// </summary>
        /// <returns></returns>
        public float Radius() {
            return (x2 - x) * sqrt3;
        }
        // assume cubic

        /// <summary>
        /// test for point inside volume (inclusive if point exactly on surface)
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Includes(Vector3 p) {
            return (x1 <= p.x && p.x <= x2 &&
            y1 <= p.y && p.y <= y2 &&
            z1 <= p.z && p.z <= z2);
        }

        public override string ToString() {
            return String.Format("Vol({0:0.00}..{1:0.00}, {2:0.00}..{3:0.00}, {4:0.00}..{5:0.00}", x1, x2, y1, y2, z1, z2);
        }

        public Vector3 Centre() {
            return new Vector3((x1 + x2) / 2, (y1 + y2) / 2, (z1 + z2) / 2);
        }

        public Vector3 Size() {
            return new Vector3(x2 - x1, y2 - y1, z2 - z1);
        }

        public float MaxSize() {
            Vector3 s = Size();
            return Mathf.Max(s.x, s.y, s.z);
        }

        public Volume union(Volume a) {
            return new Volume(CSGNode.xmin(this.x1, a.x1), CSGNode.xmax(this.x2, a.x2),
                CSGNode.xmin(this.y1, a.y1), CSGNode.xmax(this.y2, a.y2),
                CSGNode.xmin(this.z1, a.z1), CSGNode.xmax(this.z2, a.z2));
        }

        public Volume intersect(Volume a) {
            return new Volume(CSGNode.xmax(this.x1, a.x1), CSGNode.xmin(this.x2, a.x2),
                CSGNode.xmax(this.y1, a.y1), CSGNode.xmin(this.y2, a.y2),
                CSGNode.xmax(this.z1, a.z1), CSGNode.xmin(this.z2, a.z2));
        }
    }

    /// <summary>
    /// CSGControl is a helper class giving access to various static control values
    /// </summary>
    public class CSGControl {
        /// <summary>Used for debug/performance tests</summary>
        public static int TestRepeats = 1;

        /// <summary>Maximum depth of subdivision that will be considered.</summary>
        public static int MaxLev = 10;

        /// <summary>Minimum level of subdivision.</summary>
        public static int MinLev = 0;

        /// <summary>If more than this many nodes after full subdivision, give up and do not try to draw.</summary>
        public static int GiveUpNodes = 50;

        /// <summary>If this is set, no attempt will be made to common up dynamically generated OP2 nodes.</summary>
        public static bool Op2AlwaysNew = false;

        /// <summary>Trace level to be used for CSG node.</summary>
        public static int TRLEV = 4;

        /// <summary>array size for polygon before overflow into list</summary>
        public static int PNUM = 6;

        /// <summary>
        /// If non-zero, faces will be given a little random color.
        /// Used for debugging and seeing exactly how a csg output has been made.
        /// </summary>
        public static float TextureColourAmount = 0.0f;

        /// <summary>
        /// Sset to true to generate truth tables dynaically during the split process
        /// </summary>
        public static bool Dyntt = false;

        /// <summary>
        /// Set to true to replace top level union with multiple polygons.
        /// rather than doing proper union processing.
        /// This may be faster, but will leave polygons on the inside of solids where they intersect.
        /// </summary>
        public static bool QuickUnion = true;

        /// <summary>
        /// Set to true to ignore provenance information (which may also distort colouring)
        /// This uses provenance to collect information, but then rewrites the expression ingoring.
        /// </summary>
        public static bool CheatProvenance = false;

        /// <summary>should be part of QuickHouse.  If false, generate separate roof for each component (cheaper but not such good results)</summary>
        public static bool CombineComponentRoofMode = true;

        /// <summary>should be part of QuickHouse.  It true, show the lines used to generate custom object maximal convex subobjects.</summary>
        public static bool ShowLinesAsHUD = false;

        /// <summary>should be part of QuickHouse.  Offset used to arrange offset of different rendering levels to clarify some z fighting.</summary>
        public static float ZOFF = 0.00001f;

        /// <summary>
        /// Set to true to use truth table code even where invalid. (eg >6 polys at lowest subdivide point)
        /// May generate wrong answers, but should be quicker.
        /// Partly used for performance tests to give an indication of whether it will be worthwhile to write more specific larger truth table code.
        /// </summary>
        public static bool CheatTruthTable = false;

        /// <summary>Set to true to indicate subdivision should watch for a break point. For debugging only. </summary>
        public static bool UseBreak = false;

        /// <summary>Subdivision will break at this position if UseBreak is set. For debugging only. </summary>
        public static Vector3 BreakPosition;

        public static int nspheres = 20;
        public static int sphereRadx = 25;
        public static float radInfluence = 1.4f;  // how far sphere influence extends, multiple of radius (default value)
        public static bool doBalance = true;
        public static float fieldThresh = 1;
        /// <summary>
        /// true to igore non-adjacent facets during split
        /// </summary>
        public static bool FacetOptimize = true;


        public static void Break(Vector3 b) {
            UseBreak = true;
            BreakPosition = b;
        }
        //[InteractiveCommand] //pjt Unity ::: I might want to reintroduce this attribute in CommandHandlerIsh... for now, simpler to not use it.
        public static void BreakOff() {
            UseBreak = false;
        }

        public void Refresh() {
            //throw new NotImplementedException(); //{}{}
            //XNAControlSet.ReshowAll();
        }

        public static bool Interrupt = false;

        public static bool stats = false;

        public static FilterString filterInput = null;

    }

    /** class used to bake in details prior to rendering csg */
    public struct Bakery {
        public string reason;
        public object provenance;
        public string provstring;
        public int provweight;
        public string texture;
        public int texweight;
        public Matrix m;
        public Matrix invM;
        public bool neg;

        public Bakery Mat(Matrix m) {
            this.m = m;
            this.invM = m.inverse;
            return this;
        }

        private void clearProvenance() {
            this.provenance = null;
            this.provstring = null;
            this.provweight = 0;
            this.texture = null;
            this.texweight = 0;
        }

        public Bakery(string reason, object provenance = null, int provweight = -999, string texture = null, int texweight = -999, bool neg = false) {
            this.reason = reason;
            if (!CSGControl.CheatProvenance) {
                this.provenance = provenance;
                this.provstring = provenance == null ? null : provenance.ToString();
                this.provweight = provweight >= 0 ? provweight : provenance != null ? 10 : 0;
                this.texture = texture;
                if (this.texture != null)
                    this.provstring += "!" + this.texture;
                this.texweight = texweight >= 0 ? texweight : texture != null ? 10 : 0;
            } else {
                this.provenance = null;  // can't call clearProvenance, C# struct initialization rules
                this.provstring = null;
                this.provweight = 0;
                this.texture = null;
                this.texweight = 0;
            }
            this.m = Matrix.identity;
            this.invM = Matrix.identity;
            this.neg = neg;
        }

        /** merge bakeries */
        public static Bakery Merge(Bakery bko, Bakery bkn) {
            if (bko.reason == null) {
                bko = S.DefaultBakery;
                GUIBits.text += "~";
            }
            Bakery n = bko;  // COPY of old bakery
            n.reason += bkn.reason;
            if (CSGControl.CheatProvenance) {
                n.clearProvenance();
            } else {
                //if (bkn.provenance != null)
                if (bkn.provstring != null && bkn.provstring.StartsWith("/" + bkn.provstring)) {
                }
                if (bkn.provstring != null)
                    n.provstring += "/" + bkn.provstring;
                n.provenance = n.provstring;   // <<< for now while fixing
                if (bkn.texweight > bko.texweight) {
                    n.texweight = bkn.texweight;
                    n.texture = bkn.texture;
                }
            }
            n.m = bko.m * bkn.m;
            n.invM = n.m.inverse;  // or bkn.mInv * bko.mInv;

            n.neg = bko.neg != bkn.neg;
            return n;
        }

        public float scale() { return Mathf.Pow(m.determinant, 1f / 3);  }

    }

    /** class used to record pending actions such as transform */
    public class BNode : CSGNode {
        CSGNode s;

        public BNode(CSGNode s, Bakery bk) {
            if (csgmode == CSGMode.rendering)
                throw new Exception("attempt to create BNode during rendering phase");
            this.s = s;
            this.bk = bk;
        }

        public override CSGNode Bake(Bakery bk) {
            Bakery nbk = Bakery.Merge(this.bk, bk);
            return BCopy(nbk);
        }


        public override CSGNode BCopy(Bakery bk) {
            return s.Bake(bk);
        }

        public override string ToStringF(string pre) {
            return pre + this.GetType().Name + "(" + bk.reason + tttstring() + "<" + Texture + ">" +
            (Provenance == null ? "noprov" : Provenance.ToString())
            + s.ToStringF(pre + ".   ") + ")";

        }


        #region implemented abstract members of CSGNode

        public override CSGNode Simplify(CSG.Volume vol, int simpguid, ref int nUnodes) {
            throw new UnbakedException();
        }

        public override CSGNode SimplifyWithFix(CSGPrim node, CSGNode fix) {
            throw new UnbakedException();
        }

        public override bool Involves(CSGPrim node) {
            throw new UnbakedException();
            //return s.Involves (node);
        }

        //        public override IColorable WithTextureI(string texture, bool force) {
        //            throw new NotImplementedException();
        //        }

        public override float Dist(float x, float y, float z) {
            throw new UnbakedException();
        }

        public override void Nodes(LinkedList<CSGPrim> nodelist) {
            s.Nodes(nodelist);
        }

        public override void UNodes(LinkedList<CSGPrim> nodelist, ref int num, int simpguid) {
            throw new UnbakedException();
        }

        public override CSGNode Expand(float e) {
            throw new NotImplementedException();
        }

        public override CSG.Volume Volume() {
            return s.Volume();
        }

        #endregion

    }

    public enum CSGMode { collecting, baking, rendering };

    /// <summary>
    /// Node class is any node of a CSG tree
    /// </summary>
    public abstract class CSGNode : CSGControl, IDist, IColorable, IHasProvenance {
//?        [ThreadStatic]
        protected static int rid = 0;
        public int Id = rid++;
        /// <summary>Count of give ups
        public static int GiveUpCount = 0;
        public static int MaxnodesForDraw = 0;

        public static CSGMode csgmode = CSGMode.collecting;

        protected CSGNode baked = null;
        /// <summary>
        /// Top level call to bake the node so that it is ready for rendering etc
        /// The result is cached.
        /// </summary>
        /// <returns></returns>
        public CSGNode Bake() {
            if (baked != null) return baked;
            try {
                csgmode = CSGMode.baking;
                S.ClearCache();
                baked = Bake(S.DefaultBakery);
            } finally {
                csgmode = CSGMode.collecting;
            }
            return baked;
        }

        /// <summary>
        /// Bake the node so it is ready for rendering etc
        /// This is a recursive process, and the bakery bk passed down gives the context of the node:
        /// including transform, negative/positive status, and provenance.
        /// At the root (primitive) nodes the transforms etc are applied to the objects for efficiency during render.
        /// </summary>
        /// <param name="bk"></param>
        /// <returns></returns>
        public virtual CSGNode Bake(Bakery bk) {
            //Bakery nbk = Bakery.Merge(this.bk, bk);
            if (CSGControl.filterInput != null) {
                if (this is CSGPrim && !CSGControl.filterInput.filter(bk.provstring))
                    return S.NONE;
            }
            CSGNode rr = BCopy(bk);
            if (csgmode == CSGMode.rendering && rr.bk.provstring != this.bk.provstring)
                GUIBits.Log("prov error " + csgmode + " " + this + " " + rr + " prov " + this.Provenance + " <-> " + rr.Provenance);
            return rr;
        }

        /// <summary>
        /// Make and return a copy of the node with the given bakery information applied.
        /// </summary>
        /// <param name="bk"></param>
        /// <returns></returns>
        public abstract CSGNode BCopy(Bakery bk);


        /// <summary>
        /// Make and return a TNode parent, with additional bakery information bk.
        /// During baking the information from bk (and higher level TNodes) is accumulated and applied to (a copy of) the node.
        /// </summary>
        /// <param name="bk"></param>
        /// <returns></returns>
        public CSGNode TNode(Bakery bk) {
            return new BNode(this, bk);
        }

        /// <summary>
        /// return a descriptive id, including information about canon (if any).
        /// </summary>
        public string IdX {
            get {
                string s = "" + Id;
                if (canon != this)
                    s += "!" + canon.IdX;
                if (canonneg != null)
                    s += "~" + canonneg.canon.Id;
                return s;
            }
        }

        public int nodes = 0;
        protected const float delta = 0.001f;
        // static bool Close(float a, float b) { return Math.Abs(a - b) < delta; }
        public static bool Small(float a) {
            return -delta < a && a < delta;
        }

        /// test for a small number

        internal CSGNode canonneg;
        // canonical node with negative value
        internal CSGNode provneg;
        // node with negative value accoriding to UseTTCanon rules
        private CSGNode _canon;
        // canonical node with this geometry
        public CSGNode canon {
            get { return _canon; }
            internal set { _canon = value; }
        }

        protected CSGNode() {
            _canon = this;
            // _ttcanon = this;
        }


        /// <summary>
        /// Simplify eliminates the parts of the tree not relevant in this given Volume
        /// and returns the simplified node
        /// </summary>
        /// <param name="vol"></param>
        /// <param name="simpguid"></param>
        /// <returns></returns>
        public abstract CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes);

        /// <summary>
        /// SimplifyWithFix does not do any geometry testing
        /// but 'fixes' any occurence within this CSG tree of the given node to the given fix value.
        /// This will often be one of S.ALL, S.NONE.
        /// </summary>
        /// <param name="vol"></param>
        /// <param name="fix"></param>
        /// <returns></returns>
        public abstract CSGNode SimplifyWithFix(CSGPrim node, CSGNode fix);

        /// <summary>
        /// Involves does not do any geometry testing
        /// but looks to see if node uses the given primitive
        /// TODO: consider if this can easily be compbined into SimplifyWithFix
        /// TODO: consider if we can implement with truth table (may need different parameters)
        ///    (part done by using truth tables before call to Involves as alternative)
        /// </summary>
        /// <param name="vol"></param>
        /// <param name="simpguid"></param>
        /// <returns></returns>
        public abstract bool Involves(CSGPrim node);

        public Bakery bk = new Bakery("newcsg");

        // protected string _texture = CSGControl.DefaultTexture;

        /// <summary>
        /// texture for this node and subnodes
        /// It is up to the using application to decide how to use this field.
        /// </summary>
        public string Texture { get { return bk.texture; } }

        public static Color DefaultColor = new Color(0, 0, 0, 0);
        // use numbers as XNA too silly to keep same name for this over different versions
        /// <summary> flag to indicate if this node has a color applied </summary>
        public bool HasColor = false;
        Color color = DefaultColor;

        public Color Color {
            get { return color; }
            set {
                color = value;
                HasColor = true;
            }
        }


        CSGNode lastSimp;
        internal int lastguid = 0;
#if TTT
        internal ulong ttt;
#endif
        // this is the truth table, valid only if lastguid == simpguid

        // internal Simplify with 'opimization' (?) for reuse of csg
        // benefit is not avoiding new operations,
        // but improving recognition and elimination of common patterns
        internal CSGNode OSimplify(Volume vol, int simpguid, ref int nUnodes) {
            if (lastguid == simpguid)
                return lastSimp;
            lastguid = simpguid;
            lastSimp = Simplify(vol, simpguid, ref nUnodes);
            return lastSimp;
        }

        /// <summary>
        /// gives the distance of point (x,y,z) from the csg (-ve for inside)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public abstract float Dist(float x, float y, float z);

        /// <summary>
        /// accumulate the nodes involved in this expression node to nodelist
        /// </summary>
        /// <param name="nodelist"></param>
        public abstract void Nodes(LinkedList<CSGPrim> nodelist);

        /// <summary>
        /// accumulate the unique nodes
        /// simpguid is a unique identifier for this particular simplification instance.
        /// and is used to 'stamp' each primitive to see if it is already in the list or if it is new.
        /// WARNING: this will not work if you try to parallelize the implementation.
        /// </summary>
        /// <param name="nodelist">accumulated node list</param>
        /// <param name="num">number of nodes collected so far (in/out parameter)</param>
        /// <param name="simpguid">unique identifier for this particular simplification instance</param>
        public abstract void UNodes(LinkedList<CSGPrim> nodelist, ref int num, int simpguid);

        public LinkedList<CSGPrim> Nodes() {
            LinkedList<CSGPrim> nodelist = new LinkedList<CSGPrim>();
            Nodes(nodelist);
            return nodelist;
        }

        public CSGPrim[] UNodes() {
            LinkedList<CSGPrim> nodelist = new LinkedList<CSGPrim>();
            int n = 0;
            UNodes(nodelist, ref n, S.Simpguid++);

            // return nodelist.Take(n).ToArray();
            CSGPrim[] nodelist2 = nodelist.ToArray();
            return nodelist2;
        }

        //@@ public abstract ulong TT();

        public CSGNode MyAttrs(CSGNode node) {
            return node;
        }

#if TTT
        internal const int TTNUM = 6;
        // max number of nodes for truth table
        static int MAXTTTEST = TTNUM;
        // max number of nodes to use truth tables for testing involvement
        //static int MAXTTUSE = TTNUM;  // min number of nodes to use truth tables for processing
#endif

        /// <summary>
        /// draw the csg in the volume, accumulate result in pl
        ///
        /// returns number of unique prims
        /// </summary>
        /// <param name="pl"></param>
        /// <param name="vol"></param>
        public virtual int Draw(ICSGOutput pl, Volume vol) {
            if (nodes == 1) {
            }

            //// find the nodes we use
            //List<CSGPrim> nodelist = Nodes();

            //// experiment to check for stats when we allow for neg
            //// uhitsA is with no neg equality, uhitsB allowing for neg equality ~ makes 6 prim 64 bit ttable look worthwhile
            //    //hits:  0->0  1->8  2->8  3->20  4->8  5->24  6->40  7->18  8->22  9->28  10->46  11->4  12->16
            //    //uhitsA:  0->0  1->8  2->18  3->22  4->54  5->48  6->22  7->50  8->18  9->2
            //    //uhitsB:  0->0  1->8  2->22  3->26  4->72  5->30  6->42  7->30  8->12

            CSGPrim[] nodea = UNodes();
            // if (nodea.Length > 0) return 0;
            MaxnodesForDraw = Math.Max(MaxnodesForDraw, nodea.Length);
            if (nodea.Length > CSGControl.GiveUpNodes) {
                GiveUpCount++;
                return nodea.Length;  // just too difficult // TODO:
            }

            //@@ // truth table draw
            //if (nodea.Length < TTNUM) {
            //    ulong tt = TT();
            //    CSGTT csgtt = new CSGTT(nodea, tt);
            //    csgtt.TTDraw(pl, vol);
            //    return nodea.Length;
            //}
            if (CSGControl.UseBreak && vol.Includes(CSGControl.BreakPosition)) {
                CSGNode xnode = SimplifyWithFix(PlaneXN.Make(99999, S.DefaultBakery), S.NONE);  // to get truth tables generated for debug
                Console.WriteLine(">>>>>>>>>>>> CSG at breakpoint " + nodea);
                Console.WriteLine(xnode.ToStringF("\n"));
                Console.WriteLine("<<<<<<<<<<<< CSG at breakpoint " + nodea);
                foreach (var u in nodea)
                    Console.WriteLine("{0} {1} {2}", u, u.Provenance, u.Texture);
                Console.WriteLine("Vol = " + vol);
            } else if (CSGControl.UseBreak) {
                return 0; // insert breakpoint here if you want it Debugger.Break();  
            }

            // get truth table data ready
            //for (int i = 0; i < nodea.Length; i++) {
            //    CSGPrim node = nodea[i];
            //    ulong oldttt = node.ttt;
            //    node.ttt = (i < CSGTT.TTI.Length) ? CSGTT.TTI[i] : CSGTT.TTIX;
            //    if (node.canonneg != null) node.canonneg.ttt = ~node.ttt;


            // and create polygons:  interate over each primitive i
            // and for each primitive do recurse subdivision of remaining primitives
            for (int i = 0; i < nodea.Length; i++) {
                CSGPrim node = nodea[i];
                ICSGPoly poly = node.PolyForVol(vol);  // find the complete polygon
                if (poly != null) {

                    // DEBUG TODO
                    if (Poly.windDebug && !(node as CSGPlane).CheckPolyWind(poly)) {
                        //ICSGPoly pp = Poly.Make();
                        //for (int ii = poly.Count - 1; ii >= 0; ii--) pp.Add(poly[ii].point);
                        poly = poly.Reverse();
                        if (poly != null && !(node as CSGPlane).CheckPolyWind(poly)) {
                        }
                    }
                    Split(poly, nodea, i, 0, pl);      // and accumulate the bits into pl
                    poly.ConditionalReleaseToPool();
                }
            }
            return nodea.Length;

        }

        /// <summary>
        /// Helper for Draw.
        /// Cut the polygon into two one recursion at a time.
        /// At the bottom level a final polygon is found and accumulated into pl.
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="nodea"></param>
        /// <param name="i"></param>
        /// <param name="nextcuti"></param>
        /// <param name="pl"></param>
        void Split(ICSGPoly poly, CSGPrim[] nodea, int i, int nextcuti, ICSGOutput pl) {
            CSGPrim polynode = nodea[i];
            if (poly == null)
                return;
            //if (!Involves(nodea[i]))  // this never seems to happen so not worth checking, to check what ensures this.
            //    return;

            while (nextcuti < nodea.Length) {
                if (nextcuti == i) {
                    nextcuti++;
                    continue;
                }
                if (nextcuti < nodea.Length && (polynode.canon == nodea[nextcuti].canon || polynode.canon == nodea[nextcuti].canonneg)) {
                    nextcuti++;
                    continue;
                }
                if (!Involves(nodea[nextcuti])) {
                    nextcuti++;
                    continue;
                }

                /* optimize facet intersection */
                if (CSGControl.FacetOptimize && nodea[i].realCSG != null && nodea[i].realCSG == nodea[nextcuti].realCSG) {
                    int d = nodea[i].realCSGid - nodea[nextcuti].realCSGid;
                    if (Math.Abs(d) != 1 && Math.Abs(d) != nodea[i].realCSGnum - 1) {
                        nextcuti++;
                        continue;
                    }
                }
                /**/
                
                break;
            }

            if (nextcuti >= nodea.Length) { // we have finished all cutting levels, mark the poly and use it
                if (this.canon != nodea[i].canon && this.canonneg != nodea[i].canon) {
                }         // for debug
                if (this is CSGPlane) {
                    CSGPlane p = (CSGPlane)this;
                    if (p.a == 0 && p.c == 0 /*&& p.b * p.d == -6*/) {
                    }
                }  // for debug

                CSGNode nn;
                //nn = nodea[i];
                //while(nn is CSGOP2) { CSGOP2 n = nn as CSGOP2; nn = n.l.Id < n.r.Id ? n.l : n.r; }
                nn = this.BestMatch(nodea[i]);
                if (nn != nodea[i])
                    i += 0;
                //if (!(nn is CSGPrim)) { poly.AddTo(nodea[i], pl); return;  } // TEMP TODO verify        // test for incomplete simplification
                if (nn.canon == nodea[i].canonneg)
                    poly = poly.Reverse();
                else if (nn.canon != nodea[i].canon) {
                    // debug here
                }
                poly.AddTo(nn as CSGPrim, pl);    // original code: allows for negatives, used to fail incomplete simplification such as (All xor Plane)
                return;
            }

            CSGPrim cutnode = nodea[nextcuti];
#if TTT
            // prepare truth tables for the lower end of the recursion.
            // (This does not give as big a performance gain as I had hoped, and sometimes gets wrong answers.)
            int ttl; // truth table length that would be needed
            if (CSGControl.Dyntt) {
                ttl = nodea.Length - nextcuti - 1;  // from nextcuti+1 to length-1, inclusive
                if (i < nextcuti)
                    ttl++;                // plus an extra if i is too low
                if (ttl == TTNUM) {                     // must make a new one
                    int ti = 0;                         // index into truth tables
                    int ai;                             // index into anode
                    if (i < nextcuti) {
                        nodea[i].ttt = CSGTT.TTI[ti];
                        if (nodea[i].provneg != null)
                            nodea[i].provneg.ttt = ~nodea[i].ttt;
                        ti++;
                    }
                    for (ai = nextcuti + 1; ai < nodea.Length; ai++) {
                        nodea[ai].ttt = CSGTT.TTI[ti];
                        if (nodea[ai].provneg != null)
                            nodea[ai].provneg.ttt = ~nodea[ai].ttt;
                        ti++;
                    }
                    if (ti != TTNUM) {
                        Debugger.Break();
                    }
                }
            } else {
                ttl = nodea.Length;
            }
#endif


            CSGNode csgin, csgout;
            // check for special cases
            if (cutnode.canon == polynode.canon) {
                csgin = csgout = SimplifyWithFix(cutnode, polynode.canon);
            } else if (cutnode.canon == polynode.canonneg) {
                csgin = csgout = SimplifyWithFix(cutnode, polynode.canonneg);
            } else if (cutnode.QParallel(polynode)) {
                Vector3 p = poly[0].point;  // a point on the poly, eg on the plane polynode
                float d = cutnode.Dist(p.x, p.y, p.z);
                if (d > 0)
                    csgin = csgout = SimplifyWithFix(cutnode, S.NONE);
                else
                    csgin = csgout = SimplifyWithFix(cutnode, S.ALL);
            } else {
                // we cut the polygon into two using node cutnode
                // simplify for both sides, trying cutnode both true and false
                csgin = SimplifyWithFix(cutnode, S.ALL);
                csgout = SimplifyWithFix(cutnode, S.NONE);
            }


            // make sure the simplified versions actually use the node of interest, polynode
            // use the truth table version if possible: cheaper and fewer(no) false positives
            // (TODO: fold tt code more into basic operations rather than here)
#if TTT
            MAXTTTEST = TTNUM;
            if (CSGControl.Dyntt && ttl <= MAXTTTEST) {
                int shift = 1 << i;
                if (((csgin.ttt >> shift ^ csgin.ttt) & CSGTT.TTI[i]) == 0) {
                    //if (csgin.ttt != 0 && csgin.Involves(polynode)) { Console.WriteLine(csgin.ToString("\n")); Debugger.Break(); }
                    csgin = S.NONE;
                }
                if (((csgout.ttt >> shift ^ csgout.ttt) & CSGTT.TTI[i]) == 0) {
                    //if (csgout.ttt != 0 && csgout.Involves(polynode)) { Debugger.Break(); }
                    csgout = S.NONE;
                }
            } else {
                if (!csgin.Involves(polynode))
                    csgin = S.NONE;
                if (!csgout.Involves(polynode))
                    csgout = S.NONE;
            }
#else
                            if (!csgin.Involves(polynode))
                    csgin = S.NONE;
                if (!csgout.Involves(polynode))
                    csgout = S.NONE;
#endif
            if (csgin.nodes == 0 && csgout.nodes == 0)
                return;  // both sides trivial, so nothing to be seen


            //// to consider: might be different even if ttt match, because ttt is now provenance-independent sjpt 15 Aug 2012
            //// 12 sept 2012, yes, found example where this gave wrong answer
            //if (csgin == csgout || (ttl <= MAXTTUSE && (csgin.ttt == csgout.ttt))) {  // both sides the same, so don't bother to do this cut, just recurse
            //    // todo: it would be good to have a more complete test [truth tables?]
            //    csgin.Split(poly, nodea, i, nextcuti + 1, pl);
            //    return;
            //}

#if TTT
            // ttt is not currently reliable for > 6 nodes handled in the case above [never will be while it's ulong]
            // However, this cheat can give some approximation and an idea of cost savings we may get from truth tables
            if (CSGControl.CheatTruthTable && csgin.ttt == csgout.ttt) {
                csgin.Split(poly, nodea, i, nextcuti + 1, pl);
                return;
            }
#endif

            // nothing easy, so do the cut and find both sides
            ICSGPoly outpoly, inpoly;
            poly.Split(cutnode, out inpoly, out outpoly);

            // and recurse on down
            if (!(csgin.nodes == 0) && inpoly != null)
                csgin.Split(inpoly, nodea, i, nextcuti + 1, pl);
            if (!(csgout.nodes == 0) && outpoly != null)
                csgout.Split(outpoly, nodea, i, nextcuti + 1, pl);
            if (inpoly != null)
                inpoly.ConditionalReleaseToPool();
            if (outpoly != null)
                outpoly.ConditionalReleaseToPool();

        }

        public bool Inside(float x, float y, float z) {
            return Dist(x, y, x) < 0;
        }



        /// <summary>
        /// implementation of IProvenance
        /// Might (for example) point back to a particular wall of a particular building.
        /// This does NOT have a property set: setting provenance must create a new object
        /// </summary>
        public virtual object Provenance { get { return bk.provenance ?? "noprov"; } }
        // return this with provenance, needed because C# won't allow subtype overloading
        public CSGNode WithProvenance(object prov) { 
            return TNode(new Bakery("prov", provenance: prov)); 
        }
        public CSGNode WP(object prov) {
            return TNode(new Bakery("prov", provenance: prov));
        }

        public virtual IHasProvenance SetProvenance(object prov) {
            Console.WriteLine("SetProvenance not correctly implemented for " + GetType());
            bk.provstring += "/" + prov;
            return this;
        }

        public virtual CSGNode SingleProvenance(object prov) {
            Console.WriteLine("SetProvenance not correctly implemented for " + GetType());
            bk.provstring = prov.ToString();
            return this;
        }

        public string Stats() {
            return string.Format("stats id:{0} nodes={1} unodes={2}", Id, nodes, Nodes().Distinct().Count());
        }

        /// <summary>
        /// Make a copy of this node translated by x,y,z
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public CSGNode Translate(double x, double y, double z) {
            //pjt Unity 
            //Matrix tm = Matrix.CreateTranslation(new Vector3((float)x, (float)y, (float)z));
            Matrix tm = Matrix.identity;
            tm.SetRow(3, new Vector4((float)x, (float)y, (float)z, 1)); //duh, SetRow or SetColumn?

            CSGNode copy = T_Bake(tm, "translate");
            return copy;
        }

        public CSGNode At(double x, double y, double z) {
            return this.Translate(x, y, z);
        }

        public CSGNode At(Vector3 v) {
            return this.Translate(v.x, v.y, v.z);
        }


        public CSGNode Stretch(double x, double y, double z) {
            Matrix tm = Matrix.identity; //was Matrix.Identity in XNA
            //pjt Unity ::: 
            tm.m11 = (float)(x);
            tm.m22 = (float)(y);
            tm.m33 = (float)(z);
            tm = tm.transpose; //XNA was row major. I suppose it would be marginally more efficient to do it right to begin with...
            
            CSGNode copy = T_Bake(tm, "stretch");
            return copy;
        }

        /// <summary>
        /// Make a copy of this node rotated by rot degrees
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public CSGNode RotateY(double rot) {
            //pjt Unity; ///Matrix.CreateRotationY((float)MathHelper.ToRadians((float)rot));
            //sure would be nice to have some unit testing... don't know how long before I hit this method...
            Matrix tm = Matrix.TRS(new Vector3(), Quaternion.Euler(new Vector3(0, (float)rot)), new Vector3(1, 1, 1)); 
            CSGNode copy = T_Bake(tm, "rotateY");
            return copy;
        }

        public CSGNode RotateFromTo(Vector3 from, Vector3 to) {
            to = to.Normal();
            from = from.Normal();
            Vector3 ax = to.cross(from);
            float dot = to.dot(from);
            float ang = Mathf.Asin(ax.magnitude) * Mathf.Rad2Deg;
            if (ang == 0)
                return dot > 0 ? this : RotateX(180);
            else
                return this.RotateAA(ax.Normal(), dot > 0 ? ang : 180 - ang);

        }

        /// <summary>
        /// Make a copy of this ode rotated about an arbitrary axis by a given angle
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        /// <returns></returns>
        public CSGNode RotateAA(Vector3 axis, float angle) {
            //Matrix tm = Matrix.CreateFromAxisAngle(axis, angle); //pjt Unity... would be nice if Matrix4x4 had some more of these methods...
            //pjt Unity;
            //sure would be nice to have some unit testing... don't know how long before I hit this method...
            Matrix tm = Matrix.TRS(new Vector3(), Quaternion.AngleAxis(angle, axis), new Vector3(1, 1, 1)); 
            return T_Bake(tm, "rotateAA");
        }

        public CSGNode RotateX(double rot) {
            //Matrix tm = Matrix.CreateRotationX((float)MathHelper.ToRadians((float)rot));
            //pjt Unity; 
            //sure would be nice to have some unit testing... don't know how long before I hit this method...
            Matrix tm = Matrix.TRS(new Vector3(), Quaternion.Euler(new Vector3((float)rot, 0)), new Vector3(1, 1, 1)); 
            CSGNode copy = T_Bake(tm, "rotateX");
            return copy;
        }

        public CSGNode RotateZ(double rot) {
            //Matrix tm = Matrix.CreateRotationZ((float)MathHelper.ToRadians((float)rot));
            //pjt Unity; ///Matrix.CreateRotationY((float)MathHelper.ToRadians((float)rot));
            //sure would be nice to have some unit testing... don't know how long before I hit this method...
            Matrix tm = Matrix.TRS(new Vector3(), Quaternion.Euler(new Vector3(0, 0, (float)rot)), new Vector3(1, 1, 1)); 
            CSGNode copy = T_Bake(tm, "rotateZ");
            return copy;
        }

        public CSGNode Difference(CSGNode n2) {
            return CSG.Difference.Make(this, n2);
        }

        public CSGNode Un(CSGNode n2) {
            return CSG.Union.Make(this, n2);
        }

        public CSGNode Int(CSGNode n2) {
            return CSG.Intersect.Make(this, n2);
        }

        public static CSGNode operator +(CSGNode n1, CSGNode n2) {
            return CSG.Union.Make(n1, n2);
        }

        public static CSGNode operator *(CSGNode n1, CSGNode n2) {
            return CSG.Intersect.Make(n1, n2);
        }

        public static CSGNode operator -(CSGNode n1, CSGNode n2) {
            return CSG.Difference.Make(n1, n2);
        }

        public static CSGNode operator ^(CSGNode n1, CSGNode n2) {
            return CSG.Xor.Make(n1, n2);
        }

        public CSGNode Scale(float s) {
            //Matrix tm = Matrix.CreateScale(s);
            //pjt Unity
            //sure would be nice to have some unit testing... don't know how long before I hit this method...
            Matrix tm = Matrix.TRS(new Vector3(), Quaternion.Euler(new Vector3(0, 0, 0)), new Vector3(s, s, s)); 

            CSGNode copy = T_Bake(tm, "scale");
            return copy;
        }

        public CSGNode Scale(float sx, float sy, float sz) {
            //Matrix tm = Matrix.CreateScale(s);
            //pjt Unity
            //sure would be nice to have some unit testing... don't know how long before I hit this method...
            Matrix tm = Matrix.TRS(new Vector3(), Quaternion.Euler(new Vector3(0, 0, 0)), new Vector3(sx, sy, sz)); 
            
            CSGNode copy = T_Bake(tm, "scale");
            return copy;
        }

        public CSGNode T_Bake(Matrix m, string reason) {
            return TNode(new Bakery("t_" + reason).Mat(m));
        }

        static readonly Bakery negbakery = new Bakery("neg", neg: true);

        public CSGNode Neg() {
//            if (csgmode == CSGMode.rendering) 
//                return DynNeg();
            return TNode(negbakery);
        }

        public CSGNode DynNeg() {
            CSGNode rr = Bake(negbakery);
            return rr;
        }

        public abstract CSGNode Expand(float e);

        public virtual string ToStringF(string pre) {
            return pre + ToString() + tttstring() + "<" + Texture + ">" +
            (Provenance == null ? "noprov" : Provenance.ToString());
        }

        /** Find the best (lowest ID) match in expression for a given primitive
         * This can match the primitive or its negative.
         * Used to find the appropriate (?) primitive instance where several instances with different provenance.  */
        public virtual CSGPrim BestMatch(CSGPrim tomatch) {
            return null;
        }

        /** return the volume */
        public abstract Volume Volume();
        // { return new Volume(0,0,0,0,0,0); }

        public const float NaN = float.NaN;

        public static Volume UnknownVolume() {
            return new Volume(NaN, NaN, NaN, NaN, NaN, NaN);
        }

        public static float xmax(float a, float b) {
            return float.IsNaN(a) ? b : float.IsNaN(b) ? a : Mathf.Max(a, b);
        }

        public static float xmin(float a, float b) {
            return float.IsNaN(a) ? b : float.IsNaN(b) ? a : Mathf.Min(a, b);
        }

        public string tttstring() {
#if TTT
            return String.Format("{0,16:X}", ttt).Replace(" ", "0") + "  ";
#else
            return "";
#endif
        }
    }
    // end CSGNode

    public abstract class CSGTrivial : CSGNode {
        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            return this;
        }

        public override CSGNode SimplifyWithFix(CSGPrim node, CSGNode fix) {
            return this;
        }

        public override bool Involves(CSGPrim node) {
            return false;
        }

        public override void Nodes(LinkedList<CSGPrim> nodelist) {
        }

        public override void UNodes(LinkedList<CSGPrim> nodelist, ref int num, int simpguid) {
        }
        //@@ public override abstract ulong TT();

        public override CSGNode Expand(float e) {
            return this;
        }

        public override Volume Volume() {
            return UnknownVolume();
        }

    }
    // end CSGTrivial


    /// <summary>
    /// Class All represent a CSG tree that is full solid
    /// </summary>
    public class All : CSGTrivial {

        public override CSGNode BCopy(Bakery bk) {
            return bk.neg ? (CSGNode)S.NONE : S.ALL; // new All();
        }

        public override float Dist(float x, float y, float z) {
            return float.NegativeInfinity;
        }

        public All() {
#if TTT
            ttt = 0xffffffffffffffff;
#endif
        }
        //@@ public override ulong TT() { return ~((ulong)0); }

    }
    // end All

    /// <summary>
    /// Class None represents a CSG tree that is empty
    /// </summary>
    public class None : CSGTrivial {
        public None() {
#if TTT
            ttt = 0;
#endif
        }

        public override CSGNode BCopy(Bakery bk) {
            return bk.neg ? (CSGNode)S.ALL : S.NONE; // new None();
        }


        public override float Dist(float x, float y, float z) {
            return float.PositiveInfinity;
        }
        //@@ public override ulong TT() { return 0; }
    }
    // end None

    /// <summary>
    /// Class to implement common features of binary ops
    /// </summary>
    public abstract class CSGOP2 : CSGNode {
        public CSGNode l, r;

        public enum OP {
            Union,
            Intersect,
            Xor

        }

        public abstract OP Op { get; }

        public abstract string Opstring { get; }

        public override string ToString() {
            //return string.Format(" [{1} {0}#{3} {2}] ", Opstring, l.ToString (), r.ToString (), IdX);
            return string.Format(" [{0}#{1}] ", Opstring, IdX);
        }


        public static int NOP2 = 0;
        // number of make primitive calls
        public static int NUniqueOP2 = 0;
        // number of unique primitives

        static CSGOP2() {
            ClearCache();
        }

        public override void Nodes(LinkedList<CSGPrim> nodelist) {
            l.Nodes(nodelist);
            r.Nodes(nodelist);
        }

        public override void UNodes(LinkedList<CSGPrim> nodelist, ref int num, int simpguid) {
            l.UNodes(nodelist, ref num, simpguid);
            r.UNodes(nodelist, ref num, simpguid);
        }

        // we only use one of these
        //protected static Dictionary<int, CSGOP2> sharedd;
        //protected static Hashtable sharedh;
//?        [ThreadStatic]
        protected static CSGOP2[] _shareda;

        protected static CSGOP2[] shareda {
            get {
                if (_shareda == null)
                    ClearCache();
                return _shareda;
            }
        }

        protected static int SIZE = 9967 * 17;
        // biggish primish: consider making more dynamic (eg smallish caches for parallel subtasks)
        long mykey;
        // my key

        public static void ClearCache() {
            //sharedd = null; //  new Dictionary<int, CSGOP2>();
            //sharedh = new Hashtable(5000);
            //SIZE = 9967 * 17; //  *19;
            _shareda = new CSGOP2[SIZE];
            NOP2 = 0;  // number of make primitive calls
            NUniqueOP2 = 0; // number of unique primitives
            rid = 0;
            //FirstOfGroup = rid;
        }

        protected static CSGOP2 MakeShared(OP op, CSGNode ll, CSGNode rr) {
            NOP2++;
            if (Op2AlwaysNew) { // TODO: TOCHECK if this was helpful || (ll.nodes + rr.nodes > 5)) {
                switch (op) {
                case OP.Union:
                    return new Union(ll, rr);
                case OP.Intersect:
                    return new Intersect(ll, rr);
                case OP.Xor:
                    return new Xor(ll, rr);
                default:
                    throw new NotImplementedException();
                }
            }

            if (ll.Id > rr.Id) { //##
                CSGNode s = rr;
                rr = ll;
                ll = s;
            }  // canonical order: NOTE assumes symmetric op
            long key = (int)op + (ll.Id << 2) + (((long)rr.Id) << 30);
            CSGOP2 r;
            int pos;
            //if (!sharedd.TryGetValue(key, out r)) {
            //r = (CSGOP2)sharedh[key];
            //key = key % SIZE; r = shareda[key];
            for (pos = (int)(key % SIZE); true; pos += 11) {
                if (pos >= SIZE)
                    pos -= SIZE;
                r = shareda[pos];
                if (r == null)
                    break;
                //if (key == r.mykey && !(r.l == ll && r.r == rr)) {
                //}  // debug TODO remove
                if (key == r.mykey)
                    return r;
            }
            switch (op) {
            case OP.Union:
                r = new Union(ll, rr);
                break;
            case OP.Intersect:
                r = new Intersect(ll, rr);
                break;
            case OP.Xor:
                r = new Xor(ll, rr);
                break;
            default:
                throw new NotImplementedException();
            }
            shareda[pos] = r;
            r.mykey = key;
            NUniqueOP2++;
            if (NUniqueOP2 > SIZE * 0.8) {
                GUIBits.Log("OP2 Cache overfull " + NUniqueOP2 + " of " + SIZE);
                var old = shareda;
                SIZE = SIZE * 2 - 1;
                ClearCache();
                for (int i=0; i<old.Length; i++) {
                    var s = old[i];
                    if (s != null)
                        MakeShared(s.Op, s.l, s.r);
                }
                return MakeShared(op, ll, rr);
            }
            //}
            return r;
        }

        public override bool Involves(CSGPrim node) {
            return l.Involves(node) || r.Involves(node);
        }

        public override IHasProvenance SetProvenance(object prov) {
            l = (CSGNode)l.SetProvenance(prov);
            r = (CSGNode)r.SetProvenance(prov);
            bk.provstring += "/" + prov;
            return this;  // incomplete SetProvenance implementation
        }


        public override string ToStringF(string pre) {
            return pre + this.GetType().Name + "(" + tttstring() + "<" + Texture + ">" +
            (Provenance == null ? "noprov" : Provenance.ToString())
            + l.ToStringF(pre + ".   ") + r.ToStringF(pre + ".   ") + ")";

        }

        /** find matches (if any) on the two sides, anch choose one with lowest Id if conflict */
        public override CSGPrim BestMatch(CSGPrim tomatch) {
            CSGPrim lm, rm;
            lm = l.BestMatch(tomatch);
            rm = r.BestMatch(tomatch);
            if (lm == null)
                return rm;
            else if (rm == null)
                return lm;
            else if (lm.Id < rm.Id)
                return lm;
            else
                return rm;
        }


    }
    // end CSGOP2

    /// <summary>
    /// Class Union represents the Union of two csg trees
    /// </summary>
    public class Union : CSGOP2 {
        internal Union(CSGNode ll, CSGNode rr) {
            l = ll;
            r = rr;
            nodes = l.nodes + r.nodes;
        }

        public override CSGNode BCopy(Bakery bk) {
            return (bk.neg) ? Intersect.Make(l.Bake(bk), r.Bake(bk)) : Make(l.Bake(bk), r.Bake(bk));
        }

        public override string Opstring { get { return "Union"; } }

        public override OP Op { get { return OP.Union; } }

        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            CSGNode sl = l.OSimplify(vol, simpguid, ref nUnodes);
            if (sl == S.ALL)
                return S.ALL;
            CSGNode sr = r.OSimplify(vol, simpguid, ref nUnodes);
            if (sr == S.ALL)
                return S.ALL;
            if (sl == S.NONE)
                return sr;
            if (sr == S.NONE)
                return sl;
            if (sl == sr)
                return sl;
            if (sl.canon == sr.canon && sl is CSGPrim)
                return sl.Id > sr.Id ? sr : sl;
            if (sl == l && sr == r)
                return this;
            if (sl.canonneg != null && sl.canonneg.canon == sr.canon)
                return S.ALL;
            if (sr.canonneg != null && sr.canonneg.canon == sl.canon)
                return S.ALL;  // todo: clean up the way we handle neg
            if (sl.canon == l.canon && sr.canon == r.canon)
                return this;
            return MakeShared(OP.Union, sl, sr);
        }

        public override CSGNode SimplifyWithFix(CSGPrim node, CSGNode fix) {
            CSGNode sl = l.SimplifyWithFix(node, fix);
            if (sl == S.ALL)
                return S.ALL;
            CSGNode sr = r.SimplifyWithFix(node, fix);
            if (sr == S.ALL)
                return S.ALL;
            if (sl == S.NONE)
                return sr;
            if (sr == S.NONE)
                return sl;
            if (sl == sr)
                return sl;
            if (sl.canon == sr.canon && sl is CSGPrim)
                return sl.Id > sr.Id ? sr : sl;
            if (sl.canonneg != null && sl.canonneg.canon == sr.canon)
                return S.ALL;
            if (sr.canonneg != null && sr.canonneg.canon == sl.canon)
                return S.ALL;  // todo: clean up the way we handle neg
            //if (sl == l && sr == r) return this;
            if (sl == l && sr == r) {
#if TTT
                ttt = sl.ttt | sr.ttt;
#endif
                return this;
            }  // << ??? remove canon, and in Xor, Int

            // some special cases that might help
            // TruthTable will do a much better job.  Also new test for csgin == csgout in CSGNode.Split helps more.
            //if (sl is Intersect && (sl as Intersect).l == sr) return sr;
            //if (sl is Intersect && (sl as Intersect).r == sr) return sr;
            //if (sr is Intersect && (sr as Intersect).l == sl) return sl;
            //if (sr is Intersect && (sr as Intersect).r == sl) return sl;
            CSGNode ret = MakeShared(OP.Union, sl, sr);
#if TTT
            ret.ttt = sl.ttt | sr.ttt;
#endif
            return ret;
        }

        public static CSGNode Make(CSGNode sl, CSGNode sr) {
            if (sl == S.ALL)
                return S.ALL;
            if (sr == S.ALL)
                return S.ALL;
            if (sl == S.NONE)
                return sr;
            if (sr == S.NONE)
                return sl;
            if (sl == sr)
                return sl;
            if (sl.canonneg == sr)
                return S.ALL;
            return MakeShared(OP.Union, sl, sr);
        }

        public override float Dist(float x, float y, float z) {
            return Math.Min(l.Dist(x, y, z), r.Dist(x, y, z));
        }


        public override CSGNode Expand(float e) {
            return MyAttrs(Union.Make(l.Expand(e), r.Expand(e)));
        }

        /// <summary>
        /// make list of union nodes in tree
        /// </summary>
        /// <returns></returns>
        void ulist(List<CSGNode> list) {
            if (l is Union)
                (l as Union).ulist(list);
            else
                list.Add(l);
            if (r is Union)
                (r as Union).ulist(list);
            else
                list.Add(r);
        }

        /// <summary>
        /// balance a tree of Union nodes
        /// </summary>
        /// <returns></returns>
        public CSGNode balance() {
            List<CSGNode> list = new List<CSGNode>();
            ulist(list);
            CSGNode[] arr = list.ToArray();
            if (arr.Any())
                return S.NONE;
            return balance(arr, 0, arr.Length);
        }

        /// <summary>
        /// return a balanced node for nodes in range low..highp-1
        /// </summary>
        /// <param name="arr"></param>
        /// <param name="low"></param>
        /// <param name="highp"></param>
        /// <returns></returns>
        private static CSGNode balance(CSGNode[] arr, int low, int highp) {
            if (low == highp - 1)
                return arr[low];
            else {
                int mid = (low + highp) / 2;
                return balance(arr, low, mid) + balance(arr, mid, highp);
            }

        }

        /// <summary>
        /// return a balanced Union node for nodes array
        /// </summary>
        /// <param name="arr"></param>
        /// <returns></returns>
        public static CSGNode balance(IEnumerable<CSGNode> iii) {
            CSGNode[] arr = iii.ToArray();
            if (arr.Any())
                return S.NONE;
            return balance(arr, 0, arr.Length);
        }

        public override Volume Volume() {
            return l.Volume().union(r.Volume());
        }
        // { return new Volume(0,0,0,0,0,0); }
    }
    // end Union



    /// <summary>
    /// Class Xor represents the Xor of two csg trees
    /// </summary>
    public class Xor : CSGOP2 {
        internal Xor(CSGNode ll, CSGNode rr) {
            l = ll;
            r = rr;
            nodes = l.nodes + r.nodes;
        }

        public override string Opstring { get { return "Xor"; } }

        public override OP Op { get { return OP.Xor; } }

        public override CSGNode BCopy(Bakery bk) {
            Bakery bk2 = bk;
            // For bk positive (bk.neg == false) there is no need to change either side, both wides will use bk positive.
            // For bk negative we need to invert just one side ... it doesn't matter which one.
            // Thus we will have bk negative and bk2 positive.
            // So, in either case, bk2 is positive.
            // ~(a ^ b) = ~a ^ b == a ^ ~b
            bk2.neg = false;
            CSGNode rr = Make(l.Bake(bk2), r.Bake(bk));
            return rr;
        }

        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {

            CSGNode sl = l.OSimplify(vol, simpguid, ref nUnodes);
            CSGNode sr = r.OSimplify(vol, simpguid, ref nUnodes);
            if (sl == S.ALL)
                return sr.provneg ?? sr.DynNeg();
            if (sl == S.NONE)
                return sr;
            if (sr == S.ALL)
                return sl.provneg ?? sl.DynNeg(); 
            if (sr == S.NONE)
                return sl;
            if (sl.canon == sr.canon)
                return S.NONE;
            if (sl == l && sr == r)
                return this;
            if (sl.canonneg != null && sl.canonneg.canon == sr.canon)
                return S.ALL;
            if (sr.canonneg != null && sr.canonneg.canon == sl.canon)
                return S.ALL;
            //if (sl.canon == l.canon && sr.canon == r.canon) return this;
            return MakeShared(OP.Xor, sl, sr);
        }

        public override CSGNode SimplifyWithFix(CSGPrim node, CSGNode fix) {
            CSGNode sl = l.SimplifyWithFix(node, fix);
            CSGNode sr = r.SimplifyWithFix(node, fix);
            if (sl == S.ALL) {
                CSGNode rr = sr.provneg ?? sr.DynNeg();
                return rr;
            }
            if (sl == S.NONE)
                return sr;
            if (sr == S.ALL) {
                CSGNode rr = sl.provneg ?? sl.DynNeg();
                return rr;
            }
            if (sr == S.NONE)
                return sl;
            if (sl.canon == sr.canon)
                return S.NONE;
            if (sl.canonneg != null && sl.canonneg.canon == sr.canon)
                return S.ALL;
            if (sr.canonneg != null && sr.canonneg.canon == sl.canon)
                return S.ALL;

            //if (ttt == 0) return S.NONE;  // TODO 
            //if (ttt == -1) return S.ALL;
            if (sl == l && sr == r) {
#if TTT
                ttt = sl.ttt ^ sr.ttt;
#endif
                return this;
            }

            // some special cases that might help
            // TruthTable will do a much better job.  Also new test for csgin == csgout in CSGNode.Split helps more.
            //if (sl.canon is Intersect && (sl.canon as Intersect).l.canon == sr.canon) return sr.canon;
            //if (sl.canon is Intersect && (sl.canon as Intersect).r.canon == sr.canon) return sr.canon;
            //if (sr.canon is Intersect && (sr.canon as Intersect).l.canon == sl.canon) return sl.canon;
            //if (sr.canon is Intersect && (sr.canon as Intersect).r.canon == sl.canon) return sl.canon;
            CSGNode ret = MakeShared(OP.Xor, sl, sr);
#if TTT
            ret.ttt = sl.ttt ^ sr.ttt;
#endif
            return ret;
        }

        public static CSGNode Make(CSGNode sl, CSGNode sr) {
            if (sl == S.ALL)
                return sr.provneg ?? sr.Neg();
            if (sr == S.ALL)
                return sl.provneg ?? sl.Neg();
            if (sl == S.NONE)
                return sr;
            if (sr == S.NONE)
                return sl;
            if (sl == sr)
                return S.NONE;
            if (sl.canonneg == sr.canon)
                return S.ALL;
            if (sr.canonneg == sl.canon)
                return S.ALL;
            return MakeShared(OP.Xor, sl, sr);
        }

        public override float Dist(float x, float y, float z) {
            return Math.Max(l.Dist(x, y, z), r.Dist(x, y, z));
        }


        public override CSGNode Expand(float e) {
            return MyAttrs(Xor.Make(l.Expand(e), r.Expand(e)));
        }

        public override Volume Volume() {
            return l.Volume().union(r.Volume());
        }
        // { return new Volume(0,0,0,0,0,0); }


    }
    // end Xor


    /// <summary>
    /// Class Intersect represents the Intersection of two CSG trees
    /// </summary>
    public class Intersect : CSGOP2 {
        internal Intersect(CSGNode ll, CSGNode rr) {
            l = ll;
            r = rr;
            nodes = l.nodes + r.nodes;
        }

        public override string Opstring { get { return "Intersect"; } }

        public override OP Op { get { return OP.Intersect; } }

        public override CSGNode BCopy(Bakery bk) {
            return bk.neg ? Union.Make(l.Bake(bk), r.Bake(bk)) : Make(l.Bake(bk), r.Bake(bk));
        }

        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            CSGNode sl = l.OSimplify(vol, simpguid, ref nUnodes);
            if (sl == S.NONE)
                return S.NONE;
            CSGNode sr = r.OSimplify(vol, simpguid, ref nUnodes);
            if (sr == S.NONE)
                return S.NONE;
            if (sl == S.ALL)
                return sr;
            if (sr == S.ALL)
                return sl;
            if (sl == sr)
                return sl;
            if (sl.canon == sr.canon && sl is CSGPrim)
                return sl.Id > sr.Id ? sr : sl;
            if (sl == l && sr == r)
                return this;
            //if (sl.canon == l.canon && sr.canon == r.canon) return this;
            if (sl.canonneg != null && sl.canonneg.canon == sr.canon)
                return S.NONE;
            if (sr.canonneg != null && sr.canonneg.canon == sl.canon)
                return S.NONE;
            return MakeShared(OP.Intersect, sl, sr);
        }


        public override CSGNode SimplifyWithFix(CSGPrim node, CSGNode fix) {
            CSGNode sl = l.SimplifyWithFix(node, fix);
            if (sl == S.NONE)
                return S.NONE;
            CSGNode sr = r.SimplifyWithFix(node, fix);
            if (sr == S.NONE)
                return S.NONE;
            if (sl == S.ALL)
                return sr;
            if (sr == S.ALL)
                return sl;
            if (sl.canon == sr.canon && sl is CSGPrim)
                return sl.Id > sr.Id ? sr : sl;
            if (sl == sr)
                return sl;
            if (sl == l && sr == r) {
#if TTT
                ttt = sl.ttt & sr.ttt;
#endif
                return this;
            }
            if (sl.canonneg != null && sl.canonneg.canon == sr.canon)
                return S.NONE;
            if (sr.canonneg != null && sr.canonneg.canon == sl.canon)
                return S.NONE;
            CSGNode ret = MakeShared(OP.Intersect, sl, sr);
#if TTT
            ret.ttt = sl.ttt & sr.ttt;
#endif
            return ret;
        }

        public static CSGNode Make(CSGNode sl, CSGNode sr) {
            if (sl == S.NONE)
                return S.NONE;
            if (sr == S.NONE)
                return S.NONE;
            if (sl == S.ALL)
                return sr;
            if (sr == S.ALL)
                return sl;
            if (sl == sr)
                return sl;
            if (sl.canonneg == sr)
                return S.NONE;
            return MakeShared(OP.Intersect, sl, sr);
        }

        public override float Dist(float x, float y, float z) {
            return Math.Max(l.Dist(x, y, z), r.Dist(x, y, z));
        }



        public override CSGNode Expand(float e) {
            return MyAttrs(Intersect.Make(l.Expand(e), r.Expand(e)));
        }

        public override Volume Volume() {
            return l.Volume().intersect(r.Volume());
        }
        // { return new Volume(0,0,0,0,0,0); }


    }
    // end Intersect

    public class Difference {
        public static CSGNode Make(CSGNode sl, CSGNode sr) {
            return Intersect.Make(sl, sr.Neg());
        }
    }


    /// <summary>
    /// Truth table constants.
    /// For up to 6 primitives we can view the exact expression as a truth table.
    ///
    /// For more than 6 primitives, we can sometimes cheat and get a reasonable answer.
    /// That is mainly for debig testing to give an idea of how effective
    /// an extended TT implementation might be.
    /// </summary>
    public static class CSGTT {
        public static ulong[] TTI = { 0x5555555555555555,
            0x3333333333333333,
            0x0f0f0f0f0f0f0f0f,
            0x00ff00ff00ff00ff,
            0x0000ffff0000ffff,
            0x00000000ffffffff,

            // cheat ones below
            0x95ac7538c53ca5a5,
            0xc7538c53ca5a5a59,
            0x38c53ca5a595ac75,
            0xa5a595ac7538c53c,
            0x5939965939435949,
            0x2356907476582047,
            0x6530345876542652,
            0x5428764592365356,
            0x6545194526794513,
        };
        public const ulong TTIX = 0x8a4572de8463bc90;
    }

    /// <summary>
    /// CSG primitive
    /// </summary>
    public abstract partial class CSGPrim : CSGNode {
        public static int NPrims = 0;
        // number of make primitive calls
        public static int NUniquePrims = 0;
        // number of unique primitives
        public override abstract CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes);

        public override abstract float Dist(float x, float y, float z);

        public CSGPrim realCSG = null;  // may be overridden for faceted cylinders etc, used for normals
        public int realCSGid;           // number within planes representing realCSG, for facet optimization
        public int realCSGnum;          // total number of lanes representing realCSG

        public float Dist(Vector3 v) {
            return Dist(v.x, v.y, v.z);
        }
        // convenience function
        public override abstract void Nodes(LinkedList<CSGPrim> nodelist);

        public override abstract void UNodes(LinkedList<CSGPrim> nodelist, ref int num, int simpguid);

        public abstract Vector3 Normal(Vector3 pos);
        // if not planes, this needs to depend on pos
        public virtual Vector3 UDir(Vector3 pos) {
            throw new NotImplementedException();
        }
        // if not planes, this needs to depend on pos
        public virtual Vector3 VDir(Vector3 pos) {
            throw new NotImplementedException();
        }
        // if not planes, this needs to depend on pos
        internal int arrayPos = -999;
        // my position withing the node array during local operations


        public override CSGNode SimplifyWithFix(CSGPrim node, CSGNode fix) {
#if TTT
            ttt = canon.ttt;
            System.Diagnostics.Debug.Assert(ttt != 0);  // debug :: PJT Unity, type ambiguity
#endif
            if (canon == node)
                return fix;
            if (canonneg == node) {
                System.Diagnostics.Debug.Assert(fix.canonneg != null);
                return fix.canonneg;
            }
            //if (node.ttneg != null && ttcanon == node.ttneg) return fix.neg;  // TODO, this should not be necsessary, same as above?
            //ttt = ttcanon.ttt;
            return this;
        }

        /// <summary>
        /// Compute the polygon for (a linear approximation to) this primitive in given volume.
        /// This is the full poly for this primitive in this volume,
        /// BEFORE being broken down to a renderable bits dependent on the CSG expression.
        /// </summary>
        /// <param name="vol"></param>
        /// <returns></returns>
        public abstract Poly PolyForVol(Volume vol);

        public abstract Vector2 TextureCoordinate(Vector3 pos);


        public override abstract CSGNode Expand(float e);

        /// <summary>
        /// quick check for parallel or anitparallel, may return false negative
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public virtual bool QParallel(CSGPrim p) {
            return false;
        }

        public override CSGPrim BestMatch(CSGPrim tomatch) {
            if (this.canon == tomatch.canon || this.canonneg == tomatch.canon)
                return this;
            return null;
        }

        public virtual void normalColor(Vector3 p, out Vector3 normal, out Color color) {
            normal = Normal(p);
            color = Color;
        }

        /** work out values prior to curvature */
        public virtual void dd(Vector3 p, out float dxx, out float dxy, out float dxz, out float dyy, out float dyz, out float dzz, out Vector3 grad) {
            throw new NotImplementedException ("dd not implemented for ...");
        }

    }


    public abstract class CSGRPrim : CSGPrim {

        public override void Nodes(LinkedList<CSGPrim> nodelist) {
            nodelist.AddFirst(this);
        }

        /// <summary>
        /// find the set of unique nodes (counting a node and its provneg as one)
        /// </summary>
        /// <param name="nodelist"></param>
        /// <param name="num"></param>
        /// <param name="simpguid"></param>
        public override void UNodes(LinkedList<CSGPrim> nodelist, ref int num, int simpguid) {
            if (canon.lastguid == simpguid) {   // already set for this simpguid, not new
#if TTT
                ttt = canon.ttt;                // bring my ttt up to date (may already be), don't bother with my lastguid, rely on canon.lastguid
                System.Diagnostics.Debug.Assert(ttt != 0);         // check things ok
#endif
                return;                             
            }

            // have to generate new entry and assign new truth table value, record here and in canon
            nodelist.AddLast(canon as CSGPrim);
#if TTT
            ttt = (num < CSGTT.TTI.Length) ? CSGTT.TTI[num] : CSGTT.TTIX >> num;  // first TTNUM good, remainder test or rubbish
            canon.ttt = ttt;
#endif
            canon.lastguid = simpguid;

            //// use negative ttt for neg.
            if (canonneg != null) {
                System.Diagnostics.Debug.Assert(canonneg.lastguid != simpguid);  // canon and canonneg should always be set together
                canonneg.lastguid = simpguid;
#if TTT
                canonneg.ttt = ~canon.ttt;
#endif
            }
            num++;  // keep track of depth so far (? cheaper than using nodelist.Count())
            System.Diagnostics.Debug.Assert(num == nodelist.Count());
        }

        /// <summary>
        /// find the set of unique nodes (counting a node and its provneg as one)
        /// </summary>
        /// <param name="nodelist"></param>
        /// <param name="num"></param>
        /// <param name="simpguid"></param>
        private void UNodesOLD(LinkedList<CSGPrim> nodelist, ref int num, int simpguid) {
            if (lastguid == simpguid)
                return;
            if (provneg != null && provneg.lastguid == simpguid)
                return;
            lastguid = simpguid;
            nodelist.AddFirst(this);
#if TTT
            ttt = (num < CSGTT.TTI.Length) ? CSGTT.TTI[num] : CSGTT.TTIX;
#endif

            //// use negative ttt for neg.
            if (provneg != null) {
                provneg.lastguid = simpguid;
#if TTT
                provneg.ttt = ~ttt;
#endif
            }
            //arrayPos = num;  // nb, this will actually be backwards becuase of AddFirst, but is not used
            num++;
        }

        /// <summary>
        /// PolyForVol returns a polygon to represent this primitive in the given volume.
        /// Default implementation is PolyForVolField.
        /// Alternative (dead) implementation is to make a Bigpoly() and then clip it.
        /// </summary>
        /// <param name="vol"></param>
        /// <returns></returns>
        public override Poly PolyForVol(Volume vol) {
            return PolyForVolField(vol);  // compute using field style
        }

        public override bool Involves(CSGPrim node) {
            return canon == node || canonneg == node;
        }
    }



    /// <summary>
    /// Class Plane represents a semi-infinite half plane.
    /// </summary>
    public class CSGPlane : CSGRPrim {
        internal float a, b, c, d;
        private Vector3 normal;
        // fixed for planes

        public override CSGNode BCopy(Bakery bk) {
            Vector4 v1 = new Vector4(a, b, c, d);
            Vector4 v = bk.invM * v1;
            return Make(v.x, v.y, v.z, v.w, Bakery.Merge(this.bk, bk));
        }

        public override string ToString() {
            return string.Format("Plane#{4}({0},{1},{2},{3})", a, b, c, d, IdX);
        }

        public Vector4 UVec, VVec;


        protected CSGPlane(float aa, float bb, float cc, float dd) {
            float s = (float)(1 / Math.Sqrt(aa * aa + bb * bb + cc * cc));
            a = s * aa;
            b = s * bb;
            c = s * cc;
            d = s * dd;
            Setup();
        }

        public static CSGPlane Make(Vector3 dir, float dd, Bakery bk) {
            return Make(dir.x, dir.y, dir.z, dd, bk);
        }

        public static CSGPlane Make(Vector3 dir, Vector3 point, Bakery bk) {
            return Make(dir, -Vector3.Dot(dir, point), bk);
        }


        /// <summary> check the winding of the given poly for this plane, and the csg correctness
        /// Mainly used for debug, but also made to correct the winding of FieldPolyForVol.
        /// </summary>
        public bool CheckPolyWind(ICSGPoly poly) {
            int N = poly.Count;
            double ld = 0;
            for (int i = 0; i < poly.Count; i++) {
                Vector3 p0 = poly[i].point;
                Vector3 p1 = poly[(i + 1) % N].point;
                Vector3 p2 = poly[(i + 2) % N].point;
                Vector3 ab = p1 - p0;
                Vector3 bc = p2 - p1;
                Vector3 x = Vector3.Cross(ab, bc);
                double d = Vector3.Dot(x, normal);
                if (ld * d < 0) {
                }
                ld = d;

                if (!Small(Dist(p0)))
                    throw new Exception("poly point not on poly csg");
                if (poly[i].othercsg != null && !Small(poly[i].othercsg.Dist(p0)))
                    Console.WriteLine("poly othercsg next not on csg i=" + i);
                int ii = i == 0 ? N - 1 : i - 1;
                if (poly[ii].othercsg != null && !Small(poly[ii].othercsg.Dist(p0)))
                    Console.WriteLine("poly othercsg prev not on csg ii=" + ii + " i=" + i);
            }
            return ld > 0;
        }


        public override CSGNode Expand(float e) {
            return MyAttrs(CSGPlane.Make(a, b, c, d - e, bk));
        }

        public static float texturescale = 0.5f;
        // no attempt to align yet, and scale 2 matches noncsg renderer
        void Setup() {
            normal = new Vector3(a, b, c);
            nodes = 1;
            // unless the plane is nearly flat, align VVec as best we can with the vertical
            // otherwise align it with X
            //
            // TODO: consider letting the texture follow the plane through rotations etc.
            Vector3 up = (Math.Abs(b) < 0.99) ? new Vector3(0, 1, 0) : new Vector3(1, 0, 0);
            Vector3 UVec3 = Vector3.Cross(up, normal).Normal();
            Vector3 VVec3 = Vector3.Cross(normal, UVec3);

            //pjt Unity
            //UVec = new Vector4(UVec3 * texturescale, 0);  // no attempt to align yet, and scale 2 matches noncsg renderer
            //VVec = new Vector4(VVec3 * texturescale, 0);
            UVec3 *= texturescale;
            VVec3 *= texturescale;
            UVec = new Vector4(UVec3.x, UVec3.y, UVec3.z, 0);
            VVec = new Vector4(VVec3.x, VVec3.y, VVec3.z, 0);

            bk = new Bakery("plane");
        }


        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            nUnodes++;
            float dist = a * vol.x + b * vol.y + c * vol.z + d;
            if (dist * dist < vol.rsq)
                return this;
            if (dist > 0)
                return S.NONE;
            return S.ALL;
        }

        public override float Dist(float x, float y, float z) {
            return a * x + b * y + c * z + d;
        }



        public override Vector2 TextureCoordinate(Vector3 pos) {
            float y = (VVec.y > 0.01) ? texturescale * texturescale * pos.y / VVec.y :
                pos.x * VVec.x + pos.y * VVec.y + pos.z * VVec.z + VVec.w;

            var r = new Vector2(pos.x * UVec.x + pos.y * UVec.y + pos.z * UVec.z + UVec.w, y);
            return r;
        }

        public override Vector3 Normal(Vector3 pos) {
            // this provenance is only reliable if it has tracked all transforms: todo
            if (realCSG != null) {
                Vector3 n = realCSG.Normal(pos);
                if (n.sqrMagnitude > 0.001f)  // apex of cone can't get a real normal
                    return n;
            }
            return normal;
        }

        public override Vector3 UDir(Vector3 pos) {
            return new Vector3(UVec.x, UVec.y, UVec.z);
        }

        public override Vector3 VDir(Vector3 pos) {
            return new Vector3(VVec.x, VVec.y, VVec.z);
            ;
        }



        // cache of planes
        static int SHAREDSIZE = 1024;
        // must be power 2
        //static int FirstOfGroup;
 //?       [ThreadStatic]
        static List<CSGPlane>[] _shared = new List<CSGPlane>[SHAREDSIZE];

        static List<CSGPlane>[] shared {
            get {
                if (null == _shared)
                    ClearCache();
                return _shared;
            }
        }

        public static CSGPlane Find(int id) {
            for (int s = 0; s < SHAREDSIZE; s++)
                foreach (CSGPlane p in shared[s])
                    if (p.Id == id)
                        return p;
            return null;
        }

        public static void ClearCache() {
            _shared = new List<CSGPlane>[SHAREDSIZE];
            for (int s = 0; s < SHAREDSIZE; s++)
                shared[s] = new List<CSGPlane>();
            NPrims = 0;  // number of make primitive calls
            NUniquePrims = 0; // number of unique primitives
            //FirstOfGroup = rid;
        }

        static CSGPlane() {
            ClearCache();
        }

        /// <summary>
        /// Make manages the cache, and negative planes
        /// </summary>
        /// <param name="aa"></param>
        /// <param name="bb"></param>
        /// <param name="cc"></param>
        /// <param name="dd"></param>
        /// <returns></returns>
        public static CSGPlane XMake(float aa, float bb, float cc, float dd) {
            return Make(aa, bb, cc, dd, S.DefaultBakery);
        }

        /// <summary>
        /// Make manages the cache, and negative planes
        /// </summary>
        /// <param name="aa"></param>
        /// <param name="bb"></param>
        /// <param name="cc"></param>
        /// <param name="dd"></param>
        /// <param name="provenance"></param>
        /// <returns></returns>
        public static CSGPlane Make(float aa, float bb, float cc, float dd, Bakery bk) {
            NPrims++;

            float a, b, c, d;
            float sc = (float)(1 / Math.Sqrt(aa * aa + bb * bb + cc * cc));
            if (bk.neg) {
                sc *= -1;
                bk.neg = false;    // just to keep the record straight, probably not needed
            }
            CSGNode canon = null;  // to hold canon if known
            a = sc * aa;
            b = sc * bb;
            c = sc * cc;
            d = sc * dd;
            if (Small(a))
                a = 0;
            if (Small(b))
                b = 0;
            if (Small(c))
                c = 0;
            if (Small(d))
                d = 0;

            float hashd = a * 3.123f + b * 7.543f + c * 11.183f + d * 13.537f;
            int hash = ((int)(hashd * 100));
            // if (hash < 0) hash = -hash + 79;  // becuase % is a silly operator
            //
            // todo: sjpt 4 June 2009 fix issues with identical walls with different provenance
            // they need to be different objects, but related.
            // Main issue probably comes in the final draw, eg of X union Y
            for (int s = (hash - 1); s <= (hash + 1); s++) {
                List<CSGPlane> pl = shared[s & (SHAREDSIZE - 1)];
                if (pl.Count() > 4) {
                }
                foreach (CSGPlane p in pl) {
                    if (p.Close(a, b, c, d)) {
                        //if (bk.provenance == p.Provenance || (bk.provenance != null && bk.provenance.Equals(p.Provenance))) {
                        if (bk.provstring == p.bk.provstring) {
                            return p;  // « todo; remember geom equality for mixed provenance
                        } else {  // close but different provenance
                            if (canon != null && canon != p.canon) {
                                Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>> canon error" + p);
                            }
                            canon = p.canon;
                        }
                    }
                }
            }

            NUniquePrims++;
            CSGPlane r;
            if (a == 0 && b == 0 && c > 0)
                r = new PlaneZN(-d / c);
            else if (a == 0 && b == 0 && c < 0)
                r = new PlaneZP(-d / c);
            else if (a == 0 && c == 0 && b > 0)
                r = new PlaneYN(-d / b);
            else if (a == 0 && c == 0 && b < 0)
                r = new PlaneYP(-d / b);
            else if (b == 0 && c == 0 && a > 0)
                r = new PlaneXN(-d / a);
            else if (b == 0 && c == 0 && a < 0)
                r = new PlaneXP(-d / a);
            else
                r = new CSGPlane(a, b, c, d);
            if (b * d == -10) {
            }
            r.bk = bk;
//            IColorable icc = provenance as IColorable;
//            if (icc != null)
//                r._texture = icc.Texture;

            if (canon != null) {
            }
            r.canon = (canon == null) ? r : canon;  // set the canon for the new plane

            if (r.Provenance == null) {
            }
            shared[hash & (SHAREDSIZE - 1)].Add(r);

            // now check for -ve
            int neghash = ((int)(-hashd * 100));
            // if (hash < 0) hash = -hash + 79;  // becuase % is a silly operator
            // todo: only need to compare two when they are both canonical ??
            for (int s = (neghash - 1); s <= (neghash + 1); s++) {
                List<CSGPlane> pl = shared[s & (SHAREDSIZE - 1)];
                if (pl.Count() > 2) {
                }
                foreach (CSGPlane p in pl) {
                    if (p.Close(-a, -b, -c, -d)) {
                        if ((p.canonneg != null && p.canonneg != r.canon) || (r.canonneg != null && r.canonneg != p.canon)) {
                            Console.WriteLine(">>>>>>>>>>>>>>>>>>>>>>>>> neg error" + p + " " + r);
                        }
                        p.canonneg = r.canon;
                        r.canonneg = p.canon;
                        //if (provenance == p.Provenance || (provenance != null && provenance.Equals(p.Provenance))) { // || CSGControl.UseTTCanon) {
                        if (p.bk.provstring == bk.provstring) {
                            p.provneg = r;
                            r.provneg = p;
                        }
                    }
                }
            }
            if (b * d == -10) {
            }
            return r;
        }


        // plane at angle throught point
        public static CSGPlane MakeNormalPoint(float aa, float bb, float cc, float x, float y, float z, Bakery bk) {
            return Make(aa, bb, cc, -(aa * x + bb * y + cc * z), bk);
        }

        public static CSGPlane MakeNormalPoint(Vector3 n, Vector3 p, Bakery bk) {
            return MakeNormalPoint(n.x, n.y, n.z, p.x, p.y, p.z, bk);
        }

        /// <summary>
        /// check for plane close to this plane
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Close(CSGPlane p) {
            return (Small(a - p.a) && Small(b - p.b) &&
            Small(c - p.c) && Small(d - p.d));
        }

        /// <summary>
        /// check for plane paramters close to this plane
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public bool Close(float pa, float pb, float pc, float pd) {
            return (Small(a - pa) && Small(b - pb) &&
            Small(c - pc) && Small(d - pd));
        }

        /// <summary>
        /// quick check for parallel or antiparallel, may return false negative
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public override bool QParallel(CSGPrim pr) {
            CSGPlane p = pr as CSGPlane;
            if (p == null)
                return false;
            return (a == p.a && b == p.b && c == p.c) || (a == -p.a && b == -p.b && c == -p.c);
        }

        public override Volume Volume() {
            return UnknownVolume();
        }
        // { return new Volume(0,0,0,0,0,0); }


    }
    // end CSGPlane

    /// <summary>
    ///  Class PlaneXP represents a semi-infinite half plane, x>d
    /// </summary>
    public class PlaneXP : CSGPlane {
        internal PlaneXP(float d)
            : base(-1, 0, 0, d) {
        }

        public static CSGPlane Make(float d, Bakery bk) {
            return CSGPlane.Make(-1, 0, 0, d, bk);
        }

        public override string ToString() {
            return string.Format("PlaneXP#{1}({0})", d, IdX);
        }

        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            nUnodes++;
            if (vol.x1 > d)
                return S.ALL;
            if (vol.x2 < d)
                return S.NONE;
            return this;
        }

        public override int Draw(ICSGOutput pl, Volume vol) {
            pl.AddLoop(this,
                new Vector3(d, vol.y1, vol.z1),
                new Vector3(d, vol.y1, vol.z2),
                new Vector3(d, vol.y2, vol.z2),
                new Vector3(d, vol.y2, vol.z1));
            return 1;
        }

        public override float Dist(float x, float y, float z) {
            return -x + d;
        }

        public override Poly PolyForVol(Volume vol) {
            Poly poly = Poly.Make();
            poly.Add(new Vector3(d, vol.y1, vol.z1));
            poly.Add(new Vector3(d, vol.y1, vol.z2));
            poly.Add(new Vector3(d, vol.y2, vol.z2));
            poly.Add(new Vector3(d, vol.y2, vol.z1));
            ///// poly.Add(new Vector3(d, vol.y1, vol.z1));
            return poly;
        }

        public static CSGPlane XMake(float d) {
            return Make(d, S.DefaultBakery);
        }

        public override Volume Volume() {
            Volume v = UnknownVolume();
            v.x1 = d;
            return v;
        }
        // { return new Volume(0,0,0,0,0,0); }

    }
    // end PlaneXP

    /// <summary>
    ///  Class PlaneXN represents a semi-infinite half plane, x<d
    /// </summary>
    public class PlaneXN : CSGPlane {
        internal PlaneXN(float d)
            : base(1, 0, 0, -d) {
        }

        public static CSGPlane Make(float d, Bakery bk) {
            return CSGPlane.Make(1, 0, 0, -d, bk);
        }

        public override string ToString() {
            return string.Format("PlaneXN#{1}({0})", -d, IdX);
        }

        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            nUnodes++;
            if (vol.x1 > -d)
                return S.NONE;
            if (vol.x2 < -d)
                return S.ALL;
            return this;
        }

        public override int Draw(ICSGOutput pl, Volume vol) {
            pl.AddLoop(this,
                new Vector3(-d, vol.y1, vol.z1),
                new Vector3(-d, vol.y2, vol.z1),
                new Vector3(-d, vol.y2, vol.z2),
                new Vector3(-d, vol.y1, vol.z2));
            return 1;
        }

        public override float Dist(float x, float y, float z) {
            return x + d;
        }

        public override Poly PolyForVol(Volume vol) {
            Poly poly = Poly.Make();
            float k = 0.00f;
            poly.Add(new Vector3(-d, vol.y1 + k, vol.z1 + k));
            poly.Add(new Vector3(-d, vol.y2 + k, vol.z1 - k));
            poly.Add(new Vector3(-d, vol.y2 - k, vol.z2 - k));
            poly.Add(new Vector3(-d, vol.y1 - k, vol.z2 + k));

            return poly;
        }

        public static CSGPlane XMake(float d) {
            return Make(d, S.DefaultBakery);
        }

        public override Volume Volume() {
            Volume v = UnknownVolume();
            v.x2 = -d;
            return v;
        }
        // { return new Volume(0,0,0,0,0,0); }

    }
    // end Planexn

    /// <summary>
    ///  Class PlaneYP represents a semi-infinite half plane, y>d
    /// </summary>
    public class PlaneYP : CSGPlane {
        internal PlaneYP(float d)
            : base(0, -1, 0, d) {
        }

        public static CSGPlane Make(float d, Bakery bk) {
            return CSGPlane.Make(0, -1, 0, d, bk);
        }

        public override string ToString() {
            return string.Format("PlaneYP#{1}({0})", d, IdX);
        }

        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            nUnodes++;
            if (vol.y1 > d)
                return S.ALL;
            if (vol.y2 < d)
                return S.NONE;
            return this;
        }

        public override int Draw(ICSGOutput pl, Volume vol) {
            pl.AddLoop(this,
                new Vector3(vol.x1, d, vol.z1),
                new Vector3(vol.x2, d, vol.z1),
                new Vector3(vol.x2, d, vol.z2),
                new Vector3(vol.x1, d, vol.z2));
            return 1;
        }


        public override float Dist(float x, float y, float z) {
            return -y + d;
        }

        public override Poly PolyForVol(Volume vol) {
            Poly poly = Poly.Make();
            poly.Add(new Vector3(vol.x1, d, vol.z1));
            poly.Add(new Vector3(vol.x2, d, vol.z1));
            poly.Add(new Vector3(vol.x2, d, vol.z2));
            poly.Add(new Vector3(vol.x1, d, vol.z2));
            return poly;
        }

        public static CSGPlane XMake(float d) {
            return Make(d, S.DefaultBakery);
        }

        public override Volume Volume() {
            Volume v = UnknownVolume();
            v.y1 = d;
            return v;
        }
        // { return new Volume(0,0,0,0,0,0); }


    }
    // end Planeyp

    /// <summary>
    ///  Class PlaneYN represents a semi-infinite half plane, y<d
    /// </summary>
    public class PlaneYN : CSGPlane {
        internal PlaneYN(float d)
            : base(0, 1, 0, -d) {
        }

        public static CSGPlane Make(float d, Bakery bk) {
            return CSGPlane.Make(0, 1, 0, -d, bk);
        }

        public override string ToString() {
            return string.Format("PlaneYN#{1}({0})", -d, IdX);
        }

        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            nUnodes++;
            if (vol.y1 > -d)
                return S.NONE;
            if (vol.y2 < -d)
                return S.ALL;
            return this;
        }

        public override int Draw(ICSGOutput pl, Volume vol) {
            pl.AddLoop(this,
                new Vector3(vol.x1, -d, vol.z1),
                new Vector3(vol.x1, -d, vol.z2),
                new Vector3(vol.x2, -d, vol.z2),
                new Vector3(vol.x2, -d, vol.z1));
            return 1;
        }

        public override float Dist(float x, float y, float z) {
            return y + d;
        }

        public override Poly PolyForVol(Volume vol) {
            Poly poly = Poly.Make();
            poly.Add(new Vector3(vol.x1, -d, vol.z1));
            poly.Add(new Vector3(vol.x1, -d, vol.z2));
            poly.Add(new Vector3(vol.x2, -d, vol.z2));
            poly.Add(new Vector3(vol.x2, -d, vol.z1));
            return poly;
        }

        public static CSGPlane XMake(float d) {
            return Make(d, S.DefaultBakery);
        }

        public override Volume Volume() {
            Volume v = UnknownVolume();
            v.y2 = -d;
            return v;
        }
        // { return new Volume(0,0,0,0,0,0); }


    }
    // end PlaneYN

    /// <summary>
    ///  Class PlaneZP represents a semi-infinite half plane, z>d
    /// </summary>
    public class PlaneZP : CSGPlane {
        internal PlaneZP(float d)
            : base(0, 0, -1, d) {
        }

        public static CSGPlane Make(float d, Bakery bk) {
            return CSGPlane.Make(0, 0, -1, d, bk);
        }

        public override string ToString() {
            return string.Format("PlaneZP#{1}({0})", d, IdX);
        }

        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            nUnodes++;
            if (vol.z1 > d)
                return S.ALL;
            if (vol.z2 < d)
                return S.NONE;
            return this;
        }

        public override int Draw(ICSGOutput pl, Volume vol) {
            pl.AddLoop(this,
                new Vector3(vol.x1, vol.y1, d),
                new Vector3(vol.x1, vol.y2, d),
                new Vector3(vol.x2, vol.y2, d),
                new Vector3(vol.x2, vol.y1, d));
            return 1;
        }

        public override float Dist(float x, float y, float z) {
            return -z + d;
        }

        public override Poly PolyForVol(Volume vol) {
            Poly poly = Poly.Make();
            poly.Add(new Vector3(vol.x1, vol.y1, d));
            poly.Add(new Vector3(vol.x1, vol.y2, d));
            poly.Add(new Vector3(vol.x2, vol.y2, d));
            poly.Add(new Vector3(vol.x2, vol.y1, d));
            return poly;
        }

        public static CSGPlane XMake(float d) {
            return Make(d, S.DefaultBakery);
        }

        public override Volume Volume() {
            Volume v = UnknownVolume();
            v.z1 = d;
            return v;
        }

    }
    // end PlaneZP


    /// <summary>
    ///  Class PlaneZN represents a semi-infinite half plane, z<d
    /// </summary>
    public class PlaneZN : CSGPlane {
        internal PlaneZN(float d)
            : base(0, 0, 1, -d) {
        }

        public static CSGPlane Make(float d, Bakery bk) {
            return CSGPlane.Make(0, 0, 1, -d, bk);
        }

        public override string ToString() {
            return string.Format("PlaneZN#{1}({0})", -d, IdX);
        }

        public override CSGNode Simplify(Volume vol, int simpguid, ref int nUnodes) {
            if (vol.z1 > -d)
                return S.NONE;
            if (vol.z2 < -d)
                return S.ALL;
            return this;
        }

        public override int Draw(ICSGOutput pl, Volume vol) {
            pl.AddLoop(this,
                new Vector3(vol.x1, vol.y1, -d),
                new Vector3(vol.x2, vol.y1, -d),
                new Vector3(vol.x2, vol.y2, -d),
                new Vector3(vol.x1, vol.y2, -d));
            return 1;
        }

        public override float Dist(float x, float y, float z) {
            return z + d;
        }

        public override Poly PolyForVol(Volume vol) {
            Poly poly = Poly.Make();
            poly.Add(new Vector3(vol.x1, vol.y1, -d));
            poly.Add(new Vector3(vol.x2, vol.y1, -d));
            poly.Add(new Vector3(vol.x2, vol.y2, -d));
            poly.Add(new Vector3(vol.x1, vol.y2, -d));
            ///// poly.Add(new Vector3(vol.x1, vol.y1, -d));
            return poly;
        }

        public static CSGPlane XMake(float d) {
            return Make(d, S.DefaultBakery);
        }

        public override Volume Volume() {
            Volume v = UnknownVolume();
            v.z2 = -d;
            return v;
        }

    }
    // end PlaneZN

    /// <summary>
    /// Class Subdivide implements the main subdivision operation
    /// </summary>
    class Subdivide {
        public static float done;
        public Subdivide(ICSGOutput output) {
            done = 0;
            csgoutput = output;
        }

        //if (CSGControl.stats) { # if STATS
            /// <summary>max number of prims at given level to keep stats for</summary>
            private const int PrimLimit = 500;
            int[] hits = new int[PrimLimit];  // number of hits for the given number of primitives
            int[] uhits = new int[PrimLimit];  // number of unique primitives //note was 500; problem cases still overflow with 1024 so set back to 500 for now
        //}
        private ICSGOutput csgoutput;

        // DoDraw gives the number of primitives to choose between subdivision and draw/split at each recursion level
        // set at !!!!!
        public static int[] DoDraw;


        /// <summary>
        /// perform spatial subdivision
        /// lev is depth to go to if needed
        /// levd is depth below bottom to which you must go
        /// </summary>
        /// <param name="vol"></param>
        /// <param name="node"></param>
        /// <param name="lev"></param>
        /// <param name="levd"></param>
        ///
        public void SD(Volume vol, CSGNode node, int lev) {
            // if (node.Color == CSGNode.DefaultColor) node.SetColor(Color.WhiteSmoke);
            //node.WithColTexture(CSGControl.DefaultTexture);
            CSGNode.csgmode = CSGMode.rendering;  // debug helper flag
            try {
                if (CSGControl.MaxLev == 0) {
                    CSGNode n2;
                    //int unodes = 0;
                    //n2 = node.Bake().Simplify(vol, -999, ref unodes);
                    n2=node;
                    //string sss = n2.ToStringF("\n");
                    //GUIBits.Log(sss);


                    n2.Draw(csgoutput, vol);
                } else {
                    SDivNoUnion(vol, node, lev);
                    }
            } finally {
                CSGNode.csgmode = CSGMode.collecting;
            }
        }

        /// <summary>
        /// perform subdivision, but implement top level Union by multiple polygons
        /// if allowed to by CSGControl.QuickUnion
        /// </summary>
        /// <param name="vol"></param>
        /// <param name="node"></param>
        /// <param name="lev"></param>
        public void SDivNoUnion(Volume vol, CSGNode node, int lev) {
            Union u = node as Union;
            if (u == null || !CSGControl.QuickUnion) {
                SDiv(vol, node, lev);
            } else {
                SDivNoUnion(vol, u.l, lev);
                SDivNoUnion(vol, u.r, lev);
            }
        }

        public void SDiv(Volume vol, CSGNode node, int lev) {
            if (CSGControl.UseBreak && !vol.Includes(CSGControl.BreakPosition))
                return;

            if (CSGControl.Interrupt) {
                CSGControl.Interrupt = false;
                throw new InterruptedException("Interrupted by user request.");
            }

            // bottom level of recursion, mark as impossible visited
            //if (lev > CSGControl.MaxLev) {
            //    //volList.Add(vol);
            //    int n = node.Draw(csgoutput, vol);  // we have to draw however complicated, or ??? leave out
            //    if (node.nodes < hits.Length) hits[(node.nodes)]++;
            //    uhits[n]++;
            //    return;
            //}

            // uNodes is not reliable as it allows for nodes that were looked at but eliminated
            // snode.nodes does include duplicates, but that is less serious
            int nUnodes = 0;
            CSGNode snode = node.OSimplify(vol, S.Simpguid++, ref nUnodes);
            if (snode.nodes == 0) {
                done += Mathf.Pow(0.125f, lev);
                return;
            }
            if (lev >= CSGControl.MinLev && (nUnodes <= DoDraw[lev] || lev >= CSGControl.MaxLev)) {  // TODO: TOCHECK was snode.nodes <=
                //volList.Add(vol);
                if (CSGControl.UseBreak) {
                    string sss = snode.ToStringF("\n");  // <<< key draw point
                    UnityEngine.Debug.Log("sss = " + sss);
                    GUIBits.Log("sss = " + sss);
                }
                done += Mathf.Pow(0.125f, lev);
                int n = snode.Draw(csgoutput, vol);
                if (CSGControl.stats) { // #if STATS
                    if (node.nodes < hits.Length) hits[snode.nodes]++;
                    uhits[n]++;
                } //   #endif
                return;
            }

            for (int i = 0; i < 8; i++) {
                SDiv(vol.SV(i), snode, lev + 1);
            }
        }

        public void PrintHits() {
            if (CSGControl.stats) { // #if STATS
                if (CSGControl.TRLEV < TraceN.minlev) return;  // save a little
                int k;
                for (k = 25; k != 0 && hits[k] == 0; k--) { }  // don't show trailing 0
                string msg = " hits:";
                for (int i = 0; i <= k; i++) msg += string.Format("  {0}->{1}", i, hits[i]);
                TraceN.trace(CSGControl.TRLEV, msg);

                msg = ("uhits:");
                for (k = 25; k != 0 && uhits[k] == 0; k--) { }  // don't show trailing 0
                for (int i = 0; i <= k; i++) msg += string.Format("  {0}->{1}", i, uhits[i]);
                TraceN.trace(CSGControl.TRLEV, msg);
            } else { // #else
                //TraceN.trace(CSGControl.TRLEV, "PrintHits() has no stats, recompile with STATS flag for stats"); //pjt Unity
                GUIBits.Log("PrintHits() has no stats, recompile with STATS flag for stats");
            } // #endif
        }


        public static void SetVals() { // !!!!!
            //Subdivide.DoDraw = new int[] { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 };
            //Subdivide.DoDraw = new int[] { 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20, 20 };
            //Subdivide.DoDraw = new int[] { 16, 16, 8, 8, 8, 8, 8, 3, 3, 3, 3, 3, 3 };  // 0.113
            int[] dd;
            //// nn = new int[] { 3, 3, 3, 4, 5, 6, 8, 12, 16, 24 }; // 0.079 with snode,nodes
            //nn = new int[] { 3, 4, 5, 6, 8, 12, 16, 24 };  // 0.07 with snode,nodes
            //nn = new int[] { 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30, 30 };  // 1.1
            //nn = new int[] { 10, 10, 10, 12, 16, 24 };  // 17
            //nn = new int[] { 10, 10, 10, 12, 16, 24, 24, 24 };  // 0.2
            //nn = new int[] { 3, 4, 5, 6, 8, 8, 8, 8, 8, 8 };  // 0.13 with snode,nodes
            //nn = new int[] { 3, 4, 5, 6, 8, 12, 16, 24 };  // 0.07 with snode,nodes [0.05 with optimize]
            //nn = new int[] { 3, 4, 5, 5, 6, 8, 8, 12 };  // 0.07 with snode,nodes [0.05 with optimize]
            //nn = new int[] { 3, 4, 5, 5, 5, 5, 8, 12 };  // 0.06 with snode,nodes [0.05 with optimize]

            //// from optimize 25, 17, 16, 12, 10, 8, 8, 6, 5, 9, 2, 3, 5
            //// from optimize maxlev 10 == 30, 29, 18, 16, 18, 9, 8, 10, 14, 5, 16_ , 3, 3   => 0.022
            //// from opt    maxlev = 10 == 25, 19, 16, 11,  9, 8, 8,  6,  5, 5, 12_
            ////nn = new int[] { 6,6,6,6,6,6,6,6 };  // 0.07 with snode,nodes [0.05 with optimize]
            ////nn = new int[] { 8,8,8,8,8,8,8,8 };  // 0.07 with snode,nodes [0.05 with optimize]
            //// 4,  7, 6, 10,  9,  9, 13, 18, 21, 29_
            //// 9, 13, 7, 13, 15, 10, 15, 23, 28, 38_
            //nn = new int[] { 25, 19, 16, 11, 9, 8, 8, 6, 5, 5, 12 };
            //nn = new int[] { 5, 5, 6, 8, 8, 9, 11, 16, 19, 25 };
            //nn = new int[] { 6, 7, 8, 8, 11, 5, 11, 12, 20, 21, 999 };
            //nn = new int[] { 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 999 };
            //nn = new int[] { 6, 7, 8, 8, 10, 11, 11, 12, 20, 21, 999 };

            //nn = new int[] { 6, 6,6,6,6,6,6,6,6,6,6,6,6, 999 };
            // , 6, 4, 10, 9, 12, 9, 12, 14, 19, 20_ , 999 => 0.0718 no opt
            // nn = new int[] { 6, 7, 8, 8, 8, 8, 8, 8, 8, 8, 8 };
            // release standalone  6, 7, 8, 8, 11, 5, 11, 12, 20, 21_  => 0.0338402876622588
            // , 8, 7, 9, 9, 11, 13, 21, 14, 21, 21_ , 999 => 0.0156

            // optimized against georgian 16 Sept 2009
            dd = new int[] {
                9,
                10,
                6,
                6,
                7,
                12,
                9,
                11,
                15,
                15,
                15,
                15,
                15,
                999
            };
            // optimized against georgian 25 Sept 2009
            dd = new int[] {
                10,
                10,
                6,
                7,
                8,
                15,
                8,
                13,
                17,
                18,
                15,
                15,
                15,
                999
            };

            // optimized with OptTest against twohouse with some fancy double profile, 10 Nov 2011
            dd = new int[] {
                11,
                12,
                4,
                4,
                10,
                20,
                18,
                16,
                999,
                999,
                999,
                999,
                999,
                999
            };

            // optimized with OptTest against twohouse with some fancy double profile, improved object merge, 13 Nov 2011
            dd = new int[] {
                12,
                12,
                4,
                5,
                8,
                21,
                29,
                26,
                17,
                18,
                999,
                999,
                999,
                999
            };

            dd = new int[] {
                12,
                11,
                4,
                7,
                8,
                21,
                30,
                27,
                17,
                20,
                20,
                20,
                20,
                20,
                20,
                20,
                20,
                20,
                999,
                999,
                999,
                999
            };
            // !!! DoDraw set here
            Subdivide.DoDraw = dd;
        }

    }

    //// class to hold a vertex and reference back to an object (not currently used)
    //public class PrimVertex {
    //    public PrimVertex(CSGPrim prim, Vector3 Position) {
    //        this.Prim = prim;
    //        this.Position = Position;
    //    }
    //    public Vector3 Position;
    //    public CSGPrim Prim;
    //}

    /// <summary>
    /// Interface to permit incremental collection of polygons for output from CSG.
    /// </summary>
    public interface ICSGOutput {
        /// <summary>
        /// Add a Poly to the items to be collected
        /// </summary>
        /// <param name="poly"></param>
        //void Add(Poly poly);

        /// <summary>
        /// Add a Poly to the items to be collected
        /// </summary>
        /// <param name="poly"></param>
        void Add(ICSGPoly poly);

        /// <summary>
        /// Add a loop of points to the items to be collected
        /// </summary>
        /// <param name="prim"></param>
        /// <param name="polyp"></param>
        void AddLoop(CSGPrim prim, params Vector3[] polyp);

        /// <summary>
        /// Count of the number of Polys/Loops collected.
        /// </summary>
        int Count { get; }
        //IEnumerable<Poly> allPolys { get; }
        //Dictionary<string, LinkedList<Poly>> polys { get; }
    }

    /// <summary>
    /// Debug implementation of ICSGOutput: no actual collection.
    /// </summary>
    public class NOCSGOut : ICSGOutput {
        private int n = 0;

        public void Add(Poly poly) {
            n++;
        }
        //public void Add(ICSGPoly poly) { n++; }
        public void AddLoop(CSGPrim prim, params Vector3[] polyp) {
            n++;
        }

        public int Count { get { return n; } }

        public static void CSGToDummy(CSGNode csg, Volume vol) {
            NOCSGOut nolist = new NOCSGOut();
            S.CSGToCSGOutput(csg, vol, nolist);
        }

    }

    /// <summary>
    /// XList is a list of values used to collect output information ready for drawing
    /// Keep them separated by texture ~ texture identified by string texture name.
    /// </summary>
    public class XList : ICSGOutput {

        public XList() {
            _polys = new Dictionary<string, LinkedList<Poly>>();
        }

        private Dictionary<string, LinkedList<Poly>> _polys;

        public Dictionary<string, LinkedList<Poly>> polys { get { return _polys; } }

        public int Count {
            get {
                int n = 0;
                foreach (LinkedList<Poly> p in _polys.Values)
                    n += p.Count;
                return n;
            }
        }

        public void Add(Poly poly) {
            if (poly.Count < 3) {
                poly.ConditionalReleaseToPool();
                return;
            }  // ignore degenerate polys
            string text = poly.Csg.Texture;
            if (!polys.ContainsKey(text))
                _polys.Add(text, new LinkedList<Poly>());
            polys[text].AddLast(poly);
        }

        //public void Add(ICSGPoly poly) { Add((Poly)poly);}


        public void AddLoop(CSGPrim prim, params Vector3[] polyp) {
            Poly poly = Poly.Make();
            for (int i = 0; i < polyp.Length; i++)
                poly.Add(polyp[i]);
            poly.AddTo(prim, this);
        }

        /// <summary>
        /// Convert CSG->XList
        /// </summary>
        /// <param name="csg"></param>
        /// <param name="vol"></param>
        /// <returns></returns>
        public static XList CSGToXList(CSGNode csg, Volume vol) {
            XList xlist = new XList();
            S.CSGToCSGOutput(csg, vol, xlist);
            // Console.WriteLine("XList count = {0}, frompool={1}, fromnew={2}", xlist.Count, Poly.frompool, Poly.fromnew); // debug pooling
            return xlist;
        }

    }



    /// <summary>
    /// Decorated point: PolyDecPoint points are only used as vertices in a Poly,
    /// and implicitly represent the line from this point to the next.
    /// Where that line is generated by the intersection of two csg primitives,
    /// one csg will be the csg for the Poly, and othercsg will hold the other csg primitive.
    ///
    /// was struct, but needed to be class to enable retrospective setting of othercsg in Poly context
    /// made back to struct, much cheaper init of array elements
    ///
    /// Used to hold normals, but was more efficient to defer normal extraction till later
    /// </summary>
    public struct PolyDecPoint {
        public Vector3 point;
        public CSGPrim othercsg;

        internal PolyDecPoint(Vector3 v, CSGPrim csg, Vector3 n) {
            point = v;
            othercsg = csg;
        }

        public override string ToString() {
            return String.Format("polydecpoint[{0:0.00}, {1:0.00}, {2:0.00}]", point.x, point.y, point.z);
        }

        public Vector3 Normal(Poly poly) {
            return poly.Csg.Normal(point);
        }
    }

    /// <summary>
    /// Poly interface used during the CSG construction process.
    /// We may introduce a faster, simplified implementation of this; which copies into 'external' polygons at AddTo() time.
    /// </summary>
    public interface ICSGPolyTest {
        void Add(Vector3 v);

        void Add(Vector3 v, CSGPrim othercsg);

        int Count { get; }

        PolyDecPoint this[int n] { get; }

        void setLastOthercsg(CSGPrim csg);

        void AddTo(CSGPrim prov, ICSGOutput pl);

        void Split(CSGPrim splitprim, out ICSGPoly inpoly, out ICSGPoly outpoly);

        void ConditionalReleaseToPool();
    }

    /// <summary>
    /// Poly holds a list of points in a polygon,
    /// and the associated csg primitive Node on whose surface the polygon lines.
    ///
    /// Each point is held as a PolyDecPoint,
    /// which also represents the line segment from that point to the next point.
    /// the othercsg is set if that line segment represents the intersection between the
    /// poly should give a closed loop.
    ///
    /// Poly performs two roles:
    ///     1 - as internal class used during CSG operations (ICSGPoly)
    ///     2 - as external class to hold the result of CSG operations (Poly)
    /// These are combined into the single Poly class implementation.
    /// The roles are distinguished by the presence of a nonnull Provenance/csg field.
    /// During CSG processing this field is null.
    /// At the lowest level of CSG processing, CSGNode.Split() decides an ICSGPoly is to be part of output.
    /// At that point, it uses the AddTo method to mark the ICSGPoly with its associated CSG, and to accumulate it into the output.
    /// (aside: previously, the csg was associated with the poly when the poly was first created.
    /// This caused issues with negative CSGs;  for example a Poly first allocated when processing PlaneXN(0) may end up associated with PlaneXP(0).)
    ///
    /// ICSGPoly role is performance critical; many are made, grown and discarded during CSG processing.
    /// Poly role is much less performance critical; many fewer Polys ever exist than ICSGPolys, and a Poly does not change size after it is established.
    /// It would be possible to split these roles into two completely separate class implementations,
    /// and to copy the ICSGPoly into a new Poly at AddTo() time.
    /// However, there is no significant overhead in the Poly implementation for use as ICSGPoly,
    /// and combining them saves data copy at AddTo() time.
    ///
    /// Fast availability of new ICSGPoly instances is a critical part of the CSG performance.
    /// We have therefore implemented pooling on their Poly implementation.
    /// Each method that causes a temporary ICSGPoly to be allocated (by Poly.Make())
    /// is also responible for returning it to the pool (by ConditionalReleaseToPool()).
    /// Some of the temporaries get to have a more extended life (role changed from ICSGPoly to Poly by AddTo());
    /// ConditionalReleaseToPool() checks for this and does not release such Poly objects to the pool.
    ///
    /// When these roles were distinguished, we separated them with an ICSGPoly interface, implemented by Poly.
    /// (We did not make a separate interface for the external CSG result role.)
    /// We have changed that in the code; re-identifying ICSGPoly with Poly.
    /// This change theoretically makes the code less pure; we have decided on it for two reasons.
    ///     1 - It makes it easier to navigate in the IDE during development between method calls and method implementations.
    ///     2 - The interface could potentially inhibit optimization;
    ///         making it more difficult to see that a given method call will always use the same implementation.
    /// We maintained most of the ICSGPoly/Poly distinction in the code, in case we wish to change the implementations.
    /// We performed the identification while maintain the distinction by the following code changes:
    ///     "using ICSGPoly = Poly;",
    ///     renaming the ICSGPoly interface to ICSGPolyTest
    ///     removing the "implements ICSGPoly" from the Poly class definition
    ///     a couple of minor changes to Add() methods
    ///
    /// Notes on growing the number of PolyDecPoint in a Poly
    /// ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    /// This used to be a major performance concern.
    /// The introduction of pooled, ready grown, Poly instances makes it much less significant.
    ///
    /// List performs better than ArrayList, LinkedList, Stack or Queue
    /// Iterating through List still seems to take quite a bit
    /// [] costs too much to do object creation
    ///
    /// Hybrid implementation seems worth the effort, 0.081 to 0.068.
    /// Use array for first PNUM elements, and then overflow into a list.
    /// Change to extensible arrays gave really worse performance (0.15),
    /// so reverted, with fix for issue with setOthercsg() (cannot modify struct within list)
    ///
    /// We finally settled on our own extensible array.
    ///
    /// </summary>
    public class Poly : CSGControl, IHasProvenance {
        // , ICSGPoly {
        // implementation of the poly as hybrid array and overflow list
        private PolyDecPoint[] points = new PolyDecPoint[PNUM];
        private int num = 0;
        // number of points in the poly
        //private List<PolyDecPoint> plist = null;  // for overflow
        public static bool windDebug = false;
        // set to true to debug winding code
        private static int pid = 0;
        public readonly int Id = pid++;

        /// <summary>
        /// Pointer to a list of 'overflow' polys, so a single Poly can represent several polygons.
        /// Only used where used where a primitive has >1 poly for a given volume.
        /// This applies to the 'full' poly returned by PolyForVol, specificaly PolyForVolField;
        /// the Poly's generated internally by splitting never use this field.
        ///
        /// It would be good to have this field only in a subclass PolyN of Poly,
        /// but this is awkward with the co/contravariance limitations in the target level of C#
        /// (eg does PolyForVol return Poly or PolyN?)
        /// </summary>
        internal List<Poly> extraPolys = null;

        /// <summary>
        ///  get/set the provenance of the object.  (Should be CSGPrim, but not allowed by co/contravariance.)
        /// </summary>
        public object Provenance { get { return csg; } /*private set { csg = (CSGPrim)value; }*/ }

        public IHasProvenance SetProvenance(object prov) {
            throw new NotSupportedException("poly prov should be set internally"); /*Provenance = prov; return this;*/
        }

        public IHasProvenance SingleProvenance(object prov) {
            throw new NotSupportedException("poly prov should be set internally"); /*Provenance = prov; return this;*/
        }

        public Poly Reverse() {
            ICSGPoly pp = Poly.Make();
            for (int ii = Count - 1; ii >= 0; ii--)
                pp.Add(this[ii].point);
            return pp;
        }

        // options for alternative implementations
        // for hybrid
        // not used ... internal PolyDecPoint First() { return points[0]; }

        /// <summary>
        /// Add a new point to this polygon; where the associated line is a construction line
        /// generated by voxel boundary and no by intersection with another CSG.
        /// </summary>
        /// <param name="v"></param>
        public void Add(Vector3 v) {
            Add(v, null);
        }

        /// <summary>
        /// Add a new point to this polygon; giving the csg (if any) that defines the line following the point.
        /// </summary>
        /// <param name="v"></param>
        /// <param name="othercsg"></param>
        public void Add(Vector3 v, CSGPrim othercsg) {
            if (num > 20) {
                v.x *= 1;
            }
            if (CSGControl.Interrupt) {
                CSGControl.Interrupt = false;
                throw new InterruptedException("Interrupted by user request.");
            }

            if (num == points.Length) {
                //PolyDecPoint[] np = new PolyDecPoint[points.Length * 2];
                //Array.Copy(points, np, points.Length);
                //points = np;
                Array.Resize(ref points, points.Length * 2);
            }
            points[num].point = v;
            points[num].othercsg = othercsg;
            num++;
        }

        public static int[] verttype = new int[3];  // stats on polygon vertex types

        public void AddTo(CSGPrim prov, ICSGOutput pl) {
            //debugpolypool.Remove(this);
            if (prov.Provenance == null) {
            }
            csg = prov;
            pl.Add(this);

            if (CSGControl.stats) {
                for (int i = 0; i < num; i++) {
                    int n = (points[i].othercsg == null ? 0 : 1) + (points[i == 0 ? num - 1 : i - 1].othercsg == null ? 0 : 1);
                    verttype[n]++;
                }
            }
        }


        /// <summary>
        /// return the number of points in the Poly
        /// </summary>
        public int Count { get { return num; } }

        /// <summary>
        /// return the i'th point in the Poly
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public PolyDecPoint this[int n] {
            get {
                return points[n];
            }
        }

        /// <summary>
        /// set othercsg for the i'th point in the Poly
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public void setOthercsg(int n, CSGPrim csg) {
            points[n].othercsg = csg;
        }


        /// <summary>
        /// set othercsg for the last point in the Poly
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        public void setLastOthercsg(CSGPrim csg) {
            setOthercsg(num - 1, csg);
        }


        // for ArrayList internal Vector3 First() { return (Vector3)this[0]; }
        // public void Add(Vector3 v) { Push(v); }  // for Stack
        // public void Add(Vector3 v) { this.Enqueue(v); }  // for Queue
        // for LinkedList public void Add(Vector3 v) { AddFirst(v); }
        //public void Add(Vector3 v) {
        //    points[num] = v;
        //    num++;
        //}


        /// <summary>
        /// Make a new Poly, using the pool if possible.
        /// </summary>
        /// <returns></returns>
        public static Poly Make() {
            //if (pool.Count == 0) {
            //    fromnew++;
            //    return new Poly();
            //} else {
            //    frompool++;
            //    return pool.Pop();
            //}
            Poly p;
//?? TODO check why this caused deadlock even on single thread			
//??            lock (pool) {
                p = pool.Pop();
                if (p == null) {
                    fromnew++;
                    pool.Push(null);
                } else {
                    frompool++;
                }
//??            }
            if (p == null)
                p = new Poly();
            //debugpolypool.Add(p);
            return p;
        }

        //public static List<Poly> debugpolypool = new List<Poly>();  // debug to help track missing ones after pooling, see CSGToXList for associated debug

        /// <summary>stats for Poly pool</summary>
        public static int frompool = 0, fromnew = 0;
        /// <summary>Poly pool; will always contain a null at the bottom to help detect empty.  Why no efficient IsEmpty() method?</summary>
        private static Stack<Poly> pool = new Stack<Poly>();

        static Poly() {
//??            lock (pool)
                pool.Push(null);
        }
        // needed if checking for null as empty pool

        /// <summary>
        /// If the poly is not used as output, release it to pool.
        /// It is used as output if it has its Provenance/csg set.
        /// </summary>
        public void ConditionalReleaseToPool() {
            //!!!if (frompool >=0 ) return;  // debug!!!
            if (csg != null)
                return;
            if (extraPolys != null) {
                foreach (Poly sp in extraPolys)
                    sp.ConditionalReleaseToPool();
            }
            num = 0;
//??            lock (pool)
                pool.Push(this);
            //debugpolypool.Remove(this);
        }

        private CSGPrim csg;

        /// <summary>
        /// get the Csg: same as Provenance but more strongly typed.
        /// </summary>
        public CSGPrim Csg { get { return csg; } }

        /// <summary>Add a Vectr to the poly, given x,y,z</summary>
        public void Add(float x, float y, float z) {
            Add(new Vector3(x, y, z));
        }


        //// go reound the poly and back to first point: a little expensive
        //IEnumerable<Vector3> Loop() {
        //    foreach (Vector3 v in this) yield return v;
        //    yield return this.First();
        //}

        public static int splits = 0;  // for stats
        /// <summary>
        /// Split the poly at another csg
        /// This assumes convex, so just two parts
        /// It forces exact match points slightly non-0,
        /// but tries to choose non-0 in the direction that causes fewer silly little polygons.
        ///
        /// TODO: SplitNEW attempted to allow properly for 0, still worth revisiting sometime
        /// 24 Sept, introduced firstd to ensure consistent treatment of d for point 0 at start/end
        /// We are reasonably likely to encounter exact hits (d==0) where we have identical geometry
        /// but with different provenance (so not equated).
        /// A common case is all the different eaves, which must be kept separate to ensure correct tagging,
        /// but which often share identical geometry.
        /// TODO: Sort out at higher level how to cope with identical geometry.
        /// </summary>
        /// <param name="csg">The CSG primitive on which to split this polygon.</param>
        /// <param name="inpoly">OUTPUT: The polygon for surface inside the split primitive.</param>
        /// <param name="outpoly">OUTPUT: The polygon for surface outside the split primitive.</param>
        public void Split(CSGPrim splitprim, out ICSGPoly inpoly, out ICSGPoly outpoly) {
            splits++;
            inpoly = null;
            outpoly = null;

            float delta = 0.0000011f;
            //##float maxd = float.MinValue, mind = float.MaxValue;
            float lastd = 0;
            Vector3 lastp = Vector3.zero;
            CSGPrim lastcsg = null;
            float firstd = 0;
            Vector3 firstp = Vector3.zero;
            bool pendingd0 = false;  // set to true where we get a d == 0
            // foreach (Vector3 p in this) {
            for (int i = 0; i < num; i++) {
                PolyDecPoint pdp = points[i]; // this[i]; inline just worth it
                Vector3 p = pdp.point;
                CSGPrim pcsg = pdp.othercsg;

                float d = splitprim.Dist(p.x, p.y, p.z);
                //##if (d == 0) d = (lastd > 0 ? 1 : -1) * delta;  // cheat for now to resolve exact hit
                if (-delta < d && d < delta)
                    d = 0;  // force near points
                if (i == 0) {
                    firstd = d;
                    firstp = p;
                }
                //##if (d > maxd) maxd = d; if (d < mind) mind = d;

                // allowing for zero reduces triangles, but can cause creation of
                // extra new Poly that is not used.
                // This zero reduction code is more expensive than the older version,
                // TODO: make more sophisticated so pending crossover does not require new
                //// if (CSGNode.Small(d)) d = 0;
                //if (d == 0) {
                //    if (inpoly == null) inpoly = Poly.Make(csg);
                //    inpoly.Add(p);
                //    if (outpoly == null) outpoly = Poly.Make(csg);
                //    outpoly.Add(p);
                //} else
                {
                    if (inpoly == this || outpoly == this) {
                        throw new Exception("poly mix up");
                    }
                    if (d * lastd < 0) {  // switch sides, find new crossing point for both in and out
                        // worryingly, the lines below were significantly faster than the slow! line
                        float s = 1 / (d - lastd);
                        float d1 = d * s, d2 = lastd * s;
                        float x = lastp.x * d1 - p.x * d2;
                        float y = lastp.y * d1 - p.y * d2;
                        float z = lastp.z * d1 - p.z * d2;
                        Vector3 newp = new Vector3(x, y, z);
                        // Vector3 newp = (lastp * d - p * lastd) * s;  // slow!
                        if (inpoly == null)
                            inpoly = Poly.Make();
                        if (outpoly == null)
                            outpoly = Poly.Make();
                        if (d < 0) {
                            outpoly.Add(newp, splitprim);
                            inpoly.Add(newp, lastcsg);
                            inpoly.Add(p, pcsg);
                        } else {
                            inpoly.Add(newp, splitprim);
                            outpoly.Add(newp, lastcsg);
                            outpoly.Add(p, pcsg);
                        }
                    } else if (d < 0) {
                        if (inpoly == null)
                            inpoly = Poly.Make();
                        inpoly.Add(p, pcsg);
                        if (pendingd0) {
                            outpoly.setLastOthercsg(splitprim);
                            pendingd0 = false;
                        }
                    } else if (d > 0) {
                        if (outpoly == null)
                            outpoly = Poly.Make();
                        outpoly.Add(p, pcsg);
                        if (pendingd0) {
                            inpoly.setLastOthercsg(splitprim);
                            pendingd0 = false;
                        }
                    } else {  // d == 0
                        if (inpoly == null)
                            inpoly = Poly.Make();
                        inpoly.Add(p, pcsg);
                        if (outpoly == null)
                            outpoly = Poly.Make();
                        outpoly.Add(p, pcsg);
                        pendingd0 = true;  // one of the above will be overridden with splitprim
                    }
                }
                lastd = d;
                lastp = p;
                lastcsg = pcsg;
            }  // end main loop on poly points

            // check for crossing between last and first
            {
                Vector3 p = firstp;
                float d = firstd;
                if (d * lastd < 0) {  // switch sides, find new crossing point for both in and out
                    // worryingly, the lines below were significantly faster than the slow! line
                    float s = 1 / (d - lastd);
                    float d1 = d * s, d2 = lastd * s;
                    float x = lastp.x * d1 - p.x * d2;
                    float y = lastp.y * d1 - p.y * d2;
                    float z = lastp.z * d1 - p.z * d2;
                    Vector3 newp = new Vector3(x, y, z);
                    // Vector3 newp = (lastp * d - p * lastd) * s;  // slow!
                    // don't create a special poly if we got this far without needing it
                    if (d < 0) {
                        if (outpoly != null)
                            outpoly.Add(newp, splitprim);
                        if (inpoly != null)
                            inpoly.Add(newp, lastcsg);
                    } else if (d > 0) {
                        if (inpoly != null)
                            inpoly.Add(newp, splitprim);
                        if (outpoly != null)
                            outpoly.Add(newp, lastcsg);
                    } else {
                    }
                }
                if (pendingd0) {
                    if (d < 0) {
                        outpoly.setLastOthercsg(splitprim);
                    } else if (d > 0) {
                        inpoly.setLastOthercsg(splitprim);
                    } else {
                        // ??? TODO think what the correct answer is for this case, probably either both or neither
                        //outpoly.setLastOthercsg(splitprim);
                        //inpoly.setLastOthercsg(splitprim);
                    }
                }
            }

            //// clean up if one just touched but did not make a poly (eg exact 0)
            //if (!inused) inpoly = null;
            //if (!outused) outpoly = null;

            // we may have generated some degenerate polys: get rid of them now
            if (inpoly != null && inpoly.Count < 3) {
                inpoly.ConditionalReleaseToPool();
                inpoly = null;
            }
            if (outpoly != null && outpoly.Count < 3) {
                outpoly.ConditionalReleaseToPool();
                outpoly = null;
            }

            //// DEBUG to remove
            //if (windDebug) {
            //    if (!(csg as CSGPlane).CheckPolyWind(this)) {
            //        Console.WriteLine("wrong wind THIS input poly");
            //    }
            //    if (inpoly != null && !(csg as CSGPlane).CheckPolyWind(inpoly)) {
            //        Console.WriteLine("wrong wind inpoly");
            //    }
            //    if (outpoly != null && !(csg as CSGPlane).CheckPolyWind(outpoly)) {
            //        Console.WriteLine("wrong wind outpoly");
            //    }
            //}

            // various alternative tests for very thin polys
            // problems where we have identical csg == splitprim
            //##if (maxd == 0) { if (outpoly != null) { };  debugsmall++; outpoly = null; }
            //##if (mind == 0) { if (inpoly != null) { };  debugsmall++; inpoly = null; }

            //##if (maxd <= delta) { if (outpoly != null) { };  debugsmall++; outpoly = null; }
            //##if (mind >= -delta) { if (inpoly != null) { };  debugsmall++; inpoly = null; }
            //if ((inpoly != null && inpoly.Count < 3) || (outpoly != null && outpoly.Count < 3)) { }
        }

        public static int debugsmall = 0;

    }


    // putting here for now
    public interface ISelectable {
    }

    /// <summary>
    /// IProvenance interface provides access to the provenance of an object.
    /// For example, a line may be derived from a polygon derived from a CSG Plane derived from a Mass Component.
    /// </summary>
    public interface IHasProvenance {
        /// <summary>
        /// provenanace property that may be got or set
        /// </summary>
        object Provenance { get; }

        /// <summary>
        /// method to set provenance where the result may be a different object
        /// </summary>
        /// <param name="provenance"></param>
        /// <returns></returns>
        IHasProvenance SetProvenance(object provenance);
    }

    /// <summary>
    /// This is a flag to indicate that this object can ue used to provide proveanance.
    /// It does not have any huge benefit, but enforces a useful extra marker/reminder on the code.
    /// I considered a stronger alternative using parameterized IHasProvenance<ProvenanceType>,
    /// but the ramifications were quite complicated for very little benefit.
    /// </summary>
    public interface NOTUSEDIIsInterface {
    }



    /// <summary>
    /// interface to implement a distance function
    /// </summary>
    public interface IDist {
        float Dist(float x, float y, float z);
    }

    /// <summary>
    ///  this interface is used to communicate for example to CSG
    /// </summary>
    public interface IColorable {
        Color Color { get; set; }

        string Texture { get; }

        //        IColorable WithTextureI(string texture, bool force);
    }

    /// <summary>
    ///  helpers for Vector work
    /// </summary>
    public static class VHelpers {
        public static Vector3 Normal(this Vector3 v) {
            return Vector3.Normalize(v);
        }

        public static Vector2 Normal(this Vector2 v) {
            v.Normalize();
            return v;
        }
        //pjt Unity XXX WARNING original had side effects
        public static Vector2 IY(this Vector2 v) {
            return new Vector2(v.x, -v.y);
        }
        // invert Y for standard 2d clockwise
        public const double Small = 0.0001;
        //note: pjt reduced this from 0.0001 as I'm encountering many close to coincident edges... set back as it didn't  help much
        public static Boolean Close(this float x, float y) {
            return Math.Abs(x - y) < Small;
        }

        public static Boolean Close(this float x, double y) {
            return Math.Abs(x - y) < Small;
        }

        public static Boolean Close(this double x, double y) {
            return Math.Abs(x - y) < Small;
        }

        public static System.Random random = new System.Random();
        //pjt Unity vs System random ambiguity...
        static double rand { get { return random.NextDouble(); } }

        /// <summary>
        /// set texture with a bit of random colour
        /// </summary>
        /// <param name="node"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        //        public static CSGNode SetTextureRCol(this CSGNode node, string text) {
        //            if (!text.EndsWith("_col"))
        //                text += "_col";
        //
        //            if (CSGControl.TextureColourAmount == 0) {
        //                node = node.WithColTexture(text);
        //                if (node.provneg != null)
        //                    node.provneg = node.provneg.WithColTexture(text);
        //                return node;
        //            }
        //            CSGNode r = node.SetColorNew(rand, rand, rand, CSGControl.TextureColourAmount).WithColTexture(text);
        //            if (r.provneg != null && !r.provneg.HasColor)
        //                r.provneg = SetTextureRCol(r.provneg, text);
        //            return r;
        //        }

    }

    public class InterruptedException : Exception {
        public InterruptedException(string m)
            : base(m) {
        }
    }

    class TraceN {
        public static int minlev = 99;

        public static void trace(int TraceLevel, String msg) {
            if (TraceLevel < minlev)
                GUIBits.Log(msg);
        }
    }

    class UnbakedException : Exception {

    }

    public class FilterString {
        string[][] ors;
        char[] bsep = { ' ' };

        public FilterString(string s) {
            string[] orss = s.Split('|');
            ors = new string[orss.Length][];
            for (int i=0; i<orss.Length; i++) {
                ors[i] = orss[i].Split(bsep, StringSplitOptions.RemoveEmptyEntries);
            }
        }

        public bool filter(string f) {
            if (f == null) return false;
            foreach(var or in ors) {
                foreach (var e in or) {
                    if (!f.Contains(e)) goto NEXTORS;
                }
                return true;
            NEXTORS:;
            }
        return false;
        }
    }

}

public static class CSGExtras {
    public static Color set(this Color col, Color c) {
        return col = c;
    }
    public static Color set(this Color col, float r, float g, float b) {
        return col.set(r, g, b, 1);
    }
    public static Color set(this Color col, float r, float g, float b, float a) {
        col.r = r;
        col.g = g;
        col.b = b;
        col.a = a;
        return col;
    }

    // not supported at this level [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static float sq(this float x) { return x * x; }
    public static double sq(this double x) { return x * x; }
    public static Color Max(this Color c1, Color c2) {
        Color cr = new Color();
        cr.r = Mathf.Max(c1.r, c2.r);
        cr.g = Mathf.Max(c1.g, c2.g);
        cr.b = Mathf.Max(c1.b, c2.b);
        cr.a = Mathf.Max(c1.a, c2.a);
        return cr;
    }
    public static Color Min(this Color c1, Color c2) {
        Color cr = new Color();
        cr.r = Mathf.Min(c1.r, c2.r);
        cr.g = Mathf.Min(c1.g, c2.g);
        cr.b = Mathf.Min(c1.b, c2.b);
        cr.a = Mathf.Min(c1.a, c2.a);
        return cr;
    }


}

#region // experiments in keeping lists of edges
/***

    public struct CSGPrimPair {
        CSGPrim a, b;
        public CSGPrimPair(CSGPrim a, CSGPrim b) { this.a = a; this.b = b; }
    }
    public class Edge {
        PolyDecPoint p0, p1;
        public Edge(PolyDecPoint p0, PolyDecPoint p1) { this.p0 = p0; this.p1 = p1; }
    }

    /// <summary>
    /// EList keeps a list of 'interesting' edges
    /// </summary>
    public class EList {
        public Dictionary<CSGPrimPair, List<Edge>> edges = new Dictionary<CSGPrimPair, List<Edge>>();

        void Add(CSGPrim csg, PolyDecPoint p0, PolyDecPoint p1) {
            Edge e = new Edge(p0, p1);

            CSGPrimPair pp = new CSGPrimPair(csg, p0.othercsg); // <<<<<<< TODO FIX
            if (!edges.ContainsKey(pp)) edges.Add(pp, new List<Edge>());
            edges[pp].Add(e);
        }

        public void Add(Poly poly) {
            PolyDecPoint pp = poly.points.Last();
            CSGPrim csg = poly.Csg;
            foreach (PolyDecPoint p in poly.points) {
                if (pp.othercsg != null) Add(csg, pp, p);


                pp = p;
            }
        }
    }


    /// <summary>
    /// LocalEList keeps a list of 'interesting' edge segments
    /// There is a new LocalEList for each primitive in each volume,
    /// and edges[] is indexed by edges between that primitives and the other prmitives.
    ///
    /// Note, sjpt 30 Sept 2009
    /// To consider, using LocalEList to clean up false edges generated by CSG process.
    ///   This will get rid of false edges, but cannot join cross-subdivide segments of true edges.
    ///   Current performance figures on georgian.  Using LocalElist increases cost from 0.79 to 0.90 (debug mode)
    /// Alternative is to clean up when the complete csg has been generated.
    ///   This can get rid of false edges and join cross-subdivide segments of true edges.
    ///   Current performance figures [PolyBasedDecoration.Rework()] indicates increased cost of 0.004 (debug mode)
    /// </summary>
    public class LocalEList {
        public class LEdge { Vector3 p1, p2; public LEdge(Vector3 p1, Vector3 p2) { this.p1 = p1; this.p2 = p2; } }
        public LEdge[] firstedge;
        public List<LEdge>[] otheredges;
        public int[] used;
        const int MAXEE = 5;

        public LocalEList(int n) { firstedge = new LEdge[n]; otheredges = new List<LEdge>[n]; }

        void Add(CSGPrim csg, PolyDecPoint p0, PolyDecPoint p1) {
            LEdge e = new LEdge(p0.point, p1.point);
            int k = p0.othercsg.arrayPos;
            if (firstedge[k] == null) {
                firstedge[k] = e;
            } else {
                if (otheredges[k] == null) otheredges[k] = new List<LEdge>();
                otheredges[k].Add(e);
            }
        }

        public void Add(Poly poly) {
            PolyDecPoint pp = poly.points.Last();
            CSGPrim csg = poly.Csg;
            foreach (PolyDecPoint p in poly.points) {
                if (pp.othercsg != null) Add(csg, pp, p);
                pp = p;
            }
        }
    }
         ***/
#endregion
