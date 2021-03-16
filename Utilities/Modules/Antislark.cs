using Divine;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using SharpDX;
using System.Linq;

namespace Utilities.Modules
{
    internal sealed class Antislark
    {
        private Vector3 PosOfPounce;
        private Vector3 PosForBranch;
        private float leashradius;

        private readonly Hero localHero;

        private readonly MenuSwitcher enableAS;
        private readonly MenuSwitcher useIB;
        private readonly MenuSwitcher useHW;
        private readonly MenuSwitcher useIWT;

        public Antislark(Context context)
        {
            localHero = EntityManager.LocalHero;

            ParticleManager.ParticleAdded += ParticleManager_ParticleAdded;
            UpdateManager.CreateIngameUpdate(140, GameManager_IngameUpdate);

            var AntislarkMenu = context.rootMenu.CreateMenu("AntiPounce");
            enableAS = AntislarkMenu.CreateSwitcher("Enabled/Disabled", false);
            useIB = AntislarkMenu.CreateSwitcher("Use IRONBRANCH", false);
            useHW = AntislarkMenu.CreateSwitcher("Use ACORN SHOT", false);
            useIWT = AntislarkMenu.CreateSwitcher("Use IRONWOOD TREE", false);
        }

        internal static void Dispose()
        {
            //TODO
        }

        private int GetReason()
        {
            if (useIWT.Value == true
                && localHero.Inventory.NeutralItem != null
                && localHero.Inventory.NeutralItem.Id == AbilityId.item_ironwood_tree
                && localHero.Inventory.NeutralItem.Cooldown == 0)
                return 1;
            if (useIB.Value == true
                && localHero.Inventory.MainItems.Any(x => x.Id == AbilityId.item_branches))
                return 2;
            if (localHero.Name == "npc_dota_hero_hoodwink"
                && useHW.Value == true
                && localHero.Spellbook.Spell1.Level > 0
                && localHero.Spellbook.Spell1.Cooldown == 0)
                return 3;
            return 0;
        }

        private void GameManager_IngameUpdate()
        {
            if (enableAS.Value == true && localHero.HasModifier("modifier_slark_pounce_leash") && localHero.Distance2D(PosOfPounce) < leashradius && localHero.Distance2D(PosOfPounce) > leashradius - 16)
            {
                PosForBranch = localHero.Position.Extend(PosOfPounce, 16);
                if (GetReason() == 0) return;
                if (GetReason() == 1) { localHero.Inventory.NeutralItem.Cast(PosForBranch); return; }
                if (GetReason() == 2) { localHero.Inventory.MainItems.First(x => x.Id == AbilityId.item_branches).Cast(PosForBranch); return; }
                else { localHero.Spellbook.Spell1.Cast(PosForBranch); return; }
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

