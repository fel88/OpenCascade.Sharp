using OCCPort.Common;

namespace TKMath
{
    //! Provides functions for basic geometric computation on
    //! elementary surfaces.
    //! This includes:
    //! -   calculation of a point or derived vector on a surface
    //! where the surface is provided by the gp package, or
    //! defined in canonical form (as in the gp package), and
    //! the point is defined with a parameter,
    //! -   evaluation of the parameters corresponding to a
    //! point on an elementary surface from gp,
    //! -   calculation of isoparametric curves on an elementary
    //! surface defined in canonical form (as in the gp package).
    //! Notes:
    //! -   ElSLib stands for Elementary Surfaces Library.
    //! -   If the surfaces provided by the gp package are not
    //! explicitly parameterized, they still have an implicit
    //! parameterization, similar to that which they infer on
    //! the equivalent Geom surfaces.
    //! Note: ElSLib stands for Elementary Surfaces Library.
    public class ElSLib
    {
        public static void SphereD2(double U,

                   double V,

                   gp_Ax3 Pos,
                   double Radius,
              out gp_Pnt P,
              out gp_Vec Vu,
              out gp_Vec Vv,
              out gp_Vec Vuu,
               out gp_Vec Vvv,
              out gp_Vec Vuv)
        {
            // Vxy = CosU * XDirection + SinU * YDirection
            // DVxy = -SinU * XDirection + CosU * YDirection

            // P(U,V) = Location +  R * CosV * Vxy  +   R * SinV * Direction

            // Vu = R * CosV * DVxy

            // Vuu = - R * CosV * Vxy

            // Vv = -R * SinV * Vxy + R * CosV * Direction

            // Vvv = -R * CosV * Vxy - R * SinV * Direction

            // Vuv = - R * SinV * DVxy

            P = new gp_Pnt();
            Vu = new gp_Vec();
            Vvv = new gp_Vec();
            Vv = new gp_Vec();
            Vuu = new gp_Vec();
            Vuv = new gp_Vec();

            gp_XYZ XDir = Pos.XDirection().XYZ();
            gp_XYZ YDir = Pos.YDirection().XYZ();
            gp_XYZ ZDir = Pos.Direction().XYZ();
            gp_XYZ PLoc = Pos.Location().XYZ();
            double CosU = Math.Cos(U);
            double SinU = Math.Sin(U);
            double R1 = Radius * Math.Cos(V);
            double R2 = Radius * Math.Sin(V);
            double A1 = R1 * CosU;
            double A2 = R1 * SinU;
            double A3 = R2 * CosU;
            double A4 = R2 * SinU;
            double Som1X = A1 * XDir.X() + A2 * YDir.X();
            double Som1Y = A1 * XDir.Y() + A2 * YDir.Y();
            double Som1Z = A1 * XDir.Z() + A2 * YDir.Z();
            double R2ZX = R2 * ZDir.X();
            double R2ZY = R2 * ZDir.Y();
            double R2ZZ = R2 * ZDir.Z();
            P.SetX(Som1X + R2ZX + PLoc.X());
            P.SetY(Som1Y + R2ZY + PLoc.Y());
            P.SetZ(Som1Z + R2ZZ + PLoc.Z());
            Vu.SetX(-A2 * XDir.X() + A1 * YDir.X());
            Vu.SetY(-A2 * XDir.Y() + A1 * YDir.Y());
            Vu.SetZ(-A2 * XDir.Z() + A1 * YDir.Z());
            Vv.SetX(-A3 * XDir.X() - A4 * YDir.X() + R1 * ZDir.X());
            Vv.SetY(-A3 * XDir.Y() - A4 * YDir.Y() + R1 * ZDir.Y());
            Vv.SetZ(-A3 * XDir.Z() - A4 * YDir.Z() + R1 * ZDir.Z());
            Vuu.SetX(-Som1X);
            Vuu.SetY(-Som1Y);
            Vuu.SetZ(-Som1Z);
            Vvv.SetX(-Som1X - R2ZX);
            Vvv.SetY(-Som1Y - R2ZY);
            Vvv.SetZ(-Som1Z - R2ZZ);
            Vuv.SetX(A4 * XDir.X() - A3 * YDir.X());
            Vuv.SetY(A4 * XDir.Y() - A3 * YDir.Y());
            Vuv.SetZ(A4 * XDir.Z() - A3 * YDir.Z());
        }
        public static gp_Pnt Value(double U, double V, gp_Pln Pl)
        {
            return ElSLib.PlaneValue(U, V, Pl.Position());
        }

