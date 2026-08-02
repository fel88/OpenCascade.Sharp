using OCCPort.Common;
using System.Reflection.Metadata;
using TKBRep;
using TKG2d;
using TKG3d;
using TKGeomBase;
using TKMath;

namespace OCCPort
{
    //! The BRepTools package provides  utilities for BRep
    //! data structures.
    //!
    //! * WireExplorer : A tool to explore the topology of
    //! a wire in the order of the edges.
    //!
    //! * ShapeSet :  Tools used for  dumping, writing and
    //! reading.
    //!
    //! * UVBounds : Methods to compute the  limits of the
    //! boundary  of a  face,  a wire or   an edge in  the
    //! parametric space of a face.
    //!
    //! *  Update : Methods  to call when   a topology has
    //! been created to compute all missing data.
    //!
    //! * UpdateFaceUVPoints: Method to update the UV points
    //! stored with the edges on a face.
    //!
    //! * Compare : Method to compare two vertices.
    //!
    //! * Compare : Method to compare two edges.
    //!
    //! * OuterWire : A method to find the outer wire of a
    //! face.
    //!
    //! * Map3DEdges : A method to map all the 3D Edges of
    //! a Shape.
    //!
    //! * Dump : A method to dump a BRep object.
    public class BRepTools
    {
        public static void Update(TopoDS_Edge e)
        {

        }

        //! Removes all cached polygonal representation of the shape,
        //! i.e. the triangulations of the faces of <S> and polygons on
        //! triangulations and polygons 3d of the edges.
        //! In case polygonal representation is the only available representation
        //! for the shape (shape does not have geometry) it is not removed.
        //! @param theShape  [in] the shape to clean
        //! @param theForce  [in] allows removing all polygonal representations from the shape,
        //!                       including polygons on triangulations irrelevant for the faces of the given shape.
        public static void Clean(TopoDS_Shape theShape, bool theForce = false)
        {
            if (theShape.IsNull())
                return;

            BRep_Builder aBuilder = new BRep_Builder();
            Poly_Triangulation aNullTriangulation = null;
            Poly_PolygonOnTriangulation aNullPoly;

            TopTools_MapOfShape aShapeMap = new TopTools_MapOfShape();
            TopLoc_Location anEmptyLoc = new TopLoc_Location();

            TopExp_Explorer aFaceIt = new TopExp_Explorer(theShape, TopAbs_ShapeEnum.TopAbs_FACE);
            for (; aFaceIt.More(); aFaceIt.Next())
            {
                TopoDS_Shape aFaceNoLoc = aFaceIt.Value();
                aFaceNoLoc.Location(anEmptyLoc);
                if (!aShapeMap.Add(aFaceNoLoc))
                {
                    // the face has already been processed
                    continue;
                }

                TopoDS_Face aFace = TopoDS.Face(aFaceIt.Current());
                if (!BRep_Tool.IsGeometric(aFace))
                {
                    // Do not remove triangulation as there is no surface to recompute it.
                    continue;
                }


                TopLoc_Location aLoc = null;
                Poly_Triangulation aTriangulation =
                  BRep_Tool.Triangulation(aFace, ref aLoc);

                if (aTriangulation == null)
                    continue;

                // Nullify edges
                // Theoretically, the edges on the face (with surface) may have no geometry
                // (no curve 3d or 2d or both). Such faces should be considered as invalid and
                // are not supported by current implementation. So, both triangulation of the face
                // and polygon on triangulation of the edges are removed unconditionally.
                TopExp_Explorer aEdgeIt = new TopExp_Explorer(aFace, TopAbs_ShapeEnum.TopAbs_EDGE);
                for (; aEdgeIt.More(); aEdgeIt.Next())
                {
                    TopoDS_Edge anEdge = TopoDS.Edge(aEdgeIt.Current());
                    //aBuilder.UpdateEdge(anEdge, aNullPoly, aTriangulation, aLoc);
                }

                aBuilder.UpdateFace(aFace, aNullTriangulation);
            }
        }


        //=======================================================================
        //function : UVBounds
        //purpose  : 
        //=======================================================================
        public static void UVBounds(TopoDS_Face F,
                         ref double UMin, ref double UMax,
                         ref double VMin, ref double VMax)
        {
            Bnd_Box2d B = new Bnd_Box2d();
            AddUVBounds(F, B);
            if (!B.IsVoid())
            {
                B.Get(ref UMin, ref VMin, ref UMax, ref VMax);
            }
            else
            {
                UMin = UMax = VMin = VMax = 0.0;
            }
        }


