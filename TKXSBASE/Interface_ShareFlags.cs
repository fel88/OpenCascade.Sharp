namespace TKXSBASE
{
    //! This class only says for each Entity of a Model, if it is
    //! Shared or not by one or more other(s) of this Model
    //! It uses the General Service "Shared".
    public class Interface_ShareFlags
    {
        TColStd_HSequenceOfTransient theroots;

        public Interface_ShareFlags(Interface_Graph interface_Graph)
        {
        }

        public object Root(int num)
        { return theroots.Value(num); }

        public int NbRoots()
        { return (theroots == null ? 0 : theroots.Length()); }
    }
}