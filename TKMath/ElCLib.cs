using OCCPort.Common;

namespace TKMath
{
    //! Provides functions for basic geometric computations on
    //! elementary curves such as conics and lines in 2D and 3D space.
    //! This includes:
    //! -   calculation of a point or derived vector on a 2D or
    //! 3D curve where:
    //! -   the curve is provided by the gp package, or
    //! defined in reference form (as in the gp package),
    //! and
    //! -   the point is defined by a parameter,
    //! -   evaluation of the parameter corresponding to a point
    //! on a 2D or 3D curve from gp,
    //! -   various elementary computations which allow you to
    //! position parameterized values within the period of a curve.
    //! Notes:
    //! -   ElCLib stands for Elementary Curves Library.
    //! -   If the curves provided by the gp package are not
    //! explicitly parameterized, they still have an implicit
    //! parameterization, analogous to that which they infer
    //! for the equivalent Geom or Geom2d curves.
    public class ElCLib
    {
        public static void CircleD2(double U,
                    gp_Ax2 Pos,
                    double Radius,
                  out gp_Pnt P,
                 out gp_Vec V1,
                   out gp_Vec V2)
        {
            V2 = new gp_Vec();
            V1 = new gp_Vec();
            P = new gp_Pnt();
            double Xc = Radius * Math.Cos(U);
            double Yc = Radius * Math.Sin(U);
            gp_XYZ Coord0 = new gp_XYZ();
            gp_XYZ Coord1 = new(Pos.XDirection().XYZ());
            gp_XYZ Coord2 = new(Pos.YDirection().XYZ());
            //Point courant :
            Coord0.SetLinearForm(Xc, Coord1, Yc, Coord2, Pos.Location().XYZ());
            P.SetXYZ(Coord0);
            //D1 :
            Coord0.SetLinearForm(-Yc, Coord1, Xc, Coord2);
            V1.SetXYZ(Coord0);
            //D2 :
            Coord0.SetLinearForm(-Xc, Coord1, -Yc, Coord2);
            V2.SetXYZ(Coord0);
        }

        public static void CircleD1(double U,

                   gp_Ax22d Pos,
                   double Radius,
                out  gp_Pnt2d P,
                out gp_Vec2d V1)
        {
            V1=new gp_Vec2d ();
            P = new gp_Pnt2d();

            gp_XY Vxy = new gp_XY();
            gp_XY Xdir=new (Pos.XDirection().XY());
            gp_XY Ydir=new(Pos.YDirection().XY());
            double  Xc = Radius * Math.Cos(U);
            double Yc = Radius * Math.Sin(U);
            //Point courant :
            Vxy.SetLinearForm(Xc, Xdir, Yc, Ydir, Pos.Location().XY());
            P.SetXY(Vxy);
            //V1 :
            Vxy.SetLinearForm(-Yc, Xdir, Xc, Ydir);
            V1.SetXY(Vxy);
        }

        public static gp_Pnt2d Value(double U, gp_Lin2d L)
        {
            return ElCLib.LineValue(U, L.Position());
        }

        public static void D2(
 double U, gp_Circ C, gp_Pnt P, gp_Vec V1, gp_Vec V2)
        {

            ElCLib.CircleD2(U, C.Position(), C.Radius(), out P, out V1, out V2);
        }

        public static void D1(double U, gp_Lin L, ref gp_Pnt P, ref gp_Vec V1)
        {
            ElCLib.LineD1(U, L.Position(), ref P, ref V1);
        }

        public static double Parameter(gp_Lin2d L, gp_Pnt2d P)
        {
            return ElCLib.LineParameter(L.Position(), P);
        }

        public static gp_Pnt CircleValue(double U,
                    gp_Ax2 Pos,
                    double Radius)
        {
            gp_XYZ XDir = Pos.XDirection().XYZ();
            gp_XYZ YDir = Pos.YDirection().XYZ();
            gp_XYZ PLoc = Pos.Location().XYZ();
            double A1 = Radius * Math.Cos(U);
            double A2 = Radius * Math.Sin(U);
            return new gp_Pnt(A1 * XDir.X() + A2 * YDir.X() + PLoc.X(),
                  A1 * XDir.Y() + A2 * YDir.Y() + PLoc.Y(),
                  A1 * XDir.Z() + A2 * YDir.Z() + PLoc.Z());
        }

        public static gp_Pnt2d CircleValue(double U,
                    gp_Ax22d Pos,
                    double Radius)
        {
            gp_XY XDir = Pos.XDirection().XY();
            gp_XY YDir = Pos.YDirection().XY();
            gp_XY PLoc = Pos.Location().XY();
            double A1 = Radius * Math.Cos(U);
            double A2 = Radius * Math.Sin(U);
            return new gp_Pnt2d(A1 * XDir.X() + A2 * YDir.X() + PLoc.X(),
                    A1 * XDir.Y() + A2 * YDir.Y() + PLoc.Y());
        }

        public static double LineParameter(gp_Ax2d L, gp_Pnt2d P)
        {
            gp_XY Coord = P.XY();
            Coord.Subtract(L.Location().XY());
            return Coord.Dot(L.Direction().XY());
        }

        public static double Parameter(gp_Lin L, gp_Pnt P)
        {
            return LineParameter(L.Position(), P);
        }

