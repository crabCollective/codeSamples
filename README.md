# Collective of Crab public code samples
This public repository serves as a basic showcase of my coding abilities. It contains multiple short examples of a selected excerpts from my work. All examples are written in C# and are meant to be used within Unity game engine. Currently (July 2022), following code examples are presented in this repository:

**1) Pooling system used widely in my games**

This pooling system is utilizing factory pattern and it is made for simple extendability. In typical scenario, user just derives from basic MonoObjectFactory class, adds the script on the GameObject in Unity and simple factory with pooling is ready to be used (see it's usage in Enemies section below).

**2) Simple enemy system**

This section comprises from multiple scripts:

* *EnemyFactory.cs* - utilization of MonoObjectFactory class with the usage of spawn from the spawn points put to the game world

* *EnemyController.cs*, UfoEnemyController.cs* - showcase of the enemy scripts with the usage of Unity tweening system DoTween. In this particular case, the Ufo enemy is meant to be spawned from particular spawn point, then it goes to random place in game area and then it starts "patrolling" - flying back and forth and shooting in given interval

* *EnemyGenerator.cs* - simple enemy generator which is checking the number of enemies on the scene and spawning new ones in given interval. It is using weighted random technique to generate enemies in user-defined weights

* *BulletController.cs* - bullet script, how I handle bullets in my games

**3) Procedural tilemap generator**

This generator is using perlin noise algorithm to generate random 2D terrain with continuosly changing height. The idea is that we are generating tiles to the Unity tilemap and we apply composite collider on whole chunk when generation is done. We can use either fast generation, but in that case we might experience stuttering, or we can use slow generation, when the generation itself is divided within certain amount of frames (made in coroutine) and everything is more smooth. 
