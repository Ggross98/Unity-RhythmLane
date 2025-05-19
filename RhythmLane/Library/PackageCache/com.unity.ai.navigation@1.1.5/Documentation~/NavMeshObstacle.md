# NavMesh Obstacle component reference

The NavMesh Obstacle component allows you to define obstacles that [NavMesh Agents](AboutAgents) should avoid as they navigate the world (for example, barrels or crates controlled by the physics system). It contains properties that allow you to define the size, shape, and behavior of the obstacle.

To use the NavMesh component you need to add it to a game object as follows: 
1. Select the GameObject you want to use as an obstacle.
1. In the Inspector select **Add Component**, then select **Navigation** &gt; **NavMesh Obstacle**. <br/> The NavMesh Obstacle component is displayed in the Inspector window.

You can use this component to create NavMesh obstacles. For more information, see [Create a NavMesh Obstacle](./CreateNavMeshObstacle.md). For more information on NavMesh obstacles and how to use them, see [About NavMesh obstacles](AboutObstacles).

The following table describes the properties available in the NavMesh Obstacle component.

<table>
  <thead>
    <tr>
      <th colspan="1"><strong>Property</strong></th>
      <th colspan="2"><strong>Description</strong></th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td rowspan="3"><strong>Shape</strong></td>
      <td colspan="2">Specify the shape of the obstacle geometry. Choose whichever one best fits the shape of the object. </td>
    </tr>
    <tr>
      <td><strong>Box</strong></td>
      <td>Select a cube-shaped geometry for the obstacle.</td>
    </tr>
    <tr>
      <td><strong>Capsule</strong></td>
      <td>Select a 3D oval-shaped geometry for the obstacle.</td>
    </tr>
    <tr>
      <td><strong>Center</strong></td>
      <td colspan="2"> Specify the center of the box relative to the transform position.</td>
    </tr>
    <tr>
      <td><strong>Size</strong></td>
      <td colspan="2"> Specify the size of the box. <br/> This property is visible only when <strong>Shape</strong> is set to <strong>Box</strong>. </td>
    </tr>
    <tr>
      <td><strong> Center </strong></td>
      <td colspan="2"> Specify the center of the capsule relative to the transform position.</td>
    </tr>
    <tr>
      <td><strong> Radius </strong></td>
      <td colspan="2"> Specify the radius of the capsule.  <br/> This property is visible only when <strong>Shape</strong> is set to <strong>Capsule</strong>. </td>
    </tr>
    <tr>
      <td><strong> Height </strong></td>
      <td colspan="2"> Specify the height of the capsule.  <br/> This property is visible only when <strong>Shape</strong> is set to <strong>Capsule</strong>. </td>
    </tr>
    <tr>
      <td rowspan="4"><strong>Carve</strong></td>
      <td colspan="2">Allow the NavMesh Obstacle to create a hole in the NavMesh. <br/> When selected, the NavMesh obstacle carves a hole in the NavMesh. <br/> When deselected, the NavMesh obstacle does not carve a hole in the NavMesh. </td>
    </tr>
    <tr>
      <td><strong>Move Threshold</strong></td>
      <td> Set the threshold distance for updating a moving carved hole. Unity treats the NavMesh obstacle as moving when it has moved more than the distance set by the Move Threshold.  <br/> This property is available only when <strong>Carve</strong> is selected.</td>
    </tr>
    <tr>
      <td><strong>Time To Stationary</strong></td>
      <td> Specify the time (in seconds) to wait until the obstacle is treated as stationary. <br/> This property is available only when <strong>Carve</strong> is selected.</td>
    </tr>
    <tr>
      <td><strong>Carve Only Stationary</strong></td>
      <td> Specify when the obstacle is carved. <br/> This property is available only when <strong>Carve</strong> is selected.</td>
    </tr>    
  </tbody>
</table>

## Additional resources

- [About NavMesh obstacles](AboutObstacles) - Details on how to use NavMesh obstacles.
- [Create a NavMesh Obstacle](./CreateNavMeshObstacle.md) - Guidance on creating NavMesh obstacles.
    
- [Inner Workings of the Navigation System](./NavInnerWorkings.md) - Learn more about how NavMesh Obstacles are used as part of navigation.
    
- [NavMesh Obstacle scripting reference](https://docs.unity3d.com/ScriptReference/AI.NavMeshObstacle.html) - Full description of the NavMesh Obstacle scripting API.
    
[1]: ./BuildingNavMesh.md "A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation."
[2]: https://docs.unity3d.com/Manual/CollidersOverview.html "An invisible shape that is used to handle physical collisions for an object. A collider doesn’t need to be exactly the same shape as the object’s mesh - a rough approximation is often more efficient and indistinguishable in gameplay."
[3]: https://docs.unity3d.com/Manual/CollidersOverview.html "A collision occurs when the physics engine detects that the colliders of two GameObjects make contact or overlap, when at least one has a Rigidbody component and is in motion."
[4]: https://docs.unity3d.com/Manual/class-GameObject.html "The fundamental object in Unity scenes, which can represent characters, props, scenery, cameras, waypoints, and more. A GameObject’s functionality is defined by the Components attached to it."