using TKXSBASE;

namespace TKSTEP
{
    //! Provides tools to exploit and edit a set of STEP data :
    //! editors, selections ..
    public class STEPEdit
    {  //! Returns a new empty StepModel fit for STEP
       //! i.e. with its header determined from Protocol
        public static StepData_StepModel NewModel()
        {
            APIHeaderSection_MakeHeader head = new APIHeaderSection_MakeHeader();
            return head.NewModel(STEPEdit.Protocol());
        }
        //! Returns a Protocol fit for STEP (creates the first time)
        public static Interface_Protocol Protocol()
        {
            /*
  static Handle(StepData_FileProtocol) proto;
  if (!proto.IsNull()) return proto;
  proto =  new StepData_FileProtocol;
  proto->Add (StepAP214::Protocol());
  proto->Add (HeaderSection::Protocol());
  return proto;
*/
            return StepAP214.Protocol();
        }



    }

}
