using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
	[SerializeField] private GameObject _placedPrefab = null;
	[SerializeField] private LayerMask _hitMask = 0;

	private GameObject _spawnedObject;
	private ARRaycastManager _RaycastManager;
	private List<ARRaycastHit> _Hits = new List<ARRaycastHit>();

	private void X_TouchStarted(Finger finger)
	{
		if(finger.index == 0 && GameManager.Instance.CurrentMode == GameManager.Mode.Spawn)
		{
#if UNITY_EDITOR
			var screenRay = Camera.main.ScreenPointToRay(finger.screenPosition);
			if (Physics.Raycast(screenRay, out RaycastHit hit, Mathf.Infinity, _hitMask))
#else
			if (_RaycastManager.Raycast(finger.screenPosition, _Hits, TrackableType.PlaneWithinPolygon))
#endif
			{
#if UNITY_EDITOR
				Pose hitPose = new Pose(hit.point, Quaternion.identity);
#else
				Pose hitPose = _Hits[0].pose;
#endif
				// Haus spawnen und speichern
				GameManager.Instance.House = Instantiate(_placedPrefab, hitPose.position, hitPose.rotation);
				GameManager.Instance.SwitchMode(GameManager.Mode.Placement);
			}
		}
	}

	private void Awake()
	{
		_RaycastManager = GetComponent<ARRaycastManager>();
		// ggf. Touch aktivieren
		if (!EnhancedTouchSupport.enabled)
		{
			EnhancedTouchSupport.Enable();
		}
	}

	private void OnEnable()
	{
		UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += X_TouchStarted;
	}

	private void OnDisable()
	{
		UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= X_TouchStarted;
	}

}
