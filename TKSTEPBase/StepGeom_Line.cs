namespace TKSTEPBase
{
    public class StepGeom_Line : StepGeom_Curve
    {
        public void SetPnt(StepGeom_CartesianPoint aPnt)
        {
            pnt = aPnt;
        }

        public StepGeom_CartesianPoint Pnt()
        {
            return pnt;
        }

        public StepGeom_Vector Dir()
        {
            return dir;
        }


        StepGeom_CartesianPoint pnt;
        StepGeom_Vector dir;

    }
}
