namespace AtomicChessPuzzles.HttpErrors
{
    public class Unauthorized : HttpError
    {
        public override int StatusCode { get; } = 401;
        public override string StatusText { get; } = "Unauthorized";
        public Unauthorized(string desc) : base(desc) { }
    }
}