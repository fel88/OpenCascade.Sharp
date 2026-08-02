using OCCPort.Common;
using TKernel;

namespace TKMath
{
    public interface ITheCurve
    {
        int NbIntervals(GeomAbs_Shape geomAbs_CN);
        GeomAbs_CurveType _GetType();


        //! Returns the parametric  resolution corresponding
        //! to the real space resolution <R3d>.
        double Resolution(double R3d);

        void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S);

    }
}
