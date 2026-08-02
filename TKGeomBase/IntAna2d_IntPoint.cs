using OCCPort.Common;
using TKMath;

namespace TKGeomBase
{
    //! Geometrical intersection between two 2d elements.
    public class IntAna2d_IntPoint
    {

        //! Returns the parameter on the first element.
        public double  ParamOnFirst()
        {
            return myu1;

        }

        //! Returns the parameter on the second element.
        //! If the second element is an implicit curve, an exception
        //! is raised.
        public double  ParamOnSecond()
        {
            if (myimplicit)
            {
                throw new Standard_DomainError();
            }
            return myu2;
        }

        public void SetValue(double X, double Y,
                     double U1, double U2)
        {

            myimplicit = false;
            myp.SetCoord(X, Y);
            myu1 = U1;
            myu2 = U2;

        }


        double myu1;
        double myu2;
        gp_Pnt2d myp;
        bool myimplicit;

    }
}

