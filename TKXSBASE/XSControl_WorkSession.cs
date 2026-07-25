using System.Collections.Generic;

namespace TKXSBASE
{
    //! This WorkSession completes the basic one, by adding :
    //! - use of Controller, with norm selection...
    //! - management of transfers (both ways) with auxiliary classes
    //! TransferReader and TransferWriter
    //! -> these transfers may work with a Context List : its items
    //! are given by the user, according to the transfer to be
    //! i.e. it is interpreted by the Actors
    //! Each item is accessed by a Name
    public class XSControl_WorkSession : IFSelect_WorkSession
    {


        //XSControl_Controller myController;
        XSControl_TransferReader myTransferReader;
        // XSControl_TransferWriter myTransferWriter;
        //   NCollection_DataMap<TCollection_AsciiString, Handle(Standard_Transient)> myContext;
        //XSControl_Vars myVars;

        //! Sets a Transfer Reader, by internal ways, according mode :
        //! 0 recreates it clear,  1 clears it (does not recreate)
        //! 2 aligns Roots of TransientProcess from final Results
        //! 3 aligns final Results from Roots of TransientProcess
        //! 4 begins a new transfer (by BeginTransfer)
        //! 5 recreates TransferReader then begins a new transfer
        public void InitTransferReader(int mode)
        {
            //if (mode == 0 || mode == 5) myTransferReader.Clear(-1);  // full clear
            //if (myTransferReader.IsNull()) SetTransferReader(new XSControl_TransferReader);
            //else SetTransferReader(myTransferReader);

            //// mode = 0 fait par SetTransferReader suite a Nullify
            //if (mode == 1)
            //{
            //    if (!myTransferReader.IsNull()) myTransferReader->Clear(-1);
            //    else SetTransferReader(new XSControl_TransferReader);
            //}
            //if (mode == 2)
            //{
            //    Handle(Transfer_TransientProcess) TP = myTransferReader->TransientProcess();
            //    if (TP.IsNull())
            //    {
            //        TP = new Transfer_TransientProcess;
            //        myTransferReader->SetTransientProcess(TP);
            //        TP->SetGraph(HGraph());
            //    }
            //    Handle(TColStd_HSequenceOfTransient) lis = myTransferReader->RecordedList();
            //    Standard_Integer i, nb = lis->Length();
            //    for (i = 1; i <= nb; i++) TP->SetRoot(lis->Value(i));
            //}
            //if (mode == 3)
            //{
            //    Handle(Transfer_TransientProcess) TP = myTransferReader->TransientProcess();
            //    if (TP.IsNull()) return;
            //    Standard_Integer i, nb = TP->NbRoots();
            //    for (i = 1; i <= nb; i++) myTransferReader->RecordResult(TP->Root(i));
            //}
            //if (mode == 4 || mode == 5) myTransferReader->BeginTransfer();
        }

        public XSControl_TransferReader TransferReader()
        {
            throw new NotImplementedException();
        }
    }

}