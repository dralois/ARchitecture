using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Invokes OnRenderObject callback
/// </summary>

public class InvokeOnRenderObjectCallbackPassCopy : ScriptableRenderPass
{
	public InvokeOnRenderObjectCallbackPassCopy(RenderPassEvent evt)
	{
		renderPassEvent = evt;
	}

	/// <inheritdoc/>
	public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
	{
		context.InvokeOnRenderObjectCallback();
	}
}
