
global using Select3D_BVHBuilder3d = TKMath.BVH_Builder;
global using OpenGl_Layer = TKService.Graphic3d_Layer;
using OCCPort.Common;
using OCCPort.OpenGL;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using TKernel;
using TKService;
using TKOpenGl;

namespace OCCPort
{
    //! Class defining the list of layers.
    internal class OpenGl_LayerList
    {
        public OpenGl_LayerList()
        {

        }

        public void ChangeLayer(OpenGl_Structure theStructure,
                                    Graphic3d_ZLayerId theOldLayerId,
                                    Graphic3d_ZLayerId theNewLayerId)
        {
            Graphic3d_Layer aLayerPtr = myLayerIds.Seek(theOldLayerId);
            Graphic3d_Layer aLayer = aLayerPtr != null ? aLayerPtr : myLayerIds.Find(Graphic3d_ZLayerId.Graphic3d_ZLayerId_Default);

            Graphic3d_DisplayPriority aPriority = Graphic3d_DisplayPriority.Graphic3d_DisplayPriority_INVALID;

            // take priority and remove structure from list found by <theOldLayerId>
            // if the structure is not found there, scan through all other layers
            if (aLayer.Remove(theStructure, ref aPriority, false))
            {
                if (aLayer.LayerSettings().IsRaytracable()
                && !aLayer.LayerSettings().IsImmediate()
                && theStructure.IsRaytracable())
                {
                    ++myModifStateOfRaytraceable;
                }

                --myNbStructures;
                if (aLayer.IsImmediate())
                {
                    --myImmediateNbStructures;
                }

                // isForChangePriority should be Standard_False below, because we want
                // the BVH tree in the target layer to be updated with theStructure
                AddStructure(theStructure, theNewLayerId, aPriority);
                return;
            }

            // scan through layers and remove it
            foreach (var aLayerIter in myLayers)
            {
                OpenGl_Layer aLayerEx = aLayerIter as OpenGl_Layer;
                if (aLayerEx == aLayer)
                {
                    continue;
                }

                // try to remove structure and get priority value from this layer
                if (aLayerEx.Remove(theStructure, ref aPriority, true))
                {
                    if (aLayerEx.LayerSettings().IsRaytracable()
                    && !aLayerEx.LayerSettings().IsImmediate()
                    && theStructure.IsRaytracable())
                    {
                        ++myModifStateOfRaytraceable;
                    }

                    --myNbStructures;
                    if (aLayerEx.IsImmediate())
                    {
                        --myImmediateNbStructures;
                    }

                    // isForChangePriority should be Standard_False below, because we want
                    // the BVH tree in the target layer to be updated with theStructure
                    AddStructure(theStructure, theNewLayerId, aPriority);
                    return;
                }
            }
        }

        NCollection_List<Graphic3d_Layer> myLayers = new NCollection_List<Graphic3d_Layer>();
        internal Graphic3d_Layer[] Layers()
        {
            return myLayers.ToArray();
        }
        //! Returns the map of Z-layer IDs to indexes.
        public MyLayersDic LayerIDs() { return myLayerIds; }


        //! Method returns the number of available priorities
        public int NbPriorities() { return Constants.Graphic3d_DisplayPriority_NB; }

        //! Number of displayed structures
        public int NbStructures() { return myNbStructures; }

