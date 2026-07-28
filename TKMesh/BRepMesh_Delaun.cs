using OCCPort.Common;
using System;
using System.Xml.Linq;
using TKernel;
using TKMath;

namespace TKMesh
{
    //! Compute the Delaunay's triangulation with the algorithm of Watson.
    public class BRepMesh_Delaun
    {
        //! Creates instance of triangulator, but do not run the algorithm automatically.
        public BRepMesh_Delaun(BRepMesh_DataStructureOfDelaun theOldMesh,
                                   int theCellsCountU,
                                   int theCellsCountV,
                                   bool isFillCircles)
        {
            myMeshData = (theOldMesh);
            myCircles = new BRepMesh_CircleTool();
            //(new NCollection_IncAllocator(
            //         IMeshData::MEMORY_BLOCK_SIZE_HUGE)),
            mySupVert = new VectorOfInteger(3);
            myInitCircles = (false);

            if (isFillCircles)
            {
                throw new NotImplementedException();
                //InitCirclesTool(theCellsCountU, theCellsCountV);
            }
        }
        //! Adds some vertices into the triangulation.
        public void AddVertices(VectorOfInteger theVertices,
                                    Message_ProgressRange theRange = null)
        {
            if (theRange == null)
                theRange = new Message_ProgressRange();

            ComparatorOfIndexedVertexOfDelaun aCmp = new ComparatorOfIndexedVertexOfDelaun(myMeshData);
            Std.make_heap(theVertices, aCmp);
            Std.sort_heap(theVertices, aCmp);

            createTrianglesOnNewVertices(theVertices, theRange);
        }

        //=======================================================================
        //function : calculateDist
        //purpose  : Calculates distances between the given point and edges of
        //           triangle
        //=======================================================================
        double calculateDist(gp_XY[] theVEdges,

                 gp_XY[] thePoints,

                 BRepMesh_Vertex theVertex,
                double[] theDistance,
                double[] theSqModulus,
                ref int theEdgeOn)
        {

            double aMinDist = Standard_Real.RealLast();
            for (int i = 0; i < 3; ++i)
            {
                theSqModulus[i] = theVEdges[i].SquareModulus();
                if (theSqModulus[i] <= Precision2)
                    return -1;

                theDistance[i] = theVEdges[i] ^ (theVertex.Coord() - thePoints[i]);

                double aDist = theDistance[i] * theDistance[i];
                aDist /= theSqModulus[i];

                if (aDist < aMinDist)
                {
                    theEdgeOn = i;
                    aMinDist = aDist;
                }
            }

            return aMinDist;
        }


        bool Contains(int theTriangleId,
            BRepMesh_Vertex theVertex,
             double theSqTolerance,
            ref int theEdgeOn)
        {

            theEdgeOn = 0;

            int[] p = new int[3];

            BRepMesh_Triangle aElement = GetTriangle(theTriangleId);
            int[] e = aElement.myEdges;

            BRepMesh_Edge[] anEdges = { GetEdge(e[0]),
                                        GetEdge(e[1]),
                                        GetEdge(e[2]) };

            myMeshData.ElementNodes(aElement, p);

            gp_XY[] aPoints = new gp_XY[3];
            aPoints[0] = GetVertex(p[0]).Coord();
            aPoints[1] = GetVertex(p[1]).Coord();
            aPoints[2] = GetVertex(p[2]).Coord();

            gp_XY[] aVEdges = new gp_XY[3];
            aVEdges[0] = aPoints[1];
            aVEdges[0].Subtract(aPoints[0]);

            aVEdges[1] = aPoints[2];
            aVEdges[1].Subtract(aPoints[1]);

            aVEdges[2] = aPoints[0];
            aVEdges[2].Subtract(aPoints[2]);

            double[] aDistance = new double[3];
            double[] aSqModulus = new double[3];

            double aSqMinDist;
            int aEdgeOnId = 0;
            aSqMinDist = calculateDist(aVEdges, aPoints, theVertex, aDistance, aSqModulus, ref aEdgeOnId);
            if (aSqMinDist < 0)
                return false;

            bool isNotFree = (anEdges[aEdgeOnId].Movability() != BRepMesh_DegreeOfFreedom.BRepMesh_Free);
            if (aSqMinDist > theSqTolerance)
            {
                if (isNotFree && aDistance[aEdgeOnId] < (aSqModulus[aEdgeOnId] / 5.0))
                    theEdgeOn = e[aEdgeOnId];
            }
            else if (isNotFree)
                return false;
            else
                theEdgeOn = e[aEdgeOnId];

            return (aDistance[0] >= 0.0 && aDistance[1] >= 0.0 && aDistance[2] >= 0.0);
        }

        //=======================================================================
        //function : createTrianglesOnNewVertices
        //purpose  : Creation of triangles from the new nodes
        //=======================================================================
        void createTrianglesOnNewVertices(
                  VectorOfInteger theVertexIndexes,
          Message_ProgressRange theRange)
        {

            double aTolU = 0, aTolV = 0;
            myMeshData.Data().GetTolerance(ref aTolU, ref aTolV);
            double aSqTol = aTolU * aTolU + aTolV * aTolV;

            // Insertion of nodes :
            bool isModify = true;

            int anIndex = theVertexIndexes.Lower();
            int anUpper = theVertexIndexes.Upper();
            Message_ProgressScope aPS = new Message_ProgressScope(theRange, "Create triangles on new vertices", anUpper);
            for (; anIndex <= anUpper; ++anIndex, aPS.Next())
            {
                if (!aPS.More())
                {
                    return;
                }
                //aAllocator->Reset(Standard_False);
                MapOfIntegerInteger aLoopEdges = new MapOfIntegerInteger(10);

                int aVertexIdx = theVertexIndexes[anIndex];
                BRepMesh_Vertex aVertex = GetVertex(aVertexIdx);

                // Iterator in the list of indexes of circles containing the node
                ListOfInteger aCirclesList = myCircles.Select(aVertex.Coord());

                int onEgdeId = 0, aTriangleId = 0;
                foreach (var aCircleIt in aCirclesList)
                {

                    // To add a node in the mesh it is necessary to check conditions: 
                    // - the node should be within the boundaries of the mesh and so in an existing triangle
                    // - all adjacent triangles should belong to a component connected with this triangle
                    if (Contains(aCircleIt, aVertex, aSqTol, ref onEgdeId))
                    {
                        if (onEgdeId != 0 && GetEdge(onEgdeId).Movability() != BRepMesh_DegreeOfFreedom.BRepMesh_Free)
                        {
                            // We can skip free vertex too close to the frontier edge.
                            if (aVertex.Movability() == BRepMesh_DegreeOfFreedom.BRepMesh_Free)
                                continue;

                            // However, we should add vertex that have neighboring frontier edges.
                        }

                        // Remove triangle even if it contains frontier edge in order 
                        // to prevent appearance of incorrect configurations like free 
                        // edge glued with frontier #26407
                        aTriangleId = aCircleIt;
                        aCirclesList.Remove(aCircleIt);
                        break;
                    }
                }

                if (aTriangleId > 0)
                {
                    deleteTriangle(aTriangleId, aLoopEdges);

                    isModify = true;
                    while (isModify && !aCirclesList.IsEmpty())
                    {
                        isModify = false;
                        foreach (var aCircleIt1 in aCirclesList)
                        {
                            BRepMesh_Triangle aElement = GetTriangle(aCircleIt1);
                            int[] e = aElement.myEdges;

                            if (aLoopEdges.IsBound(e[0]) ||
                                 aLoopEdges.IsBound(e[1]) ||
                                 aLoopEdges.IsBound(e[2]))
                            {
                                isModify = true;
                                deleteTriangle(aCircleIt1, aLoopEdges);
                                aCirclesList.Remove(aCircleIt1);
                                break;
                            }
                        }
                    }

                    // Creation of triangles with the current node and free edges
                    // and removal of these edges from the list of free edges
                    createTriangles(aVertexIdx, aLoopEdges);
                }
            }

            ProcessConstraints();
        }
        //=======================================================================
        //function : createTriangles
        //purpose  : Creates the triangles between the node and the polyline.
        //=======================================================================
        void createTriangles(int theVertexIndex,
                                              MapOfIntegerInteger thePoly)
        {
            ListOfInteger aLoopEdges = new ListOfInteger(), anExternalEdges = new ListOfInteger();
            gp_XY aVertexCoord = myMeshData.GetNode(theVertexIndex).Coord();

            MapOfIntegerInteger.Iterator anEdges = new MapOfIntegerInteger.Iterator(thePoly);
            for (; anEdges.More(); anEdges.Next())
            {
                int anEdgeId = anEdges.Key();
                BRepMesh_Edge anEdge = GetEdge(anEdgeId);

                bool isPositive = thePoly[anEdgeId] != 0;

                int[] aNodes = new int[3];
                if (isPositive)
                {
                    aNodes[0] = anEdge.FirstNode();
                    aNodes[2] = anEdge.LastNode();
                }
                else
                {
                    aNodes[0] = anEdge.LastNode();
                    aNodes[2] = anEdge.FirstNode();
                }
                aNodes[1] = theVertexIndex;

                BRepMesh_Vertex aFirstVertex = GetVertex(aNodes[0]);
                BRepMesh_Vertex aLastVertex = GetVertex(aNodes[2]);

                gp_XY anEdgeDir = new gp_XY(aLastVertex.Coord() - aFirstVertex.Coord());
                double anEdgeLen = anEdgeDir.Modulus();
                if (anEdgeLen < _Precision)
                    continue;

                anEdgeDir.SetCoord(anEdgeDir.X() / anEdgeLen,
                                    anEdgeDir.Y() / anEdgeLen);

                gp_XY aFirstLinkDir = new gp_XY(aFirstVertex.Coord() - aVertexCoord);
                gp_XY aLastLinkDir = new gp_XY(aVertexCoord - aLastVertex.Coord());

                double aDist12 = aFirstLinkDir ^ anEdgeDir;
                double aDist23 = anEdgeDir ^ aLastLinkDir;
                if (Math.Abs(aDist12) < _Precision ||
                    Math.Abs(aDist23) < _Precision)
                {
                    continue;
                }

                BRepMesh_Edge aFirstLink = new BRepMesh_Edge(aNodes[1], aNodes[0], BRepMesh_DegreeOfFreedom.BRepMesh_Free);
                BRepMesh_Edge aLastLink = new BRepMesh_Edge(aNodes[2], aNodes[1], BRepMesh_DegreeOfFreedom.BRepMesh_Free);

                int[] anEdgesInfo = new int[] {
      myMeshData.AddLink( aFirstLink ),
      isPositive ? anEdgeId : -anEdgeId,
      myMeshData.AddLink( aLastLink ) };

                bool isSensOK = (aDist12 > 0.0 && aDist23 > 0.0);
                if (isSensOK)
                {
                    int[] anEdgeIds = new int[3];
                    bool[] anEdgesOri = new bool[3];
                    for (int aTriLinkIt = 0; aTriLinkIt < 3; ++aTriLinkIt)
                    {
                        int anEdgeInfo = anEdgesInfo[aTriLinkIt];
                        anEdgeIds[aTriLinkIt] = Math.Abs(anEdgeInfo);
                        anEdgesOri[aTriLinkIt] = anEdgeInfo > 0;
                    }

                    addTriangle(anEdgeIds, anEdgesOri, aNodes);
                }
                else
                {
                    if (isPositive)
                        aLoopEdges.Append(anEdges.Key());
                    else
                        aLoopEdges.Append(-anEdges.Key());

                    if (aFirstLinkDir.SquareModulus() > aLastLinkDir.SquareModulus())
                        anExternalEdges.Append(Math.Abs(anEdgesInfo[0]));
                    else
                        anExternalEdges.Append(Math.Abs(anEdgesInfo[2]));
                }
            }

            thePoly.Clear();
            while (!anExternalEdges.IsEmpty())
            {
                BRepMesh_PairOfIndex aPair =
                 myMeshData.ElementsConnectedTo(Math.Abs(anExternalEdges.First()));


                if (!aPair.IsEmpty())
                    deleteTriangle(aPair.FirstIndex(), thePoly);

                anExternalEdges.RemoveFirst();
            }

            for (anEdges.Initialize(thePoly); anEdges.More(); anEdges.Next())
            {
                if (myMeshData.ElementsConnectedTo(anEdges.Key()).IsEmpty())
                    myMeshData.RemoveLink(anEdges.Key());
            }

            while (!aLoopEdges.IsEmpty())
            {
                BRepMesh_Edge anEdge = GetEdge(Math.Abs(aLoopEdges.First()));
                if (anEdge.Movability() != BRepMesh_DegreeOfFreedom.BRepMesh_Deleted)
                {
                    int anEdgeIdx = aLoopEdges.First();
                    meshLeftPolygonOf(Math.Abs(anEdgeIdx), (anEdgeIdx > 0));
                }

                aLoopEdges.RemoveFirst();
            }
        }

