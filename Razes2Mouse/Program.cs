using Divine;
using Divine.Menu;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Managers.Update;

using SharpDX;

namespace Razes2Mouse
{
    public class SFRAZES : Bootstrapper
    {
        private MenuSwitcher DrawRazes;

        private MenuSwitcher Razes2Mouse;
        protected override void OnActivate()
        {
            OrderManager.OrderAdding += OnUnitOrder;
            GameManager.IngameUpdate += OnUpdate;
            var rootMenu = MenuManager.CreateRootMenu("SF HELPER");
            DrawRazes = rootMenu.CreateSwitcher("Draw Shadowrazes");
            Razes2Mouse = rootMenu.CreateSwitcher("Shadowrazes to mouse direction");
        }

        private void OnUpdate()
        {
            if (!DrawRazes.Value)
            {
                for (int i = 0; i < 3; i++)
                    ParticleManager.RemoveParticle($"DrawRaze_{i}");
                return;
            }
            var localHero = EntityManager.LocalHero;
            var razes = new[] { 200, 450, 700 };
            for (int i = 0; i < 3; i++)
            {
                var inFront = localHero.InFront(razes[i]);

                ParticleManager.CreateOrUpdateParticle(
                    $"DrawRaze_{i}",
                    "materials/ensage_ui/particles/alert_range.vpcf",
                    localHero,
                    ParticleAttachment.AbsOrigin,
                    new ControlPoint(0, inFront),
                    new ControlPoint(1, new Color(255,255,255)),
                    new ControlPoint(2, 200, 255, 40));
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
            }
            if (e.Order.Type == OrderType.Cast && e.Order.Ability.Id == AbilityId.nevermore_shadowraze2)
            {
                localHero.MoveToDirection(GameManager.MousePosition);
                localHero.Spellbook.Spell2.Cast();
            }
            if (e.Order.Type == OrderType.Cast && e.Order.Ability.Id == AbilityId.nevermore_shadowraze3)
            {
                localHero.MoveToDirection(GameManager.MousePosition);
                localHero.Spellbook.Spell3.Cast();
            }
        }
    }
}
