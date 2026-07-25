using OCCPort;
using OCCPort.Common;
using TKBRep;
using TKG3d;
using TKMath;

namespace TKShHealing
{
    //! This class provides a data structure necessary for work with the wire as with
    //! ordered list of edges, what is required for many algorithms. The advantage of
    //! this class is that it allows to work with wires which are not correct.
    //! The object of the class ShapeExtend_WireData can be initialized by
    //! TopoDS_Wire, and converted back to TopoDS_Wire.
    //! An edge in the wire is defined by its rank number. Operations of accessing,
    //! adding and removing edge at the given rank number are provided. On the whole
    //! wire, operations of circular permutation and reversing (both orientations of
    //! all edges and order of edges) are provided as well.
    //! This class also provides a method to check if the edge in the wire is a seam
    //! (if the wire lies on a face).
    //! This class is handled by reference. Such an approach gives the following advantages:
    //! 1.    Sharing the object of this class strongly optimizes the processes of
    //! analysis and fixing performed in parallel on the wire stored in the form
    //! of this class. Fixing tool (e.g. ShapeFix_Wire) fixes problems one by
    //! one using analyzing tool (e.g. ShapeAnalysis_Wire). Sharing allows not
    //! to reinitialize each time the analyzing tool with modified
    //! ShapeExtend_WireData what consumes certain time.
    //! 2.    No copying of contents. The object of ShapeExtend_WireData class has
    //! quite big size, returning it as a result of the function would cause
    //! additional copying of contents if this class were one handled by value.
    //! Moreover, this class is stored as a field in other classes which are
    //! they returned as results of functions, storing only a handle to
    //! ShapeExtend_WireData saves time and memory.
    public class ShapeExtend_WireData
    {

        public ShapeExtend_WireData() { }
        TopTools_HSequenceOfShape myEdges;
        int mySeamF;
        int mySeamR;
        TopTools_HSequenceOfShape myNonmanifoldEdges;

        bool myManifoldMode;
        public int NbEdges()
        {
            return myEdges.Count;
        }
        public void Clear()
        {
            myEdges = new TopTools_HSequenceOfShape();
            myNonmanifoldEdges = new TopTools_HSequenceOfShape();
            mySeamF = mySeamR = -1;
            //mySeams.Nullify();
            myManifoldMode = true;
        }

        //! Constructor initializing the data from TopoDS_Wire. Calls Init(wire,chained).
        public ShapeExtend_WireData(TopoDS_Wire wire, bool chained = true, bool theManifold = true)
        {
            Init(wire, chained, theManifold);
        }
        public void Init(ShapeExtend_WireData other)
        {
            Clear();
            int i, nb = other.NbEdges();
            /*for (i = 1; i <= nb; i++) Add(other.Edge(i));
            nb = other.NbNonManifoldEdges();
            for (i = 1; i <= nb; i++) Add(other.NonmanifoldEdge(i));
            myManifoldMode = other.ManifoldMode();*/
        }
        public bool Init(TopoDS_Wire wire,
                         bool chained,
                                             bool theManifold)
        {
            Clear();
            myManifoldMode = theManifold;
            bool OK = true;
            TopoDS_Vertex Vlast = new TopoDS_Vertex();
            for (TopoDS_Iterator it = new TopoDS_Iterator(wire); it.More(); it.Next())
            {
                TopoDS_Edge E = TopoDS.Edge(it.Value());

                // protect against INTERNAL/EXTERNAL edges
                if ((E.Orientation() != TopAbs_Orientation.TopAbs_REVERSED &&
                 E.Orientation() != TopAbs_Orientation.TopAbs_FORWARD))
                {
                    myNonmanifoldEdges.Append(E);
                    continue;
                }

                TopoDS_Vertex V1 = new TopoDS_Vertex(), V2 = new TopoDS_Vertex();
                for (TopoDS_Iterator itv = new TopoDS_Iterator(E); itv.More(); itv.Next())
                {
                    TopoDS_Vertex V = TopoDS.Vertex(itv.Value());
                    if (V.Orientation() == TopAbs_Orientation.TopAbs_FORWARD) V1 = V;
                    else if (V.Orientation() == TopAbs_Orientation.TopAbs_REVERSED) V2 = V;
                }

                // chainage? Si pas bon et chained False on repart sur WireExplorer
                if (!Vlast.IsNull() && !Vlast.IsSame(V1) && theManifold)
                {
                    OK = false;
                    if (!chained)
                        break;
                }
                Vlast = V2;
                if (wire.Orientation() == TopAbs_Orientation.TopAbs_REVERSED)
                    myEdges.Prepend(E);
                else
                    myEdges.Append(E);
            }

            if (!myManifoldMode)
            {
                int nb = myNonmanifoldEdges.Length();
                int i = 1;
                for (; i <= nb; i++)
                    myEdges.Append(myNonmanifoldEdges.Value(i));
                myNonmanifoldEdges.Clear();
            }
            //    refaire chainage ?  Par WireExplorer
            if (OK || chained)
                return OK;

            Clear();

            for (BRepTools_WireExplorer we = new BRepTools_WireExplorer(wire); we.More(); we.Next())
                myEdges.Append(TopoDS.Edge(we.Current()));

            return OK;
        }

        public TopoDS_Edge Edge(int num)
        {
            if (num < 0)
            {
                TopoDS_Edge E = Edge(-num);
                E.Reverse();
                return E;
            }
            return TopoDS.Edge(myEdges.Value(num));
        }

    }
    }
