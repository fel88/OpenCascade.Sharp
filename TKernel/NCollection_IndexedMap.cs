using OCCPort.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace TKernel
{
    public class NCollection_IndexedMap<T> : NCollection_IndexedMap<T, NCollection_DefaultHasher<T>>
    {

    }

    public class NCollection_IndexedMap<T, Hasher> : NCollection_BaseMap where Hasher : IHasher<T>, new()
    {

        public T this[int key]
        {
            get => FindKey(key);
            //set => dic[key ]=new KeyValuePair<T1, T2> () = value;
        }
        //! Contains
        public bool Contains(T theKey1)
        {
            if (IsEmpty())
                return false;

            int iK1 = hasher.HashCode(theKey1, NbBuckets());
            IndexedMapNode pNode1;
            pNode1 = (IndexedMapNode)myData1[iK1];
            while (pNode1 != null)
            {
                if (hasher.IsEqual(pNode1.Key1(), theKey1))
                    return true;
                pNode1 = (IndexedMapNode)pNode1.Next();
            }
            return false;
        }


        //! Clear data. If doReleaseMemory is false then the table of
        //! buckets is not released and will be reused.
        public void Clear(bool doReleaseMemory = true)
        {
            Destroy(null, doReleaseMemory);
        }

        public class Iterator
        {

            public Iterator(NCollection_IndexedMap<T, Hasher> theMap)
            {
                myMap = theMap;
                myIndex = 1;
            }
            NCollection_IndexedMap<T, Hasher> myMap;   // Pointer to the map being iterated

            int myIndex = 0;// Current index
            public bool More()
            {
                return (myMap != null) && (myIndex <= myMap.Extent());

            }

            //! Make a step along the collection
            public void Next()
            {
                myIndex++;
            }

            public T Value()
            {
                Exceptions.Standard_NoSuchObject_Raise_if(!More(), "NCollection_IndexedMap::Iterator::Value");
                return myMap.FindKey(myIndex);
            }
        }

        public NCollection_IndexedMap() : base(1, false)
        {

        }

        public NCollection_IndexedMap(int theNbBuckets) : base(theNbBuckets, false)
        {
        }

        //! Adaptation of the TListNode to the INDEXEDmap
        class IndexedMapNode : NCollection_TListNode<T>
        {
            public IndexedMapNode(T theKey1, int theIndex, NCollection_ListNode theNext1) : base(theKey1, theNext1)
            {
                myIndex = (theIndex);
            }

            //! Key1
            public T Key1() { return this.ChangeValue(); }

            //! Index
            public int Index() { return myIndex; }


            public int myIndex;

        }

        //! FindKey
        public T FindKey(int theIndex)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex < 1 || theIndex > Extent(), "NCollection_IndexedMap::FindKey");
            IndexedMapNode pNode2 = (IndexedMapNode)myData2[theIndex - 1];
            return pNode2.Key1();
        }

        public void Swap(int theIndex1, int theIndex2)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex1 < 1 || theIndex1 > Extent()
                                  || theIndex2 < 1 || theIndex2 > Extent(), "NCollection_IndexedMap::Swap");

            if (theIndex1 == theIndex2)
                return;

            IndexedMapNode aP1 = (IndexedMapNode)myData2[theIndex1 - 1];
            IndexedMapNode aP2 = (IndexedMapNode)myData2[theIndex2 - 1];

            (aP1.myIndex, aP2.myIndex) = (aP2.myIndex, aP1.myIndex);

            myData2[theIndex2 - 1] = aP1;
            myData2[theIndex1 - 1] = aP2;
        }

        public void RemoveLast()
        {
            int aLastIndex = Extent();
            Exceptions.Standard_OutOfRange_Raise_if(aLastIndex == 0, "NCollection_IndexedMap::RemoveLast");

            // Find the node for the last index and remove it
            IndexedMapNode p = (IndexedMapNode)myData2[aLastIndex - 1];
            myData2[aLastIndex - 1] = null;

            // remove the key
            int iK1 = hasher.HashCode(p.Key1(), NbBuckets());
            IndexedMapNode q = (IndexedMapNode)myData1[iK1];
            if (q == p)
                myData1[iK1] = (IndexedMapNode)p.Next();
            else
            {
                while (q.Next() != p)
                    q = (IndexedMapNode)q.Next();
                q.Next(p.Next());
            }
            //p->~IndexedMapNode();
            //   this->myAllocator->Free(p);
            Decrement();
        }

        //! FindIndex
        public int FindIndex(T theKey1)
        {
            if (IsEmpty()) return 0;
            IndexedMapNode pNode1 = (IndexedMapNode)myData1[hasher.HashCode(theKey1, NbBuckets())];
            while (pNode1 != null)
            {
                if (hasher.IsEqual(pNode1.Key1(), theKey1))
                {
                    return pNode1.Index();
                }
                pNode1 = (IndexedMapNode)pNode1.Next();
            }
            return 0;
        }

        Hasher hasher = new Hasher();

        //! Remove the key of the given index.
        //! Caution! The index of the last key can be changed.
        public void RemoveFromIndex(int theIndex)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex < 1 || theIndex > Extent(), "NCollection_IndexedMap::RemoveFromIndex");
            int aLastInd = Extent();
            if (theIndex != aLastInd)
            {
                Swap(theIndex, aLastInd);
            }
            RemoveLast();
        }
        //! Remove the given key.
        //! Caution! The index of the last key can be changed.
        public bool RemoveKey(T theKey1)
        {
            int anIndToRemove = FindIndex(theKey1);
            if (anIndToRemove < 1)
            {
                return false;
            }

            RemoveFromIndex(anIndToRemove);
            return true;
        }

        //! ReSize
        void ReSize(int theExtent)
        {
            NCollection_ListNode[] ppNewData1 = null;
            NCollection_ListNode[] ppNewData2 = null;
            int newBuck = 0;
            if (BeginResize(theExtent, ref newBuck, ref ppNewData1, ref ppNewData2))
            {
                if (myData1 != null)
                {
                    //memcpy(ppNewData2, myData2, sizeof(IndexedMapNode*) * Extent());
                    for (int i = 0; i < myData2.Length; i++)
                    {
                        ppNewData2[i] = myData2[i];
                    }

                    for (int aBucketIter = 0; aBucketIter <= NbBuckets(); ++aBucketIter)
                    {
                        if (myData1[aBucketIter] != null)
                        {
                            IndexedMapNode p = (IndexedMapNode)myData1[aBucketIter];
                            while (p != null)
                            {
                                int iK1 = hasher.HashCode(p.Key1(), newBuck);
                                IndexedMapNode q = (IndexedMapNode)p.Next();
                                p.Next(ppNewData1[iK1]);
                                ppNewData1[iK1] = p;
                                p = q;
                            }
                        }
                    }
                }
                EndResize(theExtent, newBuck, ppNewData1, ppNewData2);
            }
        }
        public int Add(T theKey1)
        {
            if (Resizable())
            {
                ReSize(Extent());
            }

            int iK1 = hasher.HashCode(theKey1, NbBuckets());
            IndexedMapNode pNode = (IndexedMapNode)myData1[iK1];
            while (pNode != null)
            {
                if (hasher.IsEqual(pNode.Key1(), theKey1))
                {
                    return pNode.Index();
                }
                pNode = (IndexedMapNode)pNode.Next();
            }

            int aNewIndex = Increment();
            pNode = new IndexedMapNode(theKey1, aNewIndex, myData1[iK1]);
            myData1[iK1] = pNode;
            myData2[aNewIndex - 1] = pNode;
            return aNewIndex;
        }

        public int Size()
        {
            return Extent();
        }

    }
}
