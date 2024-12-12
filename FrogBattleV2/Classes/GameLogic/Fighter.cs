using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using DotID = FrogBattleV2.Classes.GameLogic.StatusEffect.DamageOverTime.DotID;
using EffID = FrogBattleV2.Classes.GameLogic.StatusEffect.Effect.EffectID;
using PropID = FrogBattleV2.Classes.GameLogic.StatusEffect.PropertyID;
using static FrogBattleV2.Classes.GameLogic.Ability;

namespace FrogBattleV2.Classes.GameLogic
{
    internal abstract class Fighter
    {
        protected int BaseHp;
        protected int BaseAtk;
        protected int BaseDef;
        protected int BaseSpd;
        protected int MaxEnergy;
        protected double CurrentHp;
        protected double CurrentMana = 50;
        protected double CurrentEnergy = 0;
        public enum DmgType
        { // KEEP DmgType BONUS AND RES NEXT TO EACHOTHER IN EffectID!!!!!!!!!!!!!!!!!!!!
            None = 0,
            Blunt = EffID.BluntBonus,
            Slash = EffID.SlashBonus,
            Stab = EffID.StabBonus,
            Bullet = EffID.BulletBonus,
            Blast = EffID.BlastBonus,
            Magic = EffID.MagicBonus,
        }

        // List of all abilities. This allows each character to
        // have a different amount of them, and it's not a
        // nightmare to implement in Program.cs
        /// <summary>
        /// List containing every ability usable by the fighter.
        /// </summary>
        private List<Ability> _abilities = new();
        protected internal virtual List<Ability> Abilities
        {
            get
            {
                return _abilities;
            }
            protected set
            {
                _abilities = value;
            }
        }
        protected virtual string SpecialStat { get { return string.Empty; } }
        /// <summary>
        /// Returns a random double precision floating-point number that is greater or equal to 0.0, and less than 1.0.
        /// </summary>
        protected static double RNG { get { return random.NextDouble(); } }
        protected static double DmgRNG { get { return RNG / 5 + 0.9; } }
        public virtual string DodgeMsg { get { return $"{Name} dodges the attack!"; } }
        // Unlike a string such as ShieldBlockMsg, a method that returns these strings lets us do extra things
        // when someone blocks with a shield, or when it breaks, etc. It's quite handy and lets me go nuts.
        public virtual string ShieldBlock(Fighter? target) { return $"{Name}'s shield blocked all of the damage!"; }
        public virtual string ShieldBreak(Fighter? target) { return $"\n{Name}'s shield breaks!"; }
        public virtual string BarrierBlock(Fighter? target) { return $"{Name}'s barrier nullifies the damage!"; }
        // Static error messages common across all Fighters
        public static readonly ApplicationException GameEndMechanic = new("This is a game mechanic that is supposed to end the game.");

        public static readonly AbilityCheckResult AbilityUsable = new(AbilityStatus.Success, string.Empty);
        public static readonly AbilityCheckResult RepeatsTurn = new(AbilityStatus.RepeatsTurn, string.Empty);
        public static readonly AbilityCheckResult NoMana = new(AbilityStatus.NoResources, "Not enough mana!");
        public static readonly AbilityCheckResult NoEnergy = new(AbilityStatus.NoResources, "Not enough energy!");
        public static readonly AbilityCheckResult NoHp = new(AbilityStatus.NoResources, "You are going to die!");
        public static readonly AbilityCheckResult TooMuchMana = new(AbilityStatus.NoResources, "Too much mana!");
        public static readonly AbilityCheckResult TooMuchEnergy = new(AbilityStatus.NoResources, "Too much energy!");
        public static readonly AbilityCheckResult TooMuchHp = new(AbilityStatus.NoResources, "You are doing too well!");
        public static readonly AbilityCheckResult AbilityBlocked = new(AbilityStatus.TemporarilyBlocked, "You cannot use this ability right now!");
        public static readonly AbilityCheckResult InvalidAbility = new(AbilityStatus.PermanentlyBlocked, "Invalid ability!");

