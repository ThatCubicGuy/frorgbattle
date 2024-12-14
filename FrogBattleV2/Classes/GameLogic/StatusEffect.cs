using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using EffectID = FrogBattleV2.Classes.GameLogic.StatusEffect.Effect.EffectID;
namespace FrogBattleV2.Classes.GameLogic
{
    internal class StatusEffect : ICloneable
    {
        private void ReplaceEffects(List<Effect> eff)
        {
            List<Effect> eff2 = new();
            foreach (var item in eff)
            {
                eff2.Add((Effect)(item as ICloneable).Clone());
            }
            Effects = eff2;
        }
        private static int _statusEffectID = 0;
        private readonly int UID;

        [Flags] // These are all the possible properties that a StatusEffect can have. Very handy
                // and easy to use, far superior to my previous, quite silly method.
        public enum PropertyID
        {
            None        = 0b_0000_0000, // 0
            Debuff      = 0b_0000_0001, // 1
            Unremovable = 0b_0000_0010, // 2
            Invisible   = 0b_0000_0100  // 4
        }
        public static bool IsShield(StatusEffect se) => se.Effects.Any(x => x is Shield);
        // Predicate checks for effects that have run out [negative turn values are infinite]
        public static bool Expired(StatusEffect se) => se.Turns == 0 ||
        // Broken Shields
        se.GetEffectsOfType(EffectID.Shield, 0) == 0 ||
        // Smashed barriers
        se.GetEffectsOfType(EffectID.Barrier, 0) == 0;
        public static void Expire(StatusEffect statusEffect) { if (statusEffect.Turns > 0) statusEffect.Turns--; }
        /// <summary>
        /// Names should be unique per effect. The methods responsible for adding/removing effects look for
        /// a matching name in the list, and then act on the item with that name. So, if a character has a special
        /// Slow debuff, for instance, you might want to rename it to [Name] Slow. Though, I don't do that for now. Hm.
        /// </summary>
        public string Name { get; }
        public int Turns { get; protected set; }

        protected readonly PropertyID Properties;

        public uint Stacks { get; protected set; } = 1;
        public List<Effect> Effects { get => effects; protected set => effects = value; }

        protected uint MaxStacks;
        /// <summary>
        /// A single consequence of a StatusEffect. These can influence stats in many ways, and have 3 values -
        /// the type of the effect, the value, and whether it's percentage based or not.
        /// </summary>
        internal class Effect : ICloneable
        {
            object ICloneable.Clone()
            {
                return (Effect)MemberwiseClone();
            }
            public enum EffectID
            { // KEEP DmgType BONUS AND RES NEXT TO EACHOTHER IN EffectID!!!!!!!!!!!!!!!!!!!!
                Shield,
                ATK,
                DEF,
                SPD,
                DmgBonus,
                DmgTaken,
                Energy,
                DoT,
                DoTTaken,
                Barrier,
                ManaRecovery,
                ManaCost,
                IncomingHealing,
                AllTypeRES,
                BluntBonus,
                BluntRES,
                SlashBonus,
                SlashRES,
                StabBonus,
                StabRES,
                BulletBonus,
                BulletRES,
                BlastBonus,
                BlastRES,
                MagicBonus,
                MagicRES,
            }
            internal EffectID Type = 0;
            internal double Value = 0;
            internal bool IsPercent = false;
            /// <summary>
            /// Generate a new Effect to use inside a StatusEffect.
            /// </summary>
            /// <param name="type">The type of the effect. See <see cref="EffectID"/>.</param>
            /// <param name="value">The value of the effect.</param>
            /// <param name="isPercent">Whether the value is a percentage of a base value.</param>
            internal Effect(EffectID type, double value, bool isPercent)
            {
                Type = type; Value = value; IsPercent = isPercent;
            }
        };
        internal class DamageOverTime : Effect
        {
            public enum DotID
            {
                None,
                Bleed,
                Burn,
                Shock,
                WindShear,
                Flat,
                Pierce
            }
            internal DotID DotType;
            internal bool IgnoreDef = false;
            internal DamageOverTime(DotID type, double value) : base(EffectID.DoT, value, false)
            {
                DotType = type;
                Value = value;
                switch (DotType)
                {
                    case DotID.Bleed:
                        IsPercent = true;
                        break;
                    case DotID.Burn:
                        break;
                    case DotID.Shock:
                        IsPercent = true;
                        break;
                    case DotID.WindShear:
                        IgnoreDef = true;
                        IsPercent = true;
                        break;
                    case DotID.Pierce:
                        IgnoreDef = true;
                        break;
                    default:
                        break;
                }
            }
        }
        internal class Shield : Effect
        {
            internal int BaseValue;
            internal Shield(int value) : base(EffectID.Shield, value, false)
            {
                Value = value;
                BaseValue = value;
            }
            internal double ReduceValue(double damage)
            {
                Value -= damage;
                if (Value < 0)
                {
                    damage = Value * -1;
                    Value = 0;
                    return damage;
                }
                else return 0;
            }
        }

