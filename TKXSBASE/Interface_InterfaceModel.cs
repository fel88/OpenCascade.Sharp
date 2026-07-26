using OCCPort.Common;
using System;
using System.Reflection.Metadata;
using TKXSBASE;

namespace TKXSBASE
{
    //! Defines an (Indexed) Set of data corresponding to a complete
    //! Transfer by a File Interface, i.e. File Header and Transient
    //! Entities (Objects) contained in a File. Contained Entities are
    //! identified in the Model by unique and consecutive Numbers.
    //!
    //! In addition, a Model can attach to each entity, a specific
    //! Label according to the norm (e.g. Name for VDA, #ident for
    //! Step ...), intended to be output on a string or a stream
    //! (remark : labels are not obliged to be unique)
    //!
    //! InterfaceModel itself is not Transient, it is intended to
    //! work on a set of Transient Data. The services offered are
    //! basic Listing and Identification operations on Transient
    //! Entities, storage of Error Reports, Copying.
    //!
    //! Moreovere, it is possible to define and use templates. These
    //! are empty Models, from which copies can be obtained in order
    //! to be filled with effective data. This allows to record
    //! standard definitions for headers, avoiding to recreate them
    //! for each sendings, and assuring customisation of produced
    //! files for a given site.
    //! A template is attached to a name. It is possible to define a
    //! template from another one (get it, edit it then record it
    //! under another name).
    //!
    //! See also Graph, ShareTool, CheckTool for more
    public abstract class Interface_InterfaceModel
    {
        public object Value(int num)
        {
            return theentities.FindKey(num);
        }
        public void FillSemanticChecks(Interface_CheckIterator checks, bool clear)
        {
            if (checks.Model() != null)
            {
                object t1 = checks.Model();
                object t2 = this;
                if (t2 != t1) return;
            }

            if (clear)
            {
                therepch.Clear(); thechecksem.Clear();
            }

            int nb = 0;
            for (checks.Start(); checks.More(); checks.Next()) nb++;
            therepch.ReSize(therepch.Extent() + nb + 2);
            for (checks.Start(); checks.More(); checks.Next())
            {
                Interface_Check ach = checks.Value();
                int num = checks.Number();
                //    global check : ok si MEME MODELE
                if (num == 0) thechecksem.GetMessages(ach);
                else
                {
                    var ent = Value(num);
                    Interface_ReportEntity rep = new Interface_ReportEntity(ach, ent);
                    therepch.Bind(num, rep);
                }
            }
            haschecksem = true;
        }

        public bool HasSemanticChecks()
        {
            return haschecksem;
        }



        //! Returns a ReportEntity identified by its number in the Model,
        //! or a Null Handle If <num> does not identify a ReportEntity.
        //!
        //! By default, queries main report, if <semantic> is True, it
        //! queries report for semantic check
        public Interface_ReportEntity ReportEntity(int num, bool semantic = false)
        {
            Interface_ReportEntity rep = null;
            if (!IsReportEntity(num, semantic)) return rep;
            if (semantic) rep = (Interface_ReportEntity)(therepch.Find(num));
            else rep = (Interface_ReportEntity)(thereports.Find(num));
            return rep;
        }

        public bool IsReportEntity(int num, bool semantic)
        {
            return (semantic ? therepch.IsBound(num) : thereports.IsBound(num));
        }

        public bool IsErrorEntity(int num)
        {
            Interface_ReportEntity rep = ReportEntity(num);
            if (rep == null) return false;
            return rep.IsError();
        }

        public bool SetCategoryNumber(int num, int val)
        {
            int i, nb = NbEntities();
            if (num < 1 || num > nb) return false;
            if (thecategory == null) thecategory = new string(' ', nb);
            else if (thecategory.Length() < nb)
            {
                var c = new string(' ', nb).ToCharArray();
                for (i = thecategory.Length(); i > 0; i--)
                    c.SetValue(i, thecategory.Value(i));
                thecategory = new string(c);
            }
            char cval = (char)(val + 32);
            var temp = thecategory.ToCharArray();
            temp.SetValue(num, cval);
            thecategory = new string(temp);
            return true;
        }

        public void SetProtocol(Interface_Protocol proto)
        {
            thegtool = new Interface_GTool(proto);
        }
        public int NbEntities()
        {
            return theentities.Extent();
        }

        //! Erases information about labels, if any : specific to each
        //! norm
        public abstract void ClearLabels();

        //! Clears Model's header : specific to each norm
        public abstract void ClearHeader();


        TColStd_IndexedMapOfTransient theentities;
        TColStd_DataMapOfIntegerTransient thereports;
        TColStd_DataMapOfIntegerTransient therepch;
        Interface_Check thecheckstx;
        Interface_Check thechecksem;
        bool haschecksem;
        bool isdispatch;
        string thecategory;
        Interface_GTool thegtool;



        //! Clears the entities; uses the general service WhenDelete, in
        //! addition to the standard Memory Manager; can be redefined
        public virtual void ClearEntities()
        {
            thereports.Clear();
            therepch.Clear();
            haschecksem = false;

            if (thegtool != null)
            {
                // WhenDeleteCase is not applicable    
                /*    Handle(Interface_GeneralModule) module;  Standard_Integer CN;
                    Standard_Integer nb = NbEntities();
                    for (Standard_Integer i = 1; i <= nb ; i ++) {
                      Handle(Standard_Transient) anent = Value(i);
                      if (thegtool->Select (anent,module,CN))
                    module->WhenDeleteCase (CN,anent,isdispatch);
                    }*/
                thegtool.ClearEntities(); //smh#14 FRA62479
            }
            isdispatch = false;
            theentities.Clear();
        }

        public void Clear()
        {
            ClearEntities();
            thecheckstx.Clear();
            thechecksem.Clear();
            ClearHeader();
            ClearLabels();
            thecategory = null;
        }
    }


    //! A ReportEntity is produced to aknowledge and memorize the
    //! binding between a Check and an Entity. The Check can bring
    //! Fails (+ Warnings if any), or only Warnings. If it is empty,
    //! the Report Entity is for an Unknown Entity.
    //!
    //! The ReportEntity brings : the Concerned Entity, the
    //! Check, and if the Entity is empty (Fails due to Read
    //! Errors, hence the Entity could not be loaded), a Content.
    //! The Content is itself an Transient Object, but remains in a
    //! literal form : it is an "Unknown Entity". If the Concerned
    //! Entity is itself Unknown, Concerned and Content are equal.
    //!
    //! According to the Check, if it brings Fail messages,
    //! the ReportEntity is an "Error Entity", the Concerned Entity is
    //! an "Erroneous Entity". Else it is a "Correction Entity", the
    //! Concerned Entity is a "Corrected Entity". With no Check
    //! message and if Concerned and Content are equal, it reports
    //! for an "Unknown Entity".
    //!
    //! Each norm must produce its own type of Unknown Entity, but can
    //! use the class UndefinedContent to brings parameters : it is
    //! enough for most of information and avoids to redefine them,
    //! only the specific part remains to be defined for each norm.
    public class Interface_ReportEntity
    {

        Interface_Check thecheck;

        public Interface_ReportEntity(Interface_Check ach, object ent)
        {
        }

        //! Returns True for an Error Entity, i.e. if the Check
        //! brings at least one Fail message
        public bool IsError()
        {
            return (thecheck.NbFails() > 0);
        }


    }


    //! Defines an Iterator on Entities.
    //! Allows considering of various criteria
    public class Interface_EntityIterator
    {
    }
}
