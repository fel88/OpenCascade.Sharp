using OCCPort;
using OCCPort.Common;
using TKBRep;
using TKMath;
using static System.Net.WebRequestMethods;

namespace TKPrim
{
    public abstract class BRepPrim_OneAxis
    {
        public BRepPrim_OneAxis()
        {
            for (int i = 0; i < myWires.Length; i++)
            {
                myWires[i] = new TopoDS_Wire();
            }
            for (int i = 0; i < myEdges.Length; i++)
            {
                myEdges[i] = new TopoDS_Edge();
            }
            for (int i = 0; i < myVertices.Length; i++)
            {
                myVertices[i] = new();
            }
            for (int i = 0; i < myFaces.Length; i++)
            {
                myFaces[i] = new();
            }
        }

        //! Returns the meridian point at parameter <V> in the
        //! plane XZ.
        public abstract gp_Pnt2d MeridianValue(double V);

        //! Returns the Shell containing all the  Faces of the
        //! primitive.
        public TopoDS_Shell Shell()
        {
            if (!ShellBuilt)
            {
                myBuilder.MakeShell(myShell);

                myBuilder.AddShellFace(myShell, LateralFace());
                if (HasTop())
                    myBuilder.AddShellFace(myShell, TopFace());
                if (HasBottom())
                    myBuilder.AddShellFace(myShell, BottomFace());
                if (HasSides())
                {
                    myBuilder.AddShellFace(myShell, StartFace());
                    myBuilder.AddShellFace(myShell, EndFace());
                }

                myShell.Closed(BRep_Tool.IsClosed(myShell));
                myBuilder.CompleteShell(myShell);
                ShellBuilt = true;
            }
            return myShell;
        }


        TopoDS_Wire EndWire()
        {
            // do it if not done
            if (!WiresBuilt[WEND])
            {

                Exceptions.Standard_DomainError_Raise_if(!HasSides(),
                                  "BRepPrim_OneAxes::EndWire:no sides");

                myBuilder.MakeWire(myWires[WEND]);

                if (HasTop())
                    myBuilder.AddWireEdge(myWires[WEND], EndTopEdge(), true);
                if (!MeridianClosed())
                {
                    if (!VMaxInfinite() || !VMinInfinite())
                    {
                        myBuilder.AddWireEdge(myWires[WEND], AxisEdge(), true);
                    }
                }
                if (HasBottom())
                    myBuilder.AddWireEdge(myWires[WEND], EndBottomEdge(), false);
                myBuilder.AddWireEdge(myWires[WEND], EndEdge(), false);

                myBuilder.CompleteWire(myWires[WEND]);
                WiresBuilt[WEND] = true;
            }
            return myWires[WEND];
        }

        TopoDS_Edge EndBottomEdge()
        {
            // do it if not done
            if (!EdgesBuilt[EBOTEND])
            {


                Exceptions.Standard_DomainError_Raise_if
                      (!HasBottom() || !HasSides(),
                       "BRepPrim_OneAxis::EndBottomEdge:no sides or no bottom");

                // build the empty Edge
                gp_Vec V = myAxes.Direction();
                V.Multiply(MeridianValue(myVMin).Y());
                gp_Pnt P = myAxes.Location().Translated(V);
                gp_Lin L = new gp_Lin(P, myAxes.XDirection());
                L.Rotate(myAxes.Axis(), myAngle);
                myBuilder.MakeEdge(myEdges[EBOTEND], L);

                myBuilder.AddEdgeVertex(myEdges[EBOTEND], AxisBottomVertex(),
                            0.0, true);
                myBuilder.AddEdgeVertex(myEdges[EBOTEND], BottomEndVertex(),
                            MeridianValue(myVMin).X(), false);

                myBuilder.CompleteEdge(myEdges[EBOTEND]);
                EdgesBuilt[EBOTEND] = true;
            }

            return myEdges[EBOTEND];
        }
        TopoDS_Face EndFace()
        {
            // do it if not done
            if (!FacesBuilt[FEND])
            {

                Exceptions.Standard_DomainError_Raise_if(!HasSides(),
                                "BRepPrim_OneAxes::EndFace:No side faces");

                // build the empty face, perpendicular to myTool.Axes()
                gp_Ax2 axes = new(myAxes.Location(), myAxes.YDirection().Reversed(), myAxes.XDirection());
                axes.Rotate(myAxes.Axis(), myAngle);
                myBuilder.MakeFace(myFaces[FEND], new gp_Pln(new gp_Ax3(axes)));
                myBuilder.ReverseFace(myFaces[FEND]);

                if (VMaxInfinite() && VMinInfinite())
                    myBuilder.AddFaceWire(myFaces[FEND], AxisEndWire());
                myBuilder.AddFaceWire(myFaces[FEND], EndWire());

                // parametric curves
                SetMeridianPCurve(myEdges[EEND], myFaces[FEND]);
                if (EdgesBuilt[EAXIS])
                    myBuilder.SetPCurve(myEdges[EAXIS], myFaces[FEND],
                         new gp_Lin2d(new gp_Pnt2d(0, 0), new gp_Dir2d(0, 1)));
                if (EdgesBuilt[ETOPEND])
                    myBuilder.SetPCurve(myEdges[ETOPEND], myFaces[FEND],
                         new gp_Lin2d(new gp_Pnt2d(0, MeridianValue(myVMax).Y()),
                              new gp_Dir2d(1, 0)));
                if (EdgesBuilt[EBOTEND])
                    myBuilder.SetPCurve(myEdges[EBOTEND], myFaces[FEND],
                          new gp_Lin2d(new gp_Pnt2d(0, MeridianValue(myVMin).Y()),
                      new gp_Dir2d(1, 0)));

                myBuilder.CompleteFace(myFaces[FEND]);
                FacesBuilt[FEND] = true;
            }

            return myFaces[FEND];
        }

