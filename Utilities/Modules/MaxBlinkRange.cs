using Divine;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Modules
{
    class MaxBlinkRange
    {
        private Hero localHero;
        private MenuSwitcher MaxBlinkRangeSwitcher;


        public MaxBlinkRange(Context context)
        {
            MaxBlinkRangeSwitcher = context.rootMenu.CreateSwitcher("Max Blink Range").SetAbilityTexture(AbilityId.item_blink);

            MaxBlinkRangeSwitcher.ValueChanged += MaxBlinkRangeSwitcher_ValueChanged;

            localHero = EntityManager.LocalHero;
        }

        private void MaxBlinkRangeSwitcher_ValueChanged(MenuSwitcher switcher, Divine.Menu.EventArgs.SwitcherEventArgs e)
        {
            if (e.Value)
            {
                OrderManager.OrderAdding += OrderManager_OrderAdding;
            }
            else
            {
                OrderManager.OrderAdding -= OrderManager_OrderAdding;
            }
        }

        internal static void Dispose()
        {
            //todo
        }

        private static Item FindBlink(Hero hero)
        {
           Item blink = hero.Inventory.MainItems.FirstOrDefault(x => x.Id == AbilityId.item_blink
                                                                        || x.Id == AbilityId.item_swift_blink
                                                                        || x.Id == AbilityId.item_overwhelming_blink
                                                                        || x.Id == AbilityId.item_arcane_blink);
           if (blink != null)
            {
                return blink;
            }
            return null;
        }

        private void OrderManager_OrderAdding(OrderAddingEventArgs e)
        {
            Item blink = FindBlink(localHero);
            Vector3 mousePos = GameManager.MousePosition;
            float blinkRange = 1195;
            if (localHero.Inventory.MainItems.Any(x => x.Id == AbilityId.item_aether_lens))
            {
                blinkRange += 250;
            }

            if (blink == null)
            {           
                e.Process = true;
                return;
            }
            if (blink.Cooldown == 0 && !e.IsCustom && e.Order.Ability == blink)
            {
                var distance = localHero.Distance2D(mousePos);
                if (distance > blinkRange + 5)
                {
                    blink.Cast(localHero.Position.Extend(mousePos, blinkRange));
                    e.Process = false;
                    return;
                }
            }
            e.Process = true;
        }
    }
}
