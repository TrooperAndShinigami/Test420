using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using SharpDX;
using Color = System.Drawing.Color;

namespace NoobRenekton
{
    class Program
    {
        private static Obj_AI_Hero Player = ObjectManager.Player;

        private static readonly Obj_AI_Hero[] AllEnemy = HeroManager.Enemies.ToArray();
        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R;

        private static Items.Item tiamat, hydra, titanic;
        private static bool IsEUsed => Player.HasBuff("renektonsliceanddicedelay");

        private static bool IsWUsed => Player.HasBuff("renektonpreexecute");

        private static Menu _menu;

        private static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }

        /// <summary>
        /// Game Loaded Method
        /// </summary>
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Renekton") // check if the current champion is Jax
                return; // stop programm

            //Set Spells
            Q = new Spell(SpellSlot.Q, 325);
            W = new Spell(SpellSlot.W, Orbwalking.GetRealAutoAttackRange(Player));
            E = new Spell(SpellSlot.E, 450);
            R = new Spell(SpellSlot.R);

            //Set Spells
            hydra = new Items.Item(3074, 185);
            tiamat = new Items.Item(3077, 185);
            titanic = new Items.Item(3748, 450);

            // create _menu
            _menu = new Menu("Noob" + Player.ChampionName, "Noob" + Player.ChampionName, true);

            Menu orbwalkerMenu = _menu.AddSubMenu(new Menu("Orbwalker", "Orbwalker"));
            Orbwalker = new Orbwalking.Orbwalker(orbwalkerMenu);

            // create TargetSelector
            Menu ts = _menu.AddSubMenu(new Menu("Target Selector", "Target Selector"));

            // attach
            TargetSelector.AddToMenu(ts);

