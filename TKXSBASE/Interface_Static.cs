using OCCPort.Common;
using System.Xml.Linq;

namespace TKXSBASE
{
    //! This class gives a way to manage meaningful static variables,
    //! used as "global" parameters in various procedures.
    //!
    //! A Static brings a specification (its type, constraints if any)
    //! and a value. Its basic form is a string, it can be specified
    //! as integer or real or enumerative string, and queried as such.
    //! Its string content, which is a Handle(HAsciiString) can be
    //! shared by other data structures, hence gives a direct on line
    //! access to its value.
    //!
    //! All this description is inherited from TypedValue
    //!
    //! A Static can be given an initial value, it can be filled from,
    //! either a set of Resources (an applicative feature which
    //! accesses and manages parameter files), or environment or
    //! internal definition : these define families of Static.
    //! In addition, it supports a status for reinitialisation : an
    //! initialisation procedure can ask if the value of the Static
    //! has changed from its last call, in this case does something
    //! then marks the Status "uptodate", else it does nothing.
    //!
    //! Statics are named and recorded then accessed in an alphabetic
    //! dictionary
    public class Interface_Static : Interface_TypedValue
    {

        //! Returns the integer value of
        //! the translation parameter identified by the string name.
        //! Returns the value 0 if the parameter does not exist.
        //! Example
        //! Interface_Static::IVal("write.step.schema");
        //! which could return: 3
        public static int IVal(string name)
        {
            Interface_Static item = Interface_Static.Static(name);
            if (item == null)
            {
                return 0;
            }
            return item.IntegerValue();
        }
        public static Interface_Static Static(string name)
        {
            object result = null;
            MoniTool_TypedValue.Stats().Find(name, ref result);
            return (Interface_Static)(result);
        }


    }
}
