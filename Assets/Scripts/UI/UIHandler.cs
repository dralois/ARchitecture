using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using Unity.UIElements.Runtime;
using System.Collections.Generic;

[RequireComponent(typeof(PanelRenderer))]
public class UIHandler : MonoBehaviour
{

	private static PanelRenderer _UIRenderer = null;
	private string[] storeyNames = TridifyQuery.GetStoreyNames();
    private SunCalculator _sunCalculator = null;

	private void X_MenuChanged(GameManager.MenuMode newMode)
	{
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
					_UIRenderer.visualTree.Q("info-panel").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.Flex;
					_UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.None;
					break;
				}
			case GameManager.MenuMode.Interaction:
				{
					// Von Placement zu Interaction Panel wechseln
					_UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("interaction-panel").style.display = DisplayStyle.Flex;
					_UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.None;
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
            _UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.Flex;
        };

        root.Q<Button>("placement-edit-accept").clickable.clicked += () =>
        {
            _UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.Flex;
        };

        root.Q<Button>("scale-obj").clickable.clicked += () =>
        {
            _UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("placement-edit-scale-panel").style.display = DisplayStyle.Flex;

			root.Q<SliderInt>("scale-edit-slider").RegisterCallback<ChangeEvent<int>>(evt =>
			{
				Debug.Log("slider " + evt.newValue);
				PlacementEdit.ChangeScaling(evt.newValue);
			});
		};

		root.Q<Button>("scaling-accept").clickable.clicked += () =>
		{
			// Von Scaling Options zu Placement Uebersicht wechseln
			_UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.Flex;
			_UIRenderer.visualTree.Q("placement-edit-scale-panel").style.display = DisplayStyle.None;
		};

        root.Q<Button>("rotate-obj").clickable.clicked += () =>
        {
            _UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("placement-edit-rotate-panel").style.display = DisplayStyle.Flex;

            root.Q<SliderInt>("rotate-edit-slider").RegisterCallback<ChangeEvent<int>>(evt =>
            {
                PlacementEdit.ChangeRotation(evt.newValue);
            });
        };

        root.Q<Button>("rotation-accept").clickable.clicked += () =>
        {
            // Von Scaling Options zu Placement Uebersicht wechseln
            _UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.Flex;
            _UIRenderer.visualTree.Q("placement-edit-rotate-panel").style.display = DisplayStyle.None;
        };

        root.Q<Button>("move-accept").clickable.clicked += () =>
        {
            // Von Scaling Options zu Placement Uebersicht wechseln
            _UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.Flex;
            _UIRenderer.visualTree.Q("placement-edit-position-panel").style.display = DisplayStyle.None;
        };

        root.Q<Button>("move-obj").clickable.clicked += () =>
        {
            _UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("placement-edit-position-panel").style.display = DisplayStyle.Flex;
        };

        root.Q<Button>("move-up").clickable.clicked += () =>
        {
            PlacementEdit.ChangePositionUp();
        };

        root.Q<Button>("move-down").clickable.clicked += () =>
        {
            PlacementEdit.ChangePositionDown();
        };

        root.Q<Button>("move-left").clickable.clicked += () =>
        {
            PlacementEdit.ChangePositionLeft();
        };

