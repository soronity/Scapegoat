🎮 Scapegoat

Enemy & Level Systems Programming — Unity (C#)
7-week team project — this repository contains only my contributions

🧭 Overview

Scapegoat is a third-person action game developed as a team project in Unity.

This repository contains the systems and scripts I personally implemented, focusing on:

enemy behavior

combat systems

encounter design

progression logic

performance-conscious enemy management

My goal was to build scalable gameplay systems that support responsive combat while remaining stable under increasing enemy counts.

🧠 My Contributions

The code in this repository reflects my work on:

🧱 Reusable enemy architecture using inheritance

🤖 State-driven AI for melee and ranged enemies

🌊 Wave-based spawning with scaling and composition control

♻️ Object pooling system for enemy reuse

🔥 Campfire objective system tied to combat progression

🚪 Portal unlock system with save/load integration

🐛 Debugging AI state issues and combat edge cases

⚙️ Key Systems (from my implementation)
🧱 Enemy Architecture

I structured enemies using a small inheritance hierarchy:

Enemy → shared logic (health, movement, state handling)

EnemyGrunt → shared behavior layer

MeleeEnemy / RangedEnemy → specialized combat

Why this approach:

avoids duplicated logic

keeps behavior modular

makes new enemy types easier to extend

🤖 Enemy AI (State-Driven Behavior)

Enemy behavior is driven by explicit state transitions:

Patrolling

Chasing

Attacking

Dying

State changes are primarily based on distance checks and combat conditions.

if (sqrDistanceToPlayer <= attackRange * attackRange)
    SetState(EnemyState.Attacking);

💬 State-driven logic keeps behavior predictable and easier to debug.

🌊 Wave Spawning & Encounter Design

I implemented a wave-based spawning system to control combat pacing:

progressive difficulty scaling

melee/ranged composition control

multiple spawn points

active enemy cap

int waveSize = Mathf.RoundToInt(
    baseWaveSize * Mathf.Pow(spawnRateIncreaseMultiplier, waveCounter)
);

💬 Encounters escalate over time while staying readable and controlled.

♻️ Performance & Enemy Management

Frequent instantiation/destruction caused performance issues early in development.

To address this, I implemented an object pooling system:

GameObject obj = objectPool.Dequeue();
obj.SetActive(true);

Impact:

reduced allocation overhead

fewer frame spikes

more stable combat under load

Additional measures in my systems:

enemy count caps

NavMesh-based spawn validation

🔥 Objectives & Progression

I implemented a campfire-based objective flow tied to combat:

defeating enemies contributes to objective progress

completing objectives unlocks portals

portal states persist through save/load data

if (extinguishedCampfires >= requiredCampfires)
{
    UnlockPortal();
}

💬 This connects combat → objectives → progression.

🧪 Technical Challenges (from my systems)
⚠️ State transition bugs

Problem: Enemies could get stuck or fail to transition
Fix: Tightened state checks and transition conditions
Result: More reliable combat behavior

🎯 Attack range issues

Problem: Enemies attacked outside valid range
Fix: Added precise distance validation
Result: Improved fairness and responsiveness

📍 Spawn issues

Problem: Enemies spawned too close or in invalid positions
Fix: NavMesh-based spawn validation
Result: Fairer encounter starts

🚀 Performance under load

Problem: Too many enemies caused slowdowns
Fix: Enemy caps + object pooling
Result: More stable performance during intense fights

🤝 Collaboration Context

Scapegoat was developed in a team environment.

While this repository contains only my code, my systems were built to integrate with:

designer-defined gameplay flow

level structure and objectives

broader game systems developed by the team

I worked closely with designers to:

translate gameplay intent into systems

iterate on enemy behavior and pacing

refine combat through playtesting

🧠 Key Takeaways

Designing reusable systems improves iteration speed

Simple, explicit logic leads to more reliable gameplay behavior

Performance must be considered early in combat-heavy systems

Collaboration is essential for achieving strong game feel

🛠 Tech Stack

Unity

C#

NavMesh

Perforce

🔗 Notes

This repository contains a curated subset of scripts representing my contributions to the Scapegoat project.
