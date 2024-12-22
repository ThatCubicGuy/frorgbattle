using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FrogBattleV2.Classes.GameLogic;
using DotID = FrogBattleV2.Classes.GameLogic.StatusEffect.DamageOverTime.DotID;
using EffID = FrogBattleV2.Classes.GameLogic.StatusEffect.Effect.EffectID;
using PropID = FrogBattleV2.Classes.GameLogic.StatusEffect.PropertyID;

namespace FrogBattleV2.Classes.Characters
{
    internal class Rexulti : Fighter, ISummons
    {
        private uint FollowUpCount = 0;
        private bool Phase2 = false;
        #region Unique StatusEffects
        private static readonly StatusEffect _pissRain = new("PISS Rain", PropID.Debuff | PropID.Unremovable, 3, 1, new StatusEffect.Effect[]
                {
                    new(EffID.ATK, -0.15, true),
                    new(EffID.DEF, -0.15, true),
                    new StatusEffect.DamageOverTime(DotID.Pierce, 20)
                });
        private static readonly StatusEffect _stabBuff = new("STAB Buff", PropID.Unremovable, 5, 1,
                    new StatusEffect.Effect(EffID.ATK, 0.8, true));
        private static readonly StatusEffect _sinfulCreatureDebuff = new("Sinful Creature", PropID.Debuff, 3, 6, new StatusEffect.Effect[]
                {
                    new(EffID.DmgTaken, 0.15, true)
                });
        private static readonly StatusEffect _bribeDebuff = new("Bribed", PropID.Debuff, 3, 1, new StatusEffect.Effect[]
                {
                    new(EffID.ATK, -0.25, true),
                    new(EffID.DEF, -0.25, true),
                    new(EffID.SPD, -0.25, true),
                });
        private static readonly StatusEffect _evilFoodEater = new("Evil Food Eater", PropID.Unremovable, 3, 1, new StatusEffect.Effect[]
                {
                    new(EffID.ATK, 30, false),
                });
        private static readonly StatusEffect demonDealBurnout = new("Demon Deal Cooldown", PropID.Debuff | PropID.Unremovable | PropID.Invisible, 3, 10);

