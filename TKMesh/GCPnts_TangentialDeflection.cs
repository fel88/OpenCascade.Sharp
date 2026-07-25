global using TColgp_SequenceOfPnt = TKernel.NCollection_Sequence<TKMath.gp_Pnt>;

using OCCPort.Common;
using System.Linq;
using TKernel;
using TKG2d;
using TKG3d;
using TKMath;

namespace TKMesh
{
    //! Computes a set of  points on a curve from package
    //! Adaptor3d  such  as between  two successive   points
    //! P1(u1)and P2(u2) :
    //! @code
    //! . ||P1P3^P3P2||/||P1P3||*||P3P2||<AngularDeflection
    //! . ||P1P2^P1P3||/||P1P2||<CurvatureDeflection
    //! @endcode
    //! where P3 is the point of abscissa ((u1+u2)/2), with
    //! u1 the abscissa of the point P1 and u2 the abscissa
    //! of the point P2.
    //!
    //! ^ is the cross product of two vectors, and ||P1P2||
    //! the magnitude of the vector P1P2.
    //!
    //! The conditions AngularDeflection > gp::Resolution()
    //! and CurvatureDeflection > gp::Resolution() must be
    //! satisfied at the construction time.
    //!
    //! A minimum number of points can be fixed for a linear or circular element.
    //! Example:
    //! @code
    //! Handle(Geom_BezierCurve) aCurve = new Geom_BezierCurve (thePoles);
    //! GeomAdaptor_Curve aCurveAdaptor (aCurve);
    //! double aCDeflect  = 0.01; // Curvature deflection
    //! double anADeflect = 0.09; // Angular   deflection
    //!
    //! GCPnts_TangentialDeflection aPointsOnCurve;
    //! aPointsOnCurve.Initialize (aCurveAdaptor, anADeflect, aCDeflect);
    //! for (int i = 1; i <= aPointsOnCurve.NbPoints(); ++i)
    //! {
    //!   double aU   = aPointsOnCurve.Parameter (i);
    //!   gp_Pnt aPnt = aPointsOnCurve.Value (i);
    //! }
    //! @endcode
    public class GCPnts_TangentialDeflection
    {

        public GCPnts_TangentialDeflection()
        {
            myAngularDeflection = (0.0);
            myCurvatureDeflection = (0.0);
            myUTol = (0.0);
            myMinNbPnts = (0);
            myMinLen = (0.0);
            myLastU = (0.0);
            myFirstu = 0.0;

        }

        public static double ArcAngularStep(
    double theRadius,
    double theLinearDeflection,
    double theAngularDeflection,
    double theMinLength)
        {
            Exceptions.Standard_ConstructionError_Raise_if(theRadius < 0.0, "Negative radius");

            double aPrecision = Precision.Confusion();

            double Du = 0.0, aMinSizeAng = 0.0;
            if (theRadius > aPrecision)
            {
                Du = Math.Max(1.0 - (theLinearDeflection / theRadius), 0.0);

                // It is not suitable to consider min size greater than 1/4 arc len.
                if (theMinLength > aPrecision)
                    aMinSizeAng = Math.Min(theMinLength / theRadius, Math.PI / 2);
            }
            Du = 2.0 * Math.Acos(Du);
            Du = Math.Max(Math.Min(Du, theAngularDeflection), aMinSizeAng);
            return Du;
        }


        public gp_Pnt Value(int I)
        {
            return myPoints.Value(I);
        }

        public GCPnts_TangentialDeflection(Adaptor3d_Curve theC,
                                                          double theFirstParameter,
                                                          double theLastParameter,
                                                          double theAngularDeflection,
                                                          double theCurvatureDeflection,
                                                          int theMinimumOfPoints = 2,
                                                          double theUTol = 1.0e-9,
                                                          double theMinLen = 1.0e-7)
        {
            myAngularDeflection = 0.0;
            myCurvatureDeflection = (0.0);
            myUTol = (0.0);
            myMinNbPnts = (0);
            myMinLen = (0.0);
            myLastU = (0.0);
            myFirstu = 0.0;


            Initialize(theC, theFirstParameter, theLastParameter,
                        theAngularDeflection, theCurvatureDeflection,
                        theMinimumOfPoints,
                        theUTol, theMinLen);
        }

        public void Initialize(Adaptor3d_Curve theC,
                                               double theFirstParameter,
                                               double theLastParameter,
                                               double theAngularDeflection,
             double theCurvatureDeflection,
                                               int theMinimumOfPoints,
                                               double theUTol,
                                               double theMinLen)
        {
            initialize(theC, theFirstParameter, theLastParameter,
                        theAngularDeflection, theCurvatureDeflection,
                        theMinimumOfPoints,
                        theUTol,
                        theMinLen);
        }

