using UnityEngine;
using UnityEngine.UIElements;
using Unity.UIElements.Runtime;
using System.Collections.Generic;

[RequireComponent(typeof(PanelRenderer))]
public class UIHandler : MonoBehaviour
{

	private static PanelRenderer _UIRenderer = null;
	private string[] _storeyNames = TridifyQuery.GetStoreyNames();

	private string[] _currentDesc = new string[0];

	private void X_MenuChanged(GameManager.MenuMode newMode)
	{
		// Root cachen
		var root = _UIRenderer.visualTree;
		// Je nach Modus Menues aktivieren
		switch (newMode)
		{
			case GameManager.MenuMode.Spawn:
				{
					// Hier nichts tun da noch nicht initialisiert!
					break;
				}
			case GameManager.MenuMode.Placement:
				{
					// Von Info zu Placement Panel wechseln
					root.Q("info-panel").style.display = DisplayStyle.None;
					root.Q("placement-panel").style.display = DisplayStyle.Flex;
					root.Q("desc-area").style.display = DisplayStyle.None;
					root.Q("hideSections-area").style.display = DisplayStyle.None;
					break;
				}
			case GameManager.MenuMode.Interaction:
				{
					// Von Placement zu Interaction Panel wechseln
					root.Q("placement-panel").style.display = DisplayStyle.None;
					root.Q("options-panel").style.display = DisplayStyle.None;
					root.Q("interaction-panel").style.display = DisplayStyle.Flex;
					root.Q("desc-area").style.display = DisplayStyle.None;
					root.Q("hideSections-area").style.display = DisplayStyle.None;
					break;
				}
		}
	}

	private IEnumerable<Object> X_BindPanel()
	{
		// Root cachen
		var root = _UIRenderer.visualTree;

		// Placement Accept Button binden
		root.Q<Button>("placement-accept").clickable.clicked += () =>
		{
			// Modus wechseln (Placement -> Interaction)
			GameManager.Instance.SwitchMenu(GameManager.MenuMode.Interaction);
		};

		// Placement Edit Button binden
		root.Q<Button>("placement-edit").clickable.clicked += () =>
		{
			// Von Placement Uebersicht zu Placement Edit wechseln
			root.Q("placement-panel").style.display = DisplayStyle.None;
			root.Q("placement-edit-panel").style.display = DisplayStyle.Flex;
			// Callback speichern
			root.Q<Slider>("scale-edit-slider").RegisterCallback<ChangeEvent<int>>(evt =>
			{
				GameManager.Instance.ChangeScaling(evt.newValue);
			});
		};

		// Scaling Button binden
		root.Q<Button>("scaling-accept").clickable.clicked += () =>
		{
			// Von Scaling Options zu Placement Uebersicht wechseln
			root.Q("placement-panel").style.display = DisplayStyle.Flex;
			root.Q("placement-edit-panel").style.display = DisplayStyle.None;
		};

		// Optionen Button binden
		root.Q<Button>("interaction-options").clickable.clicked += () =>
		{
			// Von Interaction Uebersicht zu Interaction Options wechseln
			root.Q("interaction-panel").style.display = DisplayStyle.None;
			root.Q("options-panel").style.display = DisplayStyle.Flex;
		};

		// Optionen Exit Button binden
		root.Q<Button>("options-exit").clickable.clicked += () =>
		{
			// Von Interaction Options zu Interaction Uebersicht wechseln
			root.Q("desc-area").style.display = DisplayStyle.None;
			root.Q("hideSections-area").style.display = DisplayStyle.None;
			root.Q("options-panel").style.display = DisplayStyle.None;
			root.Q("interaction-panel").style.display = DisplayStyle.Flex;
		};

		// Ghost Mode Button binden
		root.Q<Button>("options-switch-ghosted").clickable.clicked += () =>
		{
			// Ghosted umschalten
			var newVis = GameManager.Instance.CameraController.GetVisualization() == CameraController.Visualization.Normal ?
				CameraController.Visualization.Ghosted : CameraController.Visualization.Normal;
			GameManager.Instance.CameraController.SetVisualization(newVis);
		};

		// Storey Button binden
		root.Q<Button>("options-sections-edit").clickable.clicked += () =>
		{
			var button = root.Q("hideSections-area");
			button.style.display = button.style.display == DisplayStyle.Flex ?
				DisplayStyle.None : DisplayStyle.Flex;
		};

		// Description Box erstellen
		var listView = root.Q<ListView>("desc-box");
		// Werte Anpassen
		listView.selectionType = SelectionType.None;
		listView.makeItem = X_MakeLine;
		listView.bindItem = X_BindLine;
		listView.itemsSource = _currentDesc;

		// Storey Toggles erstellen
		foreach (string storey in _storeyNames)
		{
			// Toggle erstellen
			Toggle storeyToggle = new Toggle(storey);
			// Stylesheets hinzufuegen
			foreach (var sheet in _UIRenderer.stylesheets)
			{
				storeyToggle.styleSheets.Add(sheet);
			}
			// Werte anpassen
			storeyToggle.name = "toggle-" + storey;
			storeyToggle.value = true;
			// Dem Container hinzufuegen
			root.Q("toggle-area").Add(storeyToggle);
			// Callback speichern
			storeyToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
				TridifyQuery.SetStoreyActive(GameManager.Instance.PlacedIFC.transform, storey, evt.newValue));
		}

		// Reset Button binden
		root.Q<Button>("reset-btn").clickable.clicked += () =>
		{
			foreach (string storey in _storeyNames)
			{
				// Storey aktivieren
				root.Q<Toggle>("toggle-" + storey).value = true;
				TridifyQuery.SetStoreyActive(GameManager.Instance.PlacedIFC.transform, storey, true);
			}
		};

		// Anzeige Initialisieren
		root.Q("info-panel").style.display = DisplayStyle.Flex;
		root.Q("placement-panel").style.display = DisplayStyle.None;
		root.Q("options-panel").style.display = DisplayStyle.None;
		root.Q("interaction-panel").style.display = DisplayStyle.None;
		root.Q("desc-area").style.display = DisplayStyle.None;
		root.Q("hideSections-area").style.display = DisplayStyle.None;
		root.Q("placement-edit-panel").style.display = DisplayStyle.None;

		// Kein Return notwendig
		return null;
	}

	private VisualElement X_MakeLine()
	{
		var element = new Label();
		element.AddToClassList("description-text");
		return element;
	}

	private void X_BindLine(VisualElement element, int index)
	{
		(element as Label).text = _currentDesc[index];
		element.userData = _currentDesc[index];
	}

	public void ShowDescription(string title, string[] desc)
	{
		// Array update
		_currentDesc = desc;
		// Cachen
		var root = _UIRenderer.visualTree;
		// Anpassen
		root.Q("desc-area").style.display = DisplayStyle.Flex;
		root.Q<Label>("desc-heading").text = title;
		root.Q<ListView>("desc-box").itemsSource = _currentDesc;
		root.Q<ListView>("desc-box").Refresh();
	}

	public void HideDescription()
	{
		_UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
	}

	private void Awake()
	{
		_UIRenderer = GetComponent<PanelRenderer>();
		_UIRenderer.postUxmlReload = X_BindPanel;
		GameManager.Instance.MenuChanged += X_MenuChanged;
		GameManager.Instance.UIController = this;
	}

	private void OnDestroy()
	{
		GameManager.Instance.MenuChanged -= X_MenuChanged;
	}

}
