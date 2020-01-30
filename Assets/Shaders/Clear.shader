Shader "Universal Render Pipeline/Custom/Clear"
{
	Properties
	{
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
		}

		// Clear to transparent pass
		Pass
		{
			Name "Clear"

			Tags
			{
				"LightMode" = "UniversalForward"
			}

			Cull Off

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// #pragma enable_d3d11_debug_symbols

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 positionOS   : POSITION;
			};

			struct v2f
			{
				float4 positionCS   : SV_POSITION;
			};

			v2f vert(Attributes input)
			{
				v2f output = (v2f)0;
				// Positionen berechnen
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
				// Speichern
				output.positionCS = vertexInput.positionCS;
				// An Fragment weitergeben
				return output;
			}

			half4 frag(v2f input) : SV_Target
			{
				// Weiss, voll Transparent
				return half4(1, 1, 1, 0);
			}

			ENDHLSL
		}
	}
	// Error
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
