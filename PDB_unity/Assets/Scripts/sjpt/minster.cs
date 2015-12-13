//#if(false) cc
using UnityEngine;
using System.Collections;

using CSG;
using CSGNonPlane;

public static class CSGXX {
	public static Color[] colors = new [] {
		Color.grey,
		Color.blue,
		Color.green,
		Color.cyan,
		Color.red,
		Color.magenta,
		Color.yellow,
		Color.white
	};

	public static CSGNode Scale(this CSGNode incsg, double s) {
		return incsg.Scale((float)s);
	}

	public static bool useColour = true;

	public static CSGNode Colour(this CSGNode incsg, double Color) {
		return incsg.Texture(Color + "_col");
	}

	public static CSGNode Texture(this CSGNode incsg, string Texture) {
		// compiles under MonoDevelop with provenance below, but not under Unity
		return incsg.TNode(new Bakery("texture", texture: Texture /* , provenance: Texture */));
	}


	//	public static CSGNode Colour(this CSGNode incsg, double Color) {
	//		if (!useColour)
	//			return incsg;
	//		int c = (int)Color;
	//		//if (c == 6)
	//		//	GUIBits.Log ("col 6");
	//		incsg = incsg.WithColTexture("" + c);
	//		incsg = incsg.WithProvenance("pcol" + c);
	//		if (c >= colors.Length)
	//			c = c % 10;
	//		incsg.Color = colors[c];
	//		return incsg;
	//	}

	public static CSGNode Gloss(this CSGNode incsg, double Color) {
		return incsg;
	}

	public static CSGNode Xrot(this CSGNode cthis, double rot) {
		return cthis.RotateX(rot);
	}

	public static CSGNode Yrot(this CSGNode cthis, double rot) {
		return cthis.RotateY(-rot);
	}

	public static CSGNode Zrot(this CSGNode cthis, double rot) {
		return cthis.RotateZ(-rot);
	}

}

[ExecuteInEditMode]
public class Minster : MonoBehaviour {

	public static int CWalls = 6;
	public static int CRoof = 2;
	public static float Over = 0.1f;

	public static CSGNode Cube(double x, double y, double z) {
		return S.Box(0, 0, 0, (float)x, (float)y, (float)z);
	}

	public static CSGNode Block(double x, double y, double z) {
		return S.Box(0, 0, 0, (float)x, (float)y, (float)z);
	}

	public static CSGNode Plane(double x, double y, double z) {
		return S.Plane((float)x, (float)y, (float)z, 0, S.DefaultBakery);
	}

	public static CSGNode Sphere(double r) {
		return Empty; // S.Sphere ((float)r);
	}

	static CSGNode basecyl;
	public static int CylNum = -1;
	private static int lastCylNum = -999;

	/** Make a Winsom compatible Cylinder.
	 * If CylNum > 0 use 4*CylNum facets
	 * If CylNum == 0 ignore
	 * If CylNum < 0 return 'real' cylinder
	 * */
	public static CSGNode Cylinder(double r, double x, double y, double z) {

		if (CylNum > 0) {
			Bakery prov = S.DefaultBakery;  // set as Cylinder for sensible vertex normals
			// do not use prov for now as it does not get processed correctly by rotate, etc, etc
			//prov = new CSGNonPlane.Cylinder(0,0,0, (float)x, (float)y, (float)z, (float)r, true, false);
			//prov = null;
			CSGNode c;
			if (basecyl == null || CylNum != lastCylNum) {  // caching cylinders does make a significant difference
				lastCylNum = CylNum;
				CSGNode p1 = S.Plane(1, 0, 0, -1, prov);
				c = S.ALL;
				int n = 4 * CylNum;
				for (int i = 0; i < 360; i += 360 / n) {
					c = c * p1.RotateY(i);
					//GUIBits.Log ("cyl plane " + i);
				}
				//GUIBits.Log ("cyl nodes " + c.Nodes ().Count);
				basecyl = c;
			}

			c = basecyl.Scale((float)r);
			c = c * S.XPlane(0, 1, 0, 0);
			Vector3 a = new Vector3((float)x, (float)y, (float)z);
			c = c * S.XPlane(0, -1, 0, -a.magnitude);

			//Vector3 b = new Vector3 (0, 0, 1);
			//Vector3 c = Vector3.Cross (a,b).Normalize();
			if (x == 0 && y == 0 && z > 0)
				return c.RotateX(90);
			else if (x == 0 && y == 0 && z < 0)
				return c.RotateX(-90);
			else if (x == 0 && y > 0 && z == 0)
				return c.RotateX(180);
			else if (x == 0 && y < 0 && z == 0)
				return c.RotateX(0);
			else if (x > 0 && y == 0 && z == 0)
				return c.RotateZ(-90);
			else if (x < 0 && y == 0 && z == 0)
				return c.RotateZ(90);
			else {
				GUIBits.Log("nonaxial cylinder not supported " + x + " " + y + " " + z);
				return S.NONE;
			}
		} else if (CylNum < 0) {
			return new CSGNonPlane.Cylinder(0, 0, 0, (float)x, (float)y, (float)z, (float)r, false).Flat();
		} else {
			return S.NONE;
		}

		//return S.NONE;
		//return new CSGNonPlane.Cylinder(0,0,0, (float)x, (float)y, (float)z, (float)r, false, false) ;
	}

	public static CSGNode Cone(double x, double y, double z, double r1, double r2) {
		return S.NONE;
	}

	public static CSGNode Torus(double x, double y, double z, double r1, double r2) {
		return S.NONE;
	}

	public static CSGNode Wedge(double x, double y, double z, double r1, double r2,
	                            double qx, double qy, double qz, double qr1, double qr2) {
		return S.NONE;
	}

	public static CSGNode Empty = S.NONE;
	//	CSGNode BOXFOUND(double x, double y, double z, double w) {
	//		return S.Box (0,0,0, (float)x, (float)y, (float)z);
	//	}

	/*** converted from ESME to C# */
	/* DOC: MINSTER: show final version of Minster */
	/* This MODEL file shows the final version of the Minster            */
	/* ------------------------------------------------------------------ */
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<DETAIL(0)      */
	/* function to select DETAIL on minster */
	public static int DETAILlevel = 10;
	/* make sure level declared and initialized */

	//;{ "DETAIL" function #
	//;obj =  ? CSGNode;
	//;level =  ? double;
	public static CSGNode DETAIL(CSGNode obj, string prov, double level) { 
		obj.SetProvenance(prov);
		return level < DETAILlevel ? obj : Empty;
	}
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<BOXFOUND(0)      */
	/* Generates a box of given length, height, width and thickness */
	/* Centred along the z-axis                                     */

	/* parse arg thick , l , w , h .  */
	/* l and w give distance from centre of one wall to centre of   */
	/* opposite wall                                                */
	//;{ "BOXFOUND" function #
	//;thick =  ? double;
	//;l =  ? double;
	//;w =  ? double;
	//;h =  ? double;
	public static CSGNode BOXFOUND(double thick, double l, double w, double h) {
	
		if (thick < 0)
			thick = -thick;
		if (l < 0)
			l = -l;
		if (w < 0)
			w = -w;
		if (h < 0)
			h = -h;
	
		double temp = -(w + thick) / 2;
		CSGNode boxnew = (Cube(l + thick, h, w + thick)
		                 - (Cube(l - thick, h, w - thick).At(thick, 0, thick)))
		.At(0, 0, temp);

		return DETAIL(boxnew, "box", 1);
	}
	//; };
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<STRIP(0)      */
	/* Generates a strip of given length,height and depth     */
	/* Centred along z-axis                                   */

	/* parse arg thick , l , h . */
	//;{ "STRIP" function #
	//;thick =  ? double;
	//;l =  ? double;
	//;h =  ? double;
	public static CSGNode STRIP(double thick, double l, double h) {
	
		if (thick < 0)
			thick = -thick;
		if (l < 0)
			l = -l;
		if (h < 0)
			h = -h;
	
		CSGNode stripnew = Cube(l, h, thick).At(0, 0, 0 - thick / 2);
		return stripnew;
	
	}
	//;};
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<WWORKF(0)      */
	/* Draw the westworks foundations                              */

