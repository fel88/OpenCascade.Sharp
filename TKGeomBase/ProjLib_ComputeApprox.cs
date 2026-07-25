using OCCPort.Common;
using TKG2d;
using TKG3d;
using TKMath;

namespace TKGeomBase
{
    //! Approximate the  projection of  a 3d curve   on an
    //! analytic surface and stores the result in Approx.
    //! The result is a 2d curve.
    //! For approximation some parameters are used, including 
    //! required tolerance of approximation.
    //! Tolerance is maximal possible value of 3d deviation of 3d projection of projected curve from
    //! "exact" 3d projection. Since algorithm searches 2d curve on surface, required 2d tolerance is computed
    //! from 3d tolerance with help of U,V resolutions of surface.
    //! 3d and 2d tolerances have sense only for curves on surface, it defines precision of projecting and approximation
    //! and have nothing to do with distance between the projected curve and the surface.
    public class ProjLib_ComputeApprox
    {
        double myTolerance;
        Geom2d_BSplineCurve myBSpline;
        Geom2d_BezierCurve myBezier;
        int myDegMin;
        int myDegMax;
        int myMaxSegments;
        AppParCurves_Constraint myBndPnt;


        public void Perform(Adaptor3d_Curve C, Adaptor3d_Surface S)
        {
            // if the surface is a plane and the curve a BSpline or a BezierCurve,
            // don`t make an Approx but only the projection of the poles.

            int NbKnots, NbPoles;
            GeomAbs_CurveType CType = C._GetType();
            GeomAbs_SurfaceType SType = S._GetType();

            bool SurfIsAnal = ProjLib.IsAnaSurf(S);

            bool CurvIsAnal = (CType != GeomAbs_CurveType.GeomAbs_BSplineCurve) &&
                                      (CType != GeomAbs_CurveType.GeomAbs_BezierCurve) &&
                                      (CType != GeomAbs_CurveType.GeomAbs_OffsetCurve) &&
                                      (CType != GeomAbs_CurveType.GeomAbs_OtherCurve);

            bool simplecase = SurfIsAnal && CurvIsAnal;
            if (CType == GeomAbs_CurveType.GeomAbs_BSplineCurve || CType == GeomAbs_CurveType.GeomAbs_BezierCurve)
            {
                int aNbKnots = 1;
                if (CType == GeomAbs_CurveType.GeomAbs_BSplineCurve)
                {
                    aNbKnots = C.NbKnots();
                }
                simplecase = simplecase && C.Degree() <= 2 && aNbKnots <= 2;
            }

            if (CType == GeomAbs_CurveType.GeomAbs_BSplineCurve &&
                SType == GeomAbs_SurfaceType.GeomAbs_Plane)
            {

                //// get the poles and eventually the weights
                Geom_BSplineCurve BS = C.BSpline();
                NbPoles = BS.NbPoles();
                //TColgp_Array1OfPnt P3d = new TColgp_Array1OfPnt(1, NbPoles);
                //TColgp_Array1OfPnt2d Poles = new TColgp_Array1OfPnt2d(1, NbPoles);
                //TColStd_Array1OfReal Weights = new TColStd_Array1OfReal(1, NbPoles);
                //if (BS.IsRational()) BS.Weights(Weights);
                //BS.Poles(P3d);
                //gp_Pln Plane = S.Plane();
                //double U, V;
                //for (int i = 1; i <= NbPoles; i++)
                //{
                //    ElSLib.Parameters(Plane, P3d(i), U, V);
                //    Poles.SetValue(i, new gp_Pnt2d(U, V));
                //}
                //NbKnots = BS.NbKnots();
                //TColStd_Array1OfReal Knots = new TColStd_Array1OfReal(1, NbKnots);
                //TColStd_Array1OfInteger Mults = new TColStd_Array1OfInteger(1, NbKnots);
                //BS.Knots(Knots);
                //BS.Multiplicities(Mults);
                //// get the knots and mults if BSplineCurve
                //if (BS.IsRational())
                //{
                //    myBSpline = new Geom2d_BSplineCurve(Poles,
                //                    Weights,
                //                    Knots,
                //                    Mults,
                //                    BS.Degree(),
                //                    BS.IsPeriodic());
                //}
                //else
                //{
                //    myBSpline = new Geom2d_BSplineCurve(Poles,
                //                    Knots,
                //                    Mults,
                //                    BS->Degree(),
                //                    BS->IsPeriodic());
                //}
            }
            else if (CType == GeomAbs_CurveType.GeomAbs_BezierCurve &&
                 SType == GeomAbs_SurfaceType.GeomAbs_Plane)
            {

                //            // get the poles and eventually the weights
                //            Geom_BezierCurve BezierCurvePtr = C.Bezier();
                //            NbPoles = BezierCurvePtr->NbPoles();
                //            TColgp_Array1OfPnt P3d( 1, NbPoles);
                //            TColgp_Array1OfPnt2d Poles( 1, NbPoles);
                //            TColStd_Array1OfReal Weights( 1, NbPoles);
                //            if (BezierCurvePtr->IsRational())
                //            {
                //                BezierCurvePtr->Weights(Weights);
                //            }
                //            BezierCurvePtr->Poles(P3d);

                //            // project the 3D-Poles on the plane

                //            gp_Pln Plane = S->Plane();
                //            Standard_Real U, V;
                //            for (Standard_Integer i = 1; i <= NbPoles; i++)
                //            {
                //                ElSLib::Parameters(Plane, P3d(i), U, V);
                //                Poles.SetValue(i, gp_Pnt2d(U, V));
                //            }
                //            if (BezierCurvePtr->IsRational())
                //            {
                //                myBezier = new Geom2d_BezierCurve(Poles, Weights);
                //            }
                //            else
                //            {
                //                myBezier = new Geom2d_BezierCurve(Poles);
                //            }
            }
            else
            {
                ProjLib_Function F = new ProjLib_Function(C, S);


                //-----------
                int Deg1 = 5, Deg2;
                if (simplecase)
                {
                    Deg2 = 8;
                }
                else
                {
                    Deg2 = 10;
                }
                if (myDegMin > 0)
                {
                    Deg1 = myDegMin;
                }
                //
                if (myDegMax > 0)
                {
                    Deg2 = myDegMax;
                }
                //
                int aMaxSegments = 1000;
                if (myMaxSegments > 0)
                {
                    aMaxSegments = myMaxSegments;
                }
                AppParCurves_Constraint aFistC = AppParCurves_Constraint.AppParCurves_TangencyPoint, aLastC =
                    AppParCurves_Constraint.AppParCurves_TangencyPoint;
                if (myBndPnt != AppParCurves_Constraint.AppParCurves_TangencyPoint)
                {
                    aFistC = myBndPnt;
                    aLastC = myBndPnt;
                }

                //-------------
                double aTolU = ComputeTolU(S, myTolerance);
                double aTolV = ComputeTolV(S, myTolerance);
                double aTol2d = Math.Max(Math.Sqrt(aTolU * aTolU + aTolV * aTolV), Precision.PConfusion());

                Approx_FitAndDivide2d Fit = new Approx_FitAndDivide2d(Deg1, Deg2, myTolerance, aTol2d, true, aFistC, aLastC);
                Fit.SetMaxSegments(aMaxSegments);
                if (simplecase)
                {
                    Fit.SetHangChecking(false);
                }
                Fit.Perform(F);

                //            double aNewTol2d = 0;
                //            if (Fit.IsAllApproximated())
                //            {
                //                int i;
                //                int NbCurves = Fit.NbMultiCurves();

                //                // on essaie de rendre la courbe au moins C1
                //                Convert_CompBezierCurves2dToBSplineCurve2d Conv;

                //                Standard_Real Tol3d, Tol2d;
                //                for (i = 1; i <= NbCurves; i++)
                //                {
                //                    Fit.Error(i, Tol3d, Tol2d);
                //                    aNewTol2d = Max(aNewTol2d, Tol2d);
                //                    AppParCurves_MultiCurve MC = Fit.Value(i);       //Charge la Ieme Curve
                //                    TColgp_Array1OfPnt2d Poles2d( 1, MC.Degree() + 1);//Recupere les poles
                //                MC.Curve(1, Poles2d);
                //                Conv.AddCurve(Poles2d);
                //            }

                //            //mise a jour des fields de ProjLib_Approx
                //            Conv.Perform();
                //            NbPoles = Conv.NbPoles();
                //            NbKnots = Conv.NbKnots();

                //            if (NbPoles <= 0 || NbPoles > 100000)
                //                return;
                //            if (NbKnots <= 0 || NbKnots > 100000)
                //                return;

                //            TColgp_Array1OfPnt2d NewPoles(1,NbPoles);
                //            TColStd_Array1OfReal NewKnots(1,NbKnots);
                //            TColStd_Array1OfInteger NewMults(1,NbKnots);

                //            Conv.KnotsAndMults(NewKnots, NewMults);
                //            Conv.Poles(NewPoles);

                //            BSplCLib::Reparametrize(C->FirstParameter(),
                //                                    C->LastParameter(),
                //                                    NewKnots);

                //            // Set NewKnots(NbKnots) exactly C->LastParameter()
                //            // to avoid problems if trim is used.
                //            NewKnots(NbKnots) = C->LastParameter();

                //            // il faut recadrer les poles de debut et de fin:
                //            // ( Car pour les problemes de couture, on a du ouvrir l`intervalle
                //            // de definition de la courbe.)
                //            // On choisit de calculer ces poles par prolongement de la courbe
                //            // approximee.
                //            myBSpline = new Geom2d_BSplineCurve(NewPoles,
                //                                                 NewKnots,
                //                                                 NewMults,
                //                                                 Conv.Degree());

                //            if (aFistC == AppParCurves_PassPoint || aLastC == AppParCurves_PassPoint)
                //            {
                //                // try to smoother the Curve GeomAbs_C1.
                //                Standard_Integer aDeg = myBSpline->Degree();
                //                Standard_Boolean OK = Standard_True;
                //                Standard_Real aSmoothTol = Max(Precision::Confusion(), aNewTol2d);
                //                for (Standard_Integer ij = 2; ij < NbKnots; ij++)
                //                {
                //                    OK = OK && myBSpline->RemoveKnot(ij, aDeg - 1, aSmoothTol);
                //                }
                //            }
                //        }
                //else
                //        {
                //            int NbCurves = Fit.NbMultiCurves();
                //            if (NbCurves != 0)
                //            {
                //                double Tol3d, Tol2d;
                //                Fit.Error(NbCurves, Tol3d, Tol2d);
                //                aNewTol2d = Tol2d;
                //            }
                //        }

                // restore tolerance 3d from 2d

                //Here we consider that 
                //   aTolU(new)/aTolV(new) = aTolU(old)/aTolV(old)
                //(it is assumption indeed).
                //Then,
                //  Tol3D(new)/Tol3D(old) = Tol2D(new)/Tol2D(old).
                //myTolerance *= (aNewTol2d / aTol2d);

                ////Return curve home
                double UFirst = F.FirstParameter();
                double ULast = F.LastParameter();
                double Umid = (UFirst + ULast) / 2;
                gp_Pnt P3d = C.Value(Umid);
                double u = 0.0, v = 0.0;
                switch (SType)
                {
                    case GeomAbs_SurfaceType.GeomAbs_Plane:
                        {
                            gp_Pln Plane = S.Plane();
                            ElSLib.Parameters(Plane, P3d, ref u, ref v);
                            break;
                        }
                    case GeomAbs_SurfaceType.GeomAbs_Cylinder:
                        {
                            gp_Cylinder Cylinder = S.Cylinder();
                            ElSLib.Parameters(Cylinder, P3d, ref u,ref v);
                            break;
                        }
                    //case GeomAbs_Cone:
                    //    {
                    //        gp_Cone Cone = S->Cone();
                    //        ElSLib::Parameters(Cone, P3d, u, v);
                    //        break;
                    //    }
                    case GeomAbs_SurfaceType.GeomAbs_Sphere:
                        {
                            gp_Sphere Sphere = S.Sphere();
                            ElSLib.Parameters(Sphere, P3d, ref u, ref v);
                            break;
                        }
                    //case GeomAbs_Torus:
                    //    {
                    //        gp_Torus Torus = S->Torus();
                    //        ElSLib::Parameters(Torus, P3d, u, v);
                    //        break;
                    //    }
                    default:
                        throw new Standard_NoSuchObject("ProjLib_ComputeApprox::Value");
                }
                bool ToMirror = false;
                double du = 0.0, dv = 0.0;
                int number;
                //if (F.VCouture)
                //{
                //    if (SType == GeomAbs_Sphere && Abs(u - F.myU1) > M_PI)
                //    {
                //        ToMirror = Standard_True;
                //        dv = -M_PI;
                //        v = M_PI - v;
                //    }
                //    Standard_Real newV = ElCLib::InPeriod(v, F.myV1, F.myV2);
                //    number = (Standard_Integer)(Floor((newV - v) / (F.myV2 - F.myV1)));
                //    dv -= number * (F.myV2 - F.myV1);
                //}
                //if (F.UCouture || (F.VCouture && SType == GeomAbs_Sphere))
                //{
                //    Standard_Real aNbPer;
                //    gp_Pnt2d P2d = F.Value(Umid);
                //    du = u - P2d.X();
                //    du = (du < 0) ? (du - Precision::PConfusion()) :
                //      (du + Precision::PConfusion());
                //    modf(du / M_PI, &aNbPer);
                //    number = (Standard_Integer)aNbPer;
                //    du = number * M_PI;
                //}

                //if (!myBSpline.IsNull())
                //{
                //    if (du != 0. || dv != 0.)
                //        myBSpline->Translate(gp_Vec2d(du, dv));
                //    if (ToMirror)
                //    {
                //        gp_Ax2d Axe(gp_Pnt2d(0.,0.), gp_Dir2d(1., 0.) );
                //        myBSpline->Mirror(Axe);
                //    }
            }
        }