        TopoDS_Wire AxisEndWire()
        {
            // do it if not done
            if (!WiresBuilt[WAXISEND])
            {

                Exceptions.Standard_DomainError_Raise_if
                      (!HasSides(),
                       "BRepPrim_OneAxes::AxisEndWire:no sides");

                Exceptions.Standard_DomainError_Raise_if
                  (!VMaxInfinite() || !VMinInfinite(),
                   "BRepPrim_OneAxes::AxisEndWire:not infinite");

                Exceptions.Standard_DomainError_Raise_if
                  (MeridianClosed(),
                   "BRepPrim_OneAxes::AxisEndWire:meridian closed");

                myBuilder.MakeWire(myWires[WAXISEND]);

                myBuilder.AddWireEdge(myWires[WAXISEND], AxisEdge(), true);

                myBuilder.CompleteWire(myWires[WAXISEND]);
                WiresBuilt[WAXISEND] = true;
            }
            return myWires[WAXISEND];
        }

        //! Returns the   wire in the   lateral  face with the
        //! start edge.
        public TopoDS_Wire LateralStartWire()
        {
            // do it if not done
            if (!WiresBuilt[WLATERALSTART])
            {

                myBuilder.MakeWire(myWires[WLATERALSTART]);

                myBuilder.AddWireEdge(myWires[WLATERALSTART], StartEdge(), false);

                myBuilder.CompleteWire(myWires[WLATERALSTART]);
                WiresBuilt[WLATERALSTART] = true;
            }

            return myWires[WLATERALSTART];
        }

        //! Returns the wire with in lateral face with the end
        //! edge.
        public TopoDS_Wire LateralEndWire()
        {
            // do it if not done
            if (!WiresBuilt[WLATERALEND])
            {

                myBuilder.MakeWire(myWires[WLATERALEND]);

                myBuilder.AddWireEdge(myWires[WLATERALEND], EndEdge(), true);

                myBuilder.CompleteWire(myWires[WLATERALEND]);
                WiresBuilt[WLATERALEND] = true;
            }

            return myWires[WLATERALEND];
        }

        TopoDS_Edge EndEdge()
        {
            // do it if not done
            if (!EdgesBuilt[EEND])
            {

                // is it shared with the start edge
                if (!HasSides() && EdgesBuilt[ESTART])
                    myEdges[EEND] = myEdges[ESTART];

                else
                {
                    // build the empty Edge
                    myEdges[EEND] = MakeEmptyMeridianEdge(myAngle);


                    if (MeridianClosed())
                    {
                        // Closed edge
                        myBuilder.AddEdgeVertex(myEdges[EEND],
                                    TopEndVertex(),
                                    myVMin + myMeridianOffset,
                                    myVMax + myMeridianOffset);
                    }
                    else
                    {
                        if (!VMaxInfinite())
                        {
                            myBuilder.AddEdgeVertex(myEdges[EEND],
                                        TopEndVertex(),
                                        myVMax + myMeridianOffset,
                                        false);
                        }
                        if (!VMinInfinite())
                        {
                            myBuilder.AddEdgeVertex(myEdges[EEND],
                                        BottomEndVertex(),
                                        myVMin + myMeridianOffset,
                                        true);
                        }
                    }
                }

                myBuilder.CompleteEdge(myEdges[EEND]);
                EdgesBuilt[EEND] = true;

            }

            return myEdges[EEND];
        }

        TopoDS_Vertex BottomEndVertex()
        {
            // do it if not done
            if (!VerticesBuilt[VBOTEND])
            {

                // deduct from others
                if (MeridianOnAxis(myVMin) && VerticesBuilt[VAXISBOT])
                    myVertices[VBOTEND] = new TopoDS_Vertex(myVertices[VAXISBOT]);
                else if ((MeridianOnAxis(myVMin) || !HasSides()) && VerticesBuilt[VBOTSTART])
                    myVertices[VBOTEND] = new TopoDS_Vertex(myVertices[VBOTSTART]);
                else if (MeridianClosed() && VerticesBuilt[VTOPEND])
                    myVertices[VBOTEND] = new TopoDS_Vertex(myVertices[VTOPEND]);
                else if (MeridianClosed() && !HasSides() && VerticesBuilt[VTOPSTART])
                    myVertices[VBOTEND] = new TopoDS_Vertex(myVertices[VTOPSTART]);

                else
                {
                    gp_Pnt2d mp = MeridianValue(myVMin);
                    gp_Vec V = myAxes.Direction();
                    V.Multiply(mp.Y());
                    gp_Pnt P = myAxes.Location().Translated(V);
                    V = myAxes.XDirection();
                    V.Multiply(mp.X());
                    P.Translate(V);
                    P.Rotate(myAxes.Axis(), myAngle);
                    myBuilder.MakeVertex(myVertices[VBOTEND], P);
                }

                VerticesBuilt[VBOTEND] = true;
            }

            return myVertices[VBOTEND];
        }

