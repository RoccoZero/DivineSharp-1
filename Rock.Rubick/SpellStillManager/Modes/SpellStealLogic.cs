﻿using Divine;
using Divine.SDK.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace RockRubick
{
    internal sealed class SpellStealLogic
    {
        public SpellStealLogic()
        {
            var main = General.localHero.Spellbook.Spell4;
            var ult = General.localHero.Spellbook.Spell6;
            float range = ult.CastRange;

            if (Dictionaries.LastSpell == null || Dictionaries.LastSpell.Count == 0)
            {
                return;
            }

            if (Dictionaries.LastSpell.Count > 1)
            {
                Dictionaries.LastSpell = SpellStealHelper.OrderLastSpell(Dictionaries.LastSpell, Dictionaries.SpellList);
            }

            KeyValuePair<Hero, AbilityId> lastSpell = new KeyValuePair<Hero, AbilityId>();

            lastSpell = Dictionaries.LastSpell.Where(x => x.Key.IsVisible && x.Value != main.Id && x.Key.Distance2D(General.localHero) < range && (x.Key.GetAbilityById(x.Value).Level + 1 >= main.Level
            || Dictionaries.SpellList.Where(y => y.Key == main.Id)
                                     .FirstOrDefault().Value >= 2) && !Dictionaries.Ignore.Contains(x.Value)).FirstOrDefault();


            if (lastSpell.Key == null)
            {
                return;
            }

            if ((main.Id == AbilityId.rubick_empty1 || Dictionaries.HasChargesWithAghanim.ContainsKey(main.Id) && main.CurrentCharges < Dictionaries.HasChargesWithAghanim.Where(x => x.Key == main.Id).FirstOrDefault().Value) && !General.sleeper.Sleeping)
            {
                General.sleeper.Sleep(750);
                ult.Cast(lastSpell.Key);
                return;
            }

            if (main.Id == lastSpell.Value || General.sleeper.Sleeping)
            {
                return;
            }
            if (main.Charges == 0 && main.Cooldown == 0 && !Dictionaries.ShitAbilities.Contains(main.Id))
            {
                return;
            }
            else if (main.Charges != 0 && main.Charges <= main.CurrentCharges + 1)
            {
                return;
            }

            if (General.localHero.Distance2D(lastSpell.Key) < range)
            {
                General.sleeper.Sleep(750);
                ult.Cast(lastSpell.Key);

            }
        }
    }
}
