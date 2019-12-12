using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
	[SerializeField] private GameObject _PlacedPrefab;

	private bool _placingEnabled = true;
	private GameObject _spawnedObject;
	private ARRaycastManager _RaycastManager;
	private List<ARRaycastHit> _Hits = new List<ARRaycastHit>();

	private void Awake()
	{
		_RaycastManager = GetComponent<ARRaycastManager>();
#if !UNITY_EDITOR
		EnhancedTouchSupport.Enable();
#endif
	}

	private void OnEnable()
	{
#if !UNITY_EDITOR
		UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += TouchStarted;
#endif
	}

	private void OnDisable()
	{
#if !UNITY_EDITOR
		UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown -= TouchStarted;
#endif
	}

	public void SwitchPlacingState()
	{
		_placingEnabled = !_placingEnabled;
	}

	private void TouchStarted(Finger finger)
	{
		if(finger.currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Began &&
			_placingEnabled)
		{
			if (_RaycastManager.Raycast(finger.screenPosition, _Hits, TrackableType.PlaneWithinPolygon))
			{
				var hitPose = _Hits[0].pose;
				if (_spawnedObject == null)
				{
					_spawnedObject = Instantiate(_PlacedPrefab, hitPose.position, hitPose.rotation);
				}
				else
				{
					_spawnedObject.transform.position = hitPose.position;
				}
			}
		}
	}
}
