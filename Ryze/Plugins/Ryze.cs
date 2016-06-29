using EloBuddy.SDK;
using EloBuddy.SDK.Menu;
using EloBuddy.SDK.Menu.Values;
using System;
using System.Collections.Generic;
using System.Linq;
namespace Loader
{
    class Ryze
    {
        static Menu Menu;
        static Menu comboMenu, harassMenu, clearMenu,drawMenu;
        static EloBuddy.SDK.Spell.Skillshot Q;
        static EloBuddy.SDK.Spell.Targeted W;
        static EloBuddy.SDK.Spell.Targeted E;
        static EloBuddy.SDK.Spell.Active R;
        static bool QSpellCB { get { return getCheckBoxItem(comboMenu, "Combo.Q"); } }
        static bool WSpellCB { get { return getCheckBoxItem(comboMenu, "Combo.W"); } }
        static bool ESpellCB { get { return getCheckBoxItem(comboMenu, "Combo.E"); } }
        static bool RSpellCB { get { return getCheckBoxItem(comboMenu, "Combo.R"); } }
        static bool BlockAA  { get { return getCheckBoxItem(comboMenu, "Combo.BlockAA"); } }
        static bool QSpellHR { get { return getCheckBoxItem(harassMenu, "Harass.Q"); } }
        static bool WSpellHR { get { return getCheckBoxItem(harassMenu, "Harass.W"); } }
        static bool ESpellHR { get { return getCheckBoxItem(harassMenu, "Harass.E"); } }
        static bool QSpellFR { get { return getCheckBoxItem(clearMenu, "Clear.Q"); } }
        static bool WSpellFR { get { return getCheckBoxItem(clearMenu, "Clear.W"); } }
        static bool ESpellFR { get { return getCheckBoxItem(clearMenu, "Clear.E"); } }
        static bool RSpellFR { get { return getCheckBoxItem(clearMenu, "Clear.R"); } }
        static bool QSpellDr { get { return getCheckBoxItem(drawMenu, "Draws.Q"); } }
        static bool WSpellDR { get { return getCheckBoxItem(drawMenu, "Draws.W"); } }
        static bool ESpellDR { get { return getCheckBoxItem(drawMenu, "Draws.E"); } }