	/* parse arg fdepth . */
	//;{ "WWORKF" function #
	//;fdepth =  ? double;
	public static CSGNode WWORKF(double fdepth) {
	
		if (fdepth < 0)
			fdepth = -fdepth;
		double temp = -fdepth;
	
		/* St. Martins Tower                                          */
		CSGNode wworkfnew = 
			(BOXFOUND(0.83, 6.17, 10.17, fdepth).At(730.9, temp, 500.2))
			
		/* North and South Nave walls                                  */
			
			+ (STRIP(1.33, 20.5, fdepth).At(737.9, temp, 505.6335))
			+ (STRIP(1.33, 20.5, fdepth).At(737.9, temp, 495.3650))
			
		/* North and South walls                                       */
			
			+ (STRIP(1.33, 22, fdepth).At(731.23, temp, 510.535))
			+ (STRIP(1.33, 22, fdepth).At(731.23, temp, 489.865))
			
		/* Cross walls on North                                        */
			
			+ (STRIP(4.17, 1.33, fdepth).At(731.23, temp, 507.785))
			+ (STRIP(4.17, 1.33, fdepth).At(736.57, temp, 507.785))
			+ (STRIP(4.17, 1.00, fdepth).At(741.90, temp, 507.785))
			+ (STRIP(4.17, 1.00, fdepth).At(746.90, temp, 507.785))
			+ (STRIP(4.17, 1.33, fdepth).At(751.90, temp, 507.785))
			
		/* Cross walls on South                                        */
			
			+ (STRIP(4.17, 1.33, fdepth).At(731.23, temp, 492.615))
			+ (STRIP(4.17, 1.33, fdepth).At(736.57, temp, 492.615))
			+ (STRIP(4.17, 1.00, fdepth).At(741.90, temp, 492.615))
			+ (STRIP(4.17, 1.00, fdepth).At(746.90, temp, 492.615))
			+ (STRIP(4.17, 1.33, fdepth).At(751.90, temp, 492.615));
		return wworkfnew;
	
	}
	//;};
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<CHURCHF(0)      */
	/* Draw the 7th Century church together with facades and chapels  */
	/* foundations only!                                              */

	/* parse arg fdepth . */
	//;{ "CHURCHF" function #
	//;fdepth =  ? double;
	public static CSGNode CHURCHF(double fdepth) {
	
		if (fdepth < 0)
			fdepth = -fdepth;
		double temp = -fdepth;
	
		/* Nave                                                           */
		CSGNode churchfnew = 
			(BOXFOUND(0.66, 21.35, 10.34, fdepth).At(758.4, temp, 500.2))
			
		/* North and South portici                                        */
			
			+ (BOXFOUND(0.66, 4.84, 5.5, fdepth).At(773.065, temp, 508.12))
			+ (BOXFOUND(0.66, 4.84, 5.5, fdepth).At(773.065, temp, 492.28))
			
		/* Facades                                                        */
			
			+ (STRIP(9.45, 2.44, fdepth).At(761.46, temp, 510.425))
			+ (STRIP(9.45, 2.44, fdepth).At(761.46, temp, 489.975))
			
		/* Chapels                                                        */
		/* East walls                                                     */
			
			+ (STRIP(8.81, 0.91, fdepth).At(768.17, temp, 510.105))
			+ (STRIP(8.81, 0.91, fdepth).At(768.17, temp, 490.295))
			
		/* Cross walls                                                    */
			
			+ (STRIP(0.91, 4.27, fdepth).At(763.90, temp, 514.085))
			+ (STRIP(0.91, 4.27, fdepth).At(763.90, temp, 511.335))
			+ (STRIP(0.91, 4.27, fdepth).At(763.90, temp, 508.745))
			+ (STRIP(0.91, 4.27, fdepth).At(763.90, temp, 491.655))
			+ (STRIP(0.91, 4.27, fdepth).At(763.90, temp, 489.065))
			+ (STRIP(0.91, 4.27, fdepth).At(763.90, temp, 486.315));
	
		return churchfnew;
	}
	//: };
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<DEE(0)      */
	/* Generates a dee of given radius, thickness and height  */
	/* Centred along the z-axis                               */
	/* Radius measured from centre to outside edge of dee     */
	/* l measured from back of back wall to centre of dee     */


	/* parse arg thick , l , h , rad , back . */
	//;{ "DEE" function #
	//;thick =  ? double;
	//;l =  ? double;
	//;h =  ? double;
	//;rad =  ? double;
	//;back =  ? double;
	public static CSGNode DEE(double thick, double l, double h, double rad, double back) {
	
		if (thick < 0)
			thick = -thick;
		if (l < 0)
			l = -l;
		if (h < 0)
			h = -h;
		if (rad < 0)
			rad = -rad;
	
		double r1 = 2 * rad;
		double r2 = 2 * (rad - thick);
		CSGNode deediff;
		if (back == 1)
			deediff = ((Cube(l - thick, h, r2).At(thick, 0, thick))
			+ ((Cylinder(r2 / 2, 0, h, 0) * Plane(-1, 0, 0))
		       .At(l, 0, rad)));
		else
			deediff = ((Cube(l, h, r2).At(0, 0, thick))
			+ ((Cylinder(r2 / 2, 0, h, 0) * Plane(-1, 0, 0))
		       .At(l, 0, rad)));
		CSGNode deenew = (
		                     (Cube(l, h, r1)
		                     + ((Cylinder(rad, 0, h, 0) * Plane(-1, 0, 0)).At(l, 0, rad)))
		                     - deediff).At(0, 0, 0 - rad);
	
		return DETAIL(deenew, "dee", 1);
	}
	//;};
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<EASTENDF(0)      */
	/* Draw the 10th century eastern addition foundations              */

	/* parse arg fdepth . */
	//;{ "EASTENDF" function #
	//;fdepth =  ? double;
	public static CSGNode EASTENDF(double fdepth) {
	
		if (fdepth < 0)
			fdepth = -fdepth;
		double temp = -fdepth;
	
		CSGNode eastendfnew = 
			(DEE(0.66, 14.61, fdepth, 5.5, 1).At(779.74, temp, 500.20))
			+ (DEE(0.66, 2.94, fdepth, 3.89, 1)
			       .Yrot(-90).At(783.63, temp, 505.04))
			+ (DEE(0.66, 2.94, fdepth, 3.89, 1)
			       .Yrot(90).At(783.63, temp, 495.36))
			
		/* Aussenkryta                                                    */
			
			+ (STRIP(0.66, 5.07, fdepth).At(798.67, temp, 503.28))
			+ (STRIP(0.66, 5.07, fdepth).At(798.67, temp, 497.12))
			+ (STRIP(5.5, 0.66, fdepth).At(803.08, temp, 500.20));
	
		return eastendfnew;
	}
	//;};
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<WEG(0)      */
	/* A Wedge, centred on the z-axis                   */

	/* parse arg thick "," w "," h . */
	//;{ "Weg" function # thick =  ? double;w =  ? double;h =  ? double;
	public static CSGNode Weg(double thick, double w, double h) {
		if (thick < 0)
			thick = -thick;
		if (w < 0)
			w = -w;
		if (h < 0)
			h = -h;
	
		CSGNode Weg = (Cube(thick, h, w)
		              * Plane(0, w / 2, 0 - h)
		              * (Plane(0, w / 2, h).At(0, 0, w)))
		.At(0, 0, 0 - w / 2);
	
		return DETAIL(Weg, "weg", 1);
	}
	//;};
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<ROOF(0)      */
	/* Generates a roof : conical, with hips or standard             */
	/* Centred on z-axis                                             */
	/* In the case of the conical roof l is the distance to the      */
	/* centre of the Cone from other } of roof                     */
	/* We assume that there is a 10 cm overhang                      */


	/* parse arg thick "," l "," w "," h "," rooftype "," rest */
	//;{ "ROOF" function # thick =  ? double; l =  ? double;w =  ? double;h =  ? double;
	//;   rooftype =  ? sref;colwalls =  ? double;colroof =  ? double;over =  ? double;
	public static CSGNode ROOF(double thick, double l, double w, double h, string rooftype, double colwalls, double colroof, double over) {
		/* colwalls == 6 */           /* Set default wall and roof colours     */
		/* colroof == 2  */
		/* over == 0.1   */       /* Assume a 10cm overhang                */
	
		/* if ( rest != "" ) interpret rest */
	
		if (thick < 0)
			thick = -thick;
		if (l < 0)
			l = -l;
		if (w < 0)
			w = -w;
		if (h < 0)
			h = -h;
	
		double len = l + 2 * (over + thick / 2);
		double width = w + 2 * (over + thick / 2);
		/* Calculate vertical roof thickness                            */
		double rt = (2 * h * (thick + over)) / width;
		CSGNode roofnew = Empty;
	
		{ /* select */
		
			if (rooftype == "std") {
				double dummy = len + 0.2;
				roofnew = 
				(Weg(len, width, h)
				- (Weg(dummy, width, h).At(-0.1, 0 - rt, 0)))
					.Colour(colroof) +
				(((Weg(thick, width, h).At(over, 0, 0)) +
				(Weg(thick, width, h).At(len - thick - over, 0, 0)))
					 .Colour(colwalls));
			}
			;
		
			if (rooftype == "con") {
				roofnew = 
				((Weg(l + over, width, h) +
				((Cone(width / 2, 0, 0, h, 0) * Plane(-1, 0, 0))
				 .At(l + over, 0, 0)))
				- /*diff*/
				((Weg(l + 2 * over, width, h) +
				((Cone(width / 2, 0, 0, h, 0) * Plane(-1, 0, 0))
				 .At(l + 2 * over, 0, 0)))
				 .At(0 - over, 0 - rt, 0))).Colour(colroof)
				+ (Weg(thick, width, h).At(over, 0, 0)
					       .Colour(colwalls));
			}
			;
		
			if (rooftype == "hip" | rooftype == "hiphip" |
			    rooftype == "hipp" | rooftype == "hipped") { /* hip etc */
				double y = 9999;
			
				if (rooftype == "hip" | rooftype == "hiphip")
					y = width / 2;
				if (rooftype == "hipp" | rooftype == "hipped")
					y = len / 2;
			
				roofnew = Weg(len, width, h) * Plane(0 - h, y, 0);
				if (rooftype == "hiphip" | rooftype == "hipped")
					roofnew = roofnew * (Plane(h, y, 0).At(len, 0, 0));
			
				CSGNode roofdiff = Weg(len, width, h) * Plane(0 - h, y, 0);
				if (rooftype == "hiphip" | rooftype == "hipped")
					roofdiff = roofdiff * (Plane(h, y, 0).At(len, 0, 0));
			
				roofnew = roofnew - (roofdiff.At(0, 0 - rt, 0));
				roofnew = roofnew.Colour(colroof);
			
				if (rooftype == "hip" | rooftype == "hipp")
					roofnew = roofnew +
					(Weg(thick, width, h).At(len - thick - over, 0, 0).Colour(colwalls));
			}
			; /* do hip etc */
		
			/* otherwise say ERROR in rooftype - rooftype unrecognised */
		
		}
		; /* } select */
	
		return DETAIL(roofnew, "roof", 3);
	}
	//;}; /* roof */


	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<HOUSE(0)      */
	/* Generates a house with walls of a given thickness       */
	/* Centred along z-axis                                    */


