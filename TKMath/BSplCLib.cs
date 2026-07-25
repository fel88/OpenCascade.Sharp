namespace TKMath
{
    //! BSplCLib   B-spline curve Library.
    //!
    //! The BSplCLib package is  a basic library  for BSplines. It
    //! provides three categories of functions.
    //!
    //! * Management methods to  process knots and multiplicities.
    //!
    //! * Multi-Dimensions  spline methods.  BSpline methods where
    //! poles have an arbitrary number of dimensions. They divides
    //! in two groups :
    //!
    //! - Global methods modifying the  whole set of  poles. The
    //! poles are    described   by an array   of   Reals and  a
    //! Dimension. Example : Inserting knots.
    //!
    //! - Local methods  computing  points and derivatives.  The
    //! poles  are described by a pointer  on  a local array  of
    //! Reals and a Dimension. The local array is modified.
    //!
    //! *  2D  and 3D spline   curves  methods.
    //!
    //! Methods  for 2d and 3d BSplines  curves  rational or not
    //! rational.
    //!
    //! Those methods have the following structure :
    //!
    //! - They extract the pole information in a working array.
    //!
    //! -  They      process the  working   array    with   the
    //! multi-dimension  methods. (for example  a  3d  rational
    //! curve is processed as a 4 dimension curve).
    //!
    //! - They get back the result in the original dimension.
    //!
    //! Note that the  bspline   surface methods found   in the
    //! package BSplSLib  uses  the same  structure and rely on
    //! BSplCLib.
    //!
    //! In the following list  of methods the  2d and 3d  curve
    //! methods   will be  described   with  the  corresponding
    //! multi-dimension method.
    //!
    //! The 3d or 2d B-spline curve is defined with :
    //!
    //! . its control points : TColgp_Array1OfPnt(2d)        Poles
    //! . its weights        : TColStd_Array1OfReal          Weights
    //! . its knots          : TColStd_Array1OfReal          Knots
    //! . its multiplicities : TColStd_Array1OfInteger       Mults
    //! . its degree         : Standard_Integer              Degree
    //! . its periodicity    : Standard_Boolean              Periodic
    //!
    //! Warnings :
    //! The bounds of Poles and Weights should be the same.
    //! The bounds of Knots and Mults   should be the same.
    //!
    //! Note: weight and multiplicity arrays can be passed by pointer for
    //! some functions so that NULL pointer is valid.
    //! That means no weights/no multiplicities passed.
    //!
    //! No weights (BSplCLib::NoWeights()) means the curve is non rational.
    //! No mults (BSplCLib::NoMults()) means the knots are "flat" knots.
    //!
    //! KeyWords :
    //! B-spline curve, Functions, Library
    //!
    //! References :
    //! . A survey of curves and surfaces methods in CADG Wolfgang
    //! BOHM CAGD 1 (1984)
    //! . On de Boor-like algorithms and blossoming Wolfgang BOEHM
    //! cagd 5 (1988)
    //! . Blossoming and knot insertion algorithms for B-spline curves
    //! Ronald N. GOLDMAN
    //! . Modelisation des surfaces en CAO, Henri GIAUME Peugeot SA
    //! . Curves and Surfaces for Computer Aided Geometric Design,
    //! a practical guide Gerald Farin
    public class BSplCLib
    {
    }
}
