using OCCPort.Common;
using TKernel;
using TKMath;

namespace TKG3d
{
    //! This class implements an algorithm which finds all the real roots of
    //! a function with derivative within a given range.
    //! Knowledge of the derivative is required.
    public class math_FunctionRoots
    {

        bool Done;
        bool AllNull;
        TColStd_SequenceOfReal Sol;
        TColStd_SequenceOfInteger NbStateSol;

        //! Calculates all the real roots of a function F-K within the range
        //! A..B. without conditions on A and B
        //! A solution X is found when
        //! abs(Xi - Xi-1) <= Epsx and abs(F(Xi)-K) <= EpsF.
        //! The function is considered as null between A and B if
        //! abs(F-K) <= EpsNull within this range.
        public math_FunctionRoots(Adaptor3d_InterFunc F, double A, double B, int NbSample, double _EpsX, double EpsF, double EpsNull, double K)
        {


            TColStd_SequenceOfReal StaticSol;

            Sol.Clear();
            NbStateSol.Clear();

            {
                Done = true;
                double X0 = A;
                double XN = B;
                int N = NbSample;
                //-- ------------------------------------------------------------
                //-- Verifications de bas niveau 
                if (B < A)
                {
                    X0 = B;
                    XN = A;
                }
                N *= 2;
                if (N < 20)
                {
                    N = 20;
                }
                //--  On teste si EpsX est trop petit (ie : U+Nn*EpsX == U ) 
                double EpsX = _EpsX;
                double DeltaU = Math.Abs(X0) + Math.Abs(XN);
                double NEpsX = 0.0000000001 * DeltaU;
                if (EpsX < NEpsX)
                {
                    EpsX = NEpsX;
                }

                //-- recherche d un intervalle ou F(xi) et F(xj) sont de signes differents
                //-- A .............................................................. B
                //-- X0   X1   X2 ........................................  Xn-1      Xn
                int i;
                double X = X0;
                bool Ok;
                double dx = (XN - X0) / N;
                TColStd_Array1OfReal ptrval = new(0, N);
                int Nvalid = -1;
                double aux = 0;
                for (i = 0; i <= N; i++, X += dx)
                {
                    if (X > XN) X = XN;
                    Ok = F.Value(X, out aux);
                    if (Ok) ptrval[(++Nvalid)] = aux - K;
                    //      ptrval(i)-=K;
                }
                //-- Toute la fonction est nulle ? 

                if (Nvalid < N)
                {
                    Done = false;
                    return;
                }

                AllNull = true;
                //    for(i=0;AllNull && i<=N;i++) { 
                for (i = 0; AllNull && i <= N; i++)
                {
                    if (ptrval[i] > EpsNull || ptrval[i] < -EpsNull)
                    {
                        AllNull = false;
                    }
                }
                if (AllNull)
                {
                    //-- tous les points echantillons sont dans la tolerance 

                }
                else
                {
                    //-- Il y a des points hors tolerance 
                    //-- on detecte les changements de signes STRICTS 
                    int ip1;
                    //      Standard_Boolean chgtsign=Standard_False;
                    double tol = EpsX;
                    double X2;
                    for (i = 0, ip1 = 1, X = X0; i < N; i++, ip1++, X += dx)
                    {
                        X2 = X + dx;
                        if (X2 > XN) X2 = XN;
                        if (ptrval[(i)] < 0.0)
                        {
                            if (ptrval[(ip1)] > 0.0)
                            {
                                //-- --------------------------------------------------
                                //-- changement de signe dans Xi Xi+1
                                Solve(F, K, X, ptrval[(i)], X2, ptrval[(ip1)], tol, NEpsX, Sol, NbStateSol);
                            }
                        }
                        else
                        {
                            if (ptrval[(ip1)] < 0.0)
                            {
                                //-- --------------------------------------------------
                                //-- changement de signe dans Xi Xi+1
                                Solve(F, K, X, ptrval[(i)], X2, ptrval[(ip1)], tol, NEpsX, Sol, NbStateSol);
                            }
                        }
                    }
                    //-- On detecte les cas ou la fct s annule sur des Xi et est 
                    //-- non nulle au voisinage de Xi
                    //--
                    //-- On prend 2 points u0,u1 au voisinage de Xi
                    //-- Si (F(u0)-K)*(F(u1)-K) <0   on lance une recherche 
                    //-- Sinon si (F(u0)-K)*(F(u1)-K) !=0 on insere le point X
                    for (i = 0; i <= N; i++)
                    {
                        if (ptrval[(i)] == 0)
                        {
                            //	  Standard_Real Val,Deriv;
                            X = X0 + i * dx;
                            if (X > XN) X = XN;
                            double u0, u1;
                            u0 = dx * 0.5; u1 = X + u0; u0 += X;
                            if (u0 < X0) u0 = X0;
                            if (u0 > XN) u0 = XN;
                            if (u1 < X0) u1 = X0;
                            if (u1 > XN) u1 = XN;

                            double y0, y1;
                            F.Value(u0, out y0); y0 -= K;
                            F.Value(u1, out y1); y1 -= K;
                            if (y0 * y1 < 0.0)
                            {
                                Solve(F, K, u0, y0, u1, y1, tol, NEpsX, Sol, NbStateSol);
                            }
                            else
                            {
                                if (y0 != 0.0 || y1 != 0.0)
                                {
                                    AppendRoot(Sol, NbStateSol, X, F, K, NEpsX);
                                }
                            }
                        }
                    }
                    //-- --------------------------------------------------------------------------------
                    //-- Il faut traiter differement le cas des points en bout : 
                    if (ptrval[(0)] <= EpsF && ptrval[(0)] >= -EpsF)
                    {
                        AppendRoot(Sol, NbStateSol, X0, F, K, NEpsX);
                    }
                    if (ptrval[(N)] <= EpsF && ptrval[(N)] >= -EpsF)
                    {
                        AppendRoot(Sol, NbStateSol, XN, F, K, NEpsX);
                    }

                    //-- --------------------------------------------------------------------------------
                    //-- --------------------------------------------------------------------------------
                    //-- On detecte les zones ou on a sur les points echantillons un minimum avec f(x)>0
                    //--                                                          un maximum avec f(x)<0
                    //-- On reprend une discretisation plus fine au voisinage de ces extremums
                    //--
                    //-- Recherche d un minima positif
                    double xm, ym, dym, xm1, xp1;
                    double majdx = 5.0 * dx;
                    bool Rediscr;
                    //      Standard_Real ptrvalbis[MAXBIS];
                    int im1 = 0;
                    ip1 = 2;
                    for (i = 1, xm = X0 + dx; i < N; xm += dx, i++, im1++, ip1++)
                    {
                        Rediscr = false;
                        if (xm > XN) xm = XN;
                        if (ptrval[(i)] > 0.0)
                        {
                            if ((ptrval[(im1)] > ptrval[(i)]) && (ptrval[(ip1)] > ptrval[(i)]))
                            {
                                //-- Peut on traverser l axe Ox 
                                //-- -------------- Estimation a partir de Xim1
                                xm1 = xm - dx;
                                if (xm1 < X0) xm1 = X0;
                                F.Values(xm1, out ym, out dym); ym -= K;
                                if (dym < -1e-10 || dym > 1e-10)
                                {  // normalement dym < 0 
                                    double t = ym / dym; //-- t=xm-x* = (ym-0)/dym
                                    if (t < majdx && t > -majdx)
                                    {
                                        Rediscr = true;
                                    }
                                }
                                //-- -------------- Estimation a partir de Xip1
                                if (Rediscr == false)
                                {
                                    xp1 = xm + dx;
                                    if (xp1 > XN) xp1 = XN;
                                    F.Values(xp1, out ym, out dym); ym -= K;
                                    if (dym < -1e-10 || dym > 1e-10)
                                    {  // normalement dym > 0 
                                        double t = ym / dym; //-- t=xm-x* = (ym-0)/dym
                                        if (t < majdx && t > -majdx)
                                        {
                                            Rediscr = true;
                                        }
                                    }
                                }
                            }
                        }
                        else if (ptrval[i] < 0.0)
                        {
                            if ((ptrval[im1] < ptrval[i]) && (ptrval[ip1] < ptrval[i]))
                            {
                                //-- Peut on traverser l axe Ox 
                                //-- -------------- Estimation a partir de Xim1
                                xm1 = xm - dx;
                                if (xm1 < X0) xm1 = X0;
                                F.Values(xm1, out ym, out dym); ym -= K;
                                if (dym > 1e-10 || dym < -1e-10)
                                {  // normalement dym > 0 
                                    double t = ym / dym; //-- t=xm-x* = (ym-0)/dym
                                    if (t < majdx && t > -majdx)
                                    {
                                        Rediscr = true;
                                    }
                                }
                                //-- -------------- Estimation a partir de Xim1
                                if (Rediscr == false)
                                {
                                    xm1 = xm - dx;
                                    if (xm1 < X0) xm1 = X0;
                                    F.Values(xm1, out ym, out dym); ym -= K;
                                    if (dym > 1e-10 || dym < -1e-10)
                                    {  // normalement dym < 0 
                                        double t = ym / dym; //-- t=xm-x* = (ym-0)/dym
                                        if (t < majdx && t > -majdx)
                                        {
                                            Rediscr = true;
                                        }
                                    }
                                }
                            }
                        }
                        if (Rediscr)
                        {
                            double x0 = xm - dx;
                            double x3 = xm + dx;
                            if (x0 < X0) x0 = X0;
                            if (x3 > XN) x3 = XN;
                            double aSolX1 = 0.0, aSolX2 = 0.0;
                            double aVal1 = 0.0, aVal2 = 0.0;
                            double aDer1 = 0.0, aDer2 = 0.0;
                            bool isSol1 = false;
                            bool isSol2 = false;
                            //-- ----------------------------------------------------
                            //-- Find minimum of the function |F| between x0 and x3
                            //-- by searching for the zero of the function derivative
                            DerivFunction aDerF = new(F);
                            math_BracketedRoot aBR = new(aDerF, x0, x3, _EpsX);
                            if (aBR.IsDone())
                            {
                                aSolX1 = aBR.Root();
                                F.Value(aSolX1, out aVal1);
                                aVal1 = Math.Abs(aVal1);
                                if (aVal1 < EpsF)
                                {
                                    isSol1 = true;
                                    aDer1 = aBR.Value();
                                }
                            }

                            //-- --------------------------------------------------
                            //-- On recherche un extrema entre x0 et x3
                            //-- x1 et x2 sont tels que x0<x1<x2<x3 
                            //-- et |f(x0)| > |f(x1)|   et |f(x3)| > |f(x2)|
                            //--
                            //-- En entree : a=xm-dx  b=xm c=xm+dx
                            double x1, x2, f0, f3;
                            double R = 0.61803399;
                            double C = 1.0 - R;
                            double tolCR = NEpsX * 10.0;
                            f0 = ptrval[(im1)];
                            f3 = ptrval[(ip1)];
                            bool recherche_minimum = (f0 > 0.0);

                            if (Math.Abs(x3 - xm) > Math.Abs(x0 - xm)) { x1 = xm; x2 = xm + C * (x3 - xm); }
                            else { x2 = xm; x1 = xm - C * (xm - x0); }
                            double f1, f2;
                            F.Value(x1, out f1); f1 -= K;
                            F.Value(x2, out f2); f2 -= K;
                            //-- printf("\n *************** RECHERCHE MINIMUM **********\n");
                            double tolX = 0.001 * NEpsX;
                            while (Math.Abs(x3 - x0) > tolCR * (Math.Abs(x1) + Math.Abs(x2)) && (Math.Abs(x1 - x2) > tolX))
                            {
                                //-- printf("\n (%10.5g,%10.5g) (%10.5g,%10.5g) (%10.5g,%10.5g) (%10.5g,%10.5g) ", 
                                //--    x0,f0,x1,f1,x2,f2,x3,f3);
                                if (recherche_minimum)
                                {
                                    if (f2 < f1)
                                    {
                                        x0 = x1; x1 = x2; x2 = R * x1 + C * x3;
                                        f0 = f1; f1 = f2; F.Value(x2, out f2); f2 -= K;
                                    }
                                    else
                                    {
                                        x3 = x2; x2 = x1; x1 = R * x2 + C * x0;
                                        f3 = f2; f2 = f1; F.Value(x1, out f1); f1 -= K;
                                    }
                                }
                                else
                                {
                                    if (f2 > f1)
                                    {
                                        x0 = x1; x1 = x2; x2 = R * x1 + C * x3;
                                        f0 = f1; f1 = f2; F.Value(x2, out f2); f2 -= K;
                                    }
                                    else
                                    {
                                        x3 = x2; x2 = x1; x1 = R * x2 + C * x0;
                                        f3 = f2; f2 = f1; F.Value(x1, out f1); f1 -= K;
                                    }
                                }
                                //-- On ne fait pas que chercher des extremas. Il faut verifier 
                                //-- si on ne tombe pas sur une racine 
                                if (f1 * f0 < 0.0)
                                {
                                    //-- printf("\n Recherche entre  (%10.5g,%10.5g) (%10.5g,%10.5g) ",x0,f0,x1,f1);
                                    Solve(F, K, x0, f0, x1, f1, tol, NEpsX, Sol, NbStateSol);
                                }
                                if (f2 * f3 < 0.0)
                                {
                                    //-- printf("\n Recherche entre  (%10.5g,%10.5g) (%10.5g,%10.5g) ",x2,f2,x3,f3);
                                    Solve(F, K, x2, f2, x3, f3, tol, NEpsX, Sol, NbStateSol);
                                }
                            }
                            if ((recherche_minimum && f1 < f2) || (!recherche_minimum && f1 > f2))
                            {
                                //-- x1,f(x1) minimum
                                if (Math.Abs(f1) < EpsF)
                                {
                                    isSol2 = true;
                                    aSolX2 = x1;
                                    aVal2 = Math.Abs(f1);
                                }
                            }
                            else
                            {
                                //-- x2.f(x2) minimum
                                if (Math.Abs(f2) < EpsF)
                                {
                                    isSol2 = true;
                                    aSolX2 = x2;
                                    aVal2 = Math.Abs(f2);
                                }
                            }
                            // Choose the best solution between aSolX1, aSolX2
                            if (isSol1 && isSol2)
                            {
                                if (aVal2 - aVal1 > EpsF)
                                    AppendRoot(Sol, NbStateSol, aSolX1, F, K, NEpsX);
                                else if (aVal1 - aVal2 > EpsF)
                                    AppendRoot(Sol, NbStateSol, aSolX2, F, K, NEpsX);
                                else
                                {
                                    aDer1 = Math.Abs(aDer1);
                                    F.Derivative(aSolX2, out aDer2);
                                    aDer2 = Math.Abs(aDer2);
                                    if (aDer1 < aDer2)
                                        AppendRoot(Sol, NbStateSol, aSolX1, F, K, NEpsX);
                                    else
                                        AppendRoot(Sol, NbStateSol, aSolX2, F, K, NEpsX);
                                }
                            }
                            else if (isSol1)
                                AppendRoot(Sol, NbStateSol, aSolX1, F, K, NEpsX);
                            else if (isSol2)
                                AppendRoot(Sol, NbStateSol, aSolX2, F, K, NEpsX);
                        } //-- Recherche d un extrema    
                    } //-- for     
                }

            }
        }

