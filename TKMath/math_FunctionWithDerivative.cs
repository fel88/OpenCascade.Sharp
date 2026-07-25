using OCCPort.Common;

namespace TKMath
{
    //! This abstract class describes the virtual functions associated with
    //! a function of a single variable for which the first derivative is
    //! available.
    public abstract class math_FunctionWithDerivative : math_Function
    {
        ////! Computes the value <F>of the function for the variable <X>.
        ////! Returns True if the calculation were successfully done,
        ////! False otherwise.
        //public abstract bool Value(double X, out double F);

        //! Computes the derivative <D> of the function
        //! for the variable <X>.
        //! Returns True if the calculation were successfully done,
        //! False otherwise.
        public abstract bool Derivative(double X, out double D);


        //! Computes the value <F> and the derivative <D> of the
        //! function for the variable <X>.
        //! Returns True if the calculation were successfully done,
        //! False otherwise.
        public abstract bool Values(double X, out double F, out double D);

    }
}
