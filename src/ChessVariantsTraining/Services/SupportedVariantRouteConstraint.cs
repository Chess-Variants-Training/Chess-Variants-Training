using Microsoft.AspNet.Routing.Constraints;

namespace ChessVariantsTraining.Services
{
    public class SupportedVariantRouteConstraint : RegexRouteConstraint
    {
        public SupportedVariantRouteConstraint() : base("Atomic|KingOfTheHill|ThreeCheck") { }
    }
}
