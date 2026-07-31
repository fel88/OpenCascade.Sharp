using OCCPort;
using OCCPort.Common;
using System.Reflection.Metadata;
using TKG2d;
using TKG3d;
using TKMath;

namespace TKBRep
{
    //! A framework providing advanced tolerance control.
    //! It is used to build Shapes.
    //! If tolerance control is required, you are advised to:
    //! 1. build a default precision for topology, using the
    //! classes provided in the BRepAPI package
    //! 2. update the tolerance of the resulting shape.
    //! Note that only vertices, edges and faces have
    //! meaningful tolerance control. The tolerance value
    //! must always comply with the condition that face
    //! tolerances are more restrictive than edge tolerances
    //! which are more restrictive than vertex tolerances. In
    //! other words: Tol(Vertex) >= Tol(Edge) >= Tol(Face).
    //! Other rules in setting tolerance include:
    //! - you can open up tolerance but should never restrict it
    //! - an edge cannot be included within the fusion of the
    //! tolerance spheres of two vertices
    public class BRep_Builder : TopoDS_Builder

    {
        public void Continuity(TopoDS_Edge E,
                                 TopoDS_Face F1,
                                 TopoDS_Face F2,
                                 GeomAbs_Shape C)
        {
            TopLoc_Location l1, l2;
            Geom_Surface S1 = BRep_Tool.Surface(F1, out l1);
            Geom_Surface S2 = BRep_Tool.Surface(F2, out l2);
            Continuity(E, S1, S2, l1, l2, C);
        }

        public void Continuity(TopoDS_Edge E,
                                  Geom_Surface S1,
                                  Geom_Surface S2,
                                  TopLoc_Location L1,
                                  TopLoc_Location L2,
                                  GeomAbs_Shape C)
        {
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            if (TE.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::Continuity");
            }
            TopLoc_Location l1 = L1.Predivided(E.Location());
            TopLoc_Location l2 = L2.Predivided(E.Location());

            UpdateCurves(TE.ChangeCurves(), S1, S2, l1, l2, C);


            TE.Modified(true);
        }
        static void UpdateCurves(BRep_ListOfCurveRepresentation lcr,
                         Geom_Surface S1,
                         Geom_Surface S2,
                         TopLoc_Location L1,
                         TopLoc_Location L2,
                         GeomAbs_Shape C)
        {
            BRep_ListIteratorOfListOfCurveRepresentation itcr = new BRep_ListIteratorOfListOfCurveRepresentation(lcr);
            while (itcr.More())
            {
                BRep_CurveRepresentation cr = itcr.Value();
                bool isregu = cr.IsRegularity(S1, S2, L1, L2);
                if (isregu) break;
                itcr.Next();
            }

            if (itcr.More())
            {
                BRep_CurveRepresentation cr = itcr.Value();
                cr.Continuity(C);
            }
            else
            {
                BRep_CurveOn2Surfaces COS = new BRep_CurveOn2Surfaces
                  (S1, S2, L1, L2, C);
                lcr.Append(COS);
            }
        }
        public void UpdateEdge(TopoDS_Edge E,
                                      Geom2d_Curve C1,
                                      Geom2d_Curve C2,
                                      TopoDS_Face F,
                                      double Tol)
        {
            TopLoc_Location l;
            UpdateEdge(E, C1, C2, BRep_Tool.Surface(F, out l), l, Tol);
        }

        public void UpdateEdge(TopoDS_Edge E,
                                  Geom2d_Curve C1,
                                  Geom2d_Curve C2,
                                  Geom_Surface S,
                                  TopLoc_Location L,
                                  double Tol)
        {
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            if (TE.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::UpdateEdge");
            }

            TopLoc_Location l = L.Predivided(E.Location());

            UpdateCurves(TE.ChangeCurves(), C1, C2, S, l);

            TE.UpdateTolerance(Tol);
            TE.Modified(true);
        }

        //=======================================================================
        //function : UpdateCurves
        //purpose  : Insert two pcurves <C1,C2> on surface <S> with location <L> 
        //           in a list of curve representations <lcr>
        //           Remove the pcurves on <S> from <lcr> if <C1> or <C2> is null
        //=======================================================================

