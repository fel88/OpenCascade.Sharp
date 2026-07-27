using OCCPort.Common;
using System.Reflection.Metadata;

namespace TKXSBASE
{
    //! GTool - General Tool for a Model
    //! Provides the functions performed by Protocol/GeneralModule for
    //! entities of a Model, and recorded in a GeneralLib
    //! Optimized : once an entity has been queried, the GeneralLib is
    //! not longer queried
    //! Shareable between several users : as a Handle
    public class Interface_GTool
    {

        public void SetProtocol(Interface_Protocol proto, bool enforce = false)
        {
            if (proto == theproto && !enforce) return;
            theproto = proto;
            thelib.Clear();
            thelib.AddProtocol(proto);
        }

        public Interface_GTool()    {  }

        //! Creates a GTool from a Protocol
        //! Optional starting count of entities
        public Interface_GTool(Interface_Protocol proto, int nb = 0)
        {
            theproto = (proto);
            thelib = new Interface_GeneralLib(proto);
            {
                if (nb > 0)
                {
                    thentnum.ReSize(nb);
                    thentmod.ReSize(nb);
                }
            }
        }
        Interface_GeneralLib thelib = new Interface_GeneralLib();



        Interface_Protocol theproto;
        //Interface_SignType thesign;
        //  Interface_GeneralLib thelib;
        Interface_DataMapOfTransientInteger thentnum;
        TColStd_IndexedDataMapOfTransientTransient thentmod;

        public void ClearEntities()
        { thentnum.Clear(); thentmod.Clear(); }

    }



    public class Interface_GeneralLib
    {  //! Creates a Library which complies with a Protocol, that is :
       //! Same class (criterium IsInstance)
       //! This creation gets the Modules from the global set, those
       //! which are bound to the given Protocol and its Resources
        public Interface_GeneralLib(Interface_Protocol aprotocol)
        {

        }
        //! Creates an empty Library : it will later by filled by method
        //! AddProtocol
        public Interface_GeneralLib()
        {

        }
        internal void AddProtocol(Interface_Protocol proto)
        {
            
        }

        internal void Clear()
        {
            
        }
    }
}
