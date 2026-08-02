using OCCPort.Common;
using System.Reflection.Metadata;
using TKMath;

namespace TKG3d
{
    //! Describes a surface of revolution (revolved surface).
    //! Such a surface is obtained by rotating a curve (called
    //! the "meridian") through a complete revolution about
    //! an axis (referred to as the "axis of revolution"). The
    //! curve and the axis must be in the same plane (the
    //! "reference plane" of the surface).
    //! Rotation around the axis of revolution in the
    //! trigonometric sense defines the u parametric
    //! direction. So the u parameter is an angle, and its
    //! origin is given by the position of the meridian on the surface.
    //! The parametric range for the u parameter is: [ 0, 2.*Pi ]
    //! The v parameter is that of the meridian.
    //! Note: A surface of revolution is built from a copy of the
    //! original meridian. As a result the original meridian is
    //! not modified when the surface is modified.
    //! The form of a surface of revolution is typically a
    //! general revolution surface
    //! (GeomAbs_RevolutionForm). It can be:
    //! - a conical surface, if the meridian is a line or a
    //! trimmed line (GeomAbs_ConicalForm),
    //! - a cylindrical surface, if the meridian is a line or a
    //! trimmed line parallel to the axis of revolution
    //! (GeomAbs_CylindricalForm),
    //! - a planar surface if the meridian is a line or a
    //! trimmed line perpendicular to the axis of revolution
    //! of the surface (GeomAbs_PlanarForm),
    //! - a toroidal surface, if the meridian is a circle or a
    //! trimmed circle (GeomAbs_ToroidalForm), or
    //! - a spherical surface, if the meridian is a circle, the
    //! center of which is located on the axis of the
    //! revolved surface (GeomAbs_SphericalForm).
    //! Warning
    //! Be careful not to construct a surface of revolution
    //! where the curve and the axis or revolution are not
    //! defined in the same plane. If you do not have a
    //! correct configuration, you can correct your initial
    //! curve, using a cylindrical projection in the reference plane.
    public class Geom_SurfaceOfRevolution : Geom_SweptSurface
    {

        //! Computes the  point P (U, V) on the surface.
        //! U is the angle of the rotation around the revolution axis.
        //! The direction of this axis gives the sense of rotation.
        //! V is the parameter of the revolved curve.
        public override void D0(double U, double V, ref gp_Pnt P)
        {
            myEvaluator.D0(U, V, out P);

        }

        //! Returns the parametric bounds U1, U2 , V1 and V2 of this surface.
        //! A surface of revolution is always complete, so U1 = 0, U2 = 2*PI.
        public override void Bounds(out double U1, out double U2, out double V1, out double V2)
        {
            U1 = 0.0;
            U2 = 2.0 * Math.PI;
            V1 = basisCurve.FirstParameter();
            V2 = basisCurve.LastParameter();
        }

        //! C : is the meridian  or the referenced curve.
        //! A1 is the axis of revolution.
        //! The form of a SurfaceOfRevolution can be :
        //! . a general revolution surface (RevolutionForm),
        //! . a conical surface if the meridian is a line or a trimmed line
        //! (ConicalForm),
        //! . a cylindrical surface if the meridian is a line or a trimmed
        //! line parallel to the revolution axis (CylindricalForm),
        //! . a planar surface if the meridian is a line perpendicular to
        //! the revolution axis of the surface (PlanarForm).
        //! . a spherical surface,
        //! . a toroidal surface,
        //! . a quadric surface.
        //! Warnings :
        //! It is not checked that the curve C is planar and that the
        //! surface axis is in the plane of the curve.
        //! It is not checked that the revolved curve C doesn't
        //! self-intersects.
        public Geom_SurfaceOfRevolution(Geom_Curve C, gp_Ax1 A1)
        {
            loc = (A1.Location());
            direction = A1.Direction();
            SetBasisCurve(C);
        }

        //! Changes the revolved curve of the surface.
        //! Warnings :
        //! It is not checked that the curve C is planar and that the
        //! surface axis is in the plane of the curve.
        //! It is not checked that the revolved curve C doesn't
        //! self-intersects.
        public void SetBasisCurve(Geom_Curve C)
        {
            basisCurve = (Geom_Curve)(C.Copy());
            smooth = C.Continuity();
            myEvaluator = new GeomEvaluator_SurfaceOfRevolution(basisCurve, direction, loc);
        }

        public override Geom_Curve UIso(double U)
        {
            throw new NotImplementedException();
        }

        public override Geom_Curve VIso(double V)
        {
            throw new NotImplementedException();
        }

        public override bool IsUClosed()
        {
            return true;
        }

        public override bool IsVClosed()
        {
            return basisCurve.IsClosed();
        }

        public override void D1(double U, double V, out gp_Pnt P, out gp_Vec D1U, out gp_Vec D1V)
        {
            myEvaluator.D1(U, V, out P, out D1U, out D1V);
        }

        public override void D2(double U, double V, out gp_Pnt P, out gp_Vec D1U, out gp_Vec D1V, out gp_Vec D2U, out gp_Vec D2V, out gp_Vec D2UV)
        {
            throw new NotImplementedException();
        }

        public override bool IsUPeriodic()
        {
            return true;
        }

        public override bool IsVPeriodic()
        {
            return basisCurve.IsPeriodic();

        }
       public  void UReverse()
        {
            direction.Reverse();
            myEvaluator.SetDirection(direction);
        }

        public override void Transform(gp_Trsf T)
        {
            loc.Transform(T);
            direction.Transform(T);
            basisCurve.Transform(T);
            if (T.ScaleFactor() * T.HVectorialPart().Determinant() < 0.0) UReverse();
            myEvaluator.SetDirection(direction);
            myEvaluator.SetLocation(loc);
        }

        public gp_Ax1 Axis()
        {

            return new gp_Ax1(loc, direction);
        }
        public override Geom_Geometry Copy()
        {
            return new Geom_SurfaceOfRevolution(basisCurve, Axis());
        }

        GeomEvaluator_SurfaceOfRevolution myEvaluator;

        gp_Pnt loc;

    }
}
