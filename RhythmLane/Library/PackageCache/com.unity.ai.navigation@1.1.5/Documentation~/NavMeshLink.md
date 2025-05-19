# NavMeshLink

NavMesh Link creates a navigable link between two locations that use NavMeshes.

This link can be from point to point or it can span a gap, in which case the Agent uses the nearest location along the entry edge to cross the link.

You must use a NavMesh Link to connect different NavMesh Surfaces.

To use the NavMesh Link component, navigate to **GameObject > AI > NavMesh Link**.

![NavMeshLink example](Images/NavMeshLink-Example.png "A NavMesh Link component open in the Inspector window")

## Parameters
| **Property**                  | **Description**          |
|:------------------------------|:-------------------------| 
| **Agent Type**                | Specify the Agent type that can use the link.|
| **Start Point**               | Specify the start point of the link, relative to the GameObject. Defined by XYZ coordinates. |
| **End Point**                 | Specify the end point of the link, relative to the GameObject. Defined by XYZ coordinates. |
| **Align Transform To Points** | Clicking this button moves the GameObject at the link’s center point and aligns the transform’s forward axis with the end point. |
| **Cost Modifier**             | When the cost modifier value is non-negative the cost of moving over the NavMeshLink is equivalent to the cost modifier value times the Euclidean distance between NavMeshLink end points.
| **Bidirectional**             | With this checkbox selected, NavMesh Agents traverse the NavMesh Link both ways (from the start point to the end point, and from the end point back to the start point).<br/>When this checkbox is cleared, the NavMesh Link only functions one-way (from the start point to the end point only). |
| **Area Type**                 | The area type of the NavMesh Link (this affects pathfinding costs). <br/> - **Walkable** (this is the default option)<br/> - **Not Walkable** <br/> - **Jump** |


## Connecting multiple NavMesh Surfaces together

![Connecting surfaces example](Images/ConnectingSurfaces-Example.png "In this image, the blue and red NavMeshes are defined in two different NavMesh Surfaces and connected by a NavMesh Link")

If you want an Agent to move between multiple NavMesh Surfaces in a Scene, they must be connected using a NavMesh Link.

In the example Scene above, the blue and red NavMeshes are defined in different NavMesh Surfaces, with a NavMesh link connecting them.

Note that when connecting NavMesh Surfaces:

* You can connect NavMesh Surfaces using multiple NavMesh Links.

* Both the NavMesh Surfaces and the NavMesh Link must have same Agent type.

* The NavMesh Link’s start and end point must only be on one NavMesh Surface - be careful if you have multiple NavMeshes at the same location. 

* If you are loading a second NavMesh Surface and you have unconnected NavMesh Links in the first Scene, check that they do not connect to any unwanted NavMesh Surfaces.

