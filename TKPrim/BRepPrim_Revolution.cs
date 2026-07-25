using OCCPort;
using System.Reflection.Metadata;
using TKBRep;
using TKG2d;
using TKG3d;
using TKMath;

namespace TKPrim
{
    //! Implement  the OneAxis algorithm   for a revolution
    //! surface.
    public class BRepPrim_Revolution : BRepPrim_OneAxis

    {
        public BRepPrim_Revolution(gp_Ax2 A, double VMin, double VMax) : base(new BRepPrim_Builder(), A, VMin, VMax)
        {
        }

        public void Meridian(Geom_Curve M,
                     Geom2d_Curve PM)
        {
            myMeridian = M;
            myPMeridian = PM;
        }

        public override gp_Pnt2d MeridianValue(double V)
        {
            return myPMeridian.Value(V);
        }

        public override TopoDS_Face MakeEmptyLateralFace()
        {
            Geom_SurfaceOfRevolution S =
    new Geom_SurfaceOfRevolution(myMeridian, Axes().Axis());

            TopoDS_Face F = new TopoDS_Face();
            myBuilder.Builder().MakeFace(F, S, Precision.Confusion());
            return F;
        }

        public override void SetMeridianPCurve(TopoDS_Edge E, TopoDS_Face F)
        {
            throw new NotImplementedException();
        }

        public override TopoDS_Edge MakeEmptyMeridianEdge(double Ang)
        {
            TopoDS_Edge E = new TopoDS_Edge();
            Geom_Curve C = (Geom_Curve)(myMeridian.Copy());
            gp_Trsf T = new gp_Trsf();
            T.SetRotation(Axes().Axis(), Ang);
            C.Transform(T);
            myBuilder.Builder().MakeEdge(E, C, Precision.Confusion());
            return E;
        }

        Geom_Curve myMeridian;
        Geom2d_Curve myPMeridian;
    }

}
