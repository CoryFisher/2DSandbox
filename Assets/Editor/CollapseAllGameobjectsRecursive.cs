using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;

public class HierarchyCollapser
{
	[MenuItem("Window/Collapse Hierarchy %&LEFT")]
	public static void CollapseHierarchy()
	{
		EditorApplication.ExecuteMenuItem("Window/Hierarchy");
		var hierarchyWindow = EditorWindow.focusedWindow;
		var expandMethodInfo = hierarchyWindow.GetType().GetMethod("SetExpandedRecursive");
		foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
		{
			expandMethodInfo.Invoke(hierarchyWindow, new object[] { root.GetInstanceID(), false });
		}
	}
}