using Divine;
using Divine.Menu;
using Divine.Menu.EventArgs;
using Divine.Menu.Items;
using Divine.SDK.Extensions;
using Divine.SDK.Helpers;
using Divine.SDK.Managers.Update;
using Divine.SDK.Orbwalker;
using SharpDX;
using System;
using System.Linq;
using System.Windows.Input;

namespace EarthSpirit
{
    class Earth_Spirit : Bootstrapper
    {
        private Hero myHero;

        private MenuSwitcher isEnable;
        private MenuHoldKey holdKey;
        private MenuSwitcher AutoStone;
        private MenuSlider autoUltiCount;

        private float stone_time;
        private float PlaceStoneTime;
        private float kick_time_delay_until;
        private float enchant_time;
        private float kick_time;
        private float roll_time;

        private bool IsIgnoreInput = false;

        private readonly Sleeper SleeperOrder = new Sleeper();
        private readonly Sleeper SleeperOrbWalker = new Sleeper();

        protected override void OnActivate()
        {
            // заново!!!! 
            if (EntityManager.LocalHero.HeroId != HeroId.npc_dota_hero_earth_spirit)
            {
                return;
            }

            var rootMenu = MenuManager.CreateRootMenu("AI Earth spirit");
            isEnable = rootMenu.CreateSwitcher("On/Off");
            isEnable.ValueChanged += isEnableChanged;

            holdKey = rootMenu.CreateHoldKey("Dynamic combo key", Key.None);
            AutoStone = rootMenu.CreateSwitcher("Auto Stone if W", false);
            autoUltiCount = rootMenu.CreateSlider("Enemyes for ult", 3, 0, 5).SetTooltip("If set to 0, it doesn't work");
        }

        private void isEnableChanged(MenuSwitcher switcher, SwitcherEventArgs e)
        {
            if (e.Value)
            {
                stone_time = 0;
                PlaceStoneTime = 0;
                kick_time_delay_until = 0;
                enchant_time = 0;
                kick_time = 0;
                roll_time = 0;

                myHero = EntityManager.LocalHero;

                GameManager.IngameUpdate += OnUpdate;
                InputManager.KeyUp += OnInputManagerKeyUpCtrl;
                InputManager.KeyDown += OnInputManagerKeyDownCtrl;
                InputManager.KeyUp += OnInputManagerKeyUpAlt;
                InputManager.KeyDown += OnInputManagerKeyDownAlt;
                OrderManager.OrderAdding += OnUnitOrder;
            }
            else
            {
                /* holdKey.IsHidden = true;
                 AutoStone.IsHidden = true;
                 autoUltiCount.IsHidden = true;*/

                GameManager.IngameUpdate -= OnUpdate;
                InputManager.KeyUp -= OnInputManagerKeyUpCtrl;
                InputManager.KeyDown -= OnInputManagerKeyDownCtrl;
                InputManager.KeyUp -= OnInputManagerKeyUpAlt;
                InputManager.KeyDown -= OnInputManagerKeyDownAlt;
                OrderManager.OrderAdding -= OnUnitOrder;
            }
        }

        private void OnInputManagerKeyDownAlt(KeyEventArgs e)
        {
            if (e.Key != Key.LeftAlt)
                return;
            IsIgnoreInput = true;
        }

        private void OnInputManagerKeyUpAlt(KeyEventArgs e)
        {
            if (e.Key != Key.LeftAlt)
                return;
            IsIgnoreInput = false;
        }

        private void OnInputManagerKeyDownCtrl(KeyEventArgs e)
        {
            if (e.Key != Key.LeftCtrl)
                return;
            IsIgnoreInput = true;
        }

