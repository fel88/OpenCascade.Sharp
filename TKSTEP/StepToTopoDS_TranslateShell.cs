using OCCPort;
using OCCPort.Common;
using TKBRep;
using TKernel;
using TKMath;
using TKSTEPBase;
using TKXSBASE;

namespace TKSTEP
{
    public class StepToTopoDS_TranslateShell : StepToTopoDS_Root
    {
        public TopoDS_Shape Value()
        {
            Exceptions.StdFail_NotDone_Raise_if(!done, "StepToTopoDS_TranslateShell::Value() - no result");
            return myResult;
        }

        StepToTopoDS_TranslateShellError myError;
        TopoDS_Shape myResult;
        public void Init
(StepShape_ConnectedFaceSet CFS,
 StepToTopoDS_Tool aTool,
 StepToTopoDS_NMTool NMTool,
  Message_ProgressRange theProgress)
        {
            //bug15697
            if (CFS == null)
                return;

            if (!aTool.IsBound(CFS))
            {

                BRep_Builder B = new BRep_Builder();
                Transfer_TransientProcess TP = aTool.TransientProcess();

                int NbFc = CFS.NbCfsFaces();
                TopoDS_Shell Sh = new TopoDS_Shell();
                B.MakeShell(Sh);
                TopoDS_Face F;
                TopoDS_Shape S;
                StepShape_Face StepFace = null;

                StepToTopoDS_TranslateFace myTranFace = new StepToTopoDS_TranslateFace();
                //   myTranFace.SetPrecision(Precision()); //gka
                // myTranFace.SetMaxTol(MaxTol());

                Message_ProgressScope PS = new(theProgress, "Face", NbFc);
                for (int i = 1; i <= NbFc && PS.More(); i++, PS.Next())
                {

                    StepFace = CFS.CfsFacesValue(i);
                    StepShape_FaceSurface theFS =
                    (StepShape_FaceSurface)(StepFace);
                    if (theFS != null)
                    {
                        myTranFace.Init(theFS, aTool, NMTool);
                        if (myTranFace.IsDone())
                        {
                            //  S = myTranFace.Value();
                            //F = TopoDS.Face(S);
                            // B.Add(Sh, F);
                        }
                        else
                        { // Warning only + add FaceSurface file Identifier
                            //TP->AddWarning(theFS, " a Face from Shell not mapped to TopoDS");
                        }
                    }
                    //  else
                    { // Warning : add identifier
                        //TP->AddWarning(StepFace, " Face is not of FaceSurface Type; not mapped to TopoDS");
                    }
                }
                Sh.Closed(BRep_Tool.IsClosed(Sh));
                myResult = Sh;
                //  aTool.Bind(CFS, myResult);
                myError = StepToTopoDS_TranslateShellError.StepToTopoDS_TranslateShellDone;
                done = true;
            }
            else
            {
                //myResult = TopoDS.Shell(aTool.Find(CFS));
                myError = StepToTopoDS_TranslateShellError.StepToTopoDS_TranslateShellDone;
                done = true;
            }
        }
    }
}
