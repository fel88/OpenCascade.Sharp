using System.Reflection.Metadata;
using TKXSBASE;

namespace TKSTEP
{
    //! defines basic controller for STEP processor
    public class STEPControl_Controller : XSControl_Controller
    {
        public override  Interface_InterfaceModel NewModel()
        {
            return STEPEdit.NewModel();
        }

    }

    //! This class allows to consult and prepare/edit  data stored in
    //! a Step Model  Header
    public class APIHeaderSection_MakeHeader
    {
        public StepData_StepModel NewModel(Interface_Protocol protocol)
        {
            StepData_StepModel stepmodel = new StepData_StepModel();
            stepmodel.SetProtocol(protocol);

            // - Make Header information

            Apply(stepmodel);
            return stepmodel;
        }

        private void Apply(StepData_StepModel stepmodel)
        {
            throw new NotImplementedException();
        }
    }

}
