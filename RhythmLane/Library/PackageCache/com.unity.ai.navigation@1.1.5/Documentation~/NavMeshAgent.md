# NavMesh Agent component reference

The NavMesh Agent component allows you to create characters (agents) that avoid each other as they move through a scene. Agents use the [NavMesh][1] to navigate through the space of the game and avoid each other and other moving obstacles. You can use the scripting API of the NavMesh Agent to handle pathfinding and spatial reasoning.

To use the NavMesh Agent component, add it to a GameObject:
1. Select the GameObject that represents your agent.
1. In the Inspector, click **Add Component**.
1. Select **Navigation** &gt; **NavMesh Agent**. 
   <br/>The NavMesh Agent component is displayed in the Inspector window.

You can use this component to create NavMesh agents. For more details, see [Create a NavMesh Agent](./CreateNavMeshAgent.md). For more information about NavMesh agents, see [About NavMesh agents](AboutAgents).

The following tables describe the properties available in the NavMesh agent component.

| Property        | Description             |
|:----------------|:------------------------|
| **Agent type**  | Select the type of agent you want to create. This allows the agent to move along any NavMesh created for the selected agent type. |
| **Base offset** | Specify the offset of the collision cylinder in relation to the transform pivot point. |

## Steering 
| Property              | Description             |
|:----------------------|:------------------------|
| **Speed**             | Set the maximum speed (in Unity units per second) at which the agent can move along a path. | 
| **Angular Speed**     | Set the maximum rotation speed (in degrees per second) of the agent. |
| **Acceleration**      | Set the maximum acceleration (in Unity units per second squared). | 
| **Stopping Distance** | Specify how close the agent can get to its destination. The agent stops when it arrives this close to the destination location. |
| **Auto Braking**      | Specify if the agent slows down as it approaches its destination. When enabled, the agent slows down as it approaches the destination. Disable this if you want the agent to move smoothly between multiple points (for example, if the agent is a guard on patrol). |

## Obstacle Avoidance 
| Property     | Description             |
|:-------------|:------------------------|
| **Radius**   | Specify the distance from the agent's center that is used to calculate [collisions][2] between the agent and other GameObjects. |
| **Height**   | Specify the height clearance that the agent needs to pass below an obstacle that is overhead. For example, the minimum height of a doorway or tunnel.|
| **Quality**  | Select the obstacle avoidance quality. If you have a high number of agents, you can reduce the obstacle avoidance quality to reduce performance costs. If you set obstacle avoidance quality to none, then collisions resolve, but other agents and obstacles are not actively avoided. |
| **Priority** | Specify how agents behave as they avoid each other. Agents avoid other agents of higher priority and ignore other agents of lower priority. The value should be in the range 0–99 where lower numbers indicate higher priority. |

## Path Finding 
| Property                       | Description             |
|:-------------------------------|:------------------------|
| **Auto Traverse OffMesh Link** | Specify whether or not the agent automatically traverses OffMesh links. When enabled, the agent automatically traverses OffMesh links. Disable **Auto Traverse OffMesh Link** if you want to use animation or a specific way to traverse OffMesh links. |
| **Auto Repath**                | Specify what the agent does when it reaches the end of a partial path. When there is no path to the destination, Unity generates a partial path to the reachable location that is closest to the destination. If this property is enabled, when the agent reaches the end of a partial path it tries again to find a path to the destination. |
| **Area Mask**                  | Specify which [area types](./AreasAndCosts.md) the agent considers as it tries to find a path. You can select multiple options. When you prepare meshes for NavMesh baking, you can set each mesh's area type. For example, you can mark stairs with a special area type, and restrict some agent types from using the stairs. |

## Additional resources
- [Create a NavMesh Agent](./CreateNavMeshAgent.md) 
- [About NavMesh agents](AboutAgents.md)
- [Inner Workings of the Navigation System](./NavInnerWorkings.md) 
- [NavMesh Agent scripting reference](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent.html) 

[1]: ./BuildingNavMesh.md "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."
[2]: https://docs.unity3d.com/Manual/CollidersOverview.html "A collision occurs when the physics engine detects that the colliders of two game objects make contact or overlap, and at least one has a Rigidbody component and is in motion."
