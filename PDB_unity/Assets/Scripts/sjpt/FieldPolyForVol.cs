#define noNEWNEW
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;


/// initially part of CSGFIELD rather than pure CSG;
/// but it turned out to be good for non-axially aligned planes as well.
/// The only method externalized from here is "Poly CSGPrim.PolyForVolField(Volume)"
/// which returns the set of Polys for the primitive surface on the volume.
///
/// Most of the internals could be prepared once and saved as a massive array.
///
/// This includes two implementatations for now.
/// if NEWNEW is defined, it uses one derived from the Java where the setup makes all the windings correct.
/// If it is not, it uses the older implementation which does not attempt to setup corerct winding but leaves it till main runtime.
///
/// For some reason, NEWNEW is stopping misnter etc working at all
/// and shows poly for metaballs but then will not interact.
/// Also NEWNEW gives 17 polys for some bm values

namespace CSG {
    public partial class CSGPrim {

        /****
                                                        *
        6       7                                       *
        *---10--*                                       *
       .|      .|                                       *
     11 |     9 |                                       *
     .  6    .  5                                       *
   2*-----8-*3  |                                       *
    |   |   |   |                                       *
    |  4*---|-2-*5                                      *
    7  .    4  .                                        *
    | 3     | 1                                         *
    |.      |.                                          *
   0*---0---*1                                          *
                                                        *

****/

        
        // which edges for each face, all outward facing winding
        static readonly int[,] faceEdge = new int[,]
        { { 1, 0, 3, 2 }, { 0, 4, 8, 7 }, { 1, 5, 9, 4 }, { 5, 2, 6, 10 }, { 3, 7, 11, 6 }, { 8, 9, 10, 11 } };
        //   bot       front      right      back         left        top
        static readonly int bot = 0, front = 1, right = 2, back = 3, left = 4, top = 5;
        #if NEWNEW
        
        // which edges are adjacent to which edges 
        private static int/*byte*/[,] adjEdge = new int/*byte*/[,]
//        {{1,4,3,7    },{5,2,4,0    },{1,5,6,3     },{7,6,2,0     },{8,0,1,9    },{10,2,1,9    },{3,11,10,2    },{3,11,8,0    },{9,11,7,4     },{8,10,5,4     },{5,6,11,9    },{10,8,6,7    }};
        {{1,4,3,7,2,8},{5,2,4,0,9,3},{1,5,6,3,10,0},{7,6,2,0,1,11},{8,0,1,9,5,7},{10,2,1,9,6,4},{3,11,10,2,5,7},{3,11,8,0,4,6},{9,11,7,4,10,0},{8,10,5,4,11,1},{5,6,11,9,8,2},{10,8,6,7,9,3}};
        // 0              1               2             3               4            5                6               7             8                   9            10               11


