using OCCPort.Common;
using System.Reflection.Metadata;
using System.Xml.Linq;
using TKernel;

namespace TKXSBASE
{
    //! This class allows a general X-STEP engine to run generic
    //! functions on any interface norm, in the same way. It includes
    //! the transfer operations. I.e. it gathers the already available
    //! general modules, the engine has just to know it
    //!
    //! The important point is that a given X-STEP Controller is
    //! attached to a given couple made of an Interface Norm (such as
    //! IGES-5.1) and an application data model (CasCade Shapes for
    //! instance).
    //!
    //! Finally, Controller can be gathered in a general dictionary then
    //! retrieved later by a general call (method Recorded)
    //!
    //! It does not manage the produced data, but the Actors make the
    //! link between the norm and the application
    public abstract class XSControl_Controller
    {

        //! Returns the WorkLibrary attached to the Norm. Remark that it
        //! has to be in phase with the Protocol  (read from field)
        public  IFSelect_WorkLibrary WorkLibrary() 
        { return myAdaptorLibrary; }

        Interface_Protocol myAdaptorProtocol;

        //! Returns the Protocol attached to the Norm (from field)
        public  Interface_Protocol Protocol() 
        { return myAdaptorProtocol; }
  
        IFSelect_WorkLibrary myAdaptorLibrary;

        //! Returns a name, as given when initializing :
        //! rsc = False (D) : True Name attached to the Norm (long name)
        //! rsc = True : Name of the resource set (i.e. short name)
        public string Name(bool rsc = false)
        { return (rsc ? myShortName : myLongName); }

        string myShortName;
        string myLongName;

        //! Records <me> is a general dictionary under Short and Long
        //! Names (see method Name)
        public void AutoRecord()
        {
            Record(Name(true));
            Record(Name(false));
        }

        public void Record(string theName)
        {
            if (listad.IsBound(theName))
            {
                var thisadapt = (this);
                var newadapt = listad.ChangeFind(theName);
                if (newadapt.GetType() == thisadapt.GetType())//??
                    return;

                //if (!(thisadapt->IsKind(newadapt->DynamicType())) && thisadapt != newadapt)
                if (!(thisadapt.GetType() == (newadapt.GetType())) && thisadapt != newadapt)
                    throw new Standard_DomainError("XSControl_Controller : Record");
            }
            listad.Bind(theName, this);
        }

        //! Creates a new empty Model ready to receive data of the Norm
        //! Used to write data from Imagine to an interface file
        public abstract Interface_InterfaceModel NewModel();
        static NCollection_DataMap<string, object> listad = new NCollection_DataMap<string, object>();

        public static XSControl_Controller Recorded(string theName)
        {
            object recorded = null;
            return listad.Find(theName, ref recorded) ?
              (XSControl_Controller)(recorded) :
              null;
        }

    }
}