using OCCPort.Common;
using TKMath;

namespace TKG3d
{
    //! Describes an infinite line.
    //! A line is defined and positioned in space with an axis
    //! (gp_Ax1 object) which gives it an origin and a unit vector.
    //! The Geom_Line line is parameterized:
    //! P (U) = O + U*Dir, where:
    //! - P is the point of parameter U,
    //! - O is the origin and Dir the unit vector of its positioning axis.
    //! The parameter range is ] -infinite, +infinite [.
    //! The orientation of the line is given by the unit vector
    //! of its positioning axis.

    public class Geom_Line : Geom_Curve
    {

        gp_Ax1 pos = new gp_Ax1();
        public override double FirstParameter()
        { return -Precision.Infinite(); }

        //=======================================================================
        //function : LastParameter
        //purpose  : 
        //=======================================================================

        public override void D1(double U, out gp_Pnt P, out gp_Vec V1)
        {
            P = new gp_Pnt();
            V1 = new gp_Vec();
            ElCLib.LineD1(U, pos, ref  P, ref V1);
        }

        public override double LastParameter()
        { return Precision.Infinite(); }
        public override void D0(double U, ref gp_Pnt P)
        {
            P = ElCLib.LineValue(U, pos);
        }

        public override void Transform(gp_Trsf t)
        {
            pos.Transform(t);
        }   //! Returns non transient line from gp with the same geometric
            //! properties as <me>
        public gp_Lin Lin() { return new gp_Lin(pos); }


        public override Geom_Geometry Copy()
        {
            Geom_Line L = new Geom_Line(pos);
            return L;
        }

        public override bool IsPeriodic()
        {
            return false;
        }

        public override void Reverse()
        {
            pos.Reverse();

        }

        public override double ReversedParameter(double U)
        {
            return -U;
        }

        public override GeomAbs_Shape Continuity()
        {
            return GeomAbs_Shape.GeomAbs_CN;
        }

        public override bool IsClosed()
        {
            return false;
        }

        //! Constructs a line passing through point P and parallel to vector V
        //! (P and V are, respectively, the origin and the unit
        //! vector of the positioning axis of the line).
        public  Geom_Line( gp_Pnt P,  gp_Dir V)
        {
            pos = new(P, V);
        }
  

        public Geom_Line(gp_Ax1 A) { pos = (A); }

        public Geom_Line(gp_Lin L)
        {
            pos = L.Position();
        }
    }
}
