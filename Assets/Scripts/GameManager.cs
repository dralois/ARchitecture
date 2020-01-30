using UnityEngine;

public class GameManager : MonoBehaviour
{

	#region Enums

	public enum MenuMode
	{
		None = -1,
		Spawn,
		Placement,
		Interaction,
		Decoration
	}

	public enum LightTime
	{
		Day,
		Night
	}

	public enum SizeMode
	{
		Model,
		Normal,
		RealLife
	}

	#endregion

	#region Fields

	// Singleton
	private static GameManager _instance = null;

	#endregion

	#region Events

	public event System.Action<MenuMode> MenuChanged;

	public event System.Action<LightTime> LightChanged;

	public event System.Action<SizeMode> SizeChanged;

	#endregion

	#region Properties

	public static GameManager Instance { get =>_instance; }

	public GameObject PlacedIFC { get; set; } = null;

	public CameraController CameraController { get; set; }

	public MenuMode CurrentMenu { get; private set; } = MenuMode.None;

	public LightTime CurrentLight { get; private set; } = LightTime.Day;

	public SizeMode CurrentSize { get; private set; } = SizeMode.Model;

	#endregion

	#region Methods

	public void SwitchTime(float lumen)
	{
		LightTime nextTime = lumen < 800 ? LightTime.Night : LightTime.Day;
		// Falls geaendert
		if(nextTime != CurrentLight)
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
		if(nextMenu != CurrentMenu)
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
				case MenuMode.Decoration:
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
		if(nextSize != CurrentSize)
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
				case SizeMode.RealLife:
					{

						break;
					}
			}
			// Groesse speichern & Event ausloesen
			CurrentSize = nextSize;
			SizeChanged?.Invoke(CurrentSize);
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
		SwitchMenu(MenuMode.Spawn);
	}

	#endregion

	#endregion

}
