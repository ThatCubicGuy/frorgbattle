using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrogBattleV2.Classes.GameLogic
{
    internal class Summon : Fighter //AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA
    {
        public Ability Attack { get; }
        private Fighter Summoner { get; }
        public Summon(Ability action, string name, Fighter summoner, double baseHp, int baseAtk, int baseDef, int baseSpd) : base(name, baseHp, baseAtk, baseDef, baseSpd, 0)
        {
            Summoner = summoner;
            Attack = action;
            Abilities.Add(Attack);
        }
        public Summon(Ability action, string name, Fighter summoner) : base(name, 99999, (int)summoner.Atk, (int)summoner.Def, (int)summoner.Spd, 0)
        {
            Summoner = summoner;
            Attack = action;
            Abilities.Add(Attack);
        }
    }
}
