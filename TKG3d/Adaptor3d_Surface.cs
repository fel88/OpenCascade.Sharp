using OCCPort.Common;
using TKMath;

namespace TKG3d
{
    //! Root class for surfaces on which geometric algorithms work.
    //! An adapted surface is an interface between the
    //! services provided by a surface and those required of
    //! the surface by algorithms which use it.
    //! A derived concrete class is provided:
    //! GeomAdaptor_Surface for a surface from the Geom package.
    //! The  Surface class describes  the standard behaviour
    //! of a surface for generic algorithms.
    //!
    //! The Surface can  be decomposed in intervals of any
    //! continuity in U and V using the method NbIntervals.
    //! A current interval can be set.
    //! Most of the methods apply to the current interval.
    //! Warning: All the methods are virtual and implemented with a
    //! raise to allow to redefined only the methods really used.
    //!
    //! Polynomial coefficients of BSpline surfaces used for their evaluation are cached for better performance.
    //! Therefore these evaluations are not thread-safe and parallel evaluations need to be prevented.
    public abstract class Adaptor3d_Surface
    {
        public abstract double FirstUParameter();
        //=======================================================================
        //function : UDegree
        //purpose  : 
        //=======================================================================

        public virtual int UDegree()
        {
            throw new Standard_NotImplemented("Adaptor3d_Surface::UDegree");
        }
        public abstract bool IsVPeriodic();
        public abstract bool IsUPeriodic();

        public abstract double UPeriod();
        public abstract double VPeriod();
        public abstract double VResolution(double v);
        public abstract double UResolution(double v);

        //=======================================================================
        //function : NbVKnots
        //purpose  : 
        //=======================================================================
        //! Computes the point  and the first derivatives on the surface.
        //! Raised if the continuity of the current intervals is not C1.
        //!
        //! Tip: use GeomLib::NormEstim() to calculate surface normal at specified (U, V) point.
        public abstract void D1(double U, double V, out gp_Pnt P, out gp_Vec D1U, out gp_Vec D1V);

        public abstract int NbVKnots();
        //=======================================================================
        //function : D0
        //purpose  : 
        //=======================================================================

        //void Adaptor3d_Surface::D0(const Standard_Real U, const Standard_Real V, gp_Pnt& P) const 
        public abstract void D0(double u, double v, ref gp_Pnt pnt);



        public abstract double LastUParameter();
        //=======================================================================
        //function : NbVPoles
        //purpose  : 
        //=======================================================================

        public virtual int NbVPoles()
        {
            throw new Standard_NotImplemented("Adaptor3d_Surface::NbVPoles");
        }

        //=======================================================================
        //function : OffsetValue
        //purpose  : 
        //=======================================================================

        public virtual double OffsetValue()
        {
            throw new Standard_NotImplemented("Adaptor3d_Surface::OffsetValue");
        }


        //=======================================================================
        //function : BasisSurface
        //purpose  : 
        //=======================================================================

        public abstract Adaptor3d_Surface BasisSurface();

        public abstract double FirstVParameter();
        public abstract double LastVParameter();

        //=======================================================================
        //function : NbUPoles
        //purpose  : 
        //=======================================================================

        public virtual int NbUPoles()
        {
            throw new Standard_NotImplemented("Adaptor3d_Surface::NbUPoles");
        }

        //=======================================================================
        //function : BSpline
        //purpose  : 
        //=======================================================================

        public virtual Geom_BSplineSurface BSpline()
        {
            throw new Standard_NotImplemented("Adaptor3d_Surface::BSpline");
        }
        public abstract gp_Pln Plane();        
        public abstract gp_Sphere Sphere();
        public abstract gp_Cylinder Cylinder();
        //! Returns the type of the surface : Plane, Cylinder,
        //! Cone,      Sphere,        Torus,    BezierSurface,
        //! BSplineSurface,               SurfaceOfRevolution,
        //! SurfaceOfExtrusion, OtherSurface
        public abstract GeomAbs_SurfaceType _GetType();
    }
}
