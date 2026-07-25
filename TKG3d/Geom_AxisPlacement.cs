using TKMath;

namespace TKG3d
{
    //! The abstract class AxisPlacement describes the
    //! common behavior of positioning systems in 3D space,
    //! such as axis or coordinate systems.
    //! The Geom package provides two implementations of
    //! 3D positioning systems:
    //! - the axis (Geom_Axis1Placement class), which is defined by:
    //! - its origin, also termed the "Location point" of the  axis,
    //! - its unit vector, termed the "Direction" or "main
    //! Direction" of the axis;
    //! - the right-handed coordinate system
    //! (Geom_Axis2Placement class), which is defined by:
    //! - its origin, also termed the "Location point" of the coordinate system,
    //! - three orthogonal unit vectors, termed
    //! respectively the "X Direction", the "Y Direction"
    //! and the "Direction" of the coordinate system. As
    //! the coordinate system is right-handed, these
    //! unit vectors have the following relation:
    //! "Direction" = "X Direction" ^
    //! "Y Direction". The "Direction" is also
    //! called the "main Direction" because, when the
    //! unit vector is modified, the "X Direction" and "Y
    //! Direction" are recomputed, whereas when the "X
    //! Direction" or "Y Direction" is modified, the "main Direction" does not change.
    //! The axis whose origin is the origin of the positioning
    //! system and whose unit vector is its "main Direction" is
    //! also called the "Axis" or "main Axis" of the positioning system.
    public class Geom_AxisPlacement : Geom_Geometry
    {
        public override Geom_Geometry Copy()
        {
            throw new NotImplementedException();
        }

        protected gp_Ax1 axis;
        public override void Transform(gp_Trsf t)
        {
            throw new NotImplementedException();
        }
    }
}
