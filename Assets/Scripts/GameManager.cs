using UnityEngine;

public class GameManager : MonoBehaviour
{

	#region Enums

	public enum MenuMode
	{
		None = -1,
		Spawn,
		Placement,
		Interaction
	}

	public enum LightTime
	{
		Day,
		Night
	}

	public enum SizeMode
	{
		Model,
		Normal
	}

	#endregion

	#region Fields

	// Singleton
	private static GameManager _instance = null;

	private Vector3 originalScale;
	private GameObject _placedIFC = null;

	#endregion

	#region Events

	public event System.Action<MenuMode> MenuChanged;

	public event System.Action<LightTime> LightChanged;

	public event System.Action<SizeMode> SizeChanged;

	#endregion

	#region Properties

	public static GameManager Instance { get => _instance; }

	public GameObject PlacedIFC
	{
		get => _placedIFC;
		set
		{
			_placedIFC = value;
			originalScale = _placedIFC.transform.localScale;
		}
	}

	public CameraController CameraController { get; set; }

	public UIHandler UIController { get; set; }

	public MenuMode CurrentMenu { get; private set; } = MenuMode.None;

	public LightTime CurrentLight { get; private set; } = LightTime.Day;

	public SizeMode CurrentSize { get; private set; } = SizeMode.Model;

	#endregion

	#region Methods

	public void SwitchTime(float lumen)
	{
		LightTime nextTime = lumen < 800 ? LightTime.Night : LightTime.Day;
		// Falls geaendert
		if (nextTime != CurrentLight)
		{
			// Aktionen je nach Modus
			switch (nextTime)
			{
				case LightTime.Day:
					{

						break;
					}
				case LightTime.Night:
					{

						break;
					}
			}
			// Modus speichern & Event ausloesen
			CurrentLight = nextTime;
			LightChanged?.Invoke(CurrentLight);
		}
	}

	public void SwitchMenu(MenuMode nextMenu)
	{
		// Falls geaendert
		if (nextMenu != CurrentMenu)
		{
			// Aktionen je nach Modus
			switch (nextMenu)
			{
				case MenuMode.Spawn:
					{

						break;
					}
				case MenuMode.Placement:
					{

						break;
					}
				case MenuMode.Interaction:
					{

						break;
					}
			}
			// Modus speichern & Event ausloesen
			CurrentMenu = nextMenu;
			MenuChanged?.Invoke(CurrentMenu);
		}
	}

	public void SwitchSize(SizeMode nextSize)
	{
		// Falls geaendert
		if (nextSize != CurrentSize)
		{
			// Aktion je nach Groesse
			switch (nextSize)
			{
				case SizeMode.Model:
					{

						break;
					}
				case SizeMode.Normal:
					{

						break;
					}
			}
			// Groesse speichern & Event ausloesen
			CurrentSize = nextSize;
			SizeChanged?.Invoke(CurrentSize);
		}
	}

	public void ChangeScaling(int scalingValue)
	{
		float scaling;
		// Wert umrechnen
		if (scalingValue < 0)
		{
			scaling = 1 / Mathf.Abs(scalingValue);
		}
		else
		{
			scaling = scalingValue;
		}
		// Scale anpassen
		Vector3 scaleChange = new Vector3(1.0f, 1.0f, 1.0f) * scaling;
		scaleChange += originalScale;
		_placedIFC.transform.localScale = scaleChange;
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
		SwitchMenu(MenuMode.Spawn);
	}

	#endregion

	#endregion

}
