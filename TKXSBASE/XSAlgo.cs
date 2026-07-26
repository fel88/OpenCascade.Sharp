using TKShHealing;

namespace TKXSBASE
{
    public class XSAlgo
    {
        static XSAlgo_AlgoContainer theContainer;
        static bool init = false;

        //! Provides initerface to the algorithms from Shape Healing
        //! and others for XSTEP processors.
        //! Creates and initializes default AlgoContainer.
        public static void Init()
        {
            if (init) return;
            init = true;
            ShapeAlgo.Init();
            theContainer = new XSAlgo_AlgoContainer();

            // init parameters
            Interface_Static.Standards();

            //#74 rln 10.03.99 S4135: adding new parameter for handling use of BRepLib::SameParameter
            Interface_Static.Init("XSTEP", "read.stdsameparameter.mode", 'e', "");
            Interface_Static.Init("XSTEP", "read.stdsameparameter.mode", '&', "ematch 0");
            Interface_Static.Init("XSTEP", "read.stdsameparameter.mode", '&', "eval Off");
            Interface_Static.Init("XSTEP", "read.stdsameparameter.mode", '&', "eval On");
            //Interface_Static.SetIVal("read.stdsameparameter.mode", 0);

            // unit: supposed to be cascade unit (target unit for reading)
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", 'e', "");
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "enum 1");
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval INCH");  // 1
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval MM");    // 2
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval ??");    // 3
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval FT");    // 4
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval MI");    // 5
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval M");     // 6
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval KM");    // 7
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval MIL");   // 8
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval UM");    // 9
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval CM");    //10
            Interface_Static.Init("XSTEP", "xstep.cascade.unit", '&', "eval UIN");   //11
            //Interface_Static.SetCVal("xstep.cascade.unit", "MM");

            // init Standard Shape Processing operators
            ShapeProcess_OperLibrary.Init();
        }

    }


    public class XSAlgo_AlgoContainer
    {

    }
}