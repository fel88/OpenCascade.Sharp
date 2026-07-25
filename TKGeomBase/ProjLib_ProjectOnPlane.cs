global using TColStd_Array1OfInteger = TKernel.NCollection_Array1<int>;
using OCCPort.Common;
using System.Reflection.Metadata;
using TKernel;
using TKG3d;
using TKMath;

namespace TKGeomBase
{
    //! Class  used  to project  a 3d curve   on a plane.  The
    //! result will be a 3d curve.
    //!
    //! You  can ask   the projected curve  to  have  the same
    //! parametrization as the original curve.
    //!
    //! The projection can be done  along every direction  not
    //! parallel to the plane.
    public class ProjLib_ProjectOnPlane : Adaptor3d_Curve
    {
        public ProjLib_ProjectOnPlane(gp_Ax3 Pl,
     gp_Dir D)
        {
            myPlane = (Pl);
            myDirection = (D);
            myKeepParam = false;
            myFirstPar = (0.0);
            myLastPar = (0.0);
            myTolerance = (0.0);
            myType = GeomAbs_CurveType.GeomAbs_OtherCurve;
            myIsApprox = false;

            //  if ( Abs(D * Pl.Direction()) < Precision::Confusion()) {
            //    throw Standard_ConstructionError
            //      ("ProjLib_ProjectOnPlane:  The Direction and the Plane are parallel");
            //  }
        }
        //=======================================================================
        //function : Project
        //purpose  : Returns the projection of a point <Point> on a plane 
        //           <ThePlane>  along a direction <TheDir>.
        //=======================================================================
        public override gp_Lin Line()
        {
            if (myType != GeomAbs_CurveType.GeomAbs_Line)
                throw new Standard_NoSuchObject("ProjLib_ProjectOnPlane:Line");

            return myResult.Line();
        }

        public override gp_Circ Circle()
        {
            if (myType != GeomAbs_CurveType.GeomAbs_Circle)
                throw new Standard_NoSuchObject("ProjLib_ProjectOnPlane:Circle");

            return myResult.Circle();
        }

        public static gp_Pnt ProjectPnt(gp_Ax3 ThePlane,
  gp_Dir TheDir,
  gp_Pnt Point)
        {
            gp_Vec PO = new gp_Vec(Point, ThePlane.Location());

            double Alpha = PO * new gp_Vec(ThePlane.Direction());
            Alpha /= TheDir * ThePlane.Direction();

            gp_Pnt P = new gp_Pnt();
            P.SetXYZ(Point.XYZ() + TheDir.XYZ().Multiplied(Alpha));

            return P;
        }

        public void Load(Adaptor3d_Curve C,
  double Tolerance,
  bool KeepParametrization)

