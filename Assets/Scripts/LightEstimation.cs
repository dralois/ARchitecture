using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Light))]
public class LightEstimation : MonoBehaviour
{

	[SerializeField] private ARCameraManager _CameraManager;

	private float _maxIntensity = 1f;
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

	public float? Brightness { get; private set; }

	public float? Lumen { get; private set; }

	public float? ColorTemperature { get; private set; }

	public Color? ColorCorrection { get; private set; }

	private void Awake()
	{
		_Light = GetComponent<Light>();
		_maxIntensity = _Light.intensity;
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
		if (GameManager.Instance.CurrentMode != GameManager.Mode.Interaction)
			return;

		if (args.lightEstimation.averageBrightness.HasValue)
		{
			Lumen = args.lightEstimation.averageIntensityInLumens.Value;
			_Light.enabled = Lumen.Value < 100f;
		}

		if (args.lightEstimation.averageBrightness.HasValue)
		{
			Brightness = args.lightEstimation.averageBrightness.Value;
			_Light.intensity = Mathf.Lerp(0f, _maxIntensity, Brightness.Value);
		}

		if (args.lightEstimation.averageColorTemperature.HasValue)
		{
			ColorTemperature = args.lightEstimation.averageColorTemperature.Value;
			_Light.colorTemperature = ColorTemperature.Value;
		}

		if (args.lightEstimation.colorCorrection.HasValue)
		{
			ColorCorrection = args.lightEstimation.colorCorrection.Value;
			_Light.color = ColorCorrection.Value;
		}
	}

}
