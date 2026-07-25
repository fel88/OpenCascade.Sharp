using OCCPort.Common;

namespace TKMath
{
    //! Describes a bounding box in 3D space.
    //! A bounding box is parallel to the axes of the coordinates
    //! system. If it is finite, it is defined by the three intervals:
    //! -   [ Xmin,Xmax ],
    //! -   [ Ymin,Ymax ],
    //! -   [ Zmin,Zmax ].
    //! A bounding box may be infinite (i.e. open) in one or more
    //! directions. It is said to be:
    //! -   OpenXmin if it is infinite on the negative side of the   "X Direction";
    //! -   OpenXmax if it is infinite on the positive side of the "X Direction";
    //! -   OpenYmin if it is infinite on the negative side of the   "Y Direction";
    //! -   OpenYmax if it is infinite on the positive side of the "Y Direction";
    //! -   OpenZmin if it is infinite on the negative side of the   "Z Direction";
    //! -   OpenZmax if it is infinite on the positive side of the "Z Direction";
    //! -   WholeSpace if it is infinite in all six directions. In this
    //! case, any point of the space is inside the box;
    //! -   Void if it is empty. In this case, there is no point included in the box.
    //! A bounding box is defined by:
    //! -   six bounds (Xmin, Xmax, Ymin, Ymax, Zmin and
    //! Zmax) which limit the bounding box if it is finite,
    //! -   eight flags (OpenXmin, OpenXmax, OpenYmin,
    //! OpenYmax, OpenZmin, OpenZmax,
    //! WholeSpace and Void) which describe the
    //! bounding box if it is infinite or empty, and
    //! -   a gap, which is included on both sides in any direction
    //! when consulting the finite bounds of the box.
    public class Bnd_Box
    {
        public Bnd_Box()
        {
            Xmin = (Standard_Real.RealLast());
            Xmax = -Standard_Real.RealLast();
            Ymin = Standard_Real.RealLast();
            Ymax = -Standard_Real.RealLast();
            Zmin = Standard_Real.RealLast();
            Zmax = -Standard_Real.RealLast();
            Gap = (0.0);



            SetVoid();
        }

        public double GetGap()
        {
            return Gap;
        }


        //! Creates a bounding box, it contains:
        //! -   minimum/maximum point of bounding box,
        //! The constructed box is qualified Void. Its gap is null.
        public Bnd_Box(gp_Pnt theMin, gp_Pnt theMax)
        {
            Gap = (0.0);
            SetVoid();
            Update(theMin.X(), theMin.Y(), theMin.Z(), theMax.X(), theMax.Y(), theMax.Z());
        }


        //! Returns true if this bounding box is empty (Void flag).
        public bool IsVoid()
        {
            //return (Flags.HasFlag(MaskFlags.VoidMask));
            return (Flags & MaskFlags.VoidMask) != 0;
        }

