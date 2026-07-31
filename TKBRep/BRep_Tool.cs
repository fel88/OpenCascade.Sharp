using OCCPort;
using OCCPort.Common;
using OCCPort.Tester;
using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Threading;
using TKernel;
using TKG2d;
using TKG3d;
using TKGeomBase;
using TKMath;

namespace TKBRep
{
    public static class BRep_Tool
    {

        //=======================================================================
        //function : Parameters
        //purpose  : Returns the parameters of the vertex on the face.
        //=======================================================================

        public static gp_Pnt2d Parameters(TopoDS_Vertex V,
                                 TopoDS_Face F)
        {
            TopLoc_Location L;
            Geom_Surface S = Surface(F, out L);
            L = L.Predivided(V.Location());
            BRep_TVertex TV = (BRep_TVertex)V.TShape();
            // It is checked if there is PointRepresentation (case non Manifold)
            foreach (var itpr in TV.Points())
            {

                if (itpr.IsPointOnSurface(S, ref L))
                {
                    return new gp_Pnt2d(itpr.Parameter(),
                                    itpr.Parameter2());
                }

            }

            TopoDS_Vertex Vf = new TopoDS_Vertex(), Vl = new TopoDS_Vertex();
            TopoDS_Edge E;
            // Otherwise the edges are searched (PMN 4/06/97) It is not possible to succeed 999/1000!
            // even if often there is a way to make more economically than above...
            TopExp_Explorer exp = new TopExp_Explorer();
            for (exp.Init(F, TopAbs_ShapeEnum.TopAbs_EDGE); exp.More(); exp.Next())
            {
                E = TopoDS.Edge(exp.Current());
                TopExp.Vertices(E, ref Vf, ref Vl);
                if (V.IsSame(Vf) || V.IsSame(Vl))
                {
                    gp_Pnt2d Pf = new gp_Pnt2d(), Pl = new gp_Pnt2d();
                    UVPoints(E, F, ref Pf, ref Pl);
                    if (V.IsSame(Vf))
                        return Pf;
                    else
                        return Pl;//Ambiguity (natural) for degenerated edges.
                }
            }
            throw new Standard_NoSuchObject("BRep_Tool:: no parameters on surface");
        }

