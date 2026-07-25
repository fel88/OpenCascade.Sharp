using OCCPort.Common;

namespace TKMath
{
    //! This abstract class describes the virtual functions
    //! associated with a Function of a single variable.
    public abstract class math_Function
    {
        //! Computes the value of the function <F> for a given value of
        //! variable <X>.
        //! returns True if the computation was done successfully,
        //! False otherwise.
        public abstract bool Value(double X, out double F);

        //! returns the state of the function corresponding to the
        //! latest call of any methods associated with the function.
        //! This function is called by each of the algorithms
        //! described later which defined the function Integer
        //! Algorithm::StateNumber(). The algorithm has the
        //! responsibility to call this function when it has found
        //! a solution (i.e. a root or a minimum) and has to maintain
        //! the association between the solution found and this
        //! StateNumber.
        //! Byu default, this method returns 0 (which means for the
        //! algorithm: no state has been saved). It is the
        //! responsibility of the programmer to decide if he needs
        //! to save the current state of the function and to return
        //! an Integer that allows retrieval of the state.
        public virtual   int GetStateNumber()
        {
            return 0;
        }

    }
}
