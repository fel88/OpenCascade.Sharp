global using TColgp_SequenceOfPnt = TKernel.NCollection_Sequence<TKMath.gp_Pnt>;
using OCCPort;
using OCCPort.Common;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using TKBRep;
using TKG3d;
using TKGeomBase;
using TKMath;
using TKService;

namespace TKV3d
{
    //! Tool for computing wireframe presentation of a TopoDS_Shape.
    class StdPrs_WFShape : Prs3d_Root
    {


        public static void Add(Prs3d_Presentation thePresentation,
                              TopoDS_Shape theShape,
                              Prs3d_Drawer theDrawer,
                              bool theIsParallel = false)
        {
            if (theShape.IsNull())
            {
                return;
            }

            if (theDrawer.IsAutoTriangulation())
            {
                StdPrs_ToolTriangulatedShape.Tessellate(theShape, theDrawer);
            }

            // draw triangulation-only edges
            Graphic3d_ArrayOfPrimitives aTriFreeEdges = AddEdgesOnTriangulation(theShape, true);
            if (aTriFreeEdges != null)
            {
                Graphic3d_Group aGroup = thePresentation.NewGroup();
                //aGroup.SetPrimitivesAspect(theDrawer.FreeBoundaryAspect().Aspect());
                aGroup.AddPrimitiveArray(aTriFreeEdges);
            }

            Prs3d_NListOfSequenceOfPnt aCommonPolylines = new Prs3d_NListOfSequenceOfPnt();
            Prs3d_LineAspect aWireAspect = theDrawer.WireAspect();
            double aShapeDeflection = StdPrs_ToolTriangulatedShape.GetDeflection(theShape, theDrawer);


            // Draw isolines
            {
                Prs3d_NListOfSequenceOfPnt aUPolylines = new Prs3d_NListOfSequenceOfPnt(), aVPolylines = new Prs3d_NListOfSequenceOfPnt();
                Prs3d_NListOfSequenceOfPnt aUPolylinesPtr = aUPolylines;
                Prs3d_NListOfSequenceOfPnt aVPolylinesPtr = aVPolylines;

                Prs3d_LineAspect anIsoAspectU = theDrawer.UIsoAspect();
                Prs3d_LineAspect anIsoAspectV = theDrawer.VIsoAspect();

                if (anIsoAspectV.Aspect().IsEqual(anIsoAspectU.Aspect()))
                {
                    aVPolylinesPtr = aUPolylinesPtr;  // put both U and V isolines into single group
                }
                if (anIsoAspectU.Aspect().IsEqual(aWireAspect.Aspect()))
                {
                    aUPolylinesPtr = aCommonPolylines; // put U isolines into single group with common edges
                }
                if (anIsoAspectV.Aspect().IsEqual(aWireAspect.Aspect()))
                {
                    aVPolylinesPtr = aCommonPolylines; // put V isolines into single group with common edges
                }

                bool isParallelIso = false;
                if (theIsParallel)
                {
                    int aNbFaces = 0;
                    for (TopExp_Explorer aFaceExplorer = new TopExp_Explorer(theShape, TopAbs_ShapeEnum.TopAbs_FACE); aFaceExplorer.More(); aFaceExplorer.Next())
                    {
                        ++aNbFaces;
                    }
                    if (aNbFaces > 1)
                    {
                        isParallelIso = true;
                        List<TopoDS_Face> aFaces = new List<TopoDS_Face>(aNbFaces);
                        aNbFaces = 0;
                        for (TopExp_Explorer aFaceExplorer = new TopExp_Explorer(theShape, TopAbs_ShapeEnum.TopAbs_FACE); aFaceExplorer.More(); aFaceExplorer.Next())
                        {
                            TopoDS_Face aFace = TopoDS.Face(aFaceExplorer.Current());
                            if (theDrawer.IsoOnPlane() || !StdPrs_ShapeTool.IsPlanarFace(aFace))
                            {
                                aFaces[aNbFaces++] = aFace;
                            }
                        }

                        //StdPrs_WFShape_IsoFunctor anIsoFunctor(*aUPolylinesPtr, *aVPolylinesPtr, aFaces, theDrawer, aShapeDeflection);
                        //  OSD_Parallel::For(0, aNbFaces, anIsoFunctor, aNbFaces < 2);
                    }
                }

                if (!isParallelIso)
                {
                    for (TopExp_Explorer aFaceExplorer = new TopExp_Explorer(theShape, TopAbs_ShapeEnum.TopAbs_FACE); aFaceExplorer.More(); aFaceExplorer.Next())
                    {
                        TopoDS_Face aFace = TopoDS.Face(aFaceExplorer.Current());
                        if (theDrawer.IsoOnPlane() || !StdPrs_ShapeTool.IsPlanarFace(aFace))
                        {
                            StdPrs_Isolines.Add(aFace, theDrawer, aShapeDeflection, aUPolylinesPtr, aVPolylinesPtr);
                        }
                    }
                }

                Prs3d.AddPrimitivesGroup(thePresentation, anIsoAspectU, aUPolylines);
                Prs3d.AddPrimitivesGroup(thePresentation, anIsoAspectV, aVPolylines);
            }

            {
                Prs3d_NListOfSequenceOfPnt anUnfree = new Prs3d_NListOfSequenceOfPnt(), aFree = new Prs3d_NListOfSequenceOfPnt();
                Prs3d_NListOfSequenceOfPnt anUnfreePtr = anUnfree;
                Prs3d_NListOfSequenceOfPnt aFreePtr = aFree;
                /*
                if (!theDrawer.UnFreeBoundaryDraw())
                {
                    anUnfreePtr = null;
                }
                else if (theDrawer.UnFreeBoundaryAspect()->Aspect()->IsEqual(aWireAspect.Aspect()))
                {
                    anUnfreePtr = &aCommonPolylines; // put unfree edges into single group with common edges
                }

                if (!theDrawer.FreeBoundaryDraw())
                {
                    aFreePtr = null;
                }
                else if (theDrawer.FreeBoundaryAspect().Aspect().IsEqual(aWireAspect.Aspect()))
                {
                    aFreePtr = &aCommonPolylines; // put free edges into single group with common edges
                }*/

                addEdges(theShape,
                          theDrawer,
                          aShapeDeflection,
                          theDrawer.WireDraw() ? aCommonPolylines : null,
                          aFreePtr,
                          anUnfreePtr);

                Prs3d.AddPrimitivesGroup(thePresentation, theDrawer.UnFreeBoundaryAspect(), anUnfree);
                Prs3d.AddPrimitivesGroup(thePresentation, theDrawer.FreeBoundaryAspect(), aFree);
            }


            Prs3d.AddPrimitivesGroup(thePresentation, theDrawer.WireAspect(), aCommonPolylines);

            Graphic3d_ArrayOfPoints aVertexArray = AddVertexes(theShape, theDrawer.VertexDrawMode());
            if (aVertexArray != null)
            {
                Graphic3d_Group aGroup = thePresentation.NewGroup();
                //aGroup.SetPrimitivesAspect(theDrawer.PointAspect()->Aspect());
                aGroup.AddPrimitiveArray(aVertexArray);
            }
        }

