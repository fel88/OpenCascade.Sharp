using OCCPort.Common;
using TKernel;
using TKG2d;
using TKG3d;
using TKMath;

namespace TKGeomBase
{
    //! This class computes a distribution of points on a curve.
    //! The points may respect the deflection.
    //! The algorithm is not based on the classical prediction (with second derivative of curve),
    //! but either on the evaluation of the distance between the mid point
    //! and the point of mid parameter of the two points,
    //! or the distance between the mid point and the point at parameter 0.5
    //! on the cubic interpolation of the two points and their tangents.
    //!
    //! Note: this algorithm is faster than a GCPnts_UniformDeflection algorithm,
    //! and is able to work with non-"C2" continuous curves.
    //! However, it generates more points in the distribution.
    public class GCPnts_QuasiUniformDeflection
    {
        //! Computes a QuasiUniform Deflection distribution of points on the Curve.
        public GCPnts_QuasiUniformDeflection(Adaptor2d_Curve2d theC,
                                                  double theDeflection,
                                                  GeomAbs_Shape theContinuity = GeomAbs_Shape.GeomAbs_C1)
        {
            myDone = (false);
            myDeflection = (theDeflection);
            myCont = GeomAbs_Shape.GeomAbs_C1;
            Initialize(theC, theDeflection, theContinuity);
        }
        void Initialize(Adaptor2d_Curve2d theC,
                                                 double theDeflection,
                                                 GeomAbs_Shape theContinuity)
        {
            Initialize(theC, theDeflection, theC.FirstParameter(), theC.LastParameter(), theContinuity);
        }
        //! Returns the number of points of the distribution
        //! computed by this algorithm.
        //! Exceptions
        //! StdFail_NotDone if this algorithm has not been
        //! initialized, or if the computation was not successful.
        public int NbPoints()
        {
            Exceptions.StdFail_NotDone_Raise_if(!myDone, "GCPnts_QuasiUniformDeflection::NbPoints()");
            return myParams.Length();
        }

        //! Returns the point of index Index in the distribution
        //! computed by this algorithm.
        //! Warning
        //! Index must be greater than or equal to 1, and less
        //! than or equal to the number of points of the
        //! distribution. However, pay particular attention as this
        //! condition is not checked by this function.
        //! Exceptions
        //! StdFail_NotDone if this algorithm has not been
        //! initialized, or if the computation was not successful.
        public gp_Pnt Value(int theIndex)
        {
            Exceptions.StdFail_NotDone_Raise_if(!myDone, "GCPnts_QuasiUniformAbscissa::Parameter()");
            return myPoints.Value(theIndex);
        }


        void Initialize(Adaptor2d_Curve2d theC,
                                                 double theDeflection,
                                                 double theU1, double theU2,
                                                 GeomAbs_Shape theContinuity)
        {
            initialize(theC, theDeflection, theU1, theU2, theContinuity);
        }

        static GCPnts_DeflectionType GetDefType(ITheCurve theC)
        {
            if (theC.NbIntervals(GeomAbs_Shape.GeomAbs_C1) > 1)
            {
                return GCPnts_DeflectionType.GCPnts_DefComposite;
            }

            // pour forcer les decoupages aux cassures.
            // G1 devrait marcher, mais donne des exceptions...
            switch (theC._GetType())
            {
                case GeomAbs_CurveType.GeomAbs_Line: return GCPnts_DeflectionType.GCPnts_Linear;
                case GeomAbs_CurveType.GeomAbs_Circle: return GCPnts_DeflectionType.GCPnts_Circular;
                //case GeomAbs_CurveType.GeomAbs_BSplineCurve:
                //    {
                //        Handle(typename GCPnts_TCurveTypes < TheCurve >::BSplineCurve) aBS = theC.BSpline();
                //        return (aBS->NbPoles() == 2) ? GCPnts_Linear : GCPnts_Curved;
                //    }
                //    case GeomAbs_CurveType.GeomAbs_BezierCurve:
                //    {
                //        Handle(typename GCPnts_TCurveTypes < TheCurve >::BezierCurve) aBZ = theC.Bezier();
                //        return (aBZ->NbPoles() == 2) ? GCPnts_Linear : GCPnts_Curved;
                //    }
                default: return GCPnts_DeflectionType.GCPnts_Curved;
            }
        }


