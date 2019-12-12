void Shadow_float(float3 ObjectPos, out float Attenuation)
{
#ifndef SHADERGRAPH_PREVIEW
	VertexPositionInputs transformedPos = GetVertexPositionInputs(ObjectPos);
	float4 shadowCoord = GetShadowCoord(transformedPos);
	Attenuation = MainLightRealtimeShadow(shadowCoord);
#else
	Attenuation = 1.0;
#endif
}