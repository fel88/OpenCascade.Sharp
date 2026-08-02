using OCCPort.Common;

namespace TKMath
{
    //! Describes a bounding box in 2D space.
    //! A bounding box is parallel to the axes of the coordinates
    //! system. If it is finite, it is defined by the two intervals:
    //! -   [ Xmin,Xmax ], and
    //! -   [ Ymin,Ymax ].
    //! A bounding box may be infinite (i.e. open) in one or more
    //! directions. It is said to be:
    //! -   OpenXmin if it is infinite on the negative side of the   "X Direction";
    //! -   OpenXmax if it is infinite on the positive side of the   "X Direction";
    //! -   OpenYmin if it is infinite on the negative side of the   "Y Direction";
    //! -   OpenYmax if it is infinite on the positive side of the   "Y Direction";
    //! -   WholeSpace if it is infinite in all four directions. In
    //! this case, any point of the space is inside the box;
    //! -   Void if it is empty. In this case, there is no point included in the box.
    //! A bounding box is defined by four bounds (Xmin, Xmax, Ymin and Ymax) which
    //! limit the bounding box if it is finite, six flags (OpenXmin, OpenXmax, OpenYmin,
    //! OpenYmax, WholeSpace and Void) which describe the bounding box if it is infinite or empty, and
    //! -   a gap, which is included on both sides in any direction when consulting the finite bounds of the box.
    public class Bnd_Box2d
    {
        //! Sets this 2D bounding box so that it is empty. All points are outside a void box.
        public void SetVoid()
        {
            Flags = MaskFlags.VoidMask;
            Gap = 0.0;
        }
        double Xmin;
        double Xmax;
        double Ymin;
        double Ymax;
        double Gap;

        //! Sets this bounding box so that it covers the whole 2D
        //! space, i.e. it is infinite in all directions.
        void SetWhole() { Flags = MaskFlags.WholeMask; }

        //! Returns true if this bounding box is open in the Xmin direction.
        bool IsOpenXmin() { return (Flags.HasFlag(MaskFlags.XminMask)); }

        //! Returns true if this bounding box is open in the Xmax direction.
        bool IsOpenXmax() { return (Flags.HasFlag(MaskFlags.XmaxMask)); }

        //! Returns true if this bounding box is open in the Ymin direction.
        bool IsOpenYmin() { return (Flags.HasFlag(MaskFlags.YminMask)); }

        //! Returns true if this bounding box is open in the Ymax direction.
        bool IsOpenYmax() { return (Flags.HasFlag(MaskFlags.YmaxMask)); }
        //! Adds the 2d point.
        public void Add(gp_Pnt2d thePnt)
        {
            Update(thePnt.X(), thePnt.Y());
        }
        public void Update(double X, double Y)
        {
            if ((Flags & MaskFlags.VoidMask) > 0)
            {
                Xmin = X;
                Ymin = Y;
                Xmax = X;
                Ymax = Y;
                Flags &= ~MaskFlags.VoidMask;
            }
            else
            {
                if (!(Flags.HasFlag(MaskFlags.XminMask)) && (X < Xmin)) Xmin = X;
                else if (!(Flags.HasFlag(MaskFlags.XmaxMask)) && (X > Xmax)) Xmax = X;
                if (!(Flags.HasFlag(MaskFlags.YminMask)) && (Y < Ymin)) Ymin = Y;
                else if (!(Flags.HasFlag(MaskFlags.YmaxMask)) && (Y > Ymax)) Ymax = Y;
            }
        }

        //! Enlarges     the  box  with    a  tolerance  value.
        //! This means that the minimum values of its X and Y
        //! intervals of definition, when they are finite, are reduced by
        //! the absolute value of Tol, while the maximum values are
        //! increased by the same amount.
        public void Enlarge(double theTol)
        {
            double aTol = theTol < 0.0 ? -theTol : theTol;
            if (Gap < aTol)
                Gap = aTol;

        }

        //! Returns true if this bounding box is infinite in all 4
        //! directions (Whole Space flag).
        public bool IsWhole() { return (Flags.HasFlag(MaskFlags.WholeMask)); }
        public void Add(Bnd_Box2d Other)
        {
            if (IsWhole()) return;
            else if (Other.IsVoid()) return;
            else if (Other.IsWhole()) SetWhole();
            //else if (IsVoid()) (*this) = Other;//todo: ???? rewrite from upper-level
            else
            {
                if (!IsOpenXmin())
                {
                    if (Other.IsOpenXmin()) OpenXmin();
                    else if (Xmin > Other.Xmin) Xmin = Other.Xmin;
                }
                if (!IsOpenXmax())
                {
                    if (Other.IsOpenXmax()) OpenXmax();
                    else if (Xmax < Other.Xmax) Xmax = Other.Xmax;
                }
                if (!IsOpenYmin())
                {
                    if (Other.IsOpenYmin()) OpenYmin();
                    else if (Ymin > Other.Ymin) Ymin = Other.Ymin;
                }
                if (!IsOpenYmax())
                {
                    if (Other.IsOpenYmax()) OpenYmax();
                    else if (Ymax < Other.Ymax) Ymax = Other.Ymax;
                }
                Gap = Math.Max(Gap, Other.Gap);
            }
        }
        //! The Box will be infinitely long in the Xmin direction.
        void OpenXmin() { Flags |= MaskFlags.XminMask; }

        //! The Box will be infinitely long in the Xmax direction.
        void OpenXmax() { Flags |= MaskFlags.XmaxMask; }

        //! The Box will be infinitely long in the Ymin direction.
        void OpenYmin() { Flags |= MaskFlags.YminMask; }

        //! The Box will be infinitely long in the Ymax direction.
        void OpenYmax() { Flags |= MaskFlags.YmaxMask; }
        public void Update(double x, double y,

            double X, double Y)
        {
            if (Flags.HasFlag(MaskFlags.VoidMask))
            {
                Xmin = x;
                Ymin = y;
                Xmax = X;
                Ymax = Y;

                Flags &= ~MaskFlags.VoidMask;
            }
            else
            {
                if (!(Flags.HasFlag(MaskFlags.XminMask)) && (x < Xmin)) Xmin = x;
                if (!(Flags.HasFlag(MaskFlags.XmaxMask)) && (X > Xmax)) Xmax = X;
                if (!(Flags.HasFlag(MaskFlags.YminMask)) && (y < Ymin)) Ymin = y;
                if (!(Flags.HasFlag(MaskFlags.YmaxMask)) && (Y > Ymax)) Ymax = Y;
            }
        }

        public void Get(ref double x, ref double y,
                      ref double Xm, ref double Ym)
        {
            if (Flags.HasFlag(MaskFlags.VoidMask))
                throw new Standard_ConstructionError("Bnd_Box is void");
            double pinf = 1e+100; //-- Precision::Infinite();
            if (Flags.HasFlag(MaskFlags.XminMask)) x = -pinf;
            else x = Xmin - Gap;
            if (Flags.HasFlag(MaskFlags.XmaxMask)) Xm = pinf;
            else Xm = Xmax + Gap;
            if (Flags.HasFlag(MaskFlags.YminMask)) y = -pinf;
            else y = Ymin - Gap;
            if (Flags.HasFlag(MaskFlags.YmaxMask)) Ym = pinf;
            else Ym = Ymax + Gap;
        }

        MaskFlags Flags;
        //! Returns true if this 2D bounding box is empty (Void flag).
        public bool IsVoid() { return (Flags.HasFlag(MaskFlags.VoidMask)); }

    }
}
