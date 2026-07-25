using OCCPort.Common;
using TKernel;
using TKG2d;
using TKG3d;
using TKMath;

namespace TKGeomBase
{
    //! Compute the 2d-curve.  Try to solve the particular
    //! case if possible.  Otherwise, an approximation  is
    //! done. For approximation some parameters are used, including 
    //! required tolerance of approximation.
    //! Tolerance is maximal possible value of 3d deviation of 3d projection of projected curve from
    //! "exact" 3d projection. Since algorithm searches 2d curve on surface, required 2d tolerance is computed
    //! from 3d tolerance with help of U,V resolutions of surface.
    //! 3d and 2d tolerances have sense only for curves on surface, it defines precision of projecting and approximation
    //! and have nothing to do with distance between the projected curve and the surface.
    public class ProjLib_ProjectedCurve : Adaptor2d_Curve2d
    {

        double myTolerance;
        Adaptor3d_Surface mySurface;
        Adaptor3d_Curve myCurve;
        //ProjLib_Projector myResult;
        int myDegMin;
        int myDegMax;
        int myMaxSegments;
        double myMaxDist;
        AppParCurves_Constraint myBndPnt;

        public ProjLib_ProjectedCurve
    (Adaptor3d_Surface S,
    Adaptor3d_Curve C)
        {
            myTolerance = (Precision.Confusion());
            myDegMin = (-1);
            myDegMax = (-1);
            myMaxSegments = (-1);
            myMaxDist = (-1.0);
            myBndPnt = AppParCurves_Constraint.AppParCurves_TangencyPoint;
            Load(S);
            Perform(C);
        }


        public override double FirstParameter()
        {
            return myCurve.FirstParameter();
        }


        //=======================================================================
        //function : LastParameter
        //purpose  : 
        //=======================================================================

        public override double LastParameter()
        {
            return myCurve.LastParameter();
        }

        public override gp_Lin2d Line()
        {
            return myResult.Line();
        }

        public void Load(Adaptor3d_Surface S)
        {
            mySurface = S;
        }
        public static void Project(ProjLib_Projector P, Adaptor3d_Curve C)
        {
            GeomAbs_CurveType CType = C._GetType();
            switch (CType)
            {
                case GeomAbs_CurveType.GeomAbs_Line:
                    P.Project(C.Line());
                    break;
                case GeomAbs_CurveType.GeomAbs_Circle:
                    P.Project(C.Circle());
                    break;
                //case GeomAbs_Ellipse:
                //    P.Project(C->Ellipse());
                //    break;
                //case GeomAbs_Hyperbola:
                //    P.Project(C->Hyperbola());
                //    break;
                //case GeomAbs_Parabola:
                //    P.Project(C->Parabola());
                //    break;
                //case GeomAbs_BSplineCurve:
                //case GeomAbs_BezierCurve:
                //case GeomAbs_OffsetCurve:
                //case GeomAbs_OtherCurve:    // try the approximation
                //    break;
                default:
                    throw new Standard_NoSuchObject(" ");
            }
        }

