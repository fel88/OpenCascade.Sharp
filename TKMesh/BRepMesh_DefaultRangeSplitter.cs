using OCCPort;
using TKBRep;
using TKG3d;
using TKMath;

namespace TKMesh
{
    //! Default tool to define range of discrete face model and 
    //! obtain grid points distributed within this range.
    public class BRepMesh_DefaultRangeSplitter : AbstractRangeSplitter
    {
        public BRepMesh_DefaultRangeSplitter()
        {
            myIsValid = true;
        }
        //! Returns face model.
        public  IFaceHandle GetDFace() 
        {
    return myDFace;
  }
        (double, double) myRangeU;
        (double, double) myRangeV;
        (double, double) myDelta;
        (double, double) myTolerance;
        bool myIsValid;
        public override (double, double) GetToleranceUV()
        {
            return myTolerance;
        }
        public override bool IsValid()
        {
            return myIsValid;
        }

        public override void AddPoint(gp_Pnt2d thePoint)
        {
            myRangeU.Item1 = Math.Min(thePoint.X(), myRangeU.Item1);
            myRangeU.Item2 = Math.Max(thePoint.X(), myRangeU.Item2);
            myRangeV.Item1 = Math.Min(thePoint.Y(), myRangeV.Item1);
            myRangeV.Item2 = Math.Max(thePoint.Y(), myRangeV.Item2);
        }

        void updateRange(
   double theGeomFirst,
   double theGeomLast,
   bool isPeriodic,
  ref double theDiscreteFirst,
 ref double theDiscreteLast)
        {
            if (theDiscreteFirst < theGeomFirst ||
                theDiscreteLast > theGeomLast)
            {
                if (isPeriodic)
                {
                    if ((theDiscreteLast - theDiscreteFirst) > (theGeomLast - theGeomFirst))
                    {
                        theDiscreteLast = theDiscreteFirst + (theGeomLast - theGeomFirst);
                    }
                }
                else
                {
                    if ((theDiscreteFirst < theGeomLast) && (theDiscreteLast > theGeomFirst))
                    {
                        //Protection against the faces whose pcurve is out of the surface's domain
                        //(see issue #23675 and test cases "bugs iges buc60820*")

                        if (theGeomFirst > theDiscreteFirst)
                        {
                            theDiscreteFirst = theGeomFirst;
                        }

                        if (theGeomLast < theDiscreteLast)
                        {
                            theDiscreteLast = theGeomLast;
                        }
                    }
                }
            }
        }

        //=======================================================================
        // Function: AdjustRange
        // Purpose : 
        //=======================================================================
        public override void AdjustRange()
        {
            BRepAdaptor_Surface aSurface = GetSurface();
            updateRange(aSurface.FirstUParameter(), aSurface.LastUParameter(),
                        aSurface.IsUPeriodic(), ref myRangeU.Item1, ref myRangeU.Item2);

            if (myRangeU.Item2 < myRangeU.Item1)
            {
                myIsValid = false;
                return;
            }

            updateRange(aSurface.FirstVParameter(), aSurface.LastVParameter(),
                        aSurface.IsVPeriodic(), ref myRangeV.Item1, ref myRangeV.Item2);

            if (myRangeV.Item2 < myRangeV.Item1)
            {
                myIsValid = false;
                return;
            }

            var aLengthU = computeLengthU();
            var aLengthV = computeLengthV();
            myIsValid = aLengthU > Precision.PConfusion() && aLengthV > Precision.PConfusion();

            if (myIsValid)
            {
                computeTolerance(aLengthU, aLengthV);
                computeDelta(aLengthU, aLengthV);
            }
        }
        double computeLengthV()
        {
            double longv = 0.0;
            gp_Pnt P11 = new gp_Pnt(),
                P12 = new gp_Pnt(),
                P21 = new gp_Pnt(),
                P22 = new gp_Pnt(),
                P31 = new gp_Pnt(),
                P32 = new gp_Pnt();

            double dv = 0.05 * (myRangeV.Item2 - myRangeV.Item1);
            double dfuave = 0.5 * (myRangeU.Item2 + myRangeU.Item1);
            double dfvcur;
            int i1;

            BRepAdaptor_Surface gFace = GetSurface();
            gFace.D0(myRangeU.Item1, myRangeV.Item1, ref P11);
            gFace.D0(dfuave, myRangeV.Item1, ref P21);
            gFace.D0(myRangeU.Item2, myRangeV.Item1, ref P31);
            for (i1 = 1, dfvcur = myRangeV.Item1 + dv; i1 <= 20; i1++, dfvcur += dv)
            {
                gFace.D0(myRangeU.Item1, dfvcur, ref P12);
                gFace.D0(dfuave, dfvcur, ref P22);
                gFace.D0(myRangeU.Item2, dfvcur, ref P32);
                longv += (P11.Distance(P12) + P21.Distance(P22) + P31.Distance(P32));
                P11 = P12;
                P21 = P22;
                P31 = P32;
            }

            return longv / 3.0;
        }