        public static void AddUVBounds(TopoDS_Face FF, Bnd_Box2d B)
        {
            TopoDS_Face F = FF;
            F.Orientation(TopAbs_Orientation.TopAbs_FORWARD);
            TopExp_Explorer ex = new TopExp_Explorer(F, TopAbs_ShapeEnum.TopAbs_EDGE);

            // fill box for the given face
            Bnd_Box2d aBox = new Bnd_Box2d();
            for (; ex.More(); ex.Next())
            {
                BRepTools.AddUVBounds(F, TopoDS.Edge(ex.Current()), aBox);
            }

            // if the box is empty (face without edges or without pcurves),
            // get natural bounds
            if (aBox.IsVoid())
            {
                double UMin = 0, UMax = 0, VMin = 0, VMax = 0;
                TopLoc_Location L;
                Geom_Surface aSurf = BRep_Tool.Surface(F, out L);
                if (aSurf == null)
                {
                    return;
                }

                aSurf.Bounds(out UMin, out UMax, out VMin, out VMax);
                aBox.Update(UMin, VMin, UMax, VMax);
            }

            // add face box to result
            B.Add(aBox);
        }

        //=======================================================================
        //function : AddUVBounds
        //purpose  : 
        //=======================================================================
        static void AddUVBounds(TopoDS_Face aF,
                             TopoDS_Edge aE,
                            Bnd_Box2d aB)
        {
            double aT1 = 0, aT2 = 0, aXmin = 0.0, aYmin = 0.0, aXmax = 0.0, aYmax = 0.0;
            double aUmin, aUmax, aVmin, aVmax;
            Bnd_Box2d aBoxC = new Bnd_Box2d(), aBoxS = new Bnd_Box2d();
            TopLoc_Location aLoc;
            Geom2d_Curve aC2D = BRep_Tool.CurveOnSurface(aE, aF, ref aT1, ref aT2);
            if (aC2D == null)
            {
                return;
            }//
            BndLib_Add2dCurve.Add(aC2D, aT1, aT2, 0.0, aBoxC);
            if (!aBoxC.IsVoid())
            {
                aBoxC.Get(ref aXmin, ref aYmin, ref aXmax, ref aYmax);
            }
            //
            Geom_Surface aS = BRep_Tool.Surface(aF, out aLoc);
            aS.Bounds(out aUmin, out aUmax, out aVmin, out aVmax);

            if (aS.DynamicType() == typeof(Geom_RectangularTrimmedSurface))
            {
                Geom_RectangularTrimmedSurface aSt =
                           (Geom_RectangularTrimmedSurface)(aS);
                aS = aSt.BasisSurface();
            }

            //

            if (!aS.IsUPeriodic())
            {
                bool isUPeriodic = false;

                // Additional verification for U-periodicity for B-spline surfaces
                // 1. Verify that the surface is U-closed (if such flag is false). Verification uses 2 points
                // 2. Verify periodicity of surface inside UV-bounds of the edge. Verification uses 3 or 6 points.
                if (aS.DynamicType() == typeof(Geom_BSplineSurface) &&
                    (aXmin < aUmin || aXmax > aUmax))
                {
                    double aTol2 = 100 * Precision.Confusion() * Precision.Confusion();
                    isUPeriodic = true;
                    gp_Pnt P1, P2;
                    // 1. Verify that the surface is U-closed
                    if (!aS.IsUClosed())
                    {
                        double aVStep = aVmax - aVmin;
                        for (double aV = aVmin; aV <= aVmax; aV += aVStep)
                        {
                            P1 = aS.Value(aUmin, aV);
                            P2 = aS.Value(aUmax, aV);
                            if (P1.SquareDistance(P2) > aTol2)
                            {
                                isUPeriodic = false;
                                break;
                            }
                        }
                    }
                    // 2. Verify periodicity of surface inside UV-bounds of the edge
                    if (isUPeriodic) // the flag still not changed
                    {
                        double aV = (aVmin + aVmax) * 0.5;
                        double[] aU = new double[6]; // values of U lying out of surface boundaries
                        double[] aUpp = new double[6]; // corresponding U-values plus/minus period
                        int aNbPnt = 0;
                        if (aXmin < aUmin)
                        {
                            aU[0] = aXmin;
                            aU[1] = (aXmin + aUmin) * 0.5;
                            aU[2] = aUmin;
                            aUpp[0] = aU[0] + aUmax - aUmin;
                            aUpp[1] = aU[1] + aUmax - aUmin;
                            aUpp[2] = aU[2] + aUmax - aUmin;
                            aNbPnt += 3;
                        }
                        if (aXmax > aUmax)
                        {
                            aU[aNbPnt] = aUmax;
                            aU[aNbPnt + 1] = (aXmax + aUmax) * 0.5;
                            aU[aNbPnt + 2] = aXmax;
                            aUpp[aNbPnt] = aU[aNbPnt] - aUmax + aUmin;
                            aUpp[aNbPnt + 1] = aU[aNbPnt + 1] - aUmax + aUmin;
                            aUpp[aNbPnt + 2] = aU[aNbPnt + 2] - aUmax + aUmin;
                            aNbPnt += 3;
                        }
                        for (int anInd = 0; anInd < aNbPnt; anInd++)
                        {
                            P1 = aS.Value(aU[anInd], aV);
                            P2 = aS.Value(aUpp[anInd], aV);
                            if (P1.SquareDistance(P2) > aTol2)
                            {
                                isUPeriodic = false;
                                break;
                            }
                        }
                    }
                }

                if (!isUPeriodic)
                {
                    if ((aXmin < aUmin) && (aUmin < aXmax))
                    {
                        aXmin = aUmin;
                    }
                    if ((aXmin < aUmax) && (aUmax < aXmax))
                    {
                        aXmax = aUmax;
                    }
                }
            }

            if (!aS.IsVPeriodic())
            {
                bool isVPeriodic = false;

                // Additional verification for V-periodicity for B-spline surfaces
                // 1. Verify that the surface is V-closed (if such flag is false). Verification uses 2 points
                // 2. Verify periodicity of surface inside UV-bounds of the edge. Verification uses 3 or 6 points.
                if (aS.DynamicType() == typeof(Geom_BSplineSurface) &&
                    (aYmin < aVmin || aYmax > aVmax))
                {
                    double aTol2 = 100 * Precision.Confusion() * Precision.Confusion();
                    isVPeriodic = true;
                    gp_Pnt P1, P2;
                    // 1. Verify that the surface is V-closed
                    if (!aS.IsVClosed())
                    {
                        double aUStep = aUmax - aUmin;
                        for (double aU = aUmin; aU <= aUmax; aU += aUStep)
                        {
                            P1 = aS.Value(aU, aVmin);
                            P2 = aS.Value(aU, aVmax);
                            if (P1.SquareDistance(P2) > aTol2)
                            {
                                isVPeriodic = false;
                                break;
                            }
                        }
                    }
                    // 2. Verify periodicity of surface inside UV-bounds of the edge
                    if (isVPeriodic) // the flag still not changed
                    {
                        double aU = (aUmin + aUmax) * 0.5;
                        double[] aV = new double[6]; // values of V lying out of surface boundaries
                        double[] aVpp = new double[6]; // corresponding V-values plus/minus period
                        int aNbPnt = 0;
                        if (aYmin < aVmin)
                        {
                            aV[0] = aYmin;
                            aV[1] = (aYmin + aVmin) * 0.5;
                            aV[2] = aVmin;
                            aVpp[0] = aV[0] + aVmax - aVmin;
                            aVpp[1] = aV[1] + aVmax - aVmin;
                            aVpp[2] = aV[2] + aVmax - aVmin;
                            aNbPnt += 3;
                        }
                        if (aYmax > aVmax)
                        {
                            aV[aNbPnt] = aVmax;
                            aV[aNbPnt + 1] = (aYmax + aVmax) * 0.5;
                            aV[aNbPnt + 2] = aYmax;
                            aVpp[aNbPnt] = aV[aNbPnt] - aVmax + aVmin;
                            aVpp[aNbPnt + 1] = aV[aNbPnt + 1] - aVmax + aVmin;
                            aVpp[aNbPnt + 2] = aV[aNbPnt + 2] - aVmax + aVmin;
                            aNbPnt += 3;
                        }
                        for (int anInd = 0; anInd < aNbPnt; anInd++)
                        {
                            P1 = aS.Value(aU, aV[anInd]);
                            P2 = aS.Value(aU, aVpp[anInd]);
                            if (P1.SquareDistance(P2) > aTol2)
                            {
                                isVPeriodic = false;
                                break;
                            }
                        }
                    }
                }

                if (!isVPeriodic)
                {
                    if ((aYmin < aVmin) && (aVmin < aYmax))
                    {
                        aYmin = aVmin;
                    }
                    if ((aYmin < aVmax) && (aVmax < aYmax))
                    {
                        aYmax = aVmax;
                    }
                }
            }

            aBoxS.Update(aXmin, aYmin, aXmax, aYmax);

            aB.Add(aBoxS);
        }

