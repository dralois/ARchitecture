Shader "Universal Render Pipeline/Custom/Occlusion"
{
	Properties
	{
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry-100"
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
		}

		// Depth write only pass
		Pass
		{
			Name "DepthWrite"

			Cull Off
			ZWrite On
			ZTest LEqual

			ColorMask 0

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

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
				// Kein Farboutput
				return 0;
			}

			ENDHLSL
		}
		// Shadow caster pass
		UsePass "Universal Render Pipeline/Lit/ShadowCaster"
	}
	// Error
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