        {
            myCurve = C;
            myType = GeomAbs_CurveType.GeomAbs_OtherCurve;
            myIsApprox = false;
            myTolerance = Tolerance;

            Geom_BSplineCurve ApproxCurve = null;
            GeomAdaptor_Curve aGAHCurve;

            Geom_Line GeomLinePtr;
            Geom_Circle GeomCirclePtr = null;
            Geom_Ellipse GeomEllipsePtr = null;/*
            Handle(Geom_Hyperbola) GeomHyperbolaPtr;
            Handle(Geom_Parabola) GeomParabolaPtr;*/

            gp_Lin aLine;
            //gp_Elips Elips;
            //  gp_Hypr  Hypr ;

            int num_knots;
            GeomAbs_CurveType Type = C._GetType();

            gp_Ax2 Axis = new gp_Ax2();
            double R1 = 0.0, R2 = 0.0;

            myKeepParam = KeepParametrization;

            switch (Type)
            {
                case GeomAbs_CurveType.GeomAbs_Line:
                    {
                        //     P(u) = O + u * Xc
                        // ==> Q(u) = f(P(u)) 
                        //          = f(O) + u * f(Xc)

                        gp_Lin L = myCurve.Line();
                        gp_Vec Xc = ProjectVec(myPlane, myDirection, new gp_Vec(L.Direction()));

                        if (Xc.Magnitude() < Precision.Confusion())
                        { // line orthog au plan
                            myType = GeomAbs_CurveType.GeomAbs_BSplineCurve;
                            gp_Pnt P = ProjectPnt(myPlane, myDirection, L.Location());
                            TColStd_Array1OfInteger Mults = new TColStd_Array1OfInteger(1, 2);
                            Mults.Init(2);
                            TColgp_Array1OfPnt Poles = new TColgp_Array1OfPnt(1, 2);
                            Poles.Init(P);
                            TColStd_Array1OfReal Knots = new TColStd_Array1OfReal(1, 2);
                            Knots[1] = (float)myCurve.FirstParameter();
                            Knots[2] = (float)myCurve.LastParameter();
                            //Geom_BSplineCurve BSP =
                            //new Geom_BSplineCurve(Poles, Knots, Mults, 1);

                            ////  Modified by Sergey KHROMOV - Tue Jan 29 16:57:29 2002 Begin
                            //GeomAdaptor_Curve aGACurve = new GeomAdaptor_Curve(BSP);
                            //myResult = new GeomAdaptor_Curve(aGACurve);
                            ////  Modified by Sergey KHROMOV - Tue Jan 29 16:57:30 2002 End
                        }
                        else if (Math.Abs(Xc.Magnitude() - 1.0) < Precision.Confusion())
                        {
                            myType = GeomAbs_CurveType.GeomAbs_Line;
                            gp_Pnt P = ProjectPnt(myPlane, myDirection, L.Location());
                            myFirstPar = myCurve.FirstParameter();
                            myLastPar = myCurve.LastParameter();
                            aLine = new gp_Lin(P, new gp_Dir(Xc));
                            GeomLinePtr = new Geom_Line(aLine);

                            //  Modified by Sergey KHROMOV - Tue Jan 29 16:57:29 2002 Begin
                            GeomAdaptor_Curve aGACurve = new GeomAdaptor_Curve(GeomLinePtr,
                              myCurve.FirstParameter(),
                              myCurve.LastParameter());
                            //myResult = new GeomAdaptor_Curve(aGACurve);
                            myResult = aGACurve;
                            //  Modified by Sergey KHROMOV - Tue Jan 29 16:57:30 2002 End
                        }
                        else
                        {
                            myType = GeomAbs_CurveType.GeomAbs_Line;
                            gp_Pnt P = ProjectPnt(myPlane, myDirection, L.Location());
                            aLine = new gp_Lin(P, new gp_Dir(Xc));
                            double Udeb, Ufin;

                            // eval the first and last parameters of the projected curve
                            Udeb = myCurve.FirstParameter();
                            Ufin = myCurve.LastParameter();
                            gp_Pnt P1 = ProjectPnt(myPlane, myDirection,
                              myCurve.Value(Udeb));
                            gp_Pnt P2 = ProjectPnt(myPlane, myDirection,
                              myCurve.Value(Ufin));
                            myFirstPar = new gp_Vec(aLine.Direction()).Dot(new gp_Vec(P, P1));
                            myLastPar = new gp_Vec(aLine.Direction()).Dot(new gp_Vec(P, P2));
                            GeomLinePtr = new Geom_Line(aLine);
                            if (!myKeepParam)
                            {
                                //  Modified by Sergey KHROMOV - Tue Jan 29 16:57:29 2002 Begin
                                GeomAdaptor_Curve aGACurve = new GeomAdaptor_Curve(GeomLinePtr,
                                  myFirstPar,
                                  myLastPar);
                                //myResult = new GeomAdaptor_Curve(aGACurve);
                                myResult = aGACurve;
                                //  Modified by Sergey KHROMOV - Tue Jan 29 16:57:30 2002 End
                            }
                            else
                            {
                                myType = GeomAbs_CurveType.GeomAbs_BSplineCurve;
                                //
                                // make a linear BSpline of degree 1 between the end points of
                                // the projected line 
                                //
                                //Geom_TrimmedCurve NewTrimCurvePtr =
                                //  new Geom_TrimmedCurve(GeomLinePtr,
                                //    myFirstPar,
                                //    myLastPar);

                                //Geom_BSplineCurve NewCurvePtr =
                                //  GeomConvert::CurveToBSplineCurve(NewTrimCurvePtr);
                                //num_knots = NewCurvePtr->NbKnots();
                                //TColStd_Array1OfReal BsplineKnots(1, num_knots);
                                //NewCurvePtr.Knots(BsplineKnots);

                                //BSplCLib::Reparametrize(myCurve.FirstParameter(),
                                //  myCurve.LastParameter(),
                                //  BsplineKnots);

                                //NewCurvePtr.SetKnots(BsplineKnots);
                                ////  Modified by Sergey KHROMOV - Tue Jan 29 16:57:29 2002 Begin
                                //GeomAdaptor_Curve aGACurve = new GeomAdaptor_Curve(NewCurvePtr);
                                //myResult = new GeomAdaptor_Curve(aGACurve);
                                ////  Modified by Sergey KHROMOV - Tue Jan 29 16:57:30 2002 End
                            }
                        }
                        break;
                    }
                    break;
                case GeomAbs_CurveType.GeomAbs_Circle:
                    // Pour le cercle et l ellipse on a les relations suivantes:
                    // ( Rem : pour le cercle R1 = R2 = R)
                    //     P(u) = O + R1 * Cos(u) * Xc + R2 * Sin(u) * Yc
                    // ==> Q(u) = f(P(u)) 
                    //          = f(O) + R1 * Cos(u) * f(Xc) + R2 * Sin(u) * f(Yc)

                    gp_Circ Circ = myCurve.Circle();
                    Axis = Circ.Position();
                    R1 = R2 = Circ.Radius();
                    goto case GeomAbs_CurveType.GeomAbs_Ellipse;

                case GeomAbs_CurveType.GeomAbs_Ellipse:
                    {

                        {
                            if (Type == GeomAbs_CurveType.GeomAbs_Ellipse)
                            {
                                //gp_Elips E = myCurve->Ellipse();
                                //Axis = E.Position();
                                //R1 = E.MajorRadius();
                                //R2 = E.MinorRadius();
                            }

                            // Common Code  for CIRCLE & ELLIPSE begin here
                            gp_Dir X = Axis.XDirection();
                            gp_Dir Y = Axis.YDirection();
                            gp_Vec VDx = ProjectVec(myPlane, myDirection, X);
                            gp_Vec VDy = ProjectVec(myPlane, myDirection, Y);
                            gp_Dir Dx, Dy;

                            double Tol2 = myTolerance * myTolerance;
                            if (VDx.SquareMagnitude() < Tol2 ||
                              VDy.SquareMagnitude() < Tol2 ||
                              VDx.CrossSquareMagnitude(VDy) < Tol2)
                            {
                                myIsApprox = true;
                            }

                            if (!myIsApprox)
                            {
                                Dx = new gp_Dir(VDx);
                                Dy = new gp_Dir(VDy);
                                gp_Pnt O = Axis.Location();
                                gp_Pnt P = ProjectPnt(myPlane, myDirection, O);
                                gp_Pnt Px = ProjectPnt(myPlane, myDirection, O.Translated(R1 * new gp_Vec(X)));
                                gp_Pnt Py = ProjectPnt(myPlane, myDirection, O.Translated(R2 * new gp_Vec(Y)));
                                double Major = P.Distance(Px);
                                double Minor = P.Distance(Py);

                                if (myKeepParam)
                                {
                                    myIsApprox = !new gp_Dir(VDx).IsNormal(new gp_Dir(VDy), Precision.Angular());
                                }
                                else
                                {
                                    // Since it is not necessary to keep the same parameter for the point on the original and on the projected curves,
                                    // we will use the following approach to find axes of the projected ellipse and provide the canonical curve:
                                    // https://www.geometrictools.com/Documentation/ParallelProjectionEllipse.pdf
                                    math_Matrix aMatrA = new(1, 2, 1, 2);
                                    // A = Jp^T * Pr(Je), where
                                    //   Pr(Je) - projection of axes of original ellipse to the target plane
                                    //   Jp - X and Y axes of the target plane
                                    aMatrA[1, 1] = myPlane.XDirection().XYZ().Dot(VDx.XYZ());
                                    aMatrA[1, 2] = myPlane.XDirection().XYZ().Dot(VDy.XYZ());
                                    aMatrA[2, 1] = myPlane.YDirection().XYZ().Dot(VDx.XYZ());
                                    aMatrA[2, 2] = myPlane.YDirection().XYZ().Dot(VDy.XYZ());

                                    math_Matrix aMatrDelta2 = new(1, 2, 1, 2, 0.0);
                                    //           | 1/MajorRad^2       0       |
                                    // Delta^2 = |                            |
                                    //           |      0        1/MajorRad^2 |
                                    aMatrDelta2[1, 1] = 1.0 / (R1 * R1);
                                    aMatrDelta2[2, 2] = 1.0 / (R2 * R2);

                                    math_Matrix aMatrAInv = aMatrA.Inverse();
                                    math_Matrix aMatrM = aMatrAInv.Transposed() * aMatrDelta2 * aMatrAInv;

                                    // perform eigenvalues calculation
                                    math_Jacobi anEigenCalc = new(aMatrM);
                                    if (anEigenCalc.IsDone())
                                    {
                                        // radii of the projected ellipse
                                        Minor = 1.0 / Math.Sqrt(anEigenCalc.Value(1));
                                        Major = 1.0 / Math.Sqrt(anEigenCalc.Value(2));

                                        // calculate the rotation angle for the plane axes to meet the correct axes of the projected ellipse
                                        // (swap eigenvectors in respect to major and minor axes)
                                        math_Matrix anEigenVec = anEigenCalc.Vectors();
                                        gp_Trsf2d aTrsfInPlane = new gp_Trsf2d();
                                        //aTrsfInPlane.SetValues(anEigenVec[1, 2], anEigenVec[1, 1], 0.0,
                                        //  anEigenVec[2, 2], anEigenVec[2, 1], 0.0);
                                        gp_Trsf aRot = new gp_Trsf();
                                        //  aRot.SetRotation(new gp_Ax1(P, myPlane.Direction()), aTrsfInPlane.RotationPart());

                                        Dx = myPlane.XDirection().Transformed(aRot);
                                        Dy = myPlane.YDirection().Transformed(aRot);
                                    }
                                    else
                                    {
                                        myIsApprox = true;
                                    }
                                }

                                if (!myIsApprox)
                                {
                                    gp_Ax2 Axe = new(P, Dx ^ Dy, Dx);

                                    if (Math.Abs(Major - Minor) < Precision.Confusion())
                                    {
                                        myType = GeomAbs_CurveType.GeomAbs_Circle;
                                        gp_Circ Circ1 = new(Axe, Major);
                                        GeomCirclePtr = new Geom_Circle(Circ1);
                                        //  Modified by Sergey KHROMOV - Tue Jan 29 16:57:29 2002 Begin
                                        GeomAdaptor_Curve aGACurve = new(GeomCirclePtr);
                                        myResult = new GeomAdaptor_Curve(aGACurve);
                                        //  Modified by Sergey KHROMOV - Tue Jan 29 16:57:30 2002 End
                                    }
                                    else if (Major > Minor)
                                    {
                                        myType = GeomAbs_CurveType.GeomAbs_Ellipse;
                                        //Elips = gp_Elips(Axe, Major, Minor);

                                        //GeomEllipsePtr = new Geom_Ellipse(Elips);
                                        ////  Modified by Sergey KHROMOV - Tue Jan 29 16:57:29 2002 Begin
                                        //GeomAdaptor_Curve aGACurve(GeomEllipsePtr);
                                        //myResult = new GeomAdaptor_Curve(aGACurve);
                                        ////  Modified by Sergey KHROMOV - Tue Jan 29 16:57:30 2002 End
                                    }
                                    else
                                    {
                                        myIsApprox = true;
                                    }
                                }
                            }

                            // No way to build the canonical curve, approximate as B-spline
                            if (myIsApprox)
                            {
                                //myType = GeomAbs_BSplineCurve;
                                //PerformApprox(myCurve, myPlane, myDirection, ApproxCurve);
                                ////  Modified by Sergey KHROMOV - Tue Jan 29 16:57:29 2002 Begin
                                //GeomAdaptor_Curve aGACurve(ApproxCurve);
                                //myResult = new GeomAdaptor_Curve(aGACurve);
                                ////  Modified by Sergey KHROMOV - Tue Jan 29 16:57:30 2002 End
                            }
                            else if (GeomCirclePtr != null || GeomEllipsePtr != null)
                            {
                                Geom_Curve aResultCurve = GeomCirclePtr;
                                if (aResultCurve == null)
                                    aResultCurve = GeomEllipsePtr;
                                // start and end parameters of the projected curve
                                double aParFirst = myCurve.FirstParameter();
                                double aParLast = myCurve.LastParameter();
                                gp_Pnt aPntFirst = ProjectPnt(myPlane, myDirection, myCurve.Value(aParFirst));
                                gp_Pnt aPntLast = ProjectPnt(myPlane, myDirection, myCurve.Value(aParLast));
                                GeomLib_Tool.Parameter(aResultCurve, aPntFirst, Precision.Confusion(), ref myFirstPar);
                                GeomLib_Tool.Parameter(aResultCurve, aPntLast, Precision.Confusion(), ref myLastPar);
                                while (myLastPar <= myFirstPar)
                                    myLastPar += myResult.Period();
                            }
                        }
                    }
                    break;
                default:
                    {
                        myKeepParam = true;
                        myIsApprox = true;
                        myType = GeomAbs_CurveType.GeomAbs_BSplineCurve;
                        //PerformApprox(myCurve, myPlane, myDirection, ApproxCurve);
                        //  Modified by Sergey KHROMOV - Tue Jan 29 16:57:29 2002 Begin
                        GeomAdaptor_Curve aGACurve = new GeomAdaptor_Curve(ApproxCurve);
                        //   myResult = new GeomAdaptor_Curve(aGACurve);
                        //  Modified by Sergey KHROMOV - Tue Jan 29 16:57:30 2002 End
                    }
                    break;
            }
        }