        public void AddStructure(OpenGl_Structure theStruct,
                                      Graphic3d_ZLayerId theLayerId,
                                      Graphic3d_DisplayPriority thePriority,
                                     bool isForChangePriority = false)
        {
            // add structure to associated layer,
            // if layer doesn't exists, display structure in default layer
            Graphic3d_Layer aLayerPtr = myLayerIds.Seek(theLayerId);
            Graphic3d_Layer aLayer = aLayerPtr != null ? aLayerPtr : myLayerIds.Find(Graphic3d_ZLayerId.Graphic3d_ZLayerId_Default);
            aLayer.Add(theStruct, thePriority, isForChangePriority);
            ++myNbStructures;
            if (aLayer.IsImmediate())
            {
                ++myImmediateNbStructures;
            }

            // Note: In ray-tracing mode we don't modify modification
            // state here. It is redundant, because the possible changes
            // will be handled in the loop for structures
        }
        public void InsertLayerBefore(Graphic3d_ZLayerId theNewLayerId,
                                          Graphic3d_ZLayerSettings theSettings,
                                          Graphic3d_ZLayerId theLayerAfter)
        {
            if (myLayerIds.IsBound(theNewLayerId))
            {
                return;
            }

            Graphic3d_Layer aNewLayer = new Graphic3d_Layer(theNewLayerId, myBVHBuilder);
            aNewLayer.SetLayerSettings(theSettings);

            Graphic3d_Layer anOtherLayer;
            if (theLayerAfter != Graphic3d_ZLayerId.Graphic3d_ZLayerId_UNKNOWN
             && myLayerIds.Find(theLayerAfter, out anOtherLayer))
            {
                foreach (var aLayerIter in myLayers)

                //	for (NCollection_List < Handle(Graphic3d_Layer) >::Iterator aLayerIter(myLayers); aLayerIter.More(); aLayerIter.Next())
                {
                    //if (aLayerIter.Value() == anOtherLayer)
                    {
                        myLayers.InsertBefore(aNewLayer, aLayerIter);
                        break;
                    }
                }
            }
            else
            {
                myLayers.Prepend(aNewLayer);
            }

            myLayerIds.Bind(theNewLayerId, aNewLayer);
            //myTransparentToProcess.Allocate(myLayers.Size());
        }

        Select3D_BVHBuilder3d myBVHBuilder;      //!< BVH tree builder for frustum culling

        int myNbStructures;

        //=======================================================================
        //function : InsertLayerAfter
        //purpose  :
        //=======================================================================
        public void InsertLayerAfter(Graphic3d_ZLayerId theNewLayerId,

                                         Graphic3d_ZLayerSettings theSettings,
                                         Graphic3d_ZLayerId theLayerBefore)
        {
            if (myLayerIds.IsBound(theNewLayerId))
            {
                return;
            }

            Graphic3d_Layer aNewLayer = new Graphic3d_Layer(theNewLayerId, myBVHBuilder);
            aNewLayer.SetLayerSettings(theSettings);

            Graphic3d_Layer anOtherLayer;
            if (theLayerBefore != Graphic3d_ZLayerId.Graphic3d_ZLayerId_UNKNOWN
             && myLayerIds.Find(theLayerBefore, out anOtherLayer))
            {
                foreach (var aLayerIter in myLayers)

                //for (NCollection_List < Handle(Graphic3d_Layer) >::Iterator aLayerIter(myLayers); aLayerIter.More(); aLayerIter.Next())
                {
                    if (aLayerIter == anOtherLayer)
                    {
                        myLayers.InsertAfter(aNewLayer, aLayerIter);
                        break;
                    }
                }
            }
            else
            {
                myLayers.Append(aNewLayer);
            }

            myLayerIds.Bind(theNewLayerId, aNewLayer);
            //myTransparentToProcess.Allocate(myLayers.Size());
        }



        //! Return number of structures within immediate layers
        public int NbImmediateStructures() { return myImmediateNbStructures; }

        internal void UpdateCulling(OpenGl_Workspace theWorkspace, bool theToDrawImmediate)
        {

            OpenGl_FrameStats aStats = theWorkspace.GetGlContext().FrameStats();
            //OSD_Timer & aTimer = aStats->ActiveDataFrame().ChangeTimer(Graphic3d_FrameStatsTimer_CpuCulling);
            //aTimer.Start();

            int aViewId = theWorkspace.View().Identification();
            Graphic3d_CullingTool aSelector = theWorkspace.View().BVHTreeSelector();
            for (NCollection_List<Graphic3d_Layer>.Iterator aLayerIter = new NCollection_List<OpenGl_Layer>.Iterator(myLayers); aLayerIter.More(); aLayerIter.Next())
            {
                Graphic3d_Layer aLayer = aLayerIter.ChangeValue();
                if (aLayer.IsImmediate() != theToDrawImmediate)
                {
                    continue;
                }

                aLayer.UpdateCulling(aViewId, aSelector, theWorkspace.View().RenderingParams().FrustumCullingState);
            }

            //aTimer.Stop();
            //aStats->ActiveDataFrame()[Graphic3d_FrameStatsTimer_CpuCulling] = aTimer.UserTimeCPU();
            /*
             more code here
             */
            //throw new NotImplementedException();
        }

