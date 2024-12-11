using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogBattleV2.Classes.GameLogic;
using DotID = FrogBattleV2.Classes.GameLogic.StatusEffect.DamageOverTime.DotID;
using EffID = FrogBattleV2.Classes.GameLogic.StatusEffect.Effect.EffectID;
using PropID = FrogBattleV2.Classes.GameLogic.StatusEffect.PropertyID;

namespace FrogBattleV2.Classes.Characters
{
    internal class God : Fighter    // DEBUG class. not actually used in gameplay.
    {
        private static StatusEffect AtkBuff
        {
            get
            {
                return new StatusEffect("ATK Buff", 0, 3, 1, new StatusEffect.Effect(EffID.ATK, 0.2, true));
            }
        }
        private static StatusEffect DefBuff
        {
            get
            {
                return new StatusEffect("DEF Buff", 0, 3, 1, new StatusEffect.Effect(EffID.DEF, 0.2, true));
            }
        }
        private static StatusEffect SpdBuff
        {
            get
            {
                return new StatusEffect("SPD Buff", 0, 3, 1, new StatusEffect.Effect(EffID.SPD, 0.2, true));
            }
        }
        private static StatusEffect DmgDealtBuff
        {
            get
            {
                return new StatusEffect("Dmg Dealt Increase", 0, 3, 1, new StatusEffect.Effect(EffID.DmgBonus, 1, true));
            }
        }
        private static StatusEffect DmgTakenBuff
        {
            get
            {
                return new StatusEffect("Dmg Taken Decrease", 0, 3, 1,
                    new StatusEffect.Effect(EffID.DmgTaken, -1, true));
            }
        }
        private static StatusEffect Shield1
        {
            get
            {
                return new("Shield 1", PropID.None, 10, 1, new StatusEffect.Shield(500));
            }
        }
        private static StatusEffect Shield2
        {
            get
            {
                return new("Shield 2", PropID.None, 10, 1, new StatusEffect.Shield(500));
            }
        }
        private static StatusEffect Shield3
        {
            get
            {
                return new("Shield 3", PropID.None, 10, 1, new StatusEffect.Shield(500));
            }
        }
        private static StatusEffect Shield4
        {
            get
            {
                return new("Shield 4", PropID.None, 10, 3, new StatusEffect.Shield(400));
            }
        }
        public God(string name) : base(name, 1000.0, 100, 50, 100, 100)
        {
            //Abilities.Add(Light);
            //Abilities.Add(Medium);
            //Abilities.Add(Heavy);
            //Abilities.Add(DmgTakenDecrease);
            //Abilities.Add(AddShields);
            //Abilities.Add(AddShield);
        }
        public string Light(Fighter target)
        {
            string output = $"Light Damage based on {Atk} ATK\n";
            double dmg = LightDmg(Atk, 0, target);
            output += $"{target.Name} takes {dmg:0.#} damage";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        public string Medium(Fighter target)
        {
            string output = $"Medium Damage based on {Atk} ATK\n";
            double dmg = MediumDmg(Atk, 0, target);
            output += $"{target.Name} takes {dmg:0.#} damage";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        public string Heavy(Fighter target)
        {
            string output = $"Heavy Damage based on {Atk} ATK\n";
            double dmg = HeavyDmg(Atk, 0, target);
            output += $"{target.Name} takes {dmg:0.#} damage";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        public string DmgTakenDecrease(Fighter target)
        {
            string output = $"Apply buff ({DmgTakenBuff.GetEffectsOfType(EffID.DmgTaken, -100)}% damage taken) for {DmgTakenBuff.Turns} turns\n";
            AddEffect(DmgTakenBuff);
            return output;
        }
        public string AddShields(Fighter target)
        {
            AddEffect(Shield1);
            AddEffect(Shield2);
            AddEffect(Shield3);
            return $"Spawned 3 separate shields with values of {Shield1.GetEffectsOfType(EffID.Shield, null)} each";
        }
        public string AddShield(Fighter target)
        {
            AddEffect(Shield4, 3);
            return $"Spawned 1 shield effect with 3 stacks of value {Shield4.GetEffectsOfType(EffID.Shield, null)}";
        }
    }
}