        public static void UVPoints(TopoDS_Edge E,
                              TopoDS_Face F,
                           ref gp_Pnt2d PFirst,
                          ref gp_Pnt2d PLast)
        {
            TopLoc_Location L;
            Geom_Surface S = Surface(F, out L);
            TopoDS_Edge aLocalEdge = E;
            if (F.Orientation() == TopAbs_Orientation.TopAbs_REVERSED)
            {
                aLocalEdge.Reverse();
                //    UVPoints(E,S,L,PFirst,PLast);
            }
            //    UVPoints(TopoDS::Edge(E.Reversed()),S,L,PFirst,PLast);
            //  else
            //    UVPoints(E,S,L,PFirst,PLast);
            UVPoints(aLocalEdge, S, L, ref PFirst, ref PLast);
        }
        public static void UVPoints(TopoDS_Edge E,
                          Geom_Surface S,
                          TopLoc_Location L,
                          ref gp_Pnt2d PFirst,
                          ref gp_Pnt2d PLast)
        {
            TopLoc_Location l = L.Predivided(E.Location());
            bool Eisreversed = E.Orientation() == TopAbs_Orientation.TopAbs_REVERSED;

            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            BRep_ListIteratorOfListOfCurveRepresentation itcr = new(TE.Curves());
            while (itcr.More())
            {
                BRep_CurveRepresentation cr = itcr.Value();
                if (cr.IsCurveOnSurface(S, l))
                {
                    if (cr.IsCurveOnClosedSurface() && Eisreversed)
                    {
                        throw new NotImplementedException();
                        /*const BRep_CurveOnClosedSurface* CR =
                          static_cast <const BRep_CurveOnClosedSurface*> (cr.get());
                        CR->UVPoints2(PFirst, PLast);*/
                    }
                    else
                    {
                        throw new NotImplementedException();

                        /* BRep_CurveOnSurface CR =
                          (BRep_CurveOnSurface)cr;
                         CR.UVPoints(PFirst, PLast);*/
                    }
                    return;
                }

            }

            // for planar surface project the vertices
            // modif 21-05-97 : for RectangularTrimmedSurface, project the vertices
            Geom_Plane GP;
            Geom_RectangularTrimmedSurface GRTS;
            GRTS = (Geom_RectangularTrimmedSurface)S;
            if (GRTS != null)
                GP = (Geom_Plane)GRTS.BasisSurface();
            else
                GP = (Geom_Plane)S;
            //fin modif du 21-05-97
            if (GP != null)
            {
                // get the two vertices
                TopoDS_Vertex Vf = new TopoDS_Vertex(), Vl = new TopoDS_Vertex();
                TopExp.Vertices(E, ref Vf, ref Vl);

                TopLoc_Location Linverted = L.Inverted();
                Vf.Move(Linverted, false);
                Vl.Move(Linverted, false);
                double u, v;
                gp_Pln pln = GP.Pln();

                u = v = 0.0;
                if (!Vf.IsNull())
                {
                    gp_Pnt PF = Pnt(Vf);
                    ElSLib.Parameters(pln, PF, ref u, ref v);
                }
                PFirst.SetCoord(u, v);

                u = v = 0.0;
                if (!Vl.IsNull())
                {
                    gp_Pnt PL = Pnt(Vl);
                    ElSLib.Parameters(pln, PL, ref u, ref v);
                }
                PLast.SetCoord(u, v);
            }
            else
            {
                PFirst.SetCoord(0.0, 0.0);
                PLast.SetCoord(0.0, 0.0);
            }
        }
        public static double Parameter(TopoDS_Vertex V,
                                    TopoDS_Edge E)
        {
            double p;
            if (Parameter(V, E, out p))
                return p;
            throw new Standard_NoSuchObject("BRep_Tool:: no parameter on edge");
        }
        public static void Range(TopoDS_Edge E,
                       out double First,
                       out double Last)
        {
            //  set the range to all the representations
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            foreach (var cr in TE.Curves())
            {


                if (cr.IsCurve3D())
                {
                    BRep_Curve3D CR = (BRep_Curve3D)cr;
                    if (CR.Curve3D() != null)
                    {
                        First = CR.First();
                        Last = CR.Last();
                        return;
                    }
                }
                else if (cr.IsCurveOnSurface())
                {
                    BRep_GCurve CR = (BRep_GCurve)cr;
                    First = CR.First();
                    Last = CR.Last();
                    return;
                }
            }
            First = Last = 0.0;
        }

