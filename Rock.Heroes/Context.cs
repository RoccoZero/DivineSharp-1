using Divine.Menu;
using Divine.Menu.Items;
using RockHeroes.Modules;

namespace RockHeroes
{
    class Context
    {
        public readonly RootMenu rootMenu;
        public readonly AiEarthSpirit aiEarthSpirit;
        public readonly SfRazes sfRazes;

        public Context()
        {
            rootMenu = MenuManager.CreateRootMenu("Rock.Heroes");
            aiEarthSpirit = new AiEarthSpirit(this);
            sfRazes = new SfRazes(this);
        }

        public void Dispose()
        {
            aiEarthSpirit.Dispose();
            sfRazes.Dispose();
        }

    }
}