        public string Name { get; protected set; }
        private static readonly Random random = new();
        protected List<StatusEffect> ActiveEffects = new();
        public int Shield
        {
            get
            {
                return (int)ActiveEffects.Where(StatusEffect.IsShield).Sum(v => v.Effects.OfType<StatusEffect.Shield>().First().BaseValue * (v.Stacks - 1) + v.Effects.OfType<StatusEffect.Shield>().First().Value);
            }
        }
        public virtual double Hp
        { // Compute HP based on Shield and other effects
            get
            {
                if (CurrentHp <= 0) throw new ApplicationException($"{Name}'s HP has dropped to 0! They lost the game! ({CurrentHp})", GameEndMechanic);
                return CurrentHp;
            }
            //set
            //{
            //    if (value > BaseHp) CurrentHp = BaseHp;
            //    CurrentHp = value;
            //}
        }
        public virtual double Atk
        { // Compute ATK based on every buff and debuff
            get
            {
                double atk = BaseAtk + GetEffectsValue(EffID.ATK, BaseAtk);
                if (atk < 0) atk = 0;
                return atk;
            }
        }
        public virtual double Def
        { // Compute DEF based on every buff and debuff
            get
            {
                double def = BaseDef + GetEffectsValue(EffID.DEF, BaseDef);
                if (def < 0) def = 0;
                return def;
            }
        }
        public virtual double Spd
        { // Compute SPD based on every buff and debuff
            get
            {
                double spd = BaseSpd + GetEffectsValue(EffID.SPD, BaseSpd);
                if (spd < 0) spd = 0;
                return spd;
            }
        }
        public virtual double Mana
        { // Make sure mana doesn't go in the negatives from potential debuffs
            get
            {
                if (CurrentMana < 0) CurrentMana = 0;
                return CurrentMana;
            }
            //set
            //{
            //    else CurrentMana = value;
            //}
        }
        public virtual double Energy
        {
            get
            {
                if (CurrentEnergy > MaxEnergy) CurrentEnergy = MaxEnergy;
                else if (CurrentEnergy < 0) CurrentEnergy = 0;
                return CurrentEnergy;
            }
            //set
            //{
            //    CurrentEnergy = value;
            //}
        }
        public virtual int StunTime { get; protected set; }

        protected Ability SkipTurn => new(SkipTurnAction, new(), null);

        #region Common buffs and debuffs that can be reimplemented by different fighters
        private static readonly StatusEffect bleed = new("Bleed", PropID.Debuff, 2, 4, new StatusEffect.DamageOverTime(DotID.Bleed, 0.0125));
        private static readonly StatusEffect burn = new("Burn", PropID.Debuff, 3, 1, new StatusEffect.DamageOverTime(DotID.Burn, 100));
        private static readonly StatusEffect shock = new("Shock", PropID.Debuff, 3, 1, new StatusEffect.DamageOverTime(DotID.Shock, 0.6));
        private static readonly StatusEffect windShear = new("Wind Shear", PropID.Debuff, 3, 5, new StatusEffect.DamageOverTime(DotID.WindShear, 0.2));
        private static readonly StatusEffect slow = new("Slow", PropID.Debuff, 4, 1, new StatusEffect.Effect(EffID.SPD, -0.15, true));
        private static readonly StatusEffect vulnerability = new("Vulnerable", PropID.Debuff, 3, 1, new StatusEffect.Effect(EffID.DmgTaken, 0.15, true));
        private static readonly StatusEffect weaken = new("Weakened", PropID.Debuff, 3, 3, new StatusEffect.Effect(EffID.DmgBonus, -0.2, true));
        private static readonly StatusEffect dmgIncrease = new("Dmg Increase", 0, 4, 1, new StatusEffect.Effect(EffID.DmgBonus, 0.2, true));
        private static readonly StatusEffect defShred = new("DEF Reduction", PropID.Debuff, 3, 1, new StatusEffect.Effect(EffID.DEF, -0.4, true));
        private static readonly StatusEffect bluntRES = new("10% Blunt RES", PropID.Unremovable | PropID.Invisible, -1, 10, new StatusEffect.Effect(EffID.BluntRES, 0.1, true));
        private static readonly StatusEffect slashRES = new("10% Slash RES", PropID.Unremovable | PropID.Invisible, -1, 10, new StatusEffect.Effect(EffID.SlashRES, 0.1, true));
        private static readonly StatusEffect stabRES = new("10% Stab RES", PropID.Unremovable | PropID.Invisible, -1, 10, new StatusEffect.Effect(EffID.StabRES, 0.1, true));
        private static readonly StatusEffect bulletRES = new("10% Bullet RES", PropID.Unremovable | PropID.Invisible, -1, 10, new StatusEffect.Effect(EffID.BulletRES, 0.1, true));
        private static readonly StatusEffect blastRES = new("10% Blast RES", PropID.Unremovable | PropID.Invisible, -1, 10, new StatusEffect.Effect(EffID.BlastRES, 0.1, true));
        private static readonly StatusEffect magicRES = new("10% Magic RES", PropID.Unremovable | PropID.Invisible, -1, 10, new StatusEffect.Effect(EffID.MagicRES, 0.1, true));
        private static readonly StatusEffect allTypeRES = new("5% Damage RES", PropID.Unremovable | PropID.Invisible, -1, 10, new StatusEffect.Effect(EffID.AllTypeRES, 0.05, true));

