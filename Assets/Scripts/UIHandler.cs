using UnityEngine;

public class UIHandler : MonoBehaviour
{

	[SerializeField] private GameObject _spawnUI = null;
	[SerializeField] private GameObject _placementUI = null;
	[SerializeField] private GameObject _interactionUI = null;
	[SerializeField] private GameObject _decorationUI = null;

	public void AcceptPlacement()
	{
		GameManager.Instance.SwitchMode(GameManager.InputMode.Interaction);
	}

	private void X_ModeChange(GameManager.InputMode newMode)
	{
		// Zunaechst alles deaktivieren
		_spawnUI.SetActive(false);
		_placementUI.SetActive(false);
		_interactionUI.SetActive(false);
		_decorationUI.SetActive(false);
		// UI umschalten
		switch (newMode)
		{
			case GameManager.InputMode.Spawn:
				{
					_spawnUI.SetActive(true);
					break;
				}
			case GameManager.InputMode.Placement:
				{
					_placementUI.SetActive(true);
					break;
				}
			case GameManager.InputMode.Interaction:
				{
					_interactionUI.SetActive(true);
					break;
				}
			case GameManager.InputMode.Decoration:
				{
					_decorationUI.SetActive(true);
					break;
				}
		}
	}

	private void OnEnable()
	{
		GameManager.Instance.ModeChanged += X_ModeChange;
	}

	private void OnDisable()
	{
		GameManager.Instance.ModeChanged -= X_ModeChange;
	}
}
