// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

#ifndef TREE_VERTEXLIT_CG_INCLUDED
#define TREE_VERTEXLIT_CG_INCLUDED

#include "UnityCG.cginc"

float4 _Color;
half3 _TranslucencyColor;
half _TranslucencyViewDependency;
half _ShadowStrength;

// --- Lighting ----------------------------------------------------------------

float3 _LightColor0;
float3 ShadeTranslucentMainLight (float4 vertex, float3 normal)
{
	float3 viewDir = normalize(WorldSpaceViewDir(vertex));
	float3 lightDir = normalize(WorldSpaceLightDir(vertex));
	float3 lightColor = _LightColor0.rgb;

	float nl = dot (normal, lightDir);
	
	// view dependent back contribution for translucency
	float backContrib = saturate(dot(viewDir, -lightDir));
	
	// normally translucency is more like -nl, but looks better when it's view dependent
	backContrib = lerp(saturate(-nl), backContrib, _TranslucencyViewDependency);
	
	// wrap-around diffuse
	float diffuse = max(0, nl * 0.6 + 0.4);
	
	return lightColor.rgb * (diffuse + backContrib * _TranslucencyColor);
}


float3 ShadeTranslucentLights (float4 vertex, float3 normal)
{
	float3 viewDir = normalize(WorldSpaceViewDir(vertex));
	float3 mainLightDir = normalize(WorldSpaceLightDir(vertex));
	float3 frontlight = ShadeSH9 (float4(normal,1.0));
	float3 backlight = ShadeSH9 (float4(-normal,1.0));
	#ifdef VERTEXLIGHT_ON
	float3 worldPos = mul(unity_ObjectToWorld, vertex).xyz;
	frontlight += Shade4PointLights (
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor0, unity_LightColor1, unity_LightColor2, unity_LightColor3,
		unity_4LightAtten0, worldPos, normal);
	backlight += Shade4PointLights (
		unity_4LightPosX0, unity_4LightPosY0, unity_4LightPosZ0,
		unity_LightColor0, unity_LightColor1, unity_LightColor2, unity_LightColor3,
		unity_4LightAtten0, worldPos, -normal);
	#endif
	
	// view dependent back contribution for translucency using main light as a cue
	float backContrib = saturate(dot(viewDir, -mainLightDir));
	backlight = lerp(backlight, backlight * backContrib, _TranslucencyViewDependency);
	
	// as we integrate over whole sphere instead of normal hemi-sphere
	// lighting gets too washed out, so let's half it down
	return 0.5 * (frontlight + backlight * _TranslucencyColor);
}
		

#endif