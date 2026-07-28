using OCCPort;
using OCCPort.Common;
using TKBRep;
using TKernel;
using TKG3d;
using TKMath;

namespace TKMesh
{
    //! Extends base meshing algo in order to enable possibility 
    //! of addition of free vertices into the mesh.    
    public class BRepMesh_NodeInsertionMeshAlgo<RangeSplitter> : BRepMesh_DelaunayBaseMeshAlgo where RangeSplitter : AbstractRangeSplitter, new() //: BaseAlgo
    {
        //! Returns range splitter.
        public RangeSplitter getRangeSplitter()
        {
            return myRangeSplitter;
        }

        //! Adds the given 2d point to mesh data structure.
        //! Returns index of node in the structure.
        public override int addNodeToStructure(
    gp_Pnt2d thePoint,
    int theLocation3d,
    BRepMesh_DegreeOfFreedom theMovability,
    bool isForceAdd)
        {
            return base.addNodeToStructure(
              myRangeSplitter.Scale(thePoint, true),
              theLocation3d, theMovability, isForceAdd);
        }


        //! Performs initialization of data structure using existing model data.
        protected override bool initDataStructure()
        {
            var aDFace = this.getDFace();
            NCollection_Array1<SequenceOfPnt2d> aWires = new NCollection_Array1<SequenceOfPnt2d>(0, aDFace.WiresNb() - 1);
            for (int aWireIt = 0; aWireIt < aDFace.WiresNb(); ++aWireIt)
            {
                var aDWire = aDFace.GetWire(aWireIt);
                if (aDWire.IsSet(IMeshData_Status.IMeshData_SelfIntersectingWire) ||
                   (aDWire.IsSet(IMeshData_Status.IMeshData_OpenWire) && aWireIt != 0))
                {
                    continue;
                }

                aWires[aWireIt] = collectWirePoints(aDWire);// todo: ??
            }

            myRangeSplitter.AdjustRange();
            if (!myRangeSplitter.IsValid())
            {
                aDFace.SetStatus(IMeshData_Status.IMeshData_Failure);
                return false;
            }

            var aDelta = myRangeSplitter.GetDelta();
            var aTolUV = myRangeSplitter.GetToleranceUV();
            double uCellSize = 14.0 * aTolUV.Item1;
            double vCellSize = 14.0 * aTolUV.Item2;

            this.getStructure().Data().SetCellSize(uCellSize / aDelta.Item1, vCellSize / aDelta.Item2);
            this.getStructure().Data().SetTolerance(aTolUV.Item1 / aDelta.Item1, aTolUV.Item2 / aDelta.Item2);

            for (int aWireIt = 0; aWireIt < aDFace.WiresNb(); ++aWireIt)
            {
                SequenceOfPnt2d aWire = aWires[aWireIt];
                if (aWire != null && !aWire.IsEmpty())
                {
                    myClassifier.RegisterWire(aWire, aTolUV,
                                               myRangeSplitter.GetRangeU(),
                                               myRangeSplitter.GetRangeV());
                }
            }

            if (this.getParameters().InternalVerticesMode)
            {
                insertInternalVertices();
            }

            return base.initDataStructure();
        }

        public class SequenceOfPnt2d : NCollection_Sequence<gp_Pnt2d>
        {

        }
        //! Iterates over internal vertices of a face and 
        //! creates corresponding nodes in data structure.
        void insertInternalVertices()
        {
            TopExp_Explorer aExplorer = new TopExp_Explorer(this.getDFace().GetFace(), TopAbs_ShapeEnum.TopAbs_VERTEX, TopAbs_ShapeEnum.TopAbs_EDGE);
            for (; aExplorer.More(); aExplorer.Next())
            {
                TopoDS_Vertex aVertex = TopoDS.Vertex(aExplorer.Current());
                if (aVertex.Orientation() != TopAbs_Orientation.TopAbs_INTERNAL)
                {
                    continue;
                }

                insertInternalVertex(aVertex);
            }
        }


        //! Inserts the given vertex into mesh.
        void insertInternalVertex(TopoDS_Vertex theVertex)
        {
            try
            {
                //OCC_CATCH_SIGNALS

                gp_Pnt2d aPnt2d = BRep_Tool.Parameters(theVertex, this.getDFace().GetFace());
                // check UV values for internal vertices
                if (myClassifier.Perform(aPnt2d) != TopAbs_State.TopAbs_IN)
                    return;

                this.registerNode(BRep_Tool.Pnt(theVertex), aPnt2d,
                                   BRepMesh_DegreeOfFreedom.BRepMesh_Fixed, false);
            }
            catch (Standard_Failure ex)
            {
            }
        }

        //! Creates collection of points representing discrete wire.
        SequenceOfPnt2d collectWirePoints(
    IWireHandle theDWire
    )
        {
            SequenceOfPnt2d aWirePoints = new SequenceOfPnt2d();
            for (int aEdgeIt = 0; aEdgeIt < theDWire.EdgesNb(); ++aEdgeIt)
            {
                var aDEdge = theDWire.GetEdge(aEdgeIt);
                var aPCurve = aDEdge.GetPCurve(
                  this.getDFace(), theDWire.GetEdgeOrientation(aEdgeIt));

                int aPointIt, aEndIndex, aInc;
                if (aPCurve.IsForward())
                {
                    // For an infinite cylinder (for example)
                    // aPCurve->ParametersNb() == 0

                    aEndIndex = aPCurve.ParametersNb() - 1;
                    aPointIt = Math.Min(0, aEndIndex);
                    aInc = 1;
                }
                else
                {
                    // For an infinite cylinder (for example)
                    // aPCurve->ParametersNb() == 0

                    aPointIt = aPCurve.ParametersNb() - 1;
                    aEndIndex = Math.Min(0, aPointIt);
                    aInc = -1;
                }

                // For an infinite cylinder (for example)
                // this cycle will not be executed.
                for (; aPointIt != aEndIndex; aPointIt += aInc)
                {
                    var aPnt2d = aPCurve.GetPoint(aPointIt);
                    aWirePoints.Append(aPnt2d);
                    myRangeSplitter.AddPoint(aPnt2d);
                }
            }

            return aWirePoints;
        }
        //! Performs processing of the given face.
        public override void Perform(
    IMeshData_Face theDFace,
     IMeshTools_Parameters theParameters,
     Message_ProgressRange theRange)
        {
            myRangeSplitter.Reset(theDFace, theParameters);
            myClassifier = new BRepMesh_Classifier();
            if (!theRange.More())
            {
                return;
            }
            base.Perform(theDFace, theParameters, theRange);
            myClassifier = null;
        }


        //! Returns classifier.
        public BRepMesh_Classifier getClassifier()
        {
            return myClassifier;
        }
        BRepMesh_Classifier myClassifier;

        RangeSplitter myRangeSplitter = new RangeSplitter();
    }
}