        // =========================================================================
        // function : addEdges
        // purpose  :
        // =========================================================================
        public static void addEdges(TopoDS_Shape theShape,
                                Prs3d_Drawer theDrawer,
                                double theShapeDeflection,
                                Prs3d_NListOfSequenceOfPnt theWire,
                                Prs3d_NListOfSequenceOfPnt theFree,
                                Prs3d_NListOfSequenceOfPnt theUnFree)
        {
            if (theShape.IsNull())
            {
                return;
            }

            TopTools_ListOfShape aLWire = new TopTools_ListOfShape();
            TopTools_ListOfShape aLFree = new TopTools_ListOfShape();
            TopTools_ListOfShape aLUnFree = new TopTools_ListOfShape();

            TopTools_IndexedDataMapOfShapeListOfShape anEdgeMap = new TopTools_IndexedDataMapOfShapeListOfShape();
            TopExp.MapShapesAndAncestors(theShape, TopAbs_ShapeEnum.TopAbs_EDGE, TopAbs_ShapeEnum.TopAbs_FACE, anEdgeMap);

            for (TopTools_IndexedDataMapOfShapeListOfShape.Iterator anEdgeIter = new TopTools_IndexedDataMapOfShapeListOfShape.Iterator(anEdgeMap); anEdgeIter.More(); anEdgeIter.Next())
            {
                TopoDS_Edge anEdge = TopoDS.Edge(anEdgeIter.Key());
                int aNbNeighbours = anEdgeIter.Value().Extent();
                switch (aNbNeighbours)
                {
                    case 0:
                        {
                            if (theWire != null)
                            {
                                aLWire.Append(anEdge);
                            }
                            break;
                        }
                    case 1:
                        {
                            if (theFree != null)
                            {
                                aLFree.Append(anEdge);
                            }
                            break;
                        }
                    default:
                        {
                            if (theUnFree != null)
                            {
                                aLUnFree.Append(anEdge);
                            }
                            break;
                        }
                }
            }

            if (!aLWire.IsEmpty())
            {
                addEdges(aLWire, theDrawer, theShapeDeflection, theWire);
            }
            if (!aLFree.IsEmpty())
            {
                addEdges(aLFree, theDrawer, theShapeDeflection, theFree);
            }
            if (!aLUnFree.IsEmpty())
            {
                addEdges(aLUnFree, theDrawer, theShapeDeflection, theUnFree);
            }
        }

