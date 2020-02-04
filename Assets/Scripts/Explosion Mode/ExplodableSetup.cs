using UnityEngine;
using UnityEditor;
using Tridify;

#if UNITY_EDITOR

public class ExplodableSetup : Editor
{
	[MenuItem("Tools/Make Explodable", false, 0)]
	public static void MakeExplodable()
	{
		foreach(var go in Selection.gameObjects)
		{
			go.AddComponent(typeof(ExplodableRoot));
			for (int i = 0; i < go.transform.childCount; i++)
			{
				go.transform.GetChild(i).gameObject.AddComponent(typeof(MeshCollider));
				go.transform.GetChild(i).gameObject.AddComponent(typeof(ExplodableComponent));
			}
		}
	}

	[MenuItem("Tools/Remove Explodable", false, 0)]
	public static void RemoveExplodable()
	{
		foreach (var go in Selection.gameObjects)
		{
			DestroyImmediate(go.GetComponent(typeof(ExplodableRoot)));
			for (int i = 0; i < go.transform.childCount; i++)
			{
				DestroyImmediate(go.transform.GetChild(i).gameObject.GetComponent(typeof(MeshCollider)));
				DestroyImmediate(go.transform.GetChild(i).gameObject.GetComponent(typeof(ExplodableComponent)));
			}
		}
	}

	[MenuItem("Tools/Make Collider", false, 0)]
	public static void MakeCollider()
	{
		foreach(var go in Selection.gameObjects)
		{
			var coll = go.AddComponent(typeof(MeshCollider)) as MeshCollider;
			coll.convex = true;
		}
	}

	[MenuItem("Tools/Remove Collider", false, 0)]
	public static void RemoveCollider()
	{
		foreach(var go in Selection.gameObjects)
		{
			foreach(var coll in go.GetComponents(typeof(MeshCollider)))
			{
				DestroyImmediate(coll);
			}
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
		}

		var coll = newObj.AddComponent<BoxCollider>();
		coll.size = combindeBound.size;
	}

	[MenuItem("Tools/Make Storey", false, 0)]
	public static void Storey()
	{
		var storey = Selection.activeGameObject.AddComponent<IfcBuildingStorey>();
		storey.Attributes = new IfcAttribute[1] { new IfcAttribute("Name", "4. OG") };
	}
}

#endif