        static EloBuddy.AIHeroClient Target
        {
            get
            {
                return EloBuddy.SDK.TargetSelector.GetTarget(Q.Range, EloBuddy.DamageType.Magical);
            }
        }
        static List<EloBuddy.Obj_AI_Minion> Minions
        {
            get
            {
                return EloBuddy.SDK.EntityManager.MinionsAndMonsters.EnemyMinions.Where(m => EloBuddy.SDK.Extensions.IsMinion(m) && EloBuddy.SDK.Extensions.IsValidTarget(m, Q.Range)).ToList();
            }
        }
        static List<EloBuddy.Obj_AI_Minion> JungleMinions
        {
            get
            {
                return EloBuddy.SDK.EntityManager.MinionsAndMonsters.Monsters.Where(m => EloBuddy.SDK.Extensions.IsValidTarget(m, Q.Range)).ToList();
            }
        }
       public static void RyzeLoading()
        {
            SetSpells();
            SetMenu();
            EloBuddy.Game.OnUpdate += Game_OnUpdate;
            Orbwalker.OnPreAttack += Orbwalker_OnPreAttack;
            EloBuddy.Drawing.OnDraw += OnDraw;
        }
        static void SetSpells()
        {
            Q = new EloBuddy.SDK.Spell.Skillshot(EloBuddy.SpellSlot.Q, 900, EloBuddy.SDK.Enumerations.SkillShotType.Linear, 250, 1700, 50);
            if (HasRyzeRBuff)
            {
                Q.AllowedCollisionCount = int.MaxValue;
            }
            W = new EloBuddy.SDK.Spell.Targeted(EloBuddy.SpellSlot.W, 600);
            E = new EloBuddy.SDK.Spell.Targeted(EloBuddy.SpellSlot.E, 600);
            R = new EloBuddy.SDK.Spell.Active(EloBuddy.SpellSlot.R);
        }
        static void SetMenu()
        {
            Menu = MainMenu.AddMenu("Sida's Ryze", "Sida's Ryze");
            comboMenu = Menu.AddSubMenu("Combo", "Combo");
            comboMenu.Add("Combo.Q", new CheckBox("Use Q"));
            comboMenu.Add("Combo.W", new CheckBox("Use W"));
            comboMenu.Add("Combo.E", new CheckBox("Use E"));
            comboMenu.Add("Combo.R", new CheckBox("Use R"));
            comboMenu.Add("Combo.BlockAA", new CheckBox("BlockAA"));
            harassMenu = Menu.AddSubMenu("Harass", "Harass");
            harassMenu.Add("Harass.Q", new CheckBox("Use Q"));
            harassMenu.Add("Harass.W", new CheckBox("Use W"));
            harassMenu.Add("Harass.E", new CheckBox("Use E"));
            clearMenu = Menu.AddSubMenu("Clear", "Clear");
            clearMenu.Add("Clear.Q", new CheckBox("Use Q"));
            clearMenu.Add("Clear.W", new CheckBox("Use W"));
            clearMenu.Add("Clear.E", new CheckBox("Use E"));
            clearMenu.Add("Clear.R", new CheckBox("Use R"));
            clearMenu.Add("Clear.Mana", new Slider("Min Mana %", 20, 0, 100));
            drawMenu= Menu.AddSubMenu("Draws", "Draws");
            drawMenu.Add("Draws.Q", new CheckBox("Draw Q"));
            drawMenu.Add("Draws.W", new CheckBox("Draw W"));
            drawMenu.Add("Draws.E", new CheckBox("Draw E",false));
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
        private static void Orbwalker_OnPreAttack(EloBuddy.AttackableUnit target, EloBuddy.SDK.Orbwalker.PreAttackArgs args)
        {
            if (EloBuddy.SDK.Orbwalker.ActiveModesFlags.HasFlag(EloBuddy.SDK.Orbwalker.ActiveModes.Combo) && args.Target is EloBuddy.AIHeroClient && args.Target.Distance(EloBuddy.Player.Instance) > 400 && (W.IsReady() || E.IsReady() ||Q.IsReady()) && BlockAA)
                args.Process = false;
        }

        static void OnDraw(EventArgs args)
        {
            if (QSpellDr && Q.IsLearned)
            {
                EloBuddy.SDK.Rendering.Circle.Draw(SharpDX.Color.DeepSkyBlue, Q.Range, EloBuddy.Player.Instance.Position);
            }
            if (WSpellDR && W.IsLearned)
            {
                EloBuddy.SDK.Rendering.Circle.Draw(SharpDX.Color.DeepSkyBlue, W.Range, EloBuddy.Player.Instance.Position);
            }
            if (ESpellDR && E.IsLearned)
            {
                EloBuddy.SDK.Rendering.Circle.Draw(SharpDX.Color.DeepSkyBlue, E.Range, EloBuddy.Player.Instance.Position);
            }
        }
        static bool HasRyzeRBuff
        {
            get
            {
                return EloBuddy.Player.Instance.GetBuff("RyzeR") != null;
            }
        }
        static void Combo()
        {
            var target = EloBuddy.SDK.TargetSelector.GetTarget(Q.Range, EloBuddy.DamageType.Magical);
            if (EloBuddy.SDK.Extensions.CountEnemiesInRange(EloBuddy.ObjectManager.Player, Q.Range) <= 2)
            {
                target = EloBuddy.SDK.TargetSelector.GetTarget(Q.Range, EloBuddy.DamageType.Magical);
            }
            else if (2 < EloBuddy.SDK.Extensions.CountEnemiesInRange(EloBuddy.ObjectManager.Player, Q.Range))
            {
                target = EloBuddy.SDK.TargetSelector.GetTarget(W.Range, EloBuddy.DamageType.Magical);
            }
            if (!EloBuddy.Player.Instance.HasBuff("RyzePassiveStack") && !EloBuddy.Player.HasBuff("RyzePassiveCharged"))
            {
                if (!E.IsReady())
                {
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady() && QSpellCB && EloBuddy.SDK.Enumerations.HitChance.High <= Q.GetPrediction(target).HitChance)
                    {
                        Q.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, W.Range) && W.IsReady() && WSpellCB)
                    {
                        W.Cast(target);
                    }
                }
                if (E.IsReady())
                {
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, E.Range) && ESpellCB)
                    {
                        E.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, W.Range) && W.IsReady() && WSpellCB)
                    {
                        W.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady() && QSpellCB && EloBuddy.SDK.Enumerations.HitChance.High <= Q.GetPrediction(target).HitChance)
                    {
                        Q.Cast(target);
                    }
                }
            }
            if (EloBuddy.Player.Instance.HasBuff("RyzePassiveStack"))
            {
                if (EloBuddy.Player.Instance.GetBuff("RyzePassiveStack").Count == 1)
                {
                    if (R.IsReady() && RSpellCB)
                    {
                        if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && !(!Q.IsReady() && E.IsReady()) || !(Q.IsReady() && !E.IsReady()))
                        {
                            R.Cast();
                        }
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, E.Range) && E.IsReady() && ESpellCB)
                    {
                        E.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady() && QSpellCB && EloBuddy.SDK.Enumerations.HitChance.High <= Q.GetPrediction(target).HitChance)
                    {
                        Q.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, W.Range) && W.IsReady() && WSpellCB)
                    {
                        W.Cast(target);
                    }
                }
                if (EloBuddy.Player.GetBuff("RyzePassiveStack").Count == 2)
                {
                    if (R.IsReady() && RSpellCB)
                    {
                        if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && !Q.IsReady() && E.IsReady())
                        {
                            R.Cast();
                        }
                        if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady() && !E.IsReady())
                        {
                            R.Cast();
                        }
                        if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && !Q.IsReady() && !E.IsReady() && W.IsReady())
                        {
                            R.Cast();
                        }
                    }

                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady() && QSpellCB && EloBuddy.SDK.Enumerations.HitChance.High <= Q.GetPrediction(target).HitChance)
                    {
                        Q.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, E.Range) && E.IsReady() && ESpellCB)
                    {
                        E.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, W.Range) && W.IsReady() && WSpellCB)
                    {
                        W.Cast(target);
                    }
                }
                if (EloBuddy.Player.GetBuff("RyzePassiveStack").Count == 3)
                {
                    if (R.IsReady() && RSpellCB)
                    {
                        if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady())
                        {
                            R.Cast();
                        }
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady() && QSpellCB && EloBuddy.SDK.Enumerations.HitChance.High <= Q.GetPrediction(target).HitChance)
                    {
                        Q.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, E.Range) && E.IsReady() && ESpellCB)
                    {
                        E.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, W.Range) && W.IsReady() && WSpellCB && !Q.IsReady() && !E.IsReady() && !R.IsReady())
                    {
                        W.Cast(target);
                    }
                }
                if (EloBuddy.Player.GetBuff("RyzePassiveStack").Count == 4)
                {
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, W.Range) && W.IsReady() && WSpellCB)
                    {
                        W.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady() && !W.IsReady() && QSpellCB && EloBuddy.SDK.Enumerations.HitChance.High <= Q.GetPrediction(target).HitChance)
                    {
                        Q.Cast(target);
                    }
                    if (EloBuddy.SDK.Extensions.IsValidTarget(target, E.Range) && E.IsReady() && !W.IsReady() && ESpellCB)
                    {
                        E.Cast(target);
                    }
                }
            }
            if (EloBuddy.Player.HasBuff("RyzePassiveCharged"))
            {
                if (EloBuddy.SDK.Extensions.IsValidTarget(target, W.Range) && W.IsReady() && WSpellCB)
                {
                    W.Cast(target);
                }
                if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady() && QSpellCB && EloBuddy.SDK.Enumerations.HitChance.High <= Q.GetPrediction(target).HitChance && !W.IsReady())
                {
                    Q.Cast(target);
                }
                if (EloBuddy.SDK.Extensions.IsValidTarget(target, E.Range) && E.IsReady() && ESpellCB && !W.IsReady())
                {
                    E.Cast(target);
                }
            }
        }
        static void Mixed()
        {
            var target = EloBuddy.SDK.TargetSelector.GetTarget(Q.Range, EloBuddy.DamageType.Magical);
            if (EloBuddy.SDK.Extensions.CountEnemiesInRange(EloBuddy.ObjectManager.Player, Q.Range) <= 2)
            {
                target = EloBuddy.SDK.TargetSelector.GetTarget(Q.Range, EloBuddy.DamageType.Magical);
            }
            else if (2 < EloBuddy.SDK.Extensions.CountEnemiesInRange(EloBuddy.ObjectManager.Player, Q.Range))
            {
                target = EloBuddy.SDK.TargetSelector.GetTarget(W.Range, EloBuddy.DamageType.Magical);
            }
            if (!E.IsReady())
            {
                if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady() && QSpellHR && EloBuddy.SDK.Enumerations.HitChance.High <= Q.GetPrediction(target).HitChance)
                {
                    Q.Cast(target);
                }
                if (EloBuddy.SDK.Extensions.IsValidTarget(target, W.Range) && W.IsReady() && WSpellHR)
                {
                    W.Cast(target);
                }
            }
            if (E.IsReady())
            {
                if (EloBuddy.SDK.Extensions.IsValidTarget(target, E.Range) && ESpellHR)
                {
                    E.Cast(target);
                }
                if (EloBuddy.SDK.Extensions.IsValidTarget(target, W.Range) && W.IsReady() && WSpellHR)
                {
                    W.Cast(target);
                }
                if (EloBuddy.SDK.Extensions.IsValidTarget(target, Q.Range) && Q.IsReady() && QSpellHR && Q.GetPrediction(target).CollisionObjects.Any(c => c.IsMinion) && EloBuddy.SDK.Enumerations.HitChance.High <= Q.GetPrediction(target).HitChance)
                {
                    Q.Cast(target);
                }
            }
        }
        static void Clear()
        {
            if (R.IsReady() && E.IsReady() && getSliderItem(clearMenu, "Clear.Mana") <= EloBuddy.Player.Instance.ManaPercent && RSpellFR)
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
            if (Q.IsReady() && getSliderItem(clearMenu, "Clear.Mana") <= EloBuddy.Player.Instance.ManaPercent && QSpellFR)
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
            if (W.IsReady() && getSliderItem(clearMenu, "Clear.Mana") <= EloBuddy.Player.Instance.ManaPercent && WSpellFR)
            {
                if (Minions.Any())
                {
                    W.Cast(Minions[0]);
                }
                if (JungleMinions.Any())
                {
                    W.Cast(JungleMinions[0]);
                }
            }
            if (E.IsReady() && getSliderItem(clearMenu, "Clear.Mana") <= EloBuddy.Player.Instance.ManaPercent && ESpellFR)
            {
                if (Minions.Any())
                {
                    E.Cast(Minions[0]);
                }
                else if (JungleMinions.Any())
                {
                    E.Cast(JungleMinions[0]);
                }
            }
        }
    }
}
