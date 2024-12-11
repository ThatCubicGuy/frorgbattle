using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FrogBattleV2.Classes.GameLogic;
using DotID = FrogBattleV2.Classes.GameLogic.StatusEffect.DamageOverTime.DotID;
using EffID = FrogBattleV2.Classes.GameLogic.StatusEffect.Effect.EffectID;
using PropID = FrogBattleV2.Classes.GameLogic.StatusEffect.PropertyID;

namespace FrogBattleV2.Classes.Characters
{
    internal class MamiTomoe : Fighter, ISummons
    {
        public override string BarrierBlock(Fighter? target)
        {
            if (target == null) return $"{Name}'s musket is triggered!";
            double dmg = HeavyDmg(Atk, DmgType.Blast, target);
            string output = $"{Name}'s musket parries the damage and blasts {target.Name} for {dmg:0.#} damage!{target.TakeDamage(dmg, this)}";
            dmg = Heal(dmg);
            GetEnergy(10);
            return output + $"\n{Name} also heals for {dmg:0.#} HP!";
        }
        public Ability Summon { get; }
        #region Unique StatusEffects
        private static readonly StatusEffect musketBarrier = new("Guard Muskets", PropID.Unremovable | PropID.Invisible, 6, 1, new StatusEffect.Effect(EffID.Barrier, 3, false));
        private static readonly StatusEffect cheesed = new("Cheesed", PropID.Debuff, 5, 1);
        private static readonly StatusEffect tangled = new("Tangled", PropID.Debuff, 3, 10, new StatusEffect.Effect[]
                {
                    new(EffID.Energy, -0.1, true),
                    new(EffID.ManaRecovery, -0.1, true)
                });
        protected static readonly StatusEffect lingeringElegance = new("Lingering Elegance", PropID.Debuff, 3, 3, new StatusEffect.Effect[]
                {
                    new(EffID.BulletRES, -0.08, true),
                    new(EffID.BlastRES, -0.15, true)
                });
        private static readonly StatusEffect rage = new("Rage", PropID.Unremovable, -1, 20, new StatusEffect.Effect(EffID.DmgBonus, 0.015, true));
        private static readonly StatusEffect precision = new("Precision", PropID.Unremovable, 4, 20, new StatusEffect.Effect[]
                {
                    new(EffID.ManaRecovery, 0.05, true),
                    new(EffID.BulletBonus, 0.035, true),
                    new(EffID.BlastBonus, 0.05, true),
                    new(EffID.Energy, -0.05, true)
                });
        private static readonly StatusEffect peaceful = new("Peaceful", PropID.Debuff | PropID.Unremovable, 1, 6, new StatusEffect.Effect(EffID.DmgBonus, -0.1, true));
        private static readonly StatusEffect energyCooldown = new("Energy to Rage CD", PropID.Debuff | PropID.Unremovable | PropID.Invisible, 4, 1);
        private static StatusEffect MusketBarrier => musketBarrier.Clone();
        private static StatusEffect Cheesed => cheesed.Clone();
        private static StatusEffect Tangled => tangled.Clone();
        private static StatusEffect LingeringElegance => lingeringElegance.Clone();
        private static StatusEffect Rage => rage.Clone();
        private static StatusEffect Precision => precision.Clone();
        private static StatusEffect Peaceful => peaceful.Clone();
        private static StatusEffect EnergyCooldown => energyCooldown.Clone();
        #endregion
        private uint RageStacks => ActiveEffects.FirstOrDefault(Rage.Equals)?.Stacks ?? 0;
        private uint PrecisionStacks => ActiveEffects.FirstOrDefault(Precision.Equals)?.Stacks ?? 0;
        public MamiTomoe(string name) : base(name, 1.00, 75, 30, 70, 180)
        {
            Abilities = new List<Ability>()
            {
                new(Ability1, new(mana: 12), null),
                new(Ability2, new(mana: 14), null),
                new(Ability3, new(mana: 27), null),
                new(Ability4, new(mana: 25), null),
                new(Ability5, new(mana: 45), null),
                new(Ability6, new(mana: 17), null),
                new(Ability7, new(mana: 33), null),
                new(Burst, new(energy: MaxEnergy), null)
            };
            Summon = new(SummonAction, new(), null);
            AddEffect(BluntRES);
        }
        public override void GetEnergy(double energy)
        {
            if (Energy == MaxEnergy && RageStacks < 20 && !FindEffect(EnergyCooldown.Name))
            {
                AddEffect(Rage);
                CurrentEnergy -= 25;
                AddEffect(EnergyCooldown);
            }
            else base.GetEnergy(energy);
        }
        #region Abilities
        private string Ability1(Fighter target)
        {
            string output = $"{Name} summons some flintlocks!";
            double dmg = -1, totalDmg = 0, nr = 0;
            for (uint i = 4 + PrecisionStacks / 10; i > 0; i--)
            {
                GetEnergy(3);
                if (RNG >= PrecisionStacks / 20.0 && target.Dodge(this))
                {
                    output += '\n' + target.DodgeMsg;
                    AddEffect(Rage);
                }
                else
                {
                    dmg = LightDmg(Atk, DmgType.Bullet, target);
                    totalDmg += dmg;
                    nr++;
                    output += $"\n{target.Name} is shot for {dmg:0.#} damage!";
                    output += target.TakeDamage(dmg, this);
                }
            }
            if (dmg == -1)
            {
                output += $"\n{Name} missed all of her shots!";
                GetEnergy(10);
                AddEffect(Rage);
            }
            else
            {
                output += $"\n{target.Name} takes a total of {totalDmg:0.#} damage!";
            }
            return output;
        }
        private string Ability2(Fighter target)
        {
            GetEnergy(25);
            string output = $"{Name} loads up her double barrel!";
            double dmg = -1;
            for (int i = 0; i < 2; i++)
            {
                if (RNG >= PrecisionStacks / 20.0 && target.Dodge(this))
                {
                    output += '\n' + target.DodgeMsg;
                    AddEffect(Rage, 2);
                }
                else
                {
                    dmg = HeavyDmg(Atk, DmgType.Bullet, target, PrecisionStacks > RageStacks ? PrecisionStacks / 20.0 : RNG / 4);
                    output += $"\n{target.Name} tanks the {i switch { 0 => "first", 1 => "second", _ => "\u0008" }} hit for {dmg:0.#} damage!{target.TakeDamage(dmg, this)}";
                }
            }
            if (dmg == -1)
            {
                output += $"\n{Name} missed both shells!";
                AddEffect(Rage);
            }
            return output;
        }
        private string Ability3(Fighter target)
        {
            AddEffect(MusketBarrier);
            return $"{Name} summons some muskets to protect her!";
        }
        private string Ability4(Fighter target)
        {
            GetEnergy(25);
            string output = $"{Name} shoots {target.Name} with a bullet imbued with Cheese!\n";
            if (RNG >= PrecisionStacks / 20.0 && target.Dodge(this))
            {
                AddEffect(Rage, 3);
                return output + target.DodgeMsg;
            }
            target.AddEffect(Cheesed);
            double dmg = HeavyDmg(Atk, DmgType.Bullet, target, 0.25);
            return output + $"{target.Name} takes {dmg:0.#} damage and is now Cheesed!" + target.TakeDamage(dmg, this);
        }
        private string Ability5(Fighter target)
        {
            string output = $"{Name} can't have {target.Name} ruining everything!\n";
            int nr = 0;
            for (int i = 0; i < 10; i++)
            {
                GetEnergy(3);
                if (RNG >= PrecisionStacks / 20.0 && target.Dodge(this))
                {
                    AddEffect(Rage);
                }
                else
                {
                    nr++;
                    target.AddEffect(Tangled);
                    if (PrecisionStacks > 0) target.AddEffect(Slow);
                    RegenMana(2);
                }
            }
            if (nr == 0)
            {
                output += $"What? {Name} missed every ribbon!? Ridiculous...";
                AddEffect(Rage, 3);
            }
            else output += $"{Name} tangles {target.Name} in {nr} magic ribbons! Energy regen {Tangled.GetEffectsOfType(EffID.Energy, 100) * nr}%," +
                    $" Mana Regen {Tangled.GetEffectsOfType(EffID.ManaRecovery, 100) * nr}%!";
            if (nr == 10)
            {
                output += $"\n{Name} even stuns {target.Name} for 1 turn!";
                target.TrueStun(1);
            }
            return output;
        }
        private string Ability6(Fighter target)
        {
            int x = 3;
            double totalDmg = 0;
            string output = $"{Name} materialises {x} bullets!";
            for (int i = 0; i < x; i++)
            {
                GetEnergy(2);
                if (RNG >= PrecisionStacks / 20.0 && target.Dodge(this))
                {
                    output += '\n' + target.DodgeMsg;
                    if (RNG >= RageStacks / 20.0 && x < 12)
                    {
                        x++;
                        output += $"\n{Name} recalls her bullet!";
                    }
                    AddEffect(Rage);
                }
                else
                {
                    double dmg = LightDmg(Atk, DmgType.Bullet, target);
                    totalDmg += dmg;
                    output += $"\n{Name} fires a ribbon bullet at {target.Name}, dealing {dmg:0.#} damage";
                    if ((PrecisionStacks > 0 ? RNG < PrecisionStacks / 20.0 : RNG < 0.5) && x < 9)
                    {
                        x++;
                        output += " and getting the shot back";
                    }
                    else if (i < 5) AddEffect(Rage);
                    output += '!' + target.TakeDamage(dmg, this);
                }
            }
            if (totalDmg > 0) output += $"\n{Name} dealt a total of {totalDmg:0.#} damage to {target.Name}!";
            else
            {
                output += $"\n{Name} didn't deal any damage! Outrageous!";
                AddEffect(Rage, (uint)(RNG * 4) + 1);
            }
            return output;
        }
        private string Ability7(Fighter target)
        {
            int missed = 0;
            string output = $"{Name} launches a barrage of pellets!";
            for (int i = 0; i < 100; i++)
            {
                if (RNG * 100 < target.Spd - Spd + 30 && RNG >= PrecisionStacks / 20.0) missed++;
            }
            GetEnergy((int)Math.Round(missed / 2.5));
            double dmg = HeavyDmg((100 - missed) * Atk / 50, DmgType.Bullet, target);
            output += $"\n{Name} hits {100 - missed} shots of the {100} fired, dealing {dmg:0.#} damage!{target.TakeDamage(dmg, this)}";
            if (missed > 0)
            {
                output += $"\nEvery miss spawns a ribbon!";
                if (missed >= 80)
                {
                    int stun = missed / 10 - 7;
                    target.TrueStun(stun);
                    output += $"\nThe ribbons overwhelm {target.Name} and stun them for {stun} rounds!";
                }
                if (missed >= 60)
                {
                    double mana = -1 * target.RegenMana((missed + 1) / -4);
                    RegenMana(mana);
                    output += $"\nThe ribbons assault {target.Name} and steal {mana:0.#} mana!";
                }
                else
                {
                    output += $"\nThere aren't enough ribbons to affect {target.Name}!";
                }
            }
            return output;
        }
        private string Burst(Fighter target)
        {
            string output = $"No more running away!\n{Name} unleashes TIRO FINALE!";
            AddEffect(Precision, RageStacks);
            if (RageStacks < 6)
            {
                AddEffect(Peaceful, 6 - RageStacks);
                output += $"\nBut {Name} is not motivated enough... damage reduced by {Peaceful.GetEffectsOfType(EffID.DmgBonus, -100) * (6 - RageStacks)}%!";
            }
            RemoveEffect(Rage);
            double totalDmg = MassiveDmg(Atk, DmgType.Blast, target, 0.6);
            output += $"\n{Name} fires a giant blaster at {target.Name}! They take {totalDmg:0.#} damage!" +
                target.TakeDamage(totalDmg, this) + $"\n{Name} also lines up some supporting muskets! FIRE!";
            for (int i = 0; i < 3; i++)
            {
                double dmg = HeavyDmg(Atk, DmgType.Bullet, target);
                totalDmg += dmg;
                output += $"\n{target.Name} takes {dmg:0.#} damage!" + target.TakeDamage(dmg, this);
            }
            target.AddEffect(LingeringElegance, (PrecisionStacks + 1) / 7);
            return output + $"\n{Name} has dealt a total of {totalDmg:0.#} damage to {target.Name}!";
        }
        #endregion
        public string SummonAction(Fighter target)
        {
            double dmg;
            if (RNG < 0.01)
            {
                dmg = MediumDmg(Atk * 10, 0, target);
                return $"\nCharlotte REALLY NEEDS that cheese and goes a bit overboard, chomping off {target.Name}'s" +
                    $" head and dealing {dmg:0.#} damage!{target.TakeDamage(dmg, null)}";
            }
            if (!target.FindEffect("Cheesed")) return string.Empty;
            GetEnergy(5);
            if (RNG >= PrecisionStacks / 20.0 && target.Dodge(this))
            {
                AddEffect(Rage);
                return '\n' + target.DodgeMsg;
            }
            dmg = MediumDmg(Atk, 0, target);
            return $"\nCharlotte seeks cheese and attacks {target.Name} for {dmg:0.#} damage!" + target.TakeDamage(dmg, null);
        }
    }
}
