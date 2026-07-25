using OCCPort.Common;
using System.Reflection.Metadata;
using TKernel;
using TKMath;

namespace TKG3d
{
    //! An interface between the services provided by any
    //! surface from the package Geom and those required
    //! of the surface by algorithms which use it.
    //! Creation of the loaded surface the surface is C1 by piece
    //!
    //! Polynomial coefficients of BSpline surfaces used for their evaluation are
    //! cached for better performance. Therefore these evaluations are not
    //! thread-safe and parallel evaluations need to be prevented.
    public class GeomAdaptor_Surface : Adaptor3d_Surface
    {
        public GeomAdaptor_Surface()
        {
            myUFirst = (0.0);
            myULast = (0.0);
            myVFirst = (0.0);
            myVLast = (0.0);
            myTolU = (0.0);
            myTolV = (0.0);
            mySurfaceType = GeomAbs_SurfaceType.GeomAbs_OtherSurface;
        }

        GeomEvaluator_Surface myNestedEvaluator; ///< Calculates values of nested complex surfaces (offset surface, surface of extrusion or revolution)


        //! Returns the  intervals with the requested continuity
        //! in the U direction.
        public override void UIntervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {
            int myNbUIntervals = 1;

            switch (mySurfaceType)
            {
                //case GeomAbs_SurfaceType.GeomAbs_BSplineSurface:
                //    {
                //        GeomAdaptor_Curve myBasisCurve
                //          (myBSplineSurface->VIso(myBSplineSurface->VKnot(myBSplineSurface->FirstVKnotIndex())),myUFirst,myULast);
                //        myNbUIntervals = myBasisCurve.NbIntervals(S);
                //        myBasisCurve.Intervals(T, S);
                //        return;
                //    }
                //case GeomAbs_SurfaceType.GeomAbs_SurfaceOfExtrusion:
                //    {
                //        GeomAdaptor_Curve myBasisCurve
                //          (Handle(Geom_SurfaceOfLinearExtrusion)::DownCast(mySurface)->BasisCurve(),myUFirst,myULast);
                //        if (myBasisCurve.GetType() == GeomAbs_BSplineCurve)
                //        {
                //            myNbUIntervals = myBasisCurve.NbIntervals(S);
                //            myBasisCurve.Intervals(T, S);
                //            return;
                //        }
                //        break;
                //    }
                case GeomAbs_SurfaceType.GeomAbs_OffsetSurface:
                    {
                        GeomAbs_Shape BaseS = GeomAbs_Shape.GeomAbs_CN;
                        switch (S)
                        {
                            case GeomAbs_Shape.GeomAbs_G1:
                            case GeomAbs_Shape.GeomAbs_G2: throw new Standard_DomainError("GeomAdaptor_Curve::UIntervals");
                            case GeomAbs_Shape.GeomAbs_C0: BaseS = GeomAbs_Shape.GeomAbs_C1; break;
                            case GeomAbs_Shape.GeomAbs_C1: BaseS = GeomAbs_Shape.GeomAbs_C2; break;
                            case GeomAbs_Shape.GeomAbs_C2: BaseS = GeomAbs_Shape.GeomAbs_C3; break;
                            case GeomAbs_Shape.GeomAbs_C3:
                            case GeomAbs_Shape.GeomAbs_CN: break;
                        }
                        Geom_OffsetSurface myOffSurf = (Geom_OffsetSurface)(mySurface);
                        GeomAdaptor_Surface Sur = new(myOffSurf.BasisSurface(), myUFirst, myULast, myVFirst, myVLast);
                        myNbUIntervals = Sur.NbUIntervals(BaseS);
                        Sur.UIntervals(T, BaseS);
                        return;
                    }
                case GeomAbs_SurfaceType.GeomAbs_Plane:
                case GeomAbs_SurfaceType.GeomAbs_Cylinder:
                case GeomAbs_SurfaceType.GeomAbs_Cone:
                case GeomAbs_SurfaceType.GeomAbs_Sphere:
                case GeomAbs_SurfaceType.GeomAbs_Torus:
                case GeomAbs_SurfaceType.GeomAbs_BezierSurface:
                case GeomAbs_SurfaceType.GeomAbs_OtherSurface:
                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution: break;
            }

            T[(T.Lower())] = myUFirst;
            T[(T.Lower() + myNbUIntervals)] = myULast;
        }