        private static StatusEffect PissRain => _pissRain.Clone();
        private static StatusEffect StabBuff => _stabBuff.Clone();
        private static StatusEffect SinfulCreatureDebuff => _sinfulCreatureDebuff.Clone();
        private static StatusEffect BribeDebuff => _bribeDebuff.Clone();
        private static StatusEffect EvilFoodEater => _evilFoodEater.Clone();
        private static StatusEffect DemonDealBurnout => demonDealBurnout.Clone();
        #endregion
        public Summon Summoned { get; }
        public Rexulti(string name) : base(name, 1.00, 80, 30, 100, 120)
        {
            Abilities = new()
            {
                new(Ability11, new(mana: 11)),
                new(Ability12, new(mana: 18)),
                new(Ability13, new(mana: 16, health: 100)),
                new(Ability14, new(mana: 22)),
                new(Ability15, new(mana: 20)),
                new(Burst, new(energy: MaxEnergy))
            };
            Summoned = new(new(SummonAction, new()), "Fake friends", this);
            AddEffect(SlashRES);
        }
        #region Phase 1 abilities
        private string Ability11(Fighter target)
        {
            string output = $"{Name} unsheathes their sword!\n";
            if (target.Dodge(this)) return output + target.DodgeMsg;
            GetEnergy(10);
            double dmg = MediumDmg(Atk, DmgType.Slash, target);
            output += $"{Name} slices {target.Name} for {dmg:0.#} damage!";
            if (dmg > Atk - target.Def)
            {
                output += $"\nA critical hit - {target.Name} starts bleeding for {Bleed.Turns} turns!";
                target.AddEffect(Bleed);
            }
            output += target.TakeDamage(dmg, this);
            return output;
        }
        private string Ability12(Fighter target)
        {
            GetEnergy(13);
            string output = $"{Name} calls upon the dark heavens!\n";
            output += $"{Name} summons the lost sould of TODD, frorg battle 1.0 extraordinaire, cursing {target.Name} with piss rain from the ancient times!\n" +
                $"{target.Name}'s ATK and DEF reduced by {PissRain.GetEffectsOfType(EffID.ATK, -100)}% and -{PissRain.GetEffectsOfType(EffID.DoT, 0)} HP per round for {PissRain.Turns} rounds!";
            target.AddEffect(PissRain);
            return output;
        }
        private string Ability13(Fighter target)
        {
            GetEnergy(20);
            string output = $"STAB! What? Why would you do that!? {target.Name} is baffled!\n";
            int dmg = 100;
            TakeTrueDamage(dmg);
            AddEffect(StabBuff);
            output += $"{Name} stabs himself, taking {dmg:0.#} damage but getting a {StabBuff.GetEffectsOfType(EffID.ATK, 100)}% ATK buff for {StabBuff.Turns} turns!";
            return output;
        }
        private string Ability14(Fighter target)
        {
            string output = $"{Name} finds an inherited bug in the code!\n";
            GetEnergy(20);
            double dmg = Atk * DmgRNG;
            output += $"{Name} ignores literally anything that influences the damage {target.Name} should take and directly reduces their HP by {dmg:0.#}!";
            target.TakeTrueDamage(dmg);
            return output;
        }
        private string Ability15(Fighter target)
        {
            string output = $"{Name} loads up his revolver!\n";
            if (target.Dodge(this)) return output + target.DodgeMsg;
            GetEnergy(17);
            double dmg = HeavyDmg(Atk, DmgType.Bullet, target);
            output += $"{Name} shoots a silver bullet straight through {target.Name}'s chest! They take {dmg:0.#} damage and lose 1 buff!";
            output += target.TakeDamage(dmg, this);
            target.CleanseEffects(1, false);
            return output;
        }
        private string Burst(Fighter target)
        {
            string output = $"{Name} is done messing around!\n";
            //Stat changes are done here
            Name += "ulti";
            BaseAtk = 100;
            RegenMana(15);
            double dmg = MediumDmg(Atk, DmgType.Blast, target);
            output += $"{Name} abandons the mortal realm in a big fiery explosion (dealing {dmg:0.#} damage) and uses their body to forge a blade of frog!";
            output += target.TakeDamage(dmg, this);
            output += $"\n{Name} swings once!";
            if (!target.Dodge(this))
            {
                dmg = HeavyDmg(Atk, DmgType.Slash, target, 1);
                output += $"\n{Name} hits {target.Name} with a DEF-ignoring slash for {dmg:0.#} damage!";
                output += target.TakeDamage(dmg, this);
            }
            else output += '\n' + target.DodgeMsg;
            output += $"\n{Name} swings twice!";
            if (!target.Dodge(this))
            {
                dmg = HeavyDmg(Atk, DmgType.Slash, target);
                output += $"\n{target.Name} gets hit with a judgemental slash for {dmg:0.#} damage and gets the \"Sinful Creature\" debuff!";
                output += target.TakeDamage(dmg, this);
                target.AddEffect(SinfulCreatureDebuff);
            }
            else output += '\n' + target.DodgeMsg;
            output += $"\nHaving ascended to a higher plane of existence, {Name} completely reworks his abilites!";
            Abilities = new()
            {
                new(Ability21, new(mana: 14)),
                new(Ability22, new(mana: 15)),
                new(Ability23, new(mana: 20)),
                new(Ability24, new(mana: 25)),
                new(Ability25, new(mana: 23)),
                new(Burst2, new(energy: MaxEnergy))
            };
            Phase2 = true;
            return output;
        }
        #endregion
        #region Phase 2 abilities
        private string Ability21(Fighter target)
        {
            string output = $"{Name} embodies the Demon of Pride and assumes himself the position of dictator!\n";
            if (target.Dodge(this)) return output + target.DodgeMsg;
            GetEnergy(12);
            double dmg = HeavyDmg(Atk, DmgType.None, target);
            output += $"{target.Name} has to suffer though a terrible regime, taking {dmg:0.#} forced labour induced damage!";
            output += target.TakeDamage(dmg, this);
            target.AddEffect(SinfulCreatureDebuff);
            return output;
        }
        private string Ability22(Fighter target)
        {
            string output = $"{Name} sneaks a bribe to the demons!\n";
            GetEnergy(16);
            output += $"{target.Name} is declared guilty and loses {BribeDebuff.GetEffectsOfType(EffID.ATK, -100)}% of their ATK, DEF and SPD!";
            target.AddEffect(BribeDebuff);
            target.AddEffect(SinfulCreatureDebuff);
            return output;
        }
        private string Ability23(Fighter target)
        {
            string output = $"{Name} embodies the Demon of Lust and tries to make a few deals!";
            double dmg = -1;
            for (int i = 1; i <= 3; i++)
            {
                if (target.Dodge(this) || RNG < (ActiveEffects.FirstOrDefault(DemonDealBurnout.Equals)?.Stacks ?? 0) / 10)
                {
                    double dmg2 = LightDmg(Atk, DmgType.Magic, this);
                    output += $"\nDemon #{i} DOESN'T take the deal! He instead deals {(int)dmg2} damage to {Name}!";
                    output += TakeDamage(dmg2, null);
                }
                else
                {
                    GetEnergy(10);
                    dmg = MediumDmg(Atk, DmgType.Magic, target);
                    double manaRegen = RegenMana(5);
                    output += $"\nDemon #{i} TAKES the deal! {target.Name} loses {dmg:0.#} HP while {Name} receives {manaRegen:0.#} mana!";
                    output += target.TakeDamage(dmg, null);
                    AddEffect(DemonDealBurnout);
                }
            }
            if (dmg != -1) target.AddEffect(SinfulCreatureDebuff);
            return output;
        }
        private string Ability24(Fighter target)
        {
            string output = $"{Name} REALLY loads up his revolver!";
            double dmg = -1;
            for (int i = 1; i <= 3; i++)
            {
                if (target.Dodge(this)) output += '\n' + target.DodgeMsg;
                else
                {
                    GetEnergy(12);
                    dmg = HeavyDmg(Atk, DmgType.Bullet, target);
                    output += $"\n{Name} shoots {target.Name}, dealing {dmg:0.#} damage!";
                    output += target.TakeDamage(dmg, this);
                }
            }
            if (dmg != -1)
            {
                output += $"\n{Name} also STEALS a buff from {target.Name}!";
                ActiveEffects.AddRange(target.CleanseEffects(1, false));
                target.AddEffect(SinfulCreatureDebuff);
            }
            return output;
        }
        private string Ability25(Fighter target)
        {
            GetEnergy(15);
            FollowUpCount = 3;
            string output = $"{Name} embodies the Demon of Envy! The people {target.Name} used to know and love have " +
                $"turned against him for {FollowUpCount} turns!";
            return output;
        }
        private string Burst2(Fighter target)
        {
            string output = $"Time to end this!\n{Name} embodies the Demon of Gluttony!\n";
            AddEffect(new("+30 SPD", 0, 1, 1, new StatusEffect.Effect(EffID.SPD, 30, false)));
            if (target.Dodge(this)) return output + target.DodgeMsg + " Impressive!";
            double dmg = HeavyDmg(Atk, DmgType.Magic, target);
            target.TrueStun(2);
            output += $"{Name} traps {target.Name} in HELL! Every demon around comes to take a bite! {target.Name} " +
                $"takes {dmg:0.#} damage and is stunned for {target.StunTime} rounds!\n";
            target.TakeDamage(dmg, this);
            target.AddEffect(SinfulCreatureDebuff, 3);
            AddEffect(EvilFoodEater);
            double manaRegen = RegenMana(35);
            output += $"{Name} also regains {manaRegen:0.#} Mana and gains {EvilFoodEater.GetEffectsOfType(EffID.ATK, 0)} ATK!";
            return output;
        }
        #endregion
        public string SummonAction(Fighter target)
        {
            if (Phase2 && FollowUpCount > 0)
            {
                FollowUpCount--;
                GetEnergy(5);
                string output = $"\n{target.Name}'s insecurities are prayed on by their friends!\n";
                if (target.Dodge(this)) return output + target.DodgeMsg;
                double dmg = MediumDmg(Atk, DmgType.None, target, 0.5);
                output += $"They lose {dmg:0.#} HP!";
                output += target.TakeDamage(dmg, Summoned);
                target.AddEffect(SinfulCreatureDebuff);
                return output;
            }
            else return string.Empty;
        }
    }
}
