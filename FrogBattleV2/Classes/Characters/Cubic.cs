using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogBattleV2.Classes.GameLogic;
using EffID = FrogBattleV2.Classes.GameLogic.StatusEffect.Effect.EffectID;
using PropID = FrogBattleV2.Classes.GameLogic.StatusEffect.PropertyID;

namespace FrogBattleV2.Classes.Characters
{
    internal class Cubic : Fighter, IFollowsUp
    {
        private uint Phase = 1;
        private bool Charging = false;
        public override string DodgeMsg
        {
            get
            {
                if (RNG < 0.5) return $"{Name} slides to the left!"; else return $"{Name} slides to the right!";
            }
        }
        public Ability FollowUp { get; }

        #region Unique / reworked StatusEffects

        private static readonly StatusEffect boneShield = new("Bone Shield", PropID.Unremovable, 6, 1,
                new StatusEffect.Shield(300));
        private static readonly StatusEffect healDebuff = new("Incoming Healing Penalty", PropID.Debuff | PropID.Unremovable | PropID.Invisible, 7, 3,
                new StatusEffect.Effect(EffID.IncomingHealing, -0.25, true));
        protected static readonly StatusEffect _dmgIncrease = new("Dmg Increase", PropID.None, 5, 5,
                new StatusEffect.Effect(EffID.DmgBonus, 0.15, true));
        private static readonly StatusEffect defBuff = new("DEF Increase", PropID.None, 4, 1,
                new StatusEffect.Effect[]
                {
                    new(EffID.DEF, 20, false),
                    new(EffID.ATK, -10, false),
                });
        private static readonly StatusEffect manaRegenBuff = new("Increased Mana Recharge", PropID.None, 4, 1,
                new StatusEffect.Effect(EffID.ManaRecovery, 1, true));
        private static readonly StatusEffect manaCostDebuff = new("Increased Mana Cost", PropID.Debuff, 4, 1,
                new StatusEffect.Effect(EffID.ManaCost, 0.5, true));
        private static readonly StatusEffect doubleDmgDealt = new("DMG Dealt Doubled", PropID.None, 2, 1,
                new StatusEffect.Effect(EffID.DmgBonus, 1, true));
        private static readonly StatusEffect doubleDmgTaken = new("DMG Taken Doubled", PropID.Debuff, 2, 1,
                new StatusEffect.Effect(EffID.DmgTaken, 1, true));
        private static readonly StatusEffect determination = new("Determination", PropID.Unremovable, -1, 1,
                new StatusEffect.Effect(EffID.DoT, 50, false));
        private static readonly StatusEffect phase2EnergyBuff = new("Energy Recharge", PropID.Unremovable | PropID.Invisible, -1, 1,
                new StatusEffect.Effect(EffID.Energy, 1.5, true));
        private static readonly StatusEffect afraidDebuff = new("AFRAID", PropID.Debuff, -1, 1,
                new StatusEffect.Effect(EffID.DEF, -1, true));
        private static readonly StatusEffect blasterAtkBuff = new("Intimidating", PropID.Unremovable, -1, 5,
                new StatusEffect.Effect(EffID.ATK, 0.15, true));
        private static StatusEffect BoneShield => boneShield.Clone();
        private static StatusEffect HealDebuff => healDebuff.Clone();
        protected override StatusEffect DmgIncrease => _dmgIncrease.Clone();
        private static StatusEffect DefBuff => defBuff.Clone();
        private static StatusEffect ManaRegenBuff => manaRegenBuff.Clone();
        private static StatusEffect ManaCostDebuff => manaCostDebuff.Clone();
        private static StatusEffect DoubleDmgDealt => doubleDmgDealt.Clone();
        private static StatusEffect DoubleDmgTaken => doubleDmgTaken.Clone();
        private static StatusEffect Determination => determination.Clone();
        private static StatusEffect Phase2EnergyBuff => phase2EnergyBuff.Clone();
        private static StatusEffect AfraidDebuff => afraidDebuff.Clone();
        private static StatusEffect BlasterAtkBuff => blasterAtkBuff.Clone();

        #endregion

        public override double Hp
        {
            get
            {
                if (Energy == MaxEnergy && CurrentHp <= 0)
                {
                    CurrentHp = 1;
                    if (Phase == 3) throw new ApplicationException($"So... I guess this is the end. {Name} fades away into a pile of dust.", GameEndMechanic);
                }
                return base.Hp;
            }
        }


