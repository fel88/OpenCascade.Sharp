using OCCPort.Common;
using System.Reflection.Metadata;
using TKernel;
using TKMath;

namespace TKService
{
    //! This class allows the definition a graphic object.
    //! This graphic structure can be displayed, erased, or highlighted.
    //! This graphic structure can be connected with another graphic structure.
    public class Graphic3d_Structure
    {
      public   void SetOwner( object theOwner) { myOwner = theOwner; }
        //! Sets if the structure location has mutable nature (content or location will be changed regularly).
        public void SetMutable(bool theIsMutable)
        {
            if (myCStructure != null) { myCStructure.IsMutable = theIsMutable; }
        }

        //! Creates a graphic object in the manager theManager.
        //! It will appear in all the views of the visualiser.
        //! The structure is not displayed when it is created.
        //! @param theManager structure manager holding this structure
        //! @param theLinkPrs another structure for creating a shadow (linked) structure
        public Graphic3d_Structure(Graphic3d_StructureManager theManager,
                    Graphic3d_Structure theLinkPrs = null)
        {
            myStructureManager = theManager;
            myOwner = null;
            myVisual = Graphic3d_TypeOfStructure.Graphic3d_TOS_ALL;
            myComputeVisual = Graphic3d_TypeOfStructure.Graphic3d_TOS_ALL;

            if (theLinkPrs != null)
            {
                myOwner = theLinkPrs.myOwner;
                if (theLinkPrs.myVisual != Graphic3d_TypeOfStructure.Graphic3d_TOS_COMPUTED)
                {
                    myVisual = theLinkPrs.myVisual;
                }
                myComputeVisual = theLinkPrs.myComputeVisual;
                myCStructure = theLinkPrs.myCStructure.ShadowLink(theManager);
            }
            else
            {
                myCStructure = theManager.GraphicDriver().CreateStructure(theManager);
            }


        }
        object myOwner;

        //! Returns the highlight attributes.
        public Graphic3d_PresentationAttributes HighlightStyle() { return myCStructure.HighlightStyle(); }

        //! Returns the last created group or creates new one if list is empty.
        public Graphic3d_Group CurrentGroup()
        {
            if (Groups().IsEmpty())
            {
                return NewGroup();
            }
            return Groups().Last();
        }



        public void Network(Graphic3d_Structure theStructure,

                                Graphic3d_TypeOfConnection theType,
                               List<Graphic3d_Structure> theSet)
        {

            theSet.Add(theStructure);
            //switch (theType)
            //{
            //    case Graphic3d_TOC_DESCENDANT:
            //        {
            //            for (NCollection_IndexedMap<Graphic3d_Structure*>::Iterator anIter (theStructure->myDescendants); anIter.More(); anIter.Next())
            //            {
            //                Graphic3d_Structure.Network(anIter.Value(), theType, theSet);
            //            }
            //            break;
            //        }
            //    case Graphic3d_TOC_ANCESTOR:
            //        {
            //            for (NCollection_IndexedMap<Graphic3d_Structure*>::Iterator anIter (theStructure->myAncestors); anIter.More(); anIter.Next())
            //            {
            //                Graphic3d_Structure.Network(anIter.Value(), theType, theSet);
            //            }
            //            break;
            //        }
            //}

        }
        public void SetTransformPersistence(Graphic3d_TransformPers theTrsfPers)
        {
            if (IsDeleted())
            {
                return;
            }

            myCStructure.SetTransformPersistence(theTrsfPers);
        }

        //! Internal method which sets new transformation without calling graphic manager callbacks.
        public void GraphicTransform(TopLoc_Datum3D theTrsf)
        {
            if (myCStructure != null)
            {
                myCStructure.SetTransformation(theTrsf);
            }
        }

        //! Changes a sequence of clip planes slicing the structure on rendering.
        //! @param thePlanes [in] the set of clip planes.
        public void SetClipPlanes(Graphic3d_SequenceOfHClipPlane thePlanes)
        {
            if (myCStructure != null)
            {
                myCStructure.SetClipPlanes(thePlanes);
            }
        }

        //! Returns TRUE if the structure is transformed.
        public bool IsTransformed()
        {
            return myCStructure != null
                && myCStructure.Transformation() != null
                && myCStructure.Transformation().Form() != gp_TrsfForm.gp_Identity;
        }


