using UnityEngine;
using UnityEngine.Rendering.Universal;

public class CameraController : MonoBehaviour
{

	public enum Visualization
	{
		Normal,
		Ghosted
	}

	[SerializeField] private float _maxCutPlane = 5.0f;

	private Visualization _currVis = Visualization.Normal;
	private UniversalAdditionalCameraData _cameraData = null;
	private float _nearDefault = 0f;
	private Camera _camera = null;

	public event System.Action<Visualization> VisualizationChanged;

	/// <summary>
	/// Schnittebene setzen
	/// </summary>
	/// <param name="percent">0-1 (ca. 0-5m)</param>
	public void SetCutPlane(float percent)
	{
		// Near Plane verschieben
		_camera.nearClipPlane = Mathf.Lerp(_nearDefault, _maxCutPlane, Mathf.Clamp01(percent));
	}

	public float GetCutPlane()
	{
		return _camera.nearClipPlane / _maxCutPlane;
	}

	public void SetVisualization(Visualization newVis)
	{
		// Abbruch falls nicht initialisiert
		if (!_cameraData)
			return;
		// Renderer setzen
		_cameraData.SetRenderer((int) newVis);
		// Event ausloesen
		if (_currVis != newVis)
			VisualizationChanged?.Invoke(newVis);
		// Speichern
		_currVis = newVis;
	}

	public Visualization GetVisualization()
	{
		return _currVis;
	}

	private void Start()
	{
		_camera = GetComponent<Camera>();
		_nearDefault = _camera.nearClipPlane;
		_cameraData = GetComponent<UniversalAdditionalCameraData>();
		// Controller im GameManager speichern
		GameManager.Instance.CameraController = this;
	}

}