        public static bool Parameter(TopoDS_Vertex theV,
                                         TopoDS_Edge theE,
                                      out double theParam)
        {
            // Search the vertex in the edge
            theParam = 0;
            bool rev = false;
            TopoDS_Shape VF = null;
            TopAbs_Orientation orient = TopAbs_Orientation.TopAbs_INTERNAL;

            TopoDS_Iterator itv = new TopoDS_Iterator(theE.Oriented(TopAbs_Orientation.TopAbs_FORWARD));

            // if the edge has no vertices
            // and is degenerated use the vertex orientation
            // RLE, june 94

            if (!itv.More() && Degenerated(theE))
            {
                orient = theV.Orientation();
            }

            while (itv.More())
            {
                TopoDS_Shape Vcur = itv.Value();
                if (theV.IsSame(Vcur))
                {
                    if (VF == null)
                    {
                        VF = Vcur;
                    }
                    else
                    {
                        rev = theE.Orientation() == TopAbs_Orientation.TopAbs_REVERSED;
                        if (Vcur.Orientation() == theV.Orientation())
                        {
                            VF = Vcur;
                        }
                    }
                }
                itv.Next();
            }

            if (!VF.IsNull()) orient = VF.Orientation();

            double f, l;

            if (orient == TopAbs_Orientation.TopAbs_FORWARD)
            {
                Range(theE, out f, out l);
                theParam = rev ? l : f;
                return true;
            }

            else if (orient == TopAbs_Orientation.TopAbs_REVERSED)
            {
                Range(theE, out f, out l);
                theParam = rev ? f : l;
                return true;
            }

            else
            {
                TopLoc_Location L;
                Geom_Curve C = Curve(theE, out L, out f, out l);
                L = L.Predivided(theV.Location());
                if (C != null || Degenerated(theE))
                {
                    //const BRep_TVertex* TV = static_cast <const BRep_TVertex*> (theV.TShape().get());
                    //BRep_ListIteratorOfListOfPointRepresentation itpr(TV->Points());

                    //while (itpr.More())
                    //{
                    //    const Handle(BRep_PointRepresentation)&pr = itpr.Value();
                    //    if (pr->IsPointOnCurve(C, L))
                    //    {
                    //        Standard_Real p = pr->Parameter();
                    //        Standard_Real res = p;// SVV 4 nov 99 - to avoid warnings on Linux
                    //        if (!C.IsNull())
                    //        {
                    //            // Closed curves RLE 16 june 94
                    //            if (Precision::IsNegativeInfinite(f))
                    //            {
                    //                theParam = pr->Parameter();//p;
                    //                return Standard_True;
                    //            }
                    //            ;
                    //            if (Precision::IsPositiveInfinite(l))
                    //            {
                    //                theParam = pr->Parameter();//p;
                    //                return Standard_True;
                    //            }
                    //            gp_Pnt Pf = C->Value(f).Transformed(L.Transformation());
                    //            gp_Pnt Pl = C->Value(l).Transformed(L.Transformation());
                    //            Standard_Real tol = BRep_Tool::Tolerance(theV);
                    //            if (Pf.Distance(Pl) < tol)
                    //            {
                    //                if (Pf.Distance(BRep_Tool::Pnt(theV)) < tol)
                    //                {
                    //                    if (theV.Orientation() == TopAbs_FORWARD) res = f;//p = f;
                    //                    else res = l;//p = l;
                    //                }
                    //            }
                    //        }
                    //        theParam = res;//p;
                    //        return Standard_True;
                    //    }
                    //    itpr.Next();
                    //}
                }
                else
                {
                    // no 3d curve !!
                    // let us try with the first pcurve
                    //Handle(Geom2d_Curve) PC;
                    //Handle(Geom_Surface) S;
                    //BRep_Tool::CurveOnSurface(theE, PC, S, L, f, l);
                    //L = L.Predivided(theV.Location());
                    //const BRep_TVertex* TV = static_cast <const BRep_TVertex*> (theV.TShape().get());
                    //BRep_ListIteratorOfListOfPointRepresentation itpr(TV->Points());

                    //while (itpr.More())
                    //{
                    //    const Handle(BRep_PointRepresentation)&pr = itpr.Value();
                    //    if (pr->IsPointOnCurveOnSurface(PC, S, L))
                    //    {
                    //        Standard_Real p = pr->Parameter();
                    //        // Closed curves RLE 16 june 94
                    //        if (PC->IsClosed())
                    //        {
                    //            if ((p == PC->FirstParameter()) ||
                    //              (p == PC->LastParameter()))
                    //            {
                    //                if (theV.Orientation() == TopAbs_FORWARD) p = PC->FirstParameter();
                    //                else p = PC->LastParameter();
                    //            }
                    //        }
                    //        theParam = p;
                    //        return tru;
                    //    }
                    //    itpr.Next();
                    //}
                }
            }

            return false;
        }

        public static Geom_Curve Curve(TopoDS_Edge E,
                                            out TopLoc_Location L,
                                            out double First,
                                            out double Last)
        {
            L = new TopLoc_Location();
            First = 0;
            Last = 0;
            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();

            foreach (var cr in TE.Curves())
            {
                if (cr.IsCurve3D())
                {
                    BRep_Curve3D GC = (BRep_Curve3D)cr;
                    L = E.Location() * GC.Location();
                    GC.Range(ref First, ref Last);
                    return GC.Curve3D();
                }
            }

            L.Identity();
            First = Last = 0.0;
            return null;
        }

        public static void CurveOnSurface(TopoDS_Edge E,
                                ref Geom2d_Curve C,
                                ref Geom_Surface S,
                                ref TopLoc_Location L,
                                ref double First,
                                ref double Last,
                                int Index)
        {
            if (Index < 1)
                return;

            int i = 0;
            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            //BRep_ListIteratorOfListOfCurveRepresentation itcr(TE->Curves());
            //for (; itcr.More(); itcr.Next())
            foreach (var cr in TE.Curves())
            {
                if (cr.IsCurveOnSurface())
                {
                    BRep_GCurve GC = (BRep_GCurve)cr;
                    ++i;
                    // Compare index taking into account the fact that for the curves on
                    // closed surfaces there are two PCurves
                    if (i == Index)
                        C = GC.PCurve();
                    else if (GC.IsCurveOnClosedSurface() && ++i == Index)
                        C = GC.PCurve2();
                    else
                        continue;

                    S = GC.Surface();
                    L = E.Location() * GC.Location();
                    GC.Range(ref First, ref Last);
                    return;
                }
            }

            C = null;
            S = null;
            L.Identity();
            First = Last = 0.0;
        }
        //=======================================================================
        //function : CurveOnSurface
        //purpose  : Returns the curve  associated to the  edge in  the
        //           parametric  space of  the  face.  Returns   a NULL
        //           handle  if this curve  does not exist.  Returns in
        //           <First> and <Last> the parameter range.
        //=======================================================================
        public static Geom2d_Curve CurveOnSurface(TopoDS_Edge E,
                                               TopoDS_Face F,
                                               ref double First,
                                               ref double Last
                                             )
        {
            bool? v = null;
            return CurveOnSurface(E, F, ref First, ref Last, ref v);
        }

