using Divine;
using Divine.SDK.Extensions;
using System;
using System.Linq;

namespace RockRubick
{
    internal sealed class SpellStealLogic
    {
        public SpellStealLogic()
        {
            var main = General.localHero.Spellbook.Spell4;
            var ult = General.localHero.Spellbook.Spell6;

            if (Dictionaries.LastSpell == null || Dictionaries.LastSpell.Count == 0)
            {
                return;
            }

            if (Dictionaries.LastSpell.Count > 1)
            {
                Dictionaries.LastSpell = SpellStealHelper.OrderLastSpell(Dictionaries.LastSpell, Dictionaries.SpellList);
            }

            var lastSpell = Dictionaries.LastSpell.Where(x => x.Key.IsVisible && x.Key.Distance2D(General.localHero) < 1500 && (x.Key.GetAbilityById(x.Value).Level + 1 >= main.Level
            || Dictionaries.SpellList.Where(y => y.Key == main.Id).FirstOrDefault().Value >= 4)).FirstOrDefault(); // for test


            if (lastSpell.Key == null)
            {
                return;
            }

            if (main.Id == AbilityId.rubick_empty1 && !General.sleeper.Sleeping)
            {
                UpdateManager.BeginInvoke(400, () =>
                {
                    ult.Cast(lastSpell.Key);
                });
                General.sleeper.Sleep(750);
                return;
            }

            if (main.Id == lastSpell.Value || General.sleeper.Sleeping)
            {
                return;
            }
            if (main.Charges == 0 && main.Cooldown == 0 && !SpellStealHelper.toggleableAbilityes.Contains(main.Id))
            {
                return;
            }
            else if (main.Charges != 0 && main.Charges <= main.CurrentCharges + 1)
            {
                return;
            }

            if (General.localHero.Distance2D(lastSpell.Key) < 1500)
            {
                UpdateManager.BeginInvoke((int)Math.Floor(lastSpell.Key.GetAbilityById(lastSpell.Value).CastPoint * 1000), () =>
                {
                    ult.Cast(lastSpell.Key);
                });
                General.sleeper.Sleep(750);
            }
        }
    }
}
