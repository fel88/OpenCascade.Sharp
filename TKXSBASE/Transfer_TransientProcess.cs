using OCCPort.Common;
using System.Reflection.Metadata;

namespace TKXSBASE
{
    //! Adds specific features to the generic definition :
    //! TransientProcess is intended to work from an InterfaceModel
    //! to a set of application objects.
    //!
    //! Hence, some information about starting entities can be gotten
    //! from the model : for Trace, CheckList, Integrity Status
    public class Transfer_TransientProcess : Transfer_ProcessForTransient
    {
        //! Sets a Graph : superseedes SetModel if already done
        public void SetGraph(Interface_HGraph HG)
        {
            thegraph = HG;
            if (thegraph != null)
                SetModel(thegraph.Graph().Model());
            else
                themodel = null;
        }

        public void SetModel(Interface_InterfaceModel model)
        {
            themodel = model;
        }

        Interface_HGraph thegraph;
        Interface_InterfaceModel themodel;

        internal void Clear()
        {
            throw new NotImplementedException();
        }
    }

}