        //! Returns the curve  associated to the  edge in  the
        //! parametric  space of  the  face.  Returns   a NULL
        //! handle  if this curve  does not exist.  Returns in
        //! <First> and <Last> the parameter range.
        //! If the surface is a plane the curve can be not stored but created a new
        //! each time. The flag pointed by <theIsStored> serves to indicate storage status. 
        //! It is valued if the pointer is non-null.
        public static Geom2d_Curve CurveOnSurface(TopoDS_Edge E,
                                               TopoDS_Face F,
                                               ref double First,
                                               ref double Last,
                                               ref bool? theIsStored)
        {
            TopLoc_Location l;
            Geom_Surface S = Surface(F, out l);
            TopoDS_Edge aLocalEdge = E;
            if (F.Orientation() == TopAbs_Orientation.TopAbs_REVERSED)
            {
                aLocalEdge.Reverse();
            }
            return CurveOnSurface(aLocalEdge, S, ref l, ref First, ref Last, ref theIsStored);
        }

        //! Returns the  curve associated to   the edge in the
        //! parametric  space of the   surface. Returns a NULL
        //! handle  if this curve does  not exist.  Returns in
        //! <First> and <Last> the parameter range.
        //! If the surface is a plane the curve can be not stored but created a new
        //! each time. The flag pointed by <theIsStored> serves to indicate storage status. 
        //! It is valued if the pointer is non-null.
        public static Geom2d_Curve CurveOnSurface(TopoDS_Edge E,
                                               Geom_Surface S,
                                               ref TopLoc_Location L,
                                               ref double First,
                                               ref double Last,
                                               ref bool? theIsStored)
        {
            TopLoc_Location loc = L.Predivided(E.Location());
            bool Eisreversed = E.Orientation() == TopAbs_Orientation.TopAbs_REVERSED;
            if (theIsStored.HasValue)//??
                theIsStored = true;//??

            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();

            foreach (var cr in TE.Curves())
            {

                if (cr.IsCurveOnSurface(S, loc))
                {
                    BRep_GCurve GC = (BRep_GCurve)cr;
                    GC.Range(ref First, ref Last);
                    if (GC.IsCurveOnClosedSurface() && Eisreversed)
                        return GC.PCurve2();
                    else
                        return GC.PCurve();
                }

            }

            // Curve is not found. Try projection on plane
            if (theIsStored.HasValue)
                theIsStored = false;
            return CurveOnPlane(E, S, L, ref First, ref Last);
        }

        //! Returns in <C>, <S>, <L> a 2d curve, a surface and
        //! a location for the edge <E>. <C> and <S>  are null
        //! if the  edge has no curve on  surface.  Returns in
        //! <First> and <Last> the parameter range.
        public static void CurveOnSurface(TopoDS_Edge E,
                                      out Geom2d_Curve C,
                                     out Geom_Surface S,
                                   ref TopLoc_Location L,
                                    ref double First,
                                   ref double Last)
        {
            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            foreach (var cr in TE.Curves())
            {

                if (cr.IsCurveOnSurface())
                {
                    BRep_GCurve GC = (BRep_GCurve)cr;
                    C = GC.PCurve();
                    S = GC.Surface();//strange code here??
                    L = E.Location() * GC.Location();
                    GC.Range(ref First, ref Last);
                    return;
                }
            }

            C = null;
            S = null;
            L.Identity();
            First = Last = 0.0;
        }