        public gp_Pnt Value(double U,
                                 double V)
        {
            gp_Pnt aValue = new gp_Pnt();
            D0(U, V, ref aValue);
            return aValue;
        }

        public override gp_Cylinder Cylinder()
        {
            if (mySurfaceType != GeomAbs_SurfaceType.GeomAbs_Cylinder)
                throw new Standard_NoSuchObject("GeomAdaptor_Surface::Cylinder");
            return ((Geom_CylindricalSurface)(mySurface)).Cylinder();
        }

        public override gp_Sphere Sphere()
        {
            if (mySurfaceType != GeomAbs_SurfaceType.GeomAbs_Sphere)
                throw new Standard_NoSuchObject("GeomAdaptor_Surface::Sphere");
            return ((Geom_SphericalSurface)mySurface).Sphere();
        }

        public override Adaptor3d_Surface BasisSurface()
        {
            if (mySurfaceType != GeomAbs_SurfaceType.GeomAbs_OffsetSurface)
                throw new Standard_NoSuchObject("GeomAdaptor_Surface::BasisSurface");
            return new GeomAdaptor_Surface
              (((Geom_OffsetSurface)mySurface).BasisSurface(), myUFirst, myULast, myVFirst, myVLast);
        }

        public override gp_Pln Plane()
        {
            if (mySurfaceType != GeomAbs_SurfaceType.GeomAbs_Plane)
                throw new Standard_NoSuchObject("GeomAdaptor_Surface::Plane");
            return ((Geom_Plane)mySurface).Pln();
        }

        public void Load(Geom_Surface theSurf)
        {
            if (theSurf == null)
            {
                throw new Standard_NullObject("GeomAdaptor_Surface::Load");
            }

            double aU1 = 0, aU2 = 0, aV1 = 0, aV2 = 0;
            theSurf.Bounds(out aU1, out aU2, out aV1, out aV2);
            load(theSurf, aU1, aU2, aV1, aV2);
        }

        public override double FirstUParameter() { return myUFirst; }

        public override double LastUParameter() { return myULast; }

        //! Returns the type of the surface : Plane, Cylinder,
        //! Cone,      Sphere,        Torus,    BezierSurface,
        //! BSplineSurface,               SurfaceOfRevolution,
        //! SurfaceOfExtrusion, OtherSurface
        public override GeomAbs_SurfaceType _GetType() { return mySurfaceType; }
        //! Standard_ConstructionError is raised if theUFirst>theULast or theVFirst>theVLast
        public void Load(Geom_Surface theSurf,
             double theUFirst, double theULast,
             double theVFirst, double theVLast,
             double theTolU = 0.0, double theTolV = 0.0)
        {
            if (theSurf == null)
            {
                throw new Standard_NullObject("GeomAdaptor_Surface::Load");
            }
            if (theUFirst > theULast || theVFirst > theVLast)
            {
                throw new Standard_ConstructionError("GeomAdaptor_Surface::Load");
            }

            load(theSurf, theUFirst, theULast, theVFirst, theVLast, theTolU, theTolV);
        }

        public override double FirstVParameter() { return myVFirst; }

        public override double LastVParameter() { return myVLast; }


        Geom_Surface mySurface;
        double myUFirst;
        double myULast;
        double myVFirst;
        double myVLast;
        double myTolU;
        double myTolV;

        Geom_BSplineSurface myBSplineSurface; ///< B-spline representation to prevent downcasts



        //=======================================================================
        //function : Load
        //purpose  : 
        //=======================================================================

