namespace TKXSBASE
{
    //! Builds the Graph of Dependencies, from the General Service
    //! "Shared" -> builds for each Entity of a Model, the Shared and
    //! Sharing Lists, and gives access to them.
    //! Allows to complete with Implied References (which are not
    //! regarded as Shared Entities, but are nevertheless Referenced),
    //! this can be useful for Reference Checking
    public class Interface_ShareTool
    {
        Interface_HGraph theHGraph;

        //! Returns the Model used for Creation (directly or for Graph)
        public Interface_InterfaceModel Model()
        {
            return theHGraph.Graph().Model();
        }

    }
}