        public static Geom2d_Curve CurveOnSurface(TopoDS_Edge E,
                                           Geom_Surface S,
                                           TopLoc_Location L,
                                          ref double First,
                                         ref double Last,
                                           bool? theIsStored = null)
        {
            TopLoc_Location loc = L.Predivided(E.Location());
            bool Eisreversed = E.Orientation() == TopAbs_Orientation.TopAbs_REVERSED;
            if (theIsStored.HasValue)
                theIsStored = true;

            // find the representation
            BRep_TEdge TE = E.TShape() as BRep_TEdge;

            foreach (var cr in TE.Curves())
            {
                if (cr.IsCurveOnSurface(S, loc))
                {
                    BRep_GCurve GC = cr as BRep_GCurve;
                    GC.Range(ref First, ref Last);
                    if (GC.IsCurveOnClosedSurface() && Eisreversed)
                        return GC.PCurve2();
                    else
                        return GC.PCurve();
                }
            }

            // Curve is not found. Try projection on plane
            if (theIsStored.HasValue)
                theIsStored = false;
            return CurveOnPlane(E, S, L, ref First, ref Last);
        }



        //=======================================================================
        //function : CurveOnSurface
        //purpose  : Returns the curve  associated to the  edge in  the
        //           parametric  space of  the  face.  Returns   a NULL
        //           handle  if this curve  does not exist.  Returns in
        //           <First> and <Last> the parameter range.
        //=======================================================================

        public static Geom2d_Curve CurveOnSurface(TopoDS_Edge E,
                                               TopoDS_Face F,
                                               ref double First,
                                               ref double Last,
                                               ref bool theIsStored)
        {
            TopLoc_Location l;
            Geom_Surface S = Surface(F, out l);
            TopoDS_Edge aLocalEdge = E;
            if (F.Orientation() == TopAbs_Orientation.TopAbs_REVERSED)
            {
                aLocalEdge.Reverse();
            }
            return CurveOnSurface(aLocalEdge, S, l, ref First, ref Last, theIsStored);
        }


        static Geom2d_Curve nullPCurve = null;

        //=======================================================================
        //function : CurveOnPlane
        //purpose  : For planar surface returns projection of the edge on the plane
        //=======================================================================
        public static Geom2d_Curve CurveOnPlane(TopoDS_Edge E,
                                             Geom_Surface S,
                                             TopLoc_Location L,
                                           ref double First,
                                           ref double Last)
        {

            First = Last = 0.0;

            // Check if the surface is planar
            Geom_Plane GP;
            Geom_RectangularTrimmedSurface GRTS;
            GRTS = S as Geom_RectangularTrimmedSurface;
            if (GRTS != null)
                GP = GRTS.BasisSurface() as Geom_Plane;
            else
                GP = S as Geom_Plane;

            if (GP == null)
                // not a plane
                return nullPCurve;

            // Check existence of 3d curve in edge
            double f = 0, l = 0;
            TopLoc_Location aCurveLocation = new TopLoc_Location();
            Geom_Curve C3D = Curve(E, out aCurveLocation, out f, out l);

            if (C3D == null)
                // no 3d curve
                return nullPCurve;

            aCurveLocation = aCurveLocation.Predivided(L);
            First = f; Last = l;

            // Transform curve and update parameters in account of scale factor
            if (!aCurveLocation.IsIdentity())
            {
                gp_Trsf aTrsf = aCurveLocation.Transformation();
                C3D = C3D.Transformed(aTrsf) as Geom_Curve;
                f = C3D.TransformedParameter(f, aTrsf);
                l = C3D.TransformedParameter(l, aTrsf);
            }

            // Perform projection
            Geom_Curve ProjOnPlane =
            GeomProjLib.ProjectOnPlane(new Geom_TrimmedCurve(C3D, f, l, true, false),
                                        GP,
                                        GP.Position().Direction(),
                                        true);

            GeomAdaptor_Surface HS = new GeomAdaptor_Surface(GP);
            GeomAdaptor_Curve HC = new GeomAdaptor_Curve(ProjOnPlane);

            ProjLib_ProjectedCurve Proj = new ProjLib_ProjectedCurve(HS, HC);
            Geom2d_Curve pc = Geom2dAdaptor.MakeCurve(Proj);

            if (pc is Geom2d_TrimmedCurve)
            {
                Geom2d_TrimmedCurve TC = pc as Geom2d_TrimmedCurve;
                pc = TC.BasisCurve();
            }

            return pc;
        }

        //=======================================================================
        //function : IsGeometric
        //purpose  : Returns True if <F> has a surface.
        //=======================================================================
        public static bool IsGeometric(TopoDS_Face F)
        {
            BRep_TFace TF = F.TShape() as BRep_TFace;
            Geom_Surface S = TF.Surface();
            return S != null;
        }