        public static gp_Circ SphereUIso(gp_Ax3 Pos,
                     double Radius,
                     double U)
        {
            gp_Vec dx = Pos.XDirection();
            gp_Vec dy = Pos.YDirection();
            gp_Dir dz = Pos.Direction();
            gp_Dir cx = Math.Cos(U) * dx + Math.Sin(U) * dy;
            gp_Ax2 axes = new(Pos.Location(),
                    cx.Crossed(dz),
                    cx);
            gp_Circ Circ = new(axes, Radius);
            return Circ;
        }

        public static gp_Circ SphereVIso(gp_Ax3 Pos,
                   double Radius,
                   double V)
        {
            gp_Ax2 axes = Pos.Ax2();
            gp_Vec Ve = new(Pos.Direction());
            Ve.Multiply(Radius * Math.Sin(V));
            axes.Translate(Ve);
            double radius = Radius * Math.Cos(V);
            // #23170: if V is even slightly (e.g. by double epsilon) greater than PI/2,
            // radius will become negative and constructor of gp_Circ will raise exception.
            // Lets try to create correct isoline even on analytical continuation for |V| > PI/2...
            if (radius < 0.0)
            {
                axes.SetDirection(-axes.Direction());
                radius = -radius;
            }
            gp_Circ Circ = new(axes, radius);
            return Circ;
        }
        public static void SphereD0(double U,
                          double V,
                          gp_Ax3 Pos,
                          double Radius, ref gp_Pnt P)
        {
            gp_XYZ XDir = Pos.XDirection().XYZ();
            gp_XYZ YDir = Pos.YDirection().XYZ();
            gp_XYZ ZDir = Pos.Direction().XYZ();
            gp_XYZ PLoc = Pos.Location().XYZ();
            double R = Radius * Math.Cos(V);
            double A3 = Radius * Math.Sin(V);
            double A1 = R * Math.Cos(U);
            double A2 = R * Math.Sin(U);
            P.SetX(A1 * XDir.X() + A2 * YDir.X() + A3 * ZDir.X() + PLoc.X());
            P.SetY(A1 * XDir.Y() + A2 * YDir.Y() + A3 * ZDir.Y() + PLoc.Y());
            P.SetZ(A1 * XDir.Z() + A2 * YDir.Z() + A3 * ZDir.Z() + PLoc.Z());
        }



        static double PIPI = Math.PI + Math.PI;
        public static void CylinderParameters(gp_Ax3 Pos,
                   double dd,
                   gp_Pnt P,
                  double U,
                  double V)
        {
            gp_Trsf T = new gp_Trsf();
            T.SetTransformation(Pos);
            gp_Pnt Ploc = P.Transformed(T);
            U = Math.Atan2(Ploc.Y(), Ploc.X());
            if (U < -1e-16) U += PIPI;
            else if (U < 0) U = 0;
            V = Ploc.Z();
        }

        const double M_PI_2 = Math.PI / 2.0;

        public static void Parameters(gp_Sphere S,
                    gp_Pnt P,
                   ref double U,
                   ref double V)
        {
            ElSLib.SphereParameters(S.Position(), S.Radius(), P, out U, out V);
        }

