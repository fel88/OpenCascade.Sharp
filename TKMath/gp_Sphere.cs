using OCCPort.Common;

namespace TKMath
{
    //! Describes a sphere.
    //! A sphere is defined by its radius and positioned in space
    //! with a coordinate system (a gp_Ax3 object). The origin of
    //! the coordinate system is the center of the sphere. This
    //! coordinate system is the "local coordinate system" of the sphere.
    //! Note: when a gp_Sphere sphere is converted into a
    //! Geom_SphericalSurface sphere, some implicit
    //! properties of its local coordinate system are used explicitly:
    //! -   its origin, "X Direction", "Y Direction" and "main
    //! Direction" are used directly to define the parametric
    //! directions on the sphere and the origin of the parameters,
    //! -   its implicit orientation (right-handed or left-handed)
    //! gives the orientation (direct, indirect) to the
    //! Geom_SphericalSurface sphere.
    //! See Also
    //! gce_MakeSphere which provides functions for more
    //! complex sphere constructions
    //! Geom_SphericalSurface which provides additional
    //! functions for constructing spheres and works, in
    //! particular, with the parametric equations of spheres.
    public class gp_Sphere
    {

        //! Constructs a sphere with radius theRadius, centered on the origin
        //! of theA3.  theA3 is the local coordinate system of the sphere.
        //! Warnings :
        //! It is not forbidden to create a sphere with null radius.
        //! Raises ConstructionError if theRadius < 0.0
        public gp_Sphere(gp_Ax3 theA3, double theRadius)
        {
            pos = (theA3);
            radius = (theRadius);
            Exceptions.Standard_ConstructionError_Raise_if(theRadius < 0.0, "gp_Sphere() - radius should be >= 0");
        }

        //! --- Purpose ;
        //! Returns the center of the sphere.
        public  gp_Pnt Location() { return pos.Location(); }

        public gp_Sphere(gp_Sphere gp_Sphere)
        {
            radius = gp_Sphere.radius;
            pos = gp_Sphere.pos;
        }

        public gp_Sphere Transformed(gp_Trsf theT)
        {
            gp_Sphere aC = new gp_Sphere(this);
            aC.pos.Transform(theT);
            aC.radius *= theT.ScaleFactor();
            if (aC.radius < 0)
            {
                aC.radius = -aC.radius;
            }
            return aC;
        }

        //! Returns the local coordinates system of the sphere.
        public gp_Ax3 Position() { return pos; }


        //! Returns the radius of the sphere.
        public double Radius() { return radius; }

        gp_Ax3 pos;
        double radius;
    }
}
