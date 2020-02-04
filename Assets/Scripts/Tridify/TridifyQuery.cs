using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Tridify;
using UnityEngine;

public class TridifyQuery
{

	private static string[] _storeyNames = { "Dach", "4. OG", "3. OG", "2. OG", "1. OG", "EG" };

	// IfcMaterial
	private static string[] _materialFilter = { "Name" };
	// IfcPropertySet Allgemeine Werte
	private static string[] _generalFilter = { "U-Wert" };
	// IfcPropertySet Component Properties
	private static string[] _compontentFilter = { "Enthaltenes CO2" };
	// IfcPropertySet ArchiCADProperties
	private static string[] _archiCADFilter = { "Nominale B x H x T", "Innenseite Oberfläche", "Außenseite Oberfläche", "Treppenneigung",
																							"Baustoffe (Alle)", "Oberfläche (Alle)", "Oberfläche unten", "Oberfläche oben"};
	// IfcPropertySet Material Properties
	private static Tuple<string, string>[] _matPropertiesFilter = { new Tuple<string, string>("ThermalConductivity", "Wärmeleitfähigkeit"),
																																	new Tuple<string, string>( "SpecificHeatCapacity", "Spezifische Wärmekapazität") };
	// IfcPropertySet Pset_StairCommon
	private static Tuple<string, string>[] _stairFilter = { new Tuple<string, string>("NumberOfTreads", "Stufenzahl"),
																													new Tuple<string, string>("RequiredHeadroom", "Kopffreiheit") };
	// IfcPropertySet Names
	private static string[] _ifcPropertyFilter = { "ArchiCADProperties", "Allgemeine Werte", "Component Properties", "Material Properties", "Pset_StairCommon" };

	public static string[] GetStoreyNames()
	{
		// Kopie des Arrays um Modifikation definitiv zu unterbinden
		string[] storeyCopy = new string[_storeyNames.Length];
		Array.Copy(_storeyNames, storeyCopy, _storeyNames.Length);
		return storeyCopy;
	}

	public static string[] GetDescription(GameObject target)
	{
		List<string> descList = new List<string>();
		// Sanity Check
		if (target == null)
			return descList.ToArray();
		// Material
		if (target.TryGetComponent(out IfcMaterial mat))
		{
			// Filtern nach den Attributen die erlaubt sind
			var attributes = mat.Attributes.Join(_materialFilter, attr => attr.Name, fltr => fltr, (attr, fltr) => "Material: " + attr.Value);
			descList.AddRange(attributes);
		}
		// Eigene Informationen holen
		foreach (var prop in target.GetComponents<IfcPropertySet>())
		{
			// Beschreibungen laden
			X_AddDescription(prop, ref descList);
		}
		// Falls Explodable (Wand, Dach..)
		if (target.TryGetComponent(out ExplodableComponent explodable))
		{
			// Explodable Root ist extra Informationsgeber
			GameObject explodableObj = explodable.Root.gameObject;
			foreach (var prop in explodableObj.GetComponents<IfcPropertySet>())
			{
				// Beschreibungen laden
				X_AddDescription(prop, ref descList);
			}
		}
		// Kombinierten Info-String zurueck
		return descList.ToArray();
	}

	private static void X_AddDescription(IfcPropertySet prop, ref List<string> descList)
	{
		// Falls Property interessant ist
		if(_ifcPropertyFilter.Contains(prop.Attributes.FirstOrDefault(attr => attr.Name == "Name").Value))
		{
			// Filtern nach den Attributen die erlaubt sind
			var filtered = prop.Attributes.Join(_generalFilter, attr => attr.Name, fltr => fltr, (attr, fltr) => attr.Name + ": " + attr.Value);
			// Tridify Bug
			foreach (var filter in _compontentFilter)
			{
				var attr = prop.Attributes.FirstOrDefault(curr => curr.Name == filter);
				if (attr.Name != null)
				{
					filtered = filtered.Append(attr.Name + ": " + attr.Value);
				}
			}
			// Tridify Bug
			foreach (var filter in _matPropertiesFilter)
			{
				var attr = prop.Attributes.FirstOrDefault(curr => curr.Name == filter.Item1);
				if (attr.Name != null)
				{
					filtered = filtered.Append(filter.Item2 + ": " + attr.Value);
				}
			}
			filtered = filtered.Concat(prop.Attributes.Join(_stairFilter, attr => attr.Name, fltr => fltr.Item1, (attr, fltr) => fltr.Item2 + ": " + attr.Value));
			filtered = filtered.Concat(prop.Attributes.Join(_archiCADFilter, attr => attr.Name, fltr => fltr, (attr, fltr) => attr.Name + ": " + attr.Value));
			// Hinzufuegen, fertig
			descList.AddRange(filtered);
		}
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
				if(X_GetTitle(prop, out string title))
				{
					return title;
				}
			}
		}
		else
		{
			foreach (var prop in target.GetComponents<IfcPropertySet>())
			{
				if (X_GetTitle(prop, out string title))
				{
					return title;
				}
			}
		}
		// Kein Name
		return "Error";
	}

	private static bool X_GetTitle(IfcPropertySet prop, out string title)
	{
		// Default-Wert
		title = "Error";
		// Sanity Check
		if (prop == null)
			return false;
		// Falls ArchiCAD Property
		if (prop.Attributes.Any(attr => attr.Value == "ArchiCADProperties"))
		{
			// Falls Bibliothekselement (z.b. Fenster)
			if (prop.Attributes.Any(attr => attr.Name == "Bibliothekselement-Name"))
			{
				// Dann ist das der Name
				title = Regex.Replace(prop.Attributes.First(attr => attr.Name == "Bibliothekselement-Name").Value, @"(1|2|3|4|5|6|7|8|9|0|-)", "");
				return true;
			}
			else
			{
				// Sonst ist Element ID Name
				title = Regex.Replace(prop.Attributes.First(attr => attr.Name == "Element ID").Value, @"(1|2|3|4|5|6|7|8|9|0|-)", "");
				return true;
			}
		}
		else
		{
			return false;
		}
	}

	public static void SetStoreyActive(Transform target, string storey, bool active)
	{
		// Sanity Check
		if (target == null || !_storeyNames.Contains(storey))
			return;
		// Ebenen finden
		var storeys = target.GetComponentsInChildren<IfcBuildingStorey>(true);
		// Filtern
		var result = storeys.First(curr => curr.Attributes.Any(attr => attr.Value == storey));
		// GO (de)aktivieren
		result.gameObject.SetActive(active);
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
