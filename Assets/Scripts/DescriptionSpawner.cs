using UnityEngine;

public class DescriptionSpawner : MonoBehaviour
{

	[SerializeField] private float _offsetMultiplier;
	private LineRenderer _pointerRender = null;

	private void Awake()
	{
		_pointerRender = GetComponent<LineRenderer>();
		var worldCanvas = GetComponent<Canvas>();
		worldCanvas.worldCamera = Camera.main;
	}

	public void CreateReference(Vector3 refPos, Vector3 refNormal)
	{
		transform.position = refPos + refNormal * _offsetMultiplier;
		transform.rotation = Quaternion.LookRotation(-1f * refNormal);
		_pointerRender.positionCount = 2;
		_pointerRender.SetPosition(0, refPos);
		_pointerRender.SetPosition(1, transform.position);
	}
}
