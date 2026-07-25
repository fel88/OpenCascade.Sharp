using OCCPort.Common;
using TKBRep;
using TKernel;
using TKG2d;
using TKG3d;
using TKMath;

namespace OCCPort
{
    //! The Curve from BRepAdaptor  allows to use  an Edge
    //! of the BRep topology like a 3D curve.
    //!
    //! It has the methods the class Curve from Adaptor3d.
    //!
    //! It  is created or  Initialized  with  an Edge.  It
    //! takes  into account local  coordinate systems.  If
    //! the Edge has a 3D curve it is  use  with priority.
    //! If the edge  has no 3D curve one  of the curves on
    //! surface is used. It is possible to enforce using a
    //! curve on surface by creating  or initialising with
    //! an Edge and a Face.
    public class BRepAdaptor_Curve : Adaptor3d_Curve
    {

        public override double FirstParameter()
        {
            if (myConSurf == null)
            {
                return myCurve.FirstParameter();
            }
            else
            {
                return myConSurf.FirstParameter();
            }
        }
        public override double Resolution(double R)
        {
            if (myConSurf == null)
            {
                return myCurve.Resolution(R);
            }
            else
            {
                return myConSurf.Resolution(R);
            }
        }
        public Adaptor3d_CurveOnSurface CurveOnSurface()
        {
            return myConSurf;
        }


        public override double LastParameter()
        {
            if (myConSurf == null)
            {
                return myCurve.LastParameter();
            }
            else
            {
                return myConSurf.LastParameter();
            }
        }

        gp_Trsf myTrsf;
        //GeomAdaptor_Curve myCurve;
        Adaptor3d_CurveOnSurface myConSurf;
        TopoDS_Edge myEdge;

        //! Returns True if the edge geometry is computed from
        //! a pcurve on a surface.
        public bool IsCurveOnSurface()
        {
            return myConSurf != null;
        }
        //! Creates an undefined Curve with no Edge loaded.

        public BRepAdaptor_Curve() { }

        public BRepAdaptor_Curve(TopoDS_Edge E)
        {
            Initialize(E);
        }

        public BRepAdaptor_Curve(TopoDS_Edge E,
                     TopoDS_Face F)
        {
            Initialize(E, F);
        }
        public void Initialize(TopoDS_Edge E,
                   TopoDS_Face F)
        {
            myConSurf = null;

            myEdge = E;
            TopLoc_Location L;
            double pf = 0, pl = 0;
            Geom_Surface S = BRep_Tool.Surface(F, out L);
            Geom2d_Curve PC = BRep_Tool.CurveOnSurface(E, F, ref pf, ref pl);

            GeomAdaptor_Surface HS = new GeomAdaptor_Surface();
            HS.Load(S);
            Geom2dAdaptor_Curve HC = new Geom2dAdaptor_Curve();
            HC.Load(PC, pf, pl);
            myConSurf = new Adaptor3d_CurveOnSurface();
            myConSurf.Load(HC, HS);

            myTrsf = L.Transformation();
        }
        public override gp_Lin Line()
        {
            gp_Lin L = null;
            if (myConSurf == null)
                L = myCurve.Line();
            else
                L = myConSurf.Line();

            L.Transform(myTrsf);
            return L;
        }

        GeomAdaptor_Curve myCurve = new GeomAdaptor_Curve();
        //! Sets  the Curve <me>  to access the  geometry of
        //! edge <E>.
        public void Initialize(TopoDS_Edge E)
        {
            myConSurf = null;
            myEdge = E;
            double pf, pl;

            TopLoc_Location L;
            Geom_Curve C = BRep_Tool.Curve(E, out L, out pf, out pl);

            if (C != null)
            {
                myCurve.Load(C, pf, pl);
            }
            else
            {
                Geom2d_Curve PC;
                Geom_Surface S;
                BRep_Tool.CurveOnSurface(E, out PC, out S, ref L, ref pf, ref pl);
                if (PC != null)
                {
                    GeomAdaptor_Surface HS = new GeomAdaptor_Surface();
                    HS.Load(S);
                    Geom2dAdaptor_Curve HC = new Geom2dAdaptor_Curve();
                    HC.Load(PC, pf, pl);
                    myConSurf = new Adaptor3d_CurveOnSurface();
                    myConSurf.Load(HC, HS);
                }

                else
                {
                    throw new Standard_NullObject("BRepAdaptor_Curve::No geometry");
                }
            }
            myTrsf = L.Transformation();
        }

        public override GeomAbs_CurveType _GetType()
        {
            if (myConSurf == null)
            {
                return myCurve._GetType();
            }
            else
            {
                return myConSurf._GetType();
            }
        }

        public override int Degree()
        {
            throw new System.NotImplementedException();
        }

        public override int NbKnots()
        {
            throw new System.NotImplementedException();
        }

        public override Geom_BSplineCurve BSpline()
        {
            throw new System.NotImplementedException();
        }

        public override gp_Pnt Value(double U)
        {
            gp_Pnt P;
            if (myConSurf == null)
                P = myCurve.Value(U);
            else
                P = myConSurf.Value(U);
            P.Transform(myTrsf);
            return P;
        }

        public override void D0(double U, ref gp_Pnt P)
        {
            if (myConSurf == null)
                myCurve.D0(U, ref P);
            else
                myConSurf.D0(U, ref P);
            P.Transform(myTrsf);
        }

        public override bool IsPeriodic()
        {
            if (myConSurf == null)
            {
                return myCurve.IsPeriodic();
            }
            else
            {
                return myConSurf.IsPeriodic();
            }
        }

        //! Returns  the number  of  intervals for  continuity
        //! <S>. May be one if Continuity(me) >= <S>
        public override int NbIntervals(GeomAbs_Shape S)
        {
            if (myConSurf == null)
            {
                return myCurve.NbIntervals(S);
            }
            else
            {
                return myConSurf.NbIntervals(S);
            }
        }

        public override void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {
            if (myConSurf == null)
            {
                myCurve.Intervals(T, S);
            }
            else
            {
                myConSurf.Intervals(T, S);
            }
        }

        public override gp_Circ Circle()
        {
            gp_Circ C = new gp_Circ();
            if (myConSurf == null)
                C = myCurve.Circle();
            else
                C = myConSurf.Circle();
            C.Transform(myTrsf);
            return C;
        }

        public override double Period()
        {
            if (myConSurf == null)
            {
                return myCurve.Period();
            }
            else
            {
                return myConSurf.Period();
            }
        }

        public override void D1(double d, out gp_Pnt p, out gp_Vec v)
        {
            throw new NotImplementedException();
        }


        //! Returns the point P of parameter U, the first and second
        //! derivatives V1 and V2.
        //! Raised if the continuity of the current interval
        //! is not C2.
        public override void D2(double U, out gp_Pnt P, out gp_Vec V1, out gp_Vec V2)
        {
            if (myConSurf == null)
                myCurve.D2(U, out P, out V1, out V2);
            else
                myConSurf.D2(U, out P, out V1, out V2);
            P.Transform(myTrsf);
            V1.Transform(myTrsf);
            V2.Transform(myTrsf);
        }
    }
}

