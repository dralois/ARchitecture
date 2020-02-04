Shader "Universal Render Pipeline/Custom/ShadowBlend"
{
	Properties
	{
		[HideInInspector] _BaseMap("BaseMap", 2D) = "black" {}
		_ShadowColor("Shadow Strength", Range(0, 1)) = 1
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Background"
			"Queue" = "Transparent"
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
		}

		// Full Blend Pass
		Pass
		{
			Name "Shadow Blend"

			Tags
			{
				"LightMode" = "UniversalForward"
			}

			Cull Off
			Blend DstColor SrcColor, One Zero

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// #pragma enable_d3d11_debug_symbols

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			float _ShadowColor;
			CBUFFER_END

			struct Attributes
			{
				float4 positionOS   : POSITION;
				float2 uv           : TEXCOORD0;
			};

			struct v2f
			{
				float4 positionCS   : SV_POSITION;
				float2 uv           : TEXCOORD0;
			};

			v2f vert(Attributes input)
			{
				v2f output = (v2f)0;
				// Positionen berechnen
				VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
				// Speichern
				output.positionCS = vertexInput.positionCS;
				output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
				// An Fragment weitergeben
				return output;
			}

			half4 frag(v2f input) : SV_Target
			{
				// Modifizierte Attenuation zurueck
				return 1 - ((1 - SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv)) * _ShadowColor);
			}

			ENDHLSL
		}
	}
	// Error
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
