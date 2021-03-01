using Divine;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Helpers;
using System.Linq;

namespace RockHeroes.Modules.EarthSpirit
{
    internal class Shivas_guard
    {
        public Shivas_guard(Hero hero, Unit target, MenuItemToggler hasItem, ref Sleeper sleeper)
        {
            var item = AbilityId.item_shivas_guard;

            if (hasItem.GetValue(item) && ItemsHelper.FindItem(hero, item) && hero.Position.Distance2D(target.Position) <= 700 && hero.Inventory.MainItems.Where(x => x.Id == item).FirstOrDefault().Cooldown == 0)
            {
                ItemsHelper.CastItem(hero, item);
                sleeper.Sleep(100);
            }
        }
    }
}
