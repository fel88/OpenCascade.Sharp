using System.Net.Http.Headers;
using System.Numerics;

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
        const int YY_NULLPTR = 0;

        sbyte[] yyr1_ =
        {
            0,    27,    28,    28,    29,    29,    30,    31,    32,    33,
      33,    33,    34,    34,    35,    35,    36,    37,    37,    37,
      37,    37,    38,    39,    40,    41,    41,    42,    42,    43,
      43,    44,    44,    44,    44,    45,    45,    46,    46,    47,
      48,    49,    49,    50,    51,    51,    52,    53
        };
        sbyte[]
yyr2_ =
  {
            0,     2,     1,     2,     1,     2,     8,     7,     6,     1,
       1,     1,     1,     2,     3,     1,     1,     1,     1,     1,
       2,     1,     1,     1,     1,     2,     3,     1,     3,     1,
       2,     4,     7,     6,     1,     2,     3,     2,     3,     1,
       1,     1,     3,     1,     1,     4,     1,     1
  };

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

        /// Constants.
        enum consts
        {
            yylast_ = 82,     ///< Last index in yytable_.
            yynnts_ = 27,  ///< Number of nonterminal symbols.
            yyfinal_ = 7 ///< Termination state number.
        };

        /// "External" symbols: returned by the scanner.
        public class symbol_type //: basic_symbol<by_kind>
        {

            /// The symbol kind.
            /// \a S_YYEMPTY when empty.
            public symbol_kind.symbol_kind_type kind_;

        };

        public symbol_type YY_MOVE(symbol_type t)
        {
            return t;
        }
        public stack_symbol_type YY_MOVE(stack_symbol_type t)
        {
            return t;
        }
        internal int parse()
        {
            int yyn;
            /// Length of the RHS of the rule being reduced.
            int yylen = 0;

            // Error handling.
            int yynerrs_ = 0;
            int yyerrstatus_ = 0;
            symbol_type yyla = new symbol_type();

            /* Initialize the stack.  The initial state will be set in
               yynewstate, since the latter expects the semantical and the
               location values to have been already stored, initialize these
               stacks with a primary value.  */
            yystack_.clear();
            yypush_(YY_NULLPTR, (char)0, YY_MOVE(yyla));

        /*-----------------------------------------------.
        | yynewstate -- push a new symbol on the stack.  |
        `-----------------------------------------------*/
        yynewstate:
            //YYCDEBUG << "Entering state " << int(yystack_[0].state) << '\n';
            //  YY_STACK_PRINT();

            // Accept?
            //if (yystack_[0].state == (sbyte)consts.yyfinal_)
             //   goto yyacceptlab;

            goto yybackup;



        /*-----------.
        | yybackup.  |
        `-----------*/
        yybackup:
            // Try to take a decision without lookahead.
            yyn = yypact_[+yystack_[0].state];

        /*-----------------------------------------------------------.
        | yydefault -- do the default action for the current state.  |
        `-----------------------------------------------------------*/
        //yydefault:
           // yyn = yydefact_[+yystack_[0].state];
           // if (yyn == 0)
           //     goto yyerrlab;
          // goto yyreduce;

        /*-----------------------------.
        | yyreduce -- do a reduction.  |
        `-----------------------------*/
        yyreduce:
            yylen = yyr2_[yyn];
            {
                stack_symbol_type yylhs = new stack_symbol_type();
                //yylhs.state = yy_lr_goto_state_(yystack_[yylen].state, yyr1_[yyn]);
                /* If YYLEN is nonzero, implement the default value of the
                   action: '$$ = $1'.  Otherwise, use the top of the stack.

                   Otherwise, the following line sets YYLHS.VALUE to garbage.
                   This behavior is undocumented and Bison users should not rely
                   upon it.  */
                /*  if (yylen)
                      yylhs.value = yystack_[yylen - 1].value;
                  else
                      yylhs.value = yystack_[0].value;
                */

                // Perform the reduction.
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

            }// todo: check this bracet location

            /*-------------------------------------.
            | yyacceptlab -- YYACCEPT comes here.  |
            `-------------------------------------*/
            yyacceptlab:
                yyresult = 0;
                goto yyreturn;


            /*-----------------------------------------------------.
            | yyreturn -- parsing is finished, return the result.  |
            `-----------------------------------------------------*/
            yyreturn:
                //  if (!yyla.empty())
                //    yy_destroy_("Cleanup: discarding lookahead", yyla);

                /* Do not reclaim the symbols of the rule whose action triggered
                   this YYABORT or YYACCEPT.  */
                yypop_(yylen);
                //YY_STACK_PRINT();
                while (1 < yystack_.size())
                {
                    //  yy_destroy_("Cleanup: popping", yystack_[0]);
                    yypop_();
                }

                return yyresult;

            }

            sbyte[]
            yypgoto_ =
            {
     -24,   -24,   -24,   -24,   -24,   -24,   -24,   -24,    62,    58,
      31,   -24,   -24,    46,   -13,   -24,   -23,   -21,   -24,    -6,
     -24,    -2,   -24,   -24,    18,   -24,    -5
  };

             sbyte[]            yytable_ =
            {
      20,    13,    40,    42,    27,    47,    13,    21,    10,     6,
      79,    10,    80,    10,    28,    57,    22,    29,    27,    48,
      43,    40,    30,    63,    48,    18,    31,    58,    28,     1,
       8,    29,    31,    44,     9,     7,    30,    40,    60,    18,
      10,    38,    65,    55,     8,    39,    21,    14,    16,    64,
      22,    72,    21,    69,    10,    22,    52,    41,    18,    37,
      -8,    22,    76,    46,    10,    56,    61,    70,    66,    73,
      77,    81,    78,    17,    26,    53,    68,    82,     0,     0,
       0,     0,    45
  };


            sbyte[]  
            yycheck_ =
            {
      13,     6,    23,    26,     1,     8,    11,     1,    11,     4,
      24,    11,    26,    11,    11,     9,    10,    14,     1,    22,
      33,    42,    19,    23,    22,    22,    23,    50,    11,     3,
       1,    14,    23,    24,     5,     0,    19,    58,    51,    22,
      11,     1,    55,    48,     1,     5,     1,     6,     5,    54,
      10,    64,     1,    59,    11,    10,     5,    25,    22,    21,
       0,    10,    68,     7,    11,    21,     7,    20,    26,    14,
      21,    21,    20,    11,    16,    44,    58,    79,    -1,    -1,
      -1,    -1,    36
  };


            sbyte[]
            yydefgoto_ =
            {
      -1,    71,    62,     2,     3,     4,     5,    11,    12,    15,
      32,    33,    19,    34,    35,    36,    23,    24,    54,    49,
      50,    74,    75,    67,    59,    25,    51
  };
            int yy_lr_goto_state_(int yystate, int yysym)
            {
                int yyr = yypgoto_[yysym - (int)symbol_kind.symbol_kind_type.YYNTOKENS] + yystate;
                if (0 <= yyr && yyr <= (int)consts. yylast_ && yycheck_[yyr] == yystate)
                    return yytable_[yyr];
                else
                    return yydefgoto_[yysym - (int)symbol_kind.symbol_kind_type.YYNTOKENS];
            }

       
        private void yypush_(int yY_NULLPTR, char s, symbol_type sym)
        {
            // if (m!=null)
            //YY_SYMBOL_PRINT(m, sym);
            stack_symbol_type ss = new(s, sym);

            yystack_.push(YY_MOVE(ss));
        }

        /// Pop \a n symbols from the stack.
        void yypop_(int n = 1)
        {
            yystack_.pop(n);
        }
        /// The return value of parse ().
        int yyresult;
    }
}