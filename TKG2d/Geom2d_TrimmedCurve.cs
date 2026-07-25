using OCCPort.Common;
using TKMath;

namespace TKG2d
{
    //! Defines a portion of a curve limited by two values of
    //! parameters inside the parametric domain of the curve.
    //! The trimmed curve is defined by:
    //! - the basis curve, and
    //! - the two parameter values which limit it.
    //! The trimmed curve can either have the same
    //! orientation as the basis curve or the opposite orientation.
    public class Geom2d_TrimmedCurve : Geom2d_BoundedCurve
    {
        Geom2d_Curve basisCurve;
        double uTrim1;
        double uTrim2;
        public Geom2d_Curve BasisCurve()
        {
            return basisCurve;
        }

        public Geom2d_TrimmedCurve(Geom2d_Curve C,
                                          double U1,
                                          double U2,
                                          bool Sense,
                                          bool theAdjustPeriodic)
        {
            uTrim1 = (U1);
            uTrim2 = (U2);

            if (C == null) throw new Standard_ConstructionError("Geom2d_TrimmedCurve:: C is null");
            // kill trimmed basis curves
            Geom2d_TrimmedCurve T = (Geom2d_TrimmedCurve)C;
            if (T != null)
                basisCurve = (Geom2d_Curve)(T.BasisCurve().Copy());
            else
                basisCurve = (Geom2d_Curve)(C.Copy());

            SetTrim(U1, U2, Sense, theAdjustPeriodic);
        }
        public void SetTrim(double U1,
                            double U2,
                                   bool Sense,
                                   bool theAdjustPeriodic)
        {
            bool sameSense = true;
            if (U1 == U2)
                throw new Standard_ConstructionError("Geom2d_TrimmedCurve::U1 == U2");

            double Udeb = basisCurve.FirstParameter();
            double Ufin = basisCurve.LastParameter();

            if (basisCurve.IsPeriodic())
            {
                sameSense = Sense;

                // set uTrim1 in the range Udeb , Ufin
                // set uTrim2 in the range uTrim1 , uTrim1 + Period()
                uTrim1 = U1;
                uTrim2 = U2;
                if (theAdjustPeriodic)
                    ElCLib.AdjustPeriodic(Udeb, Ufin,
                                           Math.Min(Math.Abs(uTrim2 - uTrim1) / 2, Precision.PConfusion()),
                                         ref uTrim1, ref uTrim2);
            }
            else
            {
                if (U1 < U2)
                {
                    sameSense = Sense;
                    uTrim1 = U1;
                    uTrim2 = U2;
                }
                else
                {
                    sameSense = !Sense;
                    uTrim1 = U2;
                    uTrim2 = U1;
                }

                if ((Udeb - uTrim1 > Precision.PConfusion()) ||
                (uTrim2 - Ufin > Precision.PConfusion()))
                {
                    throw new Standard_ConstructionError("Geom_TrimmedCurve::parameters out of range");
                }
            }

            if (!sameSense)
                Reverse();
        }

        public override Geom2d_Geometry Copy()
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
    }
}
