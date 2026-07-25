using OCCPort.Common;

namespace TKMath
{
    public struct gp_Vec
    {
        gp_XYZ coord;
        //! Multiplies a vector by a scalar
        public gp_Vec Multiplied(double theScalar)
        {
            gp_Vec aV = this;
            aV.coord.Multiply(theScalar);
            return aV;
        }

        //! Computes the magnitude of the cross
        //! product between <me> and theRight.
        //! Returns || <me> ^ theRight ||
        public double CrossMagnitude(gp_Vec theRight) { return coord.CrossMagnitude(theRight.coord); }

        //! Assigns the three coordinates of theCoord to this vector.
        public void SetXYZ(gp_XYZ theCoord) { coord = theCoord; }

        public void Transform(gp_Trsf T)
        {
            if (T.Form() == gp_TrsfForm.gp_Identity || T.Form() == gp_TrsfForm.gp_Translation) { }
            else if (T.Form() == gp_TrsfForm.gp_PntMirror) { coord.Reverse(); }
            else if (T.Form() == gp_TrsfForm.gp_Scale) { coord.Multiply(T.ScaleFactor()); }
            else { coord.Multiply(T.VectorialPart()); }
        }

        public static implicit operator gp_Dir(gp_Vec f)
        {
            return new gp_Dir(f);
        }

        //! For this vector, assigns
        //! -   the values theXv, theYv and theZv to its three coordinates.
        public void SetCoord(double theXv, double theYv, double theZv)
        {
            coord.SetX(theXv);
            coord.SetY(theYv);
            coord.SetZ(theZv);
        }

        //! Adds two vectors
        public void Add(gp_Vec theOther) { coord.Add(theOther.coord); }

        //! <me> is set to the following linear form : theV1 + theV2
        public void SetLinearForm(gp_Vec theV1, gp_Vec theV2)
        {
            // coord.SetLinearForm(theV1.coord, theV2.coord);
        } //! <me> is set to the following linear form : theA1 * theV1 + theV2
        public void SetLinearForm(double theA1, gp_Vec theV1, gp_Vec theV2)
        {
            //coord.SetLinearForm(theA1, theV1.coord, theV2.coord);
        }  //! <me> is set to the following linear form :
           //! theA1 * theV1 + theA2 * theV2
        public void SetLinearForm(double theA1, gp_Vec theV1,
                       double theA2, gp_Vec theV2)
        {
            coord.SetLinearForm(theA1, theV1.coord, theA2, theV2.coord);
        }

        //! <me> is set to the following linear form :
        //! theA1 * theV1 + theA2 * theV2 + theA3 * theV3
        public void SetLinearForm(double theA1, gp_Vec theV1,
                        double theA2, gp_Vec theV2,
                        double theA3, gp_Vec theV3)
        {
            coord.SetLinearForm(theA1, theV1.coord, theA2, theV2.coord, theA3, theV3.coord);
        }

        //! <me> is set to the following linear form :
        //! theA1 * theV1 + theA2 * theV2 + theV3
        public void SetLinearForm(double theA1, gp_Vec theV1,
                           double theA2, gp_Vec theV2, gp_Vec theV3)
        {
            coord.SetLinearForm(theA1, theV1.coord, theA2, theV2.coord, theV3.coord);
        }

        //! computes the cross product between two vectors
        public void Cross(gp_Vec theRight) { coord.Cross(theRight.coord); }

        //! Assigns the given value to the X coordinate of this vector.
        public void SetX(double theX) { coord.SetX(theX); }

        //! Assigns the given value to the X coordinate of this vector.
        public void SetY(double theY) { coord.SetY(theY); }

        //! Assigns the given value to the X coordinate of this vector.
        public void SetZ(double theZ) { coord.SetZ(theZ); }
        //! For this vector, returns its X coordinate.
        public double X() { return coord.X(); }

        //! For this vector, returns its Y coordinate.
        public double Y() { return coord.Y(); }

        //! For this vector, returns its Z  coordinate.
        public double Z() { return coord.Z(); }
        public void Rotate(gp_Ax1 theA1, double theAng)
        {
            gp_Trsf aT = new gp_Trsf();
            aT.SetRotation(theA1, theAng);
            coord.Multiply(aT.VectorialPart());
        }

        //! Multiplies a vector by a scalar
        public void Multiply(double theScalar) { coord.Multiply(theScalar); }


