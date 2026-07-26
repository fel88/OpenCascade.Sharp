namespace TKShHealing
{
    public class ShapeAlgo
    {
        static bool init = false;
        static ShapeAlgo_AlgoContainer theContainer;

        public static void Init()
        {
            if (init) return;
            init = true;
            theContainer = new ShapeAlgo_AlgoContainer();

            // initialization of Standard Shape Healing
            ShapeExtend.Init();
        }


    }
}

