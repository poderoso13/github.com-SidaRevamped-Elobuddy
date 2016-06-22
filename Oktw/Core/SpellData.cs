using EloBuddy;
using EloBuddy.SDK;
using SharpDX;
using System;
using System.Linq;

namespace Core
{
    public enum SpellType
    {
        Self,
        Targeted,
        Linear,
        Circular,
        Cone,
        Unknown
    }
    public class SpellBase
    {
        private float _speed;

        private float _width;

        private int _allowedCollisionCount = int.MaxValue; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public int AllowedCollisionCount
        {
            get
            {
                return _allowedCollisionCount;
            }
            set
            {
                _allowedCollisionCount = value;
            }
        }
        private bool _aoe; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public bool Aoe
        {
            get
            {
                return _aoe;
            }
            set
            {
                _aoe = value;
            }
        }

        private float _delay; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public float Delay
        {
            get
            {
                return _delay;
            }
            set
            {
                _delay = value;
            }
        }
        private bool _collidesWithYasuoWall = true; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public bool CollidesWithYasuoWall
        {
            get
            {
                return _collidesWithYasuoWall;
            }
            set
            {
                _collidesWithYasuoWall = value;
            }
        }

        private int _lastCastTime; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public int LastCastTime
        {
            get
            {
                return _lastCastTime;
            }
            set
            {
                _lastCastTime = value;
            }
        }
        private Vector3 _lastEndPosition; // ENCAPSULATE FIELD BY CODEIT.RIGHT

        public Vector3 LastEndPosition
        {
            get
            {
                return _lastEndPosition;
            }
            set
            {
                _lastEndPosition = value;
            }
        }
        public int LastCastSpellAttempt;
        public Vector3 LastStartPosition;
        public float MinHitChance = 60f;

        public float Range;

        public SpellSlot Slot;
        public SpellType Type;

        public SpellBase(SpellSlot slot, SpellType? type, float range = float.MaxValue)
        {
            Slot = slot;
            Type = type ?? SpellType.Self;
            Range = range;
        }

        public SpellDataInst Instance
        {
            get { return Slot != SpellSlot.Unknown ? Player.Instance.Spellbook.GetSpell(Slot) : null; }
        }

        public float Speed
        {
            get { return _speed > 0 ? _speed : float.MaxValue; }
            set { _speed = value; }
        }

        public float Width
        {
            get { return _width > 0 ? _width : 1; }
            set { _width = value; }
        }

        public string Name
        {
            get { return Slot != SpellSlot.Unknown ? Instance.Name : ""; }
        }

        public bool IsReady
        {
            get { return Slot != SpellSlot.Unknown && Instance.IsReady; }
        }

        public float Mana
        {
            get { return Slot != SpellSlot.Unknown ? Instance.SData.Mana : 0; }
        }

        public EloBuddy.SDK.Enumerations.HitChance[] hitChance = { EloBuddy.SDK.Enumerations.HitChance.High, EloBuddy.SDK.Enumerations.HitChance.Medium, EloBuddy.SDK.Enumerations.HitChance.Low };

        public static int hc
        {
            get
            {
                return 0;
            }
        }

        public static EloBuddy.SDK.Enumerations.HitChance getHitChance
        {
            get
            {
                switch(hc)
                {
                    case 0:
                        return EloBuddy.SDK.Enumerations.HitChance.High;
                    case 1:
                        return EloBuddy.SDK.Enumerations.HitChance.Medium;
                    case 2:
                        return EloBuddy.SDK.Enumerations.HitChance.Low;
                    default:
                        return EloBuddy.SDK.Enumerations.HitChance.High;
                }
            }
        }

        public bool IsInRange(Obj_AI_Base target)
        {
            switch (Type)
            {
                case SpellType.Targeted:
                    return Player.Instance.IsInRange(target, Range);
                case SpellType.Circular:
                    return Player.Instance.IsInRange(target, Range + Width / 2 + target.BoundingRadius / 2f);
                case SpellType.Linear:
                    return Player.Instance.IsInRange(target, Range + Width);
                default:
                    // do the default action
                    break;
            }
            //Self
            return Player.Instance.IsInRange(target, Range + target.BoundingRadius / 2f);
        }

