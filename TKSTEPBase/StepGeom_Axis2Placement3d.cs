namespace TKSTEPBase
{
    public class StepGeom_Axis2Placement3d : StepGeom_Placement
    {
        public bool HasAxis()
        {
            return hasAxis;
        }

        public StepGeom_Direction Axis()
        {
            return axis;
        }

        public StepGeom_Direction RefDirection()
        {
            return refDirection;
        }

        public bool HasRefDirection()
        {
            return hasRefDirection;
        }


        StepGeom_Direction axis;
        StepGeom_Direction refDirection;
        bool hasAxis;
        bool hasRefDirection;
    }

}