	//;{ "HOUSE" function #
	//;thick =  ? double;
	//;l =  ? double;
	//;w =  ? double;
	//;h1 =  ? double;
	//;h2 =  ? double;
	//;rooftype =  ? sref;
	//;fh =  ? double;   // floor height
	//;colwalls =  ? double;
	//;colroof =  ? double;
	//;over =  ? double;
	public static CSGNode HOUSE(string prov, double thick, double l, double w, double h1, double h2, string rooftype, double fh, double colwalls, double colroof, double over) {
		/* colwalls =  6;      Set default wall and roof colours    */
		/* colroof =  2; */
		/* over =  0.1;     Assume a 10 cm overhang                 */
	
		/* if ( rest ^= "" ) interpret rest */
	
		if (thick < 0)
			thick = -thick;
		if (l < 0)
			l = -l;
		if (w < 0)
			w = -w;
		if (h1 < 0)
			h1 = -h1;
		if (h2 < 0)
			h2 = -h2;
	
		/* l,w give distance from centre of one wall to centre of */
		/* opposite wall.  In case of conical house, l is the     */
		/* length from back of wall to centre of dee              */
	
		/* fh gives the thickness of the floor in the house.If   */
		/* fh is 0 ) the house has no floor.                  */
	
		if (fh < 0)
			fh = 0.05; /* standard  floor thickness is 0.05 */
		CSGNode newhouse = Empty;
	
		if (rooftype == "con") { /* con */
			double dummy = (w + thick) / 2;
		
			newhouse = 
			DEE(thick, l, h1, dummy, 0).Colour(colwalls)
			+ (ROOF(thick, l, w, h2, rooftype, colwalls, colroof, over)
				       .At(0 - over, h1, 0));
		
			/*  the base of houses with conical shaped rooves     */
			/*  note programme is like dee model                  */
		
			if (fh != 0) {              /* if floorht is not 0 */  /* is floor */
				double p = 2 * dummy;
			
				// 10 Dec 2015 issues with base/wall rendering with both real and faceted cylinders
				// either main (above fh) house can be moved up (increasing its height)
				// or cut off at fh to disambiguate
				// or base slightly extended so it is seen non-ambiguously (resolved faceted as well)
				//
				// HOWEVER, still errors in CSG code with wrong colours (faceted) or missing bits (real)
				// as none of these should be needed other than to disabgiguate base colour
				float xrad = 0;
				xrad = 0.01f;						// ONE OF ... added stephen 10 Dec 2015 to ensure correct
				newhouse = (newhouse
				// .At (0, fh, 0) 				// OR ... added stephen 10 Dec 2015 to ensure correct 
				// * Plane(0,-1,0).At (0,fh,0)	// OR ... added stephen 10 Dec 2015 to ensure correct 
				) + (
				    (Block(l, fh, p))/* square end of base */
				    + (Cylinder(dummy + xrad, 0, fh, 0)/* curved end */
				    * Plane(-1, 0, 0)).At(l, 0, dummy)
				)
					.At(0, 0, 0 - dummy)
					.Colour(2);
			
			}
			; /* end floor */
		
		}  /* end rooftype con */
	
	else {  /* rooftype not con */
			double dummy = (w + thick) / 2;
		
			newhouse = 
			BOXFOUND(thick, l, w, h1).Colour(colwalls)
			+ (ROOF(thick, l, w, h2, rooftype, colwalls, colroof, over)
				       .At(0 - over, h1, 0));
		
			/* the base of houses with std shaped rooves  */
		
			if (fh != 0)
				newhouse = newhouse
				+ (Block(l + thick, fh, w + thick).At(0, 0, 0 - dummy))
				.Colour(2);
		
		}
		; /* rooftype not con */
	
		/* note if floor Colour is different to wall Colour then we have */
		/*  problems, when arches cut into walls , with Arch Colour       */
	
		return DETAIL(newhouse, "house" + rooftype + "_" + prov, 2);
	}
	//;};   /* exit */

	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<TOWER(0)      */
	/* Draw the staged Tower                                             */
	public static CSGNode Tower() {
		return DETAIL((
		    ((HOUSE("tower1", 0.66, 7.12, 7.12, 12.33, 6.44, "hiphip", 0, 6, 2, 0)
		    * (Plane(0, 1, 0).At(0, 15.55, 0))).At(-3.89, 0, 0))
		    +
		    ((HOUSE("tower2", 0.66, 3.23, 3.23, 3.89, 3.22, "hiphip", 0, 6, 2, 0)
		    * (Plane(0, 1, 0).At(0, 5.5, 0))).At(-1.945, 15.55, 0))
		    +
		    (HOUSE("tower3", 0.66, 1.29, 1.29, 1.95, 0.81, "hiphip", 0, CWalls, CRoof, Over)
 .At(-0.975, 21.05, 0))
		    ///*+ (BELL().Scale(0.3)  .At (785.245, 12.33, 501.815))          */
		    ///*+ (BELL().Scale(0.3)  .At (785.245, 12.33, 498.585))          */
		    ///*+ (BELL().Scale(0.3)  .At (782.015, 12.33, 501.815))          */
		    ///*+ (BELL().Scale(0.3)  .At (782.015, 12.33, 489.585))          */
		), "tower", 1);
	}

	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<CLEANOUT(0)      */
	/* Clear out all overhanging rooves, too wide walls etc.             */


	/* A */

	/* all overhanging roofs from the side buildings are cleared from    */
	/* the space which is to form the minster Altar room . Further       */
	/* the overhanging roofs extend  only half the way into the Altar    */
	/* room walls.This avoids the red roof outlines which would be       */
	/* obtained if the side buildings lay flush with the Inside of the   */
	/* walls of the Altar room of the minster.                           */


	public static CSGNode Cleanout() {
		return (
		    (
		
		        (
		            Cube(35.29, 11, 10.34)
		            + (Cylinder(5.17, 0, 11, 0).At(35.29, 0, 5.17))
		        )
		.At(759.06, 0.05, 495.03)
		
		
		/* B */
		
		/* overhang in other areas in the minster */
		
		/* West end                                                      */
		/* Small Towers                                                  */
		
		        + (Cube(4.01, 27, 4.17).At(732.56, 0.05, 505.70))
		        + (Cube(4.01, 27, 4.17).At(732.56, 0.05, 490.53))
		
		/* Large Tower                                                   */
		
		        + (Cube(9.67, 25, 8.34).At(742.90, 0.05, 496.03))
		
		/* Overlap in side houses                                        */
		
		        + (Cube(14, 11, 4.17).At(737.90, 0.05, 505.70))
		        + (Cube(14, 11, 4.17).At(737.90, 0.05, 490.53))
		
		    )
	
	.Colour(86)  // was 6, quick experiment with 4
	
		).WithProvenance("cleanout"); 
	}
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<ARCH(0)      */
	/* Generate an Arch, centred on z-axis                    */

	/* parse arg thick "," l "," h . */
	//;{ "Arch" function #
	//;thick =  ? double;
	//;l =  ? double;
	//;h =  ? double;
	public static CSGNode Arch(double thick, double l, double h) {
	
		/* h is the distance from the bottom to top of straight bit */
	
		if (thick < 0)
			thick = -thick;
		if (l < 0)
			l = -l;
		if (h < 0)
			h = -h;
	
		var k = 1;
		var at = thick * (1 - k) / 2;
		CSGNode archnew;
		archnew = (Cube(thick, h, l).At(0, 0, 0 - l / 2))
		+ (Cylinder(l / 2, thick, 0, 0).At(0, h, 0));

		// allowing below increases csg generation, eg from 1.84 to 3.75
		// but caching cylinders reduces it to 0.87
		// Almost all CSG generation goes into Cylinder ???
		//archnew = (Cube (thick*k, h, l).Colour (4) .At (at, 0, 0 - l / 2))
		//	+  (Cylinder (l / 2, thick*k, 0, 0) .At (at, h, 0));
		//archnew = S.NONE;
			
		return DETAIL(archnew, "arch", 4);  // sjpt colour added for winding debug
	}
	//;};
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<WINDOW(0)      */
	/* Generate a double Window, centred along the z-axis         */

