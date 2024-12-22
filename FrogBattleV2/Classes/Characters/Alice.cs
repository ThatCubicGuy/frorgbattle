using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogBattleV2.Classes.GameLogic;
using DotID = FrogBattleV2.Classes.GameLogic.StatusEffect.DamageOverTime.DotID;
using EffID = FrogBattleV2.Classes.GameLogic.StatusEffect.Effect.EffectID;
using PropID = FrogBattleV2.Classes.GameLogic.StatusEffect.PropertyID;
using static FrogBattleV2.Classes.GameLogic.StatusEffect;

namespace FrogBattleV2.Classes.Characters
{
    internal class Alice : Fighter, IFollowsUp
    {
        private uint Raining = 0;
        private uint DemonDice = 0;
        private double totalDmg;
        private bool Phase2 = false;
        private readonly int DressType;
        private readonly string[] DressNames = { "Classic", "Checkmate", "Fleshmaiden", "Cheshire" };
        private Demons ActiveDemon;
        private enum Demons
        {
            Mad_Hatter,
            Cheshire_Cat
        }

        #region Unique StatusEffects

        private static readonly StatusEffect knifeRain = new("Knife Rain", PropID.None, 2, 3, new Effect[]
                {
                    new(EffID.ATK, 10, false),
                    new(EffID.DEF, 5, false),
                    new(EffID.SPD, 1, false)
                });
        private static readonly StatusEffect checkmate = new("Checkmate", PropID.Unremovable | PropID.Invisible, -1, 1, new Effect[]
                {
                    new(EffID.SlashBonus, 1, true),
                    new(EffID.BulletBonus, 1, true)
                });
        private StatusEffect Hysteria
        {
            get
            {
                if (DressType == 2) return new("Hysteria", PropID.Unremovable, 4, 1, new Effect[]
                {
                    new(EffID.ATK, 0.5 - CurrentEnergy / 100.0, true),
                    new(EffID.DEF, 0.15, true),
                    new(EffID.SPD, 0.15, true),
                    new(EffID.ManaRecovery, 0.15, true),
                    new(EffID.AllTypeRES, 0.75, true)
                });
                else return new("Hysteria", PropID.Unremovable, 4, 1, new Effect[]
                {
                    new(EffID.ATK, 0.35 - CurrentEnergy / 100.0, true),
                    new(EffID.AllTypeRES, 0.75, true),
                });
            }
        }
        private static StatusEffect KnifeRain => knifeRain.Clone();
        private static StatusEffect Checkmate => checkmate.Clone();

        #endregion

        public Ability FollowUp { get; }
        private string ActiveDemonName { get { return ActiveDemon.ToString().Replace('_', ' '); } }
        protected override string SpecialStat { get { return DressNames[DressType] + " Dress" + (Phase2 ? " --- " + ActiveDemonName : string.Empty); } }
        public Alice(string name) : base(name, 0.375, 70, 40, 100, 100)
        {
            CurrentEnergy = MaxEnergy;
            Abilities = new List<Ability>()
            {
                new(Ability1, new(mana: 12)),
                new(Ability2, new(mana: 18)),
                new(Ability3, new(mana: 30), (user) => Raining > 0 ? AbilityBlocked : AbilityUsable),
                new(Ability4, new(mana: 16)),
                new(Ability5, new(mana: 24)),
                new(Burst, new(energy: MaxEnergy, costType: Cost.CostType.ReverseCost))
            };
            FollowUp = new(FollowUpAction, new());
            AddEffect(AllTypeRES);
            DressType = (int)Math.Floor(RNG * DressNames.Length);
            switch (DressType)
            {
                case 0:
                    break;
                case 1:
                    AddEffect(Checkmate);
                    break;
                case 2:
                    break;
                case 3:
                    break;
                default:
                    break;
            }
        }
        public override void GetEnergy(double energy)
        {
            base.GetEnergy(energy / -2);
        }
        