        public static Poly_Polygon3D nullPolygon3D = null;
        //=======================================================================
        //function : IsGeometric
        //purpose  : Returns True if <E> is a 3d curve or a curve on
        //           surface.
        //=======================================================================

        public static bool IsGeometric(TopoDS_Edge E)
        {
            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();

            foreach (var cr in TE.Curves())
            {
                if (cr.IsCurve3D())
                {
                    BRep_Curve3D GC = cr as BRep_Curve3D;
                    if (GC != null && GC.Curve3D() != null)
                        return true;
                }
                else if (cr.IsCurveOnSurface())
                    return true;
            }
            return false;
        }

        public static gp_Pnt Pnt(TopoDS_Vertex V)
        {
            BRep_TVertex TV = V.TShape() as BRep_TVertex;

            if (TV == null)
            {
                throw new Standard_NullObject("BRep_Tool:: TopoDS_Vertex hasn't gp_Pnt");
            }

            gp_Pnt P = TV.Pnt();
            if (V.Location().IsIdentity())
            {
                return P;
            }

            return P.Transformed(V.Location().Transformation());
        }

        public static Poly_Polygon3D Polygon3D(TopoDS_Edge E,
                                                         ref TopLoc_Location L)
        {
            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            //BRep_ListIteratorOfListOfCurveRepresentation itcr(TE->Curves());

            foreach (var cr in TE.Curves())
            {
                if (cr.IsPolygon3D())
                {
                    BRep_Polygon3D GC = (BRep_Polygon3D)cr;
                    L = E.Location() * GC.Location();
                    return GC.Polygon3D();
                }
            }

            L.Identity();
            return nullPolygon3D;
        }
        //=======================================================================
        //function : Tolerance
        //purpose  : Returns the tolerance for <E>.
        //=======================================================================

        public static double Tolerance(TopoDS_Edge E)
        {
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            double p = TE.Tolerance();
            double pMin = Precision.Confusion();
            if (p > pMin)
                return p;


            return pMin;
        }

        public static double Tolerance(TopoDS_Vertex V)
        {
            BRep_TVertex aTVert = V.TShape() as BRep_TVertex;

            if (aTVert == null)
            {
                throw new Standard_NullObject("BRep_Tool:: TopoDS_Vertex hasn't gp_Pnt");
            }

            double p = aTVert.Tolerance();
            double pMin = Precision.Confusion();
            if (p > pMin) return p;
            else return pMin;
        }

        //=======================================================================
        //function : Tolerance
        //purpose  : Returns the tolerance of the face.
        //=======================================================================

        public static double Tolerance(TopoDS_Face F)
        {
            BRep_TFace TF = (BRep_TFace)F.TShape();
            double p = TF.Tolerance();
            double pMin = Precision.Confusion();
            if (p > pMin)
                return p;

            return pMin;
        }
        //! Returns  True  if  <E>  has  two  PCurves  in  the
        //! parametric space of <F>. i.e.  <F>  is on a closed
        //! surface and <E> is on the closing curve.
        //=======================================================================
        //function : IsClosed
        //purpose  : Returns  True  if  <E>  has  two  PCurves  in  the
        //           parametric space of <F>. i.e.  <F>  is on a closed
        //           surface and <E> is on the closing curve.
        //=======================================================================

        public static bool IsClosed(TopoDS_Edge E, TopoDS_Face F)
        {
            TopLoc_Location l;
            Geom_Surface S = Surface(F, out l);
            if (IsClosed(E, S, l))
                return true;

            Poly_Triangulation T = Triangulation(F, ref l);
            return IsClosed(E, T, l);
        }


        //! Returns  True  if <E> has two arrays of indices in
        //! the triangulation <T>.
        public static bool IsClosed(TopoDS_Edge E, Poly_Triangulation T, TopLoc_Location L)
        {
            TopLoc_Location l = L.Predivided(E.Location());

            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            //BRep_ListIteratorOfListOfCurveRepresentation itcr(TE->Curves());
            foreach (var cr in TE.Curves())
            {
                if (cr.IsPolygonOnTriangulation(T, l) &&
                    cr.IsPolygonOnClosedTriangulation())
                    return true;
            }
            return false;
        }


        //=======================================================================
        //function : IsClosed
        //purpose  : Returns  True  if  <E>  has  two  PCurves  in  the
        //           parametric space  of <S>.  i.e.   <S>  is a closed
        //           surface and <E> is on the closing curve.
        //=======================================================================

