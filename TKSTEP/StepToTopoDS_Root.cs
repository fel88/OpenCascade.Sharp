namespace TKSTEP
{
    //! This class implements the common services for
    //! all classes of StepToTopoDS which report error
    //! and sets and returns precision.
    public class StepToTopoDS_Root
    {
        public bool IsDone()
        {
            return done;
        }
        protected bool done;
    }
}
