namespace TKSTEPBase
{
    public class StepShape_ConnectedFaceSet : StepShape_TopologicalRepresentationItem
    {
        public int NbCfsFaces()
        {
            if (cfsFaces==null)
                return 0;
            return cfsFaces.Length();
        }

        public StepShape_Face CfsFacesValue(int num)
        {
            return cfsFaces.Value(num);
        }
        StepShape_HArray1OfFace cfsFaces;

    }

}
