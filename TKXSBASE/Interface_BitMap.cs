using OCCPort.Common;
using TKernel;

namespace TKXSBASE
{
    //! A bit map simply allows to associate a boolean flag to each
    //! item of a list, such as a list of entities, etc... numbered
    //! between 1 and a positive count nbitems
    //!
    //! The BitMap class allows to associate several binary flags,
    //! each of one is identified by a number from 0 to a count
    //! which can remain at zero or be positive : nbflags
    //!
    //! Flags lists over than numflag=0 are added after creation
    //! Each of one can be named, hence the user can identify it
    //! either by its flag number or by a name which gives a flag n0
    //! (flag n0 0 has no name)
    public class Interface_BitMap
    {
        int thenbitems;
        int thenbwords;
        int thenbflags;
        TColStd_HArray1OfInteger theflags;

        public void SetTrue(int item, int flag)
        {
            int numw = (thenbwords * flag) + (item >> 5);
            int numb = item & 31;
            theflags.ChangeValue(numw, theflags.ChangeValue(numw) | (1 << numb));
        }
    }

    public class TColStd_HArray1OfInteger : NCollection_Array1<int>
    {
    }
}


