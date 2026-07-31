using OCCPort;
using OCCPort.Common;
using TKBRep;
using TKernel;
using TKG2d;
using TKG3d;
using TKGeomBase;
using TKMath;

namespace TKMesh
{
    //! Auxiliary class performing tessellation of passed edge according to specified parameters.
    public class BRepMesh_CurveTessellator : IMeshTools_CurveTessellator
    {
        public BRepMesh_CurveTessellator(
  IMeshData_Edge theEdge,
  IMeshTools_Parameters theParameters,
  int theMinPointsNb)
        {

            myDEdge = (theEdge);
            myParameters = (theParameters);
            myEdge = (theEdge.GetEdge());
            myCurve = new BRepAdaptor_Curve(myEdge);//??
            myMinPointsNb = (theMinPointsNb);
            init();
        }

        public BRepMesh_CurveTessellator(
  IMeshData_Edge theEdge,
  TopAbs_Orientation theOrientation,
  IMeshData_Face theFace,
  IMeshTools_Parameters theParameters,
  int theMinPointsNb)

        {
            myDEdge = (theEdge);
            myParameters = (theParameters);
            myEdge = (TopoDS.Edge(theEdge.GetEdge().Oriented(theOrientation)));
            myCurve = new BRepAdaptor_Curve(myEdge, theFace.GetFace());
            myMinPointsNb = theMinPointsNb;
            init();
        }
        void init()
        {
            if (myParameters.MinSize <= 0.0)
            {
                Standard_Failure.Raise("The structure \"myParameters\" is not initialized");
            }

            TopExp.Vertices(myEdge, ref myFirstVertex, ref myLastVertex);

            double aPreciseAngDef = 0.5 * myDEdge.GetAngularDeflection();
            double aPreciseLinDef = 0.5 * myDEdge.GetDeflection();
            if (myEdge.Orientation() == TopAbs_Orientation.TopAbs_INTERNAL)
            {
                aPreciseLinDef *= 0.5;
            }

            aPreciseLinDef = Math.Max(aPreciseLinDef, Precision.Confusion());
            aPreciseAngDef = Math.Max(aPreciseAngDef, Precision.Angular());

            double aMinSize = myParameters.MinSize;
            if (myParameters.AdjustMinSize)
            {
                aMinSize = Math.Min(aMinSize, IMeshTools_Parameters.RelMinSize() * GCPnts_AbscissaPoint.Length(
                  myCurve, myCurve.FirstParameter(), myCurve.LastParameter(), aPreciseLinDef));
            }

            mySquareEdgeDef = aPreciseLinDef * aPreciseLinDef;
            mySquareMinSize = Math.Max(mySquareEdgeDef, aMinSize * aMinSize);

            myEdgeSqTol = BRep_Tool.Tolerance(myEdge);
            myEdgeSqTol *= myEdgeSqTol;

            int aMinPntNb = Math.Max(myMinPointsNb,
              (myCurve._GetType() == GeomAbs_CurveType.GeomAbs_Circle) ? 4 : 2); //OCC287

            myDiscretTool.Initialize(myCurve,
                                      myCurve.FirstParameter(), myCurve.LastParameter(),
                                      aPreciseAngDef, aPreciseLinDef, aMinPntNb,
                                      Precision.PConfusion(), aMinSize);

            if (myCurve.IsCurveOnSurface())
            {
                Adaptor3d_CurveOnSurface aCurve = myCurve.CurveOnSurface();
                Adaptor3d_Surface aSurface = aCurve.GetSurface();

                double aTol = Precision.Confusion();
                double aDu = aSurface.UResolution(aTol);
                double aDv = aSurface.VResolution(aTol);

                myFaceRangeU[0] = aSurface.FirstUParameter() - aDu;
                myFaceRangeU[1] = aSurface.LastUParameter() + aDu;

                myFaceRangeV[0] = aSurface.FirstVParameter() - aDv;
                myFaceRangeV[1] = aSurface.LastVParameter() + aDv;
            }

            addInternalVertices();
            splitByDeflection2d();
        }