        //=======================================================================
        //function : AddUVBounds
        //purpose  : s
        //=======================================================================
        static void AddUVBounds(TopoDS_Face F,
                                TopoDS_Wire W,
                               Bnd_Box2d B)
        {
            TopExp_Explorer ex = new TopExp_Explorer();
            for (ex.Init(W, TopAbs_ShapeEnum.TopAbs_EDGE); ex.More(); ex.Next())
            {
                BRepTools.AddUVBounds(F, TopoDS.Edge(ex.Current()), B);
            }
        }

        public static bool Triangulation(TopoDS_Shape theShape,
                                                    double theLinDefl,
                                                    bool theToCheckFreeEdges)
        {
            TopExp_Explorer anEdgeIter = new TopExp_Explorer();
            TopLoc_Location aDummyLoc = new TopLoc_Location();
            for (TopExp_Explorer aFaceIter = new TopExp_Explorer(theShape, TopAbs_ShapeEnum.TopAbs_FACE); aFaceIter.More(); aFaceIter.Next())
            {
                TopoDS_Face aFace = TopoDS.Face(aFaceIter.Current());
                Poly_Triangulation aTri = BRep_Tool.Triangulation(aFace, ref aDummyLoc);
                if (aTri == null
                 || aTri.Deflection() > theLinDefl)
                {
                    return false;
                }

                for (anEdgeIter.Init(aFace, TopAbs_ShapeEnum.TopAbs_EDGE); anEdgeIter.More(); anEdgeIter.Next())
                {
                    TopoDS_Edge anEdge = TopoDS.Edge(anEdgeIter.Current());
                    Poly_PolygonOnTriangulation aPoly = BRep_Tool.PolygonOnTriangulation(anEdge, aTri, aDummyLoc);
                    if (aPoly == null)
                    {
                        return false;
                    }
                }
            }
            if (!theToCheckFreeEdges)
            {
                return true;
            }

            Poly_Triangulation anEdgeTri = null;
            //for (anEdgeIter.Init(theShape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE); anEdgeIter.More(); anEdgeIter.Next())
            //{
            //    TopoDS_Edge anEdge = TopoDS.Edge(anEdgeIter.Current());
            //    Poly_Polygon3D aPolygon = BRep_Tool.Polygon3D(anEdge, aDummyLoc);
            //    if (aPolygon != null)
            //    {
            //        if (aPolygon.Deflection() > theLinDefl)
            //        {
            //            return false;
            //        }
            //    }
            //    else
            //    {
            //        Poly_PolygonOnTriangulation aPoly = BRep_Tool.PolygonOnTriangulation(anEdge, anEdgeTri, aDummyLoc);
            //        if (aPoly == null
            //         || anEdgeTri == null
            //         || anEdgeTri.Deflection() > theLinDefl)
            //        {
            //            return false;
            //        }
            //    }
            //}

            return true;
        }

