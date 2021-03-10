using Divine;
using System.Collections.Generic;
using System.Linq;

namespace RockRubick
{
    internal sealed class SpellStealHelper
    {
        public static List<AbilityId> toggleableAbilityes = new List<AbilityId>
        {
            AbilityId.ancient_apparition_chilling_touch,
            AbilityId.clinkz_searing_arrows,
            AbilityId.doom_bringer_infernal_blade,
            AbilityId.drow_ranger_frost_arrows,
            AbilityId.enchantress_impetus,
            AbilityId.storm_spirit_ball_lightning,
            AbilityId.huskar_burning_spear,
            AbilityId.jakiro_liquid_fire,
            AbilityId.pudge_rot,
            AbilityId.morphling_morph,
            AbilityId.pugna_life_drain,
            AbilityId.sniper_take_aim,
            AbilityId.witch_doctor_voodoo_restoration,
            AbilityId.windrunner_windrun,
            AbilityId.tinker_rearm
        };

        public static bool IsCastable(Hero localHero, Ability ability) //Проверка на возможность скастовать
        {
            if (localHero.Mana > ability.ManaCost && ability.Cooldown == 0)
            {
                return true;
            }
            return false;
        }

        public static Dictionary<Hero, AbilityId> OrderLastSpell(Dictionary<Hero, AbilityId> lastspell, Dictionary<AbilityId, int> dictionary) //Сортировка словаря последних использованых способностей по убыванию приоритета
        {
            Dictionary<Hero, AbilityId> orderedDictionary = new Dictionary<Hero, AbilityId> { };
            Dictionary<Hero, (AbilityId, int)> HeroAbilityInt = new Dictionary<Hero, (AbilityId, int)> { };

            foreach (var element in lastspell)
            {
                HeroAbilityInt.Add(element.Key, (element.Value, dictionary.Where(x => x.Key == element.Value).FirstOrDefault().Value));
            }
            var HAI = HeroAbilityInt.OrderByDescending(x => x.Value.Item2);
            foreach (var element in HAI)
            {
                orderedDictionary.Add(element.Key, element.Value.Item1);
            }
            return orderedDictionary;
        }
    }
}
