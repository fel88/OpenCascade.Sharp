using OCCPort.Common;
using System;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TKMath
{
    //! Defines  a non-persistent 2D cartesian point.
    public struct gp_Pnt2d
    {
        public override string ToString()
        {
            return $"gp_Pnt2d  X: {coord.X()} Y: {coord.Y()} ";
        }

        public gp_XY coord;
        //=======================================================================
        public double SquareDistance(gp_Pnt2d theOther)
        {
            gp_XY aXY = theOther.coord;
            double aX = coord.X() - aXY.X();
            double aY = coord.Y() - aXY.Y();
            return aX * aX + aY * aY;
        }


        //! Assigns the given value to the X  coordinate of this point.
        public void SetX(double theX) { coord.SetX(theX); }

        //! For this point returns its two coordinates as a number pair.
        public void Coord(out double theXp, out double theYp) { coord.Coord(out theXp, out theYp); }

        //! For this point, returns its two coordinates as a number pair.
        public gp_XY Coord() { return coord; }

        //! For this point, assigns the values theXp and theYp to its two coordinates
        public void SetCoord(double theXp, double theYp) { coord.SetCoord(theXp, theYp); }

        public double X()
           => coord.X();
        public double Y()
           => coord.Y();
        //! For this point, returns its two coordinates as a number pair.
        public gp_XY XY() { return coord; }

        //! Creates a  point with its 2 cartesian's coordinates : theXp, theYp.
        public gp_Pnt2d(double theXp, double theYp)

        {
            coord = new gp_XY(theXp, theYp);

        }
        //! Assigns the two coordinates of Coord to this point.
        public void SetXY(gp_XY theCoord) { coord = theCoord; }
        //! Returns the coordinates of this point.
        //! Note: This syntax allows direct modification of the returned value.
        internal gp_XY ChangeCoord()
        {

            return coord;

        }

        //! Comparison
        //! Returns True if the distance between the two
        //! points is lower or equal to theLinearTolerance.
        public bool IsEqual(gp_Pnt2d theOther, double theLinearTolerance)
        {
            return Distance(theOther) <= theLinearTolerance;
        }

        public double Distance(gp_Pnt2d theOther)
        {
            gp_XY aXY = theOther.coord;
            double aX = coord.X() - aXY.X();
            double aY = coord.Y() - aXY.Y();
            return Math.Sqrt(aX * aX + aY * aY);
        }

        public gp_Pnt2d(gp_XY gp_XY) : this()
        {
            coord = new gp_XY(gp_XY.X(), gp_XY.Y());
        }
    }
}