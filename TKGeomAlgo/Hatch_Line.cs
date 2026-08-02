using OCCPort.Common;
using System.Linq;
using TKernel;
using TKMath;

namespace TKGeomAlgo
{
    //! Stores a Line in the Hatcher. Represented by :
    //!
    //! * A Lin2d from gp, the geometry of the line.
    //!
    //! * Bounding parameters for the line.
    //!
    //! * A sorted List  of Parameters, the  intersections
    //! on the line.
    public class Hatch_Line
    {
        public Hatch_Line(gp_Lin2d L,
                   Hatch_LineForm T)
        {
            myLin = (L);
            myForm = (T);
        }

        //! Insert a new intersection in the sorted list.
        public void AddIntersection(double Par1, bool Start, int Index, double Par2, double theToler)
        {
            Hatch_Parameter P = new(Par1, Start, Index, Par2);
            int i;
            for (i = 1; i <= myInters.Length(); i++)
            {
                double dfIntPar1 = myInters[(i)].myPar1;
                // akm OCC109 vvv : Two intersections too close
                if (Math.Abs(Par1 - dfIntPar1) < theToler)
                {
                    myInters.Remove(i);
                    return;
                }
                // akm OCC109 ^^^
                if (Par1 < dfIntPar1)
                {
                    myInters.InsertBefore(i, P);
                    return;
                }
            }
            myInters.Append(P);
        }

        public gp_Lin2d myLin;
        public Hatch_LineForm myForm;
        public NCollection_Sequence<Hatch_Parameter> myInters = new NCollection_Sequence<Hatch_Parameter>();


    }
}
