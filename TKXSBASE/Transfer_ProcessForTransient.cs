
global using Transfer_TransferMapOfProcessForTransient= TKernel.NCollection_IndexedDataMap <object,TKXSBASE.Transfer_Binder, TKernel.NCollection_DefaultHasher<object>> ;
namespace TKXSBASE
{
    //! Manages Transfer of Transient Objects. Produces also
    //! ActorOfTransientProcess       (deferred class),
    //! IteratorOfTransientProcess    (for Results),
    //! TransferMapOfTransientProcess (internally used)
    //! Normally uses as TransientProcess, which adds some specifics
    public class Transfer_ProcessForTransient
    {
        //! Returns the maximum possible value for Map Index
        //! (no result can be bound with a value greater than it)
        public  int NbMapped()
        {
            return themap.Extent();

        }
        Transfer_TransferMapOfProcessForTransient themap = new Transfer_TransferMapOfProcessForTransient();

    }

}