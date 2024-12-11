using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using FrogBattleV2.Classes.GameLogic;
using DotID = FrogBattleV2.Classes.GameLogic.StatusEffect.DamageOverTime.DotID;
using EffID = FrogBattleV2.Classes.GameLogic.StatusEffect.Effect.EffectID;
using PropID = FrogBattleV2.Classes.GameLogic.StatusEffect.PropertyID;

namespace FrogBattleV2.Classes.Characters
{
    internal class Raiden : Fighter
    {
        private bool RipperMode = false;
        protected override string SpecialStat => SelectedWeapon.ActiveCombo;
        private string dodgemsg = string.Empty;
        public override string DodgeMsg { get { return dodgemsg; } }
        public override string BarrierBlock(Fighter? target) { GetEnergy(10); return $"{Name} blocks the attack and nullifies the damage!"; }

        #region Unique / reworked StatusEffects

        private static readonly StatusEffect ripperBuff = new("Ripper Mode", PropID.Unremovable, 7 + 1 /*One turn is wasted after applying*/, 1, new StatusEffect.Effect[]
                {
                    new(EffID.ATK, 0.8, true),
                    new(EffID.DEF, -1, true),
                    new(EffID.SPD, 0.5, true),
                    new(EffID.Energy, 2, true),
                });
        private static readonly StatusEffect bmiErrorDebuff = new("B.M.I. Error", PropID.Debuff | PropID.Unremovable, 3, 1,
                new StatusEffect.Effect(EffID.SPD, -0.5, true));
        private static readonly StatusEffect barrier = new("Block", PropID.Unremovable | PropID.Invisible, 2, 1,
                    new StatusEffect.Effect(EffID.Barrier, 99, false));
        private static readonly StatusEffect speedBuff = new("Rapid Slicer", PropID.None, 4, 1,
                    new StatusEffect.Effect(EffID.SPD, 0.25, true));
        private static readonly StatusEffect _shock = new("Shock", PropID.Debuff, 2, 10,
                    new StatusEffect.DamageOverTime(DotID.Shock, 0.2));
        private static readonly StatusEffect _vulnerability = new("Dmg Taken Increase", PropID.Debuff, 5, 1,
                    new StatusEffect.Effect(EffID.DmgTaken, 0.25, true));
        private static readonly StatusEffect _weaken = new("Weakened", PropID.Debuff, 5, 1,
                    new StatusEffect.Effect(EffID.DmgBonus, -0.25, true));
        private static readonly StatusEffect _dmgIncrease = new("Dmg Increase", PropID.None, 5, 1,
                    new StatusEffect.Effect(EffID.DmgBonus, 0.25, true));
        private static readonly StatusEffect _slow = new("Slow", PropID.Debuff, 6, 1,
                    new StatusEffect.Effect(EffID.SPD, -0.15, true));
        private static readonly StatusEffect _defShred = new("Defense Reduction", PropID.Debuff, 5, 3,
                    new StatusEffect.Effect(EffID.DEF, -0.3, true));
        private static StatusEffect BMIErrorDebuff => bmiErrorDebuff.Clone();
        private static StatusEffect Barrier => barrier.Clone();
        private static StatusEffect SpeedBuff => speedBuff.Clone();
        private static StatusEffect RipperBuff => ripperBuff.Clone();
        protected override StatusEffect Shock => _shock.Clone();
        protected override StatusEffect Vulnerability => _vulnerability.Clone();
        protected override StatusEffect Weaken => _weaken.Clone();
        protected override StatusEffect DmgIncrease => _dmgIncrease.Clone();
        protected override StatusEffect Slow => _slow.Clone();
        protected override StatusEffect DefShred => _defShred.Clone();

