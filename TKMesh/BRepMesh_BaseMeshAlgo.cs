using OCCPort.Common;
using TKernel;
using TKG3d;
using TKMath;
using TKTopAlgo;

namespace TKMesh
{
    //! Class provides base functionality for algorithms building face triangulation.
    //! Performs initialization of BRepMesh_DataStructureOfDelaun and nodes map structures.
    public abstract class BRepMesh_BaseMeshAlgo : IMeshTools_MeshAlgo
    {
        //! Gets mesh structure.
        public BRepMesh_DataStructureOfDelaun getStructure()
        {
            return myStructure;
        }


        //! Gets meshing parameters.
        protected IMeshTools_Parameters getParameters()
        {
            return myParameters;
        }

        //! Gets discrete face.
        public IMeshData_Face getDFace()
        {
            return myDFace;
        }
        //! Gets 3d nodes map.
        public VectorOfPnt getNodesMap()
        {
            return myNodesMap;
        }

        int addLinkToMesh(
  int theFirstNodeId,
  int theLastNodeId,
  TopAbs_Orientation theOrientation)
        {
            int aLinkIndex;
            if (theOrientation == TopAbs_Orientation.TopAbs_REVERSED)
                aLinkIndex = myStructure.AddLink(new BRepMesh_Edge(theLastNodeId, theFirstNodeId, BRepMesh_DegreeOfFreedom.BRepMesh_Frontier));
            else if (theOrientation == TopAbs_Orientation.TopAbs_INTERNAL)
                aLinkIndex = myStructure.AddLink(new BRepMesh_Edge(theFirstNodeId, theLastNodeId, BRepMesh_DegreeOfFreedom.BRepMesh_Fixed));
            else
                aLinkIndex = myStructure.AddLink(new BRepMesh_Edge(theFirstNodeId, theLastNodeId, BRepMesh_DegreeOfFreedom.BRepMesh_Frontier));

            return Math.Abs(aLinkIndex);
        }

        [Aux]
        List<(gp_Pnt, gp_Pnt2d)> debugCache = new List<(gp_Pnt, gp_Pnt2d)>();

        //! Registers the given point in vertex map and adds 2d point to mesh data structure.
        //! Returns index of node in the structure.
        public int registerNode(
  gp_Pnt thePoint,
  gp_Pnt2d thePoint2d,
  BRepMesh_DegreeOfFreedom theMovability,
  bool isForceAdd)
        {
            int aNodeIndex = addNodeToStructure(
              thePoint2d, myNodesMap.Size(), theMovability, isForceAdd);

            //check same points  here

            if (aNodeIndex > myNodesMap.Size())
            {
                //not origin code
                //if (debugCache.Any(z => CheckEqual(z.Item1, thePoint) && !CheckEqual(z.Item2, thePoint2d)))                
                 //   throw new Exception("error");
                
             //   debugCache.Add((thePoint, thePoint2d));
                //end not origin code
                myNodesMap.Append(thePoint);
            }

            return aNodeIndex;
        }

        private bool CheckEqual(gp_Pnt item1, gp_Pnt thePoint)
        {
            return
                item1.X() == thePoint.X() &&
                item1.Y() == thePoint.Y() &&
                item1.Z() == thePoint.Z();
        }

        private bool CheckEqual(gp_Pnt2d item1, gp_Pnt2d thePoint)
        {
            return
          item1.X() == thePoint.X() &&
          item1.Y() == thePoint.Y();
        }

        VectorOfPnt myNodesMap;

        //! Adds the given 2d point to mesh data structure.
        //! Returns index of node in the structure.
        public virtual int addNodeToStructure(
          gp_Pnt2d thePoint,
          int theLocation3d,
          BRepMesh_DegreeOfFreedom theMovability,
          bool isForceAdd)
        {
            BRepMesh_Vertex aNode = new BRepMesh_Vertex(thePoint.XY(), theLocation3d, theMovability);
            return myStructure.AddNode(aNode, isForceAdd);
        }