	/* parse arg thick , l , h . */
	//;{ "Window" function #
	//;thick =  ? double;
	//;l =  ? double;
	//;h =  ? double;
	public static CSGNode Window(double thick, double l, double h) {
	
		/* h is distance from bottom to top of straight bit           */
		/* thick is thickness of large Arch                           */
	
		if (thick < 0)
			thick = -thick;
		if (l < 0)
			l = -l;
		if (h < 0)
			h = -h;
	
		double dummy = l * 3 / 7;
		double temp = l * 2 / 7;
	
		CSGNode windnew = Arch(thick, l, h) +
		                  (Arch(thick / 2, dummy, h).At(thick, 0, temp)) +
		                  (Arch(thick / 2, dummy, h).At(thick, 0, 0 - temp));
	
		return DETAIL(windnew, "window", 6);
	}
	//;};
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<WINS(0)      */
	/* Put in all the windows                                            */
	public static CSGNode Wins() { 
		//;	( //;begin  /* all windows */
	 
		/* Simple arches on St. Martins Tower                               */
		CSGNode nwin = (Cylinder(0.9, 0.83, 0, 0).At(730.90, 15.0, 500.20));
	 
		//;i =  -1;
		for (int i = -1; i <= 1; i++) { //;do(3, /* i == -1 to 1 */
			//j =  0;  /* 1 */
			for (int j = 0; j < 2; j++) { //;do(3, /* j == 0 to 2 */
				if ((i == 0) && (j == 0)) {
				} else
					nwin = nwin + (Arch(0.83, 0.5, 2.0)
			                         .At(730.90, 1.3 + j * 4.4, 500.20 + i * 3.5));
				//;j =  j + 1;
			}
			;
			//;i =  i + 1;
		}
		;  /*i loop */
	
		/* Simple arches on small towers                                     */
	
		//;j =  0;  /* 2 */
		for (int j = 0; j <= 3; j++) { //;do(4, /* j == 0 to 3 */
			//;i =  -1;
			for (int i = -1; i <= 1; i++) { //; do(3, /* i == -1 to 1 */
				if (i == 0) {
				} else {
					nwin = nwin + (Arch(1.53, 0.5, 1.0)
				                    .At(731.13, 1.0 + j * 5.5, 500.20 + i * 9.37))
					+ (Arch(1.53, 0.5, 1.0)
					       .At(731.13, 3.75 + j * 5.5, 500.20 + i * 6.0));
				}
				; /* if */
				//;i =  i + 1;
			}//;);
			//;j =  j + 1;
		}//;);
	
		//#i =  -1;
		for (int i = -1; i <= 1; i++) { //;do(3, /* i == -1 to 1 */
			if (i == 0) {
			} else {
				//;j =  0;
				for (int j = 0; j <= 3; j++) { //;do(4, /* j == 0 to 3 */
					nwin = nwin + (Arch(1.53, 0.5, 1.0).Yrot(0 - i * 90)
				                    .At(736.07, 1.0 + j * 5.5, 500.20 - i * 11.1))
					+ (Arch(1.53, 0.5, 1.0).Yrot(0 - i * 90)
					       .At(733.06, 3.75 + j * 5.5, 500.20 - i * 11.1));
					j = j + 1;
				} //;);
			}
			; /* if  else */
			//;i =  i + 1;
		}//;); /* i loop */
	
		//;i =  -1;
		for (int i = -1; i <= 1; i++) { //; do(3, /* i == -1 to 1 */
			if (i == 0) {
			} else {
				nwin = nwin
				+ (Arch(1.53, 0.5, 1.0).At(736.47, 14.75, 500.20 + i * 9.37))
				+ (Arch(1.53, 0.5, 1.0).At(736.47, 20.25, 500.20 + i * 9.37))
				+ (Arch(1.53, 0.5, 1.0).At(736.47, 17.50, 500.20 + i * 6.0));
			}
			; /* if  */
			//;i =  i + 1;
		}//;; /* i loop */
		
		/* Double windows in small towers                                    */
		
		nwin = nwin + (Window(1.0, 2.22, 1.7).At(731.13, 22.63, 492.615))
		+ (Window(1.0, 2.22, 1.7).Yrot(180).At(738.00, 22.63, 492.615))
		+ (Window(1.0, 2.22, 1.7).At(731.13, 22.63, 507.785))
		+ (Window(1.0, 2.22, 1.7).Yrot(180).At(738.00, 22.63, 507.785))
		+ (Window(1.0, 2.22, 1.7).Yrot(-90).At(734.565, 22.63, 489.10))
		+ (Window(1.0, 2.22, 1.7).Yrot(-90).At(734.565, 22.63, 504.27))
		+ (Window(1.0, 2.22, 1.7).Yrot(90).At(734.565, 22.63, 496.13))
		+ (Window(1.0, 2.22, 1.7).Yrot(90).At(734.565, 22.63, 511.30));
		
		/* Double windows in Westworks                                       */
		
		//;k =  0;
		//;do(3, /* k == 0 to 2 */
		//;  j =  0;
		//;   do(2, /* j == 0 to 1 */
		//;     i =  -1;
		//;      do(3, /* i == -1 to 1 */
		for (int k = 0; k <= 2; k++) {	  
			for (int j = 0; j <= 1; j++) {
				for (int i = -1; i <= 1; i++) {
					if (i == 0) {
					} else {
						nwin = nwin + (Window(1.0, 1.5, 2.0).Yrot(0 - i * 90)
						                    .At(740.4 + k * 4.5, 1.5 + j * 5.5, 500.20 - i * 11.1));
					}
					; /* if  */
					//;i =  i + 1;
				}//;); /* i loop */
				//;j =  j + 1;
			}//;); /* j loop */
			//;k =  k + 1;
		}//; ); /* k loop */
		
		
		/* Big double windows in Tower                                       */
		
		nwin = nwin + (Window(1.0, 3.0, 2.5).Yrot(90)
		                    .At(745.18, 19, 505.80))
		+ (Window(1.0, 3.0, 2.5).Yrot(-90).At(745.18, 19, 494.60))
		+ (Window(1.0, 3.0, 2.5).Yrot(90).At(750.29, 19, 505.80))
		+ (Window(1.0, 3.0, 2.5).Yrot(-90).At(750.29, 19, 494.60))
		+ (Window(1.0, 3.0, 2.5).At(741.47, 19, 497.867))
		+ (Window(1.0, 3.0, 2.5).At(741.47, 19, 502.534))
		+ (Window(1.0, 3.0, 2.5).Yrot(180).At(754.00, 19, 497.867))
		+ (Window(1.0, 3.0, 2.5).Yrot(180).At(754.00, 19, 502.534))
				
		/*Window in bit that joins East to West                              */
				
		+ (Window(1.0, 2.0, 2.2).Yrot(90).At(757.345, 6.6, 505.80))
		+ (Window(1.0, 2.0, 2.2).Yrot(-90).At(757.345, 6.6, 494.60));
		
		/* Arches in facades and chapels                                     */
		
		//;j =  0;
		//;do(2, /* j == 0 to 1 */
		//;  i =  -1;
		//;   do(3, /* i == -1 to 1 */
		for (int j = 0; j <= 1; j++) {
			for (int i = -1; i <= 2; i++) {
				if (i == 0) {
				} else {
					nwin = nwin + (Arch(1.11, 0.5, 2.0)
					                    .At(761.36, 2 + j * 4, 500.20 + i * 6.92))
					+ (Arch(1.11, 0.5, 2.0)
						       .At(761.36, 2 + j * 4, 500.20 + i * 9.84))
					+ (Arch(1.11, 0.5, 2.0)
							       .At(761.36, 2 + j * 4, 500.20 + i * 12.51))
					+ (Arch(1.11, 0.5, 2.0)
							       .At(768.07, 2 + j * 4, 500.20 + i * 6.92))
					+ (Arch(1.11, 0.5, 2.0)
							       .At(768.07, 2 + j * 4, 500.20 + i * 9.84))
					+ (Arch(1.11, 0.5, 2.0)
							       .At(768.07, 2 + j * 4, 500.20 + i * 12.51));
				}
				; /* if */
				//;i =  i + 1;
			}//;); /* i loop */
			//;j =  j + 1;
		}//;); /* j loop*/
		
		/* Side windows                                                      */
		
		nwin = nwin + (Arch(1.11, 0.5, 2.0).Yrot(90)
		                    .At(762.68, 2.0, 486.01))
		+ (Arch(1.11, 0.5, 2.0).Yrot(90).At(762.68, 6.0, 486.01))
		+ (Arch(1.11, 0.5, 2.0).Yrot(90).At(766.49, 2.0, 486.87))
		+ (Arch(1.11, 0.5, 2.0).Yrot(90).At(766.49, 6.0, 486.87))
		+ (Arch(1.11, 0.5, 2.0).Yrot(-90).At(762.68, 2.0, 514.39))
		+ (Arch(1.11, 0.5, 2.0).Yrot(-90).At(762.68, 6.0, 514.39))
		+ (Arch(1.11, 0.5, 2.0).Yrot(-90).At(766.49, 2.0, 513.53))
		+ (Arch(1.11, 0.5, 2.0).Yrot(-90).At(766.49, 6.0, 513.53))
				
		/* Windows in portici                                                */
				
		+ (Arch(0.86, 0.5, 1.7).At(772.965, 1.9, 491.95))
		+ (Arch(0.86, 0.5, 1.7).At(772.965, 1.9, 508.45))
		+ (Arch(0.86, 0.5, 1.7).At(777.805, 1.9, 491.95))
		+ (Arch(0.86, 0.5, 1.7).At(777.805, 1.9, 508.45))
		+ (Arch(0.86, 0.5, 1.7).Yrot(90).At(775.815, 1.9, 489.96))
		+ (Arch(0.86, 0.5, 1.7).Yrot(-90).At(775.815, 1.9, 510.44))
				
		/* Tower windows                                                     */
				
		/* First stage                                                       */
				
		+ (Arch(0.86, 5.0, 3.0).At(779.64, 16.5, 500.2))
		+ (Arch(0.86, 5.0, 3.0).At(786.76, 16.5, 500.2))
		+ (Arch(0.86, 5.0, 3.0).Yrot(90).At(783.63, 16.5, 497.07))
		+ (Arch(0.86, 5.0, 3.0).Yrot(-90).At(783.63, 16.5, 503.33))
				
		/* Small arches                                                      */
				
		+ (Arch(0.86, 0.7, 1.0).At(779.64, 21, 502.73))
		+ (Arch(0.86, 0.7, 1.0).At(779.64, 21, 497.67))
		+ (Arch(0.86, 0.7, 1.0).At(786.76, 21, 502.73))
		+ (Arch(0.86, 0.7, 1.0).At(786.76, 21, 497.67))
				
		/* Second stage                                                      */
				
		+ (Arch(0.86, 0.7, 1.4).At(781.585, 27.445, 500.20))
		+ (Arch(0.86, 0.7, 1.4).At(784.815, 27.445, 500.20))
		+ (Arch(0.86, 0.7, 1.4).Yrot(90).At(783.63, 27.445, 499.015))
		+ (Arch(0.86, 0.7, 1.4).Yrot(-90).At(783.63, 27.445, 501.385))
		+ (Cylinder(0.35, 4.09, 0, 0).At(781.585, 28.5, 501.035))
		+ (Cylinder(0.35, 4.09, 0, 0).At(781.585, 28.5, 499.365))
		+ (Arch(0.86, 0.35, 0.7).At(782.555, 32.5, 500.20))
		+ (Arch(0.86, 0.35, 0.7).At(783.845, 32.5, 500.20))
		+ (Arch(0.86, 0.35, 0.7).Yrot(90).At(783.63, 32.5, 499.985))
		+ (Arch(0.86, 0.35, 0.7).Yrot(-90).At(783.63, 32.5, 500.415))
				
		/* Windows in apses                                                  */
				
		+ (Arch(1.5, 1.0, 2.0).Yrot(90).At(783.63, 3.5, 489.29))
		+ (Arch(1.5, 1.0, 2.0).Yrot(-90).At(783.63, 3.5, 511.11))
		+ (Arch(1.5, 2.0, 2.5).At(799.09, 5.875, 500.20))
				
		/* Aussenkrypta                                                      */
				
		+ (Arch(0.86, 2.50, 0.0).At(802.98, -0.4, 500.20))
				
		/* Door                                                              */
				
		+ (Arch(0.83, 3.5, 2.5).At(730.90, 0.05, 500.20));
		
		return nwin;
		//;) //;end;
	}

