namespace AtomicChessPuzzles.HttpErrors
{
    public class Forbidden : HttpError
    {
        public override int StatusCode { get; } = 403;
        public override string StatusText { get; } = "Forbidden";
        public Forbidden(string desc) : base(desc) { }
    }
}