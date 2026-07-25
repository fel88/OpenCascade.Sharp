using System;
using System.Reflection.Metadata;
using TKG2d;
using TKG3d;
using TKMath;

namespace OCCPort
{
    //! Representation  of a  curve   by a   curve  in the
    //! parametric space of a surface.
    public class BRep_CurveOnSurface : BRep_GCurve
    {
        public BRep_CurveOnSurface(Geom2d_Curve PC,
            Geom_Surface S, TopLoc_Location L)
            : base(L, PC.FirstParameter(), PC.LastParameter())
        {


            myPCurve = PC;
            mySurface = S;

        }
        public override Geom_Surface Surface()
        {
            return mySurface;
        }


        public override bool IsCurveOnSurface(Geom_Surface S,
            TopLoc_Location L)
        {
            return (S == mySurface) && (L == myLocation);
        }


        public override Geom2d_Curve PCurve()
        {
            return myPCurve;
        }


        public override void Update()
        {
            double f = First();
            double l = Last();
            bool isneg = Precision.IsNegativeInfinite(f);
            bool ispos = Precision.IsPositiveInfinite(l);
            if (!isneg)
            {
                myPCurve.D0(f, out myUV1);
            }
            if (!ispos)
            {
                myPCurve.D0(l, out myUV2);
            }
        }

        Geom2d_Curve myPCurve;
        Geom_Surface mySurface;

        protected gp_Pnt2d myUV1;
        protected gp_Pnt2d myUV2;



        public override bool IsCurveOnSurface()
        {
            return true;
        }

        public override BRep_CurveRepresentation Copy()
        {
            throw new NotImplementedException();
        }

        public override void Continuity(GeomAbs_Shape shape)
        {
            throw new NotImplementedException();
        }
    }
}