        public override string TakeDamage(double damage, Fighter? source)
        {
            return base.TakeDamage(damage, source) + (!Phase2 && CurrentHp <= 0 ? GoPhase2() : string.Empty);
        }
        private string GoPhase2()
        {
            Phase2 = true;
            CurrentHp = BaseHp = 2000;
            ActiveDemon = (Demons)(int)Math.Floor(RNG * Enum.GetNames(typeof(Demons)).Length);
            return $"\n{Name} won't go down so easily! The {ActiveDemonName} is summoned and lends her a hand, reviving her for {BaseHp} HP!";
        }
        protected override string SkipTurnAction(Fighter target)
        {
            totalDmg = 0;
            return base.SkipTurnAction(target);
        }
        private string Ability1(Fighter target)
        {
            totalDmg = 0;
            string output = $"{Name} pulls out her Vorpal Blade!";
            for (int i = 0; i < 4; i++)
            {
                if (target.Dodge(this)) output += '\n' + target.DodgeMsg;
                else
                {
                    GetEnergy(3);
                    double dmg = LightDmg(Atk, DmgType.Slash, target);
                    totalDmg += dmg;
                    output += $"\n{Name} slashes {target.Name} for {dmg:0.#} damage!" +
                        target.TakeDamage(dmg, this);
                }
            }
            if (totalDmg == 0) return output + "\nQuite the opponent...";
            return output + $"\n{Name} dealt a total of {totalDmg:0.#} damage to {target.Name}!";
        }
        private string Ability2(Fighter target)
        {
            totalDmg = 0;
            string output = $"{Name} readies her Pepper Grinder!";
            if (target.Dodge(this)) return output + '\n' + target.DodgeMsg;
            double dmg = totalDmg = MediumDmg(Atk, DmgType.Bullet, target);
            return output + $"\n{Name} fires a powerful bullet at {target.Name}, dealing {dmg:0.#} damage!" +
                target.TakeDamage(dmg, this) + target.Stun(2, this);
        }
        private string Ability3(Fighter target)
        {
            Raining = 3;
            return $"{Name} throws a bunch of blades into the air!" +
                $"\nKnives start falling from the sky for {Raining} rounds!";
        }
        private string Ability4(Fighter target)
        {
            totalDmg = 0;
            string output = $"{Name} wants to make some tea!\n";
            if (target.Dodge(this)) return output + target.DodgeMsg;
            else
            {
                GetEnergy(4);
                double dmg = totalDmg = HeavyDmg(Atk, DmgType.Magic, target);
                target.AddEffect(Burn);
                return output + $"{Name} \"accidentally\" touches {target.Name} with the steaming teapot, " +
                    $"dealing {dmg:0.#} damage and burning them for {Burn.Turns} rounds!{target.TakeDamage(dmg, this)}";
            }
        }
        private string Ability5(Fighter target)
        {
            DemonDice = (uint)Math.Floor(RNG * 6 + 1);
            return $"{Name} rolls a dice and summons a demon to fight by her side for {DemonDice} turns!";
        }
        private string Burst(Fighter target)
        {
            AddEffect(Hysteria);
            string output = $"{Name} is going insane! She gains a {Hysteria.GetEffectsOfType(EffID.ATK, 100)}% ATK boost and a " +
                $"{Hysteria.GetEffectsOfType(EffID.AllTypeRES, 100)}% DMG resistance boost for {Hysteria.Turns - 1} turns!";
            CurrentEnergy = MaxEnergy;
            totalDmg = 200;
            return output;
        }
        private string RoseFollowUp()
        {
            if (RNG < 0.1 && DressType != 3)
            {
                return $"\n{Name} collects a rose and heals for {Heal(totalDmg / 2):0.#} HP!";
            }
            else return string.Empty;
        }
        private string DemonFollowUp(Fighter target)
        {
            if (DemonDice > 0)
            {
                string output = $"\nThe demon attacks alongside {Name}!\n";
                DemonDice--;
                if (DemonDice != 0) totalDmg /= 2;
                if (target.Dodge(this)) return output + target.DodgeMsg;
                else return output + $"He deals {totalDmg:0.#} damage to {target.Name}!{target.TakeDamage(totalDmg, null)}";
            }
            else return string.Empty;
        }
        private string RainFollowUp(Fighter target)
        {
            if (Raining > 0)
            {
                totalDmg = 0;
                string output = $"\nThe knives befall {target.Name}!";
                for (int i = 0; i < 5; i++)
                {
                    if (target.Dodge(this)) output += '\n' + target.DodgeMsg;
                    else
                    {
                        GetEnergy(1);
                        double dmg = LightDmg(Atk, DmgType.Stab, target);
                        totalDmg += dmg;
                        output += target.TakeDamage(dmg, this);
                        //output += $"\nThe {i + 1}{i switch { 0 => "st", 1 => "nd", 2 => "rd", _ => "th" }} " +
                        //$"knife hits {target.Name} for {dmg:0.#} damage!{target.TakeDamage(dmg, this)}";
                    }
                }
                Raining--;
                AddEffect(KnifeRain);
                return output + $"\nThey deal {totalDmg:0.#} total damage to {target.Name}!" +
                    $"\n{Name} also gets {Heal(20):0.#} HP and {RegenMana(10):0.#} mana!";
            }
            else return string.Empty;
        }
        public string FollowUpAction(Fighter target)
        {
            return RoseFollowUp() + DemonFollowUp(target) + RainFollowUp(target) + (Phase2 ? Phase2FollowUp(target) : string.Empty);
        }
        private string Phase2FollowUp(Fighter target)
        {
            totalDmg = ActiveDemon switch
            {
                Demons.Mad_Hatter => HeavyDmg(Atk, DmgType.None, target),
                Demons.Cheshire_Cat => MediumDmg(Atk, DmgType.None, target),
                _ => 999999999
            };
            return $"\n{ActiveDemonName} attacks {target.Name}, dealing {totalDmg:0.#} damage!{target.TakeDamage(totalDmg, this)}";
        }
    }
}
