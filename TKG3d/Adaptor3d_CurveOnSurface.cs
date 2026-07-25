using OCCPort.Common;
using TKernel;
using TKG2d;
using TKMath;

namespace TKG3d
{
    //! An interface between the services provided by a curve
    //! lying on a surface from the package Geom and those
    //! required of the curve by algorithms which use it. The
    //! curve is defined as a 2D curve from the Geom2d
    //! package, in the parametric space of the surface.
    public class Adaptor3d_CurveOnSurface : Adaptor3d_Curve
    {
        public override double FirstParameter()
        {
            return myCurve.FirstParameter();
        }
        public Adaptor3d_Surface GetSurface()
        {
            return mySurface;
        }
        public Adaptor2d_Curve2d GetCurve()
        {
            return myCurve;
        }

        public override double LastParameter()
        {
            return myCurve.LastParameter();
        }


        Adaptor3d_Surface mySurface;
        Adaptor2d_Curve2d myCurve;
        GeomAbs_CurveType myType;
        gp_Circ myCirc;
        gp_Lin myLin;
        Adaptor3d_Surface myFirstSurf;
        Adaptor3d_Surface myLastSurf;
        //TColStd_HSequenceOfReal) myIntervals;
        GeomAbs_Shape myIntCont;
        public override GeomAbs_CurveType _GetType()
        {
            return myType;

        }

        public override int Degree()
        {
            // on a parametric surface should multiply
            // return TheCurve2dTool::Degree(myCurve);

            return myCurve.Degree();
        }

        public override int NbKnots()
        {
            if (mySurface._GetType() == GeomAbs_SurfaceType.GeomAbs_Plane)
                return myCurve.NbKnots();
            else
            {
                throw new Standard_NoSuchObject();
            }
        }

        public override Geom_BSplineCurve BSpline()
        {
            throw new System.NotImplementedException();
        }

        public override gp_Pnt Value(double U)
        {
            gp_Pnt P = new gp_Pnt();
            gp_Pnt2d Puv = new gp_Pnt2d();

            if (myType == GeomAbs_CurveType.GeomAbs_Line) P = ElCLib.Value(U, myLin);
            else if (myType == GeomAbs_CurveType.GeomAbs_Circle) P = ElCLib.Value(U, myCirc);
            else
            {
                myCurve.D0(U, ref Puv);
                mySurface.D0(Puv.X(), Puv.Y(), ref P);
            }

            return P;
        }

        public override void D0(double U, ref gp_Pnt P)
        {
            gp_Pnt2d Puv = new gp_Pnt2d();

            if (myType == GeomAbs_CurveType.GeomAbs_Line) P = ElCLib.Value(U, myLin);
            else if (myType == GeomAbs_CurveType.GeomAbs_Circle) P = ElCLib.Value(U, myCirc);
            else
            {
                myCurve.D0(U, ref Puv);
                mySurface.D0(Puv.X(), Puv.Y(), ref P);
            }
        }
        public void Load(Adaptor2d_Curve2d C)
        {
            myCurve = C;
            if (mySurface == null)
            {
                return;
            }

            EvalKPart();

            GeomAbs_SurfaceType SType = mySurface._GetType();
            if (SType == GeomAbs_SurfaceType.GeomAbs_OffsetSurface)
            {
                SType = mySurface.BasisSurface()._GetType();
            }

            if (SType == GeomAbs_SurfaceType.GeomAbs_BSplineSurface ||
                SType == GeomAbs_SurfaceType.GeomAbs_SurfaceOfExtrusion ||
                SType == GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution)
            {
                EvalFirstLastSurf();
            }
        }

        private void EvalFirstLastSurf()
        {
            throw new NotImplementedException();
        }

        static gp_Circ to3d(gp_Pln Pl, gp_Circ2d C)
        {
            return new gp_Circ(to3d(Pl, C.Axis()), C.Radius());
        }

        static gp_Vec to3d(gp_Pln Pl, gp_Vec2d V)
        {
            gp_Vec Vx = Pl.XAxis().Direction();
            gp_Vec Vy = Pl.YAxis().Direction();
            Vx.Multiply(V.X());
            Vy.Multiply(V.Y());
            Vx.Add(Vy);
            return Vx;
        }



        static gp_Ax2 to3d(gp_Pln Pl, gp_Ax22d A)
        {
            gp_Pnt P = to3d(Pl, A.Location());
            gp_Vec VX = to3d(Pl, new gp_Vec2d(A.XAxis().Direction()));
            gp_Vec VY = to3d(Pl, new gp_Vec2d(A.YAxis().Direction()));
            return new gp_Ax2(P, VX.Crossed(VY), VX);
        }