        //! Set tolerance of approximation.
        //! Default value is Precision::Confusion().
        public void SetTolerance(double theTolerance)
        {
            myTolerance = theTolerance;

        }

        //! Set min and max possible degree of result BSpline curve2d, which is got by approximation.
        //! If theDegMin/Max < 0, algorithm uses values that are chosen depending of types curve 3d
        //! and surface.
        public void SetDegree(int theDegMin, int theDegMax)
        {
            myDegMin = theDegMin;
            myDegMax = theDegMax;
        }

        //! Set the parameter, which defines maximal value of parametric intervals the projected
        //! curve can be cut for approximation. If theMaxSegments < 0, algorithm uses default 
        //! value = 1000.
        public void SetMaxSegments(int theMaxSegments)
        {
            myMaxSegments = theMaxSegments;
        }

        //! Set the parameter, which defines type of boundary condition between segments during approximation.
        //! It can be AppParCurves_PassPoint or AppParCurves_TangencyPoint.
        //! Default value is AppParCurves_TangencyPoint;
        public void SetBndPnt(AppParCurves_Constraint theBndPnt)
        {
            myBndPnt = theBndPnt;

        }
        public Geom2d_BSplineCurve BSpline()

        {
            return myBSpline;
        }

        //=======================================================================
        //function : Bezier
        //purpose  : 
        //=======================================================================

        public Geom2d_BezierCurve Bezier()

        {
            return myBezier;
        }

        public static double ComputeTolU(Adaptor3d_Surface theSurf,
                                 double theTolerance)
        {
            double aTolU = theSurf.UResolution(theTolerance);
            if (theSurf.IsUPeriodic())
            {
                aTolU = Math.Min(aTolU, 0.01 * theSurf.UPeriod());
            }

            return aTolU;
        }

        //=======================================================================
        //function : ComputeTolV
        //purpose  : 
        //=======================================================================

        public static double ComputeTolV(Adaptor3d_Surface theSurf,
                                 double theTolerance)
        {
            double aTolV = theSurf.VResolution(theTolerance);
            if (theSurf.IsVPeriodic())
            {
                aTolV = Math.Min(aTolV, 0.01 * theSurf.VPeriod());
            }

            return aTolV;
        }
    }
}
