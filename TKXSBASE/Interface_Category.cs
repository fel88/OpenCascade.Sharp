namespace TKXSBASE
{
    //! This class manages categories
    //! A category is defined by a name and a number, and can be
    //! seen as a way of rough classification, i.e. less precise than
    //! a cdl type.
    //! Hence, it is possible to dispatch every entity in about
    //! a dozen of categories, twenty is a reasonable maximum.
    //!
    //! Basically, the system provides the following categories :
    //! Shape (Geometry, BRep, CSG, Features, etc...)
    //! Drawing (Drawing, Views, Annotations, Pictures, Sketches ...)
    //! Structure (Component & Part, Groups & Patterns ...)
    //! Description (Meta-Data : Relations, Properties, Product ...)
    //! Auxiliary   (those which do not enter in the above list)
    //! and some dedicated categories
    //! FEA , Kinematics , Piping , etc...
    //! plus Professional  for other dedicated non-classed categories
    //!
    //! In addition, this class provides a way to compute then quickly
    //! query category numbers for an entire model.
    //! Values are just recorded as a list of numbers, control must
    //! then be done in a wider context (which must provide a Graph)
    public class Interface_Category
    {

    }
}


