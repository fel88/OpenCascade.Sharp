using TKBRep;
using TKSTEPBase;
using TKXSBASE;

namespace TKSTEP
{
    public class StepToTopoDS_TranslateFace : StepToTopoDS_Root
    {
        StepToTopoDS_TranslateFaceError myError;
        TopoDS_Shape myResult;
        public void Init(StepShape_FaceSurface FS, StepToTopoDS_Tool aTool, StepToTopoDS_NMTool NMTool)
        {
            done = true;
            if (aTool.IsBound(FS))
            {
                myResult = TopoDS.Face(aTool.Find(FS));
                myError = StepToTopoDS_TranslateFaceError.StepToTopoDS_TranslateFaceDone;
                done = true;
                return;
            }

            Transfer_TransientProcess TP = aTool.TransientProcess();

            // ----------------------------------------------
            // Map the Face Geometry and create a TopoDS_Face
            // ----------------------------------------------


            StepGeom_Surface StepSurf = FS.FaceGeometry();
        }
    }
}
