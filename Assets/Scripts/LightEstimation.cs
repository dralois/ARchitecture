using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Light))]
public class LightEstimation : MonoBehaviour
{

	[SerializeField] private ARCameraManager _CameraManager = null;
	[SerializeField] private bool _isHouseLight = true;

	private Light _Light = null;

	public ARCameraManager CameraManager
	{
		get { return _CameraManager; }
		set
		{
			if (_CameraManager == value)
				return;

			if (_CameraManager != null)
				_CameraManager.frameReceived -= X_FrameChanged;

			_CameraManager = value;

			if (_CameraManager != null & enabled)
				_CameraManager.frameReceived += X_FrameChanged;
		}
	}

	public float? Lumen { get; private set; }

	public float? ColorTemperature { get; private set; }

	public Color? ColorCorrection { get; private set; }

	private void Awake()
	{
		_Light = GetComponent<Light>();
	}

	private void OnEnable()
	{
		if (_CameraManager != null)
			_CameraManager.frameReceived += X_FrameChanged;
	}

	private void OnDisable()
	{
		if (_CameraManager != null)
			_CameraManager.frameReceived -= X_FrameChanged;
	}

	private void X_FrameChanged(ARCameraFrameEventArgs args)
	{
		// Early out
		if (GameManager.Instance.CurrentMode != GameManager.Mode.Interaction)
			return;

		// Licht ggf. aktivieren
		if (args.lightEstimation.averageIntensityInLumens.HasValue)
		{
			Lumen = args.lightEstimation.averageIntensityInLumens.Value;
			_Light.enabled = Lumen.Value < 800f || !_isHouseLight;
		}

		// Farbtemperatur setzen
		if (args.lightEstimation.averageColorTemperature.HasValue)
		{
			ColorTemperature = args.lightEstimation.averageColorTemperature.Value;
			_Light.colorTemperature = ColorTemperature.Value;
		}

		// Farbkorrektur setzen
		if (args.lightEstimation.colorCorrection.HasValue)
		{
			ColorCorrection = args.lightEstimation.colorCorrection.Value;
			_Light.color = ColorCorrection.Value;
		}
	}

}
