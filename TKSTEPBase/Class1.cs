using OCCPort.Common;
using System;
using System.Reflection.Metadata;

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

    public class StepGeom_Curve : StepGeom_GeometricRepresentationItem
    {
    }

    public class StepGeom_Point : StepGeom_GeometricRepresentationItem
    {
    }
}