        public void load(Geom_Surface S,
                               double UFirst,
                                 double ULast,
                                double VFirst,
                                double VLast,
                                double TolU = 0.0,
                                double TolV = 0.0)
        {
            myTolU = TolU;
            myTolV = TolV;
            myUFirst = UFirst;
            myULast = ULast;
            myVFirst = VFirst;
            myVLast = VLast;
            //  mySurfaceCache.Nullify();

            if (mySurface != S)
            {
                mySurface = S;
                //  myNestedEvaluator.Nullify();
                // myBSplineSurface.Nullify();

                var TheType = S.DynamicType();
                if (TheType == typeof(Geom_RectangularTrimmedSurface))
                {
                    Load(((Geom_RectangularTrimmedSurface)S).BasisSurface(),
                         UFirst, ULast, VFirst, VLast);
                }
                else if (TheType == typeof(Geom_Plane))
                    mySurfaceType = GeomAbs_SurfaceType.GeomAbs_Plane;
                else if (TheType == typeof(Geom_CylindricalSurface))
                    mySurfaceType = GeomAbs_SurfaceType.GeomAbs_Cylinder;
                else if (TheType == typeof(Geom_ConicalSurface))
                    mySurfaceType = GeomAbs_SurfaceType.GeomAbs_Cone;
                else if (TheType == typeof(Geom_SphericalSurface))
                    mySurfaceType = GeomAbs_SurfaceType.GeomAbs_Sphere;/*
                else if (TheType == typeof(Geom_ToroidalSurface))
                    mySurfaceType = GeomAbs_SurfaceType.GeomAbs_Torus;*/
                else if (TheType == typeof(Geom_SurfaceOfRevolution))
                {
                    mySurfaceType = GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution;
                    Geom_SurfaceOfRevolution myRevSurf =
                        (Geom_SurfaceOfRevolution)(mySurface);
                    // Create nested adaptor for base curve
                    /*Geom_Curve aBaseCurve = myRevSurf.BasisCurve();
                    Adaptor3d_Curve aBaseAdaptor = new GeomAdaptor_Curve(aBaseCurve);
                    // Create corresponding evaluator
                    myNestedEvaluator =
                        new GeomEvaluator_SurfaceOfRevolution(aBaseAdaptor, myRevSurf.Direction(), myRevSurf.Location());*/
                }/*
                else if (TheType == STANDARD_TYPE(Geom_SurfaceOfLinearExtrusion))
                {
                    mySurfaceType = GeomAbs_SurfaceOfExtrusion;
                    Handle(Geom_SurfaceOfLinearExtrusion) myExtSurf =
                        Handle(Geom_SurfaceOfLinearExtrusion)::DownCast(mySurface);
                    // Create nested adaptor for base curve
                    Handle(Geom_Curve) aBaseCurve = myExtSurf->BasisCurve();
                    Handle(Adaptor3d_Curve) aBaseAdaptor = new GeomAdaptor_Curve(aBaseCurve);
                    // Create corresponding evaluator
                    myNestedEvaluator =
                      new GeomEvaluator_SurfaceOfExtrusion(aBaseAdaptor, myExtSurf->Direction());
                }
                else if (TheType == STANDARD_TYPE(Geom_BezierSurface))
                {
                    mySurfaceType = GeomAbs_BezierSurface;
                }
                else if (TheType == STANDARD_TYPE(Geom_BSplineSurface))
                {
                    mySurfaceType = GeomAbs_BSplineSurface;
                    myBSplineSurface = Handle(Geom_BSplineSurface)::DownCast(mySurface);
                }
                else if (TheType == STANDARD_TYPE(Geom_OffsetSurface))
                {
                    mySurfaceType = GeomAbs_OffsetSurface;
                    Handle(Geom_OffsetSurface) myOffSurf = Handle(Geom_OffsetSurface)::DownCast(mySurface);
                    // Create nested adaptor for base surface
                    Handle(Geom_Surface) aBaseSurf = myOffSurf->BasisSurface();
                    Handle(GeomAdaptor_Surface) aBaseAdaptor =
                        new GeomAdaptor_Surface(aBaseSurf, myUFirst, myULast, myVFirst, myVLast, myTolU, myTolV);
                    myNestedEvaluator = new GeomEvaluator_OffsetSurface(
                        aBaseAdaptor, myOffSurf->Offset(), myOffSurf->OsculatingSurface());
                }*/
                else
                    mySurfaceType = GeomAbs_SurfaceType.GeomAbs_OtherSurface;
            }
        }


