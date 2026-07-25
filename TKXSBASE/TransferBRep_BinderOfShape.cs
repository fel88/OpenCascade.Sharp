namespace TKXSBASE
{
    //! Allows direct binding between a starting Object and the Result
    //! of its transfer when it is Unique.
    //! The Result itself is defined as a formal parameter <Shape from TopoDS>
    //! Warning : While it is possible to instantiate BinderOfShape with any Type
    //! for the Result, it is not advisable to instantiate it with
    //! Transient Classes, because such Results are directly known and
    //! managed by TransferProcess & Co, through
    //! SimpleBinderOfTransient : this class looks like instantiation
    //! of BinderOfShape, but its method ResultType
    //! is adapted (reads DynamicType of the Result)
    public class TransferBRep_BinderOfShape : Transfer_Binder
    {
    }
}
