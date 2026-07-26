using TKernel;

namespace TKSTEPBase
{
    public class StepRepr_Representation
    {
        NCollection_Array1<StepRepr_RepresentationItem> items;

        public int NbItems()
        {
            if (items == null) return 0;
            return items.Length();
        }

        public StepRepr_RepresentationItem ItemsValue(int num)
        {
            return items.Value(num);
        }
    }

}
