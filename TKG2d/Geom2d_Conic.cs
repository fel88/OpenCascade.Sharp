using TKMath;

namespace TKG2d
{
    //! The abstract class Conic describes the common
    //! behavior of conic curves in 2D space and, in
    //! particular, their general characteristics. The Geom2d
    //! package provides four specific classes of conics:
    //! Geom2d_Circle, Geom2d_Ellipse,
    //! Geom2d_Hyperbola and Geom2d_Parabola.
    //! A conic is positioned in the plane with a coordinate
    //! system (gp_Ax22d object), where the origin is the
    //! center of the conic (or the apex in case of a parabola).
    //! This coordinate system is the local coordinate
    //! system of the conic. It gives the conic an explicit
    //! orientation, determining the direction in which the
    //! parameter increases along the conic. The "X Axis" of
    //! the local coordinate system also defines the origin of
    //! the parameter of the conic.
    public class Geom2d_Conic : Geom2d_Curve
    {
        //! Returns the local coordinates system of the conic.
        public  gp_Ax22d Position()  { return pos; }

        public override Geom2d_Geometry Copy()
        {
            throw new NotImplementedException();
        }
       protected gp_Ax22d pos;

        public override void D0(double U, out gp_Pnt2d P)
        {
            throw new NotImplementedException();
        }

        public override void D1(double U, out gp_Pnt2d P, out gp_Vec2d V1)
        {
            throw new NotImplementedException();
        }

        public override double FirstParameter()
        {
            throw new NotImplementedException();
        }

        public override bool IsPeriodic()
        {
            throw new NotImplementedException();
        }

        public override double LastParameter()
        {
            throw new NotImplementedException();
        }

        public override void Reverse()
        {
            throw new NotImplementedException();
        }

        public override double ReversedParameter(double U)
        {
            throw new NotImplementedException();
        }

        public override void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2)
        {
            throw new NotImplementedException();
        }

        public override gp_Vec2d DN(double U, int N)
        {
            throw new NotImplementedException();
        }
    }
}
