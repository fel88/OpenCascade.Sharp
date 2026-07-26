global using ShapeProcess_OperFunc = System.Func<TKShHealing.ShapeProcess_Context, TKernel.Message_ProgressRange, bool>;
using OCCPort;
using System.Reflection.Metadata;
using TKBRep;
using TKernel;
using TKG2d;
using TKG3d;
using TKMath;

namespace TKShHealing
{
    //! Tool for analyzing the edge.
    //! Queries geometrical representations of the edge (3d curve, pcurve
    //! on the given face or surface) and topological sub-shapes (bounding
    //! vertices).
    //! Provides methods for analyzing geometry and topology consistency
    //! (3d and pcurve(s) consistency, their adjacency to the vertices).
    public class ShapeAnalysis_Edge
    {
        //! Returns start vertex of the edge (taking edge orientation
        //! into account).
        public TopoDS_Vertex FirstVertex(TopoDS_Edge edge)
        {
            TopoDS_Vertex V;
            if (edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED)
            {
                V = TopExp.LastVertex(edge);
                V.Reverse();
            }
            else
            {
                V = TopExp.FirstVertex(edge);
            }
            return V;
        }

        public bool Curve3d(TopoDS_Edge edge,
                        out Geom_Curve C3d,
                          ref double cf, ref double cl,
                           bool orient)
        {
            TopLoc_Location L;
            C3d = BRep_Tool.Curve(edge, out L, out cf, out cl);
            if (C3d != null && !L.IsIdentity())
            {
                C3d = (Geom_Curve)C3d.Transformed(L.Transformation());
                cf = C3d.TransformedParameter(cf, L.Transformation());
                cl = C3d.TransformedParameter(cl, L.Transformation());
            }
            if (orient)
            {
                if (edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED)
                { double tmp = cf; cf = cl; cl = tmp; }
            }
            return C3d != null;
        }

        public bool PCurve(TopoDS_Edge edge,
                         TopoDS_Face face,
                       ref Geom2d_Curve C2d,
                         ref double cf, ref double cl,
                         bool orient = true)
        {
            //:abv 20.05.02: take into account face orientation
            // COMMENTED BACK - NEEDS MORE CHANGES IN ALL SHAPEHEALING
            //   C2d = BRep_Tool::CurveOnSurface (edge, face, cf, cl);
            //   if (orient && edge.Orientation() == TopAbs_REVERSED) {
            //     Standard_Real tmp = cf; cf = cl; cl = tmp;
            //   }
            //   return !C2d.IsNull();
            TopLoc_Location L;
            Geom_Surface S = BRep_Tool.Surface(face, out L);
            return PCurve(edge, S, L, ref C2d, ref cf, ref cl, orient);
        }
        //! Returns the pcurve and bounding parameteres for the edge
        //! lying on the surface.
        //! Returns False if the edge has no pcurve on this surface.
        //! If <orient> is True (default), takes orientation into account:
        //! if the edge is reversed, cf and cl are toggled
        public bool PCurve(TopoDS_Edge edge,
                         Geom_Surface surface,
                         TopLoc_Location location,
                       ref Geom2d_Curve C2d,
                         ref double cf, ref double cl,
                         bool orient = true)
        {
            C2d = BRep_Tool.CurveOnSurface(edge, surface, location, ref cf, ref cl);
            if (orient && edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED)
            {
                double tmp = cf; cf = cl; cl = tmp;
            }
            return C2d != null;
        }

        //! Returns end vertex of the edge (taking edge orientation
        //! into account).
        public TopoDS_Vertex LastVertex(TopoDS_Edge edge)
        {
            TopoDS_Vertex V;
            if (edge.Orientation() == TopAbs_Orientation.TopAbs_REVERSED)
            {
                V = TopExp.FirstVertex(edge);
                V.Reverse();
            }
            else
            {
                V = TopExp.LastVertex(edge);
            }
            return V;
        }
    }


    //! Provides a set of following operators
    //!
    //! DirectFaces
    //! FixShape
    //! SameParameter
    //! SetTolerance
    //! SplitAngle
    //! BSplineRestriction
    //! ElementaryToRevolution
    //! SurfaceToBSpline
    //! ToBezier
    //! SplitContinuity
    //! SplitClosedFaces
    //! FixWireGaps
    //! FixFaceSize
    //! DropSmallEdges
    //! FixShape
    //! SplitClosedEdges
    public class ShapeProcess_OperLibrary
    {

