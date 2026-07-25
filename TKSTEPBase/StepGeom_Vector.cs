using OCCPort.Common;

namespace TKSTEPBase
{
    public class StepGeom_Vector : StepGeom_GeometricRepresentationItem
    {
        public StepGeom_Direction Orientation()
        {
            return orientation;
        }
        public double Magnitude()
        {
            return magnitude;
        }
        StepGeom_Direction orientation;
        double magnitude;
    }
}