        public static void UpdateCurves(BRep_ListOfCurveRepresentation lcr,
                               Geom2d_Curve C1,
                               Geom2d_Curve C2,
                               Geom_Surface S,
                               TopLoc_Location L)
        {
            BRep_ListIteratorOfListOfCurveRepresentation itcr = new BRep_ListIteratorOfListOfCurveRepresentation(lcr);
            BRep_CurveRepresentation cr;
            BRep_GCurve GC;
            double f = -Precision.Infinite(), l = Precision.Infinite();

            while (itcr.More())
            {
                GC = (BRep_GCurve)(itcr.Value());
                if (GC != null)
                {
                    if (GC.IsCurve3D())
                    {
                        GC.Range(ref f, ref l);
                    }
                    bool iscos = GC.IsCurveOnSurface(S, L);
                    if (iscos) break;
                }
                itcr.Next();
            }

            if (itcr.More())
            {
                // cr is used to keep a reference on the curve representation
                // this avoid deleting it as its content may be referenced by C or S
                cr = itcr.Value();
                lcr.Remove(itcr);
            }

            if (C1 != null && C2 != null)
            {
                BRep_CurveOnClosedSurface COS = new
                                BRep_CurveOnClosedSurface(C1, C2, S, L, GeomAbs_Shape.GeomAbs_C0);
                double aFCur = 0.0, aLCur = 0.0;
                COS.Range(ref aFCur, ref aLCur);
                if (!Precision.IsInfinite(f))
                {
                    aFCur = f;
                }

                if (!Precision.IsInfinite(l))
                {
                    aLCur = l;
                }

                COS.SetRange(aFCur, aLCur);
                lcr.Append(COS);
            }
        }
        public void UpdateEdge(TopoDS_Edge E,
                                                     Poly_Polygon3D P,
                                                     ref TopLoc_Location L)
        {
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            if (TE.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::UpdateEdge");
            }
            BRep_ListOfCurveRepresentation lcr = TE.ChangeCurves();
            BRep_ListIteratorOfListOfCurveRepresentation itcr = new BRep_ListIteratorOfListOfCurveRepresentation(lcr);

            while (itcr.More())
            {
                if (itcr.Value().IsPolygon3D())
                {
                    if (P == null)
                        lcr.Remove(itcr);
                    else
                        itcr.Value().Polygon3D(P);
                    TE.Modified(true);
                    return;
                }
                itcr.Next();
            }

            TopLoc_Location l = L.Predivided(E.Location());
            BRep_Polygon3D P3d = new BRep_Polygon3D(P, l);
            lcr.Append(P3d);

            TE.Modified(true);
        }

        public void UpdateEdge(TopoDS_Edge E,
                                 Poly_Polygon3D P)
        {
            var tt = new TopLoc_Location();
            UpdateEdge(E, P, ref tt);
        }

        public void NaturalRestriction(TopoDS_Face F,
                                       bool N)
        {
            BRep_TFace TF = (BRep_TFace)F.TShape();
            if (TF.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::NaturalRestriction");
            }
            TF.NaturalRestriction(N);
            F.TShape().Modified(true);
        }

        //! Sets the range of the 3d curve if Only3d=TRUE,
        //! otherwise sets the range to all the representations
        public void Range(TopoDS_Edge E, double First, double Last, bool Only3d = false)
        {
            //  set the range to all the representations if Only3d=FALSE
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            if (TE.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::Range");
            }

            BRep_GCurve GC;
            foreach (var item in TE.ChangeCurves())
            {
                GC = item as BRep_GCurve;
                if (GC != null && (!Only3d || GC.IsCurve3D()))
                    GC.SetRange(First, Last);
            }

            TE.Modified(true);
        }


        public void Degenerated(TopoDS_Edge E,
                                bool D)
        {
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            if (TE.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::Degenerated");
            }
            TE.Degenerated(D);
            if (D)
            {
                // set a null 3d curve
                UpdateCurves(TE.ChangeCurves(), null, E.Location());
            }
            TE.Modified(true);
        }

