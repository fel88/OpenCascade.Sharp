using OCCPort.Common;
using System.Reflection.Metadata;
using TKernel;
using TKMath;

namespace TKG3d
{
    //! This class provides an interface between the services provided by any
    //! curve from the package Geom and those required of the curve by algorithms which use it.
    //! Creation of the loaded curve the curve is C1 by piece.
    //!
    //! Polynomial coefficients of BSpline curves used for their evaluation are
    //! cached for better performance. Therefore these evaluations are not
    //! thread-safe and parallel evaluations need to be prevented.
    public class GeomAdaptor_Curve : Adaptor3d_Curve
    {
        public GeomAdaptor_Curve()
        {
            myTypeCurve = GeomAbs_CurveType.GeomAbs_OtherCurve;
            myFirst = 0;
            myLast = 0;
        }

        public GeomAdaptor_Curve(Geom_Curve theCurve)
        {
            Load(theCurve);
        }

        public override double Period()
        {
            return myCurve.LastParameter() - myCurve.FirstParameter();
        }

        public override double FirstParameter()
        {
            return myFirst;
        }

        public override double LastParameter()
        {
            return myLast;
        }

        public override gp_Lin Line()
        {
            Standard_NoSuchObject_Raise_if(myTypeCurve != GeomAbs_CurveType.GeomAbs_Line,
                                            "GeomAdaptor_Curve::Line() - curve is not a Line");
            return ((Geom_Line)myCurve).Lin();
        }



        private void Standard_NoSuchObject_Raise_if(bool v1, string v2)
        {
            if (v1)
                throw new Exception(v2);
        }

        public override GeomAbs_CurveType _GetType() { return myTypeCurve; }

        //! Standard_ConstructionError is raised if theUFirst>theULast
        public GeomAdaptor_Curve(Geom_Curve theCurve,
                      double theUFirst,
                      double theULast)
        {
            Load(theCurve, theUFirst, theULast);
        }

        [NotOrigin]
        public GeomAdaptor_Curve(GeomAdaptor_Curve aGACurve)
        {
            myCurve = aGACurve.myCurve;
            myBSplineCurve = aGACurve.myBSplineCurve;
            myFirst = aGACurve.myFirst;
            myLast = aGACurve.myLast;
            myNestedEvaluator = aGACurve.myNestedEvaluator;
            myTypeCurve = aGACurve.myTypeCurve;
        }

        GeomAbs_CurveType myTypeCurve;

        //! Standard_ConstructionError is raised if theUFirst>theULast
        public void Load(Geom_Curve theCurve,
             double theUFirst,
             double theULast)
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

        public void Load(Geom_Curve theCurve)
        {
            if (theCurve == null)
            {
                throw new Standard_NullObject();
            }
            load(theCurve, theCurve.FirstParameter(), theCurve.LastParameter());
        }

        double myFirst;
        double myLast;
        Geom_Curve myCurve;
        GeomEvaluator_Curve myNestedEvaluator; ///< Calculates value of offset curve
		Geom_BSplineCurve myBSplineCurve; ///< B-spline representation to prevent castings

