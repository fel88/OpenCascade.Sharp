using TKernel;
using TKService;

namespace TKV3d
{
    //! A graphic attribute manager which governs how
    //! A framework to define the display attributes of isoparameters.
    //! This framework can be used to modify the default
    //! setting for isoparameters in Prs3d_Drawer.
    public class Prs3d_IsoAspect : Prs3d_LineAspect
    {
        //! Constructs a framework to define display attributes of isoparameters.
        //! These include:
        //! -   the color attribute aColor
        //! -   the type of line aType
        //! -   the width value aWidth
        //! -   aNumber, the number of isoparameters to be   displayed.
        public Prs3d_IsoAspect(Quantity_Color theColor,
            Aspect_TypeOfLine theType,
            double theWidth,
            int theNumber)
            : base(theColor, theType, theWidth)
        {
            myNumber = (theNumber);
        }

        //! defines the number of U or V isoparametric curves
        //! to be drawn for a single face.
        //! Default value: 10
        public void SetNumber(int theNumber) { myNumber = theNumber; }

        //! returns the number of U or V isoparametric curves drawn for a single face.
        public int Number() { return myNumber; }
        protected int myNumber;
    }
}

