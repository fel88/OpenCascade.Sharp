using OCCPort.Common;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using TKernel;
using TKMath;

namespace TKG2d
{
    //! An interface between the services provided by any
    //! curve from the package Geom2d and those required
    //! of the curve by algorithms which use it.
    //!
    //! Polynomial coefficients of BSpline curves used for their evaluation are
    //! cached for better performance. Therefore these evaluations are not
    //! thread-safe and parallel evaluations need to be prevented.
    public class Geom2dAdaptor_Curve : Adaptor2d_Curve2d
    {
        Geom2d_Curve myCurve;
        GeomAbs_CurveType myTypeCurve;
        double myFirst;
        double myLast;

        public override double Period()
        {
            return myCurve.LastParameter() - myCurve.FirstParameter();
        }
        public override gp_Circ2d Circle()
        {
            Exceptions.Standard_NoSuchObject_Raise_if(myTypeCurve != GeomAbs_CurveType.GeomAbs_Circle,
                                               "Geom2dAdaptor_Curve::Circle() - curve is not a Circle");
            return ((Geom2d_Circle)(myCurve)).Circ2d();
        }

        public override Geom2d_BezierCurve Bezier()
        {
            return (Geom2d_BezierCurve)myCurve;
        }

        public override Geom2d_BSplineCurve BSpline()
        {
            return myBSplineCurve;
        }

        Geom2d_BSplineCurve myBSplineCurve; ///< B-spline representation to prevent castings

        public gp_Pnt2d Value(double U)
        {
            gp_Pnt2d aRes = new gp_Pnt2d();
            D0(U, ref aRes);
            return aRes;
        }

        public override double FirstParameter() { return myFirst; }

        public override double LastParameter() { return myLast; }

        public override void D0(double U, ref gp_Pnt2d P)
        {
            //switch (myTypeCurve)
            //{
            //case GeomAbs_BezierCurve:
            //case GeomAbs_BSplineCurve:
            //    {
            //        Standard_Integer aStart = 0, aFinish = 0;
            //        if (IsBoundary(U, aStart, aFinish))
            //        {
            //            myBSplineCurve->LocalD0(U, aStart, aFinish, P);
            //        }
            //        else
            //        {
            //            // use cached data
            //            if (myCurveCache.IsNull() || !myCurveCache->IsCacheValid(U))
            //                RebuildCache(U);
            //            myCurveCache->D0(U, P);
            //        }
            //        break;
            //    }

            //case GeomAbs_OffsetCurve:
            //    myNestedEvaluator->D0(U, P);
            //    break;

            //default:
            myCurve.D0(U, out P);
            //}
        }

        public Geom2dAdaptor_Curve(Geom2d_Curve theCrv)
        {
            myTypeCurve = GeomAbs_CurveType.GeomAbs_OtherCurve;
            myFirst = (0.0);
            myLast = 0.0;

            Load(theCrv);
        }

        public void Load(Geom2d_Curve theCurve)
        {
            if (theCurve == null) { throw new Standard_NullObject(); }
            load(theCurve, theCurve.FirstParameter(), theCurve.LastParameter());
        }

        public Geom2dAdaptor_Curve(Geom2d_Curve theCrv, double theUFirst, double theULast)
        {
            myTypeCurve = GeomAbs_CurveType.GeomAbs_OtherCurve;
            myFirst = theUFirst;
            myLast = theULast;

            Load(theCrv, theUFirst, theULast);
        }

        public Geom2dAdaptor_Curve()
        {
            myTypeCurve = GeomAbs_CurveType.GeomAbs_OtherCurve;
            myFirst = (0.0);
            myLast = 0.0;
        }