        public void load(Geom_Curve C,
                             double UFirst,
                             double ULast)
        {
            myFirst = UFirst;
            myLast = ULast;
            //myCurveCache.Nullify();

            if (myCurve != C)
            {
                myCurve = C;
                myNestedEvaluator = null;
                myBSplineCurve = null;

                var TheType = C.GetType();
                if (TheType == typeof(Geom_TrimmedCurve))
                {
                    Load((C as Geom_TrimmedCurve).BasisCurve(), UFirst, ULast);
                }
                else
                if (TheType == typeof(Geom_Circle))
                {
                    myTypeCurve = GeomAbs_CurveType.GeomAbs_Circle;
                }
                else
                if (TheType == typeof(Geom_Line))
                {
                    myTypeCurve = GeomAbs_CurveType.GeomAbs_Line;
                }
                //else if (TheType == STANDARD_TYPE(Geom_Ellipse))
                //{
                //	myTypeCurve = GeomAbs_CurveType.GeomAbs_Ellipse;
                //}
                //else if (TheType == STANDARD_TYPE(Geom_Parabola))
                //{
                //	myTypeCurve = GeomAbs_CurveType.GeomAbs_Parabola;
                //}
                //else if (TheType == STANDARD_TYPE(Geom_Hyperbola))
                //{
                //	myTypeCurve = GeomAbs_CurveType.GeomAbs_Hyperbola;
                //}
                //else if (TheType == STANDARD_TYPE(Geom_BezierCurve))
                //{
                //	myTypeCurve = GeomAbs_CurveType.GeomAbs_BezierCurve;
                //}
                //else if (TheType == STANDARD_TYPE(Geom_BSplineCurve))
                //{
                //	myTypeCurve = GeomAbs_CurveType.GeomAbs_BSplineCurve;
                //	myBSplineCurve = Handle(Geom_BSplineCurve)::DownCast(myCurve);
                //}
                //else if (TheType == STANDARD_TYPE(Geom_OffsetCurve))
                //{
                //	myTypeCurve = GeomAbs_CurveType.GeomAbs_OffsetCurve;
                //	Geom_OffsetCurve anOffsetCurve = Handle(Geom_OffsetCurve)::DownCast(myCurve);
                //	// Create nested adaptor for base curve
                //	Geom_Curve aBaseCurve = anOffsetCurve.BasisCurve();
                //	GeomAdaptor_Curve aBaseAdaptor = new GeomAdaptor_Curve(aBaseCurve);
                //	myNestedEvaluator = new GeomEvaluator_OffsetCurve(
                //		aBaseAdaptor, anOffsetCurve.Offset(), anOffsetCurve.Direction());
                //}
                else
                {
                    myTypeCurve = GeomAbs_CurveType.GeomAbs_OtherCurve;
                }
            }
        }

        public override int Degree()
        {
            throw new NotImplementedException();
        }

        public override int NbKnots()
        {
            throw new NotImplementedException();
        }

        public override Geom_BSplineCurve BSpline()
        {
            throw new NotImplementedException();
        }

        public override gp_Pnt Value(double U)
        {
            gp_Pnt aValue = new gp_Pnt();
            D0(U, ref aValue);
            return aValue;
        }

        BSplCLib_Cache myCurveCache; ///< Cached data for B-spline or Bezier curve


        public void RebuildCache(double theParameter)
        {
            if (myTypeCurve == GeomAbs_CurveType.GeomAbs_BezierCurve)
            {
                // Create cache for Bezier
                Geom_BezierCurve aBezier = (Geom_BezierCurve)myCurve;
                //int aDeg = aBezier.Degree();
                //TColStd_Array1OfReal aFlatKnots(BSplCLib.FlatBezierKnots(aDeg), 1, 2 * (aDeg + 1));
                //if (myCurveCache == null)
                //    myCurveCache = new BSplCLib_Cache(aDeg, aBezier->IsPeriodic(), aFlatKnots,
                //      aBezier->Poles(), aBezier->Weights());
                //myCurveCache->BuildCache(theParameter, aFlatKnots, aBezier->Poles(), aBezier->Weights());
            }
            else if (myTypeCurve == GeomAbs_CurveType.GeomAbs_BSplineCurve)
            {
                // Create cache for B-spline
                //if (myCurveCache == null)
                //    myCurveCache = new BSplCLib_Cache(myBSplineCurve->Degree(), myBSplineCurve->IsPeriodic(),
                //      myBSplineCurve->KnotSequence(), myBSplineCurve->Poles(), myBSplineCurve->Weights());
                //myCurveCache->BuildCache(theParameter, myBSplineCurve->KnotSequence(),
                //                          myBSplineCurve->Poles(), myBSplineCurve->Weights());
            }
        }
        public override void D0(double U, ref gp_Pnt P)
        {
            switch (myTypeCurve)
            {
                case GeomAbs_CurveType.GeomAbs_BezierCurve:
                case GeomAbs_CurveType.GeomAbs_BSplineCurve:
                    {
                        int aStart = 0, aFinish = 0;
                        /*if (IsBoundary(U, aStart, aFinish))
                        {
                            myBSplineCurve.LocalD0(U, aStart, aFinish, P);
                        }
                        else*/
                        {
                            // use cached data
                            //if (myCurveCache == null || !myCurveCache.IsCacheValid(U))
                            RebuildCache(U);
                            myCurveCache.D0(U, ref P);
                        }
                        break;
                    }

                case GeomAbs_CurveType.GeomAbs_OffsetCurve:
                    //myNestedEvaluator.D0(U, ref P);
                    break;

                default:
                    myCurve.D0(U, ref P);
                    break;
            }
        }