        //! Adjust U1 and  U2 in the  parametric range  UFirst
        //! Ulast of a periodic curve, where ULast -
        //! UFirst is its period. To do this, this function:
        //! -   sets U1 in the range [ UFirst, ULast ] by
        //! adding/removing the period to/from the value U1, then
        //! -   sets U2 in the range [ U1, U1 + period ] by
        //! adding/removing the period to/from the value U2.
        //! Precision is used to test the equalities.
        public static void AdjustPeriodic(double UFirst,
            double ULast, double Preci, ref double U1, ref double U2)
        {
            if (Precision.IsInfinite(UFirst) ||
     Precision.IsInfinite(ULast))
            {
                U1 = UFirst;
                U2 = ULast;
                return;
            }

            double period = ULast - UFirst;

            //if (period < Epsilon(ULast)) //todo: fix here to origin
            if (Math.Abs(period) < double.Epsilon) //todo: fix here to origin
            {
                // In order to avoid FLT_Overflow exception
                // (test bugs moddata_1 bug22757)
                U1 = UFirst;
                U2 = ULast;
                return;
            }

            U1 -= Math.Floor((U1 - UFirst) / period) * period;
            if (ULast - U1 < Preci) U1 -= period;
            U2 -= Math.Floor((U2 - U1) / period) * period;
            if (U2 - U1 < Preci) U2 += period;
        }
        //-------------------------------------------------------------------
        // Epsilon : The function returns absolute value of difference
        //           between 'Value' and other nearest value of
        //           Standard_Real type.
        //           Nearest value is chosen in direction of infinity
        //           the same sign as 'Value'.
        //           If 'Value' is 0 then returns minimal positive value
        //           of Standard_Real type.
        //-------------------------------------------------------------------
        public static double Epsilon(double Value)
        {
            double aEpsilon;

            if (Value >= 0.0)
            {
                aEpsilon = NextAfter(Value, Standard_Real.RealLast()) - Value;
            }
            else
            {
                aEpsilon = Value - NextAfter(Value, Standard_Real.RealFirst());
            }
            return aEpsilon;
        }

        private static double NextAfter(double value, double value2)
        {
            return Math.BitIncrement(value);
        }

        public static gp_Pnt LineValue(double U,
              gp_Ax1 Pos)
        {
            gp_XYZ ZDir = Pos.Direction().XYZ();
            gp_XYZ PLoc = Pos.Location().XYZ();
            return new gp_Pnt(U * ZDir.X() + PLoc.X(),
                  U * ZDir.Y() + PLoc.Y(),
                  U * ZDir.Z() + PLoc.Z());
        }

        public static double LineParameter(gp_Ax1 L, gp_Pnt P)
        {
            return (P.XYZ() - L.Location().XYZ()).Dot(L.Direction().XYZ());
        }

        public static gp_Pnt2d LineValue(double U, gp_Ax2d Pos)
        {
            gp_XY ZDir = Pos.Direction().XY();
            gp_XY PLoc = Pos.Location().XY();
            return new gp_Pnt2d(U * ZDir.X() + PLoc.X(),
                    U * ZDir.Y() + PLoc.Y());

        }



        public static void LineD1(double U,
              gp_Ax1 Pos,
           ref gp_Pnt P,
           ref gp_Vec V1)
        {
            gp_XYZ Coord = Pos.Direction().XYZ();
            V1.SetXYZ(Coord);
            Coord.SetLinearForm(U, Coord, Pos.Location().XYZ());
            P.SetXYZ(Coord);
        }


        public static void LineD1(double U, gp_Ax2d Pos, ref gp_Pnt2d P, ref gp_Vec2d V1)
        {
            gp_XY Coord = Pos.Direction().XY();
            V1.SetXY(Coord);
            Coord.SetLinearForm(U, Coord, Pos.Location().XY());
            P.SetXY(Coord);
        }

        public static gp_Pnt Value(double U, gp_Lin L)
        {
            return ElCLib.LineValue(U, L.Position());
        }

        public static gp_Pnt Value(double U, gp_Circ C)
        {
            return ElCLib.CircleValue(U, C.Position(), C.Radius());
        }


        //=======================================================================
        //function : InPeriod
        //purpose  : Value theULast is never returned.
        //          Example of some case (checked on WIN64 platform)
        //          with some surface having period 2*PI = 6.2831853071795862.
        //            Let theUFirst be equal to 6.1645624650899675. Then,
        //          theULast must be equal to
        //              6.1645624650899675+6.2831853071795862=12.4477477722695537.
        //
        //          However, real result is 12.447747772269555.
        //          Therefore, new period value to adjust will be equal to
        //              12.447747772269555-6.1645624650899675=6.2831853071795871.
        //
        //          As we can see, (6.2831853071795871 != 6.2831853071795862).
        //
        //          According to above said, this method should be used carefully.
        //          In order to increase reliability of this method, input arguments
        //          needs to be replaced with following: 
        //            (theU, theUFirst, thePeriod). theULast parameter is excess.
        //=======================================================================
        public static double InPeriod(double theU, double theUFirst, double theULast)
        {
            if (Precision.IsInfinite(theU) ||
      Precision.IsInfinite(theUFirst) ||
      Precision.IsInfinite(theULast))
            {//In order to avoid FLT_Overflow exception
                return theU;
            }

            double aPeriod = theULast - theUFirst;

            if (aPeriod < Epsilon(theULast))
                return theU;

            return Math.Max(theUFirst, theU + aPeriod * Math.Ceiling((theUFirst - theU) / aPeriod));
        }
    }
}