        //! Returns a bounding box which is the result of applying the
        //! transformation T to this bounding box.
        //! Warning
        //! Applying a geometric transformation (for example, a
        //! rotation) to a bounding box generally increases its
        //! dimensions. This is not optimal for algorithms which use it.
        public Bnd_Box Transformed(gp_Trsf T)
        {
            if (IsVoid())
            {
                return new Bnd_Box();
            }
            else if (T.Form() == gp_TrsfForm.gp_Identity)
            {
                Bnd_Box b = this;
                return b;
            }
            else if (T.Form() == gp_TrsfForm.gp_Translation)
            {
                if (!HasFinitePart())
                {
                    Bnd_Box b = this;
                    return b;
                }

                gp_XYZ aDelta = T.TranslationPart();
                Bnd_Box _aNewBox = this;
                _aNewBox.Xmin += aDelta.X();
                _aNewBox.Xmax += aDelta.X();
                _aNewBox.Ymin += aDelta.Y();
                _aNewBox.Ymax += aDelta.Y();
                _aNewBox.Zmin += aDelta.Z();
                _aNewBox.Zmax += aDelta.Z();
                return _aNewBox;
            }

            Bnd_Box aNewBox = new Bnd_Box();
            if (HasFinitePart())
            {
                gp_Pnt[] aCorners =
                {
      new gp_Pnt (Xmin, Ymin, Zmin),
      new gp_Pnt (Xmax, Ymin, Zmin),
      new gp_Pnt (Xmin, Ymax, Zmin),
      new gp_Pnt (Xmax, Ymax, Zmin),
      new gp_Pnt (Xmin, Ymin, Zmax),
      new gp_Pnt (Xmax, Ymin, Zmax),
      new gp_Pnt (Xmin, Ymax, Zmax),
      new gp_Pnt (Xmax, Ymax, Zmax),
    };
                for (int aCornerIter = 0; aCornerIter < 8; ++aCornerIter)
                {
                    aCorners[aCornerIter].Transform(T);
                    aNewBox.Add(aCorners[aCornerIter]);
                }
            }
            aNewBox.Gap = Gap;
            if (!IsOpen())
            {
                return aNewBox;
            }

            gp_Dir[] aDirs = new gp_Dir[6];
            int aNbDirs = 0;
            if (IsOpenXmin())
            {
                aDirs[aNbDirs++].SetCoord(-1.0, 0.0, 0.0);
            }
            if (IsOpenXmax())
            {
                aDirs[aNbDirs++].SetCoord(1.0, 0.0, 0.0);
            }
            if (IsOpenYmin())
            {
                aDirs[aNbDirs++].SetCoord(0.0, -1.0, 0.0);
            }
            if (IsOpenYmax())
            {
                aDirs[aNbDirs++].SetCoord(0.0, 1.0, 0.0);
            }
            if (IsOpenZmin())
            {
                aDirs[aNbDirs++].SetCoord(0.0, 0.0, -1.0);
            }
            if (IsOpenZmax())
            {
                aDirs[aNbDirs++].SetCoord(0.0, 0.0, 1.0);
            }

            for (int aDirIter = 0; aDirIter < aNbDirs; ++aDirIter)
            {
                aDirs[aDirIter].Transform(T);
                aNewBox.Add(aDirs[aDirIter]);
            }

            return aNewBox;
        }

        //! Sets this bounding box so that it is empty. All points are outside a void box.
        public void SetVoid()
        {
            Xmin = Standard_Real.RealLast();
            Xmax = -Standard_Real.RealLast();
            Ymin = Standard_Real.RealLast();
            Ymax = -Standard_Real.RealLast();
            Zmin = Standard_Real.RealLast();
            Zmax = -Standard_Real.RealLast();
            Flags = MaskFlags.VoidMask;
            Gap = 0.0;
        }
        //! Returns the lower corner of this bounding box. The gap is included.
        //! If this bounding box is infinite (i.e. "open"), returned values
        //! may be equal to +/- Precision::Infinite().
        //! Standard_ConstructionError exception will be thrown if the box is void.
        //! if IsVoid()
        public gp_Pnt CornerMin()
        {
            gp_Pnt aCornerMin = new gp_Pnt();
            if (IsVoid())
            {
                throw new Standard_ConstructionError("Bnd_Box is void");
            }
            if (IsOpenXmin()) aCornerMin.SetX(-Bnd_Precision_Infinite);
            else aCornerMin.SetX(Xmin - Gap);
            if (IsOpenYmin()) aCornerMin.SetY(-Bnd_Precision_Infinite);
            else aCornerMin.SetY(Ymin - Gap);
            if (IsOpenZmin()) aCornerMin.SetZ(-Bnd_Precision_Infinite);
            else aCornerMin.SetZ(Zmin - Gap);
            return aCornerMin;
        }

        //! Returns the upper corner of this bounding box. The gap is included.
        //! If this bounding box is infinite (i.e. "open"), returned values
        //! may be equal to +/- Precision::Infinite().
        //! Standard_ConstructionError exception will be thrown if the box is void.
        //! if IsVoid()
        public gp_Pnt CornerMax()
        {
            gp_Pnt aCornerMax = new gp_Pnt();
            if (IsVoid())
            {
                throw new Standard_ConstructionError("Bnd_Box is void");
            }
            if (IsOpenXmax()) aCornerMax.SetX(Bnd_Precision_Infinite);
            else aCornerMax.SetX(Xmax + Gap);
            if (IsOpenYmin()) aCornerMax.SetY(Bnd_Precision_Infinite);
            else aCornerMax.SetY(Ymax + Gap);
            if (IsOpenZmin()) aCornerMax.SetZ(Bnd_Precision_Infinite);
            else aCornerMax.SetZ(Zmax + Gap);
            return aCornerMax;
        }
        //! Returns TRUE if this box has finite part.
        public bool HasFinitePart()
        {
            return !IsVoid()
                 && Xmax >= Xmin;
        }