        //=======================================================================
        //function : addTriangle
        //purpose  : Add a triangle based on the given oriented edges into mesh
        //=======================================================================
        void addTriangle(int[] theEdgesId,
                                   bool[] theEdgesOri,
                                   int[] theNodesId)
        {
            int aNewTriangleId =
              myMeshData.AddElement(new BRepMesh_Triangle(theEdgesId,
                theEdgesOri, BRepMesh_DegreeOfFreedom.BRepMesh_Free));

            bool isAdded = true;
            if (myInitCircles)
            {
                isAdded = myCircles.Bind(
                  aNewTriangleId,
                  GetVertex(theNodesId[0]).Coord(),
                  GetVertex(theNodesId[1]).Coord(),
                  GetVertex(theNodesId[2]).Coord());
            }

            if (!isAdded)
                myMeshData.RemoveElement(aNewTriangleId);
        }

        //! Destruction of auxiliary triangles containing the given vertices.
        //! Removes auxiliary vertices also.
        //! @param theAuxVertices auxiliary vertices to be cleaned up.
        public void RemoveAuxElements()
        {
            MapOfIntegerInteger aLoopEdges = new MapOfIntegerInteger(10);

            // Destruction of triangles containing a top of the super triangle
            BRepMesh_SelectorOfDataStructureOfDelaun aSelector = new BRepMesh_SelectorOfDataStructureOfDelaun(myMeshData);
            for (int aSupVertId = 0; aSupVertId < mySupVert.Size(); ++aSupVertId)
                aSelector.NeighboursOfNode(mySupVert[aSupVertId]);

            //            IteratorOfMapOfInteger aFreeTriangles=new IteratorOfMapOfInteger ( aSelector.Elements());
            foreach (var aFreeTriangles in aSelector.Elements())
            {
                deleteTriangle(aFreeTriangles, aLoopEdges);
            }

            // All edges that remain free are removed from aLoopEdges;
            // only the boundary edges of the triangulation remain there
            MapOfIntegerInteger.Iterator aFreeEdges = new NCollection_DataMap<int, int, NCollection_DefaultHasher<int>>.Iterator(aLoopEdges);
            for (; aFreeEdges.More(); aFreeEdges.Next())
            {
                if (myMeshData.ElementsConnectedTo(aFreeEdges.Key()).IsEmpty())
                    myMeshData.RemoveLink(aFreeEdges.Key());
            }

            // The tops of the super triangle are destroyed
            for (int aSupVertId = 0; aSupVertId < mySupVert.Size(); ++aSupVertId)
                myMeshData.RemoveNode(mySupVert[aSupVertId]);
        }

        //! Forces insertion of constraint edges into the base triangulation. 
        public void ProcessConstraints()
        {
            insertInternalEdges();

            // Adjustment of meshes to boundary edges
            frontierAdjust();
        }
        //! Gives the list of frontier edges.
        MapOfInteger Frontier()
        {
            return getEdgesByType(BRepMesh_DegreeOfFreedom.BRepMesh_Frontier);
        }

        void frontierAdjust()
        {
            MapOfInteger aFrontier = Frontier();

            VectorOfInteger aFailedFrontiers = new VectorOfInteger(256);
            MapOfIntegerInteger aLoopEdges = new MapOfIntegerInteger(10);
            MapOfInteger aIntFrontierEdges = new MapOfInteger();

            for (int aPass = 1; aPass <= 2; ++aPass)
            {
                // 1 pass): find external triangles on boundary edges;
                // 2 pass): find external triangles on boundary edges appeared 
                //          during triangles replacement.


                foreach (var aFrontierId in aFrontier)
                {
                    BRepMesh_PairOfIndex aPair = myMeshData.ElementsConnectedTo(aFrontierId);
                    int aNbElem = aPair.Extent();
                    for (int aElemIt = 1; aElemIt <= aNbElem; ++aElemIt)
                    {
                        int aPriorElemId = aPair.Index(aElemIt);
                        if (aPriorElemId < 0)
                            continue;

                        BRepMesh_Triangle aElement = GetTriangle(aPriorElemId);
                        var e = aElement.myEdges;
                        var o = aElement.myOrientations;

                        bool isTriangleFound = false;
                        for (int n = 0; n < 3; ++n)
                        {
                            if (aFrontierId == e[n] && !o[n])
                            {
                                // Destruction  of external triangles on boundary edges
                                isTriangleFound = true;
                                deleteTriangle(aPriorElemId, aLoopEdges);
                                break;
                            }
                        }

                        if (isTriangleFound)
                            break;
                    }
                }

                // destrucrion of remaining hanging edges :
                MapOfIntegerInteger.Iterator aLoopEdgesIt = new(aLoopEdges);
                for (; aLoopEdgesIt.More(); aLoopEdgesIt.Next())
                {
                    var aLoopEdgeId = aLoopEdgesIt.Key();
                    if (myMeshData.ElementsConnectedTo(aLoopEdgeId).IsEmpty())
                        myMeshData.RemoveLink(aLoopEdgeId);
                }


                // destruction of triangles crossing the boundary edges and 
                // their replacement by makeshift triangles

                foreach (var aFrontierId in aFrontier)
                {

                    if (!myMeshData.ElementsConnectedTo(aFrontierId).IsEmpty())
                        continue;

                    bool isSuccess =
                      meshLeftPolygonOf(aFrontierId, true, aIntFrontierEdges);

                    if (aPass == 2 && !isSuccess)
                        aFailedFrontiers.Append(aFrontierId);
                }
            }

            cleanupMesh();

            // When the mesh has been cleaned up, try to process frontier edges 
            // once again to fill the possible gaps that might be occurred in case of "saw" -
            // situation when frontier edge has a triangle at a right side, but its free 
            // links cross another frontieres  and meshLeftPolygonOf itself can't collect 
            // a closed polygon.
            //VectorOfInteger::Iterator aFailedFrontiersIt(aFailedFrontiers );
            //for (; aFailedFrontiersIt.More(); aFailedFrontiersIt.Next())
            foreach (var aFailedFrontiersIt in aFailedFrontiers)
            {
                int aFrontierId = aFailedFrontiersIt;
                if (!myMeshData.ElementsConnectedTo(aFrontierId).IsEmpty())
                    continue;

                meshLeftPolygonOf(aFrontierId, true, aIntFrontierEdges);
            }
        }

        //! Gives the list of free edges used only one time
        MapOfInteger FreeEdges()
        {
            return getEdgesByType(BRepMesh_DegreeOfFreedom.BRepMesh_Free);
        }
        //=======================================================================
        //function : getEdgesByType
        //purpose  : Gives the list of edges with type defined by input parameter
        //=======================================================================
        MapOfInteger getEdgesByType(
        BRepMesh_DegreeOfFreedom theEdgeType)
        {

            MapOfInteger aResult = new MapOfInteger();
            foreach (var anEdgeIt in myMeshData.LinksOfDomain())
            {
                int anEdge = anEdgeIt;
                bool isToAdd = (theEdgeType == BRepMesh_DegreeOfFreedom.BRepMesh_Free) ?
                  (myMeshData.ElementsConnectedTo(anEdge).Extent() <= 1) :
                  (GetEdge(anEdge).Movability() == theEdgeType);

                if (isToAdd)
                    aResult.Add(anEdge);
            }

            return aResult;
        }

        const double Angle2PI = 2 * Math.PI;

