global using TColStd_Array1OfInteger = TKernel.NCollection_Array1<int>;

using OCCPort.Common;
using TKernel;

namespace TKXSBASE
{
    //! Specific FileReaderData for Step
    //! Contains literal description of entities (for each one : type
    //! as a string, ident, parameter list)
    //! provides references evaluation, plus access to literal data
    //! and specific access methods (Boolean, XY, XYZ)
    public class StepData_StepReaderData : Interface_FileReaderData
    {
        int thenbents;
        int thelastn;
        int thenbhead;
        int thenbscop;

        public StepData_StepReaderData(int nbhead, int nbrec, int nbpar, Resource_FormatType resource_FormatType)
        {
        }

        public void SetEntityNumbers(bool withmap)
        {
            //Message_Messenger::StreamBuffer sout = Message::SendTrace();
            //   Passe initiale : Resolution directe par Map
            //   si tout passe (pas de collision), OK. Sinon, autres passes a prevoir
            //   On resoud du meme coup les sous-listes
            int nbdirec = NbRecords();
            TColStd_Array1OfInteger subn = new(0, thelastn);


            bool pbmap = false;        // au moins un conflit
            int nbmap = 0;
            NCollection_IndexedMap<int, NCollection_DefaultHasher<int>> imap = new(thenbents);
            TColStd_Array1OfInteger indm = new(0, nbdirec);    // Index Map -> Record Number (seulement si map)

        }

        internal void AddStepParam(int nr, string val, Interface_ParamType typa)
        {
            throw new NotImplementedException();
        }

        internal void InitParams(int nr)
        {
            throw new NotImplementedException();
        }

        internal void SetRecord(int nr, out string ident, string? typrec, out int nbarg)
        {
            throw new NotImplementedException();
        }
    }
}