        private void splitByDeflection2d()
        {
            int aNodesNb = myDiscretTool.NbPoints();
            if (!myDEdge.IsFree() &&
                myDEdge.GetSameParam() &&
                myDEdge.GetSameRange() &&
                aNodesNb > 1)
            {
                for (int aPCurveIt = 0; aPCurveIt < myDEdge.PCurvesNb(); ++aPCurveIt)
                {
                    TopLoc_Location aLoc;
                    var aPCurve = myDEdge.GetPCurve(aPCurveIt);
                    TopoDS_Face aFace = aPCurve.GetFace().GetFace();
                    Geom_Surface aSurface = BRep_Tool.Surface(aFace, out aLoc);
                    if (aSurface is Geom_Plane)
                    {
                        continue;
                    }

                    TopoDS_Edge aCurrEdge = TopoDS.Edge(myEdge.Oriented(aPCurve.GetOrientation()));

                    double aF = 0, aL = 0;
                    Geom2d_Curve aCurve2d = BRep_Tool.CurveOnSurface(aCurrEdge, aFace, ref aF, ref aL);
                    TColStd_Array1OfReal aParamArray = new TColStd_Array1OfReal(1, aNodesNb);
                    for (int i = 1; i <= aNodesNb; ++i)
                        aParamArray.SetValue(i, myDiscretTool.Parameter(i));

                    for (int i = 1; i < aNodesNb; ++i)
                        splitSegment(aSurface, aCurve2d, aParamArray[i], aParamArray[i + 1], 1);
                }
            }
        }
        public void splitSegment(
  Geom_Surface theSurf,
  Geom2d_Curve theCurve2d,
  double theFirst,
  double theLast,
  int theNbIter)
        {
            // limit iteration depth
            if (theNbIter > 10)
            {
                return;
            }

            gp_Pnt2d uvf = new gp_Pnt2d(), uvl = new gp_Pnt2d(), uvm;
            gp_Pnt P3dF, P3dL, midP3d = new gp_Pnt(), midP3dFromSurf;
            double midpar;

            if (Math.Abs(theLast - theFirst) < 2 * Precision.PConfusion())
            {
                return;
            }

            if ((theCurve2d.FirstParameter() - theFirst > Precision.PConfusion()) ||
                (theLast - theCurve2d.LastParameter() > Precision.PConfusion()))
            {
                // E.g. test bugs moddata_3 bug30133
                return;
            }

            theCurve2d.D0(theFirst, out uvf);
            theCurve2d.D0(theLast, out uvl);

            P3dF = theSurf.Value(uvf.X(), uvf.Y());
            P3dL = theSurf.Value(uvl.X(), uvl.Y());

            if (P3dF.SquareDistance(P3dL) < mySquareMinSize)
            {
                return;
            }

            uvm = new gp_Pnt2d((uvf.XY() + uvl.XY()) * 0.5);
            midP3dFromSurf = theSurf.Value(uvm.X(), uvm.Y());

            gp_XYZ Vec1 = midP3dFromSurf.XYZ() - P3dF.XYZ();
            if (Vec1.SquareModulus() < mySquareMinSize)
            {
                return;
            }

            gp_XYZ aVec = P3dL.XYZ() - P3dF.XYZ();
            aVec.Normalize();

            double aModulus = Vec1.Dot(aVec);
            gp_XYZ aProj = aVec * aModulus;
            gp_XYZ aDist = Vec1 - aProj;

            if (aDist.SquareModulus() < mySquareEdgeDef)
            {
                return;
            }

            midpar = (theFirst + theLast) * 0.5;
            myCurve.D0(midpar, ref midP3d);
            myDiscretTool.AddPoint(midP3d, midpar, false);

            splitSegment(theSurf, theCurve2d, theFirst, midpar, theNbIter + 1);
            splitSegment(theSurf, theCurve2d, midpar, theLast, theNbIter + 1);
        }

        private void addInternalVertices()
        {
            // PTv, chl/922/G9, Take into account internal vertices
            // it is necessary for internal edges, which do not split other edges, by their vertex
            TopExp_Explorer aVertexIt = new TopExp_Explorer(myEdge, TopAbs_ShapeEnum.TopAbs_VERTEX);
            for (; aVertexIt.More(); aVertexIt.Next())
            {
                TopoDS_Vertex aVertex = TopoDS.Vertex(aVertexIt.Current());
                if (aVertex.Orientation() != TopAbs_Orientation.TopAbs_INTERNAL)
                {
                    continue;
                }

                myDiscretTool.AddPoint(BRep_Tool.Pnt(aVertex),
                  BRep_Tool.Parameter(aVertex, myEdge), true);
            }
        }

        public int PointsNb()
        {
            return myDiscretTool.NbPoints();
        }

        public bool Value(int theIndex, out gp_Pnt thePoint, out double theParameter)
        {
            thePoint = myDiscretTool.Value(theIndex);
            theParameter = myDiscretTool.Parameter(theIndex);

            /*if (!isInToleranceOfVertex(thePoint, myFirstVertex) &&
                !isInToleranceOfVertex(thePoint, myLastVertex))
            {*/
            if (!myCurve.IsCurveOnSurface())
            {
                return true;
            }

            // If point coordinates are out of surface range, 
            // it is necessary to re-project point.
            Adaptor3d_CurveOnSurface aCurve = myCurve.CurveOnSurface();
            Adaptor3d_Surface aSurface = aCurve.GetSurface();
            if (aSurface._GetType() != GeomAbs_SurfaceType.GeomAbs_BSplineSurface &&
                aSurface._GetType() != GeomAbs_SurfaceType.GeomAbs_BezierSurface &&
                aSurface._GetType() != GeomAbs_SurfaceType.GeomAbs_OtherSurface)
            {
                return true;
            }

            // Let skip periodic case.
            if (aSurface.IsUPeriodic() || aSurface.IsVPeriodic())
            {
                return true;
            }

            gp_Pnt2d aUV = new gp_Pnt2d();
            aCurve.GetCurve().D0(theParameter, ref aUV);
            // Point lies within the surface range - nothing to do.
            if (aUV.X() > myFaceRangeU[0] && aUV.X() < myFaceRangeU[1] &&
                aUV.Y() > myFaceRangeV[0] && aUV.Y() < myFaceRangeV[1])
            {
                return true;
            }

            gp_Pnt aPntOnSurf = new gp_Pnt();
            aSurface.D0(aUV.X(), aUV.Y(), ref aPntOnSurf);

            return (thePoint.SquareDistance(aPntOnSurf) < myEdgeSqTol);
        }

        IMeshData_Edge myDEdge;
        IMeshTools_Parameters myParameters;
        TopoDS_Edge myEdge;
        BRepAdaptor_Curve myCurve;
        int myMinPointsNb;
        GCPnts_TangentialDeflection myDiscretTool = new GCPnts_TangentialDeflection();
        TopoDS_Vertex myFirstVertex = new TopoDS_Vertex();
        TopoDS_Vertex myLastVertex = new TopoDS_Vertex();
        double mySquareEdgeDef;
        double mySquareMinSize;
        double myEdgeSqTol;
        double[] myFaceRangeU = new double[2];
        double[] myFaceRangeV = new double[2];
    }
}



