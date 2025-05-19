# About NavMesh agents

The NavMesh agent is a [GameObject][1] that is represented by an upright cylinder whose size is specified by the Radius and Height properties. The cylinder moves with the GameObject, but remains upright even if the GameObject rotates. The shape of the cylinder is used to detect and respond to collisions with other agents and obstacles. When the anchor point of the GameObject is not at the base of the cylinder, use the Base Offset property to specify the height difference.

![How the anchor point and base offset work together](./Images/NavMeshAgentOffset.svg)

The height and radius of the cylinder are specified in the [Navigation window](NavigationWindow) and the [NavMesh Agent component](NavMeshAgent) properties of the individual agents.

- **Navigation window settings** describe how all the NavMesh Agents collide and avoid static world geometry. To keep memory on budget and CPU load at a reasonable level, you can only specify one size in the bake settings.
- **NavMesh Agent component properties** values describe how the agent collides with moving obstacles and other agents.

Typically you set the size of the agent with the same values in both places. However, you might, give a heavy soldier a larger radius, so that other agents leave more space around your soldier. Otherwise, your soldier avoids the environment in the same manner as the other agents.

## Additional resources

- [Create a NavMesh Agent](./CreateNavMeshAgent.md) 
- [NavMesh Agent component reference](NavMeshAgent.md)
- [NavMesh Agent scripting reference](ScriptRef:AI.NavMeshAgent) 

[1]: https://docs.unity3d.com/Manual/GameObjects.html "The fundamental object in Unity scenes, which can represent characters, props, scenery, cameras, waypoints, and more."