        double myAngularDeflection;
        double myCurvatureDeflection;
        double myUTol;
        int myMinNbPnts;
        double myMinLen;
        double myLastU;
        double myFirstu;

        public void initialize(Adaptor3d_Curve theC,
                                              double theFirstParameter,
                                              double theLastParameter,
                                              double theAngularDeflection,
                                              double theCurvatureDeflection,
                                              int theMinimumOfPoints,
                                              double theUTol,
                                              double theMinLen)
        {
            Exceptions.Standard_ConstructionError_Raise_if(theCurvatureDeflection < Precision.Confusion() || theAngularDeflection < Precision.Angular(),
                                                 "GCPnts_TangentialDeflection::Initialize - Zero Deflection");
            myParameters.Clear();
            myPoints.Clear();
            if (theFirstParameter < theLastParameter)
            {
                myFirstu = theFirstParameter;
                myLastU = theLastParameter;
            }
            else
            {
                myLastU = theFirstParameter;
                myFirstu = theLastParameter;
            }
            myUTol = theUTol;
            myAngularDeflection = theAngularDeflection;
            myCurvatureDeflection = theCurvatureDeflection;
            myMinNbPnts = Math.Max(theMinimumOfPoints, 2);
            myMinLen = Math.Max(theMinLen, Precision.Confusion());

            switch (theC._GetType())
            {
                case GeomAbs_CurveType.GeomAbs_Line:
                    {
                        PerformLinear(theC);
                        break;
                    }
                case GeomAbs_CurveType.GeomAbs_Circle:
                    {
                        PerformCircular(theC);
                        break;
                    }
                //case GeomAbs_BSplineCurve:
                //    {
                //        Handle(typename GCPnts_TCurveTypes < TheCurve >::BSplineCurve) aBS = theC.BSpline();
                //        if (aBS->NbPoles() == 2) PerformLinear(theC);
                //        else PerformCurve(theC);
                //        break;
                //    }
                //case GeomAbs_BezierCurve:
                //    {
                //        Handle(typename GCPnts_TCurveTypes < TheCurve >::BezierCurve) aBZ = theC.Bezier();
                //        if (aBZ->NbPoles() == 2) PerformLinear(theC);
                //        else PerformCurve(theC);
                //        break;
                //    }
                default:
                    {
                        PerformCurve(theC);
                        break;
                    }
            }
        }

        public void PerformCircular(dynamic theC)
        {
            // akm 8/01/02 : check the radius before divide by it
            double dfR = theC.Circle().Radius();
            double Du = GCPnts_TangentialDeflection.ArcAngularStep(dfR, myCurvatureDeflection, myAngularDeflection, myMinLen);

            double aDiff = myLastU - myFirstu;
            // Round up number of points to satisfy curvatureDeflection more precisely
            int NbPoints = (int)Math.Min(Math.Ceiling(aDiff / Du), 1.0e+6);
            NbPoints = Math.Max(NbPoints, myMinNbPnts - 1);
            Du = aDiff / NbPoints;

            gp_Pnt P = new gp_Pnt();
            double U = myFirstu;
            for (int i = 1; i <= NbPoints; i++)
            {
                D0(theC, U, ref P);
                myParameters.Append(U);
                myPoints.Append(P);
                U += Du;
            }

            D0(theC, myLastU, ref P);
            myParameters.Append(myLastU);
            myPoints.Append(P);
        }

        const double Us3 = 0.3333333333333333333333333333;

        void PerformCurve(Adaptor3d_Curve theC)
        {
            int i, j;
            gp_XYZ V1, V2;
            gp_Pnt MiddlePoint, CurrentPoint, LastPoint;
            double Du, Dusave, MiddleU, L1, L2;

            double U1 = myFirstu;
            double LTol = Precision.Confusion(); // protection longueur nulle
            double ATol = 1e-2 * myAngularDeflection;
            if (ATol > 1e-2)
            {
                ATol = 1e-2;
            }
            else if (ATol < 1e-7)
            {
                ATol = 1e-7;
            }

            LastPoint = new gp_Pnt();//not origin
            D0(theC, myLastU, ref LastPoint);

            // Initialization du calcul

            bool NotDone = true;
            Dusave = (myLastU - myFirstu) * Us3;
            Du = Dusave;
            EvaluateDu(theC, U1, out CurrentPoint, Du, NotDone);
            myParameters.Append(U1);
            myPoints.Append(CurrentPoint);

            // Used to detect "isLine" current bspline and in Du computation in general handling.
            int NbInterv = theC.NbIntervals(GeomAbs_Shape.GeomAbs_CN);
            TColStd_Array1OfReal Intervs = new(1, NbInterv + 1);
            theC.Intervals(Intervs, GeomAbs_Shape.GeomAbs_CN);


            /* more code */
        }

