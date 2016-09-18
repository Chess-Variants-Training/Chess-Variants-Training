namespace ChessVariantsTraining.HttpErrors
{
    public class BadRequest : HttpError
    {
        public override int StatusCode { get; } = 400;
        public override string StatusText { get; } = "Bad Request";
        public BadRequest(string desc) : base(desc) { }
    }
}