using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FrogBattleV2.Classes.GameLogic;
using DotID = FrogBattleV2.Classes.GameLogic.StatusEffect.DamageOverTime.DotID;
using EffID = FrogBattleV2.Classes.GameLogic.StatusEffect.Effect.EffectID;
using PropID = FrogBattleV2.Classes.GameLogic.StatusEffect.PropertyID;

namespace FrogBattleV2.Classes.Characters
{
    internal class Bayonetta : Fighter, ISummons
    {
        private short weapon = 0;
        private bool Phase2 = false;
        protected override string SpecialStat => weaponName[weapon].ToUpper();
        // Stuff unique to Bayonetta
        private readonly string[] weaponName = { "Colour My World", "Ignis Araneae Yo-Yo", "Unforgiven" };
        private readonly List<List<Ability>> Weapons;
        public override string DodgeMsg { get { return $"{Name} dodges the attack while doing a sexy pose!"; } }

        #region Unique StatusEffects
        private static readonly StatusEffect yoyoBuff = new("Yo-Yo speed", PropID.Unremovable, -1, 1,
                    new StatusEffect.Effect(EffID.SPD, 10, false));
        private static readonly StatusEffect mistressDebuff = new("Mistress' Charm", PropID.Debuff, 3, 1, new StatusEffect.Effect[]
                {
                    new(EffID.ATK, -10, false),
                    new(EffID.DEF, -10, false),
                    new StatusEffect.DamageOverTime(DotID.Flat ,20)
                });
        private static readonly StatusEffect umbKissDebuff = new("Umbran Kiss", PropID.Debuff, 3, 1, new StatusEffect.Effect[]
                {
                    new(EffID.ATK, -0.15, true),
                    new(EffID.DEF, -0.15, true),
                    new(EffID.SPD, -0.15, true)
                });
        private static readonly StatusEffect burstDebuff = new("Witch Time", PropID.Unremovable, 4, 1,
                    new StatusEffect.Effect(EffID.Energy, -1, true));
        private static readonly StatusEffect umbraShield = new("Umbra Shield", PropID.None , 4, 1,
                    new StatusEffect.Shield(400));
        private static StatusEffect YoyoBuff => yoyoBuff.Clone();
        private static StatusEffect MistressDebuff => mistressDebuff.Clone();
        private static StatusEffect UmbKissDebuff => umbKissDebuff.Clone();
        private static StatusEffect BurstDebuff => burstDebuff.Clone();
        private static StatusEffect UmbraShield => umbraShield.Clone();
        #endregion
        public Summon Summoned { get; }

