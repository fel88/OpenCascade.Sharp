using OCCPort.Common;
using System.Reflection.Metadata;
using TKernel;
using TKMath;

namespace TKG3d
{
    //! Defines an isoparametric curve on  a surface.  The
    //! type  of isoparametric curve  (U  or V) is defined
    //! with the   enumeration  IsoType from   GeomAbs  if
    //! NoneIso is given an error is raised.
    public class Adaptor3d_IsoCurve : Adaptor3d_Curve
    {
        public override Geom_BSplineCurve BSpline()
        {
            throw new NotImplementedException();
        }
        public void Load(Adaptor3d_Surface S)
        {
            mySurface = S;
            myIso =GeomAbs_IsoType. GeomAbs_NoneIso;
        }
        public override gp_Circ Circle()
        {
            gp_Ax3 axes;
            double  radius, h = 0.0;

            switch (mySurface._GetType())
            {

                case GeomAbs_SurfaceType.GeomAbs_Cylinder:
                    {
                        gp_Cylinder cyl = mySurface.Cylinder();

                        switch (myIso)
                        {

                            case GeomAbs_IsoType.GeomAbs_IsoU:
                                {
                                    throw new Standard_NoSuchObject("Adaptor3d_IsoCurve:UIso");
                                }
                            case GeomAbs_IsoType.GeomAbs_IsoV:
                                {
                                    throw new NotImplementedException();
                                    //return ElSLib.CylinderVIso(cyl.Position(), cyl.Radius(), myParameter);
                                }
                                case GeomAbs_IsoType.GeomAbs_NoneIso:
                                {
                                    throw new Standard_NoSuchObject("Adaptor3d_IsoCurve:NoneIso");
                                }
                        }
                        break;
                    }

                case GeomAbs_SurfaceType. GeomAbs_Cone:
                    {
                        throw new NotImplementedException();
                        //gp_Cone cone = mySurface->Cone();

                        //switch (myIso)
                        //{

                        //    case GeomAbs_IsoU:
                        //        {
                        //            throw Standard_NoSuchObject("Adaptor3d_IsoCurve:UIso");
                        //        }
                        //    case GeomAbs_IsoV:
                        //        {
                        //            return ElSLib::ConeVIso(cone.Position(), cone.RefRadius(),
                        //                        cone.SemiAngle(), myParameter);
                        //        }
                        //    case GeomAbs_NoneIso:
                        //        {
                        //            throw Standard_NoSuchObject("Adaptor3d_IsoCurve:NoneIso");
                        //        }
                        //}
                        break;
                    }

                case GeomAbs_SurfaceType. GeomAbs_Sphere:
                    {
                        gp_Sphere sph = mySurface.Sphere();

                        switch (myIso)
                        {

                            case GeomAbs_IsoType.GeomAbs_IsoU:
                                {
                                    return ElSLib.SphereUIso(sph.Position(), sph.Radius(), myParameter);
                                }

                            case GeomAbs_IsoType.GeomAbs_IsoV:
                                {
                                    return ElSLib.SphereVIso(sph.Position(), sph.Radius(), myParameter);
                                }

                            case GeomAbs_IsoType.GeomAbs_NoneIso:
                                {
                                    throw new Standard_NoSuchObject("Adaptor3d_IsoCurve:NoneIso");
                                }
                        }
                        break;
                    }

                case GeomAbs_SurfaceType.GeomAbs_Torus:
                    {
                        throw new NotImplementedException();

                        //        gp_Torus tor = mySurface->Torus();

                        //        switch (myIso)
                        //        {

                        //            case GeomAbs_IsoU:
                        //                {
                        //                    return ElSLib::TorusUIso(tor.Position(), tor.MajorRadius(),
                        //                                 tor.MinorRadius(), myParameter);
                        //                }

                        //            case GeomAbs_IsoV:
                        //                {
                        //                    return ElSLib::TorusVIso(tor.Position(), tor.MajorRadius(),
                        //                                 tor.MinorRadius(), myParameter);
                        //                }

                        //            case GeomAbs_NoneIso:
                        //                {
                        //                    throw Standard_NoSuchObject("Adaptor3d_IsoCurve:NoneIso");
                        //                }
                        //        }
                        break;
                    }

                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution:
                    {
                        throw new NotImplementedException();

//                        if (myIso ==GeomAbs_IsoType. GeomAbs_IsoV)
//                        {
//                             gp_Pnt aVal0 = Value(0.0);
//                            gp_Ax1 Ax1 = mySurface.AxeOfRevolution();
//                            if (new gp_Lin(Ax1).Contains(aVal0, Precision.SquareConfusion.Confusion()))
//                            {
//                                return new gp_Circ(new gp_Ax2(aVal0, Ax1.Direction()), 0);
//                            }
//                            else
//                            {
//                                gp_Vec DX=new(Ax1.Location(), aVal0);
//                                axes = new gp_Ax3(Ax1.Location(), Ax1.Direction(), DX);
//                                computeHR(axes, aVal0, h, radius);
//                                gp_Vec VT = axes.Direction();
//                                axes.Translate(VT * h);
//                                return new gp_Circ(axes.Ax2(), radius);
//                            }
//                        }
//                        else
//                        {
//                            throw new NotImplementedException();
//                            ///return mySurface.BasisCurve().Circle().Rotated
////                              (mySurface.AxeOfRevolution(), myParameter);
                        //}
                    }

                //case GeomAbs_SurfaceOfExtrusion:
                //    {
                //        return mySurface->BasisCurve()->Circle().Translated
                //          (myParameter * gp_Vec(mySurface->Direction()));
                //    }
                default:
                    {
                        throw new Standard_NoSuchObject("Adaptor3d_IsoCurve:Circle");
                    }

            }
            // portage WNT
            return new gp_Circ();
        }

