using OCCPort.Common;
using TKMath;
using static System.Net.Mime.MediaTypeNames;

namespace TKGeomBase
{
    //! The BndLib package provides functions to add a geometric primitive to a bounding box.
    //! Note: these functions work with gp objects, optionally
    //! limited by parameter values. If the curves and surfaces
    //! provided by the gp package are not explicitly
    //! parameterized, they still have an implicit parameterization,
    //! similar to that which they infer for the equivalent Geom or Geom2d objects.
    //! Add : Package to compute the bounding boxes for elementary
    //! objects from gp in 2d and 3d .
    //!
    //! AddCurve2d : A class to compute the bounding box for a curve
    //! in 2d dimensions ;the curve is defined by a tool
    //!
    //! AddCurve : A class to compute the bounding box for a curve
    //! in 3d dimensions ;the curve is defined by a tool
    //!
    //! AddSurface : A class to compute the bounding box for a surface.
    //! The surface is defined by a tool for the geometry and another
    //! tool for the topology (only the edges in 2d dimensions)
    public class BndLib
    {
        public static void OpenMinMax(gp_Dir V, Bnd_Box B)
        {
            gp_Dir OX = new gp_Dir(1.0, 0.0, 0.0);
            gp_Dir OY = new gp_Dir(0.0, 1.0, 0.0);
            gp_Dir OZ = new gp_Dir(0.0, 0.0, 1.0);
            if (V.IsParallel(OX, Precision.Angular()))
            {
                B.OpenXmax(); B.OpenXmin();
            }
            else if (V.IsParallel(OY, Precision.Angular()))
            {
                B.OpenYmax(); B.OpenYmin();
            }
            else if (V.IsParallel(OZ, Precision.Angular()))
            {
                B.OpenZmax(); B.OpenZmin();
            }
            else
            {
                B.OpenXmin(); B.OpenYmin(); B.OpenZmin();
                B.OpenXmax(); B.OpenYmax(); B.OpenZmax();
            }
        }


        static void ComputeSphere(gp_Sphere Sphere,
                            double UMin, double UMax,
                            double VMin, double VMax,
                           Bnd_Box B)
        {
            gp_Pnt P = Sphere.Location();
            double R = Sphere.Radius();
            double xmin, ymin, zmin, xmax, ymax, zmax;
            xmin = P.X() - R;
            xmax = P.X() + R;
            ymin = P.Y() - R;
            ymax = P.Y() + R;
            zmin = P.Z() - R;
            zmax = P.Z() + R;

            double uper = 2.0 * Math.PI - Precision.PConfusion();
            double vper = Math.PI - Precision.PConfusion();
            if (UMax - UMin >= uper && VMax - VMin >= vper)
            {
                // a whole sphere
                B.Update(xmin, ymin, zmin, xmax, ymax, zmax);
            }
            else
            {
                double u, v;
                double umax = UMin + 2.0 * Math.PI;
                gp_Ax3 Pos = Sphere.Position();
                gp_Pnt PExt = P;
                PExt.SetX(xmin);
                ElSLib.SphereParameters(Pos, R, PExt, out u, out v);
                u = ElCLib.InPeriod(u, UMin, umax);
                if (u >= UMin && u <= UMax && v >= VMin && v <= VMax)
                {
                    B.Add(PExt);
                }
                //
                PExt.SetX(xmax);
                ElSLib.SphereParameters(Pos, R, PExt, out u, out v);
                u = ElCLib.InPeriod(u, UMin, umax);
                if (u >= UMin && u <= UMax && v >= VMin && v <= VMax)
                {
                    B.Add(PExt);
                }
                PExt.SetX(P.X());
                //
                PExt.SetY(ymin);
                ElSLib.SphereParameters(Pos, R, PExt, out u, out v);
                u = ElCLib.InPeriod(u, UMin, umax);
                if (u >= UMin && u <= UMax && v >= VMin && v <= VMax)
                {
                    B.Add(PExt);
                }
                //
                PExt.SetY(ymax);
                ElSLib.SphereParameters(Pos, R, PExt, out u, out v);
                u = ElCLib.InPeriod(u, UMin, umax);
                if (u >= UMin && u <= UMax && v >= VMin && v <= VMax)
                {
                    B.Add(PExt);
                }
                PExt.SetY(P.Y());
                //
                PExt.SetZ(zmin);
                ElSLib.SphereParameters(Pos, R, PExt, out u, out v);
                u = ElCLib.InPeriod(u, UMin, umax);
                if (u >= UMin && u <= UMax && v >= VMin && v <= VMax)
                {
                    B.Add(PExt);
                }
                //
                PExt.SetZ(zmax);
                ElSLib.SphereParameters(Pos, R, PExt, out u, out v);
                u = ElCLib.InPeriod(u, UMin, umax);
                if (u >= UMin && u <= UMax && v >= VMin && v <= VMax)
                {
                    B.Add(PExt);
                }
                //
                // Add boundaries of patch
                // UMin, UMax
                gp_Circ aC = ElSLib.SphereUIso(Pos, R, UMin);
                BndLib.Add(aC, VMin, VMax, 0.0, B);
                aC = ElSLib.SphereUIso(Pos, R, UMax);
                BndLib.Add(aC, VMin, VMax, 0.0, B);
                // VMin, VMax
                aC = ElSLib.SphereVIso(Pos, R, VMin);
                BndLib.Add(aC, UMin, UMax, 0.0, B);
                aC = ElSLib.SphereVIso(Pos, R, VMax);
                BndLib.Add(aC, UMin, UMax, 0.0, B);
            }
        }


