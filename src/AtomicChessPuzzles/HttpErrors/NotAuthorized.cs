namespace AtomicChessPuzzles.HttpErrors
{
    public class NotAuthorized : HttpError
    {
        public override int StatusCode { get; } = 401;
        public override string StatusText { get; } = "Not Authorized";
        public NotAuthorized(string desc) : base(desc) { }
    }
}