        const int ITMAX = 100;
        const double EPS = 1e-14;
        const double EPSEPS = 2e-14;
        const int MAXBIS = 100;

        static void Solve(math_FunctionWithDerivative F,
          double K,
          double x1,
          double y1,
          double x2,
          double y2,
          double tol,
          double dX,
           TColStd_SequenceOfReal Sol,
           TColStd_SequenceOfInteger NbStateSol)
        {


            int iter = 0;
            double tols2 = 0.5 * tol;
            double a, b, c, d = 0, e = 0, fa, fb, fc, p, q, r, s, tol1, xm, min1, min2;
            a = x1; b = c = x2; fa = y1; fb = fc = y2;
            for (iter = 1; iter <= ITMAX; iter++)
            {
                if ((fb > 0.0 && fc > 0.0) || (fb < 0.0 && fc < 0.0))
                {
                    c = a; fc = fa; e = d = b - a;
                }
                if (Math.Abs(fc) < Math.Abs(fb))
                {
                    a = b; b = c; c = a; fa = fb; fb = fc; fc = fa;
                }
                tol1 = EPSEPS * Math.Abs(b) + tols2;
                xm = 0.5 * (c - b);
                if (Math.Abs(xm) < tol1 || fb == 0)
                {
                    //-- On tente une iteration de newton
                    double Xp, Yp, Dp;
                    int itern = 5;
                    bool Ok;
                    Xp = b;
                    do
                    {
                        Ok = F.Values(Xp, out Yp, out Dp);
                        if (Ok)
                        {
                            Ok = false;
                            if (Dp > 1e-10 || Dp < -1e-10)
                            {
                                Xp = Xp - (Yp - K) / Dp;
                            }
                            if (Xp <= x2 && Xp >= x1)
                            {
                                F.Value(Xp, out Yp); Yp -= K;
                                if (Math.Abs(Yp) < Math.Abs(fb))
                                {
                                    b = Xp;
                                    fb = Yp;
                                    Ok = true;
                                }
                            }
                        }
                    }
                    while (Ok && --itern >= 0);

                    AppendRoot(Sol, NbStateSol, b, F, K, dX);
                    return;
                }
                if (Math.Abs(e) >= tol1 && Math.Abs(fa) > Math.Abs(fb))
                {
                    s = fb / fa;
                    if (a == c)
                    {
                        p = xm * s; p += p;
                        q = 1.0 - s;
                    }
                    else
                    {
                        q = fa / fc;
                        r = fb / fc;
                        p = s * ((xm + xm) * q * (q - r) - (b - a) * (r - 1.0));
                        q = (q - 1.0) * (r - 1.0) * (s - 1.0);
                    }
                    if (p > 0.0)
                    {
                        q = -q;
                    }
                    p = Math.Abs(p);
                    min1 = 3.0 * xm * q - Math.Abs(tol1 * q);
                    min2 = Math.Abs(e * q);
                    if ((p + p) < ((min1 < min2) ? min1 : min2))
                    {
                        e = d;
                        d = p / q;
                    }
                    else
                    {
                        d = xm;
                        e = d;
                    }
                }
                else
                {
                    d = xm;
                    e = d;
                }
                a = b;
                fa = fb;
                if (Math.Abs(d) > tol1)
                {
                    b += d;
                }
                else
                {
                    if (xm >= 0) b += Math.Abs(tol1);
                    else b += -Math.Abs(tol1);
                }
                F.Value(b, out fb);
                fb -= K;
            }
        }


