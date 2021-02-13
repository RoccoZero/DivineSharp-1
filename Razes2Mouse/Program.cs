using Divine;
using Divine.Menu;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Managers.Update;

using SharpDX;
using System.Collections.Generic;

namespace Razes2Mouse
{
    public class SFRAZES : Bootstrapper
    {
        private MenuSwitcher DrawRazes;
        private MenuSwitcher Razes2Mouse;
        private MenuSlider ColorSwitcher;

        Color[] Colours = { Color.White, Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Pink, Color.Purple };

        protected override void OnActivate()
        {
            OrderManager.OrderAdding += OnUnitOrder;
            UpdateManager.Subscribe(8, OnDraw);
            var rootMenu = MenuManager.CreateRootMenu("SF HELPER");
            string[] Colours = { "White", "Red", "Green", "Blue" };
            DrawRazes = rootMenu.CreateSwitcher("Draw Shadowrazes");
            Razes2Mouse = rootMenu.CreateSwitcher("Shadowrazes to mouse direction");
            ColorSwitcher = rootMenu.CreateSlider("Colors", 0, 0, 6);
        }

        public static Vector3 InFront(Unit unit, float distance)
        {
            var alpha = unit.RotationRad;
            var vector2FromPolarAngle = SharpDXExtensions.FromPolarCoordinates(1f, alpha);

            var v = unit.Position + (vector2FromPolarAngle.ToVector3() * distance);
            return new Vector3(v.X, v.Y, 0);
        }

        private void OnDraw()
        {
            if (!DrawRazes.Value)
            {
                for (int i = 0; i < 3; i++)
                    ParticleManager.RemoveParticle($"DrawRaze_{i}");
                return;
            }
            Color Choosed = Colours[ColorSwitcher.Value];
            var localHero = EntityManager.LocalHero;
            if(localHero.Name != "npc_dota_hero_nevermore")
            {
                return;
            }
            var razes = new[] { 200, 450, 700 };
            for (int i = 0; i < 3; i++)
            {
                var inFront = InFront(localHero, razes[i]);

                ParticleManager.CreateOrUpdateParticle(
                    $"DrawRaze_{i}",
                    "materials/ensage_ui/particles/alert_range.vpcf",
                    localHero,
                    ParticleAttachment.AbsOrigin,
                    new ControlPoint(0, inFront),
                    new ControlPoint(1, Choosed),
                    new ControlPoint(2, 250, 255, 7));
            }
        }

        private void OnUnitOrder(OrderAddingEventArgs e)
        {
            if (!Razes2Mouse.Value)
            {
                return;
            }
            var localHero = EntityManager.LocalHero;
            if (localHero.Name != "npc_dota_hero_nevermore" || e.IsCustom)
            {
                return;
            }
            if (e.Order.Type == OrderType.Cast && e.Order.Ability.Id == AbilityId.nevermore_shadowraze1)
            {              
                localHero.MoveToDirection(GameManager.MousePosition);
                localHero.Spellbook.Spell1.Cast();
                e.Process = false;
                return;
            }
            if (e.Order.Type == OrderType.Cast && e.Order.Ability.Id == AbilityId.nevermore_shadowraze2)
            {
                localHero.MoveToDirection(GameManager.MousePosition);
                localHero.Spellbook.Spell2.Cast();
                e.Process = false;
                return;
            }
            if (e.Order.Type == OrderType.Cast && e.Order.Ability.Id == AbilityId.nevermore_shadowraze3)
            {
                localHero.MoveToDirection(GameManager.MousePosition);
                localHero.Spellbook.Spell3.Cast();
                e.Process = false;
                return;
            }
        }
    }
}
