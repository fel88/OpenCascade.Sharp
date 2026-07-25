using OCCPort.Common;
using TKernel;
using TKG2d;
using TKG3d;
using TKMath;

namespace TKGeomBase
{
    public class ProjLib_CompProjectedCurve : Adaptor2d_Curve2d
    {
        public ProjLib_CompProjectedCurve(Adaptor3d_Surface theSurface,
            Adaptor3d_Curve theCurve, double theTolU, double theTolV, double theMaxDist)
        {
            mySurface = (theSurface);
            myCurve = (theCurve);
            myNbCurves = (0);
            //mySequence(new ProjLib_HSequenceOfHSequenceOfPnt()),
            myTol3d = (1e-6);
            myContinuity = GeomAbs_Shape.GeomAbs_C2;
            myMaxDegree = (14);
            myMaxSeg = (16);
            myProj2d = (true);
            myProj3d = (false);
            myMaxDist = (theMaxDist);
            myTolU = theTolU;
            myTolV = theTolV;
            Init();

        }

        public override double FirstParameter()
        {
            return myCurve.FirstParameter();
        }

        //=======================================================================
        //function : LastParameter
        //purpose  : 
        //=======================================================================

        public override double LastParameter()
        {
            return myCurve.LastParameter();
        }

        int myNbCurves;
        double myTol3d;

        Adaptor3d_Surface mySurface;
        Adaptor3d_Curve myCurve;

        GeomAbs_Shape myContinuity;
        int myMaxDegree;
        int myMaxSeg;
        bool myProj2d;
        bool myProj3d;
        double myMaxDist;
        double myTolU;
        double myTolV;
        TColStd_HArray1OfReal myTabInt;

        void Init()
        {
            myTabInt = null;
            List<double> aSplits = new List<double>();
            aSplits.Clear();

            /*
             a lot of code here
             */

        }
        public override gp_Lin2d Line()
        {
            throw new System.NotImplementedException();
        }

        public override GeomAbs_CurveType _GetType()
        {
            return GeomAbs_CurveType.GeomAbs_OtherCurve;


        }

        internal int NbCurves()
        {
            return myNbCurves;

        }
        ProjLib_SequenceOfHSequenceOfPnt mySequence;

        //! returns the bounds of the continuous part corresponding to Index
        public void Bounds(int Index, out double Udeb, out double Ufin)
        {
            if (Index < 1 || Index > myNbCurves)
                throw new Standard_NoSuchObject();

            Udeb = mySequence.Value(Index).Value(1).X();
            Ufin = mySequence.Value(Index).Value(mySequence.Value(Index).Length()).X();

        }

        public override int Degree()
        {
            throw new NotImplementedException();
        }

        public override int NbKnots()
        {
            throw new NotImplementedException();
        }

        public override Geom2d_BSplineCurve BSpline()
        {
            throw new NotImplementedException();
        }

        public override Geom2d_BezierCurve Bezier()
        {
            throw new NotImplementedException();
        }

        public override void D1(double U, out gp_Pnt2d P, out gp_Vec2d V)
        {
            throw new NotImplementedException();
        }

        public override void D0(double U, ref gp_Pnt2d P)
        {
            throw new NotImplementedException();
        }

        public override double Resolution(double u)
        {
            throw new NotImplementedException();
        }

        public override bool IsPeriodic()
        {
            throw new NotImplementedException();
        }

        public override gp_Circ2d Circle()
        {
            throw new NotImplementedException();
        }

        public override double Period()
        {
            throw new NotImplementedException();
        }

        public override void D2(double U, out gp_Pnt2d P, out gp_Vec2d V1, out gp_Vec2d V2)
        {
            throw new NotImplementedException();
        }

        public override int NbIntervals(GeomAbs_Shape shape)
        {
            throw new NotImplementedException();
        }

        public override void Intervals(TColStd_Array1OfReal T, GeomAbs_Shape S)
        {
            throw new NotImplementedException();
        }
    }
}