        // This allows me to implement single StatusEffects with multiple different
        // effects [e.g. +20% Atk and -50 Def]
        private List<Effect> effects = new();

        public StatusEffect(string name, PropertyID propertySum, int turns, uint maxStacks, params Effect[] effects)
        {
            if (maxStacks < 1) throw new ArgumentOutOfRangeException(nameof(maxStacks), "MaxStacks must not be null!");
            Name = name;
            Properties = propertySum;
            Turns = turns;
            MaxStacks = maxStacks;
            Effects.AddRange(effects);
            UID = ++_statusEffectID;
        }
        public override string ToString()
        {
            string output = string.Empty;
            if (IsType(PropertyID.Invisible)) return output;
            if (Stacks > 5)
            {
                if (IsType(PropertyID.Debuff)) output += $"↓{Stacks}";
                else output += $"↑{Stacks}";
            }
            else if (IsType(PropertyID.Debuff)) for (int i = 0; i < Stacks; i++) output += '↓';
            else for (int i = 0; i < Stacks; i++) output += '↑';
            if (Turns > 0) output += $"[{Name} ({Turns})]";
            else output += $"[{Name}]";
            return output;
        }
        public bool IsType(PropertyID bit)
        {
            return (Properties & bit) != 0;
        }
        public static string NameOfEffectType(EffectID effectType)
        {
            return effectType.ToString();/*
            return effectType switch
            {
                 0 => "Shield", // Must be positive
                 1 => "ATK",
                 2 => "DEF",
                 3 => "SPD",
                 4 => "All-Type DMG Bonus",
                 5 => "All-Type DMG Resistance",
                 6 => "Energy",
                 7 => "DoT", // Must be positive [also bad]
                 8 => "DoT Vulnerability", // Positive values = Bad
                 9 => "Barrier", // Nullifies Value instances of damage.
                10 => "Mana Recovery",
                11 => "Mana Cost", // Positive values = Bad
                12 => "Incoming Healing",
                // Various DMG Bonuses
                13 => "Blunt DMG Bonus",
                14 => "Slash DMG Bonus",
                15 => "Stab DMG Bonus",
                16 => "Bullet DMG Bonus",
                17 => "Blast DMG Bonus",
                18 => "Magic DMG Bonus",
                // Various DMG Resistances
                19 => "Blunt DMG Resistance",
                20 => "Slash DMG Resistance",
                21 => "Stab DMG Resistance",
                22 => "Bullet DMG Resistance",
                23 => "Blast DMG Resistance",
                24 => "Magic DMG Resistance",
                 _ => throw new ArgumentOutOfRangeException(nameof(effectType), "Unknown effect type")
            };//*/
        }
        /*public static string NameOfEffectType(EffectID effectType)
        {
            return effectType switch
            {
                (EffectID)0     => "Shield", // Must be positive
                (EffectID)1     => "ATK",
                (EffectID)2     => "DEF",
                (EffectID)3     => "SPD",
                (EffectID)4     => "All-Type DMG Bonus",
                (EffectID)5     => "All-Type DMG Resistance",
                (EffectID)6     => "Energy",
                (EffectID)7     => "DoT", // Must be positive [also bad]
                (EffectID)8     => "DoT Vulnerability", // Positive values = Bad
                (EffectID)9     => "Barrier", // Nullifies Value instances of damage.
                (EffectID)10    => "Mana Recovery",
                (EffectID)11    => "Mana Cost", // Positive values = Bad
                (EffectID)12    => "Incoming Healing",
                                // Various DMG Bonuses
                (EffectID)13    => "Blunt DMG Bonus",
                (EffectID)14    => "Slash DMG Bonus",
                (EffectID)15    => "Stab DMG Bonus",
                (EffectID)16    => "Bullet DMG Bonus",
                (EffectID)17    => "Blast DMG Bonus",
                (EffectID)18    => "Magic DMG Bonus",
                                // Various DMG Resistances
                (EffectID)19    => "Blunt DMG Resistance",
                (EffectID)20    => "Slash DMG Resistance",
                (EffectID)21    => "Stab DMG Resistance",
                (EffectID)22    => "Bullet DMG Resistance",
                (EffectID)23    => "Blast DMG Resistance",
                (EffectID)24    => "Magic DMG Resistance",
                _ => throw new ArgumentOutOfRangeException(nameof(effectType), "Unknown effect type")
            };
        }//*/
        public double DamageShield(double damage) // Special method for handling shield damage
        {
            foreach (var item in Effects.OfType<Shield>())
            {
                bool repeat = true;
                while (item.Value > 0 && repeat == true)
                {
                    repeat = false;
                    damage = item.ReduceValue(damage);
                    if (damage == 0) return 0;
                    if (Stacks > 1)
                    {
                        Stacks--;
                        item.Value = item.BaseValue;
                        repeat = true;
                    }
                }
            }
            return damage;
        }
        public bool RemoveBarrier() // Special method for handling barrier damage
        {
            foreach (var item in Effects)
            {
                if (item.Type == EffectID.Barrier && item.Value > 0)
                {
                    item.Value--;
                    return true;
                }
            }
            return false;
        }
        public string GetEffects(string separator)
        {
            string output = string.Empty;
            foreach (Effect effect in Effects)
            {
                if (effect.Value > 0) output += '+';
                if (effect.IsPercent) output += (int)effect.Value*100 + "% " + NameOfEffectType(effect.Type) + separator;
                else output += (int)effect.Value + ' ' + NameOfEffectType(effect.Type) + separator;
            }
            return output;
        }
        public double? GetEffectsOfType(EffectID type, double? baseValue)
        {
            double? value = null;
            foreach (Effect effect in Effects)
            {
                if (effect.Type == type)
                {
                    value ??= 0;
                    if (effect.IsPercent)
                        if (baseValue == null) throw new ArgumentNullException(nameof(baseValue), "Base Value for percentage effects cannot be null!");
                        else value += Stacks * baseValue * effect.Value;
                    else value += Stacks * effect.Value;
                }
            }
            return value;
        }
        public int GetEffectCount()
        {
            return Effects.Count;
        }
        public void AddStacks(uint stacks)
        {
            Stacks += stacks;
            if (Stacks > MaxStacks) Stacks = MaxStacks;
        }
        object ICloneable.Clone()
        {
            return Clone();
        }
        public StatusEffect Clone()
        {
            StatusEffect clone = (StatusEffect)MemberwiseClone();
            clone.ReplaceEffects(Effects);
            return clone;
        }
        // Why are there 5 billion things I need to implement and make them do the same thing bruh
        public static bool operator ==(StatusEffect? a, StatusEffect? b)
        {
            if (a is null && b is null) return true;
            else if (a is null || b is null) return false;
            if (a.UID == b.UID) return true;
            else return false;
        }
        public static bool operator !=(StatusEffect? a, StatusEffect? b)
        {
            return !(a == b);
        }
        public override bool Equals(object? obj)
        {
            if (obj is null) return false;
            if (obj is StatusEffect effect) return effect.UID == UID;
            else return false;
        }
        public override int GetHashCode()
        {
            return HashCode.Combine(Effects.GetHashCode(), Name.GetHashCode(), MaxStacks.GetHashCode());
        }
        // what even is a hash code dawg i just wanted to make removing buffs easier
        /*public int? GetDots(Fighter source, Fighter target)
{
   int? dmg = null;
   foreach (var dot in Effects.OfType<DamageOverTime>())
   {
       dmg ??= 0;
       dmg += target.CalcReceiverDamage(dot.DotType switch
       {
           DotID.Bleed => dmg += (int)dot.Value * target.BaseHp,
           DotID.Burn => dmg += (int)dot.Value,
           DotID.Shock => dmg += (int)dot.Value * source.Atk,
           DotID.WindShear => (int)(dot.Value * source.Atk * Stacks),
           _ => 0
       }, DmgType.None, dot.DotType == DotID.Bleed ? 1 : 0);
   }
   return dmg;
}//*/
    }
}
// waga baba bobo