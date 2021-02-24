using Divine;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Helpers;
using System.Linq;

namespace Utilities.Modules
{
    class AutoDust
    {
        private readonly Hero localHero;
        private Item dust;

        private readonly MenuSwitcher autoDustEnabled;

        private readonly Sleeper dustSleeper = new Sleeper();

        public AutoDust(Context context)
        {
            localHero = EntityManager.LocalHero;

            autoDustEnabled = context.rootMenu.CreateSwitcher("Auto Dust");

            autoDustEnabled.ValueChanged += AutoDustEnabled_ValueChanged;
        }

        internal static void Dispose()
        {
            //TODO
        }

        private void AutoDustEnabled_ValueChanged(MenuSwitcher switcher, Divine.Menu.EventArgs.SwitcherEventArgs e)
        {
            if (e.Value)
            {
                UpdateManager.IngameUpdate += UpdateManager_IngameUpdate;
            }
            else
            {
                UpdateManager.IngameUpdate -= UpdateManager_IngameUpdate;
            }
        }

        private void UpdateManager_IngameUpdate()
        {
            dust = localHero.Inventory.MainItems.Where(x => x.Name == "item_dust").FirstOrDefault();

            if (!localHero.IsAlive || dust == null || dust.Cooldown != 0 || localHero.IsInvisible() || dustSleeper.Sleeping)
            {
                return;
            }

            Hero[] enemyes = EntityManager.GetEntities<Hero>().Where(x => x.Distance2D(localHero) < 900 && x.IsEnemy(localHero) && x.IsVisible && !x.IsIllusion && !x.HasModifier("modifier_truesight") && !x.HasModifier("modifier_item_dustofappearance")).ToArray();

            if (enemyes == null || enemyes.Length == 0)
            {
                return;
            }

            foreach (var enemy in enemyes)
            {
                if (enemy.Modifiers.Where(
                    x =>
                        (x.Name == "modifier_bounty_hunter_wind_walk" ||
                         x.Name == "modifier_riki_permanent_invisibility" ||
                         x.Name == "modifier_mirana_moonlight_shadow" || x.Name == "modifier_treant_natures_guise" ||
                         x.Name == "modifier_weaver_shukuchi" ||
                         x.Name == "modifier_item_invisibility_edge_windwalk" || x.Name == "modifier_rune_invis" ||
                         x.Name == "modifier_clinkz_wind_walk" || x.Name == "modifier_item_shadow_amulet_fade" ||
                         x.Name == "modifier_bounty_hunter_track" || x.Name == "modifier_bloodseeker_thirst_vision" ||
                         x.Name == "modifier_slardar_amplify_damage" || x.Name == "modifier_item_dustofappearance") ||
                         x.Name == "modifier_invoker_ghost_walk_enemy").Any())
                {
                    dust.Cast();
                    dustSleeper.Sleep(300);
                }

                if ((enemy.HeroId == HeroId.npc_dota_hero_templar_assassin || enemy.HeroId == HeroId.npc_dota_hero_sand_king) &&
                    (enemy.HealthPercent() < 0.3))
                {
                    dust.Cast();
                    dustSleeper.Sleep(300);
                }

                if (enemy.HeroId == HeroId.npc_dota_hero_nyx_assassin && enemy.Spellbook.Spell4.Cooldown == 0 && enemy.Mana - enemy.Spellbook.Spell4.ManaCost > 0)
                {
                    dust.Cast();
                    dustSleeper.Sleep(300);
                }
            }
        }
    }
}
