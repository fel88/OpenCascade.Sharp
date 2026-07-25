using TKMath;

namespace TKG3d
{
    //! The abstract class Vector describes the common
    //! behavior of vectors in 3D space.
    //! The Geom package provides two concrete classes of
    //! vectors: Geom_Direction (unit vector) and Geom_VectorWithMagnitude.
    public class Geom_Vector : Geom_Geometry
    {
        public override Geom_Geometry Copy()
        {
            throw new NotImplementedException();
        }

        //! Converts this vector into a gp_Vec vector.
        public   gp_Vec Vec()
        {
            return gpVec;
        }

       protected gp_Vec gpVec;

        public override void Transform(gp_Trsf t)
        {
            throw new NotImplementedException();
        }
    }
}
