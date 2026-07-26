using System.Reflection.Metadata;
using TKXSBASE;

namespace TKSTEP
{
    //! defines basic controller for STEP processor
    public class STEPControl_Controller : XSControl_Controller
    {
        static bool inic = false;
        public static bool Init()
        {
            if (!inic)
            {
                STEPControl_Controller STEPCTL = new STEPControl_Controller();
                STEPCTL.AutoRecord();  // avec les noms donnes a la construction
                XSAlgo.Init();
                inic = true;
            }
            return true;
        }

        public override Interface_InterfaceModel NewModel()
        {
            return STEPEdit.NewModel();
        }

    }

}
