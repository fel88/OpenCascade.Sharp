
global using TopTools_SequenceOfShape = TKernel.NCollection_Sequence<TKBRep.TopoDS_Shape>;
using OCCPort;
using OCCPort.Common;
using System.Reflection.Metadata;
using TKBRep;
using TKernel;
using TKG3d;
using TKShHealing;
using TKXSBASE;

namespace TKSTEP
{
    //! A groundwork to convert a shape to data which complies
    //! with a particular norm. This data can be that of a whole
    //! model or that of a specific list of entities in the model.
    //! You specify the list using a single selection or a
    //! combination of selections. A selection is an operator which
    //! computes a list of entities from a list given in input. To
    //! specify the input, you can use:
    //! - A predefined selection such as "xst-transferrable-roots"
    //! - A filter based on a  signature.
    //! A signature is an operator which returns a string from an
    //! entity according to its type.
    //! For example:
    //! - "xst-type" (CDL)
    //! - "iges-level"
    //! - "step-type".
    //! A filter can be based on a signature by giving a value to
    //! be matched by the string returned. For example,
    //! "xst-type(Curve)".
    //! If no list is specified, the selection computes its list of
    //! entities from the whole model. To use this class, you have to
    //! initialize the transfer norm first, as shown in the example below.
    //! Example:
    //! Control_Reader reader;
    //! IFSelect_ReturnStatus status = reader.ReadFile (filename.);
    //! When using IGESControl_Reader or STEPControl_Reader - as the
    //! above example shows - the reader initializes the norm directly.
    //! Note that loading the file only stores the data. It does
    //! not translate this data. Shapes are accumulated by
    //! successive transfers. The last shape is cleared by:
    //! - ClearShapes which allows you to handle a new batch
    //! - TransferRoots which restarts the list of shapes from scratch.
    public class XSControl_Reader
    {

        public XSControl_Reader()
        {
            SetWS(new XSControl_WorkSession());


        }
        protected bool therootsta;

        //! Sets a specific session to <me>
        public void SetWS(XSControl_WorkSession WS,
                                bool scratch = true)
        {
            therootsta = false;
            theroots.Clear();
            thesession = WS;
            //  Il doit y avoir un Controller ...  Sinon onverra plus tard (apres SetNorm)
            if (thesession.NormAdaptor() == null)
                return;

            Interface_InterfaceModel model = thesession.Model();
            if (scratch || model == null) model = thesession.NewModel();
            thesession.InitTransferReader(0);
            thesession.InitTransferReader(4);
        }

        //! Loads a file and returns the read status
        //! Zero for a Model which compies with the Controller
        public IFSelect_ReturnStatus ReadFile(string filename)
        {
            IFSelect_ReturnStatus stat = thesession.ReadFile(filename);
            thesession.InitTransferReader(4);
            return stat;
        }

        public void PrintCheckLoad(
                                          bool failsonly,
                                          IFSelect_PrintCount mode)
        {
            //Message_Messenger.StreamBuffer aBuffer = Message.SendInfo();
            //PrintCheckLoad(aBuffer, failsonly, mode);
        }

        //! Returns the shape resulting
        //! from a translation and identified by the rank num.
        //! num equals 1 by default. In other words, the first shape
        //! resulting from the translation is returned.
        public TopoDS_Shape Shape(int num = 1)
        {
            return theshapes.Value(num);

        }

        protected NCollection_Sequence<object> theroots = new NCollection_Sequence<object>();

        XSControl_WorkSession thesession;
        TopTools_SequenceOfShape theshapes;
        public int TransferRoots(Message_ProgressRange theProgress = null)
        {
            if (theProgress == null) theProgress = new Message_ProgressRange();
            //NbRootsForTransfer();
            int nbt = 0;
            int i, nb = theroots.Length();
            XSControl_TransferReader TR = thesession.TransferReader();

            //TR.BeginTransfer();
            //  ClearShapes();
            ShapeExtend_Explorer STU = new ShapeExtend_Explorer();
            Message_ProgressScope PS = new(theProgress, "Root", nb);
            for (i = 1; i <= nb && PS.More(); i++)
            {
                object start = theroots.Value(i);
                if (TR.TransferOne(start, true, PS.Next()) == 0) continue;
                //   TopoDS_Shape sh = TR.ShapeResult(start);
                // if (STU.ShapeType(sh, true) == TopAbs_ShapeEnum.TopAbs_SHAPE) continue;  // nulle-vide
                //   theshapes.Append(sh);
                nbt++;
            }
            return nbt;
        }

    }
}