        public void SetTransformation(TopLoc_Datum3D theTrsf)
        {
            if (IsDeleted())
                return;

            bool wasTransformed = IsTransformed();

            if (theTrsf != null
              && theTrsf.Trsf().Form() == gp_TrsfForm.gp_Identity)
            {
                myCStructure.SetTransformation(null);
            }
            else
            {
                myCStructure.SetTransformation(theTrsf);
            }

            // If transformation, no validation of hidden already calculated parts
            if (IsTransformed() || (!IsTransformed() && wasTransformed))
            {
                ReCompute();
            }

            myStructureManager.SetTransform(this, theTrsf);

            Update(true);
        }
        //! Returns true if structure has mutable nature (content or location are be changed regularly).
        //! Mutable structure will be managed in different way than static onces.
        public bool IsMutable()
        {
            return myCStructure != null
                && myCStructure.IsMutable;
        }

        public void SetZLayer(Graphic3d_ZLayerId theLayerId)
        {
            // if the structure is not displayed, unable to change its display layer
            if (IsDeleted())
                return;

            myStructureManager.ChangeZLayer(this, theLayerId);
            myCStructure.SetZLayer(theLayerId);
        }

        public void SetVisual(Graphic3d_TypeOfStructure theVisual)
        {
            if (IsDeleted()
             || myVisual == theVisual)
            {
                return;
            }

            if (myCStructure.stick == 0)
            {
                myVisual = theVisual;
                SetComputeVisual(theVisual);
            }
            else
            {
                erase();
                myVisual = theVisual;
                SetComputeVisual(theVisual);
                Display();
            }
        }
        public void SetComputeVisual(Graphic3d_TypeOfStructure theVisual)
        {
            // The ComputeVisual is saved only if the structure is declared TOS_ALL, TOS_WIREFRAME or TOS_SHADING.
            // This declaration permits to calculate proper representation of the structure calculated by Compute instead of passage to TOS_COMPUTED.
            if (theVisual != Graphic3d_TypeOfStructure.Graphic3d_TOS_COMPUTED)
            {
                myComputeVisual = theVisual;
            }
        }

        //! Erases this structure in all the views of the visualiser.
        public virtual void Erase() { erase(); }
        public void ReCompute()
        {
            myStructureManager.ReCompute(this);
        }
        public Graphic3d_TypeOfStructure ComputeVisual() { return myComputeVisual; }
        Graphic3d_TypeOfStructure myComputeVisual;

        void erase()
        {
            if (IsDeleted())
            {
                return;
            }

            if (myCStructure.stick != 0)
            {
                myCStructure.stick = 0;
                myStructureManager.Erase(this);
            }
        }
        //
        //! Alias for porting code.
        //typedef Graphic3d_Structure Prs3d_Presentation;
        //! Returns the groups sequence included in this structure.
        public Graphic3d_SequenceOfGroup Groups()
        {
            return myCStructure.Groups();
        }
        //! Returns the visualisation mode for the structure <me>.
        public Graphic3d_TypeOfStructure Visual() { return myVisual; }
        Graphic3d_TypeOfStructure myVisual;

        //! Returns the display indicator for this structure.
        public virtual bool IsDisplayed()
        {
            return myCStructure != null
                && myCStructure.stick != 0;
        }
        //=============================================================================
        //function : StructureManager
        //purpose  :
        //=============================================================================
        public Graphic3d_StructureManager StructureManager()
        {
            return myStructureManager;

        }
        public virtual void Display()
        {

            if (IsDeleted()) return;

            if (myCStructure.stick == 0)
            {
                myCStructure.stick = 1;
                myStructureManager.Display(this);
            }

            if (myCStructure.visible != 1)
            {
                myCStructure.visible = 1;
                myCStructure.OnVisibilityChanged();
            }

        }

