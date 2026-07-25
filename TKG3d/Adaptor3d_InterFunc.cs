using OCCPort.Common;
using System.Reflection.Metadata;
using TKG2d;
using TKMath;

namespace TKG3d
{
    //! Used to find the points U(t) = U0 or V(t) = V0 in
    //! order to determine the  Cn discontinuities of  an
    //! Adpator_CurveOnSurface  relatively  to    the
    //! discontinuities of the surface. Used to
    //! find the roots of the functions
    public class Adaptor3d_InterFunc : math_FunctionWithDerivative
    {
        //! build the function  U(t)=FixVal   if Fix =1 or
        //! V(t)=FixVal if Fix=2
        public Adaptor3d_InterFunc(Adaptor2d_Curve2d C, double FixVal, int Fix)
        {
            myCurve2d = (C);
            myFixVal = (FixVal);
            myFix = (Fix);
            if (Fix != 1 && Fix != 2) throw new Standard_ConstructionError();

        }
        Adaptor2d_Curve2d myCurve2d;
        double myFixVal;
        int myFix;
        public override bool Values(double X, out double F, out double D)
        {
            gp_Pnt2d C = new gp_Pnt2d();
            gp_Vec2d DC = new gp_Vec2d();
            myCurve2d.D1(X, out C, out DC);
            if (myFix == 1)
            {
                F = C.X() - myFixVal;
                D = DC.X();
            }
            else
            {
                F = C.Y() - myFixVal;
                D = DC.Y();
            }
            return true;
        }

        //! computes the derivative <D> of the function
        //! for the variable <X>.
        //! Returns True if the calculation were successfully done,
        //! False otherwise.
        public override  bool Derivative( double  X, out double D)
        {
            double F;
            return Values(X, out F, out D );
        }
  
        public override bool Value(double X, out double F)
        {
            gp_Pnt2d C = new gp_Pnt2d();
            myCurve2d.D0(X, ref C);
            if (myFix == 1)
                F = C.X() - myFixVal;
            else
                F = C.Y() - myFixVal;

            return true;
        }
    }
}
