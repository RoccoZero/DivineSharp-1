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
        private readonly MaxBlinkRange maxBlinkRange;

        public Context()
        {
            rootMenu = MenuManager.CreateRootMenu("Rock.Utilities");
            antislark = new Antislark(this);
            creepblock = new Creepblocker(this);
            autoDust = new AutoDust(this);
            maxBlinkRange = new MaxBlinkRange(this);

        }
        public void Dispose()
        {
            Antislark.Dispose();
            Creepblocker.Dispose();
            AutoDust.Dispose();
            MaxBlinkRange.Dispose();
        }

    }
}
