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

	private bool _delegateHooked = false;
	private bool _canSpawnDesc = true;
	private Vector3 _lastNormal = Vector3.zero;
	public GameObject _lastHit = null;

	private GameObject playerCanvas = null;

	private void X_FingerDown(Finger finger)
	{
		// Early out
		if (GameManager.Instance.CurrentMenu != GameManager.MenuMode.Interaction || !_canSpawnDesc)
			return;
		// Mit Screen Ray ersten Hit bestimmen
		var screenRay = _ARCam.ScreenPointToRay(finger.screenPosition);
		if (Physics.Raycast(screenRay, out RaycastHit hit, Mathf.Infinity, _tridifyMask))
		{
			// Layer anpassen
			if (_lastHit)
				_lastHit.layer = LayerMask.NameToLayer("Tridify");
			// GameObjekt des Hits holen
			_lastHit = hit.transform.gameObject;
			#region RemoveMe
			// ggf. alte GUI loeschen
			if (_descSpawned)
				Destroy(_descSpawned.gameObject);
			// Neue instantiieren und befuellen
			_descSpawned = Instantiate(_UIPrefab.gameObject).GetComponent<DescriptionSpawner>();
			_descSpawned.CreateReference(hit.point, hit.normal);
			_descSpawned.FillDescription(TridifyQuery.GetTitle(_lastHit),
										TridifyQuery.GetDescription(_lastHit));
			#endregion
			// Layer anpassen
			_lastHit.layer = LayerMask.NameToLayer("Outline");
			// Description setzen
			UIHandlerNew.ShowDescription(TridifyQuery.GetTitle(_lastHit),
										TridifyQuery.GetDescription(_lastHit));
			// Vorheriges Explodable einklappen
			_explodable?.ExitExplosionMode();
			// ggf. Explodable ausklappen
			if (hit.transform.TryGetComponent(out _explodable))
			{
				_explodable.EnterExplosionMode(hit.normal);
				_lastNormal = hit.normal;
			}
		}
		else
		{
			#region RemoveMe
			// ggf. alte GUI loeschen
			if (_descSpawned)
				Destroy(_descSpawned.gameObject);
			#endregion
			// Vorheriges Explodable einklappen
			_explodable?.ExitExplosionMode();
			// Layer anpassen
			if(_lastHit)
				_lastHit.layer = LayerMask.NameToLayer("Tridify");
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
		// Events abonnieren
		Touch.onFingerDown += X_FingerDown;
		GameManager.Instance.MenuChanged += ModeChange;
	}

	private void ModeChange(GameManager.MenuMode mode)
	{
		// Beim ersten mal hooken
		if (mode == GameManager.MenuMode.Interaction && !_delegateHooked)
		{
			GameManager.Instance.CameraController.VisualizationChanged += VisualizationChange;
			_delegateHooked = true;
		}
	}

	private void VisualizationChange(CameraController.Visualization mode)
	{
		if(mode != CameraController.Visualization.Normal)
		{
			#region RemoveMe
			// ggf. alte GUI loeschen
			if (_descSpawned)
				Destroy(_descSpawned.gameObject);
			#endregion
			_canSpawnDesc = false;
			// Explodable einklappen
			_explodable?.ExitExplosionMode();
			// Layer anpassen
			if (_lastHit)
				_lastHit.layer = LayerMask.NameToLayer("Tridify");
		}
	}

	private void OnDestroy()
	{
		// Events entfernen
		Touch.onFingerDown -= X_FingerDown;
		GameManager.Instance.MenuChanged -= ModeChange;
		if(GameManager.Instance.CameraController)
			GameManager.Instance.CameraController.VisualizationChanged -= VisualizationChange;
	}

}
