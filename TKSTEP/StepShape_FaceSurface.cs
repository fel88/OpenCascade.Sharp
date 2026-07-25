using TKSTEPBase;

namespace TKSTEP
{
    public class StepShape_FaceSurface : StepShape_Face
    {
        StepGeom_Surface faceGeometry;
        bool sameSense;
        public StepGeom_Surface FaceGeometry()
        {
            return faceGeometry;
        }
    }
}