        public override  double  FirstParameter()   { return myFirst; }
        public override  double LastParameter()   { return myLast; }
          
Adaptor3d_Surface mySurface;
        GeomAbs_IsoType myIso;
        double myFirst;
        double myLast;
        double myParameter;

        public void Load(GeomAbs_IsoType Iso,

                 double Param,

                 double WFirst,

                 double WLast)
        {
            myIso = Iso;
            myParameter = Param;
            myFirst = WFirst;
            myLast = WLast;


            if (myIso == GeomAbs_IsoType.GeomAbs_IsoU)
            {
                myFirst = Math.Max(myFirst, mySurface.FirstVParameter());
                myLast = Math.Min(myLast, mySurface.LastVParameter());
            }
            else
            {
                myFirst = Math.Max(myFirst, mySurface.FirstUParameter());
                myLast = Math.Min(myLast, mySurface.LastUParameter());
            }

            // Adjust the parameters on periodic surfaces

            double dummy = myParameter;

            if (mySurface.IsUPeriodic())
            {

                if (myIso == GeomAbs_IsoType.GeomAbs_IsoU)
                {
                    ElCLib.AdjustPeriodic
                  (mySurface.FirstUParameter(),
                   mySurface.FirstUParameter() +
                   mySurface.UPeriod(),
                   mySurface.UResolution(Precision.Confusion()),
                  ref myParameter, ref dummy);
                }
                else
                {
                    ElCLib.AdjustPeriodic
                  (mySurface.FirstUParameter(),
                   mySurface.FirstUParameter() +
                   mySurface.UPeriod(),
                   mySurface.UResolution(Precision.Confusion()),
                  ref myFirst, ref myLast);
                }
            }

            if (mySurface.IsVPeriodic())
            {

                if (myIso == GeomAbs_IsoType.GeomAbs_IsoV)
                {
                    ElCLib.AdjustPeriodic
                  (mySurface.FirstVParameter(),
                   mySurface.FirstVParameter() +
                   mySurface.VPeriod(),
                   mySurface.VResolution(Precision.Confusion()),
                   ref myParameter, ref dummy);
                }
                else
                {
                    ElCLib.AdjustPeriodic
                  (mySurface.FirstVParameter(),
                   mySurface.FirstVParameter() +
                   mySurface.VPeriod(),
                   mySurface.VResolution(Precision.Confusion()),
                 ref myFirst, ref myLast);
                }
            }

        }

        public override void D0(double T, ref gp_Pnt P)
        {
            switch (myIso)
            {

                case GeomAbs_IsoType.GeomAbs_IsoU:
                    mySurface.D0(myParameter, T, ref P);
                    break;

                case GeomAbs_IsoType.GeomAbs_IsoV:
                    mySurface.D0(T, myParameter, ref P);
                    break;

                case GeomAbs_IsoType.GeomAbs_NoneIso:
                    throw new Standard_NoSuchObject("Adaptor3d_IsoCurve:NoneIso");
                    break;
            }
        }

        public override void D1(double d, out gp_Pnt p, out gp_Vec v)
        {
            throw new NotImplementedException();
        }

        public override void D2(double d, out gp_Pnt p, out gp_Vec v1, out gp_Vec v2)
        {
            throw new NotImplementedException();
        }

        public override int Degree()
        {
            throw new NotImplementedException();
        }

        public override void Intervals(TColStd_Array1OfReal TI, GeomAbs_Shape S)
        {
            if (myIso ==GeomAbs_IsoType. GeomAbs_NoneIso) throw new Standard_NoSuchObject();
            bool UIso = (myIso == GeomAbs_IsoType.GeomAbs_IsoU);

            int nbInter = UIso ?
                mySurface.NbVIntervals(S) :
                mySurface.NbUIntervals(S);

            TColStd_Array1OfReal T=new(1,nbInter + 1);

            if (UIso)
                mySurface.VIntervals(T, S);
            else
                mySurface.UIntervals(T, S);

            if (nbInter == 1)
            {
                TI[(TI.Lower())] = myFirst;
                TI[(TI.Lower() + 1)] = myLast;
                return;
            }

            int first = 1;
            while (T[(first)] <= myFirst) first++;
            int last = nbInter + 1;
            while (T[(last)] >= myLast) last--;

            int i = TI.Lower(), j;
            for (j = first - 1; j <= last + 1; j++)
            {
                TI[(i)] = T[(j)];
                i++;
            }
            TI[(TI.Lower())] = myFirst;
            TI[(TI.Lower() + last - first + 2)] = myLast;
        }

