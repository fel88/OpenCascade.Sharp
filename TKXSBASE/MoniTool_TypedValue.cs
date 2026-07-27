global using MoniTool_ValueSatisfies = System.Func<string, bool>;
using OCCPort.Common;
using System;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
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

        public bool SetIntegerValue(int ival)
        {
            string hval = (ival).ToString();
            if (hval.IsSameString(thehval)) return true;
            if (!Satisfies(hval)) return false;
            //thehval.Clear();
            thehval = "";
            if (thetype == MoniTool_ValueType.MoniTool_ValueEnum)
            {
                //thehval.AssignCat(EnumVal(ival)); 
                thehval += (EnumVal(ival));
            }
            else
            {
                thehval += hval;
                //thehval.AssignCat(hval);
            }
            theival = ival;
            return true;
        }


        public bool IntegerLimit(bool max, out int val)
        {
            bool res = false;
            if (max) { res = (thelims & 2) != 0; val = (res ? theintup : Standard_Integer.IntegerLast()); }
            else { res = (thelims & 1) != 0; val = (res ? theintlow : Standard_Integer.IntegerFirst()); }
            return res;
        }

        string thehval;
        MoniTool_ValueSatisfies thesatisf;

        public bool RealLimit(bool max, out double val)
        {
            bool res = false;
            if (max) { res = (thelims & 2) != 0; val = (res ? therealup : Standard_Real.RealLast()); }
            else { res = (thelims & 1) != 0; val = (res ? therealow : Standard_Real.RealFirst()); }
            return res;
        }

        public bool Satisfies(string val)
        {
            if (val == null) return false;
            if (thesatisf != null)
                if (!thesatisf(val)) return false;
            if (val.Length() == 0) return true;
            switch (thetype)
            {
                case MoniTool_ValueType.MoniTool_ValueInteger:
                    {
                        if (!val.IsIntegerValue()) return false;
                        int ival, ilim; ival = int.Parse(val);
                        if (IntegerLimit(false, out ilim))
                            if (ilim > ival) return false;
                        if (IntegerLimit(true, out ilim))
                            if (ilim < ival) return false;
                        return true;
                    }
                case MoniTool_ValueType.MoniTool_ValueReal:
                    {
                        if (!val.IsRealValue()) return false;
                        double rval, rlim; rval = val.RealValue();
                        if (RealLimit(false, out rlim))
                            if (rlim > rval) return false;
                        if (RealLimit(true, out rlim))
                            if (rlim < rval) return false;
                        return true;
                    }
                case MoniTool_ValueType.MoniTool_ValueEnum:
                    {
                        //  On admet les deux formes : Enum de preference, sinon Integer
                        int startcase = 0, endcase = 0;// unused ival;
                        bool match = false;
                        EnumDef(ref startcase, ref endcase, ref match);
                        if (!match) return true;
                        if (EnumCase(val) >= startcase) return true;
                        //  Ici, on admet un entier dans la fourchette
                        ////      if (val->IsIntegerValue()) ival = atoi (val->ToCString());

                        // PTV 16.09.2000 The if is comment, cause this check is never been done (You can see the logic)
                        //      if (ival >= startcase && ival <= endcase) return Standard_True;
                        return false;
                    }
                case MoniTool_ValueType.MoniTool_ValueText:
                    {
                        if (themaxlen > 0 && val.Length() > themaxlen) return false;
                        break;
                    }
                default: break;
            }
            return true;
        }

        public bool EnumDef(ref int startcase, ref int endcase, ref bool match)
        {
            if (thetype != MoniTool_ValueType.MoniTool_ValueEnum) return false;
            startcase = theintlow; endcase = theintup;
            match = ((thelims & 4) != 0);
            return true;
        }

        public bool SetCStringValue(string val)
        {
            string hval = val;
            if (hval.IsSameString(thehval)) return true;
            if (!Satisfies(hval)) return false;
            if (thetype == MoniTool_ValueType.MoniTool_ValueInteger)
            {
                //thehval.Clear();
                thehval = "";

                theival = int.Parse(val);
                //thehval.AssignCat(val);
                thehval += (val);
            }
            else if (thetype == MoniTool_ValueType.MoniTool_ValueEnum)
            {
                int ival = EnumCase(val);
                string cval = EnumVal(ival);
                if (cval == null || cval[0] == '\0') return false;
                theival = ival;
                //thehval.Clear();
                //thehval.AssignCat(cval);
                thehval = "";
                thehval += cval;

            }
            else
            {
                //  thehval.Clear();
                // thehval.AssignCat(val);
                thehval = "";

                thehval += val;
                return true;
            }
            return true;
        }

        NCollection_DataMap<string, int> theeadds = new NCollection_DataMap<string, int>();

        public int EnumCase(string val)
        {
            if (thetype != MoniTool_ValueType.MoniTool_ValueEnum) return (theintlow - 1);
            int i; // svv Jan 10 2000 : porting on DEC
            for (i = theintlow; i <= theintup; i++)
                if (theenums.Value(i).IsEqual(val)) return i;
            //  cas additionnel ?
            if (!theeadds.IsEmpty())
            {
                if (theeadds.Find(val, ref i)) return i;
            }
            //  entier possible
            //gka S4054
            for (i = 0; val[i] != '\0'; i++)
                if (val[i] != ' ' && val[i] != '-' && (val[i] < '0' || val[i] > '9')) return (theintlow - 1);
            return int.Parse(val);
        }

        NCollection_Array1<string> theenums = new NCollection_Array1<string>();

        public string EnumVal(int num)
        {
            if (thetype != MoniTool_ValueType.MoniTool_ValueEnum) return "";
            if (num < theintlow || num > theintup) return "";
            return theenums.Value(num);
        }

        public void SetIntegerLimit(bool max, int val)
        {
            if (thetype != MoniTool_ValueType.MoniTool_ValueInteger) throw new Standard_ConstructionError("MoniTool_TypedValue : SetIntegerLimit, not an Integer");

            if (max) { thelims |= 2; theintup = val; }
            else { thelims |= 1; theintlow = val; }
        }
        MoniTool_ValueType thetype;

        int thelims;
        int themaxlen;
        int theintlow;
        int theintup;
        double therealow;
        double therealup;


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


    enum MoniTool_ValueType
    {
        MoniTool_ValueMisc,
        MoniTool_ValueInteger,
        MoniTool_ValueReal,
        MoniTool_ValueIdent,
        MoniTool_ValueVoid,
        MoniTool_ValueText,
        MoniTool_ValueEnum,
        MoniTool_ValueLogical,
        MoniTool_ValueSub,
        MoniTool_ValueHexa,
        MoniTool_ValueBinary
    };

}
