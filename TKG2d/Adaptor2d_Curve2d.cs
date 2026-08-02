using OCCPort.Common;
using TKernel;
using TKMath;

namespace TKG2d
{
    //! Root class for 2D curves on which geometric
    //! algorithms work.
    //! An adapted curve is an interface between the
    //! services provided by a curve, and those required of
    //! the curve by algorithms, which use it.
    //! A derived concrete class is provided:
    //! Geom2dAdaptor_Curve for a curve from the Geom2d package.
    //!
    //! Polynomial coefficients of BSpline curves used for their evaluation are
    //! cached for better performance. Therefore these evaluations are not
    //! thread-safe and parallel evaluations need to be prevented.
    public abstract class Adaptor2d_Curve2d : ITheCurve
    {

        public abstract bool IsPeriodic();

        //! Computes the point of parameter U on the curve.
        public abstract void D0(double U, ref gp_Pnt2d P);
        public abstract double Resolution(double u);
        //! Computes the point of parameter U on the curve.
        public abstract gp_Pnt2d Value(double U);
        public abstract double Period();

        public abstract int NbIntervals(GeomAbs_Shape shape);

        //! Stores in <T> the  parameters bounding the intervals
        //! of continuity <S>.
        //!
        //! The array must provide  enough room to  accommodate
        //! for the parameters. i.e. T.Length() > NbIntervals()
        public abstract void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S);


        public abstract int Degree();
        //! Computes the point of parameter U on the curve with its
        //! first derivative.
        //! Raised if the continuity of the current interval
        //! is not C1.
        public abstract void D1(double U, out gp_Pnt2d P, out gp_Vec2d V);
        public abstract void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2);
        public abstract int NbKnots();

        public abstract GeomAbs_CurveType _GetType();
        public abstract gp_Lin2d Line();
        public abstract double FirstParameter();
        public virtual int NbSamples()
        {
            return 20;
        }
        public abstract double LastParameter();
        public abstract Geom2d_BSplineCurve BSpline();
        public abstract Geom2d_BezierCurve Bezier();

        public Adaptor2d_Curve2d Trim(double myFirst, double myLast, double v)
        {
            throw new NotImplementedException();
        }

        public abstract gp_Circ2d Circle();
    }
}
