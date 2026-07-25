using TKG3d;
using TKMath;

namespace TKGeomBase
{
    //! computes the box from a surface
    //! Functions to add a surface to a bounding box.
    //! The surface is defined from a Geom surface.
    public class BndLib_AddSurface
    {//=======================================================================
     //function : Add
     //purpose  : 
     //=======================================================================
        public static void Add(Adaptor3d_Surface S,
                double Tol,
                Bnd_Box B)
        {

            BndLib_AddSurface.Add(S,
                       S.FirstUParameter(),
                       S.LastUParameter(),
                       S.FirstVParameter(),
                       S.LastVParameter(), Tol, B);
        }

        //=======================================================================
        //function : NbUSamples
        //purpose  : 
        //=======================================================================

        public static int NbUSamples(Adaptor3d_Surface S)
        {
            int N;
            GeomAbs_SurfaceType Type = S._GetType();
            switch (Type)
            {
                case GeomAbs_SurfaceType.GeomAbs_BezierSurface:
                    {
                        N = 2 * S.NbUPoles();
                    }
                    break;
                case GeomAbs_SurfaceType.GeomAbs_BSplineSurface:
                    {
                        Geom_BSplineSurface BS = S.BSpline();
                        N = 2 * (BS.UDegree() + 1) * (BS.NbUKnots() - 1);

                    }
                    break;
                default:
                    N = 33;
                    break;
            }
            return Math.Min(50, N);
        }

        //=======================================================================
        //function : NbVSamples
        //purpose  : 
        //=======================================================================

        public static int NbVSamples(Adaptor3d_Surface S)
        {
            int N;
            GeomAbs_SurfaceType Type = S._GetType();
            switch (Type)
            {
                case GeomAbs_SurfaceType.GeomAbs_BezierSurface:
                    {
                        N = 2 * S.NbVPoles();
                        break;
                    }
                case GeomAbs_SurfaceType.GeomAbs_BSplineSurface:
                    {
                        Geom_BSplineSurface BS = S.BSpline();
                        N = 2 * (BS.VDegree() + 1) * (BS.NbVKnots() - 1);
                        break;
                    }
                default:
                    N = 33;
                    break;
            }
            return Math.Min(50, N);
        }