        TopoDS_Vertex TopEndVertex()
        {
            // do it if not done
            if (!VerticesBuilt[VTOPEND])
            {


                // deduct from others
                if (MeridianOnAxis(myVMax) && VerticesBuilt[VAXISTOP])
                    myVertices[VTOPEND] = new TopoDS_Vertex(myVertices[VAXISTOP]);//not origin
                else if ((MeridianOnAxis(myVMax) || !HasSides()) && VerticesBuilt[VTOPSTART])
                    myVertices[VTOPEND] = new TopoDS_Vertex(myVertices[VTOPSTART]);
                else if (MeridianClosed() && VerticesBuilt[VBOTEND])
                    myVertices[VTOPEND] = new TopoDS_Vertex(myVertices[VBOTEND]);
                else if ((MeridianClosed() && !HasSides()) && VerticesBuilt[VBOTSTART])
                    myVertices[VTOPEND] = new TopoDS_Vertex(myVertices[VBOTSTART]);

                else
                {
                    gp_Pnt2d mp = MeridianValue(myVMax);
                    gp_Vec V = myAxes.Direction();
                    V.Multiply(mp.Y());
                    gp_Pnt P = myAxes.Location().Translated(V);
                    V = myAxes.XDirection();
                    V.Multiply(mp.X());
                    P.Translate(V);
                    P.Rotate(myAxes.Axis(), myAngle);
                    myBuilder.MakeVertex(myVertices[VTOPEND], P);
                }

                VerticesBuilt[VTOPEND] = true;
            }

            return myVertices[VTOPEND];
        }

        //! Returns a face with  no edges.  The surface is the
        //! lateral surface with normals pointing outward. The
        //! U parameter is the angle with the  origin on the X
        //! axis. The  V parameter is   the  parameter of  the
        //! meridian.
        public abstract TopoDS_Face MakeEmptyLateralFace();

        //! Returns  the lateral Face.   It is oriented toward
        //! the outside of the primitive.
        public TopoDS_Face LateralFace()
        {
            // do it if not done
            if (!FacesBuilt[FLATERAL])
            {

                // build an empty lateral face
                myFaces[FLATERAL] = MakeEmptyLateralFace();

                // add the wires
                if (VMaxInfinite() && VMinInfinite())
                {
                    myBuilder.AddFaceWire(myFaces[FLATERAL], LateralStartWire());
                    myBuilder.AddFaceWire(myFaces[FLATERAL], LateralEndWire());
                }
                else
                    myBuilder.AddFaceWire(myFaces[FLATERAL], LateralWire());

                // put the parametric curves
                if (MeridianClosed())
                {
                    // closed edge
                    myBuilder.SetPCurve(myEdges[ETOP], myFaces[FLATERAL],
                         new gp_Lin2d(new gp_Pnt2d(0, myVMin), new gp_Dir2d(1, 0)),
                           new gp_Lin2d(new gp_Pnt2d(0, myVMax), new gp_Dir2d(1, 0)));
                }
                else
                {
                    if (!VMaxInfinite())
                    {
                        myBuilder.SetPCurve(myEdges[ETOP], myFaces[FLATERAL],
                                    new gp_Lin2d(new gp_Pnt2d(0, myVMax), new gp_Dir2d(1, 0)));
                        if (!HasSides() || MeridianOnAxis(myVMax))
                        {
                            // closed edge set parameters
                            myBuilder.SetParameters(myEdges[ETOP],
                                        TopEndVertex(),
                                        0.0, myAngle);
                        }
                    }
                    if (!VMinInfinite())
                    {
                        myBuilder.SetPCurve(myEdges[EBOTTOM], myFaces[FLATERAL],
                                   new gp_Lin2d(new gp_Pnt2d(0, myVMin), new gp_Dir2d(1, 0)));
                        if (!HasSides() || MeridianOnAxis(myVMin))
                        {
                            // closed edge set parameters
                            myBuilder.SetParameters(myEdges[EBOTTOM],
                                        BottomEndVertex(),
                                        0.0, myAngle);
                        }
                    }
                }
                if (HasSides())
                {
                    myBuilder.SetPCurve(myEdges[ESTART], myFaces[FLATERAL],
                          new gp_Lin2d(new gp_Pnt2d(0, -myMeridianOffset),
                                new gp_Dir2d(0, 1)));

                    myBuilder.SetPCurve(myEdges[EEND], myFaces[FLATERAL],
                          new gp_Lin2d(new gp_Pnt2d(myAngle, -myMeridianOffset),
                            new gp_Dir2d(0, 1)));
                }
                else
                {
                    // closed edge
                    myBuilder.SetPCurve(myEdges[ESTART], myFaces[FLATERAL],
                          new gp_Lin2d(new gp_Pnt2d(myAngle, -myMeridianOffset),
                              new gp_Dir2d(0, 1)),
                          new gp_Lin2d(new gp_Pnt2d(0, -myMeridianOffset),
                               new gp_Dir2d(0, 1)));
                }
                myBuilder.CompleteFace(myFaces[FLATERAL]);
                FacesBuilt[FLATERAL] = true;
            }
            return myFaces[FLATERAL];
        }


        TopoDS_Wire LateralWire()
        {
            // do it if not done
            if (!WiresBuilt[WLATERAL])
            {

                myBuilder.MakeWire(myWires[WLATERAL]);

                if (!VMaxInfinite())
                    myBuilder.AddWireEdge(myWires[WLATERAL], TopEdge(), false);
                myBuilder.AddWireEdge(myWires[WLATERAL], EndEdge(), true);
                if (!VMinInfinite())
                    myBuilder.AddWireEdge(myWires[WLATERAL], BottomEdge(), true);
                myBuilder.AddWireEdge(myWires[WLATERAL], StartEdge(), false);

                myBuilder.CompleteWire(myWires[WLATERAL]);
                WiresBuilt[WLATERAL] = true;
            }

            return myWires[WLATERAL];
        }


        //! Sets the  parametric curve of the  edge <E> in the
        //! face  <F> to be  the   2d representation  of   the
        //! meridian.
        public abstract void SetMeridianPCurve(TopoDS_Edge E, TopoDS_Face F);