        //=======================================================================
        //function : isVertexInsidePolygon
        //purpose  : Checks is the given vertex lies inside the polygon
        //=======================================================================
        bool isVertexInsidePolygon(
        int theVertexId,
        VectorOfInteger thePolygonVertices)
        {
            int aPolyLen = thePolygonVertices.Length();
            if (aPolyLen < 3)
                return false;


            gp_XY aCenterPointXY = GetVertex(theVertexId).Coord();

            BRepMesh_Vertex aFirstVertex = GetVertex(thePolygonVertices[0]);
            gp_Vec2d aPrevVertexDir = new gp_Vec2d(aFirstVertex.Coord() - aCenterPointXY);
            if (aPrevVertexDir.SquareMagnitude() < Precision2)
                return true;

            double aTotalAng = 0.0;
            for (int aPolyIt = 1; aPolyIt < aPolyLen; ++aPolyIt)
            {
                BRepMesh_Vertex aPolyVertex = GetVertex(thePolygonVertices[aPolyIt]);

                gp_Vec2d aCurVertexDir = new gp_Vec2d(aPolyVertex.Coord() - aCenterPointXY);
                if (aCurVertexDir.SquareMagnitude() < Precision2)
                    return true;

                aTotalAng += aCurVertexDir.Angle(aPrevVertexDir);
                aPrevVertexDir = aCurVertexDir;
            }

            if (Math.Abs(Angle2PI - aTotalAng) > Precision.Angular())
                return false;

            return true;
        }
        //=======================================================================
        //function : killTrianglesAroundVertex
        //purpose  : Remove all triangles and edges that are placed
        //           inside the polygon or crossed it.
        //=======================================================================
        void killTrianglesAroundVertex(
          int theZombieNodeId,
          VectorOfInteger thePolyVertices,
          MapOfInteger thePolyVerticesFindMap,
          SequenceOfInteger thePolygon,
          SequenceOfBndB2d thePolyBoxes,
          MapOfInteger theSurvivedLinks,
          MapOfIntegerInteger theLoopEdges)
        {
            ListOfInteger.Iterator aNeighborsIt =new NCollection_List<int>.Iterator(
                          myMeshData.LinksConnectedTo(theZombieNodeId));

            // Try to infect neighbor nodes
            VectorOfInteger aVictimNodes = new VectorOfInteger();
            for (; aNeighborsIt.More(); aNeighborsIt.Next())
            {
                int aNeighborLinkId = aNeighborsIt.Value();
                if (theSurvivedLinks.Contains(aNeighborLinkId))
                    continue;

                BRepMesh_Edge aNeighborLink = GetEdge(aNeighborLinkId);
                if (aNeighborLink.Movability() == BRepMesh_DegreeOfFreedom.BRepMesh_Frontier)
                {
                    // Though, if it lies onto the polygon boundary -
                    // take its triangles
                    Bnd_B2d aBox = new Bnd_B2d();
                    bool isNotIntersect =
                      checkIntersection(aNeighborLink, thePolygon,
                      thePolyBoxes, false, true,
                      false, aBox);

                    if (isNotIntersect)
                    {
                        // Don't touch it
                        theSurvivedLinks.Add(aNeighborLinkId);
                        continue;
                    }
                }
                else
                {
                    int anOtherNode = aNeighborLink.FirstNode();
                    if (anOtherNode == theZombieNodeId)
                        anOtherNode = aNeighborLink.LastNode();

                    // Possible sacrifice
                    if (!thePolyVerticesFindMap.Contains(anOtherNode))
                    {
                        if (isVertexInsidePolygon(anOtherNode, thePolyVertices))
                        {
                            // Got you!
                            aVictimNodes.Append(anOtherNode);
                        }
                        else
                        {
                            // Lucky. But it may intersect the polygon boundary.
                            // Let's check it!
                            killTrianglesOnIntersectingLinks(aNeighborLinkId,
                              aNeighborLink, anOtherNode, thePolygon,
                              thePolyBoxes, theSurvivedLinks, theLoopEdges);

                            continue;
                        }
                    }
                }

                // Add link to the survivors to avoid cycling
                theSurvivedLinks.Add(aNeighborLinkId);
                killLinkTriangles(aNeighborLinkId, theLoopEdges);
            }

            // Go and do your job!

            foreach (var item in aVictimNodes)
            {
                killTrianglesAroundVertex(item, thePolyVertices,
                  thePolyVerticesFindMap, thePolygon, thePolyBoxes,
                  theSurvivedLinks, theLoopEdges);
            }
        }


        //=======================================================================
        //function : killTrianglesOnIntersectingLinks
        //purpose  : Checks is the given link crosses the polygon boundary.
        //           If yes, kills its triangles and checks neighbor links on
        //           boundary intersection. Does nothing elsewhere.
        //=======================================================================
        void killTrianglesOnIntersectingLinks(
        int theLinkToCheckId,
        BRepMesh_Edge theLinkToCheck,
        int theEndPoint,
        SequenceOfInteger thePolygon,
        SequenceOfBndB2d thePolyBoxes,
        MapOfInteger theSurvivedLinks,
        MapOfIntegerInteger theLoopEdges)
        {
            if (theSurvivedLinks.Contains(theLinkToCheckId))
                return;

            Bnd_B2d aBox = new Bnd_B2d();
            bool isNotIntersect =
              checkIntersection(theLinkToCheck, thePolygon,
                thePolyBoxes, false, false,
                false, aBox);

            theSurvivedLinks.Add(theLinkToCheckId);

            if (isNotIntersect)
                return;

            killLinkTriangles(theLinkToCheckId, theLoopEdges);



            foreach (var aNeighborsIt in myMeshData.LinksConnectedTo(theEndPoint))
            {
                int aNeighborLinkId = aNeighborsIt;
                BRepMesh_Edge aNeighborLink = GetEdge(aNeighborLinkId);
                int anOtherNode = aNeighborLink.FirstNode();
                if (anOtherNode == theEndPoint)
                    anOtherNode = aNeighborLink.LastNode();

                killTrianglesOnIntersectingLinks(aNeighborLinkId,
                  aNeighborLink, anOtherNode, thePolygon,
                  thePolyBoxes, theSurvivedLinks, theLoopEdges);
            }
        }


        //=======================================================================
        //function : killLinkTriangles
        //purpose  : Kill triangles bound to the given link.
        //=======================================================================
        void killLinkTriangles(
        int theLinkId,
        MapOfIntegerInteger theLoopEdges)
        {
            BRepMesh_PairOfIndex aPair =
              myMeshData.ElementsConnectedTo(theLinkId);

            int anElemNb = aPair.Extent();
            for (int aPairIt = 1; aPairIt <= anElemNb; ++aPairIt)
            {
                int anElemId = aPair.FirstIndex();
                if (anElemId < 0)
                    continue;

                deleteTriangle(anElemId, theLoopEdges);
            }
        }



        //! Cleanup mesh from the free triangles.
        //=======================================================================
        //function : cleanupMesh
        //purpose  : Cleanup mesh from the free triangles
        //=======================================================================
        void cleanupMesh()
        {
            for (; ; )
            {
                //aAllocator->Reset(Standard_False);
                MapOfIntegerInteger aLoopEdges = new MapOfIntegerInteger(10);
                MapOfInteger aDelTriangles = new MapOfInteger();

                MapOfInteger aFreeEdges = FreeEdges();

                foreach (var aFreeEdgeId in aFreeEdges)
                {
                    BRepMesh_Edge anEdge = GetEdge(aFreeEdgeId);
                    if (anEdge.Movability() == BRepMesh_DegreeOfFreedom.BRepMesh_Frontier)
                        continue;

                    BRepMesh_PairOfIndex aPair =
                      myMeshData.ElementsConnectedTo(aFreeEdgeId);
                    if (aPair.IsEmpty())
                    {
                        aLoopEdges.Bind(aFreeEdgeId, 1);
                        continue;
                    }

                    int aTriId = aPair.FirstIndex();

                    // Check that the connected triangle is not surrounded by another triangles
                    BRepMesh_Triangle aElement = GetTriangle(aTriId);
                    var anEdges = aElement.myEdges;

                    bool isCanNotBeRemoved = true;
                    for (int aCurEdgeIdx = 0; aCurEdgeIdx < 3; ++aCurEdgeIdx)
                    {
                        if (anEdges[aCurEdgeIdx] != aFreeEdgeId)
                            continue;

                        for (int anOtherEdgeIt = 1; anOtherEdgeIt <= 2 && isCanNotBeRemoved; ++anOtherEdgeIt)
                        {
                            int anOtherEdgeId = (aCurEdgeIdx + anOtherEdgeIt) % 3;
                            BRepMesh_PairOfIndex anOtherEdgePair =
                              myMeshData.ElementsConnectedTo(anEdges[anOtherEdgeId]);

                            if (anOtherEdgePair.Extent() < 2)
                            {
                                isCanNotBeRemoved = false;
                            }
                            else
                            {
                                for (int aTriIdx = 1; aTriIdx <= anOtherEdgePair.Extent() && isCanNotBeRemoved; ++aTriIdx)
                                {
                                    if (anOtherEdgePair.Index(aTriIdx) == aTriId)
                                        continue;

                                    int[] v = new int[3];
                                    BRepMesh_Triangle aCurTriangle = GetTriangle(anOtherEdgePair.Index(aTriIdx));
                                    myMeshData.ElementNodes(aCurTriangle, v);
                                    for (int aNodeIdx = 0; aNodeIdx < 3 && isCanNotBeRemoved; ++aNodeIdx)
                                    {
                                        if (isSupVertex(v[aNodeIdx]))
                                        {
                                            isCanNotBeRemoved = false;
                                        }
                                    }
                                }
                            }
                        }

                        break;
                    }

                    if (isCanNotBeRemoved)
                        continue;

                    bool[] isConnected = { false, false };
                    for (int aLinkNodeIt = 0; aLinkNodeIt < 2; ++aLinkNodeIt)
                    {
                        isConnected[aLinkNodeIt] = isBoundToFrontier((aLinkNodeIt == 0) ?
                          anEdge.FirstNode() : anEdge.LastNode(), aFreeEdgeId);
                    }

                    if (!isConnected[0] || !isConnected[1])
                        aDelTriangles.Add(aTriId);
                }

                // Destruction of triangles :
                int aDeletedTrianglesNb = 0;

                foreach (var aDelTrianglesIt in aDelTriangles)
                {
                    deleteTriangle(aDelTrianglesIt, aLoopEdges);
                    aDeletedTrianglesNb++;
                }

                // Destruction of remaining hanging edges
                MapOfIntegerInteger.Iterator aLoopEdgesIt = new(aLoopEdges);
                for (; aLoopEdgesIt.More(); aLoopEdgesIt.Next())
                {
                    if (myMeshData.ElementsConnectedTo(aLoopEdgesIt.Key()).IsEmpty())
                        myMeshData.RemoveLink(aLoopEdgesIt.Key());
                }

                if (aDeletedTrianglesNb == 0)
                    break;
            }
        }

