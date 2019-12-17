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
		// ggf. Touch aktivieren
		if (!EnhancedTouchSupport.enabled)
		{
			EnhancedTouchSupport.Enable();
		}
		// Touch Event abonnieren
		Touch.onFingerDown += X_FingerDown;
	}

	private void X_FingerDown(Finger finger)
	{
		// Early out
		if (GameManager.Instance.CurrentMode != GameManager.InputMode.Interaction)
			return;
		// Mit Screen Ray ersten Hit bestimmen
		var screenRay = _ARCam.ScreenPointToRay(finger.screenPosition);
		if (Physics.Raycast(screenRay, out RaycastHit hit, Mathf.Infinity, _rayMask))
		{
			// ggf. alte GUI loeschen
			if (_descSpawned)
				Destroy(_descSpawned.gameObject);
			// Neue instantiieren und TODO: befuellen
			_descSpawned = Instantiate(_UIPrefab.gameObject).GetComponent<DescriptionSpawner>();
			_descSpawned.CreateReference(hit.point, hit.normal);
			//_descSpawned.FillDescription();
		}
	}
}