	public static CSGNode WinX() {
		/* First stage windows  */
		
		return (Arch(0.86, 5.0, 3.0).At(779.64, 16.5, 500.2))
		+ (Arch(0.86, 5.0, 3.0).At(786.76, 16.5, 500.2))
		+ (Arch(0.86, 5.0, 3.0).Yrot(90).At(783.63, 16.5, 497.07))
		+ (Arch(0.86, 5.0, 3.0).Yrot(-90).At(783.63, 16.5, 503.33));
	}

	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<WINDOW(0)      */
	/* Generate a double Window, centred along the z-axis         */
				
	/* parse arg thick , l , h . */
	//;{ "Window" function #
	//;thick =  ? double;
	//;l =  ? double;
	//;h =  ? double;
	CSGNode windowSECOND(double thick, double l, double h) {
			
		/* h is distance from bottom to top of straight bit           */
		/* thick is thickness of large Arch                           */
			
		if (thick < 0)
			thick = -thick;
		if (l < 0)
			l = -l;
		if (h < 0)
			h = -h;
			
		double dummy = l * 3 / 7;
		double temp = l * 2 / 7;
			
		CSGNode windnew = Arch(thick, l, h) +
		                  (Arch(thick / 2, dummy, h).At(thick, 0, temp)) +
		                  (Arch(thick / 2, dummy, h).At(thick, 0, 0 - temp));
			
		return DETAIL(windnew, "windowSECOND", 6);
	}
	//;};
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<WININ(0)      */
	/*arches Inside minster*/
		
