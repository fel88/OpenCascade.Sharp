global using DMapOfIntegerInteger = TKernel.NCollection_DataMap<int, int>;
using System;
using TKMath;
using static System.Net.Mime.MediaTypeNames;

namespace TKMesh
{
    //! Default implementation of IMeshTools_MeshAlgoFactory providing algorithms 
    //! of different complexity depending on type of target surface.
    public class BRepMesh_MeshAlgoFactory : IMeshTools_MeshAlgoFactory
    {
        class DeflectionControlMeshAlgo<T> : BRepMesh_DelaunayDeflectionControlMeshAlgo<T> where T : AbstractRangeSplitter, new()//<RangeSplitter, BaseAlgo>
        {

        }

        class NodeInsertionMeshAlgo<T> : BRepMesh_DelaunayNodeInsertionMeshAlgo<T> where T : AbstractRangeSplitter, new()//<RangeSplitter, BaseAlgo>
        {
            
        }

        public IMeshTools_MeshAlgo GetAlgo(GeomAbs_SurfaceType theSurfaceType, ref IMeshTools_Parameters theParameters)
        {
            var algo1 = new BRepMesh_DelaunayNodeInsertionMeshAlgo<BRepMesh_DefaultRangeSplitter>();
            var algo2 = new BRepMesh_DelabellaBaseMeshAlgo();
            var algo3 = new BRepMesh_DelaunayDeflectionControlMeshAlgo<BRepMesh_DefaultRangeSplitter>();
            switch (theSurfaceType)
            {
                /**
                 *   struct DeflectionControlMeshAlgo
  {
    typedef BRepMesh_DelaunayDeflectionControlMeshAlgo<RangeSplitter, BRepMesh_DelaunayBaseMeshAlgo> Type;
  };
                 */
                case GeomAbs_SurfaceType. GeomAbs_Sphere:
                    return theParameters.EnableControlSurfaceDeflectionAllSurfaces ?
                      new DeflectionControlMeshAlgo<BRepMesh_SphereRangeSplitter>() :
                      new NodeInsertionMeshAlgo<BRepMesh_SphereRangeSplitter>();
                    break;
                case GeomAbs_SurfaceType.GeomAbs_Plane:
                    return theParameters.EnableControlSurfaceDeflectionAllSurfaces ?
                        algo3 : theParameters.InternalVerticesMode ?
                        algo1 : algo2;
                    /* new DeflectionControlMeshAlgo<BRepMesh_DefaultRangeSplitter>::Type :
                       (theParameters.InternalVerticesMode ?
                        new NodeInsertionMeshAlgo<BRepMesh_DefaultRangeSplitter>::Type :
                        new BaseMeshAlgo::Type);*/

                    break;
                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution:
                    return new BRepMesh_DelaunayDeflectionControlMeshAlgo<BRepMesh_BoundaryParamsRangeSplitter>();
                    break;

            }
            return null;
        }
    }
}