        public override double FirstParameter()
        {
            if (myKeepParam || myIsApprox)
                return myCurve.FirstParameter();
            else
                return myFirstPar;
        }

        public override double LastParameter()
        {
            if (myKeepParam || myIsApprox)
                return myCurve.LastParameter();
            else
                return myLastPar;
        }

        //=======================================================================
        //function : Project
        //purpose  : Returns the projection of a Vector <Vec> on a plane 
        //           <ThePlane> along a direction <TheDir>.
        //=======================================================================

        public static gp_Vec ProjectVec(gp_Ax3 ThePlane,
  gp_Dir TheDir,
  gp_Vec Vec)
        {
            gp_Vec D = Vec;//todo replace with OpenTK Vector3d
            gp_Vec Z = ThePlane.Direction();

            D -= ((Vec * Z) / (TheDir * new gp_Dir(Z))) * TheDir;

            return D;
        }

        public override GeomAbs_CurveType _GetType()
        {
            return myType;

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

        public override gp_Pnt Value(double d)
        {
            throw new NotImplementedException();
        }

        public override void D0(double d, ref gp_Pnt p)
        {
            throw new NotImplementedException();
        }

        public override double Resolution(double R3d)
        {
            throw new NotImplementedException();
        }

        public override bool IsPeriodic()
        {
            throw new NotImplementedException();
        }

        public override int NbIntervals(GeomAbs_Shape S)
        {
            return myCurve.NbIntervals(S);
        }

        public override void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {
            throw new NotImplementedException();
        }

        public override double Period()
        {
            if (!IsPeriodic())
            {
                throw new Standard_NoSuchObject("ProjLib_ProjectOnPlane::Period");
            }

            if (myIsApprox)
                //return false;
                return 0;
            else
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
            if (myType != GeomAbs_CurveType.GeomAbs_OtherCurve)
            {
                myResult.D2(U, out P, out V1, out V2);
            }
            else
            {
                OnPlane_D2(U,
                 out P,
                  out V1,
                out V2,
                  myCurve,
                  myPlane,
                  myDirection);
            }
        }

        static bool OnPlane_D2(double U,
          out gp_Pnt P,
 out gp_Vec V1,
 out gp_Vec V2,
   Adaptor3d_Curve aCurvePtr,
   gp_Ax3 Pl,
   gp_Dir D)
        {
            double Alpha;
            gp_Pnt Point;
            gp_Vec Vector1,
              Vector2;

            P = new gp_Pnt();
            V1 = new gp_Vec();
            V2 = new gp_Vec();

            gp_Dir Z = Pl.Direction();

            aCurvePtr.D2(U, out Point, out Vector1, out Vector2);

            // evaluate the point as in `OnPlane_Value`
            gp_Vec PO = new(Point, Pl.Location());
            Alpha = PO * new gp_Vec(Z);
            Alpha /= D * Z;
            P.SetXYZ(Point.XYZ() + Alpha * D.XYZ());

            // evaluate the derivative.
            // 
            //   d(Proj)  d(P)       1        d(P)
            //   ------ = ---  - -------- * ( --- . Z ) * D
            //     dU     dU     ( D . Z)     dU  
            //

            Alpha = Vector1 * new gp_Vec(Z);
            Alpha /= D * Z;

            V1.SetXYZ(Vector1.XYZ() - Alpha * D.XYZ());

            Alpha = Vector2 * new gp_Vec(Z);
            Alpha /= D * Z;

            V2.SetXYZ(Vector2.XYZ() - Alpha * D.XYZ());
            return true;
        }

        Adaptor3d_Curve myCurve;
        gp_Ax3 myPlane;
        gp_Dir myDirection;
        bool myKeepParam;
        double myFirstPar;
        double myLastPar;
        double myTolerance;
        GeomAbs_CurveType myType;
        GeomAdaptor_Curve myResult;
        bool myIsApprox;

    }
}