        public static bool IsClosed(TopoDS_Edge E,
    Geom_Surface S,
    TopLoc_Location L)
        {
            //modified by NIZNHY-PKV Fri Oct 17 12:16:58 2008f
            if (IsPlane(S))
            {
                return false;
            }
            //modified by NIZNHY-PKV Fri Oct 17 12:16:54 2008t
            //
            TopLoc_Location l = L.Predivided(E.Location());

            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();

            foreach (var cr in TE.Curves())
            {
                if (cr.IsCurveOnSurface(S, l) &&
                    cr.IsCurveOnClosedSurface())
                    return true;
            }
            return false;
        }

        public static bool IsPlane(Geom_Surface aS)
        {
            bool bRet;
            Geom_Plane aGP;
            Geom_RectangularTrimmedSurface aGRTS;
            Geom_OffsetSurface aGOFS;
            //
            aGRTS = aS as Geom_RectangularTrimmedSurface;
            aGOFS = aS as Geom_OffsetSurface;
            //
            if (aGOFS != null)
            {
                aGP = (Geom_Plane)aGOFS.BasisSurface();
            }
            else if (aGRTS != null)
            {
                aGP = (Geom_Plane)aGRTS.BasisSurface();
            }

            else
            {
                aGP = (Geom_Plane)aS;
            }
            //
            bRet = aGP != null;
            //
            return bRet;
        }


        //! If S is Shell, returns True if it has no free boundaries (edges).
        //! If S is Wire, returns True if it has no free ends (vertices).
        //! (Internal and External sub-shepes are ignored in these checks)
        //! If S is Edge, returns True if its vertices are the same.
        //! For other shape types returns S.Closed().
        public static bool IsClosed(TopoDS_Shape theShape)
        {
            if (theShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_SHELL)
            {
                NCollection_Map<TopoDS_Shape, TopTools_ShapeMapHasher> aMap = new();

                TopExp_Explorer exp = new TopExp_Explorer(theShape.Oriented(TopAbs_Orientation.TopAbs_FORWARD), TopAbs_ShapeEnum.TopAbs_EDGE);
                bool hasBound = false;
                for (; exp.More(); exp.Next())
                {
                    TopoDS_Edge E = TopoDS.Edge(exp.Current());
                    if (Degenerated(E) || E.Orientation() == TopAbs_Orientation.TopAbs_INTERNAL || E.Orientation() == TopAbs_Orientation.TopAbs_EXTERNAL)
                        continue;
                    hasBound = true;
                    if (!aMap.Add(E))
                        aMap.Remove(E);
                }
                return hasBound && aMap.IsEmpty();
            }
            else if (theShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_WIRE)
            {
                NCollection_Map<TopoDS_Shape, TopTools_ShapeMapHasher> aMap = new();

                TopExp_Explorer exp = new TopExp_Explorer(theShape.Oriented(TopAbs_Orientation.TopAbs_FORWARD), TopAbs_ShapeEnum.TopAbs_VERTEX);
                bool hasBound = false;
                for (; exp.More(); exp.Next())
                {
                    TopoDS_Shape V = exp.Current();
                    if (V.Orientation() == TopAbs_Orientation.TopAbs_INTERNAL || V.Orientation() == TopAbs_Orientation.TopAbs_EXTERNAL)
                        continue;
                    hasBound = true;
                    if (!aMap.Add(V))
                        aMap.Remove(V);
                }
                return hasBound && aMap.IsEmpty();
            }
            else if (theShape.ShapeType() == TopAbs_ShapeEnum.TopAbs_EDGE)
            {
                TopoDS_Vertex aVFirst = new TopoDS_Vertex(), aVLast = new TopoDS_Vertex();
                TopExp.Vertices(TopoDS.Edge(theShape), ref aVFirst, ref aVLast);
                return !aVFirst.IsNull() && aVFirst.IsSame(aVLast);
            }
            return theShape.Closed();
        }
        public static bool SameRange(TopoDS_Edge E)
        {
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            return TE.SameRange();
        }

        public static bool Degenerated(TopoDS_Edge e)
        {
            BRep_TEdge TE = e.TShape() as BRep_TEdge;

            //const BRep_TEdge* TE = static_cast <const BRep_TEdge*> (E.TShape().get());
            return TE.Degenerated();
        }

        //! Returns the geometric surface of the face. Returns
        //! in <L> the location for the surface.
        public static Geom_Surface Surface(TopoDS_Face F, out TopLoc_Location L)
        {
            BRep_TFace TF = (BRep_TFace)F.TShape();
            L = F.Location() * TF.Location();
            return TF.Surface();
        }

