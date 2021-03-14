using Divine;
using Divine.SDK.Extensions;
using System;
using System.Linq;

namespace RockRubick
{
    internal sealed class LastSpellManager
    {

        public LastSpellManager()
        {
            UpdateManager.CreateIngameUpdate(50, InGameUpdate);
            ParticleManager.ParticleAdded += ParticleSpecific;
        }


        public static void Dispose()
        {
            UpdateManager.DestroyGameUpdate(InGameUpdate);
            ParticleManager.ParticleAdded -= ParticleSpecific;
        }

        private static void AddSpecific(Hero hero, AbilityId abilityId)
        {
            if (!Dictionaries.LastSpell.ContainsKey(hero))
            {
                Dictionaries.LastSpell.Add(hero, abilityId);
            }
            else
            {
                Dictionaries.LastSpell.Remove(hero);
                Dictionaries.LastSpell.Add(hero, abilityId);
            }
        }

        private static void ParticleSpecific(ParticleAddedEventArgs e)
        {
            var particle = e.Particle;
            if (particle.Owner == General.localHero)
            {
                return;
            }
            if (particle.Name == "particles/units/heroes/hero_earthshaker/earthshaker_echoslam_start.vpcf")
            {
                AddSpecific((Hero)e.Particle.Owner, AbilityId.earthshaker_echo_slam);
            }

            if (e.Particle.Name == "particles/units/heroes/hero_void_spirit/aether_remnant/void_spirit_aether_remnant_pre.vpcf")
            {
                AddSpecific((Hero)e.Particle.Owner.Owner, AbilityId.void_spirit_aether_remnant);
            }
            if (e.Particle.Name == "particles/units/heroes/hero_shredder/shredder_whirling_death.vpcf")
            {
                AddSpecific((Hero)e.Particle.Owner, AbilityId.shredder_whirling_death);
            }
            //Console.WriteLine($"{e.Particle.Owner.Name} | {e.Particle.Name}");
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
                var e = enemy;
                var cp = ability.CastPoint;
                if (!enemy.Spellbook.Spells.Any(x => Dictionaries.LastSpell.ContainsKey(enemy)))
                {
                    if (cp * 1000 >= 60)
                    {
                        UpdateManager.BeginInvoke((int)(cp * 1000 - 50), () =>
                        {
                            if (ability.IsInAbilityPhase)
                            {
                                Dictionaries.LastSpell.Add(enemy, ability.Id);
                            }
                        });
                    }
                    else
                    {
                        Dictionaries.LastSpell.Add(enemy, ability.Id);
                    }
                    
                    //Dictionaries.LastSpell.Add(enemy, ability.Id);
                }
                else
                {
                    if (cp * 1000 >= 60)
                    {
                        UpdateManager.BeginInvoke((int)(ability.CastPoint * 1000 - 50), () =>
                        {
                            if (ability.IsInAbilityPhase)
                            {
                                Dictionaries.LastSpell.Remove(enemy);
                                Dictionaries.LastSpell.Add(enemy, ability.Id);
                            }
                        });
                    }
                    else
                    {
                        Dictionaries.LastSpell.Remove(enemy);
                        Dictionaries.LastSpell.Add(enemy, ability.Id);
                    }
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