	/*arches in large Tower*/
	public static CSGNode Winin() {
		return ( 
		    //;begin
		    Arch(1.5, 3.5, 2.5).At(741.57, 0.06, 500.2)
		    + (Arch(1.5, 3.5, 2.5).At(752.57, 0.76, 500.2))
				
		/* 2nd floor arches in large Tower */
				
		    + (Arch(1.5, 2, 1.5).At(741.57, 7.5, 500.2))
		    + (Arch(1.5, 2, 1.5).At(752.57, 7.5, 500.2))
				
		/*Arch in St. Martins Tower*/
		    + (Arch(1.5, 3.5, 2.5).At(736.4, 0.06, 500.2))
				
		/*arches in the St. Martins Inside walls*/
				
		    + (Arch(0.84, 2, 2.2).Yrot(-90).At(733.985, 0.06, 501.95))
		    + (Arch(0.84, 2, 2.2).Yrot(-90).At(733.985, 0.06, 497.62))
				
		/*Arch in bit that joins east to west*/
		    + (Arch(2.0, 5, 6).At(758.4, 0.06, 500.2))
				
		/*arches in side of large Tower*/
				
		    + (Arch(2.0, 3.5, 2.5).Yrot(-90).At(749.32, 0.76, 504.035))
		    + (Arch(2.0, 3.5, 2.5).Yrot(-90).At(749.32, 0.76, 494.535))
				
		/* 2nd floor arches in large Tower   */
				
		    + (Arch(2.0, 2, 1.5).Yrot(-90).At(749.32, 7.5, 504.035))
		    + (Arch(2.0, 2, 1.5).Yrot(-90).At(749.32, 7.5, 494.535))
				
		/*arches in portici*/
				
		    + (Arch(1.3, 3, 2).Yrot(-90).At(775.815, 0.4, 505.04))
		    + (Arch(1.3, 3, 2).Yrot(90).At(775.815, 0.4, 495.36))
				
		/*arches in the apses*/
				
		    + (Arch(1.3, 3, 2).Yrot(-90).At(783.63, 1.0, 505.04))
		    + (Arch(1.3, 3, 2).Yrot(90).At(783.63, 1.0, 495.36))
				
		/*arches in the facades*/
				
		    + (Arch(1, 1, 2.2).Yrot(-90).At(762.68, 0.06, 505.04))
		    + (Arch(1, 1, 2.2).Yrot(90).At(762.68, 0.06, 495.36))
				
		/*arches in the chapels*/
				
		    + (Arch(1, 2, 2.2).At(762.99, 0.05, 512.71))
		    + (Arch(1, 2, 2.2).At(762.99, 0.05, 510.04))
		    + (Arch(1, 2, 2.2).At(762.99, 0.05, 506.995))
		    + (Arch(1, 2, 2.2).At(762.99, 0.05, 487.69))
		    + (Arch(1, 2, 2.2).At(762.99, 0.05, 490.36))
		    + (Arch(1, 2, 2.2).At(762.99, 0.05, 493.405))
				
		/*arches in ausenkrypta*/
		    + (Arch(2.5, 1, 0.7).At(798.19, 0.05, 498.66))
		    + (Arch(2.5, 1, 0.7).At(798.19, 0.05, 501.74))
				
		/*arces in west end towers*/
				
		    + (Arch(1.5, 2, 2.2).At(736.57, 0.76, 508.805))
		    + (Arch(1.5, 2, 2.2).At(736.57, 0.76, 491.935))
		).WithProvenance("winin");
	}
	//; end;  /* Winin */
		
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<BUILDING(0)      */
	/* Draw all the Building - starting .At the east end                 */
		
		
	/* the Altar room (Building) in the minster , which contains the    */
	/* Altar. It extends from the middle of the minster to the east end */
	public static CSGNode Building() {
		return (
		    //;BEGIN
		    (HOUSE("build1", 0.66, 35.95, 10.34, 11, 4.55, "con", -1, CWalls, CRoof, Over)
				 .At(758.40, 0, 500.2))
				
				
		    +
				
				
		/* the side buildings( apses, portici, facades, and chapels)to the  */
		/* Altar room, and east and west end buildings.                     */
				
		    (
					
		        /* Apses                                                         */
					
		        (HOUSE("apse1", 0.66, 2.94, 7.12, 7.5, 3.5, "con", 1, CWalls, CRoof, Over).Yrot(-90)
				 .At(783.63, 0, 505.04))
		        + (HOUSE("apse2", 0.66, 2.94, 7.12, 7.5, 3.5, "con", 1, CWalls, CRoof, Over).Yrot(90)
				       .At(783.63, 0, 495.36))
					
		/* Portici                                                       */
					
		        + (HOUSE("portico1", 0.66, 5.5, 4.84, 5.5, 2.39, "std", 0.4, CWalls, CRoof, Over).Yrot(-90)
				       .At(775.815, 0, 505.04))
		        + (HOUSE("portico2", 0.66, 5.5, 4.84, 5.5, 2.39, "std", 0.4, CWalls, CRoof, Over).Yrot(90)
				       .At(775.815, 0, 495.36))
					
		/* Facades                                                      */
					
		        + (HOUSE("facade1", 0.66, 9.45, 1.78, 10, 1, "std", -1, CWalls, CRoof, Over).Yrot(-90)
				       .At(762.68, 0, 505.04))
		        + (HOUSE("facade2", 0.66, 9.45, 1.78, 10, 1, "std", -1, CWalls, CRoof, Over).Yrot(90)
				       .At(762.68, 0, 495.36))
					
		/* Chapels - North                                              */
					
		        + (HOUSE("chapeln1", 0.91, 5.18, 2.75, 9, 1, "std", -1, CWalls, CRoof, Over)
				       .At(762.99, 0, 512.71))
		        + (HOUSE("chapeln2", 0.91, 5.18, 2.59, 9, 1, "std", -1, CWalls, CRoof, Over)
				       .At(762.99, 0, 510.04))
		        + (HOUSE("chapeln3", 0.91, 5.18, 3.50, 9, 1, "std", -1, CWalls, CRoof, Over)
				       .At(762.99, 0, 506.995))
					
		/* Chapels - South                                              */
					
		        + (HOUSE("chapels1", 0.91, 5.18, 2.75, 9, 1, "std", -1, CWalls, CRoof, Over)
				       .At(762.99, 0, 487.69))
		        + (HOUSE("chapels2", 0.91, 5.18, 2.59, 9, 1, "std", -1, CWalls, CRoof, Over)
				       .At(762.99, 0, 490.36))
		        + (HOUSE("chapels3", 0.91, 5.18, 3.50, 9, 1, "std", -1, CWalls, CRoof, Over)
				       .At(762.99, 0, 493.405))
					
		/* Aussenkrypta                                                 */
					
					
				+ (HOUSE("Aussenkrypta", 0.66, 7.12, 6.16, 1.0, 3.5, "hipped", -1, CWalls, CRoof, Over)
				       .At(795.96, 0, 500.20))
					
					
		/* Staged Tower                                                 */
					
		        + (Tower().At(783.63, 11, 500.20))
					
		/* West end                                                     */
		/* Towers                                                       */
					
		        + (HOUSE("wtower1", 1.33, 5.34, 5.5, 27, 2.66, "hiphip", -1, CWalls, CRoof, Over)
				       .At(731.23, 0, 507.785))
		        + (HOUSE("wtower2", 1.33, 5.34, 5.5, 27, 2.66, "hiphip", -1, CWalls, CRoof, Over)
				       .At(731.23, 0, 492.615))
					
		/* Main House -                                                 */
					
		        + (HOUSE("main", 1.33, 15.33, 20.67, 11, 7.58, "std", -1, CWalls, CRoof, Over)
				       .At(736.57, 0, 500.20))
					
		/* St. Martins Tower - so that it joins the other house        */
					
		        + (HOUSE("stmarttower", 0.83, 6.17, 10.17, 14.756, 3.824, "std", -1, CWalls, CRoof, Over)
				       .At(730.90, 0, 500.20))
					
		/* Large Tower                                                  */
					
		        + (HOUSE("largetower", 1.33, 11, 9.67, 25, 4.78, "hiphip", -1, CWalls, CRoof, Over)
				       .At(741.57, 0, 500.20))
					
		/* Bit that joins East to West                                  */
					
		        + (HOUSE("ewjoin", 1.33, 5.83, 9.67, 11, 4.55, "std", -1, CWalls, CRoof, Over)
				       .At(752.57, 0, 500.20))
					
					
		/* Clear out all overhanging rooves, wide walls etc.               */
		/* Cleanout() is applied to the side buildings of the Altar room   */
		/* but not to the Altar room itself, otherwise we would get some   */
		/* of the Altar room walls erased which we do not want. For a      */
		/* further explaination see Cleanout() .                           */
					
		        - Cleanout()
					
		    )
				
		/* the arches and windows are added.                            */
				
				
		    - (Wins().Colour(16))
				
		    - (Winin().Colour(26))
		).WithProvenance("building");
				
	}
	//;END;
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<TOWER(0)      */
	/* Draw the staged Tower                                             */
	static CSGNode towerSECOND() {
		return DETAIL((
		    ((HOUSE("towerS1", 0.66, 7.12, 7.12, 12.33, 6.44, "hiphip", 0, 6, 2, 0)
		    * (Plane(0, 1, 0).At(0, 15.55, 0))).At(-3.89, 0, 0))
		    +
		    ((HOUSE("towerS2", 0.66, 3.23, 3.23, 3.89, 3.22, "hiphip", 0, 6, 2, 0)
		    * (Plane(0, 1, 0).At(0, 5.5, 0))).At(-1.945, 15.55, 0))
		    +
		    (HOUSE("towerS3", 0.66, 1.29, 1.29, 1.95, 0.81, "hiphip", 0, CWalls, CRoof, Over)
		 .At(-0.975, 21.05, 0))
		    ///*+ (BELL().Scale(0.3)  .At (785.245, 12.33, 501.815))          */
		    ///*+ (BELL().Scale(0.3)  .At (785.245, 12.33, 498.585))          */
		    ///*+ (BELL().Scale(0.3)  .At (782.015, 12.33, 501.815))          */
		    ///*+ (BELL().Scale(0.3)  .At (782.015, 12.33, 489.585))          */
		), "towerSECOND", 1);
	}
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<TOMHS(0)      */
	/* this is the same model file as house(), except that    */
	/* it generatesdifferent coloured walls and roofs,        */
	/* which we want for St Swithin"s tomb .                  */
		
		
	/* Generates a house with walls of a given thickness       */
	/* Centred along z-axis                                    */
		
	/*parse arg thick "," l "," w "," h1 "," h2 "," rooftype "," fh ","rest*/
	//;{ "TOMHS" function #
	//;thick =  ? double;
	//;l =  ? double;
	//;w =  ? double;
	//;h1 =  ? double;
	//;h2 =  ? double;
	//;rooftype =  ? sref;
	//;fh =  ? double;
	//;colwalls =  ? double;
	//;colroof =  ? double;
	//;over =  ? double;
	public static CSGNode TOMHS(double thick, double l, double w, double h1, double h2, string rooftype, double fh, double colwalls, double colroof, double over) {
			
		/* colwalls == 7       Set default wall and roof colours    */
		/*colroof == 6        */
		/* over == 0.1      Assume a 10 cm overhang                 */
			
		/* if ( rest != "" ) interpret rest*/
			
		if (thick < 0)
			thick = -thick;
		if (l < 0)
			l = -l;
		if (w < 0)
			w = -w;
		if (h1 < 0)
			h1 = -h1;
		if (h2 < 0)
			h2 = -h2;
			
		/* l,w give distance from centre of one wall to centre of */
		/* opposite wall.  In case of conical house, l is the     */
		/* length from back of wall to centre of dee              */
			
		/* fh gives the thickness of the floor in the house.If   */
		/* fh is 0 ) the house has no floor.                  */
			
		if (fh < 0)
			fh = 0.05;  /* standard  floor thickness is 0.05 */
		CSGNode roofnew = Empty;
			
			
		if (rooftype == "con") {
			double dummy = (w + thick) / 2;
			roofnew = 
					DEE(thick, l, h1, dummy, 0).Colour(colwalls)
			+ (ROOF(thick, l, w, h2, rooftype, colwalls, colroof, over)
						       .At(0 - over, h1, 0));
				
			/*  the base of houses with conical shaped rooves     */
			/*  note programme is like dee model                  */
				
			if (fh != 0                   /* if ( floorht is not 0 */) {
					
				double p = 2 * dummy;
					
				roofnew = roofnew +
				(
				    (Block(l, fh, p))/* square end of base */
				    + (Cylinder(dummy, 0, fh, 0)/* curved end */
				    * Plane(-1, 0, 0))
							.At(l, 0, dummy)
				)
							.At(0, 0, 0 - dummy)
							.Colour(36);
					
			}
			;
				
		} else {
			double dummy = (w + thick) / 2;
			roofnew = BOXFOUND(thick, l, w, h1).Colour(colwalls)
			+ (ROOF(thick, l, w, h2, rooftype, colwalls, colroof, over)
					       .At(0 - over, h1, 0));
				
			/* the base of houses with std shaped rooves  */
				
			if (fh != 0) {
				roofnew = roofnew
				+ (Block(l + thick, fh, w + thick).At(0, 0, 0 - dummy))
							.Colour(46);
					
			}
			;
				
		}
		;
		/* note if ( floor Colour is different to wall Colour ) we have */
		/*  problems, when arches cut into walls , with Arch Colour       */
		return DETAIL(roofnew, "tomhs", 6);
	}
	//;};  /* tomhs */
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<TOMGATE(0)    */
	/* creates a  grill  gate for St Swithins tomb     */
		
