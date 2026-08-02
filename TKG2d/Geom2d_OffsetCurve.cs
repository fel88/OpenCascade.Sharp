using OCCPort.Common;
using TKMath;

namespace TKG2d
{
    //! This class implements the basis services for the creation,
    //! edition, modification and evaluation of planar offset curve.
    //! The offset curve is obtained by offsetting by distance along
    //! the normal to a basis curve defined in 2D space.
    //! The offset curve in this package can be a self intersecting
    //! curve even if the basis curve does not self-intersect.
    //! The self intersecting portions are not deleted at the
    //! construction time.
    //! An offset curve is a curve at constant distance (Offset) from a
    //! basis curve and the offset curve takes its parametrization from
    //! the basis curve. The Offset curve is in the direction of the
    //! normal to the basis curve N.
    //! The distance offset may be positive or negative to indicate the
    //! preferred side of the curve :
    //! . distance offset >0 => the curve is in the direction of N
    //! . distance offset >0 => the curve is in the direction of - N
    //! On the Offset curve :
    //! Value(u) = BasisCurve.Value(U) + (Offset * (T ^ Z)) / ||T ^ Z||
    //! where T is the tangent vector to the basis curve and Z the
    //! direction of the normal vector to the plane of the curve,
    //! N = T ^ Z defines the offset direction and should not have
    //! null length.
    //!
    //! Warnings :
    //! In this package we suppose that the continuity of the offset
    //! curve is one degree less than the continuity of the
    //! basis curve and we don't check that at any point ||T^Z|| != 0.0
    //!
    //! So to evaluate the curve it is better to check that the offset
    //! curve is well defined at any point because an exception could
    //! be raised. The check is not done in this package at the creation
    //! of the offset curve because the control needs the use of an
    //! algorithm which cannot be implemented in this package.
    //! The OffsetCurve is closed if the first point and the last point
    //! are the same (The distance between these two points is lower or
    //! equal to the Resolution sea package gp) . The OffsetCurve can be
    //! closed even if the basis curve is not closed.
    public class Geom2d_OffsetCurve : Geom2d_Curve
    {
        public double  Offset() 
{ return offsetValue; }
        double  offsetValue;

        public override Geom2d_Geometry Copy()
        {
            throw new NotImplementedException();
        }

        public Geom2d_Curve BasisCurve()
        {
            return basisCurve;
        }
        Geom2d_Curve basisCurve;

        public override void D0(double U, out gp_Pnt2d P)
        {
            throw new NotImplementedException();
        }

        public override void D1(double U, out gp_Pnt2d P, out gp_Vec2d V1)
        {
            throw new NotImplementedException();
        }

        public override void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2)
        {
            throw new NotImplementedException();
        }

        public override double FirstParameter()
        {
            throw new NotImplementedException();
        }

        public override bool IsPeriodic()
        {
            throw new NotImplementedException();
        }

        public override double LastParameter()
        {
            throw new NotImplementedException();
        }

        public override void Reverse()
        {
            throw new NotImplementedException();
        }

        public override double ReversedParameter(double U)
        {
            throw new NotImplementedException();
        }

        public override gp_Vec2d DN(double U, int N)
        {
            throw new NotImplementedException();
        }
    }
}
