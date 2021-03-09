﻿using Divine;
using Divine.SDK.Extensions;

namespace RockRubick
{
    internal sealed class SpellStealMain
    {
        public SpellStealMain() //Если свитчер переводят в состояние On
        {
            new Dictionaries();
            new LastSpellManager();
            new CooldownManager();
            //new ConsoleWriter();

            UpdateManager.CreateIngameUpdate(25, IngameUpdate);
        }
        public static void Dispose() //Если свитчер переводят в состояние Off
        {
            LastSpellManager.Dispose();
            CooldownManager.Dispose();

            UpdateManager.DestroyIngameUpdate(IngameUpdate);
        }

        private static void IngameUpdate() //использование необходимых классов
        {
            if (General.localHero.HasAghanimsScepter())
            {
                new AghanimMode();
            }
            else
            {
                new NonAghanimMode();
            }
        }
    }
}
