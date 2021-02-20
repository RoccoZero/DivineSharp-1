using Divine;
using Divine.Menu;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Managers.Update;
using SharpDX;
using System;
using System.Linq;
using System.Windows.Input;

namespace EarthSpirit
{
    class Earth_Spirit : Bootstrapper
    {
        private Hero myHero;


        private MenuHoldKey holdKey;
        private MenuSwitcher AutoStone;
        private bool isComboKeyPressed;

        private float stone_time;
        private float PlaceStoneTime;
        private float kick_time_delay_until;
        private float enchant_time;
        private float kick_time;

        private bool IsCtrlKey;

        protected override void OnActivate()
        {
            stone_time = 0;
            PlaceStoneTime = 0;
            kick_time_delay_until = 0;
            enchant_time = 0;
            kick_time = 0;
            myHero = EntityManager.LocalHero;
            if (myHero.HeroId == HeroId.npc_dota_hero_earth_spirit)
            {
                var rootMenu = MenuManager.CreateRootMenu("AI Earth spirit");
                AutoStone = rootMenu.CreateSwitcher("Auto Stone if W", false);
                GameManager.IngameUpdate += OnUpdate;
                InputManager.KeyDown += OnInputManagerKeyDown;
                InputManager.KeyUp += OnInputManagerKeyUp;
                OrderManager.OrderAdding += OnUnitOrder;
                holdKey = rootMenu.CreateHoldKey("Combo key", Key.None);
                holdKey.ValueChanged += (sender, e) => isComboKeyPressed = e.Value;
            }
        }

        private void OnInputManagerKeyDown(KeyEventArgs e)
        {
            if (e.Key != Key.LeftCtrl)
                return;
            IsCtrlKey = true;
        }

        private void OnInputManagerKeyUp(KeyEventArgs e)
        {
            if (e.Key != Key.LeftCtrl)
                return;
            IsCtrlKey = false;
        }

        private void OnUnitOrder(OrderAddingEventArgs e)
        {
            if (e.IsCustom || IsCtrlKey == true) return;
            Ability boulSmash = myHero.Spellbook.Spell1;
            Ability pull = myHero.Spellbook.Spell3;
            Ability roll = myHero.Spellbook.Spell2;
            Vector3 mousePos = GameManager.MousePosition;
            if (e.Order.Ability == boulSmash && !EntityManager.GetEntities<Entity>()
                                                              .Where(x => x.Name == "npc_dota_earth_spirit_stone" && myHero.Distance2D(x.Position) < 200)
                                                              .Any())
            {
                bool isIn = false;
                isIn = EntityManager.GetEntities<Hero>().Where(x => x.IsEnemy(myHero) && x.Distance2D(myHero) < 200).Any();
                if (isIn)
                {
                    myHero.Spellbook.Spell1.Cast(mousePos);
                    return;
                }
                var extendet = myHero.Position.Extend(mousePos, 100);
                myHero.Spellbook.Spell4.Cast(extendet);
                return;
            }
            if (e.Order.Ability == pull && !EntityManager.GetEntities<Entity>()
                                                              .Where(x => x.Name == "npc_dota_earth_spirit_stone" && mousePos.Distance2D(x.Position) < 200)
                                                              .Any())
            {
                myHero.Spellbook.Spell4.Cast(mousePos);
                return;
            }
            if (AutoStone.Value && e.Order.Ability == roll)
            {
                if (!HasStoneBetween(myHero, myHero.Position, mousePos))
                    PlaceRockInFront(myHero);
                return;
            }
            e.Process = true;
            return;
        }

        private static bool IsCastable(float manaCost, float manaPool)
        {
            if (manaPool - manaCost > 0)
                return true;
            return false;
        }

        private static Tower GetNearestAlliedTowerToMyHero(Hero myHero)
        {
            Tower tower;
            tower = EntityManager.GetEntities<Tower>().Where(x => x.IsAlly(myHero) && x.Distance2D(myHero) < 2000)
                                       .OrderBy(x => x.Distance2D(myHero)).FirstOrDefault();
            if (tower != null)
                return tower;
            return default;
        }