        TopoDS_Face StartFace()
        {
            // do it if not done
            if (!FacesBuilt[FSTART])
            {

                Exceptions.Standard_DomainError_Raise_if(!HasSides(),
                                  "BRepPrim_OneAxes::StartFace:No side faces");

                // build the empty face, perpendicular to myTool.Axes()
                gp_Ax2 axes = new(myAxes.Location(), myAxes.YDirection().Reversed(), myAxes.XDirection());
                myBuilder.MakeFace(myFaces[FSTART], new gp_Pln(new gp_Ax3(axes)));


                if (VMaxInfinite() && VMinInfinite())
                    myBuilder.AddFaceWire(myFaces[FSTART], AxisStartWire());

                myBuilder.AddFaceWire(myFaces[FSTART], StartWire());

                // parametric curves
                SetMeridianPCurve(myEdges[ESTART], myFaces[FSTART]);
                if (EdgesBuilt[EAXIS])
                    myBuilder.SetPCurve(myEdges[EAXIS], myFaces[FSTART],
                            new gp_Lin2d(new gp_Pnt2d(0, 0), new gp_Dir2d(0, 1)));
                if (EdgesBuilt[ETOPSTART])
                    myBuilder.SetPCurve(myEdges[ETOPSTART], myFaces[FSTART],
                           new gp_Lin2d(new gp_Pnt2d(0, MeridianValue(myVMax).Y()), new gp_Dir2d(1, 0)));
                if (EdgesBuilt[EBOTSTART])
                    myBuilder.SetPCurve(myEdges[EBOTSTART], myFaces[FSTART],
                           new gp_Lin2d(new gp_Pnt2d(0, MeridianValue(myVMin).Y()), new gp_Dir2d(1, 0)));


                myBuilder.CompleteFace(myFaces[FSTART]);
                FacesBuilt[FSTART] = true;
            }

            return myFaces[FSTART];
        }

        public void SetMeridianOffset(double O)
        {
            myMeridianOffset = O;
        }

        TopoDS_Wire StartWire()
        {
            // do it if not done
            if (!WiresBuilt[WSTART])
            {

                Exceptions.Standard_DomainError_Raise_if(!HasSides(),
                                "BRepPrim_OneAxes::StartWire:no sides");

                myBuilder.MakeWire(myWires[WSTART]);

                if (HasBottom())
                    myBuilder.AddWireEdge(myWires[WSTART], StartBottomEdge(), true);

                if (!MeridianClosed())
                {
                    if (!VMaxInfinite() || !VMinInfinite())
                        myBuilder.AddWireEdge(myWires[WSTART], AxisEdge(), false);
                }

                if (HasTop())
                    myBuilder.AddWireEdge(myWires[WSTART], StartTopEdge(), false);
                myBuilder.AddWireEdge(myWires[WSTART], StartEdge(), true);

                myBuilder.CompleteWire(myWires[WSTART]);
                WiresBuilt[WSTART] = true;
            }

            return myWires[WSTART];
        }

        //! Returns  an  edge with  a 3D curve   made from the
        //! meridian  in the XZ  plane rotated by <Ang> around
        //! the Z-axis. Ang may be 0 or myAngle.
        public abstract TopoDS_Edge MakeEmptyMeridianEdge(double Ang);

        TopoDS_Edge StartEdge()
        {
            // do it if not done
            if (!EdgesBuilt[ESTART])
            {

                // is it shared with the EndEdge

                if (!HasSides() && EdgesBuilt[EEND])
                    myEdges[ESTART] = myEdges[EEND];

                else
                {
                    // build the empty Edge
                    myEdges[ESTART] = MakeEmptyMeridianEdge(0.0);

                    if (MeridianClosed())
                    {
                        // Closed edge
                        myBuilder.AddEdgeVertex(myEdges[ESTART],
                                    TopStartVertex(),
                                    myVMin + myMeridianOffset,
                                    myVMax + myMeridianOffset);
                    }
                    else
                    {
                        if (!VMaxInfinite())
                        {
                            myBuilder.AddEdgeVertex(myEdges[ESTART],
                                        TopStartVertex(),
                                        myVMax + myMeridianOffset,
                                        false);
                        }
                        if (!VMinInfinite())
                        {
                            myBuilder.AddEdgeVertex(myEdges[ESTART],
                                        BottomStartVertex(),
                                        myVMin + myMeridianOffset,
                                        true);
                        }
                    }
                }

                myBuilder.CompleteEdge(myEdges[ESTART]);
                EdgesBuilt[ESTART] = true;

            }

            return myEdges[ESTART];
        }


        TopoDS_Edge EndTopEdge()
        {
            // do it if not done
            if (!EdgesBuilt[ETOPEND])
            {

                Exceptions.Standard_DomainError_Raise_if
                      (!HasTop() || !HasSides(),
                       "BRepPrim_OneAxis::EndTopEdge:no sides or no top");

                // build the empty Edge
                gp_Vec V = myAxes.Direction();
                V.Multiply(MeridianValue(myVMax).Y());
                gp_Pnt P = myAxes.Location().Translated(V);
                gp_Lin L = new gp_Lin(P, myAxes.XDirection());
                L.Rotate(myAxes.Axis(), myAngle);
                myBuilder.MakeEdge(myEdges[ETOPEND], L);

                myBuilder.AddEdgeVertex(myEdges[ETOPEND], AxisTopVertex(),
                            0.0, true);
                myBuilder.AddEdgeVertex(myEdges[ETOPEND], TopEndVertex(),
                            MeridianValue(myVMax).X(), false);

                myBuilder.CompleteEdge(myEdges[ETOPEND]);
                EdgesBuilt[ETOPEND] = true;
            }

            return myEdges[ETOPEND];
        }

