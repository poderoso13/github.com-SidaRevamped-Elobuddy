using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Loader
{
    class Ryze
    {
        static Menu Menu;
        static Menu comboMenu, harassMenu, clearMenu;
        static LeagueSharp.Common.Spell Q, W, E, R;
        static bool QSpellCB { get { return getCheckBoxItem(comboMenu, "Combo.Q"); } }
        static bool WSpellCB { get { return getCheckBoxItem(comboMenu, "Combo.W"); } }
        static bool ESpellCB { get { return getCheckBoxItem(comboMenu, "Combo.E"); } }
        static bool RSpellCB { get { return getCheckBoxItem(comboMenu, "Combo.R"); } }
        static bool QSpellHR { get { return getCheckBoxItem(harassMenu, "Harass.Q"); } }
        static bool WSpellHR { get { return getCheckBoxItem(harassMenu, "Harass.W"); } }
        static bool ESpellHR { get { return getCheckBoxItem(harassMenu, "Harass.E"); } }
        static bool QSpellFR { get { return getCheckBoxItem(clearMenu, "Clear.Q"); } }
        static bool WSpellFR { get { return getCheckBoxItem(clearMenu, "Clear.W"); } }
        static bool ESpellFR { get { return getCheckBoxItem(clearMenu, "Clear.E"); } }
        static bool RSpellFR { get { return getCheckBoxItem(clearMenu, "Clear.R"); } }
        static EloBuddy.AIHeroClient Target => EloBuddy.SDK.TargetSelector.GetTarget(Q.Range, EloBuddy.DamageType.Magical);
        static List<EloBuddy.Obj_AI_Minion> Minions =>LeagueSharp.SDK.GameObjects.EnemyMinions.Where(m =>LeagueSharp.Common.MinionManager.IsMinion(m) &&LeagueSharp.Common.Utility.LSIsValidTarget(m, Q.Range)).ToList();
        static List<EloBuddy.Obj_AI_Minion> JungleMinions=>LeagueSharp.SDK.GameObjects.Jungle.Where(m =>LeagueSharp.Common.Utility.LSIsValidTarget(m, Q.Range) && !LeagueSharp.SDK.GameObjects.JungleSmall.Contains(m)).ToList();
    public static void RyzeLoading()
        {
            SetSpells();
            SetMenu();
            EloBuddy.Game.OnUpdate += Game_OnUpdate;
        }
        static void SetSpells()
        {
            Q = new LeagueSharp.Common.Spell(EloBuddy.SpellSlot.Q, 900);
            W = new LeagueSharp.Common.Spell(EloBuddy.SpellSlot.W, 600);
            E = new LeagueSharp.Common.Spell(EloBuddy.SpellSlot.E, 600);
            R = new LeagueSharp.Common.Spell(EloBuddy.SpellSlot.R);
            Q.SetSkillshot(0.25f, 50f, 1700f, true, LeagueSharp.Common.SkillshotType.SkillshotLine);
        }
        static void SetMenu()
        {
            Menu = MainMenu.AddMenu("Sida's Ryze", "Sida's Ryze");
            comboMenu = Menu.AddSubMenu("Combo", "Combo");
            comboMenu.Add("Combo.Q", new CheckBox("Use Q"));
            comboMenu.Add("Combo.W", new CheckBox("Use W"));
            comboMenu.Add("Combo.E", new CheckBox("Use E"));
            comboMenu.Add("Combo.R", new CheckBox("Use R"));
            harassMenu = Menu.AddSubMenu("Harass", "Harass");
            harassMenu.Add("Harass.Q", new CheckBox("Use Q"));
            harassMenu.Add("Harass.W", new CheckBox("Use W"));
            harassMenu.Add("Harass.E", new CheckBox("Use E"));
            clearMenu = Menu.AddSubMenu("Clear", "Clear");
            clearMenu.Add("Clear.Q", new CheckBox("Use Q"));
            clearMenu.Add("Clear.W", new CheckBox("Use W"));
            clearMenu.Add("Clear.E", new CheckBox("Use E"));
            clearMenu.Add("Clear.R", new CheckBox("Use R"));
            clearMenu.Add("Clear.Mana", new Slider("Min Mana %",20,0,100));
        }
        static bool getCheckBoxItem(Menu m, string item)
        {
            return m[item].Cast<CheckBox>().CurrentValue;
        }

        static int getSliderItem(Menu m, string item)
        {
            return m[item].Cast<Slider>().CurrentValue;
        }

        static bool getKeyBindItem(Menu m, string item)
        {
            return m[item].Cast<KeyBind>().CurrentValue;
        }

        static int getBoxItem(Menu m, string item)
        {
            return m[item].Cast<ComboBox>().CurrentValue;
        }
        static void Game_OnUpdate(EventArgs args)
        {
            if (EloBuddy.Player.Instance.IsDead)
            {
                return;
            }
            if (EloBuddy.SDK.Orbwalker.ActiveModesFlags.HasFlag(EloBuddy.SDK.Orbwalker.ActiveModes.Combo))
            {
                Combo();
            }
            if (EloBuddy.SDK.Orbwalker.ActiveModesFlags.HasFlag(EloBuddy.SDK.Orbwalker.ActiveModes.Harass))
            {
                Mixed();
            }
            if (EloBuddy.SDK.Orbwalker.ActiveModesFlags.HasFlag(EloBuddy.SDK.Orbwalker.ActiveModes.LaneClear) || EloBuddy.SDK.Orbwalker.ActiveModesFlags.HasFlag(EloBuddy.SDK.Orbwalker.ActiveModes.JungleClear))
            {
                Clear();
            }
        }
        static void Combo()
        {
            var stacks = EloBuddy.Player.GetBuff("RyzePassiveStack").Count;
            var target = EloBuddy.SDK.TargetSelector.GetTarget(Q.Range, EloBuddy.DamageType.Magical);
            if (LeagueSharp.Common.Utility.LSCountEnemiesInRange(EloBuddy.ObjectManager.Player, Q.Range) <= 2)
            {
                target = EloBuddy.SDK.TargetSelector.GetTarget(Q.Range, EloBuddy.DamageType.Magical);
            }
            else if (2 < LeagueSharp.Common.Utility.LSCountEnemiesInRange(EloBuddy.ObjectManager.Player, Q.Range))
            {
                target = EloBuddy.SDK.TargetSelector.GetTarget(W.Range, EloBuddy.DamageType.Magical);
            }
            if (stacks == 0)
            {
                if (!LeagueSharp.Common.Utility.IsReady(E))
                {
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) && QSpellCB && LeagueSharp.Common.HitChance.High <= Q.GetPrediction(target).Hitchance)
                    {
                        Q.Cast(target);
                    }
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, W.Range) && LeagueSharp.Common.Utility.IsReady(W) && WSpellCB)
                    {
                        W.CastOnUnit(target);
                    }
                }
                if (LeagueSharp.Common.Utility.IsReady(E))
                {
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, E.Range) && ESpellCB)
                    {
                        E.CastOnUnit(target);
                    }
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, W.Range) && LeagueSharp.Common.Utility.IsReady(W) && WSpellCB)
                    {
                        W.CastOnUnit(target);
                    }
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) && QSpellCB && LeagueSharp.Common.HitChance.High <= Q.GetPrediction(target).Hitchance)
                    {
                        Q.Cast(target);
                    }
                }
            }
            if (stacks == 1)
            {
                if (LeagueSharp.Common.Utility.IsReady(R) && RSpellCB)
                {
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, W.Range) && !LeagueSharp.Common.Utility.IsReady(Q) || !LeagueSharp.Common.Utility.IsReady(E))
                    {
                        R.Cast();
                    }
                }

                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) && QSpellCB && LeagueSharp.Common.HitChance.High <= Q.GetPrediction(target).Hitchance)
                {
                    Q.Cast(target);
                }
                if(LeagueSharp.Common.Utility.LSIsValidTarget(target, E.Range) && LeagueSharp.Common.Utility.IsReady(E) && ESpellCB)
                {
                    E.CastOnUnit(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, W.Range) && LeagueSharp.Common.Utility.IsReady(W) && WSpellCB)
                {
                    W.CastOnUnit(target);
                }
            }
            if (stacks == 2)
            {
                if (LeagueSharp.Common.Utility.IsReady(R) && RSpellCB)
                {
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) &&  !LeagueSharp.Common.Utility.IsReady(Q) && LeagueSharp.Common.Utility.IsReady(E))
                    {
                        R.Cast();
                    }
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) && !LeagueSharp.Common.Utility.IsReady(E))
                    {
                        R.Cast();
                    }
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && !LeagueSharp.Common.Utility.IsReady(Q) && !LeagueSharp.Common.Utility.IsReady(E))
                    {
                        R.Cast();
                    }
                }

                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) && QSpellCB && LeagueSharp.Common.HitChance.High <= Q.GetPrediction(target).Hitchance)
                {
                    Q.Cast(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, E.Range) && LeagueSharp.Common.Utility.IsReady(E) && ESpellCB)
                {
                    E.CastOnUnit(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, W.Range) && LeagueSharp.Common.Utility.IsReady(W) && WSpellCB)
                {
                    W.CastOnUnit(target);
                }
            }
            if (stacks == 3)
            {
                if (LeagueSharp.Common.Utility.IsReady(R) && RSpellCB)
                {
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) || !LeagueSharp.Common.Utility.IsReady(W))
                    {
                        R.Cast();
                    }
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) && QSpellCB && LeagueSharp.Common.HitChance.High <= Q.GetPrediction(target).Hitchance)
                {
                    Q.Cast(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, E.Range) && LeagueSharp.Common.Utility.IsReady(E) && ESpellCB)
                {
                    E.CastOnUnit(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, W.Range) && LeagueSharp.Common.Utility.IsReady(W) && WSpellCB)
                {
                    W.CastOnUnit(target);
                }
            }
            if (stacks == 4)
            {
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, W.Range) && LeagueSharp.Common.Utility.IsReady(W) && WSpellCB)
                {
                    W.CastOnUnit(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) && !LeagueSharp.Common.Utility.IsReady(W) && QSpellCB && LeagueSharp.Common.HitChance.High <= Q.GetPrediction(target).Hitchance)
                {
                    Q.Cast(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, E.Range) && LeagueSharp.Common.Utility.IsReady(E) && !LeagueSharp.Common.Utility.IsReady(W) && ESpellCB)
                {
                    E.CastOnUnit(target);
                }
            }
            if (LeagueSharp.SDK.GameObjects.Player.HasBuff("RyzePassiveCharged"))
            {
                if (LeagueSharp.Common.Utility.IsReady(R) && RSpellCB)
                {
                    if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && !LeagueSharp.Common.Utility.IsReady(W))
                    {
                        R.Cast();
                    }
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, W.Range) && LeagueSharp.Common.Utility.IsReady(W) && WSpellCB)
                {
                    W.CastOnUnit(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) && !LeagueSharp.Common.Utility.IsReady(W) && QSpellCB && LeagueSharp.Common.HitChance.High <= Q.GetPrediction(target).Hitchance)
                {
                    Q.Cast(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, E.Range) && LeagueSharp.Common.Utility.IsReady(E) && !LeagueSharp.Common.Utility.IsReady(W) && ESpellCB)
                {
                    E.CastOnUnit(target);
                }
            }
        }
        static void Mixed()
        {
            var target = EloBuddy.SDK.TargetSelector.GetTarget(Q.Range, EloBuddy.DamageType.Magical);
            if (LeagueSharp.Common.Utility.LSCountEnemiesInRange(EloBuddy.ObjectManager.Player, Q.Range) <= 2)
            {
                target = EloBuddy.SDK.TargetSelector.GetTarget(Q.Range, EloBuddy.DamageType.Magical);
            }
            else if (2 < LeagueSharp.Common.Utility.LSCountEnemiesInRange(EloBuddy.ObjectManager.Player, Q.Range))
            {
                target = EloBuddy.SDK.TargetSelector.GetTarget(W.Range, EloBuddy.DamageType.Magical);
            }
            if (!LeagueSharp.Common.Utility.IsReady(E))
            {
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) && QSpellHR && Q.GetPrediction(target).CollisionObjects.Any(c => c.IsMinion) && LeagueSharp.Common.HitChance.High <= Q.GetPrediction(target).Hitchance)
                {
                    Q.Cast(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, W.Range) && LeagueSharp.Common.Utility.IsReady(W) && WSpellHR)
                {
                    W.CastOnUnit(target);
                }
            }
            if (LeagueSharp.Common.Utility.IsReady(E))
            {
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, E.Range) && ESpellHR)
                {
                    E.CastOnUnit(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, W.Range) && LeagueSharp.Common.Utility.IsReady(W) && WSpellHR)
                {
                    W.CastOnUnit(target);
                }
                if (LeagueSharp.Common.Utility.LSIsValidTarget(target, Q.Range) && LeagueSharp.Common.Utility.IsReady(Q) && QSpellHR && Q.GetPrediction(target).CollisionObjects.Any(c => c.IsMinion) && LeagueSharp.Common.HitChance.High <= Q.GetPrediction(target).Hitchance)
                {
                    Q.Cast(target);
                }
            }
        }
        static void Clear()
        {
            if (LeagueSharp.Common.Utility.IsReady(R) && LeagueSharp.Common.Utility.IsReady(E) && getSliderItem(clearMenu, "Clear.Mana") <= EloBuddy.Player.Instance.ManaPercent && RSpellFR)
            {
                if (Minions.Any())
                {
                    if (Minions.Count() >= 3)
                    {
                        R.Cast();
                    }
                }
                else if (JungleMinions.Any())
                {
                    R.Cast();
                }
            }
            if (LeagueSharp.Common.Utility.IsReady(Q) && getSliderItem(clearMenu, "Clear.Mana") <= EloBuddy.Player.Instance.ManaPercent && QSpellFR)
            {
                if (Minions.Any())
                {
                    Q.Cast(Minions[0].ServerPosition);
                }
                if (JungleMinions.Any())
                {
                    Q.Cast(JungleMinions[0].ServerPosition);
                }
            }
            if (LeagueSharp.Common.Utility.IsReady(W) && getSliderItem(clearMenu, "Clear.Mana") <= EloBuddy.Player.Instance.ManaPercent && WSpellFR)
            {
                if (Minions.Any())
                {
                    W.CastOnUnit(Minions[0]);
                }
                if (JungleMinions.Any())
                {
                    W.CastOnUnit(JungleMinions[0]);
                }
            }
            if (LeagueSharp.Common.Utility.IsReady(E) && getSliderItem(clearMenu, "Clear.Mana") <= EloBuddy.Player.Instance.ManaPercent && ESpellFR)
            {
                if (Minions.Any())
                {
                    E.CastOnUnit(Minions[0]);
                }
                else if (JungleMinions.Any())
                {
                    E.CastOnUnit(JungleMinions[0]);
                }
            }
        }
    }
}
