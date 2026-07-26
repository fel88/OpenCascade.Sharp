namespace TKSTEPBase
{
    public class StepGeom_Placement : StepGeom_GeometricRepresentationItem
    {
        public StepGeom_CartesianPoint Location()
        {
            return location;
        }
        StepGeom_CartesianPoint location;

    }

}