        TopoDS_Edge StartTopEdge()
        {
            // do it if not done
            if (!EdgesBuilt[ETOPSTART])
            {

                Exceptions.Standard_DomainError_Raise_if
                      (!HasTop() || !HasSides(),
                       "BRepPrim_OneAxis::StartTopEdge:no sides or no top");

                // build the empty Edge
                gp_Vec V = myAxes.Direction();
                V.Multiply(MeridianValue(myVMax).Y());
                gp_Pnt P = myAxes.Location().Translated(V);
                myBuilder.MakeEdge(myEdges[ETOPSTART], new gp_Lin(P, myAxes.XDirection()));

                myBuilder.AddEdgeVertex(myEdges[ETOPSTART], AxisTopVertex(),
                            0.0, true);
                myBuilder.AddEdgeVertex(myEdges[ETOPSTART], TopStartVertex(),
                            MeridianValue(myVMax).X(), false);

                myBuilder.CompleteEdge(myEdges[ETOPSTART]);
                EdgesBuilt[ETOPSTART] = true;
            }

            return myEdges[ETOPSTART];
        }

        TopoDS_Vertex TopStartVertex()
        {
            // do it if not done
            if (!VerticesBuilt[VTOPSTART])
            {

                // deduct from others
                if (MeridianOnAxis(myVMax) && VerticesBuilt[VAXISTOP])
                    myVertices[VTOPSTART] = new TopoDS_Vertex(myVertices[VAXISTOP]);
                else if ((MeridianOnAxis(myVMax) || !HasSides()) && VerticesBuilt[VTOPEND])
                    myVertices[VTOPSTART] = new TopoDS_Vertex(myVertices[VTOPEND]);
                else if (MeridianClosed() && VerticesBuilt[VBOTSTART])
                    myVertices[VTOPSTART] = new TopoDS_Vertex(myVertices[VBOTSTART]);
                else if ((MeridianClosed() && !HasSides()) && VerticesBuilt[VBOTEND])
                    myVertices[VTOPSTART] = new TopoDS_Vertex(myVertices[VBOTEND]);

                else
                {
                    gp_Pnt2d mp = MeridianValue(myVMax);
                    gp_Vec V = myAxes.Direction();
                    V.Multiply(mp.Y());
                    gp_Pnt P = myAxes.Location().Translated(V);
                    V = myAxes.XDirection();
                    V.Multiply(mp.X());
                    P.Translate(V);
                    myBuilder.MakeVertex(myVertices[VTOPSTART], P);
                }

                VerticesBuilt[VTOPSTART] = true;
            }

            return myVertices[VTOPSTART];
        }
        TopoDS_Edge StartBottomEdge()
        {
            // do it if not done
            if (!EdgesBuilt[EBOTSTART])
            {

                Exceptions.Standard_DomainError_Raise_if
                      (!HasBottom() || !HasSides(),
                       "BRepPrim_OneAxis::StartBottomEdge:no sides or no top");

                // build the empty Edge
                gp_Vec V = myAxes.Direction();
                V.Multiply(MeridianValue(myVMin).Y());
                gp_Pnt P = myAxes.Location().Translated(V);
                myBuilder.MakeEdge(myEdges[EBOTSTART], new gp_Lin(P, myAxes.XDirection()));

                myBuilder.AddEdgeVertex(myEdges[EBOTSTART], BottomStartVertex(),
                            MeridianValue(myVMin).X(), false);
                myBuilder.AddEdgeVertex(myEdges[EBOTSTART], AxisBottomVertex(),
                            0.0, true);

                myBuilder.CompleteEdge(myEdges[EBOTSTART]);
                EdgesBuilt[EBOTSTART] = true;
            }

            return myEdges[EBOTSTART];
        }

        TopoDS_Vertex BottomStartVertex()
        {
            // do it if not done
            if (!VerticesBuilt[VBOTSTART])
            {

                // deduct from others
                if (MeridianOnAxis(myVMin) && VerticesBuilt[VAXISBOT])
                    myVertices[VBOTSTART] = new TopoDS_Vertex(myVertices[VAXISBOT]);
                else if ((MeridianOnAxis(myVMin) || !HasSides()) && VerticesBuilt[VBOTEND])
                    myVertices[VBOTSTART] = new TopoDS_Vertex(myVertices[VBOTEND]);
                else if (MeridianClosed() && VerticesBuilt[VTOPSTART])
                    myVertices[VBOTSTART] = new TopoDS_Vertex(myVertices[VTOPSTART]);
                else if ((MeridianClosed() && !HasSides()) && VerticesBuilt[VTOPEND])
                    myVertices[VBOTSTART] = new TopoDS_Vertex(myVertices[VTOPEND]);

                else
                {
                    gp_Pnt2d mp = MeridianValue(myVMin);
                    gp_Vec V = myAxes.Direction();
                    V.Multiply(mp.Y());
                    gp_Pnt P = myAxes.Location().Translated(V);
                    V = myAxes.XDirection();
                    V.Multiply(mp.X());
                    P.Translate(V);
                    myBuilder.MakeVertex(myVertices[VBOTSTART], P);
                }

                VerticesBuilt[VBOTSTART] = true;
            }

            return myVertices[VBOTSTART];
        }

        //! Returns  the wire   in the  start   face  with the
        //! AxisEdge.
        public TopoDS_Wire AxisStartWire()
        {
            // do it if not done
            if (!WiresBuilt[WAXISSTART])
            {

                Exceptions.Standard_DomainError_Raise_if
                     (!HasSides(),
                      "BRepPrim_OneAxes::AxisStartWire:no sides");

                Exceptions.Standard_DomainError_Raise_if
                  (!VMaxInfinite() || !VMinInfinite(),
                   "BRepPrim_OneAxes::AxisStartWire:not infinite");

                Exceptions.Standard_DomainError_Raise_if
                  (MeridianClosed(),
                   "BRepPrim_OneAxes::AxisStartWire:meridian closed");

                myBuilder.MakeWire(myWires[WAXISSTART]);

                myBuilder.AddWireEdge(myWires[WAXISSTART], AxisEdge(), false);

                myBuilder.CompleteWire(myWires[WAXISSTART]);
                WiresBuilt[WAXISSTART] = true;
            }

            return myWires[WAXISSTART];
        }

