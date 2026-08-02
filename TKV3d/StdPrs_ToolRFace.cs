using OCCPort;
using OCCPort.Common;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using TKBRep;
using TKG2d;
using TKG3d;

namespace TKV3d
{
    //! Iterator over 2D curves restricting a face (skipping internal/external edges).
    //! In addition, the algorithm skips NULL curves - IsInvalidGeometry() can be checked if this should be handled within algorithm.
    public class StdPrs_ToolRFace
    {
        public StdPrs_ToolRFace(BRepAdaptor_Surface theSurface)
        {
            myFace = (theSurface.Face());
            myHasNullCurves = (false);
            myFace.Orientation(TKG3d.TopAbs_Orientation.TopAbs_FORWARD);
        }


        //! Return TRUE indicating that iterator looks only for oriented edges.
        public bool IsOriented() { return true; }
  
        //! Return current curve.
        public Adaptor2d_Curve2d Value() { return myCurve; }

        //! Return current edge orientation.
        public TopAbs_Orientation Orientation() { return myExplorer.Current().Orientation(); }
        //! Move iterator to the first element.
        public void Init()
        {
            myExplorer.Init(myFace, TKG3d.TopAbs_ShapeEnum.TopAbs_EDGE);
            next();
        }

        //! Return TRUE if iterator points to the curve.
        public bool More() { return myExplorer.More(); }

        //! Go to the next curve in the face.
        public void Next()
        {
            myExplorer.Next();
            next();
        }
        void next()
        {
            double aParamU1 = 0, aParamU2 = 0;
            for (; myExplorer.More(); myExplorer.Next())
            {
                // skip INTERNAL and EXTERNAL edges
                if (myExplorer.Current().Orientation() != TKG3d.TopAbs_Orientation.TopAbs_FORWARD
                 && myExplorer.Current().Orientation() != TKG3d.TopAbs_Orientation.TopAbs_REVERSED)
                {
                    continue;
                }
                Geom2d_Curve aCurve = BRep_Tool.CurveOnSurface(TopoDS.Edge(myExplorer.Current()), myFace, ref aParamU1, ref aParamU2);
                if (aCurve != null)
                {
                    myCurve.Load(aCurve, aParamU1, aParamU2);
                    return;
                }
                else
                {
                    myHasNullCurves = true;
                }
            }

            myCurve.Reset();
        }

        TopoDS_Face myFace;
        TopExp_Explorer myExplorer = new TopExp_Explorer();
        Geom2dAdaptor_Curve myCurve = new Geom2dAdaptor_Curve();
        bool myHasNullCurves;
    }
}

