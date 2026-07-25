using OCCPort.Common;
using TKBRep;
using TKG2d;
using TKG3d;
using TKMath;

namespace TKPrim
{
    //! Implements the sphere primitive
    public class BRepPrim_Sphere : BRepPrim_Revolution
    {
        // parameters on the meridian
        const double PMIN = (-0.5 * Math.PI);
        const double PMAX = (0.5 * Math.PI);

        //! Creates a Sphere at  origin with  Radius. The axes
        //! of the sphere are the  reference axes. An error is
        //! raised if the radius is < Resolution.
        public BRepPrim_Sphere(double Radius) : base(gp.XOY(), PMIN, PMAX)
        {

            myRadius = Radius;
            SetMeridian();
        }

        public override TopoDS_Face MakeEmptyLateralFace()
        {
            Geom_SphericalSurface S =
              new Geom_SphericalSurface(new gp_Ax3(Axes()), myRadius);
            TopoDS_Face F = new TopoDS_Face();
            myBuilder.Builder().MakeFace(F, S, Precision.Confusion());
            return F;
        }


        //! Creates a sphere with given axes system.
        public BRepPrim_Sphere(gp_Ax2 Axes, double Radius) : base(Axes, PMIN, PMAX)
        {
            myRadius = Radius;
            SetMeridian();
        }

        double myRadius;
        void SetMeridian()
        {
            // Offset the parameters on the meridian
            // to trim the edge in 3pi/2, 5pi/2

            SetMeridianOffset(2 * Math.PI);

            gp_Dir D = Axes().YDirection();
            D.Reverse();
            gp_Ax2 A = new(Axes().Location(), D, Axes().XDirection());
            Geom_Circle C = new Geom_Circle(A, myRadius);
            Geom2d_Circle C2d =
              new Geom2d_Circle(new gp_Ax2d(new gp_Pnt2d(0, 0), new gp_Dir2d(1, 0)),
                        myRadius);
            Meridian(C, C2d);
        }
    }
}
