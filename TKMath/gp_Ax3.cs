namespace TKMath
{
    //! Describes a coordinate system in 3D space. Unlike a
    //! gp_Ax2 coordinate system, a gp_Ax3 can be
    //! right-handed ("direct sense") or left-handed ("indirect sense").
    //! A coordinate system is defined by:
    //! -   its origin (also referred to as its "Location point"), and
    //! -   three orthogonal unit vectors, termed the "X
    //! Direction", the "Y Direction" and the "Direction" (also
    //! referred to as the "main Direction").
    //! The "Direction" of the coordinate system is called its
    //! "main Direction" because whenever this unit vector is
    //! modified, the "X Direction" and the "Y Direction" are
    //! recomputed. However, when we modify either the "X
    //! Direction" or the "Y Direction", "Direction" is not modified.
    //! "Direction" is also the "Z Direction".
    //! The "main Direction" is always parallel to the cross
    //! product of its "X Direction" and "Y Direction".
    //! If the coordinate system is right-handed, it satisfies the equation:
    //! "main Direction" = "X Direction" ^ "Y Direction"
    //! and if it is left-handed, it satisfies the equation:
    //! "main Direction" = -"X Direction" ^ "Y Direction"
    //! A coordinate system is used:
    //! -   to describe geometric entities, in particular to position
    //! them. The local coordinate system of a geometric
    //! entity serves the same purpose as the STEP function
    //! "axis placement three axes", or
    //! -   to define geometric transformations.
    //! Note:
    //! -   We refer to the "X Axis", "Y Axis" and "Z Axis",
    //! respectively, as the axes having:
    //! -   the origin of the coordinate system as their origin, and
    //! -   the unit vectors "X Direction", "Y Direction" and
    //! "main Direction", respectively, as their unit vectors.
    //! -   The "Z Axis" is also the "main Axis".
    //! -   gp_Ax2 is used to define a coordinate system that must be always right-handed.
    public struct gp_Ax3
    {
        public gp_Ax3(gp_Ax2 theA)
        {
            axis = (theA.Axis());
            vydir = (theA.YDirection());
            vxdir = theA.XDirection();
        }

        //! Returns  True if  the  coordinate  system is right-handed. i.e.
        //! XDirection().Crossed(YDirection()).Dot(Direction()) > 0
        public bool Direct() { return (vxdir.Crossed(vydir).Dot(axis.Direction()) > 0.0); }

        public gp_Ax2 Ax2()
        {
            gp_Dir aZz = axis.Direction();
            if (!Direct())
            {
                aZz.Reverse();
            }
            return new gp_Ax2(axis.Location(), aZz, vxdir);
        }
        public void Transform(gp_Trsf theT)
        {
            axis.Transform(theT);
            vxdir.Transform(theT);
            vydir.Transform(theT);
        }

        //! Returns the main axis of <me>. It is the "Location" point
        //! and the main "Direction".
        public gp_Ax1 Axis() { return axis; }

        //! Creates an object corresponding to the reference
        //! coordinate system (OXYZ).
        /*public gp_Ax3()
        {
            vydir = new gp_Dir(0.0, 1.0, 0.0);
            // vxdir(1.,0.,0.) use default ctor of gp_Dir, as it creates the same dir(1,0,0)
        }*/



        //! Creates a  right handed axis placement with the
        //! "Location" point theP and  two directions, theN gives the
        //! "Direction" and theVx gives the "XDirection".
        //! Raises ConstructionError if theN and theVx are parallel (same or opposite orientation).
        public gp_Ax3(gp_Pnt theP, gp_Dir theN, gp_Dir theVx)
        {
            axis = new gp_Ax1(theP, theN);
            vydir = (theN);
            vxdir = (theN);

            vxdir.CrossCross(theVx, theN);
            vydir.Cross(vxdir);
        }

        public gp_Dir XDirection()
        {
            return vxdir;
        }

        gp_Ax1 axis;
        gp_Dir vydir;
        gp_Dir vxdir;

        public gp_Dir YDirection()
        {
            return vydir;
        }

        //! Returns the main direction of <me>.
        public gp_Dir Direction() { return axis.Direction(); }


        //! Returns the "Location" point (origin) of <me>.
        public gp_Pnt Location() { return axis.Location(); }

    }
}
