using OCCPort.Common;
using TKXSBASE;

namespace TKSTEP
{

    //! Reads STEP files, checks them and translates their contents
    //! into Open CASCADE models. The STEP data can be that of
    //! a whole model or that of a specific list of entities in the model.
    //! As in XSControl_Reader, you specify the list using a selection.
    //! For the translation of iges files it is possible to use next sequence:
    //! To change translation parameters
    //! class Interface_Static should be used before beginning of
    //! translation  (see STEP Parameters and General Parameters)
    //! Creation of reader - STEPControl_Reader reader;
    //! To load s file in a model use method reader.ReadFile("filename.stp")
    //! To print load results reader.PrintCheckLoad(failsonly,mode)
    //! where mode is equal to the value of enumeration IFSelect_PrintCount
    //! For definition number of candidates :
    //! Standard_Integer nbroots = reader. NbRootsForTransfer();
    //! To transfer entities from a model the following methods can be used:
    //! for the whole model - reader.TransferRoots();
    //! to transfer a list of entities: reader.TransferList(list);
    //! to transfer one entity Handle(Standard_Transient)
    //! ent = reader.RootForTransfer(num);
    //! reader.TransferEntity(ent), or
    //! reader.TransferOneRoot(num), or
    //! reader.TransferOne(num), or
    //! reader.TransferRoot(num)
    //! To obtain the result the following method can be used:
    //! reader.NbShapes() and reader.Shape(num); or reader.OneShape();
    //! To print the results of transfer use method:
    //! reader.PrintCheckTransfer(failwarn,mode);
    //! where printfail is equal to the value of enumeration
    //! IFSelect_PrintFail, mode see above; or reader.PrintStatsTransfer();
    //! Gets correspondence between a STEP entity and a result
    //! shape obtained from it.
    //! Handle(XSControl_WorkSession)
    //! WS = reader.WS();
    //! if ( WS->TransferReader()->HasResult(ent) )
    //! TopoDS_Shape shape = WS->TransferReader()->ShapeResult(ent);
    public class STEPControl_Reader : XSControl_Reader

    {
        public STEPControl_Reader()
        {
            STEPControl_Controller.Init();
            SetNorm("STEP");
        }
        public override int NbRootsForTransfer()
        {
            if (therootsta) return theroots.Length();
            therootsta = true;

            //theroots.Clear();
            int nb = Model().NbEntities();
            for (int i = 1; i <= nb; i++)
            {
                object ent = Model().Value(i);
                if (Interface_Static.IVal("read.step.all.shapes") == 1)
                {
                    // Special case to read invalid shape_representation without links to shapes.
                    if (ent is StepShape_ManifoldSolidBrep)
                    {
                        /*Interface_EntityIterator aShareds = WS().Graph().Sharings(ent);
                        if (!aShareds.More())
                        {
                            theroots.Append(ent);
                            WS().TransferReader().TransientProcess().RootsForTransfer().Append(ent);
                        }*/
                    }
                }
            }



            return theroots.Length();
        }

    }





}