using Divine;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Helpers;
using Divine.SDK.Prediction;
using Divine.SDK.Prediction.Collision;
using SharpDX;
using System;
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
        private Sleeper sleeper = new Sleeper();

        private bool HookModifierDetected;

        private float HookStartCastTime;

        private UpdateHandler HookUpdateHandler;

        private Vector3 HookCastPosition;

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

            Entity.NetworkPropertyChanged += OnNetworkPropertyChanged;
            ModifierManager.ModifierAdded += OnModifierAdded;
            ModifierManager.ModifierRemoved += OnModifierRemoved;
            HookUpdateHandler = UpdateManager.CreateIngameUpdate(0, false, HookHitCheck);
        }

        internal void Dispose()
        {
            
        }

        private void OnIngameUpdate()
        {
            if (!holdKey)
            {
                return;
            }

            if (localHero == null)
            {
                return;
            }

            if (HookModifierDetected)
            {
                return;
            }

            var target = EntityManager.GetEntities<Hero>().Where(x => x.Distance2D(GameManager.MousePosition) < 800 && x.IsVisible && !x.IsAlly(localHero) && x.IsAlive).OrderBy(x => x.Distance2D(GameManager.MousePosition)).FirstOrDefault();
            if (target == null)
            {
                return;
            }

            var hook = localHero.Spellbook.Spell1;
            var lens = AbilityId.item_aether_lens;
            float HookRange = 1300;

            if (localHero.Inventory.MainItems.FirstOrDefault(x => x.Id == lens) != null)
                HookRange = 1550;

            var input = new PredictionInput
            {
                Owner = localHero,
                AreaOfEffect = false,
                CollisionTypes = CollisionTypes.AllUnits | CollisionTypes.Runes,
                Delay = hook.CastPoint,
                Speed = hook.GetAbilitySpecialData("hook_speed"),
                Range = HookRange,
                Radius = hook.GetAbilitySpecialData("hook_width"),
                PredictionSkillshotType = PredictionSkillshotType.SkillshotLine
            };

            input = input.WithTarget(target);

            var hookOutput = PredictionManager.GetPrediction(input);

            if (hookOutput.HitChance >= HitChance.Medium && hookOutput.HitChance != HitChance.Collision && hook.Cooldown == 0) /*&& !sleeper.Sleeping*/
            {
                HookCastPosition = hookOutput.UnitPosition;
                hook.Cast(HookCastPosition);
                //sleeper.Sleep(50);
            }
        }

        private void HookHitCheck()
        {
            var target = EntityManager.GetEntities<Hero>().Where(x => x.Distance2D(GameManager.MousePosition) < 800 && x.IsVisible && !x.IsAlly(localHero) && x.IsAlive).OrderBy(x => x.Distance2D(GameManager.MousePosition)).FirstOrDefault();
            if (target == null)
            {
                return;
            }

            var hook = localHero.Spellbook.Spell1;
            var lens = AbilityId.item_aether_lens;

            float HookRange = 1300;
            if (localHero.Inventory.MainItems.FirstOrDefault(x => x.Id == lens) != null)
                HookRange = 1550;

            var input = new PredictionInput
            {
                Owner = localHero,
                AreaOfEffect = false,
                CollisionTypes = CollisionTypes.AllUnits | CollisionTypes.Runes,
                Delay = Math.Max((HookStartCastTime - GameManager.RawGameTime) + hook.CastPoint, 0),
                Speed = hook.GetAbilitySpecialData("hook_speed"),
                Range = HookRange,
                Radius = hook.GetAbilitySpecialData("hook_width"),
                PredictionSkillshotType = PredictionSkillshotType.SkillshotLine
            };

            input = input.WithTarget(target);

            var hookOutput = PredictionManager.GetPrediction(input);

            if ((hookOutput.HitChance == HitChance.OutOfRange || hookOutput.HitChance == HitChance.Collision || hookOutput.HitChance == HitChance.Impossible || target.IsRotating() || hookOutput.HitChance == HitChance.Low)/* && !sleeper.Sleeping*/)
            {
                localHero.Stop();
                //sleeper.Sleep(50);
                HookUpdateHandler.IsEnabled = false;
            }
        }

        private void OnNetworkPropertyChanged(Entity sender, NetworkPropertyChangedEventArgs e)
        {
            if (!holdKey)
            {
                return;
            }

            if (e.PropertyName != "m_bInAbilityPhase")
            {
                return;
            }

            UpdateManager.BeginInvoke(() =>
            {
                var newValue = e.NewValue.GetBoolean();
                if (newValue == e.OldValue.GetBoolean() || sender != localHero.Spellbook.Spell1)
                {
                    return;
                }

                if (newValue)
                {
                    HookStartCastTime = GameManager.RawGameTime;
                    HookUpdateHandler.IsEnabled = true;
                }
                else
                {
                    HookUpdateHandler.IsEnabled = false;
                }
            });
        }

        private void OnModifierAdded(ModifierAddedEventArgs e)
        {
            var modifier = e.Modifier;

            UpdateManager.BeginInvoke(() =>
            {
                if (!modifier.IsValid || modifier.Name != "modifier_pudge_meat_hook")
                {
                    return;
                }

                var owner = modifier.Owner;
                if (owner is Hero && localHero.IsEnemy(owner))
                {
                    HookModifierDetected = true; 
                    localHero.Stop();
                }
            });
            

        }

        private void OnModifierRemoved(ModifierRemovedEventArgs e)
        {
            var modifier = e.Modifier;
            if (modifier.Name != "modifier_pudge_meat_hook")
            {
                return;
            }

            if (localHero.IsEnemy(modifier.Owner))
            {
                HookModifierDetected = false;
            }
        }
    }
}