using UnityEngine;

public class EstimationSetter : MonoBehaviour
{
	private void Awake()
	{
		var manager = FindObjectOfType<UnityEngine.XR.ARFoundation.ARCameraManager>();

		foreach (LightEstimation curr in GetComponentsInChildren<LightEstimation>())
		{
			curr.CameraManager = manager;
			curr.enabled = true;
		}
	}
}