        TopoDS_Vertex AxisBottomVertex()
        {
            // do it if not done
            if (!VerticesBuilt[VAXISBOT])
            {

                // deduct from others
                if (MeridianOnAxis(myVMin) && VerticesBuilt[VBOTSTART])
                    myVertices[VAXISBOT] = new TopoDS_Vertex(myVertices[VBOTSTART]);

                else if (MeridianOnAxis(myVMin) && VerticesBuilt[VBOTEND])
                    myVertices[VAXISBOT] = new TopoDS_Vertex(myVertices[VBOTEND]);

                else
                {
                    Exceptions.Standard_DomainError_Raise_if(MeridianClosed(),
                                    "BRepPrim_OneAxis::AxisBottomVertex");
                    Exceptions.Standard_DomainError_Raise_if(VMinInfinite(),
                                  "BRepPrim_OneAxis::AxisBottomVertex");

                    gp_Vec V = myAxes.Direction();
                    V.Multiply(MeridianValue(myVMin).Y());
                    gp_Pnt P = myAxes.Location().Translated(V);
                    myBuilder.MakeVertex(myVertices[VAXISBOT], P);
                }

                VerticesBuilt[VAXISBOT] = true;
            }

            return myVertices[VAXISBOT];
        }
        TopoDS_Edge AxisEdge()
        {
            // do it if not done
            if (!EdgesBuilt[EAXIS])
            {

                Exceptions.Standard_DomainError_Raise_if(!HasSides(),
                               "BRepPrim_OneAxis::AxisEdge:no sides");
                Exceptions.Standard_DomainError_Raise_if(MeridianClosed(),
                              "BRepPrim_OneAxis::AxisEdge:closed");

                // build the empty edge.
                myBuilder.MakeEdge(myEdges[EAXIS], new gp_Lin(myAxes.Axis()));

                if (!VMaxInfinite())
                    myBuilder.AddEdgeVertex(myEdges[EAXIS], AxisTopVertex(),
                                MeridianValue(myVMax).Y(), false);
                if (!VMinInfinite())
                    myBuilder.AddEdgeVertex(myEdges[EAXIS], AxisBottomVertex(),
                                MeridianValue(myVMin).Y(), true);

                myBuilder.CompleteEdge(myEdges[EAXIS]);
                EdgesBuilt[EAXIS] = true;
            }

            return myEdges[EAXIS];
        }

        TopoDS_Vertex AxisTopVertex()
        {
            // do it if not done
            if (!VerticesBuilt[VAXISTOP])
            {

                // deduct from others
                if (MeridianOnAxis(myVMax) && VerticesBuilt[VTOPSTART])
                    myVertices[VAXISTOP] = new TopoDS_Vertex(myVertices[VTOPSTART]);

                else if (MeridianOnAxis(myVMax) && VerticesBuilt[VTOPEND])
                    myVertices[VAXISTOP] = new TopoDS_Vertex(myVertices[VTOPEND]);

                else
                {
                    Exceptions.Standard_DomainError_Raise_if(MeridianClosed(),
                                    "BRepPrim_OneAxis::AxisTopVertex");
                    Exceptions.Standard_DomainError_Raise_if(VMaxInfinite(),
                                  "BRepPrim_OneAxis::AxisTopVertex");

                    gp_Vec V = myAxes.Direction();
                    V.Multiply(MeridianValue(myVMax).Y());
                    gp_Pnt P = myAxes.Location().Translated(V);
                    myBuilder.MakeVertex(myVertices[VAXISTOP], P);
                }

                VerticesBuilt[VAXISTOP] = true;
            }

            return myVertices[VAXISTOP];
        }

        public bool HasSides()
        {
            return 2 * Math.PI - myAngle > Precision.Angular();
        }

        //! Returns  the Bottom planar Face.   It is  Oriented
        //! toward the -Z axis (outside).
        public TopoDS_Face BottomFace()
        {
            // do it if not done
            if (!FacesBuilt[FBOTTOM])
            {

                Exceptions.Standard_DomainError_Raise_if(!HasBottom(),
                                "BRepPrim_OneAxis::BottomFace:No bottom face");

                // make the empty face by translating the axes
                double z = MeridianValue(myVMin).Y();
                gp_Vec V = myAxes.Direction();
                V.Multiply(z);
                gp_Ax2 axes = myAxes.Translated(V);
                myBuilder.MakeFace(myFaces[FBOTTOM], new gp_Pln(new gp_Ax3(axes)));
                myBuilder.ReverseFace(myFaces[FBOTTOM]);
                myBuilder.AddFaceWire(myFaces[FBOTTOM], BottomWire());

                // put the parametric curves
                myBuilder.SetPCurve(myEdges[EBOTTOM], myFaces[FBOTTOM],
                        new gp_Circ2d(new gp_Ax2d(new gp_Pnt2d(0, 0), new gp_Dir2d(1, 0)),
                              MeridianValue(myVMin).X()));
                if (HasSides())
                {
                    myBuilder.SetPCurve(myEdges[EBOTSTART], myFaces[FBOTTOM],
                           new gp_Lin2d(new gp_Pnt2d(0, 0), new gp_Dir2d(1, 0)));
                    myBuilder.SetPCurve(myEdges[EBOTEND], myFaces[FBOTTOM],
                           new gp_Lin2d(new gp_Pnt2d(0, 0),
                               new gp_Dir2d(Math.Cos(myAngle), Math.Sin(myAngle))));
                }

                myBuilder.CompleteFace(myFaces[FBOTTOM]);
                FacesBuilt[FBOTTOM] = true;
            }

            return myFaces[FBOTTOM];
        }

