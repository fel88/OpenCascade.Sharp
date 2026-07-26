using OCCPort.Common;
using System.Xml.Linq;
using TKXSBASE;

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
        public static bool Init(string family, string name,
       char type, string init)
        {
            Interface_ParamType epyt;
            switch (type)
            {
                case 'e': epyt = Interface_ParamType.Interface_ParamEnum; break;
                case 'i': epyt = Interface_ParamType.Interface_ParamInteger; break;
                case 'o': epyt = Interface_ParamType.Interface_ParamIdent; break;
                case 'p': epyt = Interface_ParamType.Interface_ParamText; break;
                case 'r': epyt = Interface_ParamType.Interface_ParamReal; break;
                case 't': epyt = Interface_ParamType.Interface_ParamText; break;
                case '=': epyt = Interface_ParamType.Interface_ParamMisc; break;
                case '&':
                    {
                        Interface_Static unstat = Interface_Static.Static(name);
                        if (unstat==null) return false;
                        //    Editions : init donne un petit texte d edition, en 2 termes "cmd var" :
                        //  imin <ival>  imax <ival>  rmin <rval>  rmax <rval>  unit <def>
                        //  enum <from>  ematch <from>  eval <cval>
                        int i, iblc = 0;
                        for (i = 0; init[i] != '\0'; i++) if (init[i] == ' ') iblc = i + 1;
                        //  Reconnaissance du sous-cas et aiguillage
                     /*   if (init[0] == 'i' && init[2] == 'i')
                            unstat.SetIntegerLimit(false, int.Parse(init[iblc]));
                        else if (init[0] == 'i' && init[2] == 'a')
                            unstat.SetIntegerLimit(Standard_True, atoi(&init[iblc]));
                        else if (init[0] == 'r' && init[2] == 'i')
                            unstat.SetRealLimit(Standard_False, Atof(&init[iblc]));
                        else if (init[0] == 'r' && init[2] == 'a')
                            unstat.SetRealLimit(Standard_True, Atof(&init[iblc]));
                        else if (init[0] == 'u')
                            unstat.SetUnitDef(&init[iblc]);
                        else if (init[0] == 'e' && init[1] == 'm')
                            unstat.StartEnum(atoi(&init[iblc]), Standard_True);
                        else if (init[0] == 'e' && init[1] == 'n')
                            unstat.StartEnum(atoi(&init[iblc]), Standard_False);
                        else if (init[0] == 'e' && init[1] == 'v')
                            unstat.AddEnum(&init[iblc]);
                        else return false;*/
                        return true;
                    }
                default: return false;
            }
         //   if (!Interface_Static.Init(family, name, epyt, init)) return false;
            if (type != 'p') return true;
            Interface_Static stat = Interface_Static.Static(name);
            //NT  stat->SetSatisfies (StaticPath,"Path");
           // if (!stat->Satisfies(stat->HStringValue())) stat->SetCStringValue("");
            return true;
        }
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

        internal static void Standards()
        {
            throw new NotImplementedException();
        }
    }



   public  enum Interface_ParamType
    {
        Interface_ParamMisc,
        Interface_ParamInteger,
        Interface_ParamReal,
        Interface_ParamIdent,
        Interface_ParamVoid,
        Interface_ParamText,
        Interface_ParamEnum,
        Interface_ParamLogical,
        Interface_ParamSub,
        Interface_ParamHexa,
        Interface_ParamBinary
    };
}
