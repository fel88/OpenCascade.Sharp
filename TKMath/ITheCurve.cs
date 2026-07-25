using TKernel;

namespace TKMath
{
    public interface ITheCurve
    {
        int NbIntervals(GeomAbs_Shape geomAbs_CN);
        GeomAbs_CurveType _GetType();

        
        void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S);

    }
}
