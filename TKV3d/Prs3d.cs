using TKGeomBase;
using TKMath;
using TKService;

namespace TKV3d
{
    //! The Prs3d package provides the following services
    //! -   a presentation object (the context for all
    //! modifications to the display, its presentation will be
    //! displayed in every view of an active viewer)
    //! -   an attribute manager governing how objects such
    //! as color, width, and type of line are displayed;
    //! these are generic objects, whereas those in
    //! StdPrs are specific geometries and topologies.
    //! -   generic   algorithms providing default settings for
    //! objects such as points, curves, surfaces and shapes
    //! -   a root object which provides the abstract
    //! framework for the DsgPrs definitions at work in
    //! display of dimensions, relations and trihedra.
    public class Prs3d
    {
        internal static void AddFreeEdges(TColgp_SequenceOfPnt theSegments, Poly_Triangulation aPolyTri, TopLoc_Location aLocation)
        {
            throw new NotImplementedException();
        }
        //! Computes the absolute deflection value based on relative deflection Prs3d_Drawer::DeviationCoefficient().
        //! @param theBndBox [in] bounding box
        //! @param theDeviationCoefficient [in] relative deflection coefficient from Prs3d_Drawer::DeviationCoefficient()
        //! @param theMaximalChordialDeviation [in] absolute deflection coefficient from Prs3d_Drawer::MaximalChordialDeviation()
        //! @return absolute deflection coefficient based on bounding box dimensions or theMaximalChordialDeviation if bounding box is Void or Infinite
        public static double GetDeflection(Bnd_Box theBndBox,
                                      double theDeviationCoefficient,
                                      double theMaximalChordialDeviation)
        {
            if (theBndBox.IsVoid())
            {
                return theMaximalChordialDeviation;
            }

            Bnd_Box aBndBox = theBndBox;
            if (theBndBox.IsOpen())
            {
                if (!theBndBox.HasFinitePart())
                {
                    return theMaximalChordialDeviation;
                }
                aBndBox = theBndBox.FinitePart();
            }

            Graphic3d_Vec3d aVecMin = new Graphic3d_Vec3d(), aVecMax = new Graphic3d_Vec3d();
            double x1, x2, y1, y2, z1, z2;
            aBndBox.Get(out x1, out y1, out z1, out x2, out y2, out z2);
            aVecMin.SetValues(x1, y1, z1);
            aVecMax.SetValues(x2, y2, z2);

            return GetDeflection(aVecMin, aVecMax, theDeviationCoefficient);
        }

        //! Computes the absolute deflection value based on relative deflection Prs3d_Drawer::DeviationCoefficient().
        //! @param theBndMin [in] bounding box min corner
        //! @param theBndMax [in] bounding box max corner
        //! @param theDeviationCoefficient [in] relative deflection coefficient from Prs3d_Drawer::DeviationCoefficient()
        //! @return absolute deflection coefficient based on bounding box dimensions
        public static double GetDeflection(Graphic3d_Vec3d theBndMin,
                                      Graphic3d_Vec3d theBndMax,
                                      double theDeviationCoefficient)
        {

            var aDiag = theBndMax - theBndMin;
            return Math.Max(aDiag.maxComp() * theDeviationCoefficient * 4.0, Precision.Confusion());
        }

        public static Graphic3d_ArrayOfPrimitives PrimitivesFromPolylines(Prs3d_NListOfSequenceOfPnt thePoints)
        {
            if (thePoints.IsEmpty())
            {
                return null;
            }

            int aNbVertices = 0;
            foreach (var anIt in thePoints)
            {
                aNbVertices += anIt.Length();
            }

            int aSegmentEdgeNb = (aNbVertices - thePoints.Size()) * 2;
            Graphic3d_ArrayOfSegments aSegments = new Graphic3d_ArrayOfSegments(aNbVertices, aSegmentEdgeNb);
            for (Prs3d_NListOfSequenceOfPnt.Iterator anIt = new(thePoints); anIt.More(); anIt.Next())
            {
                TColgp_SequenceOfPnt aPoints = anIt.Value();

                int aSegmentEdge = aSegments.VertexNumber() + 1;
                aSegments.AddVertex(aPoints.First());
                for (int aPntIter = aPoints.Lower() + 1; aPntIter <= aPoints.Upper(); ++aPntIter)
                {
                    aSegments.AddVertex(aPoints.Value(aPntIter));
                    aSegments.AddEdge(aSegmentEdge);
                    aSegments.AddEdge(++aSegmentEdge);
                }
            }

            return aSegments;
        }


        public static void AddPrimitivesGroup(Prs3d_Presentation thePrs,
                                  Prs3d_LineAspect theAspect,
                                  Prs3d_NListOfSequenceOfPnt thePolylines)
        {
            Graphic3d_ArrayOfPrimitives aPrims = Prs3d.PrimitivesFromPolylines(thePolylines);
            thePolylines.Clear();
            if (aPrims != null)
            {
                Graphic3d_Group aGroup = thePrs.NewGroup();
                aGroup.SetPrimitivesAspect(theAspect.Aspect());
                aGroup.AddPrimitiveArray(aPrims);
            }
        }

    }
}

