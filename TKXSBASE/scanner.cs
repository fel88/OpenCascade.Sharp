using TKG2d;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TKXSBASE
{
    // To feed data back to bison, the yylex method needs yylval and
    // yylloc parameters. Since the stepFlexLexer class is defined in the
    // system header <FlexLexer.h> the signature of its yylex() method
    // can not be changed anymore. This makes it necessary to derive a
    // scanner class that provides a method with the desired signature:
    public class scanner : stepFlexLexer
    {
        public scanner(StepFile_ReadData aFileDataModel, Stream aStreamPtr)
        {
        }
        public StepFile_ReadData myDataModel;

        
    }
}