        void renderLayer(OpenGl_Workspace theWorkspace,
                                     OpenGl_GlobalLayerSettings theDefaultSettings,
                                     Graphic3d_Layer theLayer)
        {

            /* more code here*/

            // render priority list
            int aViewId = theWorkspace.View().Identification();
            for (int aPriorityIter = (int)Graphic3d_DisplayPriority.Graphic3d_DisplayPriority_Bottom; aPriorityIter <= (int)Graphic3d_DisplayPriority.Graphic3d_DisplayPriority_Topmost; ++aPriorityIter)
            {
                Graphic3d_IndexedMapOfStructure aStructures = theLayer.Structures((Graphic3d_DisplayPriority)aPriorityIter);
                for (OpenGl_Structure.StructIterator aStructIter =new(aStructures); aStructIter.More(); aStructIter.Next())
                //for (OpenGl_Structure.StructIterator aStructIter = new OpenGl_Structure.StructIterator(aStructures); aStructIter.More(); aStructIter.Next())
                {
                    OpenGl_Structure aStruct = aStructIter.Value();
                    if (aStruct.IsCulled()
                    || !aStruct.IsVisible(aViewId))
                    {
                        continue;
                    }

                    aStruct.Render(theWorkspace);
                }
            }
        }
        internal struct OpenGl_GlobalLayerSettings
        {
            public int DepthFunc;
            public bool DepthMask;
        }

        void renderTransparent(OpenGl_Workspace theWorkspace,
                                           //OpenGl_LayerStack.iterator theLayerIter,
                                           OpenGl_GlobalLayerSettings theGlobalSettings,
                                          OpenGl_FrameBuffer theReadDrawFbo,
                                          OpenGl_FrameBuffer theOitAccumFbo)
        {

        }