        private static Hero GetNearestHeroToCursor()
        {
            Hero hero = null;
            hero = EntityManager.GetEntities<Hero>()
                                           .Where(x => !x.IsAlly(EntityManager.LocalHero) && x.IsAlive && x.IsVisible && x.IsValid && !x.IsIllusion && x.Distance2D(GameManager.MousePosition) < 800)
                                           .OrderBy(x => x.Distance2D(GameManager.MousePosition)).FirstOrDefault();
            if (hero != default || hero != null)
                return hero;
            return null;
        }

        private static bool CantMove(Hero hero)
        {
            if (hero == null)
                return false;
            if (hero.UnitState == UnitState.Hexed
                || hero.UnitState == UnitState.Rooted
                || hero.UnitState == UnitState.Stunned
                || hero.HasModifier("modifier_legion_commander_duel")
                || hero.HasModifier("modifier_axe_berserkers_call")
                || hero.HasModifier("modifier_faceless_void_chronosphere_freeze")
                || hero.HasModifier("modifier_bashed"))
                return true;
            return false;
        }
        private static Vector3 GetPredictedPosition(Hero Hero, float delay)
        {
            Vector3 pos = Hero.Position;
            if (CantMove(Hero) || !Hero.IsMoving || delay == 0) return pos;
            float speed = GetMS(Hero);
            return pos + (Vector3)SharpDXExtensions.FromPolarCoordinates(1f, Hero.RotationRad) * speed * delay;
        }

        private static bool IsPositionInRange(Hero hero, Vector3 pos, float radius)
        {
            if (hero.Distance2D(pos) <= radius)
                return true;
            return false;
        }

        private static Vector3 InFront(Unit unit, float distance)
        {
            var alpha = unit.RotationRad;
            var vector2FromPolarAngle = SharpDXExtensions.FromPolarCoordinates(1f, alpha);

            var v = unit.Position + (vector2FromPolarAngle.ToVector3() * distance);
            return new Vector3(v.X, v.Y, 0);
        }

        private static float GetMS(Hero enemy)
        {
            if (enemy != null)
                return enemy.MovementSpeed;
            float base_speed = enemy.BaseMovementSpeed;
            float bonus_speed = enemy.MovementSpeed - enemy.BaseMovementSpeed;

            Modifier modifierHex = null;
            Modifier modSheep = enemy.GetModifierByName("modifier_sheepstick_debuff");
            Modifier modLionVoodoo = enemy.GetModifierByName("modifier_lion_voodoo");
            Modifier modShamanVoodoo = enemy.GetModifierByName("modifier_shadow_shaman_voodoo");

            if (modSheep != null)
                modifierHex = modSheep;
            if (modLionVoodoo != null)
                modifierHex = modLionVoodoo;
            if (modShamanVoodoo != null)
                modifierHex = modShamanVoodoo;

            if (modifierHex != null)
                if (Math.Max(modifierHex.DieTime - GameManager.GameTime, 0) > 0)
                    return 140 + bonus_speed;

            if (enemy.HasModifier("modifier_invoker_ice_wall_slow_debuff"))
                return 100;
            if (enemy.HasModifier("modifier_invoker_cold_snap_freeze") || enemy.HasModifier("modifier_invoker_cold_snap"))
                return (base_speed + bonus_speed) * 0.5f;

            return base_speed + bonus_speed;
        }

        private static void PlaceRockInFront(Hero myHero)
        {
            if (myHero == null)
                return;
            Ability stone = myHero.Spellbook.GetSpellById(AbilityId.earth_spirit_stone_caller);
            if (stone != null && stone.IsValid)
                stone.Cast(InFront(myHero, 100));
        }

        private static bool HasStoneInRadius(Hero myHero, Vector3 pos, float radius)
        {
            if (pos == null || radius == 0) return false;

            bool stones = false;
            stones = EntityManager.GetEntities<Entity>().Where(x => x.Name == "npc_dota_earth_spirit_stone" && x.Distance2D(pos) < radius).Any();
            if (stones == true)
                return true;
            return false;
        }