        public Geom_Surface Surface() { return mySurface; }

        public override bool IsVPeriodic()
        {
            return (mySurface.IsVPeriodic());
        }

        public override bool IsUPeriodic()
        {
            return (mySurface.IsUPeriodic());
        }

        public override double UPeriod()
        {
            Exceptions.Standard_NoSuchObject_Raise_if(!IsUPeriodic(), " ");
            return mySurface.UPeriod();
        }


        public override double VPeriod()
        {
            Exceptions.Standard_NoSuchObject_Raise_if(!IsVPeriodic(), " ");
            return mySurface.VPeriod();
        }

        public override double VResolution(double R3d)
        {
            double Res = 0.0;

            switch (mySurfaceType)
            {
                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution:
                    {
                        GeomAdaptor_Curve myBasisCurve = new(
                          ((Geom_SurfaceOfRevolution)mySurface).BasisCurve(), myUFirst, myULast);
                        return myBasisCurve.Resolution(R3d);
                    }
                case GeomAbs_SurfaceType.GeomAbs_Torus:
                    {
                        Geom_ToroidalSurface S = (Geom_ToroidalSurface)mySurface;
                        double R = S.MinorRadius();
                        if (R > Precision.Confusion())
                            Res = R3d / (2.0 * R);
                        break;
                    }
                case GeomAbs_SurfaceType.GeomAbs_Sphere:
                    {
                        Geom_SphericalSurface S = ((Geom_SphericalSurface)(mySurface));
                        double R = S.Radius();
                        if (R > Precision.Confusion())
                            Res = R3d / (2.0 * R);
                        break;
                    }
                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfExtrusion:
                case GeomAbs_SurfaceType.GeomAbs_Cylinder:
                case GeomAbs_SurfaceType.GeomAbs_Cone:
                case GeomAbs_SurfaceType.GeomAbs_Plane:
                    {
                        return R3d;
                    }
                //case GeomAbs_BezierSurface:
                //    {
                //        Standard_Real Ures, Vres;
                //        Handle(Geom_BezierSurface)::DownCast(mySurface)->Resolution(R3d, Ures, Vres);
                //        return Vres;
                //    }
                //case GeomAbs_BSplineSurface:
                //    {
                //        Standard_Real Ures, Vres;
                //        myBSplineSurface->Resolution(R3d, Ures, Vres);
                //        return Vres;
                //    }
                //case GeomAbs_OffsetSurface:
                //    {
                //        Handle(Geom_Surface) base = Handle(Geom_OffsetSurface)::DownCast(mySurface)->BasisSurface();
                //        GeomAdaptor_Surface gabase(base,myUFirst,myULast,myVFirst,myVLast);
                //        return gabase.VResolution(R3d);
                //    }
                default: return Precision.Parametric(R3d);
            }

            if (Res <= 1.0)
                return 2.0 * Math.Asin(Res);

            return 2.0 * Math.PI;
        }

