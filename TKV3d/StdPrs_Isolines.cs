using OCCPort;
using OCCPort.Common;
using TKBRep;
using TKernel;
using TKG2d;
using TKG3d;
using TKGeomAlgo;
using TKGeomBase;
using TKMath;
using TKV3d;

namespace TKV3d
{
    //! Tool for computing isoline representation for a face or surface.
    //! Depending on a flags set to the given Prs3d_Drawer instance, on-surface (is used
    //! by default) or on-triangulation isoline builder algorithm will be used.
    //! If the given shape is not triangulated, on-surface isoline builder will be applied
    //! regardless of Prs3d_Drawer flags.
    public class StdPrs_Isolines : Prs3d_Root
    {

        //! Computes isolines presentation for a TopoDS face.
        //! This method chooses proper version of isoline builder algorithm : on triangulation
        //! or surface depending on the flag passed from Prs3d_Drawer attributes.
        //! This method is a default way to display isolines for a given TopoDS face.
        //! @param theFace [in] the face.
        //! @param theDrawer [in] the display settings.
        //! @param theDeflection [in] the deflection for isolines-on-surface version.
        public static void Add(TopoDS_Face theFace,
                   Prs3d_Drawer theDrawer,
                   double theDeflection,
                   Prs3d_NListOfSequenceOfPnt theUPolylines,
                   Prs3d_NListOfSequenceOfPnt theVPolylines)
        {
            if (theDrawer.IsoOnTriangulation() && StdPrs_ToolTriangulatedShape.IsTriangulated(theFace))
            {
                AddOnTriangulation(theFace, theDrawer, theUPolylines, theVPolylines);
            }
            else
            {
                AddOnSurface(theFace, theDrawer, theDeflection, theUPolylines, theVPolylines);
            }
        }

        //==================================================================
        // function : UVIsoParameters
        // purpose  :
        //==================================================================
        public static void UVIsoParameters(TopoDS_Face theFace,
                                         int theNbIsoU,
                                         int theNbIsoV,
                                         double theUVLimit,
                                         TColStd_SequenceOfReal theUIsoParams,
                                         TColStd_SequenceOfReal theVIsoParams,
                                      ref double theUmin,
                                     ref double theUmax,
                                     ref double theVmin,
                                    ref double theVmax)
        {

            TopLoc_Location aLocation = null;
            Geom_Surface aSurface = BRep_Tool.Surface(theFace, out aLocation);
            if (aSurface == null)
            {
                return;
            }

            BRepTools.UVBounds(theFace, ref theUmin, ref theUmax, ref theVmin, ref theVmax);

            double aUmin = theUmin;
            double aUmax = theUmax;
            double aVmin = theVmin;
            double aVmax = theVmax;

            if (Precision.IsInfinite(aUmin))
                aUmin = -theUVLimit;
            if (Precision.IsInfinite(aUmax))
                aUmax = theUVLimit;
            if (Precision.IsInfinite(aVmin))
                aVmin = -theUVLimit;
            if (Precision.IsInfinite(aVmax))
                aVmax = theUVLimit;


            bool isUClosed = aSurface.IsUClosed();
            bool isVClosed = aSurface.IsVClosed();

            if (!isUClosed)
            {
                aUmin = aUmin + (aUmax - aUmin) / 1000.0;
                aUmax = aUmax - (aUmax - aUmin) / 1000.0;
            }

            if (!isVClosed)
            {
                aVmin = aVmin + (aVmax - aVmin) / 1000.0;
                aVmax = aVmax - (aVmax - aVmin) / 1000.0;
            }

            double aUstep = (aUmax - aUmin) / (1 + theNbIsoU);
            double aVstep = (aVmax - aVmin) / (1 + theNbIsoV);

            for (int anIso = 1; anIso <= theNbIsoU; ++anIso)
            {
                theUIsoParams.Append(aUmin + aUstep * anIso);
            }

            for (int anIso = 1; anIso <= theNbIsoV; ++anIso)
            {
                theVIsoParams.Append(aVmin + aVstep * anIso);
            }
        }