        TopoDS_Wire BottomWire()
        {
            // do it if not done
            if (!WiresBuilt[WBOTTOM])
            {

                Exceptions.Standard_DomainError_Raise_if(!HasBottom(),
                                   "BRepPrim_OneAxis::BottomWire: no bottom");

                myBuilder.MakeWire(myWires[WBOTTOM]);

                myBuilder.AddWireEdge(myWires[WBOTTOM], BottomEdge(), false);
                if (HasSides())
                {
                    myBuilder.AddWireEdge(myWires[WBOTTOM], EndBottomEdge(), true);
                    myBuilder.AddWireEdge(myWires[WBOTTOM], StartBottomEdge(), false);
                }

                myBuilder.CompleteWire(myWires[WBOTTOM]);
                WiresBuilt[WBOTTOM] = true;
            }

            return myWires[WBOTTOM];
        }



        TopoDS_Edge BottomEdge()
        {
            // do it if not done
            if (!EdgesBuilt[EBOTTOM])
            {

                // Test if shared with top edge
                if (MeridianClosed() && EdgesBuilt[ETOP])
                {
                    myEdges[EBOTTOM] = myEdges[ETOP];
                }

                else
                {

                    // build the empty Edge

                    if (!MeridianOnAxis(myVMin))
                    {
                        gp_Pnt2d mp = MeridianValue(myVMin);
                        gp_Vec V = myAxes.Direction();
                        V.Multiply(mp.Y());
                        gp_Pnt P = myAxes.Location().Translated(V);
                        gp_Circ C = new(new gp_Ax2(P, myAxes.Direction(), myAxes.XDirection()), mp.X());
                        myBuilder.MakeEdge(myEdges[EBOTTOM], C);
                    }
                    else
                        myBuilder.MakeDegeneratedEdge(myEdges[EBOTTOM]);

                    if (!HasSides())
                    {
                        // closed edge
                        myBuilder.AddEdgeVertex(myEdges[EBOTTOM],
                                    BottomEndVertex(),
                                    0.0, myAngle);
                    }
                    else
                    {
                        myBuilder.AddEdgeVertex(myEdges[EBOTTOM],
                                    BottomEndVertex(),
                                    myAngle,
                                    false);
                        myBuilder.AddEdgeVertex(myEdges[EBOTTOM],
                                    BottomStartVertex(),
                                    0.0,
                                    true);
                    }
                }

                myBuilder.CompleteEdge(myEdges[EBOTTOM]);
                EdgesBuilt[EBOTTOM] = true;
            }

            return myEdges[EBOTTOM];
        }

        bool HasBottom()
        {
            if (VMinInfinite()) return false;
            if (MeridianClosed()) return false;
            if (MeridianOnAxis(myVMin)) return false;
            return true;
        }

        const int NBVERTICES = 6;
        const int VAXISTOP = 0;
        const int VAXISBOT = 1;
        const int VTOPSTART = 2;
        const int VTOPEND = 3;
        const int VBOTSTART = 4;
        const int VBOTEND = 5;
        const int NBEDGES = 9;
        const int EAXIS = 0;
        const int ESTART = 1;
        const int EEND = 2;
        const int ETOPSTART = 3;
        const int ETOPEND = 4;
        const int EBOTSTART = 5;
        const int EBOTEND = 6;
        const int ETOP = 7;
        const int EBOTTOM = 8;
        const int NBWIRES = 9;
        const int WLATERAL = 0;
        const int WLATERALSTART = 0;
        const int WLATERALEND = 1;
        const int WTOP = 2;
        const int WBOTTOM = 3;
        const int WSTART = 5;
        const int WAXISSTART = 6;
        const int WEND = 7;
        const int WAXISEND = 8;
        const int NBFACES = 5;
        const int FLATERAL = 0;
        const int FTOP = 1;
        const int FBOTTOM = 2;
        const int FSTART = 3;
        const int FEND = 4;


        public bool HasTop()
        {
            if (VMaxInfinite()) return false;
            if (MeridianClosed()) return false;
            if (MeridianOnAxis(myVMax)) return false;
            return true;
        }

        bool MeridianOnAxis(double V)
        {
            return Math.Abs(MeridianValue(V).X()) < Precision.Confusion();
        }

        bool MeridianClosed()
        {
            if (VMaxInfinite()) return false;
            if (VMinInfinite()) return false;
            return MeridianValue(myVMin).IsEqual(MeridianValue(myVMax),
                                 Precision.Confusion());
        }

        bool VMinInfinite()
        {
            return Precision.IsNegativeInfinite(myVMin);
        }


        bool VMaxInfinite()
        {
            return Precision.IsPositiveInfinite(myVMax);
        }

        public TopoDS_Face TopFace()
        {
            // do it if not done
            if (!FacesBuilt[FTOP])
            {

                Exceptions.Standard_DomainError_Raise_if(!HasTop(),
                                 "BRepPrim_OneAxis::TopFace:No top face");

                // make the empty face by translating the axes
                double z = MeridianValue(myVMax).Y();
                gp_Vec V = myAxes.Direction();
                V.Multiply(z);
                myBuilder.MakeFace(myFaces[FTOP], new gp_Pln(new gp_Ax3(myAxes.Translated(V))));

                myBuilder.AddFaceWire(myFaces[FTOP], TopWire());

                // put the parametric curves
                myBuilder.SetPCurve(myEdges[ETOP], myFaces[FTOP],
                        new gp_Circ2d(new gp_Ax2d(new gp_Pnt2d(0, 0), new gp_Dir2d(1, 0)),
                              MeridianValue(myVMax).X()));
                if (HasSides())
                {
                    myBuilder.SetPCurve(myEdges[ETOPSTART], myFaces[FTOP],
                            new gp_Lin2d(new gp_Pnt2d(0, 0), new gp_Dir2d(1, 0)));
                    myBuilder.SetPCurve(myEdges[ETOPEND], myFaces[FTOP],
                            new gp_Lin2d(new gp_Pnt2d(0, 0),
                                new gp_Dir2d(Math.Cos(myAngle), Math.Sin(myAngle))));
                }

                myBuilder.CompleteFace(myFaces[FTOP]);
                FacesBuilt[FTOP] = true;
            }

            return myFaces[FTOP];
        }


