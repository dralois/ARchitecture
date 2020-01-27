using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshRenderer))]
public class CastShadowSwitcher : MonoBehaviour
{

	MeshRenderer _meshRender = null;

	private void Awake()
	{
		// Renderer cachen
		_meshRender = GetComponent<MeshRenderer>();
		// Event abonnieren
		RenderPipelineManager.beginCameraRendering += BeginRender;
	}

	private void BeginRender(ScriptableRenderContext ctx, Camera cam)
	{
		// Bei Shadow Camera Schattenwurf des Renderers deaktivieren
		if (cam.CompareTag("ShadowCam"))
		{
			_meshRender.shadowCastingMode = ShadowCastingMode.On;
		}
		else
		{
			_meshRender.shadowCastingMode = ShadowCastingMode.Off;
		}
	}

	private void OnDestroy()
	{
		// Event entfernen
		RenderPipelineManager.beginCameraRendering -= BeginRender;
	}
}
