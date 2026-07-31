using OCCPort.Common;
using TKBRep;
using TKernel;
using TKMath;

namespace TKMesh
{
    //! Builds the mesh of a shape with respect of their 
    //! correctly triangulated parts 
    public class BRepMesh_IncrementalMesh : BRepMesh_DiscretRoot
    {
        public BRepMesh_IncrementalMesh()

        {
            myModified = (false);
            myStatus = (int)IMeshData_Status.IMeshData_NoError;
        }

        IMeshTools_Parameters myParameters = new IMeshTools_Parameters();
        bool myModified;
        IMeshData_Status myStatus;
        //! Constructor.
        //! Automatically calls method Perform.
        //! @param theShape shape to be meshed.
        //! @param theLinDeflection linear deflection.
        //! @param isRelative if TRUE deflection used for discretization of 
        //! each edge will be <theLinDeflection> * <size of edge>. Deflection 
        //! used for the faces will be the maximum deflection of their edges.
        //! @param theAngDeflection angular deflection.
        //! @param isInParallel if TRUE shape will be meshed in parallel.
        public BRepMesh_IncrementalMesh(TopoDS_Shape theShape,
                                           double theLinDeflection,

                                           bool isRelative = false,

                                           double theAngDeflection = 0.5,

                                           bool isInParallel = false)
        {

        }


        //! Default flag to control parallelization for BRepMesh_IncrementalMesh
        //! tool returned for Mesh Factory
        static bool IS_IN_PARALLEL = false;

        public static int Discret(TopoDS_Shape theShape,
            double theDeflection,
            double theAngle,
            ref BRepMesh_DiscretRoot theAlgo)
        {
            BRepMesh_IncrementalMesh anAlgo = new BRepMesh_IncrementalMesh();
            anAlgo.ChangeParameters().Deflection = theDeflection;
			anAlgo.ChangeParameters().Angle = theAngle;
			anAlgo.ChangeParameters().InParallel = IS_IN_PARALLEL;
            anAlgo.SetShape(theShape);
            theAlgo = anAlgo;
            return 0; // no error
        }

        private IMeshTools_Parameters ChangeParameters()
        {
            return myParameters;
        }

        public override void Perform(Message_ProgressRange theRange)
        {
            BRepMesh_Context aContext = new BRepMesh_Context(myParameters.MeshAlgo);
            Perform(aContext, theRange);
        }
        //! Initializes specific parameters
        void initParameters()
        {
            if (myParameters.Deflection < Precision.Confusion())
            {
                throw new Standard_NumericError("BRepMesh_IncrementalMesh::initParameters : invalid parameter value");
            }
            if (myParameters.DeflectionInterior < Precision.Confusion())
            {
                myParameters.DeflectionInterior = myParameters.Deflection;
            }

            if (myParameters.MinSize < Precision.Confusion())
            {
                myParameters.MinSize =
                Math.Max(IMeshTools_Parameters.RelMinSize() * Math.Min(myParameters.Deflection,
                                                                myParameters.DeflectionInterior),
                      Precision.Confusion());
            }

            if (myParameters.Angle < Precision.Angular())
            {
                throw new Standard_NumericError("BRepMesh_IncrementalMesh::initParameters : invalid parameter value");
            }
            if (myParameters.AngleInterior < Precision.Angular())
            {
                myParameters.AngleInterior = 2.0 * myParameters.Angle;
            }
        }
        public void Perform(IMeshTools_Context theContext, Message_ProgressRange theRange = null)
        {

            initParameters();

            theContext.SetShape(Shape());
            theContext.SetParameters(myParameters);
            theContext.ChangeParameters().CleanModel = false;

            Message_ProgressScope aPS = new Message_ProgressScope(theRange, "Perform incmesh", 10);
            IMeshTools_MeshBuilder aIncMesh = new MeshTools_MeshBuilder(theContext);
            aIncMesh.Perform(aPS.Next(9));
            if (!aPS.More())
            {
                myStatus = IMeshData_Status.IMeshData_UserBreak;
                return;
            }
            myStatus = IMeshData_Status.IMeshData_NoError;
            IMeshData_Model aModel = theContext.GetModel();
            if (aModel != null)
            {
                for (int aFaceIt = 0; aFaceIt < aModel.FacesNb(); ++aFaceIt)
                {
                    IMeshData_Face aDFace = aModel.GetFace(aFaceIt);
                    myStatus |= aDFace.GetStatusMask();

                    for (int aWireIt = 0; aWireIt < aDFace.WiresNb(); ++aWireIt)
                    {
                        IWireHandle aDWire = aDFace.GetWire(aWireIt);
                        myStatus |= aDWire.GetStatusMask();
                    }
                }
            }
            aPS.Next(1);
            setDone();
        }
    }
    public interface IMeshTools_MeshBuilder
    {
        void Perform(Message_ProgressRange message_ProgressRange);
    }
}

