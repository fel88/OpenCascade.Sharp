using OCCPort.Common;
using TKMath;

namespace TKGeomBase
{
    //! Implementation of the analytical intersection between:
    //! - two Lin2d,
    //! - two Circ2d,
    //! - a Lin2d and a Circ2d,
    //! - an element of gp (Lin2d, Circ2d, Elips2d, Parab2d, Hypr2d)
    //! and another conic.
    //! No tolerance is given for all the intersections: the tolerance
    //! will be the "precision machine".
    public class IntAna2d_AnaIntersection
    {


        //! Returns TRUE if the computation was successful.
        public bool IsDone()
        {
            return done;

        }

        //! Intersection between two lines.
        public void Perform(gp_Lin2d L1, gp_Lin2d L2)
        {

            done = false;

            double A1, B1, C1;
            double A2, B2, C2;
            L1.Coefficients(out A1, out B1, out C1);
            L2.Coefficients(out A2, out B2, out C2);

            double al1, be1, ga1;
            double al2, be2, ga2;

            double Det = Math.Max(Math.Abs(A1), Math.Max(Math.Abs(A2), Math.Max(Math.Abs(B1), Math.Abs(B2))));

            if (Math.Abs(A1) == Det)
            {
                al1 = A1;
                be1 = B1;
                ga1 = C1;
                al2 = A2;
                be2 = B2;
                ga2 = C2;
            }
            else if (Math.Abs(B1) == Det)
            {
                al1 = B1;
                be1 = A1;
                ga1 = C1;
                al2 = B2;
                be2 = A2;
                ga2 = C2;
            }
            else if (Math.Abs(A2) == Det)
            {
                al1 = A2;
                be1 = B2;
                ga1 = C2;
                al2 = A1;
                be2 = B1;
                ga2 = C1;
            }
            else
            {
                al1 = B2;
                be1 = A2;
                ga1 = C2;
                al2 = B1;
                be2 = A1;
                ga2 = C1;
            }

            double rap = al2 / al1;
            double denom = be2 - rap * be1;

            if (Math.Abs(denom) <= Standard_Real.RealEpsilon())
            {                // Directions confondues
                para = true;
                nbp = 0;
                if (Math.Abs(ga2 - rap * ga1) <= Standard_Real.RealEpsilon())
                {          // Droites confondues
                    iden = true;
                    empt = false;
                }
                else
                {                                       // Droites paralleles
                    iden = false;
                    empt = true;
                }
            }
            else
            {
                para = false;
                iden = false;
                empt = false;
                nbp = 1;
                double XS = (be1 * ga2 / al1 - be2 * ga1 / al1) / denom;
                double YS = (rap * ga1 - ga2) / denom;

                if (((Math.Abs(A1) != Det) && (Math.Abs(B1) == Det)) ||
                ((Math.Abs(A1) != Det) && (Math.Abs(B1) != Det) && (Math.Abs(A2) != Det)))
                {
                    double temp = XS;
                    XS = YS;
                    YS = temp;
                }

                double La, Mu;
                if (Math.Abs(A1) >= Math.Abs(B1))
                {
                    La = (YS - L1.Location().Y()) / A1;
                }
                else
                {
                    La = (L1.Location().X() - XS) / B1;
                }
                if (Math.Abs(A2) >= Math.Abs(B2))
                {
                    Mu = (YS - L2.Location().Y()) / A2;
                }
                else
                {
                    Mu = (L2.Location().X() - XS) / B2;
                }
                lpnt[0].SetValue(XS, YS, La, Mu);
            }
            done = true;
        }

        //! returns the intersection point of range N;
        //! If (N<=0) or (N>NbPoints), an exception is raised.
        public IntAna2d_IntPoint Point(int N)
        {

            if (!done)
            {
                throw new StdFail_NotDone();
            }
            else
            {
                if ((N <= 0) || (N > nbp))
                {
                    throw new Standard_OutOfRange();
                }
                else
                {
                    return lpnt[N - 1];
                }
            }
        }

        //! For the intersection between an element of gp and a conic
        //! known by an implicit equation, the result will be TRUE
        //! if the element of gp verifies the implicit equation.
        //! For the intersection between two Lin2d or two Circ2d, the
        //! result will be TRUE if the elements are identical.
        //! The function returns FALSE in all the other cases.
        public bool IdenticalElements()
        {
            if (!done)
            {
                throw new StdFail_NotDone();
            }
            return iden;
        }

        //! returns the number of IntPoint between the 2 curves.
        public int NbPoints()
        {

            if (!done)
            {
                throw new StdFail_NotDone();
            }
            return nbp;
        }

        //! For the intersection between two Lin2d or two Circ2d,
        //! the function returns TRUE if the elements are parallel.
        //! The function returns FALSE in all the other cases.
        public bool ParallelElements()
        {
            if (!done)
            {
                throw new StdFail_NotDone();
            }
            return para;
        }

        bool done;
        bool para;
        bool iden;
        bool empt;
        int nbp;
        IntAna2d_IntPoint[] lpnt = new IntAna2d_IntPoint[4] { new IntAna2d_IntPoint(), new IntAna2d_IntPoint(), new IntAna2d_IntPoint(), new IntAna2d_IntPoint() };
    }
}