        //! Standard_ConstructionError is raised if theUFirst>theULast
        public void Load(Geom2d_Curve theCurve, double theUFirst, double theULast)
        {
            if (theCurve == null)
            {
                throw new Standard_NullObject();
            }
            if (theUFirst > theULast)
            {
                throw new Standard_ConstructionError();
            }
            load(theCurve, theUFirst, theULast);
        }
        void load(Geom2d_Curve C,
                                     double UFirst,
                                     double ULast)
        {
            myFirst = UFirst;
            myLast = ULast;
            //myCurveCache.Nullify();

            if (myCurve != C)
            {
                myCurve = C;
                //myNestedEvaluator.Nullify();
                //myBSplineCurve.Nullify();

                Type TheType = C.GetType();
                //if (TheType == STANDARD_TYPE(Geom2d_TrimmedCurve))
                //{
                //    Load(Handle(Geom2d_TrimmedCurve)::DownCast(C)->BasisCurve(),
                //     UFirst, ULast);
                //}
                //else if (TheType == STANDARD_TYPE(Geom2d_Circle))
                //{
                //    myTypeCurve = GeomAbs_Circle;
                //}
                //else 
                if (C is Geom2d_Line)
                {
                    myTypeCurve = GeomAbs_CurveType.GeomAbs_Line;
                }
                //else if (TheType == STANDARD_TYPE(Geom2d_Ellipse))
                //{
                //    myTypeCurve = GeomAbs_Ellipse;
                //}
                //else if (TheType == STANDARD_TYPE(Geom2d_Parabola))
                //{
                //    myTypeCurve = GeomAbs_Parabola;
                //}
                //else if (TheType == STANDARD_TYPE(Geom2d_Hyperbola))
                //{
                //    myTypeCurve = GeomAbs_Hyperbola;
                //}
                //else if (TheType == STANDARD_TYPE(Geom2d_BezierCurve))
                //{
                //    myTypeCurve = GeomAbs_BezierCurve;
                //}
                else if (C is Geom2d_BSplineCurve)
                {
                    myTypeCurve = GeomAbs_CurveType.GeomAbs_BSplineCurve;
                    myBSplineCurve = (Geom2d_BSplineCurve)myCurve;
                }
                //else if (TheType == STANDARD_TYPE(Geom2d_OffsetCurve))
                //{
                //    myTypeCurve = GeomAbs_OffsetCurve;
                //    Handle(Geom2d_OffsetCurve) anOffsetCurve = Handle(Geom2d_OffsetCurve)::DownCast(myCurve);
                //    // Create nested adaptor for base curve
                //    Handle(Geom2d_Curve) aBaseCurve = anOffsetCurve->BasisCurve();
                //    Handle(Geom2dAdaptor_Curve) aBaseAdaptor = new Geom2dAdaptor_Curve(aBaseCurve);
                //    myNestedEvaluator = new Geom2dEvaluator_OffsetCurve(aBaseAdaptor, anOffsetCurve->Offset());
                //}
                else
                {
                    myTypeCurve = GeomAbs_CurveType.GeomAbs_OtherCurve;
                }
            }
        }

        public override int NbSamples()
        {
            return nbPoints(myCurve);
        }
        public static int nbPoints(Geom2d_Curve theCurve)
        {

            int nbs = 20;

            if (theCurve is Geom2d_Line)
                nbs = 2;
            //else if (theCurve->IsKind(STANDARD_TYPE(Geom2d_BezierCurve)))
            //{
            //    nbs = 3 + Handle(Geom2d_BezierCurve)::DownCast(theCurve)->NbPoles();
            //}
            else if (theCurve is Geom2d_BSplineCurve)
            {
                nbs = ((Geom2d_BSplineCurve)theCurve).NbKnots();
                nbs *= ((Geom2d_BSplineCurve)theCurve).Degree();
                if (nbs < 2.0) nbs = 2;
            }
            //else if (theCurve->IsKind(STANDARD_TYPE(Geom2d_OffsetCurve)))
            //{
            //    Handle(Geom2d_Curve) aCurve = Handle(Geom2d_OffsetCurve)::DownCast(theCurve)->BasisCurve();
            //    return Max(nbs, nbPoints(aCurve));
            //}

            //else if (theCurve->IsKind(STANDARD_TYPE(Geom2d_TrimmedCurve)))
            //{
            //    Handle(Geom2d_Curve) aCurve = Handle(Geom2d_TrimmedCurve)::DownCast(theCurve)->BasisCurve();
            //    return Max(nbs, nbPoints(aCurve));
            //}
            if (nbs > 300)
                nbs = 300;
            return nbs;

        }

        public override GeomAbs_CurveType _GetType()
        {
            return myTypeCurve;
        }

        public override gp_Lin2d Line()
        {
            Exceptions.Standard_NoSuchObject_Raise_if(myTypeCurve != GeomAbs_CurveType.GeomAbs_Line,
                                  "Geom2dAdaptor_Curve::Line() - curve is not a Line");
            return ((Geom2d_Line)myCurve).Lin2d();
        }



