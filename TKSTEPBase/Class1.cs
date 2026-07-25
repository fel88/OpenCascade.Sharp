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


    public class StepGeom_Plane : StepGeom_ElementarySurface
    {

    }

    public class StepGeom_ElementarySurface : StepGeom_Surface
    {
        public StepGeom_Axis2Placement3d Position()
        {
            return position;
        }
        StepGeom_Axis2Placement3d position;

    }

    public class StepGeom_Surface : StepGeom_GeometricRepresentationItem
    {

    }


    public class StepGeom_Placement : StepGeom_GeometricRepresentationItem
    {
        public StepGeom_CartesianPoint Location()
        {
            return location;
        }
        StepGeom_CartesianPoint location;

    }

}