        public static Geom_Surface Surface(TopoDS_Face F)
        {
            BRep_TFace TF = (BRep_TFace)F.TShape();
            Geom_Surface S = TF.Surface();

            if (S == null)
                return S;

            TopLoc_Location L = F.Location() * TF.Location();
            if (!L.IsIdentity())
            {
                Geom_Geometry aCopy = S.Transformed(L.Transformation());
                Geom_Surface aGS = (Geom_Surface)aCopy;
                return aGS;
            }
            return S;
        }


        //=======================================================================
        //function : Triangulations
        //purpose  :
        //=======================================================================
        public static Poly_ListOfTriangulation Triangulations(TopoDS_Face theFace,
                                                           ref TopLoc_Location theLocation)
        {
            theLocation = theFace.Location();
            BRep_TFace aTFace = (BRep_TFace)theFace.TShape();
            return aTFace.Triangulations();
        }

        public static Poly_Triangulation Triangulation(TopoDS_Face theFace,
            ref TopLoc_Location theLocation,
            Poly_MeshPurpose theMeshPurpose = Poly_MeshPurpose.Poly_MeshPurpose_NONE)
        {
            theLocation = theFace.Location();
            var aTFace = theFace.TShape() as BRep_TFace;
            //const BRep_TFace* aTFace = static_cast <const BRep_TFace*> (theFace.TShape().get());
            return aTFace.Triangulation(theMeshPurpose);
        }

        //! Returns in <P>, <T>, <L> a polygon on triangulation, a
        //! triangulation and a location for the edge <E>.
        //! <P>  and  <T>  are null  if  the  edge has no
        //! polygon on  triangulation.
        public static void PolygonOnTriangulation(TopoDS_Edge E, ref Poly_PolygonOnTriangulation P, ref Poly_Triangulation T, TopLoc_Location L)
        {
            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            foreach (var item in TE.Curves())
            {
                BRep_CurveRepresentation cr = item;
                if (cr.IsPolygonOnTriangulation())
                {
                    BRep_PolygonOnTriangulation PT =
                        (BRep_PolygonOnTriangulation)cr;
                    P = PT.PolygonOnTriangulation();
                    T = PT.Triangulation();
                    L = E.Location() * PT.Location();
                    return;
                }
            }

            L.Identity();
            P = null;
            T = null;
        }


        //! Returns the polygon associated to the  edge in  the
        //! parametric  space of  the  face.  Returns   a NULL
        //! handle  if this polygon  does not exist.
        public static Poly_PolygonOnTriangulation PolygonOnTriangulation(TopoDS_Edge E,
            Poly_Triangulation T, TopLoc_Location L)
        {
            TopLoc_Location l = L.Predivided(E.Location());
            bool Eisreversed = E.Orientation() == TopAbs_Orientation.TopAbs_REVERSED;

            // find the representation
            BRep_TEdge TE = (BRep_TEdge)E.TShape();

            foreach (var cr in TE.Curves())
            {
                if (cr.IsPolygonOnTriangulation(T, l))
                {
                    if (cr.IsPolygonOnClosedTriangulation() && Eisreversed)
                        return cr.PolygonOnTriangulation2();
                    else
                        return cr.PolygonOnTriangulation();
                }
            }

            return null;
        }

        //! Returns the max continuity of edge between some surfaces or GeomAbs_C0 if there no such surfaces.
        public static GeomAbs_Shape MaxContinuity(TopoDS_Edge theEdge)
        {
            GeomAbs_Shape aMaxCont = GeomAbs_Shape.GeomAbs_C0;
            var curves = ((BRep_TEdge)theEdge.TShape()).ChangeCurves();

            for (BRep_ListIteratorOfListOfCurveRepresentation aReprIter = new(curves);
              aReprIter.More(); aReprIter.Next())
            //foreach (var item in curves)
            {
                BRep_CurveRepresentation aRepr = aReprIter.Value();
                if (aRepr.IsRegularity())
                {
                    GeomAbs_Shape aCont = aRepr.Continuity();
                    if ((int)aCont > (int)aMaxCont)
                    {
                        aMaxCont = aCont;
                    }
                }
            }
            return aMaxCont;
        }

        public static bool SameParameter(TopoDS_Edge E)
        {

            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            return TE.SameParameter();

        }
    }

}