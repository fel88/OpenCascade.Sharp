


using static TKXSBASE.parser;

namespace TKXSBASE
{
    internal class stack_type
    {

        public stack_symbol_type this[int key]{
            get=>stack.ToArray()[key];
            }
        public void pop(int n)
        {
            for (int i = 0; i < n; i++)
                stack.Pop();
        }

        internal int size()
        {
            return stack.Count;
        }

        internal void clear()
        {
            stack.Clear();
        }

        internal void push(stack_symbol_type symbol_type)
        {
            stack.Push(symbol_type);
        }

        Stack<stack_symbol_type> stack = new Stack<stack_symbol_type>();
    }

    public class stack_symbol_type
    {
        public stack_symbol_type()
        {

        }

        /// Steal the contents from \a sym to build this.
        public stack_symbol_type(int s, symbol_type that)
        {
            that.kind_ = symbol_kind.symbol_kind_type.S_YYEMPTY;

        }

        /// The state.
        /// \a empty when empty.
        public sbyte state;

    }


    /// Symbol kinds.
  public   struct symbol_kind
    {
      public   enum symbol_kind_type
        {
            YYNTOKENS = 46, ///< Number of tokens.
            S_YYEMPTY = -2,
            S_YYEOF = 0,                             // "end of file"
            S_YYerror = 1,                           // error
            S_YYUNDEF = 2,                           // "invalid token"
            S_KSCHEM = 3,                            // KSCHEM
            S_KENDS = 4,                             // KENDS
            S_KTYP = 5,                              // KTYP
            S_KENDT = 6,                             // KENDT
            S_KENT = 7,                              // KENT
            S_KENDE = 8,                             // KENDE
            S_KREF = 9,                              // KREF
            S_KFROM = 10,                            // KFROM
            S_KSEL = 11,                             // KSEL
            S_KENUM = 12,                            // KENUM
            S_KLIST = 13,                            // KLIST
            S_KARR = 14,                             // KARR
            S_KBAG = 15,                             // KBAG
            S_KSET = 16,                             // KSET
            S_KOF = 17,                              // KOF
            S_KNUM = 18,                             // KNUM
            S_KINT = 19,                             // KINT
            S_KDBL = 20,                             // KDBL
            S_KSTR = 21,                             // KSTR
            S_KLOG = 22,                             // KLOG
            S_KBOOL = 23,                            // KBOOL
            S_KOPT = 24,                             // KOPT
            S_KUNIQ = 25,                            // KUNIQ
            S_KSELF = 26,                            // KSELF
            S_KABSTR = 27,                           // KABSTR
            S_KSUBT = 28,                            // KSUBT
            S_KSPRT = 29,                            // KSPRT
            S_KANDOR = 30,                           // KANDOR
            S_K1OF = 31,                             // K1OF
            S_KAND = 32,                             // KAND
            S_NUMBER = 33,                           // NUMBER
            S_NAME = 34,                             // NAME
            S_35_ = 35,                              // ','
            S_36_ = 36,                              // ';'
            S_37_ = 37,                              // '='
            S_38_ = 38,                              // '('
            S_39_ = 39,                              // ')'
            S_40_ = 40,                              // '['
            S_41_ = 41,                              // ':'
            S_42_ = 42,                              // ']'
            S_43_ = 43,                              // '?'
            S_44_ = 44,                              // '\\'
            S_45_ = 45,                              // '.'
            S_YYACCEPT = 46,                         // $accept
            S_SCHEMA = 47,                           // SCHEMA
            S_ILIST = 48,                            // ILIST
            S_ITEM = 49,                             // ITEM
            S_ENUM = 50,                             // ENUM
            S_SELECT = 51,                           // SELECT
            S_ALIAS = 52,                            // ALIAS
            S_ENTITY = 53,                           // ENTITY
            S_REFERENCE = 54,                        // REFERENCE
            S_TLIST = 55,                            // TLIST
            S_TLIST1 = 56,                           // TLIST1
            S_TYPE = 57,                             // TYPE
            S_TSTD = 58,                             // TSTD
            S_TNAME = 59,                            // TNAME
            S_TSET = 60,                             // TSET
            S_INDEX = 61,                            // INDEX
            S_OPTUNI = 62,                           // OPTUNI
            S_SUBT = 63,                             // SUBT
            S_SUPERT = 64,                           // SUPERT
            S_SUPLST = 65,                           // SUPLST
            S_FLIST = 66,                            // FLIST
            S_FLIST1 = 67,                           // FLIST1
            S_FIELD = 68,                            // FIELD
            S_REDEF = 69,                            // REDEF
            S_SPECIF = 70,                           // SPECIF
            S_OPTNL = 71,                            // OPTNL
            S_UNIQIT = 72,                           // UNIQIT
            S_UNIQLS = 73,                           // UNIQLS
            S_UNIQUE = 74,                           // UNIQUE
            S_SPCLST = 75                            // SPCLST
        };
    };
}