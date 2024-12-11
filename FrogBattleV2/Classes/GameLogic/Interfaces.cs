using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattleV2.Classes.GameLogic
{
    internal interface ICounters
    {
        string Counter(Fighter? target);
    }
    internal interface ISummons
    {
        Ability Summon { get; }
    }
    internal interface IFollowsUp
    {
        Ability FollowUp { get; }
    }
    internal interface IAbilityBonus
    {
        Ability Bonus { get; }
    }
}