        //=======================================================================
        //function : Enlarge
        //purpose  : 
        //=======================================================================

        public void Enlarge(double Tol)
        {
            Gap = Math.Max(Gap, Math.Abs(Tol));
        }
        //! Extends the Box  in the given Direction, i.e. adds
        //! an  half-line. The   box  may become   infinite in
        //! 1,2 or 3 directions.
        public void Add(gp_Dir D)
        {
            double DX, DY, DZ;
            D.Coord(out DX, out DY, out DZ);

            if (DX < -Standard_Real.RealEpsilon())
                OpenXmin();
            else if (DX > Standard_Real.RealEpsilon())
                OpenXmax();

            if (DY < -Standard_Real.RealEpsilon())
                OpenYmin();
            else if (DY > Standard_Real.RealEpsilon())
                OpenYmax();

            if (DZ < -Standard_Real.RealEpsilon())
                OpenZmin();
            else if (DZ > Standard_Real.RealEpsilon())
                OpenZmax();
        }

        public void Add(Bnd_Box Other)
        {
            if (Other.IsVoid())
            {
                return;
            }
            else if (IsVoid())
            {
                Copy(Other);
                //*this = Other;//??
                return;
            }

            if (Xmin > Other.Xmin) Xmin = Other.Xmin;
            if (Xmax < Other.Xmax) Xmax = Other.Xmax;
            if (Ymin > Other.Ymin) Ymin = Other.Ymin;
            if (Ymax < Other.Ymax) Ymax = Other.Ymax;
            if (Zmin > Other.Zmin) Zmin = Other.Zmin;
            if (Zmax < Other.Zmax) Zmax = Other.Zmax;
            Gap = Math.Max(Gap, Other.Gap);

            if (IsWhole())
            {
                return;
            }
            else if (Other.IsWhole())
            {
                SetWhole();
                return;
            }

            if (Other.IsOpenXmin()) OpenXmin();
            if (Other.IsOpenXmax()) OpenXmax();
            if (Other.IsOpenYmin()) OpenYmin();
            if (Other.IsOpenYmax()) OpenYmax();
            if (Other.IsOpenZmin()) OpenZmin();
            if (Other.IsOpenZmax()) OpenZmax();
        }

        private void Copy(Bnd_Box other)
        {
            Gap = other.Gap;
            Flags = other.Flags;
            Ymin = other.Ymin;
            Ymax = other.Ymax;
            Zmin = other.Zmin;
            Zmax = other.Zmax;
            Xmin = other.Xmin;
            Xmax = other.Xmax;
        }

        //! Sets this bounding box so that it covers the whole of 3D space.
        //! It is infinitely long in all directions.
        public void SetWhole() { Flags = MaskFlags.WholeMask; }


        //! The   Box will be   infinitely   long  in the Xmin
        //! direction.
        public void OpenXmin() { Flags |= MaskFlags.XminMask; }

        //! The   Box will be   infinitely   long  in the Xmax
        //! direction.
        public void OpenXmax() { Flags |= MaskFlags.XmaxMask; }

        //! The   Box will be   infinitely   long  in the Ymin
        //! direction.
        public void OpenYmin() { Flags |= MaskFlags.YminMask; }

        //! The   Box will be   infinitely   long  in the Ymax
        //! direction.
        public void OpenYmax() { Flags |= MaskFlags.YmaxMask; }

        //! The   Box will be   infinitely   long  in the Zmin
        //! direction.
        public void OpenZmin() { Flags |= MaskFlags.ZminMask; }

        //! The   Box will be   infinitely   long  in the Zmax
        //! direction.
        public void OpenZmax() { Flags |= MaskFlags.ZmaxMask; }
        //! Returns true if this bounding box is infinite in all 6 directions (WholeSpace flag).
        public bool IsWhole() { return (Flags & MaskFlags.WholeMask) == MaskFlags.WholeMask; }
        public void Add(gp_Pnt P)
        {
            double X, Y, Z;
            P.Coord(out X, out Y, out Z);
            Update(X, Y, Z);
        }

