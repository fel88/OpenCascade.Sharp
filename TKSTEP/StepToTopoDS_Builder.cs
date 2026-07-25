using OCCPort;
using OCCPort.Common;
using TKBRep;
using TKernel;
using TKMath;
using TKSTEPBase;
using TKXSBASE;

namespace TKSTEP
{
    public class StepToTopoDS_Builder : StepToTopoDS_Root
    {
        StepToTopoDS_BuilderError myError;
        TopoDS_Shape myResult;

        static void ResetPreci(TopoDS_Shape S, double maxtol)
        {
            //:S4136
            //int modetol = Interface_Static.IVal("read.maxprecision.mode");
            //if (modetol)
            {
               // ShapeFix_ShapeTolerance STU;
              //  STU.LimitTolerance(S, Precision.Confusion(), maxtol);
            }
        }

        public void Init
          (StepShape_ManifoldSolidBrep aManifoldSolid,
            Transfer_TransientProcess TP,
            Message_ProgressRange theProgress)
        {
            //Message_Messenger::StreamBuffer sout = TP->Messenger()->SendInfo();
            // Initialisation of the Tool

            StepToTopoDS_Tool myTool = new StepToTopoDS_Tool();
            //StepToTopoDS_DataMapOfTRI aMap;

          //  myTool.Init(aMap, TP);

            // Start Mapping

            StepShape_ConnectedFaceSet aShell = new StepShape_ConnectedFaceSet();
            aShell = aManifoldSolid.Outer();

            StepToTopoDS_TranslateShell myTranShell = new StepToTopoDS_TranslateShell();
            //myTranShell.SetPrecision(Precision());
            // myTranShell.SetMaxTol(MaxTol());
            // Non-manifold topology is not referenced by ManifoldSolidBrep (ssv; 14.11.2010)
            StepToTopoDS_NMTool dummyNMTool = new StepToTopoDS_NMTool();
            myTranShell.Init(aShell, myTool, dummyNMTool, theProgress);

            if (myTranShell.IsDone())
            {
                TopoDS_Shape Sh = myTranShell.Value();
                Sh.Closed(true);
                //BRepLib::SameParameter(Sh);
                TopoDS_Solid S = new TopoDS_Solid();
                BRep_Builder B = new BRep_Builder();
                B.MakeSolid(S);
                B.Add(S, Sh);
                myResult = S;
                myError = StepToTopoDS_BuilderError.StepToTopoDS_BuilderDone;
                done = true;

                // Get Statistics :

                /* if (TP->TraceLevel() > 2)
                 {
                     sout << "Geometric Statistics : " << std::endl;
                     sout << "   Surface Continuity : - C0 : " << myTool.C0Surf() << std::endl;
                     sout << "                        - C1 : " << myTool.C1Surf() << std::endl;
                     sout << "                        - C2 : " << myTool.C2Surf() << std::endl;
                     sout << "   Curve Continuity :   - C0 : " << myTool.C0Cur3() << std::endl;
                     sout << "                        - C1 : " << myTool.C1Cur3() << std::endl;
                     sout << "                        - C2 : " << myTool.C2Cur3() << std::endl;
                     sout << "   PCurve Continuity :  - C0 : " << myTool.C0Cur2() << std::endl;
                     sout << "                        - C1 : " << myTool.C1Cur2() << std::endl;
                     sout << "                        - C2 : " << myTool.C2Cur2() << std::endl;
                 }*/

                //:S4136    ShapeFix::SameParameter (S,Standard_False);
                ResetPreci(S, MaxTol());
            }
            else
            {
                //TP->AddWarning(aShell, " OuterShell from ManifoldSolidBrep not mapped to TopoDS");
                myError = StepToTopoDS_BuilderError.StepToTopoDS_BuilderOther;
                done = false;
            }
        }

        private double MaxTol()
        {
            throw new NotImplementedException();
        }
    }
}
