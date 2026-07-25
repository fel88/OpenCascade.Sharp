using OCCPort;
using OCCPort.Common;
using TKMath;

namespace TKPrim
{
    //! Describes functions to build cylinders or portions of  cylinders.
    //! A MakeCylinder object provides a framework for:
    //! -   defining the construction of a cylinder,
    //! -   implementing the construction algorithm, and
    //! -   consulting the result.
    public class BRepPrimAPI_MakeCylinder : BRepPrimAPI_MakeOneAxis
    {

        BRepPrim_Cylinder myCylinder;


        //! Make a cylinder.
        //! @param R [in] cylinder radius
        //! @param H [in] cylinder height
        public BRepPrimAPI_MakeCylinder(double R, double H)
        {
            myShape = new TopoDS_Solid();

            myCylinder = new BRepPrim_Cylinder(gp.XOY(), R, H);
        }

        //! Make a cylinder (part cylinder).
        //! @param R     [in] cylinder radius
        //! @param H     [in] cylinder height
        //! @param Angle [in] defines the missing portion of the cylinder
        public BRepPrimAPI_MakeCylinder(double R,
                        double H,
                        double Angle)
        {
            myCylinder = new(R, H);
            myCylinder.Angle(Angle);
        }

        public override object OneAxis()
        {
            return myCylinder;
        }


        //! Make a cylinder of radius R and length H.
        //! @param Axes [in] coordinate system for the construction of the cylinder
        //! @param R    [in] cylinder radius
        //! @param H    [in] cylinder height
        public BRepPrimAPI_MakeCylinder(gp_Ax2 Axes, double R, double H)
        {
            myCylinder = new BRepPrim_Cylinder(Axes, R, H);
        }

    }

    //! Describes functions to build spheres or portions of spheres.
    //! A MakeSphere object provides a framework for:
    //! -   defining the construction of a sphere,
    //! -   implementing the construction algorithm, and
    //! -   consulting the result.
    public class BRepPrimAPI_MakeSphere : BRepPrimAPI_MakeOneAxis
    {
        public BRepPrimAPI_MakeSphere(double R)
        {
             
            myShape = new TopoDS_Solid();

            mySphere = new(gp.XOY(), R);
        }

        public override object OneAxis()
        {
            return mySphere;
        }


        BRepPrim_Sphere mySphere;

    }
}
