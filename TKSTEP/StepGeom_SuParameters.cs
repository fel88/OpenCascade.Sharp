using OCCPort.Common;
using TKSTEPBase;

namespace TKSTEP
{
    //! Representation of STEP entity SuParameters
    public class StepGeom_SuParameters : StepGeom_GeometricRepresentationItem
    {
        //! Returns field Gamma
        public double Gamma()
        {
            return myGamma;

        }

        public double C()
        {
            return myC;
        }


        public double A()
        {
            return myA;
        }

        public double B()
        {
            return myB;
        }
        public double Beta()
        {
            return myBeta;
        }
        //! Returns field Alpha
        public double Alpha()
        {
            return myAlpha;

        }
        double myA;
        double myAlpha;
        double myB;
        double myBeta;
        double myC;
        double myGamma;
    }
}
