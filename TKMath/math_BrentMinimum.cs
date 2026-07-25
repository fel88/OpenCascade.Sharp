using OCCPort.Common;
using System.Reflection.Emit;

namespace TKMath
{
    //! This class implements the Brent's method to find the minimum of
    //! a function of a single variable.
    //! No knowledge of the derivative is required.
    public class math_BrentMinimum
    {

        //! This constructor should be used in a sub-class to initialize
        //! correctly all the fields of this class.
        public math_BrentMinimum(double theTolX, int theNbIterations = 100, double theZEPS = 1.0e-12)
        {
            a = (0.0);
            b = (0.0);
            x = (0.0);
            fx = (0.0);
            fv = (0.0);
            fw = (0.0);
            XTol = (theTolX);
            EPSZ = (theZEPS);
            Done = (false);
            iter = (0);
            Itermax = (theNbIterations);
            myF = (false);
        }


        //! returns the value of the minimum.
        //! Exception NotDone is raised if the minimum was not found.
        public double Minimum()
        {
            Exceptions.StdFail_NotDone_Raise_if(!Done, " ");
            return fx;
        }

        public bool IsSolutionReached(math_Function mf)
        {
            double TwoTol = 2.0 * (XTol * Math.Abs(x) + EPSZ);
            return (x <= (TwoTol + a)) && (x >= (b - TwoTol));
        }

        const double CGOLD = 0.3819660; //0.5*(3 - sqrt(5));
        //! Brent minimization is performed on function F from a given
        //! bracketing triplet of abscissas Ax, Bx, Cx (such that Bx is
        //! between Ax and Cx, F(Bx) is less than both F(Bx) and F(Cx))
        //! The solution is found when: abs(Xi - Xi-1) <= TolX * abs(Xi) + ZEPS;
        public void Perform(math_Function F, double ax, double bx, double cx)
        {
            bool OK;
            double etemp, fu, p, q, r;
            double tol1, tol2, u, v, w, xm;
            double e = 0.0;
            double d = Standard_Real.RealLast();

            a = ((ax < cx) ? ax : cx);
            b = ((ax > cx) ? ax : cx);
            x = w = v = bx;
            if (!myF)
            {
                OK = F.Value(x, out fx);
                if (!OK) return;
            }
            fw = fv = fx;
            for (iter = 1; iter <= Itermax; iter++)
            {
                xm = 0.5 * (a + b);
                tol1 = XTol * Math.Abs(x) + EPSZ;
                tol2 = 2.0 * tol1;
                if (IsSolutionReached(F))
                {
                    Done = true;
                    return;
                }
                if (Math.Abs(e) > tol1)
                {
                    r = (x - w) * (fx - fv);
                    q = (x - v) * (fx - fw);
                    p = (x - v) * q - (x - w) * r;
                    q = 2.0 * (q - r);
                    if (q > 0.0) p = -p;
                    q = Math.Abs(q);
                    etemp = e;
                    e = d;
                    if (Math.Abs(p) >= Math.Abs(0.5 * q * etemp)
                      || p <= q * (a - x) || p >= q * (b - x))
                    {
                        e = (x >= xm ? a - x : b - x);
                        d = CGOLD * e;
                    }
                    else
                    {
                        d = p / q;
                        u = x + d;
                        if (u - a < tol2 || b - u < tol2) d = Standard_Real.Sign(tol1, xm - x);
                    }
                }
                else
                {
                    e = (x >= xm ? a - x : b - x);
                    d = CGOLD * e;
                }
                u = (Math.Abs(d) >= tol1 ? x + d : x + Standard_Real.Sign(tol1, d));
                OK = F.Value(u, out fu);
                if (!OK) return;
                if (fu <= fx)
                {
                    if (u >= x) a = x; else b = x;
                    SHFT(ref v, ref w, ref x, u);
                    SHFT(ref fv, ref fw, ref fx, fu);
                }
                else
                {
                    if (u < x) a = u; else b = u;
                    if (fu <= fw || w == x)
                    {
                        v = w;
                        w = u;
                        fv = fw;
                        fw = fu;
                    }
                    else if (fu <= fv || v == x || v == w)
                    {
                        v = u;
                        fv = fu;
                    }
                }
            }
            Done = false;
            return;
        }
        void SHFT(ref double theA, ref double theB,
                ref double theC, double theD)
        {
            theA = theB;
            theB = theC;
            theC = theD;
        }

        //! Returns true if the computations are successful, otherwise returns false.
        public bool IsDone()
        {
            return Done;

        }

        //! returns the location value of the minimum.
        //! Exception NotDone is raised if the minimum was not found.
        public double Location()
        {
            Exceptions.StdFail_NotDone_Raise_if(!Done, " ");
            return x;
        }


        protected double a;
        protected double b;
        protected double x;
        protected double fx;
        protected double fv;
        protected double fw;
        protected double XTol;
        protected double EPSZ;

        bool Done;
        int iter;
        int Itermax;
        bool myF;
    }
}
