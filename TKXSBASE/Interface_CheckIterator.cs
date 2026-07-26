
namespace TKXSBASE
{
    //! Result of a Check operation (especially from InterfaceModel)
    public class Interface_CheckIterator
    {

        public Interface_InterfaceModel Model()
        {
            return themod;
        }

        Interface_InterfaceModel themod;

        public void SetModel(Interface_InterfaceModel model)
        {
            themod = model;
        }

        internal object Start()
        {
            throw new NotImplementedException();
        }

        internal bool More()
        {
            throw new NotImplementedException();
        }

        internal object Next()
        {
            throw new NotImplementedException();
        }

        internal int Number()
        {
            throw new NotImplementedException();
        }

        internal Interface_Check Value()
        {
            throw new NotImplementedException();
        }
    }
}


