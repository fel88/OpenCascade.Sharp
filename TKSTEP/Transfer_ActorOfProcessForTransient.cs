namespace TKSTEP
{
    public abstract class Transfer_ActorOfProcessForTransient
    {
        //! Prerequesite for Transfer : the method Transfer is
        //! called on a starting object only if Recognize has
        //! returned True on it
        //! This allows to define a list of Actors, each one
        //! processing a definite kind of data
        //! TransferProcess calls Recognize on each one before
        //! calling Transfer. But even if Recognize has returned
        //! True, Transfer can reject by returning a Null Binder
        //! (afterwards rejection), the next actor is then invoked
        //!
        //! The provided default returns True, can be redefined
        public abstract bool Recognize(object start);
    }
}
