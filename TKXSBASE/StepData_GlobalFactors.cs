namespace TKXSBASE
{
    //! Class for using global units variables
    public class StepData_GlobalFactors
    {
        //! Returns a global static object
        public static StepData_GlobalFactors Intance = new StepData_GlobalFactors();
        public double  LengthFactor()
        {
            return myLengthFactor;
        }

       double myLengthFactor;
       double myPlaneAngleFactor;
       double mySolidAngleFactor;
       double myFactRD;
       double myFactDR;
       double myCascadeUnit;
    }
}
