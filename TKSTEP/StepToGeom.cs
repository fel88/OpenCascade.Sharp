using OCCPort.Common;
using System.Reflection.Metadata;
using TKG3d;
using TKMath;
using TKSTEPBase;
using TKXSBASE;

namespace TKSTEP
{
    //! This class provides static methods to convert STEP geometric entities to OCCT.
    //! The methods returning handles will return null handle in case of error.
    //! The methods returning boolean will return True if succeeded and False if error.
    public class StepToGeom
    {
        public Geom_CartesianPoint MakeCartesianPoint(StepGeom_CartesianPoint SP)
        {
            if (SP.NbCoordinates() == 3)
            {
                double LF = StepData_GlobalFactors.Intance.LengthFactor();
                double X = SP.CoordinatesValue(1) * LF;
                double Y = SP.CoordinatesValue(2) * LF;
                double Z = SP.CoordinatesValue(3) * LF;
                return new Geom_CartesianPoint(X, Y, Z);
            }
            return null;
        }

        Geom_Direction MakeDirection(StepGeom_Direction SD)
        {
            if (SD.NbDirectionRatios() >= 3)
            {
                double X = SD.DirectionRatiosValue(1);
                double Y = SD.DirectionRatiosValue(2);
                double Z = SD.DirectionRatiosValue(3);
                //5.08.2021. Unstable test bugs xde bug24759: Y is very large value - FPE in SquareModulus
                if (Precision.IsInfinite(X) || Precision.IsInfinite(Y) || Precision.IsInfinite(Z))
                {
                    return null;
                }
                // sln 22.10.2001. CTS23496: Direction is not created if it has null magnitude
                if (new gp_XYZ(X, Y, Z).SquareModulus() > gp.Resolution() * gp.Resolution())
                {
                    return new Geom_Direction(X, Y, Z);
                }
            }
            return null;
        }


        //==============================================================
        // Creation d' un VectorWithMagnitude de Geom a partir d' un Vector de Step
        //=============================================================================

        Geom_VectorWithMagnitude MakeVectorWithMagnitude(StepGeom_Vector SV)
        {
            // sln 22.10.2001. CTS23496: Vector is not created if direction have not been successfully created
            Geom_Direction D = MakeDirection(SV.Orientation());
            if (D != null)
            {
                gp_Vec V = new(D.Dir().XYZ() * SV.Magnitude() * StepData_GlobalFactors.Intance.LengthFactor());
                return new Geom_VectorWithMagnitude(V);
            }
            return null;
        }

        public Geom_Line MakeLine(StepGeom_Line SC)
        {
            Geom_CartesianPoint P = MakeCartesianPoint(SC.Pnt());
            if (P!=null)
            {
                // sln 22.10.2001. CTS23496: Line is not created if direction have not been successfully created
                Geom_VectorWithMagnitude D = MakeVectorWithMagnitude(SC.Dir());
                if (D != null)
                {
                    if (D.Vec().SquareMagnitude() < Precision.Confusion() * Precision.Confusion())
                        return null;

                    gp_Dir V = new(D.Vec());
                    return new Geom_Line(P.Pnt(), V);
                }
            }
            return null;
        }

        public Geom_Curve MakeCurve(StepGeom_Curve SC)
        {
            if (SC == null)
            {
                return null;
            }
            if (SC is StepGeom_Line)
            {
                return MakeLine((StepGeom_Line)SC);
            }
            return null;
        }
    }
}
