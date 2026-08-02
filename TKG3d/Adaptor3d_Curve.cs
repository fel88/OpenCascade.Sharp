using OCCPort.Common;
using TKernel;
using TKMath;

namespace TKG3d
{
    //! Root class for 3D curves on which geometric
    //! algorithms work.
    //! An adapted curve is an interface between the
    //! services provided by a curve and those required of
    //! the curve by algorithms which use it.
    //! Two derived concrete classes are provided:
    //! - GeomAdaptor_Curve for a curve from the Geom package
    //! - Adaptor3d_CurveOnSurface for a curve lying on
    //! a surface from the Geom package.
    //!
    //! Polynomial coefficients of BSpline curves used for their evaluation are
    //! cached for better performance. Therefore these evaluations are not
    //! thread-safe and parallel evaluations need to be prevented.
    public abstract class Adaptor3d_Curve : ITheCurve
    {
        

        //! Returns  the number  of  intervals for  continuity
        //! <S>. May be one if Continuity(me) >= <S>
        public abstract int NbIntervals(GeomAbs_Shape S);

        //! Stores in <T> the  parameters bounding the intervals
        //! of continuity <S>.
        //!
        //! The array must provide  enough room to  accommodate
        //! for the parameters. i.e. T.Length() > NbIntervals()
        public abstract void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S);


        public abstract double Period();


        //=======================================================================
        //function : GetType
        //purpose  : 
        //=======================================================================
        public abstract int Degree();
        public abstract int NbKnots();

        public abstract Geom_BSplineCurve BSpline();

        //! Returns the parametric  resolution corresponding
        //! to the real space resolution <R3d>.
        public abstract double Resolution(double R3d);
        public abstract bool IsPeriodic();

        [NotOrigin]
        public abstract GeomAbs_CurveType _GetType();
        //=======================================================================
        //function : Line
        //purpose  : 
        //=======================================================================

        public abstract gp_Lin Line();
        public abstract gp_Circ Circle();

        public abstract gp_Pnt Value(double d);

        public abstract double FirstParameter();
        public abstract double LastParameter();

        //void Adaptor3d_Curve::D0(const Standard_Real U, gp_Pnt& P) const 
        public abstract void D0(double d, ref gp_Pnt p);
        public abstract void D1(double d, out gp_Pnt p, out gp_Vec v);
        public abstract void D2(double d, out gp_Pnt p, out gp_Vec v1, out gp_Vec v2);
    }
}
