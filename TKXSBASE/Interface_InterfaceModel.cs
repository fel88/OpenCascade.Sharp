using System.Reflection.Metadata;

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
        public void SetProtocol(Interface_Protocol proto)
        {
            thegtool = new Interface_GTool(proto);
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
}
