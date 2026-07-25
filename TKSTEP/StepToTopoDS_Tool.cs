using TKBRep;
using TKSTEPBase;
using TKXSBASE;


namespace TKSTEP
{
    //! This Tool Class provides Information to build
    //! a Cas.Cad BRep from a ProSTEP Shape model.
    public class StepToTopoDS_Tool
    {
        internal bool IsBound(StepShape_TopologicalRepresentationItem TRI)
        {
            return myDataMap.IsBound(TRI);

        }
        StepToTopoDS_DataMapOfTRI myDataMap;

        Transfer_TransientProcess myTransProc;

        internal Transfer_TransientProcess TransientProcess()
        {
            return myTransProc;
        }

        internal TopoDS_Shape Find(StepShape_FaceSurface fS)
        {
            throw new NotImplementedException();
        }
    }
}