        public Cubic(string name) : base(name, 0.75, 60, 0, 250, 250)
        {
            CurrentMana = 60;
            AddEffect(StabRES);
            Abilities = new List<Ability>()
            {
                new(Ability11, new(mana: 20), null),
                new(Ability12, new(mana: 20), null),
                new(Ability13, new(mana: 20), null),
                new(Ability14, new(mana: 15), null),
                new(Ability15, new(mana: 20), null)
            };
            FollowUp = new(FollowUpAction, new(), null);
        }
        #region Overrides
        public override bool Dodge(Fighter target)
        { // Cubic is guaranteed to dodge in his first phase, so you just need to spam weak attacks to reduce his speed.
          // Regenerating mana each time helps with this. Cubic gets a damage headstart, while the enemy doesn't have to
          // lose all of their mana doing 0 damage.
            if (Phase == 1)
            {
                if (Spd - target.Spd > RNG * 100)
                {
                    int value = 5;
                    BaseSpd -= value;
                    target.RegenMana(value);
                    return true;
                }
                else return false;
            }
            else return base.Dodge(target);
        }
        public override string TakeDamage(double damage, Fighter? target)
        { // Cubic switches phases if he takes damage during phase 1
            if (Phase == 1)
            {
                if (damage <= 0) return base.TakeDamage(0, target);
                Phase2();
                base.TakeDamage(damage, this);
                return $"\nD-did you just hit {Name}!? How could you? It.. hurt...\nThey enter the WOUNDED phase, turning their excess SPD into HP and Mana!";
            }
            else if (Phase == 2)
            {
                if (Energy == MaxEnergy) return base.TakeDamage(damage, this) + Phase3();
                else return base.TakeDamage(damage, this);
            }
            else if (Phase == 3)
            {
                int atkGain = (int)Math.Round(damage / 25.0);
                BaseAtk += atkGain;
                return base.TakeDamage(damage, this) + $"\n{Name}'s ATK increases by {atkGain:0.#}!";
            }
            else throw new ApplicationException($"Unknown phase??? ({Phase})");
        }
        #endregion
        #region Phase 1 Abilities
        public string Ability11(Fighter target)
        {
            string output = $"{Name} throws a few knives in {target.Name}'s direction!";
            double dmg = -1;
            double total = 0;
            for (int i = 0; i < 5; i++)
            {
                if (target.Dodge(this)) output += '\n' + target.DodgeMsg;
                else
                {
                    dmg = LightDmg(Atk, DmgType.Stab, target, 0.2);
                    output += $"\n{target.Name} is hit for {dmg:0.#} damage!";
                    output += target.TakeDamage(dmg, this);
                    total += dmg;
                }
            }
            if (dmg == -1) output += $"\n{target.Name} dodged everything! Impressive!";
            else output += $"\n{Name} deals a total of {total:0.#} damage to {target.Name}!";
            return output;
        }
        public string Ability12(Fighter target)
        {
            string output = $"{Name} summons some gaster blasters!";
            double dmg = -1;
            double total = 0;
            for (int i = 0; i < 3; i++)
            {
                if (target.Dodge(this)) output += '\n' + target.DodgeMsg;
                else
                {
                    dmg = MediumDmg(Atk, DmgType.Blast, target);
                    output += $"\n{target.Name} is hit for {dmg:0.#} damage!";
                    output += target.TakeDamage(dmg, this);
                    total += dmg;
                }
            }
            if (dmg == -1) output += $"\n{target.Name} dodged everything! Impressive!";
            else output += $"\n{Name} deals a total of {total:0.#} damage to {target.Name}!";
            return output;
        }
        public string Ability13(Fighter target)
        {
            string output = $"{Name} picks up a RED knife!\n";
            if (target.Dodge(this)) return output + target.DodgeMsg;
            double dmg = MediumDmg(Atk, DmgType.Stab, target, 0.5);
            output += $"{target.Name} gets stabbed for {dmg:0.#} damage!" + target.TakeDamage(dmg, this) +
                $"\n{Name} also gets a {ManaRegenBuff.GetEffectsOfType(EffID.ManaRecovery, 100)}% Mana Recharge buff!";
            AddEffect(ManaRegenBuff);
            return output;
        }
        public string Ability14(Fighter target)
        {
            string output = $"{Name} throws a D10!\n";
            int die = (int)(RNG * 10) + 1;
            output += $"{Name} rolled a {die}!\n";
            double dmg;
            switch (die)
            {
                case 1:
                    output += $"Deplorable... {Name} gets a {ManaCostDebuff.GetEffectsOfType(EffID.ManaCost, 100)}% Mana cost increase for {ManaCostDebuff.Turns - 1} turns!";
                    AddEffect(ManaCostDebuff);
                    break;
                case 2:
                    output += $"BAD! {Name} gets a {Weaken.GetEffectsOfType(EffID.DmgBonus, -100)}% DMG reduction for {Weaken.Turns - 1} turns!";
                    AddEffect(Weaken);
                    break;
                case 3:
                    output += $"Poor! {Name} loses 1 buff!";
                    CleanseEffects(1, false);
                    break;
                case 4:
                    output += $"Below average! {Name} doesn't gain anything.";
                    break;
                case 5:
                    output += $"Mediocre! {Name} launches a weak attack.\n";
                    if (target.Dodge(this)) output += target.DodgeMsg;
                    dmg = LightDmg(Atk, DmgType.Blunt, target);
                    output += $"{Name} deals some light damage worth {dmg:0.#} points.";
                    output += target.TakeDamage(dmg, this);
                    break;
                case 6:
                    output += $"Average. {Name} launches a regular attack.\n";
                    if (target.Dodge(this)) output += target.DodgeMsg;
                    dmg = MediumDmg(Atk, DmgType.Slash, target);
                    output += $"{Name} deals some medium damage worth {dmg:0.#} points.";
                    output += target.TakeDamage(dmg, this);
                    break;
                case 7:
                    output += $"Above expectations - {Name} launches a powerful attack!\n";
                    if (target.Dodge(this)) output += target.DodgeMsg;
                    double ignoreValue = 0.5;
                    dmg = MediumDmg(Atk, DmgType.Stab, target, ignoreValue);
                    output += $"{Name} ignores {ignoreValue * 100}% DEF and deals {dmg:0.#} damage.";
                    output += target.TakeDamage(dmg, this);
                    break;
                case 8:
                    output += $"Pretty good! {Name} launches a recovery attack!\n";
                    if (target.Dodge(this)) output += target.DodgeMsg;
                    dmg = HeavyDmg(Atk, DmgType.Magic, target);
                    output += $"{Name} deals {dmg:0.#} damage and restores {Heal(dmg):0.#} HP!";
                    output += target.TakeDamage(dmg, this);
                    break;
                case 9:
                    dmg = HeavyDmg(Atk, DmgType.Magic, target, 0.5);
                    double dmg2 = LightDmg(Atk, DmgType.Magic, target);
                    output += $"Great! {Name} launches an undodgeable attack and deals {dmg2:0.#} light damage before actually " +
                        $"trying and dealing {dmg:0.#} damage!{target.TakeDamage(dmg2, this)}{target.TakeDamage(dmg, this)}";
                    break;
                case 10:
                    output += $"Splendid! {Name} increases the damage taken by {target.Name} by {DoubleDmgTaken.GetEffectsOfType(EffID.DmgTaken, -100)}%, and his own damage by another {DoubleDmgDealt.GetEffectsOfType(EffID.DmgBonus, 100)}% for 1 turn!";
                    target.AddEffect(DoubleDmgTaken);
                    AddEffect(DoubleDmgDealt);
                    RegenMana(20);
                    break;
                default:
                    output += "...Nothing happened..? How peculiar.";
                    break;
            }
            return output;
        }
        public string Ability15(Fighter target)
        {
            AddEffect(DmgIncrease);
            return $"{Name} gets MAD! They get a stackable {DmgIncrease.GetEffectsOfType(EffID.DmgBonus, 100)}% DMG Increase buff!";
        }
        #endregion
        #region Phase 2 Abilities
        public string Ability21(Fighter target)
        {
            GetEnergy(10);
            double healing = Heal(400);
            CleanseEffects(1, true);
            AddEffect(HealDebuff);
            return $"{Name} prays to the lost spirit of FRORG! They are blessed with {healing:0.#} HP! FRORG also dispells 1 debuff from them!";
        }
        public string Ability22(Fighter target)
        {
            GetEnergy(20);
            Charging = true;
            return $"{Name} summons a giant gaster blaster which starts charging!";
        }
        public string Ability23(Fighter target)
        {
            string output = $"{Name} tries reasoning with {target.Name}.\n";
            if (target.Dodge(this)) return $"{output}{target.Name} was not swayed by {Name}'s gobbledygook!\n{target.DodgeMsg}";
            output += $"{Name} convinces {target.Name} to chill out a little! Their DMG dealt is reduced by " +
                $"{Weaken.GetEffectsOfType(EffID.DmgBonus, -100)}% for {Weaken.Turns} turns!";
            target.AddEffect(Weaken);
            return output;
        }
        public string Ability24(Fighter target)
        {
            string output = $"{Name} is tired from all this fighting! They increase their DEF by {DefBuff.GetEffectsOfType(EffID.DEF, 100)}," +
                $" but reduce their ATK by {DefBuff.GetEffectsOfType(EffID.ATK, -100)}!";
            AddEffect(DefBuff);
            return output;
        }
        public string Ability25(Fighter target)
        {
            AddEffect(BoneShield);
            return $"{Name} puts up a giant barricade made of bones! They will be shielded from {BoneShield.GetEffectsOfType(EffID.Shield, 0)} damage!";
        }
        #endregion
        #region Phase 3 Abilities
        public string Ability31(Fighter target)
        {
            string output = $"{Name} uses 3 random abilities!\n\n";
            double tempMana = Mana;
            CurrentMana = 100000;
            for(int i = 0; i < 3; i++)
            {
                int selector = (int)Math.Floor(RNG * 10);
                var result = PlayTurn(target, selector);
                if (!result.IsUsable) i--;
            }
            CurrentMana = tempMana;
            return output;
        }
        public string Ability32(Fighter target)
        {
            target.AddEffect(AfraidDebuff);
            return $"{Name} makes {target.Name} AFRAID! {AfraidDebuff.GetEffectsOfType(EffID.DEF, 100)}% DEF!";
        }
        #endregion
        public string FollowUpAction(Fighter target)
        {
            if (Charging && RNG < 0.2)
            {
                Charging = false;
                double dmg = HeavyDmg(Atk, DmgType.Blast, target, 0.5) + HeavyDmg(Atk, DmgType.Blast, target, 0.5);
                string output = $"\n{target.Name} also gets BLASTED by the previously charging blaster for {dmg:0.#} damage!";
                output += target.TakeDamage(dmg, this);
                if (!target.Dodge(this))
                {
                    output += $" They also have their DEF reduced by {DefShred.GetEffectsOfType(EffID.DEF, -100)}% for {DefShred.Turns} turns!";
                    target.AddEffect(DefShred);
                }
                RemoveEffect(BlasterAtkBuff);
                return output;
            }
            else AddEffect(BlasterAtkBuff);
            return string.Empty;
        }
        private void Phase2()
        {
            Abilities.AddRange(new List<Ability>()
            {
                new(Ability21, new(mana: 40), null),
                new(Ability22, new(mana: 20), null, user => Charging ? AbilityBlocked : AbilityUsable),
                new(Ability23, new(mana: 15), null),
                new(Ability24, new(mana: 20), null),
                new(Ability25, new(mana: 22), null)
            });
            Phase = 2;
            BaseHp += (BaseSpd - 100) * 10;
            CurrentHp = BaseHp;
            RegenMana((BaseSpd - 100) / 2);
            BaseSpd = 100;
            BaseAtk = 80;
            BaseDef = 30;
            AddEffect(Phase2EnergyBuff);
        }
        private string Phase3()
        {
            // This should never be the case, but maybe I'm debugging and
            // just put Phase3() in the constructor and forgot about the other one. idk.
            if (Phase == 1) Phase2();

            Name = Name.ToUpper();
            Phase = 3;
            ActiveEffects.Remove(Phase2EnergyBuff);
            Abilities.AddRange(new List<Ability>()
            {
                new(Ability31, new(mana: 50), null),
                new(Ability32, new(mana: 25), null)
            });
            BaseAtk = 100;
            BaseDef = 20;
            BaseSpd = 120;
            MaxEnergy = 0;
            AddEffect(Determination);
            CurrentHp = Math.Max(BaseHp / 2, Hp);
            return $"\nNo... not yet. You won't get past {Name} that easily. And now, during their final breaths... they will give it their all!";
        }
    }
}
