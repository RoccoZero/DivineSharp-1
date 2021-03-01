using Divine;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Helpers;
using System.Linq;

namespace RockHeroes.Modules.EarthSpirit
{
    internal class Sheepstick
    {
        public Sheepstick(Hero hero, Unit target, MenuItemToggler hasItem, ref Sleeper sleeper)
        {
            var item = AbilityId.item_sheepstick;

            if (hasItem.GetValue(item)
                && ItemsHelper.FindItem(hero, item)
                && hero.Position.Distance2D(target.Position) <= 750
                && hero.Inventory.MainItems.Where(x => x.Id == item).FirstOrDefault().Cooldown == 0
                && !target.IsHexed()
                && !target.IsStunned())
            {
                ItemsHelper.CastItemEnemy(hero, target, item);
                sleeper.Sleep(150);
            }
        }

    }
}
