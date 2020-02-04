using UnityEngine;

public class GameManager : MonoBehaviour
{

	#region Enums

	public enum PlacementMode
	{
		None,
		Free,
		QR
	}

	public enum MenuMode
	{
		None = -1,
		Spawn,
		Placement,
		Interaction,
		Options
	}

	public enum LightMode
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

	[SerializeField] private float _ScaleModeChange = 15f;

	// Singleton
	private static GameManager _instance = null;

	private GameObject _placedIFC = null;
	private Vector3 _originalScale = Vector3.one;
	private float _originalRot = 0f;

	private float _actualScale = 1f;

	#endregion

	#region Events

	public event System.Action<MenuMode> MenuChanged;

	public event System.Action<LightMode> LightChanged;

	public event System.Action<SizeMode> SizeChanged;

	public event System.Action<PlacementMode> PlacementModeSelected;

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

	public PlacementMode SelectedPlacementMode { get; private set; } = PlacementMode.None;

	public MenuMode CurrentMenu { get; private set; } = MenuMode.None;

	public LightMode CurrentLight { get; private set; } = LightMode.Day;

	public SizeMode CurrentSize { get; private set; } = SizeMode.Normal;

	public float IFCScale { get => _actualScale; }

	public float SizeScale { get => CurrentSize == SizeMode.Normal ? 1f : _ScaleModeChange; }

	#endregion

	#region Methods

	public void SetPlacementMode(PlacementMode mode)
	{
		// Speichern und Event ausloesen
		SelectedPlacementMode = mode;
		PlacementModeSelected?.Invoke(SelectedPlacementMode);
	}

	public void SwitchLightMode(float lumen)
	{
		// 800 Lumen ist ziemlich dunkel
		LightMode nextTime = lumen < 800 ? LightMode.Night : LightMode.Day;
		// Falls geaendert
		if (nextTime != CurrentLight)
		{
			// Aktionen je nach Modus
			switch (nextTime)
			{
				case LightMode.Day:
					{

						break;
					}
				case LightMode.Night:
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
						_placedIFC.transform.localScale /= _ScaleModeChange;
						break;
					}
				case SizeMode.Scaled:
					{
						_placedIFC.transform.localScale *= _ScaleModeChange;
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
		// Slider Wert umrechnen
		if(scalingValue < 0)
		{
			// -100 - 0: 10% - 100%
			_actualScale = (((scalingValue + 100f) * 0.9f) / 100f) + 0.1f;
		}
		else
		{
			// 0 - 100: 100% - 1000%
			_actualScale = ((scalingValue * 9f) / 100f) + 1f;
		}
		// Skalierung anpassen
		Vector3 scaleChange = _originalScale * _actualScale;
		_placedIFC.transform.localScale = scaleChange;
	}

	public void ChangeRotation(float rotValue)
	{
		float rot = rotValue;
		rot += _originalRot;
		_placedIFC.transform.localRotation = Quaternion.Euler(0, rot, 0);
	}

	public void ChangePosition(Vector2 direction)
	{
		_placedIFC.transform.localPosition += new Vector3(direction.x, 0, direction.y);
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