        //! Returns <ASet> the group of structures :
        //! - directly or indirectly connected to <AStructure> if the
        //! TypeOfConnection == TOC_DESCENDANT
        //! - to which <AStructure> is directly or indirectly connected
        //! if the TypeOfConnection == TOC_ANCESTOR
        public static void Network(Graphic3d_Structure theStructure,

                                        Graphic3d_TypeOfConnection theType,
                                       NCollection_Map<Graphic3d_Structure> theSet)
        {
            theSet.Add(theStructure);
            switch (theType)
            {
                case Graphic3d_TypeOfConnection.Graphic3d_TOC_DESCENDANT:
                    {
                        for (NCollection_IndexedMap<Graphic3d_Structure>.Iterator anIter =new(theStructure.myDescendants); anIter.More(); anIter.Next())                            
                        {
                            Network(anIter.Value(), theType, theSet);
                        }
                        break;
                    }
                case Graphic3d_TypeOfConnection.Graphic3d_TOC_ANCESTOR:
                    {
                        for (NCollection_IndexedMap<Graphic3d_Structure>.Iterator anIter=new (theStructure.myAncestors); anIter.More(); anIter.Next())
                        {
                            Network(anIter.Value(), theType, theSet);
                        }
                        break;
                    }
            }
        }


        public void SetVisible(bool theValue)
        {
            if (IsDeleted())
                return;

            int isVisible = theValue ? 1 : 0;
            if (myCStructure.visible == isVisible)
            {
                return;
            }

            myCStructure.visible = isVisible;
            myCStructure.OnVisibilityChanged();
            Update(true);
        }
        public void GraphicConnect(Graphic3d_Structure theDaughter)
        {
            if (myCStructure != null)
            {
                myCStructure.Connect(theDaughter.myCStructure);
            }
        }

        public static bool AcceptConnection(Graphic3d_Structure theStructure1,
                                                        Graphic3d_Structure theStructure2,
                                                        Graphic3d_TypeOfConnection theType)
        {
            // cycle detection
            NCollection_Map<Graphic3d_Structure> aSet = new NCollection_Map<Graphic3d_Structure>();
            Network(theStructure2, theType, aSet);
            return !aSet.Contains(theStructure1);
        }

        public void Connect(Graphic3d_Structure theStructure,
                                    Graphic3d_TypeOfConnection theType,
                                    bool theWithCheck = false)
        {
            if (IsDeleted())
            {
                return;
            }

            // cycle detection
            if (theWithCheck
             && !AcceptConnection(this, theStructure, theType))
            {
                return;
            }

            if (theType == Graphic3d_TypeOfConnection.Graphic3d_TOC_DESCENDANT)
            {
                if (!AppendDescendant(theStructure))
                {
                    return;
                }

                CalculateBoundBox();
                theStructure.Connect(this, Graphic3d_TypeOfConnection.Graphic3d_TOC_ANCESTOR);

                GraphicConnect(theStructure);
                myStructureManager.Connect(this, theStructure);

                Update(true);
            }
            else // Graphic3d_TOC_ANCESTOR
            {
                if (!AppendAncestor(theStructure))
                {
                    return;
                }

                CalculateBoundBox();
                theStructure.Connect(this, Graphic3d_TypeOfConnection.Graphic3d_TOC_DESCENDANT);

                // myStructureManager->Connect is called in case if connection between parent and child
            }
        }
        public bool AppendDescendant(Graphic3d_Structure theDescendant)
        {
            int aSize = myDescendants.Size();
            return myDescendants.Add(theDescendant) > aSize; // new object
        }



        public bool AppendAncestor(Graphic3d_Structure theAncestor)
        {
            int aSize = myAncestors.Size();

            return myAncestors.Add(theAncestor) > aSize; // new object
        }
        Graphic3d_BndBox4f minMaxCoord()
        {
            Graphic3d_BndBox4f aBnd = new Graphic3d_BndBox4f();
            //for (Graphic3d_SequenceOfGroup::Iterator aGroupIter (myCStructure->Groups()); aGroupIter.More(); aGroupIter.Next())
            foreach (var aGroupIter in myCStructure.Groups())
            {
                if (aGroupIter.TransformPersistence() != null)
                {
                    continue; // should be translated to current view orientation to make sense
                }

                aBnd.Combine(aGroupIter.BoundingBox());
            }
            return aBnd;
        }

        public void CalculateBoundBox()
        {
            Graphic3d_BndBox3d aBox = new Graphic3d_BndBox3d();
            addTransformed(aBox, true);
            myCStructure.ChangeBoundingBox(aBox);
        }

