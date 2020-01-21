﻿using Tridify;
using UnityEngine;
using System.Linq;

public class TridifyQuery
{

	private static string[] _storeyNames = { "EG", "1. OG", "1. OG", "2. OG", "3. OG" };

	private static string[] _materialFilter = { "Name" };
	private static string[] _archiCADFilter = { "Nominale B x H x T", "Innenseite Oberfläche", "Außenseite Oberfläche" };

	public static string GetDescription(GameObject target)
	{
		string returnString = "";
		// Sanity Check
		if (target == null)
			return returnString;
		// Material
		if (target.TryGetComponent(out IfcMaterial mat))
		{
			// Filtern nach den Attributen die erlaubt sind
			var attributes = mat.Attributes.Join(_materialFilter, attr => attr.Name, fltr => fltr, (attr, fltr) => "Material: " + attr.Value);
			returnString += string.Join(System.Environment.NewLine, attributes) + System.Environment.NewLine;
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
					returnString += string.Join(System.Environment.NewLine, filtered) + System.Environment.NewLine;
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
					returnString += string.Join(System.Environment.NewLine, filtered) + System.Environment.NewLine;
				}
			}
		}
		// Kombinierten Info-String zurueck
		return returnString;
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