        void computeTolerance(
  double theLenU,
  double theLenV)
        {
            double aDiffU = myRangeU.Item2 - myRangeU.Item1;
            double aDiffV = myRangeV.Item2 - myRangeV.Item1;

            // Slightly increase exact resolution so to cover links with approximate 
            // length equal to resolution itself on sub-resolution differences.
            double aTolerance = BRep_Tool.Tolerance(myDFace.GetFace());
            Adaptor3d_Surface aSurface = GetSurface().Surface();
            double aResU = aSurface.UResolution(aTolerance) * 1.1;
            double aResV = aSurface.VResolution(aTolerance) * 1.1;

            double aDeflectionUV = 1e-05;
            myTolerance.Item1 = Math.Max(Math.Min(aDeflectionUV, aResU), 1e-7 * aDiffU);
            myTolerance.Item2 = Math.Max(Math.Min(aDeflectionUV, aResV), 1e-7 * aDiffV);
        }

        void computeDelta(
   double theLengthU,
   double theLengthV)
        {
            double aDiffU = myRangeU.Item2 - myRangeU.Item1;
            double aDiffV = myRangeV.Item2 - myRangeV.Item1;

            myDelta.Item1 = aDiffU / (theLengthU < myTolerance.Item1 ? 1.0 : theLengthU);
            myDelta.Item2 = aDiffV / (theLengthV < myTolerance.Item2 ? 1.0 : theLengthV);
        }

        double computeLengthU()
        {
            double longu = 0.0;
            gp_Pnt P11 = new gp_Pnt(), P12 = new gp_Pnt(), P21 = new gp_Pnt(), P22 = new gp_Pnt(), P31 = new gp_Pnt(), P32 = new gp_Pnt();

            double du = 0.05 * (myRangeU.Item2 - myRangeU.Item1);
            double dfvave = 0.5 * (myRangeV.Item2 + myRangeV.Item1);
            double dfucur;
            int i1;

            BRepAdaptor_Surface gFace = GetSurface();
            gFace.D0(myRangeU.Item1, myRangeV.Item1, ref P11);
            gFace.D0(myRangeU.Item1, dfvave, ref P21);
            gFace.D0(myRangeU.Item1, myRangeV.Item2, ref P31);
            for (i1 = 1, dfucur = myRangeU.Item1 + du; i1 <= 20; i1++, dfucur += du)
            {
                gFace.D0(dfucur, myRangeV.Item1, ref P12);
                gFace.D0(dfucur, dfvave, ref P22);
                gFace.D0(dfucur, myRangeV.Item2, ref P32);
                longu += (P11.Distance(P12) + P21.Distance(P22) + P31.Distance(P32));
                P11 = P12;
                P21 = P22;
                P31 = P32;
            }

            return longu / 3.0;
        }

        //! Returns point in 3d space corresponded to the given 
        //! point defined in parameteric space of surface.
        public override gp_Pnt Point(gp_Pnt2d thePoint2d)
        {
            return GetSurface().Value(thePoint2d.X(), thePoint2d.Y());
        }

        //! Returns surface.
        public BRepAdaptor_Surface GetSurface()
        {
            return myDFace.GetSurface();
        }
        //! Returns U range.
        public override (double, double) GetRangeU()
        {
            return myRangeU;
        }
        //! Resets this splitter. Must be called before first use.
        public override void Reset(IMeshData_Face theDFace,
                                     IMeshTools_Parameters theParameters)
        {
            myDFace = theDFace;
            myRangeU.Item1 = myRangeV.Item1 = 1e+100;
            myRangeU.Item2 = myRangeV.Item2 = -1e+100;
            myDelta.Item1 = myDelta.Item2 = 1.0;
            myTolerance.Item1 = myTolerance.Item2 = Precision.Confusion();
        }
        IMeshData_Face myDFace;

        public override ListOfPnt2d GenerateSurfaceNodes(
  IMeshTools_Parameters theParameters)
        {
            return null;
        }
        //! Returns V range.
        public override (double, double) GetRangeV()
        {
            return myRangeV;
        }

        //! Returns delta.
        public override (double, double) GetDelta()
        {
            return myDelta;
        }

        public override gp_Pnt2d Scale(gp_Pnt2d thePoint, bool isToFaceBasis)
        {
            return isToFaceBasis ?
    new gp_Pnt2d((thePoint.X() - myRangeU.Item1) / myDelta.Item1,
              (thePoint.Y() - myRangeV.Item1) / myDelta.Item2) :
    new gp_Pnt2d(thePoint.X() * myDelta.Item1 + myRangeU.Item1,
              thePoint.Y() * myDelta.Item2 + myRangeV.Item1);
        }
    }


}



