namespace TKXSBASE
{
    //! This class gives a frame for Actions which modify the effect
    //! of a Dispatch, i.e. :
    //! By Selections and Dispatches, an original Model can be
    //! split into one or more "target" Models : these Models
    //! contain Entities copied from the original one (that is, a
    //! part of it). Basically, these dispatched Entities are copied
    //! as identical to their original counterparts. Also the copied
    //! Models reproduce the Header of the original one.
    //!
    //! Modifiers allow to change this copied content : this is the
    //! way to be used for any kind of alterations, adaptations ...
    //! They are exploited by a ModelCopier, which firstly performs
    //! the copy operation described by Dispatches, then invokes the
    //! Modifiers to work on the result.
    //!
    //! Each GeneralModifier can be attached to :
    //! - all the Models produced
    //! - a Dispatch (it will be applied to all the Models obtained
    //! from this Dispatch) designated by its Ident in a ShareOut
    //! - in addition, to a Selection (facultative) : this adds a
    //! criterium, the Modifier is invoked on a produced Model only
    //! if this Model contains an Entity copied from one of the
    //! Entities designated by this Selection.
    //! (for special Modifiers from IFAdapt, while they must work on
    //! definite Entities, this Selection is mandatory to run)
    //!
    //! Remark : this class has no action attached, it only provides
    //! a frame to work on criteria. Then, sub-classes will define
    //! their kind of action, which can be applied at a precise step
    //! of the production of a File : see Modifier, and in the
    //! package IFAdapt, EntityModifier and EntityCopier
    public class IFSelect_GeneralModifier 
    {
    }
}