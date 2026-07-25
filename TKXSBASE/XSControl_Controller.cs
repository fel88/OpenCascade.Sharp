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
        //! Creates a new empty Model ready to receive data of the Norm
        //! Used to write data from Imagine to an interface file
        public abstract   Interface_InterfaceModel NewModel() ;

    }
}