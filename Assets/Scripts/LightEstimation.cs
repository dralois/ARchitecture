using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Light))]
public class LightEstimation : MonoBehaviour
{

	[SerializeField] private ARCameraManager _CameraManager;
	private Light _Light;

	public ARCameraManager CameraManager
	{
		get { return _CameraManager; }
		set
		{
			if (_CameraManager == value)
				return;

			if (_CameraManager != null)
				_CameraManager.frameReceived -= FrameChanged;

			_CameraManager = value;

			if (_CameraManager != null & enabled)
				_CameraManager.frameReceived += FrameChanged;
		}
	}

	public float? Brightness { get; private set; }

	public float? ColorTemperature { get; private set; }

	public Color? ColorCorrection { get; private set; }

	private void Awake()
	{
		_Light = GetComponent<Light>();
	}

	private void OnEnable()
	{
		if (_CameraManager != null)
			_CameraManager.frameReceived += FrameChanged;
	}

	private void OnDisable()
	{
		if (_CameraManager != null)
			_CameraManager.frameReceived -= FrameChanged;
	}

	private void FrameChanged(ARCameraFrameEventArgs args)
	{
		if (args.lightEstimation.averageBrightness.HasValue)
		{
			Brightness = args.lightEstimation.averageBrightness.Value;
			_Light.intensity = Brightness.Value;
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
