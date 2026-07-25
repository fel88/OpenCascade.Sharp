using OCCPort.Common;
using OpenTK.Mathematics;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace TKernel
{
    public class NCollection_IndexedDataMap<TheKeyType, T2> : NCollection_IndexedDataMap<TheKeyType, T2, NCollection_DefaultHasher<TheKeyType>>
    {

    }

    public class NCollection_IndexedDataMap<TheKeyType, T2, Hasher> : NCollection_BaseMap where Hasher : IHasher<TheKeyType>, new()
    {
        public void ChangeFromIndex(int theIndex, T2 v)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex < 1 || theIndex > Extent(), "NCollection_IndexedDataMap::ChangeFromIndex");
            ((IndexedDataMapNode<TheKeyType, T2>)myData2[theIndex - 1]).ChangeValue( v);

        }
        public class Iterator
        {
            public Iterator(NCollection_IndexedDataMap<TheKeyType, T2, Hasher> theMap)
            {
                myMap = theMap;
                myIndex = 1;
            }
            NCollection_IndexedDataMap<TheKeyType, T2, Hasher> myMap;   //!< Pointer to current node
            int myIndex; //!< Current index

            //! Query if the end of collection is reached by iterator
            public bool More()
            {
                return myMap != null && myIndex <= myMap.Extent();
            }

            public TheKeyType Key()
            {
                Exceptions.Standard_NoSuchObject_Raise_if(!More(), "NCollection_IndexedDataMap::Iterator::Key");
                return myMap.FindKey(myIndex);
            }

            //! Make a step along the collection
            public void Next()
            {
                ++myIndex;
            }
            //! Value access
            public T2 Value()
            {
                Exceptions.Standard_NoSuchObject_Raise_if(!More(), "NCollection_IndexedDataMap::Iterator::Value");
                return myMap.FindFromIndex(myIndex);
            }



            public void ChangeValue(T2 v)
            {
                Exceptions.Standard_NoSuchObject_Raise_if(!More(), "NCollection_IndexedDataMap::Iterator::Value");
                ((IndexedDataMapNode<TheKeyType, T2>)myMap.myData2[myIndex - 1]).ChangeValue( v);
            }
        }

        public NCollection_IndexedDataMap() : base(1, false)
        {
            // myData2 = null;
            //myData1 = null;
        }

        public void Clear()
        {
            myData2 = null;
            myData1 = null;
        }

        public T2 ChangeFromKey(TheKeyType theKey1)
        {
            Exceptions.Standard_NoSuchObject_Raise_if(IsEmpty(), "NCollection_IndexedDataMap::ChangeFromKey");
            //if (IsEmpty())
            //return default;

            var pNode1 = (IndexedDataMapNode<TheKeyType, T2>)(myData1[hasher.HashCode(theKey1, NbBuckets())]);
            while (pNode1 != null)
            {
                if (hasher.IsEqual(pNode1.Key1(), theKey1))
                {
                    return pNode1.Value();//todo replace with new signature of changevalue or TRef
                }
                pNode1 = (IndexedDataMapNode<TheKeyType, T2>)pNode1.Next();
            }
            throw new Standard_NoSuchObject("NCollection_IndexedDataMap::ChangeFromKey");
            //foreach (var item in myData2)
            //{
            //    if (hasher.Equals(item.Key, theKey1))
            //        return item.Value;
            //}
            //return default;
        }



        //! Contains
        public bool Contains(TheKeyType theKey1)
        {
            if (IsEmpty())
                return false;

            //foreach (var item in dic)
            //{
            //    if (hasher.Equals(item.Key, theKey1))
            //        return true;
            //}
            //return false;

            int iK1 = hasher.HashCode(theKey1, NbBuckets());
            IndexedDataMapNode<TheKeyType, T2> pNode1 = null;
            pNode1 = (IndexedDataMapNode<TheKeyType, T2>)myData1[iK1];
            while (pNode1 != null)
            {
                if (hasher.IsEqual(pNode1.Key1(), theKey1))
                    return true;
                pNode1 = (IndexedDataMapNode<TheKeyType, T2>)pNode1.Next();
            }
            return false;
        }



        Hasher hasher = new Hasher();
        public T2 this[int key]
        {
            get => FindFromIndex(key);
            //set => dic[key ]=new KeyValuePair<T1, T2> () = value;
        }


        //! FindFromIndex
        public T2 FindFromIndex(int theIndex)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex < 1 || theIndex > Extent(), "NCollection_IndexedDataMap::FindFromIndex");
            return ((IndexedDataMapNode<TheKeyType, T2>)myData2[theIndex - 1]).Value();
            //IndexedDataMapNode* aNode = (IndexedDataMapNode*)myData2[theIndex - 1];
            //return aNode->Value();
        }

        public TheKeyType FindKey(int theIndex)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex < 1 || theIndex > Extent(), "NCollection_IndexedDataMap::FindKey");
            return ((IndexedDataMapNode<TheKeyType, T2>)myData2[theIndex - 1]).Key1();
        }

        //! ReSize
      public   void ReSize(int N)
        {
            NCollection_ListNode[] ppNewData1 = null;
            NCollection_ListNode[] ppNewData2 = null;
            int newBuck = 0;//??
            if (BeginResize(N, ref newBuck, ref ppNewData1, ref ppNewData2))
            {
                if (myData1 != null)
                {
                    //memcpy(ppNewData2, myData2, sizeof(IndexedDataMapNode*) * Extent());
                    for (int i = 0; i < myData2.Length; i++)
                    {
                        ppNewData2[i] = myData2[i];
                    }
                    
                    for (int aBucketIter = 0; aBucketIter <= NbBuckets(); ++aBucketIter)
                    {
                        if (myData1[aBucketIter] != null)
                        {
                            NCollection_ListNode p = myData1[aBucketIter];
                            while (p != null)
                            {
                                int iK1 = hasher.HashCode(((IndexedDataMapNode<TheKeyType, T2>)p).Key1(), newBuck);
                                var q = p.Next();
                                p.Next( ppNewData1[iK1]);
                                ppNewData1[iK1] = p;
                                p = q;
                            }
                        }
                    }
                }
                EndResize(N, newBuck, ppNewData1, ppNewData2);
            }
        }

        //! Returns the Index of already bound Key or appends new Key with specified Item value.
        //! @param theKey1 Key to search (and to bind, if it was not bound already)
        //! @param theItem Item value to set for newly bound Key; ignored if Key was already bound
        //! @return index of Key
        public int Add(TheKeyType theKey1, T2 theItem)
        {
            //for (int i = 0; i < myData2.Count; i++)
            //{
            //    if (hasher.Equals(myData2[i].Key, theKey1))
            //        //if (dic[i].Key.Equals(theKey1))
            //        return i + 1;
            //}
            //myData2.Add(new IndexedDataMapNode<TheKeyType, T2>(theKey1, Increment(), theItem));
            //return myData2.Count;
            if (Resizable())
            {
                ReSize(Extent());
            }

            int iK1 = hasher.HashCode(theKey1, NbBuckets());
            NCollection_ListNode pNode = myData1[iK1];
            while (pNode != null)
            {
                var ipNode = (IndexedDataMapNode<TheKeyType, T2>)pNode;
                if (hasher.IsEqual(ipNode.Key1(), theKey1))
                {
                    return ipNode.Index();
                }
                pNode = pNode.Next();
            }

            int aNewIndex = Increment();
            //pNode = new(this->myAllocator) IndexedDataMapNode(theKey1, aNewIndex, theItem, myData1[iK1]);
            pNode = new IndexedDataMapNode<TheKeyType, T2>(theKey1, aNewIndex, theItem, myData1[iK1]);
            myData1[iK1] = pNode;
            myData2[aNewIndex - 1] = pNode;
            return aNewIndex;
        }

        //! Substitute
        public void Substitute(int theIndex,
                     TheKeyType theKey1,
                     T2 theItem)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex < 1 || theIndex > Extent(),
                                          "NCollection_IndexedDataMap::Substitute : " +
                                          "Index is out of range");

            //myData2[theIndex - 1] = new IndexedDataMapNode<TheKeyType, T2>(theKey1, Increment(), theItem);
            // check if theKey1 is not already in the map
            int iK1 = hasher.HashCode(theKey1, NbBuckets());
            IndexedDataMapNode<TheKeyType, T2> p = ((IndexedDataMapNode<TheKeyType, T2>)myData1[iK1]);
            while (p != null)
            {
                if (hasher.IsEqual(p.Key1(), theKey1))
                {
                    if (p.Index() != theIndex)
                    {
                        throw new Standard_DomainError("NCollection_IndexedDataMap::Substitute : " +
                                                    "Attempt to substitute existing key");
                    }
                    p.myKey1 = theKey1;
                    p.ChangeValue( theItem);
                    return;
                }
                p = (IndexedDataMapNode<TheKeyType, T2>)p.Next();
            }

            // Find the node for the index I
            p = (IndexedDataMapNode<TheKeyType, T2>)myData2[theIndex - 1];

            // remove the old key
            int iK = hasher.HashCode(p.Key1(), NbBuckets());
            var q = myData1[iK];
            if (q == p)
                myData1[iK] = p.Next();
            else
            {
                while (q.Next() != p)
                    q = q.Next();
                q.Next( p.Next());
            }

            // update the node
            p.myKey1 = theKey1;
            p.ChangeValue(theItem);
            p.Next( myData1[iK1]);
            myData1[iK1] = p;
        }

        //! FindIndex
        public int FindIndex(TheKeyType theKey1)
        {
            if (IsEmpty())
                return 0;

            var pNode1 = (IndexedDataMapNode<TheKeyType, T2>)myData1[hasher.HashCode(theKey1, NbBuckets())];
            while (pNode1 != null)
            {
                if (hasher.IsEqual(pNode1.Key1(), theKey1))
                {
                    return pNode1.Index();
                }
                pNode1 = (IndexedDataMapNode<TheKeyType, T2>)pNode1.Next();
            }
            return 0;

        }


    }
}