using UnityEngine;
using UnityEditor;

#if UNITY_EDITOR

public class ExplodableSetup : Editor
{
	[MenuItem("GameObject/Make Explodable", false, 0)]
	public static void MakeExplodable()
	{
		for (int i = 0; i < Selection.activeTransform.childCount; i++)
		{
			Selection.activeTransform.GetChild(i).gameObject.AddComponent(typeof(MeshCollider));
			Selection.activeTransform.GetChild(i).gameObject.AddComponent(typeof(ExplodableComponent));
		}
	}

	[MenuItem("GameObject/Remove Explodable", false, 0)]
	public static void RemoveExplodable()
	{
		for (int i = 0; i < Selection.activeTransform.childCount; i++)
		{
			DestroyImmediate(Selection.activeTransform.GetChild(i).gameObject.GetComponent(typeof(MeshCollider)));
			DestroyImmediate(Selection.activeTransform.GetChild(i).gameObject.GetComponent(typeof(ExplodableComponent)));
		}
	}
}

#endif