using Divine;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Helpers;
using System.Linq;

namespace RockHeroes.Modules.EarthSpirit
{
    internal class Urn
    {
        public Urn(Hero hero, Hero target, MenuItemToggler hasItem, ref Sleeper sleeper)
        {
            var item = AbilityId.item_urn_of_shadows;

            if (hasItem.GetValue(item) && ItemsHelper.FindItem(hero, item) && hero.Position.Distance2D(target.Position) <= 500 && hero.Inventory.MainItems.Where(x => x.Id == item).FirstOrDefault().Cooldown == 0)
            {
                ItemsHelper.CastItemEnemy(hero, target, item);
                sleeper.Sleep(100);
            }
        }
    }
}