        #endregion
        private readonly AbilityTree SelectedWeapon;
        private class AbilityNode
        {
            public Func<Fighter, string> StoredAbility { get; }
            public AbilityNode? Left { get; set; }
            public AbilityNode? Right { get; set; }
            public AbilityNode(Func<Fighter, string> ability)
            {
                StoredAbility = ability;
            }
        }
        private class AbilityTree
        {
            public AbilityNode Root { get; }
            private AbilityNode CurrentNode;
            public string ActiveCombo { get; private set; } = string.Empty;
            public AbilityTree(AbilityNode root)
            {
                Root = CurrentNode = root;
            }
            public Func<Fighter, string> TraverseLeft()
            {
                if (CurrentNode.Left == null)
                {
                    ResetCombo();
                }
                ActiveCombo += 'L';
                CurrentNode = CurrentNode.Left!;
                return CurrentNode.StoredAbility;
            }
            public Func<Fighter, string> TraverseRight()
            {
                if (CurrentNode.Right == null)
                {
                    ResetCombo();
                }
                ActiveCombo += 'R';
                CurrentNode = CurrentNode.Right!;
                return CurrentNode.StoredAbility;
            }
            public void ResetCombo()
            {
                CurrentNode = Root;
                ActiveCombo = string.Empty;
            }
        }
        public Raiden(string name) : base(name, 0.80, 100, 30, 110, 150)
        {
            SelectedWeapon = HFBlade();
            Abilities = new()
            {
                new(Ability1, new DynamicCost(this, user => ((Raiden)user).RipperMode ? new(energy: 75, health: 50, costType: Cost.CostType.SoftCost) : new(mana: 10)), null),
                new(Ability2, new DynamicCost(this, user => ((Raiden)user).RipperMode ? new(energy: 75, health: 75, costType: Cost.CostType.SoftCost) : new(mana: 15)), null),
                new(Ability3, new(mana: 20), null, user => RipperMode ? AbilityBlocked : AbilityUsable),
                new(Ability4, new(), null, successValue: RepeatsTurn),
                new(Burst, new(energy: MaxEnergy), null, (user) => RipperMode ? AbilityBlocked : AbilityUsable),
            };
            AddEffect(BulletRES);
        }
        private AbilityTree HFBlade()
        {
            AbilityNode root = new(LightAttack); // This should STILL be unreachable
            root.Left = new(LightAttack);
            root.Right = new(StrongAttack);
            root.Left.Left = new(LightAttack);
            root.Left.Right = new(AnkleSlicer);
            root.Right.Left = new(TornadoSlash);
            root.Right.Right = new(RightBackKick);
            root.Left.Left.Left = new(LightAttack);
            root.Left.Right.Left = new(SkyHigh);
            root.Left.Right.Right = new(HeelDrop);
            root.Right.Left.Right = new(StabKick);
            root.Right.Right.Left = new(CrossSlice);
            root.Right.Right.Right = new(TripleKick);
            root.Left.Left.Left.Left = new(LightAttack);
            root.Left.Right.Left.Left = new(MidAirSlice);
            root.Left.Right.Left.Right = new(CrescentSlice);
            root.Right.Right.Left.Right = new(LowRoundhouse);
            root.Right.Right.Right.Left = new(TripleKickUpswing);
            root.Right.Right.Right.Right = new(FlurryKick);
            root.Left.Left.Left.Left.Left = new(LightAttackFin);
            root.Right.Right.Right.Left.Right = new(ThroatSlicer);
            return new(root);
        }
        #region Abilities and overrides
        protected override string SkipTurnAction(Fighter target)
        {
            SelectedWeapon.ResetCombo();
            return base.SkipTurnAction(target) + " Combo reset." + RipperCheck();
        }
        protected override void TurnStartResets()
        {
            CanParry = true;
            base.TurnStartResets();
        }
        private string Ability1(Fighter target)
        {
            return SelectedWeapon.TraverseLeft()(target) + RipperCheck(target);
        }
        private string Ability2(Fighter target)
        {
            return SelectedWeapon.TraverseRight()(target) + RipperCheck(target);
        }
        private string Ability3(Fighter target)
        {
            AddEffect(Barrier);
            SelectedWeapon.ResetCombo();
            return $"{Name} strikes a pose!" + RipperCheck();
        }
        private string Ability4(Fighter target)
        {
            RegenMana((int)Math.Round(Energy / 2.5));
            CurrentEnergy = 0;
            return $"{Name} absorbs the electrolytes stored in his weapon and regenerates his Mana to {Mana}!" + RipperCheck();
        }
        public string Burst(Fighter target)
        {
            CurrentEnergy = MaxEnergy; // CheckEnergy() is designed to also reduce energy to 0, since that's how 99% of bursts should work.
            RipperMode = true;         // Of course, that leaves a few stragglers which I have to fix manually. Not that big of a deal.
            ActiveEffects.Insert(0, RipperBuff); // Putting it in slot #1 makes it more visible and important-looking.
            return $"{Name} enters RIPPER MODE! DEF reduced, but ATK, SPD and Energy Recharge massively increased!\n" +
                $"{Name}'s attacks will also consume HP instead of mana!";
        }
        #endregion
        // Ripper mode stuff
        private string RipperCheck(Fighter? target = null)
        {
            string output = string.Empty;
            if (RipperMode)
            {
                if (target != null)
                {
                    target.AddEffect(Shock);
                    output += $"\n{target.Name} is also electrified for a total of {target.CalculateDoT(this, DotID.Shock):0.#} damage!";
                }
                if (Energy == 0 || ActiveEffects[0].Name != RipperBuff.Name || ActiveEffects[0].Turns <= 1)
                {
                    CurrentEnergy = 0;
                    RipperMode = false;
                    RemoveEffect(RipperBuff);
                    output += $"\n{Name}'s Ripper Mode has worn off!";
                }
            }
            return output;
        }

