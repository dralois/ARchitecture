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
			if (_raycastManager.Raycast(finger.screenPosition, _hits, TrackableType.PlaneWithinPolygon))
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
		if (GameManager.Instance.CurrentMenu == GameManager.MenuMode.Spawn &&
			GameManager.Instance.SelectedPlacementMode == GameManager.PlacementMode.QR)
		{
			ARTrackedImage qrCode = null;
			// Entweder erstes hinzugefuegtes oder geupdatetes Bild nehmen
			if (images.added.Count > 0)
			{
				qrCode = images.added[0];
			}
			else if (images.updated.Count > 0)
			{
				qrCode = images.updated[0];
			}
			else
			{
				return;
			}
			// Falls das Tracking gut ist
			if (qrCode.trackingState == TrackingState.Tracking)
			{
				//// Pose erstellen (In Local Space)
				//Pose spawnPose = new Pose(qrCode.transform.localPosition, qrCode.transform.localRotation);
				//// In World Space Transformieren
				//spawnPose = _session.trackablesParent.TransformPose(spawnPose);
				//// Spawnen
				//X_SpawnIFC(spawnPose);
				// IFC aktivieren
				GameManager.Instance.PlacedIFC.SetActive(true);
				// Parent setzen
				GameManager.Instance.PlacedIFC.transform.SetParent(qrCode.transform, false);
				// Modus wechseln
				GameManager.Instance.SwitchMenu(GameManager.MenuMode.Placement);
				// Detection deaktivieren
				_planeManager.subsystem?.Stop();
				_raycastManager.subsystem?.Stop();
				// Script deaktivieren
				enabled = false;
			}
		}
	}

	private void X_SpawnIFC(Pose spawnPose)
	{
		// IFC aktivieren
		GameManager.Instance.PlacedIFC.SetActive(true);
		// Platzieren
#if UNITY_EDITOR || UNITY_STANDALONE
		var newObj = new GameObject("Parent");
		newObj.transform.SetPositionAndRotation(spawnPose.position, spawnPose.rotation);
		GameManager.Instance.PlacedIFC.transform.SetParent(newObj.transform, false);
#else
		// Anchor erstellen und damit platzieren
		var anchor = _anchorManager.AddAnchor(spawnPose);
		GameManager.Instance.PlacedIFC.transform.SetParent(anchor.transform, false);
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

	private void X_ModeSelected(GameManager.PlacementMode mode)
	{
		// Je nach Modus
		switch (mode)
		{
			case GameManager.PlacementMode.None:
				{
					// Das sollte niemals passieren
					Application.Quit();
					break;
				}
			case GameManager.PlacementMode.Free:
				{
					// Manager aktivieren
					_planeManager.subsystem?.Start();
					_raycastManager.subsystem?.Start();
					break;
				}
			case GameManager.PlacementMode.QR:
				{
					// Manager aktivieren
					_imageManager.subsystem?.Start();
					break;
				}
		}
	}

	private void Awake()
	{
		// Cachen
		_imageManager = GetComponent<ARTrackedImageManager>();
		_raycastManager = GetComponent<ARRaycastManager>();
		_anchorManager = GetComponent<ARAnchorManager>();
		_planeManager = GetComponent<ARPlaneManager>();
		_session = GetComponent<ARSessionOrigin>();
		// Manager zunaechst deaktivieren
		_raycastManager.subsystem?.Stop();
		_planeManager.subsystem?.Stop();
		_imageManager.subsystem?.Stop();
		// IFC instantiieren
		GameManager.Instance.PlacedIFC = Instantiate(_placedPrefab);
		GameManager.Instance.PlacedIFC.SetActive(false);
		// Event abonnieren
		GameManager.Instance.PlacementModeSelected += X_ModeSelected;
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

	private void OnDestroy()
	{
		// Event entfernen
		GameManager.Instance.PlacementModeSelected -= X_ModeSelected;
	}

}