        public static void Update(TopoDS_Face F)
        {
            if (!F.Checked())
            {
                UpdateFaceUVPoints(F);
                F.TShape().Checked(true);
            }
        }

        private static void UpdateFaceUVPoints(TopoDS_Face theF)
        {
            // For each edge of the face <F> reset the UV points to the bounding
            // points of the parametric curve of the edge on the face.

            // Get surface of the face
            TopLoc_Location aLoc;
            Geom_Surface aSurf = BRep_Tool.Surface(theF, out aLoc);
            // Iterate on edges and reset UV points
            TopExp_Explorer anExpE = new TopExp_Explorer(theF, TopAbs_ShapeEnum.TopAbs_EDGE);
            for (; anExpE.More(); anExpE.Next())
            {
                TopoDS_Edge aE = TopoDS.Edge(anExpE.Current());

                BRep_TEdge TE = (BRep_TEdge)aE.TShape();
                if (TE.Locked())
                    return;

                TopLoc_Location aELoc = aLoc.Predivided(aE.Location());
                // Edge representations
                BRep_ListOfCurveRepresentation aLCR = TE.ChangeCurves();
                BRep_ListIteratorOfListOfCurveRepresentation itLCR = new BRep_ListIteratorOfListOfCurveRepresentation(aLCR);
                for (; itLCR.More(); itLCR.Next())
                {
                    BRep_GCurve GC = (itLCR.Value()) as BRep_GCurve;

                    if (GC != null && GC.IsCurveOnSurface(aSurf, aELoc))
                    {
                        // Update UV points
                        GC.Update();
                        break;
                    }
                }
            }

        }

        public static void Update(TopoDS_Shell s)
        {
            TopExp_Explorer ex = new TopExp_Explorer(s, TopAbs_ShapeEnum.TopAbs_FACE);
            while (ex.More())
            {
                Update(TopoDS.Face(ex.Current()));
                ex.Next();
            }
        }

        public static void Update(TopoDS_Wire w)
        {

        }
    }
}