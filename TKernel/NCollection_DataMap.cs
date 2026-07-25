using OCCPort.Common;
using System.Data;
using System.Reflection.Emit;

namespace TKernel
{
    /**
   * Purpose:     The DataMap is a Map to store keys with associated
   *              Items. See Map  from NCollection for  a discussion
   *              about the number of buckets.
   *
   *              The DataMap can be seen as an extended array where
   *              the Keys  are the   indices.  For this reason  the
   *              operator () is defined on DataMap to fetch an Item
   *              from a Key. So the following syntax can be used :
   *
   *              anItem = aMap(aKey);
   *              aMap(aKey) = anItem;
   *
   *              This analogy has its  limit.   aMap(aKey) = anItem
   *              can  be done only  if aKey was previously bound to
   *              an item in the map.
*/

    public class NCollection_DataMap<T1, T2> : NCollection_DataMap<T1, T2, NCollection_DefaultHasher<T1>>
    {
        public NCollection_DataMap()
        {
        }
        public NCollection_DataMap(int v, NCollection_IncAllocator myAllocator)
        {
        }
    }

    public class NCollection_DataMap<T1, T2, Hasher> : NCollection_BaseMap where Hasher : IHasher<T1>, new()
    {
        public T2 ChangeSeek(T1 theIObj)
        {
            throw new NotImplementedException();
        }
        Hasher hasher = new Hasher();


        //! Clear data. If doReleaseMemory is false then the table of
        //! buckets is not released and will be reused.
        public void Clear(bool doReleaseMemory = true)
        {
            //Destroy(DataMapNode::delNode, doReleaseMemory);
            Destroy(null, doReleaseMemory);
        }
        public T2 this[T1 key]
        {
            get => Find(key);
            //set => dic[key ]=new KeyValuePair<T1, T2> () = value;
        }
        public class Iterator : NCollection_BaseMap.Iterator
        {
            //! Constructor
            public Iterator(NCollection_DataMap<T1, T2, Hasher> theMap) : base(theMap) { }
            public Iterator(NCollection_DataMap<T1, T2> theMap) : base(theMap) { }
            //! Query if the end of collection is reached by iterator
            public bool More()
            {
                return PMore();
            }
            //! Make a step along the collection
            public void Next()
            { PNext(); }

            //! Key
            public T1 Key()
            {
                Exceptions.Standard_NoSuchObject_Raise_if(!More(), "NCollection_DataMap::Iterator::Key");
                return ((DataMapNode)myNode).Key();
            }

            //! Value inquiry
            public T2 Value()
            {
                Exceptions.Standard_NoSuchObject_Raise_if(!More(), "NCollection_DataMap::Iterator::Value");
                return ((DataMapNode)myNode).Value();
            }
        }

        // **************** Adaptation of the TListNode to the DATAmap
        public class DataMapNode : NCollection_TListNode<T2>
        {
            public DataMapNode(T1 theKey,
                  T2 theItem,
                 NCollection_ListNode theNext) : base(theItem, theNext)
            {
                myKey = (theKey);

            }
            //! Key
            public T1 Key()
            { return myKey; }

            T1 myKey;

        }

        public bool UnBind(T1 theKey)
        {
            if (IsEmpty())
                return false;
            var data = myData1;
            int k = hasher.HashCode(theKey, NbBuckets());
            DataMapNode p = (DataMapNode)data[k];
            DataMapNode q = null;
            while (p != null)
            {
                if (hasher.IsEqual(p.Key(), theKey))
                {
                    Decrement();
                    if (q != null)
                        q.Next(p.Next());
                    else
                        data[k] = (DataMapNode)p.Next();
                    //p->~DataMapNode();
                    //   this->myAllocator->Free(p);
                    return true;
                }
                q = p;
                p = (DataMapNode)p.Next();
            }
            return false;
        }

        public T2 Find(T1 theKey)
        {
            DataMapNode p = null;
            if (!lookup(theKey, ref p))
                throw new Standard_NoSuchObject("NCollection_DataMap::Find");
            return p.Value();
        }

        bool lookup(T1 theKey, ref DataMapNode thepNode)
        {
            if (IsEmpty())
                return false; // Not found
            for (thepNode = (DataMapNode)myData1[hasher.HashCode(theKey, NbBuckets())];
                thepNode != null; thepNode = (DataMapNode)thepNode.Next())
            {
                if (hasher.IsEqual(thepNode.Key(), theKey))
                    return true;
            }
            return false; // Not found
        }

        public T2 ChangeFind(T1 theKey)
        {
            DataMapNode p = null;
            if (!lookup(theKey, ref p))
                throw new Standard_NoSuchObject("NCollection_DataMap::Find");
            return p.ChangeValue();
        }

        public bool Find(T1 theKey, ref T2 theValue)
        {
            DataMapNode p = null;
            if (!lookup(theKey, ref p))
                return false;

            theValue = p.Value();
            return true;
        }

        public NCollection_DataMap() : base(1, true)
        {

        }

        public bool IsBound(T1 theKey)
        {
            DataMapNode p = null;
            return lookup(theKey, ref p);
        }


        public int Size()
        {
            return Extent();
        }
        //! ReSize
       public  void ReSize(int N)
        {
            NCollection_ListNode[] newdata = null;
            NCollection_ListNode[] dummy = null;
            int newBuck = 0;
            if (BeginResize(N, ref newBuck, ref newdata, ref dummy))
            {
                if (myData1 != null)
                {
                    var olddata = myData1;
                    DataMapNode p, q;
                    int i, k;
                    for (i = 0; i <= NbBuckets(); i++)
                    {
                        if (olddata[i] != null)
                        {
                            p = (DataMapNode)olddata[i];
                            while (p != null)
                            {
                                k = hasher.HashCode(p.Key(), newBuck);
                                q = (DataMapNode)p.Next();
                                p.Next(newdata[k]);
                                newdata[k] = p;
                                p = q;
                            }
                        }
                    }
                }
                EndResize(N, newBuck, newdata, dummy);
            }
        }
        //! Bind binds Item to Key in map.
        //! @param theKey  key to add/update
        //! @param theItem new item; overrides value previously bound to the key, if any
        //! @return Standard_True if Key was not bound already
        public bool Bind(T1 theKey, T2 theItem)
        {
            if (Resizable())
                ReSize(Extent());
            var data = myData1;
            int k = hasher.HashCode(theKey, NbBuckets());
            DataMapNode p = (DataMapNode)data[k];
            while (p != null)
            {
                if (hasher.IsEqual(p.Key(), theKey))
                {
                    p.ChangeValue(theItem);
                    return false;
                }
                p = (DataMapNode)p.Next();
            }
            data[k] = new DataMapNode(theKey, theItem, data[k]);
            Increment();
            return true;
        }
    }


}