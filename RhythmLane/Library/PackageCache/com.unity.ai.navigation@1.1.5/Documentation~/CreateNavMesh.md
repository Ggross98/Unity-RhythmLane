# Create a NavMesh
You need to create a NavMesh to define an area of your scene within which a character can navigate intelligently.

To create a NavMesh do the following:
1. Select the scene geometry where you want to add the NavMesh.
1. In the Inspector window, click **Add Component**.
1. Select **Navigation** > **NavMesh Surface**.
1. In the NavMesh Surface component, specify the necessary settings. For details on the available settings, refer to [NavMesh Surface component](./NavMeshSurface.md).
1. When you are finished, click **Bake**. <br/>
The NavMesh is generated and displayed in the scene as a blue overlay on the underlying scene geometry whenever the Navigation window is open and visible.

## Additional resources
- [Navigation window](./NavigationWindow.md)
- [Creating a NavMeshAgent](./CreateNavMeshAgent.md)
- [NavMesh Surface component](./NavMeshSurface.md)
- [Navigation Areas and Costs](./AreasAndCosts.md)