        //! Computes isolines on surface and adds them to presentation.
        //! @param theFace [in] the face
        //! @param theDrawer [in] the display settings
        //! @param theDeflection [in] the deflection value
        //! @param theUPolylines [out] the sequence of result polylines
        //! @param theVPolylines [out] the sequence of result polylines
        public static void AddOnSurface(TopoDS_Face theFace,
                                             Prs3d_Drawer theDrawer,
                                             double theDeflection,
                                            Prs3d_NListOfSequenceOfPnt theUPolylines,
                                            Prs3d_NListOfSequenceOfPnt theVPolylines)
        {
            int aNbIsoU = theDrawer.UIsoAspect().Number();
            int aNbIsoV = theDrawer.VIsoAspect().Number();
            if (aNbIsoU < 1 && aNbIsoV < 1)
            {
                return;
            }

            // Evaluate parameters for uv isolines.
            TColStd_SequenceOfReal aUIsoParams = new TColStd_SequenceOfReal(), aVIsoParams = new TColStd_SequenceOfReal();
            double aUmin = 0.0, aUmax = 0.0, aVmin = 0.0, aVmax = 0.0;
            UVIsoParameters(theFace, aNbIsoU, aNbIsoV, theDrawer.MaximalParameterValue(), aUIsoParams, aVIsoParams,
                            ref aUmin, ref aUmax, ref aVmin, ref aVmax);

            BRepAdaptor_Surface aSurface = new(theFace);
            addOnSurface(new BRepAdaptor_Surface(aSurface),
                          theDrawer,
                          theDeflection,
                          aUIsoParams,
                          aVIsoParams,
                          theUPolylines,
                          theVPolylines);
        }

