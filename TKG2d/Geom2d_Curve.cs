using OCCPort.Common;
using TKMath;

namespace TKG2d
{
    public abstract class Geom2d_Curve : Geom2d_Geometry
    {

        //! Returns true if the parameter of the curve is periodic.
        //! It is possible only if the curve is closed and if the
        //! following relation is satisfied :
        //! for each parametric value U the distance between the point
        //! P(u) and the point P (u + T) is lower or equal to Resolution
        //! from package gp, T is the period and must be a constant.
        //! There are three possibilities :
        //! . the curve is never periodic by definition (SegmentLine)
        //! . the curve is always periodic by definition (Circle)
        //! . the curve can be defined as periodic (BSpline). In this case
        //! a function SetPeriodic allows you to give the shape of the
        //! curve.  The general rule for this case is : if a curve can be
        //! periodic or not the default periodicity set is non periodic
        //! and you have to turn (explicitly) the curve into a periodic
        //! curve  if you want the curve to be periodic.
        public abstract bool IsPeriodic();
        public gp_Pnt2d Value(double U)
        {
            gp_Pnt2d P = new gp_Pnt2d();
            D0(U, out P);
            return P;
        }
        //! Returns the point P of parameter U and the first derivative V1.
        //! Raised if the continuity of the curve is not C1.
        public abstract void D1(double U, out gp_Pnt2d P, out gp_Vec2d V1);


        //! Returns the point P of parameter U, the first and second
        //! derivatives V1 and V2.
        //! Raised if the continuity of the curve is not C2.
        public abstract void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2);


        //! curve becomes the StartPoint of the reversed curve.
        public abstract void Reverse();
        //! Computes the parameter on the reversed curve for
        //! the point of parameter U on this curve.
        //! Note: The point of parameter U on this curve is
        //! identical to the point of parameter
        //! ReversedParameter(U) on the reversed curve.
        public abstract double ReversedParameter(double U);

        //! Returns the value of the first parameter.
        //! Warnings :
        //! It can be RealFirst or RealLast from package Standard
        //! if the curve is infinite
        public abstract double FirstParameter();

        //! Value of the last parameter.
        //! Warnings :
        //! It can be RealFirst or RealLast from package Standard
        //! if the curve is infinite

        public abstract double LastParameter();

        //! Returns in P the point of parameter U.
        //! If the curve is periodic  then the returned point is P(U) with
        //! U = Ustart + (U - Uend)  where Ustart and Uend are the
        //! parametric bounds of the curve.
        //!
        //! Raised only for the "OffsetCurve" if it is not possible to
        //! compute the current point. For example when the first
        //! derivative on the basis curve and the offset direction
        //! are parallel.
        public abstract void D0(double U, out gp_Pnt2d P);

    }
}
