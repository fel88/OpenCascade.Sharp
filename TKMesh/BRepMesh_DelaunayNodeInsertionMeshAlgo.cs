using TKernel;
using TKG3d;
using TKMath;

namespace TKMesh
{
    //! Extends base Delaunay meshing algo in order to enable possibility 
    //! of addition of free vertices and internal nodes into the mesh.
    public class BRepMesh_DelaunayNodeInsertionMeshAlgo<RangeSplitter> : BRepMesh_NodeInsertionMeshAlgo<RangeSplitter> where RangeSplitter : AbstractRangeSplitter, new()
    {
        public BRepMesh_DelaunayNodeInsertionMeshAlgo()
        {

        }
        //! Returns size of cell to be used by acceleration circles grid structure.
        public override (int, int) getCellsCount(int theVerticesNb)
        {
            return BRepMesh_GeomTool.CellsCount(getDFace().GetSurface(), theVerticesNb,
                                                  getDFace().GetDeflection(),
                                                  getRangeSplitter());
        }

        //! Sets PreProcessSurfaceNodes flag.
        //! If TRUE, registers surface nodes before generation of base mesh.
        //! If FALSE, inserts surface nodes after generation of base mesh. 
        public void SetPreProcessSurfaceNodes(bool isPreProcessSurfaceNodes)
        {
            myIsPreProcessSurfaceNodes = isPreProcessSurfaceNodes;
        }

        bool myIsPreProcessSurfaceNodes;

        //! Performs processing of generated mesh. Generates surface nodes and inserts them into structure.
        public override void postProcessMesh(BRepMesh_Delaun theMesher,
                                Message_ProgressRange theRange)
        {
            if (!theRange.More())
            {
                return;
            }
            base.postProcessMesh(theMesher, new Message_ProgressRange()); // shouldn't be range passed here?

            if (!myIsPreProcessSurfaceNodes)
            {
                ListOfPnt2d aSurfaceNodes =
                  this.getRangeSplitter().GenerateSurfaceNodes(getParameters());

                insertNodes(aSurfaceNodes, theMesher, theRange);
            }
        }

        //! Performs initialization of data structure using existing model data.
        protected override bool initDataStructure()
        {
            if (!base.initDataStructure())
            {
                return false;
            }

            if (myIsPreProcessSurfaceNodes)
            {
                ListOfPnt2d aSurfaceNodes =
                 this.getRangeSplitter().GenerateSurfaceNodes(this.getParameters());

                registerSurfaceNodes(aSurfaceNodes);
            }

            return true;
        }
        //! Registers surface nodes in data structure.
        bool registerSurfaceNodes(ListOfPnt2d theNodes)
        {
            if (theNodes == null || theNodes.IsEmpty())
            {
                return false;
            }

            bool isAdded = false;
            //ListOfPnt2d::Iterator aNodesIt(*theNodes);
            foreach (var aNodesIt in theNodes)
            {
                gp_Pnt2d aPnt2d = aNodesIt;
                if (this.getClassifier().Perform(aPnt2d) == TopAbs_State.TopAbs_IN)
                {
                    isAdded = true;
                    this.registerNode(this.getRangeSplitter().Point(aPnt2d),
                                       aPnt2d, BRepMesh_DegreeOfFreedom.BRepMesh_Free, false);
                }
            }

            return isAdded;
        }

        //! Inserts nodes into mesh.
        bool insertNodes(
                    ListOfPnt2d theNodes,
                    BRepMesh_Delaun theMesher,
                    Message_ProgressRange theRange)
        {
            if (theNodes == null || theNodes.IsEmpty())
            {
                return false;
            }

            VectorOfInteger aVertexIndexes = new VectorOfInteger(theNodes.Size());
            foreach (var aNodesIt in theNodes)
            {
                gp_Pnt2d aPnt2d = aNodesIt;
                if (this.getClassifier().Perform(aPnt2d) == TopAbs_State.TopAbs_IN)
                {
                    aVertexIndexes.Append(this.registerNode(this.getRangeSplitter().Point(aPnt2d),
                        aPnt2d, BRepMesh_DegreeOfFreedom.BRepMesh_Free,
                      false));
                }
            }

            theMesher.AddVertices(aVertexIndexes, theRange);
            if (!theRange.More())
            {
                return false;
            }
            return !aVertexIndexes.IsEmpty();
        }

    }
}