        public override double Resolution(double R3D)
        {
            switch (myTypeCurve)
            {
                case GeomAbs_CurveType.GeomAbs_Line:
                    return R3D;
                case GeomAbs_CurveType.GeomAbs_Circle:
                    {
                        double R = ((Geom_Circle)(myCurve)).Circ().Radius();
                        if (R > R3D / 2.0 )
                            return 2 * Math.Asin(R3D / (2 * R));
                        else
                            return 2 * Math.PI;
                    }
                //case GeomAbs_Ellipse:
                //    {
                //        return R3D / Handle(Geom_Ellipse)::DownCast(myCurve)->MajorRadius();
                //    }
                //case GeomAbs_BezierCurve:
                //    {
                //        Standard_Real res;
                //        Handle(Geom_BezierCurve)::DownCast(myCurve)->Resolution(R3D, res);
                //        return res;
                //    }
                //case GeomAbs_BSplineCurve:
                //    {
                //        Standard_Real res;
                //        myBSplineCurve->Resolution(R3D, res);
                //        return res;
                //    }
                default:
                    return Precision.Parametric(R3D);
            }
        }

        public override bool IsPeriodic()
        {
            return myCurve.IsPeriodic();            
        }

        public override int NbIntervals(GeomAbs_Shape S)
        {
            int myNbIntervals = 1;
            int NbSplit;
            if (myTypeCurve == GeomAbs_CurveType.GeomAbs_BSplineCurve)
            {
                //    int FirstIndex = myBSplineCurve.FirstUKnotIndex();
                //    int LastIndex = myBSplineCurve.LastUKnotIndex();
                //    TColStd_Array1OfInteger Inter(1, LastIndex - FirstIndex + 1);
                //    bool aContPer = (S >= Continuity()) && myBSplineCurve->IsPeriodic();
                //    bool aContNotPer = (S > Continuity()) && !myBSplineCurve->IsPeriodic();

                //    if (aContPer || aContNotPer)
                //    {
                //        int Cont;
                //        switch (S)
                //        {
                //            case GeomAbs_Shape. GeomAbs_G1:
                //            case GeomAbs_Shape.GeomAbs_G2:
                //                throw new Standard_DomainError("GeomAdaptor_Curve::NbIntervals");
                //                break;
                //            case GeomAbs_Shape.GeomAbs_C0:
                //                myNbIntervals = 1;
                //                break;
                //            case GeomAbs_Shape.GeomAbs_C1:
                //            case GeomAbs_Shape.GeomAbs_C2:
                //                case GeomAbs_Shape.GeomAbs_C3:
                //            case GeomAbs_Shape.GeomAbs_CN:
                //                {
                //                    if (S == GeomAbs_Shape.GeomAbs_C1) Cont = 1;
                //                    else if (S == GeomAbs_Shape.GeomAbs_C2) Cont = 2;
                //                    else if (S == GeomAbs_Shape.GeomAbs_C3) Cont = 3;
                //                    else Cont = myBSplineCurve->Degree();
                //                    int Degree = myBSplineCurve->Degree();
                //                    int NbKnots = myBSplineCurve->NbKnots();
                //                    TColStd_Array1OfInteger Mults(1, NbKnots);
                //                    myBSplineCurve->Multiplicities(Mults);
                //                    NbSplit = 1;
                //                    Standard_Integer Index = FirstIndex;
                //                    Inter(NbSplit) = Index;
                //                    Index++;
                //                    NbSplit++;
                //                    while (Index < LastIndex)
                //                    {
                //                        if (Degree - Mults(Index) < Cont)
                //                        {
                //                            Inter(NbSplit) = Index;
                //                            NbSplit++;
                //                        }
                //                        Index++;
                //                    }
                //                    Inter(NbSplit) = Index;

                //                    Standard_Integer NbInt = NbSplit - 1;

                //                    Standard_Integer Nb = myBSplineCurve->NbKnots();
                //                    Standard_Integer Index1 = 0;
                //                    Standard_Integer Index2 = 0;
                //                    const TColStd_Array1OfReal&TK = myBSplineCurve->Knots();
                //                    const TColStd_Array1OfInteger&TM = myBSplineCurve->Multiplicities();
                //                    Standard_Real Eps = Min(Resolution(Precision::Confusion()),
                //                      Precision::PConfusion());

                //                    myNbIntervals = 1;

                //                    if (!myBSplineCurve->IsPeriodic())
                //                    {
                //                        myNbIntervals = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                          myFirst, myLast, Eps, Standard_False, myNbIntervals);
                //                    }
                //                    else
                //                    {
                //                        Standard_Real aCurFirst = myFirst;
                //                        Standard_Real aCurLast = myLast;
                //                        Standard_Real aLower = myBSplineCurve->FirstParameter();
                //                        Standard_Real anUpper = myBSplineCurve->LastParameter();

                //                        if ((Abs(aCurFirst - aLower) < Eps) && (aCurFirst < aLower))
                //                        {
                //                            aCurFirst = aLower;
                //                        }
                //                        if ((Abs(aCurLast - anUpper) < Eps) && (aCurLast < anUpper))
                //                        {
                //                            aCurLast = anUpper;
                //                        }

                //                        Standard_Real aPeriod = myBSplineCurve->Period();
                //                        Standard_Integer aLPer = 1; Standard_Integer aFPer = 1;
                //                        if ((Abs(aLower - myFirst) < Eps) && (aCurFirst < aLower))
                //                        {
                //                            aCurFirst = aLower;
                //                        }
                //                        else
                //                        {
                //                            DefinFPeriod(aLower, anUpper,
                //                              Eps, aPeriod, aCurFirst, aFPer);
                //                        }
                //                        DefinLPeriod(aLower, anUpper,
                //                          Eps, aPeriod, aCurLast, aLPer);

                //                        Standard_Real aNewFirst;
                //                        Standard_Real aNewLast;
                //                        BSplCLib::LocateParameter(myBSplineCurve->Degree(), TK, TM, myFirst,
                //                          Standard_True, 1, Nb, Index1, aNewFirst);
                //                        BSplCLib::LocateParameter(myBSplineCurve->Degree(), TK, TM, myLast,
                //                          Standard_True, 1, Nb, Index2, aNewLast);
                //                        if ((aNewFirst == myFirst && aNewLast == myLast) && (aFPer != aLPer))
                //                        {
                //                            myNbIntervals = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                              myFirst, myLast, Eps, Standard_True, myNbIntervals, aLower, aPeriod);
                //                        }
                //                        else
                //                        {
                //                            Standard_Integer aSumPer = Abs(aLPer - aFPer);

                //                            Standard_Real aFirst = 0;
                //                            if (aLower < 0 && anUpper == 0)
                //                            {
                //                                if (Abs(aCurLast) < Eps)
                //                                {
                //                                    aCurLast = 0;
                //                                }
                //                                aFirst = aLower;
                //                            }

                //                            if (aSumPer <= 1)
                //                            {
                //                                if ((Abs(myFirst - TK(Nb) - aPeriod * (aFPer - 1)) <= Eps) && (myLast < (TK(Nb) + aPeriod * (aLPer - 1))))
                //                                {
                //                                    myNbIntervals = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                                      myFirst, myLast, Eps, Standard_True, myNbIntervals, aLower, aPeriod);
                //                                    return myNbIntervals;
                //                                }
                //                                if ((Abs(myFirst - aLower) < Eps) && (Abs(myLast - anUpper) < Eps))
                //                                {
                //                                    myNbIntervals = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                                      myFirst, myLast, Eps, Standard_True, myNbIntervals, aLower, aPeriod);
                //                                    return myNbIntervals;
                //                                }
                //                            }

                //                            if (aSumPer != 0)
                //                            {
                //                                Standard_Integer aFInt = 0;
                //                                Standard_Integer aLInt = 0;
                //                                Standard_Integer aPInt = NbInt;

                //                                if ((aCurFirst != aPeriod) || ((aCurFirst != anUpper) && (Abs(myFirst) < Eps)))
                //                                {
                //                                    aFInt = 1;
                //                                }
                //                                if ((aCurLast != aLower) && (aCurLast != anUpper))
                //                                {
                //                                    aLInt = 1;
                //                                }

                //                                aFInt = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                                  aCurFirst, anUpper, Eps, Standard_True, aFInt, aLower, aPeriod);

                //                                if (aCurLast == anUpper)
                //                                {
                //                                    aLInt = NbInt;
                //                                }
                //                                else
                //                                {
                //                                    if (Abs(aCurLast - aFirst) > Eps)
                //                                    {
                //                                        aLInt = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                                          aFirst, aCurLast, Eps, Standard_True, aLInt, aLower, aPeriod, 1);
                //                                    }
                //                                    else
                //                                    {
                //                                        aLInt = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                                          aFirst, aCurLast, Eps, Standard_True, aLInt, aLower, aPeriod);
                //                                    }
                //                                }

                //                                myNbIntervals = aFInt + aLInt + aPInt * (aSumPer - 1);
                //                            }
                //                            else
                //                            {
                //                                myNbIntervals = LocalNbIntervals(TK, TM, Inter, Degree, Nb, NbInt,
                //                                  aCurFirst, aCurLast, Eps, Standard_True, myNbIntervals, aLower, aPeriod);
                //                            }
                //                        }
                //                    }
                //                }
                //                break;
                //        }
                //    }
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
                    default:
                        BaseS = GeomAbs_Shape.GeomAbs_CN;
                        break;
                }
                //GeomAdaptor_Curve C
                //  (Handle(Geom_OffsetCurve)::DownCast(myCurve)->BasisCurve());
                //// akm 05/04/02 (OCC278)  If our curve is trimmed we must recalculate 
                ////                    the number of intervals obtained from the basis to
                ////              vvv   reflect parameter bounds
                //Standard_Integer iNbBasisInt = C.NbIntervals(BaseS), iInt;
                //if (iNbBasisInt > 1)
                //{
                //    TColStd_Array1OfReal rdfInter(1,1 + iNbBasisInt);
                //    C.Intervals(rdfInter, BaseS);
                //    for (iInt = 1; iInt <= iNbBasisInt; iInt++)
                //        if (rdfInter(iInt) > myFirst && rdfInter(iInt) < myLast)
                //            myNbIntervals++;
                //}
                // akm 05/04/02 ^^^
            }
            return myNbIntervals;
        }

