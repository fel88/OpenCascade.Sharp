using OCCPort.Common;
using System;
using System.Reflection.Metadata;
using TKernel;

namespace TKSTEPBase
{

    public class StepRepr_RepresentationItem
    {
        string name;

        public void SetName(string aName)
        {
            name = aName;
        }
    }
    public class StepGeom_GeometricRepresentationItem : StepRepr_RepresentationItem
    {
    }

    public class StepShape_HArray1OfFace : StepShape_Array1OfFace
    {

    }
    public class StepShape_Array1OfFace : NCollection_Array1<StepShape_Face>
    {
    }

    public class StepGeom_Curve : StepGeom_GeometricRepresentationItem
    {
    }

    public class StepGeom_Point : StepGeom_GeometricRepresentationItem
    {
    }
    public class StepShape_Face : StepShape_TopologicalRepresentationItem
    { }

}