        public bool PredictedPosInRange(Obj_AI_Base target, Vector3 position)
        {
            switch (Type)
            {
                case SpellType.Targeted:
                    return Player.Instance.IsInRange(position, Range);
                case SpellType.Circular:
                    return Player.Instance.IsInRange(position, Range + Width / 2f + target.BoundingRadius / 2f);
                case SpellType.Linear:
                    return Player.Instance.IsInRange(position, Range);
                default:
                    // do the default action
                    break;
            }
            //Self
            return Player.Instance.IsInRange(position, Range + target.BoundingRadius / 2f);
        }
        public PredictionResult GetPrediction(Obj_AI_Base target, Vector3? startPos = null)
        {
            var startPosition = startPos ?? MyHero.Position;
            PredictionResult result;
            switch (Type)
            {
                case SpellType.Circular:
                    result = EloBuddy.SDK.Prediction.Position.PredictCircularMissile(target, Range, (int)Width, (int)(1000 * Delay), Speed,
                        startPosition);
                    break;
                case SpellType.Cone:
                    result = EloBuddy.SDK.Prediction.Position.PredictConeSpell(target, Range, (int)Width, (int)(1000 * Delay), Speed,
                        startPosition);
                    break;
                case SpellType.Self:
                    result = EloBuddy.SDK.Prediction.Position.PredictCircularMissile(target, Range, (int)Width, (int)(1000 * Delay), Speed,
                        startPosition);
                    break;
                default:
                    result = EloBuddy.SDK.Prediction.Position.PredictLinearMissile(target, Range, (int)Width, (int)(1000 * Delay), Speed,
                        AllowedCollisionCount, startPosition);
                    break;
            }
            return result;
        }

        public bool WillHitYasuoWall(Vector3 position)
        {
            return Speed > 0 && CollidesWithYasuoWall && YasuoWallManager.WillHitYasuoWall(Player.Instance.Position, position);
        }

        public AIHeroClient Target
        {
            get { return TargetSelector.GetTarget(Range, DamageType.Physical, null, true); }
        }

        internal static AIHeroClient MyHero
        {
            get { return Player.Instance; }
        }

        public void Cast()
        {
            if (Chat.IsOpen || !IsReady || (Orbwalker.ActiveModesFlags.HasFlag(Orbwalker.ActiveModes.Harass) && Orbwalker.ShouldWait))
            {
                return;
            }
            AIHeroClient target;
            if ((MyHero.Hero == Champion.Viktor && Slot == SpellSlot.E) || (MyHero.Hero == Champion.Rumble && Slot == SpellSlot.R))
            {
                const float realRange = 525f;
                Range += realRange;
                target = Target;
                if (target != null)
                {
                    var startPos = target.IsInRange(MyHero, realRange) ? target.Position : (MyHero.Position + (target.Position - MyHero.Position).Normalized() * realRange);
                    var pred = GetPrediction(target, startPos);
                    var endPos = startPos + (pred.CastPosition - startPos).Normalized() * (Range - realRange);
                    if (pred.HitChance >= getHitChance)
                    {
                        if (WillHitYasuoWall(pred.CastPosition) || !PredictedPosInRange(target, pred.CastPosition))
                        {
                            return;
                        }
                        if (Player.Instance.Spellbook.CastSpell(Slot, endPos, startPos))
                        {
                            LastCastSpellAttempt = Core.GameTickCount;
                        }
                    }
                }
                return;
            }
            target = Target;
            if (target == null)
            {
                return;
            }
            if (!IsInRange(target))
            {
                return;
            }
            if (Type == SpellType.Linear || Type == SpellType.Circular || Type == SpellType.Cone)
            {
                var pred = GetPrediction(target);
                if (pred.HitChance >= getHitChance)
                {
                    if (WillHitYasuoWall(pred.CastPosition) || !PredictedPosInRange(target, pred.CastPosition))
                    {
                        return;
                    }
                    if (Player.Instance.Spellbook.CastSpell(Slot, pred.CastPosition))
                    {
                        LastCastSpellAttempt = Core.GameTickCount;
                    }
                }
            }
            else if (Type == SpellType.Targeted)
            {
                if (WillHitYasuoWall(target.ServerPosition))
                {
                    return;
                }
                if (Player.Instance.Spellbook.CastSpell(Slot, target))
                {
                    LastCastSpellAttempt = Core.GameTickCount;
                }
            }
            else if (Type == SpellType.Self)
            {
                var pred = GetPrediction(target);
                if (pred.HitChance >= getHitChance)
                {
                    if (!PredictedPosInRange(target, pred.CastPosition))
                    {
                        return;
                    }
                    if (Player.Instance.Spellbook.CastSpell(Slot))
                    {
                        LastCastSpellAttempt = Core.GameTickCount;
                    }
                }
            }
        }

