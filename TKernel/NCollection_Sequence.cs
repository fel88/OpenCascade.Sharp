using OCCPort.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Security.Principal;
using System.Xml.Linq;

namespace TKernel
{
    /**
 * Purpose:     Definition of a sequence of elements indexed by
 *              an Integer in range of 1..n
 */
    public class NCollection_Sequence<T> : List<T>
    {
        public NCollection_Sequence<T> ChangeSequence()
        {
            return this;
        }
        public int Length()
        {
            return Count;
        }
        public bool IsEmpty()
        {
            return Count == 0;
        }
        public T ChangeValue(int i)
        {
            return this[i];
        }
        public void Remove(Iterator aPrsIter)
        {
            Remove(aPrsIter.Value());
        }

        //! Set item value by theIndex
        public void SetValue(int theIndex,
                      T theItem)
        { ChangeValue(theIndex, theItem); }


        //! Split in two sequences
        public void Split(int theIndex, NCollection_Sequence<T> theSeq)
        {
            //todo check this!!
            for (int i = theIndex + 1; i < Count; i++)
            {
                theSeq.Add(this[i]);
            }
            this.RemoveRange(theIndex, this.Count - theIndex);

        }


        //! Constant item access by theIndex
        public T Value(int theIndex)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex <= 0 || theIndex > Count, "NCollection_Sequence::Value");
            return this[theIndex];
            //NCollection_Sequence * const aLocalTHIS = (NCollection_Sequence*)this;
            //aLocalTHIS->myCurrentItem = Find(theIndex);
            //aLocalTHIS->myCurrentIndex = theIndex;
            //return ((const Node*) myCurrentItem) -> Value();
        }

        public void ChangeValue(int i, T thePnt)
        {
            this[i] = thePnt;
        }

        public int Size()
        {
            return Count;
        }

        //! InsertBefore theIndex theItem
        public void InsertBefore(int theIndex,
                     T theItem)
        {
            //InsertAfter(theIndex - 1, theItem); 
            Insert(theIndex - 1, theItem);
        }

        //! InsertBefore theIndex another sequence (making it empty)
        public void InsertBefore(int theIndex,
                           NCollection_Sequence<T> theSeq)
        {

            //    InsertAfter(theIndex - 1, theSeq);
            InsertRange(theIndex - 1, theSeq);
        }



        public void Append(T t)
        {
            Add(t);
        }

        public new T this[int key]
        {
            get => base[key - Lower()];
            set => base[key - Lower()] = value;
        }

        //! Method for consistency with other collections.
        //! @return Lower bound (inclusive) for iteration.
        public int Lower()
        {
            return 1;
        }
        public int Upper()
        {
            return Count;
        }
        //! Method for consisten
        internal T First()
        {
            return this[Lower()];
        }


        public void Remove(int v)
        {
            RemoveAt(v - 1);
        }

        public class Iterator
        {
            //! Initialisation
            public void Init(NCollection_Sequence<T> theSeq,
                   bool isStart = true)
            {
                throw new NotImplementedException();
                //myCurrent = (isStart ? theSeq.myFirstItem : NULL);
                //  myPrevious = (isStart ? NULL : theSeq.myLastItem);
            }

            NCollection_Sequence<T> target;
            public Iterator(NCollection_Sequence<T> list)
            {
                target = list;
                index = list.Lower();
            }
            int index = -1;
            public bool More()
            {
                return index <= target.Upper();
            }

            public void Next()
            {
                index++;
            }

            public T Value()
            {
                return target[index];
            }
            public T ChangeValue()
            {
                return target[index];
            }
        }
    }
}