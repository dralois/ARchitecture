using UnityEngine;

public class GameManager : MonoBehaviour
{

	#region Enums

	public enum Mode
	{
		None = -1,
		Spawn,
		Placement,
		Interaction,
		Decoration
	}

	#endregion

	#region Fields

	private static GameManager _instance = null;

	private GameObject _houseGO = null;

	#endregion

	#region Events

	public event System.Action<Mode> ModeChanged;

	#endregion

	#region Properties

	public static GameManager Instance { get =>_instance; }

	public GameObject House { get => _houseGO; set => _houseGO = value; }

	public Mode CurrentMode { get; private set; } = Mode.None;

	#endregion

	#region Methods

	public void SwitchMode(Mode nextMode)
	{
		// Aktionen je nach Modus
		switch (nextMode)
		{
			case Mode.Spawn:
				{

					break;
				}
			case Mode.Placement:
				{

					break;
				}
			case Mode.Interaction:
				{

					break;
				}
			case Mode.Decoration:
				{

					break;
				}
		}
		// Modus speichern & Event ausloesen
		CurrentMode = nextMode;
		ModeChanged?.Invoke(CurrentMode);
	}

	#region Unity

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
		}
		else
		{
			_instance = this;
		}
	}

	private void Start()
	{
		SwitchMode(Mode.Spawn);
	}

	#endregion

	#endregion

}
