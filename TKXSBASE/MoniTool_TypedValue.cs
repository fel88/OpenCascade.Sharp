using System.Xml.Linq;
using TKernel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TKXSBASE
{
    //! This class allows to dynamically manage .. typed values, i.e.
    //! values which have an alphanumeric expression, but with
    //! controls. Such as "must be an Integer" or "Enumerative Text"
    //! etc
    //!
    //! Hence, a TypedValue brings a specification (type + constraints
    //! if any) and a value. Its basic form is a string, it can be
    //! specified as integer or real or enumerative string, then
    //! queried as such.
    //! Its string content, which is a Handle(HAsciiString) can be
    //! shared by other data structures, hence gives a direct on line
    //! access to its value.
    public class MoniTool_TypedValue
    {
        int theival;

        public int IntegerValue()
        { return theival; }

        static NCollection_DataMap<string, object> astats = new NCollection_DataMap<string, object>();

        public static NCollection_DataMap<string, object> Stats()
        {
            return astats;
        }

        

}


//! Defines services which are required to load an InterfaceModel
//! from a File. Typically, it may firstly transform a system
//! file into a FileReaderData object, then work on it, not longer
//! considering file contents, to load an Interface Model.
//! It may also work on a FileReaderData already loaded.
//!
//! FileReaderTool provides, on one hand, some general services
//! which are common to all read operations but can be redefined,
//! plus general actions to be performed specifically for each
//! Norm, as deferred methods to define.
//!
//! In particular, FileReaderTool defines the Interface's Unknown
//! and Error entities
public class Interface_FileReaderTool
{
    Interface_FileReaderData thereader;

    int thetrace;
    bool theerrhand;
    int thenbrep0;
    int thenbreps;


    public Interface_FileReaderData Data()
    {
        return thereader;
    }


    //! Fills records with empty entities; once done, each entity can
    //! ask the FileReaderTool for any entity referenced through an
    //! identifier. Calls Recognize which is specific to each specific
    //! type of FileReaderTool
    public void SetEntities()
    {
        int num;
        thenbreps = 0; thenbrep0 = 0;

        // for (num = thereader.FindNextRecord(0); num > 0;
        // num = thereader.FindNextRecord(num))
        {
            object newent;
            Interface_Check ach = new Interface_Check();
            // if (!Recognize(num, ach, newent))
            {
                //   newent = UnknownEntity();
                //   if (thereports.IsNull()) thereports =
                //  new TColStd_HArray1OfTransient(1, thereader->NbRecords());
                thenbreps++; thenbrep0++;
                //  thereports.SetValue(num, new Interface_ReportEntity(ach, newent));
            }
            //  else if ((ach.NbFails() + ach.NbWarnings() > 0) && !newent.IsNull())
            {
                //    if (thereports.IsNull()) thereports =
                //  new TColStd_HArray1OfTransient(1, thereader.NbRecords());
                //  thenbreps++; thenbrep0++;
                //  thereports.SetValue(num, new Interface_ReportEntity(ach, newent));
            }
            //     thereader.BindEntity(num, newent);
        }
    }
}
}
