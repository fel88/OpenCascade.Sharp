using TKG2d;
using TKG3d;
using TKMath;

namespace TKGeomAlgo
{
    //! This class provides a Geom2dCurveTool as < Geom2dCurveTool from IntCurve >
    //! from a Tool as < Geom2dCurveTool from Adaptor3d > .
    public class Geom2dInt_Geom2dCurveTool
    {
        public static int NbSamples(Adaptor2d_Curve2d C)
        {
            int nbs = C.NbSamples();
            GeomAbs_CurveType typC = C._GetType();
            if (typC == GeomAbs_CurveType.GeomAbs_Circle)
            {
                ////Try to reach deflection = eps*R, eps = 0.01
                //double minR = 1.0; //eps = 0.01
                //double R = C.Circle().Radius();
                //if (R > minR)
                //{
                //    double angl = 0.283079; //2.*ACos(1. - eps);
                //    int  n = RealToInt((C.LastParameter() - C.FirstParameter()) / angl);
                //    nbs = Math.Max(n, nbs);
                //}
            }

            return nbs;
        }
    }
}
