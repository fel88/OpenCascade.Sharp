namespace TKXSBASE
{
    //! Gives access to
    //! - entities in a STEP file,
    //! - the STEP file header.
    public class StepData_StepModel : Interface_InterfaceModel
    {

        //! Return the encoding of STEP file for converting names into UNICODE.
        //! Initialized from "read.step.codepage" variable by constructor, which is Resource_UTF8 by default.
    public     Resource_FormatType SourceCodePage()  { return mySourceCodePage; }

        //! erases specific labels, i.e. clears the map (entity-ident)
        public override void ClearLabels()
        {
            theidnums = null;
        }

        public override void ClearHeader()
        {
            throw new NotImplementedException();
        }

        Interface_EntityList theheader;
        string theidnums;
        Resource_FormatType mySourceCodePage;
        bool myReadUnitIsInitialized;
        double myWriteUnit;

    }
}