        Poly_Triangulation collectTriangles()
        {
            MapOfInteger aTriangles = myStructure.ElementsOfDomain();
            if (aTriangles.IsEmpty())
            {
                return null;
            }

            Poly_Triangulation aRes = new Poly_Triangulation();
            aRes.ResizeTriangles(aTriangles.Extent(), false);
            //IteratorOfMapOfInteger aTriIt = new IteratorOfMapOfInteger(aTriangles);

            for (int aTriangeId = 1; aTriangeId <= aTriangles.Count; aTriangeId++)
            {
                int item = aTriangles.ToArray()[aTriangeId - 1];
                BRepMesh_Triangle aCurElem = myStructure.GetElement(item);

                int[] aNode = new int[3];
                myStructure.ElementNodes(aCurElem, aNode);

                for (int i = 0; i < 3; ++i)
                {
                    if (!myUsedNodes.IsBound(aNode[i]))
                    {
                        myUsedNodes.Bind(aNode[i], myUsedNodes.Size() + 1);
                    }

                    aNode[i] = myUsedNodes.Find(aNode[i]);
                }

                aRes.SetTriangle(aTriangeId, new Poly_Triangle(aNode[0], aNode[1], aNode[2]));
            }
            aRes.ResizeNodes(myUsedNodes.Extent(), false);
            aRes.AddUVNodes();
            return aRes;
        }

        //! Commits generated triangulation to TopoDS face.
        private void commitSurfaceTriangulation()
        {
            Poly_Triangulation aTriangulation = collectTriangles();
            if (aTriangulation == null)
            {
                myDFace.SetStatus(IMeshData_Status.IMeshData_Failure);
                return;
            }

            collectNodes(aTriangulation);

            BRepMesh_ShapeTool.AddInFace(myDFace.GetFace(), aTriangulation);
        }

        //=======================================================================
        public gp_Pnt2d getNodePoint2d(BRepMesh_Vertex theVertex)
        {
            return new gp_Pnt2d(theVertex.Coord());
        }

        //=======================================================================
        //function : collectNodes
        //purpose  :
        //=======================================================================
        public void collectNodes(Poly_Triangulation theTriangulation)
        {
            for (int i = 1; i <= myNodesMap.Size(); ++i)
            {
                if (myUsedNodes.IsBound(i))
                {
                    BRepMesh_Vertex aVertex = myStructure.GetNode(i);

                    int aNodeIndex = myUsedNodes.Find(i);
                    theTriangulation.SetNode(aNodeIndex, myNodesMap.Value(aVertex.Location3d()));
                    theTriangulation.SetUVNode(aNodeIndex, getNodePoint2d(aVertex));
                }
            }
        }
        NCollection_IncAllocator myAllocator;

        //! Generates mesh for the contour stored in data structure.
        public abstract void generateMesh(Message_ProgressRange theRange);
        public virtual void Perform(IMeshData_Face theDFace, IMeshTools_Parameters theParameters, Message_ProgressRange theRange)
        {
            try
            {
                myDFace = theDFace;
                myParameters = theParameters;
                myAllocator = new NCollection_IncAllocator(BRepLib.MEMORY_BLOCK_SIZE_HUGE);
                myStructure = new BRepMesh_DataStructureOfDelaun(myAllocator);
                myNodesMap = new VectorOfPnt(256);
                myUsedNodes = new DMapOfIntegerInteger(1, myAllocator);

                if (initDataStructure())
                {
                    if (!theRange.More())
                    {
                        return;
                    }
                    generateMesh(theRange);
                    commitSurfaceTriangulation();
                }
            }
            catch (Standard_Failure ex /*theException*/)
            {
            }

            myDFace = null; // Do not hold link to face.
            myStructure = null;
            myNodesMap = null;
            myUsedNodes = null;
            //myAllocator.Nullify();
        }

