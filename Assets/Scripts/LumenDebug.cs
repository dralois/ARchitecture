using UnityEngine.XR.ARFoundation;
using UnityEngine;
using TMPro;

public class LumenDebug : MonoBehaviour
{

	[SerializeField] private ARCameraManager _CameraManager;
	[SerializeField] private TextMeshProUGUI _lumenText;

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
			_lumenText.text = $"{args.lightEstimation.averageIntensityInLumens.Value}";
		}
	}

}
