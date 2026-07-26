using OCCPort.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Xml.Linq;

namespace TKernel
{
    public class NCollection_Array1<T>
    {
        public T[] list = null;
        //! Empty constructor; should be used with caution.
        //! @sa methods Resize() and Move().
        public NCollection_Array1()
        {
            myLowerBound = 1;
            myUpperBound = 0;

            //myDeletable(Standard_False),
            //  myData(NULL)

            //
        }
        public T First()
        {
            return this[myLowerBound];
        }
        public bool IsEmpty()
        {
            return list == null || list.Length == 0;
        }

        bool myDeletable; //!< Flag showing who allocated the array

        //! Constructor
        public NCollection_Array1(int theLower,
                     int theUpper)
        {
            myLowerBound = theLower;
            myUpperBound = theUpper;
            myDeletable = true;

            list = new T[theUpper - theLower + 1];
            //new Standard_RangeError_Raise_if(theUpper < theLower, "NCollection_Array1::Create");
            //TheItemType* pBegin = new TheItemType[Length()];
            //Standard_OutOfMemory_Raise_if(!pBegin, "NCollection_Array1 : Allocation failed");

            //myData = pBegin - theLower;
        }
        //! Copy constructor 
        public NCollection_Array1(T[] theOther, int lower, int upper)
        {
            myLowerBound = lower;
            myUpperBound = upper;
            list = theOther.ToArray();

        }

        //! C array-based constructor.
        //!
        //! Makes this array to use the buffer pointed by theBegin
        //! instead of allocating it dynamically.
        //! Argument theBegin should be a reference to the first element
        //! of the pre-allocated buffer (usually local C array buffer),
        //! with size at least theUpper - theLower + 1 items.
        //!
        //! Warning: returning array object created using this constructor
        //! from function by value will result in undefined behavior
        //! if compiler performs return value optimization (this is likely
        //! to be true for all modern compilers in release mode).
        //! The same happens if array is copied using Move() function
        //! or move constructor and target object's lifespan is longer
        //! than that of the buffer.
        public NCollection_Array1(T theBegin,
                     int theLower,
                     int theUpper)
        {
            myLowerBound = theLower;
            myUpperBound = theUpper;
            myDeletable = false;
            Exceptions.Standard_RangeError_Raise_if(theUpper < theLower, "NCollection_Array1::Create");
            list = new T[theUpper - theLower + 1];

        }

        public T this[int key]
        {
            get => list[key - Lower()];
            set => list[key - Lower()] = value;
        }

        int myLowerBound;
        int myUpperBound;
        public void Init(T val)
        {
            for (int i = myLowerBound; i <= myUpperBound; i++)
            {
                list[i - myLowerBound] = val;
            }
        }

        public int Lower()
        {
            return myLowerBound;
        }

        public void SetValue(int v, T aDrawBuffer)
        {
            this[v] = aDrawBuffer;
        }

        public int Upper()
        {
            return myUpperBound;
        }

        public T Value(int theIndex)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex < myLowerBound || theIndex > myUpperBound, "NCollection_Array1::Value");
            return list[theIndex - myLowerBound];
        }

        public int Length()
        {
            if (list == null)
                return 0;
            return list.Length;
        }
        public void Resize(int theLower, int theUpper, bool theToCopyData)
        {
            myLowerBound = theLower;
            myUpperBound = theUpper;
            var old = list;
            list = new T[theUpper - theLower + 1];

            if (old != null && theToCopyData)
            {
                int min = Math.Min(list.Length, old.Length);

                for (int i = 0; i < min; i++)
                {
                    list[i] = old[i];
                }
            }
        }

        public int Size()
        {
            if (list == null)
                return 0;
            return list.Length;
        }

        public T ChangeFirst()
        {
            return list.First();
        }

        public T ChangeValue(int theIndex)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex < myLowerBound || theIndex > myUpperBound, "NCollection_Array1::Value");
            return list[theIndex - myLowerBound];
        }

        public void ChangeValue(int theIndex, T val)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex < myLowerBound || theIndex > myUpperBound, "NCollection_Array1::Value");
            list[theIndex - myLowerBound] = val;
        }

        public class TRef : ITref<T>
        {

            public NCollection_Array1<T> Array;
            public int Index;

            public T Get()
            {
                return Array.list[Index];
            }

            public void Set(T obj)
            {
                Array.list[Index] = obj;
            }
        }

        //! Implementation of the Iterator interface.
        public class Iterator
        {
            public Iterator(NCollection_Array1<T> ar)
            {
                col = ar;
            }

            NCollection_Array1<T> col;
            int index = 0;
            //! Constant value access
            public T ChangeValue()
            {
                return Value();
            }
            public TRef TRef()
            {
                return new TRef() { Array = col, Index = index };
            }
            public void ChangeValue(T val)
            {
                col[index] = val;
            }
            public void Nullify()
            {
                col[index] = default(T);
            }
            public T Value()
            { return col[index]; }


            public void Next()
            {
                index++;
            }
            public bool More()
            {
                return index < col.Length();
            }
        }
        public class iterator
        {
            private object value;

            public iterator(object value)
            {
                this.value = value;
            }
        }
    }

    public interface ITref<T>
    {
        void Set(T obj);
        T Get();
    }
}