        public void Update(double X,

              double Y,

              double Z)
        {
            if (IsVoid())
            {
                Xmin = X;
                Ymin = Y;
                Zmin = Z;
                Xmax = X;
                Ymax = Y;
                Zmax = Z;
                ClearVoidFlag();
            }
            else
            {
                if (X < Xmin) Xmin = X;
                else if (X > Xmax) Xmax = X;
                if (Y < Ymin) Ymin = Y;
                else if (Y > Ymax) Ymax = Y;
                if (Z < Zmin) Zmin = Z;
                else if (Z > Zmax) Zmax = Z;
            }
        }


        //! Returns a finite part of an infinite bounding box (returns self if this is already finite box).
        //! This can be a Void box in case if its sides has been defined as infinite (Open) without adding any finite points.
        //! WARNING! This method relies on Open flags, the infinite points added using Add() method will be returned as is.
        public Bnd_Box FinitePart()
        {
            if (!HasFinitePart())
            {
                return new Bnd_Box();
            }

            Bnd_Box aBox = new Bnd_Box();
            aBox.Update(Xmin, Ymin, Zmin, Xmax, Ymax, Zmax);
            aBox.SetGap(Gap);
            return aBox;
        }

        //=======================================================================
        //function : SetGap
        //purpose  : 
        //=======================================================================

        public void SetGap(double Tol)
        {
            Gap = Tol;
        }

        //=======================================================================
        //function : Update
        //purpose  : 
        //=======================================================================

        public void Update(double x,
              double y,
              double z,
              double X,
              double Y,
              double Z)
        {
            if (IsVoid())
            {
                Xmin = x;
                Ymin = y;
                Zmin = z;
                Xmax = X;
                Ymax = Y;
                Zmax = Z;
                ClearVoidFlag();
            }
            else
            {
                if (x < Xmin) Xmin = x;
                if (X > Xmax) Xmax = X;
                if (y < Ymin) Ymin = y;
                if (Y > Ymax) Ymax = Y;
                if (z < Zmin) Zmin = z;
                if (Z > Zmax) Zmax = Z;
            }
        }

        // set the flag to one
        void ClearVoidFlag() { Flags &= ~MaskFlags.VoidMask; }

        //! Returns true if this bounding box has at least one open direction.
        public bool IsOpen() { return (Flags & MaskFlags.WholeMask) != 0; }

        private double Xmin;
        private double Xmax;
        private double Ymin;
        private double Ymax;
        private double Zmin;
        private double Zmax;
        private double Gap;
        private MaskFlags Flags;

        const double Bnd_Precision_Infinite = 1e+100;
        public void Get(out double theXmin,
                        out double theYmin,
                        out double theZmin,
                       out double theXmax,
                        out double theYmax,
                        out double theZmax)
        {
            if (IsVoid())
            {
                throw new Exception("Bnd_Box is void");
            }

            if (IsOpenXmin()) theXmin = -Bnd_Precision_Infinite;
            else theXmin = Xmin - Gap;
            if (IsOpenXmax()) theXmax = Bnd_Precision_Infinite;
            else theXmax = Xmax + Gap;
            if (IsOpenYmin()) theYmin = -Bnd_Precision_Infinite;
            else theYmin = Ymin - Gap;
            if (IsOpenYmax()) theYmax = Bnd_Precision_Infinite;
            else theYmax = Ymax + Gap;
            if (IsOpenZmin()) theZmin = -Bnd_Precision_Infinite;
            else theZmin = Zmin - Gap;
            if (IsOpenZmax()) theZmax = Bnd_Precision_Infinite;
            else theZmax = Zmax + Gap;
        }


        //! Returns true if this bounding box is open in the  Xmin direction.
        public bool IsOpenXmin() { return (Flags & MaskFlags.XminMask) != 0; }

        //! Returns true if this bounding box is open in the  Xmax direction.
        public bool IsOpenXmax() { return (Flags & MaskFlags.XmaxMask) != 0; }

        //! Returns true if this bounding box is open in the  Ymix direction.
        public bool IsOpenYmin() { return (Flags & MaskFlags.YminMask) != 0; }

        //! Returns true if this bounding box is open in the  Ymax direction.
        public bool IsOpenYmax() { return (Flags & MaskFlags.YmaxMask) != 0; }

        //! Returns true if this bounding box is open in the  Zmin direction.
        public bool IsOpenZmin() { return (Flags & MaskFlags.ZminMask) != 0; }

        //! Returns true if this bounding box is open in the  Zmax  direction.
        public bool IsOpenZmax() { return (Flags & MaskFlags.ZmaxMask) != 0; }



    }//! Bit flags.
}
