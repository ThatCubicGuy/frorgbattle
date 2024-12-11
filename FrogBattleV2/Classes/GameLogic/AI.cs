using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FrogBattleV2.Classes.Characters;

namespace FrogBattleV2.Classes.GameLogic
{
    internal class AI
    {
        private readonly Random random = new Random();
        private readonly Fighter Enemy;
        private readonly Fighter ControlledUnit;
        public AI(Fighter enemy, Fighter unit)
        {
            Enemy = enemy;
            ControlledUnit = unit;
        }
        private Ability ChooseMove()
        {
            foreach (var ability in ControlledUnit.Abilities)
            {
                var (canUse, _) = ControlledUnit.CheckUsability(ability);
            }
            throw new NotImplementedException();
        }
        public double CalculateBenefitScore()
        {
            throw new NotImplementedException();
        }
        private double CalculateAggressionLevel(Ability ability)
        {
            throw new NotImplementedException();
        }
        private double CalculateRiskLevel(Ability ability)
        {
            return (ability.AbilityCost.ManaCost / ControlledUnit.Mana) + (ability.AbilityCost.HealthCost / ControlledUnit.Hp);
        }
        private double CalculateImpactLevel(Ability ability)
        {
            throw new NotImplementedException();
        }
        public void TakeTurn()
        {
            throw new NotImplementedException();
        }
    }
}