        root.Q<Button>("move-right").clickable.clicked += () =>
        {
            PlacementEdit.ChangePositionRight();
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
            _UIRenderer.visualTree.Q("create-section-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("switch-daytime-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("interaction-panel").style.display = DisplayStyle.Flex;
		};

		root.Q<Button>("options-switch-ghosted").clickable.clicked += () =>
		{
			// Ghosted umschalten
			var newVis = GameManager.Instance.CameraController.GetVisualization() == CameraController.Visualization.Normal ?
				CameraController.Visualization.Ghosted : CameraController.Visualization.Normal;
			GameManager.Instance.CameraController.SetVisualization(newVis);

            _UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.None;
        };

		root.Q<Button>("options-storeys-edit").clickable.clicked += () =>
		{
			_UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.Flex;
            _UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
            foreach (string storey in storeyNames)
			{
                if (!root.Q("toggle-area").Contains(root.Q("toggle-area").Q<Toggle>("toggle-" + storey)))
                {
                    Toggle storeyToggle = new Toggle(storey);
                    storeyToggle.AddToClassList("toggle-section");
                    storeyToggle.name = "toggle-" + storey;
                    storeyToggle.value = true;
                    root.Q("toggle-area").Add(storeyToggle);
                }     
			}

			foreach (string storey in storeyNames)
				root.Q("toggle-area").Q<Toggle>("toggle-" + storey).RegisterCallback<ChangeEvent<bool>>(e => TridifyQuery.changeStoreyState(e.newValue, storey));
		};

		root.Q<Button>("reset-btn").clickable.clicked += () =>
		{
			foreach (string storey in storeyNames)
			{
				root.Q<Toggle>("toggle-" + storey).value = true;
				TridifyQuery.changeStoreyState(true, storey);
			}
		};

        root.Q<Button>("options-sections-create").clickable.clicked += () =>
        {
            _UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("create-section-panel").style.display = DisplayStyle.Flex;

            root.Q<SliderInt>("move-section-slider").RegisterCallback<ChangeEvent<int>>(evt =>
            {
                float value = (float)evt.newValue / 100.0f;
                Debug.Log("SectionCreate, value: " + evt.newValue+",Floatvalue: "+value);
                CameraController.SetCutPlane(value);
            });
        };

        root.Q<Button>("create-section").clickable.clicked += () =>
        {
            CameraController.SetCutPlane(50.0f);
        };

        root.Q<Button>("create-section-panel-exit").clickable.clicked += () =>
        {
            _UIRenderer.visualTree.Q("create-section-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.Flex;
        };     

        root.Q<Button>("options-switch-daytime").clickable.clicked += () =>
        {
            _UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("switch-daytime-panel").style.display = DisplayStyle.Flex;

            root.Q<SliderInt>("daytime-slider").RegisterCallback<ChangeEvent<int>>(evt =>
            {
                Debug.Log("SunLight changed, value: " + evt.newValue);
                int hours = evt.newValue / 60;
                int minutes = evt.newValue % 60;
                Debug.Log("hours: " + hours+",minutes: "+minutes);
                _sunCalculator.SetTime(hours, minutes);
            });
        };

        root.Q<Button>("switch-daytime-exit").clickable.clicked += () =>
        {
            _UIRenderer.visualTree.Q("switch-daytime-panel").style.display = DisplayStyle.None;
            _UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.Flex;
        };


        // Initialisieren
        _UIRenderer.visualTree.Q("info-panel").style.display = DisplayStyle.Flex;
		_UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.None;
		_UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.None;
		_UIRenderer.visualTree.Q("interaction-panel").style.display = DisplayStyle.None;
		_UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
		_UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.None;
		_UIRenderer.visualTree.Q("placement-edit-panel").style.display = DisplayStyle.None;
        _UIRenderer.visualTree.Q("placement-edit-scale-panel").style.display = DisplayStyle.None;
        _UIRenderer.visualTree.Q("placement-edit-rotate-panel").style.display = DisplayStyle.None;
        _UIRenderer.visualTree.Q("placement-edit-position-panel").style.display = DisplayStyle.None;
        _UIRenderer.visualTree.Q("create-section-panel").style.display = DisplayStyle.None;
        _UIRenderer.visualTree.Q("switch-daytime-panel").style.display = DisplayStyle.None;


        return null;
	}

	private void Awake()
	{
		_UIRenderer = GetComponent<PanelRenderer>();
		_UIRenderer.postUxmlReload = BindPanel;
		GameManager.Instance.MenuChanged += X_MenuChanged;
        _sunCalculator = GetComponent<SunCalculator>();

    }

	public static void ShowDescription(string title, List<string> descs)
	{
		_UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.Flex;
        _UIRenderer.visualTree.Q("hideSections-area").style.display = DisplayStyle.None;
        _UIRenderer.visualTree.Q<TextElement>("desc-heading").text = title;

        foreach (string desc in descs)
        {
            TextElement descLabel = new TextElement();
            descLabel.AddToClassList("desc-label");
            descLabel.text = desc;
            _UIRenderer.visualTree.Q("desc-txt").Add(descLabel);
        }
	}

	public void HideDescription()
	{
		_UIRenderer.visualTree.Q("desc-area").style.display = DisplayStyle.None;
	}

	private void OnDestroy()
	{
		GameManager.Instance.MenuChanged -= X_MenuChanged;
	}

}