        public void UpdateEdge(TopoDS_Edge E,
                                      Poly_PolygonOnTriangulation P,
                                     ref Poly_Triangulation T)
        {
            var tt = new TopLoc_Location();
            UpdateEdge(E, P, T, ref tt);
        }

        public void UpdateEdge(TopoDS_Edge E,
                               Poly_PolygonOnTriangulation P,
                               Poly_Triangulation T,
                               ref TopLoc_Location L)
        {
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            if (TE.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::UpdateEdge");
            }
            TopLoc_Location l = L.Predivided(E.Location());

            bool isModified = false;

            BRep_ListOfCurveRepresentation lcr = TE.ChangeCurves();
            BRep_ListIteratorOfListOfCurveRepresentation itcr = new BRep_ListIteratorOfListOfCurveRepresentation(lcr);
            BRep_CurveRepresentation cr = null;

            while (itcr.More())
            {
                if (itcr.Value().IsPolygonOnTriangulation(T, l))
                {
                    // cr is used to keep a reference on the curve representation
                    // this avoid deleting it as its content may be referenced by T
                    cr = itcr.Value();
                    lcr.Remove(itcr);
                    isModified = true;
                    break;
                }
                itcr.Next();
            }

            if (P != null)
            {
                BRep_PolygonOnTriangulation PT =
                  new BRep_PolygonOnTriangulation(P, T, l);
                lcr.Append(PT);
                isModified = true;
            }

            if (isModified)
                TE.Modified(true);
        }

        public void MakeEdge(TopoDS_Edge E,
            Geom_Curve C, double Tol)
        {
            MakeEdge(E);
            UpdateEdge(E, C, new TopLoc_Location(), Tol);

        }

        public void UpdateEdge(TopoDS_Edge E,
            Geom_Curve C, TopLoc_Location L, double Tol)
        {
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            if (TE.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::UpdateEdge");
            }
            TopLoc_Location l = L.Predivided(E.Location());

            UpdateCurves(TE.ChangeCurves(), C, l);

            TE.UpdateTolerance(Tol);
            TE.Modified(true);

        }
        //=======================================================================
        //function : UpdateCurves
        //purpose  : Insert a pcurve <C> on surface <S> with location <L> 
        //           in a list of curve representations <lcr>
        //           Remove the pcurve on <S> from <lcr> if <C> is null
        //=======================================================================

        public static void UpdateCurves(BRep_ListOfCurveRepresentation lcr,
                          Geom2d_Curve C,
                          Geom_Surface S,
                          TopLoc_Location L)
        {
            BRep_ListIteratorOfListOfCurveRepresentation itcr = new BRep_ListIteratorOfListOfCurveRepresentation(lcr);
            BRep_CurveRepresentation cr;
            BRep_GCurve GC = null;
            double f = -Precision.Infinite(), l = Precision.Infinite();
            // search the range of the 3d curve
            // and remove any existing representation

            while (itcr.More())
            {
                GC = (BRep_GCurve)itcr.Value();
                if (GC != null)
                {
                    if (GC.IsCurve3D())
                    {
                        //      if (!C.IsNull()) { //xpu031198, edge degeneree

                        // xpu151298 : parameters can be set for null curves
                        //             see lbo & flo, to determine whether range is defined
                        //             compare first and last parameters with default values.
                        GC.Range(ref f, ref l);
                    }
                    if (GC.IsCurveOnSurface(S, L))
                    {
                        // remove existing curve on surface
                        // cr is used to keep a reference on the curve representation
                        // this avoid deleting it as its content may be referenced by C or S
                        cr = itcr.Value();
                        //lcr.Remove(itcr);
                    }
                    else
                    {
                        itcr.Next();
                    }
                }
                else
                {
                    itcr.Next();
                }
            }

            if (C != null)
            {
                BRep_CurveOnSurface COS = new BRep_CurveOnSurface(C, S, L);
                double aFCur = 0.0, aLCur = 0.0;
                COS.Range(ref aFCur, ref aLCur);
                if (!Precision.IsInfinite(f))
                {
                    aFCur = f;
                }

                if (!Precision.IsInfinite(l))
                {
                    aLCur = l;
                }

                COS.SetRange(aFCur, aLCur);
                lcr.Append(COS);
            }
        }