        public override bool IsPeriodic()
        {
            throw new NotImplementedException();
        }

        public override gp_Lin Line()
        {
            throw new NotImplementedException();
        }

        public override int NbIntervals(GeomAbs_Shape S)
        {
            if (myIso == GeomAbs_IsoType.GeomAbs_NoneIso) throw new Standard_NoSuchObject();
            bool UIso = (myIso ==GeomAbs_IsoType. GeomAbs_IsoU);

            int nbInter = UIso ?
                mySurface.NbVIntervals(S) :
                mySurface.NbUIntervals(S);

            TColStd_Array1OfReal T=new(1,nbInter + 1);

            if (UIso)
                mySurface.VIntervals(T, S);
            else
                mySurface.UIntervals(T, S);

            if (nbInter == 1) return nbInter;

            int first = 1;
            while (T[(first)] <= myFirst) first++;
            int last = nbInter + 1;
            while (T[(last)] >= myLast) last--;
            return (last - first + 2);
        }

        public override int NbKnots()
        {
            throw new NotImplementedException();
        }

        public override double Period()
        {
            throw new NotImplementedException();
        }

        public override double Resolution(double R3d)
        {
            throw new NotImplementedException();
        }

        public override gp_Pnt Value(double T)
        {
            switch (myIso)
            {
                case GeomAbs_IsoType.GeomAbs_IsoU:
                    return mySurface.Value(myParameter, T);

                case GeomAbs_IsoType.GeomAbs_IsoV:
                    return mySurface.Value(T, myParameter);

                case GeomAbs_IsoType.GeomAbs_NoneIso:
                    {
                        throw new Standard_NoSuchObject("Adaptor3d_IsoCurve:NoneIso");
                        break;
                    }
            }
            // portage WNT
            return new gp_Pnt();
        }

        public override GeomAbs_CurveType _GetType()
        {
            switch (mySurface._GetType())
            {

                case GeomAbs_SurfaceType.GeomAbs_Plane:
                    return GeomAbs_CurveType. GeomAbs_Line;

                case GeomAbs_SurfaceType.GeomAbs_Cylinder:
                case GeomAbs_SurfaceType.GeomAbs_Cone:
                    {
                        switch (myIso)
                        {
                            case GeomAbs_IsoType.GeomAbs_IsoU:
                                return GeomAbs_CurveType.GeomAbs_Line;

                            case GeomAbs_IsoType.GeomAbs_IsoV:
                                return GeomAbs_CurveType.GeomAbs_Circle;

                            case GeomAbs_IsoType.GeomAbs_NoneIso:
                                {
                                    throw new Standard_NoSuchObject("Adaptor3d_IsoCurve:NoneIso");
                                }
                        }
                        break;
                    }

                case GeomAbs_SurfaceType.GeomAbs_Sphere:
                case GeomAbs_SurfaceType.GeomAbs_Torus:
                    return GeomAbs_CurveType. GeomAbs_Circle;

                case GeomAbs_SurfaceType.GeomAbs_BezierSurface:
                    return  GeomAbs_CurveType.GeomAbs_BezierCurve;

                case GeomAbs_SurfaceType.GeomAbs_BSplineSurface:
                    return GeomAbs_CurveType.GeomAbs_BSplineCurve;

                case GeomAbs_SurfaceType.GeomAbs_SurfaceOfRevolution:
                    {
                        switch (myIso)
                        {
                            case GeomAbs_IsoType.GeomAbs_IsoU:                                
                                return mySurface.BasisCurve()._GetType();

                            case GeomAbs_IsoType.GeomAbs_IsoV:
                                return GeomAbs_CurveType.GeomAbs_Circle;

                            case GeomAbs_IsoType.GeomAbs_NoneIso:
                                throw new Standard_NoSuchObject("Adaptor3d_IsoCurve:NoneIso");
                                break;
                        }
                        break;
                    }

                    case GeomAbs_SurfaceType.GeomAbs_SurfaceOfExtrusion:
                    {
                        switch (myIso)
                        {
                            case GeomAbs_IsoType.GeomAbs_IsoU:
                                return GeomAbs_CurveType.GeomAbs_Line;

                            case GeomAbs_IsoType.GeomAbs_IsoV:                                
                                return mySurface.BasisCurve()._GetType();

                            case GeomAbs_IsoType.GeomAbs_NoneIso:
                                throw new Standard_NoSuchObject("Adaptor3d_IsoCurve:NoneIso");
                                break;
                        }
                        break;
                    }
                default:
                    return GeomAbs_CurveType. GeomAbs_OtherCurve;
            }

            // portage WNT
            return GeomAbs_CurveType.GeomAbs_OtherCurve;
        }
    }


    //! this enumeration describes if a curve is an U isoparaetric
    //! or V isoparametric
    public enum GeomAbs_IsoType
    {
        GeomAbs_IsoU,
        GeomAbs_IsoV,
        GeomAbs_NoneIso
    };
}