        //! if WithDestruction == Standard_True then
        //! suppress all the groups of primitives in the structure.
        //! and it is mandatory to create a new group in <me>.
        //! if WithDestruction == Standard_False then
        //! clears all the groups of primitives in the structure.
        //! and all the groups are conserved and empty.
        //! They will be erased at the next screen update.
        //! The structure itself is conserved.
        //! The transformation and the attributes of <me> are conserved.
        //! The childs of <me> are conserved.
        public virtual void Clear(bool WithDestruction = true)
        {
            clear(WithDestruction);
        }

        public void clear(bool theWithDestruction)
        {
            if (IsDeleted())
                return;

            // clean groups in graphics driver at first
            GraphicClear(theWithDestruction);

            myCStructure.SetGroupTransformPersistence(false);
            myStructureManager.Clear(this, theWithDestruction);

            Update(true);
        }

        NCollection_IndexedMap<Graphic3d_Structure, NCollection_DefaultHasher<Graphic3d_Structure>> myDescendants = new NCollection_IndexedMap<Graphic3d_Structure, NCollection_DefaultHasher<Graphic3d_Structure>>();
        NCollection_IndexedMap<Graphic3d_Structure, NCollection_DefaultHasher<Graphic3d_Structure>> myAncestors = new NCollection_IndexedMap<Graphic3d_Structure, NCollection_DefaultHasher<Graphic3d_Structure>>();
        void getBox(ref Graphic3d_BndBox3d theBox,
                                  bool theToIgnoreInfiniteFlag)
        {
            Graphic3d_BndBox4f aBoxF = minMaxCoord();

            if (aBoxF.IsValid())
            {
                theBox = new Graphic3d_BndBox3d(new Graphic3d_Vec3d((double)aBoxF.CornerMin().X,
                                                              (double)aBoxF.CornerMin().Y,
                                                              (double)aBoxF.CornerMin().Z),
                                             new Graphic3d_Vec3d((double)aBoxF.CornerMax().X,
                                                              (double)aBoxF.CornerMax().Y,
                                                              (double)aBoxF.CornerMax().Z));
                if (IsInfinite()
                && !theToIgnoreInfiniteFlag)
                {
                    Graphic3d_Vec3d aDiagVec = new Graphic3d_Vec3d(theBox.CornerMax() - theBox.CornerMin());
                    if (aDiagVec.SquareModulus() >= 500000.0 * 500000.0)
                    {
                        // bounding borders of infinite line has been calculated as own point in center of this line
                        theBox = new Graphic3d_BndBox3d((theBox.CornerMin() + theBox.CornerMax()) * 0.5);
                    }
                    else
                    {
                        theBox = new Graphic3d_BndBox3d(new Graphic3d_Vec3d(
                            Standard_Real.RealFirst(),
                            Standard_Real.RealFirst(),
                            Standard_Real.RealFirst()),
                                                  new Graphic3d_Vec3d(
                                                      Standard_Real.RealLast(),
                                                      Standard_Real.RealLast(),
                                                      Standard_Real.RealLast()));
                        return;
                    }
                }
            }
        }
        void addTransformed(Graphic3d_BndBox3d theBox,
                                          bool theToIgnoreInfiniteFlag)
        {
            Graphic3d_BndBox3d aCombinedBox = new Graphic3d_BndBox3d(), aBox = new Graphic3d_BndBox3d();
            getBox(ref aCombinedBox, theToIgnoreInfiniteFlag);

            for (NCollection_IndexedMap<Graphic3d_Structure>.Iterator anIter =new(myDescendants); anIter.More(); anIter.Next())            
            {
                Graphic3d_Structure aStruct = anIter.Value();
                aStruct.getBox(ref aBox, theToIgnoreInfiniteFlag);
                aCombinedBox.Combine(aBox);
            }

            aBox = aCombinedBox;
            if (aBox.IsValid())
            {
                if (myCStructure.Transformation() != null)
                {
                    var minv = aBox.CornerMin();
                    var maxv = aBox.CornerMax();
                    TransformBoundaries(myCStructure.Transformation().Trsf(),
                         ref minv.v[0], ref minv.v[1], ref minv.v[2],
                         ref maxv.v[0], ref maxv.v[1], ref maxv.v[2]);
                    aBox.myMaxPoint = maxv;
                    aBox.myMinPoint = minv;
                }

                // if box is still valid after transformation
                if (aBox.IsValid())
                {
                    theBox.Combine(aBox);
                }
                else // it was infinite, return untransformed
                {
                    theBox.Combine(aCombinedBox);
                }
            }
        }
        public static void Transforms(gp_Trsf theTrsf,
                                       double theX, double theY, double theZ,
                                       out double theNewX, out double theNewY, out double theNewZ)
        {
            double aRL = Standard_Real.RealLast();
            double aRF = Standard_Real.RealFirst();
            theNewX = theX;
            theNewY = theY;
            theNewZ = theZ;
            if ((theX == aRF) || (theY == aRF) || (theZ == aRF)
             || (theX == aRL) || (theY == aRL) || (theZ == aRL))
            {
                return;
            }

            theTrsf.Transforms(ref theNewX, ref theNewY, ref theNewZ);
        }

