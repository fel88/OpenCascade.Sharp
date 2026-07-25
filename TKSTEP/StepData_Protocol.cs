using TKXSBASE;

namespace TKSTEP
{
    //! Description of Basic Protocol for Step
    //! The class Protocol from StepData itself describes a default
    //! Protocol, which recognizes only UnknownEntities.
    //! Sub-classes will redefine CaseNumber and, if necessary,
    //! NbResources and Resources.
    public class StepData_Protocol : Interface_Protocol
    {
    }

}
