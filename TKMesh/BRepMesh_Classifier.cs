using TKernel;
using TKG3d;
using TKMath;

namespace TKMesh
{
    //! Auxiliary class intended for classification of points
    //! regarding internals of discrete face.
    public class BRepMesh_Classifier
    {
        public void RegisterWire(
  NCollection_Sequence<gp_Pnt2d> theWire,
   (double, double) theTolUV,
   (double, double) theRangeU,
   (double, double) theRangeV)
        {
            int aNbPnts = theWire.Length();
            if (aNbPnts < 2)
            {
                return;
            }

            // Accumulate angle
            TColgp_Array1OfPnt2d aPClass = new TColgp_Array1OfPnt2d(1, aNbPnts);
            double anAngle = 0.0;
            gp_Pnt2d p1 = theWire[1], p2 = theWire[2], p3;
            aPClass[1] = p1;
            aPClass[2] = p2;

            double aAngTol = Precision.Angular();
            double aSqConfusion =
             Precision.PConfusion() * Precision.PConfusion();

            for (int i = 1; i <= aNbPnts; i++)
            {
                int ii = i + 2;
                if (ii > aNbPnts)
                {
                    p3 = aPClass[ii - aNbPnts];
                }
                else
                {
                    p3 = theWire.Value(ii);
                    aPClass[ii] = p3;
                }

                gp_Vec2d A = new gp_Vec2d(p1, p2), B = new gp_Vec2d(p2, p3);
                if (A.SquareMagnitude() > aSqConfusion &&
                    B.SquareMagnitude() > aSqConfusion)
                {
                    double aCurAngle = A.Angle(B);
                    double aCurAngleAbs = Math.Abs(aCurAngle);
                    // Check if vectors are opposite
                    if (aCurAngleAbs > aAngTol && (Math.PI - aCurAngleAbs) > aAngTol)
                    {
                        anAngle += aCurAngle;
                        p1 = p2;
                    }
                }
                p2 = p3;
            }
            // Check for zero angle - treat self intersecting wire as outer
            if (Math.Abs(anAngle) < aAngTol)
                anAngle = 0.0;

            myTabClass.Append(new CSLib_Class2d(
                              aPClass, theTolUV.Item1, theTolUV.Item2,
                              theRangeU.Item1, theRangeV.Item1,
                              theRangeU.Item2, theRangeV.Item2));

            myTabOrient.Append(!(anAngle < 0.0));
        }

        //! Performs classification of the given point regarding to face internals.
        //! @param thePoint Point in parametric space to be classified.
        //! @return TopAbs_IN if point lies within face boundaries and TopAbs_OUT elsewhere.
        public TopAbs_State Perform(gp_Pnt2d thePoint)
        {
            bool isOut = false;
            int aNb = myTabClass.Length();

            for (int i = 0; i < aNb; i++)
            {
                int aCur = myTabClass[i].SiDans(thePoint);
                if (aCur == 0)
                {
                    // Point is ON, but mark it as OUT
                    isOut = true;
                }
                else
                {
                    isOut = myTabOrient[i] ? (aCur == -1) : (aCur == 1);
                }

                if (isOut)
                {
                    return TopAbs_State.TopAbs_OUT;
                }
            }

            return TopAbs_State.TopAbs_IN;
        }
        NCollection_Vector<CSLib_Class2d> myTabClass = new NCollection_Vector<CSLib_Class2d>();
        VectorOfBoolean myTabOrient = new VectorOfBoolean();


    }
}

