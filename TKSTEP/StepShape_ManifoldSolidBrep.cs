using TKSTEPBase;


namespace TKSTEP
{
    public class StepShape_ManifoldSolidBrep : StepShape_SolidModel
    {
        public StepShape_ManifoldSolidBrep()
        {

        }
        public StepShape_ConnectedFaceSet Outer()
        {
            return outer;
        }
        StepShape_ConnectedFaceSet outer;

    }
}
