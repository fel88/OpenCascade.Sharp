using OCCPort.Common;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.Intrinsics.Arm;
using System.Xml.Linq;
using TKernel;
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
                        if (unstat == null) return false;
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

        public static bool SetCVal(string name, string val)
        {
            Interface_Static item = Interface_Static.Static(name);
            if (item == null) return false;
            return item.SetCStringValue(val);
        }

        public static bool SetIVal(string name, int val)
        {
            Interface_Static item = Interface_Static.Static(name);
            if (item == null) return false;
            if (!item.SetIntegerValue(val)) return false;
            return true;
        }

        public static Interface_Static Static(string name)
        {
            object result = null;
            MoniTool_TypedValue.Stats().Find(name, ref result);
            return (Interface_Static)(result);
        }
        static int THE_Interface_Static_deja = 0;

        internal static void Standards()
        {
            if (THE_Interface_Static_deja!=0)
            {
                return;
            }

            THE_Interface_Static_deja = 1;

            //   read precision
            //#74 rln 10.03.99 S4135: new values and default value
            Interface_Static.Init("XSTEP", "read.precision.mode", 'e', "");
            Interface_Static.Init("XSTEP", "read.precision.mode", '&', "ematch 0");
            Interface_Static.Init("XSTEP", "read.precision.mode", '&', "eval File");
            Interface_Static.Init("XSTEP", "read.precision.mode", '&', "eval User");
            Interface_Static.SetIVal("read.precision.mode", 0);

            Interface_Static.Init("XSTEP", "read.precision.val", 'r', "1.e-03");

            Interface_Static.Init("XSTEP", "read.maxprecision.mode", 'e', "");
            Interface_Static.Init("XSTEP", "read.maxprecision.mode", '&', "ematch 0");
            Interface_Static.Init("XSTEP", "read.maxprecision.mode", '&', "eval Preferred");
            Interface_Static.Init("XSTEP", "read.maxprecision.mode", '&', "eval Forced");
            Interface_Static.SetIVal("read.maxprecision.mode", 0);

            Interface_Static.Init("XSTEP", "read.maxprecision.val", 'r', "1.");

            //   encode regularity
            //  negatif ou nul : ne rien faire. positif : on y va
            Interface_Static.Init("XSTEP", "read.encoderegularity.angle", 'r', "0.01");

            //   compute surface curves
            //  0 : par defaut. 2 : ne garder que le 2D. 3 : ne garder que le 3D
            //gka S4054
            Interface_Static.Init("XSTEP", "read.surfacecurve.mode", 'e', "");
            Interface_Static.Init("XSTEP", "read.surfacecurve.mode", '&', "ematch -3");
            Interface_Static.Init("XSTEP", "read.surfacecurve.mode", '&', "eval 3DUse_Forced");
            Interface_Static.Init("XSTEP", "read.surfacecurve.mode", '&', "eval 2DUse_Forced");
            Interface_Static.Init("XSTEP", "read.surfacecurve.mode", '&', "eval ?");
            Interface_Static.Init("XSTEP", "read.surfacecurve.mode", '&', "eval Default");
            Interface_Static.Init("XSTEP", "read.surfacecurve.mode", '&', "eval ?");
            Interface_Static.Init("XSTEP", "read.surfacecurve.mode", '&', "eval 2DUse_Preferred");
            Interface_Static.Init("XSTEP", "read.surfacecurve.mode", '&', "eval 3DUse_Preferred");
            Interface_Static.SetIVal("read.surfacecurve.mode", 0);

            //   write precision
            Interface_Static.Init("XSTEP", "write.precision.mode", 'e', "");
            Interface_Static.Init("XSTEP", "write.precision.mode", '&', "ematch -1");
            Interface_Static.Init("XSTEP", "write.precision.mode", '&', "eval Min");
            Interface_Static.Init("XSTEP", "write.precision.mode", '&', "eval Average");
            Interface_Static.Init("XSTEP", "write.precision.mode", '&', "eval Max");
            Interface_Static.Init("XSTEP", "write.precision.mode", '&', "eval User");
            Interface_Static.SetIVal("write.precision.mode", 0);

            Interface_Static.Init("XSTEP", "write.precision.val", 'r', "1.e-03");

            // Write surface curves
            // 0: write (defaut), 1: do not write, 2: write except for analytical surfaces
            Interface_Static.Init("XSTEP", "write.surfacecurve.mode", 'e', "");
            Interface_Static.Init("XSTEP", "write.surfacecurve.mode", '&', "ematch 0");
            Interface_Static.Init("XSTEP", "write.surfacecurve.mode", '&', "eval Off");
            Interface_Static.Init("XSTEP", "write.surfacecurve.mode", '&', "eval On");
            //  Interface_Static::Init("XSTEP"  ,"write.surfacecurve.mode", '&',"eval NoAnalytic");
            Interface_Static.SetIVal("write.surfacecurve.mode", 1);

            //  lastpreci : pour recuperer la derniere valeur codee (cf XSControl)
            //    (0 pour dire : pas codee)
            //:S4136  Interface_Static::Init("std"    ,"lastpreci", 'r',"0.");

            // load messages if needed
            //if (!Message_MsgFile::HasMsg("XSTEP_1"))
            //{
            //    if (!Message_MsgFile::LoadFromEnv("CSF_XSMessage", "XSTEP"))
            //    {
            //        Message_MsgFile::LoadFromString(XSMessage_XSTEP_us, sizeof(XSMessage_XSTEP_us) - 1);
            //    }
            //    if (!Message_MsgFile.HasMsg("XSTEP_1"))
            //    {
            //        throw Standard_ProgramError("Critical Error - message resources for Interface_Static are invalid or undefined!");
            //    }
            //}
        }
    }



    public enum Interface_ParamType
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


    public class RWHeaderSection
    {
        static RWHeaderSection_ReadWriteModule rwm;
        static RWHeaderSection_GeneralModule rwg;

        public static void Init()
        {
            HeaderSection_Protocol proto = HeaderSection.Protocol();
            StepData.AddHeaderProtocol(proto);
            if (rwm == null) rwm = new RWHeaderSection_ReadWriteModule();
            if (rwg == null) rwg = new RWHeaderSection_GeneralModule();
        }
    }



    //! Specific features for General Services adapted to STEP
    public class StepData_GeneralModule : Interface_GeneralModule
    {
    }


    //! This class defines general services, which must be provided
    //! for each type of Entity (i.e. of Transient Object processed
    //! by an Interface) : Shared List, Check, Copy, Delete, Category
    //!
    //! To optimise processing (e.g. firstly bind an Entity to a Module
    //! then calls  Module), each recognized Entity Type corresponds
    //! to a Case Number, determined by the Protocol each class of
    //! GeneralModule belongs to.
    public class Interface_GeneralModule
    {

    }



    //! Gives basic data definition for Step Interface.
    //! Any class of a data model described in EXPRESS Language
    //! is candidate to be managed by a Step Interface
    public class StepData
    {
        static StepData_Protocol theheader = null;//??

        public static void AddHeaderProtocol(StepData_Protocol header)
        {
            if (theheader == null) theheader = header;
            else
            {
                StepData_FileProtocol headmult = theheader as StepData_FileProtocol;
                if (headmult == null)
                {
                    headmult = new StepData_FileProtocol();
                    headmult.Add(theheader);
                }
                headmult.Add(header);
                theheader = headmult;
            }
        }
    }


    //! Defines General Services for HeaderSection Entities
    //! (Share,Check,Copy; Trace already inherited)
    //! Depends (for case numbers) of Protocol from HeaderSection
    public class RWHeaderSection_GeneralModule : StepData_GeneralModule
    {
    }


    //! A FileProtocol is defined as the addition of several already
    //! existing Protocols. It corresponds to the definition of a
    //! SchemaName with several Names, each one being attached to a
    //! specific Protocol. Thus, a File defined with a compound Schema
    //! is processed as any other one, once built the equivalent
    //! compound Protocol, a FileProtocol
    public class StepData_FileProtocol : StepData_Protocol
    {//! Adds a Protocol to the definition list of the FileProtocol
     //! But ensures that each class of Protocol is present only once
     //! in this list
        public void Add(StepData_Protocol protocol)
        {
            if (protocol == null) return;
            Type ptype = protocol.GetType();
            int nb = thecomps.Length();
            for (int i = 1; i <= nb; i++)
            {
                if (thecomps.Value(i).GetType() == (ptype)) return;
            }
            thecomps.Append(protocol);
        }

        NCollection_Sequence<object> thecomps = new NCollection_Sequence<object>();

    }


    //! General module to read and write HeaderSection entities
    public class RWHeaderSection_ReadWriteModule : StepData_ReadWriteModule
    {

    }


    //! Defines basic File Access Module (Recognize, Read, Write)
    //! That is : ReaderModule (Recognize & Read) + Write for
    //! StepWriter (for a more centralized description)
    //! Warning : A sub-class of ReadWriteModule, which belongs to a particular
    //! Protocol, must use the same definition for Case Numbers (give
    //! the same Value for a StepType defined as a String from a File
    //! as the Protocol does for the corresponding Entity)
    public class StepData_ReadWriteModule : Interface_ReaderModule
    {

    }


    //! Defines unitary operations required to read an Entity from a
    //! File (see FileReaderData, FileReaderTool), under control of
    //! a FileReaderTool. The initial creation is performed by a
    //! GeneralModule (set in GeneralLib). Then, which remains is
    //! Loading data from the FileReaderData to the Entity
    //!
    //! To work, a GeneralModule has formerly recognized the Type read
    //! from FileReaderData as a positive Case Number, then the
    //! ReaderModule reads it according to this Case Number
    public class Interface_ReaderModule
    {
    }
}
