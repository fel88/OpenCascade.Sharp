using TKG3d;
using TKMath;

namespace TKSTEP
{
    //! The abstract class Point describes the common
    //! behavior of geometric points in 3D space.
    //! The Geom package also provides the concrete class
    //! Geom_CartesianPoint.
    public abstract class Geom_Point : Geom_Geometry
    {
        public override Geom_Geometry Copy()
        {
            throw new NotImplementedException();
        }

        //! returns a non transient copy of <me>
        public abstract gp_Pnt Pnt();


        public override void Transform(gp_Trsf t)
        {
            throw new NotImplementedException();
        }
    }
}