        public void TransformBoundaries(gp_Trsf theTrsf,
                                            ref double theXMin,
                                            ref double theYMin,
                                              ref double theZMin,
                                              ref double theXMax,
                                              ref double theYMax,
                                              ref double theZMax)
        {
            double aXMin, aYMin, aZMin, aXMax, aYMax, aZMax, anU, aV, aW;

            Transforms(theTrsf, theXMin, theYMin, theZMin, out aXMin, out aYMin, out aZMin);
            Transforms(theTrsf, theXMax, theYMax, theZMax, out aXMax, out aYMax, out aZMax);

            Transforms(theTrsf, theXMin, theYMin, theZMax, out anU, out aV, out aW);
            aXMin = Math.Min(anU, aXMin); aXMax = Math.Max(anU, aXMax);
            aYMin = Math.Min(aV, aYMin); aYMax = Math.Max(aV, aYMax);
            aZMin = Math.Min(aW, aZMin); aZMax = Math.Max(aW, aZMax);

            Transforms(theTrsf, theXMax, theYMin, theZMax, out anU, out aV, out aW);
            aXMin = Math.Min(anU, aXMin); aXMax = Math.Max(anU, aXMax);
            aYMin = Math.Min(aV, aYMin); aYMax = Math.Max(aV, aYMax);
            aZMin = Math.Min(aW, aZMin); aZMax = Math.Max(aW, aZMax);

            Transforms(theTrsf, theXMax, theYMin, theZMin, out anU, out aV, out aW);
            aXMin = Math.Min(anU, aXMin); aXMax = Math.Max(anU, aXMax);
            aYMin = Math.Min(aV, aYMin); aYMax = Math.Max(aV, aYMax);
            aZMin = Math.Min(aW, aZMin); aZMax = Math.Max(aW, aZMax);

            Transforms(theTrsf, theXMax, theYMax, theZMin, out anU, out aV, out aW);
            aXMin = Math.Min(anU, aXMin); aXMax = Math.Max(anU, aXMax);
            aYMin = Math.Min(aV, aYMin); aYMax = Math.Max(aV, aYMax);
            aZMin = Math.Min(aW, aZMin); aZMax = Math.Max(aW, aZMax);

            Transforms(theTrsf, theXMin, theYMax, theZMax, out anU, out aV, out aW);
            aXMin = Math.Min(anU, aXMin); aXMax = Math.Max(anU, aXMax);
            aYMin = Math.Min(aV, aYMin); aYMax = Math.Max(aV, aYMax);
            aZMin = Math.Min(aW, aZMin); aZMax = Math.Max(aW, aZMax);

            Transforms(theTrsf, theXMin, theYMax, theZMin, out anU, out aV, out aW);
            aXMin = Math.Min(anU, aXMin); aXMax = Math.Max(anU, aXMax);
            aYMin = Math.Min(aV, aYMin); aYMax = Math.Max(aV, aYMax);
            aZMin = Math.Min(aW, aZMin); aZMax = Math.Max(aW, aZMax);

            theXMin = aXMin;
            theYMin = aYMin;
            theZMin = aZMin;
            theXMax = aXMax;
            theYMax = aYMax;
            theZMax = aZMax;
        }