        static gp_Pnt to3d(gp_Pln Pl, gp_Pnt2d P)
        {
            return ElSLib.Value(P.X(), P.Y(), Pl);
        }

        void EvalKPart()
        {
            myType = GeomAbs_CurveType.GeomAbs_OtherCurve;

            GeomAbs_SurfaceType STy = mySurface._GetType();
            GeomAbs_CurveType CTy = myCurve._GetType();
            if (STy == GeomAbs_SurfaceType.GeomAbs_Plane)
            {
                myType = CTy;
                if (myType == GeomAbs_CurveType.GeomAbs_Circle)
                    myCirc = to3d(mySurface.Plane(), myCurve.Circle());
                else
                if (myType == GeomAbs_CurveType.GeomAbs_Line)
                {
                    gp_Pnt P;
                    gp_Vec V = new gp_Vec();
                    gp_Pnt2d Puv;
                    gp_Vec2d Duv;
                    myCurve.D1(0.0, out Puv, out Duv);
                    gp_Vec D1U, D1V;
                    mySurface.D1(Puv.X(), Puv.Y(), out P, out D1U, out D1V);
                    V.SetLinearForm(Duv.X(), D1U, Duv.Y(), D1V);
                    myLin = new gp_Lin(P, V);
                }
            }
            else
            {
                if (CTy == GeomAbs_CurveType.GeomAbs_Line)
                {
                    gp_Dir2d D = myCurve.Line().Direction();
                    if (D.IsParallel(gp.DX2d(), Precision.Angular()))
                    {
                        // Iso V.
                        if (STy == GeomAbs_SurfaceType.GeomAbs_Sphere)
                        {
                            //                gp_Pnt2d P = myCurve->Line().Location();
                            //                if (Abs(Abs(P.Y()) - M_PI / 2. ) >= Precision::PConfusion())
                            //                {
                            //                    myType = GeomAbs_Circle;
                            //                    gp_Sphere Sph = mySurface->Sphere();
                            //                    gp_Ax3 Axis = Sph.Position();
                            //                    myCirc = ElSLib::SphereVIso(Axis,
                            //                                  Sph.Radius(),
                            //                                  P.Y());
                            //                    gp_Dir DRev = Axis.XDirection().Crossed(Axis.YDirection());
                            //                    gp_Ax1 AxeRev(Axis.Location(), DRev);
                            //                    myCirc.Rotate(AxeRev, P.X());
                            //                    if (D.IsOpposite(gp::DX2d(), Precision::Angular()))
                            //                    {
                            //                        gp_Ax2 Ax = myCirc.Position();
                            //                        Ax.SetDirection(Ax.Direction().Reversed());
                            //                        myCirc.SetPosition(Ax);
                            //                    }
                            //                }
                        }
                        else if (STy == GeomAbs_SurfaceType.GeomAbs_Cylinder)
                        {
                            //                myType = GeomAbs_Circle;
                            //                gp_Cylinder Cyl = mySurface->Cylinder();
                            //                gp_Pnt2d P = myCurve->Line().Location();
                            //                gp_Ax3 Axis = Cyl.Position();
                            //                myCirc = ElSLib::CylinderVIso(Axis,
                            //                                    Cyl.Radius(),
                            //                                    P.Y());
                            //                gp_Dir DRev = Axis.XDirection().Crossed(Axis.YDirection());
                            //                gp_Ax1 AxeRev(Axis.Location(), DRev);
                            //                myCirc.Rotate(AxeRev, P.X());
                            //                if (D.IsOpposite(gp::DX2d(), Precision::Angular()))
                            //                {
                            //                    gp_Ax2 Ax = myCirc.Position();
                            //                    Ax.SetDirection(Ax.Direction().Reversed());
                            //                    myCirc.SetPosition(Ax);
                            //                }
                        }
                        else if (STy == GeomAbs_SurfaceType.GeomAbs_Cone)
                        {
                            //                myType = GeomAbs_Circle;
                            //                gp_Cone Cone = mySurface->Cone();
                            //                gp_Pnt2d P = myCurve->Line().Location();
                            //                gp_Ax3 Axis = Cone.Position();
                            //                myCirc = ElSLib::ConeVIso(Axis,
                            //                                 Cone.RefRadius(),
                            //                                 Cone.SemiAngle(),
                            //                                 P.Y());
                            //                gp_Dir DRev = Axis.XDirection().Crossed(Axis.YDirection());
                            //                gp_Ax1 AxeRev(Axis.Location(), DRev);
                            //                myCirc.Rotate(AxeRev, P.X());
                            //                if (D.IsOpposite(gp::DX2d(), Precision::Angular()))
                            //                {
                            //                    gp_Ax2 Ax = myCirc.Position();
                            //                    Ax.SetDirection(Ax.Direction().Reversed());
                            //                    myCirc.SetPosition(Ax);
                            //                }
                        }
                        else if (STy == GeomAbs_SurfaceType.GeomAbs_Torus)
                        {
                            //                myType = GeomAbs_Circle;
                            //                gp_Torus Tore = mySurface->Torus();
                            //                gp_Pnt2d P = myCurve->Line().Location();
                            //                gp_Ax3 Axis = Tore.Position();
                            //                myCirc = ElSLib::TorusVIso(Axis,
                            //                                  Tore.MajorRadius(),
                            //                                  Tore.MinorRadius(),
                            //                                  P.Y());
                            //                gp_Dir DRev = Axis.XDirection().Crossed(Axis.YDirection());
                            //                gp_Ax1 AxeRev(Axis.Location(), DRev);
                            //                myCirc.Rotate(AxeRev, P.X());
                            //                if (D.IsOpposite(gp::DX2d(), Precision::Angular()))
                            //                {
                            //                    gp_Ax2 Ax = myCirc.Position();
                            //                    Ax.SetDirection(Ax.Direction().Reversed());
                            //                    myCirc.SetPosition(Ax);
                        }
                    }
                    else if (D.IsParallel(gp.DY2d(), Precision.Angular()))
                    { // Iso U.
                      //            if (STy == GeomAbs_Sphere)
                      //            {
                      //                myType = GeomAbs_Circle;
                      //                gp_Sphere Sph = mySurface->Sphere();
                      //                gp_Pnt2d P = myCurve->Line().Location();
                      //                gp_Ax3 Axis = Sph.Position();
                      //                // calcul de l'iso 0.
                      //                myCirc = ElSLib::SphereUIso(Axis, Sph.Radius(), 0.);

                        //                // mise a sameparameter (rotation du cercle - decalage du Y)
                        //                gp_Dir DRev = Axis.XDirection().Crossed(Axis.Direction());
                        //                gp_Ax1 AxeRev(Axis.Location(), DRev);
                        //                myCirc.Rotate(AxeRev, P.Y());

                        //                // transformation en iso U ( = P.X())
                        //                DRev = Axis.XDirection().Crossed(Axis.YDirection());
                        //                AxeRev = gp_Ax1(Axis.Location(), DRev);
                        //                myCirc.Rotate(AxeRev, P.X());

                        //                if (D.IsOpposite(gp::DY2d(), Precision::Angular()))
                        //                {
                        //                    gp_Ax2 Ax = myCirc.Position();
                        //                    Ax.SetDirection(Ax.Direction().Reversed());
                        //                    myCirc.SetPosition(Ax);
                        //                }
                        //            }
                        //            else if (STy == GeomAbs_Cylinder)
                        //            {
                        //                myType = GeomAbs_Line;
                        //                gp_Cylinder Cyl = mySurface->Cylinder();
                        //                gp_Pnt2d P = myCurve->Line().Location();
                        //                myLin = ElSLib::CylinderUIso(Cyl.Position(),
                        //                                   Cyl.Radius(),
                        //                                   P.X());
                        //                gp_Vec Tr(myLin.Direction());
                        //                Tr.Multiply(P.Y());
                        //                myLin.Translate(Tr);
                        //                if (D.IsOpposite(gp::DY2d(), Precision::Angular()))
                        //                    myLin.Reverse();
                        //            }
                        //            else if (STy == GeomAbs_Cone)
                        //            {
                        //                myType = GeomAbs_Line;
                        //                gp_Cone Cone = mySurface->Cone();
                        //                gp_Pnt2d P = myCurve->Line().Location();
                        //                myLin = ElSLib::ConeUIso(Cone.Position(),
                        //                                 Cone.RefRadius(),
                        //                                 Cone.SemiAngle(),
                        //                                 P.X());
                        //                gp_Vec Tr(myLin.Direction());
                        //                Tr.Multiply(P.Y());
                        //                myLin.Translate(Tr);
                        //                if (D.IsOpposite(gp::DY2d(), Precision::Angular()))
                        //                    myLin.Reverse();
                        //            }
                        //            else if (STy == GeomAbs_Torus)
                        //            {
                        //                myType = GeomAbs_Circle;
                        //                gp_Torus Tore = mySurface->Torus();
                        //                gp_Pnt2d P = myCurve->Line().Location();
                        //                gp_Ax3 Axis = Tore.Position();
                        //                myCirc = ElSLib::TorusUIso(Axis,
                        //                                  Tore.MajorRadius(),
                        //                                  Tore.MinorRadius(),
                        //                                  P.X());
                        //                myCirc.Rotate(myCirc.Axis(), P.Y());

                        //                if (D.IsOpposite(gp::DY2d(), Precision::Angular()))
                        //                {
                        //                    gp_Ax2 Ax = myCirc.Position();
                        //                    Ax.SetDirection(Ax.Direction().Reversed());
                        //                    myCirc.SetPosition(Ax);
                        //                }
                        //            }
                        //        }
                    }
                }
            }
        }
        public void Load(Adaptor3d_Surface S)
        {
            mySurface = S;
            if (myCurve != null) EvalKPart();
        }

