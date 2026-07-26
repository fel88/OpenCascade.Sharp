using OCCPort.Common;
using System.Reflection.Metadata;

namespace TKXSBASE
{
    //! General description of Interface Protocols. A Protocol defines
    //! a set of Entity types. This class provides also the notion of
    //! Active Protocol, as a working context, defined once then
    //! exploited by various Tools and Libraries.
    //!
    //! It also gives control of type definitions. By default, types
    //! are provided by CDL, but specific implementations, or topics
    //! like multi-typing, may involve another way
    public abstract class Interface_Protocol
    {
        public static void SetActive(Interface_Protocol aprotocol)
        {
            theact = aprotocol;
        }

        //  Gestion du Protocol actif : tres simple, une variable statique
        static Interface_Protocol theactive()
        {
            return theact;
        }
        static Interface_Protocol theact;

        //! Returns a Resource, given its rank (between 1 and NbResources)
        public abstract Interface_Protocol Resource(int num);
    }
}
