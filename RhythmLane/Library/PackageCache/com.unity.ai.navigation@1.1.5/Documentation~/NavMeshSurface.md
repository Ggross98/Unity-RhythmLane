# NavMesh Surface

The NavMesh Surface component represents the walkable area for a specific [NavMesh Agent](https://docs.unity3d.com/Manual/class-NavMeshAgent.html) type, and defines a part of the Scene where a NavMesh should be built. 

To use the NavMesh Surface component, navigate to **GameObject > AI > NavMesh Surface**. This creates an empty GameObject with a NavMesh Surface component attached to it. A Scene can contain multiple NavMesh Surfaces.

You can add the NavMesh Surface component to any GameObject. This is useful for when you want to use the GameObject parenting [Hierarchy](https://docs.unity3d.com/Manual/Hierarchy.html) to define which GameObjects contribute to the NavMesh.

![NavMeshSurface example](Images/NavMeshSurface-Example.png "A NavMesh Surface component open in the Inspector window")

## Parameters
| **Property**        | **Description**            |
|:--------------------|:---------------------------|
| **Agent Type**      | The [NavMesh Agent](https://docs.unity3d.com/Manual/class-NavMeshAgent.html) type using the NavMesh Surface. Use for bake settings and matching the NavMesh Agent to proper surfaces during pathfinding. |
| **Default Area**    | Defines the area type generated when building the NavMesh.<br/> - **Walkable** (this is the default option)<br/> - **Not Walkable**<br/> - **Jump** <br/> Use the [NavMesh Modifier](NavMeshModifier.md) component to modify the area type in more detail. |
| **Generate Links**  | If this option is enabled, objects collected by the surface will be considered to generate links during the baking process.<br/>See the **Links Generation** section for more information. |
| **Use Geometry**    | Select which geometry to use for baking.<br/>- **Render Meshes** – Use geometry from Render Meshes and [Terrains](https://docs.unity3d.com/Manual/terrain-UsingTerrains.html).<br/>-  **Physics [Colliders](https://docs.unity3d.com/Manual/CollidersOverview.html)** – Use geometry from Colliders and Terrains. Agents can move closer to the edge of the physical bounds of the environment with this option than they can with the **Render Meshes** option.      |
| **NavMesh Data**    | (Read-only) Locate the asset file where the NavMesh is stored. |
| **Clear**           | |
| **Bake**            | Bake a NavMesh with the current settings. |

Use the main settings for the NavMesh Surface component to filter the input geometry on a broad scale. Fine tune how Unity treats input geometry on a per-GameObject basis, using the [NavMesh Modifier](NavMeshModifier.md) component. 

The baking process automatically excludes GameObjects that have a NavMesh Agent or NavMesh Obstacle. They are dynamic users of the NavMesh, and so do not contribute to NavMesh building.

## Object collection


| **Property**        | **Description**      |
|:--------------------|:---------------------|
| **Collect Objects** | Defines which GameObjects to use for baking.<br/>- **All** – Use all active GameObjects (this is the default option).<br/>- **Volume** – Use all active GameObjects overlapping the bounding volume. Geometry outside of the bounding volume but within the agent radius is taken into account for baking.<br/>- **Children** – Use all active GameObjects which are children of the NavMesh Surface component, in addition to the object the component is placed on. |
| **Include Layers**  | Define the layers on which GameObjects are included in the bake process. In addition to **Collect Objects**, this allows for further exclusion of specific GameObjects from the bake (for example, effects or animated characters).<br/> This is set to **Everything** by default, but you can toggle options on (denoted by a tick) or off individually. |


## Advanced Settings

The Advanced settings section allows you to customize the following additional parameters:

| **Property**            | **Description**      |
|:------------------------|:---------------------|
| **Override Voxel Size** | Controls how accurately Unity processes the input geometry for NavMesh baking (this is a tradeoff between speed and accuracy). Select the checkbox to enable. The default is unchecked (disabled).<br/> 3 voxels per Agent radius (6 per diameter) allows the capture of narrow passages, such as doors, while maintaining a quick baking time. For big open areas, using 1 or 2 voxels per radius speeds up baking. Tight indoor spots are better suited to smaller voxels, for example 4 to 6 voxels per radius. More than 8 voxels per radius does not usually provide much additional benefit. |
| **Override Tile Size** | In order to make the bake process parallel and memory efficient, the Scene is divided into tiles for baking. The white lines visible on the NavMesh are tile boundaries. <br/> The default tile size is 256 voxels, which provides a good tradeoff between memory usage and NavMesh fragmentation. <br/> To change this default tile size, check this checkbox and, in the **Tile Size** field,  enter the number of voxels you want the Tile Size to be. <br/> The smaller the tiles, the more fragmented the NavMesh is. This can sometimes cause non-optimal paths. NavMesh carving also operates on tiles. If you have a lot of obstacles, you can often speed up carving by making the tile size smaller (for example around 64 to 128 voxels). If you plan to bake the NavMesh at runtime, using a smaller tile size to keep the maximum memory usage low. |
| **Minimum Region Area**| Allows you to cull away the small regions disconnected from the larger NavMesh. The process that builds the NavMesh does not retain the stretches of the mesh that have a surface size smaller than the specified value. Please note that some areas may not get removed despite the Minimum Region Area parameter. The NavMesh is built in parallel as a grid of tiles. If an area straddles a tile boundary, the area is not removed. The reason for this is that the area pruning step takes place at a stage in the build process when the surrounding tiles are not accessible. |
| **Build Height Mesh** | Enables the creation of additional data used for determining more accurately the height at any position on the NavMesh. See the **Height Mesh** section for more information. This option is available starting with Unity 2022.2.0f1. |