        private void UpdateCurves(BRep_ListOfCurveRepresentation lcr, Geom_Curve C, TopLoc_Location L)
        {
            BRep_ListIteratorOfListOfCurveRepresentation itcr = new BRep_ListIteratorOfListOfCurveRepresentation(lcr);
            BRep_GCurve GC = null;
            double f = 0.0, l = 0.0;

            while (itcr.More())
            {
                if (itcr.Value() is BRep_GCurve bb)
                {
                    GC = bb;
                }
                //GC = Handle(BRep_GCurve)::DownCast(itcr.Value());
                if (GC != null)
                {
                    GC.Range(ref f, ref l);
                    if (GC.IsCurve3D()) break;

                }
                itcr.Next();
            }

            if (itcr.More())
            {
                itcr.Value().Curve3D(C);
                itcr.Value().Location(L);
            }
            else
            {
                BRep_Curve3D C3d = new BRep_Curve3D(C, L);
                // test if there is already a range
                if (GC != null)
                {
                    C3d.SetRange(f, l);
                }
                lcr.Append(C3d);
            }
        }

        public void MakeEdge(TopoDS_Edge E)
        {
            BRep_TEdge TE = new BRep_TEdge();
            if (!E.IsNull() && E.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::MakeEdge");
            }
            MakeShape(E, TE);
        }

        public void MakeFace(TopoDS_Face F,
                                   Geom_Surface S,
                                   TopLoc_Location L,
                                   double Tol)
        {
            BRep_TFace TF = new BRep_TFace();
            if (!F.IsNull() && F.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::MakeFace");
            }
            TF.Surface(S);
            TF.Tolerance(Tol);
            TF.Location(L);
            MakeShape(F, TF);
        }

        public void MakeFace(TopoDS_Face theFace,

                                     Poly_Triangulation theTriangulation)
        {
            BRep_TFace aTFace = new BRep_TFace();
            if (!theFace.IsNull() && theFace.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::MakeFace");
            }
            aTFace.Triangulation(theTriangulation);
            MakeShape(theFace, aTFace);
        }

        //! Changes a face triangulation.
        //! A NULL theTriangulation removes face triangulations.
        //! If theToReset is TRUE face triangulations will be reset to new list with only one input triangulation that will be active.
        //! Else if theTriangulation is contained in internal triangulations list it will be made active,
        //!      else the active triangulation will be replaced to theTriangulation one.
        public void UpdateFace(TopoDS_Face theFace,
                               Poly_Triangulation theTriangulation,
                               bool theToReset = true)
        {
            BRep_TFace aTFace = (BRep_TFace)theFace.TShape();
            if (aTFace.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::UpdateFace");
            }
            aTFace.Triangulation(theTriangulation, theToReset);
            theFace.TShape().Modified(true);
        }
        //=======================================================================
        //function : MakeFace
        //purpose  :
        //=======================================================================
        public void MakeFace(TopoDS_Face theFace,
                               Poly_ListOfTriangulation theTriangulations,
                               Poly_Triangulation theActiveTriangulation)
        {
            BRep_TFace aTFace = new BRep_TFace();
            if (!theFace.IsNull() && theFace.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::MakeFace");
            }
            aTFace.Triangulations(theTriangulations, theActiveTriangulation);
            MakeShape(theFace, aTFace);
        }

        public void MakeFace(TopoDS_Face F, Geom_Surface S,
                                            double Tol)
        {
            BRep_TFace TF = new BRep_TFace();
            if (!F.IsNull() && F.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::MakeFace");
            }
            TF.Surface(S);
            TF.Tolerance(Tol);
            MakeShape(F, TF);
        }



