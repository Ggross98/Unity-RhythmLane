# NavMeshModifierVolume

NavMesh Modifier Volume marks a defined area as a certain type (for example, __Lava__ or __Door__). Whereas [NavMesh Modifier](NavMeshModifier.md) marks certain GameObjects with an area type. NavMesh Modifier Volume allows you to change an area type locally based on a specific volume.

To use the NavMesh Modifier Volume component, navigate to __GameObject__ > __AI__ > __NavMesh Modifier Volume__.

NavMesh Modifier Volume is useful for marking certain areas of walkable surfaces that might not be represented as separate geometry, for example danger areas. You can also use It to make certain areas non-walkable.

The NavMesh Modifier Volume also affects the NavMesh generation process, meaning the NavMesh has to be updated to reflect any changes to NavMesh Modifier Volumes.

![NavMeshModifierVolume example](Images/NavMeshModifierVolume-Example.png "A NavMesh Modifier Volume component open in the Inspector")

## Parameters
| __Property__| __Function__ |
|:---|:---| 
| __Size__| Dimensions of the NavMesh Modifier Volume, defined by XYZ measurements.  |
| __Center__| The center of the NavMesh Modifier Volume relative to the GameObject center, defined by XYZ measurements. |
| __Area Type__| Describes the area type to which the NavMesh Modifier Volume applies.<br/> - __Walkable__ (this is the default option)<br/> - __Not Walkable__<br/> - __Jump__ |
| __Affected Agents__| A selection of agent types that the NavMesh Modifier Volume affects. For example, you may choose to make the selected NavMesh Modifier Volume a danger zone for specific Agent types only. |

