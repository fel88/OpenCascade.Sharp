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

        public XSControl_Controller(string theLongName, string theShortName)
        {
            myShortName = (theShortName);
            myLongName = (theLongName);

            // Standard parameters
            Interface_Static.Standards();
            TraceStatic("read.precision.mode", 5);
            TraceStatic("read.precision.val", 5);
            TraceStatic("write.precision.mode", 6);
            TraceStatic("write.precision.val", 6);
        }

        //! Records a Session Item, to be added for customisation of the Work Session.
        //! It must have a specific name.
        //! <setapplied> is used if <item> is a GeneralModifier, to decide
        //! If set to true, <item> will be applied to the hook list "send".
        //! Else, it is not applied to any hook list.
        //! Remark : this method is to be called at Create time,
        //! the recorded items will be used by Customise
        //! Warning : if <name> conflicts, the last recorded item is kept
        public void AddSessionItem(object theItem, string theName, bool toApply = false)
        {
            if (theItem == null || theName[0] == '\0') return;
            myAdaptorSession.Bind(theName, theItem);
            if (toApply && theItem is IFSelect_GeneralModifier)
                myAdaptorApplied.Append(theItem);
        }

        NCollection_DataMap<string, object> myAdaptorSession = new NCollection_DataMap<string, object>();
        NCollection_Sequence<object> myAdaptorApplied = new NCollection_Sequence<object>();

        NCollection_Vector<object> myParams = new NCollection_Vector<object>();
        NCollection_Vector<int> myParamUses = new NCollection_Vector<int>();

        public void TraceStatic(string theName, int theUse)
        {
            Interface_Static val = Interface_Static.Static(theName);
            if (val == null) return;
            myParams.Append(val);
            myParamUses.Append(theUse);
        }

        //! Returns the WorkLibrary attached to the Norm. Remark that it
        //! has to be in phase with the Protocol  (read from field)
        public IFSelect_WorkLibrary WorkLibrary()
        { return myAdaptorLibrary; }

     protected   Interface_Protocol myAdaptorProtocol;

        //! Returns the Protocol attached to the Norm (from field)
        public Interface_Protocol Protocol()
        { return myAdaptorProtocol; }

      protected  IFSelect_WorkLibrary myAdaptorLibrary;
        

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