        public override double UResolution(double R3d)
        {
            double Res = 0.0;

            switch (mySurfaceType)
            {
                //case GeomAbs_SurfaceType.GeomAbs_SurfaceOfExtrusion:
                //    {
                //        GeomAdaptor_Curve myBasisCurve=new GeomAdaptor_Curve (
                //          ((Geom_SurfaceOfLinearExtrusion)mySurface.BasisCurve()),myUFirst,myULast);
                //        return myBasisCurve.Resolution(R3d);
                //    }
                //case GeomAbs_SurfaceType.GeomAbs_Torus:
                //    {
                //        Handle(Geom_ToroidalSurface) S(Handle(Geom_ToroidalSurface)::DownCast(mySurface));
                //        const Standard_Real R = S->MajorRadius() + S->MinorRadius();
                //        if (R > Precision::Confusion())
                //            Res = R3d / (2.* R);
                //        break;
                //    }
                //case GeomAbs_SurfaceType.GeomAbs_Sphere:
                //    {
                //        Handle(Geom_SphericalSurface) S(Handle(Geom_SphericalSurface)::DownCast(mySurface));
                //        const Standard_Real R = S->Radius();
                //        if (R > Precision::Confusion())
                //            Res = R3d / (2.* R);
                //        break;
                //    }
                case GeomAbs_SurfaceType.GeomAbs_Cylinder:
                    {
                        Geom_CylindricalSurface S = (Geom_CylindricalSurface)mySurface;
                        double R = S.Radius();
                        if (R > Precision.Confusion())
                            Res = R3d / (2.0 * R);
                        break;
                    }
                case GeomAbs_SurfaceType.GeomAbs_Cone:
                    {

                        if (myVLast - myVFirst > 1e10)
                        {
                            // Pas vraiment borne => resolution inconnue
                            return Precision.Parametric(R3d);
                        }
                        Geom_ConicalSurface S = ((Geom_ConicalSurface)(mySurface));
                        Geom_Curve C = S.VIso(myVLast);
                        double Rayon1 = ((Geom_Circle)(C)).Radius();
                        C = S.VIso(myVFirst);
                        double Rayon2 = ((Geom_Circle)(C)).Radius();
                        double R = (Rayon1 > Rayon2) ? Rayon1 : Rayon2;
                        return (R > Precision.Confusion() ? (R3d / R) : 0.0);
                    }
                case GeomAbs_SurfaceType.GeomAbs_Plane:
                    {
                        return R3d;
                    }

                //case GeomAbs_BezierSurface:
                //    {
                //        Standard_Real Ures, Vres;
                //        Handle(Geom_BezierSurface)::DownCast(mySurface)->Resolution(R3d, Ures, Vres);
                //        return Ures;
                //    }
                //case GeomAbs_BSplineSurface:
                //    {
                //        Standard_Real Ures, Vres;
                //        myBSplineSurface->Resolution(R3d, Ures, Vres);
                //        return Ures;
                //    }
                //case GeomAbs_OffsetSurface:
                //    {
                //        Handle(Geom_Surface) base = Handle(Geom_OffsetSurface)::DownCast(mySurface)->BasisSurface();
                //        GeomAdaptor_Surface gabase(base,myUFirst,myULast,myVFirst,myVLast);
                //        return gabase.UResolution(R3d);
                //    }
                default: return Precision.Parametric(R3d);
            }

            if (Res <= 1.0)
                return 2.0 * Math.Asin(Res);

            return 2.0 * Math.PI;
        }

        public override void D1(double U, double V, out gp_Pnt P, out gp_Vec D1U, out gp_Vec D1V)
        {
            int Ideb, Ifin, IVdeb, IVfin, USide = 0, VSide = 0;
            double u = U, v = V;
            if (Math.Abs(U - myUFirst) <= myTolU) { USide = 1; u = myUFirst; }
            else if (Math.Abs(U - myULast) <= myTolU) { USide = -1; u = myULast; }
            if (Math.Abs(V - myVFirst) <= myTolV) { VSide = 1; v = myVFirst; }
            else if (Math.Abs(V - myVLast) <= myTolV) { VSide = -1; v = myVLast; }

            switch (mySurfaceType)
            {
                //case GeomAbs_BezierSurface:
                //case GeomAbs_BSplineSurface:
                //    {
                //        if (!myBSplineSurface.IsNull() &&
                //            (USide != 0 || VSide != 0) &&
                //            IfUVBound(u, v, Ideb, Ifin, IVdeb, IVfin, USide, VSide))
                //            myBSplineSurface->LocalD1(u, v, Ideb, Ifin, IVdeb, IVfin, P, D1U, D1V);
                //        else
                //        {
                //            if (mySurfaceCache.IsNull() || !mySurfaceCache->IsCacheValid(U, V))
                //                RebuildCache(U, V);
                //            mySurfaceCache->D1(U, V, P, D1U, D1V);
                //        }
                //        break;
                //    }

                //case GeomAbs_SurfaceOfExtrusion:
                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution:
                    //case GeomAbs_OffsetSurface:
                    Exceptions.Standard_NoSuchObject_Raise_if(myNestedEvaluator == null,
                            "GeomAdaptor_Surface::D1: evaluator is not initialized");
                    myNestedEvaluator.D1(u, v, out P, out D1U, out D1V);
                    break;

                default:
                    mySurface.D1(u, v, out P, out D1U, out D1V);
                    break;
            }
        }

