using UnityEngine;
using TMPro;

public class DescriptionSpawner : MonoBehaviour
{

	[SerializeField] private Transform _anchorPoint = null;
	[SerializeField] private float _offsetMultiplier = 0.5f;
	[SerializeField] private TextMeshProUGUI _title = null;
	[SerializeField] private TextMeshProUGUI _desc = null;

	private LineRenderer _pointerRender = null;
	private Canvas _worldCanvas = null;

	public void CreateReference(Vector3 refPos, Vector3 refNormal)
	{
		transform.position = refPos + new Vector3(refNormal.x, 1f, refNormal.z) * _offsetMultiplier;
		transform.rotation = Mathf.Abs(refNormal.y) < 1f ? Quaternion.LookRotation(new Vector3(-refNormal.x, 0, -refNormal.z)) : Quaternion.identity;
		_pointerRender.positionCount = 2;
		_pointerRender.SetPosition(0, refPos);
		_pointerRender.SetPosition(1, _anchorPoint.position);
	}

	public void FillDescription(string title, string description)
	{
		_title.text = title;
		_desc.text = description;
	}

	private void Awake()
	{
		_pointerRender = GetComponent<LineRenderer>();
		_worldCanvas = GetComponent<Canvas>();
		_worldCanvas.worldCamera = Camera.main;
	}

	private void Update()
	{
		var camFwd = (transform.position - _worldCanvas.worldCamera.transform.position).normalized;
		transform.rotation = Quaternion.LookRotation(new Vector3(camFwd.x, 0, camFwd.z));
	}

}
