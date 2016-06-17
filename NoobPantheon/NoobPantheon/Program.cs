using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LeagueSharp;
using LeagueSharp.Common;
using Color = System.Drawing.Color;

namespace NoobPantheon
{
    class Program
    {
        private static Obj_AI_Hero Player
        {
            get { return ObjectManager.Player; }
        }
        private static readonly Obj_AI_Hero[] AllEnemy = HeroManager.Enemies.ToArray();
        private static Orbwalking.Orbwalker Orbwalker;

        private static Spell Q, W, E, R;

        private static Items.Item tiamat, hydra;

        private static Menu _menu;

        static void Main(string[] args)
        {
            CustomEvents.Game.OnGameLoad += Game_OnGameLoad;
        }
        /// <summary>
        /// Game Loaded Method
        /// </summary>
        private static void Game_OnGameLoad(EventArgs args)
        {
            if (Player.ChampionName != "Pantheon")            
                return;         

            Q = new Spell(SpellSlot.Q, 600);
            W = new Spell(SpellSlot.W, 600);
            E = new Spell(SpellSlot.E, 700);

            // Create Items
            hydra = new Items.Item(3074, 185);
            tiamat = new Items.Item(3077, 185);

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
            comboMenu.AddItem(new MenuItem("useW2", "Use W when target is in AA range").SetValue(false));
            comboMenu.AddItem(new MenuItem("useE", "Use E").SetValue(true));            
            _menu.AddSubMenu(comboMenu);

            //Harrass-_menu
            Menu harassMenu = new Menu("Harass", "Harass");
            harassMenu.AddItem(new MenuItem("harassQ", "Use Q to Harass").SetValue(true));
            _menu.AddSubMenu(harassMenu);

            //Laneclear-_menu
            Menu laneclear = new Menu("Laneclear", "Laneclear");
            laneclear.AddItem(new MenuItem("laneclearQ", "Use Q to LaneClear").SetValue(true));
            _menu.AddSubMenu(laneclear);

            //Jungleclear-_menu
            Menu jungleclear = new Menu("Jungleclear", "Jungleclear");
            jungleclear.AddItem(new MenuItem("jungleclearQ", "Use Q to JungleClear").SetValue(true));
            jungleclear.AddItem(new MenuItem("jungleclearW", "Use W to JungleClear").SetValue(true));
            jungleclear.AddItem(new MenuItem("jungleclearE", "Use E to JungleClear").SetValue(true));
            _menu.AddSubMenu(jungleclear);

            //KS-_menu
            Menu ksMenu = new Menu("Killsteal", "Killsteal");
            ksMenu.AddItem(new MenuItem("Killsteal", "Killsteal with Q").SetValue(true));
            _menu.AddSubMenu(ksMenu);

            //Drawings-_menu
            Menu drawingsMenu = new Menu("Drawings", "Drawings");
            drawingsMenu.AddItem(new MenuItem("draw Q", "Draw Q range").SetValue(false));
            drawingsMenu.AddItem(new MenuItem("drawW", "Draw W range").SetValue(false));
            _menu.AddSubMenu(drawingsMenu);

            Drawing.OnDraw += OnDraw;
            OnDoCast();
            Game.OnUpdate += OnUpdate;
            Game.PrintChat("NoobPantheon by 1Shinigamix3");
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
        }
        private static void Combo()
        {
            if (Orbwalker.ActiveMode != Orbwalking.OrbwalkingMode.Combo)
                return;
            Obj_AI_Hero Qtarget = TargetSelector.GetTarget(600, TargetSelector.DamageType.Physical);
            Obj_AI_Hero Wtarget = TargetSelector.GetTarget(600, TargetSelector.DamageType.Magical);
            // ACTUAL COMBO
            if ((Qtarget != null && !Qtarget.IsZombie) || (Wtarget != null && !Wtarget.IsZombie))
            {
                if (W.IsReady() && _menu.Item("useW").GetValue<bool>())
                {
                    if ((Player.Distance(Wtarget.Position) > Orbwalking.GetRealAutoAttackRange(Player)) || _menu.Item("useW2").GetValue<bool>())
                    {
                        W.CastOnUnit(Wtarget);
                    }
                }
                if (Q.IsReady() && (_menu.Item("useQ").GetValue<bool>()))
                {
                    Q.CastOnUnit(Qtarget);
                }
                if (E.IsReady() && (_menu.Item("useE").GetValue<bool>()) && !W.IsReady())
                {
                    E.Cast(Qtarget);
                }
            }
        }
        private static void OnDoCast()
        {
            Obj_AI_Base.OnDoCast += (sender, args) =>
            {
                //if (!sender.IsMe || !Orbwalking.IsAutoAttack((args.SData.Name))) return;
                if (sender.IsMe && args.SData.IsAutoAttack())
                {
                    Obj_AI_Hero target = TargetSelector.GetTarget(600, TargetSelector.DamageType.Physical);
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Combo || Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.Mixed)
                    {
                        if (hydra.IsOwned() && Player.Distance(target) < hydra.Range && hydra.IsReady())
                            hydra.Cast();
                        if (tiamat.IsOwned() && Player.Distance(target) < tiamat.Range && tiamat.IsReady())
                            tiamat.Cast();
                    }
                    // Jungleclear 
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    {
                        if (args.Target is Obj_AI_Minion)
                        {
                            var allJungleMinions = MinionManager.GetMinions(Q.Range, MinionTypes.All,
                        MinionTeam.Neutral, MinionOrderTypes.MaxHealth);
                            if (allJungleMinions.Count != 0)
                            {
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
                                if (_menu.Item("jungleclearW").GetValue<bool>() && W.IsReady())
                                {
                                    foreach (var minion in allJungleMinions)
                                    {
                                        if (minion.IsValidTarget())
                                        {
                                            W.CastOnUnit(minion);
                                        }
                                    }
                                }
                                if (_menu.Item("jungleclearE").GetValue<bool>() && E.IsReady())
                                {
                                    foreach (var minion in allJungleMinions)
                                    {
                                        if (minion.IsValidTarget())
                                        {
                                            E.Cast(minion);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // Laneclear
                    if (Orbwalker.ActiveMode == Orbwalking.OrbwalkingMode.LaneClear)
                    {
                        if (args.Target is Obj_AI_Minion)
                        {
                            var allLaneMinions = MinionManager.GetMinions(Q.Range);
                            //Lane
                            if (_menu.Item("laneclearW").GetValue<bool>() && Q.IsReady())
                            {
                                foreach (var minion in allLaneMinions)
                                {
                                    if (minion.IsValidTarget())
                                    {
                                        Q.Cast(minion);
                                    }
                                }
                            }
                        }
                    }
                }
            };
        }
        private static void OnDraw(EventArgs args)
        {
            if (_menu.Item("drawQ").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, Q.Range,
                    Color.Tan);
            }
            if (_menu.Item("drawW").GetValue<bool>())
            {
                Render.Circle.DrawCircle(Player.Position, W.Range,
                    Color.Blue);
            }
        }
        private static void Killsteal()
        {
            Obj_AI_Hero qTarget = TargetSelector.GetTarget(Q.Range, TargetSelector.DamageType.Physical);
            if (qTarget == null || qTarget.HasBuffOfType(BuffType.Invulnerability) || qTarget.IsZombie)
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
    }
}
