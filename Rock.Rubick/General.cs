using Divine;
using Divine.SDK.Helpers;

namespace RockRubick
{
    internal sealed class General
    {
        public static Hero localHero;
        public static Sleeper sleeper = new Sleeper();

        public General()
        {
            localHero = EntityManager.LocalHero;
        }
    }
}
