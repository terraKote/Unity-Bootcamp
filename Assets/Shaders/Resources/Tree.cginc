#ifndef TREE_CG_INCLUDED
#define TREE_CG_INCLUDED

#include "TerrainEngine.cginc"

float4 _Color;
half3 _TranslucencyColor;
half _TranslucencyViewDependency;
half _ShadowStrength;

struct LeafSurfaceOutput {
	half3 Albedo;
	half3 Normal;
	half3 Emission;
	half Translucency;
	half ShadowOffset;
	half Specular;
	half Gloss;
	half Alpha;
};

inline half4 LightingTreeLeaf (LeafSurfaceOutput s, half3 lightDir, half3 viewDir, half atten)
{
	half nl = dot (s.Normal, lightDir);
		
	// view dependent back contribution for translucency
	half backContrib = saturate(dot(viewDir, -lightDir));
	
	// normally translucency is more like -nl, but looks better when it's view dependent
	backContrib = lerp(saturate(-nl), backContrib, _TranslucencyViewDependency);
	
	half3 translucencyColor = backContrib * s.Translucency * _TranslucencyColor;
	
	// wrap-around diffuse
	nl = max(0, nl * 0.6 + 0.4);
	
	half4 c;
	c.rgb = s.Albedo * (translucencyColor * 2 + nl);
	c.rgb = c.rgb * _LightColor0.rgb;
	
	c.rgb *= lerp(2, atten * 2, _ShadowStrength);
	
	return c;
}

#endif