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
		Options
	}

	public enum LightTime
	{
		Day,
		Night
	}

	public enum SizeMode
	{
		Normal,
		Scaled
	}

	#endregion

	#region Fields

	// Singleton
	private static GameManager _instance = null;

	private GameObject _placedIFC = null;
	private Vector3 _originalScale = Vector3.one;
	private float _originalRot = 0f;

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
			_originalScale = _placedIFC.transform.localScale;
			_originalRot = _placedIFC.transform.localRotation.eulerAngles.y;
		}
	}

	public CameraController CameraController { get; set; }

	public UIHandler UIController { get; set; }

	public MenuMode CurrentMenu { get; private set; } = MenuMode.None;

	public LightTime CurrentLight { get; private set; } = LightTime.Day;

	public SizeMode CurrentSize { get; private set; } = SizeMode.Normal;

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
				case SizeMode.Normal:
					{
						_placedIFC.transform.localScale /= 10;
						break;
					}
				case SizeMode.Scaled:
					{
						_placedIFC.transform.localScale *= 10;
						break;
					}
			}
			// Groesse speichern & Event ausloesen
			CurrentSize = nextSize;
			SizeChanged?.Invoke(CurrentSize);
		}
	}

	public void ChangeScaling(float scalingValue)
	{
		float scaling = (float)scalingValue / 10.0f;
		Vector3 scaleChange = new Vector3(1.0f, 1.0f, 1.0f) * scaling;
		scaleChange += _originalScale;
		_placedIFC.transform.localScale = scaleChange;
	}

	public void ChangeRotation(float rotValue)
	{
		float rot = rotValue;
		rot += _originalRot;
		_placedIFC.transform.rotation = Quaternion.Euler(0, rot, 0);
	}

	public void ChangePosition(Vector2 direction)
	{
		_placedIFC.transform.position += new Vector3(direction.x, 0, direction.y);
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
