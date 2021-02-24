using Divine;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using SharpDX;

namespace RockHeroes.Modules
{
    class SfRazes
    {
        private MenuSwitcher DrawRazes;
        private MenuSwitcher Razes2Mouse;
        private MenuSlider ColorSwitcher;

        Color[] Colours = { Color.White, Color.Red, Color.Green, Color.Blue, Color.Yellow, Color.Pink, Color.Purple };

        public SfRazes(Context context)
        {
            if (EntityManager.LocalHero.HeroId != HeroId.npc_dota_hero_nevermore)
            {
                return;
            }

            var SfRazesMenu = context.rootMenu.CreateMenu("SF helper").SetHeroTexture(HeroId.npc_dota_hero_nevermore);
            Razes2Mouse = SfRazesMenu.CreateSwitcher("Shadowrazes to mouse direction");
            DrawRazes = SfRazesMenu.CreateSwitcher("Draw Shadowrazes");
            ColorSwitcher = SfRazesMenu.CreateSlider("Colors", 0, 0, 6);

            OrderManager.OrderAdding += OnUnitOrder;
            DrawRazes.ValueChanged += DrawRazes_ValueChanged;
        }

        private void DrawRazes_ValueChanged(MenuSwitcher switcher, Divine.Menu.EventArgs.SwitcherEventArgs e)
        {
            if (e.Value)
            {
                RendererManager.Draw += OnDraw;
            }
            else
            {
                RendererManager.Draw -= OnDraw;
                for (int i = 0; i < 3; i++)
                    ParticleManager.RemoveParticle($"DrawRaze_{i}");
                return;
            }

        }

        internal void Dispose()
        {
            //TODO
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
            Color Choosed = Colours[ColorSwitcher.Value];
            var localHero = EntityManager.LocalHero;
            if (localHero.Name != "npc_dota_hero_nevermore")
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
            if (!Razes2Mouse.Value) { return; }
            var localHero = EntityManager.LocalHero;
            Ability order = e.Order.Ability;
            if (localHero.Name != "npc_dota_hero_nevermore" || e.IsCustom) { return; }
            if (e.Order.Type == OrderType.Cast && (order.Id == AbilityId.nevermore_shadowraze1
                                                   || order.Id == AbilityId.nevermore_shadowraze2
                                                   || order.Id == AbilityId.nevermore_shadowraze3))
            {
                localHero.MoveToDirection(GameManager.MousePosition);
                order.Cast();
                e.Process = true;
            }
        }
    }
}
