using OCCPort.Common;

namespace TKMath
{
    //! This class implements the Brent method to find the root of a function
    //! located within two bounds. No knowledge of the derivative is required.
    public class math_BracketedRoot
    {


        //! The Brent method is used to find the root of the function F between
        //! the bounds Bound1 and Bound2 on the function F.
        //! If F(Bound1)*F(Bound2) >0 the Brent method fails.
        //! The tolerance required for the root is given by Tolerance.
        //! The solution is found when :
        //! abs(Xi - Xi-1) <= Tolerance;
        //! The maximum number of iterations allowed is given by NbIterations.
        public math_BracketedRoot(math_Function F, double Bound1, double Bound2, double Tolerance, int NbIterations = 100, double ZEPS = 1.0e-12)
        {

            double Fa, Fc, a, c = 0, d = 0, e = 0;
            double min1, min2, p, q, r, s, tol1, xm;

            a = Bound1;
            TheRoot = Bound2;
            F.Value(a, out Fa);
            F.Value(TheRoot, out TheError);
            if (Fa * TheError > 0.0) { Done = false; }
            else
            {
                Fc = TheError;
                for (NbIter = 1; NbIter <= NbIterations; NbIter++)
                {
                    if (TheError * Fc > 0.0)
                    {
                        c = a;      // rename a TheRoot c and adjust bounding interval d
                        Fc = Fa;
                        d = TheRoot - a;
                        e = d;
                    }
                    if (Math.Abs(Fc) < Math.Abs(Fa))
                    {
                        a = TheRoot;
                        TheRoot = c;
                        c = a;
                        Fa = TheError;
                        TheError = Fc;
                        Fc = Fa;
                    }
                    tol1 = 2.0 * ZEPS * Math.Abs(TheRoot) + 0.5 * Tolerance; // convergence check
                    xm = 0.5 * (c - TheRoot);
                    if (Math.Abs(xm) <= tol1 || TheError == 0.0 )
                    {
                        Done = true;
                        return;
                    }
                    if (Math.Abs(e) >= tol1 && Math.Abs(Fa) > Math.Abs(TheError))
                    {
                        s = TheError / Fa; // attempt inverse quadratic interpolation
                        if (a == c)
                        {
                            p = 2.0* xm * s;
                            q = 1.0 - s;
                        }
                        else
                        {
                            q = Fa / Fc;
                            r = TheError / Fc;
                            p = s * (2.0 * xm * q * (q - r) - (TheRoot - a) * (r - 1.0));
                            q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                        }
                        if (p > 0.0) { q = -q; } // check whether in bounds
                        p = Math.Abs(p);
                        min1 = 3.0 * xm * q - Math.Abs(tol1 * q);
                        min2 = Math.Abs(e * q);
                        if (2.0 * p < (min1 < min2 ? min1 : min2))
                        {
                            e = d;  // accept interpolation
                            d = p / q;
                        }
                        else
                        {
                            d = xm;  // interpolation failed,use bissection
                            e = d;
                        }
                    }
                    else
                    {   // bounds decreasing too slowly ,use bissection
                        d = xm;
                        e = d;
                    }
                    a = TheRoot;   // move last best guess to a
                    Fa = TheError;
                    if (Math.Abs(d) > tol1)
                    {  // evaluate new trial root
                        TheRoot += d;
                    }
                    else
                    {
                        TheRoot += (xm > 0.0 ? Math.Abs(tol1) : -Math.Abs(tol1));
                    }
                    F.Value(TheRoot, out TheError);
                }
                Done = false;
            }
        }


        //! Returns true if the computations are successful, otherwise returns false.
        public bool IsDone()
        {
            return Done;
        }


        //! returns the value of the function at the root.
        //! Exception NotDone is raised if the minimum was not found.
        public double Value()
        {
            Exceptions.StdFail_NotDone_Raise_if(!Done, " ");
            return TheError;
        }

        //! returns the value of the root.
        //! Exception NotDone is raised if the minimum was not found.
        public double Root()
        {
            Exceptions.StdFail_NotDone_Raise_if(!Done, " ");
            return TheRoot;
        }

        bool Done;
        double TheRoot;
        double TheError;
        int NbIter;
    }
}
