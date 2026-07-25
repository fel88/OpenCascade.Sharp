using TKMath;

namespace TKG3d
{
    //! Describes a right-handed coordinate system in 3D space.
    //! A coordinate system is defined by:
    //! - its origin, also termed the "Location point" of the coordinate system,
    //! - three orthogonal unit vectors, termed respectively
    //! the "X Direction", "Y Direction" and "Direction" (or
    //! "main Direction") of the coordinate system.
    //! As a Geom_Axis2Placement coordinate system is
    //! right-handed, its "Direction" is always equal to the
    //! cross product of its "X Direction" and "Y Direction".
    //! The "Direction" of a coordinate system is called the
    //! "main Direction" because when this unit vector is
    //! modified, the "X Direction" and "Y Direction" are
    //! recomputed, whereas when the "X Direction" or "Y
    //! Direction" is changed, the "main Direction" is
    //! retained. The "main Direction" is also the "Z Direction".
    //! Note: Geom_Axis2Placement coordinate systems
    //! provide the same kind of "geometric" services as
    //! gp_Ax2 coordinate systems but have more complex
    //! data structures. The geometric objects provided by
    //! the Geom package use gp_Ax2 objects to include
    //! coordinate systems in their data structures, or to
    //! define the geometric transformations, which are applied to them.
    //! Geom_Axis2Placement coordinate systems are
    //! used in a context where they can be shared by
    //! several objects contained inside a common data structure.
    public class Geom_Axis2Placement : Geom_AxisPlacement
    {
        //! Returns a non transient copy of <me>.
       public   gp_Ax2 Ax2()
        {
            return new gp_Ax2(axis.Location(), axis.Direction(), vxdir);
        }


        //! Returns a transient copy of A2.
        public Geom_Axis2Placement(gp_Ax2 A2) 
        {
            vxdir = A2.XDirection();
            vydir = A2.YDirection();
            axis = A2.Axis();
        }

        gp_Dir vxdir;
        gp_Dir vydir;

    }
}
