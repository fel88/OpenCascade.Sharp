using TKMath;

namespace TKG2d
{
    //! Describes an infinite line in the plane (2D space).
    //! A line is defined and positioned in the plane with an
    //! axis (gp_Ax2d object) which gives it an origin and a unit vector.
    //! The Geom2d_Line line is parameterized as follows:
    //! P (U) = O + U*Dir
    //! where:
    //! - P is the point of parameter U,
    //! - O is the origin and Dir the unit vector of its positioning axis.
    //! The parameter range is ] -infinite, +infinite [.
    //! The orientation of the line is given by the unit vector
    //! of its positioning axis.
    //! See Also
    //! GCE2d_MakeLine which provides functions for more
    //! complex line constructions
    //! gp_Ax2d
    //! gp_Lin2d for an equivalent, non-parameterized data structure.
    public class Geom2d_Line : Geom2d_Curve
    {


        public Geom2d_Line(gp_Lin2d L)
        {
            pos = (L.Position());
        }
        //! Constructs a line passing through point P and parallel to
        //! vector V (P and V are, respectively, the origin
        //! and the unit vector of the positioning axis of the line).
        public Geom2d_Line(gp_Pnt2d P, gp_Dir2d V)
        {
            pos = new gp_Ax2d(P, V);

        }

        public override Geom2d_Geometry Copy()
        {
            Geom2d_Line L;
            L = new Geom2d_Line(pos);
            return L;
        }
        public gp_Lin2d Lin2d() { return new gp_Lin2d(pos); }

        //! Computes the parameter on the reversed line for the
        //! point of parameter U on this line.
        //! For a line, the returned value is -U.
        public override double ReversedParameter(double U)
        {
            return (-U);
        }

        public override void D0(double U, out gp_Pnt2d P)
        {
            P = ElCLib.LineValue(U, pos);
        }

        public override double FirstParameter()
        { return -Precision.Infinite(); }

        //=======================================================================
        //function : LastParameter
        //purpose  : 
        //=======================================================================
        gp_Ax2d pos;
        public Geom2d_Line(gp_Ax2d A) { pos = (A); }

        public override double LastParameter()
        { return Precision.Infinite(); }

        public override void Reverse()
        {
            pos.Reverse();
        }

        public override bool IsPeriodic()
        {
            return false;
        }

        public override void D1(double U, out gp_Pnt2d P, out gp_Vec2d V1)
        {
            P = new gp_Pnt2d();
            V1 = new gp_Vec2d();
            ElCLib.LineD1(U, pos, ref P, ref V1);

        }

        public override void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2)
        {
            V2 = new gp_Vec2d();
            P = new gp_Pnt2d();
            V1 = new gp_Vec2d();
            ElCLib.LineD1(U, pos, ref P, ref V1);
            V2.SetCoord(0.0, 0.0);
        }
    }
}
