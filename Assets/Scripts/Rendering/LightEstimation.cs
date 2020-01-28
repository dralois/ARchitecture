using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(Light))]
public class LightEstimation : MonoBehaviour
{

	[SerializeField] private ARCameraManager _cameraManager = null;

	private Light _Light = null;

	public float? Lumen { get; private set; }
	public float? ColorTemperature { get; private set; }
	public Color? ColorCorrection { get; private set; }

	private void X_FrameChanged(ARCameraFrameEventArgs args)
	{
		// Early out
		if (GameManager.Instance.CurrentMenu != GameManager.MenuMode.Interaction)
			return;

		// Licht ggf. aktivieren
		if (args.lightEstimation.averageIntensityInLumens.HasValue)
		{
			Lumen = args.lightEstimation.averageIntensityInLumens.Value;
			GameManager.Instance.SwitchTime(Lumen.Value);
			_Light.enabled = GameManager.Instance.CurrentLight == GameManager.LightTime.Day;
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

	private void Awake()
	{
		_Light = GetComponent<Light>();
	}

	private void OnEnable()
	{
		if (_cameraManager != null)
			_cameraManager.frameReceived += X_FrameChanged;
	}

	private void OnDisable()
	{
		if (_cameraManager != null)
			_cameraManager.frameReceived -= X_FrameChanged;
	}

}