        public override int Degree()
        {
            if (myTypeCurve == GeomAbs_CurveType.GeomAbs_BezierCurve)
                return ((Geom2d_BezierCurve)myCurve).Degree();
            else if (myTypeCurve == GeomAbs_CurveType.GeomAbs_BSplineCurve)
                return myBSplineCurve.Degree();
            else
                throw new Standard_NoSuchObject();
        }

        public override int NbKnots()
        {
            if (myTypeCurve != GeomAbs_CurveType.GeomAbs_BSplineCurve)
                throw new Standard_NoSuchObject("Geom2dAdaptor_Curve::NbKnots");
            return myBSplineCurve.NbKnots();
        }

        public override void D1(double U, out gp_Pnt2d P, out gp_Vec2d V)
        {
            switch (myTypeCurve)
            {
                case GeomAbs_CurveType.GeomAbs_BezierCurve:
                //case GeomAbs_CurveType.GeomAbs_BSplineCurve:
                //    {
                //        int aStart = 0, aFinish = 0;
                //        if (IsBoundary(U, aStart, aFinish))
                //        {
                //            myBSplineCurve.LocalD1(U, aStart, aFinish, P, V);
                //        }
                //        else
                //        {
                //            // use cached data
                //            if (myCurveCache.IsNull() || !myCurveCache->IsCacheValid(U))
                //                RebuildCache(U);
                //            myCurveCache->D1(U, P, V);
                //        }
                //        break;
                //    }

                //case GeomAbs_CurveType.GeomAbs_OffsetCurve:
                //    myNestedEvaluator.D1(U, P, V);
                //    break;

                default:
                    myCurve.D1(U, out P, out V);
                    break;
            }
        }

        public override double Resolution(double Ruv)
        {
            switch (myTypeCurve)
            {
                case GeomAbs_CurveType.GeomAbs_Line:
                    return Ruv;
                //case GeomAbs_Circle:
                //    {
                //        Standard_Real R = Handle(Geom2d_Circle)::DownCast(myCurve)->Circ2d().Radius();
                //        if (R > Ruv / 2.)
                //            return 2 * ASin(Ruv / (2 * R));
                //        else
                //            return 2 * M_PI;
                //    }
                //case GeomAbs_Ellipse:
                //    {
                //        return Ruv / Handle(Geom2d_Ellipse)::DownCast(myCurve)->MajorRadius();
                //    }
                //case GeomAbs_BezierCurve:
                //    {
                //        Standard_Real res;
                //        Handle(Geom2d_BezierCurve)::DownCast(myCurve)->Resolution(Ruv, res);
                //        return res;
                //    }
                //case GeomAbs_BSplineCurve:
                //    {
                //        Standard_Real res;
                //        Handle(Geom2d_BSplineCurve)::DownCast(myCurve)->Resolution(Ruv, res);
                //        return res;
                //    }
                default:
                    return Precision.Parametric(Ruv);
            }
        }

        public override bool IsPeriodic()
        {
            return myCurve.IsPeriodic();

        }


        //! Returns the point P of parameter U, the first and second
        //! derivatives V1 and V2.
        //! Raised if the continuity of the current interval
        //! is not C2.
        public override void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2)
        {
            switch (myTypeCurve)
            {
                //case GeomAbs_CurveType.GeomAbs_BezierCurve:
                //case GeomAbs_CurveType.GeomAbs_BSplineCurve:
                //    {
                //        Standard_Integer aStart = 0, aFinish = 0;
                //        if (IsBoundary(U, aStart, aFinish))
                //        {
                //            myBSplineCurve->LocalD2(U, aStart, aFinish, P, V1, V2);
                //        }
                //        else
                //        {
                //            // use cached data
                //            if (myCurveCache.IsNull() || !myCurveCache->IsCacheValid(U))
                //                RebuildCache(U);
                //            myCurveCache->D2(U, P, V1, V2);
                //        }
                //        break;
                //    }

                //case GeomAbs_CurveType.GeomAbs_OffsetCurve:
                //    myNestedEvaluator.D2(U, P, V1, V2);
                //    break;

                default:

                    myCurve.D2(U, out P, out V1, out V2);
                    break;
            }
        }