        static bool directfaces(ShapeProcess_Context context,
                                      Message_ProgressRange _)
        {
            ShapeProcess_ShapeContext ctx = (ShapeProcess_ShapeContext)(context);
            if (ctx == null) return false;

            // activate message mechanism if it is supported by context
            //ShapeExtend_MsgRegistrator msg = null;
            //  if (!ctx->Messages().IsNull()) msg = new ShapeExtend_MsgRegistrator();

            //  ShapeCustom_DirectModification DM = new ShapeCustom_DirectModification();
            //  DM.SetMsgRegistrator(msg);
            ////  TopTools_DataMapOfShapeShape map;
            //TopoDS_Shape res = ShapeProcess_OperLibrary.ApplyModifier(ctx->Result(), ctx, DM, map, msg, Standard_True);
            //   ctx.RecordModification(map, msg);
            //   ctx.SetResult(res);
            return true;
        }

        static bool done = false;

        public static void Init()
        {
            if (done) return;
            done = true;

            ShapeExtend.Init();

            ShapeProcess.RegisterOperator("DirectFaces", new ShapeProcess_UOperator(directfaces));
            //ShapeProcess.RegisterOperator("SameParameter", new ShapeProcess_UOperator(sameparam));
            //ShapeProcess.RegisterOperator("SetTolerance", new ShapeProcess_UOperator(settol));
            //ShapeProcess.RegisterOperator("SplitAngle", new ShapeProcess_UOperator(splitangle));
            //ShapeProcess.RegisterOperator("BSplineRestriction", new ShapeProcess_UOperator(bsplinerestriction));
            //ShapeProcess.RegisterOperator("ElementaryToRevolution", new ShapeProcess_UOperator(torevol));
            //ShapeProcess.RegisterOperator("SweptToElementary", new ShapeProcess_UOperator(swepttoelem));
            //ShapeProcess.RegisterOperator("SurfaceToBSpline", new ShapeProcess_UOperator(converttobspline));
            //ShapeProcess.RegisterOperator("ToBezier", new ShapeProcess_UOperator(shapetobezier));
            //ShapeProcess.RegisterOperator("SplitContinuity", new ShapeProcess_UOperator(splitcontinuity));
            //ShapeProcess.RegisterOperator("SplitClosedFaces", new ShapeProcess_UOperator(splitclosedfaces));
            //ShapeProcess.RegisterOperator("FixWireGaps", new ShapeProcess_UOperator(fixwgaps));
            //ShapeProcess.RegisterOperator("FixFaceSize", new ShapeProcess_UOperator(fixfacesize));
            //ShapeProcess.RegisterOperator("DropSmallSolids", new ShapeProcess_UOperator(dropsmallsolids));
            //ShapeProcess.RegisterOperator("DropSmallEdges", new ShapeProcess_UOperator(mergesmalledges));
            //ShapeProcess.RegisterOperator("FixShape", new ShapeProcess_UOperator(fixshape));
            //ShapeProcess.RegisterOperator("SplitClosedEdges", new ShapeProcess_UOperator(spltclosededges));
            //ShapeProcess.RegisterOperator("SplitCommonVertex", new ShapeProcess_UOperator(splitcommonvertex));
        }

    }


    //! Shape Processing module
    //! allows to define and apply general Shape Processing as a
    //! customizable sequence of Shape Healing operators. The
    //! customization is implemented via user-editable resource
    //! file which defines sequence of operators to be executed
    //! and their parameters.
    public class ShapeProcess
    {
        static NCollection_DataMap<string, ShapeProcess_Operator> aMapOfOperators = new NCollection_DataMap<string, ShapeProcess_Operator>();
        public static bool RegisterOperator(string name,
                                                     ShapeProcess_Operator op)
        {
            if (aMapOfOperators.IsBound(name))
            {

                return false;
            }
            aMapOfOperators.Bind(name, op);
            return true;
        }
    }


    //! Abstract Operator class providing a tool to
    //! perform an operation on Context
    public class ShapeProcess_Operator
    {
    }


    //! Defines operator as container for static function
    //! OperFunc. This allows user to create new operators
    //! without creation of new classes
    public class ShapeProcess_UOperator : ShapeProcess_Operator
    {
        //! Creates operator with implementation defined as
        //! OperFunc (static function)
        public ShapeProcess_UOperator(ShapeProcess_OperFunc func)
        {
            myFunc = (func);
        }
        ShapeProcess_OperFunc myFunc;
    }




    //! Provides convenient interface to resource file
    //! Allows to load resource file and get values of
    //! attributes starting from some scope, for example
    //! if scope is defined as "ToV4" and requested parameter
    //! is "exec.op", value of "ToV4.exec.op" parameter from
    //! the resource file will be returned
    public class ShapeProcess_Context
    {
    }


    //! Extends Context to handle shapes
    //! Contains map of shape-shape, and messages
    //! attached to shapes
    public class ShapeProcess_ShapeContext : ShapeProcess_Context
    {
    }


 public    class ShapeAlgo_AlgoContainer 
{
        }
}

