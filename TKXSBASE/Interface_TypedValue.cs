namespace TKXSBASE
{
    //! Now strictly equivalent to TypedValue from MoniTool,
    //! except for ParamType which remains for compatibility reasons
    //!
    //! This class allows to dynamically manage .. typed values, i.e.
    //! values which have an alphanumeric expression, but with
    //! controls. Such as "must be an Integer" or "Enumerative Text"
    //! etc
    //!
    //! Hence, a TypedValue brings a specification (type + constraints
    //! if any) and a value. Its basic form is a string, it can be
    //! specified as integer or real or enumerative string, then
    //! queried as such.
    //! Its string content, which is a Handle(HAsciiString) can be
    //! shared by other data structures, hence gives a direct on line
    //! access to its value.
    public class Interface_TypedValue : MoniTool_TypedValue
    {

    }
}
