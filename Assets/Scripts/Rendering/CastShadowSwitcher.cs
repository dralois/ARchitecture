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
		// Quick and dirty hack: Bei Shadow Camera Schattenwurf des Renderers deaktivieren
		if (cam.name != "Shadow Camera")
		{
			_meshRender.shadowCastingMode = ShadowCastingMode.Off;
		}
		else
		{
			_meshRender.shadowCastingMode = ShadowCastingMode.On;
		}
	}

	private void OnDestroy()
	{
		// Event entfernen
		RenderPipelineManager.beginCameraRendering -= BeginRender;
	}
}
