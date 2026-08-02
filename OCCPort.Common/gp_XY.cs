using System.Threading;

namespace OCCPort.Common
{
    //! This class describes a cartesian coordinate entity in 2D
    //! space {X,Y}. This class is non persistent. This entity used
    //! for algebraic calculation. An XY can be transformed with a
    //! Trsf2d or a  GTrsf2d from package gp.
    //! It is used in vectorial computations or for holding this type
    //! of information in data structures.
    public struct gp_XY
    {
        public void Subtract(gp_XY theOther)
        {
            x -= theOther.x;
            y -= theOther.y;
        }  //! Divides <me> by a real.
        public gp_XY Divided(double theScalar)
        {
            return new gp_XY(x / theScalar, y / theScalar);
        }

        //! @code
        //! <me>.X() = <me>.X() * theScalar;
        //! <me>.Y() = <me>.Y() * theScalar;
        //! @endcode
        public void Multiply(double theScalar)
        {
            x *= theScalar;
            y *= theScalar;
        }

        //! divides <me> by a real.
        public void Divide(double theScalar)
        {
            x /= theScalar;
            y /= theScalar;
        }

        public override string ToString()
        {
            return $"gp_XY X:{x} Y:{y}";
        }
        public static gp_XY operator /(gp_XY vv, double theInvFactor)
        {
            return vv.Divided(theInvFactor);
        }
        public gp_XY Multiplied(gp_XY theOther) { return new gp_XY(x * theOther.X(), y * theOther.Y()); }
        public gp_XY Multiplied(double v) { return new gp_XY(x * v, y * v); }

        public bool IsEqual(gp_XY Other,
                  double Tolerance)
        {
            double val;
            val = x - Other.x;
            if (val < 0) val = -val;
            if (val > Tolerance)
                return false;

            val = y - Other.y;
            if (val < 0) val = -val;
            if (val > Tolerance)
                return false;

            return true;
        }


        public static double operator ^(gp_XY theOther, gp_XY v2) { return theOther.Crossed(v2); }


        //! --  Computes  the following linear combination and
        //! assigns the result to this number pair:
        //! @code
        //! theA1 * theXY1 + theA2 * theXY2 + theXY3
        //! @endcode
        public void SetLinearForm(double theA1, gp_XY theXY1,
                             double theA2, gp_XY theXY2,
                              gp_XY theXY3)
        {
            x = theA1 * theXY1.x + theA2 * theXY2.x + theXY3.x;
            y = theA1 * theXY1.y + theA2 * theXY2.y + theXY3.y;
        }
        //! Computes  the following linear combination and
        //! assigns the result to this number pair:
        //! @code
        //! theA1 * theXY1 + theA2 * theXY2
        //! @endcode
        public void SetLinearForm(double theA1, gp_XY theXY1,
                              double theA2, gp_XY theXY2)
        {
            x = theA1 * theXY1.x + theA2 * theXY2.x;
            y = theA1 * theXY1.y + theA2 * theXY2.y;
        }
        public void SetLinearForm(double theA1, gp_XY theXY1,
                              gp_XY theXY2)
        {
            x = theA1 * theXY1.x + theXY2.x;
            y = theA1 * theXY1.y + theXY2.y;
        }
        public gp_XY Subtracted(gp_XY theOther)
        {
            gp_XY aCoord2D = new gp_XY(this);
            aCoord2D.Subtract(theOther);
            return aCoord2D;
        }

        //! @code
        //! <me>.X() = -<me>.X()
        //! <me>.Y() = -<me>.Y()
        public void Reverse()
        {
            x = -x;
            y = -y;
        }

        //! Returns the X coordinate of this number pair.
        public double X() { return x; }

        //! Returns the Y coordinate of this number pair.
        public double Y() { return y; }

        //! a number pair defined by the XY coordinates
        public gp_XY(double theX, double theY)
        {
            x = theX;
            y = theY;
        }

        public gp_XY(gp_XY gp_XY) : this()
        {
            x = gp_XY.x;
            y = gp_XY.y;
        }

        //! Computes X*X + Y*Y where X and Y are the two coordinates of this number pair.
        public double SquareModulus() { return x * x + y * y; }

        public static gp_XY operator -(gp_XY vv, gp_XY v2)
        {
            return new gp_XY(vv.x - v2.x,
                    vv.y - v2.y);
        }
        public static gp_XY operator +(gp_XY vv, gp_XY v2)
        {
            return new gp_XY(vv.x + v2.x,
                    vv.y + v2.y);
        }
        public static gp_XY operator *(gp_XY vv, double v2)
        {
            return new gp_XY(vv.x * v2,
                    vv.y * v2);
        }
        //! Assigns the given value to the X coordinate of this number pair.
        public void SetX(double theX) { x = theX; }

        //! Assigns the given value to the Y  coordinate of this number pair.
        public void SetY(double theY) { y = theY; }

        //! Computes the scalar product between <me> and theOther
        public double Dot(gp_XY theOther) { return x * theOther.x + y * theOther.y; }

        //! @code
        //! double D = <me>.X() * theOther.Y() - <me>.Y() * theOther.X()
        //! @endcode
        public double Crossed(gp_XY theOther) { return x * theOther.y - y * theOther.x; }

        //! For this number pair, returns its coordinates X and Y.
        public void Coord(out double theX, out double theY)
        {
            theX = x;
            theY = y;
        }


        //! Computes Sqrt (X*X + Y*Y) where X and Y are the two coordinates of this number pair.
        public double Modulus()
        {
            return Math.Sqrt(x * x + y * y);
        }


        //! modifies the coordinate of range theIndex
        //! theIndex = 1 => X is modified
        //! theIndex = 2 => Y is modified
        //! Raises OutOfRange if theIndex != {1, 2}.
        public void SetCoord(int theIndex, double theXi)
        {
            Exceptions.Standard_OutOfRange_Raise_if(theIndex < 1 || theIndex > 2, null);
            if (theIndex == 1) x = theXi;
            else
                y = theXi;
            //(&x)[theIndex - 1] = theXi;
        }

        //! For this number pair, assigns
        //! the values theX and theY to its coordinates
        public void SetCoord(double theX, double theY)
        {
            x = theX;
            y = theY;
        }

        public double ChangeCoord(int v)
        {
            if (v == 1) return X();
            return Y();
        }
        public void ChangeCoord(int index, double v)
        {
            SetCoord(index, v);
        }
        double x;
        double y;

    }
}