        public void Perform(Adaptor3d_Curve C)
        {
            myTolerance = Math.Max(myTolerance, Precision.Confusion());
            myCurve = C;
            double FirstPar = C.FirstParameter();
            double LastPar = C.LastParameter();
            GeomAbs_SurfaceType SType = mySurface._GetType();
            GeomAbs_CurveType CType = myCurve._GetType();
            bool isAnalyticalSurf = true;
            bool[] IsTrimmed = { false, false };
            int[] SingularCase = new int[2];
            double eps = 0.01;
            double TolConf = Precision.Confusion();
            double dt = (LastPar - FirstPar) * eps;
            double U1 = 0.0, U2 = 0.0, V1 = 0.0, V2 = 0.0;
            U1 = mySurface.FirstUParameter();
            U2 = mySurface.LastUParameter();
            V1 = mySurface.FirstVParameter();
            V2 = mySurface.LastVParameter();

            switch (SType)
            {
                case GeomAbs_SurfaceType.GeomAbs_Plane:
                    {
                        ProjLib_Plane P = new ProjLib_Plane(mySurface.Plane());
                        Project(P, myCurve);
                        myResult = P;
                    }
                    break;

                case GeomAbs_SurfaceType.GeomAbs_Cylinder:
                    {
                        ProjLib_Cylinder P = new(mySurface.Cylinder());
                        Project(P, myCurve);
                        myResult = P;
                    }
                    break;


                //case GeomAbs_Cone:
                //	{
                //		ProjLib_Cone P(mySurface->Cone());
                //		Project(P, myCurve);
                //		myResult = P;
                //	}
                //	break;

                //case GeomAbs_Sphere:
                //	{
                //		ProjLib_Sphere P(mySurface->Sphere());
                //		Project(P, myCurve);
                //		if (P.IsDone())
                //		{
                //			// on met dans la pseudo-periode ( car Sphere n'est pas
                //			// periodique en V !)
                //			P.SetInBounds(myCurve->FirstParameter());
                //		}
                //		else
                //		{
                //			const Standard_Real Vmax = M_PI / 2.;
                //			const Standard_Real Vmin = -Vmax;
                //			const Standard_Real minang = 1.e - 5 * M_PI;
                //			gp_Sphere aSph = mySurface->Sphere();
                //			Standard_Real anR = aSph.Radius();
                //			Standard_Real f = myCurve->FirstParameter();
                //			Standard_Real l = myCurve->LastParameter();

                //			gp_Pnt Pf = myCurve->Value(f);
                //			gp_Pnt Pl = myCurve->Value(l);
                //			gp_Pnt aLoc = aSph.Position().Location();
                //			Standard_Real maxdist = Max(Pf.Distance(aLoc), Pl.Distance(aLoc));
                //			TolConf = Max(anR * minang, Abs(anR - maxdist));

                //			//Surface has pole at V = Vmin and Vmax
                //			gp_Pnt Pole = mySurface->Value(U1, Vmin);
                //			TrimC3d(myCurve, IsTrimmed, dt, Pole, SingularCase, 3, TolConf);
                //			Pole = mySurface->Value(U1, Vmax);
                //			TrimC3d(myCurve, IsTrimmed, dt, Pole, SingularCase, 4, TolConf);
                //		}
                //		myResult = P;
                //	}
                //	break;

                //case GeomAbs_Torus:
                //	{
                //		ProjLib_Torus P(mySurface->Torus());
                //		Project(P, myCurve);
                //		myResult = P;
                //	}
                //	break;

                //case GeomAbs_BezierSurface:
                //case GeomAbs_BSplineSurface:
                //	{
                //		isAnalyticalSurf = Standard_False;
                //		Standard_Real f, l;
                //		f = myCurve->FirstParameter();
                //		l = myCurve->LastParameter();
                //		dt = (l - f) * eps;

                //		const Adaptor3d_Surface&S = *mySurface;
                //		U1 = S.FirstUParameter();
                //		U2 = S.LastUParameter();
                //		V1 = S.FirstVParameter();
                //		V2 = S.LastVParameter();

                //		if (IsoIsDeg(S, U1, GeomAbs_IsoU, 0., myTolerance))
                //		{
                //			//Surface has pole at U = Umin
                //			gp_Pnt Pole = mySurface->Value(U1, V1);
                //			TrimC3d(myCurve, IsTrimmed, dt, Pole, SingularCase, 1, TolConf);
                //		}

                //		if (IsoIsDeg(S, U2, GeomAbs_IsoU, 0., myTolerance))
                //		{
                //			//Surface has pole at U = Umax
                //			gp_Pnt Pole = mySurface->Value(U2, V1);
                //			TrimC3d(myCurve, IsTrimmed, dt, Pole, SingularCase, 2, TolConf);
                //		}

                //		if (IsoIsDeg(S, V1, GeomAbs_IsoV, 0., myTolerance))
                //		{
                //			//Surface has pole at V = Vmin
                //			gp_Pnt Pole = mySurface->Value(U1, V1);
                //			TrimC3d(myCurve, IsTrimmed, dt, Pole, SingularCase, 3, TolConf);
                //		}

                //		if (IsoIsDeg(S, V2, GeomAbs_IsoV, 0., myTolerance))
                //		{
                //			//Surface has pole at V = Vmax
                //			gp_Pnt Pole = mySurface->Value(U1, V2);
                //			TrimC3d(myCurve, IsTrimmed, dt, Pole, SingularCase, 4, TolConf);
                //		}

                //		ProjLib_ComputeApproxOnPolarSurface polar;
                //		polar.SetTolerance(myTolerance);
                //		polar.SetDegree(myDegMin, myDegMax);
                //		polar.SetMaxSegments(myMaxSegments);
                //		polar.SetBndPnt(myBndPnt);
                //		polar.SetMaxDist(myMaxDist);
                //		polar.Perform(myCurve, mySurface);

                //		Handle(Geom2d_BSplineCurve) aRes = polar.BSpline();

                //		if (!aRes.IsNull())
                //		{
                //			myTolerance = polar.Tolerance();
                //			if ((IsTrimmed[0] || IsTrimmed[1]))
                //			{
                //				if (IsTrimmed[0])
                //				{
                //					//Add segment before start of curve
                //					f = myCurve->FirstParameter();
                //					ExtendC2d(aRes, f, -dt, U1, U2, V1, V2, 0, SingularCase[0]);
                //				}
                //				if (IsTrimmed[1])
                //				{
                //					//Add segment after end of curve
                //					l = myCurve->LastParameter();
                //					ExtendC2d(aRes, l, dt, U1, U2, V1, V2, 1, SingularCase[1]);
                //				}
                //				Handle(Geom2d_Curve) NewCurve2d;
                //				GeomLib::SameRange(Precision::PConfusion(), aRes,
                //				  aRes->FirstParameter(), aRes->LastParameter(),
                //				  FirstPar, LastPar, NewCurve2d);
                //				aRes = Handle(Geom2d_BSplineCurve)::DownCast(NewCurve2d);
                //			}
                //			myResult.SetBSpline(aRes);
                //			myResult.Done();
                //			myResult.SetType(GeomAbs_BSplineCurve);
                //		}
                //	}
                //	break;

                default:
                    {
                        isAnalyticalSurf = false;
                        double[] Vsingular = new double[2] { 0.0, 0.0 }; //for surfaces of revolution
                        double f = 0.0, l = 0.0;
                        dt = 0.0;

                        if (mySurface._GetType() == GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution)
                        {
                            //			//Check possible singularity

                            //			gp_Pnt P = mySurface->AxeOfRevolution().Location();
                            //			gp_Dir N = mySurface->AxeOfRevolution().Direction();
                            //			gp_Lin L(P, N);

                            //			f = myCurve->FirstParameter();
                            //			l = myCurve->LastParameter();
                            //			dt = (l - f) * eps;

                            //			P = myCurve->Value(f);
                            //			if (L.Distance(P) < Precision::Confusion())
                            //			{
                            //				IsTrimmed[0] = Standard_True;
                            //				f = f + dt;
                            //				myCurve = myCurve->Trim(f, l, Precision::Confusion());
                            //				// Searching the parameter on the basis curve for surface of revolution
                            //				Extrema_ExtPC anExtr(P, * mySurface->BasisCurve(), myTolerance);
                            //				if (anExtr.IsDone())
                            //				{
                            //					Standard_Real aMinDist = RealLast();
                            //					for (Standard_Integer anIdx = 1; anIdx <= anExtr.NbExt(); anIdx++)
                            //					{
                            //						if (anExtr.IsMin(anIdx) &&
                            //							anExtr.SquareDistance(anIdx) < aMinDist)
                            //						{
                            //							aMinDist = anExtr.SquareDistance(anIdx);
                            //							Vsingular[0] = anExtr.Point(anIdx).Parameter();
                            //						}
                            //					}
                            //				}
                            //				else
                            //					Vsingular[0] = ElCLib::Parameter(L, P);
                            //				//SingularCase[0] = 3;
                            //			}

                            //			P = myCurve->Value(l);
                            //			if (L.Distance(P) < Precision::Confusion())
                            //			{
                            //				IsTrimmed[1] = Standard_True;
                            //				l = l - dt;
                            //				myCurve = myCurve->Trim(f, l, Precision::Confusion());
                            //				// Searching the parameter on the basis curve for surface of revolution
                            //				Extrema_ExtPC anExtr(P, * mySurface->BasisCurve(), myTolerance);
                            //				if (anExtr.IsDone())
                            //				{
                            //					Standard_Real aMinDist = RealLast();
                            //					for (Standard_Integer anIdx = 1; anIdx <= anExtr.NbExt(); anIdx++)
                            //					{
                            //						if (anExtr.IsMin(anIdx) &&
                            //							anExtr.SquareDistance(anIdx) < aMinDist)
                            //						{
                            //							aMinDist = anExtr.SquareDistance(anIdx);
                            //							Vsingular[1] = anExtr.Point(anIdx).Parameter();
                            //						}
                            //					}
                            //				}
                            //				else
                            //					Vsingular[1] = ElCLib::Parameter(L, P);
                            //				//SingularCase[1] = 4;
                            //			}
                        }

                        double aTolU = Math.Max(ComputeTolU(mySurface, myTolerance), Precision.Confusion());
                        double aTolV = Math.Max(ComputeTolV(mySurface, myTolerance), Precision.Confusion());
                        double aTol2d = Math.Sqrt(aTolU * aTolU + aTolV * aTolV);

                        double aMaxDist = 100.0 * myTolerance;
                        if (myMaxDist > 0.0)
                        {
                            aMaxDist = myMaxDist;
                        }
                        ProjLib_CompProjectedCurve HProjector = new ProjLib_CompProjectedCurve(mySurface, myCurve, aTolU, aTolV, aMaxDist);

                        // Normalement, dans le cadre de ProjLib, le resultat 
                        // doit etre une et une seule courbe !!!
                        // De plus, cette courbe ne doit pas etre Single point
                        int NbCurves = HProjector.NbCurves();
                        double Udeb, Ufin;

                        if (NbCurves > 0)
                            HProjector.Bounds(1, out Udeb, out Ufin);
                        else
                            return;

                        // Approximons cette courbe algorithmique.
                        bool Only3d = false;
                        bool Only2d = true;
                        GeomAbs_Shape Continuity = GeomAbs_Shape.GeomAbs_C1;
                        if (myBndPnt == AppParCurves_Constraint.AppParCurves_PassPoint)
                        {
                            Continuity = GeomAbs_Shape.GeomAbs_C0;
                        }
                        int MaxDegree = 14;
                        if (myDegMax > 0)
                        {
                            MaxDegree = myDegMax;
                        }
                        int MaxSeg = 16;
                        if (myMaxSegments > 0)
                        {
                            MaxSeg = myMaxSegments;
                        }

                        Approx_CurveOnSurface appr = new Approx_CurveOnSurface(HProjector, mySurface, Udeb, Ufin, myTolerance);
                        appr.Perform(MaxSeg, MaxDegree, Continuity, Only3d, Only2d);

                        Geom2d_BSplineCurve aRes = appr.Curve2d();

                        //		if (!aRes.IsNull())
                        //		{
                        //			aTolU = appr.MaxError2dU();
                        //			aTolV = appr.MaxError2dV();
                        //			Standard_Real aNewTol2d = Sqrt(aTolU * aTolU + aTolV * aTolV);
                        //			myTolerance *= (aNewTol2d / aTol2d);
                        //			if (IsTrimmed[0] || IsTrimmed[1])
                        //			{
                        //				// Treatment only for surface of revolution
                        //				Standard_Real u1, u2, v1, v2;
                        //				u1 = mySurface->FirstUParameter();
                        //				u2 = mySurface->LastUParameter();
                        //				v1 = mySurface->FirstVParameter();
                        //				v2 = mySurface->LastVParameter();

                        //				if (IsTrimmed[0])
                        //				{
                        //					//Add segment before start of curve
                        //					ExtendC2d(aRes, f, -dt, u1, u2, Vsingular[0], v2, 0, 3);
                        //				}
                        //				if (IsTrimmed[1])
                        //				{
                        //					//Add segment after end of curve
                        //					ExtendC2d(aRes, l, dt, u1, u2, v1, Vsingular[1], 1, 4);
                        //				}
                        //				Handle(Geom2d_Curve) NewCurve2d;
                        //				GeomLib::SameRange(Precision::PConfusion(), aRes,
                        //				  aRes->FirstParameter(), aRes->LastParameter(),
                        //				  FirstPar, LastPar, NewCurve2d);
                        //				aRes = Handle(Geom2d_BSplineCurve)::DownCast(NewCurve2d);
                        //				if (Continuity == GeomAbs_C0)
                        //				{
                        //					// try to smoother the Curve GeomAbs_C1.
                        //					Standard_Integer aDeg = aRes->Degree();
                        //					Standard_Boolean OK = Standard_True;
                        //					Standard_Real aSmoothTol = Max(Precision::Confusion(), aNewTol2d);
                        //					for (Standard_Integer ij = 2; ij < aRes->NbKnots(); ij++)
                        //					{
                        //						OK = OK && aRes->RemoveKnot(ij, aDeg - 1, aSmoothTol);
                        //					}
                        //				}
                        //			}

                        //			myResult.SetBSpline(aRes);
                        //			myResult.Done();
                        //myResult.SetType(GeomAbs_BSplineCurve);
                        //		}
                    }
                    break;
            }

            if (!myResult.IsDone() && isAnalyticalSurf)
            {
                //	// Use advanced analytical projector if base analytical projection failed.
                ProjLib_ComputeApprox Comp = new ProjLib_ComputeApprox();
                Comp.SetTolerance(myTolerance);
                Comp.SetDegree(myDegMin, myDegMax);
                Comp.SetMaxSegments(myMaxSegments);
                Comp.SetBndPnt(myBndPnt);
                Comp.Perform(myCurve, mySurface);
                if (Comp.Bezier() == null && Comp.BSpline() == null)
                    return; // advanced projector has been failed too

                myResult.Done();
                Geom2d_BSplineCurve aRes;
                if (Comp.BSpline() == null)
                {
                    aRes = Geom2dConvert.CurveToBSplineCurve(Comp.Bezier());
                }
                else
                {
                    aRes = Comp.BSpline();
                }
                //	if ((IsTrimmed[0] || IsTrimmed[1]))
                //	{
                //		if (IsTrimmed[0])
                //		{
                //			//Add segment before start of curve
                //			Standard_Real f = myCurve->FirstParameter();
                //			ExtendC2d(aRes, f, -dt, U1, U2, V1, V2, 0, SingularCase[0]);
                //		}
                //		if (IsTrimmed[1])
                //		{
                //			//Add segment after end of curve
                //			Standard_Real l = myCurve->LastParameter();
                //			ExtendC2d(aRes, l, dt, U1, U2, V1, V2, 1, SingularCase[1]);
                //		}
                //		Handle(Geom2d_Curve) NewCurve2d;
                //		GeomLib::SameRange(Precision::PConfusion(), aRes,
                //		  aRes->FirstParameter(), aRes->LastParameter(),
                //		  FirstPar, LastPar, NewCurve2d);
                //		aRes = Handle(Geom2d_BSplineCurve)::DownCast(NewCurve2d);
                //		myResult.SetBSpline(aRes);
                //		myResult.SetType(GeomAbs_BSplineCurve);
                //	}
                //	else
                //	{
                //		// set the type
                //		if (SType == GeomAbs_Plane && CType == GeomAbs_BezierCurve)
                //		{
                //			myResult.SetType(GeomAbs_BezierCurve);
                //			myResult.SetBezier(Comp.Bezier());
                //		}
                //		else
                //		{
                //			myResult.SetType(GeomAbs_BSplineCurve);
                //			myResult.SetBSpline(Comp.BSpline());
                //		}
                //	}
                //	// set the periodicity flag
                //	if (SType == GeomAbs_Plane &&
                //	  CType == GeomAbs_BSplineCurve &&
                //	  myCurve->IsPeriodic())
                //	{
                //		myResult.SetPeriodic();
                //	}
                //	myTolerance = Comp.Tolerance();
                //}

                //bool[] isPeriodic = {mySurface.IsUPeriodic(),
                //				   mySurface.IsVPeriodic()};
                //if (myResult.IsDone() && (isPeriodic[0] || isPeriodic[1]))
                {
                    // Check result curve to be in params space.

                    // U and V parameters space correspondingly.
                    //double[] aSurfFirstPar = {mySurface->FirstUParameter(),
                    //							mySurface->FirstVParameter()};
                    //double[] aSurfPeriod = { 0.0, 0.0 };
                    //if (isPeriodic[0])
                    //	aSurfPeriod[0] = mySurface.UPeriod();
                    //if (isPeriodic[1])
                    //	aSurfPeriod[1] = mySurface.VPeriod();

                    //for (int anIdx = 1; anIdx <= 2; anIdx++)
                    //{
                    //	if (!isPeriodic[anIdx - 1])
                    //		continue;

                    //	if (myResult.GetType() == GeomAbs_BSplineCurve)
                    //	{
                    //		NCollection_DataMap<Standard_Integer, Standard_Integer> aMap;
                    //		Handle(Geom2d_BSplineCurve) aRes = myResult.BSpline();
                    //		const Standard_Integer aDeg = aRes->Degree();

                    //		for (Standard_Integer aKnotIdx = aRes->FirstUKnotIndex();
                    //							 aKnotIdx < aRes->LastUKnotIndex();
                    //							 aKnotIdx++)
                    //		{
                    //			const Standard_Real aFirstParam = aRes->Knot(aKnotIdx);
                    //			const Standard_Real aLastParam = aRes->Knot(aKnotIdx + 1);

                    //			for (Standard_Integer anIntIdx = 0; anIntIdx <= aDeg; anIntIdx++)
                    //			{
                    //				const Standard_Real aCurrParam = aFirstParam + (aLastParam - aFirstParam) * anIntIdx / (aDeg + 1.0);
                    //				gp_Pnt2d aPnt2d;
                    //				aRes->D0(aCurrParam, aPnt2d);

                    //				Standard_Integer aMapKey = Standard_Integer((aPnt2d.Coord(anIdx) - aSurfFirstPar[anIdx - 1]) / aSurfPeriod[anIdx - 1]);

                    //				if (aPnt2d.Coord(anIdx) - aSurfFirstPar[anIdx - 1] < 0.0)
                    //					aMapKey--;

                    //				if (aMap.IsBound(aMapKey))
                    //					aMap.ChangeFind(aMapKey)++;
                    //				else
                    //					aMap.Bind(aMapKey, 1);
                    //			}
                    //		}

                    //		Standard_Integer aMaxPoints = 0, aMaxIdx = 0;
                    //		NCollection_DataMap<Standard_Integer, Standard_Integer>::Iterator aMapIter(aMap);
                    //		for (; aMapIter.More(); aMapIter.Next())
                    //		{
                    //			if (aMapIter.Value() > aMaxPoints)
                    //			{
                    //				aMaxPoints = aMapIter.Value();
                    //				aMaxIdx = aMapIter.Key();
                    //			}
                    //		}
                    //		if (aMaxIdx != 0)
                    //		{
                    //			gp_Pnt2d aFirstPnt = aRes->Value(aRes->FirstParameter());
                    //			gp_Pnt2d aSecondPnt = aFirstPnt;
                    //			aSecondPnt.SetCoord(anIdx, aFirstPnt.Coord(anIdx) - aSurfPeriod[anIdx - 1] * aMaxIdx);
                    //			aRes->Translate(gp_Vec2d(aFirstPnt, aSecondPnt));
                    //		}
                    //	}

                    //	if (myResult.GetType() == GeomAbs_Line)
                    //	{
                    //		Standard_Real aT1 = myCurve->FirstParameter();
                    //		Standard_Real aT2 = myCurve->LastParameter();

                    //		if (anIdx == 1)
                    //		{
                    //			// U param space.
                    //			myResult.UFrame(aT1, aT2, aSurfFirstPar[anIdx - 1], aSurfPeriod[anIdx - 1]);
                    //		}
                    //		else
                    //		{
                    //			// V param space.
                    //			myResult.VFrame(aT1, aT2, aSurfFirstPar[anIdx - 1], aSurfPeriod[anIdx - 1]);
                    //		}
                    //	}
                }
            }
        }
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

