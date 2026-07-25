
namespace TKSTEPBase
{
    public class StepGeom_CartesianPoint : StepGeom_Point
    {
        public int NbCoordinates()
        {
            return nbcoord;
            //	return coordinates->Length();
        }


        public double CoordinatesValue(int num)
        {
            return coords[num - 1];
            //	return coordinates->Value(num);
        }


        int nbcoord;
        double[] coords = new double[3];
    }
}
