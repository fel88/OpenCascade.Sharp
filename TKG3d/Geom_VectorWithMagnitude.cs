using TKMath;

namespace TKG3d
{
    //! Defines a vector with magnitude.
    //! A vector with magnitude can have a zero length.
    public class Geom_VectorWithMagnitude : Geom_Vector
    {
        //! Creates a transient copy of V.
        public Geom_VectorWithMagnitude(gp_Vec V) {
            gpVec = V;
        }
  
    }
}