        private void Update(bool theUpdateLayer)
        {
            if (IsDeleted())
            {
                return;
            }

            myStructureManager.Update(theUpdateLayer ? myCStructure.ZLayer() : Graphic3d_ZLayerId.Graphic3d_ZLayerId_UNKNOWN);

        }


        //! Returns the highlight indicator for this structure.
        public bool IsHighlighted()
        {
            return myCStructure != null && myCStructure.highlight != 0;
        }

        //! Returns Standard_True if the structure <me> is infinite.
        public bool IsInfinite()
        {
            return IsDeleted() || myCStructure.IsInfinite;
        }

        public bool IsDeleted()
        {
            return myCStructure == null;
        }

        public bool IsVisible()
        {
            return myCStructure != null && myCStructure.visible != 0;
        }

        public Graphic3d_TransformPers TransformPersistence()
        {
            return myCStructure.TransformPersistence();
        }

        public Graphic3d_CStructure CStructure()
        {
            return myCStructure;
        }

        public Graphic3d_CStructure myCStructure;

        Graphic3d_StructureManager myStructureManager;


        public Graphic3d_Group NewGroup()
        {
            return myCStructure.NewGroup(this);
        }

        public void SetInfiniteState(bool v)
        {
            throw new NotImplementedException();
        }

        public virtual void Compute()
        {

        }
        public void SetIsForHighlight(bool isForHighlight)
        {
            if (myCStructure != null)
            {
                myCStructure.IsForHighlight = isForHighlight;
            }
        }



        public void GraphicClear(bool theWithDestruction)
        {
            //throw new NotImplementedException();
        }

        internal void SetHLRValidation(bool v)
        {
            //throw new NotImplementedException();
        }

        public int Identification()
        {
            return myCStructure.Identification();
        }
        //! Returns the current display priority for this structure.
        public Graphic3d_DisplayPriority DisplayPriority()
        {
            return myCStructure.Priority();
        }

        //! Get Z layer ID of displayed structure.
        //! The method returns -1 if the structure has no ID (deleted from graphic driver).
        public Graphic3d_ZLayerId GetZLayer()
        {
            return myCStructure.ZLayer();
        }

        internal void Highlight(object value, bool v)
        {

        }


    }

    public class Graphic3d_BndBox3d : BVH_Box<Graphic3d_Vec3d, Graphic3d_Vec3d_BoxMinMax>
    {


        public Graphic3d_BndBox3d()
        {
            myIsInited = false;
        }
        public Graphic3d_BndBox3d(Graphic3d_Vec3d min, Graphic3d_Vec3d max)
        {
            myIsInited = true;
            myMaxPoint = max;
            myMinPoint = min;
        }
        public Graphic3d_BndBox3d(Graphic3d_Vec3d min)
        {
            myIsInited = true;
            myMaxPoint = min;
            myMinPoint = min;
        }

    }

    public class Graphic3d_Vec3d_BoxMinMax : IBoxMinMax<Graphic3d_Vec3d>
    {
        public void CwiseMax(ref Graphic3d_Vec3d theVec1, Graphic3d_Vec3d theVec2)
        {
            theVec1.X = Math.Max(theVec1.X, theVec2.X);
            theVec1.Y = Math.Max(theVec1.Y, theVec2.Y);
            theVec1.Z = Math.Max(theVec1.Z, theVec2.Z);
        }

        public Graphic3d_Vec3d CwiseMax(Graphic3d_Vec3d theVec1, Graphic3d_Vec3d theVec2)
        {
            throw new NotImplementedException();
        }

        public void CwiseMin(ref Graphic3d_Vec3d theVec1, Graphic3d_Vec3d theVec2)
        {
            theVec1.X = Math.Min(theVec1.X, theVec2.X);
            theVec1.Y = Math.Min(theVec1.Y, theVec2.Y);
            theVec1.Z = Math.Min(theVec1.Z, theVec2.Z);
        }

        public Graphic3d_Vec3d CwiseMin(Graphic3d_Vec3d theVec1, Graphic3d_Vec3d theVec2)
        {
            throw new NotImplementedException();
        }
    }
}
