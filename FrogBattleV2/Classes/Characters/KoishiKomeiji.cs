using FrogBattleV2.Classes.GameLogic;

namespace FrogBattleV2.Classes.Characters
{
    internal class KoishiKomeiji : Fighter, IFollowsUp, ICounters
    {
        public Ability FollowUp { get; }
        public Ability Counter { get; }
        private int[] Thoughts;
        private List<Ability> Thinkables;
        public KoishiKomeiji(string name) : base(name, 1.0, 80, 30, 100, 120)
        {
            Thoughts = new int[10];
            Thinkables = new List<Ability>()
            {
                new(Ability1, new()),
                new(Ability2, new()),
                new(Ability3, new()),
            };
            FollowUp = new(FollowUpThoughts, new());
            Counter = new(CounterThoughts, new());
        }
        #region Abilities
        private string Ability1(Fighter target)
        {
            double healing = Heal(0.1 * (BaseHp - CurrentHp));
            return $"{Name} forgets how much damage she took and restores {healing:0.#} HP!";
        }
        private string Ability2(Fighter target)
        {
            return $"";
        }
        private string Ability3(Fighter target)
        {
            return $"";
        }
        private string Ability4(Fighter target)
        {
            return $"";
        }
        private string Ability5(Fighter target)
        {
            return $"";
        }
        private string Ability6(Fighter target)
        {
            return $"";
        }
        private string Ability7(Fighter target)
        {
            return $"";
        }
        private string Ability8(Fighter target)
        {
            return $"";
        }
        private string Ability9(Fighter target)
        {
            return $"";
        }
        private string Ability10(Fighter target)
        {
            return $"";
        }
        #endregion
        // Now to handle the THOUGHT ACTIVATION
        private string CounterThoughts(Fighter target)
        {
            string output = string.Empty;
            for (int nr = 0; nr < 5; ++nr)
            {
                if (Thoughts[nr] > 0)
                {
                    var result = Thinkables[nr].ExecuteAbility(this, target);
                    if (result.IsUsable)
                    {
                        output += result.Message + '\n';
                        --Thoughts[nr];
                    }
                }
            }
            return output;
        }
        private string FollowUpThoughts(Fighter target)
        {
            string output = string.Empty;
            for (int nr = 5; nr < 10; ++nr)
            {
                if (Thoughts[nr] > 0)
                {
                    output += Thinkables[nr];
                    --Thoughts[nr];
                }
            }
            return output;
        }
    }
}
