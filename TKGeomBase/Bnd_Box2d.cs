using OCCPort.Common;
using TKMath;

namespace TKGeomBase
{
    ////! Describes a bounding box in 2D space.
    ////! A bounding box is parallel to the axes of the coordinates
    ////! system. If it is finite, it is defined by the two intervals:
    ////! -   [ Xmin,Xmax ], and
    ////! -   [ Ymin,Ymax ].
    ////! A bounding box may be infinite (i.e. open) in one or more
    ////! directions. It is said to be:
    ////! -   OpenXmin if it is infinite on the negative side of the   "X Direction";
    ////! -   OpenXmax if it is infinite on the positive side of the   "X Direction";
    ////! -   OpenYmin if it is infinite on the negative side of the   "Y Direction";
    ////! -   OpenYmax if it is infinite on the positive side of the   "Y Direction";
    ////! -   WholeSpace if it is infinite in all four directions. In
    ////! this case, any point of the space is inside the box;
    ////! -   Void if it is empty. In this case, there is no point included in the box.
    ////! A bounding box is defined by four bounds (Xmin, Xmax, Ymin and Ymax) which
    ////! limit the bounding box if it is finite, six flags (OpenXmin, OpenXmax, OpenYmin,
    ////! OpenYmax, WholeSpace and Void) which describe the bounding box if it is infinite or empty, and
    ////! -   a gap, which is included on both sides in any direction when consulting the finite bounds of the box.
    //public class Bnd_Box2d
    //{
    //    //! Sets this 2D bounding box so that it is empty. All points are outside a void box.
    //    public void SetVoid()
    //    {
    //        Flags = MaskFlags.VoidMask;
    //        Gap = 0.0;
    //    }


    //    public void Update(double X, double Y)
    //    {
    //        if (Flags.HasFlag(MaskFlags. VoidMask))
    //        {
    //            Xmin = X;
    //            Ymin = Y;
    //            Xmax = X;
    //            Ymax = Y;
    //            Flags &= ~MaskFlags.VoidMask;
    //        }
    //        else
    //        {
    //            if (!(Flags.HasFlag( MaskFlags.XminMask)) && (X < Xmin)) Xmin = X;
    //            else if (!(Flags.HasFlag(MaskFlags.XmaxMask)) && (X > Xmax)) Xmax = X;
    //            if (!(Flags.HasFlag(MaskFlags.YminMask)) && (Y < Ymin)) Ymin = Y;
    //            else if (!(Flags.HasFlag(MaskFlags.YmaxMask)) && (Y > Ymax)) Ymax = Y;
    //        }
    //    }

    //    //! Adds the 2d point.
    //    public void Add(gp_Pnt2d thePnt) { Update(thePnt.X(), thePnt.Y()); }
    //    double Xmin;
    //    double Xmax;
    //    double Ymin;
    //    double Ymax;
    //    double Gap;
    //    MaskFlags Flags;

    //    //! Bit flags.
    //    public enum MaskFlags
    //    {
    //        VoidMask = 0x01,
    //        XminMask = 0x02,
    //        XmaxMask = 0x04,
    //        YminMask = 0x08,
    //        YmaxMask = 0x10,
    //        WholeMask = 0x1e
    //    };

    //}
}