        static void addEdges(TopTools_ListOfShape theEdges,
                                   Prs3d_Drawer theDrawer,
                                   double theShapeDeflection,
                                  Prs3d_NListOfSequenceOfPnt thePolylines)
        {
            TopTools_ListIteratorOfListOfShape anEdgesIter = new TopTools_ListIteratorOfListOfShape();
            for (anEdgesIter.Initialize(theEdges); anEdgesIter.More(); anEdgesIter.Next())
            {
                TopoDS_Edge anEdge = TopoDS.Edge(anEdgesIter.Value());
                if (BRep_Tool.Degenerated(anEdge))
                {
                    continue;
                }

                TColgp_SequenceOfPnt aPoints = new TColgp_SequenceOfPnt();

                TopLoc_Location aLocation = new TopLoc_Location();
                Poly_Triangulation aTriangulation = null;
                Poly_PolygonOnTriangulation anEdgeIndicies = null;
                BRep_Tool.PolygonOnTriangulation(anEdge, ref anEdgeIndicies, ref aTriangulation, aLocation);
                Poly_Polygon3D aPolygon;

                if (anEdgeIndicies != null)
                {
                    // Presentation based on triangulation of a face.
                    TColStd_Array1OfInteger anIndices = anEdgeIndicies.Nodes();

                    int anIndex = anIndices.Lower();
                    if (aLocation.IsIdentity())
                    {
                        for (; anIndex <= anIndices.Upper(); ++anIndex)
                        {
                            aPoints.Append(aTriangulation.Node(anIndices[anIndex]));
                        }
                    }
                    else
                    {
                        for (; anIndex <= anIndices.Upper(); ++anIndex)
                        {
                            aPoints.Append(aTriangulation.Node(anIndices[anIndex]).Transformed(aLocation));
                        }
                    }
                }
                else if ((aPolygon = BRep_Tool.Polygon3D(anEdge, ref aLocation)) != null)
                {
                    // Presentation based on triangulation of the free edge on a surface.
                    TColgp_Array1OfPnt aNodes = aPolygon.Nodes();
                    int anIndex = aNodes.Lower();
                    if (aLocation.IsIdentity())
                    {
                        for (; anIndex <= aNodes.Upper(); ++anIndex)
                        {
                            aPoints.Append(aNodes.Value(anIndex));
                        }
                    }
                    else
                    {
                        for (; anIndex <= aNodes.Upper(); ++anIndex)
                        {
                            aPoints.Append(aNodes.Value(anIndex).Transformed(aLocation));
                        }
                    }
                }
                else if (BRep_Tool.IsGeometric(anEdge))
                {
                    // Default presentation for edges without triangulation.
                    BRepAdaptor_Curve aCurve = new BRepAdaptor_Curve(anEdge);
                    StdPrs_DeflectionCurve.Add(null,
                                                 aCurve,
                                                 theShapeDeflection,
                                                 theDrawer,
                                                 aPoints.ChangeSequence(),
                                                 false);
                }

                if (!aPoints.IsEmpty())
                {
                    thePolylines.Append(aPoints);
                }
            }
        }