        public void StartCast()
        {
            var target = Target;
            if (target != null)
            {
                if (Chat.IsOpen || !IsReady || !IsInRange(target))
                {
                    return;
                }
                if (Player.Instance.Spellbook.CastSpell(Slot, Game.CursorPos))
                {
                    LastCastSpellAttempt = Core.GameTickCount;
                }
            }
        }
        public void ReleaseCast()
        {
            var target = Target;
            if (target != null)
            {
                if (Chat.IsOpen || !IsReady || !IsInRange(target))
                {
                    return;
                }
                if (Type == SpellType.Linear || Type == SpellType.Circular || Type == SpellType.Cone)
                {
                    var pred = GetPrediction(target);
                    if (pred.HitChance >= getHitChance)
                    {
                        if (WillHitYasuoWall(pred.CastPosition) || !PredictedPosInRange(target, pred.CastPosition))
                        {
                            return;
                        }
                        if (Player.Instance.Spellbook.UpdateChargeableSpell(Slot, pred.CastPosition, true))
                        {
                            LastCastSpellAttempt = Core.GameTickCount;
                        }
                    }
                }
            }
        }
        public void Cast(Vector3 position)
        {
            if (Chat.IsOpen || !IsReady)
            {
                return;
            }
            if (Player.Instance.Spellbook.CastSpell(Slot, position))
            {
                LastCastSpellAttempt = Core.GameTickCount;
            }
        }

    }
    public static class YasuoWallManager
    {
        private static Vector3 _startPosition;
        private static GameObject _wallObject;
        private static bool _containsYasuo;

        public static void Initialize()
        {
            if (EntityManager.Heroes.Enemies.Any(h => h.Hero == Champion.Yasuo))
            {
                _containsYasuo = true;
                GameObject.OnCreate += delegate (GameObject sender, EventArgs args)
                {
                    if (sender.Name.Contains("Yasuo_Base_W_windwall") && !sender.Name.Contains("_activate.troy"))
                    {
                        _wallObject = sender;
                    }
                };
                GameObject.OnDelete += delegate (GameObject sender, EventArgs args)
                {
                    if (sender.Name.Contains("Yasuo_Base_W_windwall") && !sender.Name.Contains("_activate.troy"))
                    {
                        _wallObject = null;
                    }
                };
                Obj_AI_Base.OnProcessSpellCast +=
                    delegate (Obj_AI_Base sender, GameObjectProcessSpellCastEventArgs args)
                    {
                        var hero = sender as AIHeroClient;
                        if (hero != null && hero.IsEnemy && hero.Hero == Champion.Yasuo && args.Slot == SpellSlot.W)
                        {
                            _startPosition = hero.ServerPosition;
                        }
                    };
            }
        }

        public static bool WillHitYasuoWall(Vector3 startPosition, Vector3 endPosition)
        {
            if (_containsYasuo)
            {
                if (_wallObject != null && _wallObject.IsValid && !_wallObject.IsDead)
                {
                    var level = Convert.ToInt32(_wallObject.Name.Substring(_wallObject.Name.Length - 6, 1));
                    var width = 250f + level * 50f;
                    var pos1 = _wallObject.Position.To2D() +
                               (_wallObject.Position.To2D() - _startPosition.To2D()).Normalized().Perpendicular() * width /
                               2f;
                    var pos2 = _wallObject.Position.To2D() +
                               (_wallObject.Position.To2D() - _startPosition.To2D()).Normalized().Perpendicular2() * width /
                               2f;
                    var intersection = pos1.Intersection(pos2, startPosition.To2D(), endPosition.To2D());
                    return intersection.Point.IsValid();
                }
            }
            return false;
        }
    }
}