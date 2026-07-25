namespace TKXSBASE
{
    //! Qualifies an execution status :
    //! RetVoid  : normal execution which created nothing, or
    //! no data to process
    //! RetDone  : normal execution with a result
    //! RetError : error in command or input data, no execution
    //! RetFail  : execution was run and has failed
    //! RetStop  : indicates end or stop (such as Raise)
    public enum IFSelect_ReturnStatus
    {
        IFSelect_RetVoid,
        IFSelect_RetDone,
        IFSelect_RetError,
        IFSelect_RetFail,
        IFSelect_RetStop
    };

}
