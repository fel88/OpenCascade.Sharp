using OCCPort.Common;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using TKernel;

namespace TKXSBASE
{
    //! This class defines the (empty) frame which can be used to
    //! enrich a XSTEP set with new capabilities
    //! In particular, a specific WorkLibrary must give the way for
    //! Reading a File into a Model, and Writing a Model to a File
    //! Thus, it is possible to define several Work Libraries for each
    //! norm, but recommended to define one general class for each one :
    //! this general class will define the Read and Write methods.
    //!
    //! Also a Dump service is provided, it can produce, according the
    //! norm, either a parcel of a file for an entity, or any other
    //! kind of information relevant for the norm,
    public abstract class IFSelect_WorkLibrary
    {
        public void SetDumpHelp(int level, string help)
        {
            if (thelevhlp == null) return;
            if (level < 0 || level > thelevhlp.Upper()) return;
            string str = (help);
            thelevhlp.SetValue(level, str);
        }

        public  void SetDumpLevels(int def, int max)
        {
            thelevdef = def;
            thelevhlp = null;
            if (max >= 0) thelevhlp = new NCollection_Array1<string>(0, max);
        }

        int thelevdef;
        NCollection_Array1<string> thelevhlp = null;
        //! Gives the way to Read a File and transfer it to a Model
        //! <mod> is the resulting Model, which has to be created by this
        //! method. In case of error, <mod> must be returned Null
        //! Return value is a status with free values.
        //! Simply, 0 is for "Execution OK"
        //! The Protocol can be used to work (e.g. create the Model, read
        //! and recognize the Entities)
        public abstract int ReadFile(string name, ref Interface_InterfaceModel model, Interface_Protocol protocol);

    }
}
