using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

[RequireComponent(typeof(ARRaycastManager), typeof(ARTrackedImageManager))]
[RequireComponent(typeof(ARPlaneManager), typeof(ARSessionOrigin), typeof(ARAnchorManager))]
public class ARSpawnManager : MonoBehaviour
{
	[SerializeField] private GameObject _placedPrefab = null;
	[SerializeField] private LayerMask _hitMask = 0;

	private List<ARRaycastHit> _hits = new List<ARRaycastHit>();
	private ARTrackedImageManager _imageManager;
	private ARRaycastManager _raycastManager;
	private ARAnchorManager _anchorManager;
	private ARPlaneManager _planeManager;
	private ARSessionOrigin _session;

	private void X_TouchStarted(Finger finger)
	{
		// Sanity Check
		if (finger.index == 0 && GameManager.Instance.CurrentMenu == GameManager.MenuMode.Spawn &&
				GameManager.Instance.SelectedPlacementMode == GameManager.PlacementMode.Free)
		{
#if UNITY_EDITOR || UNITY_STANDALONE
			// Ray bestimmen
			var screenRay = Camera.main.ScreenPointToRay(finger.screenPosition);
			// Normaler Raycast
			if (Physics.Raycast(screenRay, out RaycastHit hit, Mathf.Infinity, _hitMask))
#else
			// AR Raycast
			if (_RaycastManager.Raycast(finger.screenPosition, _hits, TrackableType.PlaneWithinPolygon))
#endif
			{
#if UNITY_EDITOR || UNITY_STANDALONE
				Pose hitPose = new Pose(hit.point, Quaternion.identity);
#else
				Pose hitPose = _hits[0].pose;
#endif
				// Spawnen
				X_SpawnIFC(hitPose);
			}
		}
	}

	private void X_ImageDetected(ARTrackedImagesChangedEventArgs images)
	{
		// Sanity Check
		if(GameManager.Instance.CurrentMenu == GameManager.MenuMode.Spawn &&
			GameManager.Instance.SelectedPlacementMode == GameManager.PlacementMode.QR)
		{
			ARTrackedImage qrCode = null;
			// Entweder erstes hinzugefuegtes oder geupdatetes Bild nehmen
			if(images.added.Count > 0)
			{
				qrCode = images.added[0];
			}
			else if(images.updated.Count > 0)
			{
				qrCode = images.updated[0];
			}
			else
			{
				return;
			}
			// Falls das Tracking gut ist
			if(qrCode.trackingState == TrackingState.Tracking)
			{
				// Pose erstellen (In Local Space)
				Pose spawnPose = new Pose(qrCode.transform.localPosition, qrCode.transform.localRotation);
				// In World Space Transformieren
				spawnPose = _session.trackablesParent.TransformPose(spawnPose);
				// Spawnen
				X_SpawnIFC(spawnPose);
			}
		}
	}

	private void X_SpawnIFC(Pose spawnPose)
	{
		// IFC aktivieren
		GameManager.Instance.PlacedIFC.SetActive(true);
		// Platzieren
#if UNITY_EDITOR || UNITY_STANDALONE
		GameManager.Instance.PlacedIFC.transform.SetPositionAndRotation(spawnPose.position, spawnPose.rotation);
#else
		// Anchor erstellen und damit platzieren
		var anchor = _anchorManager.AddAnchor(spawnPose);
		GameManager.Instance.PlacedIFC.transform.SetParent(anchor.transform);
#endif
		// Modus wechseln
		GameManager.Instance.SwitchMenu(GameManager.MenuMode.Placement);
		// Detection deaktivieren
		_planeManager.subsystem?.Stop();
		_raycastManager.subsystem?.Stop();
		_imageManager.subsystem?.Stop();
		// Script deaktivieren
		enabled = false;
	}

	private void Awake()
	{
		// Cachen
		_imageManager = GetComponent<ARTrackedImageManager>();
		_raycastManager = GetComponent<ARRaycastManager>();
		_anchorManager = GetComponent<ARAnchorManager>();
		_planeManager = GetComponent<ARPlaneManager>();
		_session = GetComponent<ARSessionOrigin>();
		// IFC instantiieren
		GameManager.Instance.PlacedIFC = Instantiate(_placedPrefab);
		GameManager.Instance.PlacedIFC.SetActive(false);
		// ggf. Touch aktivieren
		if (!EnhancedTouchSupport.enabled)
		{
			EnhancedTouchSupport.Enable();
		}
	}

	private void OnEnable()
	{
		Touch.onFingerDown += X_TouchStarted;
		_imageManager.trackedImagesChanged += X_ImageDetected;
	}

	private void OnDisable()
	{
		Touch.onFingerDown -= X_TouchStarted;
		_imageManager.trackedImagesChanged -= X_ImageDetected;
	}

}