        //  Modified by skv - Fri Aug 27 12:29:04 2004 OCC6503 End
        //=======================================================================
        //function : Add
        //purpose  : 
        //=======================================================================
        public static void Add(Adaptor3d_Surface S,
                double UMin,
                double UMax,
                double VMin,
                double VMax,
                double Tol,
                Bnd_Box B)
        {
            GeomAbs_SurfaceType Type = S._GetType(); // skv OCC6503

            if (Precision.IsInfinite(VMin) ||
                Precision.IsInfinite(VMax) ||
                Precision.IsInfinite(UMin) ||
                Precision.IsInfinite(UMax))
            {
                //  Modified by skv - Fri Aug 27 12:29:04 2004 OCC6503 Begin
                //     B.SetWhole();
                //     return;
                switch (Type)
                {
                    case GeomAbs_SurfaceType.GeomAbs_Plane:
                        {
                            //TreatInfinitePlane(S.Plane(), UMin, UMax, VMin, VMax, Tol, B);
                            return;
                        }
                    default:
                        {
                            //B.SetWhole();
                            return;
                        }
                }
                //  Modified by skv - Fri Aug 27 12:29:04 2004 OCC6503 End
            }

            //   GeomAbs_SurfaceType Type = S.GetType(); // skv OCC6503

            switch (Type)
            {

                case GeomAbs_SurfaceType.GeomAbs_Plane:
                    {
                        /*gp_Pln Plan = S.Plane();
						B.Add(ElSLib.Value(UMin, VMin, Plan));
						B.Add(ElSLib.Value(UMin, VMax, Plan));
						B.Add(ElSLib.Value(UMax, VMin, Plan));
						B.Add(ElSLib.Value(UMax, VMax, Plan));
						B.Enlarge(Tol);
						*/
                    }
                    break;
                case GeomAbs_SurfaceType.GeomAbs_Cylinder:
                    {
                        //	BndLib.Add(S.Cylinder(), UMin, UMax, VMin, VMax, Tol, B);

                    }
                    break;
                case GeomAbs_SurfaceType.GeomAbs_Cone:
                    {
                        //BndLib.Add(S.Cone(), UMin, UMax, VMin, VMax, Tol, B);

                    }
                    break;
                case GeomAbs_SurfaceType.GeomAbs_Torus:
                    {
                        //BndLib.Add(S.Torus(), UMin, UMax, VMin, VMax, Tol, B);

                    }
                    break;
                case GeomAbs_SurfaceType.GeomAbs_Sphere:
                    {
                        if (Math.Abs(UMin) < Precision.Angular() &&
                            Math.Abs(UMax - 2.0 * Math.PI) < Precision.Angular() &&
                            Math.Abs(VMin + Math.PI / 2.0) < Precision.Angular() &&
                            Math.Abs(VMax - Math.PI / 2.0) < Precision.Angular()) // a whole sphere
							BndLib.Add(S.Sphere(), Tol, B);
						else
							BndLib.Add(S.Sphere(), UMin, UMax, VMin, VMax, Tol, B);

                    }
                    break;
                case GeomAbs_SurfaceType.GeomAbs_OffsetSurface:
                    {
                        Adaptor3d_Surface HS = S.BasisSurface();
                        Add(HS, UMin, UMax, VMin, VMax, Tol, B);
                        B.Enlarge(S.OffsetValue());
                        B.Enlarge(Tol);
                    }
                    break;
                case GeomAbs_SurfaceType.GeomAbs_BezierSurface:
                case GeomAbs_SurfaceType.GeomAbs_BSplineSurface:
                    {
                        bool isUseConvexHullAlgorithm = true;
                        /*	double PTol = Precision.Parametric(Precision.Confusion());
							// Borders of underlying geometry.
							double anUMinParam = UMin, anUMaxParam = UMax,// BSpline case.
										   aVMinParam = VMin, aVMaxParam = VMax;
							Geom_BSplineSurface aBS;*/
                        /*if (Type == GeomAbs_SurfaceType.GeomAbs_BezierSurface)
						{
							// Bezier surface:
							// All of poles used for any parameter,
							// that's why in case of trimmed parameters handled by grid algorithm.

							if (Math.Abs(UMin - S.FirstUParameter()) > PTol ||
								Math.Abs(VMin - S.FirstVParameter()) > PTol ||
								Math.Abs(UMax - S.LastUParameter()) > PTol ||
								Math.Abs(VMax - S.LastVParameter()) > PTol)
							{
								// Borders not equal to topology borders.
								isUseConvexHullAlgorithm = false;
							}
						}
						else
						{
							// BSpline:
							// If Umin, Vmin, Umax, Vmax lies inside geometry bounds then:
							// use convex hull algorithm,
							// if Umin, VMin, Umax, Vmax lies outside then:
							// use grid algorithm on analytic continuation (default case).
							aBS = S.BSpline();
							aBS.Bounds(anUMinParam, anUMaxParam, aVMinParam, aVMaxParam);
							if ((UMin - anUMinParam) < -PTol ||
								 (VMin - aVMinParam) < -PTol ||
								 (UMax - anUMaxParam) > PTol ||
								 (VMax - aVMaxParam) > PTol)
							{
								// Out of geometry borders.
								isUseConvexHullAlgorithm = false;
							}
						}*/

                        /*if (isUseConvexHullAlgorithm)
						{
							int aNbUPoles = S.NbUPoles(), aNbVPoles = S.NbVPoles();
							TColgp_Array2OfPnt Tp(1, aNbUPoles, 1, aNbVPoles);
							int UMinIdx = 0, UMaxIdx = 0;
							int VMinIdx = 0, VMaxIdx = 0;
							bool isUPeriodic = S.IsUPeriodic(), isVPeriodic = S.IsVPeriodic();
							if (Type == GeomAbs_SurfaceType.GeomAbs_BezierSurface)
							{
								S.Bezier()->Poles(Tp);
								UMinIdx = 1; UMaxIdx = aNbUPoles;
								VMinIdx = 1; VMaxIdx = aNbVPoles;
							}
							else
							{
								aBS->Poles(Tp);

								UMinIdx = 1;
								UMaxIdx = aNbUPoles;
								VMinIdx = 1;
								VMaxIdx = aNbVPoles;
								if (UMin > anUMinParam ||
									UMax < anUMaxParam)
								{
									TColStd_Array1OfInteger aMults(1, aBS->NbUKnots());
									TColStd_Array1OfReal aKnots(1, aBS->NbUKnots());
									aBS->UKnots(aKnots);
									aBS->UMultiplicities(aMults);

									ComputePolesIndexes(aKnots,
									  aMults,
									  aBS->UDegree(),
									  UMin, UMax,
									  aNbUPoles,
									  isUPeriodic,
									  UMinIdx, UMaxIdx); // the Output indexes

								}
								if (VMin > aVMinParam ||
								  VMax < aVMaxParam)
								{
									TColStd_Array1OfInteger aMults(1, aBS->NbVKnots());
									TColStd_Array1OfReal aKnots(1, aBS->NbVKnots());
									aBS->VKnots(aKnots);
									aBS->VMultiplicities(aMults);

									ComputePolesIndexes(aKnots,
									  aMults,
									  aBS->VDegree(),
									  VMin, VMax,
									  aNbVPoles,
									  isVPeriodic,
									  VMinIdx, VMaxIdx); // the Output indexes
								}

							}

							// Use poles to build convex hull.
							int ip, jp;
							for (int i = UMinIdx; i <= UMaxIdx; i++)
							{
								ip = i;
								if (isUPeriodic && ip > aNbUPoles)
								{
									ip = ip - aNbUPoles;
								}
								for (int j = VMinIdx; j <= VMaxIdx; j++)
								{
									jp = j;
									if (isVPeriodic && jp > aNbVPoles)
									{
										jp = jp - aNbVPoles;
									}
									B.Add(Tp(ip, jp));
								}
							}

							B.Enlarge(Tol);
						}*/
                    }
                    break;


                default:
                    {
                        int Nu = NbUSamples(S);
                        int Nv = NbVSamples(S);
                        gp_Pnt P = new gp_Pnt();
                        for (int i = 1; i <= Nu; i++)
                        {
                            double U = UMin + ((UMax - UMin) * (i - 1) / (Nu - 1));
                            for (int j = 1; j <= Nv; j++)
                            {
                                double V = VMin + ((VMax - VMin) * (j - 1) / (Nv - 1));
                                S.D0(U, V, ref P);
                                B.Add(P);
                            }
                        }
                        B.Enlarge(Tol);
                    }
                    break;
            }
        }
        //----- Methods for AddOptimal ---------------------------------------

    }
}