        // =========================================================================
        // function : AddVertexes
        // purpose  :
        // =========================================================================
        public static Graphic3d_ArrayOfPoints AddVertexes(TopoDS_Shape theShape,
                                                             Prs3d_VertexDrawMode theVertexMode)
        {
            TColgp_SequenceOfPnt aShapeVertices = new TColgp_SequenceOfPnt();
            if (theVertexMode == Prs3d_VertexDrawMode.Prs3d_VDM_All)
            {
                for (TopExp_Explorer aVertIter = new TopExp_Explorer(theShape, TopAbs_ShapeEnum.TopAbs_VERTEX); aVertIter.More(); aVertIter.Next())
                {
                    TopoDS_Vertex aVert = TopoDS.Vertex(aVertIter.Current());
                    aShapeVertices.Append(BRep_Tool.Pnt(aVert));
                }
            }
            else
            {
                // isolated vertices
                for (TopExp_Explorer aVertIter = new TopExp_Explorer(theShape, TopAbs_ShapeEnum.TopAbs_VERTEX, TopAbs_ShapeEnum.TopAbs_EDGE); aVertIter.More(); aVertIter.Next())
                {
                    TopoDS_Vertex aVert = TopoDS.Vertex(aVertIter.Current());
                    aShapeVertices.Append(BRep_Tool.Pnt(aVert));
                }

                // internal vertices
                for (TopExp_Explorer anEdgeIter = new TopExp_Explorer(theShape, TopAbs_ShapeEnum.TopAbs_EDGE); anEdgeIter.More(); anEdgeIter.Next())
                {
                    for (TopoDS_Iterator aVertIter = new TopoDS_Iterator(anEdgeIter.Current(), false, true); aVertIter.More(); aVertIter.Next())
                    {
                        TopoDS_Shape aVertSh = aVertIter.Value();
                        if (aVertSh.Orientation() == TopAbs_Orientation.TopAbs_INTERNAL
                            && aVertSh.ShapeType() == TopAbs_ShapeEnum.TopAbs_VERTEX)
                        {
                            TopoDS_Vertex aVert = TopoDS.Vertex(aVertSh);
                            aShapeVertices.Append(BRep_Tool.Pnt(aVert));
                        }
                    }
                }
            }

            if (aShapeVertices.IsEmpty())
            {
                return null;
            }

            int aNbVertices = aShapeVertices.Length();
            Graphic3d_ArrayOfPoints aVertexArray = new Graphic3d_ArrayOfPoints(aNbVertices);
            for (int aVertIter = 1; aVertIter <= aNbVertices; ++aVertIter)
            {
                aVertexArray.AddVertex(aShapeVertices.Value(aVertIter));
            }
            return aVertexArray;
        }

        public static void AddEdgesOnTriangulation(TColgp_SequenceOfPnt theSegments,

                                              TopoDS_Shape theShape,
                                              bool theToExcludeGeometric = true)
        {
            TopLoc_Location aLocation = new TopLoc_Location(), aDummyLoc = new TopLoc_Location();
            for (TopExp_Explorer aFaceIter = new TopExp_Explorer(theShape, TopAbs_ShapeEnum.TopAbs_FACE); aFaceIter.More(); aFaceIter.Next())
            {
                TopoDS_Face aFace = TopoDS.Face(aFaceIter.Current());
                if (theToExcludeGeometric)
                {
                    Geom_Surface aSurf = BRep_Tool.Surface(aFace, out aDummyLoc);
                    if (aSurf != null)
                    {
                        continue;
                    }
                }
                Poly_Triangulation aPolyTri = BRep_Tool.Triangulation(aFace, ref aLocation);
                if (aPolyTri != null)
                {
                    Prs3d.AddFreeEdges(theSegments, aPolyTri, aLocation);
                }
            }
        }

        public static Graphic3d_ArrayOfPrimitives AddEdgesOnTriangulation(TopoDS_Shape theShape,
                                                                             bool theToExcludeGeometric)
        {
            TColgp_SequenceOfPnt aSeqPnts = new TColgp_SequenceOfPnt();
            AddEdgesOnTriangulation(aSeqPnts, theShape, theToExcludeGeometric);
            if (aSeqPnts.Size() < 2)
            {
                return null;
            }

            int aNbVertices = aSeqPnts.Size();
            Graphic3d_ArrayOfSegments aSurfArray = new Graphic3d_ArrayOfSegments(aNbVertices);
            for (int anI = 1; anI <= aNbVertices; anI += 2)
            {
                aSurfArray.AddVertex(aSeqPnts.Value(anI));
                aSurfArray.AddVertex(aSeqPnts.Value(anI + 1));
            }
            return aSurfArray;
        }

    }
}

