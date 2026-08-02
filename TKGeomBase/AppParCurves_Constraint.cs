namespace TKGeomBase
{
    //! -   NoConstraint: this point has no constraints.
    //! -   PassPoint: the approximation curve passes through this point.
    //! -   TangencyPoint: this point has a tangency constraint.
    //! -   CurvaturePoint: this point has a curvature constraint.
    public enum AppParCurves_Constraint
    {
        AppParCurves_NoConstraint,
        AppParCurves_PassPoint,
        AppParCurves_TangencyPoint,
        AppParCurves_CurvaturePoint
    };
    }


