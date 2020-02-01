using UnityEngine;
using UnityEditor;
using Tridify;

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

	[MenuItem("Tools/Combine Object", false, 0)]
	public static void MakeObject()
	{
		var newObj = Instantiate(new GameObject(Selection.activeTransform.name + "_root"),
			Tools.handlePosition, Quaternion.identity, Selection.activeTransform.parent);

		newObj.layer = LayerMask.NameToLayer("Tridify");

		Bounds combindeBound = Selection.transforms[0].GetComponent<MeshRenderer>().bounds;

		foreach (var trans in Selection.transforms)
		{
			combindeBound.Encapsulate(trans.gameObject.GetComponent<MeshRenderer>().bounds);
			trans.parent = newObj.transform;
			PrefabUtility.RecordPrefabInstancePropertyModifications(trans);
		}

		var coll = newObj.AddComponent<BoxCollider>();
		coll.size = combindeBound.size;
	}
}

#endif