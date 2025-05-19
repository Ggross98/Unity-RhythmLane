# Navigation window reference

Use the Navigation window to specify the types of NavMesh agents and areas used in your scenes.

To get to the Navigation window, in the main menu go to **Window** &gt; **AI** &gt; **Navigation**.

## Agents tab
The Agents tab contains properties that allow you to define the type of agents that you use in your scenes.
  
| **Property**        | **Description**           |
| :------------------ | :------------------------ |
| **Agent Types**     | Select an agent type to modify. <br/> Click the "+" icon to add an agent type. <br/> Click the "-" icon to remove the currently selected agent type. |
| **Name**            | Specify the name of the type of agent. |
| **Radius**          | Define how close the agent center can get to a wall or a ledge. |
| **Height**          | Specify the height of this type of agent in Unity units. |
| **Step Height**     | Specify the maximum step height that this type of agent can climb. |
| **Max Slope**       | Specify how steep of a ramp the agent can walk up. Type a value, in degrees, in the text box or drag the slider to adjust the value. |

### Generated Links
The following table describes the properties that define the limits of this agent type with respect to generated links.

| **Property**        | **Description**           |
| :------------------ | :------------------------ |
| **Drop Height**     | Specify the maximum height from which this agent type can jump down. |
| **Jump Distance**   | Specify the maximum distance of jump-across links for this agent type. |

## Areas tab
The Areas tab contains properties that allow you to specify how difficult it is to walk across the different area types used in your scenes. There are 29 custom area types, and 3 built-in area types: 

- **Walkable** is a generic area type which specifies that the area can be walked on.
- **Not Walkable** is a generic area type which prevents navigation. It is useful for cases where you want to mark a certain object to be an obstacle, but you don't want to put a NavMesh on top of it.
- **Jump** is an area type that is assigned to all auto-generated OffMesh links.

The following table describes the properties available on the Areas tab.
  
| **Property**    | **Description**           |
| :-------------- | :------------------------ |
| **Name**        | Specify a name for the area type. |
| **Cost**        | Specify the cost of traveling across this area. Costs are multipliers applied to the distance traveled across an area. A cost of 2 means an area is twice as difficult to cross as an area with a cost of 1. The default value is 1. |

## Additional resources
- [About NavMesh agents](AboutAgents.md)
- [Create a NavMesh agent](CreateNavMeshAgent.md)
- [Navigation Areas and costs](AreasAndCosts.md) 
