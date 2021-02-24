using Divine.Menu;
using Divine.Menu.Items;
using Utilities.Modules;

namespace Utilities
{
    internal sealed class Context
    {
        public readonly RootMenu rootMenu;

        public readonly Antislark antislark;
        public readonly Creepblocker creepblock;
        public readonly AutoDust autoDust;

        public Context()
        {
            rootMenu = MenuManager.CreateRootMenu("Rock.Utilities");
            antislark = new Antislark(this);
            creepblock = new Creepblocker(this);
            autoDust = new AutoDust(this);
        }
        public void Dispose()
        {
            Antislark.Dispose();
            Creepblocker.Dispose();
            AutoDust.Dispose();
        }

    }
}
