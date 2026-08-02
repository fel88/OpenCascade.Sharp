using OCCPort.Common;
using System.Collections.Generic;
using TKernel;
using TKGeomBase;
using TKMath;

namespace TKGeomAlgo
{
    //! The Hatcher   is  an algorithm  to   compute cross
    //! hatchings in a 2d plane. It is mainly dedicated to
    //! display purpose.
    //!
    //! Computing cross hatchings is a 3 steps process :
    //!
    //! 1.  The users stores in the   Hatcher a set  of 2d
    //! lines to   be  trimmed. Methods   in  the  "Lines"
    //! category.
    //!
    //! 2.  The user trims the lines with a boundary.  The
    //! inside of a boundary is on the left side.  Methods
    //! in the "Trimming" category.
    //!
    //! 3. The user reads  back the trimmed lines. Methods
    //! in the "Results" category.
    //!
    //! The result is a set of parameter intervals  on the
    //! line. The first  parameter of an  Interval may  be
    //! RealFirst() and the last may be RealLast().
    //!
    //! A line can be a line parallel to the axis (X  or Y
    //! line or a 2D line.
    //!
    //! The Hatcher has two modes :
    //!
    //! *  The "Oriented" mode,  where the  orientation of
    //! the trimming curves is  considered. The  hatch are
    //! kept on  the left of  the  trimming curve. In this
    //! mode infinite hatch can be computed.
    //!
    //! *   The "UnOriented"  mode,  where  the  hatch are
    //! always finite.
    public class Hatch_Hatcher
    {
        //! Returns a empty  hatcher.  <Tol> is the  tolerance
        //! for intersections.
        public  Hatch_Hatcher( double  Tol,  bool Oriented = true)
        {
            myToler = (Tol);
            myOrient= (Oriented);
        }


        //! Add an infinite line   parallel to the Y-axis   at
        //! abciss <X>.
        public void AddXLine(double X)
        {
            gp_Pnt2d O = new(X, 0);
            gp_Dir2d D = new(0, 1);
            gp_Lin2d L = new(O, D);
            AddLine(L, Hatch_LineForm.Hatch_XLINE);
        }

        //! Returns  True if the  line   of  index <I>  has  a
        //! constant X value.
        public bool IsXLine(int I)
        {
            return LineForm(I) == Hatch_LineForm.Hatch_XLINE;
        }
        public void Trim
       (gp_Lin2d L,
         double Start,
         double End,
         int Index)
        {
            IntAna2d_IntPoint Pinter;
            IntAna2d_AnaIntersection Inters = new IntAna2d_AnaIntersection();
            int iLine;
            for (iLine = 1; iLine <= myLines.Length(); iLine++)
            {
                Inters.Perform(myLines[(iLine)].myLin, L);
                if (Inters.IsDone())
                {
                    if (!Inters.IdenticalElements() && !Inters.ParallelElements())
                    {
                        // we have got something
                        Pinter = Inters.Point(1);
                        double linePar = Pinter.ParamOnSecond();
                        if (linePar - Start < -myToler) continue;
                        if (linePar - End > myToler) continue;
                        double norm = L.Direction() ^ myLines[(iLine)].myLin.Direction();
                        if (linePar - Start < myToler)
                        {
                            // on the limit of the trimming segment
                            // accept if the other extremity is on the left
                            if (norm < 0) continue;
                        }
                        if (linePar - End > -myToler)
                        {
                            // on the limit of the trimming segment
                            // accept if the other extremity is on the left
                            if (norm > 0) continue;
                        }
                        // insert the parameter
                        myLines[(iLine)].AddIntersection(Pinter.ParamOnFirst(),
                                        norm > 0,
                                        Index,
                                        Pinter.ParamOnSecond(),
                                        myToler);
                    }
                }
            }
        }

        public int NbIntervals(int I)
        {
            int l = myLines[(I)].myInters.Length();
            if (l == 0)
                l = myOrient ? 1 : 0;
            else
            {
                l = l / 2;
                if (myOrient) if (!myLines[(I)].myInters[(1)].myStart) l++;
            }
            return l;
        }

        //! Trims the line at  intersection with  the oriented
        //! segment P1,P2.
        public void Trim(gp_Pnt2d P1, gp_Pnt2d P2, int Index = 0)
        {
            gp_Vec2d V = new(P1, P2);
            if (Math.Abs(V.X()) > .9 * Standard_Real.RealLast())
                V.Multiply(1 / V.X());
            else if (Math.Abs(V.Y()) > .9 * Standard_Real.RealLast())
                V.Multiply(1 / V.Y());
            if (V.Magnitude() > myToler)
            {
                gp_Dir2d D = new(V);
                gp_Lin2d L = new(P1, D);
                Trim(L, 0, P1.Distance(P2), Index);
            }
        }

        //! Returns  the type of the  line   of  index <I>.
        public Hatch_LineForm LineForm(int I)
        {
            return myLines[I].myForm;

        }
        public double Coordinate(int I)
        {
            switch (myLines[(I)].myForm)
            {

                case Hatch_LineForm.Hatch_XLINE:
                    return myLines[I].myLin.Location().X();

                case Hatch_LineForm.Hatch_YLINE:
                    return myLines[I].myLin.Location().Y();

                case Hatch_LineForm.Hatch_ANYLINE:
                    throw new Standard_OutOfRange("Hatcher : not an X or Y line");
            }

            return 0.0;
        }
        public int NbLines()
        {
            return myLines.Length();
        }
        public void AddLine(gp_Lin2d L, Hatch_LineForm T)
        {
            Hatch_Line HL = new(L, T);
            myLines.Append(HL);
        }

        //! Returns the last   parameter of  interval <J>  on
        //! line  <I>.
        public double End(int I, int J)
        {
            if (myLines[I].myInters.IsEmpty())
            {
                if (J != 1 || !myOrient) throw new Standard_OutOfRange();
                return Standard_Real.RealLast();
            }
            else
            {
                int jj = 2 * J;
                if (!myLines[I].myInters[(1)].myStart && myOrient) jj--;
                if (jj > myLines[I].myInters.Length()) return Standard_Real.RealLast();
                return myLines[I].myInters[(jj)].myPar1;
            }
        }

        //! Returns the first   parameter of  interval <J>  on
        //! line  <I>.
        public double Start(int I, int J)
        {
            if (myLines[I].myInters.IsEmpty())
            {
                if (J != 1 || !myOrient) throw new Standard_OutOfRange();
                return Standard_Real.RealFirst();
            }
            else
            {
                int jj = 2 * J - 1;
                if (!myLines[I].myInters[1].myStart && myOrient) jj--;
                if (jj == 0) return Standard_Real.RealFirst();
                return myLines[I].myInters[jj].myPar1;
            }
        }

        double myToler;
        NCollection_Sequence<Hatch_Line> myLines = new NCollection_Sequence<Hatch_Line>();
        bool myOrient;

        //! Add an infinite line   parallel to the X-axis   at
        //! ordinate <Y>.
        public void AddYLine(double Y)
        {

        }

    }
}
