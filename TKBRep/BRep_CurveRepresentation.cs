using OCCPort.Common;
using System.ComponentModel.Design;
using System.Reflection.Metadata;
using TKG2d;
using TKG3d;
using TKMath;

namespace OCCPort
{
    //! Root class for the curve representations. Contains
    //! a location.
    public abstract class BRep_CurveRepresentation
    {
        public virtual Geom_Curve Curve3D()
        {
            throw new Standard_DomainError("BRep_CurveRepresentation");
        }

        public virtual Poly_Polygon3D Polygon3D()
        {
            throw new Standard_DomainError("BRep_CurveRepresentation");
        }

        public virtual void Polygon3D(Poly_Polygon3D P)
        {

        }
        //! Return a copy of this representation.
        public abstract BRep_CurveRepresentation Copy();

        public virtual bool IsRegularity
          (Geom_Surface S1,
   Geom_Surface S2,
   TopLoc_Location L1,
   TopLoc_Location L2)
        {
            return false;
        }

        public virtual bool IsRegularity()
        {
            return false;
        }
        public virtual Geom_Surface Surface()
        {
            throw new Standard_DomainError("BRep_CurveRepresentation");
        }

        //! A representation by two arrays of nodes on a
        //! triangulation.
        public virtual bool IsPolygonOnClosedTriangulation()
        {
            return false;
        }
        public virtual Poly_PolygonOnTriangulation PolygonOnTriangulation2()
        {
            throw new Standard_DomainError();
        }
        public virtual Poly_PolygonOnTriangulation PolygonOnTriangulation()
        {
            throw new Standard_DomainError();
        }

        public virtual bool IsCurveOnClosedSurface()
        {
            return false;
        }
        //! A representation by an array of nodes on a
        //! triangulation.
        public virtual bool IsPolygonOnTriangulation()
        {
            return false;
        }
        public virtual bool IsPolygonOnTriangulation
  (Poly_Triangulation t, TopLoc_Location l)
        {
            return false;
        }
        public virtual Geom2d_Curve PCurve2()
        {
            throw new Standard_DomainError("BRep_CurveRepresentation");
        }

        public virtual Geom2d_Curve PCurve()
        {
            throw new Standard_DomainError("BRep_CurveRepresentation");
        }

        //! A 3D curve representation.
        public virtual bool IsCurve3D()
        {
            return false;
        }
        //! A 3D polygon representation.
        public virtual bool IsPolygon3D()
        {
            return false;
        }
        public virtual bool IsCurveOnSurface(Geom_Surface a, TopLoc_Location f)
        {
            return false;
        }

        public abstract void Continuity(GeomAbs_Shape shape);

        //=======================================================================
        //function : Continuity
        //purpose  : 
        //=======================================================================

        public virtual GeomAbs_Shape Continuity()
        {
            throw new Standard_DomainError("BRep_CurveRepresentation");
        }

        public void Curve3D(Geom_Curve cc)
        {
            throw new Standard_DomainError("BRep_CurveRepresentation");
        }


        public virtual bool IsCurveOnSurface()
        {
            return false;
        }

        public TopLoc_Location Location()
        {
            return myLocation;
        }
        //Standard_EXPORT BRep_CurveRepresentation(const TopLoc_Location& L);
        public void Location(TopLoc_Location L)
        {
            myLocation = L;
        }

        protected TopLoc_Location myLocation = new TopLoc_Location();

        protected BRep_CurveRepresentation(TopLoc_Location L)
        {
            myLocation = (L);
        }
    }
}
