using OCCPort.Common;
using System;

namespace TKMath
{
    //! Describes a line in 2D space.
    //! A line is positioned in the plane with an axis (a gp_Ax2d
    //! object) which gives the line its origin and unit vector. A
    //! line and an axis are similar objects, thus, we can convert
    //! one into the other.
    //! A line provides direct access to the majority of the edit
    //! and query functions available on its positioning axis. In
    //! addition, however, a line has specific functions for
    //! computing distances and positions.
    //! See Also
    //! GccAna and Geom2dGcc packages which provide
    //! functions for constructing lines defined by geometric
    //! constraints
    //! gce_MakeLin2d which provides functions for more
    //! complex line constructions
    //! Geom2d_Line which provides additional functions for
    //! constructing lines and works, in particular, with the
    //! parametric equations of lines
    public class gp_Lin2d
    {
        //! Creates a line located with theA.
        public gp_Lin2d(gp_Ax2d theA)

        { pos = theA; }

        //! Returns the axis placement one axis with the same
        //! location and direction as <me>.
        public gp_Ax2d Position() { return pos; }


        //! Returns the normalized coefficients of the line :
        //! theA * X + theB * Y + theC = 0.
        public void Coefficients(out double theA, out double theB, out double theC)
        {
            theA = pos.Direction().Y();
            theB = -pos.Direction().X();
            theC = -(theA * pos.Location().X() + theB * pos.Location().Y());
        }

        //! Returns the direction of the line.
        public gp_Dir2d Direction() { return pos.Direction(); }

        //! Returns the location point (origin) of the line.
        public gp_Pnt2d Location() { return pos.Location(); }

        //! Computes the distance between <me> and the point <theP>.
        public double Distance(gp_Pnt2d theP)
        {
            gp_XY aCoord = theP.XY();
            aCoord.Subtract(pos.Location().XY());
            double aVal = aCoord.Crossed(pos.Direction().XY());
            if (aVal < 0)
            {
                aVal = -aVal;
            }
            return aVal;
        }

        gp_Ax2d pos;

        //! <theP> is the location point (origin) of the line and
        //! <theV> is the direction of the line.
        public gp_Lin2d(gp_Pnt2d theP, gp_Dir2d theV)
        {
            pos = new gp_Ax2d(theP, theV);
        }
    }
}