        //! Returns True if Angle(<me>, theOther) <= theAngularTolerance or
        //! PI - Angle(<me>, theOther) <= theAngularTolerance
        //! This definition means that two parallel vectors cannot define
        //! a plane but two vectors with opposite directions are considered
        //! as parallel. Raises VectorWithNullMagnitude if <me>.Magnitude() <= Resolution or
        //! Other.Magnitude() <= Resolution from gp
        public bool IsParallel(gp_Vec theOther, double theAngularTolerance)
        {
            double anAng = Angle(theOther);
            return anAng <= theAngularTolerance || Math.PI - anAng <= theAngularTolerance;
        }
        public double Angle(gp_Vec theOther)
        {
            gp_VectorWithNullMagnitude_Raise_if(coord.Modulus() <= gp.Resolution() ||
                                                 theOther.coord.Modulus() <= gp.Resolution(), " ");
            return (new gp_Dir(coord)).Angle(theOther);
        }

        private void gp_VectorWithNullMagnitude_Raise_if(bool v1, string v2)
        {
            if (v1)
                throw new Exception(v2);
        }

        //! computes the scalar product
        public double Dot(gp_Vec theOther) { return coord.Dot(theOther.coord); }

        //! computes the scalar product
        public double Dot(gp_XYZ theOther) { return coord.Dot(theOther); }



        public gp_XYZ XYZ()
        {
            return coord;
        }

        public gp_Vec Added(gp_Vec theOther)
        {
            gp_Vec aV = this;
            aV.coord.Add(theOther.coord);
            return aV;
        }



        public static double operator *(gp_Vec one, gp_Vec theOther)
        {
            return one.Dot(theOther);
        }
        public static gp_Vec operator *(gp_Vec v, double theScalar)
        {
            return v.Multiplied(theScalar);
        }
        public static gp_Vec operator ^(gp_Vec v, gp_Vec theRight)
        {
            return v.Crossed(theRight);
        }

        //! computes the cross product between two vectors
        public gp_Vec Crossed(gp_Vec theRight)
        {
            this.coord.Cross(theRight.coord);
            return this;
        }
        public static gp_Vec operator *(double theScalar, gp_Vec v)
        {
            return v.Multiplied(theScalar);
        }


        //! Subtracts two vectors
        public void Subtract(gp_Vec theRight) { coord.Subtract(theRight.coord); }

        public static gp_Vec operator -(gp_Vec left, gp_Vec v)
        {
            left.Subtract(v);
            return left;
        }

        public static gp_Vec operator -(gp_Vec f)
        {
            return f.Reversed();
        }
        //! Reverses the direction of a vector
        public gp_Vec Reversed()
        {
            this.coord.Reverse();
            return this;
        }

        public static gp_Vec operator +(gp_Vec v, gp_Vec theOther)
        {
            return v.Added(theOther);
        }


        //! Creates a vector with a triplet of coordinates.
        public gp_Vec(gp_XYZ theCoord)
        {
            coord = (theCoord);
        }

        //! Creates a point with its three cartesian coordinates.
        public gp_Vec(double theXv, double theYv, double theZv)
        {

            coord = new gp_XYZ(theXv, theYv, theZv);
        }


        //! Computes the magnitude of this vector.
        public double Magnitude() { return coord.Modulus(); }

        //! normalizes a vector
        //! Raises an exception if the magnitude of the vector is
        //! lower or equal to Resolution from gp.
        public void Normalize()
        {
            double aD = coord.Modulus();

            if (aD <= gp.Resolution())
                throw new Exception("gp_Vec::Normalize() - vector has zero norm");

            coord.Divide(aD);
        }
        //! Computes the square magnitude of this vector.
        public double SquareMagnitude() => coord.SquareModulus();
        //! Computes the square magnitude of
        //! the cross product between <me> and theRight.
        //! Returns || <me> ^ theRight ||**2
        public double CrossSquareMagnitude(gp_Vec theRight)
        {
            return coord.CrossSquareMagnitude(theRight.coord);
        }

        public gp_Vec(gp_Dir theV)
        {
            coord = theV.XYZ();
        }

        public gp_Vec(gp_Pnt theP1, gp_Pnt theP2) : this()
        {

            coord = theP2.XYZ().Subtracted(theP1.XYZ());
        }

        public override string ToString()
        {
            return $"gp_Vec: X:{coord.X()} Y:{coord.Y()} Z:{coord.Z()}";
        }

    }
}
