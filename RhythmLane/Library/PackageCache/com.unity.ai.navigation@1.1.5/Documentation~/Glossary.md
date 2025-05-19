
# Glossary

## Animation blend tree
Used for continuous blending between similar Animation Clips based on float Animation Parameters.

## Animation clip
Animation data that can be used for animated characters or simple animations. An animation clip is one piece of motion, such as (one specific instance of) “Idle”, “Walk” or “Run”.

## Animation parameters
Used to communicate between scripting and the Animator Controller. Some parameters can be set in scripting and used by the controller, while other parameters are based on Custom Curves in Animation Clips and can be sampled using the scripting API.

## Animator window
The window where the Animator Controller is visualized and edited.

## Colliders
An invisible shape that is used to handle physical collisions for an object. A collider doesn’t need to be exactly the same shape as the object’s mesh - a rough approximation is often more efficient and indistinguishable in gameplay.

## Collision
A collision occurs when the physics engine detects that the colliders of two GameObjects make contact or overlap, and at least one has a Rigidbody component and is in motion.

## GameObject
The fundamental object in Unity scenes, which can represent characters, props, scenery, cameras, waypoints, and more.

## Inspector
A Unity window that displays information about the currently selected GameObject, asset or project settings, allowing you to inspect and edit the values.

## Mesh
The main graphics primitive of Unity. Meshes make up a large part of your 3D worlds. Unity supports triangulated or Quadrangulated polygon meshes. Nurbs, Nurms, Subdiv surfaces must be converted to polygons.

## NavMesh 
A mesh that Unity generates to approximate the walkable areas and obstacles in your environment for path finding and AI-controlled navigation. 

## Prefab
An asset type that allows you to store a GameObject complete with components and properties. The prefab acts as a template from which you can create new object instances in the scene.

## Rigidbody
A component that allows a GameObject to be affected by simulated gravity and other forces.

## Root motion
Motion of character’s root node, whether it’s controlled by the animation itself or externally.

## Scenes
 A Scene contains the environments and menus of your game. Think of each unique Scene file as a unique level. In each Scene, you place your environments, obstacles, and decorations, essentially designing and building your game in pieces.

## Scripts
A piece of code that allows you to create your own Components, trigger game events, modify Component properties over time and respond to user input in any way you like.

## Terrains
The landscape in your scene. A Terrain GameObject adds a large flat plane to your scene and you can use the Terrain’s Inspector window to create a detailed landscape.

## Unity unit 
The unit size used in Unity projects. By default, 1 Unity unit is 1 meter. To use a different scale, set the Scale Factor in the Import Settings when importing assets.