        public override int NbIntervals(GeomAbs_Shape S)
        {
            int myNbIntervals = 1;
            int NbSplit;
            if (myTypeCurve == GeomAbs_CurveType.GeomAbs_BSplineCurve)
            {
                //Standard_Integer FirstIndex = myBSplineCurve->FirstUKnotIndex();
                //Standard_Integer LastIndex = myBSplineCurve->LastUKnotIndex();
                //TColStd_Array1OfInteger Inter(1, LastIndex - FirstIndex + 1);
                //Standard_Boolean aContPer = (S >= Continuity()) && myBSplineCurve->IsPeriodic();
                //Standard_Boolean aContNotPer = (S > Continuity()) && !myBSplineCurve->IsPeriodic();
                //if (aContPer || aContNotPer)
                //{
                //    Standard_Integer Cont;
                //    switch (S)
                //    {
                //        case GeomAbs_G1:
                //        case GeomAbs_G2:
                //            throw Standard_DomainError("Geom2dAdaptor_Curve::NbIntervals");
                //            break;
                //        case GeomAbs_C0:
                //            myNbIntervals = 1;
                //            break;
                //        case GeomAbs_C1:
                //        case GeomAbs_C2:
                //        case GeomAbs_C3:
                //        case GeomAbs_CN:
                //            {
                //                if (S == GeomAbs_C1) Cont = 1;
                //                else if (S == GeomAbs_C2) Cont = 2;
                //                else if (S == GeomAbs_C3) Cont = 3;
                //                else Cont = myBSplineCurve->Degree();
                //                Standard_Integer Degree = myBSplineCurve->Degree();
                //                Standard_Integer NbKnots = myBSplineCurve->NbKnots();
                //                TColStd_Array1OfInteger Mults(1, NbKnots);
                //                myBSplineCurve->Multiplicities(Mults);
                //                NbSplit = 1;
                //                Standard_Integer Index = FirstIndex;
                //                Inter(NbSplit) = Index;
                //                Index++;
                //                NbSplit++;
                //                while (Index < LastIndex)
                //                {
                //                    if (Degree - Mults(Index) < Cont)
                //                    {
                //                        Inter(NbSplit) = Index;
                //                        NbSplit++;
                //                    }
                //                    Index++;
                //                }
                //                Inter(NbSplit) = Index;

                //                Standard_Integer NbInt = NbSplit - 1;

                //                Standard_Integer Nb = myBSplineCurve->NbKnots();
                //                TColStd_Array1OfReal TK(1, Nb);
                //                TColStd_Array1OfInteger TM(1, Nb);
                //                myBSplineCurve->Knots(TK);
                //                myBSplineCurve->Multiplicities(TM);
                //                Standard_Real Eps = Min(Resolution(Precision::Confusion()),
                //                  Precision::PConfusion());

                //                myNbIntervals = 1;

                //                if (!myBSplineCurve->IsPeriodic())
                //                {
                //                    myNbIntervals = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                      myFirst, myLast, Eps, Standard_False, myNbIntervals);
                //                }
                //                else
                //                {
                //                    Standard_Real aCurFirst = myFirst;
                //                    Standard_Real aCurLast = myLast;

                //                    Standard_Real aLower = myBSplineCurve->FirstParameter();
                //                    Standard_Real anUpper = myBSplineCurve->LastParameter();

                //                    if ((Abs(aCurFirst - aLower) < Eps) && (aCurFirst < aLower))
                //                    {
                //                        aCurFirst = aLower;
                //                    }
                //                    if ((Abs(aCurLast - anUpper) < Eps) && (aCurLast < anUpper))
                //                    {
                //                        aCurLast = anUpper;
                //                    }

                //                    Standard_Real aPeriod = myBSplineCurve->Period();
                //                    Standard_Integer aLPer = 1; Standard_Integer aFPer = 1;

                //                    if ((Abs(aLower - myFirst) < Eps) && (aCurFirst < aLower))
                //                    {
                //                        aCurFirst = aLower;
                //                    }
                //                    else
                //                    {
                //                        DefinFPeriod(aLower, anUpper,
                //                          Eps, aPeriod, aCurFirst, aFPer);
                //                    }
                //                    DefinLPeriod(aLower, anUpper,
                //                      Eps, aPeriod, aCurLast, aLPer);

                //                    if ((Abs(aLower - myFirst) < Eps) && (Abs(anUpper - myLast) < Eps))
                //                    {
                //                        myNbIntervals = NbInt;
                //                    }
                //                    else
                //                    {
                //                        Standard_Integer aSumPer = Abs(aLPer - aFPer);

                //                        Standard_Real aFirst = 0;
                //                        if (aLower < 0 && anUpper == 0)
                //                        {
                //                            if (Abs(aCurLast) < Eps)
                //                            {
                //                                aCurLast = 0;
                //                            }
                //                            aFirst = aLower;
                //                        }

                //                        if (aSumPer <= 1)
                //                        {
                //                            if ((Abs(myFirst - TK(Nb) - aPeriod * (aFPer - 1)) <= Eps) && (myLast < (TK(Nb) + aPeriod * (aLPer - 1))))
                //                            {
                //                                myNbIntervals = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                                  myFirst, myLast, Eps, Standard_True, myNbIntervals, aLower, aPeriod);
                //                                return myNbIntervals;
                //                            }
                //                            if ((Abs(myFirst - aLower) < Eps) && (Abs(myLast - anUpper) < Eps))
                //                            {
                //                                myNbIntervals = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                                  myFirst, myLast, Eps, Standard_True, myNbIntervals, aLower, aPeriod);
                //                                return myNbIntervals;
                //                            }
                //                        }

                //                        if (aSumPer != 0)
                //                        {
                //                            Standard_Integer aFInt = 0;
                //                            Standard_Integer aLInt = 0;
                //                            Standard_Integer aPInt = NbInt;

                //                            if ((aCurFirst != aPeriod) || ((aCurFirst != anUpper) && (Abs(myFirst) < Eps)))
                //                            {
                //                                aFInt = 1;
                //                            }
                //                            if ((aCurLast != 0) && (aCurLast != anUpper))
                //                            {
                //                                aLInt = 1;
                //                            }

                //                            aFInt = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                              aCurFirst, anUpper, Eps, Standard_True, aFInt, aLower, aPeriod);

                //                            if (aCurLast == anUpper)
                //                            {
                //                                aLInt = NbInt;
                //                            }
                //                            else
                //                            {
                //                                if (Abs(aCurLast - aFirst) > Eps)
                //                                {
                //                                    aLInt = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                                      aFirst, aCurLast, Eps, Standard_True, aLInt, aLower, aPeriod, 1);
                //                                }
                //                                else
                //                                {
                //                                    aLInt = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                                      aFirst, aCurLast, Eps, Standard_True, aLInt, aLower, aPeriod);
                //                                }
                //                            }

                //                            myNbIntervals = aFInt + aLInt + aPInt * (aSumPer - 1);
                //                        }
                //                        else
                //                        {
                //                            myNbIntervals = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                              aCurFirst, aCurLast, Eps, Standard_True, myNbIntervals, aLower, aPeriod);
                //                        }
                //                    }
                //                }
                //            }
                //            break;
                //    }
                //}
            }
            else if (myTypeCurve == GeomAbs_CurveType.GeomAbs_OffsetCurve)
            {
                GeomAbs_Shape BaseS = GeomAbs_Shape.GeomAbs_C0;
                switch (S)
                {
                    case GeomAbs_Shape.GeomAbs_G1:
                    case GeomAbs_Shape.GeomAbs_G2:
                        throw new Standard_DomainError("GeomAdaptor_Curve::NbIntervals");
                        break;
                    case GeomAbs_Shape.GeomAbs_C0: BaseS = GeomAbs_Shape.GeomAbs_C1; break;
                    case GeomAbs_Shape.GeomAbs_C1: BaseS = GeomAbs_Shape.GeomAbs_C2; break;
                    case GeomAbs_Shape.GeomAbs_C2: BaseS = GeomAbs_Shape.GeomAbs_C3; break;
                    default: BaseS = GeomAbs_Shape.GeomAbs_CN; break;
                }
                Geom2dAdaptor_Curve anAdaptor = new Geom2dAdaptor_Curve((((Geom2d_OffsetCurve)myCurve).BasisCurve()));
                myNbIntervals = anAdaptor.NbIntervals(BaseS);
            }

            return myNbIntervals;
        }
        public GeomAbs_Shape Continuity()
        {
            if (myTypeCurve == GeomAbs_CurveType.GeomAbs_BSplineCurve)
            {
                throw new NotImplementedException();
//                return LocalContinuity(myFirst, myLast);
            }
            else if (myTypeCurve ==GeomAbs_CurveType. GeomAbs_OffsetCurve)
            {
                throw new NotImplementedException();
                //GeomAbs_Shape S =
               // ((Geom2d_OffsetCurve)(myCurve)).GetBasisCurveContinuity();
                //switch (S)
                //{
                //    case GeomAbs_Shape.GeomAbs_CN: return GeomAbs_Shape.GeomAbs_CN;
                //    case GeomAbs_Shape.GeomAbs_C3: return GeomAbs_Shape.GeomAbs_C2;
                //    case GeomAbs_Shape.GeomAbs_C2: return GeomAbs_Shape.GeomAbs_C1;
                //    case GeomAbs_Shape.GeomAbs_C1: return GeomAbs_Shape.GeomAbs_C0;
                //    case GeomAbs_Shape.GeomAbs_G1: return GeomAbs_Shape.GeomAbs_G1;
                //    case GeomAbs_Shape.GeomAbs_G2: return GeomAbs_Shape.GeomAbs_G2;

                //    default:
                //        throw new Standard_NoSuchObject("Geom2dAdaptor_Curve::Continuity");
                //}
            }

            else if (myTypeCurve == GeomAbs_CurveType.GeomAbs_OtherCurve)
            {
                throw new Standard_NoSuchObject("Geom2dAdaptor_Curve::Continuity");
            }
            else
            {
                return GeomAbs_Shape.GeomAbs_CN;
            }
        }

