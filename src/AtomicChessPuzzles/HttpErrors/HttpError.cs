namespace AtomicChessPuzzles.HttpErrors
{
    public abstract class HttpError
    {
        public abstract int StatusCode { get; }
        public abstract string StatusText { get; }
        public string Description { get; private set; }

        public HttpError(string desc)
        {
            Description = desc;
        }
    }
}