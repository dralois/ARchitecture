using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.Universal.Internal;

/// <summary>
/// Copy of default forward renderer
/// </summary>
public class ShadowRenderer : ScriptableRenderer
{
	const int k_DepthStencilBufferBits = 32;
	const string k_CreateCameraTextures = "Create Camera Texture";

	DepthOnlyPass m_DepthPrepass;
	MainLightShadowCasterPass m_MainLightShadowCasterPass;
	AdditionalLightsShadowCasterPass m_AdditionalLightsShadowCasterPass;
	ScreenSpaceShadowResolvePass m_ScreenSpaceShadowResolvePass;
	CopyDepthPass m_CopyDepthPass;
	CopyColorPass m_CopyColorPass;
	FinalBlitPass m_FinalBlitPass;
	CapturePassCopy m_CapturePass;

#if UNITY_EDITOR
	SceneViewDepthCopyPassCopy m_SceneViewDepthCopyPass;
#endif

	RenderTargetHandle m_ActiveCameraColorAttachment;
	RenderTargetHandle m_ActiveCameraDepthAttachment;
	RenderTargetHandle m_CameraColorAttachment;
	RenderTargetHandle m_CameraDepthAttachment;
	RenderTargetHandle m_DepthTexture;
	RenderTargetHandle m_OpaqueColor;

	ForwardLights m_ForwardLights;
	StencilState m_DefaultStencilState;

	public ShadowRenderer(ShadowRendererData data) : base(data)
	{
		Material blitMaterial = CoreUtils.CreateEngineMaterial(data.shaders.blitPS);
		Material copyDepthMaterial = CoreUtils.CreateEngineMaterial(data.shaders.copyDepthPS);
		Material samplingMaterial = CoreUtils.CreateEngineMaterial(data.shaders.samplingPS);
		Material screenspaceShadowsMaterial = CoreUtils.CreateEngineMaterial(data.shaders.screenSpaceShadowPS);

		StencilStateData stencilData = data.defaultStencilState;
		m_DefaultStencilState = StencilState.defaultValue;
		m_DefaultStencilState.enabled = stencilData.overrideStencilState;
		m_DefaultStencilState.SetCompareFunction(stencilData.stencilCompareFunction);
		m_DefaultStencilState.SetPassOperation(stencilData.passOperation);
		m_DefaultStencilState.SetFailOperation(stencilData.failOperation);
		m_DefaultStencilState.SetZFailOperation(stencilData.zFailOperation);

		// Note: Since all custom render passes inject first and we have stable sort,
		// we inject the builtin passes in the before events.
		m_MainLightShadowCasterPass = new MainLightShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
		m_AdditionalLightsShadowCasterPass = new AdditionalLightsShadowCasterPass(RenderPassEvent.BeforeRenderingShadows);
		m_DepthPrepass = new DepthOnlyPass(RenderPassEvent.BeforeRenderingPrepasses, RenderQueueRange.opaque, LayerMask.GetMask("AR", "Tridify", "Outline"));
		m_ScreenSpaceShadowResolvePass = new ScreenSpaceShadowResolvePass(RenderPassEvent.BeforeRenderingPrepasses, screenspaceShadowsMaterial);
		m_CopyDepthPass = new CopyDepthPass(RenderPassEvent.BeforeRenderingOpaques, copyDepthMaterial);
		m_CopyColorPass = new CopyColorPass(RenderPassEvent.BeforeRenderingTransparents, samplingMaterial);
		m_CapturePass = new CapturePassCopy(RenderPassEvent.AfterRendering);
		m_FinalBlitPass = new FinalBlitPass(RenderPassEvent.AfterRendering, blitMaterial);

#if UNITY_EDITOR
		m_SceneViewDepthCopyPass = new SceneViewDepthCopyPassCopy(RenderPassEvent.AfterRendering + 9, copyDepthMaterial);
#endif

		// RenderTexture format depends on camera and pipeline (HDR, non HDR, etc)
		// Samples (MSAA) depend on camera and pipeline
		m_CameraColorAttachment.Init("_CameraColorTexture");
		m_CameraDepthAttachment.Init("_CameraDepthAttachment");
		m_DepthTexture.Init("_CameraDepthTexture");
		m_OpaqueColor.Init("_CameraOpaqueTexture");
		m_ForwardLights = new ForwardLights();
	}