        public void MakeFace(TopoDS_Face F)
        {
            BRep_TFace TF = new BRep_TFace();
            MakeShape(F, TF);
        }
        public void UpdateEdge(TopoDS_Edge E,
                                      Poly_PolygonOnTriangulation P1,
                                      Poly_PolygonOnTriangulation P2,
                                      Poly_Triangulation T)
        {
            UpdateEdge(E, P1, P2, T, new TopLoc_Location());
        }

        public void UpdateEdge(TopoDS_Edge E,
                                      Geom_Curve C,
                                      double Tol)
        {
            UpdateEdge(E, C, new TopLoc_Location(), Tol);
        }

        public void UpdateEdge(TopoDS_Edge E,
                                  Poly_PolygonOnTriangulation P1,
                                 Poly_PolygonOnTriangulation P2,
                                  Poly_Triangulation T,
                                  TopLoc_Location L)
        {
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            if (TE.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::UpdateEdge");
            }
            var l = L.Predivided(E.Location());

            bool isModified = false;

            BRep_ListOfCurveRepresentation lcr = TE.ChangeCurves();
            BRep_ListIteratorOfListOfCurveRepresentation itcr = new BRep_ListIteratorOfListOfCurveRepresentation(lcr);
            BRep_CurveRepresentation cr;

            while (itcr.More())
            {
                if (itcr.Value().IsPolygonOnTriangulation(T, l)) //szv:was L
                {
                    // cr is used to keep a reference on the curve representation
                    // this avoid deleting it as its content may be referenced by T
                    cr = itcr.Value();
                    //lcr.Remove(itcr);
                    lcr.Remove(cr);
                    isModified = true;
                    break;
                }
                itcr.Next();
            }

            if (P1 != null && P2 != null)
            {
                BRep_PolygonOnClosedTriangulation PT =
                  new BRep_PolygonOnClosedTriangulation(P1, P2, T, l);
                lcr.Append(PT);
                isModified = true;
            }

            if (isModified)
                TE.Modified(true);
        }


        public void UpdateEdge(TopoDS_Edge e,
                Geom_Curve geom2d_Line, TopoDS_Face f, double v)
        {
            throw new NotImplementedException();
        }


        public void MakeVertex(TopoDS_Vertex V)
        {
            BRep_TVertex TV = new BRep_TVertex();
            MakeShape(V, TV);
        }

        public void MakeVertex(TopoDS_Vertex V, gp_Pnt P, double Tol)
        {
            MakeVertex(V);
            UpdateVertex(V, P, Tol);

        }
        public void UpdateVertex(TopoDS_Vertex V,
                                  double Par,
                                  TopoDS_Edge E,
                                  double Tol)
        {
            if (Precision.IsPositiveInfinite(Par) ||
                Precision.IsNegativeInfinite(Par))
                throw new Standard_DomainError("BRep_Builder::Infinite parameter");

            BRep_TVertex TV = (BRep_TVertex)V.TShape();
            BRep_TEdge TE = (BRep_TEdge)E.TShape();

            if (TV.Locked() || TE.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::UpdateVertex");
            }

            TopLoc_Location L = E.Location().Predivided(V.Location());

            // Search the vertex in the edge
            TopAbs_Orientation ori = TopAbs_Orientation.TopAbs_INTERNAL;

            TopoDS_Iterator itv = new TopoDS_Iterator(E.Oriented(TopAbs_Orientation.TopAbs_FORWARD));

            // if the edge has no vertices
            // and is degenerated use the vertex orientation
            // RLE, june 94

            if (!itv.More() && TE.Degenerated())
                ori = V.Orientation();

            while (itv.More())
            {
                TopoDS_Shape Vcur = itv.Value();
                if (V.IsSame(Vcur))
                {
                    ori = Vcur.Orientation();
                    if (ori == V.Orientation()) break;
                }
                itv.Next();
            }

            BRep_ListOfCurveRepresentation lcr = TE.ChangeCurves();
            BRep_ListIteratorOfListOfCurveRepresentation itcr = new BRep_ListIteratorOfListOfCurveRepresentation(lcr);
            BRep_GCurve GC;

            while (itcr.More())
            {
                GC = (BRep_GCurve)itcr.Value();
                if (GC != null)
                {
                    if (ori == TopAbs_Orientation.TopAbs_FORWARD)
                        GC.First(Par);
                    else if (ori == TopAbs_Orientation.TopAbs_REVERSED)
                        GC.Last(Par);
                    else
                    {
                        BRep_ListOfPointRepresentation lpr = TV.ChangePoints();
                        TopLoc_Location GCloc = GC.Location();
                        TopLoc_Location LGCloc = L * GCloc;
                        if (GC.IsCurve3D())
                        {
                            Geom_Curve GC3d = GC.Curve3D();
                            //UpdatePoints(lpr, Par, GC3d, LGCloc);
                        }
                        else if (GC.IsCurveOnSurface())
                        {
                            Geom2d_Curve GCpc = GC.PCurve();
                            Geom_Surface GCsu = GC.Surface();
                            UpdatePoints(lpr, Par, GCpc, GCsu, LGCloc);
                        }
                    }
                }
                itcr.Next();
            }

            if (ori != TopAbs_Orientation.TopAbs_FORWARD && ori != TopAbs_Orientation.TopAbs_REVERSED)
                TV.Modified(true);
            TV.UpdateTolerance(Tol);
            TE.Modified(true);
        }

