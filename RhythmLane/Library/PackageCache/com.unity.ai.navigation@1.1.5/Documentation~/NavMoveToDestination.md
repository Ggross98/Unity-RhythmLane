# Telling a NavMeshAgent to Move to a Destination

You can tell an agent to start calculating a path simply by setting the [NavMeshAgent.destination](https://docs.unity3d.com/ScriptReference/AI.NavMeshAgent-destination.html) property with the point you want the agent to move to. As soon as the calculation is finished, the agent will automatically move along the path until it reaches its destination. The following code implements a simple class that uses a [**GameObject**][1] to mark the target point which gets assigned to the _destination_ property in the _Start_ function. Note that the script assumes you have already added and configured the NavMeshAgent component from the editor.

```
    // MoveDestination.cs
    using UnityEngine;
    
    public class MoveDestination : MonoBehaviour {
       
       public Transform goal;
       
       void Start () {
          NavMeshAgent agent = GetComponent<NavMeshAgent>();
          agent.destination = goal.position; 
       }
    }
```

[1]: https://docs.unity3d.com/Manual/class-GameObject.html "The fundamental object in Unity scenes, which can represent characters, props, scenery, cameras, waypoints, and more. A GameObject’s functionality is defined by the Components attached to it."