using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattleV2.Classes.GameLogic
{
    internal interface ICounters
    {
        Ability Counter { get; }
    }
    internal interface ISummons
    {
        Summon Summoned { get; }
    }
    internal interface IFollowsUp
    {
        Ability FollowUp { get; }
    }
    internal interface IAbilityBonus
    {
        Ability Bonus { get; }
    }
    internal interface ITakesAction
    {
        double Hp { get; }
        double Atk { get; }
        double Def { get; }
        double Spd { get; }
        double Mana { get; }
        Ability.AbilityCheckResult PlayTurn(Fighter target);

        // I REALLY WANT TO IMPLEMENT THIS BUT IDK WHAT TO MAKE IT CAUSE LIKE TECHNICALLY YOU CAN TAKE ACTION AND DO LIKE NOTHING
        // SO I HAVE NO IDEA WHAT I ACTUALLY SHOULD PUT HERE BECAUSE NOTHING IS REALLY UNIVERSAL THEN CAUSE I DON'T HAVE LIKE AN
        // ACTION TAB MEANING THAT EVERYONE HAS TO HAVE SPD NO TECHNICALLY THEY DO NOT EVEN ATK DOESN'T MAKE SENSE CAUSE WHAT IF
        // YOU DON'T ATTACK AND YOU JUST BUFF YOUR SUMMONER LIKE WHAT DO I DO THEN DO I JUST ABANDON THIS OR DO I BASE SUMMON ON
        // THE FIGHTER CLASS EVEN THOUGH THERE'S LOTS OF THINGS IT DOESN'T USE LIKE THE ABILITY LIST OR WHATEVER MAN IDK
    }
}
