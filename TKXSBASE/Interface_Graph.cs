using OCCPort.Common;
using System;

namespace TKXSBASE
{
    //! Gives basic data structure for operating and storing
    //! graph results (usage is normally internal)
    //! Entities are Mapped according their Number in the Model
    //!
    //! Each Entity from the Model can be known as "Present" or
    //! not; if it is, it is Mapped with a Status : an Integer
    //! which can be used according to needs of each algorithm
    //! In addition, the Graph brings a BitMap which can be used
    //! by any caller
    //!
    //! Also, it is bound with two lists : a list of Shared
    //! Entities (in fact, their Numbers in the Model) which is
    //! filled by a ShareTool, and a list of Sharing Entities,
    //! computed by deduction from the Shared Lists
    //!
    //! Moreover, it is possible to redefine the list of Entities
    //! Shared by an Entity (instead of standard answer by general
    //! service Shareds) : this new list can be empty; it can
    //! be changed or reset (i.e. to come back to standard answer)
    public class Interface_Graph
    {

        public void SetStatus(int num, int stat)
        {
            if (thestats != null)
                thestats.SetValue(num, stat);
        }

        TColStd_HArray1OfInteger thestats;

        //! Returns the Model with which this Graph was created
        public Interface_InterfaceModel Model()
        {
            return themodel;
        }
        Interface_InterfaceModel themodel;


    }
}