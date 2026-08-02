using TKernel;
using TKG3d;
using TKMath;
using TKMesh;
using TKService;

namespace TKV3d
{
    //! A framework to provide display of any curve with
    //! respect to the maximal chordal deviation defined in
    //! the Prs3d_Drawer attributes manager.
    public class StdPrs_DeflectionCurve : Prs3d_Root
    {

        static bool FindLimits(Adaptor3d_Curve aCurve,
                                   double aLimit,
                                   out double First,
                                   out double Last)
        {
            First = aCurve.FirstParameter();
            Last = aCurve.LastParameter();
            bool firstInf = Precision.IsNegativeInfinite(First);
            bool lastInf = Precision.IsPositiveInfinite(Last);

            if (firstInf || lastInf)
            {
                gp_Pnt P1 = new gp_Pnt(), P2 = new gp_Pnt();
                double delta = 1;
                int count = 0;
                if (firstInf && lastInf)
                {
                    do
                    {
                        if (count++ == 100000)
                            return false;

                        delta *= 2;
                        First = -delta;
                        Last = delta;
                        aCurve.D0(First, ref P1);
                        aCurve.D0(Last, ref P2);
                    } while (P1.Distance(P2) < aLimit);
                }
                else if (firstInf)
                {
                    aCurve.D0(Last, ref P2);
                    do
                    {
                        if (count++ == 100000) return false;
                        delta *= 2;
                        First = Last - delta;
                        aCurve.D0(First, ref P1);
                    } while (P1.Distance(P2) < aLimit);
                }
                else if (lastInf)
                {
                    aCurve.D0(First, ref P1);
                    do
                    {
                        if (count++ == 100000) return false;
                        delta *= 2;
                        Last = First + delta;
                        aCurve.D0(Last, ref P2);
                    } while (P1.Distance(P2) < aLimit);
                }
            }
            return true;
        }

        //! adds to the presentation aPresentation the drawing of the curve
        //! aCurve with respect to the maximal chordial deviation aDeflection.
        //! The aspect is the current aspect
        //! The drawing will be limited between the points of parameter U1 and U2.
        //! Points give a sequence of curve points.
        //! If drawCurve equals Standard_False the curve will not be displayed,
        //! it is used if the curve is a part of some shape and PrimitiveArray
        //! visualization approach is activated (it is activated by default).
        public static  void Add(Prs3d_Presentation aPresentation,
                                    Adaptor3d_Curve aCurve,
                                     double U1,
                                     double U2,
                                     double aDeflection,
                                    TColgp_SequenceOfPnt Points,
                                     double anAngle,
                                     bool theToDrawCurve)
        {
            Graphic3d_Group aGroup = null;
            if (theToDrawCurve)
            {
                aGroup = aPresentation.CurrentGroup();
            }

            drawCurve(aCurve, aGroup, aDeflection, anAngle, U1, U2, Points);
        }


        //! adds to the presentation aPresentation the drawing of the curve
        //! aCurve with respect to the maximal chordial deviation aDeflection.
        //! The aspect is the current aspect
        //! Points give a sequence of curve points.
        //! If drawCurve equals Standard_False the curve will not be displayed,
        //! it is used if the curve is a part of some shape and PrimitiveArray
        //! visualization approach is activated (it is activated by default).
        public static void Add(Prs3d_Presentation aPresentation,
            Adaptor3d_Curve aCurve,
            double aDeflection,
            Prs3d_Drawer aDrawer,
            TColgp_SequenceOfPnt Points,
             bool theToDrawCurve = true)
        {
            double V1, V2;
            if (!FindLimits(aCurve, aDrawer.MaximalParameterValue(), out V1, out V2))
            {
                return;
            }

            Graphic3d_Group aGroup = null;
            if (theToDrawCurve)
            {
                aGroup = aPresentation.CurrentGroup();
            }
            drawCurve(aCurve, aGroup, aDeflection, aDrawer.DeviationAngle(), V1, V2, Points);
        }

        static void drawCurve(Adaptor3d_Curve aCurve,
                       Graphic3d_Group aGroup,
                       double TheDeflection,
                       double anAngle,
                       double U1,
                       double U2,
                       TColgp_SequenceOfPnt Points)
        {
            switch (aCurve._GetType())
            {
                case GeomAbs_CurveType.GeomAbs_Line:
                    {
                        gp_Pnt p1 = aCurve.Value(U1);
                        gp_Pnt p2 = aCurve.Value(U2);
                        Points.Append(p1);
                        Points.Append(p2);
                        if (aGroup != null)
                        {
                            Graphic3d_ArrayOfSegments aPrims = new Graphic3d_ArrayOfSegments(2);
                            aPrims.AddVertex(p1);
                            aPrims.AddVertex(p2);
                            aGroup.AddPrimitiveArray(aPrims);
                        }
                        break;
                    }
                default:
                    {
                        int nbinter = aCurve.NbIntervals(GeomAbs_Shape.GeomAbs_C1);
                        TColStd_Array1OfReal T = new TColStd_Array1OfReal(1, nbinter + 1);
                        aCurve.Intervals(T, GeomAbs_Shape.GeomAbs_C1);

                        double theU1, theU2;
                        int NumberOfPoints, i, j;
                        TColgp_SequenceOfPnt SeqP = new TColgp_SequenceOfPnt();

                        for (j = 1; j <= nbinter; j++)
                        {
                            theU1 = T[j]; theU2 = T[j + 1];
                            if (theU2 > U1 && theU1 < U2)
                            {
                                theU1 = Math.Max(theU1, U1);
                                theU2 = Math.Min(theU2, U2);

                                GCPnts_TangentialDeflection Algo = new GCPnts_TangentialDeflection(aCurve, theU1, theU2, anAngle, TheDeflection);
                                NumberOfPoints = Algo.NbPoints();

                                if (NumberOfPoints > 0)
                                {
                                    for (i = 1; i <= NumberOfPoints; i++)
                                        SeqP.Append(Algo.Value(i));
                                }
                            }
                        }

                        Graphic3d_ArrayOfPolylines aPrims = null;
                        if (aGroup != null)
                            aPrims = new Graphic3d_ArrayOfPolylines(SeqP.Length());

                        for (i = 1; i <= SeqP.Length(); i++)
                        {
                            gp_Pnt p = SeqP.Value(i);
                            Points.Append(p);
                            if (aGroup != null)
                            {
                                aPrims.AddVertex(p);
                            }
                        }
                        if (aGroup != null)
                        {
                            aGroup.AddPrimitiveArray(aPrims);
                        }
                    }
                    break;
            }
        }

    }
}