        //! Computes isolines on surface.
        //! @param theSurface [in] the surface
        //! @param theDrawer [in] the display settings
        //! @param theDeflection [in] the deflection value
        //! @param theUIsoParams [in] the parameters of u isolines to compute
        //! @param theVIsoParams [in] the parameters of v isolines to compute
        //! @param theUPolylines [out] the sequence of result polylines
        //! @param theVPolylines [out] the sequence of result polylines
        public static void addOnSurface(BRepAdaptor_Surface theSurface,
                                             Prs3d_Drawer theDrawer,
                                             double theDeflection,
                                             TColStd_SequenceOfReal theUIsoParams,
                                             TColStd_SequenceOfReal theVIsoParams,
                                            Prs3d_NListOfSequenceOfPnt theUPolylines,
                                            Prs3d_NListOfSequenceOfPnt theVPolylines)
        {
            // Choose a deflection for sampling edge uv curves.
            double aUVLimit = theDrawer.MaximalParameterValue();
            double aUmin = Math.Max(theSurface.FirstUParameter(), -aUVLimit);
            double aUmax = Math.Min(theSurface.LastUParameter(), aUVLimit);
            double aVmin = Math.Max(theSurface.FirstVParameter(), -aUVLimit);
            double aVmax = Math.Min(theSurface.LastVParameter(), aUVLimit);
            double aSamplerDeflection = Math.Max(aUmax - aUmin, aVmax - aVmin) * theDrawer.DeviationCoefficient();
            double aHatchingTolerance = Standard_Real.RealLast();

            try
            {
                //OCC_CATCH_SIGNALS
                // Determine edge points for trimming uv hatch region.
                NCollection_Sequence<gp_Pnt2d> aTrimPoints = new NCollection_Sequence<gp_Pnt2d>();
                StdPrs_ToolRFace anEdgeTool = new(theSurface);
                for (anEdgeTool.Init(); anEdgeTool.More(); anEdgeTool.Next())
                {
                    TopAbs_Orientation anOrientation = anEdgeTool.Orientation();
                    Adaptor2d_Curve2d anEdgeCurve = anEdgeTool.Value();
                    if (anEdgeCurve._GetType() != GeomAbs_CurveType.GeomAbs_Line)
                    {
                        GCPnts_QuasiUniformDeflection aSampler = new(anEdgeCurve, aSamplerDeflection);
                        if (!aSampler.IsDone())
                        {
                            //# ifdef OCCT_DEBUG
                            //     std::cout << "Cannot evaluate curve on surface" << std::endl;
                            //#endif
                            continue;
                        }

                        int aNumberOfPoints = aSampler.NbPoints();
                        if (aNumberOfPoints < 2)
                        {
                            continue;
                        }

                        for (int anI = 1; anI < aNumberOfPoints; ++anI)
                        {
                            gp_Pnt2d aP1 = new(aSampler.Value(anI).X(), aSampler.Value(anI).Y());
                            gp_Pnt2d aP2 = new(aSampler.Value(anI + 1).X(), aSampler.Value(anI + 1).Y());

                            aHatchingTolerance = Math.Min(aP1.SquareDistance(aP2), aHatchingTolerance);

                            aTrimPoints.Append(anOrientation == TopAbs_Orientation.TopAbs_FORWARD ? aP1 : aP2);
                            aTrimPoints.Append(anOrientation == TopAbs_Orientation.TopAbs_FORWARD ? aP2 : aP1);
                        }
                    }
                    else
                    {
                        double aU1 = anEdgeCurve.FirstParameter();
                        double aU2 = anEdgeCurve.LastParameter();

                        // MSV 17.08.06 OCC13144: U2 occurred less than U1, to overcome it
                        // ensure that distance U2-U1 is not greater than aLimit*2,
                        // if greater then choose an origin and use aLimit to define
                        // U1 and U2 anew.
                        double anOrigin = 0.0;

                        if (!Precision.IsNegativeInfinite(aU1) || !Precision.IsPositiveInfinite(aU2))
                        {
                            if (Precision.IsNegativeInfinite(aU1))
                            {
                                anOrigin = aU2 - aUVLimit;
                            }
                            else if (Precision.IsPositiveInfinite(aU2))
                            {
                                anOrigin = aU1 + aUVLimit;
                            }
                            else
                            {
                                anOrigin = (aU1 + aU2) * 0.5;
                            }
                        }

                        aU1 = Math.Max(anOrigin - aUVLimit, aU1);
                        aU2 = Math.Min(anOrigin + aUVLimit, aU2);

                        gp_Pnt2d aP1 = anEdgeCurve.Value(aU1);
                        gp_Pnt2d aP2 = anEdgeCurve.Value(aU2);

                        aHatchingTolerance = Math.Min(aP1.SquareDistance(aP2), aHatchingTolerance);

                        aTrimPoints.Append(anOrientation == TopAbs_Orientation.TopAbs_FORWARD ? aP1 : aP2);
                        aTrimPoints.Append(anOrientation == TopAbs_Orientation.TopAbs_FORWARD ? aP2 : aP1);
                    }
                }

                // re-calculate UV-range basing on p-curves tessellation
                Bnd_Range aTrimU = new Bnd_Range(), aTrimV = new Bnd_Range();
                for (int anI = 1; anI <= aTrimPoints.Length(); ++anI)
                {
                    gp_Pnt2d aTrimPnt = aTrimPoints.Value(anI);
                    aTrimU.Add(aTrimPnt.X());
                    aTrimV.Add(aTrimPnt.Y());
                }
                // ignore p-curves tessellation under sampler deflection - it might clamp range
                if (!aTrimU.IsVoid() && aTrimU.Delta() <= aSamplerDeflection)
                {
                    aTrimU.SetVoid();
                }
                if (!aTrimV.IsVoid() && aTrimV.Delta() <= aSamplerDeflection)
                {
                    aTrimV.SetVoid();
                }

                // Compute a hatching tolerance.
                aHatchingTolerance *= 0.1;
                aHatchingTolerance = Math.Max(Precision.Confusion(), aHatchingTolerance);
                aHatchingTolerance = Math.Min(1.0E-5, aHatchingTolerance);

                // Load isolines into hatcher.
                Hatch_Hatcher aHatcher = new(aHatchingTolerance, anEdgeTool.IsOriented());

                for (int anIso = 1; anIso <= theUIsoParams.Length(); ++anIso)
                {
                    double anIsoParamU = theUIsoParams.Value(anIso);
                    if (aTrimU.IsVoid()
                    || !aTrimU.IsOut(anIsoParamU))
                    {
                        aHatcher.AddXLine(anIsoParamU);
                    }
                }
                for (int anIso = 1; anIso <= theVIsoParams.Length(); ++anIso)
                {
                    double anIsoParamV = theVIsoParams.Value(anIso);
                    if (aTrimV.IsVoid()
                    || !aTrimV.IsOut(anIsoParamV))
                    {
                        aHatcher.AddYLine(anIsoParamV);
                    }
                }

                // Trim hatching region.
                for (int anI = 1; anI <= aTrimPoints.Length(); anI += 2)
                {
                    aHatcher.Trim(aTrimPoints[(anI)], aTrimPoints[(anI + 1)]);
                }

                // Use surface definition for evaluation of Bezier, B-spline surface.
                // Use isoline adapter for other types of surfaces.
                GeomAbs_SurfaceType aSurfType = theSurface._GetType();
                Geom_Surface aBSurface = null;
                GeomAdaptor_Curve aBSurfaceCurve = new GeomAdaptor_Curve();
                Adaptor3d_IsoCurve aCanonicalCurve = new Adaptor3d_IsoCurve();
                //if (aSurfType == GeomAbs_SurfaceType.GeomAbs_BezierSurface)
                //{
                //    aBSurface = theSurface.Bezier();
                //}
                //else if (aSurfType == GeomAbs_SurfaceType.GeomAbs_BSplineSurface)
                //{
                //    aBSurface = theSurface.BSpline();
                //}
                //else
                {
                    aCanonicalCurve.Load(theSurface);
                }

                // For each isoline: compute its segments.
                for (int anI = 1; anI <= aHatcher.NbLines(); anI++)
                {
                    double anIsoParam = aHatcher.Coordinate(anI);
                    bool isIsoU = aHatcher.IsXLine(anI);

                    // For each isoline's segment: evaluate its points.
                    for (int aJ = 1; aJ <= aHatcher.NbIntervals(anI); aJ++)
                    {
                        double aSegmentP1 = aHatcher.Start(anI, aJ);
                        double aSegmentP2 = aHatcher.End(anI, aJ);

                        if (aBSurface != null)
                        {
                            aBSurfaceCurve.Load(isIsoU ? aBSurface.UIso(anIsoParam) : aBSurface.VIso(anIsoParam));

                            findLimits(aBSurfaceCurve, aUVLimit, ref aSegmentP1, ref aSegmentP2);

                            if (aSegmentP2 - aSegmentP1 <= Precision.Confusion())
                            {
                                continue;
                            }
                        }
                        else
                        {
                            aCanonicalCurve.Load(isIsoU ? GeomAbs_IsoType.GeomAbs_IsoU : GeomAbs_IsoType.GeomAbs_IsoV, anIsoParam, aSegmentP1, aSegmentP2);

                            findLimits(aCanonicalCurve, aUVLimit, ref aSegmentP1, ref aSegmentP2);

                            if (aSegmentP2 - aSegmentP1 <= Precision.Confusion())
                            {
                                continue;
                            }
                        }
                        Adaptor3d_Curve aCurve = aBSurface == null ? (Adaptor3d_Curve)aCanonicalCurve
                          : (Adaptor3d_Curve)aBSurfaceCurve;

                        NCollection_Sequence<gp_Pnt> aPoints = new NCollection_Sequence<gp_Pnt>();
                        StdPrs_DeflectionCurve.Add(null,
                                                     aCurve,
                                                     aSegmentP1,
                                                     aSegmentP2,
                                                     theDeflection,
                                                     aPoints.ChangeSequence(),
                                                     theDrawer.DeviationAngle(),
                                                     false);
                        if (aPoints.IsEmpty())
                        {
                            continue;
                        }

                        if (isIsoU)
                        {
                            theUPolylines.Append(aPoints);
                        }
                        else
                        {
                            theVPolylines.Append(aPoints);
                        }
                    }
                }
            }
            catch (Standard_Failure ex)
            {
                // ...
            }
        }

