Scapegoat

A 7-week team project built in Unity where I worked as an enemy and level systems programmer, focusing on combat systems, AI behavior, wave spawning, progression logic, and performance-conscious design.

🎮 Overview

Scapegoat is a third-person action game centered around combat encounters and objective-based progression.

My work focused on building systems that:

support scalable enemy behavior

enable controllable encounter pacing

integrate combat with level progression

remain performant under increasing enemy counts

🧠 My contributions

Designed and implemented a reusable enemy architecture using inheritance

Built state-driven AI for melee and ranged enemies

Developed a wave-based spawning system with scaling and composition control

Implemented a Unity object pooling system for enemy reuse

Built campfire-based objective flow tied to combat progression

Implemented portal unlock logic with save/load integration

Debugged and resolved AI state and combat edge cases

Collaborated closely with designers to translate gameplay intent into systems

⚙️ Key systems
Enemy architecture

I structured enemies using a small inheritance hierarchy:

Enemy → shared functionality (health, movement, state handling)

EnemyGrunt → shared behavior layer

MeleeEnemy / RangedEnemy → specialized combat logic

This allowed me to:

reuse core functionality across enemy types

isolate combat differences cleanly

extend behavior without duplicating logic

Enemy AI (state-driven behavior)

Enemy behavior is driven by explicit state transitions:

Patrolling

Chasing

Attacking

Dying

State changes are based primarily on distance checks and combat conditions.

This approach:

kept behavior predictable

simplified debugging

reduced conflicting logic between actions

Example:

if (sqrDistanceToPlayer <= attackRange * attackRange)
    SetState(EnemyState.Attacking);
Wave spawning and encounter design

I implemented a wave-based spawning system to control combat pacing.

Key features:

progressive wave scaling

melee/ranged enemy composition control

multiple spawn points

active enemy cap to prevent overload

Example:

int waveSize = Mathf.RoundToInt(baseWaveSize * Mathf.Pow(spawnRateIncreaseMultiplier, waveCounter));

This allowed encounters to:

escalate over time

remain readable for the player

avoid overwhelming spikes

Performance and enemy management

During development, frequent instantiation/destruction caused performance issues.

To address this, I implemented an object pooling system:

GameObject obj = objectPool.Dequeue();
obj.SetActive(true);

Instead of creating/destroying enemies:

enemies are reused from a pool

allocation overhead is reduced

combat performance becomes more stable

Additional measures:

enemy cap limits

NavMesh-based spawn validation

Objectives and progression

I implemented a campfire-based objective system tied to combat progression.

defeating enemies contributes to objective completion

completing objectives unlocks portals

portal states persist through save/load data

Example:

if (extinguishedCampfires >= requiredCampfires)
{
    UnlockPortal();
}

This connected:

combat → objectives → level progression

🧪 Technical challenges
Enemy state issues

Problem: Enemies could get stuck or fail to transition correctly
Fix: Tightened state transition logic and validation checks
Result: More reliable and predictable combat behavior

Attack range bugs

Problem: Enemies attacked outside valid range
Fix: Added precise distance checks with thresholds
Result: Improved combat feel and fairness

Spawn issues

Problem: Enemies spawned too close or in invalid locations
Fix: Added NavMesh-based spawn validation
Result: Fairer and more consistent encounters

Performance under load

Problem: Too many enemies caused runtime slowdowns
Fix: Introduced enemy caps and object pooling
Result: More stable performance during intense encounters

🤝 Collaboration

I worked closely with designers to:

translate gameplay flow into systems

iterate on enemy behavior and pacing

refine difficulty and progression through playtesting

Design intent was often communicated through flowcharts, which I translated into structured gameplay logic.

🧠 Key takeaways

Designing reusable gameplay systems improves iteration speed

Simple, explicit state logic is often more robust than complex abstractions

Performance considerations must be integrated early in system design

Collaboration with designers is essential for achieving good game feel

🛠 Tech

Unity

C#

NavMesh

Perforce

📎 Notes

This repository contains selected systems and scripts from the project.