        static void UpdatePoints(BRep_ListOfPointRepresentation lpr,
                         double p,
                         Geom2d_Curve PC,
                         Geom_Surface S,
                         TopLoc_Location L)
        {
            //BRep_ListIteratorOfListOfPointRepresentation itpr = new BRep_ListIteratorOfListOfPointRepresentation(lpr);
            int i = 0;
            for (; i < lpr.Count; i++)
            {
                BRep_PointRepresentation itpr = lpr[i];
                BRep_PointRepresentation pr = itpr;
                bool isponcons = pr.IsPointOnCurveOnSurface(PC, S, L);
                if (isponcons)
                    break;


            }

            if (i < lpr.Count)
            {
                BRep_PointRepresentation pr = lpr[i];
                pr.Parameter(p);
            }
            else
            {
                BRep_PointOnCurveOnSurface POCS =
                  new BRep_PointOnCurveOnSurface(p, PC, S, L);
                lpr.Append(POCS);
            }
        }
        public void UpdateVertex(TopoDS_Vertex V,
                                 gp_Pnt P,
                                 double Tol)
        {
            BRep_TVertex TV = V.TShape() as BRep_TVertex;
            if (TV.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::UpdateVertex");
            }
            TV.Pnt(P.Transformed(V.Location().Inverted().Transformation()));
            TV.UpdateTolerance(Tol);
            TV.Modified(true);
        }

        public void UpdateEdge(TopoDS_Edge E,
                                 Geom2d_Curve C,
                                 Geom_Surface S,
                                 TopLoc_Location L,
                                 double Tol)
        {
            BRep_TEdge TE = (BRep_TEdge)E.TShape();
            if (TE.Locked())
            {
                throw new TopoDS_LockedShape("BRep_Builder::UpdateEdge");
            }
            TopLoc_Location l = L.Predivided(E.Location());

            UpdateCurves(TE.ChangeCurves(), C, S, l);

            TE.UpdateTolerance(Tol);
            TE.Modified(true);
        }
        public void UpdateEdge(TopoDS_Edge E,
                    Geom2d_Curve C, TopoDS_Face F, double Tol)
        {
            TopLoc_Location l;
            UpdateEdge(E, C, BRep_Tool.Surface(F, out l), l, Tol);
        }

        public void MakeCompound(TopoDS_Compound C)
        {
            TopoDS_TCompound TC = new TopoDS_TCompound();
            MakeShape(C, TC);
        }

        public void SameParameter(TopoDS_Edge aNE, bool v)
        {
            throw new NotImplementedException();
        }

        public void SameRange(TopoDS_Edge aNE, bool v)
        {
            throw new NotImplementedException();
        }
    }
}