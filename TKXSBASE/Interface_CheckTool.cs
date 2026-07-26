using OCCPort.Common;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using TKXSBASE;

namespace TKXSBASE
{
    //! Performs Checks on Entities, using General Service Library and
    //! Modules to work. Works on one Entity or on a complete Model
    public class Interface_CheckTool
    {
        int thestat;
        Interface_ShareTool theshare;
        Interface_GTool thegtool;
        static int errh = 1;

        public Interface_CheckTool(Interface_HGraph thegraph)
        {
        }

        static void raisecheck(Standard_Failure theException, Interface_Check ach)
        {
            //char mess[100];
            var mess = "** Exception Raised during Check : " + theException.GetType().Name;
            ach.AddFail(mess);
            /*# ifdef _WIN32
                        if (theException.IsKind(STANDARD_TYPE(OSD_Exception)))
                        {
            #else
                            if (theException.IsKind(STANDARD_TYPE(OSD_Signal)))
                            {
            #endif
                                theException.SetMessageString("System Signal received, check interrupt");
                                throw theException;
                            }*/
        }

        public void FillCheck(object ent,
                                       Interface_ShareTool sh,
                                      Interface_Check ach)
        {
           // Handle(Interface_GeneralModule) module;
          //  int CN;
         //   if (thegtool->Select(ent, module, CN))
            {
                //    Sans try/catch (fait par l appelant, evite try/catch en boucle)
              //  if (!errh)
                {
                  //  module->CheckCase(CN, ent, sh, ach);
                    return;
                }
                //    Avec try/catch
                try
                {
                    //OCC_CATCH_SIGNALS
                 //   module->CheckCase(CN, ent, sh, ach);
                }
                catch (Standard_Failure anException)
                {
                    raisecheck(anException, ach);
                }
            }
          //  else
            {
             //  DeclareAndCast(Interface_ReportEntity, rep, ent);
             //   if (rep.IsNull()) return;
            //    ach = rep->Check();
            }
           // if (theshare.Graph().HasShareErrors(ent))
              //  ach->AddFail("** Shared Items unknown from the containing Model");
        }
        public Interface_CheckIterator VerifyCheckList()
        {
            thestat = 1;
            Interface_InterfaceModel model = theshare.Model();
            Interface_CheckIterator res = new Interface_CheckIterator();
            res.SetModel(model);
            int i = 0, n0 = 1, nb = model.NbEntities();

            errh = 0;
            while (n0 <= nb)
            {
                object ent;
                Interface_Check ach = new Interface_Check();
                try
                {
                    //    OCC_CATCH_SIGNALS
                    for (i = n0; i <= nb; i++)
                    {
                        if (model.IsErrorEntity(i)) continue;
                        ent = model.Value(i);
                        ach.Clear();
                        ach.SetEntity(ent);
                        if (!model.HasSemanticChecks()) FillCheck(ent, theshare, ach);
                       // else ach = model.Check(i, false);
                       // if (ach.HasFailed() || ach.HasWarnings())
                       // { thestat |= 4; res.Add(ach, i); }
                    }
                    n0 = nb + 1;
                }
                catch (Standard_Failure anException)
                {
                    n0 = i + 1;
                    raisecheck(anException, ach);
                  //  res.Add(ach, i); thestat |= 4;
                }
            }
            return res;
        }

    }
}


