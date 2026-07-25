using OCCPort.Common;
using TKernel;

namespace TKSTEPBase
{
    public class StepGeom_Direction : StepGeom_GeometricRepresentationItem
    {
        public double DirectionRatiosValue(int num)
        {
            return directionRatios.Value(num);
        }
        public int NbDirectionRatios()
        {
            return directionRatios.Length();
        }

        TColStd_HArray1OfReal directionRatios;

    }
}
