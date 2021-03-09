﻿using Divine.Menu;
using Divine.Menu.EventArgs;
using Divine.Menu.Items;

namespace RockRubick
{
    internal sealed class Menu
    {
        private static MenuSwitcher RubickEnabled;

        public static void MenuBootstrap()
        {
            var rootmenu = MenuManager.CreateRootMenu("Rock.Rubick (BETA!)").SetHeroTexture(Divine.HeroId.npc_dota_hero_rubick);

            RubickEnabled = rootmenu.CreateSwitcher("Spell Stealer", false).SetAbilityTexture(Divine.AbilityId.rubick_spell_steal);

            RubickEnabled.ValueChanged += RubickEnabled_ValueChanged;
        }

        private static void RubickEnabled_ValueChanged(MenuSwitcher switcher, SwitcherEventArgs e)
        {
            if (e.Value)
            {
                new SpellStillMain();
            }
            else
            {
                SpellStillMain.Dispose();
            }
        }

    }
}
