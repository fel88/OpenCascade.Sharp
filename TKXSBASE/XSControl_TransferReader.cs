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
    {
        XSControl_Controller myController;
        Transfer_ActorOfTransientProcess myActor;
        public void SetGraph(Interface_HGraph graph)
        {
            if (graph == null)
            {
                myModel = null;
            }
            else
                myModel = graph.Graph().Model();

            myGraph = graph;

            if (myTP!=null) myTP.SetGraph(graph);
        }

        Interface_HGraph myGraph;

        public void SetController(XSControl_Controller control)
        {
            myController = control;
            myActor = null;
            Clear(-1);
        }

        //! Returns the currently used TransientProcess
        //! It is computed from the model by TransferReadRoots, or by
        //! BeginTransferRead
        public Transfer_TransientProcess TransientProcess()
        { return myTP; }

        //! Clears data, according mode :
        //! -1 all
        //! 0 nothing done
        //! +1 final results
        //! +2 working data (model, context, transfer process)
        public void Clear(int mode)
        {
            if ((mode & 1) != 0)
            {
                myResults.Clear();
                ///myShapeResult.Nullify();
            }
            if ((mode & 2) != 0)
            {
                myModel = null;
                //myGraph = null;
                // myTP.Nullify();
                //  myActor.Nullify();
                myFileName = null;
            }
        }


        //! Commands the transfer on reading for an entity to data for
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


    //! TransferWriter gives help to control transfer to write a file
    //! after having converted data from Cascade/Imagine
    //!
    //! It works with a Controller (which itself can work with an
    //! Actor to Write) and a FinderProcess. It records results and
    //! checks
    public class XSControl_TransferWriter
    {
        Transfer_FinderProcess myTransferWriter;

        public void Clear(int mode)
        {
            if (mode < 0 || myTransferWriter == null)
                myTransferWriter = new Transfer_FinderProcess();
            else myTransferWriter.Clear();
        }
    }


    //! Adds specific features to the generic definition :
    //! PrintTrace is adapted
    public class Transfer_FinderProcess : Transfer_ProcessForFinder
    {
        internal void Clear()
        {
            throw new NotImplementedException();
        }
    }

    public class Transfer_ProcessForFinder
    {
    }


    //! The original class was renamed. Compatibility only
    public class Transfer_ActorOfTransientProcess : Transfer_ActorOfProcessForTransient
    {
    }


    public class Transfer_ActorOfProcessForTransient
    {
    }
}