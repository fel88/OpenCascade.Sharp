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

        //! Creates a GTool from a Protocol
        //! Optional starting count of entities
        public Interface_GTool(Interface_Protocol proto, int nb = 0)
        {
            theproto = (proto);
            thelib =new Interface_GeneralLib  (proto);
            {
                if (nb > 0)
                {
                    thentnum.ReSize(nb);
                    thentmod.ReSize(nb);
                }
            }
        }
        Interface_GeneralLib thelib;



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
        public  Interface_GeneralLib( Interface_Protocol aprotocol)
        {

        }
  
    }
}
