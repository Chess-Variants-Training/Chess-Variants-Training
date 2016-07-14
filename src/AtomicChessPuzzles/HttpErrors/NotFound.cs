namespace AtomicChessPuzzles.HttpErrors
{
    public class NotFound : HttpError
    {
        public override int StatusCode { get; } = 404;
        public override string StatusText { get; } = "Not Found";
        public NotFound(string desc) : base(desc) { }
    }
}