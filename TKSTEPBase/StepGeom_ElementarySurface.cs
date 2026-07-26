namespace TKSTEPBase
{
    public class StepGeom_ElementarySurface : StepGeom_Surface
    {
        public StepGeom_Axis2Placement3d Position()
        {
            return position;
        }
        StepGeom_Axis2Placement3d position;

    }

}
