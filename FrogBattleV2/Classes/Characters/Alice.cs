using FrogBattleV2.Classes.GameLogic;
using EffID = FrogBattleV2.Classes.GameLogic.StatusEffect.Effect.EffectID;
using PropID = FrogBattleV2.Classes.GameLogic.StatusEffect.PropertyID;
using static FrogBattleV2.Classes.GameLogic.StatusEffect;

namespace FrogBattleV2.Classes.Characters
{
    internal class Alice : Fighter, IFollowsUp
    {
        private uint _raining;
        private uint _demonDice;
        private double _totalDmg;
        private bool _phase2;
        private readonly int _dressType;
        private readonly string[] _dressNames = ["Classic", "Checkmate", "Fleshmaiden", "Cheshire"];
        private Demons _activeDemon;
        private enum Demons
        {
            MadHatter,
            CheshireCat
        }

        #region Unique StatusEffects

        private StatusEffect Hysteria
        {
            get
            {
                if (_dressType == 2) return new StatusEffect("Hysteria", PropID.Unremovable, 4, 1, new Effect(EffID.ATK, 0.5 - CurrentEnergy / 100.0, true), new Effect(EffID.DEF, 0.15, true), new Effect(EffID.SPD, 0.15, true), new Effect(EffID.ManaRecovery, 0.15, true), new Effect(EffID.AllTypeRES, 0.75, true));
                return new StatusEffect("Hysteria", PropID.Unremovable, 4, 1, new Effect(EffID.ATK, 0.35 - CurrentEnergy / 100.0, true), new Effect(EffID.AllTypeRES, 0.75, true));
            }
        }
        private static StatusEffect KnifeRain => new("Knife Rain", PropID.None, 2, 3,
            new Effect(EffID.ATK, 10, false), new Effect(EffID.DEF, 5, false), new Effect(EffID.SPD, 1, false));
        private static StatusEffect Checkmate => new("Checkmate",
            PropID.Unremovable | PropID.Invisible, -1, 1, new Effect(EffID.SlashBonus, 1, true),
            new Effect(EffID.BulletBonus, 1, true));
        private static StatusEffect SpdBuff => new("Quick Reflexes", PropID.None, 3, 3, new Effect(EffID.SPD, 0.1, true));

        #endregion

        public Ability FollowUp { get; }
        private string ActiveDemonName { get { return _activeDemon.ToString().Replace('_', ' '); } }
        protected override string SpecialStat { get { return _dressNames[_dressType] + " Dress" + (_phase2 ? " --- " + ActiveDemonName : string.Empty); } }
        public Alice(string name) : base(name, 0.375, 70, 40, 100, 100)
        {
            CurrentEnergy = MaxEnergy;
            Abilities = new List<Ability>
            {
                new(Ability1, new Cost(mana: 12)),
                new(Ability2, new Cost(mana: 18)),
                new(Ability3, new Cost(mana: 30), user => _raining > 0 ? AbilityBlocked : AbilityUsable),
                new(Ability4, new Cost(mana: 16)),
                new(Ability5, new Cost(mana: 24)),
                new(Burst, new Cost(energy: MaxEnergy, costType: Cost.CostType.ReverseCost))
            };
            FollowUp = new Ability(FollowUpAction, new Cost());
            AddEffect(AllTypeRES);
            _dressType = (int)Math.Floor(RNG * _dressNames.Length);
            switch (_dressType)
            {
                case 0:
                    break;
                case 1:
                    AddEffect(Checkmate);
                    break;
                case 2:
                    break;
                case 3:
                    break;
            }
        }
        public override void GetEnergy(double energy)
        {
            base.GetEnergy(energy / -2);
        }
        
