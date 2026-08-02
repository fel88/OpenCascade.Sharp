using OCCPort.OpenGL;
using System;
using System.Windows.Forms;
using TKernel;
using TKMath;
using TKOpenGl;
using TKService;
using TKV3d;


namespace OCCPort.Tester
{
    public class OCCTProxy : IOCCTProxyInterface
    {
        public  void iterate()
        {
            gview.iterate();
        }
        bool AutoViewerUpdate;
        public V3d_Viewer myViewer;
        public V3d_View myView;
        AIS_InteractiveContext myAISContext;
        OpenGl_GraphicDriver myGraphicDriver;
        public OCCImpl impl = new OCCImpl();
        public GlfwOcctView gview = new GlfwOcctView();
        public void ActivateGrid(bool en)
        {
            if (en)
                myViewer.ActivateGrid(Aspect_GridType.Aspect_GT_Rectangular, Aspect_GridDrawMode. Aspect_GDM_Lines);
            else
                myViewer.DeactivateGrid();
        }
        public void UpdateCurrentViewer()
        {
            if (myAISContext != null)
            {
                myAISContext.UpdateCurrentViewer();
            }
        }
        public void initDemoScene()
        {
            if (myAISContext == null)
            {
                return;
            }

            /*myView()->TriedronDisplay(Aspect_TOTP_LEFT_LOWER, Quantity_NOC_GOLD, 0.08, V3d_WIREFRAME);

            gp_Ax2 anAxis;
            anAxis.SetLocation(new gp_Pnt(0.0, 0.0, 0.0));
            Handle(AIS_Shape) aBox = new AIS_Shape(BRepPrimAPI_MakeBox(anAxis, 50, 50, 50).Shape());
            myAISContext()->Display(aBox, AIS_Shaded, 0, false);
            anAxis.SetLocation(gp_Pnt(25.0, 125.0, 0.0));
            Handle(AIS_Shape) aCone = new AIS_Shape(BRepPrimAPI_MakeCone(anAxis, 25, 0, 50).Shape());
            myAISContext()->Display(aCone, AIS_Shaded, 0, false);*/

            string aGlInfo;
            {
                //   TColStd_IndexedDataMapOfStringString aRendInfo;
                //  myView()->DiagnosticInformation(aRendInfo, Graphic3d_DiagnosticInfo_Basic);
                // for (TColStd_IndexedDataMapOfStringString::Iterator aValueIter(aRendInfo); aValueIter.More(); aValueIter.Next())
                {
                    //    if (!aGlInfo.IsEmpty()) { aGlInfo += "\n"; }
                    //    aGlInfo += TCollection_AsciiString("  ") + aValueIter.Key() + ": " + aValueIter.Value();
                }
            }
            //Message::DefaultMessenger()->Send(TCollection_AsciiString("OpenGL info:\n") + aGlInfo, Message_Info);
        }

        public unsafe void runOpenTk(nint wnd, nint glctx)
        {
            gview.initWindow(800, 600, wnd, "OCCT IMGUI");

            gview.initViewer(glctx);

            initDemoScene();
            myView = gview.myView;
            myViewer = gview.myViewer;
            myAISContext = gview.myContext;
            impl.setAisCtx(myAISContext);
            SetDefaultDrawerParams();
            SetDefaultGradient();

            if (myView == null)
                return;


            myView.MustBeResized();
            //myOcctWindow->Map();
            //initGui();
            //mainloop();
            //cleanup();
        }

        private void SetDefaultDrawerParams()
        {


            var ais = myAISContext;
            var drawer = ais.DefaultDrawer();
            //drawer.SetFaceBoundaryDraw(true);
            drawer.SetColor(new Quantity_Color(Quantity_NameOfColor.Quantity_NOC_BLACK));
            myAISContext.EnableDrawHiddenLine();

            Graphic3d_RenderingParams aParams = myView.ChangeRenderingParams();
            //aParams.RenderResolutionScale = 2;
            aParams.IsShadowEnabled = false;
            // enable specular reflections
            aParams.IsReflectionEnabled = false;
            // enable adaptive anti-aliasing
            aParams.IsAntialiasingEnabled = false;

        }



        private void SetDefaultGradient()
        {
            myView.SetBgGradientColors(
            new Quantity_Color(0xAD / (float)0xFF - 0.2f, 0xD8 / (float)0xFF - 0.2f, 0xE6 / (float)0xFF, Quantity_TypeOfColor.Quantity_TOC_RGB),            
  new Quantity_Color(0xF0 / (float)0xFF - 0.3f, 0xF8 / (float)0xFF - 0.3f, 0xFF / (float)0xFF - 0.3f, Quantity_TypeOfColor.Quantity_TOC_RGB),
          Aspect_GradientFillMethod.Aspect_GFM_DIAG2);
        }

        public void RedrawView()
        {
            if (myView!= null)
            {
                myView.Redraw();
            }
        }

        public void SetDisplayMode(int theMode)
        {
            if (myAISContext==null)
            {

                return;
            }
            AIS_DisplayMode aCurrentMode;
            if (theMode == 0)
            {
                aCurrentMode = AIS_DisplayMode.AIS_WireFrame;
            }
            else
            {
                aCurrentMode =AIS_DisplayMode. AIS_Shaded;
            }

            if (myAISContext.NbSelected() == 0)
            {
                myAISContext.SetDisplayMode((int)aCurrentMode, false);
            }
            else
            {
                for (myAISContext.InitSelected(); myAISContext.MoreSelected(); myAISContext.NextSelected())
                {
                    myAISContext.SetDisplayMode(myAISContext.SelectedInteractive(), theMode, false);
                }
            }
            myAISContext.UpdateCurrentViewer();
        }

        public void SetMaterial(int theMaterial)
        {
            if (myAISContext==null)
            {
                return;
            }
            for (myAISContext.InitSelected(); myAISContext.MoreSelected(); myAISContext.NextSelected())
            {
                myAISContext.SetMaterial(myAISContext.SelectedInteractive(), new Graphic3d_MaterialAspect( (Graphic3d_NameOfMaterial)theMaterial), false);
            }
            myAISContext.UpdateCurrentViewer();
        }

        public void UpdateView()
        {
            if (myView!=null)
            {
                myView.MustBeResized();
            }
        }
    }
}