        public static void Add(gp_Sphere S, double UMin,
                     double UMax, double VMin,
                     double VMax, double Tol, Bnd_Box B)
        {
            ComputeSphere(S, UMin, UMax, VMin, VMax, B);
            B.Enlarge(Tol);
        }
        public static void Add(gp_Sphere S, double Tol, Bnd_Box B)
        {
            gp_Pnt P = S.Location();
            double R = S.Radius();
            double xmin, ymin, zmin, xmax, ymax, zmax;
            xmin = P.X() - R;
            xmax = P.X() + R;
            ymin = P.Y() - R;
            ymax = P.Y() + R;
            zmin = P.Z() - R;
            zmax = P.Z() + R;
            B.Update(xmin, ymin, zmin, xmax, ymax, zmax);
            B.Enlarge(Tol);
        }

        public static void Add(gp_Circ C,
                     double U1,
                     double U2,
                  double Tol,
                 Bnd_Box B)
        {
            double period = 2.0 * Math.PI - Standard_Real.Epsilon(2.0 * Math.PI);

            double utrim1 = U1, utrim2 = U2;
            if (U2 - U1 > period)
            {
                utrim1 = 0.0;
                utrim2 = 2.0 * Math.PI;
            }
            else
            {
                double tol = Standard_Real.Epsilon(1.0);
                ElCLib.AdjustPeriodic(0.0, 2.0 * Math.PI,
                                       tol,
                                      ref utrim1, ref utrim2);
            }
            double R = C.Radius();
            gp_XYZ O = C.Location().XYZ();
            gp_XYZ Xd = C.XAxis().Direction().XYZ();
            gp_XYZ Yd = C.YAxis().Direction().XYZ();
            gp_Ax2 pos = C.Position();
            //
            double tt;
            double xmin, xmax, txmin, txmax;
            if (Math.Abs(Xd.X()) > gp.Resolution())
            {
                txmin = Math.Atan(Yd.X() / Xd.X());
                txmin = ElCLib.InPeriod(txmin, 0.0, 2.0 * Math.PI);
            }
            else
            {
                txmin = Math.PI / 2.0;
            }
            txmax = txmin <= Math.PI ? txmin + Math.PI : txmin - Math.PI;
            xmin = R * Math.Cos(txmin) * Xd.X() + R * Math.Sin(txmin) * Yd.X() + O.X();
            xmax = R * Math.Cos(txmax) * Xd.X() + R * Math.Sin(txmax) * Yd.X() + O.X();
            if (xmin > xmax)
            {
                tt = xmin;
                xmin = xmax;
                xmax = tt;
                tt = txmin;
                txmin = txmax;
                txmax = tt;
            }
            //
            double ymin, ymax, tymin, tymax;
            if (Math.Abs(Xd.Y()) > gp.Resolution())
            {
                tymin = Math.Atan(Yd.Y() / Xd.Y());
                tymin = ElCLib.InPeriod(tymin, 0.0, 2.0 * Math.PI);
            }
            else
            {
                tymin = Math.PI / 2.0;
            }
            tymax = tymin <= Math.PI ? tymin + Math.PI : tymin - Math.PI;
            ymin = R * Math.Cos(tymin) * Xd.Y() + R * Math.Sin(tymin) * Yd.Y() + O.Y();
            ymax = R * Math.Cos(tymax) * Xd.Y() + R * Math.Sin(tymax) * Yd.Y() + O.Y();
            if (ymin > ymax)
            {
                tt = ymin;
                ymin = ymax;
                ymax = tt;
                tt = tymin;
                tymin = tymax;
                tymax = tt;
            }
            //
            double zmin, zmax, tzmin, tzmax;
            if (Math.Abs(Xd.Z()) > gp.Resolution())
            {
                tzmin = Math.Atan(Yd.Z() / Xd.Z());
                tzmin = ElCLib.InPeriod(tzmin, 0.0, 2.0 * Math.PI);
            }
            else
            {
                tzmin = Math.PI / 2.0;
            }
            tzmax = tzmin <= Math.PI ? tzmin + Math.PI : tzmin - Math.PI;
            zmin = R * Math.Cos(tzmin) * Xd.Z() + R * Math.Sin(tzmin) * Yd.Z() + O.Z();
            zmax = R * Math.Cos(tzmax) * Xd.Z() + R * Math.Sin(tzmax) * Yd.Z() + O.Z();
            if (zmin > zmax)
            {
                tt = zmin;
                zmin = zmax;
                zmax = tt;
                tt = tzmin;
                tzmin = tzmax;
                tzmax = tt;
            }
            //
            if (utrim2 - utrim1 >= period)
            {
                B.Update(xmin, ymin, zmin, xmax, ymax, zmax);
            }
            else
            {
                gp_Pnt P = ElCLib.CircleValue(utrim1, pos, R);
                B.Add(P);
                P = ElCLib.CircleValue(utrim2, pos, R);
                B.Add(P);
                double Xmin, Ymin, Zmin, Xmax, Ymax, Zmax;
                B.FinitePart().Get(out Xmin, out Ymin, out Zmin, out Xmax, out Ymax, out Zmax);
                double gap = B.GetGap();
                Xmin += gap;
                Ymin += gap;
                Zmin += gap;
                Xmax -= gap;
                Ymax -= gap;
                Zmax -= gap;
                //
                txmin = ElCLib.InPeriod(txmin, utrim1, utrim1 + 2.0 * Math.PI);
                if (txmin >= utrim1 && txmin <= utrim2)
                {
                    Xmin = Math.Min(xmin, Xmin);
                }
                txmax = ElCLib.InPeriod(txmax, utrim1, utrim1 + 2.0 * Math.PI);
                if (txmax >= utrim1 && txmax <= utrim2)
                {
                    Xmax = Math.Max(xmax, Xmax);
                }
                //
                tymin = ElCLib.InPeriod(tymin, utrim1, utrim1 + 2.0 * Math.PI);
                if (tymin >= utrim1 && tymin <= utrim2)
                {
                    Ymin = Math.Min(ymin, Ymin);
                }
                tymax = ElCLib.InPeriod(tymax, utrim1, utrim1 + 2.0 * Math.PI);
                if (tymax >= utrim1 && tymax <= utrim2)
                {
                    Ymax = Math.Max(ymax, Ymax);
                }
                //
                tzmin = ElCLib.InPeriod(tzmin, utrim1, utrim1 + 2.0 * Math.PI);
                if (tzmin >= utrim1 && tzmin <= utrim2)
                {
                    Zmin = Math.Min(zmin, Zmin);
                }
                tzmax = ElCLib.InPeriod(tzmax, utrim1, utrim1 + 2.0 * Math.PI);
                if (tzmax >= utrim1 && tzmax <= utrim2)
                {
                    Zmax = Math.Max(zmax, Zmax);
                }
                //
                B.Update(Xmin, Ymin, Zmin, Xmax, Ymax, Zmax);
            }
            //
            B.Enlarge(Tol);
        }
        public static void Add(gp_Lin L, double P1,
                 double P2,
                 double Tol, Bnd_Box B)
        {

            if (Precision.IsNegativeInfinite(P1))
            {
                if (Precision.IsNegativeInfinite(P2))
                {
                    throw new Standard_Failure("BndLib::bad parameter");
                }
                else if (Precision.IsPositiveInfinite(P2))
                {
                    OpenMinMax(L.Direction(), B);
                    B.Add(ElCLib.Value(0.0, L));
                }

                else
                {
                    OpenMin(L.Direction(), B);
                    B.Add(ElCLib.Value(P2, L));
                }
            }
            else if (Precision.IsPositiveInfinite(P1))
            {
                if (Precision.IsNegativeInfinite(P2))
                {
                    OpenMinMax(L.Direction(), B);
                    B.Add(ElCLib.Value(0.0, L));
                }
                else if (Precision.IsPositiveInfinite(P2))
                {
                    throw new Standard_Failure("BndLib::bad parameter");
                }
                else
                {
                    OpenMax(L.Direction(), B);
                    B.Add(ElCLib.Value(P2, L));
                }
            }
            else
            {
                B.Add(ElCLib.Value(P1, L));
                if (Precision.IsNegativeInfinite(P2))
                {
                    OpenMin(L.Direction(), B);
                }
                else if (Precision.IsPositiveInfinite(P2))
                {
                    OpenMax(L.Direction(), B);
                }
                else
                {
                    B.Add(ElCLib.Value(P2, L));
                }
            }
            B.Enlarge(Tol);
        }

