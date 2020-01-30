using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Unity.UIElements.Runtime;
using System.Collections.Generic;

[RequireComponent(typeof(PanelRenderer))]
public class UIHandlerNew : MonoBehaviour
{

	private static PanelRenderer _UIRenderer = null;
    private string[] storeyNames = TridifyQuery.GetStoreyNames();

    private void X_ModeChanged(GameManager.InputMode newMode)
	{
		switch (newMode)
		{
			case GameManager.InputMode.Spawn:
				{
					// Hier nichts tun da noch nicht initialisiert!
					break;
				}
			case GameManager.InputMode.Placement:
				{
					// Von Info zu Placement Panel wechseln
					_UIRenderer.visualTree.Q("info-panel").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.Flex;
                    _UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
                    _UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.None;
                    break;
				}
			case GameManager.InputMode.Interaction:
				{
					// Von Placement zu Interaction Panel wechseln
					_UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("interaction-panel").style.display = DisplayStyle.Flex;
                    _UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
                    _UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.None;
                    break;
				}
			case GameManager.InputMode.Decoration:
				{
					// Von Interaction zu Decoration Panel wechseln
					_UIRenderer.visualTree.Q("interaction-panel").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.None;
					//_UIRenderer.visualTree.Q("decoration-panel").style.display = DisplayStyle.Flex;
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
			GameManager.Instance.SwitchMode(GameManager.InputMode.Interaction);
		};

		root.Q<Button>("placement-edit").clickable.clicked += () =>
		{
			// Von Placement Uebersicht zu Placement Edit wechseln
			_UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.None;
			_UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.Flex;

            Debug.Log("slider value: " + root.Q<SliderInt>("scale-edit-slider").value.ToString());
            root.Q<SliderInt>("scale-edit-slider").RegisterCallback<ChangeEvent<int>>(evt => 
            {
                Debug.Log("slider " + evt.newValue);
                PlaceOnPlane.ChangeScaling(evt.newValue);
            });
        };

        root.Q<Button>("scaling-accept").clickable.clicked += () =>
        {
            // Von Scaling Options zu Placement Uebersicht wechseln
            _UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.Flex;
            _UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.None;
        };

            root.Q<Button>("interaction-options").clickable.clicked += () =>
		{
			// Von Interaction Uebersicht zu Interaction Options wechseln
			_UIRenderer.visualTree.Q("interaction-panel").style.display = DisplayStyle.None;
			_UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.Flex;
		};

		root.Q<Button>("options-exit").clickable.clicked += () =>
		{
            // Von Interaction Options zu Interaction Uebersicht wechseln
            _UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.None;
			_UIRenderer.visualTree.Q("interaction-panel").style.display = DisplayStyle.Flex;
		};

		root.Q<Button>("options-switch-ghosted").clickable.clicked += () =>
		{
			// Ghosted umschalten
			var newVis = GameManager.Instance.Visualizer.CurrentVisualization == VisualizationSwitcher.Visualization.Normal ?
				VisualizationSwitcher.Visualization.Ghosted : VisualizationSwitcher.Visualization.Normal;
			GameManager.Instance.Visualizer.ChangeVisualization(newVis);
		};

        root.Q<Button>("options-sections-edit").clickable.clicked += () =>
        {
            _UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.Flex;
            foreach(string storey in storeyNames)
            {
                Toggle storeyToggle = new Toggle(storey);
                storeyToggle.AddToClassList("toggle-section");
                StyleSheet styleToggle = Resources.Load<StyleSheet>("Assets/UI/toggle.uss");
                storeyToggle.styleSheets.Add(styleToggle);
                storeyToggle.name = "toggle-"+storey;
                storeyToggle.value = true;
                root.Q("toggle-area").Add(storeyToggle);
            }

            foreach (string storey in storeyNames)
                root.Q("toggle-area").Q<Toggle>("toggle-"+storey).RegisterCallback<ChangeEvent<bool>>(e => TridifyQuery.changeStoreyState(e.newValue, storey));

        };

        root.Q<Button>("reset-btn").clickable.clicked += () =>
        {
            foreach (string storey in storeyNames)
            {
                root.Q<Toggle>("toggle-" + storey).value = true;
                TridifyQuery.changeStoreyState(true, storey);
            }
        };


        // Initialisieren
        _UIRenderer.visualTree.Q("info-panel").style.display = DisplayStyle.Flex;
		_UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.None;
		_UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.None;
		_UIRenderer.visualTree.Q("interaction-panel").style.display = DisplayStyle.None;
        //_UIRenderer.visualTree.Q("decoration-panel").style.display = DisplayStyle.None;
        _UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
        _UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.None;
        _UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.None;

        return null;
	}

    private void Awake()
	{
		_UIRenderer = GetComponent<PanelRenderer>();
		_UIRenderer.postUxmlReload = BindPanel;
		GameManager.Instance.ModeChanged += X_ModeChanged;
	}

    private void CheckActiveStoreys()
    {
        Debug.Log("enter the method");
        /*foreach (string storey in storeyNames)
        {
            UIRenderer.visualTree.Q<Toggle>;
        }*/
        // alle toggle durchgehen, active toggle in liste, dann an TridifyQuery übergeben...
    }

    public static void ShowDescription(string title, string desc)
    {
        _UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.Flex;
        _UIRenderer.visualTree.Q<Label>("desc-heading").text = title;
        _UIRenderer.visualTree.Q<Label>("desc-txt").text = desc;
    }

    public void HideDescription()
    {
        _UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
    }

    private void OnDestroy()
	{
		GameManager.Instance.ModeChanged -= X_ModeChanged;
	}

}
