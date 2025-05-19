# Upgrade projects for use with AI Navigation package

Navigation and Pathfinding in Unity is handled by the AI Navigation package as of Unity 2022.2.

If you have projects that were created with the Navigation feature in previous versions of Unity, the AI Navigation package is automatically installed and added to your project. You can then do one of the following:

- Continue to use your projects as they are
- Convert your projects to use the new package

## Remove old component scripts

If your project uses the NavMesh Surface, NavMesh Modifier, NavMesh Modifier Volume or NavMesh Link components defined by scripts downloaded from [Unity’s NavMeshComponents GitHub repository](https://github.com/Unity-Technologies/NavMeshComponents), then remove those scripts and any associated files before you add the AI Navigation package to your project. If you don’t remove these scripts, you might get conflicts and errors related to these components in the Console. The new components mirror the same behavior as the old components do in your project except when using the following components:

- The NavMesh Surface component now includes an option to use only the objects that have a NavMesh Modifier in the baking process.
- You can now specify whether or not to apply the NavMesh Modifier component to child objects in the hierarchy.

## Convert your project

If you want to use the new package you need to convert your project(s). As part of the conversion process, the NavMesh Updater makes the following changes:

- Any NavMesh that was previously baked and embedded in the scene is now referenced from a NavMeshSurface component created on a new GameObject
 called Navigation.
- Any object that was marked with Navigation Static now has a NavMeshModifier component with the appropriate settings.

To convert your project do the following:

1. In the main menu go to **Window** > **AI** > **NavMesh Updater**.
1. In the NavMesh Updater window, select which kind of data to convert.
1. Click **Initialize Converters** to detect and display the types of data you selected.
1. Select the data you want to convert.
1. Click **Convert Assets** to complete the conversion. 

## Create new agent types 

If the NavMeshes in different scenes are baked with different agent settings then you need to create new agent types to match those settings. 

To create the agent types do the following:

1. In the main menu go to **Window** > **AI** > **Navigation**.
1. Select **Agents**.
1. Create new entries and specify the relevant settings.

### Assign new agent types
When you have created the new agent types you then need to assign them as follows: 

- Assign the newly created agent types to their respective NavMeshSurfaces in the Navigation created for that scene.
- Assign the agent types to the NavMeshAgents intended to use that NavMesh.

To find the settings that were used for each existing NavMesh, select the NavMesh `.asset` file in the **Project** window. The NavMesh settings will be displayed in the **Inspector**.
