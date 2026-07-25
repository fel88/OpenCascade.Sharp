using OCCPort.Common;
using System;

namespace TKMath
{
    public struct gp_Ax2
    {

        //! Creates an object corresponding to the reference
        //! coordinate system (OXYZ).
        public gp_Ax2()
        {
            vydir = new gp_Dir(0.0, 1.0, 0.0);
            // vxdir(1.,0.,0.) use default ctor of gp_Dir, as it creates the same dir(1,0,0)
        }

        //! Creates -   a coordinate system with an origin P, where V
        //! gives the "main Direction" (here, "X Direction" and "Y
        //! Direction" are defined automatically).
        public gp_Ax2(gp_Pnt P, gp_Dir V)
        {
            axis = new(P, V);

            double A = V.X();
            double B = V.Y();
            double C = V.Z();
            double Aabs = A;
            if (Aabs < 0) Aabs = -Aabs;
            double Babs = B;
            if (Babs < 0) Babs = -Babs;
            double Cabs = C;
            if (Cabs < 0) Cabs = -Cabs;

            gp_Dir D = new gp_Dir();

            //  pour determiner l axe X :
            //  on dit que le produit scalaire Vx.V = 0. 
            //  et on recherche le max(A,B,C) pour faire la division.
            //  l'une des coordonnees du vecteur est nulle. 

            if (Babs <= Aabs && Babs <= Cabs)
            {
                if (Aabs > Cabs) D.SetCoord(-C, 0.0, A);
                else D.SetCoord(C, 0.0, -A);
            }
            else if (Aabs <= Babs && Aabs <= Cabs)
            {
                if (Babs > Cabs) D.SetCoord(0.0, -C, B);
                else D.SetCoord(0.0, C, -B);
            }
            else
            {
                if (Aabs > Babs) D.SetCoord(-B, A, 0.0);
                else D.SetCoord(B, -A, 0.0);
            }
            SetXDirection(D);
        }

        public void SetDirection(gp_Dir theV)
        {
            double a = theV * vxdir;
            if (Math.Abs(Math.Abs(a) - 1.0) <= Precision.Angular())
            {
                if (a > 0.0)
                {
                    vxdir = vydir;
                    vydir = axis.Direction();
                    axis.SetDirection(theV);
                }
                else
                {
                    vxdir = axis.Direction();
                    axis.SetDirection(theV);
                }
            }
            else
            {
                axis.SetDirection(theV);
                vxdir = theV.CrossCrossed(vxdir, theV);
                vydir = theV.Crossed(vxdir);
            }
        }

        //! Changes the "Xdirection" of <me>. The main direction
        //! "Direction" is not modified, the "Ydirection" is modified.
        //! If <Vx> is not normal to the main direction then <XDirection>
        //! is computed as follows XDirection = Direction ^ (Vx ^ Direction).
        //! Exceptions
        //! Standard_ConstructionError if Vx or Vy is parallel to
        //! the "main Direction" of this coordinate system.
        public void SetXDirection(gp_Dir theVx)
        {
            vxdir = axis.Direction().CrossCrossed(theVx, axis.Direction());
            vydir = axis.Direction().Crossed(vxdir);
        }

        //! Translates an axis plaxement in the direction of the vector <theV>.
        //! The magnitude of the translation is the vector's magnitude.
        public gp_Ax2 Translated(gp_Vec theV)
        {
            gp_Ax2 aTemp = this;
            aTemp.Translate(theV);
            return aTemp;
        }

        public void Transform(gp_Trsf theT)
        {
            gp_Pnt aTemp = axis.Location();
            aTemp.Transform(theT);
            axis.SetLocation(aTemp);
            vxdir.Transform(theT);
            vydir.Transform(theT);
            axis.SetDirection(vxdir.Crossed(vydir));
        }

        public void Rotate(gp_Ax1 theA1, double theAng)
        {
            gp_Pnt aTemp = axis.Location();
            aTemp.Rotate(theA1, theAng);
            axis.SetLocation(aTemp);
            vxdir.Rotate(theA1, theAng);
            vydir.Rotate(theA1, theAng);
            axis.SetDirection(vxdir.Crossed(vydir));
        }

        public void Translate(gp_Vec theV) { axis.Translate(theV); }

        //! Creates an axis placement with an origin P such that:
        //! -   N is the Direction, and
        //! -   the "X Direction" is normal to N, in the plane
        //! defined by the vectors (N, Vx): "X
        //! Direction" = (N ^ Vx) ^ N,
        //! Exception: raises ConstructionError if N and Vx are parallel (same or opposite orientation).
        public gp_Ax2(gp_Pnt P, gp_Dir N, gp_Dir Vx)
        {
            axis = new gp_Ax1(P, N);
            vydir = N;
            vxdir = N;
            vxdir.CrossCross(Vx, N);
            vydir.Cross(vxdir);
        }

        //! Returns the main axis of <me>. It is the "Location" point
        //! and the main "Direction".
        public gp_Ax1 Axis() { return axis; }


        //! Returns the main direction of <me>.
        public gp_Dir Direction() { return axis.Direction(); }

        //! Returns the "Location" point (origin) of <me>.
        public gp_Pnt Location() { return axis.Location(); }

        //! Returns the "XDirection" of <me>.


        gp_Ax1 axis;
        gp_Dir vydir;
        gp_Dir vxdir;

        public gp_Dir XDirection()
        {
            return vxdir;
        }

        public gp_Dir YDirection()
        {
            return vydir;
        }
    }
}