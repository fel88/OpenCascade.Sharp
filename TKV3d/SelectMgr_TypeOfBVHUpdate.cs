using OCCPort;
using TKernel;
using TKMath;

namespace TKV3d
{
    public enum SelectMgr_TypeOfBVHUpdate
    {
        /*Keeps track for BVH update state for each SelectMgr_Selection entity in a following way:


	Add : 2nd level BVH does not contain any of the selection's sensitive entities and they must be added;

	Remove : all sensitive entities of the selection must be removed from 2nd level BVH;
		Renew : 2nd level BVH already contains sensitives of the selection, but the its complete update and removal is required.Therefore, sensitives of the selection with this type of update must be removed from 2nd level BVH and added after recomputation.
	Invalidate : the 2nd level BVH needs to be rebuilt;
		None : entities of the selection are up to date.

Enumerator*/
        SelectMgr_TBU_Add,
        SelectMgr_TBU_Remove,
        SelectMgr_TBU_Renew,
        SelectMgr_TBU_Invalidate,
        SelectMgr_TBU_None
    }

    internal class SeqOfVecOfSegments : NCollection_Sequence<VecOfSegments>;

    public class VecOfSegments : NCollection_Vector<SegOnIso>;
    //! Auxiliary structure defining segment of isoline.
    public class SegOnIso
    {

        public PntOnIso[] Pnts = new PntOnIso[2];

        public SegOnIso()
        {
        }

        public PntOnIso this[int i]
        {
            get => Pnts[i];
            set => Pnts[i] = value;
        }
    }
    //! Auxiliary structure defining 3D point on isoline.
    public class PntOnIso
    {
        public gp_Pnt Pnt;   //!< 3D point
        public double Param; //!< parameter along the line (for sorting)
    };
}

