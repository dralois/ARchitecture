Shader "Universal Render Pipeline/Custom/Outline"
{
	Properties
	{
		_kernel("Kernel Array", Vector) = (0,0,0,0)
		_kernelHalfWidth("1/2 Kernel Width", Float) = 1
	}

	HLSLINCLUDE

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

	TEXTURE2D(_CameraOpaqueTexture);
	SAMPLER(sampler_CameraOpaqueTexture);
	float4 _CameraOpaqueTexture_ST;
	float2 _CameraOpaqueTexture_TexelSize;

	v2f OutlineVert(Attributes input)
	{
		v2f output = (v2f)0;
		// Positionen berechnen
		VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
		// Speichern
		output.positionCS = vertexInput.positionCS;
		output.uv = TRANSFORM_TEX(input.uv, _CameraOpaqueTexture);
		// An Fragment weitergeben
		return output;
	}

	ENDHLSL

	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
			"RenderPipeline" = "UniversalPipeline"
			"IgnoreProjector" = "True"
		}
		// Mask pass
		Pass
		{
			Name "Mask"

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			#pragma vertex OutlineVert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			half4 frag(v2f input) : SV_Target
			{
				return 1;
			}

			ENDHLSL
		}
		// Blur pass
		Pass
		{
			Name "Blur"

			HLSLPROGRAM

			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma target 2.0

			#pragma vertex OutlineVert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			float _kernel[21];
			uint _kernelHalfWidth;

			half4 frag(v2f input) : SV_Target
			{
				// Falls auf Objekt dann voll Transparent
				if(SAMPLE_TEXTURE2D(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, input.uv).r > 0)
				{
					return 0;
				}

				float sampleX = 0;
				float sampleY = 0;
				float texSizeX = _CameraOpaqueTexture_TexelSize.x;
				float texSizeY = _CameraOpaqueTexture_TexelSize.y;

				[unroll]
				// Gauss Horizontal <-
				for(uint k = 0; k < _kernelHalfWidth; k += 1)
				{
					sampleX +=
						_kernel[k] * SAMPLE_TEXTURE2D(_CameraOpaqueTexture,
																					sampler_CameraOpaqueTexture,
																					float2(input.uv.x - k * texSizeX,
																								input.uv.y)).r;
				}

				[unroll]
				// Gauss Horizontal ->
				for(uint j = 0; j < _kernelHalfWidth; j += 1)
				{
					sampleX +=
						_kernel[j] * SAMPLE_TEXTURE2D(_CameraOpaqueTexture,
																					sampler_CameraOpaqueTexture,
																					float2(input.uv.x + j * texSizeX,
																								input.uv.y)).r;
				}

				[unroll]
				// Gauss Vertikal v
				for(uint h = 0; h < _kernelHalfWidth; h += 1)
				{
					sampleY +=
						_kernel[h] * SAMPLE_TEXTURE2D(_CameraOpaqueTexture,
																					sampler_CameraOpaqueTexture,
																					float2(input.uv.x,
																								input.uv.y - h * texSizeY)).r;
				}

				[unroll]
				// Gauss Vertikal ^
				for(uint g = 0; g < _kernelHalfWidth; g += 1)
				{
					sampleY +=
						_kernel[g] * SAMPLE_TEXTURE2D(_CameraOpaqueTexture,
																					sampler_CameraOpaqueTexture,
																					float2(input.uv.x,
																								input.uv.y + g * texSizeY)).r;
				}

				// Maximum zurueck fuer gleichmaessigen Blur
				return max(sampleX, sampleY) * half4(0, 1, 1, 1);
			}

			ENDHLSL
		}
	}
	// Error
	FallBack "Hidden/Universal Render Pipeline/FallbackError"
}