        void initialize(ITheCurve theC,
                                                 double theDeflection,
                                                 double theU1, double theU2,
                                                 GeomAbs_Shape theContinuity)
        {
            myCont = (theContinuity > GeomAbs_Shape.GeomAbs_G1) ? GeomAbs_Shape.GeomAbs_C1 : GeomAbs_Shape.GeomAbs_C0;
            myDeflection = theDeflection;
            myDone = false;
            myParams.Clear();
            myPoints.Clear();

            double anEPSILON = Math.Min(theC.Resolution(Precision.Confusion()), 1e50);
            GCPnts_DeflectionType aType = GetDefType(theC);
            double aU1 = Math.Min(theU1, theU2);
            double aU2 = Math.Max(theU1, theU2);
            if (aType == GCPnts_DeflectionType.GCPnts_Curved
             || aType == GCPnts_DeflectionType.GCPnts_DefComposite)
            {
                //if (theC.GetType() == GeomAbs_BSplineCurve
                // || theC.GetType() == GeomAbs_BezierCurve)
                //{
                //    double aMaxPar = Math.Max(Math.Abs(theC.FirstParameter()), Abs(theC.LastParameter()));
                //    if (anEPSILON < Epsilon(aMaxPar))
                //    {
                //        return;
                //    }
                //}
            }

            switch (aType)
            {
                case GCPnts_DeflectionType.GCPnts_Linear:
                    {
                        myDone = PerformLinear(theC, myParams, myPoints, aU1, aU2);
                        break;
                    }
                case GCPnts_DeflectionType.GCPnts_Circular:
                    {
                        throw new NotImplementedException();
                        //  myDone = PerformCircular(theC, myParams, myPoints, theDeflection, aU1, aU2);
                        break;
                    }
                case GCPnts_DeflectionType.GCPnts_Curved:
                    {
                        throw new NotImplementedException();

                        //  myDone = PerformCurve(myParams, myPoints, theC, theDeflection,
                        //                     aU1, aU2, anEPSILON, myCont);
                        break;
                    }
                case GCPnts_DeflectionType.GCPnts_DefComposite:
                    {
                        throw new NotImplementedException();

                        //myDone = PerformComposite(myParams, myPoints, theC, theDeflection,
                        //                            aU1, aU2, anEPSILON, myCont);
                        break;
                    }
            }
        }
        static bool PerformLinear(ITheCurve theC,
                                       TColStd_SequenceOfReal theParameters,
                                       TColgp_SequenceOfPnt thePoints,
                                        double theU1,
            double theU2)
        {
            if (theC is Adaptor2d_Curve2d a2)
            {
                theParameters.Append(theU1);
                gp_Pnt aPoint = Value(a2, theU1);
                thePoints.Append(aPoint);

                theParameters.Append(theU2);
                aPoint = Value(a2, theU2);
                thePoints.Append(aPoint);
            }
            else
            if (theC is Adaptor3d_Curve a3)
            {
                theParameters.Append(theU1);
                gp_Pnt aPoint = Value(a3, theU1);
                thePoints.Append(aPoint);

                theParameters.Append(theU2);
                aPoint = Value(a3, theU2);
                thePoints.Append(aPoint);
            }
            else
            {
                throw new Exception();
            }
            return true;
        }

        // mask the return of a Adaptor2d_Curve2d as a gp_Pnt 
        static gp_Pnt Value(Adaptor3d_Curve theC,
                      double theParameter)
        {
            return theC.Value(theParameter);
        }

        static gp_Pnt Value(Adaptor2d_Curve2d theC,
                      double theParameter)
        {
            gp_Pnt aPoint = new gp_Pnt();
            gp_Pnt2d a2dPoint = new(theC.Value(theParameter).coord);//not origin code
            aPoint.SetCoord(a2dPoint.X(), a2dPoint.Y(), 0.0);
            return aPoint;
        }


        //! Returns true if the computation was successful.
        //! IsDone is a protection against:
        //! -   non-convergence of the algorithm
        //! -   querying the results before computation.
        public bool IsDone()
        {
            return myDone;
        }

        bool myDone;
        double myDeflection;
        TColStd_SequenceOfReal myParams;
        TColgp_SequenceOfPnt myPoints;
        GeomAbs_Shape myCont;
    }
    }

