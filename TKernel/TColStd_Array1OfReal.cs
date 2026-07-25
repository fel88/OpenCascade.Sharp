namespace TKernel
{
    public class TColStd_Array1OfReal : NCollection_Array1<double>
    {
        public TColStd_Array1OfReal(int v1, int v2) : base(v1, v2)
        {
        }

        public TColStd_Array1OfReal(double theBegin, int theLower, int theUpper) : base(theBegin, theLower, theUpper)
        {
        }
    }
}