        private void deleteTriangle(int theIndex, MapOfIntegerInteger theLoopEdges)
        {
            if (myInitCircles)
            {
                myCircles.Delete(theIndex);
            }

            BRepMesh_Triangle aElement = GetTriangle(theIndex);
            int[] e = aElement.myEdges;
            bool[] o = aElement.myOrientations;

            myMeshData.RemoveElement(theIndex);

            for (int i = 0; i < 3; ++i)
            {
                if (!theLoopEdges.Bind(e[i], o[i] ? 1 : 0))
                {
                    theLoopEdges.UnBind(e[i]);
                    myMeshData.RemoveLink(e[i]);
                }
            }
        }

        bool isBoundToFrontier(
        int theRefNodeId,
        int theRefLinkId)
        {
            Stack<int> aLinkStack = new Stack<int>();
            TColStd_PackedMapOfInteger aVisitedLinks = new TColStd_PackedMapOfInteger();

            aLinkStack.Push(theRefLinkId);
            while (aLinkStack.Count() > 0)
            {
                int aCurrentLinkId = aLinkStack.Peek();
                aLinkStack.Pop();

                BRepMesh_PairOfIndex aPair = myMeshData.ElementsConnectedTo(aCurrentLinkId);
                if (aPair.IsEmpty())
                    return false;

                int aNbElements = aPair.Extent();
                for (int anElemIt = 1; anElemIt <= aNbElements; ++anElemIt)
                {
                    int aTriId = aPair.Index(anElemIt);
                    if (aTriId < 0)
                        continue;

                    BRepMesh_Triangle aElement = GetTriangle(aTriId);
                    var anEdges = aElement.myEdges;

                    for (int anEdgeIt = 0; anEdgeIt < 3; ++anEdgeIt)
                    {
                        int anEdgeId = anEdges[anEdgeIt];
                        if (anEdgeId == aCurrentLinkId)
                            continue;

                        BRepMesh_Edge anEdge = GetEdge(anEdgeId);
                        if (anEdge.FirstNode() != theRefNodeId &&
                            anEdge.LastNode() != theRefNodeId)
                        {
                            continue;
                        }

                        if (anEdge.Movability() != BRepMesh_DegreeOfFreedom.BRepMesh_Free)
                            return true;

                        if (aVisitedLinks.Add(anEdgeId))
                        {
                            aLinkStack.Push(anEdgeId);
                        }
                    }
                }
            }

            return false;
        }
        //! Gives edge with the given index
        public BRepMesh_Edge GetEdge(int theIndex)
        {
            return myMeshData.GetLink(theIndex);
        }

        //! Gives triangle with the given index
        public BRepMesh_Triangle GetTriangle(int theIndex)
        {
            return myMeshData.GetElement(theIndex);
        }

        //! Checks whether the given vertex id relates to super contour.
        bool isSupVertex(int theVertexIdx)
        {
            foreach (var aIt in mySupVert)
            {
                if (theVertexIdx == aIt)
                {
                    return true;
                }
            }

            return false;
        }

        //! Gives the list of internal edges.
        MapOfInteger InternalEdges()
        {
            return getEdgesByType(BRepMesh_DegreeOfFreedom.BRepMesh_Fixed);
        }

        private void insertInternalEdges()
        {

            {
                MapOfInteger anInternalEdges = InternalEdges();

                // Destruction of triangles intersecting internal edges
                // and their replacement by makeshift triangles
                IteratorOfMapOfInteger anInernalEdgesIt = new IteratorOfMapOfInteger(anInternalEdges);
                foreach (var aLinkIndex in anInternalEdges)
                {

                    BRepMesh_PairOfIndex aPair = myMeshData.ElementsConnectedTo(aLinkIndex);

                    // Check both sides of link for adjusted triangle.
                    bool[] isGo = { true, true };
                    for (int aTriangleIt = 1; aTriangleIt <= aPair.Extent(); ++aTriangleIt)
                    {
                        BRepMesh_Triangle aElement = GetTriangle(aPair.Index(aTriangleIt));
                        var e = aElement.myEdges;
                        var o = aElement.myOrientations;

                        for (int i = 0; i < 3; ++i)
                        {
                            if (e[i] == aLinkIndex)
                            {
                                isGo[o[i] ? 0 : 1] = false;
                                break;
                            }
                        }
                    }

                    if (isGo[0])
                    {
                        meshLeftPolygonOf(aLinkIndex, true);
                    }

                    if (isGo[1])
                    {
                        meshLeftPolygonOf(aLinkIndex, false);
                    }
                }
            }
        }
        const double AngDeviation1Deg = Math.PI / 180.0;

        const double AngDeviation90Deg = 90 * AngDeviation1Deg;

        static double _Precision = Precision.PConfusion();

        static double Precision2 = _Precision * _Precision;

        //=======================================================================
        //function : meshLeftPolygonOf
        //purpose  : Collect the polygon at the left of the given edge (material side)
        //=======================================================================
        bool meshLeftPolygonOf(
        int theStartEdgeId,
        bool isForward,
        MapOfInteger theSkipped = null)
        {
            if (theSkipped != null && theSkipped.Contains(theStartEdgeId))
                return true;

            BRepMesh_Edge aRefEdge = GetEdge(theStartEdgeId);

            SequenceOfInteger aPolygon = new SequenceOfInteger();
            int aStartNode, aPivotNode;
            if (isForward)
            {
                aPolygon.Append(theStartEdgeId);
                aStartNode = aRefEdge.FirstNode();
                aPivotNode = aRefEdge.LastNode();
            }
            else
            {
                aPolygon.Append(-theStartEdgeId);
                aStartNode = aRefEdge.LastNode();
                aPivotNode = aRefEdge.FirstNode();
            }

            BRepMesh_Vertex aStartEdgeVertexS = GetVertex(aStartNode);
            BRepMesh_Vertex aPivotVertex = GetVertex(aPivotNode);

            gp_Vec2d aRefLinkDir = new gp_Vec2d(aPivotVertex.Coord() -
              aStartEdgeVertexS.Coord());

            if (aRefLinkDir.SquareMagnitude() < Precision2)
                return true;

            // Auxiliary structures.
            // Bounding boxes of polygon links to be used for preliminary
            // analysis of intersections
            SequenceOfBndB2d aBoxes = new SequenceOfBndB2d();
            fillBndBox(aBoxes, aStartEdgeVertexS, aPivotVertex);

            // Hanging ends
            MapOfInteger aDeadLinks = new MapOfInteger();

            // Links are temporarily excluded from consideration
            MapOfInteger aLeprousLinks = new MapOfInteger();
            aLeprousLinks.Add(theStartEdgeId);

            bool isSkipLeprous = true;
            int aFirstNode = aStartNode;
            while (aPivotNode != aFirstNode)
            {
                Bnd_B2d aNextLinkBndBox = new Bnd_B2d();
                gp_Vec2d aNextLinkDir = new gp_Vec2d();
                int aNextPivotNode = 0;

                int aNextLinkId = findNextPolygonLink(
                  aFirstNode,
                  aPivotNode, aPivotVertex, aRefLinkDir,
                  aBoxes, aPolygon, theSkipped,
                  isSkipLeprous, aLeprousLinks, aDeadLinks,
        ref aNextPivotNode, ref aNextLinkDir, ref aNextLinkBndBox);

                if (aNextLinkId != 0)
                {
                    aStartNode = aPivotNode;
                    aRefLinkDir = aNextLinkDir;

                    aPivotNode = aNextPivotNode;
                    aPivotVertex = GetVertex(aNextPivotNode);

                    aBoxes.Append(aNextLinkBndBox);
                    aPolygon.Append(aNextLinkId);

                    isSkipLeprous = true;
                }
                else
                {
                    // Nothing to do
                    if (aPolygon.Length() == 1)
                        return false;

                    // Return to the previous point
                    int aDeadLinkId = Math.Abs(aPolygon.Last());
                    aDeadLinks.Add(aDeadLinkId);

                    aLeprousLinks.Remove(aDeadLinkId);
                    aPolygon.Remove(aPolygon.Length());
                    aBoxes.Remove(aBoxes.Length());

                    int aPrevLinkInfo = aPolygon.Last();
                    BRepMesh_Edge aPrevLink = GetEdge(Math.Abs(aPrevLinkInfo));

                    if (aPrevLinkInfo > 0)
                    {
                        aStartNode = aPrevLink.FirstNode();
                        aPivotNode = aPrevLink.LastNode();
                    }
                    else
                    {
                        aStartNode = aPrevLink.LastNode();
                        aPivotNode = aPrevLink.FirstNode();
                    }

                    aPivotVertex = GetVertex(aPivotNode);
                    aRefLinkDir =
                      new gp_Vec2d(aPivotVertex.Coord() - GetVertex(aStartNode).Coord());

                    isSkipLeprous = false;
                }
            }

            if (aPolygon.Length() < 3)
                return false;

            cleanupPolygon(aPolygon, aBoxes);
            meshPolygon(aPolygon, aBoxes, theSkipped);

            return true;
        }

