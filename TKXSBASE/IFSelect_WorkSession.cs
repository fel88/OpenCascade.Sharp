global using Interface_DataMapOfTransientInteger = TKernel.NCollection_DataMap<object, int, TKernel.NCollection_DefaultHasher<object>>;
global using TColStd_DataMapOfIntegerTransient = TKernel.NCollection_DataMap<int, object, TKernel.NCollection_DefaultHasher<int>>;
global using TColStd_IndexedDataMapOfTransientTransient = TKernel.NCollection_IndexedDataMap<object, object, TKernel.NCollection_DefaultHasher<object>>;
global using TColStd_IndexedMapOfTransient = TKernel.NCollection_IndexedMap<object, TKernel.NCollection_DefaultHasher<object>>;
global using TColStd_MapTransientHasher = TKernel.NCollection_DefaultHasher<object>;
using OCCPort.Common;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using TKernel;

namespace TKXSBASE
{

    //! This class can be used to simply manage a process such as
    //! splitting a file, extracting a set of Entities ...
    //! It allows to manage different types of Variables : Integer or
    //! Text Parameters, Selections, Dispatches, in addition to a
    //! ShareOut. To each of these variables, a unique Integer
    //! Identifier is attached. A Name can be attached too as desired.
    public class IFSelect_WorkSession
    {

        public IFSelect_WorkSession()
        {
            // theshareout = new IFSelect_ShareOut;
            theerrhand = errhand = true;
            //  thecopier = new IFSelect_ModelCopier;
            //thecopier->SetShareOut(theshareout);
            thecheckdone = false;
            thegtool = new Interface_GTool();
            themodelstat = false;
        }
        static bool errhand;  // pb : un seul a la fois, mais ca va si vite

        protected Interface_HGraph thegraph;

        //! Computes the Graph used for Selections, Displays ...
        //! If a HGraph is already set, with same model as given by method
        //! Model, does nothing. Else, computes a new Graph.
        //! If <enforce> is given True, computes a new Graph anyway.
        //! Remark that a call to ClearGraph will cause ComputeGraph to
        //! really compute a new Graph
        //! Returns True if Graph is OK, False else (i.e. if no Protocol
        //! is set, or if Model is absent or empty).
        public bool ComputeGraph(bool enforce = false)
        {
            if (theprotocol == null) return false;
            if (myModel == null) return false;
            if (enforce) thegraph = null;
            if (thegraph != null)
            {
                //   if (myModel.NbEntities() == thegraph.Graph().Size()) return true;
                //  thegraph = null;
            }
            if (myModel.NbEntities() == 0) return false;
            //  Il faut calculer le graphe pour de bon
            thegraph = new Interface_HGraph(myModel, themodelstat);
            int nb = myModel.NbEntities();
            if (themodelstat)
            {
                int i; // svv #1
                for (i = 1; i <= nb; i++) thegraph.CGraph().SetStatus(i, 0);
                //Interface_BitMap bm = thegraph.CGraph().CBitMap();
                // bm.AddFlag();
                // bm.SetFlagName(Flag_Incorrect, "Incorrect");
            }
            ComputeCheck();
            thecheckdone = true;
            if (themodelstat)
            {
                //  Calcul des categories, a present memorisees dans le modele
                //Interface_Category categ = new(thegtool);
                //   Interface_ShareTool sht = new(thegraph);
                //  int i = 1;
                //  for (; i <= nb; i++)
                //      myModel.SetCategoryNumber(i, categ.CatNum(myModel.Value(i), sht));
            }

            return true;
        }

        //! Returns the Model of the Work Session (Null Handle if none)
        //! should be C++ : return const &
        public Interface_InterfaceModel Model()
        { return myModel; }

        protected Interface_GTool thegtool;

        //! Sets a Protocol, which will be used to determine Graphs, to
        //! Read and to Write Files
        public void SetProtocol(Interface_Protocol protocol)
        {
            theprotocol = protocol;
            Interface_Protocol.SetActive(protocol);
            thegtool.SetProtocol(protocol);
        }

        public bool IsLoaded()
        {
            if (theprotocol == null) return false;
            if (myModel == null) return false;
            if (myModel.NbEntities() == 0) return false;
            if (thegraph == null) return false;
            //if (myModel.NbEntities() == thegraph.Graph().Size()) return true;
            return false;
        }
        const int Flag_Incorrect = 2;

