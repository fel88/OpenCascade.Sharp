using TKBRep;
using TKernel;
using TKSTEPBase;
using TKXSBASE;


namespace TKSTEP
{
    //! This class performs the transfer of an Entity from
    //! AP214 and AP203, either Geometric or Topologic.
    //!
    //! I.E. for each type of Entity, it invokes the appropriate Tool
    //! then returns the Binder which contains the Result
    public class STEPControl_ActorRead : Transfer_ActorOfTransientProcess
    {


        public override bool Recognize(object start)
        {
            if (start == null) return false;

            //if (start is StepBasic_ProductDefinition) return true;

            //if (start->IsKind(STANDARD_TYPE(StepRepr_NextAssemblyUsageOccurrence))) return Standard_True;

            //TCollection_AsciiString aProdMode = Interface_Static::CVal("read.step.product.mode");
            //if (!aProdMode.IsEqual("ON"))
            //    if (start->IsKind(STANDARD_TYPE(StepShape_ShapeDefinitionRepresentation))) return Standard_True;

            StepShape_ShapeRepresentation sr = start as StepShape_ShapeRepresentation;
            if (sr != null)
            {
                int i, nb = sr.NbItems();
                for (i = 1; i <= nb; i++)
                {
                    if (Recognize(sr.ItemsValue(i))) return true;
                }
                return false;
            }

            bool aCanReadTessGeom = (Interface_Static.IVal("read.step.tessellated") != 0);

            //if (start->IsKind(STANDARD_TYPE(StepShape_FacetedBrep))) return Standard_True;
            //  if (start->IsKind(STANDARD_TYPE(StepShape_BrepWithVoids))) return Standard_True;
            if (start is StepShape_ManifoldSolidBrep) return true;
            //  if (start->IsKind(STANDARD_TYPE(StepShape_ShellBasedSurfaceModel))) return Standard_True;
            ///   if (start->IsKind(STANDARD_TYPE(StepShape_FacetedBrepAndBrepWithVoids))) return Standard_True;
            //   if (start->IsKind(STANDARD_TYPE(StepShape_GeometricSet))) return Standard_True;
            //   if (start->IsKind(STANDARD_TYPE(StepRepr_MappedItem))) return Standard_True;
            if (start is StepShape_FaceSurface) return true;
            /**if (start->IsKind(STANDARD_TYPE(StepShape_EdgeBasedWireframeModel))) return Standard_True;
            if (start->IsKind(STANDARD_TYPE(StepShape_FaceBasedSurfaceModel))) return Standard_True;
            if (aCanReadTessGeom && start->IsKind(STANDARD_TYPE(StepVisual_TessellatedFace))) return Standard_True;
            if (aCanReadTessGeom && start->IsKind(STANDARD_TYPE(StepVisual_TessellatedShell))) return Standard_True;
            if (aCanReadTessGeom && start->IsKind(STANDARD_TYPE(StepVisual_TessellatedSolid))) return Standard_True;
            if (aCanReadTessGeom && start->IsKind(STANDARD_TYPE(StepVisual_TessellatedShapeRepresentation))) return Standard_True;
            */

            //  REPRESENTATION_RELATIONSHIP et consorts : on regarde le contenu ...
            //  On prend WithTransformation ou non ...

            //if (start->IsKind(STANDARD_TYPE(StepRepr_ShapeRepresentationRelationship)))
            //{
            //    DeclareAndCast(StepRepr_ShapeRepresentationRelationship, und, start);

            //    //  On prend son contenu

            //    if (Recognize(und->Rep1()) || Recognize(und->Rep2())) return Standard_True;
            //    return Standard_False;
            //}

            //if (start->IsKind(STANDARD_TYPE(StepShape_ContextDependentShapeRepresentation)))
            //{
            //    return Standard_True;
            //    //  on fait le pari que, si ce n est pas transferable tel quel,
            //    //  des CDSR implicitement references le sont ...
            //    //  Sinon cette entite n aurait pas grand sens ...
            //}

            return false;
        }


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
}
