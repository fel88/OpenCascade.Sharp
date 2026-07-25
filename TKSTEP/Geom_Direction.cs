using OCCPort.Common;
using TKG3d;
using TKMath;

namespace TKSTEP
{
    //! The class Direction specifies a vector that is never null.
    //! It is a unit vector.
    public class Geom_Direction : Geom_Vector
    {
        //! Creates a unit vector with it 3 cartesian coordinates.
        //!
        //! Raised if Sqrt( X*X + Y*Y + Z*Z) <= Resolution from gp.
        public Geom_Direction(double X, double Y, double Z)
        {
            double D = Math.Sqrt(X * X + Y * Y + Z * Z);
            Exceptions.Standard_ConstructionError_Raise_if(D <= gp.Resolution(),
                                                    "Geom_Direction() - input vector has zero length");
            gpVec = new gp_Vec(X / D, Y / D, Z / D);
        }

        //! Returns the non transient direction with the same
        //! coordinates as <me>.
        public  gp_Dir Dir()
        {
            return gpVec;
        }

    }
}
