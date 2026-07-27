namespace TKXSBASE
{
    public static class TCollection_AsciiStringExtensions
    {
        public static bool IsSameString(this string str, string str2)
        {
            return str == str2;
        }
        public static bool IsEqual(this string str, string str2)
        {
            return str == str2;
        }
        public static bool IsRealValue(this string str)
        {
            return (double.TryParse(str, out var val));


        }
        public static double RealValue(this string str)
        {
            return (double.Parse(str));


        }

        public static bool IsIntegerValue(this string str)
        {
         
                return (long.TryParse(str, out var val));


                //    for (int i=0; i<mystring.Length; i++) {
                //      if (mystring[i] == '.') return false; // what about 'e','x',etc ???

                //    return true;
                //  }
                //return false;           

        }
    }

}