	/// <inheritdoc />
	public override void Setup(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		Camera camera = renderingData.cameraData.camera;
		ref CameraData cameraData = ref renderingData.cameraData;
		RenderTextureDescriptor cameraTargetDescriptor = renderingData.cameraData.cameraTargetDescriptor;

		// Special path for depth only offscreen cameras. Only write opaques + transparents. 
		bool isOffscreenDepthTexture = camera.targetTexture != null && camera.targetTexture.format == RenderTextureFormat.Depth;
		if (isOffscreenDepthTexture)
		{
			ConfigureCameraTarget(BuiltinRenderTextureType.CameraTarget, BuiltinRenderTextureType.CameraTarget);

			for (int i = 0; i < rendererFeatures.Count; ++i)
				rendererFeatures[i].AddRenderPasses(this, ref renderingData);

			return;
		}

		bool mainLightShadows = m_MainLightShadowCasterPass.Setup(ref renderingData);
		bool additionalLightShadows = m_AdditionalLightsShadowCasterPass.Setup(ref renderingData);
		bool resolveShadowsInScreenSpace = mainLightShadows && renderingData.shadowData.requiresScreenSpaceShadowResolve;

		// Depth prepass is generated in the following cases:
		// - We resolve shadows in screen space
		// - Scene view camera always requires a depth texture. We do a depth pre-pass to simplify it and it shouldn't matter much for editor.
		// - If game or offscreen camera requires it we check if we can copy the depth from the rendering opaques pass and use that instead.
		bool requiresDepthPrepass = renderingData.cameraData.isSceneViewCamera ||
				(cameraData.requiresDepthTexture && (!CanCopyDepth(ref renderingData.cameraData)));
		requiresDepthPrepass |= resolveShadowsInScreenSpace;

		bool createColorTexture = RequiresIntermediateColorTexture(ref renderingData, cameraTargetDescriptor)
															|| rendererFeatures.Count != 0;

		// If camera requires depth and there's no depth pre-pass we create a depth texture that can be read
		// later by effect requiring it.
		bool createDepthTexture = cameraData.requiresDepthTexture && !requiresDepthPrepass;

		m_ActiveCameraColorAttachment = (createColorTexture) ? m_CameraColorAttachment : RenderTargetHandle.CameraTarget;
		m_ActiveCameraDepthAttachment = (createDepthTexture) ? m_CameraDepthAttachment : RenderTargetHandle.CameraTarget;
		bool intermediateRenderTexture = createColorTexture || createDepthTexture;

		if (intermediateRenderTexture)
			CreateCameraRenderTarget(context, ref cameraData);

		ConfigureCameraTarget(m_ActiveCameraColorAttachment.Identifier(), m_ActiveCameraDepthAttachment.Identifier());

		for (int i = 0; i < rendererFeatures.Count; ++i)
		{
			rendererFeatures[i].AddRenderPasses(this, ref renderingData);
		}

		int count = activeRenderPassQueue.Count;
		for (int i = count - 1; i >= 0; i--)
		{
			if (activeRenderPassQueue[i] == null)
				activeRenderPassQueue.RemoveAt(i);
		}
		bool hasAfterRendering = activeRenderPassQueue.Find(x => x.renderPassEvent == RenderPassEvent.AfterRendering) != null;

		if (mainLightShadows)
			EnqueuePass(m_MainLightShadowCasterPass);

		if (additionalLightShadows)
			EnqueuePass(m_AdditionalLightsShadowCasterPass);

		if (requiresDepthPrepass)
		{
			m_DepthPrepass.Setup(cameraTargetDescriptor, m_DepthTexture);
			EnqueuePass(m_DepthPrepass);
		}

		if (resolveShadowsInScreenSpace)
		{
			m_ScreenSpaceShadowResolvePass.Setup(cameraTargetDescriptor);
			EnqueuePass(m_ScreenSpaceShadowResolvePass);
		}

		// If a depth texture was created we necessarily need to copy it, otherwise we could have render it to a renderbuffer
		if (createDepthTexture)
		{
			m_CopyDepthPass.Setup(m_ActiveCameraDepthAttachment, m_DepthTexture);
			EnqueuePass(m_CopyDepthPass);
		}

		if (renderingData.cameraData.requiresOpaqueTexture)
		{
			// TODO: Downsampling method should be store in the renderer isntead of in the asset.
			// We need to migrate this data to renderer. For now, we query the method in the active asset.
			Downsampling downsamplingMethod = UniversalRenderPipeline.asset.opaqueDownsampling;
			m_CopyColorPass.Setup(m_ActiveCameraColorAttachment.Identifier(), m_OpaqueColor, downsamplingMethod);
			EnqueuePass(m_CopyColorPass);
		}

		bool afterRenderExists = renderingData.cameraData.captureActions != null ||
														 hasAfterRendering;

		// if we have additional filters
		// we need to stay in a RT
		if (afterRenderExists)
		{
			//now blit into the final target
			if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
			{
				if (renderingData.cameraData.captureActions != null)
				{
					m_CapturePass.Setup(m_ActiveCameraColorAttachment);
					EnqueuePass(m_CapturePass);
				}
				m_FinalBlitPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment);
				EnqueuePass(m_FinalBlitPass);
			}
		}
		else
		{
			if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
			{
				m_FinalBlitPass.Setup(cameraTargetDescriptor, m_ActiveCameraColorAttachment);
				EnqueuePass(m_FinalBlitPass);
			}
		}

#if UNITY_EDITOR
		if (renderingData.cameraData.isSceneViewCamera)
		{
			m_SceneViewDepthCopyPass.Setup(m_DepthTexture);
			EnqueuePass(m_SceneViewDepthCopyPass);
		}
#endif
	}

	/// <inheritdoc />
	public override void SetupLights(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		m_ForwardLights.Setup(context, ref renderingData);
	}

	/// <inheritdoc />
	public override void SetupCullingParameters(ref ScriptableCullingParameters cullingParameters,
			ref CameraData cameraData)
	{
		Camera camera = cameraData.camera;

		// If shadow is disabled, disable shadow caster culling
		if (Mathf.Approximately(cameraData.maxShadowDistance, 0.0f))
			cullingParameters.cullingOptions &= ~CullingOptions.ShadowCasters;

		cullingParameters.shadowDistance = cameraData.maxShadowDistance;
	}

	/// <inheritdoc />
	public override void FinishRendering(CommandBuffer cmd)
	{
		if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
			cmd.ReleaseTemporaryRT(m_ActiveCameraColorAttachment.id);

		if (m_ActiveCameraDepthAttachment != RenderTargetHandle.CameraTarget)
			cmd.ReleaseTemporaryRT(m_ActiveCameraDepthAttachment.id);
	}

	void CreateCameraRenderTarget(ScriptableRenderContext context, ref CameraData cameraData)
	{
		CommandBuffer cmd = CommandBufferPool.Get(k_CreateCameraTextures);
		var descriptor = cameraData.cameraTargetDescriptor;
		int msaaSamples = descriptor.msaaSamples;
		if (m_ActiveCameraColorAttachment != RenderTargetHandle.CameraTarget)
		{
			bool useDepthRenderBuffer = m_ActiveCameraDepthAttachment == RenderTargetHandle.CameraTarget;
			var colorDescriptor = descriptor;
			colorDescriptor.depthBufferBits = (useDepthRenderBuffer) ? k_DepthStencilBufferBits : 0;
			cmd.GetTemporaryRT(m_ActiveCameraColorAttachment.id, colorDescriptor, FilterMode.Bilinear);
		}

		if (m_ActiveCameraDepthAttachment != RenderTargetHandle.CameraTarget)
		{
			var depthDescriptor = descriptor;
			depthDescriptor.colorFormat = RenderTextureFormat.Depth;
			depthDescriptor.depthBufferBits = k_DepthStencilBufferBits;
			depthDescriptor.bindMS = msaaSamples > 1 && !SystemInfo.supportsMultisampleAutoResolve && (SystemInfo.supportsMultisampledTextures != 0);
			cmd.GetTemporaryRT(m_ActiveCameraDepthAttachment.id, depthDescriptor, FilterMode.Point);
		}

		context.ExecuteCommandBuffer(cmd);
		CommandBufferPool.Release(cmd);
	}

	bool RequiresIntermediateColorTexture(ref RenderingData renderingData, RenderTextureDescriptor baseDescriptor)
	{
		ref CameraData cameraData = ref renderingData.cameraData;
		int msaaSamples = cameraData.cameraTargetDescriptor.msaaSamples;
		bool isStereoEnabled = renderingData.cameraData.isStereoEnabled;
		bool isScaledRender = !Mathf.Approximately(cameraData.renderScale, 1.0f);
		bool isCompatibleBackbufferTextureDimension = baseDescriptor.dimension == TextureDimension.Tex2D;
		bool requiresExplicitMsaaResolve = msaaSamples > 1 && !SystemInfo.supportsMultisampleAutoResolve;
		bool isOffscreenRender = cameraData.camera.targetTexture != null && !cameraData.isSceneViewCamera;
		bool isCapturing = cameraData.captureActions != null;

#if ENABLE_VR && ENABLE_VR_MODULE
            if (isStereoEnabled)
                isCompatibleBackbufferTextureDimension = UnityEngine.XR.XRSettings.deviceEyeTextureDimension == baseDescriptor.dimension;
#endif

		bool requiresBlitForOffscreenCamera = cameraData.postProcessEnabled || cameraData.requiresOpaqueTexture || requiresExplicitMsaaResolve;
		if (isOffscreenRender)
			return requiresBlitForOffscreenCamera;

		return requiresBlitForOffscreenCamera || cameraData.isSceneViewCamera || isScaledRender || cameraData.isHdrEnabled ||
					 !isCompatibleBackbufferTextureDimension || !cameraData.isDefaultViewport || isCapturing || Display.main.requiresBlitToBackbuffer
					 || (renderingData.killAlphaInFinalBlit && !isStereoEnabled);
	}

	bool CanCopyDepth(ref CameraData cameraData)
	{
		bool msaaEnabledForCamera = cameraData.cameraTargetDescriptor.msaaSamples > 1;
		bool supportsTextureCopy = SystemInfo.copyTextureSupport != CopyTextureSupport.None;
		bool supportsDepthTarget = RenderingUtils.SupportsRenderTextureFormat(RenderTextureFormat.Depth);
		bool supportsDepthCopy = !msaaEnabledForCamera && (supportsDepthTarget || supportsTextureCopy);

		// TODO:  We don't have support to highp Texture2DMS currently and this breaks depth precision.
		// currently disabling it until shader changes kick in.
		//bool msaaDepthResolve = msaaEnabledForCamera && SystemInfo.supportsMultisampledTextures != 0;
		bool msaaDepthResolve = false;
		return supportsDepthCopy || msaaDepthResolve;
	}
}
