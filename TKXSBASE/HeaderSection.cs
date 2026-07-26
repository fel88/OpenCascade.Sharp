using OCCPort.Common;

namespace TKXSBASE
{
    public class HeaderSection
    {
        public static HeaderSection_Protocol Protocol()
        {
            HeaderSection_Protocol proto = new HeaderSection_Protocol();//??
            return proto;
        }


    }

    //! Protocol for HeaderSection Entities
    //! It requires HeaderSection as a Resource
    public class HeaderSection_Protocol : StepData_Protocol
    {
    }


    //! Description of Basic Protocol for Step
    //! The class Protocol from StepData itself describes a default
    //! Protocol, which recognizes only UnknownEntities.
    //! Sub-classes will redefine CaseNumber and, if necessary,
    //! NbResources and Resources.
    public class StepData_Protocol : Interface_Protocol
    {
        public override Interface_Protocol Resource(int _ /*num*/)
        {
            
            return null;
        }
    }
}

