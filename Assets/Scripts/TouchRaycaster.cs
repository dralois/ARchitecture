using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine;

public class TouchRaycaster : MonoBehaviour
{

	[SerializeField] private DescriptionSpawner _UIPrefab = null;
	[SerializeField] private LayerMask _rayMask = 0;

	private DescriptionSpawner _descSpawned = null;
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
		if(Physics.Raycast(screenRay, out RaycastHit hit, Mathf.Infinity, _rayMask))
		{
			if (_descSpawned)
				Destroy(_descSpawned.gameObject);
			_descSpawned = Instantiate(_UIPrefab.gameObject).GetComponent<DescriptionSpawner>();
			_descSpawned.CreateReference(hit.point, hit.normal);
		}
	}
}