        static void AppendRoot(TColStd_SequenceOfReal Sol,
            TColStd_SequenceOfInteger NbStateSol,

             double X,
            math_FunctionWithDerivative F,
             //			const Standard_Real K,
             double _,

             double dX)
        {

            int n = Sol.Length();
            double t;


            if (n == 0)
            {
                Sol.Append(X);
                F.Value(X, out t);
                NbStateSol.Append(F.GetStateNumber());
            }
            else
            {
                int i = 1;
                int pl = n + 1;
                while (i <= n)
                {
                    t = Sol.Value(i);
                    if (t >= X)
                    {
                        pl = i;
                        i = n;
                    }
                    if (Math.Abs(X - t) <= dX)
                    {
                        pl = 0;
                        i = n;
                    }
                    i++;
                } //-- while
                if (pl > n)
                {
                    Sol.Append(X);
                    F.Value(X, out t);
                    NbStateSol.Append(F.GetStateNumber());
                }
                else if (pl > 0)
                {
                    Sol.InsertBefore(pl, X);
                    F.Value(X, out t);
                    NbStateSol.InsertBefore(pl, F.GetStateNumber());
                }
            }
        }

        //! returns true if the function is considered as null between A and B.
        //! Exceptions
        //! StdFail_NotDone if the algorithm fails (and IsDone returns false).
        public bool IsAllNull()
        {
            Exceptions.StdFail_NotDone_Raise_if(!Done, " ");
            return AllNull;
        }

        //! Returns the number of solutions found.
        //! Exceptions
        //! StdFail_NotDone if the algorithm fails (and IsDone returns false).
        public int NbSolutions()
        {
            Exceptions.StdFail_NotDone_Raise_if(!Done, " ");
            return Sol.Length();
        }
        public double Value(int Nieme)
        {

            Exceptions.StdFail_NotDone_Raise_if(!Done, " ");
            return Sol.Value(Nieme);

        }
        public bool IsDone() { return Done; }

    }
}