        //! Reorder and adjust to the limit a curve's parameter values.
        //! @param theCurve [in] the curve.
        //! @param theLimit [in] the parameter limit value.
        //! @param theFirst [in/out] the first parameter value.
        //! @param theLast  [in/out] the last parameter value.
        static void findLimits(Adaptor3d_Curve theCurve,
                           double theLimit,
                         ref double theFirst,
                         ref double theLast)
        {
            theFirst = Math.Max(theCurve.FirstParameter(), theFirst);
            theLast = Math.Min(theCurve.LastParameter(), theLast);

            bool isFirstInf = Precision.IsNegativeInfinite(theFirst);
            bool isLastInf = Precision.IsPositiveInfinite(theLast);

            if (!isFirstInf && !isLastInf)
            {
                return;
            }

            gp_Pnt aP1 = new gp_Pnt(), aP2 = new gp_Pnt();
            double aDelta = 1.0;
            if (isFirstInf && isLastInf)
            {
                do
                {
                    aDelta *= 2.0;
                    theFirst = -aDelta;
                    theLast = aDelta;
                    theCurve.D0(theFirst, ref aP1);
                    theCurve.D0(theLast, ref aP2);
                }
                while (aP1.Distance(aP2) < theLimit);
            }
            else if (isFirstInf)
            {
                theCurve.D0(theLast, ref aP2);
                do
                {
                    aDelta *= 2.0;
                    theFirst = theLast - aDelta;
                    theCurve.D0(theFirst, ref aP1);
                }
                while (aP1.Distance(aP2) < theLimit);
            }
            else if (isLastInf)
            {
                theCurve.D0(theFirst, ref aP1);
                do
                {
                    aDelta *= 2.0;
                    theLast = theFirst + aDelta;
                    theCurve.D0(theLast, ref aP2);
                }
                while (aP1.Distance(aP2) < theLimit);
            }
        }



