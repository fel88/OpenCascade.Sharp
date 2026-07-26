using OCCPort.Common;
using TKXSBASE;

namespace TKSTEP
{
    //! Protocol for StepAP214 Entities
    //! It requires StepAP214 as a Resource
    public class StepAP214_Protocol : StepData_Protocol
    {
        public override Interface_Protocol Resource(int num)
        {
            return HeaderSection.Protocol();
        }

    }

}
