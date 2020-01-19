using UnityEngine;
using UnityEngine.UIElements;
using Unity.UIElements.Runtime;
using System.Collections.Generic;

public class UIHandlerNew : MonoBehaviour
{

	[SerializeField] private PanelRenderer _UIRenderer = null;

	private void X_ModeChanged(GameManager.InputMode newMode)
	{
		switch (newMode)
		{
			case GameManager.InputMode.Spawn:
				{
					// Nichts zu tun
					break;
				}
			case GameManager.InputMode.Placement:
				{
					// Von Info zu Placement Panel wechseln
					_UIRenderer.visualTree.Q("info-panel").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.Flex;
					break;
				}
			case GameManager.InputMode.Interaction:
				{
					// Von Placement zu Interaction Panel wechseln
					_UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.None;
					_UIRenderer.visualTree.Q("interaction-panel").style.display = DisplayStyle.Flex;
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
			//_UIRenderer.visualTree.Q("placement-panel").style.display = DisplayStyle.None;
			//_UIRenderer.visualTree.Q("edit-panel").style.display = DisplayStyle.Flex;
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
			_UIRenderer.visualTree.Q("options-panel").style.display = DisplayStyle.None;
			_UIRenderer.visualTree.Q("interaction-panel").style.display = DisplayStyle.Flex;
		};

		return null;
	}

	private void Awake()
	{
		_UIRenderer.postUxmlReload = BindPanel;
		GameManager.Instance.ModeChanged += X_ModeChanged;
	}

	private void OnDestroy()
	{
		GameManager.Instance.ModeChanged -= X_ModeChanged;
	}

}