        //=======================================================================
        //function : cleanupPolygon
        //purpose  : Remove internal triangles from the given polygon
        //=======================================================================
        void cleanupPolygon(SequenceOfInteger thePolygon,
                                     SequenceOfBndB2d thePolyBoxes)
        {
            int aPolyLen = thePolygon.Length();
            if (aPolyLen < 3)
                return;



            MapOfIntegerInteger aLoopEdges = new MapOfIntegerInteger(10);
            MapOfInteger anIgnoredEdges = new MapOfInteger();
            MapOfInteger aPolyVerticesFindMap = new MapOfInteger();
            VectorOfInteger aPolyVertices = new VectorOfInteger(256);
            // Collect boundary vertices of the polygon
            for (int aPolyIt = 1; aPolyIt <= aPolyLen; ++aPolyIt)
            {
                int aPolyEdgeInfo = thePolygon[aPolyIt];
                int aPolyEdgeId = Math.Abs(aPolyEdgeInfo);
                anIgnoredEdges.Add(aPolyEdgeId);

                bool isForward = (aPolyEdgeInfo > 0);
                BRepMesh_PairOfIndex aPair =
              myMeshData.ElementsConnectedTo(aPolyEdgeId);

                int anElemIt = 1;
                for (; anElemIt <= aPair.Extent(); ++anElemIt)
                {
                    int anElemId = aPair.Index(anElemIt);
                    if (anElemId < 0)
                        continue;

                    BRepMesh_Triangle aElement = GetTriangle(anElemId);
                    int[] anEdges = aElement.myEdges;
                    bool[] anEdgesOri = aElement.myOrientations;

                    bool isTriangleFound = false;
                    for (int anEdgeIt = 0; anEdgeIt < 3; ++anEdgeIt)
                    {
                        if (anEdges[anEdgeIt] == aPolyEdgeId &&
                             anEdgesOri[anEdgeIt] == isForward)
                        {
                            isTriangleFound = true;
                            deleteTriangle(anElemId, aLoopEdges);
                            break;
                        }
                    }

                    if (isTriangleFound)
                        break;
                }

                // Skip a neighbor link to extract unique vertices each time
                if (aPolyIt % 2 != 0)
                {
                    BRepMesh_Edge aPolyEdge = GetEdge(aPolyEdgeId);
                    int aFirstVertex = aPolyEdge.FirstNode();
                    int aLastVertex = aPolyEdge.LastNode();

                    aPolyVerticesFindMap.Add(aFirstVertex);
                    aPolyVerticesFindMap.Add(aLastVertex);

                    if (aPolyEdgeInfo > 0)
                    {
                        aPolyVertices.Append(aFirstVertex);
                        aPolyVertices.Append(aLastVertex);
                    }
                    else
                    {
                        aPolyVertices.Append(aLastVertex);
                        aPolyVertices.Append(aFirstVertex);
                    }
                }
            }

            // Make closed sequence
            if (aPolyVertices.First() != aPolyVertices.Last())
                aPolyVertices.Append(aPolyVertices.First());

            MapOfInteger aSurvivedLinks = new MapOfInteger(anIgnoredEdges);

            int aPolyVertIt = 0;
            int anUniqueVerticesNum = aPolyVertices.Length() - 1;
            for (; aPolyVertIt < anUniqueVerticesNum; ++aPolyVertIt)
            {
                killTrianglesAroundVertex(aPolyVertices[aPolyVertIt],
                  aPolyVertices, aPolyVerticesFindMap, thePolygon,
                  thePolyBoxes, aSurvivedLinks, aLoopEdges);
            }

            MapOfIntegerInteger.Iterator aLoopEdgesIt = new NCollection_DataMap<int, int, NCollection_DefaultHasher<int>>.Iterator(aLoopEdges);
            for (; aLoopEdgesIt.More(); aLoopEdgesIt.Next())
            {
                int aLoopEdgeId = aLoopEdgesIt.Key();
                if (anIgnoredEdges.Contains(aLoopEdgeId))
                    continue;

                if (myMeshData.ElementsConnectedTo(aLoopEdgeId).IsEmpty())
                    myMeshData.RemoveLink(aLoopEdgesIt.Key());
            }
        }


        //=======================================================================
        //function : meshElementaryPolygon
        //purpose  : Triangulation of closed polygon containing only three edges.
        //=======================================================================
        bool meshElementaryPolygon(
        SequenceOfInteger thePolygon)
        {
            int aPolyLen = thePolygon.Length();
            if (aPolyLen < 3)
                return true;
            else if (aPolyLen > 3)
                return false;

            // Just create a triangle
            int[] anEdges = new int[3];
            bool[] anEdgesOri = new bool[3];

            for (int anEdgeIt = 0; anEdgeIt < 3; ++anEdgeIt)
            {
                int anEdgeInfo = thePolygon[anEdgeIt + 1];
                anEdges[anEdgeIt] = Math.Abs(anEdgeInfo);
                anEdgesOri[anEdgeIt] = (anEdgeInfo > 0);
            }

            BRepMesh_Edge anEdge1 = GetEdge(anEdges[0]);
            BRepMesh_Edge anEdge2 = GetEdge(anEdges[1]);

            int[] aNodes = { anEdge1.FirstNode(),
                                 anEdge1.LastNode(),
                                 anEdge2.FirstNode() };
            if (aNodes[2] == aNodes[0] ||
                 aNodes[2] == aNodes[1])
            {
                aNodes[2] = anEdge2.LastNode();
            }

            addTriangle(anEdges, anEdgesOri, aNodes);
            return true;
        }


        //! Decomposes the given closed simple polygon (polygon without glued edges 
        //! and loops) on two simpler ones by adding new link at the most thin part 
        //! in respect to end point of the first link.
        //! In case if source polygon consists of three links, creates new triangle 
        //! and clears source container.
        //! @param thePolygon source polygon to be decomposed (first part of decomposition).
        //! @param thePolyBoxes bounding boxes corresponded to source polygon's links.
        //! @param thePolygonCut product of decomposition of source polygon (second part of decomposition).
        //! @param thePolyBoxesCut bounding boxes corresponded to resulting polygon's links.
        public void decomposeSimplePolygon(
          SequenceOfInteger thePolygon,
          SequenceOfBndB2d thePolyBoxes,
          SequenceOfInteger thePolygonCut,
          SequenceOfBndB2d thePolyBoxesCut)
        {
            // Check is the given polygon elementary
            if (meshElementaryPolygon(thePolygon))
            {
                thePolygon.Clear();
                thePolyBoxes.Clear();
                return;
            }
            // Polygon contains more than 3 links
            int aFirstEdgeInfo = thePolygon[1];
            BRepMesh_Edge aFirstEdge = GetEdge(Math.Abs(aFirstEdgeInfo));

            int[] aNodes = new int[3];
            getOrientedNodes(aFirstEdge, aFirstEdgeInfo > 0, aNodes);

            gp_Pnt2d[] aRefVertices = new gp_Pnt2d[3];
            aRefVertices[0] = new gp_Pnt2d(GetVertex(aNodes[0]).Coord());
            aRefVertices[1] = new gp_Pnt2d(GetVertex(aNodes[1]).Coord());

            gp_Vec2d aRefEdgeDir = new gp_Vec2d(aRefVertices[0], aRefVertices[1]);

            double aRefEdgeLen = aRefEdgeDir.Magnitude();
            if (aRefEdgeLen < _Precision)
            {
                thePolygon.Clear();
                thePolyBoxes.Clear();
                return;
            }

            aRefEdgeDir /= aRefEdgeLen;

            // Find a point with minimum distance respect
            // the end of reference link
            int aUsedLinkId = 0;
            double aOptAngle = 0.0;
            double aMinDist = Standard_Real.RealLast();
            int aPivotNode = aNodes[1];
            int aPolyLen = thePolygon.Length();
            for (int aLinkIt = 3; aLinkIt <= aPolyLen; ++aLinkIt)
            {
                int aLinkInfo = thePolygon[aLinkIt];
                BRepMesh_Edge aNextEdge = GetEdge(Math.Abs(aLinkInfo));

                aPivotNode = aLinkInfo > 0 ?
                    aNextEdge.FirstNode() :
                    aNextEdge.LastNode();

                //We have end points touch case in the polygon -ignore it
                if (aPivotNode == aNodes[1])
                    continue;

                gp_Pnt2d aPivotVertex = new gp_Pnt2d(GetVertex(aPivotNode).Coord());
                gp_Vec2d aDistanceDir = new(aRefVertices[1], aPivotVertex);

                double aDist = aRefEdgeDir ^ aDistanceDir;
                double aAngle = Math.Abs(aRefEdgeDir.Angle(aDistanceDir));
                double anAbsDist = Math.Abs(aDist);
                if (anAbsDist < _Precision || aDist < 0.0)
                    continue;

                if ((anAbsDist >= aMinDist) &&
                    (aAngle <= aOptAngle || aAngle > AngDeviation90Deg))
                {
                    continue;
                }

                //        Check is the test link crosses the polygon boundaries
                bool isIntersect = false;
                for (int aRefLinkNodeIt = 0; aRefLinkNodeIt < 2; ++aRefLinkNodeIt)
                {
                    int aLinkFirstNode = aNodes[aRefLinkNodeIt];
                    gp_Pnt2d aLinkFirstVertex = aRefVertices[aRefLinkNodeIt];

                    Bnd_B2d aBox = new Bnd_B2d();
                    UpdateBndBox(aLinkFirstVertex.Coord(), aPivotVertex.Coord(), aBox);

                    BRepMesh_Edge aCheckLink = new BRepMesh_Edge(aLinkFirstNode, aPivotNode, BRepMesh_DegreeOfFreedom.BRepMesh_Free);

                    int aCheckLinkIt = 2;
                    for (; aCheckLinkIt <= aPolyLen; ++aCheckLinkIt)
                    {
                        if (aCheckLinkIt == aLinkIt)
                            continue;

                        //                if (!aBox.IsOut(thePolyBoxes.Value(aCheckLinkIt)))
                        //                {
                        //                    const BRepMesh_Edge&aPolyLink =
                        //                        GetEdge(Abs(thePolygon(aCheckLinkIt)));

                        //                    if (aCheckLink.IsEqual(aPolyLink))
                        //                        continue;

                        //                    intersection is possible...
                        //                   gp_Pnt2d anIntPnt;
                        //                    BRepMesh_GeomTool::IntFlag aIntFlag = intSegSeg(aCheckLink, aPolyLink,
                        //                        Standard_False, Standard_False, anIntPnt);

                        //                    if (aIntFlag != BRepMesh_GeomTool::NoIntersection)
                        //                    {
                        //                        isIntersect = Standard_True;
                        //                        break;
                        //                    }
                        //                }
                    }

                    if (isIntersect)
                        break;
                }

                if (isIntersect)
                    continue;


                aOptAngle = aAngle;
                aMinDist = anAbsDist;
                aNodes[2] = aPivotNode;
                aRefVertices[2] = aPivotVertex;
                aUsedLinkId = aLinkIt;
            }
            if (aUsedLinkId == 0)
            {
                thePolygon.Clear();
                thePolyBoxes.Clear();
                return;
            }
            BRepMesh_Edge[] aNewEdges = {
     new  BRepMesh_Edge(aNodes[1], aNodes[2],BRepMesh_DegreeOfFreedom. BRepMesh_Free),
     new BRepMesh_Edge(aNodes[2], aNodes[0], BRepMesh_DegreeOfFreedom.BRepMesh_Free) };

            int[] aNewEdgesInfo = {
      aFirstEdgeInfo,
      myMeshData.AddLink(aNewEdges[0]),
      myMeshData.AddLink(aNewEdges[1]) };

            int[] anEdges = new int[3];
            bool[] anEdgesOri = new bool[3];
            for (int aTriEdgeIt = 0; aTriEdgeIt < 3; ++aTriEdgeIt)
            {
                int anEdgeInfo = aNewEdgesInfo[aTriEdgeIt];
                anEdges[aTriEdgeIt] = Math.Abs(anEdgeInfo);
                anEdgesOri[aTriEdgeIt] = anEdgeInfo > 0;
            }
            addTriangle(anEdges, anEdgesOri, aNodes);

            if (aUsedLinkId == 3)
            {
                thePolygon.Remove(1);
                thePolyBoxes.Remove(1);

                thePolygon.SetValue(1, -aNewEdgesInfo[2]);

                Bnd_B2d aBox = new Bnd_B2d();
                UpdateBndBox(aRefVertices[0].Coord(), aRefVertices[2].Coord(), aBox);
                thePolyBoxes.SetValue(1, aBox);
            }
            else
            {
                // Create triangle and split the source polygon on two 
                // parts (if possible) and mesh each part as independent
                // polygon.
                if (aUsedLinkId < aPolyLen)
                {
                    thePolygon.Split(aUsedLinkId, thePolygonCut);
                    thePolygonCut.Prepend(-aNewEdgesInfo[2]);
                    thePolyBoxes.Split(aUsedLinkId, thePolyBoxesCut);

                    Bnd_B2d aBox1 = new Bnd_B2d();
                    UpdateBndBox(aRefVertices[0].Coord(), aRefVertices[2].Coord(), aBox1);
                    thePolyBoxesCut.Prepend(aBox1);
                }
                else
                {
                    thePolygon.Remove(aPolyLen);
                    thePolyBoxes.Remove(aPolyLen);
                }

                thePolygon.SetValue(1, -aNewEdgesInfo[1]);

                Bnd_B2d aBox = new Bnd_B2d();
                UpdateBndBox(aRefVertices[1].Coord(), aRefVertices[2].Coord(), aBox);
                thePolyBoxes.SetValue(1, aBox);
            }
        }


