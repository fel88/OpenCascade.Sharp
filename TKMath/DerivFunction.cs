namespace TKMath
{
    public class DerivFunction : math_Function
    {
        public DerivFunction(math_FunctionWithDerivative theF)
        {
            myF = (theF);
        }
        math_FunctionWithDerivative myF;

        public override bool Value(double X, out double F)
        {
            throw new NotImplementedException();
        }

        public override int GetStateNumber()
        {
            throw new NotImplementedException();
        }
    }
}
