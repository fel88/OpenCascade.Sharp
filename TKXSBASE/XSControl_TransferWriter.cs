namespace TKXSBASE
{
    //! TransferWriter gives help to control transfer to write a file
    //! after having converted data from Cascade/Imagine
    //!
    //! It works with a Controller (which itself can work with an
    //! Actor to Write) and a FinderProcess. It records results and
    //! checks
    public class XSControl_TransferWriter
    {
        Transfer_FinderProcess myTransferWriter;
        //! Sets a new Controller, also sets a new FinderProcess
        public void SetController(XSControl_Controller theCtl)
        {
            myController = theCtl;
            Clear(-1);
        }
        XSControl_Controller myController;

        public void Clear(int mode)
        {
            if (mode < 0 || myTransferWriter == null)
                myTransferWriter = new Transfer_FinderProcess();
            else myTransferWriter.Clear();
        }
    }
}