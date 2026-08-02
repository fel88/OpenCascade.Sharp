using OCCPort.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using TKG2d;
using TKG3d;
using TKMath;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TKGeomBase
{
    //=======================================================================
    //function : BndLib_Box2dCurve
    //purpose  : 
    //=======================================================================
    public class BndLib_Box2dCurve
    {
        public BndLib_Box2dCurve()
        {
            Clear();
        }
        public Bnd_Box2d Box()
        {
            return myBox;
        }

        public void GetInfoBase()
        {
            bool bIsTypeBase;
            int iTrimmed, iOffset;
            GeomAbs_CurveType? aTypeB = null;
            Geom2d_Curve aC2DB;
            Geom2d_TrimmedCurve aCT2D;
            Geom2d_OffsetCurve aCF2D;
            //
            myErrorStatus = 0;
            myTypeBase = GeomAbs_CurveType.GeomAbs_OtherCurve;
            myOffsetBase = 0;
            //
            aC2DB = myCurve;
            bIsTypeBase = IsTypeBase(aC2DB, ref aTypeB);
            if (bIsTypeBase)
            {
                myTypeBase = aTypeB.Value;
                myCurveBase = myCurve;
                return;
            }
            //
            while (!bIsTypeBase)
            {
                iTrimmed = 0;
                iOffset = 0;
                aCT2D = (Geom2d_TrimmedCurve)(aC2DB);
                if (aCT2D != null)
                {
                    aC2DB = aCT2D.BasisCurve();
                    ++iTrimmed;
                }
                //
                aCF2D = (Geom2d_OffsetCurve)(aC2DB);
                if (aCF2D != null)
                {
                    double aOffset;
                    //
                    aOffset = aCF2D.Offset();
                    myOffsetBase = myOffsetBase + aOffset;
                    myOffsetFlag = true;
                    //
                    aC2DB = aCF2D.BasisCurve();
                    ++iOffset;
                }
                //
                if (!(iTrimmed != 0 || iOffset != 0))
                {
                    break;
                }
                //
                bIsTypeBase = IsTypeBase(aC2DB, ref aTypeB);
                if (bIsTypeBase)
                {
                    myTypeBase = aTypeB.Value;
                    myCurveBase = aC2DB;
                    return;
                }
            }
            //
            myErrorStatus = 11; // unknown type base
        }
        public bool IsTypeBase
       (Geom2d_Curve aC2D,
        ref GeomAbs_CurveType? aTypeB)
        {
            bool bRet;
            Type aType;
            //
            bRet = true;
            //
            aType = aC2D.GetType();
            if (aType == typeof(Geom2d_Line))
            {
                aTypeB = GeomAbs_CurveType.GeomAbs_Line;
            }
            else if (aType == typeof(Geom2d_Circle))
            {
                aTypeB = GeomAbs_CurveType.GeomAbs_Circle;
            }
            /*else if (aType == typeof(Geom2d_Ellipse))
            {
                aTypeB = GeomAbs_CurveType.GeomAbs_Ellipse;
            }
            else if (aType == typeof(Geom2d_Parabola))
            {
                aTypeB = GeomAbs_CurveType.GeomAbs_Parabola;
            }
            else if (aType == typeof(Geom2d_Hyperbola))
            {
                aTypeB = GeomAbs_CurveType.GeomAbs_Hyperbola;
            }*/
            else if (aType == typeof(Geom2d_BezierCurve))
            {
                aTypeB = GeomAbs_CurveType.GeomAbs_BezierCurve;
            }
            else if (aType == typeof(Geom2d_BSplineCurve))
            {
                aTypeB = GeomAbs_CurveType.GeomAbs_BSplineCurve;
            }
            else
            {
                aTypeB = GeomAbs_CurveType.GeomAbs_OtherCurve;
                bRet = !bRet;
            }
            return bRet;
        }

        public void CheckData()
        {
            myErrorStatus = 0;
            //
            if (myCurve == null)
            {
                myErrorStatus = 10;
                return;
            }
            //
            if (myT1 > myT2)
            {
                myErrorStatus = 12; // invalid range
                return;
            }
        }
        public void PerformLineConic()
        {
            int i; int[] iInf = new int[2];
            double[] aTb = new double[2];
            gp_Pnt2d aP2D;
            //
            myErrorStatus = 0;
            //
            Bnd_Box2d aBox2D = myBox;
            //
            iInf[0] = 0;
            iInf[1] = 0;
            aTb[0] = myT1;
            aTb[1] = myT2;
            //
            for (i = 0; i < 2; ++i)
            {
                if (Precision.IsNegativeInfinite(aTb[i]))
                {
                    D0(aTb[i], out aP2D);
                    aBox2D.Add(aP2D);
                    ++iInf[0];
                }
                else if (Precision.IsPositiveInfinite(aTb[i]))
                {
                    D0(aTb[i], out aP2D);
                    aBox2D.Add(aP2D);
                    ++iInf[1];
                }
                else
                {
                    D0(aTb[i], out aP2D);
                    aBox2D.Add(aP2D);
                }
            }
            //
            if (myTypeBase == GeomAbs_CurveType.GeomAbs_Line)
            {
                return;
            }
            //
            if (iInf[0] != 0 && iInf[1] != 0)
            {
                return;
            }

            //-------------
            Geom2d_Conic aConic2D = null;
            //
            aConic2D = (Geom2d_Conic)(myCurveBase);
            Compute(aConic2D, myTypeBase, aTb[0], aTb[1], aBox2D);

        }
        public int Compute
    (Geom2d_Conic aConic2D,
      GeomAbs_CurveType aType,
     double[] pT)
        {
            int iRet, i, j;
            double aCosBt, aSinBt, aCosGm, aSinGm;
            double aLx, aLy;
            //
            iRet = 0;
            //
            gp_Ax22d aPos = aConic2D.Position();
            gp_XY aXDir = aPos.XDirection().XY();
            gp_XY aYDir = aPos.YDirection().XY();
            //
            aCosBt = aXDir.X();
            aSinBt = aXDir.Y();
            aCosGm = aYDir.X();
            aSinGm = aYDir.Y();
            //
            if (aType == GeomAbs_CurveType.GeomAbs_Circle || aType == GeomAbs_CurveType.GeomAbs_Ellipse)
            {
                double aR1 = 0.0, aR2 = 0.0, aTwoPI = Math.PI + Math.PI;
                double aA11 = 0.0, aA12 = 0.0, aA21 = 0.0, aA22 = 0.0;
                double aBx = 0.0, aBy = 0.0, aB = 0.0, aCosFi = 0.0, aSinFi = 0.0, aFi = 0.0;
                //
                //if (aType == GeomAbs_CurveType.GeomAbs_Ellipse)
                //{
                //    Handle(Geom2d_Ellipse) aEL2D;
                //    //
                //    aEL2D = Handle(Geom2d_Ellipse)::DownCast(aConic2D);
                //    aR1 = aEL2D->MajorRadius();
                //    aR2 = aEL2D->MinorRadius();
                //}
                //else
                if (aType == GeomAbs_CurveType.GeomAbs_Circle)
                {
                    Geom2d_Circle aCR2D;
                    //
                    aCR2D = (Geom2d_Circle)(aConic2D);
                    aR1 = aCR2D.Radius();
                    aR2 = aR1;
                }
                //
                aA11 = -aR1 * aCosBt;
                aA12 = aR2 * aCosGm;
                aA21 = -aR1 * aSinBt;
                aA22 = aR2 * aSinGm;
                //
                for (i = 0; i < 2; ++i)
                {
                    aLx = (i==0) ? 0.0 : 1.0;
                    aLy = (i==0) ? 1.0 : 0.0;
                    aBx = aLx * aA21 - aLy * aA11;
                    aBy = aLx * aA22 - aLy * aA12;
                    aB = Math.Sqrt(aBx * aBx + aBy * aBy);
                    //
                    aCosFi = aBx / aB;
                    aSinFi = aBy / aB;
                    //
                    aFi = Math.Acos(aCosFi);
                    if (aSinFi < 0.0)
                    {
                        aFi = aTwoPI - aFi;
                    }
                    //
                    j = 2 * i;
                    pT[j] = aTwoPI - aFi;
                    pT[j] = AdjustToPeriod(pT[j], aTwoPI);
                    //
                    pT[j + 1] = Math.PI - aFi;
                    pT[j + 1] = AdjustToPeriod(pT[j + 1], aTwoPI);
                }
                iRet = 4;
            }//if (aType==GeomAbs_Ellipse) {
             //
            //else if (aType ==GeomAbs_CurveType. GeomAbs_Parabola)
            //{
            //    double aFc, aEps;
            //    double aA1, aA2;
            //    Geom2d_Parabola aPR2D;
            //    //
            //    aEps = 1e-12;
            //    //
            //    aPR2D = (Geom2d_Parabola)(aConic2D);
            //    aFc = aPR2D->Focal();
            //    //
            //    j = 0;
            //    for (i = 0; i < 2; i++)
            //    {
            //        aLx = (!i) ? 0. : 1.;
            //        aLy = (!i) ? 1. : 0.;
            //        //
            //        aA2 = aLx * aSinBt - aLy * aCosBt;
            //        if (fabs(aA2) < aEps)
            //        {
            //            continue;
            //        }
            //        //
            //        aA1 = aLy * aCosGm - aLx * aSinGm;
            //        //
            //        pT[j] = 2.* aFc * aA1 / aA2;
            //        ++j;
            //    }
            //    iRet = j;
            //}// else if (aType==GeomAbs_Parabola) {
            // //
            //else if (aType == GeomAbs_Hyperbola)
            //{
            //    Standard_Integer k;
            //    Standard_Real aR1, aR2;
            //    Standard_Real aEps, aB1, aB2, aB12, aB22, aZ, aD;
            //    Handle(Geom2d_Hyperbola) aHP2D;
            //    //
            //    aEps = 1.e - 12;
            //    //
            //    aHP2D = Handle(Geom2d_Hyperbola)::DownCast(aConic2D);
            //    aR1 = aHP2D->MajorRadius();
            //    aR2 = aHP2D->MinorRadius();
            //    //
            //    j = 0;
            //    for (i = 0; i < 2; i++)
            //    {
            //        aLx = (!i) ? 0. : 1.;
            //        aLy = (!i) ? 1. : 0.;
            //        //
            //        aB1 = aR1 * (aLx * aSinBt - aLy * aCosBt);
            //        aB2 = aR2 * (aLx * aSinGm - aLy * aCosGm);
            //        // 
            //        if (fabs(aB1) < aEps)
            //        {
            //            continue;
            //        }
            //        //
            //        if (fabs(aB2) < aEps)
            //        {
            //            pT[j] = 0.;
            //            ++j;
            //        }
            //        else
            //        {
            //            aB12 = aB1 * aB1;
            //            aB22 = aB2 * aB2;
            //            if (!(aB12 > aB22))
            //            {
            //                continue;
            //            }
            //            //
            //            aD = sqrt(aB12 - aB22);
            //            //-------------
            //            for (k = -1; k < 2; k += 2)
            //            {
            //                aZ = (aB1 + k * aD) / aB2;
            //                if (fabs(aZ) < 1.)
            //                {
            //                    pT[j] = -log((1.+ aZ) / (1.- aZ));
            //                    ++j;
            //                }
            //            }
            //        }
            //    }
            //    iRet = j;
            //}// else if (aType==GeomAbs_Hyperbola) {
             //
            return iRet;
        }

        public void Compute(Geom2d_Conic aConic2D,
                  GeomAbs_CurveType aType,
                  double aT1,
                  double aT2,
                 Bnd_Box2d aBox2D)
        {
            int i, aNbT;
            double[] pT = new double[10];
            double aT, aTwoPI, dT, aEps;
            gp_Pnt2d aP2D;
            //
            aNbT = Compute(aConic2D, aType, pT);
            //
            if (aType == GeomAbs_CurveType.GeomAbs_Parabola || aType == GeomAbs_CurveType.GeomAbs_Hyperbola)
            {
                for (i = 0; i < aNbT; ++i)
                {
                    aT = pT[i];
                    if (aT > aT1 && aT < aT2)
                    {
                        D0(aT, out aP2D);
                        aBox2D.Add(aP2D);
                    }
                }
                return;
            }
            //
            //aType==GeomAbs_Circle ||  aType==GeomAbs_Ellipse
            aEps = 1e-14;
            aTwoPI = 2.0 * Math.PI;
            dT = aT2 - aT1;
            //
            double aT1z = AdjustToPeriod(aT1, aTwoPI);
            if (Math.Abs(aT1z) < aEps)
            {
                aT1z = 0.0;
            }
            //
            double aT2z = aT1z + dT;
            if (Math.Abs(aT2z - aTwoPI) < aEps)
            {
                aT2z = aTwoPI;
            }
            //
            for (i = 0; i < aNbT; ++i)
            {
                aT = pT[i];
                // adjust aT to range [aT1z, aT1z + 2*PI]; note that pT[i] and aT1z
                // are adjusted to range [0, 2*PI], but aT2z can be greater than 2*PI
                aT = (aT < aT1z ? aT + aTwoPI : aT);
                if (aT <= aT2z)
                {
                    D0(aT, out aP2D);
                    aBox2D.Add(aP2D);
                }
            }
        }
        public double AdjustToPeriod(double aT,
                         double aPeriod)
        {
            int k;
            double aTRet;
            //
            aTRet = aT;
            if (aT < 0.0)
            {
                k = 1 + (int)(-aT / aPeriod);
                aTRet = aT + k * aPeriod;
            }
            else if (aT > aPeriod)
            {
                k = (int)(aT / aPeriod);
                aTRet = aT - k * aPeriod;
            }
            if (aTRet == aPeriod)
            {
                aTRet = 0.0;
            }
            return aTRet;
        }
        public void D0(double aU,
                   out gp_Pnt2d aP2D)
        {
            gp_Vec2d aV1;
            //
            myCurveBase.D1(aU, out aP2D, out aV1);
            //
            if (myOffsetFlag)
            {
                int aIndex, aMaxDegree;
                double aA, aB, aR, aRes;
                //
                aMaxDegree = 9;
                aIndex = 2;
                aRes = gp.Resolution();
                //
                while (aV1.Magnitude() <= aRes && aIndex <= aMaxDegree)
                {
                    aV1 = myCurveBase.DN(aU, aIndex);
                    ++aIndex;
                }
                //
                aA = aV1.Y();
                aB = -aV1.X();
                aR = Math.Sqrt(aA * aA + aB * aB);
                if (aR <= aRes)
                {
                    myErrorStatus = 13;
                    return;
                }
                //
                aR = myOffsetBase / aR;
                aA = aA * aR;
                aB = aB * aR;
                aP2D.SetCoord(aP2D.X() + aA, aP2D.Y() + aB);
            }
            //
        }

        public void Perform()
        {
            Clear();
            // 
            myErrorStatus = 0;
            //
            CheckData();
            if (myErrorStatus != 0)
            {
                return;
            }
            //
            if (myT1 == myT2)
            {
                PerformOnePoint();
                return;
            }
            //
            GetInfoBase();
            if (myErrorStatus != 0)
            {
                return;
            }
            // 
            if (myTypeBase == GeomAbs_CurveType.GeomAbs_Line ||
                myTypeBase == GeomAbs_CurveType.GeomAbs_Circle ||
                myTypeBase == GeomAbs_CurveType.GeomAbs_Ellipse ||
                myTypeBase == GeomAbs_CurveType.GeomAbs_Parabola ||
                myTypeBase == GeomAbs_CurveType.GeomAbs_Hyperbola)
            { // LineConic

                PerformLineConic();
            }
            else if (myTypeBase == GeomAbs_CurveType.GeomAbs_BezierCurve)
            { // Bezier
                throw new NotImplementedException();
                //PerformBezier();
            }
            else if (myTypeBase == GeomAbs_CurveType.GeomAbs_BSplineCurve)
            { //B-Spline
                throw new NotImplementedException();
                //PerformBSpline();
            }
            else
            {
                myErrorStatus = 11; // unknown type base
            }
        }

        public void PerformOnePoint()
        {
            gp_Pnt2d aP2D;
            //
            myCurve.D0(myT1, out aP2D);
            myBox.Add(aP2D);
        }

        public void SetCurve(Geom2d_Curve aC2D)
        {
            myCurve = aC2D;
        }

        Geom2d_Curve myCurve;
        Bnd_Box2d myBox = new Bnd_Box2d();
        int myErrorStatus;
        Geom2d_Curve myCurveBase;
        double myOffsetBase;
        bool myOffsetFlag;
        double myT1;
        double myT2;
        GeomAbs_CurveType myTypeBase;
        public void SetRange(double aT1,

                     double aT2)
        {
            myT1 = aT1;
            myT2 = aT2;
        }
        public void Clear()
        {
            myBox.SetVoid();
            //
            myErrorStatus = -1;
            myTypeBase = GeomAbs_CurveType.GeomAbs_OtherCurve;
            myOffsetBase = 0.0;
            myOffsetFlag = false;
        }
    }
}