        protected virtual StatusEffect Bleed => bleed.Clone();
        protected virtual StatusEffect Burn => burn.Clone();
        protected virtual StatusEffect Shock => shock.Clone();
        protected virtual StatusEffect WindShear => windShear.Clone();
        protected virtual StatusEffect Slow => slow.Clone();
        protected virtual StatusEffect Vulnerability => vulnerability.Clone();
        protected virtual StatusEffect Weaken => weaken.Clone();
        protected virtual StatusEffect DmgIncrease => dmgIncrease.Clone();
        protected virtual StatusEffect DefShred => defShred.Clone();
        protected static StatusEffect BluntRES => bluntRES.Clone();
        protected static StatusEffect SlashRES => slashRES.Clone();
        protected static StatusEffect StabRES => stabRES.Clone();
        protected static StatusEffect BulletRES => bulletRES.Clone();
        protected static StatusEffect BlastRES => blastRES.Clone();
        protected static StatusEffect MagicRES => magicRES.Clone();
        protected static StatusEffect AllTypeRES => allTypeRES.Clone();
        #endregion

        /// <summary>
        /// Pass the name, hp, atk, def, spd and max energy into this initialiser inside any inherited class.
        /// </summary>
        /// <param name="name">A string representing the displayed name.</param>
        /// <param name="baseHp">Percentage of the universal default HP (currently <see cref="4000"/>). Higher than 0.</param>
        /// <param name="baseAtk">Base ATK. No universal default.</param>
        /// <param name="baseDef">Base DEF. No universal default.</param>
        /// <param name="baseSpd">Base SPD. Universal default 100.</param>
        /// <param name="maxEnergy">Energy required to use Burst. No universal default.</param>
        /// <exception cref="ArgumentOutOfRangeException">Currently prevents names from having over 13 characters.</exception>
        public Fighter(string name, double baseHp, int baseAtk, int baseDef, int baseSpd, int maxEnergy)
        {
            if (name.Length > 13) throw new ArgumentOutOfRangeException(nameof(name), "Names shouldn't cross 13 characters for aesthetic reasons.");
            CurrentHp = BaseHp = (int)(baseHp * 4000);
            BaseAtk = baseAtk;
            BaseDef = baseDef;
            BaseSpd = baseSpd;
            MaxEnergy = maxEnergy;
            Name = name;
            { } // :)
        }
        protected virtual string SkipTurnAction(Fighter target)
        {
            RegenMana(5);
            return $"{Name} chooses to skip their turn and receive some mana!";
        }
        public override string ToString()
        { // Eye candy display
            string tName = Name;
            if (Name.Length <= 7) tName += "\t";
            return $"{tName}\t[{(int)Math.Ceiling(Hp) + Shield} HP, {(int)Atk} ATK, {(int)Def} DEF, {(int)Spd} SPD, {(int)Mana} MP, {(int)Math.Floor(Energy)}/{MaxEnergy}]" +
                $" === {SpecialStat}".TrimEnd(' ', '=') + DisplayEffects();
        }
        public string DisplayEffects()
        {
            string output = " - ";
            foreach (var item in ActiveEffects)
            {
                if (item.ToString() == string.Empty) continue;
                else output += item.ToString() + ", ";
            }
            return output.TrimEnd('-', ',', ' ');
        }
        public string PlayTurnCONSOLE(Fighter target, string input)
        {
            if (StunTime > 0) return input + PlayTurn(target, 0).Message;
            do
            {
                Console.Write($"{Name}: ");
                if (int.TryParse(Console.ReadLine(), out int selector))
                {
                    var output = PlayTurn(target, selector);
                    if (output.CanContinue) return output.Message;
                    else Console.WriteLine(output.Message);
                }
                else Console.WriteLine("Please enter a valid number.");
            } while (true);
        }
        /// <summary>
        /// Attempt to play the fighter's turn. Returns the success state (whether the ability was used or not) and a message output.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="abilityNr"></param>
        /// <exception cref="NoMana"></exception>
        /// <exception cref="NoEnergy"></exception>
        /// <exception cref="NoHp"></exception>
        /// <exception cref="InvalidAbility"></exception>
        /// <exception cref="AbilityBlocked"></exception>
        /// <returns></returns>
        public virtual AbilityCheckResult PlayTurn(Fighter target, int abilityNr)
        {
            string output = string.Empty;
            AbilityCheckResult result = new(AbilityStatus.FighterStunned, string.Empty);
            TurnStartResets();
            if (StunTime > 0)
            {
                output += $"\n{Name} is stunned and cannot act!";
            }
            else
            {
                result = UseAbility(abilityNr, target);
                output = result.Message;
            }
            if (result.CanContinue)
            {
                double dmg = CalculateDoT(target);
                if (dmg > 0)
                {
                    output += $"\n{Name} also takes {dmg:0.#} damage from DoT!{TakeDamage(dmg, null)}";
                }
                EffectChecks();
            }
            return new(result.Status, output);
        }