        public override int NbVKnots()
        {
            if (mySurfaceType == GeomAbs_SurfaceType.GeomAbs_BSplineSurface)
                return myBSplineSurface.NbVKnots();
            throw new Standard_NoSuchObject("GeomAdaptor_Surface::NbVKnots");
        }

        public void RebuildCache(double theU,
                                       double theV)
        {
            if (mySurfaceType == GeomAbs_SurfaceType.GeomAbs_BezierSurface)
            {
                // Create cache for Bezier
                //Geom_BezierSurface aBezier = (Geom_BezierSurface)mySurface;
                //int aDegU = aBezier->UDegree();
                //int aDegV = aBezier->VDegree();
                //TColStd_Array1OfReal aFlatKnotsU(BSplCLib.FlatBezierKnots(aDegU), 1, 2 * (aDegU + 1));
                //TColStd_Array1OfReal aFlatKnotsV(BSplCLib.FlatBezierKnots(aDegV), 1, 2 * (aDegV + 1));
                //if (mySurfaceCache.IsNull())
                //    mySurfaceCache = new BSplSLib_Cache(
                //      aDegU, aBezier->IsUPeriodic(), aFlatKnotsU,
                //      aDegV, aBezier->IsVPeriodic(), aFlatKnotsV, aBezier->Weights());
                //mySurfaceCache->BuildCache(theU, theV, aFlatKnotsU, aFlatKnotsV,
                //                            aBezier->Poles(), aBezier->Weights());
            }
            else if (mySurfaceType == GeomAbs_SurfaceType.GeomAbs_BSplineSurface)
            {
                //// Create cache for B-spline
                //if (mySurfaceCache.IsNull())
                //    mySurfaceCache = new BSplSLib_Cache(
                //      myBSplineSurface->UDegree(), myBSplineSurface->IsUPeriodic(), myBSplineSurface->UKnotSequence(),
                //      myBSplineSurface->VDegree(), myBSplineSurface->IsVPeriodic(), myBSplineSurface->VKnotSequence(),
                //      myBSplineSurface->Weights());
                //mySurfaceCache->BuildCache(theU, theV, myBSplineSurface->UKnotSequence(), myBSplineSurface->VKnotSequence(),
                //                            myBSplineSurface->Poles(), myBSplineSurface->Weights());
            }
        }
        public override void D0(double U, double V, ref gp_Pnt P)
        {
            switch (mySurfaceType)
            {
                case GeomAbs_SurfaceType.GeomAbs_BezierSurface:
                case GeomAbs_SurfaceType.GeomAbs_BSplineSurface:
                    //if (mySurfaceCache.IsNull() || !mySurfaceCache->IsCacheValid(U, V))
                    //    RebuildCache(U, V);
                    //mySurfaceCache->D0(U, V, P);
                    break;

                case GeomAbs_SurfaceType.GeomAbs_OffsetSurface:
                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfExtrusion:
                    //    //caseGeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution:
                    //  Exceptions.Standard_NoSuchObject_Raise_if(myNestedEvaluator.IsNull(),
                    //      "GeomAdaptor_Surface::D0: evaluator is not initialized");
                    //  myNestedEvaluator->D0(U, V, P);
                    break;

                default:
                    mySurface.D0(U, V, ref P);
                    break;
            }
        }

