# Using NavMesh Agent with Other Components

You can use [**NavMesh**][1] Agent, NavMesh Obstacle, and Off [**Mesh**][2] Link components with other Unity components too. Here’s a list of dos and don’ts when mixing different components together.

## NavMesh Agent and Physics

- You don’t need to add physics [**colliders**][3] to NavMesh Agents for them to avoid each other
    - That is, the navigation system simulates agents and their reaction to obstacles and the static world. Here the static world is the baked NavMesh.
- If you want a NavMesh Agent to push around physics objects or use physics triggers:
    - Add a Collider component (if not present)
    - Add [**Rigidbody**][4] component
        - Turn on kinematic (Is Kinematic) - this is important!
        - Kinematic means that the rigid body is controlled by something else than the physics simulation
- If both NavMesh Agent and Rigidbody (non-kinematic) are active at the same time, you have race condition
    - Both components may try to move the agent at the same which leads to undefined behavior
- You can use a NavMesh Agent to move e.g. a player character, without physics
    - Set players agent’s avoidance priority to a small number (high priority), to allow the player to brush through crowds
    - Move the player agent using [NavMeshAgent.velocity](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-velocity.html), so that other agents can predict the player movement to avoid the player.

## NavMesh Agent and Animator

- NavMesh Agent and Animator with [**Root Motion**][5] can cause race condition
    - Both components try to move the transform each frame
    - Two possible solutions
- Information should always flow in one direction
    - Either agent moves the character and animations follows
    - Or animation moves the character based on simulated result
    - Otherwise you’ll end up having a hard to debug feedback loop
- _Animation follows agent_
    - Use the [NavMeshAgent.velocity](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-velocity.html) as input to the Animator to roughly match the agent’s movement to the animations
    - Robust and simple to implement, will result in foot sliding where animations cannot match the velocity
- _Agent follows animation_
    - Disable [NavMeshAgent.updatePosition](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-updatePosition.html) and [NavMeshAgent.updateRotation](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-updateRotation.html) to detach the simulation from the game objects locations
    - Use the difference between the simulated agent’s position ([NavMeshAgent.nextPosition](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-nextPosition.html)) and animation root ([Animator.rootPosition](https://docs.unity3d.com/ScriptReference/Animator-rootPosition.html)) to calculate controls for the animations
    - See [Coupling Animation and Navigation](CouplingAnimationAndNavigation.md) for more details

## NavMesh Agent and NavMesh Obstacle

- Do not mix well!
    - Enabling both will make the agent trying to avoid itself
    - If carving is enabled in addition, the agent tries to constantly remap to the edge of the carved hole, even more erroneous behavior ensues
- Make sure only one of them are active at any given time
    - Deceased state, you may turn off the agent and turn on the obstacle to force other agents to avoid it
    - Alternatively you can use priorities to make certain agents to be avoided more

## NavMesh Obstacle and Physics

- If you want physics controlled object to affect NavMesh Agent’s behavior
    - Add NavMesh Obstacle component to the object which agent should be aware of, this allows the avoidance system to reason about the obstacle
- If a game object has a Rigidbody and a NavMesh Obstacle attached, the obstacle’s velocity is obtained from the Rigidbody automatically
    - This allows NavMesh Agents to predict and avoid the moving obstacle

[1]: ./Glossary.md "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."
[2]: https://docs.unity3d.com/Manual/mesh-introduction.html "The main graphics primitive of Unity. Meshes make up a large part of your 3D worlds. Unity supports triangulated or Quadrangulated polygon meshes. Nurbs, Nurms, Subdiv surfaces must be converted to polygons."
[3]: https://docs.unity3d.com/Manual/CollidersOverview.html "An invisible shape that is used to handle physical collisions for an object. A collider doesn’t need to be exactly the same shape as the object’s mesh - a rough approximation is often more efficient and indistinguishable in gameplay."
[4]: https://docs.unity3d.com/Manual/class-Rigidbody.html "A component that allows a GameObject to be affected by simulated gravity and other forces."
[5]: https://docs.unity3d.com/Manual/RootMotion.html "Motion of character’s root node, whether it’s controlled by the animation itself or externally."
