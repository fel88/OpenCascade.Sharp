using OCCPort.Common;
using System.IO;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using static System.Formats.Asn1.AsnWriter;

namespace TKXSBASE
{
    //! Performs Read and Write a STEP File with a STEP Model
    //! Following the protocols, Copy may be implemented or not
    public class StepSelect_WorkLibrary : IFSelect_WorkLibrary
    {
        //! Selects a mode to dump entities
        //! 0 (D) : prints numbers, then displays table number/label
        //! 1 : prints labels, then displays table label/number
        //! 2 : prints labels onky
        public void SetDumpLabel(int mode)
        {
            thelabmode = mode;

        }

        //! Creates a STEP WorkLibrary
        //! <copymode> precises whether Copy is implemented or not
        public StepSelect_WorkLibrary(bool copymode = true)
        {
            thecopymode = (copymode);
            thelabmode = (0);

            SetDumpLevels(1, 2);
            SetDumpHelp(0, "#id + Step Type");
            SetDumpHelp(1, "Entity as in file");
            SetDumpHelp(2, "Entity + shareds (level 1) as in file");
        }



        bool thecopymode;
        int thelabmode;

        //! Reads a STEP File and returns a STEP Model (into <mod>),
        //! or lets <mod> "Null" in case of Error
        //! Returns 0 if OK, 1 if Read Error, -1 if File not opened
        public override int ReadFile(string name, ref Interface_InterfaceModel model, Interface_Protocol protocol)
        {
            StepData_Protocol stepro = protocol as StepData_Protocol;
            if (stepro == null) return 1;
            StepData_StepModel stepmodel = new StepData_StepModel();
            model = stepmodel;
            int aStatus = StepFile_Read(name, null, stepmodel, stepro);
            return aStatus;
        }
        int StepFile_Read(string theName,
                                       Stream theIStream,
                                StepData_StepModel theStepModel,
                                StepData_Protocol theProtocol)
        {
            StepData_FileRecognizer aNulRecog = new StepData_FileRecognizer();
            return StepFile_Read(theName, theIStream, theStepModel, theProtocol, aNulRecog, aNulRecog);
        }

        static int StepFile_Read(string theName,
                                               Stream theIStream,
                                       StepData_StepModel theStepModel,
                                       StepData_Protocol theProtocol,
                                       StepData_FileRecognizer theRecogHeader,
                                       StepData_FileRecognizer theRecogData)
        {
            // if stream is not provided, open file stream here
            var aStreamPtr = theIStream;
            Stream aFileStream;
            if (aStreamPtr == null)
            {
                //const Handle(OSD_FileSystem)&aFileSystem = OSD_FileSystem::DefaultFileSystem();
                // aFileStream = aFileSystem->OpenIStream(theName, std::ios::in | std::ios::binary);
                aFileStream = File.OpenRead(theName);
                aStreamPtr = aFileStream;
            }
            if (aStreamPtr == null/* || aStreamPtr->fail()*/)
            {
                return -1;
            }

            //Message_Messenger::StreamBuffer sout = Message::SendTrace();
            // sout << "      ...    Step File Reading : '" << theName << "'";

            StepFile_ReadData aFileDataModel = new StepFile_ReadData();
            try
            {
                //  OCC_CATCH_SIGNALS
                int aLetat = 0;
                scanner aScanner = new(aFileDataModel, aStreamPtr);
                aScanner.yyrestart(aStreamPtr);
                parser aParser = new(aScanner);
                aLetat = aParser.parse();
                if (aLetat != 0)
                {
                    StepFile_Interrupt(aFileDataModel.GetLastError(), true);
                    return 1;
                }
            }
            catch (Standard_Failure anException)
            {
                //Message::SendFail() << " ...  Exception Raised while reading Step File : '" << theName << "':\n"
                //                      << anException << "    ...";
                return 1;
            }


            //   sout << "      ...    STEP File   Read    ...\n";

            int nbhead, nbrec, nbpar;
            aFileDataModel.GetFileNbR(out nbhead, out nbrec, out nbpar);  // renvoi par lex/yacc
            StepData_StepReaderData undirec =
              new StepData_StepReaderData(nbhead, nbrec, nbpar, theStepModel.SourceCodePage());  // creation tableau de records
            for (int nr = 1; nr <= nbrec; nr++)
            {
                int nbarg;
                string ident; string typrec = null;
                //aFileDataModel.GetRecordDescription(&ident, &typrec, &nbarg);
                undirec.SetRecord(nr, out ident, typrec, out nbarg);

                if (nbarg > 0)
                {
                    Interface_ParamType typa; string val;
                    while (aFileDataModel.GetArgDescription(out typa, out val) == 1)
                    {
                        undirec.AddStepParam(nr, val, typa);
                    }
                }
                undirec.InitParams(nr);
                aFileDataModel.NextRecord();
            }

            // aFileDataModel.ErrorHandle(undirec->GlobalCheck());
            // int anFailsCount = undirec->GlobalCheck()->NbFails();
            //  if (anFailsCount > 0)
            {
                // Message::SendInfo() << "**** ERR StepFile : Incorrect Syntax : Fails Count : "
                //   << anFailsCount << " ****";
            }

            //  aFileDataModel.ClearRecorder(1);

            // sout << "      ... Step File loaded  ...\n";
            // sout << "   " << undirec->NbRecords() << " records (entities,sub-lists,scopes), " << nbpar << " parameters";


            //   Analyse : par StepReaderTool

            //  StepData_StepReaderTool readtool = new(undirec, theProtocol);
            // readtool.SetErrorHandle(true);

            // readtool.PrepareHeader(theRecogHeader);  // Header. reco nul -> pour Protocol
            //readtool.Prepare(theRecogData);          // Data.   reco nul -> pour Protocol

            //        sout << "      ... Parameters prepared ...\n";


            /*readtool.LoadModel(theStepModel);
            if (theStepModel->Protocol().IsNull()) theStepModel->SetProtocol(theProtocol);
            aFileDataModel.ClearRecorder(2);
            anFailsCount = undirec->GlobalCheck()->NbFails() - anFailsCount;
            if (anFailsCount > 0)*/
            {
                //  Message::SendInfo() << "*** ERR StepReaderData : Unresolved Reference : Fails Count : "
                //    << anFailsCount << " ***";
            }

            //readtool.Clear();
            //  undirec.Nullify();

            //  sout << "      ...   Objects analysed  ...\n";
            int n = theStepModel.NbEntities();
            //     sout << "  STEP Loading done : " << n << " Entities";



            return 0;
        }

        private static void StepFile_Interrupt(object v1, bool v2)
        {
            throw new NotImplementedException();
        }
    }
}