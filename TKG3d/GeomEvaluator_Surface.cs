using OCCPort.Common;
using TKMath;

namespace TKG3d
{
    //! Interface for calculation of values and derivatives for different kinds of surfaces.
    //! Works both with adaptors and surfaces.
    public interface GeomEvaluator_Surface
    {
        //! Value and first derivatives of surface
        void D1(double theU, double theV,
                      out gp_Pnt theValue, out gp_Vec theD1U, out gp_Vec theD1V);

        //! Value, first and second derivatives of surface
        void D2( double theU, double theV,
                      out  gp_Pnt theValue, out gp_Vec theD1U, out gp_Vec theD1V,
                out gp_Vec theD2U, out gp_Vec theD2V, out gp_Vec theD2UV) ;
    }
}