        //! Render this element
        internal void Render(OpenGl_Workspace theWorkspace,
            bool theToDrawImmediate, OpenGl_LayerFilter theLayersToProcess,
            OpenGl_FrameBuffer theReadDrawFbo,
            OpenGl_FrameBuffer theOitAccumFbo)
        {
            // Remember global settings for glDepth function and write mask.
            OpenGl_GlobalLayerSettings aPrevSettings = new OpenGl_GlobalLayerSettings();

            OpenGl_Context aCtx = theWorkspace.GetGlContext();
            aCtx.core11fwd.glGetIntegerv(OpenTK.Graphics.OpenGL.All.DepthFunc, ref aPrevSettings.DepthFunc);
            aCtx.core11fwd.glGetBooleanv(OpenTK.Graphics.OpenGL.All.DepthWritemask, ref aPrevSettings.DepthMask);
            OpenGl_GlobalLayerSettings aDefaultSettings = aPrevSettings;




            bool isShadowMapPass = theReadDrawFbo != null
                                     && !theReadDrawFbo.HasColor();


            // Two render filters are used to support transparency draw. Opaque filter accepts
            // only non-transparent OpenGl elements of a layer and counts number of skipped
            // transparent ones. If the counter has positive value the layer is added into
            // transparency post-processing stack. At the end of drawing or once the depth
            // buffer is to be cleared the layers in the stack should be drawn using
            // blending and depth mask settings and another transparency filter which accepts
            // only transparent OpenGl elements of a layer. The stack <myTransparentToProcess>
            // was preallocated before going into this method and has enough space to keep
            // maximum number of references to layers, therefore it will not increase memory
            // fragmentation during regular rendering.
            int aPrevFilter = theWorkspace.RenderFilter() & ~(int)(OpenGl_RenderFilter.OpenGl_RenderFilter_OpaqueOnly | OpenGl_RenderFilter.OpenGl_RenderFilter_TransparentOnly);
            theWorkspace.SetRenderFilter(aPrevFilter | (int)OpenGl_RenderFilter.OpenGl_RenderFilter_OpaqueOnly);

            myTransparentToProcess.Clear();

            var aStackIter = new NCollection_Array1<Graphic3d_Layer>.iterator(myTransparentToProcess.Origin());
            int aClearDepthLayerPrev = -1, aClearDepthLayer = -1;
            bool toPerformDepthPrepass = theWorkspace.View().RenderingParams().ToEnableDepthPrepass
                                           && aPrevSettings.DepthMask == true
                                           && !isShadowMapPass;
            Graphic3d_LightSet aLightsBack = aCtx.ShaderManager().LightSourceState().LightSources();
            OpenGl_ShadowMapArray aShadowMaps = aCtx.ShaderManager().LightSourceState().ShadowMaps();

            for (OpenGl_FilteredIndexedLayerIterator aLayerIterStart = new OpenGl_FilteredIndexedLayerIterator(myLayers, theToDrawImmediate, theLayersToProcess); aLayerIterStart.More();)
            {
                bool hasSkippedDepthLayers = false;
                for (int aPassIter = toPerformDepthPrepass ? 0 : 2; aPassIter < 3; ++aPassIter)
                {
                    if (aPassIter == 0)
                    {
                        aCtx.SetColorMask(false);
                        aCtx.ShaderManager().UpdateLightSourceStateTo(null, theWorkspace.View().SpecIBLMapLevels(), null);
                        aDefaultSettings.DepthFunc = aPrevSettings.DepthFunc;
                        aDefaultSettings.DepthMask = true;
                    }
                    else if (aPassIter == 1)
                    {
                        if (!hasSkippedDepthLayers)
                        {
                            continue;
                        }
                        aCtx.SetColorMask(true);
                        aCtx.ShaderManager().UpdateLightSourceStateTo(aLightsBack, theWorkspace.View().SpecIBLMapLevels(), aShadowMaps);
                        aDefaultSettings = aPrevSettings;
                    }
                    else if (aPassIter == 2)
                    {
                        if (isShadowMapPass)
                        {
                            aCtx.SetColorMask(false);
                            aCtx.ShaderManager().UpdateLightSourceStateTo(null, theWorkspace.View().SpecIBLMapLevels(), null);
                        }
                        else
                        {
                            aCtx.SetColorMask(true);
                            aCtx.ShaderManager().UpdateLightSourceStateTo(aLightsBack, theWorkspace.View().SpecIBLMapLevels(), aShadowMaps);
                        }
                        if (toPerformDepthPrepass)
                        {
                            aDefaultSettings.DepthFunc = (int)All.Equal;
                            aDefaultSettings.DepthMask = false;
                        }
                    }

                    OpenGl_FilteredIndexedLayerIterator aLayerIter = new OpenGl_FilteredIndexedLayerIterator(aLayerIterStart);
                    for (; aLayerIter.More(); aLayerIter.Next())
                    {
                        OpenGl_Layer aLayer = aLayerIter.Value();

                        // make sure to clear depth of previous layers even if layer has no structures
                        if (aLayer.LayerSettings().ToClearDepth())
                        {
                            aClearDepthLayer = aLayerIter.Index();
                        }
                        if (aLayer.IsCulled())
                        {
                            continue;
                        }
                        else if (aClearDepthLayer > aClearDepthLayerPrev)
                        {
                            // At this point the depth buffer may be set to clear by previous configuration of layers or configuration of the current layer.
                            // Additional rendering pass to handle transparent elements of recently drawn layers require use of current depth
                            // buffer so we put remaining layers for processing as one bunch before erasing the depth buffer.
                            if (aPassIter == 2)
                            {
                                aLayerIterStart = aLayerIter;
                            }
                            else
                            {
                                aClearDepthLayer = -1;
                            }
                            break;
                        }
                        else if (aPassIter == 0
                             && !aLayer.LayerSettings().ToRenderInDepthPrepass())
                        {
                            hasSkippedDepthLayers = true;
                            continue;
                        }
                        else if (aPassIter == 1
                              && aLayer.LayerSettings().ToRenderInDepthPrepass())
                        {
                            continue;
                        }

                        // Render opaque OpenGl elements of a layer and count the number of skipped.
                        // If a layer has skipped (e.g. transparent) elements it should be added into
                        // the transparency post-processing stack.
                        theWorkspace.ResetSkippedCounter();

                        renderLayer(theWorkspace, aDefaultSettings, aLayer);

                        if (aPassIter != 0
                         && theWorkspace.NbSkippedTransparentElements() > 0)
                        {
                            myTransparentToProcess.Push(aLayer);
                        }
                    }
                    if (aPassIter == 2
                    && !aLayerIter.More())
                    {
                        aLayerIterStart = aLayerIter;
                    }
                }

                if (!myTransparentToProcess.IsEmpty())
                {
                    //   renderTransparent(theWorkspace, aStackIter, aPrevSettings, theReadDrawFbo, theOitAccumFbo);
                }
                if (aClearDepthLayer > aClearDepthLayerPrev)
                {
                    aClearDepthLayerPrev = aClearDepthLayer;
                    aCtx.core11fwd.glDepthMask(true);
                    aCtx.core11fwd.glClear(All.DepthBufferBit);
                }
            }

            aCtx.ShaderManager().UpdateLightSourceStateTo(aLightsBack, theWorkspace.View().SpecIBLMapLevels(), aShadowMaps);
            aCtx.core11fwd.glDepthMask(aPrevSettings.DepthMask);
            aCtx.core11fwd.glDepthFunc(aPrevSettings.DepthFunc);

            theWorkspace.SetRenderFilter(aPrevFilter);

        }