        //==================================================================
        // function : AddOnTriangulation
        // purpose  :
        //==================================================================
        public static void AddOnTriangulation(TopoDS_Face theFace,
                                           Prs3d_Drawer theDrawer,
                                           Prs3d_NListOfSequenceOfPnt theUPolylines,
                                           Prs3d_NListOfSequenceOfPnt theVPolylines)
        {
            int aNbIsoU = theDrawer.UIsoAspect().Number();
            int aNbIsoV = theDrawer.VIsoAspect().Number();
            if (aNbIsoU < 1 && aNbIsoV < 1)
            {
                return;
            }

            // Evaluate parameters for uv isolines.
            TColStd_SequenceOfReal aUIsoParams = new TColStd_SequenceOfReal();
            TColStd_SequenceOfReal aVIsoParams = new TColStd_SequenceOfReal();
            double aUmin = 0.0, aUmax = 0.0, aVmin = 0.0, aVmax = 0.0;
            UVIsoParameters(theFace, aNbIsoU, aNbIsoV, theDrawer.MaximalParameterValue(), aUIsoParams, aVIsoParams,
                          ref aUmin, ref aUmax, ref aVmin, ref aVmax);

            // Access surface definition.
            TopLoc_Location aLocSurface;
            Geom_Surface aSurface = BRep_Tool.Surface(theFace, out aLocSurface);
            if (aSurface == null)
            {
                return;
            }

            // Access triangulation.
            TopLoc_Location aLocTriangulation = null;
            Poly_Triangulation aTriangulation = BRep_Tool.Triangulation(theFace, ref aLocTriangulation);
            if (aTriangulation == null)
            {
                return;
            }

            // Setup equal location for surface and triangulation.
            if (!aLocTriangulation.IsEqual(aLocSurface))
            {
                // aSurface = (Geom_Surface)(
                //aSurface.Transformed((aLocSurface / aLocTriangulation).Transformation()));
            }

            var TheType = aSurface.DynamicType();
            if (TheType == typeof(Geom_OffsetSurface))
            {
                double u1, u2, v1, v2;
                aSurface.Bounds(out u1, out u2, out v1, out v2);
                //Isolines of Offset surfaces are calculated by approximation and
                //cannot be calculated for infinite limits.
                if (Precision.IsInfinite(u1) || Precision.IsInfinite(u2) ||
                  Precision.IsInfinite(v1) || Precision.IsInfinite(v2))
                {
                    u1 = Math.Max(aUmin, u1);
                    u2 = Math.Min(aUmax, u2);
                    v1 = Math.Max(aVmin, v1);
                    v2 = Math.Min(aVmax, v2);
                    //aSurface = new Geom_RectangularTrimmedSurface(aSurface, u1, u2, v1, v2);
                }
            }

            addOnTriangulation(aTriangulation, aSurface, aLocTriangulation, aUIsoParams, aVIsoParams, theUPolylines, theVPolylines);
        }

