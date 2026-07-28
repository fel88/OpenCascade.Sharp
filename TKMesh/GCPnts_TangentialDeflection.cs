global using TColgp_SequenceOfPnt = TKernel.NCollection_Sequence<TKMath.gp_Pnt>;

using OCCPort.Common;
using System;
using System.Linq;
using TKernel;
using TKG2d;
using TKG3d;
using TKGeomBase;
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

        void PerformCurve(ITheCurve theC)
        {
            int i, j;
            gp_XYZ V1, V2;
            gp_Pnt MiddlePoint = new gp_Pnt(), CurrentPoint, LastPoint;
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

            if (NotDone || Du > 5.0 * Dusave)
            {
                //C'est soit une droite, soit une singularite :
                V1 = (LastPoint.XYZ() - CurrentPoint.XYZ());
                L1 = V1.Modulus();
                if (L1 > LTol)
                {
                    // Si c'est une droite on verifie en calculant minNbPoints :
                    bool IsLine = true;
                    int NbPoints = (myMinNbPnts > 3) ? myMinNbPnts : 3;
                    switch (theC.GetType())
                    {
                        //case GeomAbs_BSplineCurve:
                        //    {
                        //        Handle(typename GCPnts_TCurveTypes < TheCurve >::BSplineCurve) BS = theC.BSpline();
                        //        NbPoints = Max(BS->Degree() + 1, NbPoints);
                        //        break;
                        //    }
                        //case GeomAbs_BezierCurve:
                        //    {
                        //        Handle(typename GCPnts_TCurveTypes < TheCurve >::BezierCurve) BZ = theC.Bezier();
                        //        NbPoints = Max(BZ->Degree() + 1, NbPoints);
                        //        break;
                        //    }
                        default:
                            {
                                break;
                            }
                    }
                    ////
                    double param = 0.0;
                    for (i = 1; i <= NbInterv && IsLine; ++i)
                    {
                        // Avoid usage intervals out of [myFirstu, myLastU].
                        if ((Intervs[(i + 1)] < myFirstu)
                         || (Intervs[(i)] > myLastU))
                        {
                            continue;
                        }

                        // Fix border points in applicable intervals, to avoid be out of target interval.
                        if ((Intervs[(i)] < myFirstu)
                         && (Intervs[(i + 1)] > myFirstu))
                        {
                            Intervs[(i)] = myFirstu;
                        }
                        if ((Intervs[(i)] < myLastU)
                         && (Intervs[(i + 1)] > myLastU))
                        {
                            Intervs[(i + 1)] = myLastU;
                        }

                        double delta = (Intervs[(i + 1)] - Intervs[(i)]) / NbPoints;
                        for (j = 1; j <= NbPoints && IsLine; ++j)
                        {
                            param = Intervs[(i)] + j * delta;
                            D0(theC, param, ref MiddlePoint);
                            V2 = (MiddlePoint.XYZ() - CurrentPoint.XYZ());
                            L2 = V2.Modulus();
                            if (L2 > LTol)
                            {
                                double aAngle = V2.CrossMagnitude(V1) / (L1 * L2);
                                IsLine = (aAngle < ATol);
                            }
                        }
                    }

                    if (IsLine)
                    {
                        myParameters.Clear();
                        myPoints.Clear();

                        PerformLinear(theC);
                        return;
                    }
                    else
                    {
                        // c'etait une singularite on continue:
                        //Du = Dusave;
                        EvaluateDu(theC, param, out MiddlePoint, Du, NotDone);
                    }
                }
                else
                {
                    Du = (myLastU - myFirstu) / 2.1;
                    MiddleU = myFirstu + Du;
                    D0(theC, MiddleU, ref MiddlePoint);
                    V1 = (MiddlePoint.XYZ() - CurrentPoint.XYZ());
                    L1 = V1.Modulus();
                    if (L1 < LTol)
                    {
                        // L1 < LTol C'est une courbe de longueur nulle, calcul termine :
                        // on renvoi un segment de 2 points   (protection)
                        myParameters.Append(myLastU);
                        myPoints.Append(LastPoint);
                        return;
                    }
                }
            }

            if (Du > Dusave) Du = Dusave;
            else Dusave = Du;

            if (Du < myUTol)
            {
                Du = myLastU - myFirstu;
                if (Du < myUTol)
                {
                    myParameters.Append(myLastU);
                    myPoints.Append(LastPoint);
                    return;
                }
            }

            // Traitement normal pour une courbe
            bool MorePoints = true;
            double U2 = myFirstu;
            double AngleMax = myAngularDeflection * 0.5;  // car on prend le point milieu
                                                          // Indexes of intervals of U1 and U2, used to handle non-uniform case.
            int[] aIdx = { Intervs.Lower(), Intervs.Lower() };
            bool isNeedToCheck = false;
            gp_Pnt aPrevPoint = myPoints.Last();
            while (MorePoints)
            {
                aIdx[0] = getIntervalIdx(U1, Intervs, aIdx[0]);
                U2 += Du;

                if (U2 >= myLastU)                       // Bout de courbe
                {
                    U2 = myLastU;
                    CurrentPoint = LastPoint;
                    Du = U2 - U1;
                    Dusave = Du;
                }
                else
                {
                    D0(theC, U2, ref CurrentPoint);           // Point suivant
                }

                double Coef = 0.0, ACoef = 0.0, FCoef = 0.0;
                bool Correction, TooLarge, TooSmall;
                TooLarge = false;
                Correction = true;
                TooSmall = false;
                while (Correction)                       // Ajustement Du
                {
                    if (isNeedToCheck)
                    {
                        aIdx[1] = getIntervalIdx(U2, Intervs, aIdx[0]);
                        if (aIdx[1] > aIdx[0]) // Jump to another polynom.
                        {
                            // Set Du to the smallest value and check deflection on it.
                            if (Du > (Intervs[(aIdx[0] + 1)] - Intervs[(aIdx[0])]) * Us3)
                            {
                                Du = (Intervs[(aIdx[0] + 1)] - Intervs[(aIdx[0])]) * Us3;
                                U2 = U1 + Du;
                                if (U2 > myLastU)
                                {
                                    U2 = myLastU;
                                }
                                D0(theC, U2, ref CurrentPoint);
                            }
                        }
                    }


                    MiddleU = (U1 + U2) * 0.5;                 // Verif / au point milieu
                    D0(theC, MiddleU, ref MiddlePoint);

                    V1 = (CurrentPoint.XYZ() - aPrevPoint.XYZ()); // Critere de fleche
                    V2 = (MiddlePoint.XYZ() - aPrevPoint.XYZ());
                    L1 = V1.Modulus();

                    FCoef = (L1 > myMinLen) ? V1.CrossMagnitude(V2) / (L1 * myCurvatureDeflection) : 0.0;

                    V1 = (CurrentPoint.XYZ() - MiddlePoint.XYZ()); // Critere d'angle
                    L1 = V1.Modulus();
                    L2 = V2.Modulus();
                    if (L1 > myMinLen && L2 > myMinLen)
                    {
                        double angg = V1.CrossMagnitude(V2) / (L1 * L2);
                        ACoef = angg / AngleMax;
                    }
                    else
                    {
                        ACoef = 0.0;
                    }


                    // On retient le plus penalisant
                    Coef = Math.Max(ACoef, FCoef);

                    if (isNeedToCheck && Coef < 0.55)
                    {
                        isNeedToCheck = false;
                        Du = Dusave;
                        U2 = U1 + Du;
                        if (U2 > myLastU)
                        {
                            U2 = myLastU;
                        }
                        D0(theC, U2, ref CurrentPoint);
                        continue;
                    }

                    if (Coef <= 1.0)
                    {
                        if (Math.Abs(myLastU - U2) < myUTol)
                        {
                            myParameters.Append(myLastU);
                            myPoints.Append(LastPoint);
                            MorePoints = false;
                            Correction = false;
                        }
                        else
                        {
                            if (Coef >= 0.55 || TooLarge)
                            {
                                myParameters.Append(U2);
                                myPoints.Append(CurrentPoint);
                                aPrevPoint = CurrentPoint;
                                Correction = false;
                                isNeedToCheck = true;
                            }
                            else if (TooSmall)
                            {
                                Correction = false;
                                aPrevPoint = CurrentPoint;
                            }
                            else
                            {
                                TooSmall = true;
                                //Standard_Real UUU2 = U2;
                                Du += Math.Min((U2 - U1) * (1.0- Coef), Du * Us3);

                                U2 = U1 + Du;
                                if (U2 > myLastU)
                                {
                                    U2 = myLastU;
                                }
                                D0(theC, U2, ref CurrentPoint);
                            }
                        }
                    }
                    else
                    {
                        if (Coef >= 1.5)
                        {
                            if (!aPrevPoint.IsEqual(myPoints.Last(), Precision.Confusion()))
                            {
                                myParameters.Append(U1);
                                myPoints.Append(aPrevPoint);
                            }
                            U2 = MiddleU;
                            Du = U2 - U1;
                            CurrentPoint = MiddlePoint;
                        }
                        else
                        {
                            Du *= 0.9;
                            U2 = U1 + Du;
                            D0(theC, U2,ref CurrentPoint);
                            TooLarge = true;
                        }
                    }

                }


                Du = U2 - U1;
                if (MorePoints)
                {
                    if (U1 > myFirstu)
                    {
                        if (FCoef > ACoef)
                        {
                            // La fleche est critere de decoupage
                            EvaluateDu(theC, U2, out CurrentPoint, Du, NotDone);
                            if (NotDone)
                            {
                                Du += (Du - Dusave) * (Du / Dusave);
                                if (Du > 1.5 * Dusave) Du = 1.5 * Dusave;
                                if (Du < 0.75 * Dusave) Du = 0.75 * Dusave;
                            }
                        }
                        else
                        {
                            //L'angle est le critere de decoupage
                            Du += (Du - Dusave) * (Du / Dusave);
                            if (Du > 1.5 * Dusave) Du = 1.5 * Dusave;
                            if (Du < 0.75 * Dusave) Du = 0.75 * Dusave;
                        }
                    }

                    if (Du < myUTol)
                    {
                        Du = myLastU - U2;
                        if (Du < myUTol)
                        {
                            myParameters.Append(myLastU);
                            myPoints.Append(LastPoint);
                            MorePoints = false;
                        }
                        else if (Du * Us3 > myUTol)
                        {
                            Du *= Us3;
                        }
                    }
                    U1 = U2;
                    Dusave = Du;
                }
                
            }

            // Recalage avant dernier point :
            i = myPoints.Length() - 1;
            //  Real d = myPoints (i).Distance (myPoints (i+1));
            // if (Abs(myParameters (i) - myParameters (i+1))<= 0.000001 || d < Precision::Confusion()) {
            //    cout<<"deux points confondus"<<endl;
            //    myParameters.Remove (i+1);
            //    myPoints.Remove (i+1);
            //    i--;
            //  }
            if (i >= 2)
            {
                MiddleU = myParameters[(i - 1)];
                MiddleU = (myLastU + MiddleU) * 0.5;
                D0(theC, MiddleU, ref MiddlePoint);
                myParameters.SetValue(i, MiddleU);
                myPoints.SetValue(i, MiddlePoint);
            }

            //-- On rajoute des points aux milieux des segments si le nombre
            //-- mini de points n'est pas atteint
            //--
            int Nbp = myPoints.Length();

            //std::cout << "GCPnts_TangentialDeflection: Number of Points (" << Nbp << " " << myMinNbPnts << " )" << std::endl;

            while (Nbp < myMinNbPnts)
            {
                for (i = 2; i <= Nbp; i += 2)
                {
                    MiddleU = (myParameters.Value(i - 1) + myParameters.Value(i)) * 0.5;
                    D0(theC, MiddleU, ref MiddlePoint);
                    myParameters.InsertBefore(i, MiddleU);
                    myPoints.InsertBefore(i, MiddlePoint);
                    Nbp++;
                }
            }
            // Additional check for intervals
            double MinLen2 = myMinLen * myMinLen;
            int MaxNbp = 10 * Nbp;
            for (i = 1; i < Nbp; ++i)
            {
                U1 = myParameters[(i)];
                U2 = myParameters[(i + 1)];


                if (U2 - U1 <= myUTol)
                {
                    continue;
                }

                // Check maximal deflection on interval;
                double dmax = 0.0;
                double umax = 0.0;
                double amax = 0.0;
                EstimDefl(theC, U1, U2, ref dmax, ref umax);
                gp_Pnt P1 = myPoints[(i)];
                gp_Pnt P2 = myPoints[(i + 1)];
                D0(theC, umax, ref MiddlePoint);
                amax = EstimAngl(P1, MiddlePoint, P2);
                if (dmax > myCurvatureDeflection || amax > AngleMax)
                {
                    if (umax - U1 > myUTol && U2 - umax > myUTol)
                    {
                        if (P1.SquareDistance(MiddlePoint) > MinLen2
                         && P2.SquareDistance(MiddlePoint) > MinLen2)
                        {
                            myParameters.InsertAfter(i, umax);
                            myPoints.InsertAfter(i, MiddlePoint);
                            ++Nbp;
                            --i; //To compensate ++i in loop header: i must point to first part of split interval
                            if (Nbp > MaxNbp)
                            {
                                break;
                            }
                        }
                    }
                }
            }

        }

        static double EstimAngl(gp_Pnt P1, gp_Pnt Pm, gp_Pnt P2)
        {
            gp_Vec V1 = new(P1, Pm), V2 = new(Pm, P2);
            double L = V1.Magnitude() * V2.Magnitude();
            if (L > gp.Resolution())
            {
                return V1.CrossMagnitude(V2) / L;
            }
            else
            {
                return 0.0;
            }
        }
        // Return number of interval of continuity on which theParam is located.
        // Last parameter is used to increase search speed.
        static int getIntervalIdx(double theParam,
                                               TColStd_Array1OfReal theIntervs,
                                          int thePreviousIdx)
        {
            int anIdx;
            for (anIdx = thePreviousIdx; anIdx < theIntervs.Upper(); anIdx++)
            {
                if (theParam >= theIntervs[(anIdx)] &&
                    theParam <= theIntervs[(anIdx + 1)]) // Inside of anIdx interval.
                {
                    break;
                }
            }
            return anIdx;
        }



        public abstract class AbstractDistFunction : math_Function
        {
            public AbstractDistFunction(ITheCurve c, double U1, double U2)
            {

            }
        }

        void EstimDefl(ITheCurve theC, double theU1, double theU2,
                                           ref double theMaxDefl, ref double theUMax)
        {
            double Du = (myLastU - myFirstu);
            //            
            var aFunc = Activator.CreateInstance(GetDistFunctionType(theC), new object[] { theC, theU1, theU2 }) as AbstractDistFunction;
            //typename GCPnts_TCurveTypes<TheCurve>::DistFunction aFunc(theC, theU1, theU2);
            //
            const int aNbIter = 100;
            double aRelTol = Math.Max(1e-3, 2.0 * myUTol / (Math.Abs(theU1) + Math.Abs(theU2)));
            //
            math_BrentMinimum anOptLoc = new(aRelTol, aNbIter, myUTol);
            anOptLoc.Perform(aFunc, theU1, (theU1 + theU2) / 2.0, theU2);
            if (anOptLoc.IsDone())
            {
                theMaxDefl = Math.Sqrt(-anOptLoc.Minimum());
                theUMax = anOptLoc.Location();
                return;
            }
            //
            math_Vector aLowBorder = new(1, 1), aUppBorder = new(1, 1), aSteps = new(1, 1);
            aSteps[(1)] = Math.Max(0.1 * Du, 100.0 * myUTol);
            int aNbParticles = Math.Max(8, RealToInt(32 * (theU2 - theU1) / Du));
            aLowBorder[(1)] = theU1;
            aUppBorder[(1)] = theU2;
            //
            //
            double aValue = 0.0;
            math_Vector aT = new(1, 1);

            //typename GCPnts_TCurveTypes<TheCurve>::DistFunctionMV aFuncMV(aFunc);
            var aFuncMV = Activator.CreateInstance(GetDistFunctionMVType(theC), new object[] { aFunc }) as math_MultipleVarFunction;

            math_PSO aFinder = new(aFuncMV, aLowBorder, aUppBorder, aSteps, aNbParticles);
            aFinder.Perform(aSteps, ref aValue, ref aT);
            //
            anOptLoc.Perform(aFunc,
                              Math.Max(aT[(1)] - aSteps[(1)], theU1),
                              aT[(1)],
                              Math.Min(aT[(1)] + aSteps[(1)], theU2));
            if (anOptLoc.IsDone())
            {
                theMaxDefl = Math.Sqrt(-anOptLoc.Minimum());
                theUMax = anOptLoc.Location();
                return;
            }

            theMaxDefl = Math.Sqrt(-aValue);
            theUMax = aT[(1)];
        }

        private Type GetDistFunctionType(ITheCurve theC)
        {
            if (theC is Adaptor2d_Curve2d b)
            {

            }
            if (theC is Adaptor3d_Curve a)
            {
                return typeof(GCPnts_DistFunction);
            }
            return null;
        }
        private Type GetDistFunctionMVType(ITheCurve theC)
        {
            if (theC is Adaptor2d_Curve2d b)
            {

            }
            if (theC is Adaptor3d_Curve a)
            {

            }
            return null;
        }
        private int RealToInt(double v)
        {
            return (int)v;
        }

        public static void D2(ITheCurve C, double U,
                             out gp_Pnt P, out gp_Vec V1, out gp_Vec V2)
        {
            if (C is Adaptor3d_Curve a)
            {
                D2(a, U, out P, out V1, out V2);
            }
            else
            if (C is Adaptor2d_Curve2d b)
            {
                D2(b, U, out P, out V1, out V2);
            }
            else
                throw new NotImplementedException();
        }

        public static void D2(Adaptor3d_Curve C, double U,
                              out gp_Pnt P, out gp_Vec V1, out gp_Vec V2)
        {
            C.D2(U, out P, out V1, out V2);
        }

        static void D2(Adaptor2d_Curve2d C, double U,
                        gp_Pnt PP, gp_Vec VV1, gp_Vec VV2)
        {
            double X, Y;
            gp_Pnt2d P;
            gp_Vec2d V1, V2;
            C.D2(U, out P, out V1, out V2);
            P.Coord(out X, out Y);
            PP.SetCoord(X, Y, 0.0);
            V1.Coord(out X, out Y);
            VV1.SetCoord(X, Y, 0.0);
            V2.Coord(out X, out Y);
            VV2.SetCoord(X, Y, 0.0);
        }

        public void EvaluateDu(ITheCurve theC,
                                                double theU,
                                              out gp_Pnt theP,
                                               double theDu,
                                               bool theNotDone)
        {
            gp_Vec T, N;
            D2(theC, theU, out theP, out T, out N);
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


        public static void D0(ITheCurve C, double U, ref gp_Pnt P)
        {
            if (C is Adaptor3d_Curve a)
            {
                D0(a, U, ref P);
            }
            else
            if (C is Adaptor2d_Curve2d b)
            {
                D0(b, U, ref P);
            }
            else
                throw new NotImplementedException();
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

        public void PerformLinear(ITheCurve theC)
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



