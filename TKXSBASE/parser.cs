using System.Net.Http.Headers;

namespace TKXSBASE
{
    /// A Bison parser.
    public class parser
    {
        public parser(scanner aScanner)
        {
            scanner = aScanner;
        }
        scanner scanner;
        public StepFile_ReadData StepData => this.scanner.myDataModel;
        /// The stack.
        stack_type yystack_ = new stack_type();

        sbyte[] yypact_ =
       {
            26,     5,   -24,   -24,   -24,    35,    29,   -24,   -24,    41,
     -24,    43,   -24,    36,   -24,    45,    41,   -24,   -24,     3,
      38,   -24,   -24,    40,   -24,    32,    45,   -24,   -24,   -24,
     -24,   -24,   -24,    36,   -24,   -24,     9,   -24,    60,    56,
     -24,    -3,    51,   -24,    17,   -24,   -24,   -24,    53,    44,
       6,    36,    59,   -24,     0,    36,   -24,    42,     6,     2,
     -24,    47,   -24,   -24,    36,   -24,   -24,    55,     2,    49,
     -24,    52,   -24,   -24,   -24,   -14,    50,   -24,   -24,    55,
     -24,   -24,   -24
  };
        internal int parse()
        {
            int yyn;
            /// Length of the RHS of the rule being reduced.
            int yylen = 0;

            // Error handling.
            int yynerrs_ = 0;
            int yyerrstatus_ = 0;


        /*-----------.
        | yybackup.  |
        `-----------*/
        yybackup:
            // Try to take a decision without lookahead.
            yyn = yypact_[+yystack_[0].state];

            switch (yyn)
            {
                case 24: // finlist: ')'
                    {
                        //  if (StepData.GetModePrint() > 0)
                        //  { printf("Record no : %d -- ", StepData->GetNbRecord() + 1); StepData->PrintCurrentRecord(); }
                        StepData.RecordNewEntity(); yyerrstatus_ = 0;
                    }
                    break;
            }

            return 0;//not origin
        }
    }
}