# FrorgBattle
Turn based fighting game which can be (relatively?) easily scaled by just adding more characters inheriting from the abstract Fighter class.
It's silly, ridiculous even, perchance even a nightmare at times. But it's my nightmare. And now you get to share it with me.

### Some stuff about game logic
* Most attacks have a ±10% damage variation to keep the game exciting.
* Every buff/debuff refreshes their duration when a new stack is added (or when trying to add, even if max stacks have been reached)
* The "average" stats are something around 4000 HP, 80 ATK, 30 DEF, 100 SPD. These are completely arbitrary, though, and despite making the bloody game I'm not sure if this could be called average or below it.
* DEF is perfectly linear. Trying to deal 60 damage to an enemy with 20 DEF? Only 40 of it will come through. And yes, this means that having more DEF than the damage being dealt results in 0 damage.
* Multiplicative and additive buffs do not stack. Additive buffs add to the base value, and multiplicative buffs add to the base value a percentage of itself. However, when going from attacker checks to target checks, the damage value calculated by the attacker is passed as the base damage for the target. This means that, while two 100% damage buffs on the attacker don't stack for a 300% bonus, they DO stack with a 100% damage taken increase on the target.
* There are a few different types of damage: Blunt, Slash, Stab, Bullet, Blast, and Magic. These don't do that much right now, but certain characters have small (10%) damage resistances to a specific type, just because. Can you figure out which ones resist which type?
* These specific damage buffs also have the special property of applying prior to defense calculations, unlike general damage buffs, which apply afterwards. What this means is that a 20 dmg attack on a target with 10 DEF and 100% dmg taken increase debuff will deal 20 damage ((20 Dmg - 10 DEF) + 100%). Had the debuff been a -100% \[dmg type\] RES, an attack of that type would have dealt 30 damage ((20 Dmg + 100%) - 10 DEF). And, yes, Dmg RES can go into the negatives and become evil.
* All\* characters have a Burst, which requires energy instead of mana to activate. Energy can be gained by using attacks, taking damage, or other means specific to the character.
* DoT, damage over time, is a debuff that deals damage at the end of your turn. The main types currently in the game are Bleed, Burn, Shock, and Wind Shear. Bleed deals damage based on Max HP, lasts for 2 turns, and stacks up to 4 times. Burn deals a set amount of damage. Shock deals damage based on the attacker's ATK (60%). Wind Shear also deals damage based on ATK (20%), but stacks up to 5 times and ignores DEF.
* This giant block of text is completely unreadable, isn't it.

# List of Characters
All characters have their own unique abilities to use, and some even have special properties. Currently, the game is a 1v1 and characters are selected at the start of the game via letters (a - f).