        protected virtual void TurnStartResets() { return; }

        // Basic methods for dealing common types of damage
        protected virtual double LightDmg(double src, DmgType type, Fighter target, double ignoreValue = 0)
        {
            return target.CalcReceiverDamage(CalcSenderDamage(src * 0.6, type), type, ignoreValue);
        }
        protected virtual double MediumDmg(double src, DmgType type, Fighter target, double ignoreValue = 0)
        {
            return target.CalcReceiverDamage(CalcSenderDamage(src, type), type, ignoreValue);
        }
        protected virtual double HeavyDmg(double src, DmgType type, Fighter target, double ignoreValue = 0)
        {
            return target.CalcReceiverDamage(CalcSenderDamage(src * 1.4, type), type, ignoreValue);
        }
        protected virtual double MassiveDmg(double src, DmgType type, Fighter target, double ignoreValue = 0)
        {
            return target.CalcReceiverDamage(CalcSenderDamage(src * 3, type), type, ignoreValue);
        }
        /// <summary>
        /// Checks shields and barriers before trying to reduce the HP.
        /// </summary>
        /// <param name="damage">Damage to be dealt, presumably post <see cref="CalcReceiverDamage(int, uint, double, bool)"/> calculations.</param>
        /// <returns>Flavour text, '\n' included.</returns>
        public virtual string TakeDamage(double damage, Fighter? target)
        {
            if (damage > 0)
            {
                string output = this is ICounters counters ? counters.Counter(target) : string.Empty;
                GetEnergy(3 + damage / 50);
                if (GetEffectsValue(EffID.Barrier, 0) > 0)  // If there's a barrier active
                {
                    foreach (var effect in ActiveEffects)
                    {
                        if (effect.RemoveBarrier()) return output + '\n' + BarrierBlock(target);
                    }
                }
                if (Shield > 0)
                {
                    damage = DamageShield(damage);
                    if (damage == 0) return output + '\n' + ShieldBlock(target);
                    else output += ShieldBreak(target);
                }
                CurrentHp -= damage;
                return output;
            }
            return $"\n0 damage? {Name} is amused.";
        }
        protected double DamageShield(double dmg)
        {
            while (dmg > 0 && ActiveEffects.Any(StatusEffect.IsShield))
            {
                var item = ActiveEffects.First(StatusEffect.IsShield);
                dmg = item.DamageShield(dmg);
                if (dmg > 0) ActiveEffects.Remove(item);
            }
            return dmg;
        }
        public virtual void TakeTrueDamage(double damage)
        {
            CurrentHp -= damage;
        }
        /// <summary>
        /// Calculates attacker stats (damage bonus and variation) before returning the final value as an integer.
        /// </summary>
        /// <param name="src">The character issuing the damage. Generally "this"</param>
        /// <param name="baseDamage">The raw damage to be calculated.</param>
        /// <param name="dmgType">The type of the DMG to be dealt:<para/> 0. None;<para/>1. Blunt;<para/>2. Slash;
        /// <para/>3. Stab;<para/>4. Bullet;<para/>5. Explosive;<para/>6. Magic.</param>
        /// <param name="defIgnore">A value between 0 and 1 representing how much DEF should be ignored.</param>
        /// <param name="dmgVariation">Whether the ±10% DMG variation should apply.</param>
        /// <returns>An integer representing the finalised damage number.</returns>
        public virtual double CalcSenderDamage(double baseDamage, DmgType dmgType, bool dmgVariation = true)
        {
            if (dmgVariation) baseDamage *= DmgRNG;
            double damage = baseDamage;
            damage += GetEffectsValue(EffID.DmgBonus, baseDamage);
            if (dmgType > 0) damage += GetEffectsValue((EffID)dmgType, baseDamage);
            if (damage < 0) return 0;
            else return damage;
        }
        /// <summary>
        /// Calculates receiver stats (damage resistance, defense, and def ignored) before returning the final value as an integer.
        /// </summary>
        /// <param name="src">The character issuing the damage. Generally "this"</param>
        /// <param name="baseDamage">The raw damage to be calculated.</param>
        /// <param name="dmgType">The type of the DMG to be dealt:<para/> 0. None;<para/>1. Blunt;<para/>2. Slash;
        /// <para/>3. Stab;<para/>4. Bullet;<para/>5. Explosive;<para/>6. Magic.</param>
        /// <param name="defIgnore">A value between 0 and 1 representing how much DEF should be ignored.</param>
        /// <param name="dmgVariation">Whether the ±10% DMG variation should apply.</param>
        /// <returns>An integer representing the finalised damage number.</returns>
        public virtual double CalcReceiverDamage(double baseDamage, DmgType dmgType, double defIgnore = 0)
        {
            if (defIgnore > 1) defIgnore = 1;
            double damage = baseDamage;
            damage -= GetEffectsValue(EffID.AllTypeRES, baseDamage);
            if (dmgType > 0) damage -= GetEffectsValue((EffID)dmgType + 1, baseDamage);
            damage -= Def * (1 - defIgnore);
            damage += GetEffectsValue(EffID.DmgTaken, damage);
            if (damage < 0) return 0;
            else return damage;
        }
        public virtual bool Dodge(Fighter target)
        { // Dodge chance based on SPD (Barriers guarantee a false)
            if (GetEffectsValue(EffID.Barrier, 0) > 0) return false;
            else if (StunTime > 0) return false;
            else if (Spd - target.Spd > RNG * 100) return true;
            else return false;
        }
        public virtual double RegenMana(double value)
        { // Basic Mana regen method
            if (value > 0) value += GetEffectsValue(EffID.ManaRecovery, value);
            CurrentMana += value;
            return value;
        }
        /// <summary>
        /// Heal the character, keeping track of healing effects.
        /// </summary>
        /// <param name="value">The amount of healing before effect calculations.</param>
        /// <returns>The amount that was actually healed after every calculation.</returns>
        protected virtual double Heal(double value)
        {
            value += GetEffectsValue(EffID.IncomingHealing, value);
            CurrentHp += value;
            return value;
        }
        public virtual string Stun(int turns, Fighter source)
        { // Stun has a chance to be resisted based on SPD and 
          // the amount of turns that Stun is supposed to last for
            int nr = 0;
            for (int i = 0; i < turns; i++)
            {
                if (Spd - source.Spd + i * 10 > RNG * 100)
                {
                    turns--;
                    nr++;
                }
            }
            if (turns > StunTime) StunTime = turns;
            if (turns == 0) return $"\n{Name} is too fast to get stunned!";
            //else if (nr > 0) return $"\n{Name} proves resilient and only gets stunned for {turns} of the {turns+nr} rounds!";
            else return $"\n{Name} gets stunned for {turns} rounds!";
        }
        public void TrueStun(int turns)
        {
            if (turns > StunTime) StunTime = turns;
        }
        /// <summary>
        /// <para>Checks ActiveEffects for an identical effect, then adds stacks to it (if stackable) and refreshes its duration.</para>
        /// <para>Throws an exception if <paramref name="totalStacks"/> is less than 1.</para>
        /// </summary>
        /// <param name="effect">The effect to be stacked, refreshed or added at the end of ActiveEffects.</param>
        public void AddEffect(StatusEffect effect, uint totalStacks = 1)
        {
            if (totalStacks < 1) throw new ArgumentOutOfRangeException(nameof(totalStacks), "Cannot add 0 stacks!");
            effect.AddStacks(totalStacks - 1);
            // Checks for any identical effects and if they're stackable 
            // before adding them in (stackable debuffs get their turns refreshed)
            foreach (var item in ActiveEffects)
            {
                if (item == effect)
                {
                    effect.AddStacks(item.Stacks);
                    ActiveEffects[ActiveEffects.IndexOf(item)] = effect;
                    return;
                }
            }
            ActiveEffects.Add(effect);
        }
        public bool RemoveEffect(StatusEffect effect)
        {
            return ActiveEffects.Remove(effect);
        }
        /// <summary>
        /// Removes <paramref name="count"/> buffs or debuffs, depending on the value of <paramref name="isDebuff"/>. Newest first.
        /// </summary>
        /// <param name="count">The amount of effects to remove.</param>
        /// <param name="isDebuff">Whether to remove buffs or debuffs.</param>
        /// <returns></returns>
        public List<StatusEffect> CleanseEffects(int count, bool isDebuff)
        {
            List<StatusEffect> output = new();
            if (count > ActiveEffects.Count) count = ActiveEffects.Count;
            for (int i = ActiveEffects.Count - 1; i >= ActiveEffects.Count - count && i >= 0; i--)
            {
                if (!ActiveEffects[i].IsType(PropID.Unremovable) &&
                    ActiveEffects[i].IsType(PropID.Debuff) == isDebuff)
                {
                    output.Add(ActiveEffects[i]);
                    ActiveEffects.RemoveAt(i);
                    count--;
                }
                else count++;
            }
            return output;
        }
        protected void EffectChecks()
        { // End of turn effect checks
            RegenMana(5);
            if (StunTime > 0) StunTime--;
            else
            {
                ActiveEffects.ForEach(StatusEffect.Expire);
                ActiveEffects.RemoveAll(StatusEffect.Expired);
            }
        }
        /// <summary>
        /// Gets the cumulative value for buffs and debuffs of the same type.
        /// Used for calculating current stats (ATK, DEF, etc), among other things.
        /// </summary>
        /// <param name="effectType">Check <see cref="StatusEffect.EffectID"/> for clarification.</param>
        /// <param name="baseValue">The base value used for calculating percentage based effect values.</param>
        /// <returns>The total value of all effects of type <paramref name="effectType"/>. Default is 0.</returns>
        public double GetEffectsValue(EffID effectType, double? baseValue)
        { // Gets the cumulative value for buffs and debuffs of the same type
          // Used for calculating current stats (ATK, DEF, etc) and other things
            double? value = null;
            foreach (var effect in ActiveEffects)
            {
                if (effect.GetEffectsOfType(effectType, baseValue) != null)
                {
                    value ??= 0;
                    value += effect.GetEffectsOfType(effectType, baseValue);
                }
            }
            return value ?? 0;
        }
        public bool FindEffect(string name)
        {
            return ActiveEffects.Find(x => x.Name == name) != null;
        }
        public bool FindEffect(StatusEffect effect)
        {
            return ActiveEffects.Contains(effect);
        }
        public double CalculateDoT(Fighter source)
        { // Calculates DoT by checking DoT resistance and vulnerability effects
            double dmg = 0;
            foreach (var effect in ActiveEffects)
            {
                foreach (var dot in effect.Effects.OfType<StatusEffect.DamageOverTime>())
                {
                    dmg += CalcReceiverDamage(dot.DotType switch
                    {
                        DotID.Bleed     => BaseHp,
                        DotID.Burn      => 1,
                        DotID.Shock     => source.Atk,
                        DotID.WindShear => source.Atk,
                        _ => 1
                    } * dot.Value * effect.Stacks, DmgType.None, dot.IgnoreDef ? 1 : 0);
                }
            }
            dmg += GetEffectsValue(EffID.DoTTaken, dmg);
            return dmg;
        }
        public double CalculateDoT(Fighter source, DotID dotID)
        { // Calculates DoT by checking DoT resistance and vulnerability effects
            double dmg = 0;
            foreach (var effect in ActiveEffects)
            {
                foreach (var dot in effect.Effects.OfType<StatusEffect.DamageOverTime>().ToList().FindAll(x => x.DotType == dotID))
                {
                    dmg += CalcReceiverDamage(dot.DotType switch
                    {
                        DotID.Bleed     => BaseHp,
                        DotID.Burn      => 1,
                        DotID.Shock     => source.Atk,
                        DotID.WindShear => source.Atk,
                        _ => 1
                    } * dot.Value * effect.Stacks, DmgType.None, dot.IgnoreDef ? 1 : 0);
                }
            }
            dmg += GetEffectsValue(EffID.DoTTaken, dmg);
            return dmg;
        }
        public virtual void GetEnergy(double energy)
        { // Simple calculation that checks Energy buffs/debuffs
            energy += GetEffectsValue(EffID.Energy, energy);
            CurrentEnergy += energy;
        }
        protected virtual AbilityCheckResult UseAbility(int nr, Fighter target)
        {
            AbilityCheckResult abilityResult;
            if (nr > Abilities.Count || nr < 0) return InvalidAbility;
            if (nr == 0) abilityResult = SkipTurn.ExecuteAbility(this, target);
            else abilityResult = Abilities[nr-1].ExecuteAbility(this, target);
            //if (this is IFollowsUp followsUp && abilityResult.IsUsable) return abilityResult with { Message = abilityResult.Message + followsUp.FollowUp.ExecuteAbility(this, target).Message };
            //if (this is IAbilityBonus bonus && ???) return abilityResult with { Message = abilityResult.Message + bonus.Bonus.ExecuteAbility(this, target).Message };
            if (abilityResult.CanContinue) return abilityResult + (nr != 0 && this is IFollowsUp followsUp ? followsUp.FollowUp.ExecuteAbility(this, target) : AbilityCheckResult.Empty) + (this is ISummons summons ? summons.Summon.ExecuteAbility(this, target) : AbilityCheckResult.Empty);
            else return abilityResult;
        }
        internal AbilityCheckResult CheckUsability(Ability ability)
        {
            if (ability.AbilityCost.IsHardCost)
            {
                if (Mana < ability.AbilityCost.ManaCost + GetEffectsValue(EffID.ManaCost, ability.AbilityCost.ManaCost)) return NoMana;
                if (Energy < ability.AbilityCost.EnergyCost) return NoEnergy;
                if (Hp < ability.AbilityCost.HealthCost) return NoHp;
            }
            else if (ability.AbilityCost.IsReverseCost)
            {
                if (Mana > ability.AbilityCost.ManaCost + GetEffectsValue(EffID.ManaCost, ability.AbilityCost.ManaCost)) return TooMuchMana;
                if (Energy > ability.AbilityCost.EnergyCost) return TooMuchEnergy;
                if (Hp > ability.AbilityCost.HealthCost) return TooMuchHp;
            }
            if (ability.CustomUnusabilityConditions != null) return ability.CustomUnusabilityConditions(this);
            else return ability.SuccessValue;
        }
        internal void DeductCost(Ability ability)
        {
            if (!ability.AbilityCost.IsReverseCost)
            {
                CurrentMana -= ability.AbilityCost.ManaCost + GetEffectsValue(EffID.ManaCost, ability.AbilityCost.ManaCost);
                CurrentEnergy -= ability.AbilityCost.EnergyCost;
                TakeTrueDamage(ability.AbilityCost.HealthCost);
            }
        }
    }
}
