# AI Navigation overlay reference 
  
The AI Navigation overlay allows you to control the display of NavMesh surfaces, agents, and GameObjects in the Scene view. You can use it to help you debug any issues with AI Navigation and pathfinding.
  
The Navigation overlay docks to the lower right corner of the Scene view by default.
  
## Surfaces
This section controls the way [NavMesh Surface](NavMeshSurface) instances are displayed. The following table describes the controls available in the Surfaces section of the overlay.
  
| **Control**             | **Description**           |
| :---------------------- | :------------------------ |
| **Show Only Selected**  | Display only the surfaces part of the current scene selection hierarchy.<br/> You can set the opacity of the selected and non-selected surfaces in the Preferences window. For more details, refer to [AI Navigation preferences](NavEditorPreferences.md). |
| **Show NavMesh**        | Display navigation meshes for the relevant surfaces. <br/>The colors used to display this mesh are the ones defined for the area types. |
| **Show HeightMesh**     | Display HeightMeshes (surface precise elevation information) for the relevant surfaces. |

## Agents
This section controls the displayed information for the currently selected [NavMesh Agents](NavMeshAgent). The following table describes the controls available in the Agents section of the overlay.      
  
| **Control**               | **Description**           |
| :------------------------ | :------------------------ |
| **Show Path Polygons**    | Display the NavMesh polygons part of the agent's path in a darker color. |
| **Show Path Query Nodes** | Display the path nodes explored during the pathfinding query in yellow. |
| **Show Neighbors**        | Display the collision avoidance neighbors (dynamic obstacles) relative to the agent.  |
| **Show Walls**            | Display the collision avoidance walls (static obstacles) for an agent.   |
| **Show Avoidance**        | Show the different positions sampled during the collision avoidance process.  |  

## Obstacles
This section controls the displayed information for the currently selected [NavMesh Obstacles](NavMeshObstacle.md). The following table describes the controls available in the Obstacles section of the overlay.
 
| **Control**          | **Description**           |
| :------------------- | :-------------------------| 
| **Show Carve Hull**  | Display the convex shape that is used to carve the NavMesh. |

 
## Additional resources
- [Overlays](xref:overlays) - How to use and work with overlays.
