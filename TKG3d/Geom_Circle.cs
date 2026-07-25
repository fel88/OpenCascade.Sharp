using OCCPort.Common;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using TKMath;

namespace TKG3d
{
    //! Describes a circle in 3D space.
    //! A circle is defined by its radius and, as with any conic
    //! curve, is positioned in space with a right-handed
    //! coordinate system (gp_Ax2 object) where:
    //! - the origin is the center of the circle, and
    //! - the origin, "X Direction" and "Y Direction" define the
    //! plane of the circle.
    //! This coordinate system is the local coordinate
    //! system of the circle.
    //! The "main Direction" of this coordinate system is the
    //! vector normal to the plane of the circle. The axis, of
    //! which the origin and unit vector are respectively the
    //! origin and "main Direction" of the local coordinate
    //! system, is termed the "Axis" or "main Axis" of the circle.
    //! The "main Direction" of the local coordinate system
    //! gives an explicit orientation to the circle (definition of
    //! the trigonometric sense), determining the direction in
    //! which the parameter increases along the circle.
    //! The Geom_Circle circle is parameterized by an angle:
    //! P(U) = O + R*Cos(U)*XDir + R*Sin(U)*YDir, where:
    //! - P is the point of parameter U,
    //! - O, XDir and YDir are respectively the origin, "X
    //! Direction" and "Y Direction" of its local coordinate system,
    //! - R is the radius of the circle.
    //! The "X Axis" of the local coordinate system therefore
    //! defines the origin of the parameter of the circle. The
    //! parameter is the angle with this "X Direction".
    //! A circle is a closed and periodic curve. The period is
    //! 2.*Pi and the parameter range is [ 0, 2.*Pi [.
    public class Geom_Circle : Geom_Conic
    {
        public Geom_Circle(gp_Circ C)
        {
            radius = (C.Radius());
            pos = C.Position();
        }

        public gp_Circ Circ() { return new gp_Circ(pos, radius); }

        public override void Transform(gp_Trsf T)
        {
            radius = radius * Math.Abs(T.ScaleFactor());
            pos.Transform(T);
        }

        public Geom_Circle(gp_Ax2 A2, double R)
        {
            radius = (R);
            if (R < 0.0)
                throw new Standard_ConstructionError();

            pos = A2;
        }

        public override Geom_Geometry Copy()
        {
            Geom_Circle C = new(pos, radius);
            return C;
        }

        //! Returns in P the point of parameter U.
        //! P = C + R * Cos (U) * XDir + R * Sin (U) * YDir
        //! where C is the center of the circle , XDir the XDirection and
        //! YDir the YDirection of the circle's local coordinate system.
        public override void D0(double U, ref gp_Pnt P)
        {
            P = ElCLib.CircleValue(U, pos, radius);
        }

        public override bool IsPeriodic() { return true; }

        //! Returns the radius of this circle.
        public double Radius()
        {
            return radius;
        }


        double radius;

        //! Returns the value of the first parameter of this
        //! circle. This is  0.0, which gives the start point of this circle, or
        //! The start point and end point of a circle are coincident.
        public override double FirstParameter()
        {
            return 0.0;
        }

        public override double LastParameter() { return 2.0 * Math.PI; }

        public override double Eccentricity()
        {
            return 0;
        }

        public override void D1(double U, out gp_Pnt P, out gp_Vec V1)
        {
            throw new NotImplementedException();
        }

        public override bool IsClosed()
        {
            throw new NotImplementedException();
        }
    }
}