            //Combo-_menu
            Menu comboMenu = new Menu("Combo", "Combo");
            comboMenu.AddItem(new MenuItem("useQ", "Use Q").SetValue(true));          
            comboMenu.AddItem(new MenuItem("useW", "Use W").SetValue(true));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));
            comboMenu.AddItem(new MenuItem("useR", "Use R").SetValue(true));
            comboMenu.AddItem(new MenuItem("comboSliderR", "Use Auto R at Health (%)").SetValue(new Slider(15, 1, 100)));
            _menu.AddSubMenu(comboMenu);

            //Jungleclear-_menu
            Menu jungleclear = new Menu("Jungleclear", "Jungleclear");
            jungleclear.AddItem(new MenuItem("jungleclearQ", "Use Q to JungleClear").SetValue(true));
            jungleclear.AddItem(new MenuItem("jungleclearW", "Use W to JungleClear").SetValue(true));
            _menu.AddSubMenu(jungleclear);

            //KS-_menu
            Menu ksMenu = new Menu("Killsteal", "Killsteal");
            ksMenu.AddItem(new MenuItem("Killsteal", "Killsteal with Q").SetValue(true));
            _menu.AddSubMenu(ksMenu);

            //Drawings-_menu
            Menu drawingsMenu = new Menu("Drawings", "Drawings");
            drawingsMenu.AddItem(new MenuItem("drawE", "Draw E range").SetValue(false));
            _menu.AddSubMenu(drawingsMenu);

            _menu.AddToMainMenu();

            OnDoCast();           
            Game.OnUpdate += OnUpdate;
            Drawing.OnDraw += OnDraw;
            Orbwalking.OnAttack += OnAa;
            Game.PrintChat("NoobRenekton by 1Shinigamix3");
        }
        private static void OnUpdate(EventArgs args)
        {
            if (Player.IsDead || Player.IsRecalling())
            {
                return;
            }
            if (_menu.Item("Killsteal").GetValue<bool>())
            {
                Killsteal();
            }
            Combo();
            Harass();
            if (Player.HealthPercent < _menu.Item("comboSliderR").GetValue<Slider>().Value && R.IsReady())
                R.Cast();
        }
        private static void OnDraw(EventArgs args)
        {
            if (_menu.Item("drawE").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, E.Range,
                    Color.Tan);
            }
        }
        private static void Combo()
        {
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;
            Obj_AI_Hero target = TargetSelector.GetTarget(450, TargetSelector.DamageType.Physical);

            // ACTUAL COMBO
            if (target != null && !target.IsZombie && !target.IsInvulnerable)
            {
                if (E.IsReady() && (_menu.Item("useE").GetValue<bool>()))
                {
                    E.Cast(target);
                }
                if (Q.IsReady() && (_menu.Item("useQ").GetValue<bool>()) && !W.IsReady() && !IsWUsed)
                {
                    Q.Cast();
                }
            }
        }

        private static void Harass()
        {
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Mixed)
                return;
            Obj_AI_Hero target = TargetSelector.GetTarget(325, TargetSelector.DamageType.Physical);

            if (Q.IsReady() && !W.IsReady() && !IsWUsed)
            {
                Q.Cast(target);
            }
        }

        private static void OnDoCast()
        {
            Obj_AI_Base.OnDoCast += (sender, args) =>
            {
                if (sender.IsMe && args.SData.IsAutoAttack())
                {
                    Obj_AI_Hero target = TargetSelector.GetTarget(300, TargetSelector.DamageType.Physical);
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    {
                        if (_menu.Item("useW").GetValue<bool>() && W.IsReady()) W.Cast();

                        if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo)
                        {
                            if (hydra.IsOwned() && Player.Distance(target) < hydra.Range && hydra.IsReady() && !W.IsReady()
                                && !IsWUsed && Q.IsReady())
                                hydra.Cast();
                            if (tiamat.IsOwned() && Player.Distance(target) < tiamat.Range && tiamat.IsReady() && !W.IsReady()
                                && !IsWUsed && Q.IsReady())
                                tiamat.Cast();
                            if (titanic.IsOwned() && Player.Distance(target) < titanic.Range && titanic.IsReady() && !W.IsReady()
                                    && !IsWUsed && Q.IsReady())
                                titanic.Cast();
                        }

                    }

                    // Jungleclear 
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    {
                        if (args.Target is Obj_AI_Minion)
                        {
                            var allJungleMinions = MinionManager.GetMinions(Orbwalking.GetRealAutoAttackRange(Player), MinionTypes.All,
                        MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                            if (allJungleMinions.Count != 0)
                            {
                                if (_menu.Item("jungleclearW").GetValue<bool>() && W.IsReady())
                                {
                                    foreach (var minion in allJungleMinions)
                                    {
                                        if (minion.IsValidTarget())
                                        {
                                            W.Cast(minion);
                                        }
                                    }
                                }

                                if (_menu.Item("jungleclearQ").GetValue<bool>() && Q.IsReady())
                                {
                                    foreach (var minion in allJungleMinions)
                                    {
                                        if (minion.IsValidTarget())
                                        {
                                            Q.CastOnUnit(minion);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
        private static void Killsteal()
        {
            Obj_AI_Hero qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (qTarget == null || qTarget.HasBuffOfType(BuffType.Invulnerability))
                return;
            {
                double damage = 0d;
                damage = ObjectManager.Player.GetSpellDamage(qTarget, SpellSlot.Q);

                if (damage > qTarget.Health)
                {
                    Q.Cast(qTarget);
                }
            }
        }

        private static void OnAa(AttackableUnit unit, AttackableUnit target)
        {          
            if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
            {
                if (hydra.IsOwned() && Player.Distance(target) < hydra.Range && hydra.IsReady() && !W.IsReady()
                        && !IsWUsed)
                    hydra.Cast();
                if (tiamat.IsOwned() && Player.Distance(target) < tiamat.Range && tiamat.IsReady() && !W.IsReady()
                        && !IsWUsed)
                    tiamat.Cast();
                if (titanic.IsOwned() && Player.Distance(target) < titanic.Range && titanic.IsReady() && !W.IsReady()
                        && !IsWUsed) titanic.Cast();
            }
        }

    }
  }