        internal void RemoveStructure(OpenGl_Structure theStructure)
        {
            Graphic3d_ZLayerId aLayerId = theStructure.ZLayer();
            Graphic3d_Layer aLayerPtr = myLayerIds.Seek(aLayerId);
            Graphic3d_Layer aLayer = aLayerPtr != null ? aLayerPtr : myLayerIds.Find(Graphic3d_ZLayerId.Graphic3d_ZLayerId_Default);

            Graphic3d_DisplayPriority aPriority = Graphic3d_DisplayPriority.Graphic3d_DisplayPriority_INVALID;

            // remove structure from associated list
            // if the structure is not found there,
            // scan through layers and remove it
            if (aLayer.Remove(theStructure, ref aPriority))
            {
                --myNbStructures;
                if (aLayer.IsImmediate())
                {
                    --myImmediateNbStructures;
                }

                if (aLayer.LayerSettings().IsRaytracable()
                 && theStructure.IsRaytracable())
                {
                    ++myModifStateOfRaytraceable;
                }

                return;

            }


            // scan through layers and remove it
            foreach (var aLayerEx in myLayers)
            //for (NCollection_List < Handle(Graphic3d_Layer) >::Iterator aLayerIter(myLayers); aLayerIter.More(); aLayerIter.Next())
            {
                //Graphic3d_Layer aLayerEx = aLayerIter.ChangeValue();
                if (aLayerEx == aLayer)
                {
                    continue;
                }

                if (aLayerEx.Remove(theStructure, ref aPriority))
                {
                    --myNbStructures;
                    if (aLayerEx.IsImmediate())
                    {
                        --myImmediateNbStructures;
                    }

                    if (aLayerEx.LayerSettings().IsRaytracable()
                     && theStructure.IsRaytracable())
                    {
                        ++myModifStateOfRaytraceable;
                    }
                    return;
                }
            }
        }

        public void InvalidateBVHData(Graphic3d_ZLayerId theLayerId)
        {
            Graphic3d_Layer aLayerPtr = myLayerIds.Seek(theLayerId);
            Graphic3d_Layer aLayer = aLayerPtr != null ? aLayerPtr : myLayerIds.Find(Graphic3d_ZLayerId.Graphic3d_ZLayerId_Default);
            aLayer.InvalidateBVHData();
        }

        MyLayersDic myLayerIds = new MyLayersDic();

        int myImmediateNbStructures; //!< number of structures within immediate layers

        int myModifStateOfRaytraceable;

        //! Collection of references to layers with transparency gathered during rendering pass.
        OpenGl_LayerStack myTransparentToProcess = new OpenGl_LayerStack();
    }
}