global using Graphic3d_SequenceOfGroup = TKernel.NCollection_Sequence<TKService.Graphic3d_Group>;

using OCCPort.Common;
using OpenTK.Graphics.OpenGL;
using System;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using TKernel;
using TKMath;
using TKOpenGl;
using TKService;


namespace OCCPort.OpenGL
{
    //! Implementation of low-level graphic structure.
    public class OpenGl_Structure : Graphic3d_CStructure
    {
        //! Returns instanced OpenGL structure.
        public OpenGl_Structure InstancedStructure() { return myInstancedStructure; }


        protected OpenGl_Structure myInstancedStructure;
        public override void SetTransformation(TopLoc_Datum3D theTrsf)
        {
            myTrsf = theTrsf;
            myIsMirrored = false;
            if (myTrsf != null)
            {
                // Determinant of transform matrix less then 0 means that mirror transform applied.
                gp_Trsf aTrsf = myTrsf.Transformation();
                double aDet = aTrsf.Value(1, 1) * (aTrsf.Value(2, 2) * aTrsf.Value(3, 3) - aTrsf.Value(3, 2) * aTrsf.Value(2, 3))
                                        - aTrsf.Value(1, 2) * (aTrsf.Value(2, 1) * aTrsf.Value(3, 3) - aTrsf.Value(3, 1) * aTrsf.Value(2, 3))
                                        + aTrsf.Value(1, 3) * (aTrsf.Value(2, 1) * aTrsf.Value(3, 2) - aTrsf.Value(3, 1) * aTrsf.Value(2, 2));
                myIsMirrored = aDet < 0.0;
            }

            updateLayerTransformation();
            if (IsRaytracable())
            {
                ++myModificationState;
            }
        }

        bool myIsMirrored; //!< Used to tell OpenGl to interpret polygons in clockwise order.

        int myModificationState;
        public OpenGl_Structure(Graphic3d_StructureManager theManager) : base(theManager)
        {
            myInstancedStructure = (null);
            myIsRaytracable = (false);
            myModificationState = (0);
            myIsMirrored = false;

            updateLayerTransformation();
        }
        public override void updateLayerTransformation()
        {
            gp_Trsf aRenderTrsf = new gp_Trsf();
            if (myTrsf != null)
            {
                aRenderTrsf = myTrsf.Trsf();
            }

            Graphic3d_ZLayerSettings aLayer = myGraphicDriver.ZLayerSettings(myZLayer);
            if (aLayer.OriginTransformation() != null
              && myTrsfPers == null)
            {
                aRenderTrsf.SetTranslationPart(
                    new gp_Vec(
                    aRenderTrsf.TranslationPart() - aLayer.Origin()));
            }
            aRenderTrsf.GetMat4(myRenderTrsf);
        }

        Graphic3d_Mat4 myRenderTrsf = new TKernel.NCollection_Mat4<float>(); //!< transformation, actually used for rendering (includes Local Origin shift)

        public override Graphic3d_CStructure ShadowLink(Graphic3d_StructureManager theManager)
        {
            return new OpenGl_StructureShadow(theManager, this);
        }

        //! Connect other structure to this one
        public override void Connect(Graphic3d_CStructure theStructure)
        {

            OpenGl_Structure aStruct = (OpenGl_Structure)theStructure;

            Exceptions.Standard_ASSERT_RAISE(myInstancedStructure == null || myInstancedStructure == aStruct,
              "Error! Instanced structure is already defined");

            myInstancedStructure = aStruct;

            if (aStruct.IsRaytracable())
            {
                UpdateStateIfRaytracable(false);
            }
        }


        // =======================================================================
        public void renderGeometry(OpenGl_Workspace theWorkspace,
                                         ref bool theHasClosed)
        {
            if (myInstancedStructure != null)
            {
                myInstancedStructure.renderGeometry(theWorkspace, ref theHasClosed);
            }

            bool anOldCastShadows = false;
            OpenGl_Context aCtx = theWorkspace.GetGlContext();
            //for (OpenGl_Structure.GroupIterator aGroupIter = new GroupIterator(myGroups); aGroupIter.More(); aGroupIter.Next())
            foreach (var aGroup in myGroups.OfType<OpenGl_Group>())
            {
                //OpenGl_Group aGroup = aGroupIter.Value();

                Graphic3d_TransformPers aTrsfPers = aGroup.TransformPersistence();
                if (aTrsfPers != null)
                {
                    applyPersistence(aCtx, aTrsfPers, true, anOldCastShadows);
                    aCtx.ApplyModelViewMatrix();
                }

                theHasClosed = theHasClosed || aGroup.IsClosed();
                aGroup.Render(theWorkspace);

                if (aTrsfPers != null)
                {
                    revertPersistence(aCtx, aTrsfPers, true, anOldCastShadows);
                    aCtx.ApplyModelViewMatrix();
                }
            }

        }

