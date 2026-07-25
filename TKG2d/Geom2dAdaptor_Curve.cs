using OCCPort.Common;
using System.Security.Cryptography;
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

        public override void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2)
        {
            throw new NotImplementedException();
        }
    }
}
