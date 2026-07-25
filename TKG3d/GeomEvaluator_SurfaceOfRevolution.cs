using OCCPort.Common;
using TKMath;

namespace TKG3d
{
    //! Allows to calculate values and derivatives for surfaces of revolution
    public class GeomEvaluator_SurfaceOfRevolution : GeomEvaluator_Surface
    {

        //! Initialize evaluator by revolved curve, the axis of revolution and the location
        public GeomEvaluator_SurfaceOfRevolution(Geom_Curve theBase,
                                                     gp_Dir theRevolDir,
                                                     gp_Pnt theRevolLoc)
        {

            myBaseCurve = (theBase);
            myRotAxis = new(theRevolLoc, theRevolDir);
        }


        public void D1(
       double theU, double theV,
      out gp_Pnt theValue, out gp_Vec theD1U, out gp_Vec theD1V)
        {
            
            if (myBaseAdaptor != null)
                myBaseAdaptor.D1(theV, out theValue, out theD1V);
            else
                myBaseCurve.D1(theV, out theValue, out theD1V);

            // vector from center of rotation to the point on rotated curve
            gp_XYZ aCQ = theValue.XYZ() - myRotAxis.Location().XYZ();
            theD1U = new gp_Vec(myRotAxis.Direction().XYZ().Crossed(aCQ));
            // If the point is placed on the axis of revolution then derivatives on U are undefined.
            // Manually set them to zero.
            if (theD1U.SquareMagnitude() < Precision.SquareConfusion())
                theD1U.SetCoord(0.0, 0.0, 0.0);

            gp_Trsf aRotation = new gp_Trsf();
            aRotation.SetRotation(myRotAxis, theU);
            theValue.Transform(aRotation);
            theD1U.Transform(aRotation);
            theD1V.Transform(aRotation);
        }

        public void D0(
     double theU, double theV,
    out gp_Pnt theValue)
        {
            theValue = new gp_Pnt();//not origin code
            if (myBaseAdaptor != null)
                myBaseAdaptor.D0(theV, ref theValue);
            else
                myBaseCurve.D0(theV, ref theValue);

            gp_Trsf aRotation = new gp_Trsf();
            aRotation.SetRotation(myRotAxis, theU);
            theValue.Transform(aRotation);
        }

        public void D2(double theU, double theV, out gp_Pnt theValue, out gp_Vec theD1U, out gp_Vec theD1V, out gp_Vec theD2U, out gp_Vec theD2V, out gp_Vec theD2UV)
        {
            throw new NotImplementedException();
        }

        Geom_Curve myBaseCurve;
        Adaptor3d_Curve myBaseAdaptor;
        gp_Ax1 myRotAxis;
    }
}
