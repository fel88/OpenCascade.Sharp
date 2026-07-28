using AutoDialog;
using OCCPort.Common;
using OCCPort.OpenGL;
using OpenTK;
using OpenTK.GLControl;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TKBO;
using TKBRep;
using TKG3d;
using TKMath;
using TKOpenGl;
using TKPrim;
using TKService;
using TKSTEP;
using TKTopAlgo;
using TKV3d;
using TKXSBASE;
using static OpenTK.Graphics.OpenGL.GL;
using static System.Windows.Forms.DataFormats;


namespace OCCPort.Tester
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
            //glControl = new GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));
            glControl = new GLControl(new GLControlSettings()
            {
                Profile = OpenTK.Windowing.Common.ContextProfile.Compatability,
                NumberOfSamples = 8
            });
            Shown += Form1_Shown;
            v3d_viewer = new V3d_Viewer(new OpenGl_GraphicDriver(new Aspect_DisplayConnection()));
            SizeChanged += Form1_SizeChanged;
            aIS_ViewController = new AIS_ViewController();
            //myAISContext = new AIS_InteractiveContext(v3d_viewer);

            /*if (glControl.Context.GraphicsMode.Samples == 0)
            {
                glControl = new GLControl(new OpenTK.Graphics.GraphicsMode(32, 24, 0, 8));
            }*/
            evwrapper = new EventWrapperGlControl(glControl);

            glControl.Paint += Gl_Paint;
            ViewManager = GravityViewManager = new GravityCameraViewManager(glControl);
            ViewManager.Attach(evwrapper, camera1);
            /*GravityViewManager.View.myView.Items.Add(new Graphic3d_MapOfStructure(
                new Graphic3d_Structure()
                {
                    myCStructure = new OpenGl_Structure()
                    {
                        visible = 1,
                        myBndBox = new Graphic3d_BndBox3d(new BVH_VecNt(0, 0, 0),
                             new BVH_VecNt(50, 50, 50))
                    }
                }));*/
            toolStripContainer1.ContentPanel.Controls.Add(glControl);
            glControl.Dock = DockStyle.Fill;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            GravityViewManager?.View?.MustBeResized();
        }

        OCCTProxy proxy;
        public IOCCTProxyInterface Proxy => proxy;
        [DllImport("opengl32.dll")]
        public static extern IntPtr wglGetCurrentContext();
        nint? hglrc;
        bool inited = false;

        private void Form1_Shown(object sender, EventArgs e)
        {
            proxy = new OCCTProxy();

            hglrc = wglGetCurrentContext();

            Proxy.runOpenTk(glControl.Context.WindowPtr, hglrc.Value);

            inited = true;

            //  proxy.InitOCCTProxy();


            // if (!proxy.InitViewer2(panel1.Handle))
            {

            }
            proxy.ActivateGrid(true);
            //proxy.ShowCube();
            //proxy.SetDisplayMode(1);
            proxy.SetMaterial(1);
            //proxy.SetDegenerateModeOff();
            proxy.RedrawView();

            Color clr1 = Color.DarkBlue;
            Color clr2 = Color.Olive;
            //proxy.SetBackgroundColor(clr1.R, clr1.G, clr1.B, clr2.R, clr2.G, clr2.B);


            proxy.UpdateCurrentViewer();
            proxy.UpdateView();
            Width = Width + 1;
        }

        GravityCameraViewManager GravityViewManager;
        public CameraViewManager ViewManager;
        Camera camera1 = new Camera() { IsOrtho = true };
        private EventWrapperGlControl evwrapper;
        GLControl glControl;

        bool first = true;
        private void Gl_Paint(object sender, PaintEventArgs e)
        {
            //if (!loaded)
            //  return;
            if (!glControl.Context.IsCurrent)
            {
                glControl.MakeCurrent();
            }
            if (first)
            {
                first = false;
                //V3d_View.CreateView = () => new OpenGL.OpenGl_View();


                myAISContext = proxy.gview.myContext;

                //GravityViewManager.View = v3d_viewer.CreateView();
                GravityViewManager.View = proxy.gview.myView;
                //GravityViewManager.View.SetWindow(new Aspect_NeutralWindow() { Width = glControl.Width, Height = glControl.Height }, IntPtr.Zero);
                GravityViewManager.View.MustBeResized();

            }
            /*try
            {
                GravityViewManager.View.Redraw();
            }
            catch (Exception ex)
            {

			}*/

            Redraw();
        }

        void Redraw()
        {
            //GL.ClearColor(Color.Green);
            //GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GravityViewManager.View.Redraw();
            proxy.iterate();
            glControl.SwapBuffers();
            return;


            ViewManager.Update();

            GL.ClearColor(Color.LightGray);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.Viewport(0, 0, glControl.Width, glControl.Height);
            var o2 = Matrix4.CreateOrthographic(glControl.Width, glControl.Height, 1, 1000);

            GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Projection);
            GL.LoadMatrix(ref o2);

            Matrix4 modelview2 = Matrix4.LookAt(0, 0, 70, 0, 0, 0, 0, 1, 0);
            GL.MatrixMode(OpenTK.Graphics.OpenGL.MatrixMode.Modelview);
            GL.LoadMatrix(ref modelview2);



            GL.Enable(EnableCap.DepthTest);

            float zz = -500;
            GL.Begin(PrimitiveType.Quads);
            GL.Color3(Color.LightBlue);
            GL.Vertex3(-glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Vertex3(glControl.Width / 2, -glControl.Height / 2, zz);
            GL.Color3(Color.AliceBlue);
            GL.Vertex3(glControl.Width / 2, glControl.Height / 2, zz);
            GL.Vertex3(-glControl.Width / 2, glControl.Height, zz);
            GL.End();
            GL.PushMatrix();
            GL.Translate(camera1.viewport[2] / 2 - 50, -camera1.viewport[3] / 2 + 50, 0);
            GL.Scale(0.5, 0.5, 0.5);

            var mtr = camera1.ViewMatrix;
            var q = mtr.ExtractRotation();
            var mtr3 = Matrix4d.CreateFromQuaternion(q);
            GL.MultMatrix(ref mtr3);
            GL.LineWidth(2);
            GL.Color3(Color.Red);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(100, 0, 0);
            GL.End();

            GL.Color3(Color.Green);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 100, 0);
            GL.End();

            GL.Color3(Color.Blue);
            GL.Begin(PrimitiveType.Lines);
            GL.Vertex3(0, 0, 0);
            GL.Vertex3(0, 0, 100);
            GL.End();
            GL.PopMatrix();
            camera1.Setup(glControl);

            if (true)
            {
                GL.LineWidth(2);
                GL.Color3(Color.Red);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(100, 0, 0);
                GL.End();

                GL.Color3(Color.Green);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 100, 0);
                GL.End();

                GL.Color3(Color.Blue);
                GL.Begin(PrimitiveType.Lines);
                GL.Vertex3(0, 0, 0);
                GL.Vertex3(0, 0, 100);
                GL.End();
            }

            GL.Enable(EnableCap.Light0);

            GL.ShadeModel(ShadingModel.Smooth);
            /*foreach (var item in Helpers)
            {
                item.Draw(null);
            }

            if (pickEnabled)
                PickUpdate();*/

            glControl.SwapBuffers();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            glControl.Invalidate();
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            GravityViewManager.View.StartRotation(glControl.Width / 2, glControl.Height / 2);
            GravityViewManager.View.Rotation(glControl.Width / 2, glControl.Height / 2 - 20);

        }

        private void frontToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GravityViewManager?.View?.SetProj(V3d_TypeOfOrientation.V3d_Yneg);

            //GravityViewManager.FrontView();
        }

        private void topToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GravityViewManager?.View?.SetProj(V3d_TypeOfOrientation.V3d_Zpos);
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddBoolField("zoomInPoint", "Zoom in point", GravityViewManager.ZoomInPointMode);
            if (!d.ShowDialog())
                return;

            GravityViewManager.ZoomInPointMode = d.GetBoolField("zoomInPoint");
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            GravityViewManager.View.FrontView();
            var res1 = GravityViewManager.View.Convert(40);
            var d1 = GravityViewManager.View.Camera().Distance();





            GravityViewManager.View.Camera().SetScale(3000);
            var res2 = GravityViewManager.View.Convert(40);
            var d2 = GravityViewManager.View.Camera().Distance();
            GravityViewManager.View.Rotate(0, 0.5, 0, 0, 0, 0, true);
            var res3 = GravityViewManager.View.Convert(40);

        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            GravityViewManager.View.FrontView();
            GravityViewManager.View.Camera().SetScale(1000);
            GravityViewManager.View.Rotate(0, 0.5, 0, 0, 0, 0, true);
        }

        private void toolStripButton5_Click(object sender, EventArgs e)
        {
            var proxy = GravityViewManager.View;

            //proxy.Camera().SetScale(1000);
            proxy.FrontView();

            proxy.StartZoomAtPoint(glControl.Width / 2, glControl.Height / 3);
            var p = new System.Drawing.Point(0, 0);
            double delta = (double)(120) / (15 * 8);
            int x = p.X;
            int y = p.Y;
            int x1 = (int)(p.X + glControl.Width * delta / 100);
            int y1 = (int)(p.Y + glControl.Height * delta / 100);
            proxy.ZoomAtPoint(x, y, x1, y1);

            var res1 = proxy.Convert(40);

        }
        TopoDS_Shape lastGenerated = null;
        private void toolStripButton6_Click(object sender, EventArgs e)
        {
            //GravityViewManager.View.myView.Items.Clear();
        }
        AIS_ViewController aIS_ViewController;
        V3d_Viewer v3d_viewer;
        AIS_InteractiveContext myAISContext;
        public void AddBox()
        {
            var d = DialogHelpers.StartDialog();
            d.Text = "New box";
            d.AddDouble("x", "x", 0);
            d.AddDouble("y", "y", 0);
            d.AddDouble("z", "z", 0);
            d.AddDouble("w", "Width", 50);
            d.AddDouble("l", "Length", 50);
            d.AddDouble("h", "Height", 50);

            if (!d.ShowDialog())
                return;

            var w = d.GetDouble("w");
            var h = d.GetDouble("h");
            var l = d.GetDouble("l");

            var x = d.GetDouble("x");
            var y = d.GetDouble("y");
            var z = d.GetDouble("z");
            gp_Pnt p1 = new gp_Pnt(x, y, z);
            gp_Pnt p2 = new gp_Pnt(x + w, y + h, z + l);
            BRepPrimAPI_MakeBox box = new BRepPrimAPI_MakeBox(p1, p2);

            box.Build();
            var solid = box.Solid();
            lastGenerated = solid;
            var shape = new AIS_Shape(solid);

            myAISContext.Display(shape, true);
            myAISContext.SetDisplayMode(shape, (int)AIS_DisplayMode.AIS_Shaded, false);
            myAISContext.UpdateCurrentViewer();
            shapes.Add(shape);



            //auto hn = GetHandle(*shape);
            //hh->FromObjHandle(hn);
        }

        List<AIS_Shape> shapes = new List<AIS_Shape>();
        private void toolStripButton7_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton8_Click(object sender, EventArgs e)
        {
            gp_Pnt p1 = new gp_Pnt(0, 0, 0);
            gp_Pnt p2 = new gp_Pnt(10, 10, 10);
            BRepPrimAPI_MakeBox box = new BRepPrimAPI_MakeBox(p1, p2);

            box.Build();
            var solid = box.Solid();
            var shape = new AIS_Shape(solid);

            myAISContext.Display(shape, true);
            myAISContext.SetDisplayMode(shape, (int)AIS_DisplayMode.AIS_Shaded, false);

            GravityViewManager.View.Redraw();
        }

        private void toolStripButton9_Click(object sender, EventArgs e)
        {
            GravityViewManager.View.MyViewer.StructureManager().SetDeviceLost();
            //GravityViewManager.View.Redraw();
            aIS_ViewController.FlushViewEvents(myAISContext, GravityViewManager.View, true);
        }

        private void toolStripButton10_Click(object sender, EventArgs e)
        {
            Vector2d[] points = { new Vector2d(0, 0), new Vector2d(50, 0), new Vector2d(50, 50), new Vector2d(0, 50) };
            var res = Tests.TriangulateTest1(points);
            Form form1 = new Form() { Width = 1000, Height = 800 };
            PictureBox pb = new PictureBox() { Dock = DockStyle.Fill };
            pb.Paint += (s, e) =>
            {
                var gr = e.Graphics;
                gr.Clear(Color.White);
                gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                while (res != null)
                {
                    List<PointF> pp = new List<PointF>();
                    for (int i = 0; i < res.v.Length; i++)
                    {
                        float scale = 500;
                        float offset = 400;

                        var p = new PointF((float)(res.v[i].x * scale + offset), (float)(res.v[i].y * scale + offset));

                        pp.Add(p);
                    }

                    gr.FillPolygon(Brushes.LightBlue,
                    pp.ToArray());
                    gr.DrawPolygon(Pens.Black,
                     pp.ToArray());
                    res = res.next;
                }
            };

            form1.Controls.Add(pb);
            form1.ShowDialog();

            //return;
            //gp_Pnt p1 = new gp_Pnt(0, 0, 0);
            //gp_Pnt p2 = new gp_Pnt(10, 10, 10);
            //BRepPrimAPI_MakeBox box = new BRepPrimAPI_MakeBox(p1, p2);

            //box.Build();
            //var solid = box.Solid();
            //var shape = new AIS_Shape(solid).Shape();
            //var sm = v3d_viewer.StructureManager();

            //for (TopExp_Explorer aExpFace = new TopExp_Explorer(shape, TopAbs_ShapeEnum.TopAbs_FACE); aExpFace.More(); aExpFace.Next())
            //{
            //    TopoDS_Face aFace = TopoDS.Face(aExpFace.Current());
            //    TopAbs_Orientation faceOrientation = aFace.Orientation();
            //    TopLoc_Location aLocation = new TopLoc_Location();
            //    Poly_Triangulation aTr = BRep_Tool.Triangulation(aFace, ref aLocation);
            //}
        }

        private void toolStripButton11_Click(object sender, EventArgs e)
        {
            TopExp_Explorer t = new TopExp_Explorer(lastGenerated, TopAbs_ShapeEnum.TopAbs_FACE);
            for (; t.More(); t.Next())
            {
                var face = t.Current() as TopoDS_Face;
                TopLoc_Location loc = new TopLoc_Location();
                var aTr = BRep_Tool.Triangulation(face, ref loc);
                if (aTr == null)
                    continue;

                var triangles = aTr.Triangles();
                var nodes = aTr.NbNodes();
                var nnn = aTr.NbTriangles();
            }

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void toolStripButton12_Click(object sender, EventArgs e)
        {



        }

        void MakeFace(Vector3d[] points)
        {
            // Define 4 points for the rectangle
            gp_Pnt[] pnts = points.Select(z => new gp_Pnt(z.X, z.Y, z.Z)).ToArray();
            // Create Wire (Closed Contour)
            BRepBuilderAPI_MakeWire mw = new BRepBuilderAPI_MakeWire();

            // Create Edges

            for (int i = 0; i < pnts.Length; i++)
            {
                var p1 = pnts[i % pnts.Length];
                var p2 = pnts[(i + 1) % pnts.Length];

                mw.Add(new BRepBuilderAPI_MakeEdge(p1, p2));

            }


            TopoDS_Wire wire = mw.Wire();

            // Create Face
            TopoDS_Face face = new BRepBuilderAPI_MakeFace(wire);

            var solid = face;
            var shape = new AIS_Shape(solid);

            myAISContext.Display(shape, true);
            myAISContext.SetDisplayMode(shape, (int)AIS_DisplayMode.AIS_Shaded, false);
            myAISContext.UpdateCurrentViewer();
        }
        void MakeRectFace(Vector3d v1, Vector3d v2, Vector3d v3, Vector3d v4)
        {
            // Define 4 points for the rectangle
            gp_Pnt p1 = new gp_Pnt(v1.X, v1.Y, v1.Z);
            gp_Pnt p2 = new gp_Pnt(v2.X, v2.Y, v2.Z);
            gp_Pnt p3 = new gp_Pnt(v3.X, v3.Y, v3.Z);
            gp_Pnt p4 = new gp_Pnt(v4.X, v4.Y, v4.Z);

            // Create Edges
            TopoDS_Edge e1 = new BRepBuilderAPI_MakeEdge(p1, p2);
            TopoDS_Edge e2 = new BRepBuilderAPI_MakeEdge(p2, p3);
            TopoDS_Edge e3 = new BRepBuilderAPI_MakeEdge(p3, p4);
            TopoDS_Edge e4 = new BRepBuilderAPI_MakeEdge(p4, p1);

            // Create Wire (Closed Contour)
            BRepBuilderAPI_MakeWire mw = new BRepBuilderAPI_MakeWire();
            mw.Add(e1);
            mw.Add(e2);
            mw.Add(e3);
            mw.Add(e4);
            TopoDS_Wire wire = mw.Wire();

            // Create Face
            TopoDS_Face face = new BRepBuilderAPI_MakeFace(wire);

            var solid = face;
            var shape = new AIS_Shape(solid);

            myAISContext.Display(shape, true);
            myAISContext.SetDisplayMode(shape, (int)AIS_DisplayMode.AIS_Shaded, false);
            myAISContext.UpdateCurrentViewer();
        }

        void MakeRectFaceWithHole(Vector3d[] outer, Vector3d[] hole)
        {
            var v1 = outer[0];
            var v2 = outer[1];
            var v3 = outer[2];
            var v4 = outer[3];

            // Define 4 points for the rectangle
            gp_Pnt p1 = new gp_Pnt(v1.X, v1.Y, v1.Z);
            gp_Pnt p2 = new gp_Pnt(v2.X, v2.Y, v2.Z);
            gp_Pnt p3 = new gp_Pnt(v3.X, v3.Y, v3.Z);
            gp_Pnt p4 = new gp_Pnt(v4.X, v4.Y, v4.Z);

            // Create Edges
            TopoDS_Edge e1 = new BRepBuilderAPI_MakeEdge(p1, p2);
            TopoDS_Edge e2 = new BRepBuilderAPI_MakeEdge(p2, p3);
            TopoDS_Edge e3 = new BRepBuilderAPI_MakeEdge(p3, p4);
            TopoDS_Edge e4 = new BRepBuilderAPI_MakeEdge(p4, p1);

            // Create Wire (Closed Contour)
            BRepBuilderAPI_MakeWire mw = new BRepBuilderAPI_MakeWire();
            mw.Add(e1);
            mw.Add(e2);
            mw.Add(e3);
            mw.Add(e4);
            TopoDS_Wire wire = mw.Wire();



            // Define 4 points for the rectangle
            gp_Pnt ip1 = new gp_Pnt(hole[0].X, hole[0].Y, hole[0].Z);
            gp_Pnt ip2 = new gp_Pnt(hole[1].X, hole[1].Y, hole[1].Z);
            gp_Pnt ip3 = new gp_Pnt(hole[2].X, hole[2].Y, hole[2].Z);
            gp_Pnt ip4 = new gp_Pnt(hole[3].X, hole[3].Y, hole[3].Z);

            // Create Edges
            TopoDS_Edge ie1 = new BRepBuilderAPI_MakeEdge(ip1, ip4);
            TopoDS_Edge ie2 = new BRepBuilderAPI_MakeEdge(ip4, ip3);
            TopoDS_Edge ie3 = new BRepBuilderAPI_MakeEdge(ip3, ip2);
            TopoDS_Edge ie4 = new BRepBuilderAPI_MakeEdge(ip2, ip1);

            // Create Wire (Closed Contour)
            BRepBuilderAPI_MakeWire imw = new BRepBuilderAPI_MakeWire();
            imw.Add(ie1);
            imw.Add(ie2);
            imw.Add(ie3);
            imw.Add(ie4);

            TopoDS_Wire iwire = imw.Wire();

            // Create Face
            BRepBuilderAPI_MakeFace faceMaker = new BRepBuilderAPI_MakeFace(wire);
            faceMaker.Add(iwire);

            var solid = faceMaker.Face();
            var shape = new AIS_Shape(solid);

            myAISContext.Display(shape, true);
            myAISContext.SetDisplayMode(shape, (int)AIS_DisplayMode.AIS_Shaded, false);
            myAISContext.UpdateCurrentViewer();
        }

        private void toolStripButton13_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton14_Click(object sender, EventArgs e)
        {
            GravityViewManager?.View?.FitAll();
            GravityViewManager?.View?.ZFitAll();
        }

        private void toolStripButton15_Click(object sender, EventArgs e)
        {
            // Clears all objects from the context and releases presentation memory
            myAISContext.RemoveAll(true);
            shapes.Clear();
        }

        private void toolStripButton16_Click(object sender, EventArgs e)
        {

        }

        private void toolStripButton17_Click(object sender, EventArgs e)
        {
            // Perform the Boolean Fuse (Union)
            BRepAlgoAPI_Fuse fuseAlg = new(shapes[0].Shape(), shapes[1].Shape());
            fuseAlg.Build();

            // Retrieve the resulting shape
            TopoDS_Shape fusedResult = fuseAlg.Shape();
            var shape = new AIS_Shape(fusedResult);

            myAISContext.Display(shape, true);
            myAISContext.SetDisplayMode(shape, (int)AIS_DisplayMode.AIS_Shaded, false);
            myAISContext.UpdateCurrentViewer();

        }

        private void toolStripButton18_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddDouble("radius", "Radius", 10, 1, 1000);



            if (!d.ShowDialog())
                return;

            var radius = d.GetDouble("radius");


            var sw = Stopwatch.StartNew();


            BRepPrimAPI_MakeSphere mkCylinder = new BRepPrimAPI_MakeSphere(radius);

            // Get the resulting TopoDS_Shape
            TopoDS_Shape myCylinder = mkCylinder.Shape();
            var shape = new AIS_Shape(myCylinder);

            myAISContext.Display(shape, true);
            myAISContext.SetDisplayMode(shape, (int)AIS_DisplayMode.AIS_Shaded, false);
            myAISContext.UpdateCurrentViewer();

            sw.Stop();
            var ms = sw.ElapsedMilliseconds;
            toolStripStatusLabel1.Text = $"time: {ms}ms";
        }

        private void toolStripButton19_Click(object sender, EventArgs e)
        {
            var sw = Stopwatch.StartNew();
            AddBox();
            sw.Stop();
            var ms = sw.ElapsedMilliseconds;
            toolStripStatusLabel1.Text = $"time: {ms}ms";
        }

        private void toolStripButton7_Click_1(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddDouble("radius", "Radius", 10, 1, 1000);
            d.AddDouble("h", "Height", 50, 1, 1000);


            if (!d.ShowDialog())
                return;

            var radius = d.GetDouble("radius");
            var h = d.GetDouble("h");

            var sw = Stopwatch.StartNew();

            // Create the cylinder
            BRepPrimAPI_MakeCylinder mkCylinder = new BRepPrimAPI_MakeCylinder(radius, h);

            // Get the resulting TopoDS_Shape
            TopoDS_Shape myCylinder = mkCylinder.Shape();
            var shape = new AIS_Shape(myCylinder);

            myAISContext.Display(shape, true);
            myAISContext.SetDisplayMode(shape, (int)AIS_DisplayMode.AIS_Shaded, false);
            myAISContext.UpdateCurrentViewer();


            sw.Stop();
            var ms = sw.ElapsedMilliseconds;
            toolStripStatusLabel1.Text = $"time: {ms}ms";
        }

        private void toolStripButton20_Click(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddDouble("radius", "Radius", 10, 1, 1000);



            if (!d.ShowDialog())
                return;

            var radius = d.GetDouble("radius");


            var sw = Stopwatch.StartNew();

            gp_Pnt center = new(0.0, 0.0, 0.0);
            gp_Dir normal = new(0.0, 0.0, 1.0);
            gp_Circ circle = new(new gp_Ax2(center, normal), radius);

            // Convert geometric circle to topological edge and wire
            TopoDS_Edge edge = new BRepBuilderAPI_MakeEdge(circle);
            TopoDS_Wire wire = new BRepBuilderAPI_MakeWire(edge);

            // Generate the planar circular face
            TopoDS_Face face = new BRepBuilderAPI_MakeFace(wire, true);
            var shape = new AIS_Shape(face);
            myAISContext.Display(shape, true);
            myAISContext.SetDisplayMode(shape, (int)AIS_DisplayMode.AIS_Shaded, false);
            myAISContext.UpdateCurrentViewer();

            sw.Stop();
            var ms = sw.ElapsedMilliseconds;
            toolStripStatusLabel1.Text = $"time: {ms}ms";
        }

        private void toolStripButton8_Click_1(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();

            d.AddInt("qty", "Qty", 8, 3, 60);

            d.AddDouble("rad1", "Radius 1", 100, 1, 1000);
            d.AddDouble("rad2", "Radius 2", 50, 1, 1000);

            if (!d.ShowDialog())
                return;

            var rad1 = d.GetDouble("rad1");
            var rad2 = d.GetDouble("rad2");
            var qty = d.GetInt("qty");


            List<Vector3d> pp = new List<Vector3d>();
            int q = qty * 2;
            for (int i = 0; i < q; i++)
            {
                var ang = i * 360.0 / q;
                var radians = ang * Math.PI / 180.0;
                var rad = rad1;
                if (i % 2 == 0)
                    rad = rad2;
                var xx = rad * Math.Cos(radians);
                var yy = rad * Math.Sin(radians);
                pp.Add(new Vector3d(xx, yy, 0));
            }

            MakeFace(pp.ToArray());
        }

        private void toolStripButton13_Click_1(object sender, EventArgs e)
        {
            var d = AutoDialog.DialogHelpers.StartDialog();
            d.AddDouble("cx", "X");
            d.AddDouble("cy", "Y");
            d.AddDouble("cz", "Z");
            d.AddBoolField("hole", "hole");


            d.AddDouble("w", "Width", 100);
            d.AddDouble("h", "Height", 50);

            if (!d.ShowDialog())
                return;

            var cx = d.GetDouble("cx");
            var cy = d.GetDouble("cy");
            var cz = d.GetDouble("cz");
            var w = d.GetDouble("w");
            var h = d.GetDouble("h");
            var withHole = d.GetBoolField("hole");

            double area = w * h;
            if (Math.Abs(area) < (0.0))
            {
                MessageBox.Show("zero area. operation incorrect", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var center = new Vector3d(cx, cy, cz);
            if (withHole)
            {
                /*
                 * gp_Pnt ip1(-30, -10, 0);
		gp_Pnt ip2(30, -10, 0);
		gp_Pnt ip3(30, 10, 0);
		gp_Pnt ip4(-30, 10, 0);

                 */
                MakeRectFaceWithHole([center + new Vector3d(-w / 2, -h / 2, 0),
                         center + new Vector3d(w / 2, -h / 2, 0),
                         center + new Vector3d(w / 2, h / 2, 0),
                         center + new Vector3d(-w / 2, h / 2, 0)], [new Vector3d(-30,-10,0),
                         new Vector3d(30,-10,0),
                         new Vector3d(30,10,0),
                         new Vector3d(-30,10,0)
                         ]);
            }
            else
            {

                MakeRectFace(center + new Vector3d(-w / 2, -h / 2, 0),
                         center + new Vector3d(w / 2, -h / 2, 0),
                         center + new Vector3d(w / 2, h / 2, 0),
                         center + new Vector3d(-w / 2, h / 2, 0));

            }
        }

        private void toolStripButton12_Click_1(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "step files (*.stp)|*.stp";
            if (ofd.ShowDialog() != DialogResult.OK)
                return;

            STEPControl_Reader reader = new STEPControl_Reader();
            IFSelect_ReturnStatus stat = reader.ReadFile(ofd.FileName);
            IFSelect_PrintCount mode = IFSelect_PrintCount.IFSelect_ListByItem;
            reader.PrintCheckLoad(false, mode);
            
            int NbTrans = reader.TransferRoots();
            
            TopoDS_Shape shape1 = reader.Shape();

            var shape = new AIS_Shape(shape1);

            myAISContext.Display(shape, true);
            myAISContext.SetDisplayMode(shape, (int)AIS_DisplayMode.AIS_Shaded, false);
            myAISContext.UpdateCurrentViewer();
        }
    }
}
