using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine;

public class TridifyInteractor : MonoBehaviour
{

	[SerializeField] private DescriptionSpawner _UIPrefab = null;
	[SerializeField] private LayerMask _tridifyMask = 0;

	private ExplodableComponent _explodable = null;
	private DescriptionSpawner _descSpawned = null;
	private Camera _ARCam = null;

	private void X_FingerDown(Finger finger)
	{
		// Early out
		if (GameManager.Instance.CurrentMode != GameManager.InputMode.Interaction)
			return;
		// Mit Screen Ray ersten Hit bestimmen
		var screenRay = _ARCam.ScreenPointToRay(finger.screenPosition);
		if (Physics.Raycast(screenRay, out RaycastHit hit, Mathf.Infinity, _tridifyMask))
		{
			// ggf. alte GUI loeschen
			if (_descSpawned)
				Destroy(_descSpawned.gameObject);
			// Neue instantiieren und TODO: befuellen
			_descSpawned = Instantiate(_UIPrefab.gameObject).GetComponent<DescriptionSpawner>();
			_descSpawned.CreateReference(hit.point, hit.normal);
			_descSpawned.FillDescription(TridifyQuery.GetTitle(hit.transform.gameObject),
																		TridifyQuery.GetDescription(hit.transform.gameObject));
			// Vorheriges Explodable einklappen
			_explodable?.ExitExplosionMode();
			// ggf. Explodable ausklappen
			if(hit.transform.TryGetComponent(out _explodable))
			{
				_explodable.EnterExplosionMode(hit.normal);
			}
		}
		else
		{
			// ggf. alte GUI loeschen
			if (_descSpawned)
				Destroy(_descSpawned.gameObject);
			// Vorheriges Explodable einklappen
			_explodable?.ExitExplosionMode();
		}
	}

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

	private void OnDestroy()
	{
		// Touch Event entfernen
		Touch.onFingerDown -= X_FingerDown;
	}

}
