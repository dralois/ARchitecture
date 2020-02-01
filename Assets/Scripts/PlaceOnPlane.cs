using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using System;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
	[SerializeField] private GameObject _placedPrefab = null;
	[SerializeField] private LayerMask _hitMask = 0;

	private List<ARRaycastHit> _Hits = new List<ARRaycastHit>();
	private ARRaycastManager _RaycastManager;
	private GameObject _spawnedObject;
	public static Vector3 originalScale;
    public static float originalRot;

    private void X_TouchStarted(Finger finger)
	{
		if (finger.index == 0 && GameManager.Instance.CurrentMenu == GameManager.MenuMode.Spawn)
		{
#if UNITY_EDITOR || UNITY_STANDALONE
			var screenRay = Camera.main.ScreenPointToRay(finger.screenPosition);
			if (Physics.Raycast(screenRay, out RaycastHit hit, Mathf.Infinity, _hitMask))
#else
			if (_RaycastManager.Raycast(finger.screenPosition, _Hits, TrackableType.PlaneWithinPolygon))
#endif
			{
#if UNITY_EDITOR || UNITY_STANDALONE
				Pose hitPose = new Pose(hit.point, Quaternion.identity);
#else
				Pose hitPose = _Hits[0].pose;
#endif
				// Haus spawnen und speichern
				GameManager.Instance.PlacedIFC = Instantiate(_placedPrefab, hitPose.position + new Vector3(0f, 0.0001f, 0f), hitPose.rotation);
				GameManager.Instance.SwitchMenu(GameManager.MenuMode.Placement);
				originalScale = GameManager.Instance.PlacedIFC.transform.localScale;
                originalRot = GameManager.Instance.PlacedIFC.transform.localRotation.y;
				// Detection deaktivieren
				GetComponent<ARPlaneManager>().detectionMode = PlaneDetectionMode.None;
				GetComponent<ARTrackedObjectManager>().referenceLibrary = null;
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
		Touch.onFingerDown += X_TouchStarted;
	}

	private void OnDisable()
	{
		Touch.onFingerDown -= X_TouchStarted;
	}
    
}