        public static void D2(Adaptor3d_Curve C, double U,
                              out  gp_Pnt P, out gp_Vec V1, out gp_Vec V2)
        {
            C.D2(U, out P, out V1, out V2);
        }

        static void D2(Adaptor2d_Curve2d C, double U,
                        gp_Pnt PP, gp_Vec VV1, gp_Vec VV2)
        {
            double X, Y;
            gp_Pnt2d P;
            gp_Vec2d V1, V2;
            C.D2(U, out P, out  V1, out  V2);
            P.Coord(out X, out Y);
            PP.SetCoord(X, Y, 0.0);
            V1.Coord(out X, out Y);
            VV1.SetCoord(X, Y, 0.0);
            V2.Coord(out X, out Y);
            VV2.SetCoord(X, Y, 0.0);
        }

        public  void EvaluateDu(Adaptor3d_Curve theC,
                                                double theU,
                                              out  gp_Pnt theP,
                                               double theDu,
                                               bool theNotDone)
        {
            gp_Vec T, N;
            D2(theC, theU, out theP, out T, out  N);
            double Lt = T.Magnitude();
            double LTol = Precision.Confusion();
            if (Lt > LTol && N.Magnitude() > LTol)
            {
                double Lc = N.CrossMagnitude(T);
                double Ln = Lc / Lt;
                if (Ln > LTol)
                {
                    theDu = Math.Sqrt(8.0 * Math.Max(myCurvatureDeflection, myMinLen) / Ln);
                    theNotDone = false;
                }
            }
        }
        

        public static void D0(Adaptor3d_Curve C, double U, ref gp_Pnt P)
        {
            C.D0(U, ref P);
        }

        public static void D0(Adaptor2d_Curve2d C, double U, ref gp_Pnt PP)
        {
            double X = 0, Y = 0;
            gp_Pnt2d P = new gp_Pnt2d();
            C.D0(U, ref P);
            P.Coord(out X, out Y);
            PP.SetCoord(X, Y, 0.0);
        }

        public void PerformLinear(Adaptor2d_Curve2d theC)
        {
            gp_Pnt P = new gp_Pnt();
            D0(theC, myFirstu, ref P);
            myParameters.Append(myFirstu);
            myPoints.Append(P);
            if (myMinNbPnts > 2)
            {
                double Du = (myLastU - myFirstu) / myMinNbPnts;
                double U = myFirstu + Du;
                for (int i = 2; i < myMinNbPnts; i++)
                {
                    D0(theC, U, ref P);
                    myParameters.Append(U);
                    myPoints.Append(P);
                    U += Du;
                }
            }
            D0(theC, myLastU, ref P);
            myParameters.Append(myLastU);
            myPoints.Append(P);
        }
        public void PerformLinear(Adaptor3d_Curve theC)
        {
            gp_Pnt P = new gp_Pnt();
            D0(theC, myFirstu, ref P);
            myParameters.Append(myFirstu);
            myPoints.Append(P);
            if (myMinNbPnts > 2)
            {
                double Du = (myLastU - myFirstu) / myMinNbPnts;
                double U = myFirstu + Du;
                for (int i = 2; i < myMinNbPnts; i++)
                {
                    D0(theC, U, ref P);
                    myParameters.Append(U);
                    myPoints.Append(P);
                    U += Du;
                }
            }
            D0(theC, myLastU, ref P);
            myParameters.Append(myLastU);
            myPoints.Append(P);
        }


        TColgp_SequenceOfPnt myPoints = new TColgp_SequenceOfPnt();
        TColStd_SequenceOfReal myParameters = new TColStd_SequenceOfReal();

        //! Add point to already calculated points (or replace existing)
        //! Returns index of new added point
        //! or founded with parametric tolerance (replaced if theIsReplace is true)
        public int AddPoint(gp_Pnt thePnt,
  double theParam,
  bool theIsReplace = true)
        {
            double tol = Precision.PConfusion();
            int index = -1;
            int nb = myParameters.Length();
            for (int i = 1; index == -1 && i <= nb; i++)
            {
                double dist = myParameters.Value(i) - theParam;
                if (Math.Abs(dist) <= tol)
                {
                    index = i;
                    if (theIsReplace)
                    {
                        myPoints.ChangeValue(i, thePnt); ;
                        myParameters.ChangeValue(i, theParam);
                    }
                }
                else if (dist > tol)
                {
                    myPoints.InsertBefore(i, thePnt);
                    myParameters.InsertBefore(i, theParam);
                    index = i;
                }
            }
            if (index == -1)
            {
                myPoints.Append(thePnt);
                myParameters.Append(theParam);
                index = myParameters.Length();
            }
            return index;
        }
        public int NbPoints()
        {
            return myParameters.Length();
        }
        public double Parameter(int I)
        {
            return myParameters.Value(I);
        }
    }
}