        // coords for vertices, to help calculate winding
        private static Vector3[] vertCoord = new Vector3 [] {
            new Vector3 (0, 0, 0),
            new Vector3 (1, 0, 0),
            new Vector3 (0, 1, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (0, 0, 1),
            new Vector3 (1, 0, 1),
            new Vector3 (0, 1, 1),
            new Vector3 (1, 1, 1)
        };
        
        // coords for edges, to help calculate winding
        private static Vector3[] edgeCoord = new Vector3[12];

        static CSGPrim () {
            setEdgeFace();
            for (int e = 0; e < 12; e++) {  // I'd forgotten HOW horrible Java is for this
                edgeCoord [e] = (vertCoord [edgeVertex [e,0]] + vertCoord [edgeVertex [e,1]]) * 0.5f;
            }
        }



#else

        #endif
        // which faces for each edge (computed once from faceEdge
        private static readonly int[,] edgeFace = { 
            { front, bot }, { right, bot }, { back, bot }, { left,    bot },
            { front, right }, { right, back }, { back, left }, { left, front },
            { top, front }, { top, right }, { top, back }, { top, left } 
        };




        //static readonly int[,] edgeFace = new int[12,2];
        private static void setEdgeFace() {
            
            for (int e = 0; e < 12; e++) {
                int facenum = 0;  // edgeFace number, 0 or 1
                for (int f = 0; f < 6; f++) {
                    for (int v = 0; v < 4; v++) {
                        if (faceEdge[f, v] == e) {
                            edgeFace[e, facenum] = f;
                            facenum++;
                        }  // match
                    } // v
                } // f
                if (facenum != 2) { 
                    Debug.Break();
                }
            } // e
        }
        // setEdgeFace()

        
        private static readonly int[,] edgeVertex = new int[,] { { 0, 1 }, { 1, 5 }, { 5, 4 }, { 4, 0 }, 
            { 1, 3 }, { 5, 7 }, { 4, 6 }, { 0, 2 },
            { 2, 3 }, { 3, 7 }, { 7, 6 }, { 6, 2 }
        };

        // poly points will be computed on demand for the bit pattern of +ve and -ve vertices
        // it holds polyPoints[bitpattern][polynum][pointWithinPolyNum]
        //
        // for example with bitpattern 00000001; eg just point 0 +ve, we get
        // polyEdges[1] = { {0,3,7} }
        private static int/*byte*/[][][] polyEdges = new int/*byte*/[256][][];

        public static int sharebm;
        // used for debug where wind goes wrong
        /// <summary>
        /// Compute the polygon in for this primitive using field values at the corners.
        /// 
        /// In some cases, more than one polygon must be returned.
        /// The (rare) case is handled by attaching "extraPolys" to the first polygon.
        /// This saves making a list or array for handling the commonest case.
        /// 
        /// Even though created mainly for more complex objects (spheres, fields, etc),
        /// it has proved to be the most efficient implementation even for non-axially aligned planes.
        /// Axially aligned planes have custom code for PolyForVol, so this is not used for those.
        /// </summary>
        /// <param name="vol"></param>
        /// <returns></returns>
        protected Poly PolyForVolField(Volume vol) {
            Vector3[] points = new Vector3[8];
            points[0] = new Vector3(vol.x1, vol.y1, vol.z1);
            points[1] = new Vector3(vol.x2, vol.y1, vol.z1);
            points[2] = new Vector3(vol.x1, vol.y2, vol.z1);
            points[3] = new Vector3(vol.x2, vol.y2, vol.z1);
            points[4] = new Vector3(vol.x1, vol.y1, vol.z2);
            points[5] = new Vector3(vol.x2, vol.y1, vol.z2);
            points[6] = new Vector3(vol.x1, vol.y2, vol.z2);
            points[7] = new Vector3(vol.x2, vol.y2, vol.z2);
            
            float[] vv = new float[8];  // vertex values
            
            // bm summarizes the 'topology' of this fit
            int bm = 0;
            int k = 1;  // bitmap val and val for each pos
            bool[] sign = new bool[8];
            for (int v = 0; v < 8; v++) {
                vv[v] = Dist(points[v]);
                if (vv[v] == 0)
                    vv[v] = delta;
                sign[v] = vv[v] > 0;
                if (sign[v])
                    bm += k;
                k *= 2;
            }
            sharebm = bm;  // to help debug winding
            bool newpolys = false;
            if (polyEdges[bm] == null) {
                newpolys = true;
#if NEWNEW
                setPolyEdgeNEW (bm, sign);
#else
                setPolyEdge(bm, sign);
#endif
            }
            
            if (polyEdges[bm].Length == 0)
                return null;  // all inside or all outside
            if (polyEdges[bm].Length > 4)
                Debug.Break();
            Poly mainpoly = null;  // only poly if just 1, otherwise 1st, with rest tagged on to extraPoly
            for (int polyn = 0; polyn < polyEdges[bm].Length; polyn++) {
                int/*byte*/[] edges = polyEdges[bm][polyn];
                Poly poly = Poly.Make();
                foreach (int e in edges) {
                    int v0 = edgeVertex[e, 0], v1 = edgeVertex[e, 1];  // vertex indices
                    Vector3 xing = (points[v0] * vv[v1] - points[v1] * vv[v0]) / (vv[v1] - vv[v0]);
                    poly.Add(xing);
                }
                if (polyn == 0) {  // polyn 0 is the mainpoly 
                    mainpoly = poly;
                    if (newpolys && this is CSGPlane) {  // very first time generated, check winding
                        if (!(this as CSGPlane).CheckPolyWind(poly)) {
                            polyEdges[bm][polyn] = polyEdges[bm][polyn].Reverse().ToArray();
                            return PolyForVolField(vol);  // use new reversed wind, only happens very occasionally
                        }
                    }
                } else {
                    // optimize, extraPolys not often needed, so only make them at all if they will be used
                    if (polyn == 1)
                        mainpoly.extraPolys = new List<Poly>();  
                    mainpoly.extraPolys.Add(poly);
                }
            }
            return mainpoly;
        }
        // PolyForVolField()

        /*********************************/
        /// <summary>
        /// set up the list of edges involved in polygons for this topology
        /// called lazily as each bm signature encountered
        /// </summary>
        /// <param name="bm"></param>
        /// <param name="?"></param>
        private void setPolyEdge(int bm, bool[] sign) {
            bool[] useedge = new bool[12];
            
            // find which edges will be used
            for (int e = 0; e < 12; e++) {
                useedge[e] = sign[edgeVertex[e, 0]] != sign[edgeVertex[e, 1]];
            }
            
            
            List<int[]> polylist = new List<int[]>();
            while (true) { // for All polys
                // find an edge for a new poly
                int e1 = -99;  // first used edge
                for (int e = 0; e < 12; e++)
                    if (useedge[e]) {
                        e1 = e;
                        break;
                    }
                if (e1 < 0)
                    break;  // no more edges so no more polys
                
                // now walk through the poly
                List<int> edges = new List<int>();
                edges.Add(e1);
                useedge[e1] = false;  // only use once
                int parity = sign[edgeVertex[e1, 0]] ? 1 : 0;
                int face = edgeFace[e1, parity]; 
                //?int firstface = face;
                for (int e = 0;; e++) {  // all edges on this poly
                    //// find another edge on this face
                    int ednuminface = 0;
                    int edge;
                    for (ednuminface = 0;; ednuminface++) {
                        // if (ednuminface >= 4) { TraceN.BREAK(); }  
                        // will be caught in next statement ~ wrong parity or ??? on edge calculations
                        edge = faceEdge[face, ednuminface];   
                        if (useedge[edge])
                            break;
                    }
                    edges.Add(edge);
                    useedge[edge] = false;
                    // move onto the next face
                    face = (face == edgeFace[edge, 0]) ? edgeFace[edge, 1] : edgeFace[edge, 0];
                    if (face == edgeFace[e1, 1 - parity])
                        break;  // end of poly
                } // all edges on this poly
                polylist.Add(edges.ToArray());   // add the poly (as array) to the list of polys
            }
            
            polyEdges[bm] = polylist.ToArray();  // save the list of polys (as array)
        }
        // setPolyEdge
        /****************************/

        #if NEWNEW
        /*
     * Note: only used during setup of each configuration, but called lazily
     * recursively search for potential polygons
     * poly[] as far as depth is the polygon so far
     *    (note:, an array is easier to backtrack than a list)
     * useedge[] tells us which edges are valid for this situation
     * polylist collects resulting polygons.
     */
        private static void morePoly(int/*byte*/[] poly, int depth, bool[] useedge, List<int/*byte*/[]> polylist) {
            int e = poly[depth-1];
            for (int ni = 0; ni < 6; ni++) {      // new edge index
                int/*byte*/ n = adjEdge[e,ni];           // new potential edge
                poly[depth] = n;                // each item in the poly is an edge id
                bool isloop = false;
                for (int o=1; o<depth; o++)        // found a subloop, no good
                    if (n == poly[o]) isloop = true;
                
                // check through the cases
                if (isloop) {                   // loop, so don't continue
                } else if (!useedge[n]) {       // this is not a candidate edge
                } else if (n == poly[0]) {      // completed poly
                    if (depth >= 3) { 
                        int/*byte*/ [] newpoly = new int/*byte*/[depth];
                        //System.arraycopy(poly, 0, newpoly, 0, depth);
                        Array.Copy (poly, newpoly, depth);
                        polylist.Add(newpoly);
                    }
                } else if (n < poly[0]) {       // poly[0] must be lowest to stop duplicates
                } else {
                    poly[depth] = n;
                    morePoly(poly, depth+1, useedge, polylist);
                }
            }
        }


        /**
     * Note: only used during setup of each configuration, but called lazily
     * set up the list of edges involved in polygons for this topology
     *  the polygons are returned converted to triangles
     *  called lazily as each bm signature encountered
     *  </summary>
     *  <param name="bm"></param>
     *  <param name="?"></param>
     * 
     */
        private static void setPolyEdgeNEW(int bm, bool[] sign) {
            //nSetPolyEdges++; Renderer.logc("nSetPolyEdges", nSetPolyEdges);
            bool[] useedge = new bool[12];
            
            // find which edges will be used
            for (int e=0; e<12; e++) {
                useedge[e] = sign[edgeVertex[e, 0]] != sign[edgeVertex[e, 1]];
            }

            List<int/*byte*/[]> polylist = new List<int/*byte*/[]>();
            int/*byte*/[] polyp = new int/*byte*/[13];  
            
            for (int/*byte*/ e1 = 0; e1 < 12; e1++) {  // first edge
                polyp[0] = e1;
                if (useedge[e1]) {
                    morePoly(polyp, 1, useedge, polylist);
                    useedge[e1] = false; // ??? added to reduce absurd 18 polys, but does not help
                }
            }
            
            // convert to triangles and remove excess ones
            bool[] trikeys = new bool[1<<12];
            List<int/*byte*/[]> trilist = new List<int/*byte*/[]>();
            
            foreach (int/*byte*/[] poly in polylist) {                        // poly is array of edge ids
                int/*byte*/ pe1 = poly[0];                                // pe1, pe2, pe3 are three edge ids making a triangle
                for(int ip2=1; ip2 < poly.Length-1; ip2++) {
                    int ip3 = ip2 + 1;
                    int/*byte*/ pe2 = poly[ip2], pe3 = poly[ip3];
                    int trikey = (1<<pe1) + (1<<pe2) + (1<<pe3);
                    if (!trikeys[trikey]) {
                        trilist.Add(wind(pe1,pe2,pe3, sign));
                        trikeys[trikey] = true;
                    } else {
                        //    Debug.Break ();
                    }
                }
            }
            //int/*byte*/[][] triarray = new int/*byte*/[trilist.Count()][];
            int/*byte*/[][] triarray = trilist.ToArray();
            polyEdges[bm] = triarray;  // save the list of polys (as array)
        }


        // compute correct winding.
        // * Cross the difference vectors of representative points.
        // * Find a representative 'outside' point.
        // * Check if it is outside, and reverse the winding if not.
        // * 
        // * This is horrible, there must be a better topological way ????
        // * @param pe1
        // * @param pe2
        // * @param pe3
        // * @return

        private static int/*byte*/[] wind (int/*byte*/ pe1, int/*byte*/ pe2, int/*byte*/ pe3, bool[] sign) {
            Vector3 v1 = edgeCoord [pe1];
            Vector3 v2 = edgeCoord [pe2];
            Vector3 v3 = edgeCoord [pe3];
            Vector3 v1x = v1 - v2;
            Vector3 v3x = v3 - v2;
            Vector3 cross = Vector3.Cross (v1x, v3x);
            Vector3 v2x = v2 + cross;
            int x = v2x.x > 0.5 ? 1 : 0;
            int y = v2x.y > 0.5 ? 1 : 0;
            int z = v2x.z > 0.5 ? 1 : 0;
            int e = 1 * x + 2 * y + 4 * z; 
            
            return sign [e] ? new int/*byte*/[] {pe1, pe3, pe2} : new int/*byte*/[] {pe1, pe2, pe3};
        }  // wind
#endif
    }
    // class CSGPrim
}
  // namespace CSG




/**
        static CSGPrim() {
            
            for (int e = 0; e < 12; e++) {
                int facenum = 0;  // edgeFace number, 0 or 1
                for (int f = 0; f < 6; f++) {
                    for (int v = 0; v < 4; v++) {
                        if (faceEdge[f, v] == e) {
                            edgeFace[e, facenum] = f;
                            facenum++;
                        }  // match
                    } // v
                } // f
                if (facenum != 2) { 
                    //TraceN.BREAK(); //pjt Unity
                    //TODO:::: SOMETHING!!!
                }
            } // e
        } // CSGPrim()
**/