        static gp_Lin2d isoU(double theU) { return new gp_Lin2d(new gp_Pnt2d(theU, 0.0), gp.DY2d()); }
        static gp_Lin2d isoV(double theV) { return new gp_Lin2d(new gp_Pnt2d(0.0, theV), gp.DX2d()); }



        //==================================================================
        // function : FindSegmentOnTriangulation
        // purpose  :
        //==================================================================
        static bool findSegmentOnTriangulation(Geom_Surface theSurface,
                                                              bool theIsU,
                                                              gp_Lin2d theIsoline,
                                                              gp_Pnt[] theNodesXYZ,
                                                              gp_Pnt2d[] theNodesUV,
                                                              SegOnIso theSegment)
        {
            int aNPoints = 0;

            for (int aLinkIter = 0; aLinkIter < 3 && aNPoints < 2; ++aLinkIter)
            {
                // ...
                // Check that uv isoline crosses the triangulation link in parametric space
                // ...

                gp_Pnt2d aNodeUV1 = theNodesUV[aLinkIter];
                gp_Pnt2d aNodeUV2 = theNodesUV[(aLinkIter + 1) % 3];
                gp_Pnt aNode1 = theNodesXYZ[aLinkIter];
                gp_Pnt aNode2 = theNodesXYZ[(aLinkIter + 1) % 3];

                // Compute distance of uv points to isoline taking into consideration their relative
                // location against the isoline (left or right). Null value for a node means that the
                // isoline crosses the node. Both positive or negative means that the isoline does not
                // cross the segment.
                bool isLeftUV1 = (theIsoline.Direction().XY() ^ new gp_Vec2d(theIsoline.Location(), aNodeUV1).XY()) > 0.0;
                bool isLeftUV2 = (theIsoline.Direction().XY() ^ new gp_Vec2d(theIsoline.Location(), aNodeUV2).XY()) > 0.0;
                double aDistanceUV1 = isLeftUV1 ? theIsoline.Distance(aNodeUV1) : -theIsoline.Distance(aNodeUV1);
                double aDistanceUV2 = isLeftUV2 ? theIsoline.Distance(aNodeUV2) : -theIsoline.Distance(aNodeUV2);

                // Isoline crosses first point of an edge.
                if (Math.Abs(aDistanceUV1) < Precision.PConfusion())
                {
                    theSegment[aNPoints].Param = theIsU ? aNodeUV1.Y() : aNodeUV1.X();
                    theSegment[aNPoints].Pnt = aNode1;
                    ++aNPoints;
                    continue;
                }

                // Isoline crosses second point of an edge.
                if (Math.Abs(aDistanceUV2) < Precision.PConfusion())
                {
                    theSegment[aNPoints].Param = theIsU ? aNodeUV2.Y() : aNodeUV2.X();
                    theSegment[aNPoints].Pnt = aNode2;

                    ++aNPoints;
                    ++aLinkIter;
                    continue;
                }

                // Isoline does not cross the triangle link.
                if (aDistanceUV1 * aDistanceUV2 > 0.0)
                {
                    continue;
                }

                // Isoline crosses degenerated link.
                if (aNode1.SquareDistance(aNode2) < Precision.PConfusion())
                {
                    theSegment[aNPoints].Param = theIsU ? aNodeUV1.Y() : aNodeUV1.X();
                    theSegment[aNPoints].Pnt = aNode1;
                    ++aNPoints;
                    continue;
                }

                // ...
                // Derive cross-point from parametric coordinates
                // ...

                double anAlpha = Math.Abs(aDistanceUV1) / (Math.Abs(aDistanceUV1) + Math.Abs(aDistanceUV2));

                gp_Pnt aCross = new gp_Pnt(0.0, 0.0, 0.0);
                double aCrossU = aNodeUV1.X() + anAlpha * (aNodeUV2.X() - aNodeUV1.X());
                double aCrossV = aNodeUV1.Y() + anAlpha * (aNodeUV2.Y() - aNodeUV1.Y());
                double aCrossParam = theIsU ? aCrossV : aCrossU;
                if (theSurface == null)
                {
                    // Do linear interpolation of point coordinates using triangulation nodes.
                    aCross.SetX(aNode1.X() + anAlpha * (aNode2.X() - aNode1.X()));
                    aCross.SetY(aNode1.Y() + anAlpha * (aNode2.Y() - aNode1.Y()));
                    aCross.SetZ(aNode1.Z() + anAlpha * (aNode2.Z() - aNode1.Z()));
                }
                else
                {
                    // Do linear interpolation of point coordinates by triangulation nodes.
                    // Get 3d point on surface.
                    Geom_Curve anIso1 = null, anIso2 = null;
                    double aPntOnNode1Iso = 0.0;
                    double aPntOnNode2Iso = 0.0;
                    double aPntOnNode3Iso = 0.0;

                    if (theIsoline.Direction().X() == 0.0)
                    {
                        aPntOnNode1Iso = aNodeUV1.X();
                        aPntOnNode2Iso = aNodeUV2.X();
                        aPntOnNode3Iso = aCrossU;
                        anIso1 = theSurface.VIso(aNodeUV1.Y());
                        anIso2 = theSurface.VIso(aNodeUV2.Y());
                    }
                    else if (theIsoline.Direction().Y() == 0.0)
                    {
                        aPntOnNode1Iso = aNodeUV1.Y();
                        aPntOnNode2Iso = aNodeUV2.Y();
                        aPntOnNode3Iso = aCrossV;
                        anIso1 = theSurface.UIso(aNodeUV1.X());
                        anIso2 = theSurface.UIso(aNodeUV2.X());
                    }

                    GeomAdaptor_Curve aCurveAdaptor1 = new GeomAdaptor_Curve(anIso1);
                    GeomAdaptor_Curve aCurveAdaptor2 = new GeomAdaptor_Curve(anIso2);
                    double aLength1 = GCPnts_AbscissaPoint.Length(aCurveAdaptor1, aPntOnNode1Iso, aPntOnNode3Iso, 1e-2);
                    double aLength2 = GCPnts_AbscissaPoint.Length(aCurveAdaptor2, aPntOnNode2Iso, aPntOnNode3Iso, 1e-2);
                    if (Math.Abs(aLength1) < Precision.Confusion() || Math.Abs(aLength2) < Precision.Confusion())
                    {
                        theSegment[aNPoints].Param = aCrossParam;
                        theSegment[aNPoints].Pnt = (aNode2.XYZ() - aNode1.XYZ()) * anAlpha + aNode1.XYZ();
                        ++aNPoints;
                        continue;
                    }

                    aCross = (aNode2.XYZ() - aNode1.XYZ()) * (aLength1 / (aLength1 + aLength2)) + aNode1.XYZ();
                }

                theSegment[aNPoints].Param = aCrossParam;
                theSegment[aNPoints].Pnt = aCross;
                ++aNPoints;
            }

            if (aNPoints != 2
             || Math.Abs(theSegment[1].Param - theSegment[0].Param) <= Precision.PConfusion())
            {
                return false;
            }

            if (theSegment[1].Param < theSegment[0].Param)
            {
                //Std.swap(ref theSegment[0], theSegment[1]);
                (theSegment[0], theSegment[1]) = (theSegment[1], theSegment[0]);
            }
            return true;
        }


