namespace TKXSBASE
{
    public class StepFile_ReadData
    {
        Argument myCurrArg;           //!< Current node of the arguments list
        Record myFirstRec;            //!< First node of the records list
        Record myCurRec;              //!< Current node of the records list
        Record myLastRec;             //!< Last node of the records list
        //Scope myCurScope;             //!< Current node of the scopes list
        public StepFile_ReadData()
        {
            /*:myMaxChar(50000), myMaxRec(5000), myMaxArg(10000), myModePrint(0),
  myNbRec(0), myNbHead(0), myNbPar(0), myYaRec(0),
  myNumSub(0), myErrorArg(Standard_False), myResText(NULL), myCurrType(TextValue::SubList),
  mySubArg(NULL), myTypeArg(Interface_ParamSub), myCurrArg(NULL), myFirstRec(NULL),*/
            myCurRec = (null);
            myLastRec = (null);
            //myCurScope=(null);
            //myFirstError=(null);
            // myCurError=(null);

        }
        class Argument
        {
            public Argument()
            {
                myNext = (null);
                myValue = (null);
                myType = Interface_ParamType.Interface_ParamSub;
            }

            Argument myNext;    //!< Next argument in the list for this record
            string myValue;      //!< Character value of the argument
            Interface_ParamType myType; //!< Type of the argument
        }

        public class Record
        {

            public Record()
            {
                myNext = (null);
                myFirst = (null);
                myIdent = (null);
                myType = (null);
            }

            public Record myNext;    //!< Next record in the list
            Argument myFirst; //!< First argument in the record
           public  string myIdent;     //!< Record identifier (Example: "#12345") or scope-end
            string myType;      //!< Type of the record
        }

        int myMaxChar;    //!< Maximum number of characters in a characters page
        int myMaxRec;     //!< Maximum number of records in a records page
        int myMaxArg;     //!< Maximum number of arguments in a arguments page
        int myModePrint;  //!< Control print output (for call from yacc)
        int myNbRec;      //!< Total number of data records read
        int myNbHead;     //!< Number of records taken by the Header
        int myNbPar;      //!< Total number of parameters read
        int myYaRec;      //!< Presence record already created (after 1 Ident)
        int myNumSub;     //!< Number of current sublist
        bool myErrorArg;   //!< Control of error argument (true - error argument was created)
        public void GetFileNbR(out int theNbHead, out int theNbRec, out int theNbPage)
        {
            myCurRec = myFirstRec;
            theNbHead = myNbHead;
            theNbRec = myNbRec;
            theNbPage = myNbPar;
        }

        public void RecordNewEntity()
        {
            myErrorArg = false; // Reset error argument mod
            AddNewRecord(myCurRec);
            SetTypeArg(Interface_ParamType.Interface_ParamSub);
            mySubArg = myCurRec.myIdent;
            myCurRec = myCurRec.myNext;
            myLastRec.myNext = null;
        }

        string mySubArg;                //!< Ident last record (possible sub-list)

        public void SetTypeArg(Interface_ParamType theArgType)
        {
            myTypeArg = theArgType;
        }
        Interface_ParamType myTypeArg; //!< Type of last argument read

        public void AddNewRecord(Record theNewRecord)
        {
            myNbRec++;
            if (myFirstRec == null) myFirstRec = theNewRecord;
            if (myLastRec != null) myLastRec.myNext = theNewRecord;
            myLastRec = theNewRecord;
        }

        internal void NextRecord()
        {
            throw new NotImplementedException();
        }

        internal object GetLastError()
        {
            throw new NotImplementedException();
        }

        internal int GetArgDescription(out Interface_ParamType typa, out string val)
        {
            throw new NotImplementedException();
        }
    }
}