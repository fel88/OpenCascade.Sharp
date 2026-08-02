using OCCPort.Common;
using System.Reflection.Metadata;
using TKG2d;
using TKMath;

namespace TKGeomBase
{
    //! Computes the bounding box for a curve in 2d .
    //! Functions to add a 2D curve to a bounding box.
    //! The 2D curve is defined from a Geom2d curve.
    public class BndLib_Add2dCurve
    {

        //! Adds to the bounding box B the curve C
        //! B is then enlarged by the tolerance value Tol.
        //! Note: depending on the type of curve, one of the following
        //! representations of the curve C is used to include it in the bounding box B:
        //! -   an exact representation if C is built from a line, a circle or a conic curve,
        //! -   the poles of the curve if C is built from a Bezier curve or a BSpline curve,
        //! -   if not, the points of an approximation of the curve C.
        public static void Add(Geom2d_Curve aC2D, double aTol, Bnd_Box2d aBox2D)
        {
            double aT1, aT2;
            //
            aT1 = aC2D.FirstParameter();
            aT2 = aC2D.LastParameter();
            //
            BndLib_Add2dCurve.Add(aC2D, aT1, aT2, aTol, aBox2D);
        }

        public  static void Add(Geom2d_Curve aC2D,
                   double aT1,

                   double aT2,

                   double aTol,
                  Bnd_Box2d aBox2D)
        {
            BndLib_Box2dCurve aBC = new BndLib_Box2dCurve();
            //
            aBC.SetCurve(aC2D);
            aBC.SetRange(aT1, aT2);
            //
            aBC.Perform();
            //
            Bnd_Box2d aBoxC = aBC.Box();
            aBox2D.Add(aBoxC);
            aBox2D.Enlarge(aTol);
        }
    }
}

