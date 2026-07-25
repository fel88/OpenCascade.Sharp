using TKMath;

namespace TKGeomBase
{
    //! Class to define function, which calculates square distance between point on curve
    //! C(u), U1 <= u <= U2 and line passing through points C(U1) and C(U2)
    //! This function is used in any minimization algorithm to define maximal deviation between curve and line,
    //! which required one variable function without derivative (for ex. math_BrentMinimum)
    public class GCPnts_DistFunction : math_Function
    {
        public override bool Value(double X, out double F)
        {
            throw new NotImplementedException();
        }
    }
}