        //=============================================================================
        //function : polyArea
        //purpose  : Returns area of the loop of the given polygon defined by indices 
        //           of its start and end links.
        //=============================================================================
        double polyArea(SequenceOfInteger thePolygon,
    int theStartIndex, int theEndIndex)
        {

            double aArea = 0.0;
            int aPolyLen = thePolygon.Length();
            if (theStartIndex >= theEndIndex ||
                theStartIndex > aPolyLen)
            {
                return aArea;
            }
            int aCurEdgeInfo = thePolygon[theStartIndex];
            int aCurEdgeId = Math.Abs(aCurEdgeInfo);
            BRepMesh_Edge aCurEdge = GetEdge(aCurEdgeId);

            int[] aNodes = new int[2];
            getOrientedNodes(aCurEdge, aCurEdgeInfo > 0, aNodes);

            gp_Pnt2d aRefPnt = new gp_Pnt2d(GetVertex(aNodes[0]).Coord());
            int aPolyIt = theStartIndex + 1;
            for (; aPolyIt <= theEndIndex; ++aPolyIt)
            {
                aCurEdgeInfo = thePolygon[aPolyIt];
                aCurEdgeId = Math.Abs(aCurEdgeInfo);
                aCurEdge = GetEdge(aCurEdgeId);

                getOrientedNodes(aCurEdge, aCurEdgeInfo > 0, aNodes);
                gp_Vec2d aVec1 = new gp_Vec2d(aRefPnt, new gp_Pnt2d(GetVertex(aNodes[0]).Coord()));
                gp_Vec2d aVec2 = new gp_Vec2d(aRefPnt, new gp_Pnt2d(GetVertex(aNodes[1]).Coord()));

                aArea += aVec1 ^ aVec2;
            }

            return aArea / 2.0;
        }
        //=======================================================================
        //function : getOrientedNodes
        //purpose  : Returns start and end nodes of the given edge in respect to 
        //           its orientation.
        //=======================================================================
        void getOrientedNodes(BRepMesh_Edge theEdge,
     bool isForward,
    int[] theNodes)
        {
            if (isForward)
            {
                theNodes[0] = theEdge.FirstNode();
                theNodes[1] = theEdge.LastNode();
            }
            else
            {
                theNodes[0] = theEdge.LastNode();
                theNodes[1] = theEdge.FirstNode();
            }
        }