        private void OnInputManagerKeyUpCtrl(KeyEventArgs e)
        {
            if (e.Key != Key.LeftCtrl)
                return;
            IsIgnoreInput = false;
        }
        private void OnUnitOrder(OrderAddingEventArgs e)
        {
            if (e.IsCustom || IsIgnoreInput || !isEnable)
            {
                e.Process = true;
                return;
            }
            Ability boulSmash = myHero.Spellbook.Spell1;
            Ability pull = myHero.Spellbook.Spell3;
            Ability roll = myHero.Spellbook.Spell2;
            Vector3 mousePos = GameManager.MousePosition;
            if (e.Order.Ability == boulSmash && !EntityManager.GetEntities<Entity>()
                                                              .Where(x => x.Name == "npc_dota_earth_spirit_stone" && x.IsAlive && myHero.Distance2D(x.Position) < 200)
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
                                                              .Where(x => x.Name == "npc_dota_earth_spirit_stone" && x.IsAlive && mousePos.Distance2D(x.Position) < 200)
                                                              .Any())
            {
                myHero.Spellbook.Spell4.Cast(mousePos);
                return;
            }
            if (AutoStone.Value && e.Order.Ability == roll && !roll.IsInAbilityPhase)
            {
                if (myHero.Distance2D(mousePos) > 1600)
                {
                    myHero.Stop();
                    myHero.Move(mousePos);
                    return;
                }
                if (!HasStoneBetween(myHero, myHero.Position, mousePos))
                {
                    myHero.MoveToDirection(mousePos);
                    Vector3 stonePos = myHero.Position.Extend(mousePos, 150);
                    SleeperOrder.Sleep(300);
                    UpdateManager.BeginInvoke(300, () =>
                    {
                        myHero.Spellbook.Spell4.Cast(stonePos);
                    });
                }
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
            tower = EntityManager.GetEntities<Tower>().Where(x => x.IsAlly(myHero) && x.Distance2D(myHero) <= 2000 && x.Distance2D(myHero) > 650)
                                       .OrderBy(x => x.Distance2D(myHero)).FirstOrDefault();
            if (tower != null)
                return tower;
            return null;
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

        private static bool IsCastOrChan(Hero hero)
        {
            if (hero.IsChanneling() || hero.Spellbook.MainSpells.Where(x => x.IsInAbilityPhase).Any())
                return true;
            return false;
        }
        private static Vector3 GetPredictedPosition(Hero Hero, float delay)
        {
            if (Hero == null)
                return default;
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

        private void PlaceRockInFront(Hero myHero)
        {
            if (myHero == null)
                return;
            Ability stone = myHero.Spellbook.GetSpellById(AbilityId.earth_spirit_stone_caller);
            if (stone != null && stone.IsValid && !SleeperOrder.Sleeping)
            {
                stone.Cast(InFront(myHero, 100));
                SleeperOrder.Sleep(70);
            }
        }

        private static bool HasStoneInRadius(Hero myHero, Vector3 pos, float radius)
        {
            if (pos == null || radius == 0) return false;

            bool stones = false;
            stones = EntityManager.GetEntities<Entity>().Where(x => x.Name == "npc_dota_earth_spirit_stone" && x.Distance2D(pos) < radius && x.IsAlive && x.IsValid && x.IsVisible).Any();
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

            for (int i = 0; i < num; i++)
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

            for (int i = 0; i < num; i++)
            {
                Vector3 mid = pos1 + dir * i;
                if (HasEnemyHeroInRadius(myHero, mid, radius))
                    return true;
            }
            return false;
        }

        private static bool HasTwoAlliesBehind(Hero myHero, Hero enemy)
        {
            if (enemy == null || myHero == null)
                return false;
            Vector3 postition = myHero.Position.Extend(enemy.Position, enemy.Distance2D(myHero) + 1700);
            Hero[] allies = EntityManager.GetEntities<Hero>().Where(x => x.IsAlly(myHero) && x.Distance2D(postition) < 700 && x.IsAlive).ToArray();
            if (allies != null && allies.Length > 1)
                return true;
            return false;
        }

        private static Hero AlliesBehind(Hero myHero, Hero enemy)
        {
            if (enemy == null || myHero == null)
                return null;
            Vector3 postition = myHero.Position.Extend(enemy.Position, enemy.Distance2D(myHero) + 1700);
            Hero allie = EntityManager.GetEntities<Hero>().Where(x => x.IsAlly(myHero) && x.Distance2D(postition) < 700 && x.IsAlive).FirstOrDefault();
            if (allie != null)
                return allie;
            return null;
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
                kick_time_delay_until += GameManager.GameTime + 0.35f;

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

            if (PlaceStoneTime != 0 && GameManager.GameTime > PlaceStoneTime && nearestHero != null)
            {
                PlaceStoneTime = 0;
                if (stone != null && stone_ready && myHero.Distance2D(nearestHero) >= 250)
                    if (holdKey)
                        PlaceRockInFront(myHero);
                    else if (stone != null && stone_ready && myHero.Distance2D(nearestHero) <= 250 && IsCastOrChan(nearestHero))
                        if (holdKey)
                            PlaceRockInFront(myHero);
            }

            roll_range = 800;
            float stone_roll_range = 1600;

            if (bonus_roll_range_learned)
            {
                roll_range += 400;
                stone_roll_range += 250;
            }

            // autoulti
            if (ult_ready && EntityManager.GetEntities<Hero>().Where(x => x.Distance2D(my_hero_pos) < 300 && x.IsEnemy(myHero) && x.IsAlive && x.IsVisible).Count() >= autoUltiCount.Value)
            {
                if (autoUltiCount >= 1 && !SleeperOrder.Sleeping)
                {
                    ult.Cast();
                    SleeperOrder.Sleep(70);
                }
            }

            // automatically kick enchanted enemy heroes into towers

            if (kick_ready && GetNearestAlliedTowerToMyHero(myHero) != null && GameManager.GameTime > enchant_time && enchant_time > 0 && GameManager.GameTime > kick_time)
            {
                enchant_time = 0;
                Tower nearest2HeroTower = GetNearestAlliedTowerToMyHero(myHero);
                if (nearest2HeroTower != null && nearestHero != null)
                {
                    Vector3 target = nearestHero.Position.Extend(nearest2HeroTower.Position, (nearestHero.Distance2D(nearest2HeroTower) / 4));
                    if (target != null && !SleeperOrder.Sleeping)
                    {
                        kick.Cast(target);
                        kick_time = GameManager.GameTime + 0.1f;
                        SleeperOrder.Sleep(70);
                    }
                }
            }

            // automatically kick enchanted enemy heroes into ally

            /*if (kick_ready && HasTwoAlliesBehind(myHero, nearestHero) == true && GameManager.GameTime > enchant_time && enchant_time > 0 && GameManager.GameTime > kick_time)
            {
                enchant_time = 0;
                if (HasTwoAlliesBehind(myHero, nearestHero) && !SleeperOrder.Sleeping)
                {
                    kick.Cast(nearestHero);
                    kick_time = GameManager.GameTime + 0.1f;
                    SleeperOrder.Sleep(70);
                }
            }*/

            if (holdKey && nearestHero != null)
            {
                if (pull_ready && roll_ready && HasStoneInRadius(myHero, nearestHero.Position, 300) && !SleeperOrder.Sleeping)
                {
                    Unit nearStone = EntityManager.GetEntities<Unit>().Where(x => x.Distance2D(nearestHero) < 300 && x.Name == "npc_dota_earth_spirit_stone").FirstOrDefault();
                    if (nearStone != null && nearStone.Distance2D(myHero) < 1100)
                    {
                        pull.Cast(nearStone.Position);
                        roll_time = GameManager.GameTime + 0.45f;
                    }
                }
                if (roll_ready && IsPositionInRange(myHero, nearestHero.Position, stone_roll_range) && !SleeperOrder.Sleeping && GameManager.GameTime > roll_time)
                {
                    roll_time = 0;
                    float distance2enemy = myHero.Distance2D(nearestHero.Position);
                    Vector3 enemyPos = GetPredictedPosition(nearestHero, (distance2enemy / 1800) + 0.6f);
                    roll.Cast(enemyPos);
                    SleeperOrder.Sleep(70);
                    kick_time = GameManager.GameTime + 1.25f;

                    if (!HasStoneBetween(myHero, my_hero_pos, nearestHero.Position))
                        PlaceStoneTime = GameManager.GameTime + 0.45f;
                }
                if (GameManager.GameTime > roll_time - 0.1f && !SleeperOrbWalker.Sleeping)
                {
                    OrbwalkerManager.OrbwalkTo(nearestHero);
                    SleeperOrbWalker.Sleep(50);
                }
            }

            bool inRange = EntityManager.GetEntities<Hero>().Where(x => !x.IsAlly(myHero) && x.Distance2D(myHero) < 2100).Any();
            if (inRange == true)
            {
                // automatically silence enemies with existing remnants
                if (pull_ready && holdKey)
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
                                    if (HasEnemyHeroBetween(myHero, my_hero_pos, stones.Position, 170) && !SleeperOrder.Sleeping)
                                    {
                                        pull.Cast(stones.Position);
                                        SleeperOrder.Sleep(70);
                                    }
                            }
                        }
                    }
                }

                //silence 
                if (pull_ready && nearestHero != null && stone_time < GameManager.GameTime)
                {
                    Hero enemy = nearestHero;
                    if (!HasStoneInRadius(enemy, enemy.Position, 500) && IsCastOrChan(enemy) && stone_ready && !SleeperOrder.Sleeping)
                    {
                        stone.Cast(enemy.Position);
                        stone_time = GameManager.GameTime + 0.4f;
                        pull.Cast(enemy.Position);
                        SleeperOrder.Sleep(70);
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
                            if (stone_ready && GameManager.GameTime > stone_time && !SleeperOrder.Sleeping)
                            {
                                stone.Cast(enemy.Position);
                                stone_time = GameManager.GameTime + 1;
                                SleeperOrder.Sleep(70);
                                return;
                            }
                    }
                }

                // automatically kick enemy heroes into your towers
                if (kick_ready && GameManager.GameTime > kick_time)
                {
                    Hero enemy = null;
                    Tower tower = null;
                    enemy = EntityManager.GetEntities<Hero>().Where(x => x.IsEnemy(myHero) && x.IsVisible && x.IsAlive && x.Distance2D(my_hero_pos) < 200).OrderBy(x => x.Distance2D(myHero)).FirstOrDefault();
                    tower = GetNearestAlliedTowerToMyHero(myHero);
                    if (enemy != null && tower != null)
                    {
                        Vector3 goodPoint = tower.Position.Extend(enemy.Position, tower.Distance2D(enemy) + 330);
                        if (enchant_ready && goodPoint != null && tower != null && myHero.Distance2D(goodPoint) < 60 && !SleeperOrder.Sleeping)
                        {
                            enchant.Cast(enemy);
                            enchant_time = GameManager.GameTime + 0.35f;
                            SleeperOrder.Sleep(70);
                        }
                        else if (myHero.Distance2D(goodPoint) < 120 && !SleeperOrder.Sleeping)
                        {
                            if (enchant_ready)
                            {
                                enchant.Cast(enemy);
                                enchant_time = GameManager.GameTime + 0.35f;
                                SleeperOrder.Sleep(70);
                            }
                            else
                            {
                                kick.Cast(enemy);
                                kick_time = GameManager.GameTime + 0.1f;
                                SleeperOrder.Sleep(70);
                            }
                        }
                    }
                }

                if (kick_ready && GameManager.GameTime > enchant_time && nearestHero != null && HasTwoAlliesBehind(myHero, nearestHero))
                {
                    if (myHero.Distance2D(nearestHero) < 200 && nearestHero.HasModifier("modifier_earthspirit_petrify") && !SleeperOrder.Sleeping)
                    {
                        kick.Cast(AlliesBehind(myHero, nearestHero));
                        SleeperOrder.Sleep(70);
                    }
                }

                if (kick_ready && GameManager.GameTime > enchant_time && GameManager.GameTime > kick_time)
                {
                    Hero enemy = null;
                    Hero hero = null;
                    enemy = EntityManager.GetEntities<Hero>().Where(x => x.IsEnemy(myHero) && x.Distance2D(my_hero_pos) < 200 && x.IsVisible && x.IsAlive).OrderBy(x => x.Distance2D(my_hero_pos)).FirstOrDefault();
                    hero = EntityManager.GetEntities<Hero>().Where(x => x.Position != my_hero_pos && x.HealthPercent() > 0.7 && x.IsAlly(myHero) && x.IsAlive && myHero.Distance2D(x) < 1200).OrderBy(x => x.Health).FirstOrDefault();

                    if (enemy != null && hero != null && GetNearestAlliedTowerToMyHero(myHero) != null && GetNearestAlliedTowerToMyHero(myHero).Distance2D(myHero) > 650 || enemy != null && hero != null && GetNearestAlliedTowerToMyHero(myHero) == null)
                    {
                        Vector3 goodPoint = hero.Position.Extend(enemy.Position, hero.Distance2D(enemy) + 180);
                        if (enchant_ready && myHero.Distance2D(goodPoint) < 60 && myHero.Distance2D(hero) > 800 && !SleeperOrder.Sleeping)
                        {
                            enchant.Cast(enemy);
                            enchant_time = GameManager.GameTime + 0.35f;
                            SleeperOrder.Sleep(70);
                        }
                        else if (myHero.Distance2D(goodPoint) < 80 && myHero.Distance2D(hero) > 350 && !SleeperOrder.Sleeping)
                        {
                            if (enchant_ready || enchant_ready)
                            {
                                enchant.Cast(enemy);
                                enchant_time = GameManager.GameTime + 0.35f;
                            }
                            else
                            {
                                kick.Cast(enemy);
                                kick_time = GameManager.GameTime + 0.1f;
                            }

                            SleeperOrder.Sleep(70);
                        }
                    }
                }

                // auto 1spell
                if (kick_ready && GameManager.GameTime > enchant_time && !roll.IsChanneling && !roll.IsInAbilityPhase && GameManager.GameTime > roll_time)
                {
                    roll_time = 0;
                    var enemyes = EntityManager.GetEntities<Hero>().Where(x => x.IsEnemy(myHero) && myHero.Distance2D(x.Position) < kick_end_range).OrderBy(x => myHero.Distance2D(x.Position));
                    foreach (var enemy in enemyes)
                    {
                        float distance = myHero.Distance2D(enemy);
                        if (!enemy.IsMoving && enemy.IsAlive && enemy.IsVisible
                            || distance <= 1200
                            && (enemy.Rotation
                                - myHero.Rotation) < 75
                            && (enemy.Rotation - myHero.Rotation) > -75
                            && enemy.IsMoving && enemy.IsAlive && enemy.IsVisible)
                        {
                            if (HasStoneInRadius(myHero, myHero.Position, 200) && GameManager.GameTime > kick_time && !SleeperOrder.Sleeping)
                            {
                                float speed = 900;
                                Vector3 prediction = GetPredictedPosition(enemy, distance / speed);
                                kick.Cast(prediction);
                                SleeperOrder.Sleep(70);
                            }
                            if (holdKey && !pull_ready)
                                return;
                            if (!SleeperOrder.Sleeping && holdKey && !HasStoneInRadius(myHero, myHero.Position, 200) && stone_ready && stone.Charges > 1 && GameManager.GameTime > stone_time && GameManager.GameTime > kick_time && nearestHero != null)
                            {
                                stone.Cast(myHero.Position.Extend(nearestHero.Position, 50));
                                stone_time = GameManager.GameTime + 1f;
                                SleeperOrder.Sleep(70);
                            }
                        }
                    }
                }
            }
        }
    }
}
