using Divine;

namespace RockRubick
{
    internal sealed class Bootstap : Bootstrapper
    {
        protected override void OnActivate()
        {
            if (EntityManager.LocalHero.HeroId != HeroId.npc_dota_hero_rubick)
            {
                return;
            }

            Menu.MenuBootstrap();

            new General();
            new SpellStealMain();
        }
    }
}
