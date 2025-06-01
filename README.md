## Orbital AY2425

Game Development Project

## Credits

- Jordan Low
- Maximus Ng 

### Artemis Milestone Notes

We spent this cycle focusing on getting out a basic playable prototype of a level to demonstrate our intended game features.

This cycle included a very broad set of features which made it challenging to get a minimum testable version of the game as so many moving parts ended up relying on other features, like Enemy behaviour requiring Combat to work, and in the end some of the features ended up as rough jobs to allow for time to test other features. A brief list of features include Movement, Combat, Enemy behaviours, some basic UI, enemy pathfinding, etc.

We hope that the solid foundations we've laid in this Sprint will ease the development in future Sprints.

### About Insatiable

You are controlling a blue slime, wandering through a dungeon. Slimes that are not your colour are your enemies. Fight them off and consume their remains.

### Controls

- Movement: WASD [Keyboard], Left Stick [Gamepad]
- Attacking: Left Click [Keyboard], West Button [Gamepad] (Square/Y/X on PS/Switch/XBox respectively)
- Jumping: Spacebar [Keyboard], South Button [Gamepad] (X/B/A on PS/Switch/XBox respectively)
- Consume: F [Keyboard], East Button [Gamepad] (Circle/A/B on PS/Switch/XBox respectively)
- Attenuate: Right Click [Keyboard], North Button [Gamepad] (Triangle/X/Y on PS/Switch/XBox respectively)
- Gear Wheel: E [Keyboard], Left Bumper [Gamepad]
- Exiting the game: Alt + F4

# Disclaimers
Gear Wheel presently serves little purpose (other than slowing down the game) as only one piece of grar is currently accessible
Bounding currently has some buggy behaviours regarding nearby enemy deaths
There is currently no way to restart or exit the game without simply Alt+F4 to close the program

## Control Guide

Move through the dungeon to explore what's within. Press Attack to begin a 3-stage combo. Attacking while near an enemy will move and rotate you in the correct direction, so hammer away. Killing an enemy will cause it to deflate into a puddle and disappear after a while.

Press Jump to begin a jump. You cannot jump while attacking, but you can attack and land while mid-air freely. Press Jump again while mid-air to Bound towards a nearby enemy. Bounding to an enemy will stun the enemy and bring you riding atop it. While you're bounding an enemy, your only option is to jump off it by pressing Jump again.

Enemies will attack you as well, and the HP Bar at the top left of your screen will begin to deplete. Hit an enemy a few times, then press Consume on them to eat them and recover some health. Different enemies have different health thresholds for when you can consume them, detailed below.

If you see an incoming attack, you can press Attenuate to briefly freeze in place. While frozen, you will take no damage from incoming attacks. This ability has a cooldown however, so no spamming it!

## Stats and Details

Basic Slimes will move towards the player and try to hit them
Large Slimes will do the same but hit harder
Radical Slimes will approach the player and wind up a jump, if allowed to complete the jump, they will leap towards the player and explode for huge damage
