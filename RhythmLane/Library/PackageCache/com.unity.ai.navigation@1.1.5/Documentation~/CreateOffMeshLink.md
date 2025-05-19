# Creating an Off-mesh Link

Off-Mesh Links are used to create paths crossing outside the walkable navigation [**mesh**][1] surface. For example, jumping over a ditch or a fence, or opening a door before walking through it, can be all described as Off-mesh links.

We’re going to add an Off-Mesh Link component to describe a jump from the upper platform to the ground.

![](./Images/OffMeshLinkSetup.svg)

1. First create **two cylinders**: **Game Object > 3D Object > cylinder**.
2. You can scale the cylinders to _(0.1, 0.5, 0.1)_ to make it easier to work with them.
3. Move the **first cylinder** at the edge of the top platform, close to the [**NavMesh**][2] surface.
4. Place the **second cylinder** on the ground, close to the NavMesh, at the location where the link should land.
5. Select the **first cylinder** cylinder and add an Off-Mesh Link component to it. Choose **Add Component** from the inspector and choose **Navigation > OffMesh Link**.
6. Assign the **first cylinder** in the **Start** field and the **second cylinder** in the **End** field.

Now you have a functioning Off-Mesh Link set up! If the path via the off-mesh link is shorter than via walking along the Navmesh, the off-mesh link will be used.

You can use any game object in the [**Scene**][3] to hold the Off-Mesh link component, for example a fence [**prefab**][4] could contain the off-mesh link component. Similarly you can use any game object with a Transform as the start and end marker.

The NavMesh bake process can detect and create common jump-across and drop-down links automatically. Take a look at the [Building Off-Mesh Links Automatically](./BuildingOffMeshLinksAutomatically.md) for more details.

## Details

![](./Images/OffMeshLinkDebug.svg)

If the agent does not traverse an OffMesh link make sure that both end points are connected correctly. A properly connected end point should show a circle around the access point.

Another common cause is that the Navmesh Agent’s _Area Mask_ does not have the OffMesh Link’s area included.

### Additional resources

- [Navigation HowTos](./NavHowTos.md) - common use cases for NavMesh Agent, with source code.
- [OffMesh Link component reference](https://docs.unity3d.com/Manual/class-OffMeshLink.html) – full description of all the Off-Mesh Link properties.
- [Off-Mesh Link scripting reference](https://docs.unity3d.com/ScriptReference/AI.OffMeshLink.html) - full description of the Off-Mesh Link scripting API.

[1]: https://docs.unity3d.com/Manual/mesh-introduction.html "The main graphics primitive of Unity. Meshes make up a large part of your 3D worlds. Unity supports triangulated or Quadrangulated polygon meshes. Nurbs, Nurms, Subdiv surfaces must be converted to polygons."
[2]: ./Glossary.md "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."
[3]: https://docs.unity3d.com/Manual/CreatingScenes.html "A Scene contains the environments and menus of your game. Think of each unique Scene file as a unique level. In each Scene, you place your environments, obstacles, and decorations, essentially designing and building your game in pieces."
[4]: https://docs.unity3d.com/Manual/Prefabs.html "An asset type that allows you to store a GameObject complete with components and properties. The prefab acts as a template from which you can create new object instances in the scene."
