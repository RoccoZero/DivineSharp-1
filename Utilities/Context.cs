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

        public Context()
        {
            rootMenu = MenuManager.CreateRootMenu("Utilities");
            antislark = new Antislark(this);
            creepblock = new Creepblocker(this);
        }
        public void Dispose()
        {
            Antislark.Dispose();
            Creepblocker.Dispose();
        }

    }
}