- [a. Rexulti](#rexulti)
- [b. Bayonetta](#bayonetta)
- [c. Raiden](#raiden)
- [d. Cubic](#cubic)
- [e. Mami Tomoe](#mami-tomoe)

## Rexulti
### Special stats
* Starts with 80 ATK and 30 DEF, but reaches 100 ATK upon switching to Phase 2 through the Phase 1 Burst.
* Every successful attack in Phase 2 will apply a debuff to the enemy that increases damage taken by 15%, up to 6 stacks, lasting for 3 turns.

### Abilities

Phase 1 ONLY:

1. Slices the enemy for Medium Slash Damage, inflicting bleed if damage is in the high ranges. **11 Mana**
2. Summons the dreaded Piss Rain from my original, C++ FrorgBattle. -15% ATK and DEF alongside a 20 Dmg DoT for 3 turns. **18 Mana**
3. Stab yourself for significant energy generation and an 80% ATK buff for 5 turns. **16 Mana, 100 HP**
4. Exploits a bug in the code, dealing Medium Dmg that ignores any sort of damage resistances or bonuses for both the target and the attacker. **22 Mana**
5. Shoot a silver bullet at the target and remove one of their buffs while doing Heavy Bullet Dmg. **20 Mana**
6. BURST: Regenerate 15 mana, set Base ATK to 100, do 3 instances of damage with different properties, and switch to Phase 2. **120 Energy**

Phase 2 ONLY:

1. Summon the powers of communism and deal Heavy Dmg of no type to the target. **14 Mana**
2. Bribe some demons to reduce the ATK, DEF and SPD of the target by 25%. **15 Mana**
3. Attempt to make 3 deals with demons. Each successful deal does Medium Magic Dmg to the target and regenerates 5 mana. Failures result in self-inflicted Light Magic Dmg. This ability's success rate declines with repeated uses. **20 Mana**
4. Shoot 3 silver bullets at the target, dealing Heavy Bullet Dmg for every hit and stealing a buff from the enemy if at least one bullet hit the target. **25 Mana**
5. Summon friends of the enemy for three turns, who deal Medium Dmg that ignores 50% of DEF. **23 Mana**
6. BURST: Deal Heavy Magic Dmg before stunning the target for 2 turns and gaining 35 mana alongside a 30 ATK buff (flat). **120 Energy**

## Bayonetta
### Special stats
* Bayonetta has 3 weapons to pick from, each with 4 unique abilities. Her second weapon, the yo-yo, also boosts her speed by 10% while deployed.
* Upon entering Phase 2, which is done by using an ability of the third weapon, Bayonetta reduces her defense but gains a permanent summon, which does Medium Magic Dmg on every turn.

### Abilities

Weapon 1 - "Colour My World":

1. Punch and kick your opponent for Light Blunt Damage. This ability permanently improves slightly with every use. **8 Mana**
2. Apply a 15% ATK, DEF and SPD debuff on your target for 3 turns. **25 Mana**
3. Gain a 400 HP Shield for up to 3 turns. **20 Mana**
4. Launch an undodgeable attack that deals Medium Bullet Dmg. **15 Mana**

Weapon 2 - "Ignis Araneae Yo-Yo":

1. Throw your Yo-Yo at your enemy and deal Medium Blunt Dmg. **8 Mana**
2. Stun your target for up to 3 turns (RNG Based). **25 Mana**
3. Deal Heavy Slash Dmg with your Yo-Yo and apply one stack of Bleed. **20 Mana**
4. Create an explosion that deals Heavy Blast Dmg. **17 Mana**

Weapon 3 - "Unforgiven":

1. Curse the enemy for 3 turns, reducing their ATK and DEF by 10 (flat) and inflicting a 20 Dmg DoT. **10 Mana**
2. Gain a buff to your ATK, DEF and SPD based on 10% of the enemy's stats for 3 turns. **25 Mana**
3. Deal Heavy Dmg based on your SPD. **20 Mana**
4. Switch to Phase 2 while dealing 3 instances of Heavy Magic Dmg and setting your Base ATK to 100 and Base DEF to 25. **75 Mana**  
   If this ability was already used once in this fight, deal one instance of Heavy Magic Dmg instead. **20 Mana**

Universal Abilities:

5. Switch your weapon to the next. **5 Mana**
6. BURST: Stun the enemy for 4 full turns and regenerate 30 mana. **120 Energy**

## Raiden
### Special stats
* Raiden's first and second abilities combined can launch over 15 different attacks. This is because they can combo in a binary tree structure - launching a heavy attack after a light attack is different from launching a heavy attack right off the bat. Here's the full list of combos: (L for Light, R for Heavy)  
  L, R, LR, RL, RR, LRL, LRR, RLR, RRL, RRR, LRLL, LRLR, RRLR, RRRL, RRRR, LLLLL, RRRLR
* Missing an attack, trying to make a combo that doesn't exist, and skipping your turn all reset your combo.
* Raiden's burst doesn't consume his accumulated energy. Instead, it costs 75 energy per attack and ends when you run out of energy (or after 7 turns).
* Raiden can counterattack for Light Slash Dmg once every turn (if he dodges the attack).

### Abilities

1. Use a light attack. **10 Mana**
2. Use a heavy attack. **15 Mana**
3. Hold up your sword to block every instance of damage from the enemy's next attack. Every hit blocked regenerates 10 energy. This ability resets your combo and cannot be used during Ripper Mode. **20 Mana**
4. Convert all of your current energy into mana with a 2.5 : 1 ratio. This ability does not end the turn. **Any Energy**
5. BURST: Enter Ripper Mode. ATK +80%, DEF -100%, SPD +50%, Energy Regen +200%. Ripper Mode ends when energy reaches 0, or after 7 turns. **150 Energy**

## Cubic
### Special stats
* Starts with 250 SPD. Each dodge he takes reduces his speed until he takes damage, at which point he switches to Phase 2, regenerating HP and Mana based on SPD above 100.
* Enters Phase 3 upon taking damage with 250 energy, regenerating 50% of Max HP and gaining two new abilities. In this phase, he gains Base ATK whenever he takes damage, and loses an extra 50 HP every turn.

### Abilities

Phase 1:

1. Throws a few knives, dealing 5 instances of Light Stab Dmg. **20 Mana**
2. Summons Gaster Blasters, dealing 3 instances of Medium Blast Dmg. **20 Mana**
3. Throws a knife that deals Medium Stab Dmg, ignoring 50% DEF and gaining a 2x Mana Regen buff for 3 turns. **20 Mana**
4. Throws a dice. This ability has 10 possible outcomes, ranging from a 3 turn, 2x mana cost increase for himself to doubling the damage he deals, doubling the damage the enemy receives, and regenerating 20 mana. **15 Mana**
5. Gets a 15% damage bonus with 5 max stacks for 4 turns. **20 Mana**

Phase 2:

6. Heals 400 HP and dispels one debuff. This ability reduces in effectiveness if used too often. **40 Mana**
7. Summons a blaster that has a 20% chance to trigger in subsequent turns. For every turn that it doesn't activate in, Cubic gets a 15% ATK buff, up to 5 stacks, which is removed after the blaster fires. Also has a dodge-based chance to reduce the target's DEF by 40% for 3 turns. **20 Mana**
8. Reduces target's Dmg dealt by 20% for 3 turns (max 3 stacks). **15 Mana**
9. Increases his DEF by 20, but reduces his ATK by 10 (flat). **20 Mana**
10. Gains a 300 HP shield for up to 5 turns. **22 Mana**

Phase 3:

11. Uses 3 random abilities at the same time. This ability may be a bit buggy. **50 Mana**
12. Reduces target's DEF by 100%. This debuff can be cleansed, but lasts forever otherwise. **25 Mana**

## Mami Tomoe
### Special Stats
* Mami starts with significantly lower Base SPD than everyone else, but gains one stack of Rage for every hit she misses. Many of her attacks also grant bonus Rage stacks for missing every hit. Rage stacks up to 20, and increases her damage by 1.5% per stack. Upon unleashing her burst, she consumes all Rage stacks and turns them into Precision, which increases her Bullet and Blast Dmg bonus by 3.5% and 5% respectively while also reducing her mana cost and energy regen by 2.5% and 5% respectively per stack. When Mami has Precision stacks, the enemy's dodge chance is calculated based on their amount (from 95% at 1 stack to 0% at 20 stacks) (compounding with regular dodge chance).
* When at max energy and below 20 Rage stacks, consume 25 energy to gain one stack of Rage. 3 turn cooldown.

### Abilities

1. Fire multiple flintlocks at the target, dealing 4 instances of Light Bullet Dmg. Precision increases the amount of attacks, up to 6. **12 Mana**
2. Load up a double barrel shotgun and launch 2 instances of Medium Bullet Dmg that can ignore up to 25% DEF (RNG Based). Precision increases the amount of DEF bypass, up to 100%. **14 Mana**
3. Summon a musket barrier to protect you for up to 5 turns. The barrier can nullify 3 instances of damage, as well as deal damage to the attacker and heal for 100% of damage dealt. **27 Mana**
4. Fire a cheese-imbued bullet at the enemy. If it hits, Charlotte will attack the enemy for 3 turns, dealing Medium Dmg. There may or may not be a 1% chance for her to deal 10x more damage. **25 Mana**
5. Summon 10 Magic Ribbons that assault your target, regenerating 2 mana and reducing their Energy and Mana Regen by 10% per successful hit. If all 10 ribbons hit, stun the enemy for 1 turn. Precision also adds a -15% SPD debuff upon using this attack, but reduces mana regenerated by each ribbon. **45 Mana**
6. Summon 3 recursive bullets. Each bullet has a chance to be recalled upon both hits and misses, up to 12 shots total. This ability is more effective at lower Rage stacks. **17 Mana**
7. Fire a barrage of 100 pellets with reduced accuracy, and deal Bullet Dmg based on the amount of hits. If you miss at least 60 shots, steal up to 25 mana from the opponent. If you miss at least 80 shots, stun the enemy for 1 - 3 turns, while also executing the previous effect. **33 Mana**
8. BURST: Turn all your Rage stacks into Precision and fire a giant blaster, dealing Massive Blast Dmg, followed by 3 subsequent Heavy Bullet Dmg attacks. Also reduce the enemy's Bullet and Blast RES by up to 24% and 45% respectively, based on the amount of Precision stacks. **180 Energy**

# Sections

- [FrorgBattle](#frorgbattle)
    - [Some stuff about game logic](#some-stuff-about-game-logic)
- [List of Characters](#list-of-characters)
  - [Rexulti](#rexulti)
    - [Special stats](#special-stats)
    - [Abilities](#abilities)
  - [Bayonetta](#bayonetta)
    - [Special stats](#special-stats-1)
    - [Abilities](#abilities-1)
  - [Raiden](#raiden)
    - [Special stats](#special-stats-2)
    - [Abilities](#abilities-2)
  - [Cubic](#cubic)
    - [Special stats](#special-stats-3)
    - [Abilities](#abilities-3)
  - [Mami Tomoe](#mami-tomoe)
    - [Special Stats](#special-stats-4)
    - [Abilities](#abilities-4)
- [Sections](#sections)
