using System.Threading;
using TKBRep;
using TKernel;
using TKG3d;
using TKMath;

namespace OCCPort
{
    //! The Surface from BRepAdaptor allows to  use a Face
    //! of the BRep topology look like a 3D surface.
    //!
    //! It  has  the methods  of  the class   Surface from
    //! Adaptor3d.
    //!
    //! It is created or initialized with a Face. It takes
    //! into account the local coordinates system.
    //!
    //! The  u,v parameter range is   the minmax value for
    //! the  restriction,  unless  the flag restriction is
    //! set to false.
    public class BRepAdaptor_Surface : Adaptor3d_Surface
    {
        public GeomAdaptor_Surface Surface()
        {
            return mySurf;
        }

        public override gp_Sphere Sphere()
        {
            return mySurf.Sphere().Transformed(myTrsf);
        }


        public override gp_Cylinder Cylinder()
        {
            return mySurf.Cylinder().Transformed(myTrsf);
        }

        //! Returns the type of the surface : Plane, Cylinder,
        //! Cone,      Sphere,        Torus,    BezierSurface,
        //! BSplineSurface,               SurfaceOfRevolution,
        //! SurfaceOfExtrusion, OtherSurface
        public override GeomAbs_SurfaceType _GetType() { return mySurf._GetType(); }

        //! Creates an undefined surface with no face loaded.
        public BRepAdaptor_Surface()
        {

        }

        //! Creates a surface to  access the geometry  of <F>.
        //! If  <Restriction> is  true  the parameter range is
        //! the  parameter  range  in   the  UV space  of  the
        //! restriction.
        public BRepAdaptor_Surface(TopoDS_Face F, bool R = true)
        {
            Initialize(F, R);
        }

        public BRepAdaptor_Surface(BRepAdaptor_Surface aSurface)
        {
            mySurf = aSurface.mySurf;
            myTrsf = aSurface.myTrsf;
            myFace = aSurface.myFace;
        }

        public gp_Pnt Value(double U, double V)
        {
            return mySurf.Value(U, V).Transformed(myTrsf);
        }

        public override Adaptor3d_Surface BasisSurface()
        {
            GeomAdaptor_Surface HS = new GeomAdaptor_Surface();
            HS.Load((Geom_Surface)(mySurf.Surface().Transformed(myTrsf)));
            return HS.BasisSurface();
        }

        public override double FirstVParameter() { return mySurf.FirstVParameter(); }

        public override double LastVParameter() { return mySurf.LastVParameter(); }


        public override double FirstUParameter() { return mySurf.FirstUParameter(); }
        public override double LastUParameter() { return mySurf.LastUParameter(); }

        GeomAdaptor_Surface mySurf = new GeomAdaptor_Surface();
        gp_Trsf myTrsf;
        TopoDS_Face myFace;

        //! Sets the surface to the geometry of <F>.
        public void Initialize(TopoDS_Face F, bool Restriction = true)
        {
            myFace = F;
            TopLoc_Location L;
            Geom_Surface aSurface = BRep_Tool.Surface(F, out L);
            if (aSurface == null)
                return;

            if (Restriction)
            {
                double umin = 0, umax = 0, vmin = 0, vmax = 0;
                BRepTools.UVBounds(F, ref umin, ref umax, ref vmin, ref vmax);
                mySurf.Load(aSurface, umin, umax, vmin, vmax);
            }
            else
                mySurf.Load(aSurface);
            myTrsf = L.Transformation();
        }

        public TopoDS_Face Face()
        {
            return myFace;
        }

        public override gp_Pln Plane()
        {
            return mySurf.Plane().Transformed(myTrsf);
        }

        public override bool IsVPeriodic()
        {
            return mySurf.IsVPeriodic();
        }

        public override bool IsUPeriodic()
        {
            return mySurf.IsUPeriodic();
        }

        public override double UPeriod()
        {
            return mySurf.UPeriod();
        }

        public override double VPeriod()
        {
            return mySurf.VPeriod();
        }

        public override double VResolution(double theR3d)
        {
            return mySurf.VResolution(theR3d);

        }

        public override double UResolution(double theR3d)
        {
            return mySurf.UResolution(theR3d);

        }

        public override void D1(double U, double V, out gp_Pnt P, out gp_Vec D1U, out gp_Vec D1V)
        {
            throw new System.NotImplementedException();
        }

        public override int NbVKnots()
        {
            throw new System.NotImplementedException();
        }

        public override void D0(double U, double V, ref gp_Pnt P)
        {
            mySurf.D0(U, V, ref P);
            P.Transform(myTrsf);
        }

        //! Computes   the point,  the  first  and  second
        //! derivatives on the surface.
        //! Raised  if   the   continuity   of the current
        //! intervals is not C2.
        public override void D2(double U, double V, out gp_Pnt P, out gp_Vec D1U, out gp_Vec D1V, out gp_Vec D2U, out gp_Vec D2V, out gp_Vec D2UV)
        {
            mySurf.D2(U, V, out P, out D1U, out D1V, out D2U, out D2V, out D2UV);
            P.Transform(myTrsf);
            D1U.Transform(myTrsf);
            D1V.Transform(myTrsf);
            D2U.Transform(myTrsf);
            D2V.Transform(myTrsf);
            D2UV.Transform(myTrsf);
        }

        public override int NbVIntervals(GeomAbs_Shape shape)
        {
            throw new NotImplementedException();
        }

        public override int NbUIntervals(GeomAbs_Shape shape)
        {
            throw new NotImplementedException();
        }

        public override void UIntervals(TColStd_Array1OfReal array, GeomAbs_Shape shape)
        {
            throw new NotImplementedException();
        }

        public override void VIntervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {
            throw new NotImplementedException();
        }
    }
}

