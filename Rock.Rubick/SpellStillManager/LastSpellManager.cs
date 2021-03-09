﻿using Divine;
using Divine.SDK.Extensions;
using System;
using System.Linq;

namespace RockRubick
{
    internal sealed class LastSpellManager
    {

        public LastSpellManager() //Активация лучшей памяти на спелы среди всех скриптов на рубика!)
        {
            UpdateManager.CreateIngameUpdate(50, InGameUpdate);
            ParticleManager.ParticleAdded += ParticleSpecific;
        }


        public static void Dispose()
        {
            UpdateManager.DestroyGameUpdate(InGameUpdate);
            ParticleManager.ParticleAdded -= ParticleSpecific;
        }

        private static void ParticleSpecific(ParticleAddedEventArgs e)
        {
            if (e.Particle.Owner == General.localHero)
            {
                return;
            }
            if (e.Particle.Name == "particles/units/heroes/hero_earthshaker/earthshaker_echoslam_start.vpcf")
            {
                var shaker = (Hero)e.Particle.Owner;
                if (!Dictionaries.LastSpell.ContainsKey(shaker))
                {
                    Dictionaries.LastSpell.Add(shaker, AbilityId.earthshaker_echo_slam);
                }
                else
                {
                    Dictionaries.LastSpell.Remove(shaker);
                    Dictionaries.LastSpell.Add(shaker, AbilityId.earthshaker_echo_slam); //TODO . В будущем написать функцию для добавления
                }
            }
        }

        private static void InGameUpdate()
        {
            var main = General.localHero.Spellbook.Spell4;
            foreach (var enemy in EntityManager.GetEntities<Hero>().Where(x => x.IsEnemy(General.localHero) && !x.IsIllusion && x.Spellbook.Spells.Any(y => y.IsInAbilityPhase)))
            {
                Spell ability = enemy.Spellbook.Spells.Where(x => x.IsInAbilityPhase).FirstOrDefault();
                if (enemy == null || ability == null || main == ability || !enemy.IsVisible)
                {
                    continue;
                }
                if (!enemy.Spellbook.Spells.Any(x => Dictionaries.LastSpell.ContainsKey(enemy)))
                {
                    Dictionaries.LastSpell.Add(enemy, ability.Id);
                }
                else
                {
                    Dictionaries.LastSpell.Remove(enemy);
                    Dictionaries.LastSpell.Add(enemy, ability.Id);
                }

            }

            for (int i = 0; i < Dictionaries.LastSpell.Count; i++)
            {
                var element = Dictionaries.LastSpell.ElementAtOrDefault(i);
                try
                {
                    if (Dictionaries.RubickSpellCD.ContainsKey(element.Value))
                    {
                        Dictionaries.LastSpell.Remove(element.Key);
                        Dictionaries.Removed.Add((element.Key, element.Value), Dictionaries.RubickSpellCD.Where(x => x.Key == element.Value).FirstOrDefault().Value);
                    }
                }
                catch (ArgumentException)
                {
                    //TODO
                }
            }

            for (int i = 0; i < Dictionaries.Removed.Count; i++)
            {
                var element = Dictionaries.Removed.ElementAtOrDefault(i);
                if (Dictionaries.LastSpell.ContainsKey(element.Key.Item1))
                {
                    Dictionaries.Removed.Remove(element.Key);
                    continue;
                }
                else
                {
                    if (GameManager.GameTime > element.Value)
                    {
                        Dictionaries.LastSpell.Add(element.Key.Item1, element.Key.Item2);
                        Dictionaries.Removed.Remove(element.Key);
                    }
                }
                if (i + 1 >= Dictionaries.Removed.Count)
                {
                    break;
                }
            }

        }
    }
}