        ProjLib_Projector myResult;

        public override GeomAbs_CurveType _GetType()
        {
            return myResult._GetType();
        }

        public override int Degree()
        {
            throw new NotImplementedException();
        }

        public override int NbKnots()
        {
            throw new NotImplementedException();
        }

        public override Geom2d_BSplineCurve BSpline()
        {
            throw new NotImplementedException();
        }

        public override Geom2d_BezierCurve Bezier()
        {
            throw new NotImplementedException();
        }

        public override void D1(double U, out gp_Pnt2d P, out gp_Vec2d V)
        {
            throw new NotImplementedException();
        }

        public override void D0(double U, ref gp_Pnt2d P)
        {
            throw new NotImplementedException();
        }

        public override double Resolution(double u)
        {
            throw new NotImplementedException();
        }

        public override bool IsPeriodic()
        {
            throw new NotImplementedException();
        }

        public override gp_Circ2d Circle()
        {

            return myResult.Circle();

        }

        public override double Period()
        {
            throw new Standard_NotImplemented("ProjLib_ProjectedCurve::Period() - method is not implemented");
        }

        public override void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2)
        {
            throw new NotImplementedException();
        }

        public override int NbIntervals(GeomAbs_Shape shape)
        {
            throw new NotImplementedException();
        }

        public override void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {
            throw new NotImplementedException();
        }
    }
}
