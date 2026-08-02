using OCCPort.Common;

namespace TKGeomAlgo
{
    //! Stores an intersection on a line represented by :
    //!
    //! * A Real parameter.
    //!
    //! * A flag True when the parameter starts an interval.
    public class Hatch_Parameter
    {

        public Hatch_Parameter
  (double Par1,
    bool Start,
    int Index,
    double Par2)
        {
            myPar1 = (Par1);
            myStart = Start;
            myIndex = (Index);
            myPar2 = (Par2);

        }

        public double myPar1;
        public bool myStart;
        public int myIndex;
        public double myPar2;

    }
}
