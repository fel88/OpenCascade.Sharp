using OCCPort.Common;
using TKG3d;
using TKMath;

namespace TKGeomBase
{
    //! Provides an algorithm to compute a point on a curve
    //! situated at a given distance from another point on the curve,
    //! the distance being measured along the curve (curvilinear abscissa on the curve).
    //! This algorithm is also used to compute the length of a curve.
    //! An AbscissaPoint object provides a framework for:
    //! -   defining the point to compute
    //! -   implementing the construction algorithm
    //! -   consulting the result.
    public class GCPnts_AbscissaPoint
    {
        public static double Length(Adaptor3d_Curve theC)
        {
            return Length(theC, theC.FirstParameter(), theC.LastParameter());
        }
        public static double Length(Adaptor3d_Curve theC,
                                            double theU1, double theU2,
                                            double? theTol)
        {
            return length(theC, theU1, theU2, ref theTol);
        }
        public static double Length(Adaptor3d_Curve theC,
                                            double theU1, double theU2)
        {
            double? d = 0;
            return length(theC, theU1, theU2, ref d);
        }
        //! Dimension independent used to implement GCPnts_AbscissaPoint
        //! compute the type  and the length ratio if GCPnts_LengthParametrized.

        static GCPnts_AbscissaType computeType(ITheCurve theC,
                                                ref double theRatio)
        {
            if (theC.NbIntervals(GeomAbs_Shape.GeomAbs_CN) > 1)
            {
                return GCPnts_AbscissaType.GCPnts_AbsComposite;
            }

            switch (theC._GetType())
            {
                case GeomAbs_CurveType.GeomAbs_Line:
                    {
                        theRatio = 1.0;
                        return GCPnts_AbscissaType.GCPnts_LengthParametrized;
                    }
                //case GeomAbs_Circle:
                //    {
                //        theRatio = theC.Circle().Radius();
                //        return GCPnts_LengthParametrized;
                //    }
                //case GeomAbs_BezierCurve:
                //    {
                //        Handle(typename GCPnts_TCurveTypes < TheCurve >::BezierCurve) aBz = theC.Bezier();
                //        if (aBz->NbPoles() == 2
                //        && !aBz->IsRational())
                //        {
                //            theRatio = aBz->DN(0, 1).Magnitude();
                //            return GCPnts_LengthParametrized;
                //        }
                //        return GCPnts_Parametrized;
                //    }
                //case GeomAbs_BSplineCurve:
                //    {
                //        Handle(typename GCPnts_TCurveTypes < TheCurve >::BSplineCurve) aBs = theC.BSpline();
                //        if (aBs->NbPoles() == 2
                //        && !aBs->IsRational())
                //        {
                //            theRatio = aBs->DN(aBs->FirstParameter(), 1).Magnitude();
                //            return GCPnts_LengthParametrized;
                //        }
                //        return GCPnts_Parametrized;
                //    }
                default:
                    {
                        return GCPnts_AbscissaType.GCPnts_Parametrized;
                    }
            }
        }
        public static double length(ITheCurve theC,
                                                    double theU1, double theU2,
                                                    ref double? theTol)
        {
            double aRatio = 1.0;
            GCPnts_AbscissaType aType = computeType(theC, ref aRatio);
            switch (aType)
            {
                case GCPnts_AbscissaType.GCPnts_LengthParametrized:
                    {
                        return Math.Abs(theU2 - theU1) * aRatio;
                    }
                    //case GCPnts_AbscissaType.GCPnts_Parametrized:
                    //    {
                    //        return theTol != null
                    //             ? CPnts_AbscissaPoint.Length(theC, theU1, theU2, theTol.Value)
                    //             : CPnts_AbscissaPoint::Length(theC, theU1, theU2);
                    //    }
                    //case GCPnts_AbsComposite:
                    //    {
                    //        const Standard_Integer aNbIntervals = theC.NbIntervals(GeomAbs_CN);
                    //        TColStd_Array1OfReal aTI(1, aNbIntervals + 1);
                    //        theC.Intervals(aTI, GeomAbs_CN);
                    //        const Standard_Real aUU1 = Min(theU1, theU2);
                    //        const Standard_Real aUU2 = Max(theU1, theU2);
                    //        Standard_Real aL = 0.0;
                    //        for (Standard_Integer anIndex = 1; anIndex <= aNbIntervals; ++anIndex)
                    //        {
                    //            if (aTI(anIndex) > aUU2) { break; }
                    //            if (aTI(anIndex + 1) < aUU1) { continue; }
                    //            if (theTol != NULL)
                    //            {
                    //                aL += CPnts_AbscissaPoint::Length(theC,
                    //                                                   Max(aTI(anIndex), aUU1),
                    //                                                   Min(aTI(anIndex + 1), aUU2),
                    //                                                   *theTol);
                    //            }
                    //            else
                    //            {
                    //                aL += CPnts_AbscissaPoint::Length(theC,
                    //                                                   Max(aTI(anIndex), aUU1),
                    //                                                   Min(aTI(anIndex + 1), aUU2));
                    //            }
                    //        }
                    //        return aL;
                    //    }
            }
            return Standard_Real.RealLast();
        }


    }
}

