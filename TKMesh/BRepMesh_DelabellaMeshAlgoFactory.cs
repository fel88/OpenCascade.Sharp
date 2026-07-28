using OCCPort.Common;
using System;
using System.Reflection.Metadata;
using System.Xml.Linq;
using TKernel;
using TKG2d;
using TKMath;

namespace TKMesh
{
    internal class BRepMesh_DelabellaMeshAlgoFactory : IMeshTools_MeshAlgoFactory
    {
        public IMeshTools_MeshAlgo GetAlgo(GeomAbs_SurfaceType theSurfaceType,
            ref IMeshTools_Parameters theParameters)
        {
            //=======================================================================
            // Function: GetAlgo
            // Purpose :
            //=======================================================================
            {
                var algo1 = new BRepMesh_DelaunayNodeInsertionMeshAlgo<BRepMesh_DefaultRangeSplitter>();
                var algo2 = new BRepMesh_DelabellaBaseMeshAlgo();
                switch (theSurfaceType)
                {

                    /*  struct BaseMeshAlgo
  {
    typedef BRepMesh_DelabellaBaseMeshAlgo Type;
  };*/
                    /*
  template<class RangeSplitter>
  struct NodeInsertionMeshAlgo
  {
    typedef BRepMesh_DelaunayNodeInsertionMeshAlgo<RangeSplitter, BRepMesh_CustomDelaunayBaseMeshAlgo<BRepMesh_DelabellaBaseMeshAlgo> > Type;
  };*/

                    case GeomAbs_SurfaceType.GeomAbs_Plane:
                        return theParameters.InternalVerticesMode ?
                          //new NodeInsertionMeshAlgo<BRepMesh_DefaultRangeSplitter>::Type :
                          algo1 :
                          //new BaseMeshAlgo::Type;
                          algo2;
                        break;

                    //case GeomAbs_Sphere:
                    //    {
                    //        NodeInsertionMeshAlgo<BRepMesh_SphereRangeSplitter>::Type* aMeshAlgo =
                    //          new NodeInsertionMeshAlgo<BRepMesh_SphereRangeSplitter>::Type;
                    //        aMeshAlgo->SetPreProcessSurfaceNodes(Standard_True);
                    //        return aMeshAlgo;
                    //    }
                    //    break;

                    //case GeomAbs_Cylinder:
                    //    return theParameters.InternalVerticesMode ?
                    //      new DefaultNodeInsertionMeshAlgo<BRepMesh_CylinderRangeSplitter>::Type :
                    //      new DefaultBaseMeshAlgo::Type;
                    //    break;

                    //case GeomAbs_Cone:
                    //    {
                    //        NodeInsertionMeshAlgo<BRepMesh_ConeRangeSplitter>::Type* aMeshAlgo =
                    //          new NodeInsertionMeshAlgo<BRepMesh_ConeRangeSplitter>::Type;
                    //        aMeshAlgo->SetPreProcessSurfaceNodes(Standard_True);
                    //        return aMeshAlgo;
                    //    }
                    //    break;

                    //case GeomAbs_Torus:
                    //    {
                    //        NodeInsertionMeshAlgo<BRepMesh_TorusRangeSplitter>::Type* aMeshAlgo =
                    //          new NodeInsertionMeshAlgo<BRepMesh_TorusRangeSplitter>::Type;
                    //        aMeshAlgo->SetPreProcessSurfaceNodes(Standard_True);
                    //        return aMeshAlgo;
                    //    }
                    //    break;

                    case GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution:
                        {
                            BRepMesh_DelaunayDeflectionControlMeshAlgo<BRepMesh_BoundaryParamsRangeSplitter>
                                aMeshAlgo = new();
                            aMeshAlgo.SetPreProcessSurfaceNodes(true);
                            return aMeshAlgo;
                        }

                    default:
                        {
                            /*DeflectionControlMeshAlgo<BRepMesh_NURBSRangeSplitter>::Type* aMeshAlgo =
                              new DeflectionControlMeshAlgo<BRepMesh_NURBSRangeSplitter>::Type;
                            aMeshAlgo->SetPreProcessSurfaceNodes(Standard_True);
                            return aMeshAlgo;*/
                            return null;
                        }
                }
            }
        }
    }

    public class VectorOfBoolean : NCollection_Vector<bool>
    {
    }


    //! Auxiliary class extending UV range splitter in order to generate
    //! internal nodes for NURBS surface.
    public class BRepMesh_BoundaryParamsRangeSplitter : BRepMesh_NURBSRangeSplitter
    {
    }

    //! Auxiliary class extending UV range splitter in order to generate
    //! internal nodes for NURBS surface.
    public class BRepMesh_NURBSRangeSplitter : BRepMesh_UVParamRangeSplitter
    {
    }

    //! Intended to generate internal mesh nodes using UV parameters of boundary discrete points.
    public class BRepMesh_UVParamRangeSplitter : BRepMesh_DefaultRangeSplitter
    {
    }

    
}

