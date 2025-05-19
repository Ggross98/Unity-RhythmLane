# NavMesh Modifier component reference

Use the NavMesh Modifier component to adjust the behavior of a GameObject when the NavMesh is baked at runtime. The NavMesh Modifier component affects the NavMesh during the generation process only. This means the NavMesh is updated to reflect any changes to NavMesh Modifier components when you bake the NavMesh. Use the available properties to specify changes in behavior and any limits to those changes.

To use the NavMesh Modifier component, add it to a GameObject as follows: 
1. Select the GameObject whose effect on the NavMesh you want to modify.
1. In the Inspector, select **Add Component**, then select **Navigation** &gt; **NavMesh Modifier**. <br/> The NavMesh Modifier component is displayed in the Inspector window.

The NavMesh Modifier can also affect the NavMesh generation process hierarchically. This means that the GameObject the component is attached to, as well as all its children, are affected. In addition, you can place another NavMesh Modifier further down the hierarchy to override the NavMesh Modifier that is further up the hierarchy.

To apply the NavMesh Modifier hierarchically, select the **Apply To Children** property.


**Note**: The NavMesh Modifier component replaces the legacy Navigation Static setting which you could enable from the Objects tab of the Navigation window and the Static flags dropdown on the GameObject. The NavMesh Modifier component is available for baking at runtime, whereas the Navigation Static flags were available in the Editor only. 

The following table describes the properties available in the NavMesh Modifier component.

<table>
  <thead>
    <tr>
      <th colspan="1"><strong>Property</strong></th>
      <th colspan="2"><strong>Description</strong></th>
    </tr>
  </thead>
  <tbody>
    <tr>
      <td rowspan="3"><strong>Mode</strong></td>
      <td colspan="2">Specify whether to consider or ignore the affected GameObject(s).</td>
    </tr>
    <tr>
      <td><strong>Add or Modify Object</strong></td>
      <td>Consider the affected GameObject(s) when building the NavMesh.</td>
    </tr>
    <tr>
      <td><strong>Remove Object</strong></td>
      <td>Ignore the affected object(s) when building the NavMesh for the specified agent type.</td>
    </tr>
    <tr>
      <td rowspan="3"><strong>Affected Agents</strong></td>
      <td colspan="2">Specify which agents the NavMesh Modifier affects. For example, you can choose to have certain obstacles be ignored by specific agents. </td>
    </tr>
    <tr>
      <td><strong>All</strong></td>
      <td>Modify the behavior of all agents. </td>
    </tr>
    <tr>
      <td><strong>None</strong></td>
      <td>Exclude all agents from the modified behavior.</td>
    </tr>
    <tr>
      <td rowspan="1"><strong>Apply to Children</strong></td>
      <td colspan="2">Apply the configuration to the child hierarchy of the GameObject.<br/>To override this component's influence further down the hierarchy, add another NavMesh Modifier component.</td>
    </tr>
    <tr>
      <td rowspan="2"><strong>Override Area</strong></td>
      <td colspan="2">Change the area type for the affected GameObject(s).<br/> If you want to change the area type, select the checkbox then select the new area type in the Area Type dropdown. <br/> If you do not want to change the area type, clear the checkbox.</td>
    </tr>
    <tr>
      <td><strong>Area Type</strong></td>
      <td>Select the new area type you want to apply from the dropdown.</td>
    </tr>
    <tr>
      <td rowspan="2"><strong>Override Generate Links</strong></td>
      <td colspan="2">Force the NavMesh bake process to either include or ignore the affected GameObject(s) when you generate links. </td>
    </tr>
    <tr>
      <td><strong>Generate Links</strong></td>
      <td>Specify whether or not to include the affected GameObject(s) when you generate links.<br/> To include the GameObject(s) when you generate links in the NavMesh bake process, select this checkbox. <br/> To ignore the GameObject(s) when you generate links in the NavMesh bake process, clear this checkbox.</td>
    </tr>
  </tbody>
</table>

## Additional resources
* [Create a NavMesh](CreateNavMesh.md)