        private void revertPersistence(OpenGl_Context aCtx, Graphic3d_TransformPers aTrsfPers, bool v, bool anOldCastShadows)
        {
            throw new NotImplementedException();
        }

        private void applyPersistence(OpenGl_Context aCtx, Graphic3d_TransformPers aTrsfPers, bool v, bool anOldCastShadows)
        {
            throw new NotImplementedException();
        }

        public void Render(OpenGl_Workspace theWorkspace)
        {
            // Process the structure only if visible
            if (visible == 0)
                return;

            OpenGl_Context aCtx = theWorkspace.GetGlContext();

            // Apply local transformation
            aCtx.ModelWorldState.Push();
            OpenGl_Mat4 aModelWorld = aCtx.ModelWorldState.ChangeCurrent();
            aModelWorld = myRenderTrsf;

            bool anOldGlNormalize = aCtx.IsGlNormalizeEnabled();

            // Take into account transform persistence
            aCtx.ApplyModelViewMatrix();

            // True if structure is fully clipped
            bool isClipped = false;
            bool hasDisabled = false;
            // Render groups
            bool hasClosedPrims = false;
            if (!isClipped)
            {
                renderGeometry(theWorkspace, ref hasClosedPrims);
            }


            // Restore local transformation
            aCtx.ModelWorldState.Pop();
            aCtx.SetGlNormalizeEnabled(anOldGlNormalize);
        }

        public OpenGl_GraphicDriver GlDriver()
        {
            //return (OpenGl_GraphicDriver* )myGraphicDriver.operator->();
            return (OpenGl_GraphicDriver)myGraphicDriver;
        }

        internal void UpdateStateIfRaytracable(bool toCheck)
        {
            myIsRaytracable = !toCheck;
            if (!myIsRaytracable)
            {
                for (OpenGl_Structure.GroupIterator anIter = new GroupIterator(myGroups); anIter.More(); anIter.Next())
                {
                    if (anIter.Value().IsRaytracable())
                    {
                        myIsRaytracable = true;
                        break;
                    }
                }
            }

            if (IsRaytracable())
            {
                ++myModificationState;
            }
        }

        public override Graphic3d_Group NewGroup(Graphic3d_Structure theStruct)
        {
            OpenGl_Group aGroup = new OpenGl_Group(theStruct);
            myGroups.Append(aGroup);
            return aGroup;

        }


        internal bool IsRaytracable()
        {
            if (!myGroups.IsEmpty()
              && myIsRaytracable
              && myTrsfPers == null)
            {
                return true;
            }

            return myInstancedStructure != null
               && myInstancedStructure.IsRaytracable();

        }

        public override void OnVisibilityChanged()
        {
            if (IsRaytracable())
            {
                ++myModificationState;
            }
        }

        bool myIsRaytracable;

        private class GroupIterator
        {
            public GroupIterator(Graphic3d_SequenceOfGroup myGroups)
            {
            }

            internal bool More()
            {
                throw new NotImplementedException();
            }

            internal object Next()
            {
                throw new NotImplementedException();
            }

            internal OpenGl_Group Value()
            {
                throw new NotImplementedException();
            }
        }

        //! Auxiliary wrapper to iterate through structure list.       
        public class SubclassStructIterator<T> where T : Graphic3d_CStructure
        {
            public SubclassStructIterator(NCollection_IndexedMap<Graphic3d_CStructure> theStructs)
            {
                myIter = new NCollection_IndexedMap<Graphic3d_CStructure, NCollection_DefaultHasher<Graphic3d_CStructure>>.Iterator(theStructs);
            }

            public T Value() { return ((T)myIter.Value()); }

            public bool More() { return myIter.More(); }
            public void Next() { myIter.Next(); }

            NCollection_IndexedMap<Graphic3d_CStructure>.Iterator myIter;

        }
        //! Auxiliary wrapper to iterate OpenGl_Structure sequence.
        internal class StructIterator : SubclassStructIterator<OpenGl_Structure>
        {
            public StructIterator(Graphic3d_IndexedMapOfStructure aStructures) : base(aStructures)
            {
            }


        }
    }
}