        //! Triangulatiion of a closed polygon described by the list
        //! of indexes of its edges in the structure.
        //! (negative index means reversed edge)
        void meshPolygon(SequenceOfInteger thePolygon,
                                  SequenceOfBndB2d thePolyBoxes,
                                  MapOfInteger theSkipped = null)
        {
            // Check is the source polygon elementary
            if (meshElementaryPolygon(thePolygon))
                return;


            // Check and correct boundary edges
            int aPolyLen = thePolygon.Length();
            double aPolyArea = Math.Abs(polyArea(thePolygon, 1, aPolyLen));
            double aSmallLoopArea = 0.001 * aPolyArea;
            for (int aPolyIt = 1; aPolyIt < aPolyLen; ++aPolyIt)
            {
                int aCurEdgeInfo = thePolygon[aPolyIt];
                int aCurEdgeId = Math.Abs(aCurEdgeInfo);
                BRepMesh_Edge aCurEdge = GetEdge(aCurEdgeId);
                if (aCurEdge.Movability() != BRepMesh_DegreeOfFreedom.BRepMesh_Frontier)
                    continue;

                int[] aCurNodes = new int[2];
                getOrientedNodes(aCurEdge, aCurEdgeInfo > 0, aCurNodes);

                gp_Pnt2d[] aCurPnts = [
               new   gp_Pnt2d(  GetVertex(aCurNodes[0]).Coord()),
                new gp_Pnt2d(  GetVertex(aCurNodes[1]).Coord())

                  ];

                gp_Vec2d aCurVec = new gp_Vec2d(aCurPnts[0], aCurPnts[1]);

                // check further links
                int aNextPolyIt = aPolyIt + 1;
                for (; aNextPolyIt <= aPolyLen; ++aNextPolyIt)
                {
                    int aNextEdgeInfo = thePolygon[aNextPolyIt];
                    int aNextEdgeId = Math.Abs(aNextEdgeInfo);
                    BRepMesh_Edge aNextEdge = GetEdge(aNextEdgeId);
                    if (aNextEdge.Movability() != BRepMesh_DegreeOfFreedom.BRepMesh_Frontier)
                        continue;

                    int[] aNextNodes = new int[2];
                    getOrientedNodes(aNextEdge, aNextEdgeInfo > 0, aNextNodes);

                    gp_Pnt2d[] aNextPnts = new gp_Pnt2d[2] {
                       new  gp_Pnt2d(GetVertex(aNextNodes[0]).Coord()),
                    new gp_Pnt2d(    GetVertex(aNextNodes[1]).Coord())
                      };

                    gp_Pnt2d anIntPnt = new gp_Pnt2d();
                    IntFlag aIntFlag = intSegSeg(aCurEdge, aNextEdge,
                      false, true, ref anIntPnt);

                    if (aIntFlag == IntFlag.NoIntersection)
                        continue;

                    bool isRemoveFromFirst = false;
                    bool isAddReplacingEdge = true;
                    int aIndexToRemoveTo = aNextPolyIt;
                    if (aIntFlag == IntFlag.Cross)
                    {
                        //                    Standard_Real aLoopArea = polyArea(thePolygon, aPolyIt + 1, aNextPolyIt);
                        //                    gp_Vec2d aVec1(anIntPnt, aCurPnts[1] );
                        //                    gp_Vec2d aVec2(anIntPnt, aNextPnts[0] );

                        //                    aLoopArea += (aVec1 ^ aVec2) / 2.;
                        //                    if (Abs(aLoopArea) > aSmallLoopArea)
                        //                    {
                        //                        aNextNodes[1] = aCurNodes[0];
                        //                        aNextPnts[1] = aCurPnts[0];

                        //                        aNextEdgeId = Abs(createAndReplacePolygonLink(aNextNodes, aNextPnts,
                        //                          aNextPolyIt, BRepMesh_Delaun::Replace, thePolygon, thePolyBoxes));

                        //                        processLoop(aPolyIt, aNextPolyIt, thePolygon, thePolyBoxes);
                        //                        return;
                        //                    }

                        //                    Standard_Real aDist1 = anIntPnt.SquareDistance(aNextPnts[0]);
                        //                    Standard_Real aDist2 = anIntPnt.SquareDistance(aNextPnts[1]);

                        //                    // Choose node with lower distance
                        //                    const Standard_Boolean isCloseToStart = (aDist1 < aDist2);
                        //                    const Standard_Integer aEndPointIndex = isCloseToStart ? 0 : 1;
                        //                    aCurNodes[1] = aNextNodes[aEndPointIndex];
                        //                    aCurPnts[1] = aNextPnts[aEndPointIndex];

                        //                    if (isCloseToStart)
                        //                        --aIndexToRemoveTo;

                        //                    // In this context only intersections between frontier edges
                        //                    // are possible. If intersection between edges of different
                        //                    // types occurred - treat this case as invalid (i.e. result 
                        //                    // might not reflect the expectations).
                        //                    if (!theSkipped.IsNull())
                        //                    {
                        //                        Standard_Integer aSkippedLinkIt = aPolyIt;
                        //                        for (; aSkippedLinkIt <= aIndexToRemoveTo; ++aSkippedLinkIt)
                        //                            theSkipped->Add(Abs(thePolygon(aSkippedLinkIt)));
                        //                    }
                    }
                    else if (aIntFlag == IntFlag.PointOnSegment)
                    {
                        //                    // Identify chopping link 
                        //                    Standard_Boolean isFirstChopping = Standard_False;
                        //                    Standard_Integer aCheckPointIt = 0;
                        //                    for (; aCheckPointIt < 2; ++aCheckPointIt)
                        //                    {
                        //                        gp_Pnt2d & aRefPoint = aCurPnts[aCheckPointIt];
                        //                        // Check is second link touches the first one
                        //                        gp_Vec2d aVec1(aRefPoint, aNextPnts[0] );
                        //                        gp_Vec2d aVec2(aRefPoint, aNextPnts[1] );
                        //                        if (Abs(aVec1 ^ aVec2) < Precision)
                        //                        {
                        //                            isFirstChopping = Standard_True;
                        //                            break;
                        //                        }
                        //                    }

                        //                    if (isFirstChopping)
                        //                    {
                        //                        // Split second link
                        //                        isAddReplacingEdge = Standard_False;
                        //                        isRemoveFromFirst = (aCheckPointIt == 0);

                        //                        Standard_Integer aSplitLink[3] = {
                        //        aNextNodes[0],
                        //        aCurNodes [aCheckPointIt],
                        //        aNextNodes[1]
                        //      };

                        //                        gp_Pnt2d aSplitPnts[3] = {
                        //        aNextPnts[0],
                        //        aCurPnts [aCheckPointIt],
                        //        aNextPnts[1]
                        //      };

                        //                        Standard_Integer aSplitLinkIt = 0;
                        //                        for (; aSplitLinkIt < 2; ++aSplitLinkIt)
                        //                        {
                        //                            createAndReplacePolygonLink(&aSplitLink[aSplitLinkIt],
                        //                              &aSplitPnts[aSplitLinkIt], aNextPolyIt, (aSplitLinkIt == 0) ?
                        //                              BRepMesh_Delaun::Replace : BRepMesh_Delaun::InsertAfter,
                        //                              thePolygon, thePolyBoxes);
                        //                        }

                        //                        processLoop(aPolyIt + aCheckPointIt, aIndexToRemoveTo,
                        //                          thePolygon, thePolyBoxes);
                        //                    }
                        //                    else
                        //                    {
                        //                        // Split first link
                        //                        Standard_Integer aSplitLinkNodes[2] = {
                        //        aNextNodes[1],
                        //        aCurNodes [1]
                        //      };

                        //                        gp_Pnt2d aSplitLinkPnts[2] = {
                        //        aNextPnts[1],
                        //        aCurPnts [1]
                        //      };
                        //                        createAndReplacePolygonLink(aSplitLinkNodes, aSplitLinkPnts,
                        //                          aPolyIt, BRepMesh_Delaun::InsertAfter, thePolygon, thePolyBoxes);

                        //                        aCurNodes[1] = aNextNodes[1];
                        //                        aCurPnts[1] = aNextPnts[1];
                        //                        ++aIndexToRemoveTo;

                        //                        processLoop(aPolyIt + 1, aIndexToRemoveTo,
                        //                          thePolygon, thePolyBoxes);
                        //                    }
                    }
                    else if (aIntFlag == IntFlag.Glued)
                    {
                        //                    if (aCurNodes[1] == aNextNodes[0])
                        //                    {
                        //                        aCurNodes[1] = aNextNodes[1];
                        //                        aCurPnts[1] = aNextPnts[1];
                        //                    }
                        //                    // TODO: Non-adjacent glued links within the polygon
                    }
                    else if (aIntFlag == IntFlag.Same)
                    {
                        //                    processLoop(aPolyIt, aNextPolyIt, thePolygon, thePolyBoxes);

                        //                    isRemoveFromFirst = Standard_True;
                        //                    isAddReplacingEdge = Standard_False;
                    }
                    else
                        continue; // Not supported type

                    //                if (isAddReplacingEdge)
                    //                {
                    //                    aCurEdgeId = Abs(createAndReplacePolygonLink(aCurNodes, aCurPnts,
                    //                      aPolyIt, BRepMesh_Delaun::Replace, thePolygon, thePolyBoxes));

                    //                    aCurEdge = &GetEdge(aCurEdgeId);
                    //                    aCurVec = gp_Vec2d(aCurPnts[0], aCurPnts[1]);
                    //                }

                    //                Standard_Integer aIndexToRemoveFrom =
                    //                  isRemoveFromFirst ? aPolyIt : aPolyIt + 1;

                    //                thePolygon.Remove(aIndexToRemoveFrom, aIndexToRemoveTo);
                    //                thePolyBoxes.Remove(aIndexToRemoveFrom, aIndexToRemoveTo);

                    //                aPolyLen = thePolygon.Length();
                    //                if (isRemoveFromFirst)
                    //                {
                    //                    --aPolyIt;
                    //                    break;
                    //                }

                    //                aNextPolyIt = aPolyIt;
                }
            }

            SequenceOfInteger aPolygon1 = thePolygon;
            SequenceOfBndB2d aPolyBoxes1 = thePolyBoxes;

            SequenceOfInteger aPolygon2 = new SequenceOfInteger();
            SequenceOfBndB2d aPolyBoxes2 = new SequenceOfBndB2d();

            NCollection_Sequence<SequenceOfInteger> aPolyStack = new NCollection_Sequence<SequenceOfInteger>();
            NCollection_Sequence<SequenceOfBndB2d> aPolyBoxStack = new NCollection_Sequence<SequenceOfBndB2d>();
            for (; ; )
            {
                decomposeSimplePolygon(aPolygon1, aPolyBoxes1, aPolygon2, aPolyBoxes2);
                //            if (!aPolygon2->IsEmpty())
                //            {
                //                aPolyStack.Append(aPolygon2);
                //                aPolyBoxStack.Append(aPolyBoxes2);

                //                aPolygon2 = new IMeshData::SequenceOfInteger;
                //                aPolyBoxes2 = new IMeshData::SequenceOfBndB2d;
                //            }

                //            if (aPolygon1->IsEmpty())
                //            {
                //                if (!aPolyStack.IsEmpty() && aPolygon1 == &(*aPolyStack.First()))
                //                {
                //                    aPolyStack.Remove(1);
                //                    aPolyBoxStack.Remove(1);
                //                }

                if (aPolyStack.IsEmpty())
                    break;

                //                aPolygon1 = &(*aPolyStack.ChangeFirst());
                //                aPolyBoxes1 = &(*aPolyBoxStack.ChangeFirst());
                //            }
            }
        }



        //=======================================================================
        //function : fillBndBox
        //purpose  : Add bounding box for edge defined by start & end point to
        //           the given vector of bounding boxes for triangulation edges
        //=======================================================================
        void fillBndBox(SequenceOfBndB2d theBoxes,
                                 BRepMesh_Vertex theV1,
                                 BRepMesh_Vertex theV2)
        {
            Bnd_B2d aBox = new Bnd_B2d();
            UpdateBndBox(theV1.Coord(), theV2.Coord(), aBox);
            theBoxes.Append(aBox);
        }
        void UpdateBndBox(gp_XY thePnt1, gp_XY thePnt2, Bnd_B2d theBox)
        {
            theBox.Add(thePnt1);
            theBox.Add(thePnt2);
            theBox.Enlarge(_Precision);
        }

        //=======================================================================
        //function : findNextPolygonLink
        //purpose  : Find next link starting from the given node and has maximum
        //           angle respect the given reference link.
        //           Each time the next link is found other neighbor links at the 
        //           pivot node are marked as leprous and will be excluded from 
        //           consideration next time until a hanging end is occurred.
        //=======================================================================
        int findNextPolygonLink(
        int theFirstNode,
        int thePivotNode,
        BRepMesh_Vertex thePivotVertex,
        gp_Vec2d theRefLinkDir,
        SequenceOfBndB2d theBoxes,
        SequenceOfInteger thePolygon,
        MapOfInteger theSkipped,
        bool isSkipLeprous,
        MapOfInteger theLeprousLinks,
        MapOfInteger theDeadLinks,
        ref int theNextPivotNode,
        ref gp_Vec2d theNextLinkDir,
        ref Bnd_B2d theNextLinkBndBox)
        {
            // Find the next link having the greatest angle 
            // respect to a direction of a reference one
            double aMaxAngle = Standard_Real.RealFirst();

            int aNextLinkId = 0;
            //ListOfInteger::Iterator aLinkIt( );
            foreach (var aLinkIt in myMeshData.LinksConnectedTo(thePivotNode))
            {


                //for (; aLinkIt.More(); aLinkIt.Next())
                //{
                int aNeighbourLinkInfo = aLinkIt;
                int aNeighbourLinkId = Math.Abs(aNeighbourLinkInfo);

                if (theDeadLinks.Contains(aNeighbourLinkId) ||
                   (theSkipped != null && theSkipped.Contains(aNeighbourLinkId)))
                {
                    continue;
                }

                bool isLeprous = theLeprousLinks.Contains(aNeighbourLinkId);
                if (isSkipLeprous && isLeprous)
                    continue;

                BRepMesh_Edge aNeighbourLink = GetEdge(aNeighbourLinkId);

                // Determine whether the link belongs to the mesh
                if (aNeighbourLink.Movability() == BRepMesh_DegreeOfFreedom.BRepMesh_Free &&
                     myMeshData.ElementsConnectedTo(aNeighbourLinkInfo).IsEmpty())
                {
                    theDeadLinks.Add(aNeighbourLinkId);
                    continue;
                }

                int anOtherNode = aNeighbourLink.FirstNode();
                if (anOtherNode == thePivotNode)
                    anOtherNode = aNeighbourLink.LastNode();

                gp_Vec2d aCurLinkDir = new gp_Vec2d(GetVertex(anOtherNode).Coord() -
                  thePivotVertex.Coord());

                if (aCurLinkDir.SquareMagnitude() < Precision2)
                {
                    theDeadLinks.Add(aNeighbourLinkId);
                    continue;
                }

                if (!isLeprous)
                    theLeprousLinks.Add(aNeighbourLinkId);

                double anAngle = theRefLinkDir.Angle(aCurLinkDir);
                bool isFrontier =
                  (aNeighbourLink.Movability() == BRepMesh_DegreeOfFreedom.BRepMesh_Frontier);

                bool isCheckPointOnEdge = true;
                if (isFrontier)
                {
                    if (Math.Abs(Math.Abs(anAngle) - Math.PI) < Precision.Angular())
                    {
                        // Glued constrains - don't check intersection
                        isCheckPointOnEdge = false;
                        anAngle = Math.Abs(anAngle);
                    }
                }

                if (anAngle <= aMaxAngle)
                    continue;

                bool isCheckEndPoints = (anOtherNode != theFirstNode);

                Bnd_B2d aBox = new Bnd_B2d();
                bool isNotIntersect =
                  checkIntersection(aNeighbourLink, thePolygon, theBoxes,
                  isCheckEndPoints, isCheckPointOnEdge, true, aBox);

                if (isNotIntersect)
                {
                    aMaxAngle = anAngle;

                    theNextLinkDir = aCurLinkDir;
                    theNextPivotNode = anOtherNode;
                    theNextLinkBndBox = aBox;

                    aNextLinkId = (aNeighbourLink.FirstNode() == thePivotNode) ?
                      aNeighbourLinkId : -aNeighbourLinkId;
                }
            }

            return aNextLinkId;
        }


