
global using StepToTopoDS_DataMapOfTRI= TKernel.NCollection_DataMap<TKSTEPBase.StepShape_TopologicalRepresentationItem,TKBRep.TopoDS_Shape, TKernel.NCollection_DefaultHasher<TKSTEPBase.StepShape_TopologicalRepresentationItem>> ;

using System;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.Intrinsics.X86;
using TKBRep;
using TKernel;
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
            if (P != null)
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

        //=============================================================================
        // Creation d' un Axis2Placement de Geom a partir d' un axis2_placement_3d de Step
        //=============================================================================

        public Geom_Axis2Placement MakeAxis2Placement(StepGeom_Axis2Placement3d SA)
        {
            Geom_CartesianPoint P = MakeCartesianPoint(SA.Location());
            if (P != null)
            {
                gp_Pnt Pgp = P.Pnt();

                // sln 22.10.2001. CTS23496: If problems with creation of direction occur default direction is used (MakeLine(...) function)
                gp_Dir Ngp = new(0.0, 0.0, 1.0);
                if (SA.HasAxis())
                {
                    Geom_Direction D = MakeDirection(SA.Axis());
                    if (D != null)
                        Ngp = D.Dir();
                }

                gp_Ax2 gpAx2 = new gp_Ax2();
                bool isDefaultDirectionUsed = true;
                if (SA.HasRefDirection())
                {
                    Geom_Direction D = MakeDirection(SA.RefDirection());
                    if (D != null)
                    {
                        gp_Dir Vxgp = D.Dir();
                        if (!Ngp.IsParallel(Vxgp, Precision.Angular()))
                        {
                            gpAx2 = new gp_Ax2(Pgp, Ngp, Vxgp);
                            isDefaultDirectionUsed = false;
                        }
                    }
                }
                if (isDefaultDirectionUsed)
                    gpAx2 = new gp_Ax2(Pgp, Ngp);

                return new Geom_Axis2Placement(gpAx2);
            }
            return null;
        }

        //=============================================================================
        // Creation of an AxisPlacement from a Kinematic SuParameters for Step
        //=============================================================================

        public Geom_Axis2Placement MakeAxis2Placement(StepGeom_SuParameters theSP)
        {
            double aLocX = theSP.A() * Math.Cos(theSP.Gamma()) + theSP.B() * Math.Sin(theSP.Gamma()) * Math.Sin(theSP.Alpha());
            double aLocY = theSP.A() * Math.Sin(theSP.Gamma()) - theSP.B() * Math.Cos(theSP.Gamma()) * Math.Sin(theSP.Alpha());
            double aLocZ = theSP.C() + theSP.B() * Math.Cos(theSP.Alpha());
            double anAsisX = Math.Sin(theSP.Gamma()) * Math.Sin(theSP.Alpha());
            double anAxisY = -Math.Cos(theSP.Gamma()) * Math.Sin(theSP.Alpha());
            double anAxisZ = Math.Cos(theSP.Alpha());
            double aDirX = Math.Cos(theSP.Gamma()) * Math.Cos(theSP.Beta()) - Math.Sin(theSP.Gamma()) * Math.Cos(theSP.Alpha()) * Math.Sin(theSP.Beta());
            double aDirY = Math.Sin(theSP.Gamma()) * Math.Cos(theSP.Beta()) + Math.Cos(theSP.Gamma()) * Math.Cos(theSP.Alpha()) * Math.Sin(theSP.Beta());
            double aDirZ = Math.Sin(theSP.Alpha()) * Math.Sin(theSP.Beta());
            gp_Pnt Pgp = new(aLocX, aLocY, aLocZ);
            gp_Dir Ngp = new(anAsisX, anAxisY, anAxisZ);
            gp_Dir Vxgp = new(aDirX, aDirY, aDirZ);
            gp_Ax2 gpAx2 = new gp_Ax2(Pgp, Ngp, Vxgp);
            return new Geom_Axis2Placement(gpAx2);
        }

        //=============================================================================
        // Creation d' une Surface de Geom a partir d' une Surface de Step
        //=============================================================================

        public Geom_Surface MakeSurface(StepGeom_Surface SS)
        {
            // sln 01.10.2001 BUC61003. If entry shell is NULL do nothing
            if (SS == null)
            {
                return null;
            }
            try
            {
                //OCC_CATCH_SIGNALS
                //if (SS->IsKind(STANDARD_TYPE(StepGeom_BoundedSurface)))
                //{
                //    return MakeBoundedSurface(Handle(StepGeom_BoundedSurface)::DownCast(SS));
                //  }
                if (SS is StepGeom_ElementarySurface)
                {
                    StepGeom_ElementarySurface S1 = (StepGeom_ElementarySurface)(SS);
                    if (S1.Position() == null)
                        return null;

                    return MakeElementarySurface(S1);
                }
            }
            catch (Exception ex)
            {

            }
            return null;
        }
        //=============================================================================
        // Creation d' une ElementarySurface de Geom a partir d' une
        // ElementarySurface de Step
        //=============================================================================

        public Geom_ElementarySurface MakeElementarySurface(StepGeom_ElementarySurface SS)
        {
            if (SS is StepGeom_Plane)
            {
                return MakePlane((StepGeom_Plane)(SS));
            }

            return null;
        }
        //=============================================================================
        // Creation d' un Plane de Geom a partir d' un plane de Step
        //=============================================================================

        public Geom_Plane MakePlane(StepGeom_Plane SP)
        {
            Geom_Axis2Placement A = MakeAxis2Placement(SP.Position());
            if (A != null)
                return new Geom_Plane(new gp_Ax3(A.Ax2()));

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


    //! This class performs the transfer of an Entity from
    //! AP214 and AP203, either Geometric or Topologic.
    //!
    //! I.E. for each type of Entity, it invokes the appropriate Tool
    //! then returns the Binder which contains the Result
    public class STEPControl_ActorRead : Transfer_ActorOfTransientProcess
    {
        //! Transfers  geometric representation item entity such as ManifoldSolidBRep ,...etc
        public TransferBRep_ShapeBinder TransferEntity
                    (StepGeom_GeometricRepresentationItem start,
                      Transfer_TransientProcess TP,
                      bool isManifold,
                      Message_ProgressRange theProgress)
        {
            //Message_Messenger::StreamBuffer sout = TP->Messenger()->SendInfo();
            TransferBRep_ShapeBinder shbinder;
            bool found = false;
            StepToTopoDS_Builder myShapeBuilder = new StepToTopoDS_Builder();
            TopoDS_Shape mappedShape;
            int nbTPitems = TP.NbMapped();

            // Start progress scope (no need to check if progress exists -- it is safe)
            Message_ProgressScope aPS = new(theProgress, "Transfer stage", isManifold ? 2 : 1);

            Message_ProgressRange aRange = aPS.Next();

            if (start is StepShape_ManifoldSolidBrep)
            {
                myShapeBuilder.Init(((StepShape_ManifoldSolidBrep)start), TP, aRange);
                found = true;
            }

            return null;//
        }
    }

    //! The original class was renamed. Compatibility only
    public class Transfer_ActorOfTransientProcess : Transfer_ActorOfProcessForTransient
    {
    }
    public class Transfer_ActorOfProcessForTransient
    { }


    public class StepShape_ManifoldSolidBrep : StepShape_SolidModel
    {
        public StepShape_ManifoldSolidBrep()
        {

        }
        public StepShape_ConnectedFaceSet Outer()
        {
            return outer;
        }
        StepShape_ConnectedFaceSet outer;

    }

    public class StepShape_SolidModel : StepGeom_GeometricRepresentationItem
    {
    }

    enum StepToTopoDS_BuilderError
    {
        StepToTopoDS_BuilderDone,
        StepToTopoDS_BuilderOther
    };

    public enum StepToTopoDS_TranslateShellError
    {
        StepToTopoDS_TranslateShellDone,
        StepToTopoDS_TranslateShellOther
    };





    enum StepToTopoDS_TranslateFaceError
    {
        StepToTopoDS_TranslateFaceDone,
        StepToTopoDS_TranslateFaceOther
    };



    //! Provides data to process non-manifold topology when
    //! reading from STEP.
    public class StepToTopoDS_NMTool
    {
    }
}
