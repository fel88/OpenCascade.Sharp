using OCCPort.Common;

namespace TKXSBASE
{
    //! Specific FileReaderTool for Step; works with FileReaderData
    //! provides references evaluation, plus access to literal data
    //! and specific methods defined by FileReaderTool
    //! Remarks : works with a ReaderLib to load Entities
    public class StepData_StepReaderTool : Interface_FileReaderTool
    {
        public void Prepare(bool optim)
        {
            //   SetEntityNumbers a ete mis du cote de ReaderData, because beaucoup acces
            //bool erh = ErrorHandle();
            StepData_StepReaderData stepdat = (StepData_StepReaderData)(Data());
            //DeclareAndCast(StepData_StepReaderData, stepdat, Data());
           // if (erh)
            {
                try
                {
                    // OCC_CATCH_SIGNALS
                    stepdat.SetEntityNumbers(optim);
                    SetEntities();
                }
                catch (Standard_Failure anException)
                {
                    /*Message_Messenger::StreamBuffer sout = Message::SendInfo();
                    sout << " Exception Raised during Preparation :\n";
                    sout << anException.GetMessageString();
                    sout << "\n Now, trying to continue, but with presomption of failure\n";*/
                }
            }
           // else
            {
               // stepdat.SetEntityNumbers(optim);
              //  SetEntities();
            }
        }
    }
}
