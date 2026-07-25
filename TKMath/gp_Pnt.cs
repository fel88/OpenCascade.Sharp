using OCCPort.Common;

namespace TKMath
{
    //! Defines a 3D cartesian point.
    public struct gp_Pnt
    {
        public gp_XYZ coord;

        public override string ToString()
        {
            return $"gp_Pnt  X: {coord.X()} Y: {coord.Y()} Z: {coord.Z()}";
        }
        public void Rotate(gp_Ax1 theA1, double theAng)
        {
            gp_Trsf aT = new gp_Trsf();
            aT.SetRotation(theA1, theAng);
            aT.Transforms(coord);
        }

        //! Creates a point with zero coordinates.
        //public gp_Pnt() { }

        //! Creates a point from a XYZ object.
        public gp_Pnt(gp_XYZ theCoord)

        {
            coord = new gp_XYZ(theCoord);
        }
        public gp_Pnt Translated(gp_Vec theV)
        {
            gp_Pnt aP = new gp_Pnt(this.coord);
            aP.coord.Add(theV.XYZ());
            return aP;
        }

        //! Assigns the three coordinates of theCoord to this point.
        public void SetXYZ(gp_XYZ theCoord) { coord = theCoord; }

        //=======================================================================
        public double SquareDistance(gp_Pnt theOther)
        {
            double aD = 0, aDD;
            gp_XYZ XYZ = theOther.coord;
            aDD = coord.X(); aDD -= XYZ.X(); aDD *= aDD; aD += aDD;
            aDD = coord.Y(); aDD -= XYZ.Y(); aDD *= aDD; aD += aDD;
            aDD = coord.Z(); aDD -= XYZ.Z(); aDD *= aDD; aD += aDD;
            return aD;
        }
        //! For this point, returns its three coordinates as a XYZ object.
        public gp_XYZ Coord() { return coord; }

        public double SquareDistance(gp_XYZ theOther)
        {
            return SquareDistance(new gp_Pnt(theOther));
        }

        //! For this point gives its three coordinates theXp, theYp and theZp.
        public void Coord(out double theXp, out double theYp, out double theZp)
        {
            coord.Coord(out theXp, out theYp, out theZp);
        }

        //! For this point, assigns  the values theXp, theYp and theZp to its three coordinates.
        public void SetCoord(double theXp, double theYp, double theZp)
        {
            coord.SetCoord(theXp, theYp, theZp);
        }


        public gp_Pnt(double x, double y, double z)
        {
            this.coord = new gp_XYZ(x, y, z);
        }

        //! For this point, returns its X coordinate.
        public double X() { return coord.X(); }

        //! For this point, returns its Y coordinate.
        public double Y() { return coord.Y(); }

        //! For this point, returns its Z coordinate.
        public double Z() { return coord.Z(); }

        //! For this point, returns its three coordinates as a XYZ object.
        public gp_XYZ XYZ() { return coord; }

        public bool IsEqual(gp_Pnt theEye, double v)
        {
            return coord.IsEqual(theEye.coord, v);
        }

        public double Distance(gp_Pnt theOther)
        {
            double aD = 0, aDD;
            gp_XYZ aXYZ = theOther.coord;
            aDD = coord.X(); aDD -= aXYZ.X(); aDD *= aDD; aD += aDD;
            aDD = coord.Y(); aDD -= aXYZ.Y(); aDD *= aDD; aD += aDD;
            aDD = coord.Z(); aDD -= aXYZ.Z(); aDD *= aDD; aD += aDD;
            return Math.Sqrt(aD);
        }


        public void Transform(TopLoc_Location T)
        {
            Transform(T.Transformation());
        }

        public void Transform(gp_Trsf T)
        {
            if (T.Form() == gp_TrsfForm.gp_Identity) { }
            else if (T.Form() == gp_TrsfForm.gp_Translation) { coord.Add(T.TranslationPart()); }
            else if (T.Form() == gp_TrsfForm.gp_Scale)
            {
                coord.Multiply(T.ScaleFactor());
                coord.Add(T.TranslationPart());
            }
            else if (T.Form() == gp_TrsfForm.gp_PntMirror)
            {
                coord.Reverse();
                coord.Add(T.TranslationPart());
            }
            else { T.Transforms(ref coord); }
        }

        //! Assigns the given value to the X coordinate of this point.
        public void SetX(double theX) { coord.SetX(theX); }

        //! Assigns the given value to the Y coordinate of this point.
        public void SetY(double theY) { coord.SetY(theY); }

        //! Assigns the given value to the Z coordinate of this point.
        public void SetZ(double theZ) { coord.SetZ(theZ); }


        public void Translate(gp_Vec theV)
        {
            coord.Add(theV.XYZ());
        }

        public gp_Pnt Transformed(gp_Trsf theT)
        {
            gp_Pnt aP = new gp_Pnt(this.coord);
            aP.Transform(theT);
            return aP;


        }
    } //! Defines a non-persistent vector in 3D space.
}
