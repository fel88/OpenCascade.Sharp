using OCCPort.Common;
using TKMath;

namespace TKSTEP
{
    //! Describes a point in 3D space. A
    //! Geom_CartesianPoint is defined by a gp_Pnt point,
    //! with its three Cartesian coordinates X, Y and Z.
    public class Geom_CartesianPoint : Geom_Point
    {


        //! Constructs a point defined by its three Cartesian coordinates X, Y and Z.
        public Geom_CartesianPoint(double X, double Y, double Z)
        {
            gpPnt = new gp_Pnt(X, Y, Z);
        }


        //! Returns a non transient cartesian point with
        //! the same coordinates as <me>.
        public override gp_Pnt Pnt()
        {
            return gpPnt;
        }

        gp_Pnt gpPnt;

    }
}