        //! Computes the CheckList for the Model currently loaded
        //! It can then be used for displays, queries ...
        //! Returns True if OK, False else (i.e. no Protocol set, or Model
        //! absent). If <enforce> is False, works only if not already done
        //! or if a new Model has been loaded from last call.
        //! Remark : computation is enforced by every call to
        //! SetModel or RunTransformer
        public bool ComputeCheck(bool enforce = false)
        {
            if (enforce) thecheckdone = false;
            if (thecheckdone) return true;
            if (!IsLoaded()) return false;

            Interface_Graph CG = thegraph.CGraph();
            Interface_CheckTool cht = new(thegraph);
            Interface_CheckIterator checklist = cht.VerifyCheckList();
            myModel.FillSemanticChecks(checklist, false);
            if (themodelstat)
            {
                //  Et on met a jour le Graphe (BitMap) !  Flag Incorrect (STX + SEM)
                /*   Interface_BitMap BM = CG.CBitMap();
                   BM.Init(false, Flag_Incorrect);
                   int num, nb = CG.Size();
                   for (checklist.Start(); checklist.More(); checklist.Next())
                   {
                       Interface_Check chk = checklist.Value();
                       if (!chk.HasFailed()) continue;
                       num = checklist.Number();
                       if (num > 0 && num <= nb) BM.SetTrue(num, Flag_Incorrect);
                   }*/
                //for (num = 1; num <= nb; num++)
                //if (myModel.IsErrorEntity(num)) BM.SetTrue(num, Flag_Incorrect);
            }
            return true;
        }


        public Interface_Graph Graph()
        {
            ComputeGraph();
            if (thegraph == null) throw new Standard_DomainError("IFSelect WorkSession : Graph not available");
            return thegraph.Graph();
        }

        protected TColStd_IndexedDataMapOfTransientTransient theitems;
        //! Stores the filename used for read for setting the model
        //! It is cleared by SetModel and ClearData(1)
        public void SetLoadedFile(string theFileName)
        { theloaded = theFileName; }

        //! Sets a Model as input : this will be the Model from which the
        //! ShareOut will work
        //! if <clearpointed> is True (default) all SelectPointed items
        //! are cleared, else they must be managed by the caller
        //! Remark : SetModel clears the Graph, recomputes it if a
        //! Protocol is set and if the Model is not empty, of course
        public void SetModel(Interface_InterfaceModel model, bool clearpointed = true)
        {

            if (myModel != model)
                theloaded = string.Empty;
            myModel = model;
            //  if (thegtool != null)
            //     thegtool.ClearEntities(); //smh#14 FRA62479

            myModel.SetGTool(thegtool);

            //   thegraph.Nullify();
            //  ComputeGraph();    // fait qqchose si Protocol present. Sinon, ne fait rien
            ClearData(3);      // RAZ CheckList, a refaire
                               //    thecheckrun.Clear();

            //  MISE A JOUR des SelectPointed  C-A-D  on efface leur contenu
            if (clearpointed)
                ClearData(4);

            ClearData(0);
        }

        public object Item(int id)
        {
            object res = null;
            if (id <= 0 || id > MaxIdent())
                return res;

            if (theitems.FindFromIndex(id) == null)
                return res;

            return theitems.FindKey(id);
        }
        public int MaxIdent()
        {
            return theitems.Extent();
        }

        public void ClearData(int mode)
        {
            switch (mode)
            {
                case 1:
                    {
                        theloaded = string.Empty;
                        if (myModel != null)
                        {
                            myModel.Clear();
                            myModel = null;
                        }
                        ClearData(2); ClearData(4);
                        //   thecheckrun.Clear();
                        break;
                    }
                // case 2: { thegraph.Nullify(); thecheckdone = false; thecheckana.Clear(); break; }
                case 3: { thecheckdone = false; break; }
                case 4:
                    {
                        //  MISE A JOUR des SelectPointed  C-A-D  on efface leur contenu
                        //  AINSI que des editeurs (en fait, les EditForm)
                        //  Des compteurs  C-A-D  on efface leur contenu (a reevaluer)
                        //   TColStd_HSequenceOfInteger list =
                        //ItemIdents(STANDARD_TYPE(IFSelect_SelectPointed));
                        int nb = -1;//  int nb = list.Length();
                        int i; // svv #1 
                        for (i = 1; i <= nb; i++)
                        {
                            //   DeclareAndCast(IFSelect_SelectPointed, sp, Item(list->Value(i)));
                            //   if (!sp.IsNull()) sp->Clear();
                        }
                        // list = ItemIdents(STANDARD_TYPE(IFSelect_SignatureList));
                        //nb = list->Length();
                        for (i = 1; i <= nb; i++)
                        {
                            //   DeclareAndCast(IFSelect_SignatureList, sl, Item(list->Value(i)));
                            //  if (!sl.IsNull()) sl->Clear();
                            //    DeclareAndCast(IFSelect_SignCounter, sc, sl);
                            //   if (!sc.IsNull()) sc->SetSelMode(-1);
                        }
                        //list = ItemIdents(STANDARD_TYPE(IFSelect_EditForm));
                        //   nb = list.Length();
                        object nulent;
                        for (i = 1; i <= nb; i++)
                        {
                            //DeclareAndCast(IFSelect_EditForm, edf, Item(list->Value(i)));
                            //    IFSelect_EditForm edf = Item(list.Value(i));
                            //  edf.ClearData();
                        }
                        theitems.Clear();
                        break;
                    }
                default: break;
            }
        }
        //! Sets a WorkLibrary, which will be used to Read and Write Files
        public void SetLibrary(IFSelect_WorkLibrary theLib)
        { thelibrary = theLib; }