        public Bayonetta(string name) : base(name, 1.00, 70, 50, 100, 120)
        {
            AddEffect(MagicRES);
            Ability switchWeapons = new(Ability5, new(mana: 5));
            Ability burst = new(Burst, new(energy: MaxEnergy));
            Weapons = new()
            {
                new()
                {
                    new(Ability11, new(mana: 8)),
                    new(Ability12, new(mana: 25)),
                    new(Ability13, new(mana : 20)),
                    new(Ability14, new(mana: 15)),
                    switchWeapons,
                    burst
                },
                new()
                {
                    new(Ability21, new(mana: 8)),
                    new(Ability22, new(mana: 25)),
                    new(Ability23, new(mana : 20)),
                    new(Ability24, new(mana: 17)),
                    switchWeapons,
                    burst
                },
                new()
                {
                    new(Ability31, new(mana: 10)),
                    new(Ability32, new(mana: 25)),
                    new(Ability33, new(mana : 20)),
                    new(Ability34, new(mana: 75)),
                    switchWeapons,
                    burst
                }
            };
            Summoned = new(new(SummonAction, new()), "Wicked Weave", this);
            Abilities = Weapons[0];
        }
        #region Weapon 1 Abilities
        private uint ability1boost = 0;
        private string Ability11(Fighter target)
        {
            string output = $"{Name} punches and kicks {target.Name}!\n";
            if (target.Dodge(this)) return output + target.DodgeMsg;
            GetEnergy(10);
            double dmg = LightDmg(Atk * (1 + ability1boost++/5.0), DmgType.Blunt, target);
            output += $"{target.Name} is not a skilled boxer! They take {dmg:0.#} damage!";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        private string Ability12(Fighter target)
        {
            if (target.Dodge(this)) return target.DodgeMsg;
            GetEnergy(14);
            target.AddEffect(UmbKissDebuff);
            return $"{Name} gives {target.Name} an umbran kiss! {target.Name}'s ATK, DEF and SPD reduced " +
                $"by {-UmbKissDebuff.GetEffectsOfType(EffID.ATK, 100)}% for {UmbKissDebuff.Turns} rounds!";
        }
        private string Ability13(Fighter target)
        {
            GetEnergy(15);
            AddEffect(UmbraShield);
            return $"{Name} summons Umbra Clock Tower! They will be shielded " +
                $"from {UmbraShield.GetEffectsOfType(EffID.Shield, BaseHp)} damage for {UmbraShield.Turns - 1} turns!";
        }
        private string Ability14(Fighter target)
        {
            GetEnergy(15);
            double dmg = MediumDmg(Atk, DmgType.Bullet, target);
            return $"{Name} shoots a flurry of bullets everywhere! " +
                $"{target.Name} cannot dodge them all and takes {dmg:0.#} damage!{target.TakeDamage(dmg, this)}";
        }
        #endregion
        #region Weapon 2 Abilities
        private string Ability21(Fighter target)
        {
            string output = $"{Name} throws her Yo-Yo at {target.Name}!\n";
            if (target.Dodge(this)) return target.DodgeMsg;
            GetEnergy(10);
            double dmg = MediumDmg(Atk, DmgType.Blunt, target);
            return output + $"{Name} smacks {target.Name} right in the face, dealing {dmg:0.#} damage!{target.TakeDamage(dmg, this)}";
        }
        private string Ability22(Fighter target)
        {
            if (target.Dodge(this)) return target.DodgeMsg;
            int turns = (int)Math.Floor(RNG * 3) + 1;
            return $"{Name} shoots stunning webs at {target.Name}!" + target.Stun(turns, this);
        }
        private string Ability23(Fighter target)
        {
            string output = $"{Name} does some fancy Yo-Yo tricks!\n";
            if (target.Dodge(this)) return output + target.DodgeMsg;
            GetEnergy(25);
            double dmg = HeavyDmg(Atk, DmgType.Slash, target);
            target.AddEffect(Bleed);
            return output + $"{Name} cuts {target.Name}'s throat with the Yo-Yo's rope! " +
                $"They take {dmg:0.#} damage and bleed for {Bleed.Turns} rounds!{target.TakeDamage(dmg, this)}";
        }
        private string Ability24(Fighter target)
        {
            string output = $"{Name} transforms into a spider and stabs the ground, creating a massive explosion!\n";
            if (target.Dodge(this)) return output + target.DodgeMsg;
            GetEnergy(25);
            double dmg = HeavyDmg(Atk, DmgType.Blast, target);
            return output + $"{target.Name} is in the blast radius, so they take {dmg:0.#} damage!{target.TakeDamage(dmg, this)}";
        }
        #endregion
        #region Weapon 3 Abilities
        private string Ability31(Fighter target)
        {
            if (target.Dodge(this)) return target.DodgeMsg;
            GetEnergy(16);
            target.AddEffect(MistressDebuff);
            return $"{Name} summons the Mistress of Time and Sun, cursing {target.Name} for {MistressDebuff.Turns} rounds!";
        }
        private string Ability32(Fighter target)
        {
            if (target.Dodge(this)) return target.DodgeMsg;
            GetEnergy(18);
            double stealRatio = 0.1;
            AddEffect(new("Demon-Proof", PropID.None, 4, 1, new StatusEffect.Effect[]
            {
                    new(EffID.ATK, target.Atk * stealRatio, false),
                    new(EffID.DEF, target.Def * stealRatio, false),
                    new(EffID.SPD, target.Spd * stealRatio, false),
            }));
            return $"{Name} sprays {target.Name} with demon repellant and inherits some of their stats!";
        }
        private string Ability33(Fighter target)
        {
            if (target.Dodge(this)) return target.DodgeMsg;
            GetEnergy(20);
            double dmg = HeavyDmg(Spd, DmgType.None, target, 0.4);
            return $"{Name} does a very sexy dance! Unfortunately, {target.Name} is " +
                $"too unsexy to appreciate it, and takes {dmg:0.#} emotional damage!{target.TakeDamage(dmg, this)}";
        }
        private string Ability34(Fighter target)
        {
            string output = $"{Name} smashes their Umbran Watch!";
            for (int i = 0; i < 3; i++)
            {
                if (target.Dodge(this)) output += '\n' + target.DodgeMsg;
                else
                {
                    GetEnergy(12);
                    double dmg = HeavyDmg(Atk, DmgType.Magic, target);
                    output += $"\n{target.Name} takes {dmg:0.#} Umbra damage!{target.TakeDamage(dmg, this)}";
                }
            }
            BaseAtk = 100;
            BaseDef = 25;
            Phase2 = true;
            Weapons[2][3] = Abilities[3] = new(Ability342, new(mana: 20));
            return output + $"\n{Name} gets serious! {Summoned.Name} is summoned!";
        }
        private string Ability342(Fighter target)
        {
            GetEnergy(15);
            double dmg = HeavyDmg(Atk, DmgType.Magic, target);
            return $"{Name} deals {dmg:0.#} Umbra damage to {target.Name}!{target.TakeDamage(dmg, this)}";
        }
        #endregion
        private string Ability5(Fighter target) //Switch weapons
        {
            GetEnergy(3);
            if (weapon == 0)
            {
                weapon = 1;
                AddEffect(YoyoBuff);
            }
            else if (weapon == 1)
            {
                weapon = 2;
                if (!RemoveEffect(YoyoBuff)) throw new Exception("Unable to remove YoyoBuff!");
            }
            else
            {
                weapon = 0;
            }
            Abilities = Weapons[weapon];
            return $"{Name} switches their weapon to \"{weaponName[weapon]}\"!";
        }
        public string SummonAction(Fighter target)
        {
            if (!Phase2) return string.Empty;
            GetEnergy(5);
            string output = $"\n{Summoned.Name} charges at {target.Name}!\n";
            if (target.Dodge(Summoned)) return output + target.DodgeMsg;
            double dmg = MediumDmg(Summoned.Atk, DmgType.Magic, target);
            output += $"They get hit for {dmg:0.#} damage!{target.TakeDamage(dmg, Summoned)}";
            return output;
        }
        public string Burst(Fighter target)
        {
            int turns = 4;
            target.TrueStun(turns);
            double manaRegen = RegenMana(30);
            CurrentEnergy = 0;
            AddEffect(BurstDebuff); // Bayonetta is not allowed to regenerate energy during her burst due to a near 
                                    // perfect feedback loop which allows her to regenerate 100 energy for an
                                    // effective cost of 18 mana. I don't want her to have infinite stun.
            return $"{Name} uses their BURST!\n{Name} activates Witch Time and freezes {target.Name} in time for " +
                $"{turns} turns and regenerates {manaRegen:F1} mana!";
        }
    }
}
