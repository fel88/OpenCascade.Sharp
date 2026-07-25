namespace TKXSBASE
{
    //! This class allows to store a redefinable Graph, via a Handle
    //! (useful for an Object which can work on several successive
    //! Models, with the same general conditions)
    public class Interface_HGraph
    {
        public Interface_Graph Graph()
        { return thegraph; }
        Interface_Graph thegraph;

    }
}