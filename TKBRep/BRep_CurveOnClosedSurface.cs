using OCCPort;
using System.Reflection.Metadata;
using TKG2d;
using TKG3d;
using TKMath;

namespace TKBRep
{
    //! Representation  of a    curve by two  pcurves   on
    //! a closed surface.
    public class BRep_CurveOnClosedSurface : BRep_CurveOnSurface
    {
        public BRep_CurveOnClosedSurface
   (Geom2d_Curve PC1,
    Geom2d_Curve PC2,
    Geom_Surface S,
    TopLoc_Location L,
    GeomAbs_Shape C) : base(PC1, S, L)
        {
            myPCurve2 = (PC2);
            myContinuity = (C);
        }

        public override Geom2d_Curve PCurve2()
        {
            return myPCurve2;
        }

        

        public override void Update()
        {
            if (!Precision.IsNegativeInfinite(First()))
                myPCurve2.D0(First(), out myUV21);
            if (!Precision.IsPositiveInfinite(Last()))
                myPCurve2.D0(Last(), out myUV22);
            base.Update();
        }

        Geom2d_Curve myPCurve2;
        GeomAbs_Shape myContinuity;
        gp_Pnt2d myUV21;
        gp_Pnt2d myUV22;
        public override bool IsCurveOnClosedSurface()
        {
            return true;
        }

        public override GeomAbs_Shape Continuity()
        {
            return myContinuity;
        }

        public override void Continuity(GeomAbs_Shape C)
        {
            myContinuity = C;
        }

        public override bool IsRegularity()
        {
            return true;
        }

        public override bool IsRegularity(Geom_Surface S1,
            Geom_Surface S2,
            TopLoc_Location L1,
            TopLoc_Location L2)
        {
            return ((Surface() == S1) &&
      (Surface() == S2) &&
      (Location() == L1) &&
      (Location() == L2));
        }
    }



}