        private bool CanParry = true;
        public override bool Dodge(Fighter target)
        {
            if (base.Dodge(target))
            {
                GetEnergy(5);
                if (CanParry)
                {
                    double ParryDmg = LightDmg(Spd, DmgType.Slash, target);
                    CanParry = false;
                    dodgemsg = $"{Name} parries and counterattacks for {ParryDmg:0.#} damage!{target.TakeDamage(ParryDmg, this)}";
                }
                else
                {
                    dodgemsg = $"{Name} parries the hit!";
                }
                return true;
            }
            else return false;
        }

        #region Combo Attacks
        // Every single Combo hit ability. I was initially upset about not being able to use
        // lambdas, but this is definitely better. Those would have been a nightmare to debug.
        private string LightAttack(Fighter target) //L
        {
            string output = $"{Name} swings at {target.Name}!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(10);
            double dmg = LightDmg(Atk, DmgType.Slash, target);
            output += $"{target.Name} gets sliced and takes {dmg:0.#} damage!";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        private string StrongAttack(Fighter target) //R
        {
            string output = $"{Name} swings at {target.Name}, with power!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(11);
            double dmg = MediumDmg(Atk, DmgType.Slash, target);
            output += $"{target.Name} is hurt for {dmg:0.#} damage!";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        private string AnkleSlicer(Fighter target) //LR
        {
            string output = $"{Name} ducks for a sneaky ankle slice!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(12);
            double dmg = MediumDmg(Atk, DmgType.Slash, target);
            output += $"{target.Name} is tripped and takes {dmg:0.#} damage!";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        private string TornadoSlash(Fighter target) //RL
        {
            string output = $"{Name} does a fancy spinning attack directed at {target.Name}!";
            int hitCount = 2;
            for (int i = 0; i < hitCount; i++)
            {
                if (target.Dodge(this))
                {
                    SelectedWeapon.ResetCombo();
                    output += '\n' + target.DodgeMsg;
                }
                GetEnergy(7);
                double dmg = LightDmg(Atk, DmgType.Slash, target);
                output += $"\n{target.Name} is in the way - {dmg:0.#} damage!";
                output += target.TakeDamage(dmg, this);
            }
            return output;
        }
        private string RightBackKick(Fighter target) //RR
        {
            string output = $"{Name} swings his sword back around!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(12);
            double dmg = MediumDmg(Atk, DmgType.Slash, target);
            output += $"{target.Name} is wounded for {dmg:0.#} damage!";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        private string SkyHigh(Fighter target) //LRL
        {
            string output = $"{Name} swings up fiercely!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(15);
            double dmg = MediumDmg(Atk, DmgType.Slash, target);
            output += $"{target.Name} is launched into the sky for {dmg:0.#} damage!";
            output += target.TakeDamage(dmg, this) + target.Stun(1, this);
            return output;
        }
        private string HeelDrop(Fighter target) //LRR
        {
            string output = $"{Name} drops his heel on {target.Name}'s head!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(14);
            double dmg = MediumDmg(Atk, DmgType.Blunt, target);
            output += $"{target.Name} is crushed for {dmg:0.#} damage and slightly confused, reducing their DEF by " +
                $"{DefShred.GetEffectsOfType(EffID.DEF, -100)}% for {DefShred.Turns} rounds!";
            output += target.TakeDamage(dmg, this);
            target.AddEffect(DefShred);
            return output;
        }
        private string StabKick(Fighter target) //RLR
        {
            string output = $"{Name} aggressively stabs {target.Name}!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(18);
            double pierceValue = 0.6;
            double dmg = MediumDmg(Atk, DmgType.Stab, target, pierceValue);
            output += $"{target.Name}'s chest was painfully pierced, ignoring {pierceValue * 100}% DEF and doing {dmg:0.#} damage!";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        private string CrossSlice(Fighter target) //RRL
        {
            string output = $"{Name} slices {target.Name} in an X shape!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(18);
            double dmg = LightDmg(Atk, DmgType.Slash, target) + LightDmg(Atk, DmgType.Slash, target);
            output += $"{target.Name} is visibly scarred for {dmg:0.#} damage!";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        private string TripleKick(Fighter target) //RRR
        {
            string output = $"{Name} swings around, AGAIN!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(16);
            double dmg = HeavyDmg(Atk, DmgType.Slash, target);
            output += $"{target.Name}'s bones were fractured for {dmg:0.#} damage!";
            output += target.TakeDamage(dmg, this);
            return output;
        }
        private string MidAirSlice(Fighter target) //LRLL
        {
            GetEnergy(18);
            double dmg = LightDmg(Spd, DmgType.Slash, target) + LightDmg(Spd, DmgType.Slash, target) + LightDmg(Spd, DmgType.Slash, target);
            string output = $"{Name} slices {target.Name} a bunch of times, for a total of {dmg:0.#} damage!\n";
            output += $"{Name} also gains a {SpeedBuff.GetEffectsOfType(EffID.SPD, 100)}% SPD Buff for {SpeedBuff.Turns - 1} rounds!";
            output += target.TakeDamage(dmg, this);
            AddEffect(SpeedBuff);
            return output;
        }
        private string CrescentSlice(Fighter target) //LRLR
        {
            GetEnergy(16);
            double dmg = HeavyDmg(Spd, DmgType.Blunt, target);
            string output = $"{Name} smashes {target.Name} back into the ground for {dmg:0.#} damage!\n";
            output += $"{Name} also gains a {DmgIncrease.GetEffectsOfType(EffID.DmgBonus, 100)}% dmg increase for {DmgIncrease.Turns - 1} rounds!";
            output += target.TakeDamage(dmg, this);
            AddEffect(DmgIncrease);
            return output;
        }
        private string LowRoundhouse(Fighter target) //RRLR
        {
            string output = $"{Name} sweeps the floor with his blade!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(22);
            double dmg = HeavyDmg(Atk, DmgType.Slash, target);
            output += $"{target.Name} is crippled for {dmg:0.#} damage and has their speed reduced by {Slow.GetEffectsOfType(EffID.SPD, -100)}%!";
            output += target.TakeDamage(dmg, this);
            target.AddEffect(Slow);
            return output;
        }
        private string TripleKickUpswing(Fighter target) //RRRL
        {
            string output = $"{Name} does a powerful upswing!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(20);
            double dmg = HeavyDmg(Atk, DmgType.Slash, target);
            output += $"{target.Name} is struck for {dmg:0.#} damage and loses balance, having their damage taken " +
                $"increased by {Vulnerability.GetEffectsOfType(EffID.DmgTaken, 100)}% for {Vulnerability.Turns} rounds!";
            output += target.TakeDamage(dmg, this);
            target.AddEffect(Vulnerability);
            return output;
        }
        private string FlurryKick(Fighter target) //RRRR
        {
            string output = $"{Name} unleashes a flurry of kicks at {target.Name}!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(25);
            double dmg = HeavyDmg(Atk, DmgType.Stab, target) + HeavyDmg(Atk, DmgType.Stab, target) + HeavyDmg(Atk, DmgType.Stab, target) + HeavyDmg(Atk, DmgType.Stab, target);
            return output + $"{target.Name} gets repeatedly stabbed for a grand total of {dmg:0.#} damage!{target.TakeDamage(dmg, this)}";
        }
        private string LightAttackFin(Fighter target) //LLLLL
        {
            string output = $"{Name} swings very rapidly at {target.Name}!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg;
            }
            GetEnergy(30);
            double dmg = MediumDmg((int)(Spd * 1.25), DmgType.Slash, target);
            output += $"{target.Name} gets sliced and takes {dmg:0.#} damage!\n";
            output += $"{Name} has hit {target.Name} so many times in a row that their BMI is glitching, " +
                $"reducing their SPD by {BMIErrorDebuff.GetEffectsOfType(EffID.SPD, -100)}% for {BMIErrorDebuff.Turns} turns!";
            output += target.TakeDamage(dmg, this);
            target.AddEffect(BMIErrorDebuff);
            return output;
        }
        private string ThroatSlicer(Fighter target) //RRRLR
        {
            string output = $"{Name} swings with precision at {target.Name}'s throat!\n";
            if (target.Dodge(this))
            {
                SelectedWeapon.ResetCombo();
                return output + target.DodgeMsg + "\nHow is that even possible!?";
            }
            GetEnergy(30);
            double dmg = HeavyDmg(2 * Atk, DmgType.Slash, target);
            uint bleedCount = 4;
            output += $"{Name} vaporises {target.Name}'s neck and demolishes their spine, dealing a whopping {dmg:0.#} damage and inflicting " +
                $"{bleedCount} stacks of Bleed for {Bleed.Turns} rounds!";
            output += target.TakeDamage(dmg, this);
            target.AddEffect(Bleed, bleedCount);
            return output;
        }
        #endregion
    }
}
