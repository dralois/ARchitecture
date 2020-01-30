Shader "Universal Render Pipeline/Custom/PlaneFinder"
{
	Properties
	{
		_BaseMap("Texture", 2D) = "white" {}
		_TexTintColor("Texture Tint Color", Color) = (1,1,1,1)
		_PlaneColor("Plane Color", Color) = (1,1,1,1)
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue"="Transparent"
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
		}

		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off

		Pass
		{
			Name "PlaneFinder"

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			// #pragma enable_d3d11_debug_symbols

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
				float3 uv2 : TEXCOORD1;
			};

			struct v2f
			{
				float4 positionCS : SV_POSITION;
				float2 uv : TEXCOORD0;
				float3 uv2 : TEXCOORD1;
			};

			TEXTURE2D(_BaseMap);
			SAMPLER(sampler_BaseMap);
			float _ShortestUVMapping;

			CBUFFER_START(UnityPerMaterial)
			float4 _BaseMap_ST;
			float4 _TexTintColor;
			float4 _PlaneColor;
			CBUFFER_END

			v2f vert (appdata v)
			{
				v2f o = (v2f)0;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
				o.positionCS = vertexInput.positionCS;
				o.uv = TRANSFORM_TEX(v.uv, _BaseMap);
				o.uv2 = v.uv2;
				return o;
			}

			half4 frag (v2f i) : SV_Target
			{
				half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _TexTintColor;
				col = lerp( _PlaneColor, col, col.a);
				col.a *=  1-smoothstep(1, _ShortestUVMapping, i.uv2.x);
				return col;
			}

			ENDHLSL
		}
	}
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
