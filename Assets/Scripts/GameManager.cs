using UnityEngine;

public class GameManager : MonoBehaviour
{

	#region Enums

	public enum InputMode
	{
		None = -1,
		Spawn,
		Placement,
		Interaction,
		Decoration
	}

	public enum LightMood
	{
		Day,
		Night
	}

	#endregion

	#region Fields

	// Singleton
	private static GameManager _instance = null;

	#endregion

	#region Events

	public event System.Action<InputMode> ModeChanged;

	public event System.Action<LightMood> MoodChanged;

	#endregion

	#region Properties

	public static GameManager Instance { get =>_instance; }

	public GameObject House { get; set; } = null;

	public VisualizationSwitcher Visualizer { get; set; }

	public InputMode CurrentMode { get; private set; } = InputMode.None;

	public LightMood CurrentMood { get; private set; } = LightMood.Day;

	#endregion

	#region Methods

	public void SwitchMood(float lumen)
	{
		LightMood nextMood = lumen < 800 ? LightMood.Night : LightMood.Day;
		// Falls geaendert
		if(nextMood != CurrentMood)
		{
			// Aktionen je nach Modus
			switch (nextMood)
			{
				case LightMood.Day:
					{

						break;
					}
				case LightMood.Night:
					{

						break;
					}
			}
			// Modus speichern & Event ausloesen
			CurrentMood = nextMood;
			MoodChanged?.Invoke(CurrentMood);
		}
	}

	public void SwitchMode(InputMode nextMode)
	{
		// Falls geaendert
		if(nextMode != CurrentMode)
		{
			// Aktionen je nach Modus
			switch (nextMode)
			{
				case InputMode.Spawn:
					{

						break;
					}
				case InputMode.Placement:
					{

						break;
					}
				case InputMode.Interaction:
					{

						break;
					}
				case InputMode.Decoration:
					{

						break;
					}
            }
			// Modus speichern & Event ausloesen
			CurrentMode = nextMode;
			ModeChanged?.Invoke(CurrentMode);
		}
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
		SwitchMode(InputMode.Spawn);
	}

	#endregion

	#endregion

}
