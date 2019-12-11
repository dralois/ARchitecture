using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;

[RequireComponent(typeof(ARRaycastManager))]
public class PlaceOnPlane : MonoBehaviour
{
	[SerializeField] private GameObject m_PlacedPrefab;

	private ARRaycastManager m_RaycastManager;
	private static List<ARRaycastHit> s_Hits = new List<ARRaycastHit>();

	public GameObject spawnedObject { get; private set; }

	void Awake()
	{
		m_RaycastManager = GetComponent<ARRaycastManager>();
#if !UNITY_EDITOR
		EnhancedTouchSupport.Enable();
		UnityEngine.InputSystem.EnhancedTouch.Touch.onFingerDown += TouchStarted;
#endif
	}

	private void TouchStarted(Finger finger)
	{
		if(finger.currentTouch.phase == UnityEngine.InputSystem.TouchPhase.Began)
		{
			if (m_RaycastManager.Raycast(finger.screenPosition, s_Hits, TrackableType.PlaneWithinPolygon))
			{
				var hitPose = s_Hits[0].pose;

				if (spawnedObject == null)
				{
					spawnedObject = Instantiate(m_PlacedPrefab, hitPose.position, hitPose.rotation);
				}
				else
				{
					spawnedObject.transform.position = hitPose.position;
				}
			}
		}
	}
}