	/* creates a  grill    */
	public static CSGNode Tomgate() {
		return 
			
			(
				
		    (
				
		        (Cylinder(0.040, 0, 0, 2).At(0, 0.0, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 0.2, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 0.4, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 0.6, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 0.8, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 1.0, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 1.2, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 1.4, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 1.6, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 1.8, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 2.0, -1))
		        + /*union*/ (Cylinder(0.040, 0, 0, 2).At(0, 2.2, -1))
		        + /*union*/ (Cylinder(0.040, 0, 2, 0).At(0, 0, -0.7))
		        + /*union*/ (Cylinder(0.040, 0, 2, 0).At(0, 0, -0.5))
		        + /*union*/ (Cylinder(0.040, 0, 2, 0).At(0, 0, -0.3))
		        + /*union*/ (Cylinder(0.040, 0, 2, 0).At(0, 0, -0.1))
		        + /*union*/ (Cylinder(0.040, 0, 2, 0).At(0, 0, 0.1))
		        + /*union*/ (Cylinder(0.040, 0, 2, 0).At(0, 0, 0.3))
		        + /*union*/ (Cylinder(0.040, 0, 2, 0).At(0, 0, 0.5))
		        + /*union*/ (Cylinder(0.040, 0, 2, 0).At(0, 0, 0.7))
				
		    )
				
				
		/* intersection with an Arch gets the right gate shape   */
				
				
		    * (Arch(0.2, 1.2, 1.5).At(-0.1, 0, 0))
				
		)
				
				.Colour(5)
				
				.Gloss(1);
	}
	/* end Tomgate */
		
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<STEPS(0)      */
	/* This model file creates Steps with parameters :-                   */
	/*                                                                    */
	/*          l    - Length of Steps in the X direction                 */
	/*          h    - Total height of Steps in the Y direction           */
	/*          w    - Width of Steps in the Z direction                  */
	/*          s    - Distance between start of first step and start     */
	/*                 of last step in the X direction                    */
	/*          n    - Number of Steps                                    */
	/*                                                                    */
	/* The ( X, Y, Z ) coordinates are specified .At the centre of the base*/
	/* .At the front of the Steps                                          */
	/*                                                                    */
	/* parse arg l","h","w","s","n","rest */
	//;{ "Steps" function #
	//;l =  ? double;
	//;h =  ? double;
	//;w =  ? double;
	//;s =  ? double;
	//;n =  ? double;
	public static CSGNode Steps(double l, double h, double w, double s, double n) {
			
		/* if ( rest != "" ) interpret rest */
		double thickblock = h / n;
		double stepdist = s / (n - 1);
		CSGNode stepsnew = Empty;
		//;i =  0;
		for (int i = 0; i < n; i++) { //;do(n,
			stepsnew = stepsnew
			+ (Block((l - (i * stepdist)), thickblock, w)
					       .Colour(1))
						.At((i * stepdist), (i * thickblock), (-w / 2));
			//;i =  i + 1;
		}//;);
		return stepsnew;
	}
	//;};  /* end Steps */
		
		
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<PILLAR(0)      */
	/* this file creates the pillars used for the alter         */
		
	public static CSGNode Pillar() {
		return (
			
		    /* plain Pillar with no decorations         */
			
		    Cylinder(0.1, 0, 2.5, 0).At(0, 0.0, 0)
			
		/* upper decorations on Pillar              */
			
		    + /*union*/  Torus(0.1, 0.01, 0, 1, 0).At(0, 2.2, 0)
		    + /*union*/  Torus(0.15, 0.01, 0, 1, 0).At(0, 2.49, 0)
		    + /*union*/  Cone(0.15, 0.1, 0, -0.3, 0).At(0, 2.5, 0)
			
		/* lower decorations                        */
			
		    + /*union*/  Torus(0.1, 0.01, 0, 1, 0).At(0, 0.3, 0)
		    + /*union*/  Cone(0.15, 0.1, 0, 0.3, 0).At(0, 0, 0)
			
		); 
	}
	/* Pillar */
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<ALTAR(0)      */
	/* the Altar, Altar base, canopy, table and cup are constructed */
		
	public static CSGNode Altar() {
		return (
		    Block(2.4, 0.3, 2.4).At(0, 0, 0)/*  base */
		    + Pillar().At(0.3, 0.3, 0.3)/*  pillars */
		    + Pillar().At(2.1, 0.3, 0.3)
		    + Pillar().At(0.3, 0.3, 2.1)
		    + Pillar().At(2.1, 0.3, 2.1)
		/*  2 levels added onto Altar base       */
			
		    + Block(1.6, 0.1, 1.6).At(0.4, 0.3, 0.4)
		    + Block(1.2, 0.1, 1.2).At(0.6, 0.4, 0.6)
			
		/* base of Altar table                   */
			
		    + Cylinder(0.2, 0, 0.6, 0).At(1.2, 0.5, 1.2)
			
		/* table top                             */
			
		    + Block(1.2, 0.07, 1.2).At(0.6, 1.1, 0.6)
			
		/*  Altar canopy is constructed */
			
		    +
			
		    ((Sphere(1.68).At(0, -0.9, 0)
		    - Sphere(1.56).At(0, -0.9, 0))
		    * Plane(0, -1, 0).At(0, 0, 0)
		    * Plane(-1, 0, 0).At(-1, 0, 0)
		    * Plane(1, 0, 0).At(1, 0, 0)
		    * Plane(0, 0, -1).At(0, 0, -1)
		    * Plane(0, 0, 1).At(0, 0, 1))
			.At(1.2, 2.8, 1.2)
			
			
			
		)
			
			.Colour(7)
				.At(-1.2, 0, -1.2);       /* THE Altar,BASE,CANOPY,AND TABLE */
		/* centered .At (0,y,0)             */
	}
	/* end Altar */
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<RFSUPP(0)      */
	/* timber supports for the roof */
		
	/* parse arg xpos , rest */
	//;{ "Rfsupp" function #
	//;xpos =  ? double;
	public static CSGNode Rfsupp(double xpos) {
			
		CSGNode rfsuppnew = 
			(
			    (
			        (Block(3.8, 0.3, 0.3).Zrot(45)).At(0, 3, 0)
			        + Block(0.3, 6, 0.3)
			        - Plane(1, 0, 0)
			    )
					.At(-5.17, -6, -0.15)
					
			    +
					
			    (
			        (Block(3.8, 0.3, 0.3).Zrot(45)).At(0, 3, 0)
			        + Block(0.3, 6, 0.3)
			        - Plane(1, 0, 0)
			    )
					.Yrot(180)
					.At(5.17, -6, -0.15)
					
			    +
					
			    (Block(10.34, 0.3, 0.3))
					.At(-5.17, -0.3, -0.15)
					
					
			).WithProvenance("rfsupp")
					
					.Yrot(90)
					.Colour(3)
					
					.At(xpos, 11, 500.2);
		return rfsuppnew;
	}
	//;};  /* Rfsupp */
		
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<INSIDE(0)      */
	/* all the Inside ornaments, buildings, and raised floors etc */
	/* contained in the Minster.      */
		
