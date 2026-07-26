using OCCPort.Common;
using System.Reflection.Metadata;

namespace TKXSBASE
{
    //! Defines a Check, as a list of Fail or Warning Messages under
    //! a literal form, which can be empty. A Check can also bring an
    //! Entity, which is the Entity to which the messages apply
    //! (this Entity may be any Transient Object).
    //!
    //! Messages can be stored in two forms : the definitive form
    //! (the only one by default), and another form, the original
    //! form, which can be different if it contains values to be
    //! inserted (integers, reals, strings)
    //! The original form can be more suitable for some operations
    //! such as counting messages
    public class Interface_Check
    {//! Clears a check, in order to receive information from transfer
     //! (Messages and Entity)
        public void Clear()
        {
            thefails = null; thefailo = null;
            thewarns = null; thewarno = null;
            theinfos = null; theinfoo = null;
            theent = null;
        }
        public void SetEntity(object anentity)
        {
            theent = anentity;
        }

        public void AddFail(string amess,
                                  string orig)
        {
            if (amess[0] == '\0') return;
            if (orig == null || orig[0] == '\0') AddFail(new string(amess));
            else AddFail(new string(amess),
                  new string(orig));
        }

        public void AddFail(string mess)
        {
            if (thefails == null) thefails = "";
            if (thefailo == null) thefailo = "";
            thefails += (mess); thefailo += (mess);
        }

        public bool HasFailed()
        {
            return (thefails != null);
        }

        public bool HasWarnings()
        {
            return (thewarns != null);
        }

        public int NbFails()
        {
            return (thefails == null ? 0 : thefails.Length());
        }

        internal void GetMessages(Interface_Check ach)
        {
            throw new NotImplementedException();
        }

        string thefails;
        string thefailo;
        string thewarns;
        string thewarno;
        string theinfos;
        string theinfoo;
        object theent;
    }
}
