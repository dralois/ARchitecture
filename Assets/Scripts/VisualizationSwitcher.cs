using UnityEngine;
using UnityEngine.Rendering.Universal;

public class VisualizationSwitcher : MonoBehaviour
{

	public enum Visualization
	{
		Normal,
		Ghosted
	}

	private UniversalAdditionalCameraData _cameraData = null;

	public Visualization CurrentVisualization { get; private set; } = Visualization.Normal;

	public event System.Action<Visualization> VisualizationChanged;

	public void ChangeVisualization(Visualization newVis)
	{
		// Abbruch falls nicht initialisiert
		if (!_cameraData)
			return;
		// Renderer setzen
		_cameraData.SetRenderer((int) newVis);
		// Event ausloesen
		if (CurrentVisualization != newVis)
			VisualizationChanged?.Invoke(newVis);
		// Speichern
		CurrentVisualization = newVis;
	}

	private void Start()
	{
		_cameraData = GetComponent<UniversalAdditionalCameraData>();
		GameManager.Instance.Visualizer = this;
	}

}