        TopoDS_Edge TopEdge()
        {
            // do it if not done
            if (!EdgesBuilt[ETOP])
            {

                // Test if shared with bottom edge
                if (MeridianClosed() && EdgesBuilt[EBOTTOM])
                {
                    myEdges[ETOP] = myEdges[EBOTTOM];
                }

                else
                {

                    // build the empty Edge
                    if (!MeridianOnAxis(myVMax))
                    {
                        gp_Pnt2d mp = MeridianValue(myVMax);
                        gp_Vec V = myAxes.Direction();
                        V.Multiply(mp.Y());
                        gp_Pnt P = myAxes.Location().Translated(V);
                        gp_Circ C = new gp_Circ(new gp_Ax2(P, myAxes.Direction(), myAxes.XDirection()), mp.X());
                        myBuilder.MakeEdge(myEdges[ETOP], C);
                    }
                    else
                        myBuilder.MakeDegeneratedEdge(myEdges[ETOP]);

                    if (!HasSides())
                    {
                        // closed edge
                        myBuilder.AddEdgeVertex(myEdges[ETOP],
                                    TopEndVertex(),
                                    0.0, myAngle);
                    }
                    else
                    {
                        myBuilder.AddEdgeVertex(myEdges[ETOP],
                                    TopEndVertex(),
                                    myAngle,
                                    false);
                        myBuilder.AddEdgeVertex(myEdges[ETOP],
                                    TopStartVertex(),
                                    0.0,
                                    true);
                    }
                }

                myBuilder.CompleteEdge(myEdges[ETOP]);
                EdgesBuilt[ETOP] = true;
            }

            return myEdges[ETOP];
        }

        TopoDS_Wire TopWire()
        {
            // do it if not done
            if (!WiresBuilt[WTOP])
            {

                Exceptions.Standard_DomainError_Raise_if(!HasTop(),
                               "BRepPrim_OneAxis::TopWire: no top");

                myBuilder.MakeWire(myWires[WTOP]);

                myBuilder.AddWireEdge(myWires[WTOP], TopEdge(), true);
                if (HasSides())
                {
                    myBuilder.AddWireEdge(myWires[WTOP], StartTopEdge(), true);
                    myBuilder.AddWireEdge(myWires[WTOP], EndTopEdge(), false);
                }
                myBuilder.CompleteWire(myWires[WTOP]);
                WiresBuilt[WTOP] = true;
            }

            return myWires[WTOP];
        }

        //! Creates a OneAxis algorithm.  <B> is used to build
        //! the Topology. The angle defaults to 2*PI.
        protected BRepPrim_OneAxis(BRepPrim_Builder B, gp_Ax2 A, double VMin, double VMax) : this()
        {
            myBuilder = (B);
            myAxes = (A);
            myAngle = (2 * Math.PI);
            myVMin = (VMin);
            myVMax = (VMax);
            myMeridianOffset = 0;

            // init Built flags
            int i;
            ShellBuilt = false;
            for (i = 0; i < NBVERTICES; i++)
                VerticesBuilt[i] = false;
            for (i = 0; i < NBEDGES; i++)
                EdgesBuilt[i] = false;
            for (i = 0; i < NBWIRES; i++)
                WiresBuilt[i] = false;
            for (i = 0; i < NBFACES; i++)
                FacesBuilt[i] = false;
        }

        protected BRepPrim_Builder myBuilder;

        public gp_Ax2 Axes()
        {
            return myAxes;
        }

        public void Angle(double A)
        {
            BRepPrim_OneAxis_Check(VerticesBuilt, EdgesBuilt, WiresBuilt, FacesBuilt);
            myAngle = A;
        }


        static void BRepPrim_OneAxis_Check(bool[] V,
                      bool[] E,
                      bool[] W,
                      bool[] F)
        {
            int i;
            for (i = 0; i < NBVERTICES; i++)
                if (V[i]) throw new Standard_DomainError();
            for (i = 0; i < NBEDGES; i++)
                if (E[i]) throw new Standard_DomainError();
            for (i = 0; i < NBWIRES; i++)
                if (W[i]) throw new Standard_DomainError();
            for (i = 0; i < NBFACES; i++)
                if (F[i]) throw new Standard_DomainError();
        }

        gp_Ax2 myAxes;
        double myAngle;
        double myVMin;
        double myVMax;
        double myMeridianOffset;
        TopoDS_Shell myShell = new TopoDS_Shell();
        bool ShellBuilt;
        TopoDS_Vertex[] myVertices = new TopoDS_Vertex[6];
        bool[] VerticesBuilt = new bool[6];
        TopoDS_Edge[] myEdges = new TopoDS_Edge[9];
        bool[] EdgesBuilt = new bool[9];
        TopoDS_Wire[] myWires = new TopoDS_Wire[9];
        bool[] WiresBuilt = new bool[9];
        TopoDS_Face[] myFaces = new TopoDS_Face[5];
        bool[] FacesBuilt = new bool[5];

    }
}
