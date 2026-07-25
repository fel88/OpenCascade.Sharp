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
