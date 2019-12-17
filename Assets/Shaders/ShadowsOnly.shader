Shader "Universal Render Pipeline/Custom/ShadowsOnly"
{
  Properties
  {
  }
  SubShader
  {
    Tags
    {
      "RenderType" = "AlphaTest"
      "RenderPipeline" = "UniversalPipeline"
      "IgnoreProjector" = "True"
    }
    Pass
    {
      Name "ShadowsOnly"
      Tags
      {
        "LightMode" = "UniversalForward"
      }

      ZWrite On
      Cull Off

      HLSLPROGRAM

      #pragma prefer_hlslcc gles
      #pragma exclude_renderers d3d11_9x
      #pragma target 2.0

      #pragma multi_compile _ _SHADOWS_SOFT
      #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
      #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
      #pragma multi_compile _ _ADDITIONAL_LIGHT_SHADOWS

      #pragma vertex ShadowVertex
      #pragma fragment ShadowFragment

      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
      #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

      struct Attributes
      {
        float4 positionOS   : POSITION;
      };

      struct v2f
      {
        float4 positionCS   : SV_POSITION;
        float3 positionWS   : TEXCOORD0;
        float4 shadowCoord  : TEXCOORD1;
      };

      v2f ShadowVertex(Attributes input)
      {
        v2f output = (v2f)0;
        // Positionen berechnen
        VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
        // Speichern
        output.shadowCoord = GetShadowCoord(vertexInput);
        output.positionCS = vertexInput.positionCS;
        output.positionWS = vertexInput.positionWS;
        // An Fragment weitergeben
        return output;
      }

      half4 ShadowFragment(v2f input) : SV_Target
      {
        // Main Light Attenuation bestimmen
        half shadowAtten = MainLightRealtimeShadow(input.shadowCoord);
        // Additional Light Attenuation bestimmen
        uint lightsCount = GetAdditionalLightsCount();
        // Maximalen Schatten nehmen
        for (uint lightIndex = 0u; lightIndex < lightsCount; lightIndex++)
        {
          shadowAtten = min(shadowAtten, AdditionalLightRealtimeShadow(lightIndex, input.positionWS));
        }
        // Clip falls kein Schatten (-> Schatten != 0)
        clip(-shadowAtten);
        // Schwarz zurueck
        return half4(shadowAtten.xxx, 1);
      }
      ENDHLSL
    }
    // Depth Pre-Pass
    UsePass "Universal Render Pipeline/Lit/DepthOnly"
  }
  // ggf. Error
  FallBack "Hidden/InternalErrorShader"
}