        public void Load(Geom2dAdaptor_Curve C, GeomAdaptor_Surface S)
        {
            Load(C);
            Load(S);

        }

        public override double Resolution(double R3d)
        {
            double ru, rv;
            ru = mySurface.UResolution(R3d);
            rv = mySurface.VResolution(R3d);
            return myCurve.Resolution(Math.Min(ru, rv));
        }

        public override gp_Lin Line()
        {
            Exceptions.Standard_NoSuchObject_Raise_if(myType != GeomAbs_CurveType.GeomAbs_Line, "Adaptor3d_CurveOnSurface::Line(): curve is not a line");
            return myLin;
        }

        public override bool IsPeriodic()
        {
            if (myType == GeomAbs_CurveType.GeomAbs_Circle ||
     myType == GeomAbs_CurveType.GeomAbs_Ellipse)
                return true;

            return myCurve.IsPeriodic();
        }

        public override int NbIntervals(GeomAbs_Shape S)
        {
            throw new NotImplementedException();
        }

        public override void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {
            throw new NotImplementedException();
        }

        public override gp_Circ Circle()
        {
            throw new NotImplementedException();
        }

        public override double Period()
        {
            if (myType == GeomAbs_CurveType.GeomAbs_Circle ||
      myType == GeomAbs_CurveType.GeomAbs_Ellipse)
                return (2.0 * Math.PI);

            return myCurve.Period();
        }

