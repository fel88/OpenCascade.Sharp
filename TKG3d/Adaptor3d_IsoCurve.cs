using OCCPort.Common;
using System.Reflection.Metadata;
using TKernel;
using TKMath;

namespace TKG3d
{
    //! Defines an isoparametric curve on  a surface.  The
    //! type  of isoparametric curve  (U  or V) is defined
    //! with the   enumeration  IsoType from   GeomAbs  if
    //! NoneIso is given an error is raised.
    public class Adaptor3d_IsoCurve : Adaptor3d_Curve
    {
        public override Geom_BSplineCurve BSpline()
        {
            throw new NotImplementedException();
        }
        public void Load(Adaptor3d_Surface S)
        {
            mySurface = S;
            myIso =GeomAbs_IsoType. GeomAbs_NoneIso;
        }
        public override gp_Circ Circle()
        {
            throw new NotImplementedException();
        }

        public override  double  FirstParameter()   { return myFirst; }
        public override  double LastParameter()   { return myLast; }
          
Adaptor3d_Surface mySurface;
        GeomAbs_IsoType myIso;
        double myFirst;
        double myLast;
        double myParameter;

        public void Load(GeomAbs_IsoType Iso,

                 double Param,

                 double WFirst,

                 double WLast)
        {
            myIso = Iso;
            myParameter = Param;
            myFirst = WFirst;
            myLast = WLast;


            if (myIso == GeomAbs_IsoType.GeomAbs_IsoU)
            {
                myFirst = Math.Max(myFirst, mySurface.FirstVParameter());
                myLast = Math.Min(myLast, mySurface.LastVParameter());
            }
            else
            {
                myFirst = Math.Max(myFirst, mySurface.FirstUParameter());
                myLast = Math.Min(myLast, mySurface.LastUParameter());
            }

            // Adjust the parameters on periodic surfaces

            double dummy = myParameter;

            if (mySurface.IsUPeriodic())
            {

                if (myIso == GeomAbs_IsoType.GeomAbs_IsoU)
                {
                    ElCLib.AdjustPeriodic
                  (mySurface.FirstUParameter(),
                   mySurface.FirstUParameter() +
                   mySurface.UPeriod(),
                   mySurface.UResolution(Precision.Confusion()),
                  ref myParameter, ref dummy);
                }
                else
                {
                    ElCLib.AdjustPeriodic
                  (mySurface.FirstUParameter(),
                   mySurface.FirstUParameter() +
                   mySurface.UPeriod(),
                   mySurface.UResolution(Precision.Confusion()),
                  ref myFirst, ref myLast);
                }
            }

            if (mySurface.IsVPeriodic())
            {

                if (myIso == GeomAbs_IsoType.GeomAbs_IsoV)
                {
                    ElCLib.AdjustPeriodic
                  (mySurface.FirstVParameter(),
                   mySurface.FirstVParameter() +
                   mySurface.VPeriod(),
                   mySurface.VResolution(Precision.Confusion()),
                   ref myParameter, ref dummy);
                }
                else
                {
                    ElCLib.AdjustPeriodic
                  (mySurface.FirstVParameter(),
                   mySurface.FirstVParameter() +
                   mySurface.VPeriod(),
                   mySurface.VResolution(Precision.Confusion()),
                 ref myFirst, ref myLast);
                }
            }

        }

        public override void D0(double d, ref gp_Pnt p)
        {
            throw new NotImplementedException();
        }

        public override void D1(double d, out gp_Pnt p, out gp_Vec v)
        {
            throw new NotImplementedException();
        }

        public override void D2(double d, out gp_Pnt p, out gp_Vec v1, out gp_Vec v2)
        {
            throw new NotImplementedException();
        }

        public override int Degree()
        {
            throw new NotImplementedException();
        }

        public override void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {
            throw new NotImplementedException();
        }

        public override bool IsPeriodic()
        {
            throw new NotImplementedException();
        }

        public override gp_Lin Line()
        {
            throw new NotImplementedException();
        }

        public override int NbIntervals(GeomAbs_Shape S)
        {
            throw new NotImplementedException();
        }

        public override int NbKnots()
        {
            throw new NotImplementedException();
        }

        public override double Period()
        {
            throw new NotImplementedException();
        }

        public override double Resolution(double R3d)
        {
            throw new NotImplementedException();
        }

        public override gp_Pnt Value(double d)
        {
            throw new NotImplementedException();
        }

        public override GeomAbs_CurveType _GetType()
        {
            throw new NotImplementedException();
        }
    }


    //! this enumeration describes if a curve is an U isoparaetric
    //! or V isoparametric
    public enum GeomAbs_IsoType
    {
        GeomAbs_IsoU,
        GeomAbs_IsoV,
        GeomAbs_NoneIso
    };
}
