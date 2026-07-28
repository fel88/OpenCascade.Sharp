using TKMath;

namespace TKMesh
{
    //! Auxiliary class extending default range splitter in
    //! order to generate internal nodes for spherical surface.
    public class BRepMesh_SphereRangeSplitter : BRepMesh_DefaultRangeSplitter
    {

        //! Computes step for the given range.
        void computeStep(
    ref (double, double) theRange,
     double theDefaultStep,
   ref (double, double) theStepAndOffset)
        {
            double aDiff = theRange.Item2 - theRange.Item1;
            theStepAndOffset.Item1 = aDiff / ((int)(aDiff / theDefaultStep) + 1);
            theStepAndOffset.Item2 = theRange.Item2 - Precision.PConfusion();
        }
    

    public override ListOfPnt2d GenerateSurfaceNodes(IMeshTools_Parameters theParameters)
    {

        // Calculate parameters for iteration in V direction
        double aStep = 0.7 * GCPnts_TangentialDeflection.ArcAngularStep(
          GetDFace().GetSurface().Sphere().Radius(), GetDFace().GetDeflection(),
          theParameters.Angle, theParameters.MinSize);

        (double, double)[] aRange = {
    GetRangeV(),
    GetRangeU()
  };

        (double, double)[] aStepAndOffset = new (double, double)[2];
        computeStep(ref aRange[0], aStep,ref  aStepAndOffset[0]);
        computeStep(ref aRange[1], aStep,ref  aStepAndOffset[1]);


        ListOfPnt2d aNodes = new ListOfPnt2d();

        double aHalfDu = aStepAndOffset[1].Item1 * 0.5;
        bool Shift = false;
        double aPasV = aRange[0].Item1 + aStepAndOffset[0].Item1;
        for (; aPasV < aStepAndOffset[0].Item2; aPasV += aStepAndOffset[0].Item1)
        {
            Shift = !Shift;
            double d = (Shift) ? aHalfDu : 0.0;
            double aPasU = aRange[1].Item1 + d;
            for (; aPasU < aStepAndOffset[1].Item2; aPasU += aStepAndOffset[1].Item1)
            {
                aNodes.Append(new gp_Pnt2d(aPasU, aPasV));
            }
        }

        return aNodes;
    }

}

    
}