        //=======================================================================
        //function : checkIntersection
        //purpose  : Check is the given link intersects the polygon boundaries.
        //           Returns bounding box for the given link through the
        //           <theLinkBndBox> parameter.
        //=======================================================================
        bool checkIntersection(
        BRepMesh_Edge theLink,
        SequenceOfInteger thePolygon,
        SequenceOfBndB2d thePolyBoxes,
        bool isConsiderEndPointTouch,
        bool isConsiderPointOnEdge,
        bool isSkipLastEdge,
        Bnd_B2d theLinkBndBox)
        {
            UpdateBndBox(GetVertex(theLink.FirstNode()).Coord(),
              GetVertex(theLink.LastNode()).Coord(), theLinkBndBox);

            int aPolyLen = thePolygon.Length();
            // Don't check intersection with the last link
            if (isSkipLastEdge)
                --aPolyLen;

            bool isFrontier =
              (theLink.Movability() == BRepMesh_DegreeOfFreedom.BRepMesh_Frontier);

            for (int aPolyIt = 1; aPolyIt <= aPolyLen; ++aPolyIt)
            {
                if (!theLinkBndBox.IsOut(thePolyBoxes.Value(aPolyIt)))
                {
                    // intersection is possible...
                    int aPolyLinkId = Math.Abs(thePolygon[aPolyIt]);
                    BRepMesh_Edge aPolyLink = GetEdge(aPolyLinkId);

                    // skip intersections between frontier edges
                    if (aPolyLink.Movability() == BRepMesh_DegreeOfFreedom.BRepMesh_Frontier && isFrontier)
                        continue;

                    gp_Pnt2d anIntPnt = new gp_Pnt2d();
                    IntFlag aIntFlag = intSegSeg(theLink, aPolyLink,
                      isConsiderEndPointTouch, isConsiderPointOnEdge, ref anIntPnt);

                    if (aIntFlag != IntFlag.NoIntersection)
                        return false;
                }
            }

            // Ok, no intersection
            return true;
        }
        //=============================================================================
        //function : intSegSeg
        //purpose  : Checks intersection between the two segments.
        //=============================================================================
        IntFlag intSegSeg(
        BRepMesh_Edge theEdg1,
        BRepMesh_Edge theEdg2,
        bool isConsiderEndPointTouch,
        bool isConsiderPointOnEdge,
        ref gp_Pnt2d theIntPnt)
        {
            gp_XY p1, p2, p3, p4;
            p1 = GetVertex(theEdg1.FirstNode()).Coord();
            p2 = GetVertex(theEdg1.LastNode()).Coord();
            p3 = GetVertex(theEdg2.FirstNode()).Coord();
            p4 = GetVertex(theEdg2.LastNode()).Coord();

            return BRepMesh_GeomTool.IntSegSeg(p1, p2, p3, p4,
              isConsiderEndPointTouch, isConsiderPointOnEdge, ref theIntPnt);
        }

        VectorOfInteger mySupVert;
        bool myInitCircles;
        BRepMesh_DataStructureOfDelaun myMeshData;
        BRepMesh_CircleTool myCircles;

        //! Explicitly sets ids of auxiliary vertices used to build mesh and used by 3rd-party algorithms.
        public void SetAuxVertices(VectorOfInteger theSupVert)
        {
            mySupVert = theSupVert;
        }
        //! Creates the triangulation with an existant Mesh data structure.
        public BRepMesh_Delaun(BRepMesh_DataStructureOfDelaun theOldMesh,
                                   VectorOfInteger theVertexIndices,
                                   int theCellsCountU,
                                   int theCellsCountV)
        {
            myMeshData = (theOldMesh);
            myCircles = new BRepMesh_CircleTool(theVertexIndices.Length());
            //, new NCollection_IncAllocator(
            //         IMeshData::MEMORY_BLOCK_SIZE_HUGE)),
            mySupVert = new VectorOfInteger(3);
            myInitCircles = false;

            perform(theVertexIndices, theCellsCountU, theCellsCountV);
        }
        //! Gives vertex with the given index
        BRepMesh_Vertex GetVertex(int theIndex)
        {
            return myMeshData.GetNode(theIndex);
        }



        //=======================================================================
        //function : perform
        //purpose  : Create super mesh and run triangulation procedure
        //=======================================================================
        void perform(VectorOfInteger theVertexIndices,
                              int theCellsCountU /* = -1 */,
                              int theCellsCountV /* = -1 */)
        {
            if (theVertexIndices.Length() <= 2)
            {
                return;
            }

            Bnd_Box2d aBox = new Bnd_Box2d();
            int anIndex = theVertexIndices.Lower();
            int anUpper = theVertexIndices.Upper();
            for (; anIndex <= anUpper; ++anIndex)
            {
                aBox.Add(new gp_Pnt2d(GetVertex(theVertexIndices[anIndex]).Coord()));
            }

            aBox.Enlarge(_Precision);

            initCirclesTool(aBox, theCellsCountU, theCellsCountV);
            superMesh(aBox);

            ComparatorOfIndexedVertexOfDelaun aCmp = new ComparatorOfIndexedVertexOfDelaun(myMeshData);
            Std.make_heap(theVertexIndices, aCmp);
            Std.sort_heap(theVertexIndices, aCmp);

            compute(theVertexIndices);
        }

        void initCirclesTool(Bnd_Box2d theBox,
        int theCellsCountU,
        int theCellsCountV)
        {

            double aMinX = 0, aMinY = 0, aMaxX = 0, aMaxY = 0;
            theBox.Get(ref aMinX, ref aMinY, ref aMaxX, ref aMaxY);
            double aDeltaX = aMaxX - aMinX;
            double aDeltaY = aMaxY - aMinY;

            int aScaler = 2;
            if (myMeshData.NbNodes() > 100)
            {
                aScaler = 5;
            }
            else if (myMeshData.NbNodes() > 1000)
            {
                aScaler = 7;
            }

            myCircles.SetMinMaxSize(new gp_XY(aMinX, aMinY), new gp_XY(aMaxX, aMaxY));
            myCircles.SetCellSize(aDeltaX / Math.Max(theCellsCountU, aScaler),
                aDeltaY / Math.Max(theCellsCountV, aScaler));

            myInitCircles = true;
        }

        //! Computes the triangulation and adds the vertices,
        //! edges and triangles to the Mesh data structure.
        void compute(VectorOfInteger theVertexIndexes)
        {// Insertion of edges of super triangles in the list of free edges:


            MapOfIntegerInteger aLoopEdges = new MapOfIntegerInteger(10);
            int[] e = mySupTrian.myEdges;

            aLoopEdges.Bind(e[0], 1);
            aLoopEdges.Bind(e[1], 1);
            aLoopEdges.Bind(e[2], 1);

            if (theVertexIndexes.Length() > 0)
            {
                // Creation of 3 trianglers with the first node and the edges of the super triangle:
                int anVertexIdx = theVertexIndexes.Lower();
                createTriangles(theVertexIndexes[anVertexIdx], aLoopEdges);

                // Add other nodes to the mesh
                createTrianglesOnNewVertices(theVertexIndexes, new Message_ProgressRange());
            }

            RemoveAuxElements();

        }

        BRepMesh_Triangle mySupTrian;

        void superMesh(Bnd_Box2d theBox)
        {
            double aMinX = 0, aMinY = 0, aMaxX = 0, aMaxY = 0;
            theBox.Get(ref aMinX, ref aMinY, ref aMaxX, ref aMaxY);
            double aDeltaX = aMaxX - aMinX;
            double aDeltaY = aMaxY - aMinY;

            double aDeltaMin = Math.Min(aDeltaX, aDeltaY);
            double aDeltaMax = Math.Max(aDeltaX, aDeltaY);
            double aDelta = aDeltaX + aDeltaY;

            mySupVert.Append(myMeshData.AddNode(
              new BRepMesh_Vertex((aMinX + aMaxX) / 2, aMaxY + aDeltaMax, BRepMesh_DegreeOfFreedom.BRepMesh_Free)));

            mySupVert.Append(myMeshData.AddNode(
              new BRepMesh_Vertex(aMinX - aDelta, aMinY - aDeltaMin, BRepMesh_DegreeOfFreedom.BRepMesh_Free)));

            mySupVert.Append(myMeshData.AddNode(
              new BRepMesh_Vertex(aMaxX + aDelta, aMinY - aDeltaMin, BRepMesh_DegreeOfFreedom.BRepMesh_Free)));

            int[] e = new int[3];
            bool[] o = new bool[3];
            for (int aNodeId = 0; aNodeId < 3; ++aNodeId)
            {
                int aFirstNode = aNodeId;
                int aLastNode = (aNodeId + 1) % 3;
                int aLinkIndex = myMeshData.AddLink(new BRepMesh_Edge(
                  mySupVert[aFirstNode], mySupVert[aLastNode], BRepMesh_DegreeOfFreedom.BRepMesh_Free));

                e[aNodeId] = Math.Abs(aLinkIndex);
                o[aNodeId] = (aLinkIndex > 0);
            }

            mySupTrian = new BRepMesh_Triangle(e, o, BRepMesh_DegreeOfFreedom.BRepMesh_Free);
        }
    }


    internal class MapOfIntegerInteger : NCollection_DataMap<int, int, NCollection_DefaultHasher<int>>
    {

        public MapOfIntegerInteger(int v)
        {
        }

    }

    public class SequenceOfBndB2d : NCollection_Sequence<Bnd_B2d>
    {
    }
    public class SequenceOfInteger : NCollection_Sequence<int>
    {
    }
    public enum IntFlag
    {
        NoIntersection,
        Cross,
        EndPointTouch,
        PointOnSegment,
        Glued,
        Same
    };
}