        public override void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {
            int myNbIntervals = 1;
            int NbSplit;
            if (myTypeCurve == GeomAbs_CurveType.GeomAbs_BSplineCurve)
            {
                int FirstIndex = myBSplineCurve.FirstUKnotIndex();
                int LastIndex = myBSplineCurve.LastUKnotIndex();
                TColStd_Array1OfInteger Inter = new(1, LastIndex - FirstIndex + 1);
                bool aContPer = (S >= Continuity()) && myBSplineCurve.IsPeriodic();
                bool aContNotPer = (S > Continuity()) && !myBSplineCurve.IsPeriodic();
                if (aContPer || aContNotPer)
                {
                    int Cont;
                    switch (S)
                    {
                        case GeomAbs_Shape.GeomAbs_G1:
                        case GeomAbs_Shape.GeomAbs_G2:
                            throw new Standard_DomainError("Geom2dAdaptor_Curve::NbIntervals");
                            break;
                        case GeomAbs_Shape.GeomAbs_C0:
                            myNbIntervals = 1;
                            break;
                        case GeomAbs_Shape.GeomAbs_C1:
                        case GeomAbs_Shape.GeomAbs_C2:
                        case GeomAbs_Shape.GeomAbs_C3:
                        case GeomAbs_Shape.GeomAbs_CN:
                            {
                                if (S == GeomAbs_Shape.GeomAbs_C1) Cont = 1;
                                else if (S == GeomAbs_Shape.GeomAbs_C2) Cont = 2;
                                else if (S == GeomAbs_Shape.GeomAbs_C3) Cont = 3;
                                else Cont = myBSplineCurve.Degree();
                                int Degree = myBSplineCurve.Degree();
                                int NbKnots = myBSplineCurve.NbKnots();
                                TColStd_Array1OfInteger Mults = new(1, NbKnots);
                                //myBSplineCurve.Multiplicities(Mults);
                                NbSplit = 1;
                                int Index = FirstIndex;
                                Inter[NbSplit] = Index;
                                Index++;
                                NbSplit++;
                                while (Index < LastIndex)
                                {
                                    /*if (Degree - Mults(Index) < Cont)
                                    {
                                        Inter(NbSplit) = Index;
                                        NbSplit++;
                                    }*/
                                    Index++;
                                }
                                Inter[NbSplit] = Index;
                                int NbInt = NbSplit - 1;

                                int Nb = myBSplineCurve.NbKnots();
                                int Index1 = 0;
                                int Index2 = 0;
                                double newFirst, newLast;
                                TColStd_Array1OfReal TK = new(1, Nb);
                                TColStd_Array1OfInteger TM = new(1, Nb);
                                //myBSplineCurve.Knots(TK);
                                //  myBSplineCurve.Multiplicities(TM);
                                double Eps = Math.Min(Resolution(Precision.Confusion()),
                                  Precision.PConfusion());

                                if (!myBSplineCurve.IsPeriodic())
                                {
                                    /*BSplCLib.LocateParameter(myBSplineCurve.Degree(), TK, TM, myFirst,
                                      myBSplineCurve.IsPeriodic(),
                                      1, Nb, Index1, newFirst);
                                    BSplCLib.LocateParameter(myBSplineCurve.Degree(), TK, TM, myLast,
                                      myBSplineCurve.IsPeriodic(),
                                      1, Nb, Index2, newLast);*/


                                    // On decale eventuellement les indices  
                                    // On utilise une "petite" tolerance, la resolution ne doit 
                                    // servir que pour les tres longue courbes....(PRO9248)
                                    /*if (Math.Abs(newFirst - TK(Index1 + 1)) < Eps) Index1++;
                                    if (newLast - TK[Index2] > Eps) Index2++;*/

                                    Inter[(1)] = Index1;
                                    myNbIntervals = 1;
                                    for (int i = 1; i <= NbInt; i++)
                                    {
                                        if (Inter[i] > Index1 && Inter[i] < Index2)
                                        {
                                            myNbIntervals++;
                                            Inter[myNbIntervals] = Inter[i];
                                        }
                                    }
                                    Inter[(myNbIntervals + 1)] = Index2;

                                    int ii = T.Lower() - 1;
                                    for (int I = 1; I <= myNbIntervals + 1; I++)
                                    {
                                        T[(ii + I)] = TK[(Inter[(I)])];
                                    }
                                }
                                else
                                {
                                    double aFirst = myFirst;
                                    double aLast = myLast;

                                    double aCurFirst = aFirst;
                                    double aCurLast = aLast;

                                    //double aPeriod = myBSplineCurve.Period();
                                    double aLower = myBSplineCurve.FirstParameter();
                                    double anUpper = myBSplineCurve.LastParameter();

                                    int aLPer = 0; int aFPer = 0;

                                    if (Math.Abs(myFirst - aLower) <= Eps)
                                    {
                                        aCurFirst = aLower;
                                        aFirst = aCurFirst;
                                    }
                                    if (Math.Abs(myLast - anUpper) <= Eps)
                                    {
                                        aCurLast = anUpper;
                                        aLast = aCurLast;
                                    }

                                    if ((Math.Abs(aLower - myFirst) < Eps) && (aCurFirst < aLower))
                                    {
                                        aCurFirst = aLower;
                                    }
                                    else
                                    {
                                        //DefinFPeriod(aLower, anUpper,
                                        //   Eps, aPeriod, aCurFirst, aFPer);
                                    }
                                    //  DefinLPeriod(aLower, anUpper,
                                    //     Eps, aPeriod, aCurLast, aLPer);

                                    if (myFirst == aLower)
                                    {
                                        aFPer = 0;
                                    }

                                    //   SpreadInt(TK, TM, Inter, myBSplineCurve.Degree(), Nb, aFPer, aLPer, NbInt, aLower, myFirst, myLast, aPeriod,
                                    //   aCurLast, Eps, T, myNbIntervals);
                                    T[(T.Lower())] = aFirst;
                                    T[(T.Lower() + myNbIntervals)] = aLast;
                                    return;

                                }
                            }
                            T[(T.Lower())] = myFirst;
                            T[(T.Lower() + myNbIntervals)] = myLast;
                            return;
                    }
                }
            }
            else if (myTypeCurve == GeomAbs_CurveType.GeomAbs_OffsetCurve)
            {
                GeomAbs_Shape BaseS = GeomAbs_Shape.GeomAbs_C0;
                switch (S)
                {
                    case GeomAbs_Shape.GeomAbs_G1:
                    case GeomAbs_Shape.GeomAbs_G2:
                        throw new Standard_DomainError("GeomAdaptor_Curve::NbIntervals");
                        break;
                    case GeomAbs_Shape.GeomAbs_C0: BaseS = GeomAbs_Shape.GeomAbs_C1; break;
                    case GeomAbs_Shape.GeomAbs_C1: BaseS = GeomAbs_Shape.GeomAbs_C2; break;
                    case GeomAbs_Shape.GeomAbs_C2: BaseS = GeomAbs_Shape.GeomAbs_C3; break;
                    default: BaseS = GeomAbs_Shape.GeomAbs_CN; break;
                }

                Geom2dAdaptor_Curve anAdaptor = new(((Geom2d_OffsetCurve)myCurve).BasisCurve());
                myNbIntervals = anAdaptor.NbIntervals(BaseS);
                anAdaptor.Intervals(T, BaseS);
            }

            T[(T.Lower())] = myFirst;
            T[(T.Lower() + myNbIntervals)] = myLast;
        }
    }
}
