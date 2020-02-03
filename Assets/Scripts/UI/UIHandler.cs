using UnityEngine;
using UnityEngine.UIElements;
using Unity.UIElements.Runtime;
using System.Collections.Generic;
#if UNITY_IOS
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARKit;
#endif

[RequireComponent(typeof(PanelRenderer))]
public class UIHandler : MonoBehaviour
{

	[SerializeField] private List<Texture2D> _animationFrames = new List<Texture2D>();

	private PanelRenderer _UIRenderer = null;
	private SunCalculator _sunCalculator = null;
#if UNITY_IOS
	private ARSession _session = null;
#endif

	private string[] _storeyNames = TridifyQuery.GetStoreyNames();
	private string[] _currentDesc = new string[0];

	private bool _stopAnimation = false;
	private int _currentFrame = 0;

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
					root.Q("placement-mode-select").style.display = DisplayStyle.None;
					root.Q("placement-free-info").style.display = DisplayStyle.None;
					root.Q("placement-qr-info").style.display = DisplayStyle.None;
					root.Q("placement-panel").style.display = DisplayStyle.Flex;
#if UNITY_IOS
					if (_session.subsystem is ARKitSessionSubsystem sessionSubsystem)
					{
						sessionSubsystem.SetCoachingActive(false, ARCoachingOverlayTransition.Animated);
					}
#else
					root.Q("animation-area").style.display = DisplayStyle.None;
#endif
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
					break;
				}
		}
	}

	private IEnumerable<Object> X_BindPanel()
	{
		// Root cachen
		var root = _UIRenderer.visualTree;

		#region Placement & Edit

		// Animation Setup
		var animation = root.Q("placement-animation");
		animation?.schedule.Execute(() =>
		{
			if (_animationFrames.Count == 0)
				return;
			// Frame update
			_currentFrame = (_currentFrame + 1) % _animationFrames.Count;
			var frame = _animationFrames[_currentFrame];
			animation.style.backgroundImage = frame;
			// 100ms Schritte bis Flag gesetzt wird
		}).Every(100).Until(() => { return _stopAnimation; });

		// Placement Mode Free Button binden
		root.Q<Button>("placement-select-free").clickable.clicked += () =>
		{
			root.Q("placement-mode-select").style.display = DisplayStyle.None;
			root.Q("placement-free-info").style.display = DisplayStyle.Flex;
#if UNITY_IOS
			if (_session.subsystem is ARKitSessionSubsystem sessionSubsystem)
			{
				sessionSubsystem.coachingGoal = ARCoachingGoal.HorizontalPlane;
				sessionSubsystem.SetCoachingActive(true, ARCoachingOverlayTransition.Animated);
			}
#else
			root.Q("animation-area").style.display = DisplayStyle.Flex;
#endif
			GameManager.Instance.SetPlacementMode(GameManager.PlacementMode.Free);
		};

		// Placement Mode QR Code Button binden
		root.Q<Button>("placement-select-qr").clickable.clicked += () =>
		{
			root.Q("placement-mode-select").style.display = DisplayStyle.None;
			root.Q("placement-qr-info").style.display = DisplayStyle.Flex;
			GameManager.Instance.SetPlacementMode(GameManager.PlacementMode.QR);
		};

		// Placement Accept Button binden
		root.Q<Button>("placement-accept").clickable.clicked += () =>
		{
			// Modus wechseln (Placement -> Interaction)
			GameManager.Instance.SwitchMenu(GameManager.MenuMode.Interaction);
		};

		// Placement Edit Button binden
		root.Q<Button>("placement-edit").clickable.clicked += () =>
		{
			root.Q("placement-panel").style.display = DisplayStyle.None;
			root.Q("placement-edit-panel").style.display = DisplayStyle.Flex;
		};

		// Edit Accept Button binden
		root.Q<Button>("placement-edit-accept").clickable.clicked += () =>
		{
			root.Q("placement-edit-panel").style.display = DisplayStyle.None;
			root.Q("placement-panel").style.display = DisplayStyle.Flex;
		};

		// Scale Edit Button binden
		root.Q<Button>("scale-obj").clickable.clicked += () =>
		{
			root.Q("placement-edit-panel").style.display = DisplayStyle.None;
			root.Q("placement-edit-scale-panel").style.display = DisplayStyle.Flex;
			// Scale Slider Callback speichern
			root.Q<Slider>("scale-edit-slider").RegisterCallback<ChangeEvent<float>>(evt =>
			{
				GameManager.Instance.ChangeScaling(evt.newValue);
			});
		};

		// Scale Accept Button binden
		root.Q<Button>("scaling-accept").clickable.clicked += () =>
		{
			root.Q("placement-edit-panel").style.display = DisplayStyle.Flex;
			root.Q("placement-edit-scale-panel").style.display = DisplayStyle.None;
		};

		// Rotation Edit Button binden
		root.Q<Button>("rotate-obj").clickable.clicked += () =>
		{
			root.Q("placement-edit-panel").style.display = DisplayStyle.None;
			root.Q("placement-edit-rotate-panel").style.display = DisplayStyle.Flex;
			// Rotation Slider Callback speichern
			root.Q<Slider>("rotate-edit-slider").RegisterCallback<ChangeEvent<float>>(evt =>
			{
				GameManager.Instance.ChangeRotation(evt.newValue);
			});
		};

		// Rotation Accept Button binden
		root.Q<Button>("rotation-accept").clickable.clicked += () =>
		{
			root.Q("placement-edit-panel").style.display = DisplayStyle.Flex;
			root.Q("placement-edit-rotate-panel").style.display = DisplayStyle.None;
		};

		// Move Edit Button binden
		root.Q<Button>("move-obj").clickable.clicked += () =>
		{
			root.Q("placement-edit-panel").style.display = DisplayStyle.None;
			root.Q("placement-edit-position-panel").style.display = DisplayStyle.Flex;
		};

		// Move Accept Button binden
		root.Q<Button>("move-accept").clickable.clicked += () =>
		{
			root.Q("placement-edit-panel").style.display = DisplayStyle.Flex;
			root.Q("placement-edit-position-panel").style.display = DisplayStyle.None;
		};

		// Move Up Button binden
		root.Q<Button>("move-up").clickable.clicked += () =>
		{
			GameManager.Instance.ChangePosition(Vector2.up);
		};

		// Move Down Button binden
		root.Q<Button>("move-down").clickable.clicked += () =>
		{
			GameManager.Instance.ChangePosition(Vector2.down);
		};

		// Move Left Button binden
		root.Q<Button>("move-left").clickable.clicked += () =>
		{
			GameManager.Instance.ChangePosition(Vector2.left);
		};

		// Move Right Button binden
		root.Q<Button>("move-right").clickable.clicked += () =>
		{
			GameManager.Instance.ChangePosition(Vector2.right);
		};

		#endregion

		#region Options

		// Optionen Button binden
		root.Q<Button>("interaction-options").clickable.clicked += () =>
		{
			GameManager.Instance.SwitchMenu(GameManager.MenuMode.Options);
			root.Q("interaction-panel").style.display = DisplayStyle.None;
			root.Q("options-panel").style.display = DisplayStyle.Flex;
		};

		// Options Exit Button binden
		root.Q<Button>("options-exit").clickable.clicked += () =>
		{
			GameManager.Instance.SwitchMenu(GameManager.MenuMode.Interaction);
			root.Q("desc-area").style.display = DisplayStyle.None;
			root.Q("hideSections-area").style.display = DisplayStyle.None;
			root.Q("options-panel").style.display = DisplayStyle.None;
			root.Q("create-section-panel").style.display = DisplayStyle.None;
			root.Q("switch-daytime-panel").style.display = DisplayStyle.None;
			root.Q("interaction-panel").style.display = DisplayStyle.Flex;
		};

		// Ghost Mode Button binden
		root.Q<Button>("options-switch-ghosted").clickable.clicked += () =>
		{
			// Ghosted umschalten
			var newVis = GameManager.Instance.CameraController.GetVisualization() == CameraController.Visualization.Normal ?
				CameraController.Visualization.Ghosted : CameraController.Visualization.Normal;

			if (newVis == CameraController.Visualization.Ghosted)
			{
				root.Q<Button>("options-switch-ghosted").AddToClassList("clicked-button");
			}
			else
			{
				root.Q<Button>("options-switch-ghosted").RemoveFromClassList("clicked-button");
			}

			GameManager.Instance.CameraController.SetVisualization(newVis);
		};

		// Storey Button binden
		root.Q<Button>("options-storeys-edit").clickable.clicked += () =>
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

		// Section Edit Button binden
		root.Q<Button>("options-sections-create").clickable.clicked += () =>
		{
			root.Q("options-panel").style.display = DisplayStyle.None;
			root.Q("create-section-panel").style.display = DisplayStyle.Flex;
			// Section Slider Callback speichern
			root.Q<Slider>("move-section-slider").RegisterCallback<ChangeEvent<float>>(evt =>
			{
				GameManager.Instance.CameraController.SetCutPlane(evt.newValue);
			});
		};

		// Sections Accept Button binden
		root.Q<Button>("create-section-panel-exit").clickable.clicked += () =>
		{
			GameManager.Instance.CameraController.SetCutPlane(0f);
			root.Q("create-section-panel").style.display = DisplayStyle.None;
			root.Q("options-panel").style.display = DisplayStyle.Flex;
		};

		// Scale Mode Button binden
		root.Q<Button>("options-scale").clickable.clicked += () =>
		{
			// Size umschalten
			var newSize = GameManager.Instance.CurrentSize == GameManager.SizeMode.Scaled ?
				GameManager.SizeMode.Normal : GameManager.SizeMode.Scaled;

			if(newSize == GameManager.SizeMode.Scaled)
			{
				root.Q<Button>("options-scale").AddToClassList("clicked-button");
			}
			else
			{
				root.Q<Button>("options-scale").RemoveFromClassList("clicked-button");
			}

			GameManager.Instance.SwitchSize(newSize);
		};

		// TOD Edit Button binden
		root.Q<Button>("options-switch-daytime").clickable.clicked += () =>
		{
			root.Q("options-panel").style.display = DisplayStyle.None;
			root.Q("switch-daytime-panel").style.display = DisplayStyle.Flex;
			// TOD Slider Callback speichern
			root.Q<Slider>("daytime-slider").RegisterCallback<ChangeEvent<float>>(evt =>
			{
				int hours = (int)evt.newValue / 60;
				int minutes = (int)evt.newValue % 60;
				_sunCalculator.SetTime(hours, minutes);
			});
		};

		// TOD Accept Button binden
		root.Q<Button>("switch-daytime-exit").clickable.clicked += () =>
		{
			root.Q("switch-daytime-panel").style.display = DisplayStyle.None;
			root.Q("options-panel").style.display = DisplayStyle.Flex;
		};

		#endregion

		#region Initialisierung

		root.Q("placement-mode-select").style.display = DisplayStyle.Flex;
		root.Q("placement-free-info").style.display = DisplayStyle.None;
		root.Q("placement-qr-info").style.display = DisplayStyle.None;
		root.Q("placement-panel").style.display = DisplayStyle.None;
		root.Q("options-panel").style.display = DisplayStyle.None;
		root.Q("interaction-panel").style.display = DisplayStyle.None;
		root.Q("animation-area").style.display = DisplayStyle.None;
		root.Q("desc-area").style.display = DisplayStyle.None;
		root.Q("hideSections-area").style.display = DisplayStyle.None;
		root.Q("placement-edit-panel").style.display = DisplayStyle.None;
		root.Q("placement-edit-scale-panel").style.display = DisplayStyle.None;
		root.Q("placement-edit-rotate-panel").style.display = DisplayStyle.None;
		root.Q("placement-edit-position-panel").style.display = DisplayStyle.None;
		root.Q("create-section-panel").style.display = DisplayStyle.None;
		root.Q("switch-daytime-panel").style.display = DisplayStyle.None;

		#endregion

		// Kein return notwendig
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
		root.Q("hideSections-area").style.display = DisplayStyle.None;
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
		// Panelrenderer cachen und Event binden
		_UIRenderer = GetComponent<PanelRenderer>();
		_UIRenderer.postUxmlReload = X_BindPanel;
		// GameManager diesen UI Handler zuweisen
		GameManager.Instance.MenuChanged += X_MenuChanged;
		GameManager.Instance.UIController = this;
		// Lightcalculator cachen
		_sunCalculator = FindObjectOfType<SunCalculator>();
#if UNITY_IOS
		// iOS: AR Session cachen
		_session = FindObjectOfType<ARSession>();
#endif
	}

	private void OnDestroy()
	{
		GameManager.Instance.MenuChanged -= X_MenuChanged;
	}

}
