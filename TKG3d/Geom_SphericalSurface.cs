using OCCPort.Common;
using TKMath;

namespace TKG3d
{
    //! Describes a sphere.
    //! A sphere is defined by its radius, and is positioned in
    //! space by a coordinate system (a gp_Ax3 object), the
    //! origin of which is the center of the sphere.
    //! This coordinate system is the "local coordinate
    //! system" of the sphere. The following apply:
    //! - Rotation around its "main Axis", in the trigonometric
    //! sense given by the "X Direction" and the "Y
    //! Direction", defines the u parametric direction.
    //! - Its "X Axis" gives the origin for the u parameter.
    //! - The "reference meridian" of the sphere is a
    //! half-circle, of radius equal to the radius of the
    //! sphere. It is located in the plane defined by the
    //! origin, "X Direction" and "main Direction", centered
    //! on the origin, and positioned on the positive side of the "X Axis".
    //! - Rotation around the "Y Axis" gives the v parameter
    //! on the reference meridian.
    //! - The "X Axis" gives the origin of the v parameter on
    //! the reference meridian.
    //! - The v parametric direction is oriented by the "main
    //! Direction", i.e. when v increases, the Z coordinate
    //! increases. (This implies that the "Y Direction"
    //! orients the reference meridian only when the local
    //! coordinate system is indirect.)
    //! - The u isoparametric curve is a half-circle obtained
    //! by rotating the reference meridian of the sphere
    //! through an angle u around the "main Axis", in the
    //! trigonometric sense defined by the "X Direction"
    //! and the "Y Direction".
    //! The parametric equation of the sphere is:
    //! P(u,v) = O + R*cos(v)*(cos(u)*XDir + sin(u)*YDir)+R*sin(v)*ZDir
    //! where:
    //! - O, XDir, YDir and ZDir are respectively the
    //! origin, the "X Direction", the "Y Direction" and the "Z
    //! Direction" of its local coordinate system, and
    //! - R is the radius of the sphere.
    //! The parametric range of the two parameters is:
    //! - [ 0, 2.*Pi ] for u, and
    //! - [ - Pi/2., + Pi/2. ] for v.
    public class Geom_SphericalSurface : Geom_ElementarySurface
    {


        //! A3 is the local coordinate system of the surface.
        //! At the creation the parametrization of the surface is defined
        //! such as the normal Vector (N = D1U ^ D1V) is directed away from
        //! the center of the sphere.
        //! The direction of increasing parametric value V is defined by the
        //! rotation around the "YDirection" of A2 in the trigonometric sense
        //! and the orientation of increasing parametric value U is defined
        //! by the rotation around the main direction of A2 in the
        //! trigonometric sense.
        //! Warnings :
        //! It is not forbidden to create a spherical surface with
        //! Radius = 0.0
        //! Raised if Radius < 0.0.
        public Geom_SphericalSurface(gp_Ax3 A, double R)
        {
            radius = (R);

            if (R < 0.0) throw new Standard_ConstructionError();
            pos = A;
        }

        double radius;
        public gp_Sphere Sphere()
        {
            return new gp_Sphere(pos, radius);
        }

        public override void Bounds(out double U1, out double U2, out double V1, out double V2)
        {
            U1 = 0.0;
            U2 = Math.PI * 2.0;
            V1 = -Math.PI / 2.0;
            V2 = Math.PI / 2.0;
        }

        public override Geom_Geometry Copy()
        {
            throw new NotImplementedException();
        }

        public override void D0(double U, double V, ref gp_Pnt P)
        {
            ElSLib.SphereD0(U, V, pos, radius, ref P);
        }

        //! Computes the coefficients of the implicit equation of
        //! this quadric in the absolute Cartesian coordinate system:
        //! A1.X**2 + A2.Y**2 + A3.Z**2 + 2.(B1.X.Y + B2.X.Z + B3.Y.Z) +
        //! 2.(C1.X + C2.Y + C3.Z) + D = 0.0
        //! An implicit normalization is applied (i.e. A1 = A2 = 1.
        //! in the local coordinate system of this sphere).
        public double Radius()
        {
            return radius;
        }

        public override void D1(double U, double V, out gp_Pnt P, out gp_Vec D1U, out gp_Vec D1V)
        {
            ElSLib.SphereD1(U, V, pos, radius, out P, out D1U, out D1V);
        }

        public override void D2(double U, double V, out gp_Pnt P, out gp_Vec D1U, out gp_Vec D1V, out gp_Vec D2U, out gp_Vec D2V, out gp_Vec D2UV)
        {
            ElSLib.SphereD2(U, V, pos, radius, out P, out D1U, out D1V, out D2U, out D2V, out D2UV);
        }

        public override bool IsUClosed()
        {
            return true;
        }

        public override bool IsUPeriodic()
        {
            return true;
        }

        public override bool IsVClosed()
        {
            return false;
        }

        public override bool IsVPeriodic()
        {
            return false;
        }

        public override void Transform(gp_Trsf t)
        {
            throw new NotImplementedException();
        }

        public override Geom_Curve UIso(double U)
        {
            throw new NotImplementedException();
        }

        public override Geom_Curve VIso(double V)
        {
            throw new NotImplementedException();
        }
    }
}
