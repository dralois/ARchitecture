using Tridify;
using UnityEngine;
using System.Linq;
using System;
using System.Collections.Generic;

public class TridifyQuery
{

	private static string[] _storeyNames = { "EG", "1. OG" };

	private static string[] _materialFilter = { "Name" };
	private static string[] _archiCADFilter = { "Nominale B x H x T", "Innenseite Oberfläche", "Außenseite Oberfläche" };

	public static string[] GetStoreyNames()
	{
		string[] storeyNames = _storeyNames;
		return storeyNames;
	}

	public static List<string> GetDescription(GameObject target)
	{
		string returnString = "";
        List<string> returnStrings = new List<string>();
        returnStrings.Clear();

		// Sanity Check
		if (target == null)
			return returnStrings;
		// Material
		if (target.TryGetComponent(out IfcMaterial mat))
		{
			// Filtern nach den Attributen die erlaubt sind
			var attributes = mat.Attributes.Join(_materialFilter, attr => attr.Name, fltr => fltr, (attr, fltr) => "Material: " + attr.Value);
			returnStrings.Add(string.Join("", attributes));
		}
		// Falls Explodable (Wand, Dach..)
		if (target.TryGetComponent(out ExplodableComponent explodable))
		{
			// Explodable Root ist Informationsgeber
			GameObject explodableObj = explodable.Root.gameObject;
			foreach (var prop in explodableObj.GetComponents<IfcPropertySet>())
			{
				// Falls ArchiCAD Property
				if (prop.Attributes.Any(attr => attr.Value == "ArchiCADProperties"))
				{
					// Filtern nach den Attributen die erlaubt sind
					var filtered = prop.Attributes.Join(_archiCADFilter, attr => attr.Name, fltr => fltr, (attr, fltr) => attr.Name + ": " + attr.Value);
                    Debug.Log(string.Join(System.Environment.NewLine, filtered));
                    returnStrings.Add(string.Join("", filtered));
				}
			}
		}
		else
		{
			foreach (var prop in target.GetComponents<IfcPropertySet>())
			{
				// Falls ArchiCAD Property
				if (prop.Attributes.Any(attr => attr.Value == "ArchiCADProperties"))
				{
                    // Filtern nach den Attributen die erlaubt sind
                    var filtered = prop.Attributes.Join(_archiCADFilter, attr => attr.Name, fltr => fltr, (attr, fltr) => attr.Name + ": " + attr.Value);
                    returnStrings.Add(string.Join("", filtered));
				}
			}
		}
		// Kombinierten Info-String zurueck
		return returnStrings;
	}

	public static string GetTitle(GameObject target)
	{
		// Sanity Check
		if (target == null)
			return "";
		// Falls Explodable (Wand, Dach..)
		if (target.TryGetComponent(out ExplodableComponent explodable))
		{
			// Explodable Root ist Namensgeber
			GameObject explodableObj = explodable.Root.gameObject;
			foreach (var prop in explodableObj.GetComponents<IfcPropertySet>())
			{
				// Falls ArchiCAD Property
				if (prop.Attributes.Any(attr => attr.Value == "ArchiCADProperties"))
				{
					// Element ID ist Name
					return prop.Attributes.First(attr => attr.Name == "Element ID").Value;
				}
			}
		}
		else
		{
			foreach (var prop in target.GetComponents<IfcPropertySet>())
			{
				// Falls ArchiCAD Property
				if (prop.Attributes.Any(attr => attr.Value == "ArchiCADProperties"))
				{
					// Element ID ist Name
					return prop.Attributes.First(attr => attr.Name == "Element ID").Value;
				}
			}
		}
		// Kein Name
		return "Error";
	}

	public static void changeStoreyState(bool active, string storeyName)
	{
		GameObject storeyObject = null;
        uint storeyIndex = (uint)Array.IndexOf(_storeyNames, storeyName);

        if (active)
		{
			storeyObject = FindInActiveObjectByName(storeyName);
            Transform storeyFinal = storeyObject.transform;
            SetStoreyActive(storeyFinal, storeyIndex, active);
        }
		else
		{
			storeyObject = GameObject.Find(storeyName);
            Transform storeyFinal = storeyObject.transform;
            SetStoreyActive(storeyFinal, storeyIndex, active);
        }
	}

	private static GameObject FindInActiveObjectByName(string name)
	{
		Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
		for (int i = 0; i < objs.Length; i++)
		{
			if (objs[i].hideFlags == HideFlags.None)
			{
				if (objs[i].name == name)
				{
					return objs[i].gameObject;
				}
			}
		}
		return null;
	}

	public static void SetStoreyActive(Transform target, uint storey, bool active)
	{
		// Sanity Check
		if (storey >= _storeyNames.Length || target == null)
			return;
		// Ebenen finden
		var storeys = target.GetComponentsInChildren<IfcBuildingStorey>(true);
		// Filtern
		var result = storeys.First(curr => curr.Attributes.Any(attr => attr.Value == _storeyNames[storey]));
		// GO (de)aktivieren
		result.gameObject.SetActive(active);
	}

}