        private static bool HasStoneBetween(Hero myHero, Vector3 pos1, Vector3 pos2)
        {
            if (myHero == null || pos1 == null || pos2 == null)
                return false;

            float radius = 150;
            float dis = pos1.Distance2D(pos2);
            int num = (int)Math.Floor(dis / radius);

            for (int i = 0; i < num + 1; i++)
            {
                Vector3 mid = pos1.Extend(pos2, radius * i);
                if (HasStoneInRadius(myHero, mid, radius))
                    return true;
            }
            return false;
        }

        private static bool HasEnemyHeroInRadius(Hero myHero, Vector3 pos, float radius)
        {
            if (pos == null || radius == 0) return false;

            bool enemy;
            enemy = EntityManager.GetEntities<Hero>().Where(x => x.Distance2D(pos) < radius && !x.IsAlly(myHero)).Any();
            if (enemy == true)
                return true;
            return false;
        }
        private static bool HasEnemyHeroBetween(Hero myHero, Vector3 pos1, Vector3 pos2, float radius)
        {
            if (myHero == null || pos1 == null || pos2 == null)
                return false;

            Vector3 dir = (pos2 - pos1).Normalized() * radius;
            float dis = pos1.Distance2D(pos2);
            int num = (int)Math.Floor(dis / radius);

            for (int i = 0; i < num + 1; i++)
            {
                Vector3 mid = pos1 + dir * i;
                if (HasEnemyHeroInRadius(myHero, mid, radius))
                    return true;
            }
            return false;
        }
        private void OnUpdate()
        {
            float myMana = myHero.Mana;
            bool AghanimsBuffed = false;
            if (myHero.HasAghanimsScepter())
                AghanimsBuffed = true;


            Ability kick = myHero.Spellbook.GetSpellById(AbilityId.earth_spirit_boulder_smash);
            Ability pull = myHero.Spellbook.GetSpellById(AbilityId.earth_spirit_geomagnetic_grip);
            Ability roll = myHero.Spellbook.GetSpellById(AbilityId.earth_spirit_rolling_boulder);
            Ability ult = myHero.Spellbook.GetSpellById(AbilityId.earth_spirit_magnetize);
            Ability enchant = myHero.Spellbook.GetSpellById(AbilityId.earth_spirit_petrify);

            if (roll.IsInAbilityPhase)
                kick_time_delay_until += GameManager.GameTime + 1;

            Ability stone = myHero.Spellbook.GetSpellById(AbilityId.earth_spirit_stone_caller);
            Ability bonus_talent_roll_range = myHero.Spellbook.GetSpellById(AbilityId.special_bonus_unique_earth_spirit_4);

            float kick_start_range = kick.GetCastRange();
            float kick_end_range = 2000;
            float pull_range = pull.GetCastRange();
            float roll_range = roll.GetCastRange();
            //float stone_range = 1100;
            float enchant_range = 0;

            if (AghanimsBuffed)
                enchant_range = enchant.GetCastRange();

            bool kick_ready = kick.Level > 0 && kick.Cooldown == 0 && IsCastable(kick.ManaCost, myMana);
            bool pull_ready = pull.Level > 0 && pull.Cooldown == 0 && IsCastable(pull.ManaCost, myMana);
            bool roll_ready = roll.Level > 0 && roll.Cooldown == 0 && IsCastable(roll.ManaCost, myMana);
            bool ult_ready = ult.Level > 0 && ult.Cooldown == 0 && IsCastable(ult.ManaCost, myMana);
            bool stone_ready = stone.Level > 0 && stone.Cooldown == 0 && IsCastable(stone.ManaCost, myMana);
            bool bonus_roll_range_learned = bonus_talent_roll_range.IsValid && bonus_talent_roll_range.Level > 0;
            bool enchant_ready = enchant.IsValid && enchant.Level > 0 && enchant.Cooldown == 0 && IsCastable(enchant.ManaCost, myMana);

            bool lens = myHero.Inventory.MainItems.Where(x => x.Id == AbilityId.item_aether_lens).Any();

            Hero nearestHero = GetNearestHeroToCursor();
            Vector3 my_hero_pos = myHero.Position;

            if (PlaceStoneTime != 0 && GameManager.GameTime > PlaceStoneTime)
            {
                PlaceStoneTime = 0;
                if (stone != null && stone_ready)
                    if (isComboKeyPressed)
                        PlaceRockInFront(myHero);
            }

            roll_range = 800;
            float stone_roll_range = 1600;

            if (bonus_roll_range_learned)
            {
                roll_range += 400;
                stone_roll_range += 250;
            }

            // automatically kick enchanted enemy heroes into towers
            // не делай со

            if (kick_ready && GameManager.GameTime > enchant_time && enchant_time > 0 && GameManager.GameTime > kick_time)
            {
                enchant_time = 0;
                Tower nearest2HeroTower = GetNearestAlliedTowerToMyHero(myHero);
                if (nearest2HeroTower != null || nearest2HeroTower != default)
                {
                    Vector3 target = nearestHero.Position.Extend(nearest2HeroTower.Position, (nearestHero.Distance2D(nearest2HeroTower) / 4));
                    kick.Cast(target);
                    kick_time = GameManager.GameTime + 0.1f;
                }
            }

            if (isComboKeyPressed && nearestHero != null)
                if (roll_ready && IsPositionInRange(myHero, nearestHero.Position, stone_roll_range))
                {
                    float distance2enemy = myHero.Distance2D(nearestHero.Position);
                    Vector3 enemyPos = GetPredictedPosition(nearestHero, (distance2enemy / 1800) + 0.6f);
                    roll.Cast(enemyPos);

                    if (!HasStoneBetween(myHero, my_hero_pos, nearestHero.Position))
                        PlaceStoneTime = GameManager.GameTime + 0.45f;
                }

            bool inRange = EntityManager.GetEntities<Hero>().Where(x => !x.IsAlly(myHero) && x.Distance2D(myHero) < 2100).Any();
            if (inRange == true)
            {
                // automatically silence enemies with existing remnants
                if (pull_ready)
                {
                    Hero[] enemyes = null;
                    enemyes = EntityManager.GetEntities<Hero>()
                        .Where(x => !x.IsAlly(myHero) && x.IsAlive && x.Distance2D(myHero) < 1100).ToArray();
                    if (enemyes != null)
                    {
                        foreach (var enemy in enemyes)
                        {
                            if (!enemy.IsAlive)
                                continue;
                            var unitsAround = EntityManager.GetEntities<Entity>().Where(x => x.Distance2D(myHero) < 1100 && x.IsAlive && x.IsAlly(myHero));
                            foreach (var stones in unitsAround)
                            {
                                if (stones != null && stones.Name == "npc_dota_earth_spirit_stone")
                                    if (HasEnemyHeroBetween(myHero, my_hero_pos, stones.Position, 150))
                                        pull.Cast(stones.Position);
                            }
                        }

                    }
                }

                // automatically refresh ult with stones at the last possible moment
                if (stone_ready)
                {
                    Hero[] heroesInStoneRange = EntityManager.GetEntities<Hero>()
                                                                  .Where(x => !x.IsAlly(myHero)
                                                                  && x.HasModifier("modifier_earth_spirit_magnetize")
                                                                  && x.Distance2D(myHero) < 1100).ToArray();

                    foreach (var enemy in heroesInStoneRange)
                    {
                        float dieTime = enemy.GetModifierByName("modifier_earth_spirit_magnetize").DieTime;
                        if (dieTime - GameManager.GameTime < 2 && dieTime - GameManager.GameTime > 0.4)
                            if (stone_ready && GameManager.GameTime > stone_time)
                            {
                                stone.Cast(enemy.Position);
                                stone_time = GameManager.GameTime + 1;
                                return;
                            }
                    }
                }

                // automatically kick enemy heroes into your towers
                if (kick_ready && GameManager.GameTime > enchant_time && GameManager.GameTime > kick_time)
                {
                    Hero enemy = null;
                    Tower tower = null;
                    enemy = EntityManager.GetEntities<Hero>().Where(x => x.IsEnemy(myHero) && x.IsVisible && x.IsValid && x.Distance2D(my_hero_pos) < 200).OrderBy(x => x.Distance2D(myHero)).FirstOrDefault();
                    tower = GetNearestAlliedTowerToMyHero(myHero);
                    if (tower != null && enemy != null)
                    {
                        Vector3 goodPoint = tower.Position.Extend(enemy.Position, tower.Distance2D(enemy) + 330);
                        if (enchant_ready && myHero.Distance2D(goodPoint) < 60)
                        {
                            enchant.Cast(enemy);
                            enchant_time = GameManager.GameTime + 0.35f;
                        }
                        else if (myHero.Distance2D(goodPoint) < 100)
                        {
                            if (enchant_ready)
                            {
                                enchant.Cast(enemy);
                                enchant_time = GameManager.GameTime + 0.35f;
                            }
                            else kick.Cast(enemy);
                            {
                                kick.Cast(enemy);
                                kick_time = GameManager.GameTime + 0.1f;
                            }
                        }
                    }
                }

                if (kick_ready && GameManager.GameTime > enchant_time && GameManager.GameTime > kick_time)
                {
                    Hero enemy = null;
                    Hero hero = null;
                    enemy = EntityManager.GetEntities<Hero>().Where(x => x.IsEnemy(myHero) && x.Distance2D(my_hero_pos) < 200 && x.IsVisible && x.IsAlive).OrderBy(x => x.Distance2D(my_hero_pos)).FirstOrDefault();
                    hero = EntityManager.GetEntities<Hero>().Where(x => x.Position != my_hero_pos && x.HealthPercent() > 0.7 && x.IsAlly(myHero) && x.IsAlive && myHero.Distance2D(x) < 1200).OrderByDescending(x => x.Health).FirstOrDefault();

                    if (enemy != null && hero != null)
                    {
                        Vector3 goodPoint = hero.Position.Extend(enemy.Position, hero.Distance2D(enemy) + 180);
                        if (enchant_ready && myHero.Distance2D(goodPoint) < 60 && myHero.Distance2D(hero) > 800)
                        {
                            enchant.Cast(enemy);
                            enchant_time = GameManager.GameTime + 0.35f;
                        }
                        else if (myHero.Distance2D(goodPoint) < 80 && myHero.Distance2D(hero) > 350)
                        {
                            if (enchant_ready)
                            {
                                enchant.Cast(enemy);
                                enchant_time = GameManager.GameTime + 0.35f;
                            }
                            else
                            {
                                kick.Cast(enemy);
                                kick_time = GameManager.GameTime + 0.1f;
                            }
                        }
                    }
                }

                // auto 1spell
                if (kick_ready && GameManager.GameTime > kick_time_delay_until && GameManager.GameTime > enchant_time)
                {
                    var enemyes = EntityManager.GetEntities<Hero>().Where(x => x.IsEnemy(myHero) && myHero.Distance2D(x.Position) < kick_end_range);
                    foreach (var enemy in enemyes)
                    {
                        float distance = myHero.Distance2D(enemy);
                        if (!enemy.IsMoving
                            && enemy.HealthPercent() < 0.5f
                            || distance <= 1200
                            && (enemy.Rotation - myHero.Rotation) < 75
                            && (enemy.Rotation - myHero.Rotation) > -75
                            && enemy.IsMoving
                            && GetMS(enemy) > myHero.MovementSpeed - 25)
                            if (HasStoneInRadius(myHero, myHero.Position, 200))
                            {
                                float speed = 900;
                                Vector3 prediction = GetPredictedPosition(enemy, distance / speed);
                                kick.Cast(prediction);
                            }
                    }
                }
            }
        }
    }
}