        //! Computes   the point,  the  first  and  second
        //! derivatives on the surface.
        //!
        //! Warning : On the specific case of BSplineSurface:
        //! if the surface is cut in interval of continuity at least C2,
        //! the derivatives are computed on the current interval.
        //! else the derivatives are computed on the basis surface.
        public override void D2(double U, double V, out gp_Pnt P, out gp_Vec D1U, out gp_Vec D1V, out gp_Vec D2U, out gp_Vec D2V, out gp_Vec D2UV)
        {
            int Ideb, Ifin, IVdeb, IVfin, USide = 0, VSide = 0;
            double u = U, v = V;
            if (Math.Abs(U - myUFirst) <= myTolU) { USide = 1; u = myUFirst; }
            else if (Math.Abs(U - myULast) <= myTolU) { USide = -1; u = myULast; }
            if (Math.Abs(V - myVFirst) <= myTolV) { VSide = 1; v = myVFirst; }
            else if (Math.Abs(V - myVLast) <= myTolV) { VSide = -1; v = myVLast; }

            switch (mySurfaceType)
            {
                //case GeomAbs_BezierSurface:
                //case GeomAbs_BSplineSurface:
                //    {
                //        if (!myBSplineSurface.IsNull() &&
                //            (USide != 0 || VSide != 0) &&
                //            IfUVBound(u, v, Ideb, Ifin, IVdeb, IVfin, USide, VSide))
                //            myBSplineSurface->LocalD2(u, v, Ideb, Ifin, IVdeb, IVfin, P, D1U, D1V, D2U, D2V, D2UV);
                //        else
                //        {
                //            if (mySurfaceCache.IsNull() || !mySurfaceCache->IsCacheValid(U, V))
                //                RebuildCache(U, V);
                //            mySurfaceCache->D2(U, V, P, D1U, D1V, D2U, D2V, D2UV);
                //        }
                //        break;
                //    }

                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfExtrusion:
                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution:
                case GeomAbs_SurfaceType.GeomAbs_OffsetSurface:
                    Exceptions.Standard_NoSuchObject_Raise_if(myNestedEvaluator == null,
                         "GeomAdaptor_Surface::D2: evaluator is not initialized");
                    myNestedEvaluator.D2(u, v, out P, out D1U, out D1V, out D2U, out D2V, out D2UV);
                    break;

                default:
                    {
                        mySurface.D2(u, v, out P, out D1U, out D1V, out D2U, out D2V, out D2UV);
                        break;
                    }
            }
        }

        public override int NbVIntervals(GeomAbs_Shape shape)
        {
            switch (mySurfaceType)
            {
                //case GeomAbs_BSplineSurface:
                //    {
                //        GeomAdaptor_Curve myBasisCurve
                //          (myBSplineSurface->UIso(myBSplineSurface->UKnot(myBSplineSurface->FirstUKnotIndex())),myVFirst,myVLast);
                //        return myBasisCurve.NbIntervals(S);
                //    }
                //case GeomAbs_SurfaceOfRevolution:
                //    {
                //        Handle(Geom_SurfaceOfRevolution) myRevSurf =
                //            Handle(Geom_SurfaceOfRevolution)::DownCast(mySurface);
                //        GeomAdaptor_Curve myBasisCurve(myRevSurf->BasisCurve(), myVFirst, myVLast);
                //        if (myBasisCurve.GetType() == GeomAbs_BSplineCurve)
                //            return myBasisCurve.NbIntervals(S);
                //        break;
                //    }
                //case GeomAbs_OffsetSurface:
                //    {
                //        GeomAbs_Shape BaseS = GeomAbs_CN;
                //        switch (S)
                //        {
                //            case GeomAbs_G1:
                //            case GeomAbs_G2: throw Standard_DomainError("GeomAdaptor_Curve::NbVIntervals");
                //            case GeomAbs_C0: BaseS = GeomAbs_C1; break;
                //            case GeomAbs_C1: BaseS = GeomAbs_C2; break;
                //            case GeomAbs_C2: BaseS = GeomAbs_C3; break;
                //            case GeomAbs_C3:
                //            case GeomAbs_CN: break;
                //        }
                //        Handle(Geom_OffsetSurface) myOffSurf = Handle(Geom_OffsetSurface)::DownCast(mySurface);
                //        GeomAdaptor_Surface Sur(myOffSurf->BasisSurface(), myUFirst, myULast, myVFirst, myVLast);
                //        return Sur.NbVIntervals(BaseS);
                //    }
                case GeomAbs_SurfaceType. GeomAbs_Plane:
                case GeomAbs_SurfaceType.GeomAbs_Cylinder:
                case GeomAbs_SurfaceType.GeomAbs_Cone:
                case GeomAbs_SurfaceType.GeomAbs_Sphere:
                case GeomAbs_SurfaceType.GeomAbs_Torus:
                case GeomAbs_SurfaceType.GeomAbs_BezierSurface:
                case GeomAbs_SurfaceType.GeomAbs_OtherSurface:
                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfExtrusion: break;
            }
            return 1;
        }

