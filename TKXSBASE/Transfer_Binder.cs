namespace TKXSBASE
{
    //! A Binder is an auxiliary object to Map the Result of the
    //! Transfer of a given Object : it records the Result of the
    //! Unitary Transfer (Resulting Object), status of progress and
    //! error (if any) of the Process
    //!
    //! The class Binder itself makes no definition for the Result :
    //! it is defined by sub-classes : it can be either Simple (and
    //! has to be typed : see generic class SimpleBinder) or Multiple
    //! (see class MultipleBinder).
    //!
    //! In principle, for a Transfer in progress, Result cannot be
    //! accessed : this would cause an exception raising.
    //! This is controlled by the value if StatusResult : if it is
    //! "Used", the Result cannot be changed. This status is normally
    //! controlled by TransferProcess but can be directly (see method
    //! SetAlreadyUsed)
    //!
    //! Checks can be completed by a record of cases, as string which
    //! can be used as codes, but not to be printed
    //!
    //! In addition to the Result, a Binder can bring a list of
    //! Attributes, which are additional data, each of them has a name
    public class Transfer_Binder 
    {
    }
}