        public override void D1(double d, out gp_Pnt p, out gp_Vec v)
        {
            throw new NotImplementedException();
        }

        //! Returns the point P of parameter U, the first and second
        //! derivatives V1 and V2.
        //! Raised if the continuity of the current interval
        //! is not C2.
        public override void D2(double U, out gp_Pnt P, out gp_Vec V1, out gp_Vec V2)
        {
            gp_Pnt2d UV;
            gp_Vec2d DW, D2W;
            gp_Vec D1U, D1V, D2U, D2V, D2UV;

            P = new gp_Pnt();
            V1 = new gp_Vec();
            V2 = new gp_Vec();


            double FP = myCurve.FirstParameter();
            double LP = myCurve.LastParameter();

            double Tol = Precision.PConfusion() / 10;
            if ((Math.Abs(U - FP) < Tol) && (myFirstSurf != null))
            {
                myCurve.D2(U, UV, DW, D2W);
                myFirstSurf.D2(UV.X(), UV.Y(), P, D1U, D1V, D2U, D2V, D2UV);

                V1.SetLinearForm(DW.X(), D1U, DW.Y(), D1V);
                V2.SetLinearForm(D2W.X(), D1U, D2W.Y(), D1V, 2.0 * DW.X() * DW.Y(), D2UV);
                V2.SetLinearForm(DW.X() * DW.X(), D2U, DW.Y() * DW.Y(), D2V, V2);
            }
            else
              if ((Math.Abs(U - LP) < Tol) && (myLastSurf != null))
            {
                myCurve.D2(U, out UV, out DW, out outD2W);
                myLastSurf.D2(UV.X(), UV.Y(), P, D1U, D1V, D2U, D2V, D2UV);

                V1.SetLinearForm(DW.X(), D1U, DW.Y(), D1V);
                V2.SetLinearForm(D2W.X(), D1U, D2W.Y(), D1V, 2.0 * DW.X() * DW.Y(), D2UV);
                V2.SetLinearForm(DW.X() * DW.X(), D2U, DW.Y() * DW.Y(), D2V, V2);
            }
            else
                if (myType == GeomAbs_CurveType.GeomAbs_Line)
            {
                ElCLib.D1(U, out myLin, out P, out V1);
                V2.SetCoord(0.0, 0.0, 0.0);
            }
            else if (myType == GeomAbs_CurveType.GeomAbs_Circle) ElCLib.D2(U, myCirc, P, V1, V2);
            else
            {
                myCurve.D2(U, out UV, out DW, out D2W);
                mySurface.D2(UV.X(), UV.Y(), P, D1U, D1V, D2U, D2V, D2UV);

                V1.SetLinearForm(DW.X(), D1U, DW.Y(), D1V);
                V2.SetLinearForm(D2W.X(), D1U, D2W.Y(), D1V, 2.0 * DW.X() * DW.Y(), D2UV);
                V2.SetLinearForm(DW.X() * DW.X(), D2U, DW.Y() * DW.Y(), D2V, V2);
            }
        }
    }
}
