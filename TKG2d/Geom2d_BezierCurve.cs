using TKMath;

namespace TKG2d
{
    //! Describes a rational or non-rational Bezier curve
    //! - a non-rational Bezier curve is defined by a table
    //! of poles (also called control points),
    //! - a rational Bezier curve is defined by a table of
    //! poles with varying weights.
    //! These data are manipulated by two parallel arrays:
    //! - the poles table, which is an array of gp_Pnt2d points, and
    //! - the weights table, which is an array of reals.
    //! The bounds of these arrays are 1 and "the number of poles" of the curve.
    //! The poles of the curve are "control points" used to deform the curve.
    //! The first pole is the start point of the curve, and the
    //! last pole is the end point of the curve. The segment
    //! which joins the first pole to the second pole is the
    //! tangent to the curve at its start point, and the
    //! segment which joins the last pole to the
    //! second-from-last pole is the tangent to the curve
    //! at its end point.
    //! It is more difficult to give a geometric signification
    //! to the weights but they are useful for providing
    //! exact representations of the arcs of a circle or
    //! ellipse. Moreover, if the weights of all the poles are
    //! equal, the curve is polynomial; it is therefore a
    //! non-rational curve. The non-rational curve is a
    //! special and frequently used case. The weights are
    //! defined and used only in case of a rational curve.
    //! The degree of a Bezier curve is equal to the
    //! number of poles, minus 1. It must be greater than or
    //! equal to 1. However, the degree of a
    //! Geom2d_BezierCurve curve is limited to a value
    //! (25) which is defined and controlled by the system.
    //! This value is returned by the function MaxDegree.
    //! The parameter range for a Bezier curve is [ 0, 1 ].
    //! If the first and last control points of the Bezier
    //! curve are the same point then the curve is closed.
    //! For example, to create a closed Bezier curve with
    //! four control points, you have to give a set of control
    //! points P1, P2, P3 and P1.
    //! The continuity of a Bezier curve is infinite.
    //! It is not possible to build a Bezier curve with
    //! negative weights. We consider that a weight value
    //! is zero if it is less than or equal to
    //! gp::Resolution(). We also consider that
    //! two weight values W1 and W2 are equal if:
    //! |W2 - W1| <= gp::Resolution().
    //! Warning
    //! - When considering the continuity of a closed
    //! Bezier curve at the junction point, remember that
    //! a curve of this type is never periodic. This means
    //! that the derivatives for the parameter u = 0
    //! have no reason to be the same as the
    //! derivatives for the parameter u = 1 even if the curve is closed.
    //! - The length of a Bezier curve can be null.
    public class Geom2d_BezierCurve : Geom2d_BoundedCurve
    {
        TColgp_HArray1OfPnt2d poles;

        public int Degree()
        {
            return poles.Length() - 1;
        }

        public override Geom2d_Geometry Copy()
        {
            throw new System.NotImplementedException();
        }

        public override void D1(double U, out gp_Pnt2d P, out gp_Vec2d V1)
        {
            throw new System.NotImplementedException();
        }

        public override void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2)
        {
            throw new NotImplementedException();
        }

        public override gp_Vec2d DN(double U, int N)
        {
            throw new NotImplementedException();
        }
    }
}
