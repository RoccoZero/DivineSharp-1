using Divine;
using Divine.Menu;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Managers.Update;
using SharpDX;

using System.Linq;
using System.Threading.Tasks;

namespace AntiSlark
{
    class CAntiSlark : Bootstrapper
    {
        private Vector3 PosOfPounce;
        private float leashradius;
        private Vector3 PosForBranch;

        private Hero localHero;

        private MenuSwitcher enableAS;
        private MenuSwitcher useIB;
        private MenuSwitcher useHW;
        private MenuSwitcher useIWT;

        protected override void OnActivate()
        {
            ParticleManager.ParticleAdded += ParticleManager_ParticleAdded;
            UpdateManager.Subscribe(50, GameManager_IngameUpdate);

            var rootMenu = MenuManager.CreateRootMenu("Utility");
            var antiPounceRoot = rootMenu.CreateMenu("AntiPounce");
            enableAS = antiPounceRoot.CreateSwitcher("Enable/Disable" , false);
            useIB = antiPounceRoot.CreateSwitcher("Use IRONBRANCH", false);
            useHW = antiPounceRoot.CreateSwitcher("Use ACORN SHOT", false);
            useIWT = antiPounceRoot.CreateSwitcher("Use IRONWOOD TREE", false);

            localHero = EntityManager.LocalHero;
        }

        private async void GameManager_IngameUpdate()
        {
            if (enableAS.Value == true && localHero.HasModifier("modifier_slark_pounce_leash") && localHero.Distance2D(PosOfPounce) < leashradius + 10 && localHero.Distance2D(PosOfPounce) > leashradius - 10)
            {
                PosForBranch = localHero.Position.Extend(PosOfPounce, 15);
                if (useIWT.Value == true && localHero.Inventory.NeutralItem != null && localHero.Inventory.NeutralItem.Id == AbilityId.item_ironwood_tree && localHero.Inventory.NeutralItem.Cooldown == 0)
                    localHero.Inventory.NeutralItem.Cast(PosForBranch);
                await Task.Delay(25); 
                if (useIB.Value == true && localHero.HasModifier("modifier_slark_pounce_leash") && localHero.Inventory.MainItems.Any(x => x.Id == AbilityId.item_branches))
                    localHero.Inventory.MainItems.First(x => x.Id == AbilityId.item_branches).Cast(PosForBranch);
                await Task.Delay(50);
                if (localHero.Name == "npc_dota_hero_hoodwink" && useHW.Value == true && localHero.HasModifier("modifier_slark_pounce_leash") && localHero.Spellbook.Spell1.Level > 0 && localHero.Spellbook.Spell1.Cooldown == 0)
                    localHero.Spellbook.Spell1.Cast(PosForBranch);

            }
        }

        private void ParticleManager_ParticleAdded(ParticleAddedEventArgs e)
        {
            if (e.Particle.Name != "particles/units/heroes/hero_slark/slark_pounce_ground.vpcf")
                return;
            UpdateManager.BeginInvoke(() =>
            {
                PosOfPounce = e.Particle.GetControlPoint(3);
                leashradius = Ability.GetAbilityDataById(AbilityId.slark_pounce).AbilitySpecialData.FirstOrDefault(x => x.Name == "leash_radius").Value;
            });
        }
    }
}