        //! parametrization
        //! P (U, V) = Location +
        //! Radius * Cos (V) * (Cos (U) * XDirection + Sin (U) * YDirection) +
        //! Radius * Sin (V) * ZDirection
        public static void SphereParameters(gp_Ax3 Pos,
                     double dd,

                     gp_Pnt P,
                    out double U,
                     out double V)
        {
            gp_Trsf T = new gp_Trsf();
            T.SetTransformation(Pos);
            gp_Pnt Ploc = P.Transformed(T);
            double x, y, z;
            Ploc.Coord(out x, out y, out z);
            double l = Math.Sqrt(x * x + y * y);
            if (l < gp.Resolution())
            {    // point on axis Z of the sphere
                if (z > 0.0)
                    V = M_PI_2; // PI * 0.5
                else
                    V = -M_PI_2; // PI * 0.5
                U = 0.0;
            }
            else
            {
                V = Math.Atan(z / l);
                U = Math.Atan2(y, x);
                if (U < -1e-16) U += PIPI;
                else if (U < 0) U = 0;
            }
        }

        public static void Parameters(gp_Pln Pl,
                    gp_Pnt P,
                   ref double U,
                 ref double V)
        {
            PlaneParameters(Pl.Position(), P, ref U, ref V);
        }

        //! parametrization
        //! P (U, V) = Location + V * ZDirection +
        //! Radius * (Cos(U) * XDirection + Sin (U) * YDirection)
        public static void Parameters(gp_Cylinder C, gp_Pnt P, ref double U, ref double V)
        {
            ElSLib.CylinderParameters(C.Position(), C.Radius(), P, U, V);
        }

        public static void PlaneD1(double U, double V, gp_Ax3 Pos, ref gp_Pnt P, ref gp_Vec Vu, ref gp_Vec Vv)
        {
            gp_XYZ XDir = Pos.XDirection().XYZ();
            gp_XYZ YDir = Pos.YDirection().XYZ();
            gp_XYZ PLoc = Pos.Location().XYZ();
            P.SetX(U * XDir.X() + V * YDir.X() + PLoc.X());
            P.SetY(U * XDir.Y() + V * YDir.Y() + PLoc.Y());
            P.SetZ(U * XDir.Z() + V * YDir.Z() + PLoc.Z());
            Vu.SetX(XDir.X());
            Vu.SetY(XDir.Y());
            Vu.SetZ(XDir.Z());
            Vv.SetX(YDir.X());
            Vv.SetY(YDir.Y());
            Vv.SetZ(YDir.Z());
        }


        public static gp_Lin PlaneUIso(gp_Ax3 Pos,
              double U)
        {
            gp_Lin L = new gp_Lin(Pos.Location(), Pos.YDirection());
            gp_Vec Ve = new gp_Vec(Pos.XDirection());
            Ve *= U;
            L.Translate(Ve);
            return L;
        }

        public static gp_Pnt PlaneValue(double U, double V, gp_Ax3 Pos)
        {
            gp_XYZ XDir = Pos.XDirection().XYZ();
            gp_XYZ YDir = Pos.YDirection().XYZ();
            gp_XYZ PLoc = Pos.Location().XYZ();
            return new gp_Pnt(U * XDir.X() + V * YDir.X() + PLoc.X(),
                  U * XDir.Y() + V * YDir.Y() + PLoc.Y(),
                  U * XDir.Z() + V * YDir.Z() + PLoc.Z());
        }

        //=======================================================================

        public static void PlaneParameters(gp_Ax3 Pos,
                   gp_Pnt P,
                  ref double U,
                 ref double V)
        {
            gp_Trsf T = new gp_Trsf();
            T.SetTransformation(Pos);
            gp_Pnt Ploc = P.Transformed(T);
            U = Ploc.X();
            V = Ploc.Y();
        }

        public static gp_Lin PlaneVIso(gp_Ax3 Pos, double V)
        {
            gp_Lin L = new gp_Lin(Pos.Location(), Pos.XDirection());
            gp_Vec Ve = new gp_Vec(Pos.YDirection());
            Ve *= V;
            L.Translate(Ve);
            return L;
        }
    }
}
