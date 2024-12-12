using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattleV2.Classes.GameLogic
{
    internal class Ability
    {
        private readonly Func<Fighter, string> _Action;
        public Cost AbilityCost { get; }
        public AbilityCheckResult SuccessValue { get; }
        public enum AbilityStatus
        {
            Success,
            RepeatsTurn,
            Incomplete,
            FighterStunned,
            NoResources,
            TemporarilyBlocked,
            PermanentlyBlocked
        }
        internal record AbilityCheckResult(AbilityStatus Status, string Message)
        {
            public static AbilityCheckResult Empty = new(AbilityStatus.Success, string.Empty);
            public bool CanContinue => Status switch
            {
                AbilityStatus.Success => true,
                AbilityStatus.Incomplete => true,
                AbilityStatus.FighterStunned => true,
                _ => false
            };
            public bool IsUsable => Status switch
            {
                AbilityStatus.Success => true,
                AbilityStatus.Incomplete => true,
                AbilityStatus.RepeatsTurn => true,
                _ => false
            };
            public static AbilityCheckResult operator +(AbilityCheckResult left, AbilityCheckResult right)
            {
                return new(right.IsUsable ? left.Status : AbilityStatus.Incomplete, left.Message + right.Message);
            }
        }
        public AbilityCheckResult ExecuteAbility(Fighter user, Fighter target)
        {
            var result = user.CheckUsability(this);
            if (!result.IsUsable) return result;
            user.DeductCost(this);
            return new(result.Status, _Action(target));
        }
        public Func<Fighter, AbilityCheckResult>? CustomUnusabilityConditions;
        public Func<Fighter, Fighter, AbilityEffect>? CalculateEffects; // Nullable due to incomplete implementation.
        public Ability(Func<Fighter, string> action, Cost cost, Func<Fighter, Fighter, AbilityEffect>? effects, Func<Fighter, AbilityCheckResult>? customUnusabilityConditions = null, AbilityCheckResult? successValue = null)
        {
            _Action = action;
            AbilityCost = cost;
            CalculateEffects = effects;
            CustomUnusabilityConditions = customUnusabilityConditions;
            SuccessValue = successValue ?? Fighter.AbilityUsable;
        }
    }
    /// <summary>
    /// <para>Easily expandable class for handling a part of <see cref="Ability"/> requirements.</para>
    /// <para>Pass the arguments in any order with explicit names (mana, energy, health)</para>
    /// </summary>
    internal class Cost
    {
        public virtual double ManaCost { get; }
        public virtual double EnergyCost { get; }
        public virtual double HealthCost { get; }
        public enum CostType
        {
            HardCost,
            SoftCost,
            ReverseCost
        }
        private readonly CostType _costType;
        public virtual CostType GetCostType() => _costType;
        /// <summary>
        /// <para>An ability with a soft cost can still be used if you don't have enough resources (eg, you have 5 mana and the ability costs 20)</para>
        /// <para>In such cases, the corresponding value will still be deducted, potentially into the negatives.</para>
        /// </summary>
        public bool IsHardCost => GetCostType() == CostType.HardCost;
        public bool IsSoftCost => GetCostType() == CostType.SoftCost;
        public bool IsReverseCost => GetCostType() == CostType.ReverseCost;
        public Cost(double mana = 0, double energy = 0, double health = 0, CostType costType = CostType.HardCost)
        {
            if (costType == CostType.ReverseCost)
            {
                if (mana == 0) mana = int.MaxValue;
                if (energy == 0) energy = int.MaxValue;
                if (health == 0) health = int.MaxValue;
            }
            ManaCost = mana;
            EnergyCost = energy;
            HealthCost = health;
            _costType = costType;
        }
    }
    /// <summary>
    /// <para>Cost that can change depending on the status of <see cref="user"/>.</para>
    /// <para>Pass any function that takes a <see cref="Fighter"/> as argument and returns a <see cref="Cost"/> into the constructor.</para>
    /// </summary>
    internal class DynamicCost : Cost
    {
        private readonly Fighter user;
        public override CostType GetCostType() => CalculateCost(user).GetCostType();
        public override double ManaCost => CalculateCost(user).ManaCost;
        public override double EnergyCost => CalculateCost(user).EnergyCost;
        public override double HealthCost => CalculateCost(user).HealthCost;
        public Func<Fighter, Cost> CalculateCost;
        public DynamicCost(Fighter user, Func<Fighter, Cost> calculateCost) : base()
        {
            this.user = user;
            CalculateCost = calculateCost;
        }
    }
    internal record AbilityEffect(double PotentialDamage, StatusEffect AppliedEffect, double RNGFactor);
}
