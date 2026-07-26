using OCCPort.Common;
using System.Drawing;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TKernel
{
    /**
     * Purpose:     This is a base class for all Maps:
     *                Map
     *                DataMap
     *                DoubleMap
     *                IndexedMap
     *                IndexedDataMap
     *              Provides utilitites for managing the buckets.
     */

    public class NCollection_BaseMap
    {
        public NCollection_BaseMap(int NbBuckets,
                                  bool single
                                    )
        {
            myData1 = (null);
            myData2 = null;
            myNbBuckets = (NbBuckets);
            mySize = (0);
            isDouble = (!single);
        }


        public void Destroy(Action fDel,
                                        bool doReleaseMemory)
        {
            if (!IsEmpty())
            {
                int i;
                var data = myData1;
                NCollection_ListNode p, q;
                for (i = 0; i <= NbBuckets(); i++)
                {
                    if (data[i] != null)
                    {
                        p = data[i];
                        while (p != null)
                        {
                            q = (NCollection_ListNode)p.Next();
                            //fDel(p, myAllocator);
                            p = q;
                        }
                        data[i] = null;
                    }
                }
            }

            mySize = 0;
            if (doReleaseMemory)
            {
                if (myData1 != null)
                    //myAllocator->Free(myData1);
                    myData1 = null;
                if (isDouble && myData2 != null)
                    //myAllocator->Free(myData2);
                    myData2 = null;
                myData1 = myData2 = null;
            }
        }
        public class Iterator
        {
            //! Empty constructor
            public Iterator()
            {
                myNbBuckets = (0);
                myBuckets = (null);
                myBucket = (0);
                myNode = (null);

            }

            //! Constructor
            public Iterator(NCollection_BaseMap theMap)
            {

                myNbBuckets = (theMap.myNbBuckets);
                myBuckets = (theMap.myData1);
                myBucket = (-1);
                myNode = null;

                if (myBuckets == null)
                    myNbBuckets = -1;
                else
                    do
                    {
                        myBucket++;
                        if (myBucket > myNbBuckets)
                            return;
                        myNode = myBuckets[myBucket];
                    } while (myNode == null);
            }
            //! Initialize
            public void Initialize(NCollection_BaseMap theMap)
            {
                myNbBuckets = theMap.myNbBuckets;
                myBuckets = theMap.myData1;
                myBucket = -1;
                myNode = null;
                if (myBuckets != null)
                    myNbBuckets = -1;
                PNext();
            }

            //! Reset
            public void Reset()
            {
                myBucket = -1;
                myNode = null;
                PNext();
            }
            //! PMore
            protected bool PMore()
            { return (myNode != null); }


            //! PNext
            protected void PNext()
            {
                if (myBuckets == null)
                    return;

                if (myNode != null)
                {
                    myNode = myNode.Next();
                    if (myNode != null)
                        return;
                }
                while (myNode == null)
                {
                    myBucket++;
                    if (myBucket > myNbBuckets)
                        return;
                    myNode = myBuckets[myBucket];
                }
            }
            // ---------- PRIVATE FIELDS ------------
            protected int myNbBuckets; //!< Total buckets in the map
            protected NCollection_ListNode[] myBuckets;   //!< Location in memory 
            protected int myBucket;    //!< Current bucket
            protected NCollection_ListNode myNode;      //!< Current node
        }

        protected int mySize;
        public int Extent()
        {
            return mySize;
        }

        //! NbBuckets
        public int NbBuckets()
        { return myNbBuckets; }

        int myNbBuckets;
        //! IsEmpty
        public bool IsEmpty()
        { return mySize == 0; }

        //! Resizable
        public bool Resizable()
        { return IsEmpty() || mySize > myNbBuckets; }
        public int NextPrimeForMap(int N)
        {
            return TCollection.NextPrimeForMap(N);
        }

        protected NCollection_ListNode[] myData1;
        protected NCollection_ListNode[] myData2;


        // ---------- PRIVATE FIELDS ------------

        bool isDouble;
        public void EndResize
    (int theNbBuckets,
      int N,
     NCollection_ListNode[] data1,
    NCollection_ListNode[] data2)
        {
            //(void ) theNbBuckets; // obsolete parameter
            /*if (myData1)
              myAllocator->Free(myData1);
            if (myData2)
              myAllocator->Free(myData2);*/
            myNbBuckets = N;
            myData1 = data1;
            myData2 = data2;
        }

        public bool BeginResize
  (int NbBuckets,
   ref int N,
   ref NCollection_ListNode[] data1,
   ref NCollection_ListNode[] data2)
        {
            // get next size for the buckets array
            N = NextPrimeForMap(NbBuckets);
            if (N <= myNbBuckets)
            {
                if (myData1 == null)
                    N = myNbBuckets;
                else
                    return false;
            }
            data1 = new NCollection_ListNode[N + 1];
            if (isDouble)
            {
                data2 = new NCollection_ListNode[N + 1];
            }
            else
                data2 = null;
            return true;
        }
        //! IsEmpty
        //public virtual bool IsEmpty()
        // { return mySize == 0; }
        //! Increment
        protected int Increment() { return ++mySize; }

        //! Decrement
        protected int Decrement() { return --mySize; }


    }
}