        //==================================================================
        // function : addOnTriangulation
        // purpose  :
        //==================================================================
        private static void addOnTriangulation(Poly_Triangulation theTriangulation,
                                                           Geom_Surface theSurface,
                                                           TopLoc_Location theLocation,
                                                           TColStd_SequenceOfReal theUIsoParams,
                                                           TColStd_SequenceOfReal theVIsoParams,
                                                           Prs3d_NListOfSequenceOfPnt theUPolylines,
                                                           Prs3d_NListOfSequenceOfPnt theVPolylines)
        {
            for (int anUVIter = 0; anUVIter < 2; ++anUVIter)
            {
                bool isUIso = anUVIter == 0;
                TColStd_SequenceOfReal anIsoParams = isUIso ? theUIsoParams : theVIsoParams;
                int aNbIsolines = anIsoParams.Length();
                if (aNbIsolines == 0)
                {
                    continue;
                }

                SeqOfVecOfSegments aPolylines = new SeqOfVecOfSegments();
                TColStd_Array1OfInteger anIsoIndexes = new TColStd_Array1OfInteger(1, aNbIsolines);
                anIsoIndexes.Init(-1);
                for (int anIsoIdx = 1; anIsoIdx <= aNbIsolines; ++anIsoIdx)
                {
                    gp_Lin2d anIsolineUV = isUIso ? isoU(anIsoParams.Value(anIsoIdx)) : isoV(anIsoParams.Value(anIsoIdx));
                    VecOfSegments anIsoPnts = new VecOfSegments();
                    if (anIsoIndexes.Value(anIsoIdx) != -1)
                    {
                        anIsoPnts = aPolylines.ChangeValue(anIsoIndexes.Value(anIsoIdx));
                    }

                    for (int aTriIter = 1; aTriIter <= theTriangulation.NbTriangles(); ++aTriIter)
                    {
                        int[] aNodeIdxs = new int[3];
                        theTriangulation.Triangle(aTriIter).Get(out aNodeIdxs[0], out aNodeIdxs[1], out aNodeIdxs[2]);
                        gp_Pnt[] aNodesXYZ = { theTriangulation.Node (aNodeIdxs[0]),
                                      theTriangulation.Node (aNodeIdxs[1]),
                                      theTriangulation.Node (aNodeIdxs[2]) };
                        gp_Pnt2d[] aNodesUV = { theTriangulation.UVNode (aNodeIdxs[0]),
                                       theTriangulation.UVNode (aNodeIdxs[1]),
                                       theTriangulation.UVNode (aNodeIdxs[2]) };

                        // Find intersections with triangle in uv space and its projection on triangulation.
                        SegOnIso aSegment = new SegOnIso();
                        if (!findSegmentOnTriangulation(theSurface, isUIso, anIsolineUV, aNodesXYZ, aNodesUV, aSegment))
                        {
                            continue;
                        }

                        if (anIsoPnts == null)
                        {
                            aPolylines.Append(new VecOfSegments());
                            anIsoIndexes.SetValue(anIsoIdx, aPolylines.Size());
                            anIsoPnts = aPolylines.ChangeValue(anIsoIndexes.Value(anIsoIdx));
                        }

                        if (!theLocation.IsIdentity())
                        {
                            aSegment[0].Pnt.Transform(theLocation);
                            aSegment[1].Pnt.Transform(theLocation);
                        }
                        anIsoPnts.Append(aSegment);
                    }
                }

                //sortSegments(aPolylines, isUIso ? theUPolylines : theVPolylines);
            }
        }
    }

}

