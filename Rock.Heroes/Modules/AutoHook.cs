using Divine;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Prediction;
using Divine.SDK.Prediction.Collision;
using SharpDX;
using System.Linq;
using System.Windows.Input;

namespace RockHeroes.Modules
{
    internal class AutoHook
    {
        private readonly Hero localHero;

        private readonly Menu AutoHookMenu;
        private readonly MenuSwitcher isEnable;
        private readonly MenuHoldKey holdKey;

        public AutoHook(Context context)
        {
            localHero = EntityManager.LocalHero;

            AutoHookMenu = context.rootMenu.CreateMenu("AutoHook").SetHeroTexture(HeroId.npc_dota_hero_pudge);
            isEnable = AutoHookMenu.CreateSwitcher("On/Off");
            holdKey = AutoHookMenu.CreateHoldKey("Auto hook key", Key.None);
            if (localHero.HeroId != HeroId.npc_dota_hero_pudge)
            {
                return;
            }

            UpdateManager.IngameUpdate += OnIngameUpdate;
        }

        internal void Dispose()
        {
            //
        }

        private void OnIngameUpdate()
        {
            if (!holdKey)
            {
                return;
            }

            var localHero = EntityManager.LocalHero;
            if (localHero == null)
            {
                return;
            }

            var target = EntityManager.GetEntities<Hero>().Where(x => x.Distance2D(GameManager.MousePosition) < 800 && x.IsVisible && !x.IsAlly(localHero)).OrderBy(x => x.Distance2D(GameManager.MousePosition)).FirstOrDefault();
            if (target == null)
            {
                return;
            }

            var hook = localHero.Spellbook.Spell1;

            var input = new PredictionInput
            {
                Owner = localHero,
                AreaOfEffect = false,
                CollisionTypes = CollisionTypes.AllUnits | CollisionTypes.Runes,
                Delay = hook.CastPoint,
                Speed = hook.GetAbilitySpecialData("hook_speed"),
                Range = hook.GetAbilitySpecialData("hook_distance"),
                Radius = hook.GetAbilitySpecialData("hook_width"),
                PredictionSkillshotType = PredictionSkillshotType.SkillshotLine
            };

            input = input.WithTarget(target);

            var hookOutput = PredictionManager.GetPrediction(input);

            if (hookOutput.HitChance != HitChance.OutOfRange && hookOutput.HitChance != HitChance.Collision)
            {
                hook.Cast(hookOutput.UnitPosition);
            }
        }
    }
}
