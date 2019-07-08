using Microsoft.AspNetCore.Routing.Constraints;

namespace ChessVariantsTraining.Services
{
    public class SupportedVariantRouteConstraint : RegexRouteConstraint
    {
        public SupportedVariantRouteConstraint() : base("Atomic|KingOfTheHill|ThreeCheck|Antichess|Horde|RacingKings|Crazyhouse") { }
    }
    public class SupportedVariantOrMixedRouteConstraint : RegexRouteConstraint
    {
        public SupportedVariantOrMixedRouteConstraint() : base("Atomic|KingOfTheHill|ThreeCheck|Antichess|Horde|RacingKings|Crazyhouse|Mixed") { }
    }
}
