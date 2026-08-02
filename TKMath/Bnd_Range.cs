using OCCPort.Common;

namespace TKMath
{
    //! This class describes a range in 1D space restricted
    //! by two real values.
    //! A range can be void indicating there is no point included in the range.
    public class Bnd_Range
    {

        //! Default constructor. Creates VOID range.
        public Bnd_Range()
        {
            myFirst = (0.0);
            myLast = (-1.0);

        }


        double myFirst; //!< Start of range
        double myLast;  //!< End   of range

        //! Initializes <this> by default parameters. Makes <this> VOID.
        public void SetVoid()
        {
            myLast = -1.0;
            myFirst = 0.0;
        }


        //! Returns True if the value is out of this range.
        public bool IsOut(double theValue)
        {
            return IsVoid()
                || theValue < myFirst
                || theValue > myLast;
        }

        //! Returns range value (MAX-MIN). Returns negative value for VOID range.
        public double Delta()
        {
            return (myLast - myFirst);
        }
        //! Is <this> initialized.
        public bool IsVoid()
        {
            return (myLast < myFirst);
        }

        //! Extends <this> to include theParameter
        public void Add(double theParameter)
        {
            if (IsVoid())
            {
                myFirst = myLast = theParameter;
                return;
            }

            myFirst = Math.Min(myFirst, theParameter);
            myLast = Math.Max(myLast, theParameter);
        }
    }
}