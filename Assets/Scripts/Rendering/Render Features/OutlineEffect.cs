using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class OutlineEffect : ScriptableRendererFeature
{

	[System.Serializable]
	public class OutlineEffectSettings
	{
		public Material effectMaterial = null;
	}

	public OutlineEffectSettings settings = new OutlineEffectSettings();

	private OutlineEffectPass _outlinePass;

	public override void Create()
	{
		// Pass erstellen, laueft zum Schluss
		_outlinePass = new OutlineEffectPass(settings.effectMaterial);
		_outlinePass.renderPassEvent = RenderPassEvent.AfterRendering;
	}

	public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
	{
		// Dem Renderer hinzufuegen
		renderer.EnqueuePass(_outlinePass);
	}
}

public class OutlineEffectPass : ScriptableRenderPass
{

	private static readonly float[] _gaussKernel = { 0.09672f, 0.240449f, 0.325661f, 0.240449f, 0.09672f };
	private static readonly Matrix4x4 _BackgroundOrthoProjection = Matrix4x4.Ortho(0f, 1f, 0f, 1f, -0.1f, 9.9f);
	private static Mesh _fullscreenMesh = null;

	private Material _effectMaterial = null;

	private static Mesh FullscreenMesh
	{
		get
		{
			if (_fullscreenMesh != null)
				return _fullscreenMesh;
			// Mesh erstellen
			_fullscreenMesh = new Mesh { name = "Fullscreen Quad" };
			// Vertices setzen
			_fullscreenMesh.vertices = new Vector3[]
			{
				new Vector3(0f, 0f, 0.1f),
				new Vector3(0f, 1f, 0.1f),
				new Vector3(1f, 1f, 0.1f),
				new Vector3(1f, 0f, 0.1f),
			};
			// UVs setzen
			_fullscreenMesh.uv = new Vector2[]
			{
				new Vector2(0f, 0f),
				new Vector2(0f, 1f),
				new Vector2(1f, 1f),
				new Vector2(1f, 0f),
			};
			// Upload
			_fullscreenMesh.SetIndices(new int[] { 0, 1, 2, 0, 2, 3 }, MeshTopology.Triangles, 0, false);
			_fullscreenMesh.UploadMeshData(true);
			return _fullscreenMesh;
		}
	}

	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		Camera usedCam = renderingData.cameraData.camera;
		// Buffer holen
		CommandBuffer cmdBuff = CommandBufferPool.Get("Outline Effect");
		cmdBuff.BeginSample("Outline Effect");
		// Kernel etc. speichern
		_effectMaterial.SetFloatArray("_kernel", _gaussKernel);
		_effectMaterial.SetInt("_kernelHalfWidth", _gaussKernel.Length / 2);
		// Kamera auf Orthografisch umstellen
		cmdBuff.SetViewProjectionMatrices(Matrix4x4.identity, _BackgroundOrthoProjection);
		// Rendern
		cmdBuff.DrawMesh(FullscreenMesh, Matrix4x4.identity, _effectMaterial, 0, 1);
		// Perspektivisch wiederherstellen
		cmdBuff.SetViewProjectionMatrices(usedCam.worldToCameraMatrix, usedCam.projectionMatrix);
		cmdBuff.EndSample("Outline Effect");
		// Ausfuehren
		context.ExecuteCommandBuffer(cmdBuff);
		// Buffer freigeben
		CommandBufferPool.Release(cmdBuff);
	}

	public OutlineEffectPass(Material effectMat)
	{
		_effectMaterial = effectMat;
	}
}
