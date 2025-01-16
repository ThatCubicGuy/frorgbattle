using FrogBattleV2.Classes.GameLogic;

namespace FrogBattleV2.Classes.Characters
{
    internal class Kongle : Fighter, ICounters//, ISummons
    {
        public Ability Counter { get; }
        public Ability Summon { get; }
        private string CounterAction(Fighter? target)
        {
            string output = string.Empty;
            return output;
        }
        private string SummonAction(Fighter? target)
        {
            string output = string.Empty;
            return output;
        }
        public Kongle(string name) : base(name, 1.0, 80, 30, 100, 120)
        {
            Counter = new(CounterAction, new());
            Summon = new(SummonAction, new());
        }
        private string Ability1(Fighter target)
        {
            string output = string.Empty;
            return output;
        }
        private string Ability2(Fighter target)
        {
            string output = string.Empty;
            return output;
        }
        private string Ability3(Fighter target)
        {
            string output = string.Empty;
            return output;
        }
        private string Ability4(Fighter target)
        {
            string output = string.Empty;
            return output;
        }
        private string Ability5(Fighter target)
        {
            string output = string.Empty;
            return output;
        }
        private string Burst(Fighter target)
        {
            string output = $"{Name} tells {target.Name} to turn around. {target.Name} looks behind...\nA giant mountain of pinecones blots out the sun!";
            return output;
        }
    }
}
