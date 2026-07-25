global using TColStd_Array1OfInteger = TKernel.NCollection_Array1<int>;

using TKernel;
using TKMath;

namespace TKG2d
{
    //! Describes a BSpline curve.
    //! A BSpline curve can be:
    //! - uniform or non-uniform,
    //! - rational or non-rational,
    //! - periodic or non-periodic.
    //! A BSpline curve is defined by:
    //! - its degree; the degree for a
    //! Geom2d_BSplineCurve is limited to a value (25)
    //! which is defined and controlled by the system. This
    //! value is returned by the function MaxDegree;
    //! - its periodic or non-periodic nature;
    //! - a table of poles (also called control points), with
    //! their associated weights if the BSpline curve is
    //! rational. The poles of the curve are "control points"
    //! used to deform the curve. If the curve is
    //! non-periodic, the first pole is the start point of the
    //! curve, and the last pole is the end point of the
    //! curve. The segment, which joins the first pole to the
    //! second pole, is the tangent to the curve at its start
    //! point, and the segment, which joins the last pole to
    //! the second-from-last pole, is the tangent to the
    //! curve at its end point. If the curve is periodic, these
    //! geometric properties are not verified. It is more
    //! difficult to give a geometric signification to the
    //! weights but they are useful for providing exact
    //! representations of the arcs of a circle or ellipse.
    //! Moreover, if the weights of all the poles are equal,
    //! the curve has a polynomial equation; it is
    //! therefore a non-rational curve.
    //! - a table of knots with their multiplicities. For a
    //! Geom2d_BSplineCurve, the table of knots is an
    //! increasing sequence of reals without repetition; the
    //! multiplicities define the repetition of the knots. A
    //! BSpline curve is a piecewise polynomial or rational
    //! curve. The knots are the parameters of junction
    //! points between two pieces. The multiplicity
    //! Mult(i) of the knot Knot(i) of the BSpline
    //! curve is related to the degree of continuity of the
    //! curve at the knot Knot(i), which is equal to
    //! Degree - Mult(i) where Degree is the
    //! degree of the BSpline curve.
    //! If the knots are regularly spaced (i.e. the difference
    //! between two consecutive knots is a constant), three
    //! specific and frequently used cases of knot distribution
    //! can be identified:
    //! - "uniform" if all multiplicities are equal to 1,
    //! - "quasi-uniform" if all multiplicities are equal to 1,
    //! except the first and the last knot which have a
    //! multiplicity of Degree + 1, where Degree is
    //! the degree of the BSpline curve,
    //! - "Piecewise Bezier" if all multiplicities are equal to
    //! Degree except the first and last knot which have
    //! a multiplicity of Degree + 1, where Degree is
    //! the degree of the BSpline curve. A curve of this
    //! type is a concatenation of arcs of Bezier curves.
    //! If the BSpline curve is not periodic:
    //! - the bounds of the Poles and Weights tables are 1
    //! and NbPoles, where NbPoles is the number of
    //! poles of the BSpline curve,
    //! - the bounds of the Knots and Multiplicities tables are
    //! 1 and NbKnots, where NbKnots is the number
    //! of knots of the BSpline curve.
    //! If the BSpline curve is periodic, and if there are k
    //! periodic knots and p periodic poles, the period is:
    //! period = Knot(k + 1) - Knot(1)
    //! and the poles and knots tables can be considered as
    //! infinite tables, such that:
    //! - Knot(i+k) = Knot(i) + period
    //! - Pole(i+p) = Pole(i)
    //! Note: data structures of a periodic BSpline curve are
    //! more complex than those of a non-periodic one.
    //! Warnings :
    //! In this class we consider that a weight value is zero if
    //! Weight <= Resolution from package gp.
    //! For two parametric values (or two knot values) U1, U2 we
    //! consider that U1 = U2 if Abs (U2 - U1) <= Epsilon (U1).
    //! For two weights values W1, W2 we consider that W1 = W2 if
    //! Abs (W2 - W1) <= Epsilon (W1).  The method Epsilon is
    //! defined in the class Real from package Standard.
    //!
    //! References :
    //! . A survey of curve and surface methods in CADG Wolfgang BOHM
    //! CAGD 1 (1984)
    //! . On de Boor-like algorithms and blossoming Wolfgang BOEHM
    //! cagd 5 (1988)
    //! . Blossoming and knot insertion algorithms for B-spline curves
    //! Ronald N. GOLDMAN
    //! . Modelisation des surfaces en CAO, Henri GIAUME Peugeot SA
    //! . Curves and Surfaces for Computer Aided Geometric Design,
    //! a practical guide Gerald Farin
    public class Geom2d_BSplineCurve : Geom2d_BoundedCurve
    {
        public int Degree()
        { return deg; }
        public int deg;
        public int NbKnots()
        { return knots.Length(); }


        //! For a B-spline curve the first parameter (which gives the start
        //! point of the curve) is a knot value but if the multiplicity of
        //! the first knot index is lower than Degree + 1 it is not the
        //! first knot of the curve. This method computes the index of the
        //! knot corresponding to the first parameter.
        public int FirstUKnotIndex()
        {
            if (periodic) return 1;
            throw new NotImplementedException();
            //else return BSplCLib.FirstUKnotIndex(deg, mults.Array1());
        }

        bool periodic;

        TColgp_HArray1OfPnt2d poles;
        TColStd_HArray1OfReal weights;
        TColStd_HArray1OfReal flatknots;
        TColStd_HArray1OfReal knots;
        TColStd_Array1OfInteger mults;
        public bool IsRational()
        {
            return weights != null;
        }

        public override Geom2d_Geometry Copy()
        {
            Geom2d_BSplineCurve C = null;
            //if (IsRational())
            //    C = new Geom2d_BSplineCurve(poles.Array1(),
            //                weights.Array1(),
            //                knots.Array1(),
            //                mults.Array1(),
            //                deg, periodic);
            //else
            //    C = new Geom2d_BSplineCurve(poles->Array1(),
            //                knots->Array1(),
            //                mults->Array1(),
            //                deg, periodic);
            return C;
        }

        public override void D1(double U, out gp_Pnt2d P, out gp_Vec2d V1)
        {
            throw new System.NotImplementedException();
        }

        public override void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2)
        {
            throw new NotImplementedException();
        }

        //! For a BSpline curve the last parameter (which gives the
        //! end point of the curve) is a knot value but if the
        //! multiplicity of the last knot index is lower than
        //! Degree + 1 it is not the last knot of the curve. This
        //! method computes the index of the knot corresponding to
        //! the last parameter.
        internal int LastUKnotIndex()
        {
            throw new NotImplementedException();
        }
    }

}