        public override int NbUIntervals(GeomAbs_Shape shape)
        {
            switch (mySurfaceType)
            {
                //case GeomAbs_SurfaceType. GeomAbs_BSplineSurface:
                //    {
                //        GeomAdaptor_Curve myBasisCurve
                //          (myBSplineSurface->VIso(myBSplineSurface->VKnot(myBSplineSurface->FirstVKnotIndex())),myUFirst,myULast);
                //        return myBasisCurve.NbIntervals(S);
                //    }
                //case GeomAbs_SurfaceOfExtrusion:
                //    {
                //        Handle(Geom_SurfaceOfLinearExtrusion) myExtSurf =
                //            Handle(Geom_SurfaceOfLinearExtrusion)::DownCast(mySurface);
                //        GeomAdaptor_Curve myBasisCurve(myExtSurf->BasisCurve(), myUFirst, myULast);
                //        if (myBasisCurve.GetType() == GeomAbs_BSplineCurve)
                //            return myBasisCurve.NbIntervals(S);
                //        break;
                //    }
                //case GeomAbs_OffsetSurface:
                //    {
                //        GeomAbs_Shape BaseS = GeomAbs_CN;
                //        switch (S)
                //        {
                //            case GeomAbs_G1:
                //            case GeomAbs_G2: throw Standard_DomainError("GeomAdaptor_Curve::NbUIntervals");
                //            case GeomAbs_C0: BaseS = GeomAbs_C1; break;
                //            case GeomAbs_C1: BaseS = GeomAbs_C2; break;
                //            case GeomAbs_C2: BaseS = GeomAbs_C3; break;
                //            case GeomAbs_C3:
                //            case GeomAbs_CN: break;
                //        }
                //        Handle(Geom_OffsetSurface) myOffSurf = Handle(Geom_OffsetSurface)::DownCast(mySurface);
                //        GeomAdaptor_Surface Sur(myOffSurf->BasisSurface(), myUFirst, myULast, myVFirst, myVLast);
                //        return Sur.NbUIntervals(BaseS);
                //    }
                case GeomAbs_SurfaceType.GeomAbs_Plane:
                case GeomAbs_SurfaceType.GeomAbs_Cylinder:
                case GeomAbs_SurfaceType.GeomAbs_Cone:
                case GeomAbs_SurfaceType.GeomAbs_Sphere:
                case GeomAbs_SurfaceType.GeomAbs_Torus:
                case GeomAbs_SurfaceType.GeomAbs_BezierSurface:
                case GeomAbs_SurfaceType.GeomAbs_OtherSurface:
                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution: break;
            }
            return 1;
        }

        public override void VIntervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {
            throw new NotImplementedException();
        }

        //BSplSLib_Cache mySurfaceCache; ///< Cached data for B-spline or Bezier surface

        public GeomAdaptor_Surface(Geom_Surface theSurf)

        {
            myTolU = (0.0); myTolV = (0.0);
            Load(theSurf);
        }

        //! Standard_ConstructionError is raised if UFirst>ULast or VFirst>VLast
        public GeomAdaptor_Surface(Geom_Surface theSurf,
                        double theUFirst, double theULast,
                        double theVFirst, double theVLast,
                        double theTolU = 0.0, double theTolV = 0.0)
        {
            Load(theSurf, theUFirst, theULast, theVFirst, theVLast, theTolU, theTolV);
        }


        GeomAbs_SurfaceType mySurfaceType;

    }
}