        bool theerrhand;
        //IFSelect_ShareOut theshareout;
        IFSelect_WorkLibrary thelibrary;
        Interface_Protocol theprotocol;
        Interface_InterfaceModel myModel;
        string theloaded;
        //Interface_GTool thegtool;
        bool thecheckdone;
        //Interface_CheckIterator thechecklist;
        string thecheckana;
        //IFSelect_ModelCopier thecopier;
        //Interface_InterfaceModel theoldel;
        bool themodelstat;

        //! Reads a file with the WorkLibrary (sets Model and LoadedFile)
        //! Returns a integer status which can be :
        //! RetDone if OK,  RetVoid if no Protocol not defined,
        //! RetError for file not found, RetFail if fail during read
        public IFSelect_ReturnStatus ReadFile(string filename)
        {
            if (thelibrary == null)
                return IFSelect_ReturnStatus.IFSelect_RetVoid;

            if (theprotocol == null)
                return IFSelect_ReturnStatus.IFSelect_RetVoid;

            Interface_InterfaceModel model = null;
            IFSelect_ReturnStatus status = IFSelect_ReturnStatus.IFSelect_RetVoid;
            try
            {
                //OCC_CATCH_SIGNALS
                int stat = thelibrary.ReadFile(filename, ref model, theprotocol);
                if (stat == 0) status = IFSelect_ReturnStatus.IFSelect_RetDone;
                else if (stat < 0) status = IFSelect_ReturnStatus.IFSelect_RetError;
                else status = IFSelect_ReturnStatus.IFSelect_RetFail;
            }
            catch (Exception anException)
            {
                //Message_Messenger::StreamBuffer sout = Message::SendInfo();
                //sout << "    ****    Interruption ReadFile par Exception :   ****\n";
                //sout << anException.GetMessageString();
                //sout << "\n    Abandon" << std::endl;
                status = IFSelect_ReturnStatus.IFSelect_RetFail;
            }
            if (status != IFSelect_ReturnStatus.IFSelect_RetDone) return status;
            if (model == null)
                return IFSelect_ReturnStatus.IFSelect_RetVoid;

            SetModel(model);
            SetLoadedFile(filename);
            return status;
        }

    }

    //! A SignatureList is given as result from a Counter (any kind)
    //! It gives access to a list of signatures, with counts, and
    //! optionally with list of corresponding entities
    //!
    //! It can also be used only to give a signature, through SignOnly
    //! Mode. This can be useful for a specific counter (used in a
    //! Selection), while it remains better to use a Signature
    //! whenever possible
    public class IFSelect_SignatureList
    {

    }


    //! An EditForm is the way to apply an Editor on an Entity or on
    //! the Model
    //! It gives read-only or read-write access, with or without undo
    //!
    //! It can be complete (all the values of the Editor are present)
    //! or partial (a sub-list of these value are present)
    //! Anyway, all references to Number (argument <num>) refer to
    //! Number of Value for the Editor
    //! While references to Rank are for rank in the EditForm, which
    //! may differ if it is not Complete
    //! Two methods give the correspondence between this Number and
    //! the Rank in the EditForm : RankFromNumber and NumberFromRank
    public class IFSelect_EditForm
    {
    }


    //! This type of Selection is intended to describe a direct
    //! selection without an explicit criterium, for instance the
    //! result of picking viewed entities on a graphic screen
    //!
    //! It can also be used to provide a list as internal alternate
    //! input : this use implies to clear the list once queried
    public class IFSelect_SelectPointed : IFSelect_SelectBase
    {

    }


    //! SelectBase works directly from an InterfaceModel : it is the
    //! first base for other Selections.
    public class IFSelect_SelectBase : IFSelect_Selection
    {
    }


    //! A SelectExplore determines from an input list of Entities,
    //! a list obtained by a way of exploration. This implies the
    //! possibility of recursive exploration : the output list is
    //! itself reused as input, etc...
    //! Examples : Shared Entities, can be considered at one level
    //! (immediate shared) or more, or max level
    //!
    //! Then, for each input entity, if it is not rejected, it can be
    //! either taken itself, or explored : it then produces a list.
    //! According to a level, either the produced lists or taken
    //! entities give the result (level one), or lists are themselves
    //! considered and for each item, is it taken or explored.
    //!
    //! Remark that rejection is just a safety : normally, an input
    //! entity is, either taken itself, or explored
    //! A maximum level can be specified. Else, the process continues
    //! until all entities have been either taken or rejected
    public class IFSelect_SelectExplore : IFSelect_SelectDeduct
    {
    }


    //! A SelectDeduct determines a list of Entities from an Input
    //! Selection, by a computation : Output list is not obliged to be
    //! a sub-list of Input list
    //! (for more specific, see SelectExtract for filtered sub-lists,
    //! and SelectExplore for recurcive exploration)
    //!
    //! A SelectDeduct may use an alternate input for one shot
    //! This allows to use an already existing definition, by
    //! overloading the input selection by an alternate list,
    //! already defined, for one use :
    //! If this alternate list is set, InputResult queries it instead
    //! of calling the input selection, then clears it immediately
    public class IFSelect_SelectDeduct : IFSelect_Selection
    {
    }
}