        public static void OpenMax(gp_Dir V, Bnd_Box B)
        {
            gp_Dir OX = new gp_Dir(1.0, 0.0, 0.0);
            gp_Dir OY = new gp_Dir(0.0, 1.0, 0.0);
            gp_Dir OZ = new gp_Dir(0.0, 0.0, 1.0);
            if (V.IsParallel(OX, Precision.Angular()))
                B.OpenXmax();
            else if (V.IsParallel(OY, Precision.Angular()))
                B.OpenYmax();
            else if (V.IsParallel(OZ, Precision.Angular()))
                B.OpenZmax();
            else
            {
                B.OpenXmax(); B.OpenYmax(); B.OpenZmax();
            }
        }
        public static void OpenMin(gp_Dir V, Bnd_Box B)
        {
            gp_Dir OX = new gp_Dir(1.0, 0.0, 0.0);
            gp_Dir OY = new(0.0, 1.0, 0.0);
            gp_Dir OZ = new gp_Dir(0.0, 0.0, 1.0);
            if (V.IsParallel(OX, Precision.Angular()))
                B.OpenXmin();
            else if (V.IsParallel(OY, Precision.Angular()))
                B.OpenYmin();
            else if (V.IsParallel(OZ, Precision.Angular()))
                B.OpenZmin();
            else
            {
                B.OpenXmin(); B.OpenYmin(); B.OpenZmin();
            }
        }
    }
}
