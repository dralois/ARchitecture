Shader "Universal Render Pipeline/Custom/FullBlend"
{
	Properties
	{
		_BaseMap("BaseMap", 2D) = "white" {}
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

		// Full Blend Pass
		Pass
		{
			Name "FullBlend"

			Tags
			{
				"LightMode" = "UniversalForward"
			}

			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha

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
				float2 uv           : TEXCOORD0;
			};

			struct v2f
			{
				float4 positionCS   : SV_POSITION;
				float2 uv           : TEXCOORD0;
			};

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);

			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			CBUFFER_END

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
				// Farbe holen
				half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv);
				// Blending entweder voll undurchsichtig oder transparnet
				if(col.a > 0.1)
				{
					return half4(col.rgb, 1);
				}
				else
				{
					return 0;
				}
			}

			ENDHLSL
		}
	}
	// Error
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
