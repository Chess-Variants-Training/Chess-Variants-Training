using Microsoft.AspNet.Routing.Constraints;

namespace ChessVariantsTraining.Services
{
    public class SupportedVariantOrMixedRouteConstraint : RegexRouteConstraint
    {
        public SupportedVariantOrMixedRouteConstraint() : base("Atomic|KingOfTheHill|ThreeCheck|Mixed") { }
    }
}