        public override void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {

            {
                int myNbIntervals = 1;
                int NbSplit;
                double FirstParam = myFirst, LastParam = myLast;

                //if (myTypeCurve == GeomAbs_BSplineCurve)
                //{
                //    Standard_Integer FirstIndex = myBSplineCurve->FirstUKnotIndex();
                //    Standard_Integer LastIndex = myBSplineCurve->LastUKnotIndex();
                //    TColStd_Array1OfInteger Inter(1, LastIndex - FirstIndex + 1);
                //    Standard_Boolean aContPer = (S >= Continuity()) && myBSplineCurve->IsPeriodic();
                //    Standard_Boolean aContNotPer = (S > Continuity()) && !myBSplineCurve->IsPeriodic();

                //    if (aContPer || aContNotPer)
                //    {
                //        Standard_Integer Cont;
                //        switch (S)
                //        {
                //            case GeomAbs_G1:
                //            case GeomAbs_G2:
                //                throw Standard_DomainError("Geom2dAdaptor_Curve::NbIntervals");
                //                break;
                //            case GeomAbs_C0:
                //                myNbIntervals = 1;
                //                break;
                //            case GeomAbs_C1:
                //            case GeomAbs_C2:
                //            case GeomAbs_C3:
                //            case GeomAbs_CN:
                //                {
                //                    if (S == GeomAbs_C1) Cont = 1;
                //                    else if (S == GeomAbs_C2) Cont = 2;
                //                    else if (S == GeomAbs_C3) Cont = 3;
                //                    else Cont = myBSplineCurve->Degree();
                //                    Standard_Integer Degree = myBSplineCurve->Degree();
                //                    Standard_Integer NbKnots = myBSplineCurve->NbKnots();
                //                    TColStd_Array1OfInteger Mults(1, NbKnots);
                //                    myBSplineCurve->Multiplicities(Mults);
                //                    NbSplit = 1;
                //                    Standard_Integer Index = FirstIndex;
                //                    Inter(NbSplit) = Index;
                //                    Index++;
                //                    NbSplit++;
                //                    while (Index < LastIndex)
                //                    {
                //                        if (Degree - Mults(Index) < Cont)
                //                        {
                //                            Inter(NbSplit) = Index;
                //                            NbSplit++;
                //                        }
                //                        Index++;
                //                    }
                //                    Inter(NbSplit) = Index;
                //                    Standard_Integer NbInt = NbSplit - 1;
                //                    //        GeomConvert_BSplineCurveKnotSplitting Convector(myBspl, Cont);
                //                    //        Standard_Integer NbInt = Convector.NbSplits()-1;
                //                    //        TColStd_Array1OfInteger Inter(1,NbInt+1);
                //                    //        Convector.Splitting( Inter);

                //                    Standard_Integer Nb = myBSplineCurve->NbKnots();
                //                    Standard_Integer Index1 = 0;
                //                    Standard_Integer Index2 = 0;
                //                    Standard_Real newFirst, newLast;
                //                    const TColStd_Array1OfReal&TK = myBSplineCurve->Knots();
                //                    const TColStd_Array1OfInteger&TM = myBSplineCurve->Multiplicities();
                //                    Standard_Real Eps = Min(Resolution(Precision::Confusion()),
                //                      Precision::PConfusion());

                //                    if (!myBSplineCurve->IsPeriodic() || ((Abs(myFirst - myBSplineCurve->FirstParameter()) < Eps) &&
                //                      (Abs(myLast - myBSplineCurve->LastParameter()) < Eps)))
                //                    {
                //                        BSplCLib::LocateParameter(myBSplineCurve->Degree(), TK, TM, myFirst,
                //                          myBSplineCurve->IsPeriodic(),
                //                          1, Nb, Index1, newFirst);
                //                        BSplCLib::LocateParameter(myBSplineCurve->Degree(), TK, TM, myLast,
                //                          myBSplineCurve->IsPeriodic(),
                //                          1, Nb, Index2, newLast);
                //                        FirstParam = newFirst;
                //                        LastParam = newLast;
                //                        // Protection against myFirst = UFirst - eps, which located as ULast - eps
                //                        if (myBSplineCurve->IsPeriodic() && (LastParam - FirstParam) < Precision::PConfusion())
                //                        {
                //                            if (Abs(LastParam - myBSplineCurve->FirstParameter()) < Precision::PConfusion())
                //                                LastParam += myBSplineCurve->Period();
                //                            else
                //                                FirstParam -= myBSplineCurve->Period();
                //                        }
                //                        // On decale eventuellement les indices  
                //                        // On utilise une "petite" tolerance, la resolution ne doit 
                //                        // servir que pour les tres longue courbes....(PRO9248)

                //                        if (Abs(FirstParam - TK(Index1 + 1)) < Eps) Index1++;
                //                        if (LastParam - TK(Index2) > Eps) Index2++;

                //                        myNbIntervals = 1;

                //                        TColStd_Array1OfInteger aFinalIntervals(1, Inter.Upper());
                //                        aFinalIntervals(1) = Index1;
                //                        for (Standard_Integer i = 1; i <= NbInt; i++)
                //                        {
                //                            if (Inter(i) > Index1 && Inter(i) < Index2)
                //                            {
                //                                myNbIntervals++;
                //                                aFinalIntervals(myNbIntervals) = Inter(i);
                //                            }
                //                        }
                //                        aFinalIntervals(myNbIntervals + 1) = Index2;

                //                        for (Standard_Integer I = 1; I <= myNbIntervals + 1; I++)
                //                        {
                //                            T(I) = TK(aFinalIntervals(I));
                //                        }
                //                    }
                //                    else
                //                    {
                //                        Standard_Real aFirst = myFirst;
                //                        Standard_Real aLast = myLast;

                //                        Standard_Real aCurFirst = aFirst;
                //                        Standard_Real aCurLast = aLast;

                //                        Standard_Real aPeriod = myBSplineCurve->Period();
                //                        Standard_Real aLower = myBSplineCurve->FirstParameter();
                //                        Standard_Real anUpper = myBSplineCurve->LastParameter();

                //                        Standard_Integer aLPer = 0; Standard_Integer aFPer = 0;

                //                        if (Abs(myFirst - aLower) <= Eps)
                //                        {
                //                            aCurFirst = aLower;
                //                            aFirst = aCurFirst;
                //                        }

                //                        if (Abs(myLast - anUpper) <= Eps)
                //                        {
                //                            aCurLast = anUpper;
                //                            aLast = aCurLast;
                //                        }

                //                        if ((Abs(aLower - myFirst) < Eps) && (aCurFirst < aLower))
                //                        {
                //                            aCurFirst = aLower;
                //                        }
                //                        else
                //                        {
                //                            DefinFPeriod(aLower, anUpper,
                //                              Eps, aPeriod, aCurFirst, aFPer);
                //                        }
                //                        DefinLPeriod(aLower, anUpper,
                //                          Eps, aPeriod, aCurLast, aLPer);

                //                        if (myFirst == aLower)
                //                        {
                //                            aFPer = 0;
                //                        }

                //                        SpreadInt(TK, TM, Inter, myBSplineCurve->Degree(), Nb, aFPer, aLPer, NbInt, aLower, myFirst, myLast, aPeriod,
                //                          aCurLast, Eps, T, myNbIntervals);

                //                        T(T.Lower()) = aFirst;
                //                        T(T.Lower() + myNbIntervals) = aLast;
                //                        return;
                //                    }
                //                }
                //                T(T.Lower()) = myFirst;
                //                T(T.Lower() + myNbIntervals) = myLast;
                //                return;
                //        }
                //    }
                //}

                //else if (myTypeCurve == GeomAbs_OffsetCurve)
                //{
                //    GeomAbs_Shape BaseS = GeomAbs_C0;
                //    switch (S)
                //    {
                //        case GeomAbs_G1:
                //        case GeomAbs_G2:
                //            throw Standard_DomainError("GeomAdaptor_Curve::NbIntervals");
                //            break;
                //        case GeomAbs_C0: BaseS = GeomAbs_C1; break;
                //        case GeomAbs_C1: BaseS = GeomAbs_C2; break;
                //        case GeomAbs_C2: BaseS = GeomAbs_C3; break;
                //        default: BaseS = GeomAbs_CN;
                //    }
                //    GeomAdaptor_Curve C
                //      (Handle(Geom_OffsetCurve)::DownCast(myCurve)->BasisCurve());
                //    // akm 05/04/02 (OCC278)  If our curve is trimmed we must recalculate 
                //    //                    the array of intervals obtained from the basis to
                //    //              vvv   reflect parameter bounds
                //    Standard_Integer iNbBasisInt = C.NbIntervals(BaseS), iInt;
                //    if (iNbBasisInt > 1)
                //    {
                //        TColStd_Array1OfReal rdfInter(1,1 + iNbBasisInt);
                //        C.Intervals(rdfInter, BaseS);
                //        for (iInt = 1; iInt <= iNbBasisInt; iInt++)
                //            if (rdfInter(iInt) > myFirst && rdfInter(iInt) < myLast)
                //                T(++myNbIntervals) = rdfInter(iInt);
                //    }
                //    // old - myNbIntervals = C.NbIntervals(BaseS);
                //    // old - C.Intervals(T, BaseS);
                //    // akm 05/04/02 ^^^
                //}

                T[(T.Lower())] = FirstParam;
                T[(T.Lower() + myNbIntervals)] = LastParam;
            }

        }

        public override gp_Circ Circle()
        {
            Standard_NoSuchObject_Raise_if(myTypeCurve != GeomAbs_CurveType.GeomAbs_Circle,
                                  "GeomAdaptor_Curve::Circle() - curve is not a Circle");
            return ((Geom_Circle)(myCurve)).Circ();
        }

        public override void D1(double d, out gp_Pnt p, out gp_Vec v)
        {
            throw new NotImplementedException();
        }

        public override void D2(double d, out gp_Pnt p, out gp_Vec v1, out gp_Vec v2)
        {
            throw new NotImplementedException();
        }
    }
}
