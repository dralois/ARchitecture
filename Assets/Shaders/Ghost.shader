Shader "Universal Render Pipeline/Custom/Ghost"
{
	Properties
	{
		_Color("Ghosted Color", Color) = (1,0,0,0.1)
	}
	SubShader
	{
		Tags
		{
			"RenderType" = "Transparent"
			"Queue"="Transparent"
			"IgnoreProjector" = "True"
			"RenderPipeline" = "UniversalPipeline"
		}

		Pass
		{
			Name "Ghost"

			ZTest Always
			ZWrite Off

			Blend SrcAlpha OneMinusSrcAlpha

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x

			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			struct Attributes
			{
				float4 vertex : POSITION;
			};

			struct v2f
			{
				float4 position : SV_POSITION;
			};

			v2f vert (Attributes v)
			{
				v2f o = (v2f)0;
				VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
				o.position = vertexInput.positionCS;
				return o;
			}

			CBUFFER_START(UnityPerMaterial)
			float4 _Color;
			CBUFFER_END

			half4 frag (v2f i) : SV_Target
			{
				return _Color;
			}

			ENDHLSL
		}
	}
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
