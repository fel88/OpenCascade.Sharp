using OCCPort;
using OCCPort.Common;
using System.Security.Cryptography;
using static System.Net.Mime.MediaTypeNames;

namespace TKMath
{
    //! Defines a non-persistent vector in 2D space.
    public struct gp_Vec2d
    {
        //! For this vector, returns its X  coordinate.
        public double X() { return coord.X(); }
        //! For this vector, returns its two coordinates as a number pair
        public gp_XY XY() { return coord; }

        //! For this vector, returns its Y  coordinate.
        public double Y() { return coord.Y(); }
        //! Assigns the two coordinates of theCoord to this vector.
        public void SetXY(gp_XY theCoord) { coord = theCoord; }
        //! Computes the square magnitude of this vector.
        public double SquareMagnitude() { return coord.SquareModulus(); }

        //! For this vector, returns  its two coordinates theXv and theYv
        public void Coord(out double theXv, out double theYv) { coord.Coord(out theXv, out theYv); }

        //! Creates a vector from two points. The length of the vector
        //! is the distance between theP1 and theP2
        public gp_Vec2d(gp_Pnt2d theP1, gp_Pnt2d theP2)
        {
            coord = theP2.XY().Subtracted(theP1.XY());

        }

        //! Creates a unitary vector from a direction theV.
        public gp_Vec2d(gp_Dir2d theV)
        {
            coord = theV.XY();
        }


        //! Creates a vector with a doublet of coordinates.
        public gp_Vec2d(gp_XY theCoord)
        {
            coord = (theCoord);
        }

        public static gp_Vec2d operator /(gp_Vec2d v, double theScalar)
        {
            return v.Divided(theScalar);
        }

        //! divides a vector by a scalar
        gp_Vec2d Divided(double theScalar)
        {
            gp_Vec2d aV = this;
            aV.coord.Divide(theScalar);
            return aV;
        }

        public static double operator ^(gp_Vec2d v, gp_Vec2d theRight)
        {
            return v.Crossed(theRight);
        }
        //! Computes the crossing product between two vectors
        public double Crossed(gp_Vec2d theRight)
        {
            return coord.Crossed(theRight.coord);
        }
        public gp_Vec2d() { }
        //! Computes the magnitude of this vector.
        public double Magnitude() { return coord.Modulus(); }

        public double Angle(gp_Vec2d Other)
        {
            //    Commentaires :
            //    Au dessus de 45 degres l'arccos donne la meilleur precision pour le
            //    calcul de l'angle. Sinon il vaut mieux utiliser l'arcsin.
            //    Les erreurs commises sont loin d'etre negligeables lorsque l'on est
            //    proche de zero ou de 90 degres.
            //    En 2D les valeurs angulaires sont comprises entre -PI et PI
            double theNorm = Magnitude();
            double theOtherNorm = Other.Magnitude();
            if (theNorm <= gp.Resolution() || theOtherNorm <= gp.Resolution())
                throw new gp_VectorWithNullMagnitude();

            double D = theNorm * theOtherNorm;
            double Cosinus = coord.Dot(Other.coord) / D;
            double Sinus = coord.Crossed(Other.coord) / D;
            if (Cosinus > -0.70710678118655 && Cosinus < 0.70710678118655)
            {
                if (Sinus > 0.0) return Math.Acos(Cosinus);
                else return -Math.Acos(Cosinus);
            }
            else
            {
                if (Cosinus > 0.0) return Math.Asin(Sinus);
                else
                {
                    if (Sinus > 0.0) return Math.PI - Math.Asin(Sinus);
                    else return -Math.PI - Math.Asin(Sinus);
                }
            }
        }

        gp_XY coord;



    }
}