        public override string TakeDamage(double damage, Fighter? source)
        {
            if (RNG < 0.1) AddEffect(SpdBuff);
            return base.TakeDamage(damage, source) + (!_phase2 && CurrentHp <= 0 ? GoPhase2() : string.Empty);
        }
        private string GoPhase2()
        {
            _phase2 = true;
            CurrentHp = BaseHp = 2000;
            _activeDemon = (Demons)(int)Math.Floor(RNG * Enum.GetNames(typeof(Demons)).Length);
            return $"\n{Name} won't go down so easily! The {ActiveDemonName} is summoned and lends her a hand, reviving her for {BaseHp} HP!";
        }
        protected override string SkipTurnAction(Fighter target)
        {
            _totalDmg = 0;
            return base.SkipTurnAction(target);
        }
        private string Ability1(Fighter target)
        {
            _totalDmg = 0;
            string output = $"{Name} pulls out her Vorpal Blade!";
            for (int i = 0; i < 4; i++)
            {
                if (target.Dodge(this)) output += '\n' + target.DodgeMsg;
                else
                {
                    GetEnergy(3);
                    double dmg = LightDmg(Atk, DmgType.Slash, target);
                    _totalDmg += dmg;
                    output += $"\n{Name} slashes {target.Name} for {dmg:0.#} damage!" +
                        target.TakeDamage(dmg, this);
                }
            }
            if (_totalDmg == 0) return output + "\nQuite the opponent...";
            return output + $"\n{Name} dealt a total of {_totalDmg:0.#} damage to {target.Name}!";
        }
        private string Ability2(Fighter target)
        {
            _totalDmg = 0;
            string output = $"{Name} readies her Pepper Grinder!";
            if (target.Dodge(this)) return output + '\n' + target.DodgeMsg;
            double dmg = _totalDmg = MediumDmg(Atk, DmgType.Bullet, target);
            return output + $"\n{Name} fires a powerful bullet at {target.Name}, dealing {dmg:0.#} damage!" +
                target.TakeDamage(dmg, this) + target.Stun(2, this);
        }
        private string Ability3(Fighter target)
        {
            _raining = 3;
            return $"{Name} throws a bunch of blades into the air!" +
                $"\nKnives start falling from the sky for {_raining} rounds!";
        }
        private string Ability4(Fighter target)
        {
            _totalDmg = 0;
            string output = $"{Name} wants to make some tea!\n";
            if (target.Dodge(this)) return output + target.DodgeMsg;
            GetEnergy(4);
            double dmg = _totalDmg = HeavyDmg(Atk, DmgType.Magic, target);
            target.AddEffect(Burn);
            return output + $"{Name} \"accidentally\" touches {target.Name} with the steaming teapot, " +
                   $"dealing {dmg:0.#} damage and burning them for {Burn.Turns} rounds!{target.TakeDamage(dmg, this)}";
        }
        private string Ability5(Fighter target)
        {
            _demonDice = (uint)Math.Floor(RNG * 6 + 1);
            return $"{Name} rolls a dice and summons a demon to fight by her side for {_demonDice} turns!";
        }
        private string Burst(Fighter target)
        {
            AddEffect(Hysteria);
            string output = $"{Name} is going insane! She gains a {Hysteria.GetEffectsOfType(EffID.ATK, 100)}% ATK boost and a " +
                $"{Hysteria.GetEffectsOfType(EffID.AllTypeRES, 100)}% DMG resistance boost for {Hysteria.Turns - 1} turns!";
            CurrentEnergy = MaxEnergy;
            _totalDmg = 200;
            return output;
        }
        private string RoseFollowUp()
        {
            if (RNG < 0.1 && _dressType != 3)
            {
                return $"\n{Name} collects a rose and heals for {Heal(_totalDmg / 2):0.#} HP!";
            }

            return string.Empty;
        }
        private string DemonFollowUp(Fighter target)
        {
            if (_demonDice > 0)
            {
                string output = $"\nThe demon attacks alongside {Name}!\n";
                _demonDice--;
                if (_demonDice != 0) _totalDmg /= 2;
                if (target.Dodge(this)) return output + target.DodgeMsg;
                return output + $"He deals {_totalDmg:0.#} damage to {target.Name}!{target.TakeDamage(_totalDmg, null)}";
            }

            return string.Empty;
        }
        private string RainFollowUp(Fighter target)
        {
            if (_raining > 0)
            {
                _totalDmg = 0;
                string output = $"\nThe knives befall {target.Name}!";
                for (int i = 0; i < 5; i++)
                {
                    if (target.Dodge(this)) output += '\n' + target.DodgeMsg;
                    else
                    {
                        GetEnergy(1);
                        double dmg = LightDmg(Atk, DmgType.Stab, target);
                        _totalDmg += dmg;
                        output += target.TakeDamage(dmg, this);
                        //output += $"\nThe {i + 1}{i switch { 0 => "st", 1 => "nd", 2 => "rd", _ => "th" }} " +
                        //$"knife hits {target.Name} for {dmg:0.#} damage!{target.TakeDamage(dmg, this)}";
                    }
                }
                _raining--;
                AddEffect(KnifeRain);
                return output + $"\nThey deal {_totalDmg:0.#} total damage to {target.Name}!" +
                    $"\n{Name} also gets {Heal(20):0.#} HP and {RegenMana(10):0.#} mana!";
            }

            return string.Empty;
        }
        public string FollowUpAction(Fighter target)
        {
            return RoseFollowUp() + DemonFollowUp(target) + RainFollowUp(target) + (_phase2 ? Phase2FollowUp(target) : string.Empty);
        }
        private string Phase2FollowUp(Fighter target)
        {
            _totalDmg = _activeDemon switch
            {
                Demons.MadHatter => HeavyDmg(Atk, DmgType.None, target),
                Demons.CheshireCat => MediumDmg(Atk, DmgType.None, target),
                _ => 999999999
            };
            return $"\n{ActiveDemonName} attacks {target.Name}, dealing {_totalDmg:0.#} damage!{target.TakeDamage(_totalDmg, this)}";
        }
    }
}