	public static CSGNode Inside() {
		return 
			DETAIL(
			(
				
				
			    /* St. Swithins Tomb*/
				
			    (
				
				
			        /* the house for the tomb,without ornaments  */
				
			        (TOMHS(0.05, 2.35, 1.6, 2, 1, "std", -1, CWalls, CRoof, Over))
				
				
		/* the floor in St Swithins Tomb extends beneath Ground level   */
				
			        - (Block(2.25, 0.3, 1.5).WithProvenance("sts_tomb_floor").At(0.05, -0.25, 0.05))
				
				
		/*  Arch and gate into the tomb house         */
				
			        - (Arch(0.2, 1.2, 1.5).WithProvenance("tomb_artch").At(2.30, 0, 0))
			        + (Tomgate().At(2.35, 0, 0))
				
		/*  lower side ornaments in the form of little Steps  */
				
			        + (Block(2.35, 0.1, 0.2).At(0, 0, 0.80).Colour(7))
			        + (Block(2.35, 0.1, 0.1).At(0, 0.1, 0.80).Colour(7))
			        + (Block(2.35, 0.1, 0.2).At(0, 0, -1.0).Colour(7))
			        + (Block(2.35, 0.1, 0.1).At(0, 0.1, -0.90).Colour(7))
				
		/*    upper side ornaments                  */
				
			        + (Block(2.35, 0.1, 0.1).At(0, 1.9, -0.90).Colour(7))
			        + (Block(2.35, 0.1, 0.1).At(0, 1.9, 0.80).Colour(7))
				
		/*   lower and upper ornaments to the back of the tomb house */
				
			        + (Block(0.2, 0.1, 2.0).At(-0.2, 0, -1.0).Colour(7))
			        + (Block(0.1, 0.1, 1.8).At(-0.1, 0.1, -0.90).Colour(7))
			        + (Block(0.1, 0.1, 1.8).At(-0.1, 1.9, -0.90).Colour(7))
				
				
				
				
			    ).WithProvenance("tomb")
				
			    /* the entire tomb is positioned in the west end   */
				
				.At(748.07, 0.80, 500.2)
				
				
				
		/* Steps leading up to St Swithins tomb         */
				
			    + (Steps(4, 0.75, 3.25, 3, 4).At(743.57, 0.05, 500.2))
				
		/* raised floor in west end                                          */
				
			    + (Block(6.5, 0.75, 9.67).At(751.9, 0.05, 495.365).Colour(2))
			    + (Block(20.67, 0.75, 5.25).At(731.23, 0.05, 505.04).Colour(2))
			    + (Block(20.67, 0.75, 5.25).At(731.23, 0.05, 489.87).Colour(2))
			    + (Block(4.33, 0.75, 20.67).At(747.57, 0.05, 489.87).Colour(2))
				
		/* Steps between St. Martins Tower and the large Tower              */
				
			    + (Steps(1.5, 0.75, 4.5, 0.665, 3).Yrot(-90).At(739.32, 0.05, 503.535))
			    + (Steps(1.5, 0.75, 4.5, 0.665, 3).Yrot(90).At(739.32, 0.05, 496.865))
				
		/* Steps leading towards east end                                    */
				
			    + (Steps(1.33, 0.75, 3.5, 0.665, 2).Yrot(180).At(759.73, 0.05, 500.2))
				
		/* raised floors in east end                       */
				
			    + (Block(12.5, 0.35, 11).At(767.8, 0.05, 494.7).Colour(2))
			    + (Block(10, 0.3, 7.68).At(770.3, 0.4, 496.36).Colour(2))
			    + (Block(7.5, 0.3, 2).At(772.8, 0.7, 499.2).Colour(2))
				
		/* Coffin in the East End   */
				
			    +
			    (
				
			        Wedge(0.25, 0, 0, 0, 0.5, 0, 0, 0.25, 0, 3)
			        * Plane(0, 0, 1).At(0, 0, 1.5)
				
			    )
				
				.Yrot(110)
				.At(775, 1.15, 501.0)
				.Colour(7)
				
				
		/* raised floor in far east end                    */
				
			    +
			    (
			        (Block(14.05, 1, 11))/* 14.05 by 11 by 1 metre thick */
			        +
			        (
			            (
			                Cylinder(5.5 - 0.01, 0, 1, 0)// -0.01 stephen 10 Dec 2015, cyl disambiguate
			                * Plane(-1, 0, 0)
			            )
				.At(14.05, 0, 5.5)       /* half-moon bit .At R.H. End */
			        )
			    ).WithProvenance("far east raised floor")
				.At(0, 0, -5.5)       /* base + half-moon centred on z-axis */
				.At(780.3, 0, 500.2)  /* .At r.h.end of chancel */
				.Colour(2)
				
		/* step onto raised far east end floor            */
				
			    + (Block(0.6, 0.3, 1.66).At(779.7, 0.4, 494.7).Colour(2))
			    + (Block(0.6, 0.3, 1.66).At(779.7, 0.4, 504.04).Colour(2))
				
		/* base for alter                                   */
				
			    + (Block(3.5, 2.3, 7.68).At(780.3, 0, 496.36).Colour(2))
		/* main part of base */
			    +
			    (
			        (Cylinder(3.84, 0, 2.3, 0).At(783.8, 0, 500.2))
			        - (Block(3.9, 2.3, 7.7).At(786.3, 0, 496.36))
			        - (Block(6.00, 2.3, 7.68).At(777.8, 0, 496.36))
			    )                          /* a section of a Cylinder */
				.Colour(2)/* forming east end of base */
				
		/* Steps to the alter                               */
				
			    + (Steps(1.5, 1, 2, 1.2, 4).Yrot(180).At(787.8, 1, 500.2))
			    + (Steps(1.5, 1, 2, 1.2, 4).At(778.8, 1, 500.2))
				
		/* the Altar is placed between the Apses           */
				
			    + Altar()
				
				.At(783.63, 2.3, 500.2)
				
		/*  the crypt room                                 */
				
			    - (Block(4, 2, 6).At(781.5, 0.05, 497).Colour(56))
				
				
		/*  the entrances to the crypt                     */
		/* hollow blocks into which Steps are placed       */
				
			    - (Block(6, 1, 1).At(786.3, 0.05, 501.7).Colour(66))
			    - (Block(6, 1, 1).At(786.3, 0.05, 497.7).Colour(66))
				
		/* arches into crypt  */
				
			    - (Arch(1, 1, 1.2).At(785.4, 0.05, 498.2).Colour(66))
			    - (Arch(1, 1, 1.2).At(785.4, 0.05, 502.2).Colour(66))
				
				
		/*  Steps down into the crypt  */
				
			    + (Steps(6, 0.95, 1, 5, 6).At(786.3, 0.05, 498.2))
			    + (Steps(6, 0.95, 1, 5, 6).At(786.3, 0.05, 502.2))
				
		/*  walls by the Steps to the crypt  */
				
		/* leftmost wall  */
				
			    + (Block(6, 0.5, 0.1).At(786.3, 1, 497.6).Colour(1))
				
		/* left of centre wall */
				
			    + (Block(6, 0.5, 0.1).At(786.3, 1, 498.7).Colour(1))
				
		/* right of centre wall */
				
			    + (Block(6, 0.5, 0.1).At(786.3, 1, 501.6).Colour(1))
				
				
		/* rightmost wall */
				
			    + (Block(6, 0.5, 0.1).At(786.3, 1, 502.7).Colour(1))
				
		/* two sets of entrances from the main part of the Minster into  */
		/* the aussenkrypta,consisting of Steps,and arches placed in 2   */
		/* hollowed out blocks.                                          */
				
			    - (Block(1, 0.95, 1).At(797.19, 0.05, 498.16).Colour(76))
			    - (Block(1, 0.95, 1).At(797.19, 0.05, 501.24).Colour(76))
			    - (Arch(2.5, 1, 1.5).At(798.19, 0.05, 498.66).Colour(76))
			    - (Arch(2.5, 1, 1.5).At(798.19, 0.05, 501.74).Colour(76))
			    + (Steps(3.5, 0.95, 1, 3.4, 5).Yrot(180).At(800.69, 0.05, 498.66))
			    + (Steps(3.5, 0.95, 1, 3.4, 5).Yrot(180).At(800.69, 0.05, 501.74))
				
		/* A well is added to the East End of the Minster */
				
			    +
			    (
			        Cylinder(0.6, 0, 1.6, 0)
			        - Cylinder(0.5, 0, 1.6, 0)
			    )
				.Colour(1)
				.At(796, 0, 500.2)
				
		/* high up roof supports are added from the middle of the minster  */
		/* east end.                                                       */
				
			    + Rfsupp(780.6)
			    + Rfsupp(786.6)
				
		/* 2nd floor of large Tower */
				
			    + (Block(9.7, 0.1, 8.3).At(743.55, 6.0, 496.02).Colour(2))
				
				
			), "inside", 5);   
	}
	/* end Inside */
		
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<MINST(0)      */
	/*                                                                   */
	public static CSGNode Ground() {
		return (Plane(0, 1, 0).At(0, -0.1, 0).Colour(4));
	}

	public static CSGNode Found() {
		return (Ground() + WWORKF(1) + CHURCHF(1) + EASTENDF(1));
	}

	public static CSGNode Minst() {
		basecyl = null; // in case of changed CylNum
		return (Building() + Inside() + (Arch(0.53, 3.5, 2.5)
		                                     .At(731.2, 0, 500.2).Colour(0))); //.WithProvenance ("minst");
	}

	public static CSGNode AllMinster() {
		return (
		    (//	/* (Ground + Found) .At (-720.00, 0,-460.20) */
		        /* +/*UNION* / */  (Minst().At(-720.00, 15.0, -460.20)))
			.Scale(0.011) /* .Yrot(30) .Xrot(-25) .At (0.15, 0.3, 0.7 )*/
		);
	}
	/* end */
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<WINFN(0)      */
		
	/* This MODEL file shows the final version of the Minster            */
	/*                                                                   */
	public static CSGNode Winfn() {
		return Building() + Inside();
	}
	/* ------------------------------------------------------------------ */
	/* --------------<<<<<<<BUILDER(0)      */
	//; buildtr =  .At(-720, 40 , -460) Scale (1/100);
	public static CSGNode builder() {
		return Winfn().At(-720, 40, -460).Scale(1 / 100);
	}
	//;buildtr;

	public static CSGNode test() {
		return Block(1, 1, 1);
	}
	// + Ground;

}
//#endif