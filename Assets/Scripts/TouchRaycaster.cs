using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine;

public class TouchRaycaster : MonoBehaviour
{

	[SerializeField] private DescriptionSpawner _UIPrefab = null;

	private Camera _ARCam = null;

	private void Awake()
	{
		_ARCam = Camera.main;
#if !UNITY_EDITOR
		EnhancedTouchSupport.Enable();
		Touch.onFingerDown += X_FingerDown;
#endif
	}

	private void X_FingerDown(Finger finger)
	{
		var screenRay = _ARCam.ScreenPointToRay(finger.screenPosition);
		if(Physics.Raycast(screenRay, out RaycastHit hit))
		{
			var desc = Instantiate(_UIPrefab.gameObject).GetComponent<DescriptionSpawner>();
			desc.CreateReference(hit.point, hit.normal);
		}
	}
}
