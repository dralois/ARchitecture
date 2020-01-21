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

	private void X_FingerDown(Finger finger)
	{
		// Early out
		if (GameManager.Instance.CurrentMode != GameManager.InputMode.Interaction || !_canSpawnDesc)
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
			if (hit.transform.TryGetComponent(out _explodable))
			{
				_explodable.EnterExplosionMode(hit.normal);
				_lastNormal = hit.normal;
			}
		}
		else
		{
			// ggf. alte GUI loeschen
			if (_descSpawned)
				_descSpawned.gameObject.SetActive(false);
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
		// Events abonnieren
		Touch.onFingerDown += X_FingerDown;
		GameManager.Instance.ModeChanged += ModeChange;
	}

	private void ModeChange(GameManager.InputMode mode)
	{
		// Beim ersten mal hooken
		if (mode == GameManager.InputMode.Interaction && !_delegateHooked)
		{
			GameManager.Instance.Visualizer.VisualizationChanged += VisualizationChange;
			_delegateHooked = true;
		}
	}

	private void VisualizationChange(VisualizationSwitcher.Visualization mode)
	{
		if(mode == VisualizationSwitcher.Visualization.Normal)
		{
			// Explosion & GUI reaktivieren
			if (_descSpawned)
				_descSpawned.gameObject.SetActive(true);
			_explodable?.EnterExplosionMode(_lastNormal);
			_canSpawnDesc = true;
		}
		else
		{
			// Explosion & GUI deaktivieren
			if (_descSpawned)
				_descSpawned.gameObject.SetActive(false);
			_explodable?.ExitExplosionMode();
			_canSpawnDesc = false;
		}
	}

	private void OnDestroy()
	{
		// Events entfernen
		Touch.onFingerDown -= X_FingerDown;
		GameManager.Instance.ModeChanged -= ModeChange;
		if(GameManager.Instance.Visualizer)
			GameManager.Instance.Visualizer.VisualizationChanged -= VisualizationChange;
	}

}
