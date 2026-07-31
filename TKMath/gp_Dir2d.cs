using OCCPort.Common;

namespace TKMath
{
    //! Describes a unit vector in the plane (2D space). This unit
    //! vector is also called "Direction".
    //! See Also
    //! gce_MakeDir2d which provides functions for more
    //! complex unit vector constructions
    //! Geom2d_Direction which provides additional functions
    //! for constructing unit vectors and works, in particular, with
    //! the parametric equations of unit vectors
    public class gp_Dir2d
    {                
        //! For this unit vector, returns its X coordinate.
        public double X() { return coord.X(); }

        //! For this unit vector, returns its Y coordinate.
        public double Y() { return coord.Y(); }

        //! Computes the cross product between two directions.
        public double Crossed(gp_Dir2d theRight) { return coord.Crossed(theRight.coord); }

        public void SetCoord(double theXv,
                                 double theYv)
        {
            double aD = Math.Sqrt(theXv * theXv + theYv * theYv);
            Exceptions.Standard_ConstructionError_Raise_if(aD <= gp.Resolution(), "gp_Dir2d::SetCoord() - result vector has zero norm");
            coord.SetX(theXv / aD);
            coord.SetY(theYv / aD);
        }

        //! Computes the scalar product
        public double Dot(gp_Dir2d theOther) { return coord.Dot(theOther.coord); }
        public void Reverse() { coord.Reverse(); }

        gp_XY coord;
        //! For this unit vector, returns its two coordinates as a number pair.
        //! Comparison between Directions
        //! The precision value is an input data.
        public gp_XY XY() { return coord; }
        public bool IsParallel(gp_Dir2d theOther,
                                               double theAngularTolerance)
        {
            double anAng = Angle(theOther);
            if (anAng < 0)
            {
                anAng = -anAng;
            }
            return anAng <= theAngularTolerance || Math.PI - anAng <= theAngularTolerance;
        }

        public double Angle(gp_Dir2d Other)
        {
            //    Commentaires :
            //    Au dessus de 45 degres l'arccos donne la meilleur precision pour le
            //    calcul de l'angle. Sinon il vaut mieux utiliser l'arcsin.
            //    Les erreurs commises sont loin d'etre negligeables lorsque l'on est
            //    proche de zero ou de 90 degres.
            //    En 2D les valeurs angulaires sont comprises entre -PI et PI
            double Cosinus = coord.Dot(Other.coord);
            double Sinus = coord.Crossed(Other.coord);
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


        //! Creates a direction corresponding to X axis.
        public gp_Dir2d()
        {
            coord = new(1.0, 0.0);
        }
        public gp_Dir2d(gp_Vec2d theV)
        {
            gp_XY aXY = theV.XY();
            double aX = aXY.X();
            double anY = aXY.Y();
            double aD = Math.Sqrt(aX * aX + anY * anY);
            Exceptions.Standard_ConstructionError_Raise_if(aD <= gp.Resolution(), "gp_Dir2d() - input vector has zero norm");
            coord.SetX(aX / aD);
            coord.SetY(anY / aD);
        }

        public gp_Dir2d(double theXv, double theYv)
        {
            double aD = Math.Sqrt(theXv * theXv + theYv * theYv);
            //Standard_ConstructionError_Raise_if(aD <= gp::Resolution(), "gp_Dir2d() - input vector has zero norm");
            coord.SetX(theXv / aD);
            coord.SetY(theYv / aD);
        }
    }
}