        TopAbs_Orientation fixSeamEdgeOrientation(
  IMeshData_Edge theDEdge,
  IMeshData_PCurve thePCurve)
        {
            for (int aPCurveIt = 0; aPCurveIt < theDEdge.PCurvesNb(); ++aPCurveIt)
            {
                var aPCurve = theDEdge.GetPCurve(aPCurveIt);
                if (aPCurve.GetFace() == myDFace && thePCurve != aPCurve)
                {
                    // Simple check that another pcurve of seam edge does not coincide with reference one.
                    gp_Pnt2d aPnt1_1 = thePCurve.GetPoint(0);
                    gp_Pnt2d aPnt2_1 = thePCurve.GetPoint(thePCurve.ParametersNb() - 1);

                    gp_Pnt2d aPnt1_2 = aPCurve.GetPoint(0);
                    gp_Pnt2d aPnt2_2 = aPCurve.GetPoint(aPCurve.ParametersNb() - 1);

                    double aSqDist1 = Math.Min(aPnt1_1.SquareDistance(aPnt1_2), aPnt1_1.SquareDistance(aPnt2_2));
                    double aSqDist2 = Math.Min(aPnt2_1.SquareDistance(aPnt1_2), aPnt2_1.SquareDistance(aPnt2_2));
                    if (aSqDist1 < Precision.SquareConfusion() &&
                        aSqDist2 < Precision.SquareConfusion())
                    {
                        return TopAbs_Orientation.TopAbs_INTERNAL;
                    }
                }
            }

            return thePCurve.GetOrientation();
        }

        protected virtual bool initDataStructure()
        {

            for (int aWireIt = 0; aWireIt < myDFace.WiresNb(); ++aWireIt)
            {
                var aDWire = myDFace.GetWire(aWireIt);
                if (aDWire.IsSet(IMeshData_Status.IMeshData_SelfIntersectingWire))
                {
                    // TODO: here we can add points of self-intersecting wire as fixed points
                    // in order to keep consistency of nodes with adjacent faces.
                    continue;
                }

                for (int aEdgeIt = 0; aEdgeIt < aDWire.EdgesNb(); ++aEdgeIt)
                {
                    IMeshData_Edge aDEdge = aDWire.GetEdge(aEdgeIt);
                    var aCurve = aDEdge.GetCurve();
                    IMeshData_PCurve aPCurve = aDEdge.GetPCurve(
                      myDFace, aDWire.GetEdgeOrientation(aEdgeIt));

                    TopAbs_Orientation aOri = fixSeamEdgeOrientation(aDEdge, aPCurve);

                    int aPrevNodeIndex = -1;
                    int aLastPoint = aPCurve.ParametersNb() - 1;
                    for (int aPointIt = 0; aPointIt <= aLastPoint; ++aPointIt)
                    {
                        int aNodeIndex = registerNode(
                          aCurve.GetPoint(aPointIt),
                          aPCurve.GetPoint(aPointIt),
                          BRepMesh_DegreeOfFreedom.BRepMesh_Frontier, false/*aPointIt > 0 && aPointIt < aLastPoint*/);

                        aPCurve.SetIndex(aPointIt, aNodeIndex);
                        myUsedNodes.Bind(aNodeIndex, aNodeIndex);

                        if (aPrevNodeIndex != -1 && aPrevNodeIndex != aNodeIndex)
                        {
                            int aLinksNb = myStructure.NbLinks();
                            int aLinkIndex = addLinkToMesh(aPrevNodeIndex, aNodeIndex, aOri);
                            if (aWireIt != 0 && aLinkIndex <= aLinksNb)
                            {
                                // Prevent holes around wire of zero area.
                                BRepMesh_Edge aLink = (BRepMesh_Edge)myStructure.GetLink(aLinkIndex);
                                aLink.SetMovability(BRepMesh_DegreeOfFreedom.BRepMesh_Fixed);
                            }
                        }

                        aPrevNodeIndex = aNodeIndex;
                    }
                }

            }



            return true;
        }

        IMeshData_Face myDFace;
        IMeshTools_Parameters myParameters;
        // Handle(NCollection_IncAllocator)       myAllocator;

        //Handle(VectorOfPnt)                    myNodesMap;
        DMapOfIntegerInteger myUsedNodes;

        BRepMesh_DataStructureOfDelaun myStructure;
    }
}



