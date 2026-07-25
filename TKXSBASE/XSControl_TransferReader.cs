using OCCPort.Common;
using System.Reflection.Metadata;
using TKernel;

namespace TKXSBASE
{
    //! A TransferReader performs, manages, handles results of,
    //! transfers done when reading a file (i.e. from entities of an
    //! InterfaceModel, to objects for Imagine)
    //!
    //! Running is organised around basic tools : TransientProcess and
    //! its Actor, results are Binders and CheckIterators. It implies
    //! control by a Controller (which prepares the Actor as required)
    //!
    //! Getting results can be done directly on TransientProcess, but
    //! these are immediate "last produced" results. Each transfer of
    //! an entity gives a final result, but also possible intermediate
    //! data, and checks, which can be attached to sub-entities.
    //!
    //! Hence, final results (which intermediates and checks) are
    //! recorded as ResultFromModel and can be queried individually.
    //!
    //! Some more direct access are given for results which are
    //! Transient or Shapes
    public class XSControl_TransferReader
    {//! Commands the transfer on reading for an entity to data for
     //! Imagine, using the selected Actor for Read
     //! Returns count of transferred entities, ok or with fails (0/1)
     //! If <rec> is True (D), the result is recorded by RecordResult
        public int TransferOne(object theEnt,
                                                 bool theRec = true,
                                                 Message_ProgressRange theProgress = default)
        {
            //if (myActor==null || myModel==null) return 0;

            // if (myTP.IsNull()) { if (!BeginTransfer()) return 0; }

            // Message_Messenger::StreamBuffer sout = myTP->Messenger()->SendInfo();
            // int level = myTP.TraceLevel();


            // Transfer_TransferOutput TP=new(myTP, myModel);
            return 0;
        }

        //XSControl_Controller myController;
        string myFileName;
        Interface_InterfaceModel myModel;
        //  Interface_HGraph myGraph;
        NCollection_DataMap<string, object> myContext;
        //Transfer_ActorOfTransientProcess myActor;
        Transfer_TransientProcess myTP;
        TColStd_DataMapOfIntegerTransient myResults;
        //TopTools_HSequenceOfShape myShapeResult;
    }


    //! A TransferOutput is a Tool which manages the transfer of
    //! entities created by an Interface, stored in an InterfaceModel,
    //! into a set of Objects suitable for an Application
    //! Objects to be transferred are given, by method Transfer
    //! (which calls Transfer from TransientProcess)
    //! A default action is available to get all roots of the Model
    //! Result is given as a TransferIterator (see TransferProcess)
    //! Also, it is possible to pilot directly the TransientProcess
    public class Transfer_TransferOutput
    {
    }

}