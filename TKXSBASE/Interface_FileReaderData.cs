namespace TKXSBASE
{
    //! This class defines services which permit to access Data issued
    //! from a File, in a form which does not depend of physical
    //! format : thus, each Record has an attached ParamList (to be
    //! managed) and resulting Entity.
    //!
    //! Each Interface defines its own FileReaderData : on one hand by
    //! defining deferred methods given here, on the other hand by
    //! describing literal data and their accesses, with the help of
    //! basic classes such as String, Array1OfString, etc...
    //!
    //! FileReaderData is used by a FileReaderTool, which is also
    //! specific of each Norm, to read an InterfaceModel of the Norm
    //! FileReaderData inherits TShared to be accessed by Handle :
    //! this allows FileReaderTool to define more easily the specific
    //! methods, and improves memory management.
    public class Interface_FileReaderData
    {
        public int NbRecords() 
          {  return thenumpar.Upper();  }
        TColStd_Array1OfInteger thenumpar = new TKernel.NCollection_Array1<int>();

    }
}
