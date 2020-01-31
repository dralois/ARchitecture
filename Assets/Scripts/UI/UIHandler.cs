using UnityEngine;
using UnityEngine.UIElements;
using Unity.UIElements.Runtime;
using System.Collections.Generic;

[RequireComponent(typeof(PanelRenderer))]
public class UIHandler : MonoBehaviour
{

	private static PanelRenderer _UIRenderer = null;
	private string[] _storeyNames = TridifyQuery.GetStoreyNames();

	private void X_MenuChanged(GameManager.MenuMode newMode)
	{
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

	private IEnumerable<Object> BindPanel()
	{
		var root = _UIRenderer.visualTree;

		root.Q<Button>("placement-accept").clickable.clicked += () =>
		{
			// Modus wechseln (Placement -> Interaction)
			GameManager.Instance.SwitchMenu(GameManager.MenuMode.Interaction);
		};

		root.Q<Button>("placement-edit").clickable.clicked += () =>
		{
			// Von Placement Uebersicht zu Placement Edit wechseln
			root.Q("placement-panel").style.display = DisplayStyle.None;
			root.Q("placement-edit-panel").style.display = DisplayStyle.Flex;

			root.Q<Slider>("scale-edit-slider").RegisterCallback<ChangeEvent<int>>(evt =>
			{
				PlaceOnPlane.ChangeScaling(evt.newValue);
			});
		};

		root.Q<Button>("scaling-accept").clickable.clicked += () =>
		{
			// Von Scaling Options zu Placement Uebersicht wechseln
			root.Q("placement-panel").style.display = DisplayStyle.Flex;
			root.Q("placement-edit-panel").style.display = DisplayStyle.None;
		};

		root.Q<Button>("interaction-options").clickable.clicked += () =>
		{
			// Von Interaction Uebersicht zu Interaction Options wechseln
			root.Q("interaction-panel").style.display = DisplayStyle.None;
			root.Q("options-panel").style.display = DisplayStyle.Flex;
		};

		root.Q<Button>("options-exit").clickable.clicked += () =>
		{
			// Von Interaction Options zu Interaction Uebersicht wechseln
			root.Q("desc-area").style.display = DisplayStyle.None;
			root.Q("hideSections-area").style.display = DisplayStyle.None;
			root.Q("options-panel").style.display = DisplayStyle.None;
			root.Q("interaction-panel").style.display = DisplayStyle.Flex;
		};

		root.Q<Button>("options-switch-ghosted").clickable.clicked += () =>
		{
			// Ghosted umschalten
			var newVis = GameManager.Instance.CameraController.GetVisualization() == CameraController.Visualization.Normal ?
				CameraController.Visualization.Ghosted : CameraController.Visualization.Normal;
			GameManager.Instance.CameraController.SetVisualization(newVis);
		};

		root.Q<Button>("options-sections-edit").clickable.clicked += () =>
		{
			root.Q("hideSections-area").style.display = DisplayStyle.Flex;
		};


		foreach (string storey in _storeyNames)
		{

			Toggle storeyToggle = new Toggle(storey);

			foreach (var sheet in _UIRenderer.stylesheets)
			{
				storeyToggle.styleSheets.Add(sheet);
			}

			storeyToggle.AddToClassList("toggle-sidebar");
			storeyToggle.name = "toggle-" + storey;
			storeyToggle.value = true;

			root.Q("toggle-area").Add(storeyToggle);

			storeyToggle.RegisterCallback<ChangeEvent<bool>>(evt =>
				TridifyQuery.SetStoreyActive(GameManager.Instance.PlacedIFC.transform, storey, evt.newValue));
		}

		root.Q<Button>("reset-btn").clickable.clicked += () =>
		{
			foreach (string storey in _storeyNames)
			{
				root.Q<Toggle>("toggle-" + storey).value = true;
				TridifyQuery.SetStoreyActive(GameManager.Instance.PlacedIFC.transform, storey, true);
			}
		};

		// Initialisieren
		root.Q("info-panel").style.display = DisplayStyle.Flex;
		root.Q("placement-panel").style.display = DisplayStyle.None;
		root.Q("options-panel").style.display = DisplayStyle.None;
		root.Q("interaction-panel").style.display = DisplayStyle.None;
		root.Q("desc-area").style.display = DisplayStyle.None;
		root.Q("hideSections-area").style.display = DisplayStyle.None;
		root.Q("placement-edit-panel").style.display = DisplayStyle.None;

		return null;
	}

	public void ShowDescription(string title, string desc)
	{
		_UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.Flex;
		_UIRenderer.visualTree.Q<Label>("desc-heading").text = title;
		_UIRenderer.visualTree.Q<Label>("desc-txt").text = desc;
	}

	public void HideDescription()
	{
		_UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
	}

	private void Awake()
	{
		_UIRenderer = GetComponent<PanelRenderer>();
		_UIRenderer.postUxmlReload = BindPanel;
		GameManager.Instance.MenuChanged += X_MenuChanged;
		GameManager.Instance.UIController = this;
	}

	private void OnDestroy()
	{
		GameManager.Instance.MenuChanged -= X_MenuChanged;
	}

}
